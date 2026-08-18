using DA_Common.Localization;

namespace DA_Common.Barony
{
    public static class VillagePpbFormulas
    {
        public const decimal NobilityTaxGoldPerPop = 5m;
        public const decimal BurghersTaxGoldPerPop = 5m;
        public const decimal PeasantsTaxGoldPerPop = 15m;
        public const string InputLabel = "Population";

        public static decimal FarmFoodForFertility(int fertility) => fertility switch
        {
            2 => 0.8m,
            3 => 1.5m,
            4 => 2m,
            5 => 3m,
            _ => 0m,
        };

        /// <summary>Fertility farm yield, or 0 in Winter.</summary>
        public static decimal FarmFoodForFertility(int fertility, string? season) =>
            BaronyCalendarFormulas.FarmsProduceFood(season) ? FarmFoodForFertility(fertility) : 0m;

        public static PpbVector Compute(int population, int fertility, bool hasPalisade = false) =>
            Compute(population, fertility, hasPalisade, TownTaxRates.Defaults, season: null);

        public static PpbVector Compute(int population, int fertility, bool hasPalisade, TownTaxRates taxes) =>
            Compute(population, fertility, hasPalisade, taxes, season: null);

        public static PpbVector Compute(
            int population,
            int fertility,
            bool hasPalisade,
            TownTaxRates taxes,
            string? season)
        {
            var pop = Math.Max(0, population);
            var v = new PpbVector();
            v.EnsureSize();
            v[Ppb.Food] = FarmFoodForFertility(fertility, season) - pop;
            v[Ppb.Economy] = pop / 2m;
            v[Ppb.Production] = pop;
            v[Ppb.Loyalty] = -pop;
            v[Ppb.Stability] = -2m * pop + (hasPalisade ? 3m : 0m);
            v[Ppb.Law] = -pop / 2m + (hasPalisade ? 1m : 0m);
            v[Ppb.Corruption] = pop / 4m;
            v[Ppb.Science] = pop / 4m;
            v[Ppb.Culture] = pop / 4m;
            v[Ppb.Magic] = pop / 8m;
            v[Ppb.Defense] = pop + (hasPalisade ? 5m : 0m);
            v[Ppb.Treasury] = taxes.PopulationIncome(pop, NobilityTaxGoldPerPop, BurghersTaxGoldPerPop, PeasantsTaxGoldPerPop);
            return v;
        }

        public static string? ExplainAdditive(Ppb key, bool hasPalisade = false, string? season = null)
        {
            var p = Loc.T(InputLabel);
            return key switch
            {
                Ppb.Food when !BaronyCalendarFormulas.FarmsProduceFood(season) =>
                    Loc.T("= 0 farm yield (Winter) − {0}", p),
                Ppb.Food => Loc.T("= farm yield − {0}", p),
                Ppb.Economy => Loc.T("= {0} / 2", p),
                Ppb.Production => Loc.T("= {0}", p),
                Ppb.Loyalty => Loc.T("= −{0}", p),
                Ppb.Stability when hasPalisade => Loc.T("= −2 × {0} + palisade", p),
                Ppb.Stability => Loc.T("= −2 × {0}", p),
                Ppb.Law when hasPalisade => Loc.T("= −{0} / 2 + palisade", p),
                Ppb.Law => Loc.T("= −{0} / 2", p),
                Ppb.Corruption => Loc.T("= {0} / 4", p),
                Ppb.Science => Loc.T("= {0} / 4", p),
                Ppb.Culture => Loc.T("= {0} / 4", p),
                Ppb.Magic => Loc.T("= {0} / 8", p),
                Ppb.Defense when hasPalisade => Loc.T("= {0} + palisade", p),
                Ppb.Defense => Loc.T("= {0}", p),
                Ppb.Treasury => Loc.T("= taxes% × Population × (5/5/15)"),
                _ => null,
            };
        }

        public static string CatalogDescription =>
            "A small village allowing farms and sawmills on distant land. Besides gold and production cost, requires 1 community relocated from elsewhere. "
            + "PPB comes from Population, tile fertility, and an optional palisade — hover each PPB value for its formula "
            + "(Economy = Population/2, Law = −Population/2, Corruption = Population/4, Loyalty = −Population; Food = farm yield − Population; farm yield is 0 in Winter).";
    }
}
