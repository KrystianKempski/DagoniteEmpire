namespace DagoniteEmpire.Pages.Barony;

/// <summary>
/// Barony pages that are usable below the desktop breakpoint
/// (<c>max-width: 1279.98px</c> in <c>barony.css</c>). Expand this list as more tabs get mobile layouts.
/// </summary>
public static class BaronyMobile
{
    /// <summary>Matches <c>.barony-mobile-block</c> / mobile gate in barony.css.</summary>
    public const double MaxWidthPx = 1280;

    public static readonly IReadOnlyList<string> AllowedTabKeys =
    [
        "audience-hall",
        "projects",
        "character-card",
        "notes",
        "letters",
    ];

    public static bool IsAllowedTab(string tabKey) =>
        AllowedTabKeys.Contains(tabKey, StringComparer.OrdinalIgnoreCase);

    public static bool IsAllowedPath(string path) => path switch
    {
        "/barony/audience-hall" => true,
        "/barony/projects" => true,
        "/barony/character-card" => true,
        "/barony/notes" => true,
        "/barony/letters" => true,
        _ => false,
    };

    /// <summary>Fallback when a blocked Barony URL is opened on a narrow screen.</summary>
    public const string FallbackHref = "/barony/audience-hall";
}
