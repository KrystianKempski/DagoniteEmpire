using DA_Business.Repository.BaronyRepos;
using DA_Business.Tests.Fixtures;
using DA_Business.Tests.Helpers;
using DA_Common.Notifications;
using DA_Models.BaronyModels;

namespace DA_Business.Tests.Notifications;

/// <summary>
/// Every token drag saves the whole battle map, so the repository has to tell a real turn change
/// apart from an ordinary save. Getting this wrong means a phone buzzing on every mouse move.
/// </summary>
public class BattleTurnNotificationTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;
    private readonly RecordingNotificationQueue _queue = new();
    private readonly BaronyBattleMapRepository _repository;

    public BattleTurnNotificationTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
        _fixture.ResetDatabase();
        _repository = new BaronyBattleMapRepository(_fixture.DbContextFactory, _queue);
    }

    [Fact]
    public async Task MovingTokensWithoutChangingTheTurn_NotifiesNobody()
    {
        var map = await StartedBattle();

        map.Tokens[0].X += 3;
        await _repository.Update(map);

        Assert.Empty(_queue.Raised);
    }

    [Fact]
    public async Task StartingTheBattle_NotifiesTheOwnerOfTheFirstUnit()
    {
        var map = await _repository.GetOrCreate(baronyId: 1);
        map.Phase = BaronyBattlePhases.Battle;
        map.IsActive = true;
        map.Tokens = Tokens();
        map.TurnState = new BaronyBattleTurnStateDTO
        {
            InitiativeOrder = new List<string> { "t1", "t2" },
            CurrentIndex = 0,
            SubPhase = BaronyBattleSubPhases.Movement,
            Round = 1,
        };

        await _repository.Update(map);

        var battle = Assert.IsType<BattleTurnAdvanced>(Assert.Single(_queue.Raised));
        Assert.Equal("Straż Przednia", battle.ActiveUnitLabel);
        Assert.False(battle.ActiveUnitIsEnemy);
    }

    [Fact]
    public async Task AdvancingToTheNextUnit_NotifiesOnce()
    {
        var map = await StartedBattle();

        map.TurnState.CurrentIndex = 1;
        await _repository.Update(map);

        var raised = Assert.Single(_queue.Raised);
        var battle = Assert.IsType<BattleTurnAdvanced>(raised);
        Assert.Equal("Banda Zbójców", battle.ActiveUnitLabel);
        Assert.True(battle.ActiveUnitIsEnemy);
        Assert.Equal(BaronyBattleSubPhases.Movement, battle.SubPhase);
        Assert.Equal(1, battle.Round);
    }

    [Fact]
    public async Task OpeningAttackPlanning_NotifiesTheBaronSide()
    {
        var map = await StartedBattle();

        map.TurnState.SubPhase = BaronyBattleSubPhases.AttackPlanning;
        await _repository.Update(map);

        var battle = Assert.IsType<BattleTurnAdvanced>(Assert.Single(_queue.Raised));
        Assert.Equal(BaronyBattleSubPhases.AttackPlanning, battle.SubPhase);
        Assert.False(battle.ActiveUnitIsEnemy);
        Assert.Null(battle.ActiveUnitLabel);
    }

    [Fact]
    public async Task CombatResolution_NotifiesNobody()
    {
        var map = await StartedBattle();

        map.TurnState.SubPhase = BaronyBattleSubPhases.Combat;
        await _repository.Update(map);

        Assert.Empty(_queue.Raised);
    }

    [Fact]
    public async Task SetupPhase_NotifiesNobody()
    {
        var map = await _repository.GetOrCreate(baronyId: 1);
        map.Phase = BaronyBattlePhases.Setup;
        map.Tokens = Tokens();
        map.TurnState = new BaronyBattleTurnStateDTO
        {
            InitiativeOrder = new List<string> { "t1", "t2" },
            CurrentIndex = 1,
        };

        await _repository.Update(map);

        Assert.Empty(_queue.Raised);
    }

    /// <summary>
    /// A battle already running, pointer on the baron's own first unit. The notification for
    /// entering the battle is dropped so each test starts from a quiet queue.
    /// </summary>
    private async Task<BaronyBattleMapDTO> StartedBattle()
    {
        var map = await _repository.GetOrCreate(baronyId: 1);
        map.Phase = BaronyBattlePhases.Battle;
        map.IsActive = true;
        map.Tokens = Tokens();
        map.TurnState = new BaronyBattleTurnStateDTO
        {
            InitiativeOrder = new List<string> { "t1", "t2" },
            CurrentIndex = 0,
            SubPhase = BaronyBattleSubPhases.Movement,
            Round = 1,
        };
        var saved = await _repository.Update(map);
        _queue.Raised.Clear();
        return saved;
    }

    private static List<BaronyBattleTokenDTO> Tokens() => new()
    {
        new BaronyBattleTokenDTO { Id = "t1", Label = "Straż Przednia", IsEnemy = false, X = 2, Y = 2 },
        new BaronyBattleTokenDTO { Id = "t2", Label = "Banda Zbójców", IsEnemy = true, X = 10, Y = 8 },
    };
}
