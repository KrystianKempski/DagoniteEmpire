namespace DA_Common.Barony
{
    public static class UnrestPpbFormulas
    {
        public const string InputLabel = "Unrest";
        public const int Max = 5;
        public const decimal EconomyProductionPercentPerUnrest = 10m;
        public const decimal LoyaltyStabilityLawPerUnrest = 3m;

        public static int Clamp(int unrest) => Math.Clamp(unrest, 0, Max);

        public static PpbVector ComputeAdditive(decimal unrest)
        {
            var u = Math.Max(0m, Math.Min(Max, unrest));
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
            var u = Math.Max(0m, Math.Min(Max, unrest));
            var v = new PpbVector();
            v.EnsureSize();
            if (u == 0m) return v;
            v[Ppb.Economy] = -EconomyProductionPercentPerUnrest * u;
            v[Ppb.Production] = -EconomyProductionPercentPerUnrest * u;
            return v;
        }

        public static string FormulaSummary(decimal unrest) =>
            $"This turn: Unrest {PpbFormat.Number(unrest)} (barony level, 0–{Max}).";

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

        public static string CatalogDescription =>
            "Community penalty from barony Unrest (0–5). "
            + "Also reduces Law, which can raise Crime (= max(0, −Final Law)).";
    }
}
