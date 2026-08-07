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
        public List<BaronyBattleXpTallyDTO> Tallies { get; set; } = new();
        public BaronyBattleXpSummaryDTO? XpSummary { get; set; }
    }

    public class BaronyBattleXpTallyDTO
    {
        public int UnitId { get; set; }
        public string UnitName { get; set; } = string.Empty;
        public int DamageDealt { get; set; }
        public int DamageTaken { get; set; }
        public int Kills { get; set; }
        public bool Fled { get; set; }
        public List<int> RoundsEngaged { get; set; } = new();
    }

    public class BaronyBattleXpSummaryDTO
    {
        public string Id { get; set; } = string.Empty;
        public DateTime Utc { get; set; }
        public bool AcknowledgedByBaron { get; set; }
        public List<BaronyBattleXpSummaryEntryDTO> Entries { get; set; } = new();
    }

    public class BaronyBattleXpSummaryEntryDTO
    {
        public int UnitId { get; set; }
        public string UnitName { get; set; } = string.Empty;
        public int DamageDealt { get; set; }
        public int DamageTaken { get; set; }
        public int EngagedRounds { get; set; }
        public int Kills { get; set; }
        public bool Fled { get; set; }
        public int XpFromDamageDealt { get; set; }
        public int XpFromEngagedRounds { get; set; }
        public int XpFromKills { get; set; }
        public int XpLossFromDamageTaken { get; set; }
        public int XpLossFromFlee { get; set; }
        public int XpNetBase { get; set; }
        public int MgBonusXp { get; set; }
        public string MgNote { get; set; } = string.Empty;
        public int XpNetFinal => XpNetBase + MgBonusXp;
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

        /// <summary>True when this waypoint belongs to a declared charge segment.</summary>
        public bool IsCharge { get; set; }
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

        /// <summary>
        /// Ranged attack reach in move-points (same half-point metric as movement).
        /// 0 = melee only. Allies copy it from their primary weapon; MG enemies set it by hand.
        /// </summary>
        public int Range { get; set; }

        /// <summary>
        /// Armor this unit's attacks cut through, subtracted from the target's Armor (never below 0).
        /// 0 = no pierce. Allies copy it from their primary weapon; MG enemies set it by hand.
        /// </summary>
        public int Pierce { get; set; }

        /// <summary><see cref="BaronyBattleFacing"/> — current front (0 = North).</summary>
        public int Facing { get; set; } = BaronyBattleFacing.North;

        /// <summary>Target token ID assigned during attack-planning phase; null = no attack.</summary>
        public string? AttackTargetId { get; set; }

        /// <summary>
        /// Charge intent this turn: default +2 Attack / +1 Damage vs <see cref="ChargeTargetId"/> in Combat
        /// (overridden by <see cref="ThunderCharge"/> / Shock Lance flags).
        /// Cleared when the charge target is changed or at end of combat.
        /// </summary>
        public bool ChargeBonus { get; set; }

        /// <summary>Captain Thunder Charge: charge bonus +3 Atk / +2 Dmg instead of +2/+1.</summary>
        public bool ThunderCharge { get; set; }

        /// <summary>Captain Flying Start: charge minimum path −1 tile.</summary>
        public bool FlyingStart { get; set; }

        /// <summary>Extra charge Damage when mounted (Shock Lance).</summary>
        public int ChargeDamageExtra { get; set; }

        /// <summary>This token's unit is mounted (context for Mounted Superiority).</summary>
        public bool Mounted { get; set; }

        /// <summary>This token's unit has an assigned captain (context for Kill the Captain).</summary>
        public bool HasCaptain { get; set; }

        /// <summary>Captain Initiative bonus (Discipline / Initiative abilities), added to InitiativeTotal.</summary>
        public int CommanderInitiative { get; set; }

        /// <summary>Drill — Shot: +1 Attack on ranged attacks.</summary>
        public bool CmdDrillShot { get; set; }

        /// <summary>Mounted Superiority: +1 Atk/Def when mounted vs an unmounted foe.</summary>
        public bool CmdMountedSuperiority { get; set; }

        /// <summary>Counter-Charge: +2 Defense when targeted by a charge.</summary>
        public bool CmdCounterCharge { get; set; }

        /// <summary>Pike Hedge: +3 Defense when the attacker has a charge bonus against this unit.</summary>
        public bool CmdPikeHedge { get; set; }

        /// <summary>Return Stroke: +2 Defense on melee defensive return hits.</summary>
        public bool CmdReturnStroke { get; set; }

        /// <summary>Kill the Captain: +2 Attack against units that have a captain.</summary>
        public bool CmdKillTheCaptain { get; set; }

        /// <summary>Riveted Plate: Pierce ignored when this unit is the defender.</summary>
        public int CmdPierceIgnore { get; set; }

        /// <summary>Wedge: during a charge, one Difficult tile is treated as open.</summary>
        public bool CmdWedge { get; set; }

        /// <summary>Unbroken Momentum: charge interrupt distance lowered to 1 tile (default 2).</summary>
        public bool CmdUnbrokenMomentum { get; set; }

        /// <summary>Blind Fury: blind-charge end-lock also catches side-front corners.</summary>
        public bool CmdBlindFury { get; set; }

        /// <summary>Overrun: heal 12 HP when a charge target flees this Combat.</summary>
        public bool CmdOverrun { get; set; }

        /// <summary>Long Shot: range Attack penalty is −1 per tile instead of −2.</summary>
        public bool CmdLongShot { get; set; }

        /// <summary>Snap Shot: +1 Damage on shots against adjacent targets.</summary>
        public bool CmdSnapShot { get; set; }

        /// <summary>Skirmish Screen: may shoot even while engaged (pinned).</summary>
        public bool CmdSkirmishScreen { get; set; }

        /// <summary>Enfilade: shots escalate the target's Exposure band by one step.</summary>
        public bool CmdEnfilade { get; set; }

        /// <summary>Harassing Fire: a damaging shot lowers the target's next-turn Move by 1.</summary>
        public bool CmdHarassingFire { get; set; }

        /// <summary>Knife in the Dark: +1 Damage when attacking a target from its rear.</summary>
        public bool CmdKnifeInTheDark { get; set; }

        /// <summary>Keep Facing: one in-place facing change per Movement phase spends no Move.</summary>
        public bool CmdKeepFacing { get; set; }

        /// <summary>Look Away: one post-move facing change per Movement phase spends no Move.</summary>
        public bool CmdLookAway { get; set; }

        /// <summary>Column March: squeezing past comrades costs open-ground rates, not difficult ×2.</summary>
        public bool CmdColumnMarch { get; set; }

        /// <summary>Loose Files: one diagonal step per phase may pass between two occupied corners.</summary>
        public bool CmdLooseFiles { get; set; }

        /// <summary>Ironclad: captain may spend a once-per-battle +2 Armor combat-round buff.</summary>
        public bool CmdIronclad { get; set; }

        /// <summary>Ironclad has already been spent this battle.</summary>
        public bool IroncladUsed { get; set; }

        /// <summary>Ironclad is boosting Armor (+2) for the current combat round.</summary>
        public bool IroncladActive { get; set; }

        /// <summary>Full Defense stance: +4 Defence, −5 Attack; the unit forfeits all movement this round.</summary>
        public bool FullDefense { get; set; }

        /// <summary>Set once the free commander facing change was used this Movement phase.</summary>
        public bool FreeFacingUsed { get; set; }

        /// <summary>Set when Harassing Fire hit this token; consumed at the next Movement phase.</summary>
        public bool HarassedNextTurn { get; set; }

        /// <summary>
        /// True when the charge was declared without an immediate target (blind).
        /// Blind charges can lock a target after only 2 tiles of travel on collision.
        /// </summary>
        public bool ChargeBlind { get; set; }

        /// <summary>Enemy token the charge locked onto (must match <see cref="AttackTargetId"/> for the bonus).</summary>
        public string? ChargeTargetId { get; set; }

        /// <summary>Enemy's configured base shot range before commander modifiers like Extended Range.</summary>
        public int EnemyBaseRange { get; set; }

        /// <summary>Enemy base stats captured once, so passive commander bonuses apply idempotently.</summary>
        public bool EnemyBaseCaptured { get; set; }
        public int EnemyBaseHp { get; set; }
        public int EnemyBaseMove { get; set; }
        public int EnemyBaseDamage { get; set; }
        public int EnemyBaseDefense { get; set; }
        public int EnemyBaseDiscipline { get; set; }

        /// <summary>
        /// For manually configured enemy commanders: selected commander ability keys from the skill tree.
        /// Allies derive abilities from their assigned captain sheet instead.
        /// </summary>
        public List<string> EnemyCommanderAbilities { get; set; } = new();

        /// <summary>
        /// For allies: commander ability keys currently affecting this unit (behavioral + active passives),
        /// captured from the captain sheet at deploy time so they can be shown as badges.
        /// </summary>
        public List<string> AllyCommanderAbilities { get; set; } = new();

        /// <summary>
        /// Grid cell where the charge segment begins (after any pre-charge march).
        /// Kept across collision path truncates so arrows/validation still know the split.
        /// </summary>
        public int? ChargeOriginX { get; set; }
        public int? ChargeOriginY { get; set; }

        /// <summary>
        /// Hostile tokens this unit is locked in melee with. Leaving their reach costs a free hit.
        /// Populated by movement collisions and combat exchanges; cleared when contact breaks or a unit flees.
        /// </summary>
        public List<string> EngagedEnemyIds { get; set; } = new();

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
        /// <summary>
        /// Baron confirms their attack orders are done; only the Game Master advances the phase.
        /// </summary>
        public bool BaronPhaseReady { get; set; }
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
