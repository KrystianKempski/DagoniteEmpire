namespace DA_Common.Barony
{
    /// <summary>
    /// Colors / icons for all PPB keys.
    /// Cumulative resources match the HUD; domain stats (Economy, Loyalty, …) use dedicated accents.
    /// </summary>
    public static class PpbVisuals
    {
        public static string ColorKey(Ppb p) => p switch
        {
            Ppb.Food => "food",
            Ppb.Economy => "economy",
            Ppb.Production => "production",
            Ppb.Loyalty => "loyalty",
            Ppb.Stability => "stability",
            Ppb.Law => "law",
            Ppb.Corruption => "corruption",
            Ppb.Science => "science",
            Ppb.Magic => "magic",
            Ppb.Culture => "culture",
            Ppb.Intelligence => "intelligence",
            Ppb.Defense => "defense",
            Ppb.Treasury => "gold",
            _ => "default",
        };

        /// <summary>Accent hex for masked SVG icons and tinted chips.</summary>
        public static string ColorHex(Ppb p) => p switch
        {
            Ppb.Food => "#6bcf75",
            Ppb.Economy => "#a67c52",       // brown
            Ppb.Production => "#e07a8a",
            Ppb.Loyalty => "#d63b3b",       // red
            Ppb.Stability => "#4f9a5a",     // green
            Ppb.Law => "#f0ece4",           // white / parchment
            Ppb.Corruption => "#6a7a32",    // rotten green
            Ppb.Science => "#6aa8f0",
            Ppb.Magic => "#b07aef",
            Ppb.Culture => "#f0a45a",
            Ppb.Intelligence => "#c4b8d8",
            Ppb.Defense => "#a8b0bc",
            Ppb.Treasury => "#f0d060",
            _ => "#d4cfc6",
        };

        /// <summary>Icon under wwwroot, or null when no dedicated icon exists.</summary>
        public static string? IconUrl(Ppb p) => p switch
        {
            Ppb.Food => "/icons/wheat.svg",
            Ppb.Economy => "/icons/trade.svg",
            Ppb.Production => "/icons/gear-hammer.svg",
            Ppb.Loyalty => "/icons/heart.svg",
            Ppb.Stability => "/icons/peace-dove.svg",
            Ppb.Law => "/icons/scales.svg",
            Ppb.Corruption => "/icons/dead-wood.svg",
            Ppb.Science => "/icons/erlenmeyer.svg",
            Ppb.Magic => "/icons/crystal-wand.svg",
            Ppb.Culture => "/icons/lyre.svg",
            Ppb.Intelligence => "/icons/hood.svg",
            Ppb.Defense => "/icons/shield.svg",
            Ppb.Treasury => "/icons/two-coins.svg",
            _ => null,
        };

        public static bool HasIcon(Ppb p) => !string.IsNullOrEmpty(IconUrl(p));
    }
}
