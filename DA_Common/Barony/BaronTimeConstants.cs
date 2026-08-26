namespace DA_Common.Barony
{
    /// <summary>Baron's Time (BT — Baron Time) rules from the barony docs.</summary>
    public static class BaronTimeRules
    {
        /// <summary>
        /// Default BT spent on barony management each turn (system action).
        /// Also the soft reference for “5 focus slots” (100 / <see cref="JcPerFocusSlot"/>).
        /// </summary>
        public const int RequiredManagementJc = 100;

        /// <summary>Each this many management BT unlocks one Domain Panel PPB focus (full baron effect).</summary>
        public const int JcPerFocusSlot = 20;

        /// <summary>One week of expedition away from the barony.</summary>
        public const int WeeklyExpeditionJc = 25;

        /// <summary>Max weeks away per turn without management penalties (4 × weekly).</summary>
        public const int MaxSafeExpeditionWeeks = 4;

        public const int MaxSafeExpeditionJc = WeeklyExpeditionJc * MaxSafeExpeditionWeeks;

        /// <summary>Pool = (Endurance + Willpower) × this factor.</summary>
        public const int AttributeFactor = 10;

        public const string ManagementActionName = "Barony management";

        /// <summary>
        /// How many PPB focuses the baron may mark as full-effect this turn.
        /// Unfocused PPBs still apply at half. Gold is never focusable.
        /// </summary>
        public static int FocusSlotCount(int managementJc)
        {
            if (managementJc <= 0)
                return 0;
            var slots = managementJc / JcPerFocusSlot;
            var max = BaronFocusPpbs.Focusable.Count;
            return slots > max ? max : slots;
        }

        /// <summary>Legacy no-op — management unlocks focus slots instead of a global scale.</summary>
        public static decimal ManagementSkillFactor(int managementJc) => 1m;
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
