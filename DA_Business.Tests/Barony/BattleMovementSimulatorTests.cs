using DA_Common.Barony.Battle;

namespace DA_Business.Tests.Barony;

public class BattleMovementSimulatorTests
{
    private const int Ally = 0;
    private const int Enemy = 1;

    // --- Cost and timing ---

    [Fact]
    public void StraightStepCostsOneMovePointAndDiagonalCostsOneAndAHalf()
    {
        Assert.Equal(2, BattleMovementRules.StepHalfCost(0, 0, 1, 0, entersDifficult: false));
        Assert.Equal(3, BattleMovementRules.StepHalfCost(0, 0, 1, 1, entersDifficult: false));
        Assert.Equal(4, BattleMovementRules.StepHalfCost(0, 0, 1, 0, entersDifficult: true));
        Assert.Equal(6, BattleMovementRules.StepHalfCost(0, 0, 1, 1, entersDifficult: true));
    }

    [Fact]
    public void MoveBudgetMatchesTheDocumentedDiagonalTable()
    {
        // Move 3 buys two diagonals, Move 4 buys three, Move 6 buys four.
        Assert.Equal(2, BattleMovementRules.MoveHalfBudget(3) / BattleMovementRules.DiagonalStepCost);
        Assert.Equal(3, BattleMovementRules.MoveHalfBudget(4) / BattleMovementRules.DiagonalStepCost);
        Assert.Equal(3, BattleMovementRules.MoveHalfBudget(5) / BattleMovementRules.DiagonalStepCost);
        Assert.Equal(4, BattleMovementRules.MoveHalfBudget(6) / BattleMovementRules.DiagonalStepCost);
    }

    [Fact]
    public void SpeedScalesWithMoveSoFasterUnitsCoverMoreGroundInTheSameTime()
    {
        var slow = BattleMovementRules.StepDurationMs(4, BattleMovementRules.OrthogonalStepCost);
        var fast = BattleMovementRules.StepDurationMs(8, BattleMovementRules.OrthogonalStepCost);

        Assert.Equal(700, slow);
        Assert.Equal(350, fast);
    }

    [Fact]
    public void DiagonalStepTakesHalfAgainAsLongSoPhysicalSpeedStaysEqual()
    {
        var straight = BattleMovementRules.StepDurationMs(4, BattleMovementRules.OrthogonalStepCost);
        var diagonal = BattleMovementRules.StepDurationMs(4, BattleMovementRules.DiagonalStepCost);

        Assert.Equal(straight * 3 / 2, diagonal);
    }

    [Fact]
    public void EnteringDifficultTerrainDoublesBothCostAndDuration()
    {
        var terrain = new BattleMovementTerrain(10, 10, difficult: new[] { new BattleGridPoint(1, 0) });
        var result = Simulate(terrain, Mover("a", Ally, move: 4, initiative: 10, (0, 0), (1, 0)));

        var leg = Assert.Single(result.Legs);
        Assert.Equal(1400, leg.DurationMs);
        Assert.Equal(2, result.Outcomes["a"].SpentMovePoints);
    }

    // --- Undisturbed movement ---

    [Fact]
    public void UnopposedUnitWalksItsWholeRoute()
    {
        var result = Simulate(Mover("a", Ally, move: 4, initiative: 10, (0, 0), (1, 0), (2, 0)));

        var outcome = result.Outcomes["a"];
        Assert.Equal(BattleMovementStopReason.RouteComplete, outcome.StopReason);
        Assert.Equal((2, 0), (outcome.X, outcome.Y));
        Assert.Equal(2, outcome.TilesTravelled);
        Assert.Equal(2, outcome.SpentMovePoints);
        Assert.Equal(2, outcome.RemainingMove);
    }

