using DA_Common.Barony;
using DA_Common.Localization;
using Microsoft.Extensions.Localization;

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
            => DisplayName(template, localizer: null);

        public static string DisplayName(this BuildingTemplateDTO template, IStringLocalizer? localizer)
        {
            if (template is null)
                return string.Empty;
            if (template.IsCustom)
                return template.Name;
            return localizer is null
                ? LocCatalog.Name(template.Name)
                : LocCatalog.Name(template.Name, localizer);
        }

        /// <summary>Localized catalog description; custom entries keep user-entered text.</summary>
        public static string DisplayDescription(this BuildingTemplateDTO template, IStringLocalizer? localizer = null)
        {
            if (template is null || string.IsNullOrWhiteSpace(template.Description))
                return string.Empty;
            if (template.IsCustom)
                return template.Description.Trim();
            return Phrase(template.Description, localizer);
        }

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
            => DisplayName(template, localizer: null);

        public static string DisplayName(this SeatPurposeTemplateDTO template, IStringLocalizer? localizer)
        {
            if (template is null)
                return string.Empty;
            return localizer is null
                ? LocCatalog.Name(template.Name)
                : LocCatalog.Name(template.Name, localizer);
        }

        public static string DisplayDescription(this SeatPurposeTemplateDTO template, IStringLocalizer? localizer = null)
            => Phrase(template?.Description, localizer);

        public static string DisplayWhoOccupies(this SeatPurposeTemplateDTO template, IStringLocalizer? localizer = null)
            => Phrase(template?.WhoOccupies, localizer);

        private static string Phrase(string? english, IStringLocalizer? localizer)
        {
            if (string.IsNullOrWhiteSpace(english))
                return string.Empty;
            return localizer is null ? Loc.T(english) : localizer[english].Value;
        }
    }
}
