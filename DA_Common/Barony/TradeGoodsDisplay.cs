using System.Globalization;
using DA_Common.Localization;
using Microsoft.Extensions.Localization;

namespace DA_Common.Barony
{
    /// <summary>
    /// Display-time localization for trade goods and luxury-access copy.
    /// Catalog fields stay English; translation happens only when rendering.
    /// </summary>
    public static class TradeGoodsDisplay
    {
        public static string DisplayName(this TradeGoodEntry entry, IStringLocalizer? localizer = null)
            => Phrase(entry.Name, localizer);

        public static string DisplayName(string? key, IStringLocalizer? localizer = null)
        {
            var entry = TradeGoodsCatalog.Find(key);
            if (entry is null)
                return Phrase(key ?? string.Empty, localizer);
            return Phrase(entry.Name, localizer);
        }

        public static string DisplayDescription(this TradeGoodEntry entry, IStringLocalizer? localizer = null)
            => Phrase(entry.Description, localizer);

        public static string DisplayBonus(this TradeGoodEntry entry, IStringLocalizer? localizer = null)
            => DisplayBonus(entry.BonusDisplay, localizer);

        public static string DisplayBonus(string? bonusDisplay, IStringLocalizer? localizer = null)
        {
            if (string.IsNullOrWhiteSpace(bonusDisplay) || bonusDisplay == "—")
                return bonusDisplay ?? "—";

            var s = bonusDisplay;
            foreach (var info in PpbCatalog.All.OrderByDescending(i => i.NameEn.Length))
                s = s.Replace(info.NameEn, info.Name, StringComparison.Ordinal);
            return s;
        }

        public static string DisplayUnlocks(this TradeGoodEntry entry, IStringLocalizer? localizer = null)
            => DisplayList(entry.Unlocks, localizer);

        public static string DisplayProductionBuilding(this TradeGoodEntry entry, IStringLocalizer? localizer = null)
            => Phrase(entry.ProductionBuilding, localizer);

        public static string DisplayRequirements(this TradeGoodEntry entry, IStringLocalizer? localizer = null)
            => DisplayRequirements(entry.Requirements, localizer);

