using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace DA_Models.BaronyModels
{
    public static class BaronyBattlePhases
    {
        public const string Setup = "setup";
        public const string Battle = "battle";
    }

    /// <summary>Sub-phases while <see cref="BaronyBattlePhases.Battle"/> is active.</summary>
    public static class BaronyBattleSubPhases
    {
        /// <summary>Units place destination markers (initiative low → high).</summary>
        public const string Movement = "movement";
        /// <summary>Players assign attack targets (any order, reversible).</summary>
        public const string AttackPlanning = "attack-planning";
        /// <summary>After attack planning resolves; actual damage resolution.</summary>
        public const string Combat = "combat";
    }

    public static class BaronyBattleTerrain
    {
        public const string Difficult = "difficult";
        public const string Impassable = "impassable";
        public const string Deploy = "deploy";
    }

    public class BaronyBattleMapDTO
    {
        public int Id { get; set; }
        public int BaronyId { get; set; }
        public bool IsActive { get; set; }
        public string Phase { get; set; } = BaronyBattlePhases.Setup;
        public int Width { get; set; } = 20;
        public int Height { get; set; } = 16;
        public List<BaronyBattleCellDTO> Cells { get; set; } = new();
        public List<BaronyBattleTokenDTO> Tokens { get; set; } = new();
        public BaronyBattleTurnStateDTO TurnState { get; set; } = new();
        public List<BaronyBattleLogEntryDTO> Log { get; set; } = new();
    }

    public class BaronyBattleCellDTO
    {
        public int X { get; set; }
        public int Y { get; set; }
        public string? Color { get; set; }
        public string? Text { get; set; }
        public string? TextColor { get; set; }

        /// <summary>null/empty = normal; difficult / impassable / deploy.</summary>
        public string? Terrain { get; set; }
    }

    /// <summary>One step on a planned movement path.</summary>
    public class BaronyBattleWaypointDTO
    {
        public int X { get; set; }
        public int Y { get; set; }
        /// <summary>Locked = confirmed stop on the path; unlocked = provisional tip being edited.</summary>
        public bool Locked { get; set; }
        /// <summary>Move points left after reaching this waypoint.</summary>
        public int RemainingMoveAfter { get; set; }
        /// <summary><see cref="BaronyBattleFacing"/> at this stop (0–7).</summary>
        public int Facing { get; set; } = BaronyBattleFacing.North;
        /// <summary>1 if a facing change already spent a move point at this tip.</summary>
        public int FacingCostPaid { get; set; }
    }

    public class BaronyBattleTokenDTO
    {
        public string Id { get; set; } = string.Empty;
        public int X { get; set; }
        public int Y { get; set; }
        public string Label { get; set; } = string.Empty;
        public string? IconKey { get; set; }
        public bool IsEnemy { get; set; }
        public int? UnitId { get; set; }
        public int Size { get; set; } = 1;

        public int Attack { get; set; }
        public int Defense { get; set; }
        public int Damage { get; set; }
        public int Hp { get; set; }
        public int MaxHp { get; set; }
        public int Discipline { get; set; }
        public int TroopCount { get; set; }
        public int Armor { get; set; }
        public int Move { get; set; }
        public int RemainingMove { get; set; }

        /// <summary><see cref="BaronyBattleFacing"/> — current front (0 = North).</summary>
        public int Facing { get; set; } = BaronyBattleFacing.North;

        /// <summary>Target token ID assigned during attack-planning phase; null = no attack.</summary>
        public string? AttackTargetId { get; set; }

        /// <summary>
        /// Successful charge this turn: +2 Attack / +2 Damage vs <see cref="ChargeTargetId"/> in Combat.
        /// Cleared when the charge target is changed or at end of combat.
        /// </summary>
        public bool ChargeBonus { get; set; }

        /// <summary>Enemy token the charge locked onto (must match <see cref="AttackTargetId"/> for the bonus).</summary>
        public string? ChargeTargetId { get; set; }

        public int InitiativeDie { get; set; }
        public int InitiativeTotal { get; set; }

        /// <summary>Planned movement path (waypoints + locks). Empty = stay.</summary>
        public List<BaronyBattleWaypointDTO> Path { get; set; } = new();

        [JsonIgnore]
        public bool HasPath => Path.Count > 0;

        [JsonIgnore]
        public BaronyBattleWaypointDTO? LastWaypoint => Path.Count > 0 ? Path[^1] : null;

        [JsonIgnore]
        public BaronyBattleWaypointDTO? ProvisionalWaypoint =>
            Path.LastOrDefault(w => !w.Locked);

        public void ClearPath() => Path.Clear();

        /// <summary>Final planned cell, or current position if no path.</summary>
        public (int X, int Y) PlannedEnd()
        {
            if (Path.Count == 0)
                return (X, Y);
            var last = Path[^1];
            return (last.X, last.Y);
        }

        public int PlannedFacing()
        {
            if (Path.Count == 0)
                return BaronyBattleFacing.Clamp(Facing);
            return BaronyBattleFacing.Clamp(Path[^1].Facing);
        }
    }

    public class BaronyBattleTurnStateDTO
    {
        public List<string> InitiativeOrder { get; set; } = new();
        public int CurrentIndex { get; set; }
        /// <summary><see cref="BaronyBattleSubPhases"/> while battle is running.</summary>
        public string SubPhase { get; set; } = BaronyBattleSubPhases.Movement;
        public int Round { get; set; } = 1;
    }

    public class BaronyBattleLogEntryDTO
    {
        public string Id { get; set; } = string.Empty;
        public DateTime Utc { get; set; }
        public string Kind { get; set; } = "system";
        public string Author { get; set; } = "System";
        public string Text { get; set; } = string.Empty;
    }
}
