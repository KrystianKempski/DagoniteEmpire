using DA_Business.Repository.ChatRepos;
using DA_Business.Services;
using DA_Business.Tests.Fixtures;
using DA_Business.Tests.Helpers;
using DA_Common;
using DA_Common.Notifications;
using DA_DataAccess;
using DA_DataAccess.CharacterClasses;
using DA_DataAccess.Chat;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging.Abstractions;

namespace DA_Business.Tests.Notifications;

/// <summary>
/// End to end over the whole chat push path: a message goes through the repository, which raises the
/// event, which the dispatcher turns into a push. Recipients are resolved from the thread itself, so a
/// player with heroes in several campaigns cannot be missed depending on which one is "current".
/// </summary>
public class ChatPushNotificationTests : IClassFixture<DatabaseFixture>
{
    private const string Alice = "id-alice";
    private const string Bob = "id-bob";
    private const string Carol = "id-carol";
    private const string Gm = "id-gm";

    private readonly DatabaseFixture _fixture;
    private readonly CapturingPushService _push = new();
    private readonly RecordingNotificationQueue _queue = new();
    private readonly ChatRepository _repository;
    private readonly GameNotificationDispatcher _dispatcher;

    private int _campaignOne;

    public ChatPushNotificationTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
        _fixture.ResetDatabase();
        _repository = new ChatRepository(_fixture.DbContextFactory, _queue, new ChatBroadcaster());
        _dispatcher = new GameNotificationDispatcher(
            _fixture.DbContextFactory,
            _push,
            new NotificationRecipientLookup(_fixture.DbContextFactory),
            NullLogger<GameNotificationDispatcher>.Instance);
        Seed();
    }

    [Fact]
    public async Task DirectMessage_ReachesThePeerAccount_NotTheSender()
    {
        var conversationId = await _repository.GetOrCreateDirectAsync(Alice, Bob, peerIsGameMaster: false);
        await _repository.SendAsync(conversationId, Alice, "ping");

        var sent = await DispatchSingle();

        Assert.Equal(new[] { Bob }, sent.UserIds);
        Assert.Equal(NotificationTopic.Chat, sent.Topic);
        Assert.Contains("alice", sent.Payload.Body);
        Assert.Equal($"/?chat={conversationId}", sent.Payload.Url);
    }

    [Fact]
    public async Task EveryMessage_GetsItsOwnTag_SoFollowUpsStillAlert()
    {
        var conversationId = await _repository.GetOrCreateDirectAsync(Alice, Bob, peerIsGameMaster: false);
        var first = await _repository.SendAsync(conversationId, Alice, "one");
        var second = await _repository.SendAsync(conversationId, Alice, "two");

        await _dispatcher.Dispatch(_queue.Raised[0]);
        await _dispatcher.Dispatch(_queue.Raised[1]);

        Assert.Equal(2, _push.Sent.Count);
        Assert.NotEqual(_push.Sent[0].Payload.Tag, _push.Sent[1].Payload.Tag);
        Assert.Contains(first.Id.ToString(), _push.Sent[0].Payload.Tag);
        Assert.Contains(second.Id.ToString(), _push.Sent[1].Payload.Tag);

        // One thread key across both, so the worker can clear the earlier entry from the tray.
        Assert.Equal($"chat-{conversationId}", _push.Sent[0].Payload.ThreadKey);
        Assert.Equal(_push.Sent[0].Payload.ThreadKey, _push.Sent[1].Payload.ThreadKey);
    }

    [Fact]
    public async Task PartyMessage_ReachesTheRosterAndTheGameMaster_ButNotOtherCampaigns()
    {
        var conversationId = await _repository.GetOrCreatePartyAsync(Alice, _campaignOne);
        await _repository.SendAsync(conversationId, Alice, "hello party");

        var sent = await DispatchSingle();

        Assert.Contains(Bob, sent.UserIds);
        Assert.Contains(Gm, sent.UserIds);
        Assert.DoesNotContain(Alice, sent.UserIds);
        Assert.DoesNotContain(Carol, sent.UserIds);
        Assert.Contains("Campaign One", sent.Payload.Title);
    }

    [Fact]
    public async Task MessageToTheGameMaster_ReachesEveryGameMaster()
    {
        SeedGameMaster("id-gm-2", "gm2");

        var conversationId = await _repository.GetOrCreateDirectAsync(Alice, null, peerIsGameMaster: true);
        await _repository.SendAsync(conversationId, Alice, "a question");

        var sent = await DispatchSingle();

        Assert.Equal(new[] { Gm, "id-gm-2" }.OrderBy(x => x), sent.UserIds.OrderBy(x => x));
    }

    [Fact]
    public async Task AnswerFromTheGameMaster_IsAttributedToTheRole_NotTheAccount()
    {
        var conversationId = await _repository.GetOrCreateDirectAsync(Gm, Alice, peerIsGameMaster: false);
        await _repository.SendAsync(conversationId, Gm, "an answer");

        var sent = await DispatchSingle();

        Assert.Equal(new[] { Alice }, sent.UserIds);
        Assert.Contains(SD.GameMaster_NPCName, sent.Payload.Body);
    }

    private async Task<CapturingPushService.Send> DispatchSingle()
    {
        await _dispatcher.Dispatch(Assert.Single(_queue.Raised));
        return Assert.Single(_push.Sent);
    }

    private void Seed()
    {
        SeedUser(Alice, "alice");
        SeedUser(Bob, "bob");
        SeedUser(Carol, "carol");
        SeedGameMaster(Gm, "gm");

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
