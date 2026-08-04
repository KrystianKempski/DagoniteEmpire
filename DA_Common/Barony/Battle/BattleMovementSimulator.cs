namespace DA_Common.Barony.Battle;

/// <summary>
/// Resolves a whole movement phase as one deterministic simulation: every unit sets off at the
/// same moment and advances along its planned route in wall-clock time, reacting to whatever is
/// actually in the way at that instant.
/// </summary>
/// <remarks>
/// The rules the simulation enforces:
/// <list type="bullet">
/// <item>Speed comes from <c>Move</c>; a step's duration is proportional to its cost, so
/// diagonal travel is no faster than straight travel.</item>
/// <item>A unit claims the tile it is stepping into for the whole step, so nothing can pass
/// through it. A hostile unit additionally screens the tile it is vacating.</item>
/// <item>Running into a hostile unit ends the movement then and there, in contact.</item>
/// <item>A friendly unit in the way only makes the mover wait; it halts only if the jam never
/// clears.</item>
/// <item>When two units reach for the same tile in the same instant, a charge wins over a
/// march, then the higher initiative wins.</item>
/// <item>A diagonal step is refused when both flanking tiles are held, which also stops two
/// units from swapping through each other across a diagonal.</item>
/// </list>
/// The result is a timed list of legs, so the on-screen animation replays exactly the run that
/// produced the final positions rather than recomputing its own.
/// </remarks>
public static class BattleMovementSimulator
{
    public static BattleMovementResult Simulate(BattleMovementRequest request)
    {
        var terrain = request.Terrain;
        var states = request.Movers.Select(m => new MoverState(m)).ToList();

        // Disputes are settled by this order: a charge outranks a march, then initiative.
        var order = states
            .OrderByDescending(s => s.Spec.IsCharging)
            .ThenByDescending(s => s.Spec.InitiativeTotal)
            .ThenByDescending(s => s.Spec.InitiativeDie)
            .ThenBy(s => s.Spec.Id, StringComparer.Ordinal)
            .ToList();

        var legs = new List<BattleMovementLeg>();
        var events = new List<BattleMovementEvent>();

        foreach (var state in order)
        {
            if (state.Spec.Route.Count <= 1)
                Stop(state, BattleMovementStopReason.RouteComplete, 0, null, events);
        }

        var now = 0;
        while (order.Any(s => s.Active))
        {
            foreach (var state in order)
            {
                if (state.InTransit && state.TransitEndMs <= now)
                    state.InTransit = false;
            }

            foreach (var state in order)
            {
                if (!state.Active || state.InTransit)
                    continue;

                if (state.RouteIndex >= state.Spec.Route.Count - 1)
                {
                    Stop(state, BattleMovementStopReason.RouteComplete, now, null, events);
                    continue;
                }

                TryStartStep(state, states, terrain, now, legs, events);
            }

            if (!order.Any(s => s.Active))
                break;

            now += BattleMovementRules.SimulationTickMs;
            if (now > BattleMovementRules.MaxSimulationMs)
            {
                foreach (var state in order.Where(s => s.Active))
                    Stop(state, BattleMovementStopReason.SimulationLimit, now, null, events);
                break;
            }
        }

        return new BattleMovementResult
        {
            Legs = legs,
            Events = events,
            Outcomes = order.ToDictionary(s => s.Spec.Id, BuildOutcome, StringComparer.Ordinal),
            DurationMs = legs.Count == 0 ? 0 : legs.Max(l => l.EndMs),
        };
    }

