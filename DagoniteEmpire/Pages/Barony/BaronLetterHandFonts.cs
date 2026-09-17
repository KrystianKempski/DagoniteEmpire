namespace DagoniteEmpire.Pages.Barony;

/// <summary>Handwriting faces for Barony letters (composer + stored marker in BodyHtml).</summary>
public static class BaronLetterHandFonts
{
    public const string MarkerPrefix = "<!--bh:";
    public const string MarkerSuffix = "-->";

    public const string DefaultBaronKey = "caveat";
    public const string DefaultCorrespondentKey = "pinyon";

    public const string LocalStorageBaronKey = "barony.letterHand.baron";
    public const string LocalStorageCorrespondentKey = "barony.letterHand.correspondent";

    public sealed record HandFont(string Key, string Family, string Label);

    public static readonly HandFont[] All =
    [
        new("caveat", "Caveat", "Caveat"),
        new("homemade-apple", "Homemade Apple", "Homemade Apple"),
        new("shadows-into-light", "Shadows Into Light", "Shadows Into Light"),
        new("dancing-script", "Dancing Script", "Dancing Script"),
        new("sacramento", "Sacramento", "Sacramento"),
        new("la-belle-aurore", "La Belle Aurore", "La Belle Aurore"),
        new("great-vibes", "Great Vibes", "Great Vibes"),
        new("pinyon", "Pinyon Script", "Pinyon Script"),
    ];

    public static HandFont Get(string? key) =>
        All.FirstOrDefault(f => string.Equals(f.Key, key, StringComparison.OrdinalIgnoreCase))
        ?? All[0];

    public static bool IsKnown(string? key) =>
        !string.IsNullOrWhiteSpace(key)
        && All.Any(f => string.Equals(f.Key, key, StringComparison.OrdinalIgnoreCase));

    public static string CssClass(string? key) => $"baron-mail__hand-font--{Get(key).Key}";

    public static (string Key, string BodyHtml) Split(string? html)
    {
        var body = html ?? string.Empty;
        if (!body.StartsWith(MarkerPrefix, StringComparison.Ordinal))
            return (string.Empty, body);

        var end = body.IndexOf(MarkerSuffix, MarkerPrefix.Length, StringComparison.Ordinal);
        if (end < 0)
            return (string.Empty, body);

        var key = body[MarkerPrefix.Length..end].Trim();
        var rest = body[(end + MarkerSuffix.Length)..];
        if (rest.StartsWith('\n'))
            rest = rest[1..];

        return (IsKnown(key) ? key : string.Empty, rest);
    }

    public static string Embed(string? key, string? bodyHtml)
    {
        var (_, body) = Split(bodyHtml);
        var font = Get(string.IsNullOrWhiteSpace(key) ? DefaultBaronKey : key);
        return $"{MarkerPrefix}{font.Key}{MarkerSuffix}\n{body}";
    }

    public static string Strip(string? html) => Split(html).BodyHtml;
}
