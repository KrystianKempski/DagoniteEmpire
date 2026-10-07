using DA_Business.Repository.BaronyRepos;
using DA_Business.Services;
using DA_Business.Tests.Fixtures;
using DA_Business.Tests.Helpers;
using DA_Common.Barony;
using DA_DataAccess;
using DA_DataAccess.BaronyData;
using DA_DataAccess.CharacterClasses;

namespace DA_Business.Tests.Repositories;

public class LetterInboxBadgeTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;
    private readonly BaronyRepository _repo;
    private int _lastCharacterId;

    public LetterInboxBadgeTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
        _repo = new BaronyRepository(
            _fixture.DbContextFactory,
            characters: null!,
            new RecordingNotificationQueue(),
            new NullBaronyLogService());
    }

    [Fact]
    public async Task BaronBadge_SkipsTurnResolvingBarony_KeepsOpenableMail()
    {
        var locked = SeedBarony("duke", "Lockedhold", "Baron Locked", turnResolving: true);
        var lockedCharacter = _lastCharacterId;
        var open = SeedBarony("duke", "Openhold", "Baron Open", userAlreadySeeded: true);
        var openCharacter = _lastCharacterId;

        SeedInboundLetter(locked, "Locked thread", sentAt: DateTime.UtcNow);
        var openThread = SeedInboundLetter(open, "Open thread", sentAt: DateTime.UtcNow.AddMinutes(-1));

        var badge = await _repo.GetLetterInboxBadgeForBaron(new[] { locked, open });

        Assert.Equal(1, badge.UnreadCount);
        Assert.Equal(openThread, badge.LatestThreadId);
        Assert.Equal(open, badge.BaronyId);
        Assert.Equal(openCharacter, badge.CharacterId);
        Assert.NotEqual(lockedCharacter, badge.CharacterId);
    }

    [Fact]
    public async Task BaronBadge_CountsAcrossTwoOpenBaronies()
    {
        var first = SeedBarony("duke-a", "Darkhold", "Baron A");
        var second = SeedBarony("duke-a", "Stonewatch", "Baron B", userAlreadySeeded: true);

        SeedInboundLetter(first, "A", sentAt: DateTime.UtcNow.AddMinutes(-5));
        var newest = SeedInboundLetter(second, "B", sentAt: DateTime.UtcNow);

        var badge = await _repo.GetLetterInboxBadgeForBaron(new[] { first, second });

        Assert.Equal(2, badge.UnreadCount);
        Assert.Equal(newest, badge.LatestThreadId);
        Assert.Equal(second, badge.BaronyId);
    }

    private int SeedBarony(
        string userName,
        string baronyName,
        string baronName,
        bool turnResolving = false,
        bool userAlreadySeeded = false)
    {
        if (!userAlreadySeeded)
        {
            using var users = _fixture.CreateContext();
            users.ApplicationUsers.Add(new ApplicationUser
            {
                Id = $"id-{userName}",
                UserName = userName,
                NormalizedUserName = userName.ToUpperInvariant(),
                Email = $"{userName}@test.local",
                NormalizedEmail = $"{userName}@test.local".ToUpperInvariant(),
                EmailConfirmed = true,
                Name = userName,
            });
            users.SaveChanges();
        }

        using var ctx = _fixture.CreateContext();
        var profession = new Profession { Name = "Noble", Description = "", RelatedAttributeName = "" };
        ctx.Professions.Add(profession);
        ctx.SaveChanges();

        var character = new Character
        {
            UserName = userName,
            NPCName = baronName,
            ProfessionId = profession.Id,
        };
        ctx.Characters.Add(character);
        ctx.SaveChanges();
        _lastCharacterId = character.Id;

        var barony = new DA_DataAccess.BaronyData.Barony
        {
            CharacterId = character.Id,
            Name = baronyName,
            TurnResolving = turnResolving,
        };
        ctx.Baronies.Add(barony);
        ctx.SaveChanges();
        return barony.Id;
    }

    private int SeedInboundLetter(int baronyId, string title, DateTime sentAt)
    {
        using var ctx = _fixture.CreateContext();
        var thread = new BaronLetterThread
        {
            BaronyId = baronyId,
            Title = title,
            CorrespondentName = "Lord Varen",
        };
        ctx.BaronLetterThreads.Add(thread);
        ctx.SaveChanges();

        ctx.BaronLetterMessages.Add(new BaronLetterMessage
        {
            ThreadId = thread.Id,
            BodyHtml = "<p>test</p>",
            Status = BaronLetterStatus.Sent,
            IsInbound = true,
            SeenByBaron = false,
            SeenByGm = true,
            SentAtUtc = sentAt,
            CreatedAtUtc = sentAt,
            UpdatedAtUtc = sentAt,
        });
        ctx.SaveChanges();
        return thread.Id;
    }
}
