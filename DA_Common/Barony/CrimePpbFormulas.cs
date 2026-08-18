using DA_Common.Localization;

namespace DA_Common.Barony
{
    public static class CrimePpbFormulas
    {
        public const string InputLabel = "Crime";
        public const decimal EconomyProductionPercentPerCrime = 3m;
        public const decimal EconomyProductionPercentFloor = -50m;
        public const decimal LoyaltyPerCrime = 1m;
        public const decimal StabilityPerCrime = 2m;
        public const decimal CorruptionPerCrime = 0.5m;

        /// <summary>Crime exists only when Law is negative: Crime = max(0, −Law).</summary>
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

        public static string FormulaSummary(decimal lawFinal, decimal crime) =>
            Loc.T("This turn: Final Law {0}, Crime {1} (= max(0, −Law)). ",
                PpbFormat.Number(lawFinal), PpbFormat.Number(crime))
            + (crime > 0m
                ? Loc.T("Law is negative, so Crime equals |Law|.")
                : Loc.T("Law is not negative, so Crime is 0."));

        public static string? ExplainAdditive(Ppb key) => key switch
        {
            Ppb.Loyalty => Loc.T("= −{0} × {1}", Loc.T(InputLabel), LoyaltyPerCrime.ToString("0"))
                + "\n" + Loc.T("{0} = max(0, −Final Law).", Loc.T(InputLabel)),
            Ppb.Stability => Loc.T("= −{0} × {1}", Loc.T(InputLabel), StabilityPerCrime.ToString("0"))
                + "\n" + Loc.T("{0} = max(0, −Final Law).", Loc.T(InputLabel)),
            Ppb.Corruption => Loc.T("= {0} / 2", Loc.T(InputLabel))
                + "\n" + Loc.T("{0} = max(0, −Final Law).", Loc.T(InputLabel)),
            _ => null,
        };

        public static string? ExplainPercent(Ppb key) => key switch
        {
            Ppb.Economy or Ppb.Production =>
                Loc.T("= max(−{0} × {1}, {2})", Loc.T(InputLabel), EconomyProductionPercentPerCrime.ToString("0"), EconomyProductionPercentFloor.ToString("0"))
                + "\n" + Loc.T("{0} = max(0, −Final Law).", Loc.T(InputLabel))
                + " " + Loc.T("Exists only while Law is negative."),
            _ => null,
        };

        public static string CatalogDescription =>
            "Crime is negative Law. "
            + $"{InputLabel} = max(0, −Final Law): only when Law is below zero, and then equal to |Law|. "
            + "Final Law includes Hunger and Unrest Law penalties (Crime itself does not change Law).";
    }
}
