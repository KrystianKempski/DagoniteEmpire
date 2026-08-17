using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;
using DA_Business.Repository.CharacterReps.IRepository;
using DA_Business.Services.Interfaces;
using DA_Business.Services.Wiki;
using DA_Common;
using DA_DataAccess;
using DA_DataAccess.Data;
using DA_Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DA_Business.Services;

public class WikiAccessService : IWikiAccessService
{
    private const string ManifestCacheKey = "wiki-access-manifest-v7";
    private const string DefaultLoreEntryPath = "/wiki/świat-i-zasady/";
    private const string CampaignHubEntryPath = "/wiki/index.html";
    private const string EmptySitemap =
        """<?xml version="1.0" encoding="UTF-8"?><urlset xmlns="http://www.sitemaps.org/schemas/sitemap/0.9"></urlset>""";

    private readonly ICharacterRepository _characters;
    private readonly IWebHostEnvironment _environment;
    private readonly IMemoryCache _cache;
    private readonly ILogger<WikiAccessService> _logger;
    private readonly IUserService _userService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IDbContextFactory<ApplicationDbContext> _db;
    private readonly bool _logAccessDecisions;

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
        IHttpContextAccessor httpContextAccessor,
        IDbContextFactory<ApplicationDbContext> db,
        IConfiguration configuration)
    {
        _characters = characters;
        _environment = environment;
        _cache = cache;
        _logger = logger;
        _userService = userService;
        _httpContextAccessor = httpContextAccessor;
        _db = db;
        _logAccessDecisions = configuration.GetValue(
            "WikiAccess:LogDecisions",
            string.Equals(environment.EnvironmentName, "Development", StringComparison.OrdinalIgnoreCase));
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
            return context.TreatAsAdmin
                || WikiAccessEvaluator.IsPrefixMatch(WikiAccessEvaluator.NormalizeSlug(slug), ["świat-i-zasady"]);
        }

        var allowed = WikiAccessEvaluator.CanAccessSlug(userName, context, slug, manifest);
        if (_logAccessDecisions && !allowed && !context.TreatAsAdmin)
        {
            var normalized = WikiAccessEvaluator.NormalizeSlug(slug);
            if (WikiAccessEvaluator.IsUnderCampaign(normalized, manifest)
                || WikiAccessEvaluator.IsPrefixMatch(normalized, manifest.LoggedInPublicPrefixes))
            {
                _logger.LogInformation(
                    "Wiki deny slug={Slug} identity={Identity} owner={Owner} chars=[{Chars}] campaignParticipant={Participant}",
                    normalized,
                    userName,
                    await ResolveOwnerUserNameAsync(userName),
                    string.Join(", ", context.UserCharacters),
                    context.IsCampaignParticipant);
            }
        }

        return allowed;
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
            // Fail closed: contentIndex.json carries the full page text, so a missing manifest
            // must hide everything for non-MG rather than expose the whole wiki.
            return "{}";
        }

        try
        {
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.ValueKind != JsonValueKind.Object)
            {
                return "{}";
            }

            var filtered = new Dictionary<string, JsonElement>(StringComparer.OrdinalIgnoreCase);
            foreach (var prop in doc.RootElement.EnumerateObject())
            {
                if (WikiAccessEvaluator.CanAccessSlug(userName, context, prop.Name, manifest))
                {
                    filtered[prop.Name] = prop.Value.Clone();
                }
            }

            return JsonSerializer.Serialize(filtered, JsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to filter wiki content index");
            return "{}";
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
            // Fail closed: serve an empty sitemap rather than leak every slug when the manifest is gone.
            return EmptySitemap;
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
                if (!WikiAccessEvaluator.CanAccessSlug(userName, context, slug, manifest))
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

    public async Task<string> GetDefaultWikiEntryPathAsync(string? userName, bool isAdminOrMg)
    {
        if (await ShouldBypassAccessChecksAsync(userName, isAdminOrMg)
            || await CanAccessSlug(userName, isAdminOrMg, "index"))
        {
            return CampaignHubEntryPath;
        }

        return DefaultLoreEntryPath;
    }

    public async Task<bool> ShouldBypassAccessChecksAsync(string? userName, bool isAdminOrMg)
    {
        var context = await BuildContextAsync(userName, isAdminOrMg);
        return context.TreatAsAdmin;
    }

    public async Task<WikiAccessDiagnostics> GetAccessDiagnosticsAsync(string? userName, bool isAdminOrMg)
    {
        var manifest = LoadManifest();
        var ownerUserName = await ResolveOwnerUserNameAsync(userName);
        var userInfo = await TryGetUserInfoSafeAsync();
        var context = await BuildContextAsync(userName, isAdminOrMg);
        var cookieId = TryReadWikiCharacterCookie();
        var dbId = await TryReadDatabaseSelectedCharacterIdAsync();

        var samples = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase)
        {
            ["index"] = await CanAccessSlug(userName, isAdminOrMg, "index"),
            ["mapy/mapa-powiązań"] = await CanAccessSlug(userName, isAdminOrMg, "mapy/mapa-powiązań"),
            ["w-służbie-bonefyre"] = await CanAccessSlug(userName, isAdminOrMg, "w-służbie-bonefyre"),
            ["świat-i-zasady"] = await CanAccessSlug(userName, isAdminOrMg, "świat-i-zasady"),
        };

        return new WikiAccessDiagnostics
        {
            IdentityName = userName,
            ResolvedOwnerUserName = ownerUserName,
            CookieCharacterId = cookieId,
            DatabaseSelectedCharacterId = dbId,
            BlazorSelectedCharacterId = userInfo?.SelectedCharacterId,
            SelectedNpcName = userInfo?.SelectedCharacter?.NPCName,
            UserCharacters = context.UserCharacters.OrderBy(c => c, StringComparer.OrdinalIgnoreCase).ToList(),
            IsCampaignParticipant = context.IsCampaignParticipant,
            TreatAsAdmin = context.TreatAsAdmin,
            UserInfoFromBlazorSession = userInfo is not null,
            SampleSlugAccess = samples,
        };
    }

    public bool IsAnonymousPublicPath(string slug) =>
        WikiAccessEvaluator.IsAnonymousPublicPath(LoadManifest(), slug);

    private async Task<WikiAccessContext> BuildContextAsync(string? userName, bool isAdminOrMg)
    {
        // BuildContext hits the DB (owner resolution + approved character names); a single
        // request can ask for many slugs (content index, sitemap), so memoize it per HttpContext.
        var items = _httpContextAccessor.HttpContext?.Items;
        var cacheKey = $"__wiki_access_ctx::{userName}::{isAdminOrMg}";
        if (items is not null
            && items.TryGetValue(cacheKey, out var cached)
            && cached is WikiAccessContext cachedContext)
        {
            return cachedContext;
        }

        var built = await BuildContextCoreAsync(userName, isAdminOrMg);
        if (items is not null)
        {
            items[cacheKey] = built;
        }

        return built;
    }

    private async Task<WikiAccessContext> BuildContextCoreAsync(string? userName, bool isAdminOrMg)
    {
        var manifest = LoadManifest();
        var userInfo = await TryGetUserInfoSafeAsync();

        // MG/Admin always see everything — even when a player character is selected in the UI.
        // Hidden demo accounts keep the GameMaster role for the throwaway barony, but must not
        // inherit global wiki / character access.
        var actingName = userInfo?.UserName ?? userName;
        if ((userInfo?.CharacterMG == true || isAdminOrMg) && !SD.IsDemoUserName(actingName))
        {
            return new WikiAccessContext { TreatAsAdmin = true };
        }

        var ownerUserName = await ResolveOwnerUserNameAsync(userName);
        // Union of all IsApproved characters — selection in the UI is not required for wiki ACL.
        var chars = WikiAccessEvaluator.Canonicalize(await GetApprovedCharacterNames(ownerUserName), manifest);
        return new WikiAccessContext
        {
            UserCharacters = chars,
            IsCampaignParticipant = WikiAccessEvaluator.IsCampaignParticipant(chars, manifest),
            IsDukeLoreOnly = IsDukeLoreOnlyUser(chars),
        };
    }

    private async Task<UserInfo?> TryGetUserInfoSafeAsync()
    {
        try
        {
            return await _userService.GetUserInfo();
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "GetUserInfo unavailable outside Blazor circuit");
            return null;
        }
    }

    private async Task<string?> ResolveOwnerUserNameAsync(string? identityName)
    {
        if (!string.IsNullOrWhiteSpace(identityName))
        {
            var fromIdentity = await GetApprovedCharacterNames(identityName);
            if (fromIdentity.Count > 0)
            {
                return identityName;
            }
        }

        var httpUser = _httpContextAccessor.HttpContext?.User;
        var userId = httpUser?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return identityName;
        }

        await using var context = await _db.CreateDbContextAsync();
        var appUser = await context.Users.OfType<ApplicationUser>().AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
        if (appUser is null || string.IsNullOrWhiteSpace(appUser.UserName))
        {
            return identityName;
        }

        if (!string.Equals(appUser.UserName, identityName, StringComparison.OrdinalIgnoreCase))
        {
            var fromDbUserName = await GetApprovedCharacterNames(appUser.UserName);
            if (fromDbUserName.Count > 0)
            {
                return appUser.UserName;
            }
        }

        return identityName ?? appUser.UserName;
    }

    private int? TryReadWikiCharacterCookie()
    {
        var http = _httpContextAccessor.HttpContext;
        if (http?.Request.Cookies.TryGetValue(SD.WikiSelectedCharacterCookie, out var raw) != true)
        {
            return null;
        }

        return int.TryParse(raw, out var id) ? id : null;
    }

    private async Task<int?> TryReadDatabaseSelectedCharacterIdAsync()
    {
        var userId = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return null;
        }

        await using var context = await _db.CreateDbContextAsync();
        var selected = await context.Users
            .OfType<ApplicationUser>()
            .AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(u => u.SelectedCharacterId)
            .FirstOrDefaultAsync();
        return selected == 0 ? null : selected;
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

        return !WikiAccessEvaluator.IsCampaignParticipant(userCharacters, manifest);
    }

    private async Task<HashSet<string>> GetApprovedCharacterNames(string? userName)
    {
        if (string.IsNullOrWhiteSpace(userName))
        {
            return new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        var chars = await _characters.GetAllApproved(userName);
        return chars
            .Select(c => c.NPCName)
            .Where(n => !string.IsNullOrWhiteSpace(n)
                && !string.Equals(n, SD.GameMaster_NPCName, StringComparison.OrdinalIgnoreCase))
            .ToHashSet(StringComparer.OrdinalIgnoreCase)!;
    }

    private WikiAccessManifest? LoadManifest()
    {
        var webRoot = _environment.WebRootPath ?? "";
        var accessPath = Path.Combine(webRoot, "wiki", "static", "wiki-access.json");
        var partiesPath = Path.Combine(webRoot, "wiki", "static", "wiki-parties.json");
        if (!File.Exists(accessPath))
        {
            _logger.LogWarning("Wiki access manifest missing: {Path}", accessPath);
            return null;
        }

        // Key the cache by the source files' last-write time so a rebuilt manifest is picked up
        // immediately instead of waiting out the TTL or requiring a manual cache-key bump.
        var stamp = File.GetLastWriteTimeUtc(accessPath).Ticks;
        if (File.Exists(partiesPath))
        {
            stamp ^= File.GetLastWriteTimeUtc(partiesPath).Ticks;
        }

        return _cache.GetOrCreate($"{ManifestCacheKey}:{stamp}", entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);

            try
            {
                var raw = File.ReadAllText(accessPath);
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
                    manifest.Slugs[WikiAccessEvaluator.NormalizeSlug(key)] = new WikiAccessEntry
                    {
                        Mode = ParseMode(value.Visibility ?? value.Mode),
                        Characters = value.Characters ?? [],
                        Parties = value.Parties ?? [],
                        Reason = value.Reason,
                    };
                }

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
                                manifest.CharacterCanonical[WikiAccessEvaluator.NormalizeCharacter(name)] = name;
                            }
                        }
                    }

                    if (parties?.CharacterAliases is not null)
                    {
                        foreach (var (canonical, aliases) in parties.CharacterAliases)
                        {
                            foreach (var alias in aliases)
                            {
                                manifest.CharacterCanonical[WikiAccessEvaluator.NormalizeCharacter(alias)] = canonical;
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
            return WikiAccessEvaluator.NormalizeSlug(loc);
        }

        var path = uri.AbsolutePath.Trim('/');
        const string wikiPrefix = "wiki/";
        var idx = path.IndexOf(wikiPrefix, StringComparison.OrdinalIgnoreCase);
        if (idx >= 0)
        {
            path = path[(idx + wikiPrefix.Length)..];
        }

        return WikiAccessEvaluator.NormalizeSlug(path);
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