    [Fact]
    public void EveryoneSetsOffAtTheSameMoment()
    {
        var result = Simulate(
            Mover("a", Ally, move: 4, initiative: 20, (0, 0), (0, 1)),
            Mover("b", Ally, move: 9, initiative: 3, (5, 0), (5, 1)),
            Mover("c", Enemy, move: 2, initiative: 11, (9, 0), (9, 1)));

        Assert.All(result.Legs.Where(l => l.FromY == 0), leg => Assert.Equal(0, leg.StartMs));
    }

    [Fact]
    public void UnitRunsOutOfMovePointsBeforeTheRouteEnds()
    {
        var result = Simulate(Mover("a", Ally, move: 1, initiative: 10, (0, 0), (1, 0), (2, 0)));

        var outcome = result.Outcomes["a"];
        Assert.Equal(BattleMovementStopReason.OutOfMovePoints, outcome.StopReason);
        Assert.Equal((1, 0), (outcome.X, outcome.Y));
    }

    // --- Hostile contact ---

    [Fact]
    public void UnitsMarchingHeadOnMeetInTheMiddleWithoutPassingThroughEachOther()
    {
        var result = Simulate(
            Mover("a", Ally, move: 4, initiative: 15, (0, 0), (1, 0), (2, 0), (3, 0)),
            Mover("b", Enemy, move: 4, initiative: 10, (4, 0), (3, 0), (2, 0), (1, 0)));

        var a = result.Outcomes["a"];
        var b = result.Outcomes["b"];

        Assert.Equal(BattleMovementStopReason.EnemyContact, a.StopReason);
        Assert.Equal(BattleMovementStopReason.EnemyContact, b.StopReason);
        Assert.True(a.X < b.X, "units must not swap sides");
        AssertNoOverlap(result);
        Assert.True(BattleMovementRules.FootprintsAdjacent(a.X, a.Y, 1, b.X, b.Y, 1));
    }

    [Fact]
    public void UnitHaltedByAnEnemyTurnsToFaceIt()
    {
        var result = Simulate(
            Mover("a", Ally, move: 4, initiative: 15, (0, 0), (1, 0)),
            Mover("b", Enemy, move: 4, initiative: 10, (2, 0), (1, 0)));

        var b = result.Outcomes["b"];
        Assert.Equal("a", b.EngagedWithMoverId);
        Assert.Equal(6, b.Facing); // West, toward the unit that stopped it
    }

    [Fact]
    public void HigherInitiativeTakesTheContestedTile()
    {
        var result = Simulate(
            Mover("a", Ally, move: 4, initiative: 15, (0, 0), (1, 0)),
            Mover("b", Enemy, move: 4, initiative: 10, (2, 0), (1, 0)));

        Assert.Equal((1, 0), (result.Outcomes["a"].X, result.Outcomes["a"].Y));
        Assert.Equal((2, 0), (result.Outcomes["b"].X, result.Outcomes["b"].Y));
        Assert.Equal(BattleMovementStopReason.EnemyContact, result.Outcomes["b"].StopReason);
    }

    [Fact]
    public void ChargeTakesTheContestedTileEvenAgainstHigherInitiative()
    {
        var charger = Mover("b", Enemy, move: 4, initiative: 10, (2, 0), (1, 0));
        var result = Simulate(
            Mover("a", Ally, move: 4, initiative: 15, (0, 0), (1, 0)),
            Charging(charger, chargeStartStep: 0));

        Assert.Equal((1, 0), (result.Outcomes["b"].X, result.Outcomes["b"].Y));
        Assert.Equal(BattleMovementStopReason.EnemyContact, result.Outcomes["a"].StopReason);
    }

    [Fact]
    public void EnemyBlocksWithItsWholeFootprint()
    {
        var big = Mover("b", Enemy, move: 0, initiative: 10, (2, 0));
        var result = Simulate(
            Mover("a", Ally, move: 4, initiative: 15, (0, 0), (1, 0), (2, 0)),
            Sized(big, size: 2));

        var a = result.Outcomes["a"];
        Assert.Equal(BattleMovementStopReason.EnemyContact, a.StopReason);
        Assert.Equal((1, 0), (a.X, a.Y));
        AssertNoOverlap(result, sizes: new Dictionary<string, int> { ["b"] = 2 });
    }

