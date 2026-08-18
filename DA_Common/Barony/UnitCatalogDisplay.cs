using DA_Common.Localization;
using Microsoft.Extensions.Localization;

namespace DA_Common.Barony
{
    /// <summary>
    /// Display-time localization for army unit catalogs (recruit, training, skills, race, attributes).
    /// Stored keys stay English; translation happens only when rendering.
    /// </summary>
    public static class UnitCatalogDisplay
    {
        public static string DisplayName(this UnitRecruitSelection recruit, IStringLocalizer localizer)
            => LocCatalog.Name(recruit.Name, localizer);

        public static string DisplayName(this UnitTrainingType training, IStringLocalizer localizer)
            => LocCatalog.Name(training.Name, localizer);

        public static string DisplayName(this UnitSkillDef skill, IStringLocalizer localizer)
            => LocCatalog.Name(skill.Name, localizer);

        public static string DisplayName(this UnitRaceDef race, IStringLocalizer localizer)
            => LocCatalog.Name(race.Name, localizer);

        public static string DisplayNotes(this UnitRecruitSelection recruit, IStringLocalizer localizer)
            => string.IsNullOrWhiteSpace(recruit.Notes)
                ? string.Empty
                : localizer[recruit.Notes].Value;

        public static string DisplayNotes(this UnitTrainingType training, IStringLocalizer localizer)
            => string.IsNullOrWhiteSpace(training.Notes)
                ? string.Empty
                : localizer[training.Notes].Value;

        public static string DisplayDescription(this UnitRaceDef race, IStringLocalizer localizer)
            => string.IsNullOrWhiteSpace(race.Description)
                ? string.Empty
                : localizer[race.Description].Value;

        public static string AttrLabel(string attrKey, IStringLocalizer localizer)
            => LocCatalog.Name(UnitAttr.Label(attrKey), localizer);

        /// <summary>Localize catalog Other-source labels (Race, Other); leave player-entered names unchanged.</summary>
        public static string ModifierLabel(string? label, IStringLocalizer localizer)
        {
            var trimmed = label?.Trim() ?? string.Empty;
            if (trimmed.Length == 0)
                return localizer["(unnamed)"].Value;
            if (string.Equals(trimmed, UnitRaceSkillBonus.OtherLabel, StringComparison.OrdinalIgnoreCase)
                || string.Equals(trimmed, "Other", StringComparison.OrdinalIgnoreCase))
                return localizer[trimmed].Value;
            return trimmed;
        }
    }
}
