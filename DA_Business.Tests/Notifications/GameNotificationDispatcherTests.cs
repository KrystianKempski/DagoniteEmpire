using DA_Business.Services;
using DA_Business.Services.Interfaces;
using DA_Business.Tests.Fixtures;
using DA_Common;
using DA_Common.Barony;
using DA_Common.Notifications;
using DA_DataAccess;
using DA_DataAccess.BaronyData;
using DA_DataAccess.CharacterClasses;
using DA_DataAccess.Chat;
using DA_Models.BaronyModels;
using DA_Models.NotificationModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging.Abstractions;

namespace DA_Business.Tests.Notifications;

public class GameNotificationDispatcherTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;
    private readonly CapturingPushService _push = new();
    private readonly GameNotificationDispatcher _dispatcher;

    public GameNotificationDispatcherTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
        _fixture.ResetDatabase();
        _dispatcher = new GameNotificationDispatcher(
            _fixture.DbContextFactory,
            _push,
            new NotificationRecipientLookup(_fixture.DbContextFactory),
            NullLogger<GameNotificationDispatcher>.Instance);
    }

    [Fact]
    public async Task InboundLetter_GoesToTheBaronWhoOwnsTheThread()
    {
        var baronyId = SeedBaronyOwnedBy("id-baron", "duke");
        var threadId = SeedThread(baronyId, "Sprawa podatków", "Lord Varen");

        await _dispatcher.Dispatch(new BaronLetterDelivered(threadId, IsInbound: true));

        var sent = Assert.Single(_push.Sent);
        Assert.Equal(new[] { "id-baron" }, sent.UserIds);
        Assert.Equal(NotificationTopic.BaronLetter, sent.Topic);
        Assert.Contains("Lord Varen", sent.Payload.Title);
        Assert.Equal("Sprawa podatków", sent.Payload.Body);
        Assert.Equal($"/barony/letters?thread={threadId}", sent.Payload.Url);
    }

    [Fact]
    public async Task OutboundLetter_GoesToTheGameMaster_NotTheBaron()
    {
        var baronyId = SeedBaronyOwnedBy("id-baron", "duke");
        var threadId = SeedThread(baronyId, "Prośba o wsparcie", "Lord Varen");
        SeedGameMaster("id-gm", "gm");

        await _dispatcher.Dispatch(new BaronLetterDelivered(threadId, IsInbound: false));

        var sent = Assert.Single(_push.Sent);
        Assert.Equal(new[] { "id-gm" }, sent.UserIds);
        Assert.Contains("Darkhold", sent.Payload.Title);
        Assert.Contains("Lord Varen", sent.Payload.Body);
    }

    [Fact]
    public async Task OutboundLetter_IsSkipped_WhenThereIsNoGameMaster()
    {
        var baronyId = SeedBaronyOwnedBy("id-baron", "duke");
        var threadId = SeedThread(baronyId, "Prośba o wsparcie", "Lord Varen");

        var delivered = await _dispatcher.Dispatch(new BaronLetterDelivered(threadId, IsInbound: false));

        Assert.Equal(0, delivered);
        Assert.Empty(_push.Sent);
    }

    [Fact]
    public async Task UnknownThread_IsIgnored()
    {
        Assert.Equal(0, await _dispatcher.Dispatch(new BaronLetterDelivered(9999, IsInbound: true)));
        Assert.Empty(_push.Sent);
    }

    [Fact]
    public async Task TurnResolved_TellsTheBaronAndNamesTheBarony()
    {
        var baronyId = SeedBaronyOwnedBy("id-baron", "duke");

        await _dispatcher.Dispatch(new BaronyTurnResolved(baronyId, 7));

        var sent = Assert.Single(_push.Sent);
        Assert.Equal(new[] { "id-baron" }, sent.UserIds);
        Assert.Equal(NotificationTopic.TurnResolved, sent.Topic);
        Assert.Contains("7", sent.Payload.Title);
        Assert.Contains("Darkhold", sent.Payload.Body);
        Assert.Equal("/barony", sent.Payload.Url);
    }

    [Fact]
    public async Task ChapterPost_NotifiesTheOtherParticipantsOnly()
    {
        var professionId = SeedProfession();
        var authorId = SeedCharacter("author-user", "Autor", professionId);
        var readerId = SeedCharacter("reader-user", "Czytelnik", professionId);
        SeedUser("id-author", "author-user");
        SeedUser("id-reader", "reader-user");
        var chapterId = SeedChapter("Rozdział I", authorId, readerId);

        await _dispatcher.Dispatch(new ChapterPostAdded(chapterId, authorId));

        var sent = Assert.Single(_push.Sent);
        Assert.Equal(new[] { "id-reader" }, sent.UserIds);
        Assert.Equal(NotificationTopic.Posts, sent.Topic);
        Assert.Contains("Rozdział I", sent.Payload.Title);
        Assert.Contains("Autor", sent.Payload.Body);
        Assert.Equal($"/chapter/{chapterId}", sent.Payload.Url);
    }

    [Fact]
    public async Task GmAnswer_GoesToTheBaronWhoAsked()
    {
        var baronyId = SeedBaronyOwnedBy("id-baron", "duke");
        SeedGameMaster("id-gm", "gm");
        var threadId = SeedQaThread(baronyId, "Czy mogę zbudować most?");

        await _dispatcher.Dispatch(new GmQuestionPosted(threadId, FromGameMaster: true));

        var sent = Assert.Single(_push.Sent);
        Assert.Equal(new[] { "id-baron" }, sent.UserIds);
        Assert.Equal(NotificationTopic.GmQuestion, sent.Topic);
        Assert.Equal("Czy mogę zbudować most?", sent.Payload.Body);
        Assert.Equal($"/barony/notes?tab=qa&thread={threadId}", sent.Payload.Url);
    }

    [Fact]
    public async Task BaronQuestion_GoesToTheGameMaster()
    {
        var baronyId = SeedBaronyOwnedBy("id-baron", "duke");
        SeedGameMaster("id-gm", "gm");
        var threadId = SeedQaThread(baronyId, "Czy mogę zbudować most?");

        await _dispatcher.Dispatch(new GmQuestionPosted(threadId, FromGameMaster: false));

        var sent = Assert.Single(_push.Sent);
        Assert.Equal(new[] { "id-gm" }, sent.UserIds);
    }

    [Fact]
    public async Task BattleTurn_OfTheBaronsUnit_GoesToTheBaron()
    {
        var baronyId = SeedBaronyOwnedBy("id-baron", "duke");
        SeedGameMaster("id-gm", "gm");

        await _dispatcher.Dispatch(new BattleTurnAdvanced(
            baronyId,
            Round: 3,
            SubPhase: BaronyBattleSubPhases.Movement,
            ActiveUnitLabel: "Straż Przednia",
            ActiveUnitIsEnemy: false));

        var sent = Assert.Single(_push.Sent);
        Assert.Equal(new[] { "id-baron" }, sent.UserIds);
        Assert.Equal(NotificationTopic.BattleTurn, sent.Topic);
        Assert.Contains("Straż Przednia", sent.Payload.Body);
        Assert.Contains("3", sent.Payload.Body);
        // A stable tag keeps a battle to one notification instead of a stack of them.
        Assert.Equal($"battle-{baronyId}", sent.Payload.Tag);
    }

    [Fact]
    public async Task BattleTurn_OfAnEnemyUnit_GoesToTheGameMaster()
    {
        var baronyId = SeedBaronyOwnedBy("id-baron", "duke");
        SeedGameMaster("id-gm", "gm");

        await _dispatcher.Dispatch(new BattleTurnAdvanced(
            baronyId,
            Round: 1,
            SubPhase: BaronyBattleSubPhases.Movement,
            ActiveUnitLabel: "Banda Zbójców",
            ActiveUnitIsEnemy: true));

        var sent = Assert.Single(_push.Sent);
        Assert.Equal(new[] { "id-gm" }, sent.UserIds);
    }

    [Fact]
    public async Task NothingIsSent_WhenPushIsNotConfigured()
    {
        var baronyId = SeedBaronyOwnedBy("id-baron", "duke");
        _push.Configured = false;

        Assert.Equal(0, await _dispatcher.Dispatch(new BaronyTurnResolved(baronyId, 3)));
        Assert.Empty(_push.Sent);
    }

    // ---------------- seeding ----------------

    private int SeedBaronyOwnedBy(string userId, string userName)
    {
        SeedUser(userId, userName);
        var characterId = SeedCharacter(userName, "Baron", SeedProfession());

        using var ctx = _fixture.CreateContext();
        var barony = new DA_DataAccess.BaronyData.Barony { CharacterId = characterId, Name = "Darkhold" };
        ctx.Baronies.Add(barony);
        ctx.SaveChanges();
        return barony.Id;
    }

    private int SeedThread(int baronyId, string title, string correspondent)
    {
        using var ctx = _fixture.CreateContext();
        var thread = new BaronLetterThread
        {
            BaronyId = baronyId,
            Title = title,
            CorrespondentName = correspondent,
        };
        ctx.BaronLetterThreads.Add(thread);
        ctx.SaveChanges();
        return thread.Id;
    }

    private int SeedQaThread(int baronyId, string title)
    {
        using var ctx = _fixture.CreateContext();
        var thread = new BaronQaThread { BaronyId = baronyId, Title = title };
        ctx.BaronQaThreads.Add(thread);
        ctx.SaveChanges();
        return thread.Id;
    }

    private int SeedChapter(string name, params int[] characterIds)
    {
        using var ctx = _fixture.CreateContext();
        var campaign = new Campaign { Name = "Kampania", Description = "" };
        ctx.Campaigns.Add(campaign);
        ctx.SaveChanges();

        var chapter = new Chapter { Name = name, CampaignId = campaign.Id };
        foreach (var id in characterIds)
        {
            var character = ctx.Characters.Find(id);
            if (character is not null)
                chapter.Characters.Add(character);
        }

        ctx.Chapters.Add(chapter);
        ctx.SaveChanges();
        return chapter.Id;
    }

    private int SeedProfession()
    {
        using var ctx = _fixture.CreateContext();
        var profession = new Profession { Name = "Noble", Description = "", RelatedAttributeName = "" };
        ctx.Professions.Add(profession);
        ctx.SaveChanges();
        return profession.Id;
    }

    private int SeedCharacter(string userName, string npcName, int professionId)
    {
        using var ctx = _fixture.CreateContext();
        var character = new Character
        {
            UserName = userName,
            NPCName = npcName,
            ProfessionId = professionId,
        };
        ctx.Characters.Add(character);
        ctx.SaveChanges();
        return character.Id;
    }

    private void SeedUser(string id, string userName)
    {
        using var ctx = _fixture.CreateContext();
        ctx.ApplicationUsers.Add(new ApplicationUser
        {
            Id = id,
            UserName = userName,
            NormalizedUserName = userName.ToUpperInvariant(),
        });
        ctx.SaveChanges();
    }

    private void SeedGameMaster(string id, string userName)
    {
        SeedUser(id, userName);

        using var ctx = _fixture.CreateContext();
        ctx.Roles.Add(new IdentityRole
        {
            Id = "role-gm",
            Name = SD.Role_GameMaster,
            NormalizedName = SD.Role_GameMaster.ToUpperInvariant(),
        });
        ctx.SaveChanges();

        ctx.UserRoles.Add(new IdentityUserRole<string> { UserId = id, RoleId = "role-gm" });
        ctx.SaveChanges();
    }

    /// <summary>Stands in for the real push service and records what would have gone out.</summary>
    private sealed class CapturingPushService : IPushNotificationService
    {
        public record Send(List<string> UserIds, PushNotificationDTO Payload, string? Topic);

        public List<Send> Sent { get; } = new();
        public bool Configured { get; set; } = true;

        public bool IsConfigured => Configured;
        public string PublicKey => "test-key";

        public Task SaveSubscription(string userId, WebPushSubscriptionDTO subscription, string? userAgent) =>
            Task.CompletedTask;

        public Task RemoveSubscription(string endpoint) => Task.CompletedTask;

        public Task<int> CountSubscriptions(string userId) => Task.FromResult(0);

        public Task<int> SendToUser(string userId, PushNotificationDTO payload, string? topic = null) =>
            SendToUsers(new[] { userId }, payload, topic);

        public Task<int> SendToUsers(
            IEnumerable<string> userIds,
            PushNotificationDTO payload,
            string? topic = null)
        {
            var ids = userIds.ToList();
            Sent.Add(new Send(ids, payload, topic));
            return Task.FromResult(ids.Count);
        }
    }
}