    // --- Friendly traffic ---

    [Fact]
    public void UnitWaitsForAFriendlyUnitToClearTheWayAndThenCarriesOn()
    {
        // "b" is processed first (higher initiative) and finds "a" still standing in its way.
        var result = Simulate(
            Mover("a", Ally, move: 4, initiative: 10, (1, 0), (2, 0), (3, 0)),
            Mover("b", Ally, move: 4, initiative: 20, (0, 0), (1, 0), (2, 0)));

        var b = result.Outcomes["b"];
        Assert.Equal(BattleMovementStopReason.RouteComplete, b.StopReason);
        Assert.Equal((2, 0), (b.X, b.Y));

        var firstLeg = result.Legs.First(l => l.MoverId == "b");
        Assert.True(firstLeg.StartMs > 0, "the blocked unit should have waited before setting off");
        AssertNoOverlap(result);
    }

    [Fact]
    public void UnitGivesUpWhenTheFriendlyUnitInFrontNeverMoves()
    {
        // The route ends on the comrade's tile, so there is no open ground to work through to.
        var result = Simulate(
            Mover("a", Ally, move: 0, initiative: 10, (1, 0)),
            Mover("b", Ally, move: 4, initiative: 20, (0, 0), (1, 0)));

        var b = result.Outcomes["b"];
        Assert.Equal(BattleMovementStopReason.BlockedByAlly, b.StopReason);
        Assert.Equal((0, 0), (b.X, b.Y));
        Assert.Equal(0, b.TilesTravelled);
        Assert.Contains(result.Events, e => e.Kind == BattleMovementEventKind.AllyDeadlock && e.MoverId == "b");
    }

    // --- Working through friendly bodies ---

    [Fact]
    public void UnitWorksItsWayThroughAFriendlyUnitThatWillNeverMove()
    {
        var result = Simulate(
            Mover("a", Ally, move: 0, initiative: 10, (1, 0)),
            Mover("b", Ally, move: 4, initiative: 20, (0, 0), (1, 0), (2, 0)));

        var b = result.Outcomes["b"];
        Assert.Equal(BattleMovementStopReason.RouteComplete, b.StopReason);
        Assert.Equal((2, 0), (b.X, b.Y));
        Assert.Equal((1, 0), (result.Outcomes["a"].X, result.Outcomes["a"].Y));
        AssertNoOverlap(result);
    }

    [Fact]
    public void SqueezingPastAFriendlyBodyCostsAsMuchAsDifficultGround()
    {
        var result = Simulate(
            Mover("a", Ally, move: 0, initiative: 10, (1, 0)),
            Mover("b", Ally, move: 4, initiative: 20, (0, 0), (1, 0), (2, 0)));

        // Two straight steps would cost 2 move points; the tile held by "a" is charged double.
        Assert.Equal(3, result.Outcomes["b"].SpentMovePoints);
    }

    [Fact]
    public void UnitHaltsAgainstAFriendlyUnitPinnedInMelee()
    {
        var pinned = Pinned(Mover("a", Ally, move: 0, initiative: 10, (1, 0)));
        var result = Simulate(
            pinned,
            Mover("b", Ally, move: 4, initiative: 20, (0, 0), (1, 0), (2, 0)));

        var b = result.Outcomes["b"];
        Assert.Equal(BattleMovementStopReason.BlockedByAlly, b.StopReason);
        Assert.Equal((0, 0), (b.X, b.Y));
    }

    [Fact]
    public void UnitWithoutTheMovePointsToComeOutTheOtherSideStaysPut()
    {
        // Move 2 buys five half-points: 4 for the doubled body tile leaves too little for the exit.
        var result = Simulate(
            Mover("a", Ally, move: 0, initiative: 10, (1, 0)),
            Mover("b", Ally, move: 2, initiative: 20, (0, 0), (1, 0), (2, 0)));

        var b = result.Outcomes["b"];
        Assert.Equal(BattleMovementStopReason.BlockedByAlly, b.StopReason);
        Assert.Equal((0, 0), (b.X, b.Y));
    }

