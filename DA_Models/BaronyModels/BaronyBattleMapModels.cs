using System;
using System.Collections.Generic;

namespace DA_Models.BaronyModels
{
    public static class BaronyBattlePhases
    {
        public const string Setup = "setup";
        public const string Battle = "battle";
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

        public int InitiativeDie { get; set; }
        public int InitiativeTotal { get; set; }
    }

    public class BaronyBattleTurnStateDTO
    {
        public List<string> InitiativeOrder { get; set; } = new();
        public int CurrentIndex { get; set; }
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
