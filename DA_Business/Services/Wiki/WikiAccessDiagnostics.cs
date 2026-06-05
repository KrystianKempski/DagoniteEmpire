namespace DA_Business.Services.Wiki;

/// <summary>Snapshot for /wiki/debug/access (development only).</summary>
public sealed class WikiAccessDiagnostics
{
    public string? IdentityName { get; init; }
    public string? ResolvedOwnerUserName { get; init; }
    public int? CookieCharacterId { get; init; }
    public int? DatabaseSelectedCharacterId { get; init; }
    public int? BlazorSelectedCharacterId { get; init; }
    public string? SelectedNpcName { get; init; }
    /// <summary>Union of NPC names from all <c>IsApproved</c> characters (wiki ACL identity).</summary>
    public IReadOnlyList<string> UserCharacters { get; init; } = [];
    public bool IsCampaignParticipant { get; init; }
    public bool TreatAsAdmin { get; init; }
    public bool UserInfoFromBlazorSession { get; init; }
    public IReadOnlyDictionary<string, bool> SampleSlugAccess { get; init; } = new Dictionary<string, bool>();
}