    [Fact]
    public void TwoFriendlyBodiesInARowAreClearedInOneMotion()
    {
        var result = Simulate(
            Mover("a", Ally, move: 0, initiative: 10, (1, 0)),
            Mover("b", Ally, move: 0, initiative: 9, (2, 0)),
            Mover("c", Ally, move: 6, initiative: 20, (0, 0), (1, 0), (2, 0), (3, 0)));

        var c = result.Outcomes["c"];
        Assert.Equal((3, 0), (c.X, c.Y));
        // Two doubled body tiles plus a clear exit step: 4 + 4 + 2 half-points.
        Assert.Equal(5, c.SpentMovePoints);
        AssertNoOverlap(result);
    }

    [Fact]
    public void MarchingColumnStillFilesThroughInsteadOfShovingPastEachOther()
    {
        // "b" is processed first and finds "a" in its way, but "a" clears the tile at once.
        var result = Simulate(
            Mover("a", Ally, move: 4, initiative: 10, (1, 0), (2, 0), (3, 0)),
            Mover("b", Ally, move: 4, initiative: 20, (0, 0), (1, 0), (2, 0)));

        // Nobody was shoved through, so nobody paid difficult-ground rates.
        Assert.Equal(2, result.Outcomes["a"].SpentMovePoints);
        Assert.Equal(2, result.Outcomes["b"].SpentMovePoints);
        Assert.Equal((3, 0), (result.Outcomes["a"].X, result.Outcomes["a"].Y));
        Assert.Equal((2, 0), (result.Outcomes["b"].X, result.Outcomes["b"].Y));
        AssertNoOverlap(result);
    }

    [Fact]
    public void HostileUnitsAreStillImpassable()
    {
        var result = Simulate(
            Mover("a", Enemy, move: 0, initiative: 10, (1, 0)),
            Mover("b", Ally, move: 6, initiative: 20, (0, 0), (1, 0), (2, 0)));

        var b = result.Outcomes["b"];
        Assert.Equal(BattleMovementStopReason.EnemyContact, b.StopReason);
        Assert.Equal((0, 0), (b.X, b.Y));
    }

    [Fact]
    public void FriendlyUnitsNeverEndUpStackedOnTheSameTile()
    {
        var result = Simulate(
            Mover("a", Ally, move: 6, initiative: 10, (0, 0), (1, 1), (2, 2)),
            Mover("b", Ally, move: 6, initiative: 12, (4, 0), (3, 1), (2, 2)),
            Mover("c", Ally, move: 6, initiative: 8, (0, 4), (1, 3), (2, 2)));

        AssertNoOverlap(result);
    }

    // --- Diagonal geometry ---

    [Fact]
    public void UnitsCannotSlipThroughEachOtherAcrossCrossingDiagonals()
    {
        var result = Simulate(
            Mover("a", Ally, move: 4, initiative: 15, (0, 0), (1, 1)),
            Mover("b", Enemy, move: 4, initiative: 10, (1, 0), (0, 1)));

        var a = result.Outcomes["a"];
        var b = result.Outcomes["b"];

        Assert.Equal((1, 1), (a.X, a.Y));
        Assert.Equal((1, 0), (b.X, b.Y));
        Assert.Equal(BattleMovementStopReason.EnemyContact, b.StopReason);
    }

    [Fact]
    public void UnitCannotSqueezeDiagonallyBetweenTwoOtherUnits()
    {
        var result = Simulate(
            Mover("a", Ally, move: 4, initiative: 20, (0, 0), (1, 1)),
            Mover("l", Ally, move: 0, initiative: 10, (1, 0)),
            Mover("r", Ally, move: 0, initiative: 10, (0, 1)));

        var a = result.Outcomes["a"];
        Assert.Equal(BattleMovementStopReason.BlockedByAlly, a.StopReason);
        Assert.Equal((0, 0), (a.X, a.Y));
    }

