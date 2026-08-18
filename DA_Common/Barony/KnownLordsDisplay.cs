using DA_Common.Localization;
using Microsoft.Extensions.Localization;

namespace DA_Common.Barony
{
    /// <summary>
    /// Display-time localization for known-lord titles and catalog biographies.
    /// Stored catalog strings stay English; translation happens only when rendering.
    /// </summary>
    public static class KnownLordsDisplay
    {
        /// <summary>
        /// Resx key for the noble rank "Count". The bare key "Count" is already
        /// "Ilość" (quantity) in the shared Polish file.
        /// </summary>
        public const string CountTitleKey = "Count (title)";

        public static string TitleResxKey(string? title)
        {
            var trimmed = title?.Trim() ?? string.Empty;
            if (string.Equals(trimmed, "Count", StringComparison.OrdinalIgnoreCase))
                return CountTitleKey;
            return trimmed;
        }

        public static string DisplayTitle(this KnownLordEntry lord, IStringLocalizer? localizer = null)
            => Phrase(TitleResxKey(lord.Title), localizer);

        public static string DisplayDescription(this KnownLordEntry lord, IStringLocalizer? localizer = null)
            => Phrase(lord.Description, localizer);

        public static string DisplayFullName(this KnownLordEntry lord, IStringLocalizer? localizer = null)
        {
            var title = lord.DisplayTitle(localizer);
            return $"{lord.Name} {lord.House} — {title} ({lord.Holdings})";
        }

        public static IEnumerable<string> CoverageKeys()
        {
            yield return KnownLordsCatalog.RegionEasternMarch;
            foreach (var title in KnownLordsCatalog.EasternMarch
                         .Select(l => TitleResxKey(l.Title))
                         .Where(t => t.Length > 0)
                         .Distinct(StringComparer.Ordinal))
                yield return title;
            foreach (var desc in KnownLordsCatalog.EasternMarch
                         .Select(l => l.Description)
                         .Where(d => !string.IsNullOrWhiteSpace(d))
                         .Distinct(StringComparer.Ordinal))
                yield return desc;
        }

        private static string Phrase(string? english, IStringLocalizer? localizer)
        {
            if (string.IsNullOrWhiteSpace(english))
                return string.Empty;
            return localizer is null ? Loc.T(english) : localizer[english].Value;
        }
    }
}
