using Microsoft.Extensions.Localization;

namespace DA_Common.Localization;

/// <summary>Display-time localization for skill-roll difficulty labels.</summary>
public static class DifficultyDisplay
{
    /// <summary>Resx keys for each canonical <see cref="SD.DifficultyLevel"/> name.</summary>
    public static readonly string[] ResxKeys =
    {
        "Effortless",
        "Simple difficulty",
        "Straightforward",
        "Demanding",
        "Hard",
        "Challanging",
        "Very hard",
        "Nearly impossible",
    };

    private static readonly Dictionary<string, string> ResxKeyByCanonical = new(StringComparer.Ordinal)
    {
        ["Effortless"] = "Effortless",
        ["Simple"] = "Simple difficulty",
        ["Straightforward"] = "Straightforward",
        ["Demanding"] = "Demanding",
        ["Hard"] = "Hard",
        ["Challanging"] = "Challanging",
        ["Very hard"] = "Very hard",
        ["Nearly impossible"] = "Nearly impossible",
    };

    public static string Name(string? canonicalName)
        => Name(canonicalName, localizer: null);

    public static string Name(string? canonicalName, IStringLocalizer? localizer)
    {
        if (string.IsNullOrWhiteSpace(canonicalName))
            return string.Empty;

        var key = ResxKeyByCanonical.TryGetValue(canonicalName.Trim(), out var resxKey)
            ? resxKey
            : canonicalName.Trim();

        return localizer is null ? Loc.T(key) : localizer[key].Value;
    }

    public static string Name(int difficultyValue)
        => Name(SD.GetDifficultyName(difficultyValue));

    public static string Name(int difficultyValue, IStringLocalizer localizer)
        => Name(SD.GetDifficultyName(difficultyValue), localizer);
}
