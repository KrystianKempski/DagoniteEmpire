using System.Text.RegularExpressions;
using DA_Common;
using DA_Common.Localization;
using Microsoft.Extensions.Localization;

namespace DA_Models.CharacterModels;

/// <summary>Display-time localization for wound catalog values (location, severity, seeded descriptions).</summary>
public static class WoundDisplay
{
    public const string WoundInflictedDescriptionKey = "Wound inflicted by {0} after {1} attack.";

    private static readonly Dictionary<string, string> SeverityDisplayKeys = new(StringComparer.Ordinal)
    {
        [Wounds.Severity.Scars] = "Scars",
        [Wounds.Severity.Light] = "Light wound",
        [Wounds.Severity.Moderate] = "Moderate wound",
        [Wounds.Severity.Heavy] = "Heavy wound",
        [Wounds.Severity.Critical] = "Critical wound",
        [Wounds.Severity.Deadly] = "Deadly wound",
    };

    private static readonly Regex WoundInflictedPattern = new(
        @"^Wound inflicted by (.+) after (.+) attack\.$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public static string DisplayLocation(this WoundDTO wound)
        => LocCatalog.NameOrRaw(wound?.Location, Wounds.Location.All);

    public static string DisplayLocation(this WoundDTO wound, IStringLocalizer localizer)
        => LocCatalog.NameOrRaw(wound?.Location, Wounds.Location.All, localizer);

    public static string DisplaySeverity(this WoundDTO wound)
        => DisplaySeverity(wound?.Severity, localizer: null);

    public static string DisplaySeverity(this WoundDTO wound, IStringLocalizer localizer)
        => DisplaySeverity(wound?.Severity, localizer);

    public static string DisplayDescription(this WoundDTO wound)
        => DisplayDescription(wound?.Description, localizer: null);

    public static string DisplayDescription(this WoundDTO wound, IStringLocalizer localizer)
        => DisplayDescription(wound?.Description, localizer);

    public static string DisplayDateStart(this WoundDTO wound)
        => CalendarDisplay.Format(wound.DateStart);

    public static string DisplayDateStart(this WoundDTO wound, IStringLocalizer localizer)
        => CalendarDisplay.Format(wound.DateStart, localizer);

    public static string DisplayDateReduce(this WoundDTO wound)
        => CalendarDisplay.Format(wound.DateReduce);

    public static string DisplayDateReduce(this WoundDTO wound, IStringLocalizer localizer)
        => CalendarDisplay.Format(wound.DateReduce, localizer);

    public static string FormatAttributesLabel(IEnumerable<string> attributeNames, IStringLocalizer localizer)
    {
        if (attributeNames is null)
            return string.Empty;

        var labels = attributeNames
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Select(name => LocCatalog.Name(name, localizer))
            .ToArray();

        return labels.Length == 0 ? string.Empty : string.Join(", ", labels);
    }

    public static string GetSeverityDisplayKey(string? severity)
    {
        if (string.IsNullOrWhiteSpace(severity))
            return string.Empty;

        return SeverityDisplayKeys.TryGetValue(severity.Trim(), out var key) ? key : severity.Trim();
    }

    private static string DisplaySeverity(string? severity, IStringLocalizer? localizer)
    {
        if (string.IsNullOrWhiteSpace(severity))
            return string.Empty;

        if (!SeverityDisplayKeys.TryGetValue(severity.Trim(), out var key))
            return severity;

        return localizer is null ? Loc.T(key) : localizer[key].Value;
    }

    private static string DisplayDescription(string? description, IStringLocalizer? localizer)
    {
        if (string.IsNullOrWhiteSpace(description))
            return string.Empty;

        var trimmed = description.Trim();
        var match = WoundInflictedPattern.Match(trimmed);
        if (!match.Success)
            return trimmed;

        var attacker = match.Groups[1].Value.Trim();
        var attackType = SD.AttackAction.Canonical(match.Groups[2].Value.Trim());
        var localizedAttack = localizer is null
            ? LocCatalog.Name(attackType)
            : LocCatalog.Name(attackType, localizer);

        return localizer is null
            ? Loc.T(WoundInflictedDescriptionKey, attacker, localizedAttack)
            : localizer[WoundInflictedDescriptionKey, attacker, localizedAttack].Value;
    }
}