        public static string DisplayRequirements(string? requirements, IStringLocalizer? localizer = null)
        {
            if (string.IsNullOrWhiteSpace(requirements))
                return string.Empty;

            if (requirements.Contains(" / ", StringComparison.Ordinal))
            {
                return string.Join(" / ", requirements
                    .Split(" / ", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                    .Select(p => DisplayRequirements(p, localizer)));
            }

            if (requirements.Contains(" + ", StringComparison.Ordinal))
            {
                return string.Join(" + ", requirements
                    .Split(" + ", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                    .Select(p => DisplayRequirements(p, localizer)));
            }

            if (LooksLikeCatalogList(requirements))
            {
                return string.Join(", ", requirements
                    .Split(", ", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                    .Select(p => DisplayRequirements(p, localizer)));
            }

            const string depositSuffix = " deposit";
            if (requirements.EndsWith(depositSuffix, StringComparison.OrdinalIgnoreCase))
            {
                var raw = requirements[..^depositSuffix.Length].Trim();
                var resource = ResolvePhraseKey(raw);
                var resourceLabel = Phrase(resource, localizer);
                var template = Phrase("{0} deposit", localizer);
                try
                {
                    return string.Format(CultureInfo.CurrentCulture, template, resourceLabel);
                }
                catch (FormatException)
                {
                    return template;
                }
            }

            return Phrase(ResolvePhraseKey(requirements), localizer);
        }

        public static string DisplayDescription(this LuxuryGoodsAccessTier tier, IStringLocalizer? localizer = null)
            => Phrase(tier.Description, localizer);

        public static string DisplayBonus(this LuxuryGoodsAccessTier tier, IStringLocalizer? localizer = null)
            => DisplayBonus(tier.BonusDisplay, localizer);

        public static string DisplayName(this LuxuryGoodsAccessTier tier, IStringLocalizer? localizer = null)
            => Phrase(tier.NameEn, localizer);

        /// <summary>English resx keys this helper looks up (for coverage tests).</summary>
        public static IEnumerable<string> CoverageKeys()
        {
            foreach (var g in TradeGoodsCatalog.All)
            {
                yield return g.Name;
                if (!string.IsNullOrWhiteSpace(g.Description))
                    yield return g.Description;
                foreach (var part in SplitList(g.Unlocks))
                    yield return part;
                if (!string.IsNullOrWhiteSpace(g.ProductionBuilding))
                    yield return g.ProductionBuilding;
                foreach (var key in RequirementLookupKeys(g.Requirements))
                    yield return key;
            }

            foreach (var tier in LuxuryGoodsAccessCatalog.All)
            {
                yield return tier.NameEn;
                if (!string.IsNullOrWhiteSpace(tier.Description))
                    yield return tier.Description;
            }
        }

        private static IEnumerable<string> RequirementLookupKeys(string? requirements)
        {
            if (string.IsNullOrWhiteSpace(requirements))
                yield break;

            if (requirements.Contains(" / ", StringComparison.Ordinal))
            {
                foreach (var part in requirements.Split(" / ", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
                {
                    foreach (var key in RequirementLookupKeys(part))
                        yield return key;
                }
                yield break;
            }

            if (requirements.Contains(" + ", StringComparison.Ordinal))
            {
                foreach (var part in requirements.Split(" + ", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
                {
                    foreach (var key in RequirementLookupKeys(part))
                        yield return key;
                }
                yield break;
            }

            if (LooksLikeCatalogList(requirements))
            {
                foreach (var part in requirements.Split(", ", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
                {
                    foreach (var key in RequirementLookupKeys(part))
                        yield return key;
                }
                yield break;
            }

            const string depositSuffix = " deposit";
            if (requirements.EndsWith(depositSuffix, StringComparison.OrdinalIgnoreCase))
            {
                yield return "{0} deposit";
                yield return ResolvePhraseKey(requirements[..^depositSuffix.Length].Trim());
                yield break;
            }

            yield return ResolvePhraseKey(requirements);
        }

        private static bool LooksLikeCatalogList(string requirements)
        {
            if (!requirements.Contains(", ", StringComparison.Ordinal))
                return false;
            var parts = requirements.Split(", ", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            return parts.Length >= 2 && parts.All(IsKnownCatalogPhrase);
        }

        private static bool IsKnownCatalogPhrase(string english)
        {
            if (TradeGoodsCatalog.All.Any(g => string.Equals(g.Name, english, StringComparison.OrdinalIgnoreCase)))
                return true;
            var resource = TerrainResource.Canonical(english);
            if (TerrainResource.All.Contains(resource))
                return true;
            var feature = TerrainFeature.Canonical(english);
            if (TerrainFeature.AllNames.Contains(feature))
                return true;
            return string.Equals(english, "town", StringComparison.OrdinalIgnoreCase);
        }

        private static string ResolvePhraseKey(string english)
        {
            var good = TradeGoodsCatalog.All.FirstOrDefault(g =>
                string.Equals(g.Name, english, StringComparison.OrdinalIgnoreCase));
            if (good is not null)
                return good.Name;

            var resource = TerrainResource.Canonical(english);
            if (TerrainResource.All.Contains(resource))
                return resource;

            var feature = TerrainFeature.Canonical(english);
            if (TerrainFeature.AllNames.Contains(feature))
                return feature;

            if (string.Equals(english, "town", StringComparison.OrdinalIgnoreCase))
                return "Town";

            return english;
        }

        private static IEnumerable<string> SplitList(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                yield break;
            foreach (var part in value.Split(", ", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
                yield return part;
        }

        private static string DisplayList(string? value, IStringLocalizer? localizer)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;
            return string.Join(", ", SplitList(value).Select(p => Phrase(p, localizer)));
        }

        private static string Phrase(string? english, IStringLocalizer? localizer)
        {
            if (string.IsNullOrWhiteSpace(english))
                return string.Empty;
            return localizer is null ? Loc.T(english) : localizer[english].Value;
        }
    }
}