    [Fact]
    public void DiagonalStepIsAllowedWhenOnlyOneFlankingTileIsHeld()
    {
        var result = Simulate(
            Mover("a", Ally, move: 4, initiative: 20, (0, 0), (1, 1)),
            Mover("l", Ally, move: 0, initiative: 10, (1, 0)));

        var a = result.Outcomes["a"];
        Assert.Equal(BattleMovementStopReason.RouteComplete, a.StopReason);
        Assert.Equal((1, 1), (a.X, a.Y));
    }

    // --- Terrain ---

    [Fact]
    public void ImpassableTerrainStopsTheUnit()
    {
        var terrain = new BattleMovementTerrain(10, 10, impassable: new[] { new BattleGridPoint(1, 0) });
        var result = Simulate(terrain, Mover("a", Ally, move: 4, initiative: 10, (0, 0), (1, 0)));

        Assert.Equal(BattleMovementStopReason.BlockedByTerrain, result.Outcomes["a"].StopReason);
    }

    // --- Charge bookkeeping ---

    [Fact]
    public void ChargeBuildUpIsCountedFromWhereTheChargeStarted()
    {
        var mover = Mover("a", Ally, move: 6, initiative: 10, (0, 0), (1, 0), (2, 0), (3, 0));
        var result = Simulate(Charging(mover, chargeStartStep: 1));

        Assert.Equal(2, result.Outcomes["a"].ChargeTilesTravelled);
    }

    [Fact]
    public void ChargeCutShortByAnEnemyReportsHowFarItGot()
    {
        var charger = Charging(
            Mover("a", Ally, move: 6, initiative: 10, (0, 0), (1, 0), (2, 0), (3, 0)),
            chargeStartStep: 0);

        var result = Simulate(charger, Mover("b", Enemy, move: 0, initiative: 20, (2, 0)));

        var a = result.Outcomes["a"];
        Assert.Equal(1, a.ChargeTilesTravelled);
        Assert.Equal(BattleMovementStopReason.EnemyContact, a.StopReason);
        Assert.Equal("b", a.EngagedWithMoverId);
    }

    [Fact]
    public void ChargeStoppedAfterTwoTilesStillReportsTwoTilesOfBuildUp()
    {
        var charger = Charging(
            Mover("a", Ally, move: 6, initiative: 10, (0, 0), (1, 0), (2, 0), (3, 0)),
            chargeStartStep: 0);

        var result = Simulate(charger, Mover("b", Enemy, move: 0, initiative: 20, (3, 0)));

        var a = result.Outcomes["a"];
        Assert.Equal(2, a.ChargeTilesTravelled);
        Assert.Equal((2, 0), (a.X, a.Y));
        Assert.Equal("b", a.EngagedWithMoverId);
    }

    // --- Moving while engaged ---

    [Fact]
    public void MovingAwayFromAnEngagedEnemyEmitsThePenaltyAtStepEnd()
    {
        var leaver = Engaged(
            Mover("a", Ally, move: 4, initiative: 10, (1, 0), (2, 0), (3, 0)),
            "b");
        var foe = Engaged(
            Mover("b", Enemy, move: 0, initiative: 5, (0, 0)),
            "a");

        var result = Simulate(leaver, foe);

        var penalty = Assert.Single(result.Events, e => e.Kind == BattleMovementEventKind.MovedWhileEngaged);
        Assert.Equal("a", penalty.MoverId);
        Assert.Equal("b", penalty.OtherMoverId);
        Assert.True(penalty.AtMs > 0);
        Assert.Equal((3, 0), (result.Outcomes["a"].X, result.Outcomes["a"].Y));
    }

