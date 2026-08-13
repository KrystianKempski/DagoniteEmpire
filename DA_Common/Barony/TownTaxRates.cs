namespace DA_Common.Barony
{
    public readonly record struct TownTaxRates(decimal NobilityPercent, decimal BurghersPercent, decimal PeasantsPercent)
    {
        public static readonly TownTaxRates Defaults = new(
            SocialGroup.DefaultTax(SocialGroup.Nobility),
            SocialGroup.DefaultTax(SocialGroup.Burghers),
            SocialGroup.DefaultTax(SocialGroup.Peasants));

        public static TownTaxRates FromRelations(IEnumerable<(string Group, int? TaxPercent)> relations)
        {
            decimal? nobility = null;
            decimal? burghers = null;
            decimal? peasants = null;

            foreach (var (group, taxPercent) in relations)
            {
                switch (SocialGroup.NormalizeKey(group))
                {
                    case SocialGroup.Nobility:
                        nobility = taxPercent;
                        break;
                    case SocialGroup.Burghers:
                        burghers = taxPercent;
                        break;
                    case SocialGroup.Peasants:
                        peasants = taxPercent;
                        break;
                }
            }

            return new(
                nobility ?? SocialGroup.DefaultTax(SocialGroup.Nobility),
                burghers ?? SocialGroup.DefaultTax(SocialGroup.Burghers),
                peasants ?? SocialGroup.DefaultTax(SocialGroup.Peasants));
        }

        public decimal PopulationIncome(int population, decimal nobilityGoldPerPop, decimal burghersGoldPerPop, decimal peasantsGoldPerPop)
        {
            var pop = Math.Max(0, population);
            return (NobilityPercent / 100m) * pop * nobilityGoldPerPop
                 + (BurghersPercent / 100m) * pop * burghersGoldPerPop
                 + (PeasantsPercent / 100m) * pop * peasantsGoldPerPop;
        }
    }
}
