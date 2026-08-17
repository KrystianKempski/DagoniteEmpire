using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DA_Common;
using Microsoft.Extensions.Localization;

namespace DA_Common.Localization;

/// <summary>
/// Display-time localization for canonical domain values that are stored in the database or
/// defined in code catalogs (attributes, skills, building/improvement names, offices, …).
/// <para>
/// The stored value stays the stable English key; translation happens only when a value is
/// rendered. Free-text entered by users (custom names, person names) is returned unchanged.
/// Legacy Polish attribute and skill names are accepted as aliases of the English keys.
/// </para>
/// </summary>
public static class LocCatalog
{
    /// <summary>
    /// Maps a stored catalog value to its canonical English key when it is a known alias
    /// (e.g. <c>Inteligencja</c> → <c>Intelligence</c>). Other values are returned trimmed.
    /// </summary>
    public static string CanonicalKey(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        var trimmed = value.Trim();

        var attribute = SD.Attributes.Canonical(trimmed);
        if (SD.Attributes.All.Contains(attribute))
            return attribute;

        var baseSkill = SD.BaseSkills.Canonical(trimmed);
        if (SD.BaseSkills.All.Contains(baseSkill))
            return baseSkill;

        var specialSkill = SD.SpecialSkills.Canonical(trimmed);
        if (SD.SpecialSkills.All.Contains(specialSkill))
            return specialSkill;

        return trimmed;
    }

    /// <summary>Localized display text for a canonical catalog key (falls back to the key/English).</summary>
    public static string Name(string? key)
    {
        var canonical = CanonicalKey(key);
        return canonical.Length == 0 ? string.Empty : Loc.T(canonical);
    }

    /// <summary>
    /// Same as <see cref="Name(string?)"/> but uses the caller's localizer (Razor <c>L[]</c>)
    /// so circuit culture matches the rest of the page.
    /// </summary>
    public static string Name(string? key, IStringLocalizer localizer)
    {
        var canonical = CanonicalKey(key);
        if (canonical.Length == 0)
            return string.Empty;
        return localizer[canonical].Value;
    }

    /// <summary>Localized catalog name in the current culture's uppercase (character-sheet labels).</summary>
    public static string NameUpper(string? key, IStringLocalizer localizer)
    {
        var name = Name(key, localizer);
        return name.Length == 0 ? name : name.ToUpper(CultureInfo.CurrentCulture);
    }

    /// <summary>
    /// Localizes <paramref name="value"/> only when it is a known catalog key (or a known alias);
    /// otherwise returns the raw value unchanged (used where a field may hold either a catalog key
    /// or free-text user input).
    /// </summary>
    public static string NameOrRaw(string? value, IReadOnlyCollection<string> knownKeys)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        var canonical = CanonicalKey(value);
        if (knownKeys.Contains(canonical) || knownKeys.Contains(value.Trim()))
            return Loc.T(canonical);

        return value;
    }
}
