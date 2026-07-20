namespace DA_Common.Barony
{
    public static class CrimePpbFormulas
    {
        public const string InputLabel = "Crime";
        public const decimal EconomyProductionPercentPerCrime = 5m;
        public const decimal EconomyProductionPercentFloor = -50m;
        public const decimal LoyaltyPerCrime = 1m;
        public const decimal StabilityPerCrime = 2m;
        public const decimal CorruptionPerCrime = 0.5m;

        public static decimal FromLawBalance(decimal lawBalance) => Math.Max(0m, -lawBalance);

        public static PpbVector ComputeAdditive(decimal crime)
        {
            var c = Math.Max(0m, crime);
            var v = new PpbVector();
            v.EnsureSize();
            if (c == 0m) return v;
            v[Ppb.Loyalty] = -LoyaltyPerCrime * c;
            v[Ppb.Stability] = -StabilityPerCrime * c;
            v[Ppb.Corruption] = CorruptionPerCrime * c;
            return v;
        }

        public static PpbVector ComputePercent(decimal crime)
        {
            var c = Math.Max(0m, crime);
            var v = new PpbVector();
            v.EnsureSize();
            if (c == 0m) return v;
            var ecoProd = Math.Max(-EconomyProductionPercentPerCrime * c, EconomyProductionPercentFloor);
            v[Ppb.Economy] = ecoProd;
            v[Ppb.Production] = ecoProd;
            return v;
        }

        public static string? ExplainAdditive(Ppb key) => key switch
        {
            Ppb.Loyalty => $"= −{InputLabel} × {LoyaltyPerCrime:0}",
            Ppb.Stability => $"= −{InputLabel} × {StabilityPerCrime:0}",
            Ppb.Corruption => $"= {InputLabel} / 2",
            _ => null,
        };

        public static string? ExplainPercent(Ppb key) => key switch
        {
            Ppb.Economy or Ppb.Production =>
                $"= max(−{InputLabel} × {EconomyProductionPercentPerCrime:0}, {EconomyProductionPercentFloor:0})",
            _ => null,
        };
    }
}
