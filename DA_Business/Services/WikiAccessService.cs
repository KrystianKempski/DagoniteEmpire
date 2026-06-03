using System.Text;
using System.Text.Json;
using System.Xml.Linq;
using DA_Business.Repository.CharacterReps.IRepository;
using DA_Business.Services.Interfaces;
using DA_Business.Services.Wiki;
using DA_Common;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace DA_Business.Services;

public class WikiAccessService : IWikiAccessService
{
    private const string ManifestCacheKey = "wiki-access-manifest-v6";
    private const string DefaultLoreIframePath = "/wiki/świat-i-zasady/";
    private const string CampaignHubIframePath = "/wiki/index.html";

    private readonly ICharacterRepository _characters;
    private readonly IWebHostEnvironment _environment;
    private readonly IMemoryCache _cache;
    private readonly ILogger<WikiAccessService> _logger;
    private readonly IUserService _userService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    public WikiAccessService(
        ICharacterRepository characters,
        IWebHostEnvironment environment,
        IMemoryCache cache,
        ILogger<WikiAccessService> logger,
        IUserService userService,
        IHttpContextAccessor httpContextAccessor)
    {
        _characters = characters;
        _environment = environment;
        _cache = cache;
        _logger = logger;
        _userService = userService;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<bool> CanAccessAllWiki(string? userName, bool isAdminOrMg)
    {
        var context = await BuildContextAsync(userName, isAdminOrMg);
        if (context.TreatAsAdmin)
        {
            return true;
        }

        return !string.IsNullOrWhiteSpace(userName);
    }

    public async Task<bool> CanAccessSlug(string? userName, bool isAdminOrMg, string slug)
    {
        var context = await BuildContextAsync(userName, isAdminOrMg);
        var manifest = LoadManifest();
        if (manifest is null)
        {
            return context.TreatAsAdmin || IsPrefixMatch(NormalizeSlug(slug), ["świat-i-zasady"]);
        }

        return ResolveSlugAccess(userName, context, slug, manifest);
    }

    public async Task<string?> FilterContentIndexAsync(string? userName, bool isAdminOrMg, string json)
    {
        var context = await BuildContextAsync(userName, isAdminOrMg);
        if (context.TreatAsAdmin)
        {
            return json;
        }

        var manifest = LoadManifest();
        if (manifest is null)
        {
            return json;
        }

        try
        {
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.ValueKind != JsonValueKind.Object)
            {
                return json;
            }

            var filtered = new Dictionary<string, JsonElement>(StringComparer.OrdinalIgnoreCase);
            foreach (var prop in doc.RootElement.EnumerateObject())
            {
                if (ResolveSlugAccess(userName, context, prop.Name, manifest))
                {
                    filtered[prop.Name] = prop.Value.Clone();
                }
            }

            return JsonSerializer.Serialize(filtered, JsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to filter wiki content index");
            return json;
        }
    }

    public async Task<string> FilterEncryptedContentIndexAsync(string? userName, bool isAdminOrMg, string json)
    {
        var context = await BuildContextAsync(userName, isAdminOrMg);
        if (context.TreatAsAdmin)
        {
            return json;
        }

        try
        {
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("version", out var version))
            {
                var empty = new Dictionary<string, object>
                {
                    ["version"] = version.ValueKind == JsonValueKind.Number
                        ? version.GetInt32()
                        : 1,
                    ["entries"] = Array.Empty<object>(),
                };
                return JsonSerializer.Serialize(empty, JsonOptions);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to filter encrypted wiki content index");
        }

        return """{"version":1,"entries":[]}""";
    }

    public async Task<string?> FilterSitemapAsync(string? userName, bool isAdminOrMg, string xml)
    {
        var context = await BuildContextAsync(userName, isAdminOrMg);
        if (context.TreatAsAdmin)
        {
            return xml;
        }

        var manifest = LoadManifest();
        if (manifest is null)
        {
            return xml;
        }

        try
        {
            var doc = XDocument.Parse(xml);
            var ns = doc.Root?.Name.Namespace ?? XNamespace.None;
            var urlElements = doc.Descendants(ns + "url").ToList();
            foreach (var url in urlElements)
            {
                var loc = url.Element(ns + "loc")?.Value;
                if (string.IsNullOrWhiteSpace(loc))
                {
                    continue;
                }

                var slug = ExtractSlugFromSitemapLoc(loc);
                if (!ResolveSlugAccess(userName, context, slug, manifest))
                {
                    url.Remove();
                }
            }

            using var writer = new Utf8StringWriter();
            doc.Save(writer);
            return writer.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to filter wiki sitemap");
            return null;
        }
    }

    public async Task<string> GetDefaultWikiIframePathAsync(string? userName, bool isAdminOrMg)
    {
        if (await ShouldBypassAccessChecksAsync(userName, isAdminOrMg)
            || await CanAccessSlug(userName, isAdminOrMg, "index"))
        {
            return CampaignHubIframePath;
        }

        return DefaultLoreIframePath;
    }

    public async Task<bool> ShouldBypassAccessChecksAsync(string? userName, bool isAdminOrMg)
    {
        var context = await BuildContextAsync(userName, isAdminOrMg);
        return context.TreatAsAdmin;
    }

    public bool IsAnonymousPublicPath(string slug)
    {
        var manifest = LoadManifest();
        slug = NormalizeSlug(slug);

        if (manifest is null)
        {
            return slug.StartsWith("świat-i-zasady", StringComparison.OrdinalIgnoreCase);
        }

        if (manifest.Slugs.TryGetValue(slug, out var entry) && entry.Mode == WikiAccessMode.Anonymous)
        {
            return true;
        }

        return IsPrefixMatch(slug, manifest.AnonymousPrefixes);
    }

    private bool ResolveSlugAccess(string? userName, WikiAccessContext context, string slug, WikiAccessManifest manifest)
    {
        if (context.TreatAsAdmin)
        {
            return true;
        }

        slug = NormalizeSlug(slug);

        if (IsPrefixMatch(slug, manifest.AnonymousPrefixes))
        {
            return true;
        }

        if (string.IsNullOrWhiteSpace(userName))
        {
            return false;
        }

        // Index, maps, etc. — only for characters tied to a campaign party (explorer + pages).
        if (context.IsCampaignParticipant && IsPrefixMatch(slug, manifest.LoggedInPublicPrefixes))
        {
            return true;
        }

        if (context.IsDukeLoreOnly)
        {
            return AllowsDukePlayerSlug(slug, manifest);
        }

        if (IsUnderCampaign(slug, manifest) && !context.IsCampaignParticipant)
        {
            return false;
        }

        if (!TryGetManifestEntry(slug, manifest, out var entry))
        {
            return false;
        }

        return EvaluateEntry(entry, userName, context, slug, manifest);
    }

    private static bool EvaluateEntry(
        WikiAccessEntry entry,
        string? userName,
        WikiAccessContext context,
        string slug,
        WikiAccessManifest manifest)
    {
        return entry.Mode switch
        {
            WikiAccessMode.Anonymous => true,
            WikiAccessMode.Deny => false,
            WikiAccessMode.Authenticated => IsPrefixMatch(slug, manifest.LoggedInPublicPrefixes)
                    || IsUnderCampaign(slug, manifest)
                ? context.IsCampaignParticipant
                : !string.IsNullOrWhiteSpace(userName),
            WikiAccessMode.Characters or WikiAccessMode.Party =>
                HasCharacterAccess(entry, context, manifest),
            _ => false,
        };
    }

    /// <summary>
    /// Allowed characters = explicitly listed characters ∪ members of referenced parties
    /// (expanded from the manifest at runtime, so party membership lives in one place).
    /// </summary>
    private static bool HasCharacterAccess(
        WikiAccessEntry entry,
        WikiAccessContext context,
        WikiAccessManifest manifest)
    {
        if (context.UserCharacters.Count == 0)
        {
            return false;
        }

        if (entry.Characters.Any(c =>
                context.UserCharacters.Contains(c, StringComparer.OrdinalIgnoreCase)))
        {
            return true;
        }

        foreach (var partyId in entry.Parties)
        {
            if (manifest.Parties.TryGetValue(partyId, out var members)
                && members.Any(m => context.UserCharacters.Contains(m, StringComparer.OrdinalIgnoreCase)))
            {
                return true;
            }
        }

        return false;
    }

    private static bool TryGetManifestEntry(string slug, WikiAccessManifest manifest, out WikiAccessEntry entry)
    {
        if (manifest.Slugs.TryGetValue(slug, out entry!))
        {
            return true;
        }

        if (slug.EndsWith("/index", StringComparison.OrdinalIgnoreCase))
        {
            var parent = slug[..^6].TrimEnd('/');
            if (manifest.Slugs.TryGetValue(parent, out entry!))
            {
                return true;
            }
        }

        entry = null!;
        return false;
    }

    private async Task<WikiAccessContext> BuildContextAsync(string? userName, bool isAdminOrMg)
    {
        var manifest = LoadManifest();
        var userInfo = await _userService.GetUserInfo();

        // MG/Admin always see everything — even when a player character is selected in the UI.
        if (userInfo?.CharacterMG == true || isAdminOrMg)
        {
            return new WikiAccessContext { TreatAsAdmin = true };
        }

        var selectedNpc = userInfo?.SelectedCharacter?.NPCName;
        if (!string.IsNullOrWhiteSpace(selectedNpc)
            && !string.Equals(selectedNpc, SD.GameMaster_NPCName, StringComparison.OrdinalIgnoreCase))
        {
            var selected = Canonicalize([selectedNpc], manifest);
            return new WikiAccessContext
            {
                UserCharacters = selected,
                IsCampaignParticipant = IsCampaignParticipant(selected, manifest),
                IsDukeLoreOnly = IsDukeLoreOnlyUser(selected),
            };
        }

        var chars = Canonicalize(await GetUserCharacterNames(userName), manifest);
        return new WikiAccessContext
        {
            UserCharacters = chars,
            IsCampaignParticipant = IsCampaignParticipant(chars, manifest),
            IsDukeLoreOnly = IsDukeLoreOnlyUser(chars),
        };
    }

    private static bool IsCampaignParticipant(HashSet<string> userCharacters, WikiAccessManifest? manifest)
    {
        if (manifest is null || manifest.AllPartyCharacters.Count == 0)
        {
            return false;
        }

        return userCharacters.Any(c => manifest.AllPartyCharacters.Contains(c, StringComparer.OrdinalIgnoreCase));
    }

    private bool IsDukeLoreOnlyUser(HashSet<string> userCharacters)
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user is null || !user.IsInRole(SD.Role_DukePlayer))
        {
            return false;
        }

