using DA_Business.Repository.ChatRepos;
using DA_Business.Services;
using DA_Business.Tests.Fixtures;
using DA_Business.Tests.Helpers;
using DA_Common;
using DA_Common.Notifications;
using DA_DataAccess;
using DA_DataAccess.CharacterClasses;
using DA_DataAccess.Chat;
using DA_Models.ChatModels;
using DagoniteEmpire.Exceptions;
using Microsoft.AspNetCore.Identity;

namespace DA_Business.Tests.Repositories;

/// <summary>
/// Alice holds two heroes in two different campaigns — the shape that used to break chat, because the
/// drawer and the unread badge only ever looked at the character she happened to have selected.
/// </summary>
public class ChatRepositoryTests : IClassFixture<DatabaseFixture>
{
    private const string Alice = "id-alice";
    private const string Bob = "id-bob";
    private const string Carol = "id-carol";
    private const string Gm = "id-gm";
    private const string SecondGm = "id-gm-2";

    private readonly DatabaseFixture _fixture;
    private readonly RecordingNotificationQueue _notifications = new();
    private readonly ChatBroadcaster _broadcaster = new();
    private readonly ChatRepository _repository;

    private int _campaignOne;
    private int _campaignTwo;

    public ChatRepositoryTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
        _fixture.ResetDatabase();
        _repository = new ChatRepository(
            _fixture.DbContextFactory,
            _notifications,
            _broadcaster);
        Seed();
    }

    [Fact]
    public async Task Contacts_CoverEveryCampaignOfTheAccount()
    {
        var contacts = await _repository.GetContactsAsync(Alice);

        Assert.Equal(2, contacts.Count(c => c.IsPartyChannel));
        Assert.Contains(contacts, c => c.PeerUserId == Bob);
        Assert.Contains(contacts, c => c.PeerUserId == Carol);
        Assert.Contains(contacts, c => c.PeerIsGameMaster);
        Assert.DoesNotContain(contacts, c => c.PeerUserId == Alice);
    }

    [Fact]
    public async Task UnreadTotal_CountsMessagesFromEveryCampaign()
    {
        // Bob shares campaign one with Alice's first hero, Carol campaign two with her second.
        await SendDirect(Bob, Alice, "from campaign one");
        await SendDirect(Carol, Alice, "from campaign two");

        Assert.Equal(2, await _repository.GetUnreadTotalAsync(Alice));
    }

    [Fact]
    public async Task DirectThread_IsOneThreadPerPair_NotPerCampaign()
    {
        var first = await _repository.GetOrCreateDirectAsync(Alice, Bob, peerIsGameMaster: false);
        var second = await _repository.GetOrCreateDirectAsync(Alice, Bob, peerIsGameMaster: false);
        var fromTheOtherSide = await _repository.GetOrCreateDirectAsync(Bob, Alice, peerIsGameMaster: false);

        Assert.Equal(first, second);
        Assert.Equal(first, fromTheOtherSide);
    }

    [Fact]
    public async Task Direct_IsInvisibleToAThirdParty()
    {
        var conversationId = await SendDirect(Alice, Bob, "secret");

        var forBob = await _repository.GetThreadAsync(conversationId, Bob);
        Assert.Single(forBob);
        Assert.Equal("secret", forBob[0].Content);

        await Assert.ThrowsAsync<RepositoryErrorException>(() =>
            _repository.GetThreadAsync(conversationId, Carol));
    }

    [Fact]
    public async Task Direct_BetweenAccountsWithNoSharedCampaign_IsRejected()
    {
        await Assert.ThrowsAsync<RepositoryErrorException>(() =>
            _repository.GetOrCreateDirectAsync(Bob, Carol, peerIsGameMaster: false));
    }

    [Fact]
    public async Task PartyChannel_IsVisibleToTheRosterAndTheGameMaster()
    {
        var conversationId = await _repository.GetOrCreatePartyAsync(Alice, _campaignOne);
        await _repository.SendAsync(conversationId, Alice, "hello party");

        Assert.Single(await _repository.GetThreadAsync(conversationId, Bob));
        Assert.Single(await _repository.GetThreadAsync(conversationId, Gm));

        // Carol plays in campaign two only.
        await Assert.ThrowsAsync<RepositoryErrorException>(() =>
            _repository.GetThreadAsync(conversationId, Carol));
    }

    [Fact]
    public async Task PartyChannel_OfAForeignCampaign_CannotBeCreated()
    {
        await Assert.ThrowsAsync<RepositoryErrorException>(() =>
            _repository.GetOrCreatePartyAsync(Carol, _campaignOne));
    }

    [Fact]
    public async Task GameMasters_ShareOneRoleThreadPerPlayer()
    {
        var fromAlice = await _repository.GetOrCreateDirectAsync(Alice, null, peerIsGameMaster: true);
        await _repository.SendAsync(fromAlice, Alice, "a question");

        // A different GM answers; handing the campaign over must not start a second thread.
        var fromSecondGm = await _repository.GetOrCreateDirectAsync(SecondGm, Alice, peerIsGameMaster: false);
        Assert.Equal(fromAlice, fromSecondGm);

        await _repository.SendAsync(fromSecondGm, SecondGm, "an answer");

        var thread = await _repository.GetThreadAsync(fromAlice, Alice);
        Assert.Equal(2, thread.Count);

        var answer = thread[1];
        Assert.True(answer.SentAsGameMaster);
        Assert.Equal(SD.GameMaster_Portrait, answer.SenderImageUrl);
        Assert.False(answer.IsMine);
    }

    [Fact]
    public async Task ReadState_IsPerAccount_SoOneGameMasterDoesNotClearAnother()
    {
        var conversationId = await SendDirect(Alice, null, "a question", peerIsGameMaster: true);

        Assert.Equal(1, await _repository.GetUnreadTotalAsync(Gm));
        Assert.Equal(1, await _repository.GetUnreadTotalAsync(SecondGm));

        await _repository.MarkReadAsync(conversationId, Gm);

        Assert.Equal(0, await _repository.GetUnreadTotalAsync(Gm));
        Assert.Equal(1, await _repository.GetUnreadTotalAsync(SecondGm));
    }

    [Fact]
    public async Task MarkRead_ClearsTheUnreadCount()
    {
        var conversationId = await SendDirect(Bob, Alice, "one");
        await _repository.SendAsync(conversationId, Bob, "two");

        var before = await _repository.GetContactsAsync(Alice);
        Assert.Equal(2, before.First(c => c.PeerUserId == Bob).UnreadCount);

        await _repository.MarkReadAsync(conversationId, Alice);

        var after = await _repository.GetContactsAsync(Alice);
        Assert.Equal(0, after.First(c => c.PeerUserId == Bob).UnreadCount);
        Assert.Equal(0, await _repository.GetUnreadTotalAsync(Alice));
    }

    [Fact]
    public async Task Send_BroadcastsToTheRecipients_AndEnqueuesOneNotification()
    {
        var conversationId = await _repository.GetOrCreateDirectAsync(Alice, Bob, peerIsGameMaster: false);

        ChatMessageDTO? forBob = null;
        ChatMessageDTO? forCarol = null;
        using var bobsSub = _broadcaster.Subscribe(Bob, msg => { forBob = msg; return Task.CompletedTask; });
        using var carolsSub = _broadcaster.Subscribe(Carol, msg => { forCarol = msg; return Task.CompletedTask; });

        var dto = await _repository.SendAsync(conversationId, Alice, "ping");

        Assert.NotNull(forBob);
        Assert.Equal(dto.Id, forBob!.Id);
        Assert.Null(forCarol);

        var posted = Assert.IsType<ChatMessagePosted>(Assert.Single(_notifications.Raised));
        Assert.Equal(conversationId, posted.ConversationId);
        Assert.Equal(dto.Id, posted.MessageId);
        Assert.Equal(Alice, posted.SenderUserId);
    }

    [Fact]
    public async Task Send_IntoAThreadYouAreNotPartOf_IsRejected()
    {
        var conversationId = await _repository.GetOrCreateDirectAsync(Alice, Bob, peerIsGameMaster: false);

        await Assert.ThrowsAsync<RepositoryErrorException>(() =>
            _repository.SendAsync(conversationId, Carol, "butting in"));
    }

    [Fact]
    public async Task Send_EmptyMessage_IsRejected()
    {
        var conversationId = await _repository.GetOrCreateDirectAsync(Alice, Bob, peerIsGameMaster: false);

        await Assert.ThrowsAsync<RepositoryErrorException>(() =>
            _repository.SendAsync(conversationId, Alice, "   "));
    }

    [Fact]
    public async Task DemoAccounts_HaveNoChat()
    {
        SeedUser("id-demo", SD.DemoBaronUserName);

        Assert.Empty(await _repository.GetContactsAsync("id-demo"));
        Assert.Equal(0, await _repository.GetUnreadTotalAsync("id-demo"));
    }

    [Fact]
    public async Task Contacts_KeepAThreadWhoseCampaignEnded()
    {
        var conversationId = await SendDirect(Alice, Bob, "before the campaign ended");

        using (var ctx = _fixture.CreateContext())
        {
            // Alice keeps campaign two, so she still has chat; Bob is only reachable through campaign one.
            var campaign = ctx.Campaigns.First(c => c.Id == _campaignOne);
            campaign.IsFinished = true;
            ctx.SaveChanges();
        }

        var contacts = await _repository.GetContactsAsync(Alice);

        Assert.DoesNotContain(contacts, c => c.IsPartyChannel && c.CampaignId == _campaignOne);
        var kept = Assert.Single(contacts, c => c.ConversationId == conversationId);
        Assert.Equal(Bob, kept.PeerUserId);
    }

    private async Task<long> SendDirect(
        string senderUserId,
        string? peerUserId,
        string content,
        bool peerIsGameMaster = false)
    {
        var conversationId = await _repository.GetOrCreateDirectAsync(senderUserId, peerUserId, peerIsGameMaster);
        await _repository.SendAsync(conversationId, senderUserId, content);
        return conversationId;
    }

    private void Seed()
    {
        SeedUser(Alice, "alice");
        SeedUser(Bob, "bob");
        SeedUser(Carol, "carol");
        SeedGameMaster(Gm, "gm");
        SeedGameMaster(SecondGm, "gm2");

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

        var aliceOne = Make("Alice the Bold", "alice");
        var aliceTwo = Make("Alice the Quiet", "alice");
        var bob = Make("Bob", "bob");
        var carol = Make("Carol", "carol");
        ctx.Characters.AddRange(aliceOne, aliceTwo, bob, carol);
        ctx.SaveChanges();

        var one = new Campaign
        {
            Name = "Campaign One",
            Description = "test",
            GameMaster = "gm",
            CreatedDate = DateTime.UtcNow,
            Characters = { aliceOne, bob },
        };
        var two = new Campaign
        {
            Name = "Campaign Two",
            Description = "test",
            GameMaster = "gm",
            CreatedDate = DateTime.UtcNow,
            Characters = { aliceTwo, carol },
        };
        ctx.Campaigns.AddRange(one, two);
        ctx.SaveChanges();

        _campaignOne = one.Id;
        _campaignTwo = two.Id;
    }

    private void SeedUser(string id, string userName)
    {
        using var ctx = _fixture.CreateContext();
        ctx.ApplicationUsers.Add(new ApplicationUser
        {
            Id = id,
            UserName = userName,
            NormalizedUserName = userName.ToUpperInvariant(),
            Name = userName,
        });
        ctx.SaveChanges();
    }

    private void SeedGameMaster(string id, string userName)
    {
        SeedUser(id, userName);

        using var ctx = _fixture.CreateContext();
        if (!ctx.Roles.Any(r => r.Name == SD.Role_GameMaster))
        {
            ctx.Roles.Add(new IdentityRole
            {
                Id = "role-gm",
                Name = SD.Role_GameMaster,
                NormalizedName = SD.Role_GameMaster.ToUpperInvariant(),
            });
            ctx.SaveChanges();
        }

        ctx.UserRoles.Add(new IdentityUserRole<string> { UserId = id, RoleId = "role-gm" });
        ctx.SaveChanges();
    }
}
