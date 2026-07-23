namespace DA_Common.Barony
{
    /// <summary>
    /// Domain Panel community row: Economy vs population (conjuncture).
    /// Gold (net profit) = (Economy + Conjuncture) × 2.
    /// % = 50·(Economy/(2×Population)−1) + (Conjuncture − 7), soft-capped for extremes,
    /// applied to Gold, Production, Loyalty, Stability, Magic, Culture, Science, and Defense.
    /// Conjuncture = 2d6 (turn roll) + MG modifier.
    /// </summary>
    public static class EconomyConjunctureFormulas
    {
        public const string InputLabel = "Economy";
        public const decimal EconomyPerPopulation = 2m;
        public const decimal RatioScale = 50m;
        public const decimal Cap = 40m;
        public const int ConjunctureNeutral = 7;
        /// <summary>Net gold profit: (Economy + Conjuncture) × this factor.</summary>
        public const decimal NetProfitGoldFactor = 2m;

        public static readonly Ppb[] AffectedKeys =
        {
            Ppb.Treasury,
            Ppb.Production,
            Ppb.Loyalty,
            Ppb.Stability,
            Ppb.Magic,
            Ppb.Culture,
            Ppb.Science,
            Ppb.Defense,
        };

        public static int EffectiveConjuncture(int conjunctureDice, int conjunctureModifier)
            => conjunctureDice + conjunctureModifier;

        /// <summary>Net gold profit from economy: (Economy + Conjuncture) × 2.</summary>
        public static decimal ComputeNetProfitGold(
            decimal economyAdditive,
            int conjunctureDice,
            int conjunctureModifier)
        {
            var conj = EffectiveConjuncture(conjunctureDice, conjunctureModifier);
            return (economyAdditive + conj) * NetProfitGoldFactor;
        }

        public static decimal ComputePercent(
            decimal economyAdditive,
            int population,
            int conjunctureDice,
            int conjunctureModifier)
        {
            if (population <= 0)
                return 0m;

            var ratioTerm = RatioScale * (economyAdditive / (EconomyPerPopulation * population) - 1m);
            var fortune = EffectiveConjuncture(conjunctureDice, conjunctureModifier) - ConjunctureNeutral;
            return decimal.Round(Math.Clamp(ratioTerm + fortune, -Cap, Cap), 0);
        }

        public static PpbVector ComputeAdditive(
            decimal economyAdditive,
            int conjunctureDice,
            int conjunctureModifier)
        {
            var v = new PpbVector();
            v.EnsureSize();
            v[Ppb.Treasury] = ComputeNetProfitGold(economyAdditive, conjunctureDice, conjunctureModifier);
            return v;
        }

        public static PpbVector ComputePercentVector(
            decimal economyAdditive,
            int population,
            int conjunctureDice,
            int conjunctureModifier)
        {
            var v = new PpbVector();
            v.EnsureSize();
            var pct = ComputePercent(economyAdditive, population, conjunctureDice, conjunctureModifier);
            if (pct == 0m)
                return v;

            foreach (var key in AffectedKeys)
                v[key] = pct;
            return v;
        }

        /// <summary>This-turn inputs for the Economy row name tooltip (no formulas).</summary>
        public static string FormulaSummary(
            decimal economyAdditive,
            int population,
            int conjunctureDice,
            int conjunctureModifier)
        {
            var conj = EffectiveConjuncture(conjunctureDice, conjunctureModifier);
            var modSign = conjunctureModifier >= 0 ? "+" : "";
            return
                $"This turn: Economy {PpbFormat.Number(economyAdditive)}, "
                + $"Population {population}, "
                + $"Conjuncture {conj} (2d6 {conjunctureDice}{modSign}{conjunctureModifier}).";
        }

        public static string? ExplainPercent(Ppb key)
        {
            if (!AffectedKeys.Contains(key))
                return null;

            return
                $"= {RatioScale:0} × (Economy / ({EconomyPerPopulation:0} × Population) − 1) "
                + $"+ (Conjuncture − {ConjunctureNeutral})\n"
                + "Economy = additive before this row; Population = settlement population; "
                + "Conjuncture = 2d6 turn roll + MG modifier.\n"
                + "Same % applies to Gold, Production, Loyalty, Stability, Magic, Culture, Science, and Defense.";
        }

        public static string? ExplainAdditive(Ppb key)
        {
            if (key != Ppb.Treasury)
                return null;

            return
                $"= (Economy + Conjuncture) × {NetProfitGoldFactor:0}\n"
                + "Economy = additive before this row; Conjuncture = 2d6 turn roll + MG modifier.\n"
                + "Net gold profit from the economy.";
        }

        public static string CatalogDescription =>
            "Economy is a vital part of the barony. It is produced by the population and shapes many resources. "
            + "It also depends on outside circumstances and a measure of chance. "
            + "Depending on its condition, it can strengthen resource output or weaken it. "
            + "Keeping Economy high is well worth the effort.";
    }
}
