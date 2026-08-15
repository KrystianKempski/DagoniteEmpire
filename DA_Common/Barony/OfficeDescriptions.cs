namespace DA_Common.Barony
{
    /// <summary>Flavor text for core offices (Domain Panel / Offices page).</summary>
    public static class OfficeDescriptions
    {
        public const string Chancellor =
            "Jeden z najważniejszych urzędów baronii. Kanclerz zarządza kontaktami władcy z "
            + "wasalami i suzerenami, odczytuje lojalność i nastroje poddanych wobec rządu oraz "
            + "stara się kształtować jedno i drugie. Urząd nadzoruje także rozwój kultury. Kanclerz "
            + "może rządzić przez sympatię — łagodząc konflikty i odwołując się do rozsądku — przez "
            + "strach, groźby i surowe kary za nieposłuszeństwo, albo przez wyważone połączenie obu podejść.";

        public const string GuardCaptain =
            "Niezbędny w najmniejszych włościach: jednocześnie stróż prawa, dowódca wojskowy i obrońca "
            + "barona oraz jego ziem. Później większość tych obowiązków przechodzi na generała, strażnika "
            + "granic, głównego sędziego i innych — lecz dopóki baronia nie urośnie w księstwo, sam Kapitan "
            + "Straży w zupełności im podoła.";

        public const string Steward =
            "Ekonom nadzoruje wszystko, co związane z dochodami, budową, zaopatrzeniem i poborem podatków.";

        /// <summary>Catalog text for a core office; null for Baron / Custom.</summary>
        public static string? For(string? officeType) => officeType switch
        {
            OfficeType.Chancellor => Chancellor,
            OfficeType.GuardCaptain => GuardCaptain,
            OfficeType.Steward => Steward,
            _ => null,
        };
    }
}
