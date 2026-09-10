using DA_Business.Services;
using DA_Business.Services.Interfaces;
using DA_Business.Tests.Fixtures;
using DA_Common.Barony;
using DA_DataAccess;
using DA_DataAccess.BaronyData;
using DA_Models.BaronyModels;
using Microsoft.Extensions.Logging.Abstractions;

namespace DA_Business.Tests.Barony;

/// <summary>
/// The chronicle must survive anything: a failed entry may never break the action being logged,
/// and entries have to stay grouped under the turn they belong to.
/// </summary>
public class BaronyLogServiceTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;
    private readonly BaronyLogService _service;

    public BaronyLogServiceTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
        _fixture.ResetDatabase();
        _service = new BaronyLogService(
            _fixture.DbContextFactory,
            new StubUserService("gm@example.com", isAdminOrMg: true),
            NullLogger<BaronyLogService>.Instance);
    }

    [Fact]
    public async Task Log_StampsEntryWithBaronyTurnAndActor()
    {
        var baronyId = await CreateBarony(turnNumber: 7, season: "Summer", year: 626);

        await _service.Log(baronyId, BaronyLogCategory.Resources, "Treasury topped up.");

        var entries = await _service.GetEntries(baronyId, 7);
        var entry = Assert.Single(entries);
        Assert.Equal(7, entry.TurnNumber);
        Assert.Equal("Summer", entry.Season);
        Assert.Equal(626, entry.Year);
        Assert.Equal("gm@example.com", entry.Actor);
        Assert.Equal(BaronyLogActorRole.GameMaster, entry.ActorRole);
    }

    [Fact]
    public async Task Attach_UsesStampWhenEntryBelongsToAnEarlierTurn()
    {
        var baronyId = await CreateBarony(turnNumber: 9, season: "Fall", year: 626);

        await using var ctx = _fixture.CreateContext();
        var barony = ctx.Baronies.First(b => b.Id == baronyId);
        await _service.Attach(
            ctx, barony, BaronyLogCategory.TurnResolve, "Turn 8 resolved.",
            details: "Full report.", important: true,
            stamp: new BaronyLogStamp(8, 626, 7, "Summer"));
        await ctx.SaveChangesAsync();

        var entries = await _service.GetEntries(baronyId, 8);
        var entry = Assert.Single(entries);
        Assert.Equal("Summer", entry.Season);
        Assert.True(entry.IsImportant);
        Assert.Equal("Full report.", entry.Details);
    }

    [Fact]
    public async Task GetTurns_GroupsNewestFirstWithCounts()
    {
        var baronyId = await CreateBarony(turnNumber: 3);
        await _service.Log(baronyId, BaronyLogCategory.Projects, "Project created.");
        await _service.Log(baronyId, BaronyLogCategory.Army, "Unit added.");
        await AdvanceTurn(baronyId, 4);
        await _service.Log(baronyId, BaronyLogCategory.Court, "Advisor hired.");

        var turns = await _service.GetTurns(baronyId);

        Assert.Collection(turns,
            t => Assert.Equal((4, 1), (t.TurnNumber, t.EntryCount)),
            t => Assert.Equal((3, 2), (t.TurnNumber, t.EntryCount)));
    }

    [Fact]
    public async Task GetEntries_FiltersByCategorySearchAndImportance()
    {
        var baronyId = await CreateBarony(turnNumber: 2);
        await _service.Log(baronyId, BaronyLogCategory.Projects, "Granary funded.");
        await _service.Log(baronyId, BaronyLogCategory.Army, "Militia disbanded.", important: true);

        var byCategory = await _service.GetEntries(baronyId, 2,
            new BaronyLogFilterDTO { Categories = new HashSet<string> { BaronyLogCategory.Army } });
        Assert.Equal("Militia disbanded.", Assert.Single(byCategory).Summary);

        var bySearch = await _service.GetEntries(baronyId, 2,
            new BaronyLogFilterDTO { Search = "granary" });
        Assert.Equal("Granary funded.", Assert.Single(bySearch).Summary);

        var important = await _service.GetEntries(baronyId, 2,
            new BaronyLogFilterDTO { ImportantOnly = true });
        Assert.Equal("Militia disbanded.", Assert.Single(important).Summary);
    }

    [Fact]
    public async Task Log_OnMissingBaronySwallowsTheEntry()
    {
        await _service.Log(999_999, BaronyLogCategory.Other, "Nowhere to write this.");

        await using var ctx = _fixture.CreateContext();
        Assert.Empty(ctx.BaronyLogEntries);
    }

    private async Task<int> CreateBarony(int turnNumber, string season = "Spring", int year = 625)
    {
        await using var ctx = _fixture.CreateContext();
        var barony = new global::DA_DataAccess.BaronyData.Barony
        {
            Name = "Test barony",
            TurnNumber = turnNumber,
            Season = season,
            Year = year,
            Month = 1,
        };
        ctx.Baronies.Add(barony);
        await ctx.SaveChangesAsync();
        return barony.Id;
    }

    private async Task AdvanceTurn(int baronyId, int turnNumber)
    {
        await using var ctx = _fixture.CreateContext();
        var barony = ctx.Baronies.First(b => b.Id == baronyId);
        barony.TurnNumber = turnNumber;
        await ctx.SaveChangesAsync();
    }

    private sealed class StubUserService : IUserService
    {
        private readonly UserInfo _user;

        public StubUserService(string userName, bool isAdminOrMg) =>
            _user = new UserInfo { UserName = userName, IsAdminOrMG = isAdminOrMg };

        public Task<UserInfo?> GetUserInfo() => Task.FromResult<UserInfo?>(_user);
        public Task SetSelectedCharId(int charId) => Task.CompletedTask;
        public Task<bool> IsAuthenticated() => Task.FromResult(true);
        public Task LogOut() => Task.CompletedTask;
        public Task<string?> GetBaronyTabOrderJson() => Task.FromResult<string?>(null);
        public Task SetBaronyTabOrderJson(string? json) => Task.CompletedTask;
    }
}
