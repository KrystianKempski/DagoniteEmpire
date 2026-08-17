namespace DA_Common.Barony
{
    using DA_Common.Localization;

    /// <summary>Flavor text for core offices (Domain Panel / Offices page).</summary>
    public static class OfficeDescriptions
    {
        public const string Chancellor =
            "One of the barony's most important offices. The chancellor manages the ruler's dealings with "
            + "vassals and liege lords, reads the loyalty and mood of subjects toward the government, and "
            + "works to shape both. The office also oversees cultural development. A chancellor may lead "
            + "through affection—easing conflicts and appealing to reason—through fear, threats, and harsh "
            + "penalties for disobedience, or through a balanced mix of both.";

        public const string GuardCaptain =
            "Essential in the smallest realms: at once lawkeeper, military commander, and guardian of the "
            + "baron and their lands. Later most of these duties pass to a general, border warden, chief "
            + "judge, and others—but until the barony grows into a principality, the Guard Captain alone "
            + "is enough to handle them.";

        public const string Steward =
            "The Steward oversees everything tied to income, construction, provisions, and tax collection.";

        /// <summary>Catalog text for a core office; null for Baron / Custom.</summary>
        public static string? For(string? officeType) => officeType switch
        {
            OfficeType.Chancellor => Loc.T(Chancellor),
            OfficeType.GuardCaptain => Loc.T(GuardCaptain),
            OfficeType.Steward => Loc.T(Steward),
            _ => null,
        };
    }
}