        if (user.IsInRole(SD.Role_Admin) || user.IsInRole(SD.Role_GameMaster))
        {
            return false;
        }

        var manifest = LoadManifest();
        if (manifest is null || manifest.AllPartyCharacters.Count == 0)
        {
            return false;
        }

        return !IsCampaignParticipant(userCharacters, manifest);
    }

    private static bool AllowsDukePlayerSlug(string slug, WikiAccessManifest manifest)
    {
        if (IsPrefixMatch(slug, manifest.AnonymousPrefixes))
        {
            return true;
        }

        return manifest.DukeAccessibleCampaignIds.Any(id =>
            slug.Equals(id, StringComparison.OrdinalIgnoreCase)
            || slug.StartsWith(id + "/", StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsUnderCampaign(string slug, WikiAccessManifest manifest) =>
        manifest.CampaignIds.Any(id =>
            slug.Equals(id, StringComparison.OrdinalIgnoreCase)
            || slug.StartsWith(id + "/", StringComparison.OrdinalIgnoreCase));

    private static bool IsPrefixMatch(string slug, IEnumerable<string> prefixes) =>
        prefixes.Any(p =>
            slug.Equals(p, StringComparison.OrdinalIgnoreCase)
            || slug.StartsWith(p + "/", StringComparison.OrdinalIgnoreCase));

    private async Task<HashSet<string>> GetUserCharacterNames(string? userName)
    {
        if (string.IsNullOrWhiteSpace(userName))
        {
            return new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        var chars = await _characters.GetAllForUser(userName);
        return chars
            .Select(c => c.NPCName)
            .Where(n => !string.IsNullOrWhiteSpace(n))
            .ToHashSet(StringComparer.OrdinalIgnoreCase)!;
    }

    private WikiAccessManifest? LoadManifest()
    {
        return _cache.GetOrCreate(ManifestCacheKey, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
            var path = Path.Combine(_environment.WebRootPath ?? "", "wiki", "static", "wiki-access.json");
            if (!File.Exists(path))
            {
                _logger.LogWarning("Wiki access manifest missing: {Path}", path);
                return null;
            }

            try
            {
                var raw = File.ReadAllText(path);
                var doc = JsonSerializer.Deserialize<ManifestFileDto>(raw, JsonOptions);
                if (doc?.Slugs is null)
                {
                    return null;
                }

                var manifest = new WikiAccessManifest
                {
                    Version = doc.Version,
                    Slugs = new Dictionary<string, WikiAccessEntry>(StringComparer.OrdinalIgnoreCase),
                };

                foreach (var (key, value) in doc.Slugs)
                {
                    manifest.Slugs[NormalizeSlug(key)] = new WikiAccessEntry
                    {
                        Mode = ParseMode(value.Visibility ?? value.Mode),
                        Characters = value.Characters ?? [],
                        Parties = value.Parties ?? [],
                        Reason = value.Reason,
                    };
                }

                var partiesPath = Path.Combine(_environment.WebRootPath ?? "", "wiki", "static", "wiki-parties.json");
                if (File.Exists(partiesPath))
                {
                    var parties = JsonSerializer.Deserialize<WikiPartiesConfig>(File.ReadAllText(partiesPath), JsonOptions);
                    manifest.AnonymousPrefixes = parties?.AnonymousPublicPrefixes ?? ["świat-i-zasady"];
                    manifest.LoggedInPublicPrefixes = parties?.LoggedInPublicPrefixes ?? [];
                    manifest.DukeAccessibleCampaignIds = parties?.DukeAccessibleCampaignIds ?? [];
                    manifest.CampaignIds = parties?.Campaigns?
                        .Select(c => c.Id.Trim().Trim('/'))
                        .Where(id => !string.IsNullOrWhiteSpace(id))
                        .ToList() ?? [];

                    if (parties?.Parties is not null)
                    {
                        foreach (var (partyId, party) in parties.Parties)
                        {
                            manifest.Parties[partyId] = party.Characters;
                            foreach (var name in party.Characters)
                            {
                                manifest.AllPartyCharacters.Add(name);
                                manifest.CharacterCanonical[NormalizeCharacter(name)] = name;
                            }
                        }
                    }

                    if (parties?.CharacterAliases is not null)
                    {
                        foreach (var (canonical, aliases) in parties.CharacterAliases)
                        {
                            foreach (var alias in aliases)
                            {
                                manifest.CharacterCanonical[NormalizeCharacter(alias)] = canonical;
                            }
                        }
                    }
                }
                else
                {
                    manifest.AnonymousPrefixes = ["świat-i-zasady"];
                }

                return manifest;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load wiki access manifest");
                return null;
            }
        });
    }

    /// <summary>
    /// Maps an authored <c>visibility</c> (or legacy <c>mode</c>) string to <see cref="WikiAccessMode"/>.
    /// Unknown values fail closed to <see cref="WikiAccessMode.Deny"/>.
    /// </summary>
    private static WikiAccessMode ParseMode(string? value)
    {
        return (value ?? string.Empty).Trim().ToLowerInvariant() switch
        {
            "public" or "anonymous" => WikiAccessMode.Anonymous,
            "authenticated" or "logged-in" => WikiAccessMode.Authenticated,
            "characters" => WikiAccessMode.Characters,
            "party" => WikiAccessMode.Party,
            "gm-only" or "gm" or "deny" => WikiAccessMode.Deny,
            _ => WikiAccessMode.Deny,
        };
    }

    private static string ExtractSlugFromSitemapLoc(string loc)
    {
        if (!Uri.TryCreate(loc, UriKind.Absolute, out var uri))
        {
            return NormalizeSlug(loc);
        }

        var path = uri.AbsolutePath.Trim('/');
        const string wikiPrefix = "wiki/";
        var idx = path.IndexOf(wikiPrefix, StringComparison.OrdinalIgnoreCase);
        if (idx >= 0)
        {
            path = path[(idx + wikiPrefix.Length)..];
        }

        return NormalizeSlug(path);
    }

    private static string NormalizeCharacter(string value) =>
        new string((value ?? string.Empty)
            .Where(ch => ch is not (' ' or '-' or '_' or '.'))
            .ToArray())
            .ToLowerInvariant();

    /// <summary>Maps raw character names (e.g. from the selected hero) to their canonical form.</summary>
    private static HashSet<string> Canonicalize(IEnumerable<string> names, WikiAccessManifest? manifest)
    {
        var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var name in names)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                continue;
            }

            if (manifest is not null
                && manifest.CharacterCanonical.TryGetValue(NormalizeCharacter(name), out var canonical))
            {
                result.Add(canonical);
            }
            else
            {
                result.Add(name);
            }
        }

        return result;
    }

    private static string NormalizeSlug(string slug)
    {
        slug = slug.Trim().TrimStart('/').Replace('\\', '/');
        if (slug.EndsWith(".html", StringComparison.OrdinalIgnoreCase))
        {
            slug = slug[..^5];
        }

        if (slug.EndsWith("/index", StringComparison.OrdinalIgnoreCase))
        {
            slug = slug[..^6].TrimEnd('/');
        }

        return slug;
    }

    private sealed class ManifestFileDto
    {
        public int Version { get; set; }
        public Dictionary<string, ManifestSlugDto> Slugs { get; set; } = new();
    }

    private sealed class ManifestSlugDto
    {
        /// <summary>Preferred field written by the manifest builder.</summary>
        public string? Visibility { get; set; }

        /// <summary>Legacy field kept for backward compatibility with older manifests.</summary>
        public string Mode { get; set; } = "deny";

        public List<string>? Characters { get; set; }
        public List<string>? Parties { get; set; }
        public string? Reason { get; set; }
    }

    private sealed class Utf8StringWriter : StringWriter
    {
        public override Encoding Encoding => Encoding.UTF8;
    }
}
