using DA_Business.Repository.ChatRepos;
using DA_Business.Services;
using DA_Business.Tests.Fixtures;
using DA_Business.Tests.Helpers;
using DA_Common;
using DA_Common.Notifications;
using DA_DataAccess.CharacterClasses;
using DA_DataAccess.Chat;
using DA_Models.ChatModels;
using DagoniteEmpire.Exceptions;

namespace DA_Business.Tests.Repositories;

public class CampaignChatRepositoryTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;
    private readonly RecordingNotificationQueue _notifications = new();
    private readonly CampaignChatBroadcaster _broadcaster = new();
    private readonly CampaignChatRepository _repository;

    private int _campaignId;
    private int _charAId;
    private int _charBId;
    private int _charCId;
    private int _gmId;
    private int _outsiderId;

    public CampaignChatRepositoryTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
        _fixture.ResetDatabase();
        _repository = new CampaignChatRepository(
            _fixture.DbContextFactory,
            _notifications,
            _broadcaster);
        Seed();
    }

    private void Seed()
    {
        using var ctx = _fixture.CreateContext();

        ctx.Races.Add(new Race { Name = "ChatRace", Description = "t", RaceApproved = true });
        ctx.Professions.Add(new Profession
        {
            Name = "ChatProfession",
            Description = "t",
            RelatedAttributeName = "Strength",
            IsApproved = true,
        });
        ctx.SaveChanges();

        var raceId = ctx.Races.First().Id;
        var profId = ctx.Professions.First().Id;

        Character Make(string name, string user) => new()
        {
            NPCName = name,
            UserName = user,
            RaceId = raceId,
            ProfessionId = profId,
            IsApproved = true,
        };

        var a = Make("Alice", "alice");
        var b = Make("Bob", "bob");
        var c = Make("Carol", "carol");
        var gm = Make(SD.GameMaster_NPCName, "gm");
        var outsider = Make("Outsider", "outsider");
        ctx.Characters.AddRange(a, b, c, gm, outsider);
        ctx.SaveChanges();

        _charAId = a.Id;
        _charBId = b.Id;
        _charCId = c.Id;
        _gmId = gm.Id;
        _outsiderId = outsider.Id;

        var campaign = new Campaign
        {
            Name = "ChatCampaign",
            Description = "test",
            GameMaster = "gm",
            CreatedDate = DateTime.UtcNow,
            Characters = { a, b, c },
        };
        ctx.Campaigns.Add(campaign);
        ctx.SaveChanges();
        _campaignId = campaign.Id;
    }

    [Fact]
    public async Task Send_1to1_IsVisibleOnlyToParticipants()
    {
        await _repository.SendAsync(_campaignId, _charAId, _charBId, "secret", "alice", false);

        var forB = await _repository.GetThreadAsync(_campaignId, _charBId, _charAId, "bob", false);
        var forC = await _repository.GetThreadAsync(_campaignId, _charCId, _charAId, "carol", false);

        Assert.Single(forB);
        Assert.Equal("secret", forB[0].Content);
        Assert.Empty(forC);
    }

    [Fact]
    public async Task PartyChannel_IsVisibleToAllCampaignMembers()
    {
        await _repository.SendAsync(_campaignId, _charAId, null, "hello party", "alice", false);

        var forB = await _repository.GetThreadAsync(_campaignId, _charBId, null, "bob", false);
        var forC = await _repository.GetThreadAsync(_campaignId, _charCId, null, "carol", false);

        Assert.Single(forB);
        Assert.Single(forC);
        Assert.Equal("hello party", forB[0].Content);
    }

    [Fact]
    public async Task UnreadCount_RespectsLastReadDate()
    {
        await _repository.SendAsync(_campaignId, _charAId, _charBId, "one", "alice", false);
        await _repository.SendAsync(_campaignId, _charAId, _charBId, "two", "alice", false);

        var before = await _repository.GetContactsAsync(_campaignId, _charBId, "bob", false);
        var peer = before.First(c => c.PeerCharacterId == _charAId);
        Assert.Equal(2, peer.UnreadCount);

        await _repository.MarkReadAsync(_campaignId, _charBId, _charAId, "bob", false);

        var after = await _repository.GetContactsAsync(_campaignId, _charBId, "bob", false);
        Assert.Equal(0, after.First(c => c.PeerCharacterId == _charAId).UnreadCount);
    }

    [Fact]
    public async Task Send_AsForeignCharacter_IsRejected()
    {
        await Assert.ThrowsAsync<RepositoryErrorException>(() =>
            _repository.SendAsync(_campaignId, _charAId, _charBId, "nope", "bob", false));
    }

    [Fact]
    public async Task Send_AsOutsider_IsRejected()
    {
        await Assert.ThrowsAsync<RepositoryErrorException>(() =>
            _repository.SendAsync(_campaignId, _outsiderId, _charAId, "nope", "outsider", false));
    }

    [Fact]
    public async Task Send_EnqueuesNotification_AndBroadcasts()
    {
        CampaignChatMessageDTO? received = null;
        using var sub = _broadcaster.Subscribe(_campaignId, _charBId, msg =>
        {
            received = msg;
            return Task.CompletedTask;
        });

        var dto = await _repository.SendAsync(_campaignId, _charAId, _charBId, "ping", "alice", false);

        Assert.NotNull(received);
        Assert.Equal(dto.Id, received!.Id);
        var note = Assert.Single(_notifications.Raised);
        var chat = Assert.IsType<CampaignChatMessageSent>(note);
        Assert.Equal(_campaignId, chat.CampaignId);
        Assert.Equal(_charAId, chat.SenderCharacterId);
        Assert.Equal(_charBId, chat.RecipientCharacterId);
    }

    [Fact]
    public async Task PartySend_EnqueuesPartyNotification()
    {
        await _repository.SendAsync(_campaignId, _charAId, null, "all hands", "alice", false);

        var note = Assert.IsType<CampaignChatMessageSent>(Assert.Single(_notifications.Raised));
        Assert.Null(note.RecipientCharacterId);
    }

    [Fact]
    public async Task GetContacts_IncludesPartyAndGm()
    {
        var contacts = await _repository.GetContactsAsync(_campaignId, _charAId, "alice", false);

        Assert.Contains(contacts, c => c.IsPartyChannel);
        Assert.Contains(contacts, c => c.IsGameMaster && c.PeerCharacterId == _gmId);
        Assert.Contains(contacts, c => c.PeerCharacterId == _charBId);
        Assert.DoesNotContain(contacts, c => c.PeerCharacterId == _charAId);
    }

    [Fact]
    public async Task UnreadSummary_CountsAcrossThreads()
    {
        await _repository.SendAsync(_campaignId, _charAId, _charBId, "dm", "alice", false);
        await _repository.SendAsync(_campaignId, _charAId, null, "party", "alice", false);

        var unread = await _repository.GetUnreadSummaryAsync(_charBId, "bob", false);
        Assert.Equal(2, unread);
    }

    [Fact]
    public async Task Gm_CanSendAsAdminOrMg()
    {
        var dto = await _repository.SendAsync(_campaignId, _gmId, _charAId, "from mg", "gm", true);
        Assert.Equal(_gmId, dto.SenderCharacterId);

        var thread = await _repository.GetThreadAsync(_campaignId, _charAId, _gmId, "alice", false);
        Assert.Single(thread);
    }
}
