namespace DA_Business.Services.Wiki;

internal sealed class WikiAccessContext
{
    public bool TreatAsAdmin { get; init; }
    public HashSet<string> UserCharacters { get; init; } = new(StringComparer.OrdinalIgnoreCase);
    /// <summary>Selected hero appears in wiki-parties (any campaign party).</summary>
    public bool IsCampaignParticipant { get; init; }
    public bool IsDukeLoreOnly { get; init; }
}
