namespace DA_Business.Services.Wiki;

internal sealed class WikiAccessContext
{
    public bool TreatAsAdmin { get; init; }
    public HashSet<string> UserCharacters { get; init; } = new(StringComparer.OrdinalIgnoreCase);
    public bool IsDukeLoreOnly { get; init; }
}
