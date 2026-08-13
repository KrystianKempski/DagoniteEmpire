namespace DA_Common.Barony
{
    /// <summary>Excel IFS formulas for social group relation → PPB additive and percent effects.</summary>
    public static class SocialGroupPpbFormulas
    {
        private enum RelationTier
        {
            Rebellion,
            Discontent,
            Hostile,
            Indifferent,
            Satisfied,
            Friendly,
            Adored,
            Error,
        }

        public static PpbVector ComputeAdditive(string group, int relationScore)
        {
            var tier = ToTier(relationScore);
            var vector = new PpbVector();

            switch (SocialGroup.NormalizeKey(group))
            {
                case SocialGroup.Nobility:
                    vector[Ppb.Loyalty] = NobilityLoyalty(tier);
                    vector[Ppb.Stability] = NobilityStabilityLaw(tier);
                    vector[Ppb.Law] = NobilityStabilityLaw(tier);
                    vector[Ppb.Corruption] = NobilityCorruption(tier);
                    vector[Ppb.Defense] = NobilityDefense(tier);
                    break;

                case SocialGroup.Burghers:
                    var standard = BurghersStandard(tier);
                    vector[Ppb.Economy] = standard;
                    vector[Ppb.Production] = standard;
                    vector[Ppb.Loyalty] = standard;
                    vector[Ppb.Law] = standard;
                    vector[Ppb.Defense] = standard;
                    vector[Ppb.Stability] = BurghersStability(tier);
                    vector[Ppb.Corruption] = NobilityCorruption(tier);
                    break;

                case SocialGroup.Peasants:
                    var peasants = PeasantsStandard(tier);
                    vector[Ppb.Economy] = peasants;
                    vector[Ppb.Loyalty] = peasants;
                    vector[Ppb.Stability] = peasants;
                    vector[Ppb.Defense] = peasants;
                    break;
            }

            return vector;
        }

        public static PpbVector ComputePercent(string group, int relationScore)
        {
            var tier = ToTier(relationScore);
            var pct = RelationTierPercent(tier);
            if (pct == 0m)
                return new PpbVector();

            var vector = new PpbVector();
            switch (SocialGroup.NormalizeKey(group))
            {
                case SocialGroup.Nobility:
                    vector[Ppb.Defense] = pct;
                    break;
                case SocialGroup.Burghers:
                    vector[Ppb.Production] = pct;
                    break;
                case SocialGroup.Peasants:
                    vector[Ppb.Food] = pct;
                    break;
            }

            return vector;
        }

        private static RelationTier ToTier(int score) => score switch
        {
            <= -60 => RelationTier.Rebellion,
            <= -30 => RelationTier.Discontent,
            <= -10 => RelationTier.Hostile,
            <= 20 => RelationTier.Indifferent,
            <= 40 => RelationTier.Satisfied,
            <= 70 => RelationTier.Friendly,
            < 100 => RelationTier.Adored,
            _ => RelationTier.Error,
        };

        // Nobility: Loyalty (=IFS niezadowoleni,-10; bunt,-20; zadowoleni,4; przyjaźni,7; w niebo wzięci,14; Obojętni,0))
        private static decimal NobilityLoyalty(RelationTier tier) => tier switch
        {
            RelationTier.Discontent => -10,
            RelationTier.Rebellion => -20,
            RelationTier.Satisfied => 4,
            RelationTier.Friendly => 7,
            RelationTier.Adored => 14,
            _ => 0,
        };

        // Nobility: Stability / Law (=IFS bunt,-10; niezadowoleni,-5; zadowoleni,2; przyjaźni,4; w niebo wzięci,6; Obojętni,0))
        private static decimal NobilityStabilityLaw(RelationTier tier) => tier switch
        {
            RelationTier.Rebellion => -10,
            RelationTier.Discontent => -5,
            RelationTier.Satisfied => 2,
            RelationTier.Friendly => 4,
            RelationTier.Adored => 6,
            _ => 0,
        };

        // Nobility + Burghers: Corruption (=IFS bunt,5; niezadowoleni,2; zadowoleni,-1; przyjaźni,2; w niebo wzięci,4; Obojętni,0))
        private static decimal NobilityCorruption(RelationTier tier) => tier switch
        {
            RelationTier.Rebellion => 5,
            RelationTier.Discontent => 2,
            RelationTier.Satisfied => -1,
            RelationTier.Friendly => 2,
            RelationTier.Adored => 4,
            _ => 0,
        };

        // Nobility: Defense (=IFS bunt,-20; niezadowoleni,-10; zadowoleni,5; przyjaźni,10; w niebo wzięci,20; Obojętni,0))
        private static decimal NobilityDefense(RelationTier tier) => tier switch
        {
            RelationTier.Rebellion => -20,
            RelationTier.Discontent => -10,
            RelationTier.Satisfied => 5,
            RelationTier.Friendly => 10,
            RelationTier.Adored => 20,
            _ => 0,
        };

        // Burghers: Economy, Production, Loyalty, Law, Defense
        private static decimal BurghersStandard(RelationTier tier) => tier switch
        {
            RelationTier.Rebellion => -10,
            RelationTier.Discontent => -5,
            RelationTier.Satisfied => 2,
            RelationTier.Friendly => 4,
            RelationTier.Adored => 6,
            _ => 0,
        };

        // Burghers: Stability (=IFS niezadowoleni,-10; bunt,-20; zadowoleni,4; przyjaźni,7; w niebo wzięci,14; Obojętni,0))
        private static decimal BurghersStability(RelationTier tier) => NobilityLoyalty(tier);

        // Peasants: Economy, Loyalty, Stability, Defense
        private static decimal PeasantsStandard(RelationTier tier) => BurghersStandard(tier);

        // Peasants Food %, Nobility Defense %, Burghers Production %
        private static decimal RelationTierPercent(RelationTier tier) => tier switch
        {
            RelationTier.Rebellion => -50,
            RelationTier.Discontent => -30,
            RelationTier.Hostile => -10,
            RelationTier.Satisfied => 5,
            RelationTier.Friendly => 10,
            RelationTier.Adored => 30,
            _ => 0,
        };
    }
}
