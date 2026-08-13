using DA_Common.Barony.Battle;

namespace DA_Business.Tests.Barony;

public class BattleMovementRulesShotTests
{
    [Fact]
    public void SameCellShotCostsZero()
    {
        Assert.Equal(0, BattleMovementRules.FootprintShotHalfCost(2, 2, 1, 2, 2, 1));
        Assert.False(BattleMovementRules.IsInShotRange(2, 2, 1, 2, 2, 1, rangePoints: 5));
    }

    [Fact]
    public void AdjacentOrthoShotCostsOneMovePoint()
    {
        // (0,0) → (1,0): one ortho step = 2 half-points.
        Assert.Equal(2, BattleMovementRules.FootprintShotHalfCost(0, 0, 1, 1, 0, 1));
        Assert.True(BattleMovementRules.IsInShotRange(0, 0, 1, 1, 0, 1, rangePoints: 1));
    }

    [Fact]
    public void AdjacentDiagonalShotCostsOneAndAHalf()
    {
        Assert.Equal(3, BattleMovementRules.FootprintShotHalfCost(0, 0, 1, 1, 1, 1));
        Assert.True(BattleMovementRules.IsInShotRange(0, 0, 1, 1, 1, 1, rangePoints: 1));
    }

    [Fact]
    public void StraightThreeTilesFitsRangeThreeButFourDoesNot()
    {
        // Range 3 budget = 7 half-points. Three ortho = 6; four ortho = 8.
        Assert.Equal(6, BattleMovementRules.FootprintShotHalfCost(0, 0, 1, 3, 0, 1));
        Assert.Equal(8, BattleMovementRules.FootprintShotHalfCost(0, 0, 1, 4, 0, 1));
        Assert.True(BattleMovementRules.IsInShotRange(0, 0, 1, 3, 0, 1, rangePoints: 3));
        Assert.False(BattleMovementRules.IsInShotRange(0, 0, 1, 4, 0, 1, rangePoints: 3));
    }

    [Fact]
    public void DiagonalTwoTilesFitsRangeThreeButThreeDoesNot()
    {
        // Two diagonal = 6; three diagonal = 9. Budget for Range 3 = 7.
        Assert.Equal(6, BattleMovementRules.FootprintShotHalfCost(0, 0, 1, 2, 2, 1));
        Assert.Equal(9, BattleMovementRules.FootprintShotHalfCost(0, 0, 1, 3, 3, 1));
        Assert.True(BattleMovementRules.IsInShotRange(0, 0, 1, 2, 2, 1, rangePoints: 3));
        Assert.False(BattleMovementRules.IsInShotRange(0, 0, 1, 3, 3, 1, rangePoints: 3));
    }

    [Fact]
    public void MixedKnightMoveUsesDiagonalThenOrtho()
    {
        // (0,0) → (2,1): min=1 diagonal + 1 ortho = 3+2 = 5.
        Assert.Equal(5, BattleMovementRules.FootprintShotHalfCost(0, 0, 1, 2, 1, 1));
        Assert.True(BattleMovementRules.IsInShotRange(0, 0, 1, 2, 1, 1, rangePoints: 3));
    }

    [Fact]
    public void LargeFootprintsMeasureGapBetweenClosestEdges()
    {
        // Size-2 at (0,0) covers [0,1]; target at (3,0). Gap = 2 ortho steps = 4.
        Assert.Equal(4, BattleMovementRules.FootprintShotHalfCost(0, 0, 2, 3, 0, 1));
        Assert.True(BattleMovementRules.IsInShotRange(0, 0, 2, 3, 0, 1, rangePoints: 2));
        Assert.False(BattleMovementRules.IsInShotRange(0, 0, 2, 3, 0, 1, rangePoints: 1));
    }

    [Fact]
    public void ZeroRangeNeverShoots()
    {
        Assert.False(BattleMovementRules.IsInShotRange(0, 0, 1, 1, 0, 1, rangePoints: 0));
        Assert.False(BattleMovementRules.IsInShotRange(0, 0, 1, 1, 0, 1, rangePoints: -1));
    }

    // --- Distance penalty ---

    [Fact]
    public void PointBlankShotIsUnpenalised()
    {
        var ortho = BattleMovementRules.FootprintShotHalfCost(0, 0, 1, 1, 0, 1);
        var diagonal = BattleMovementRules.FootprintShotHalfCost(0, 0, 1, 1, 1, 1);

        Assert.Equal(0, BattleMovementRules.ShotAttackPenalty(ortho));
        Assert.Equal(0, BattleMovementRules.ShotAttackPenalty(diagonal));
    }

    [Fact]
    public void EveryTileOfFlightBeyondPointBlankCostsTwoAttack()
    {
        int Penalty(int targetX) => BattleMovementRules.ShotAttackPenalty(
            BattleMovementRules.FootprintShotHalfCost(0, 0, 1, targetX, 0, 1));

        Assert.Equal(2, Penalty(2));
        Assert.Equal(4, Penalty(3));
        Assert.Equal(6, Penalty(4));
        Assert.Equal(8, Penalty(5));
    }

    [Fact]
    public void DiagonalFlightIsPenalisedOnTheSameMetricAsMovement()
    {
        // Two diagonals cost 6 half-points — three move points of flight, so −4.
        Assert.Equal(4, BattleMovementRules.ShotAttackPenalty(
            BattleMovementRules.FootprintShotHalfCost(0, 0, 1, 2, 2, 1)));
    }

    [Fact]
    public void LargeFootprintsArePenalisedFromTheirClosestEdges()
    {
        // Size-2 at (0,0) covers [0,1], so a target at (3,0) is two tiles of flight away.
        Assert.Equal(2, BattleMovementRules.ShotAttackPenalty(
            BattleMovementRules.FootprintShotHalfCost(0, 0, 2, 3, 0, 1)));
    }
}
