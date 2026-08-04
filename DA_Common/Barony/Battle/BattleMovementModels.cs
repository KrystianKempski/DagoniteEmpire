namespace DA_Common.Barony.Battle;

/// <summary>A tile anchor on the battle grid.</summary>
public readonly record struct BattleGridPoint(int X, int Y);

/// <summary>Terrain the simulation has to respect. Tiles not listed are open ground.</summary>
public sealed class BattleMovementTerrain
{
    private readonly HashSet<BattleGridPoint> _impassable;
    private readonly HashSet<BattleGridPoint> _difficult;

    public BattleMovementTerrain(
        int width,
        int height,
        IEnumerable<BattleGridPoint>? impassable = null,
        IEnumerable<BattleGridPoint>? difficult = null)
    {
        Width = width;
        Height = height;
        _impassable = new HashSet<BattleGridPoint>(impassable ?? Array.Empty<BattleGridPoint>());
        _difficult = new HashSet<BattleGridPoint>(difficult ?? Array.Empty<BattleGridPoint>());
    }

    public int Width { get; }

    public int Height { get; }

    public bool IsImpassable(int x, int y) => _impassable.Contains(new BattleGridPoint(x, y));

    public bool IsDifficult(int x, int y) => _difficult.Contains(new BattleGridPoint(x, y));

    public bool IsFootprintInBounds(int x, int y, int size) =>
        x >= 0 && y >= 0 && x + size <= Width && y + size <= Height;

    public bool IsFootprintImpassable(int x, int y, int size)
    {
        for (var dx = 0; dx < size; dx++)
            for (var dy = 0; dy < size; dy++)
                if (IsImpassable(x + dx, y + dy))
                    return true;
        return false;
    }

    /// <summary>True when the step pulls tiles of difficult terrain newly under the footprint.</summary>
    public bool FootprintEntersDifficult(int fromX, int fromY, int toX, int toY, int size)
    {
        for (var dx = 0; dx < size; dx++)
        {
            for (var dy = 0; dy < size; dy++)
            {
                var cx = toX + dx;
                var cy = toY + dy;
                var alreadyCovered = cx >= fromX && cx < fromX + size && cy >= fromY && cy < fromY + size;
                if (!alreadyCovered && IsDifficult(cx, cy))
                    return true;
            }
        }
        return false;
    }
}

/// <summary>One unit entering the movement phase, with the route its owner planned for it.</summary>
public sealed class BattleMovementMover
{
    public required string Id { get; init; }

    /// <summary>Units on opposite sides block each other permanently; same side yields.</summary>
    public bool IsEnemy { get; init; }

    /// <summary>Footprint edge in tiles (1–3).</summary>
    public int Size { get; init; } = 1;

    public int MovePoints { get; init; }

    public int InitiativeTotal { get; init; }

    public int InitiativeDie { get; init; }

    public int StartFacing { get; init; }

    /// <summary>Facing the unit settles on when it completes its route undisturbed.</summary>
    public int PlannedFacing { get; init; }

    public bool IsCharging { get; init; }

    /// <summary>Index in <see cref="Route"/> where the charge run begins.</summary>
    public int ChargeStartStep { get; init; }

    /// <summary>
    /// Hostile units this mover is locked in melee with at the start of the phase.
    /// Used to emit <see cref="BattleMovementEventKind.MovedWhileEngaged"/> once the mover budges.
    /// </summary>
    public IReadOnlyList<string> EngagedEnemyIds { get; init; } = Array.Empty<string>();

    /// <summary>
    /// True when this unit is held in a melee it cannot shrug off. A friendly unit standing free
    /// can be squeezed past; one that is pinned holds its tile against its own side too.
    /// </summary>
    public bool IsPinned { get; init; }

    /// <summary>
    /// Hard wall-clock cap: the mover stops once the simulation clock reaches this time.
    /// Used on re-runs after a unit is destroyed mid-movement by a leave-engagement hit.
    /// </summary>
    public int? StopAtMs { get; init; }

    /// <summary>Planned tiles including the starting tile. A single entry means "stay put".</summary>
    public IReadOnlyList<BattleGridPoint> Route { get; init; } = Array.Empty<BattleGridPoint>();
}

public sealed class BattleMovementRequest
{
    public required BattleMovementTerrain Terrain { get; init; }

    public required IReadOnlyList<BattleMovementMover> Movers { get; init; }
}

/// <summary>Why a unit is no longer moving.</summary>
public enum BattleMovementStopReason
{
    /// <summary>Reached the end of the planned route.</summary>
    RouteComplete,

    /// <summary>Ran out of move points before the route ended.</summary>
    OutOfMovePoints,

    /// <summary>Ran into a hostile unit and halted in contact with it.</summary>
    EnemyContact,

    /// <summary>Waited for a friendly unit to clear the way, and it never did.</summary>
    BlockedByAlly,

    /// <summary>The next tile turned out to be off-map or impassable.</summary>
    BlockedByTerrain,

    /// <summary>The simulation hit its safety bound (should not happen in practice).</summary>
    SimulationLimit,
}

/// <summary>One tile-to-tile hop, timed on the simulation clock. Drives the replay animation.</summary>
public sealed class BattleMovementLeg
{
    public required string MoverId { get; init; }

    public int StartMs { get; init; }

    public int DurationMs { get; init; }

    public int FromX { get; init; }

    public int FromY { get; init; }

    public int ToX { get; init; }

    public int ToY { get; init; }

    /// <summary>Facing while travelling this leg.</summary>
    public int Facing { get; init; }

    public int EndMs => StartMs + DurationMs;
}

public enum BattleMovementEventKind
{
    EnemyContact,
    AllyDeadlock,
    OutOfMovePoints,
    BlockedByTerrain,
    /// <summary>
    /// <see cref="BattleMovementEvent.MoverId"/> budged while locked in melee with
    /// <see cref="BattleMovementEvent.OtherMoverId"/>, who gets a free hit for it.
    /// Fires once per pair, on the mover's first completed step.
    /// </summary>
    MovedWhileEngaged,
}

public sealed class BattleMovementEvent
{
    public int AtMs { get; init; }

    public BattleMovementEventKind Kind { get; init; }

    public required string MoverId { get; init; }

    public string? OtherMoverId { get; init; }

    /// <summary>Tile the unit came to rest on.</summary>
    public int X { get; init; }

    public int Y { get; init; }
}

public sealed class BattleMovementOutcome
{
    public required string MoverId { get; init; }

    public int X { get; init; }

    public int Y { get; init; }

    public int Facing { get; init; }

    public int TilesTravelled { get; init; }

    /// <summary>Tiles covered from the point the charge run started — the charge's build-up.</summary>
    public int ChargeTilesTravelled { get; init; }

    public int SpentMovePoints { get; init; }

    public int RemainingMove { get; init; }

    public BattleMovementStopReason StopReason { get; init; }

    /// <summary>Unit that ended this one's movement, when something did.</summary>
    public string? BlockedByMoverId { get; init; }

    /// <summary>Hostile unit this one halted against and is now in contact with.</summary>
    public string? EngagedWithMoverId { get; init; }

    public int ArrivalMs { get; init; }
}

public sealed class BattleMovementResult
{
    public required IReadOnlyList<BattleMovementLeg> Legs { get; init; }

    public required IReadOnlyDictionary<string, BattleMovementOutcome> Outcomes { get; init; }

    public required IReadOnlyList<BattleMovementEvent> Events { get; init; }

    /// <summary>Wall-clock length of the whole movement phase.</summary>
    public int DurationMs { get; init; }
}
