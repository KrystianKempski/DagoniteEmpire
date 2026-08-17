using DA_Common.Barony;
using DA_Common.Localization;

namespace DA_Models.BaronyModels
{
    /// <summary>
    /// Display-time localization helpers for barony catalog entities (buildings &amp; improvements).
    /// Catalog names are localized via resx; custom/user-entered names are returned unchanged.
    /// </summary>
    public static class BaronyDisplay
    {
        /// <summary>Localized template name (custom templates keep their user-entered name).</summary>
        public static string DisplayName(this BuildingTemplateDTO template)
            => template is null ? string.Empty
             : template.IsCustom ? template.Name
             : LocCatalog.Name(template.Name);

        /// <summary>
        /// Localized building name. Buildings linked to a catalog template or core-city key are
        /// translated; free-standing custom buildings keep their user-entered name.
        /// </summary>
        public static string DisplayName(this BaronyBuildingDTO building)
            => building is null ? string.Empty
             : building.TemplateId.HasValue || !string.IsNullOrEmpty(building.CoreKey)
                 ? LocCatalog.Name(building.Name)
                 : building.Name;

        /// <summary>
        /// Localized improvement name. Template-linked or known map-improvement names are translated;
        /// custom names are returned unchanged.
        /// </summary>
        public static string DisplayName(this TerrainImprovementDTO improvement)
            => improvement is null ? string.Empty
             : improvement.TemplateId.HasValue
                 ? LocCatalog.Name(improvement.Name)
                 : LocCatalog.NameOrRaw(improvement.Name, MapImprovement.All);

        /// <summary>
        /// Localized Lord's Seat purpose template name. Seeded catalog names are translated via resx;
        /// user-entered custom names (absent from resx) are returned unchanged.
        /// </summary>
        public static string DisplayName(this SeatPurposeTemplateDTO template)
            => template is null ? string.Empty : LocCatalog.Name(template.Name);
    }
}
