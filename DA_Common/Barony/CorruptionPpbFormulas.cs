namespace DA_Common.Barony
{
    public static class CorruptionPpbFormulas
    {
        public const string InputLabel = "Corruption";
        public const decimal EconomyProductionPercentPerCorruption = 3m;
        public const decimal EconomyProductionPercentFloor = -50m;
        public const decimal LoyaltyPerCorruption = 2m;
        public const decimal StabilityPerCorruption = 1m;

        public static decimal FromCorruptionBalance(decimal corruptionBalance) => Math.Max(0m, corruptionBalance);

        public static PpbVector ComputeAdditive(decimal corruption)
        {
            var c = Math.Max(0m, corruption);
            var v = new PpbVector();
            v.EnsureSize();
            if (c == 0m) return v;
            v[Ppb.Loyalty] = -LoyaltyPerCorruption * c;
            v[Ppb.Stability] = -StabilityPerCorruption * c;
            return v;
        }

        public static PpbVector ComputePercent(decimal corruption)
        {
            var c = Math.Max(0m, corruption);
            var v = new PpbVector();
            v.EnsureSize();
            if (c == 0m) return v;
            var ecoProd = Math.Max(-EconomyProductionPercentPerCorruption * c, EconomyProductionPercentFloor);
            v[Ppb.Economy] = ecoProd;
            v[Ppb.Production] = ecoProd;
            return v;
        }

        public static string FormulaSummary(decimal corruptionFinal, decimal corruption) =>
            $"This turn: Final Corruption {PpbFormat.Number(corruptionFinal)}, "
            + $"input {PpbFormat.Number(corruption)} (= max(0, Corruption)).";

        public static string? ExplainAdditive(Ppb key) => key switch
        {
            Ppb.Loyalty => $"= −{InputLabel} × {LoyaltyPerCorruption:0}",
            Ppb.Stability => $"= −{InputLabel} × {StabilityPerCorruption:0}",
            _ => null,
        };

        public static string? ExplainPercent(Ppb key) => key switch
        {
            Ppb.Economy or Ppb.Production =>
                $"= max(−{InputLabel} × {EconomyProductionPercentPerCorruption:0}, {EconomyProductionPercentFloor:0})",
            _ => null,
        };

        public static string CatalogDescription =>
            "Community penalty from Corruption. "
            + $"Input = max(0, Final Corruption before Community).";
    }
}
