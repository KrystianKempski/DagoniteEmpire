using System.Linq;
using Abp.Collections.Extensions;
using DA_Common;
using DA_Common.Localization;
using Microsoft.Extensions.Localization;

namespace DA_Models.CharacterModels
{
    /// <summary>
    /// Display-time localization for catalog trait names (weapon parameters, combat states).
    /// Custom player trait names stay unchanged.
    /// </summary>
    public static class TraitDisplay
    {
        public static readonly string[] CatalogNames = States.Names.All
            .Append(SD.WeaponParametersDescr)
            .ToArray();

        public static readonly string[] CatalogDescriptions = States.CatalogDescriptions;

        public static string DisplayName(this TraitDTO trait)
            => LocCatalog.NameOrRaw(trait?.Name, CatalogNames);

        public static string DisplayName(this TraitDTO trait, IStringLocalizer localizer)
            => LocCatalog.NameOrRaw(trait?.Name, CatalogNames, localizer);

        public static string DisplayDescription(this TraitDTO trait)
            => LocCatalog.NameOrRaw(trait?.Descr, CatalogDescriptions);

        public static string DisplayDescription(this TraitDTO trait, IStringLocalizer localizer)
            => LocCatalog.NameOrRaw(trait?.Descr, CatalogDescriptions, localizer);

        public static string DisplaySummaryDescr(this TraitDTO trait)
            => BuildSummaryDescr(trait, localizer: null);

        public static string DisplaySummaryDescr(this TraitDTO trait, IStringLocalizer localizer)
            => BuildSummaryDescr(trait, localizer);

        private static string BuildSummaryDescr(TraitDTO? trait, IStringLocalizer? localizer)
        {
            if (trait is null)
                return string.Empty;

            var res = string.Empty;

            if (trait.Descr.IsNullOrEmpty() == false)
            {
                res = trait.DisplayDescription(localizer);
                if (res.Length > 0)
                    res += ". ";
            }

            if (trait.Bonuses is null || trait.Bonuses.Any() == false)
                return res;

            foreach (var bonus in trait.Bonuses)
            {
                if (bonus.Description != null && bonus.Description.Length > 0)
                {
                    res += bonus.Description + ", ";
                }
                else
                {
                    var val = bonus.BonusValue > 0 ? $"+{bonus.BonusValue}" : $"{bonus.BonusValue}";
                    var featureName = localizer is null
                        ? LocCatalog.Name(bonus.FeatureName)
                        : LocCatalog.Name(bonus.FeatureName, localizer);
                    res += localizer is null
                        ? Loc.T("{0} to {1}", val, featureName) + "; "
                        : localizer["{0} to {1}", val, featureName].Value + "; ";
                }
            }

            return res;
        }
    }
}