    [Fact]
    public void SidesteppingWhileStayingAdjacentStillCostsThePenalty()
    {
        // a sidesteps but stays next to b at (0,0) — budging at all is enough.
        var mover = Engaged(
            Mover("a", Ally, move: 4, initiative: 10, (1, 0), (0, 1)),
            "b");
        var foe = Engaged(
            Mover("b", Enemy, move: 0, initiative: 5, (0, 0)),
            "a");

        var result = Simulate(mover, foe);

        var penalty = Assert.Single(result.Events, e => e.Kind == BattleMovementEventKind.MovedWhileEngaged);
        Assert.Equal("a", penalty.MoverId);
        Assert.Equal("b", penalty.OtherMoverId);
        Assert.Equal((0, 1), (result.Outcomes["a"].X, result.Outcomes["a"].Y));
    }

    [Fact]
    public void StandingStillWhileEngagedCostsNothing()
    {
        var holder = Engaged(Mover("a", Ally, move: 4, initiative: 10, (1, 0)), "b");
        var foe = Engaged(Mover("b", Enemy, move: 0, initiative: 5, (0, 0)), "a");

        var result = Simulate(holder, foe);

        Assert.DoesNotContain(result.Events, e => e.Kind == BattleMovementEventKind.MovedWhileEngaged);
    }

    [Fact]
    public void ALongMarchWhileEngagedIsPenalisedOnlyOncePerPair()
    {
        var mover = Engaged(
            Mover("a", Ally, move: 6, initiative: 10, (1, 0), (2, 0), (3, 0), (4, 0)),
            "b");
        var foe = Engaged(Mover("b", Enemy, move: 0, initiative: 5, (0, 0)), "a");

        var result = Simulate(mover, foe);

        Assert.Single(result.Events, e => e.Kind == BattleMovementEventKind.MovedWhileEngaged);
    }

    [Fact]
    public void PartnerWhoStepsAwayIsTheOneWhoPays()
    {
        var stay = Engaged(
            Mover("a", Ally, move: 0, initiative: 10, (0, 0)),
            "b");
        var leaver = Engaged(
            Mover("b", Enemy, move: 4, initiative: 5, (1, 0), (2, 0), (3, 0)),
            "a");

        var result = Simulate(stay, leaver);

        var penalty = Assert.Single(result.Events, e => e.Kind == BattleMovementEventKind.MovedWhileEngaged);
        Assert.Equal("b", penalty.MoverId);
        Assert.Equal("a", penalty.OtherMoverId);
    }

    [Fact]
    public void MovingWhileEngagedWithTwoEnemiesEmitsTwoEvents()
    {
        var leaver = Engaged(
            Mover("a", Ally, move: 4, initiative: 10, (1, 1), (2, 1), (3, 1)),
            "b", "c");
        var b = Engaged(Mover("b", Enemy, move: 0, initiative: 5, (0, 0)), "a");
        var c = Engaged(Mover("c", Enemy, move: 0, initiative: 6, (0, 2)), "a");

        var result = Simulate(leaver, b, c);

        var penalties = result.Events
            .Where(e => e.Kind == BattleMovementEventKind.MovedWhileEngaged)
            .OrderBy(e => e.OtherMoverId, StringComparer.Ordinal)
            .ToList();
        Assert.Equal(2, penalties.Count);
        Assert.All(penalties, e => Assert.Equal("a", e.MoverId));
        Assert.Equal(new[] { "b", "c" }, penalties.Select(e => e.OtherMoverId).ToArray());
    }

    [Fact]
    public void StopAtMsCutsTheRouteAtTheCap()
    {
        var stepMs = BattleMovementRules.StepDurationMs(6, BattleMovementRules.OrthogonalStepCost);
        var capped = WithStopAt(
            Mover("a", Ally, move: 6, initiative: 20, (0, 0), (1, 0), (2, 0), (3, 0)),
            // Cap exactly when the first step lands — no further steps may start.
            stopAtMs: stepMs);

        var result = Simulate(capped);

        Assert.Equal((1, 0), (result.Outcomes["a"].X, result.Outcomes["a"].Y));
        Assert.Equal(1, result.Outcomes["a"].TilesTravelled);
        Assert.Single(result.Legs);
    }