    private static void TryStartStep(
        MoverState mover,
        List<MoverState> states,
        BattleMovementTerrain terrain,
        int now,
        List<BattleMovementLeg> legs,
        List<BattleMovementEvent> events)
    {
        var size = mover.Spec.Size;
        var from = mover.Spec.Route[mover.RouteIndex];
        var to = mover.Spec.Route[mover.RouteIndex + 1];

        if (!terrain.IsFootprintInBounds(to.X, to.Y, size) || terrain.IsFootprintImpassable(to.X, to.Y, size))
        {
            Stop(mover, BattleMovementStopReason.BlockedByTerrain, now, null, events);
            return;
        }

        var entersDifficult = terrain.FootprintEntersDifficult(from.X, from.Y, to.X, to.Y, size);
        var halfCost = BattleMovementRules.StepHalfCost(from.X, from.Y, to.X, to.Y, entersDifficult);
        if (mover.SpentHalfPoints + halfCost > BattleMovementRules.MoveHalfBudget(mover.Spec.MovePoints))
        {
            Stop(mover, BattleMovementStopReason.OutOfMovePoints, now, null, events);
            return;
        }

        var blocker = FindBlocker(states, mover, to.X, to.Y)
                      ?? FindDiagonalObstruction(states, mover, from, to);

        if (blocker is not null)
        {
            if (blocker.Spec.IsEnemy != mover.Spec.IsEnemy)
            {
                Stop(mover, BattleMovementStopReason.EnemyContact, now, blocker, events);
                return;
            }

            if (mover.WaitingSinceMs < 0)
                mover.WaitingSinceMs = now;
            else if (now - mover.WaitingSinceMs >= BattleMovementRules.StallLimitMs(mover.Spec.MovePoints))
                Stop(mover, BattleMovementStopReason.BlockedByAlly, now, blocker, events);
            return;
        }

        var duration = BattleMovementRules.StepDurationMs(mover.Spec.MovePoints, halfCost);
        var facing = BattleMovementRules.FacingFromStep(from.X, from.Y, to.X, to.Y);

        legs.Add(new BattleMovementLeg
        {
            MoverId = mover.Spec.Id,
            StartMs = now,
            DurationMs = duration,
            FromX = from.X,
            FromY = from.Y,
            ToX = to.X,
            ToY = to.Y,
            Facing = facing,
        });

        // Claim the destination and keep screening the tile being vacated until the step ends.
        mover.TrailAnchor = from;
        mover.Anchor = to;
        mover.InTransit = true;
        mover.TransitEndMs = now + duration;
        mover.RouteIndex++;
        mover.TilesTravelled++;
        mover.SpentHalfPoints += halfCost;
        mover.Facing = facing;
        mover.WaitingSinceMs = -1;
    }

    /// <summary>
    /// Finds whoever holds the tiles the mover wants. A hostile unit is reported ahead of a
    /// friendly one, because hostile contact ends the movement while friendly contact only delays it.
    /// </summary>
    private static MoverState? FindBlocker(List<MoverState> states, MoverState mover, int x, int y)
    {
        MoverState? allyBlocker = null;
        var size = mover.Spec.Size;

        foreach (var other in states)
        {
            if (ReferenceEquals(other, mover))
                continue;

            var otherSize = other.Spec.Size;
            var hostile = other.Spec.IsEnemy != mover.Spec.IsEnemy;

            if (BattleMovementRules.FootprintsOverlap(x, y, size, other.Anchor.X, other.Anchor.Y, otherSize))
            {
                if (hostile)
                    return other;
                allyBlocker ??= other;
                continue;
            }

            if (hostile && other.InTransit &&
                BattleMovementRules.FootprintsOverlap(
                    x, y, size, other.TrailAnchor.X, other.TrailAnchor.Y, otherSize))
                return other;
        }

        return allyBlocker;
    }

    /// <summary>
    /// A diagonal step is refused when both flanking tiles are held — you cannot squeeze between
    /// two units, and two units cannot trade places across the same diagonal.
    /// </summary>
    private static MoverState? FindDiagonalObstruction(
        List<MoverState> states,
        MoverState mover,
        BattleGridPoint from,
        BattleGridPoint to)
    {
        if (!BattleMovementRules.IsDiagonalStep(from.X, from.Y, to.X, to.Y))
            return null;

        var flankA = new BattleGridPoint(to.X, from.Y);
        var flankB = new BattleGridPoint(from.X, to.Y);

        foreach (var other in states)
        {
            if (ReferenceEquals(other, mover) || !other.InTransit)
                continue;
            var crossing =
                (other.TrailAnchor == flankA && other.Anchor == flankB) ||
                (other.TrailAnchor == flankB && other.Anchor == flankA);
            if (crossing)
                return other;
        }

        var holderA = FindBlocker(states, mover, flankA.X, flankA.Y);
        var holderB = FindBlocker(states, mover, flankB.X, flankB.Y);
        if (holderA is null || holderB is null)
            return null;

        if (holderA.Spec.IsEnemy != mover.Spec.IsEnemy)
            return holderA;
        return holderB.Spec.IsEnemy != mover.Spec.IsEnemy ? holderB : holderA;
    }

