namespace DA_Models.BaronyModels
{
    /// <summary>Percent modifier to the baron's JC pool for the current turn.</summary>
    public class BaronTimeModifierDTO
    {
        public int Id { get; set; }
        public int BaronyId { get; set; }
        public string Source { get; set; } = string.Empty;
        /// <summary>Percent change to base JC (e.g. +20 or -10).</summary>
        public decimal Percent { get; set; }
        public string? Description { get; set; }
        public int SortOrder { get; set; }
    }

    /// <summary>One activity the baron spends JC on this turn.</summary>
    public class BaronTimeActionDTO
    {
        public int Id { get; set; }
        public int BaronyId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Kind { get; set; } = DA_Common.Barony.BaronTimeActionKind.Other;
        public int CostJc { get; set; }
        public string? Description { get; set; }
        public int SortOrder { get; set; }
        public bool IsSystem { get; set; }
    }

    /// <summary>Computed JC budget for the current turn.</summary>
    public readonly record struct BaronTimeBudget(
        int Endurance,
        int Willpower,
        int BaseJc,
        decimal ModifierPercent,
        int TotalJc,
        int SpentJc,
        int RemainingJc,
        int ManagementJc,
        int AdventureJc,
        decimal ExpeditionWeeks)
    {
        public bool IsManagementShort => ManagementJc < DA_Common.Barony.BaronTimeRules.RequiredManagementJc;
        public bool IsExpeditionOverLimit =>
            AdventureJc > DA_Common.Barony.BaronTimeRules.MaxSafeExpeditionJc;
        public bool IsOverspent => SpentJc > TotalJc;
        public decimal UsedPercent => TotalJc <= 0 ? 0m : Math.Min(100m, SpentJc * 100m / TotalJc);
        public decimal ManagementPercent =>
            DA_Common.Barony.BaronTimeRules.RequiredManagementJc <= 0
                ? 0m
                : Math.Min(100m, ManagementJc * 100m / DA_Common.Barony.BaronTimeRules.RequiredManagementJc);
    }
}
