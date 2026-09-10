namespace DA_Common.Barony
{
    /// <summary>Categories of barony chronicle entries; drives filtering and colouring in the log tab.</summary>
    public static class BaronyLogCategory
    {
        public const string TurnResolve = "TurnResolve";
        public const string Resources = "Resources";
        public const string Projects = "Projects";
        public const string Events = "Events";
        public const string BaronTime = "BaronTime";
        public const string Relations = "Relations";
        public const string Battle = "Battle";
        public const string Army = "Army";
        public const string Trade = "Trade";
        public const string Court = "Court";
        public const string Other = "Other";

        /// <summary>Display order in filters and legends.</summary>
        public static readonly string[] All =
        {
            TurnResolve, Resources, Projects, Events, BaronTime,
            Relations, Battle, Army, Trade, Court, Other,
        };

        /// <summary>English label; the UI passes it through the localizer.</summary>
        public static string Label(string? category) => category switch
        {
            TurnResolve => "Turn resolve",
            Resources => "Resources",
            Projects => "Projects",
            Events => "Events",
            BaronTime => "Baron's time",
            Relations => "Relations",
            Battle => "Battles",
            Army => "Army",
            Trade => "Trade",
            Court => "Court",
            _ => "Other",
        };

        public static string Normalize(string? category)
        {
            if (string.IsNullOrWhiteSpace(category))
                return Other;
            foreach (var known in All)
            {
                if (string.Equals(known, category, StringComparison.OrdinalIgnoreCase))
                    return known;
            }
            return Other;
        }
    }

    /// <summary>Who triggered a logged action.</summary>
    public static class BaronyLogActorRole
    {
        public const string Baron = "Baron";
        public const string GameMaster = "GM";
        public const string System = "System";
    }

    /// <summary>
    /// Calendar stamp for a chronicle entry. Used when the entry belongs to a different turn
    /// than the barony currently sits in — the turn resolve report closes the ending turn.
    /// </summary>
    public sealed record BaronyLogStamp(int TurnNumber, int Year, int Month, string Season);
}
