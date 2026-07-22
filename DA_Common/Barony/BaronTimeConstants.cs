namespace DA_Common.Barony
{
    /// <summary>Baron's Time (JC — time units) rules from the barony docs.</summary>
    public static class BaronTimeRules
    {
        /// <summary>JC that must be spent on essential barony management each turn.</summary>
        public const int RequiredManagementJc = 100;

        /// <summary>One week of expedition away from the barony.</summary>
        public const int WeeklyExpeditionJc = 25;

        /// <summary>Max weeks away per turn without management penalties (4 × weekly).</summary>
        public const int MaxSafeExpeditionWeeks = 4;

        public const int MaxSafeExpeditionJc = WeeklyExpeditionJc * MaxSafeExpeditionWeeks;

        /// <summary>Pool = (Endurance + Willpower) × this factor.</summary>
        public const int AttributeFactor = 10;

        public const string ManagementActionName = "Barony management";

        /// <summary>
        /// Share of skill PPB that applies this turn (0–1).
        /// 100 JC management → 100%; 50 JC → 50%.
        /// </summary>
        public static decimal ManagementSkillFactor(int managementJc)
        {
            if (managementJc <= 0)
                return 0m;
            var factor = managementJc / (decimal)RequiredManagementJc;
            return factor > 1m ? 1m : factor;
        }
    }

    /// <summary>Categories of baron time actions.</summary>
    public readonly struct BaronTimeActionKind
    {
        public const string Management = "Management";
        public const string Adventure = "Adventure";
        public const string Hunt = "Hunt";
        public const string Relations = "Relations";
        public const string Research = "Research";
        public const string Audience = "Audience";
        public const string Skills = "Skills";
        public const string Other = "Other";

        public static readonly string[] All =
        {
            Management, Adventure, Hunt, Relations, Research, Audience, Skills, Other,
        };
    }

    /// <summary>Suggested costs for common time actions.</summary>
    public static class BaronTimeSuggestedCosts
    {
        public const int GreatHuntJc = 15;
        public const int GreatHuntProduction = 10;
        public const int GreatHuntGold = 5;
    }
}
