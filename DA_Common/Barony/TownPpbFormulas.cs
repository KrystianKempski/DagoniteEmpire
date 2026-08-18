using DA_Common.Localization;

namespace DA_Common.Barony
{
    public static class TownPpbFormulas
    {
        public const decimal NobilityTaxGoldPerPop = 20m;
        public const decimal BurghersTaxGoldPerPop = 25m;
        public const decimal PeasantsTaxGoldPerPop = 10m;
        public const string InputLabel = "Population";

        public static PpbVector Compute(int population) => Compute(population, TownTaxRates.Defaults);

        public static PpbVector Compute(int population, TownTaxRates taxes)
        {
            var pop = Math.Max(0, population);
            var v = new PpbVector();
            v.EnsureSize();
            v[Ppb.Food] = -pop;
            v[Ppb.Economy] = pop;
            v[Ppb.Production] = 2m * pop;
            v[Ppb.Loyalty] = -2m * pop;
            v[Ppb.Stability] = -3m * pop;
            v[Ppb.Law] = -pop;
            v[Ppb.Corruption] = pop;
            v[Ppb.Science] = pop / 2m;
            v[Ppb.Magic] = pop / 4m;
            v[Ppb.Culture] = pop / 2m;
            v[Ppb.Defense] = 2m * pop;
            v[Ppb.Treasury] = taxes.PopulationIncome(pop, NobilityTaxGoldPerPop, BurghersTaxGoldPerPop, PeasantsTaxGoldPerPop);
            return v;
        }

        public static string? ExplainAdditive(Ppb key) => key switch
        {
            Ppb.Food => Loc.T("= −Population"),
            Ppb.Economy => Loc.T("= Population"),
            Ppb.Production => Loc.T("= 2 × Population"),
            Ppb.Loyalty => Loc.T("= −2 × Population"),
            Ppb.Stability => Loc.T("= −3 × Population"),
            Ppb.Law => Loc.T("= −Population"),
            Ppb.Corruption => Loc.T("= Population"),
            Ppb.Science => Loc.T("= Population / 2"),
            Ppb.Magic => Loc.T("= Population / 4"),
            Ppb.Culture => Loc.T("= Population / 2"),
            Ppb.Defense => Loc.T("= 2 × Population"),
            Ppb.Treasury => Loc.T("= taxes% × Population × (20/25/10)"),
            _ => null,
        };

        public static string CatalogDescription =>
            "The barony’s main town. PPB comes from Population — hover each PPB value for its formula. "
            + "Unlike a village: Food = −Population (no farm yield); Economy / Law / Corruption at full Population "
            + "(village uses Population/2, −Population/2, Population/4); twice Defense, Science, Culture, and Magic; "
            + "Loyalty −2×Population; Stability −3×Population. "
            + "Walls are a separate building (not included here).";

        public static string PopulationRowLabel(string cityName)
        {
            var name = string.IsNullOrWhiteSpace(cityName) ? "Town" : cityName.Trim();
            return Loc.T("Population of {0}", name);
        }
    }
}
