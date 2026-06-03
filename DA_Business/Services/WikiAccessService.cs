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
    private const string ManifestCacheKey = "wiki-access-manifest-v3";

    private readonly ICharacterRepository _characters;
    private readonly IWebHostEnvironment _environment;
    private readonly IMemoryCache _cache;
    private readonly ILogger<WikiAccessService> _logger;
    private readonly IWikiViewAsService _viewAs;
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
        IWikiViewAsService viewAs,
        IHttpContextAccessor httpContextAccessor)
    {
        _characters = characters;
        _environment = environment;
        _cache = cache;
        _logger = logger;
        _viewAs = viewAs;
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
        if (context.TreatAsAdmin)
        {
            return true;
        }

        if (context.IsDukeLoreOnly && !AllowsDukePlayerSlug(NormalizeSlug(slug)))
        {
            return false;
        }

        var manifest = LoadManifest();
        if (manifest is null)
        {
            return !string.IsNullOrWhiteSpace(userName);
        }

        slug = NormalizeSlug(slug);
        if (!manifest.Slugs.TryGetValue(slug, out var entry))
        {
            if (slug.EndsWith("/index", StringComparison.OrdinalIgnoreCase))
            {
                var parent = slug[..^6].TrimEnd('/');
                if (manifest.Slugs.TryGetValue(parent, out entry))
                {
                    return EvaluateEntrySync(entry, userName, context.UserCharacters);
                }
            }

            return !string.IsNullOrWhiteSpace(userName);
        }

        return EvaluateEntrySync(entry, userName, context.UserCharacters);
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
                var slug = NormalizeSlug(prop.Name);
                if (context.IsDukeLoreOnly && !AllowsDukePlayerSlug(slug))
                {
                    continue;
                }

                if (!manifest.Slugs.TryGetValue(slug, out var entry))
                {
                    if (!string.IsNullOrWhiteSpace(userName))
                    {
                        filtered[prop.Name] = prop.Value.Clone();
                    }

                    continue;
                }

                if (EvaluateEntrySync(entry, userName, context.UserCharacters))
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
                if (string.IsNullOrWhiteSpace(slug))
                {
                    continue;
                }

                if (context.IsDukeLoreOnly && !AllowsDukePlayerSlug(slug))
                {
                    url.Remove();
                    continue;
                }

                if (!manifest.Slugs.TryGetValue(slug, out var entry))
                {
                    if (string.IsNullOrWhiteSpace(userName))
                    {
                        url.Remove();
                    }

                    continue;
                }

                if (!EvaluateEntrySync(entry, userName, context.UserCharacters))
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

    public bool IsAnonymousPublicPath(string slug)
    {
        var manifest = LoadManifest();
        if (manifest is null)
        {
            return slug.StartsWith("świat-i-zasady", StringComparison.OrdinalIgnoreCase);
        }

        slug = NormalizeSlug(slug);
        if (manifest.Slugs.TryGetValue(slug, out var entry))
        {
            return entry.Mode == WikiAccessMode.Anonymous;
        }

        return manifest.AnonymousPrefixes.Any(p =>
            slug.Equals(p, StringComparison.OrdinalIgnoreCase)
            || slug.StartsWith(p + "/", StringComparison.OrdinalIgnoreCase));
    }

    private async Task<WikiAccessContext> BuildContextAsync(string? userName, bool isAdminOrMg)
    {
        var preview = _viewAs.GetViewAsCharacterName();
        if (isAdminOrMg && !string.IsNullOrWhiteSpace(preview))
        {
            return new WikiAccessContext
            {
                TreatAsAdmin = false,
                UserCharacters = new HashSet<string>([preview], StringComparer.OrdinalIgnoreCase),
            };
        }

        if (isAdminOrMg)
        {
            return new WikiAccessContext { TreatAsAdmin = true };
        }

        var chars = await GetUserCharacterNames(userName);
        return new WikiAccessContext
        {
            UserCharacters = chars,
            IsDukeLoreOnly = IsDukeLoreOnlyUser(chars),
        };
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

        return !userCharacters.Any(c => manifest.AllPartyCharacters.Contains(c, StringComparer.OrdinalIgnoreCase));
    }

    private bool AllowsDukePlayerSlug(string slug)
    {
        if (IsAnonymousPublicPath(slug))
        {
            return true;
        }

        var manifest = LoadManifest();
        if (manifest is null)
        {
            return false;
        }

        return manifest.DukeAccessibleCampaignIds.Any(id =>
            slug.Equals(id, StringComparison.OrdinalIgnoreCase)
            || slug.StartsWith(id + "/", StringComparison.OrdinalIgnoreCase));
    }

    private static bool EvaluateEntrySync(WikiAccessEntry entry, string? userName, HashSet<string> userCharacters)
    {
        return entry.Mode switch
        {
            WikiAccessMode.Anonymous => true,
            WikiAccessMode.Authenticated => !string.IsNullOrWhiteSpace(userName),
            WikiAccessMode.Characters => entry.Characters.Any(c =>
                userCharacters.Contains(c, StringComparer.OrdinalIgnoreCase)),
            WikiAccessMode.Deny => false,
            _ => !string.IsNullOrWhiteSpace(userName),
        };
    }

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
                        Mode = Enum.TryParse<WikiAccessMode>(value.Mode, true, out var mode)
                            ? mode
                            : WikiAccessMode.Authenticated,
                        Characters = value.Characters ?? [],
                        Reason = value.Reason,
                    };
                }

                var partiesPath = Path.Combine(_environment.WebRootPath ?? "", "wiki", "static", "wiki-parties.json");
                if (File.Exists(partiesPath))
                {
                    var parties = JsonSerializer.Deserialize<WikiPartiesConfig>(File.ReadAllText(partiesPath), JsonOptions);
                    manifest.AnonymousPrefixes = parties?.AnonymousPublicPrefixes ?? ["świat-i-zasady"];
                    manifest.DukeAccessibleCampaignIds = parties?.DukeAccessibleCampaignIds ?? [];
                    if (parties?.Parties is not null)
                    {
                        foreach (var party in parties.Parties.Values)
                        {
                            foreach (var name in party.Characters)
                            {
                                manifest.AllPartyCharacters.Add(name);
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
        public string Mode { get; set; } = "authenticated";
        public List<string>? Characters { get; set; }
        public string? Reason { get; set; }
    }

    private sealed class Utf8StringWriter : StringWriter
    {
        public override Encoding Encoding => Encoding.UTF8;
    }
}
