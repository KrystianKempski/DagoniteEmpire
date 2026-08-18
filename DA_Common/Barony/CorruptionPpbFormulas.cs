using DA_Common.Localization;

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
            Loc.T("This turn: Final Corruption {0}, input {1} (= max(0, Corruption)).",
                PpbFormat.Number(corruptionFinal), PpbFormat.Number(corruption));

        public static string? ExplainAdditive(Ppb key) => key switch
        {
            Ppb.Loyalty => Loc.T("= −{0} × {1}", Loc.T(InputLabel), LoyaltyPerCorruption.ToString("0")),
            Ppb.Stability => Loc.T("= −{0} × {1}", Loc.T(InputLabel), StabilityPerCorruption.ToString("0")),
            _ => null,
        };

        public static string? ExplainPercent(Ppb key) => key switch
        {
            Ppb.Economy or Ppb.Production =>
                Loc.T("= max(−{0} × {1}, {2})", Loc.T(InputLabel), EconomyProductionPercentPerCorruption.ToString("0"), EconomyProductionPercentFloor.ToString("0")),
            _ => null,
        };

        public static string CatalogDescription =>
            "Community penalty from Corruption. "
            + $"Input = max(0, Final Corruption before Community).";
    }
}
