namespace DA_Common.Barony.Battle;

/// <summary>
/// Costs, timing and grid geometry shared by movement planning and the movement simulation.
/// Everything here is pure integer arithmetic so planning, simulation and replay always agree.
/// </summary>
public static class BattleMovementRules
{
    /// <summary>Half-move points charged for a straight (N/E/S/W) step.</summary>
    public const int OrthogonalStepCost = 2;

    /// <summary>Half-move points charged for a diagonal step.</summary>
    public const int DiagonalStepCost = 3;

    /// <summary>Cost multiplier for a step that brings new tiles of difficult terrain under the footprint.</summary>
    public const int DifficultTerrainMultiplier = 2;

    /// <summary>
    /// Wall-clock time in which a unit spends its whole move allowance. Step duration is
    /// proportional to the step's cost and inversely proportional to <c>Move</c>, so a Move 8
    /// unit covers twice the ground of a Move 4 unit in the same time, and a diagonal step
    /// takes 1.5× longer than a straight one — which keeps physical speed equal in all
    /// eight directions.
    /// </summary>
    public const int FullMoveMs = 2800;

    /// <summary>Resolution at which the simulation makes movement decisions.</summary>
    public const int SimulationTickMs = 25;

    /// <summary>Floor on a single step's duration, so extreme Move values stay animatable.</summary>
    public const int MinStepMs = 40;

    /// <summary>Safety bound so a pathological plan can never spin the simulation forever.</summary>
    public const int MaxSimulationMs = FullMoveMs * 6;

    /// <summary>
    /// Movement allowance in half-points. The extra half-point is what lets Move 4 buy three
    /// diagonal steps (9 half-points) rather than two — it defines the documented rounding.
    /// </summary>
    public static int MoveHalfBudget(int movePoints) => Math.Max(0, movePoints) * 2 + 1;

    /// <summary>Move points actually consumed (displayed value) for a number of half-points.</summary>
    public static int SpentMovePoints(int spentHalfPoints) => Math.Max(0, spentHalfPoints) / 2;

    public static bool IsDiagonalStep(int fromX, int fromY, int toX, int toY) =>
        fromX != toX && fromY != toY;

    /// <summary>Half-move points charged for one tile-to-tile step.</summary>
    public static int StepHalfCost(int fromX, int fromY, int toX, int toY, bool entersDifficult)
    {
        var cost = IsDiagonalStep(fromX, fromY, toX, toY) ? DiagonalStepCost : OrthogonalStepCost;
        return entersDifficult ? cost * DifficultTerrainMultiplier : cost;
    }

    /// <summary>Wall-clock duration of a step costing <paramref name="halfCost"/> half-points.</summary>
    public static int StepDurationMs(int movePoints, int halfCost) =>
        Math.Max(MinStepMs, halfCost * FullMoveMs / (2 * Math.Max(1, movePoints)));

    /// <summary>
    /// How long a unit keeps waiting for a friendly unit to clear the way before it gives up
    /// and halts. Three straight steps is long enough for a marching column to file through,
    /// short enough that a real jam does not stall the whole phase.
    /// </summary>
    public static int StallLimitMs(int movePoints) =>
        Math.Max(SimulationTickMs, 3 * StepDurationMs(movePoints, OrthogonalStepCost));

    /// <summary>Facing index (0 = North, clockwise) for a single grid step.</summary>
    public static int FacingFromStep(int fromX, int fromY, int toX, int toY) =>
        FacingFromDelta(Math.Sign(toX - fromX), Math.Sign(toY - fromY));

    /// <summary>
    /// Eight-way facing toward another footprint, snapping to the nearest compass point.
    /// A component is dropped when the other axis dominates it more than twofold.
    /// </summary>
    public static int FacingToward(
        int fromX, int fromY, int fromSize,
        int toX, int toY, int toSize)
    {
        var dx = (toX * 2 + toSize) - (fromX * 2 + fromSize);
        var dy = (toY * 2 + toSize) - (fromY * 2 + fromSize);
        var adx = Math.Abs(dx);
        var ady = Math.Abs(dy);

        var sx = Math.Sign(dx);
        var sy = Math.Sign(dy);
        if (adx > ady * 2)
            sy = 0;
        else if (ady > adx * 2)
            sx = 0;

        return FacingFromDelta(sx, sy);
    }

    /// <summary>True when two axis-aligned footprints share at least one tile.</summary>
    public static bool FootprintsOverlap(
        int ax, int ay, int aSize,
        int bx, int by, int bSize) =>
        ax < bx + bSize && ax + aSize > bx &&
        ay < by + bSize && ay + aSize > by;

    /// <summary>True when two footprints touch along an edge or a corner.</summary>
    public static bool FootprintsAdjacent(
        int ax, int ay, int aSize,
        int bx, int by, int bSize)
    {
        var gapX = Math.Max(0, Math.Max(ax, bx) - Math.Min(ax + aSize, bx + bSize));
        var gapY = Math.Max(0, Math.Max(ay, by) - Math.Min(ay + aSize, by + bSize));
        return Math.Max(gapX, gapY) == 0;
    }

    private static int FacingFromDelta(int sx, int sy) => (sx, sy) switch
    {
        (0, -1) => 0,
        (1, -1) => 1,
        (1, 0) => 2,
        (1, 1) => 3,
        (0, 1) => 4,
        (-1, 1) => 5,
        (-1, 0) => 6,
        (-1, -1) => 7,
        _ => 0,
    };
}