    private static void Stop(
        MoverState mover,
        BattleMovementStopReason reason,
        int atMs,
        MoverState? other,
        List<BattleMovementEvent> events)
    {
        mover.Active = false;
        mover.StopReason = reason;
        mover.ArrivalMs = atMs;
        mover.BlockedByMoverId = other?.Spec.Id;

        switch (reason)
        {
            case BattleMovementStopReason.EnemyContact when other is not null:
                mover.EngagedWithMoverId = other.Spec.Id;
                mover.Facing = BattleMovementRules.FacingToward(
                    mover.Anchor.X, mover.Anchor.Y, mover.Spec.Size,
                    other.Anchor.X, other.Anchor.Y, other.Spec.Size);
                break;
            case BattleMovementStopReason.RouteComplete:
                mover.Facing = mover.Spec.PlannedFacing;
                break;
        }

        var kind = reason switch
        {
            BattleMovementStopReason.EnemyContact => BattleMovementEventKind.EnemyContact,
            BattleMovementStopReason.BlockedByAlly => BattleMovementEventKind.AllyDeadlock,
            BattleMovementStopReason.OutOfMovePoints => BattleMovementEventKind.OutOfMovePoints,
            BattleMovementStopReason.BlockedByTerrain => BattleMovementEventKind.BlockedByTerrain,
            _ => (BattleMovementEventKind?)null,
        };

        if (kind is null)
            return;

        events.Add(new BattleMovementEvent
        {
            AtMs = atMs,
            Kind = kind.Value,
            MoverId = mover.Spec.Id,
            OtherMoverId = other?.Spec.Id,
            X = mover.Anchor.X,
            Y = mover.Anchor.Y,
        });
    }

    private static BattleMovementOutcome BuildOutcome(MoverState state)
    {
        var spent = BattleMovementRules.SpentMovePoints(state.SpentHalfPoints);
        var chargeTiles = state.Spec.IsCharging
            ? Math.Max(0, state.RouteIndex - state.Spec.ChargeStartStep)
            : 0;

        return new BattleMovementOutcome
        {
            MoverId = state.Spec.Id,
            X = state.Anchor.X,
            Y = state.Anchor.Y,
            Facing = state.Facing,
            TilesTravelled = state.TilesTravelled,
            ChargeTilesTravelled = chargeTiles,
            SpentMovePoints = spent,
            RemainingMove = Math.Max(0, state.Spec.MovePoints - spent),
            StopReason = state.StopReason,
            BlockedByMoverId = state.BlockedByMoverId,
            EngagedWithMoverId = state.EngagedWithMoverId,
            ArrivalMs = state.ArrivalMs,
        };
    }

    private sealed class MoverState
    {
        public MoverState(BattleMovementMover spec)
        {
            Spec = spec;
            Anchor = spec.Route.Count > 0 ? spec.Route[0] : new BattleGridPoint(0, 0);
            TrailAnchor = Anchor;
            Facing = spec.StartFacing;
        }

        public BattleMovementMover Spec { get; }

        /// <summary>Tile the unit holds — the destination while a step is under way.</summary>
        public BattleGridPoint Anchor { get; set; }

        /// <summary>Tile being vacated during a step.</summary>
        public BattleGridPoint TrailAnchor { get; set; }

        public int RouteIndex { get; set; }

        public bool InTransit { get; set; }

        public int TransitEndMs { get; set; }

        public int SpentHalfPoints { get; set; }

        public int TilesTravelled { get; set; }

        public int Facing { get; set; }

        public int WaitingSinceMs { get; set; } = -1;

        public bool Active { get; set; } = true;

        public BattleMovementStopReason StopReason { get; set; } = BattleMovementStopReason.RouteComplete;

        public string? BlockedByMoverId { get; set; }

        public string? EngagedWithMoverId { get; set; }

        public int ArrivalMs { get; set; }
    }
}
