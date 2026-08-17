using DA_Common.Localization;
using Microsoft.Extensions.Localization;

namespace DA_Models.CharacterModels
{
    /// <summary>
    /// Display-time localization helpers for character features (attributes, base &amp; special skills).
    /// The stored <see cref="FeatureDTO.Name"/> stays the canonical English key; use these when rendering.
    /// </summary>
    public static class FeatureDisplay
    {
        /// <summary>Localized name for a feature (attribute/skill). Falls back to the stored English key.</summary>
        public static string DisplayName(this FeatureDTO feature)
            => LocCatalog.Name(feature?.Name);

        /// <summary>Localized name using the page localizer (same culture as <c>L[]</c> in Razor).</summary>
        public static string DisplayName(this FeatureDTO feature, IStringLocalizer localizer)
            => LocCatalog.Name(feature?.Name, localizer);

        /// <summary>Sheet labels: localized name uppercased with the current culture.</summary>
        public static string DisplayNameUpper(this FeatureDTO feature, IStringLocalizer localizer)
            => LocCatalog.NameUpper(feature?.Name, localizer);
    }
}
