namespace DA_Common.Barony
{
    public static class UnrestPpbFormulas
    {
        public const string InputLabel = "Unrest";
        public const decimal EconomyProductionPercentPerUnrest = 15m;
        public const decimal LoyaltyStabilityLawPerUnrest = 3m;

        public static PpbVector ComputeAdditive(decimal unrest)
        {
            var u = Math.Max(0m, unrest);
            var v = new PpbVector();
            v.EnsureSize();
            if (u == 0m) return v;
            v[Ppb.Loyalty] = -LoyaltyStabilityLawPerUnrest * u;
            v[Ppb.Stability] = -LoyaltyStabilityLawPerUnrest * u;
            v[Ppb.Law] = -LoyaltyStabilityLawPerUnrest * u;
            return v;
        }

        public static PpbVector ComputePercent(decimal unrest)
        {
            var u = Math.Max(0m, unrest);
            var v = new PpbVector();
            v.EnsureSize();
            if (u == 0m) return v;
            v[Ppb.Economy] = -EconomyProductionPercentPerUnrest * u;
            v[Ppb.Production] = -EconomyProductionPercentPerUnrest * u;
            return v;
        }

        public static string? ExplainAdditive(Ppb key) => key switch
        {
            Ppb.Loyalty or Ppb.Stability or Ppb.Law
                => $"= −{InputLabel} × {LoyaltyStabilityLawPerUnrest:0}",
            _ => null,
        };

        public static string? ExplainPercent(Ppb key) => key switch
        {
            Ppb.Economy or Ppb.Production
                => $"= −{InputLabel} × {EconomyProductionPercentPerUnrest:0}",
            _ => null,
        };
    }
}
