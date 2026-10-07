using DA_Business.Services;
using DA_Business.Services.Interfaces;
using DA_Business.Tests.Fixtures;
using DA_Business.Tests.Helpers;
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

    /// <summary>Baron character of the barony seeded last — deep links name it.</summary>
    private int _baronCharacterId;

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

        await _dispatcher.Dispatch(new BaronLetterDelivered(threadId, MessageId: 1, IsInbound: true));

        var sent = Assert.Single(_push.Sent);
        Assert.Equal(new[] { "id-baron" }, sent.UserIds);
        Assert.Equal(NotificationTopic.BaronLetter, sent.Topic);
        Assert.Contains("Lord Varen", sent.Payload.Title);
        Assert.Equal("Sprawa podatków", sent.Payload.Body);
        Assert.Equal(
            $"/barony/letters?thread={threadId}&character={_baronCharacterId}",
            sent.Payload.Url);
    }

    [Fact]
    public async Task OutboundLetter_GoesToTheGameMaster_NotTheBaron()
    {
        var baronyId = SeedBaronyOwnedBy("id-baron", "duke");
        var threadId = SeedThread(baronyId, "Prośba o wsparcie", "Lord Varen");
        SeedGameMaster("id-gm", "gm");

        await _dispatcher.Dispatch(new BaronLetterDelivered(threadId, MessageId: 1, IsInbound: false));

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

        var delivered = await _dispatcher.Dispatch(new BaronLetterDelivered(threadId, MessageId: 1, IsInbound: false));

        Assert.Equal(0, delivered);
        Assert.Empty(_push.Sent);
    }

    [Fact]
    public async Task LettersFromTwoBaronies_ReachTheGameMaster_EachNamingItsOwnBarony()
    {
        var firstBarony = SeedBaronyOwnedBy("id-baron-1", "duke1", "Darkhold", "Baron Starszy");
        var firstCharacter = _baronCharacterId;
        var secondBarony = SeedBaronyOwnedBy("id-baron-2", "duke2", "Stonewatch", "Baron Młodszy");
        var secondCharacter = _baronCharacterId;
        SeedGameMaster("id-gm", "gm");

        var firstThread = SeedThread(firstBarony, "Podatki", "Lord Varen");
        var secondThread = SeedThread(secondBarony, "Granica", "Lady Mira");

        await _dispatcher.Dispatch(new BaronLetterDelivered(firstThread, MessageId: 1, IsInbound: false));
        await _dispatcher.Dispatch(new BaronLetterDelivered(secondThread, MessageId: 2, IsInbound: false));

        Assert.Equal(2, _push.Sent.Count);
        Assert.All(_push.Sent, s => Assert.Equal(new[] { "id-gm" }, s.UserIds));
        // Whichever barony the GM browsed last, the link carries the one the letter belongs to.
        Assert.Contains($"character={firstCharacter}", _push.Sent[0].Payload.Url);
        Assert.Contains($"character={secondCharacter}", _push.Sent[1].Payload.Url);
    }

    [Fact]
    public async Task TwoBaroniesOfOneAccount_BothNotifyThatAccount()
    {
        var firstBarony = SeedBaronyOwnedBy("id-baron", "duke", "Darkhold", "Baron Starszy");
        var firstCharacter = _baronCharacterId;
        var secondBarony = SeedBaronyOwnedBy(
            "id-baron", "duke", "Stonewatch", "Baron Młodszy", userAlreadySeeded: true);
        var secondCharacter = _baronCharacterId;

        await _dispatcher.Dispatch(new BaronyTurnResolved(firstBarony, 4));
        await _dispatcher.Dispatch(new BaronyTurnResolved(secondBarony, 9));

        Assert.Equal(2, _push.Sent.Count);
        Assert.All(_push.Sent, s => Assert.Equal(new[] { "id-baron" }, s.UserIds));
        Assert.Equal($"/barony?character={firstCharacter}", _push.Sent[0].Payload.Url);
        Assert.Equal($"/barony?character={secondCharacter}", _push.Sent[1].Payload.Url);
        // Separate tags, or the second barony's turn would replace the first in the tray.
        Assert.NotEqual(_push.Sent[0].Payload.Tag, _push.Sent[1].Payload.Tag);
    }

    [Fact]
    public async Task TwoLettersInOneThread_GetSeparateTags_SoBothAlertOnIos()
    {
        var baronyId = SeedBaronyOwnedBy("id-baron", "duke");
        var threadId = SeedThread(baronyId, "Sprawa podatków", "Lord Varen");

        await _dispatcher.Dispatch(new BaronLetterDelivered(threadId, MessageId: 1, IsInbound: true));
        await _dispatcher.Dispatch(new BaronLetterDelivered(threadId, MessageId: 2, IsInbound: true));

        Assert.Equal(2, _push.Sent.Count);
        Assert.NotEqual(_push.Sent[0].Payload.Tag, _push.Sent[1].Payload.Tag);
        Assert.Equal($"baron-letter-{threadId}", _push.Sent[0].Payload.ThreadKey);
        Assert.Equal($"baron-letter-{threadId}", _push.Sent[1].Payload.ThreadKey);
    }

    [Fact]
    public async Task PetitionerAudienceReply_GoesToTheBaron()
    {
        var baronyId = SeedBaronyOwnedBy("id-baron", "duke");
        var audienceId = SeedAudience(baronyId, "Prośba o zboże", "Farmer Tobin");

        await _dispatcher.Dispatch(new BaronAudienceExchangePosted(audienceId, ExchangeId: 1, IsFromPetitioner: true));

        var sent = Assert.Single(_push.Sent);
        Assert.Equal(new[] { "id-baron" }, sent.UserIds);
        Assert.Equal(NotificationTopic.BaronAudience, sent.Topic);
        Assert.Contains("Farmer Tobin", sent.Payload.Title);
        Assert.Equal("Prośba o zboże", sent.Payload.Body);
        Assert.Equal(
            $"/barony/audience-hall?audience={audienceId}&character={_baronCharacterId}",
            sent.Payload.Url);
        Assert.Equal($"baron-audience-{audienceId}-1", sent.Payload.Tag);
        Assert.Equal($"baron-audience-{audienceId}", sent.Payload.ThreadKey);
    }

    [Fact]
    public async Task BaronAudienceReply_GoesToTheGameMaster()
    {
        var baronyId = SeedBaronyOwnedBy("id-baron", "duke");
        var audienceId = SeedAudience(baronyId, "Prośba o zboże", "Farmer Tobin");
        SeedGameMaster("id-gm", "gm");

        await _dispatcher.Dispatch(new BaronAudienceExchangePosted(audienceId, ExchangeId: 1, IsFromPetitioner: false));

        var sent = Assert.Single(_push.Sent);
        Assert.Equal(new[] { "id-gm" }, sent.UserIds);
        Assert.Contains("Darkhold", sent.Payload.Title);
        Assert.Contains("Farmer Tobin", sent.Payload.Body);
        Assert.Contains("Prośba o zboże", sent.Payload.Body);
    }

    [Fact]
    public async Task BaronAudienceReply_IsSkipped_WhenThereIsNoGameMaster()
    {
        var baronyId = SeedBaronyOwnedBy("id-baron", "duke");
        var audienceId = SeedAudience(baronyId, "Prośba o zboże", "Farmer Tobin");

        var delivered = await _dispatcher.Dispatch(
            new BaronAudienceExchangePosted(audienceId, ExchangeId: 1, IsFromPetitioner: false));

        Assert.Equal(0, delivered);
        Assert.Empty(_push.Sent);
    }

    [Fact]
    public async Task UnknownAudience_IsIgnored()
    {
        Assert.Equal(0, await _dispatcher.Dispatch(
            new BaronAudienceExchangePosted(9999, ExchangeId: 1, IsFromPetitioner: true)));
        Assert.Empty(_push.Sent);
    }

    [Fact]
    public async Task UnknownThread_IsIgnored()
    {
        Assert.Equal(0, await _dispatcher.Dispatch(new BaronLetterDelivered(9999, MessageId: 1, IsInbound: true)));
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
        Assert.Equal($"/barony?character={_baronCharacterId}", sent.Payload.Url);
    }

    [Fact]
    public async Task ChapterPost_NotifiesOtherParticipantsAndTheGameMaster()
    {
        var professionId = SeedProfession();
        var authorId = SeedCharacter("author-user", "Autor", professionId);
        var readerId = SeedCharacter("reader-user", "Czytelnik", professionId);
        SeedUser("id-author", "author-user");
        SeedUser("id-reader", "reader-user");
        SeedGameMaster("id-gm", "gm");
        var chapterId = SeedChapter("Rozdział I", authorId, readerId);

        await _dispatcher.Dispatch(new ChapterPostAdded(chapterId, PostId: 42, authorId));

        var sent = Assert.Single(_push.Sent);
        Assert.Equal(new[] { "id-reader", "id-gm" }.OrderBy(x => x), sent.UserIds.OrderBy(x => x));
        Assert.Equal(NotificationTopic.Posts, sent.Topic);
        Assert.Contains("Rozdział I", sent.Payload.Title);
        Assert.Contains("Autor", sent.Payload.Body);
        Assert.Equal($"/chapter/{chapterId}", sent.Payload.Url);
        Assert.Equal($"chapter-{chapterId}-42", sent.Payload.Tag);
        Assert.Equal($"chapter-{chapterId}", sent.Payload.ThreadKey);
    }

    [Fact]
    public async Task ChapterPost_FromGameMasterActor_DoesNotNotifyOtherGameMasters()
    {
        var professionId = SeedProfession();
        var gmActorId = SeedCharacter("gm-actor", SD.GameMaster_NPCName, professionId);
        var readerId = SeedCharacter("reader-user", "Czytelnik", professionId);
        SeedUser("id-reader", "reader-user");
        SeedGameMaster("id-gm", "gm");
        SeedAdmin("id-admin", "admin");
        var chapterId = SeedChapter("Rozdział I", gmActorId, readerId);

        await _dispatcher.Dispatch(new ChapterPostAdded(chapterId, PostId: 7, gmActorId));

        var sent = Assert.Single(_push.Sent);
        Assert.Equal(new[] { "id-reader" }, sent.UserIds);
    }

    [Fact]
    public async Task TwoPostsInOneChapter_GetSeparateTags_SoBothAlertOnIos()
    {
        var professionId = SeedProfession();
        var authorId = SeedCharacter("author-user", "Autor", professionId);
        var readerId = SeedCharacter("reader-user", "Czytelnik", professionId);
        SeedUser("id-author", "author-user");
        SeedUser("id-reader", "reader-user");
        var chapterId = SeedChapter("Rozdział I", authorId, readerId);

        await _dispatcher.Dispatch(new ChapterPostAdded(chapterId, PostId: 1, authorId));
        await _dispatcher.Dispatch(new ChapterPostAdded(chapterId, PostId: 2, authorId));

        Assert.Equal(2, _push.Sent.Count);
        Assert.NotEqual(_push.Sent[0].Payload.Tag, _push.Sent[1].Payload.Tag);
        Assert.Equal($"chapter-{chapterId}", _push.Sent[0].Payload.ThreadKey);
        Assert.Equal($"chapter-{chapterId}", _push.Sent[1].Payload.ThreadKey);
    }

    [Fact]
    public async Task GmAnswer_GoesToTheBaronWhoAsked()
    {
        var baronyId = SeedBaronyOwnedBy("id-baron", "duke");
        SeedGameMaster("id-gm", "gm");
        var threadId = SeedQaThread(baronyId, "Czy mogę zbudować most?");

        await _dispatcher.Dispatch(new GmQuestionPosted(threadId, MessageId: 1, FromGameMaster: true));

        var sent = Assert.Single(_push.Sent);
        Assert.Equal(new[] { "id-baron" }, sent.UserIds);
        Assert.Equal(NotificationTopic.GmQuestion, sent.Topic);
        Assert.Equal("Czy mogę zbudować most?", sent.Payload.Body);
        Assert.Equal(
            $"/barony/notes?tab=qa&thread={threadId}&character={_baronCharacterId}",
            sent.Payload.Url);
    }

    [Fact]
    public async Task BaronQuestion_GoesToTheGameMaster()
    {
        var baronyId = SeedBaronyOwnedBy("id-baron", "duke");
        SeedGameMaster("id-gm", "gm");
        var threadId = SeedQaThread(baronyId, "Czy mogę zbudować most?");

        await _dispatcher.Dispatch(new GmQuestionPosted(threadId, MessageId: 1, FromGameMaster: false));

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

    private int SeedBaronyOwnedBy(
        string userId,
        string userName,
        string baronyName = "Darkhold",
        string baronName = "Baron",
        bool userAlreadySeeded = false)
    {
        if (!userAlreadySeeded)
            SeedUser(userId, userName);

        var characterId = SeedCharacter(userName, baronName, SeedProfession());
        _baronCharacterId = characterId;

        using var ctx = _fixture.CreateContext();
        var barony = new DA_DataAccess.BaronyData.Barony { CharacterId = characterId, Name = baronyName };
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

    private int SeedAudience(int baronyId, string title, string petitioner)
    {
        using var ctx = _fixture.CreateContext();
        var audience = new BaronAudience
        {
            BaronyId = baronyId,
            Title = title,
            PetitionerName = petitioner,
            Kind = BaronAudienceKind.Audience,
            Status = BaronAudienceStatus.InProgress,
            TurnNumber = 1,
        };
        ctx.BaronAudiences.Add(audience);
        ctx.SaveChanges();
        return audience.Id;
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
        EnsureRole("role-gm", SD.Role_GameMaster);

        using var ctx = _fixture.CreateContext();
        if (!ctx.UserRoles.Any(ur => ur.UserId == id && ur.RoleId == "role-gm"))
        {
            ctx.UserRoles.Add(new IdentityUserRole<string> { UserId = id, RoleId = "role-gm" });
            ctx.SaveChanges();
        }
    }

    private void SeedAdmin(string id, string userName)
    {
        SeedUser(id, userName);
        EnsureRole("role-admin", SD.Role_Admin);

        using var ctx = _fixture.CreateContext();
        if (!ctx.UserRoles.Any(ur => ur.UserId == id && ur.RoleId == "role-admin"))
        {
            ctx.UserRoles.Add(new IdentityUserRole<string> { UserId = id, RoleId = "role-admin" });
            ctx.SaveChanges();
        }
    }

    private void EnsureRole(string id, string name)
    {
        using var ctx = _fixture.CreateContext();
        if (ctx.Roles.Any(r => r.Id == id || r.Name == name))
            return;

        ctx.Roles.Add(new IdentityRole
        {
            Id = id,
            Name = name,
            NormalizedName = name.ToUpperInvariant(),
        });
        ctx.SaveChanges();
    }
}
