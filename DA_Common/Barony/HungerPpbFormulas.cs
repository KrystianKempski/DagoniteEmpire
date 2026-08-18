using DA_Common.Localization;

namespace DA_Common.Barony
{
    public static class HungerPpbFormulas
    {
        public const string InputLabel = "Hunger";
        public const decimal EconomyProductionPercentPerHunger = 5m;
        public const decimal EconomyProductionPercentFloor = -50m;
        public const decimal LoyaltyStabilityPerHunger = 3m;
        public const decimal LawPerHunger = 2m;
        public const decimal CorruptionPerHunger = 1m;

        public static decimal FromFoodBalance(decimal foodBalance) => Math.Max(0m, -foodBalance);

        public static PpbVector ComputeAdditive(decimal hunger)
        {
            var h = Math.Max(0m, hunger);
            var v = new PpbVector();
            v.EnsureSize();
            if (h == 0m) return v;
            v[Ppb.Loyalty] = -LoyaltyStabilityPerHunger * h;
            v[Ppb.Stability] = -LoyaltyStabilityPerHunger * h;
            v[Ppb.Law] = -LawPerHunger * h;
            v[Ppb.Corruption] = CorruptionPerHunger * h;
            return v;
        }

        public static PpbVector ComputePercent(decimal hunger)
        {
            var h = Math.Max(0m, hunger);
            var v = new PpbVector();
            v.EnsureSize();
            if (h == 0m) return v;
            var ecoProd = Math.Max(-EconomyProductionPercentPerHunger * h, EconomyProductionPercentFloor);
            v[Ppb.Economy] = ecoProd;
            v[Ppb.Production] = ecoProd;
            return v;
        }

        public static string FormulaSummary(decimal foodFinal, decimal hunger) =>
            Loc.T("This turn: Final Food {0}, Hunger {1} (= max(0, −Food)).",
                PpbFormat.Number(foodFinal), PpbFormat.Number(hunger));

        public static string? ExplainAdditive(Ppb key) => key switch
        {
            Ppb.Loyalty => Loc.T("= −{0} × {1}", Loc.T(InputLabel), LoyaltyStabilityPerHunger.ToString("0")),
            Ppb.Stability => Loc.T("= −{0} × {1}", Loc.T(InputLabel), LoyaltyStabilityPerHunger.ToString("0")),
            Ppb.Law => Loc.T("= −{0} × {1}", Loc.T(InputLabel), LawPerHunger.ToString("0")),
            Ppb.Corruption => Loc.T("= {0} × {1}", Loc.T(InputLabel), CorruptionPerHunger.ToString("0")),
            _ => null,
        };

        public static string? ExplainPercent(Ppb key) => key switch
        {
            Ppb.Economy or Ppb.Production =>
                Loc.T("= max(−{0} × {1}, {2})", Loc.T(InputLabel), EconomyProductionPercentPerHunger.ToString("0"), EconomyProductionPercentFloor.ToString("0")),
            _ => null,
        };

        public static string CatalogDescription =>
            "Community penalty when Food balance is below zero. "
            + $"{InputLabel} = max(0, −Final Food before Community).";
    }
}