    [Fact]
    public void StopAtMsZeroKeepsTheUnitInPlaceAsAnObstacle()
    {
        var corpse = WithStopAt(
            Mover("a", Ally, move: 6, initiative: 20, (2, 0), (3, 0), (4, 0)),
            stopAtMs: 0);
        var walker = Mover("b", Enemy, move: 4, initiative: 10, (0, 0), (1, 0), (2, 0));

        var result = Simulate(corpse, walker);

        Assert.Equal((2, 0), (result.Outcomes["a"].X, result.Outcomes["a"].Y));
        Assert.Equal(0, result.Outcomes["a"].TilesTravelled);
        // Walker runs into the stationary corpse and stops in contact.
        Assert.Equal(BattleMovementStopReason.EnemyContact, result.Outcomes["b"].StopReason);
        Assert.Equal("a", result.Outcomes["b"].EngagedWithMoverId);
        Assert.Equal((1, 0), (result.Outcomes["b"].X, result.Outcomes["b"].Y));
    }

    // --- Determinism ---

    [Fact]
    public void SameInputAlwaysProducesTheSameRun()
    {
        BattleMovementResult Run() => Simulate(
            Mover("a", Ally, move: 5, initiative: 12, (0, 0), (1, 1), (2, 1), (3, 2)),
            Mover("b", Enemy, move: 7, initiative: 12, (6, 3), (5, 2), (4, 2), (3, 1)),
            Mover("c", Ally, move: 3, initiative: 9, (0, 3), (1, 3), (2, 3)),
            Mover("d", Enemy, move: 4, initiative: 18, (6, 0), (5, 0), (4, 1)));

        var first = Run();
        var second = Run();

        Assert.Equal(first.DurationMs, second.DurationMs);
        Assert.Equal(
            first.Legs.Select(Describe).ToList(),
            second.Legs.Select(Describe).ToList());
        Assert.Equal(
            first.Outcomes.OrderBy(o => o.Key).Select(o => Describe(o.Value)).ToList(),
            second.Outcomes.OrderBy(o => o.Key).Select(o => Describe(o.Value)).ToList());
    }

    [Fact]
    public void CrowdedMeleeStillLeavesEveryUnitOnItsOwnTile()
    {
        var result = Simulate(
            Mover("a1", Ally, move: 5, initiative: 14, (0, 2), (1, 2), (2, 2), (3, 2)),
            Mover("a2", Ally, move: 5, initiative: 11, (0, 3), (1, 3), (2, 3), (3, 3)),
            Mover("a3", Ally, move: 6, initiative: 9, (0, 1), (1, 2), (2, 2), (3, 2)),
            Mover("e1", Enemy, move: 5, initiative: 13, (6, 2), (5, 2), (4, 2), (3, 2)),
            Mover("e2", Enemy, move: 5, initiative: 12, (6, 3), (5, 3), (4, 3), (3, 3)),
            Mover("e3", Enemy, move: 4, initiative: 16, (6, 1), (5, 2), (4, 2), (3, 3)));

        AssertNoOverlap(result);
        Assert.True(result.DurationMs <= BattleMovementRules.MaxSimulationMs);
        Assert.DoesNotContain(
            result.Outcomes.Values,
            o => o.StopReason == BattleMovementStopReason.SimulationLimit);
    }

    // --- Helpers ---

    private static BattleMovementResult Simulate(params BattleMovementMover[] movers) =>
        Simulate(new BattleMovementTerrain(20, 16), movers);

    private static BattleMovementResult Simulate(
        BattleMovementTerrain terrain,
        params BattleMovementMover[] movers) =>
        BattleMovementSimulator.Simulate(new BattleMovementRequest
        {
            Terrain = terrain,
            Movers = movers,
        });

    private static BattleMovementMover Mover(
        string id, int side, int move, int initiative, params (int X, int Y)[] route) =>
        new()
        {
            Id = id,
            IsEnemy = side == Enemy,
            Size = 1,
            MovePoints = move,
            InitiativeTotal = initiative,
            InitiativeDie = initiative % 10,
            StartFacing = 2,
            PlannedFacing = 2,
            Route = route.Select(p => new BattleGridPoint(p.X, p.Y)).ToList(),
        };

