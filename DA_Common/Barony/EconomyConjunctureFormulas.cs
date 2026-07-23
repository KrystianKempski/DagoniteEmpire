namespace DA_Common.Barony
{
    /// <summary>
    /// Domain Panel community row: Economy vs population (conjuncture).
    /// Gold (net profit) = (Economy + Conjuncture) × 2.
    /// % = clamp(50·(Economy/(2×Population)−1) + (Conjuncture − 7), −40, +40) applied to
    /// Gold, Production, Culture, Science, and Defense.
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

        public static string FormulaSummary(
            decimal economyAdditive,
            int population,
            int conjunctureDice,
            int conjunctureModifier)
        {
            var conj = EffectiveConjuncture(conjunctureDice, conjunctureModifier);
            var gold = ComputeNetProfitGold(economyAdditive, conjunctureDice, conjunctureModifier);
            var pct = ComputePercent(economyAdditive, population, conjunctureDice, conjunctureModifier);
            var breakEven = EconomyPerPopulation * Math.Max(0, population);
            return
                $"Economy={PpbFormat.Number(economyAdditive)}, Population={population} "
                + $"(need {PpbFormat.Number(breakEven)} Economy at break-even). "
                + $"Conjuncture={conj} (2d6 {conjunctureDice}{(conjunctureModifier >= 0 ? "+" : "")}{conjunctureModifier}). "
                + $"Net Gold profit (Economy + Conjuncture) × {NetProfitGoldFactor:0} = {PpbFormat.Additive(gold)}. "
                + $"Result {PpbFormat.Percent(pct)}.";
        }

        public static string? ExplainPercent(Ppb key)
        {
            if (!AffectedKeys.Contains(key))
                return null;

            return
                $"= clamp( {RatioScale:0} × (Economy / ({EconomyPerPopulation:0} × Population) − 1) + (Conjuncture − {ConjunctureNeutral}), −{Cap:0}, +{Cap:0} )\n"
                + "Economy = additive before this row; Population = settlement population; "
                + "Conjuncture = 2d6 turn roll + MG modifier. "
                + "Affects Gold, Production, Culture, Science, Defense.";
        }

        public static string? ExplainAdditive(Ppb key)
        {
            if (key != Ppb.Treasury)
                return null;

            return
                $"= (Economy + Conjuncture) × {NetProfitGoldFactor:0}\n"
                + "Economy = additive before this row; Conjuncture = 2d6 turn roll + MG modifier. "
                + "Net gold profit from economy.";
        }

        public static string CatalogDescription =>
            "Economy vs population (2 Economy per 1 Population). "
            + "Bonus above break-even, penalty below. "
            + $"Conjuncture (2d6 + MG mod, centered on {ConjunctureNeutral}) adds fortune. "
            + $"Capped at ±{Cap:0}%. Applies % to Gold, Production, Culture, Science, Defense. "
            + $"Net Gold profit = (Economy + Conjuncture) × {NetProfitGoldFactor:0}.";
    }
}
