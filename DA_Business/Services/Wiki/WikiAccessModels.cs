namespace DA_Business.Services.Wiki;

public sealed class WikiPartiesConfig
{
    public int PartySceneMinPlayers { get; set; } = 3;
    public List<string> AnonymousPublicPrefixes { get; set; } = [];
    public List<string> LoggedInPublicPrefixes { get; set; } = [];
    public Dictionary<string, WikiPartyDefinition> Parties { get; set; } = new();
    public List<WikiCampaignDefinition> Campaigns { get; set; } = [];
    /// <summary>Canonical character name -> alternate spellings used in wiki content.</summary>
    public Dictionary<string, List<string>> CharacterAliases { get; set; } = new();
    /// <summary>DukePlayer without a mapped hero only sees lore + these campaign slug prefixes.</summary>
    public List<string> DukeAccessibleCampaignIds { get; set; } = [];
}

public sealed class WikiLinksFile
{
    public int Version { get; set; } = 1;
    public Dictionary<string, string> Characters { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, string> Campaigns { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, string> Chapters { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public List<string> AllPlayerNames { get; set; } = [];
}

public sealed class WikiPartyDefinition
{
    public string DisplayName { get; set; } = string.Empty;
    public List<string> Aliases { get; set; } = [];
    public List<string> Characters { get; set; } = [];
}

public sealed class WikiCampaignDefinition
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string ContentFolder { get; set; } = string.Empty;
    public List<string> PartyIds { get; set; } = [];
}

public enum WikiAccessMode
{
    Anonymous,
    Authenticated,
    Characters,
    Party,
    Deny
}

public sealed class WikiAccessEntry
{
    public WikiAccessMode Mode { get; set; }

    /// <summary>Explicitly listed characters with access (visibility: characters).</summary>
    public List<string> Characters { get; set; } = [];

    /// <summary>Party ids whose members get access (visibility: party). Expanded to characters at runtime.</summary>
    public List<string> Parties { get; set; } = [];

    public string? Reason { get; set; }
}

public sealed class WikiAccessManifest
{
    public int Version { get; set; } = 1;
    public Dictionary<string, WikiAccessEntry> Slugs { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public List<string> AnonymousPrefixes { get; set; } = [];
    public List<string> LoggedInPublicPrefixes { get; set; } = [];
    public List<string> CampaignIds { get; set; } = [];
    public List<string> DukeAccessibleCampaignIds { get; set; } = [];
    public HashSet<string> AllPartyCharacters { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Party id -> member character names. Used to expand <c>Party</c> rules at runtime.</summary>
    public Dictionary<string, List<string>> Parties { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Normalized alias/name -> canonical character name, for resolving content/app name drift.</summary>
    public Dictionary<string, string> CharacterCanonical { get; set; } = new(StringComparer.Ordinal);
}