    private static BattleMovementMover Charging(BattleMovementMover mover, int chargeStartStep) =>
        Clone(mover, charging: true, chargeStartStep: chargeStartStep, size: mover.Size);

    private static BattleMovementMover Sized(BattleMovementMover mover, int size) =>
        Clone(mover, charging: mover.IsCharging, chargeStartStep: mover.ChargeStartStep, size: size);

    private static BattleMovementMover Engaged(BattleMovementMover mover, params string[] enemyIds) =>
        Clone(
            mover,
            charging: mover.IsCharging,
            chargeStartStep: mover.ChargeStartStep,
            size: mover.Size,
            engagedEnemyIds: enemyIds,
            stopAtMs: mover.StopAtMs);

    private static BattleMovementMover WithStopAt(BattleMovementMover mover, int stopAtMs) =>
        Clone(
            mover,
            charging: mover.IsCharging,
            chargeStartStep: mover.ChargeStartStep,
            size: mover.Size,
            engagedEnemyIds: mover.EngagedEnemyIds,
            stopAtMs: stopAtMs);

    private static BattleMovementMover Pinned(BattleMovementMover mover) =>
        Clone(
            mover,
            charging: mover.IsCharging,
            chargeStartStep: mover.ChargeStartStep,
            size: mover.Size,
            engagedEnemyIds: mover.EngagedEnemyIds,
            stopAtMs: mover.StopAtMs,
            pinned: true);

    private static BattleMovementMover Clone(
        BattleMovementMover mover,
        bool charging,
        int chargeStartStep,
        int size,
        IReadOnlyList<string>? engagedEnemyIds = null,
        int? stopAtMs = null,
        bool? pinned = null) =>
        new()
        {
            Id = mover.Id,
            IsEnemy = mover.IsEnemy,
            Size = size,
            MovePoints = mover.MovePoints,
            InitiativeTotal = mover.InitiativeTotal,
            InitiativeDie = mover.InitiativeDie,
            StartFacing = mover.StartFacing,
            PlannedFacing = mover.PlannedFacing,
            IsCharging = charging,
            ChargeStartStep = chargeStartStep,
            EngagedEnemyIds = engagedEnemyIds ?? mover.EngagedEnemyIds,
            IsPinned = pinned ?? mover.IsPinned,
            StopAtMs = stopAtMs ?? mover.StopAtMs,
            Route = mover.Route,
        };

    private static string Describe(BattleMovementLeg leg) =>
        $"{leg.MoverId}@{leg.StartMs}+{leg.DurationMs}:{leg.FromX},{leg.FromY}->{leg.ToX},{leg.ToY}/{leg.Facing}";

    private static string Describe(BattleMovementOutcome outcome) =>
        $"{outcome.MoverId}:{outcome.X},{outcome.Y}/{outcome.Facing}/{outcome.StopReason}/{outcome.RemainingMove}";

    private static void AssertNoOverlap(
        BattleMovementResult result, IReadOnlyDictionary<string, int>? sizes = null)
    {
        var placed = result.Outcomes.Values.ToList();
        for (var i = 0; i < placed.Count; i++)
        {
            for (var j = i + 1; j < placed.Count; j++)
            {
                var a = placed[i];
                var b = placed[j];
                var aSize = sizes is not null && sizes.TryGetValue(a.MoverId, out var sa) ? sa : 1;
                var bSize = sizes is not null && sizes.TryGetValue(b.MoverId, out var sb) ? sb : 1;
                Assert.False(
                    BattleMovementRules.FootprintsOverlap(a.X, a.Y, aSize, b.X, b.Y, bSize),
                    $"{a.MoverId} and {b.MoverId} ended up on the same tile ({a.X},{a.Y}) / ({b.X},{b.Y})");
            }
        }
    }
}
