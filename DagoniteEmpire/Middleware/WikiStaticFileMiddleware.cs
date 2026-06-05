using System.Security.Claims;
using System.Text.RegularExpressions;
using DA_Business.Services.Interfaces;
using DA_Common;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;

namespace DagoniteEmpire.Middleware;

/// <summary>
/// Serves the pre-built Quartz site from wwwroot/wiki under /wiki/* with per-slug access control.
/// Runtime model B: middleware only (no client-side ACL in the Blazor iframe host). Players get full Quartz chrome;
/// the explorer is built client-side from per-user filtered static/contentIndex.json.
/// </summary>
public class WikiStaticFileMiddleware : IMiddleware
{
    public const string WikiUrlPrefix = "/wiki";
    private static readonly string[] MgOnlyStaticFiles =
    [
        "static/wiki-access.json",
        "static/wiki-parties.json",
        // Read server-side by WikiLinkService only; never needs to be reachable over HTTP and
        // would otherwise leak the character→slug map and the full roster of player characters.
        "static/wiki-links.json",
    ];

    private const string OgImageMarker = "-og-image";

    /// <summary>
    /// Quartz slugs (e.g. wizyta-w-kojcu-cz.1, barana,-cz.-2) contain dots — Path.GetExtension
    /// wrongly treats .1 / .-2 as file extensions and blocks appending .html.
    /// </summary>
    private static readonly HashSet<string> WikiStaticExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".html", ".htm", ".css", ".js", ".mjs", ".json", ".xml", ".webp", ".png", ".jpg", ".jpeg",
        ".gif", ".svg", ".ico", ".woff", ".woff2", ".ttf", ".map", ".txt",
    };

    /// <summary>Quartz emits fingerprinted bundles (index-abc12345.css). Safe to cache forever by filename.</summary>
    private static readonly Regex ContentHashedAssetRegex = new(
        @"-[a-f0-9]{8}\.(?:css|js|mjs)$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    private readonly IWebHostEnvironment _environment;
    private readonly IWikiAccessService _wikiAccess;
    private readonly FileExtensionContentTypeProvider _contentTypes = new();
    private PhysicalFileProvider? _wikiFiles;
    private string? _accessDeniedHtmlPath;
    private long _wikiDeployStamp;

    public WikiStaticFileMiddleware(IWebHostEnvironment environment, IWikiAccessService wikiAccess)
    {
        _environment = environment;
        _wikiAccess = wikiAccess;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var requestPath = context.Request.Path;
        if (!requestPath.StartsWithSegments(WikiUrlPrefix, out var remainder))
        {
            await next(context);
            return;
        }

        var subPath = GetWikiSubPath(context, remainder.Value);
        // Breadcrumb "Home" resolves to /wiki/ (relative ../..). That must serve the Quartz hub,
        // not the Blazor @page "/wiki" iframe wrapper (iframe-in-iframe).
        if (subPath is "" or "/")
        {
            subPath = "index.html";
        }

        EnsureWikiProvider();
        if (_wikiFiles is null)
        {
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            return;
        }

        var relativePath = Uri.UnescapeDataString(subPath.TrimStart('/'));
        var slug = SlugFromRelativePath(relativePath);
        var isAdminOrMg = IsAdminOrMg(context.User);
        var userName = context.User.Identity?.Name;

        if (relativePath.Equals("debug/access.json", StringComparison.OrdinalIgnoreCase))
        {
            await ServeAccessDiagnosticsAsync(context, userName, isAdminOrMg);
            return;
        }

        var access = await AuthorizeRequestAsync(context, relativePath, slug, isAdminOrMg, userName);
        switch (access.Outcome)
        {
            case AccessOutcome.RedirectToLogin:
                var returnUrl = Uri.EscapeDataString(context.Request.Path + context.Request.QueryString);
                context.Response.Redirect($"/Account/Login?returnUrl={returnUrl}");
                return;
            case AccessOutcome.Denied:
                await ServeAccessDeniedAsync(context);
                return;
            case AccessOutcome.NotFound:
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                return;
        }

        var aclGated = access.AclGated;

        if (relativePath.Equals("static/contentIndex.json", StringComparison.OrdinalIgnoreCase))
        {
            await ServeFilteredJson(
                context,
                "static/contentIndex.json",
                async json => await _wikiAccess.FilterContentIndexAsync(userName, isAdminOrMg, json) ?? "{}");
            return;
        }

        if (relativePath.Equals("static/encryptedContentIndex.json", StringComparison.OrdinalIgnoreCase))
        {
            await ServeFilteredJson(
                context,
                "static/encryptedContentIndex.json",
                async json => await _wikiAccess.FilterEncryptedContentIndexAsync(userName, isAdminOrMg, json));
            return;
        }

        if (relativePath.Equals("sitemap.xml", StringComparison.OrdinalIgnoreCase))
        {
            await ServeFilteredXml(
                context,
                "sitemap.xml",
                xml => _wikiAccess.FilterSitemapAsync(userName, isAdminOrMg, xml));
            return;
        }

        var fileInfo = ResolveWikiFile(relativePath);

        if (!fileInfo.Exists || fileInfo.IsDirectory)
        {
            if (LooksLikePageRequest(relativePath))
            {
                await ServeNotFoundAsync(context);
            }
            else
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
            }

            return;
        }

        if (!_contentTypes.TryGetContentType(fileInfo.Name, out var contentType))
        {
            contentType = "application/octet-stream";
        }

        context.Response.ContentType = contentType;
        ApplyWikiCacheHeaders(context, relativePath, fileInfo.Name, aclGated);

        // Players get the full Quartz chrome (explorer/search) for navigation: the explorer is built
        // client-side from the per-user filtered contentIndex.json, so it only lists pages they can open.
        await context.Response.SendFileAsync(fileInfo.PhysicalPath, context.RequestAborted);
    }

    private enum AccessOutcome
    {
        Allow,
        RedirectToLogin,
        Denied,
        NotFound,
    }

    private readonly record struct AccessDecision(AccessOutcome Outcome, bool AclGated)
    {
        public static AccessDecision Allow(bool aclGated) => new(AccessOutcome.Allow, aclGated);
        public static readonly AccessDecision Redirect = new(AccessOutcome.RedirectToLogin, false);
        public static readonly AccessDecision Denied = new(AccessOutcome.Denied, false);
        public static readonly AccessDecision NotFound = new(AccessOutcome.NotFound, false);
    }

    /// <summary>
    /// Decides whether the request may be served, without writing to the response. Content assets
    /// (images, og-images, attachments) are gated by the slug of their owning page; denied asset
    /// requests return 404 (rather than a redirect/denied page) so they don't leak existence.
    /// </summary>
    private async Task<AccessDecision> AuthorizeRequestAsync(
        HttpContext context,
        string relativePath,
        string slug,
        bool isAdminOrMg,
        string? userName)
    {
        if (MgOnlyStaticFiles.Any(f => relativePath.Equals(f, StringComparison.OrdinalIgnoreCase)))
        {
            return isAdminOrMg ? AccessDecision.Allow(false) : AccessDecision.Denied;
        }

        var isAsset = IsStaticAssetFile(relativePath);
        var accessSlug = isAsset ? AssetOwnerSlug(relativePath) : slug;

        if (IsQuartzChromeAsset(relativePath) || _wikiAccess.IsAnonymousPublicPath(accessSlug))
        {
            return AccessDecision.Allow(false);
        }

        if (!(context.User.Identity?.IsAuthenticated ?? false))
        {
            return isAsset ? AccessDecision.NotFound : AccessDecision.Redirect;
        }

        if (!await _wikiAccess.CanAccessSlug(userName, isAdminOrMg, accessSlug))
        {
            return isAsset ? AccessDecision.NotFound : AccessDecision.Denied;
        }

        return AccessDecision.Allow(true);
    }

    private async Task ServeFilteredJson(
        HttpContext context,
        string relativePath,
        Func<string, Task<string?>> filter)
    {
        var fileInfo = _wikiFiles!.GetFileInfo(relativePath);
        if (!fileInfo.Exists || string.IsNullOrEmpty(fileInfo.PhysicalPath))
        {
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            return;
        }

        var json = await File.ReadAllTextAsync(fileInfo.PhysicalPath, context.RequestAborted);
        var filtered = await filter(json);
        context.Response.ContentType = "application/json";
        context.Response.Headers.CacheControl = "private, no-store";
        await context.Response.WriteAsync(filtered, context.RequestAborted);
    }

    private async Task ServeFilteredXml(
        HttpContext context,
        string relativePath,
        Func<string, Task<string?>> filter)
    {
        var fileInfo = _wikiFiles!.GetFileInfo(relativePath);
        if (!fileInfo.Exists || string.IsNullOrEmpty(fileInfo.PhysicalPath))
        {
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            return;
        }

        var xml = await File.ReadAllTextAsync(fileInfo.PhysicalPath, context.RequestAborted);
        var filtered = await filter(xml);
        if (filtered is null)
        {
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            return;
        }

        context.Response.ContentType = "application/xml";
        context.Response.Headers.CacheControl = "private, no-store";
        await context.Response.WriteAsync(filtered, context.RequestAborted);
    }

    private IFileInfo ResolveWikiFile(string relativePath)
    {
        var fileInfo = _wikiFiles!.GetFileInfo(relativePath);
        if (fileInfo.Exists && !fileInfo.IsDirectory)
        {
            return fileInfo;
        }

        if (ShouldAppendHtmlExtension(relativePath))
        {
            var htmlPath = $"{relativePath.TrimEnd('/')}.html";
            fileInfo = _wikiFiles.GetFileInfo(htmlPath);
            if (fileInfo.Exists && !fileInfo.IsDirectory)
            {
                return fileInfo;
            }
        }

        var directoryInfo = _wikiFiles.GetDirectoryContents(relativePath);
        if (directoryInfo.Exists)
        {
            fileInfo = _wikiFiles.GetFileInfo($"{relativePath.TrimEnd('/')}/index.html");
            if (fileInfo.Exists && !fileInfo.IsDirectory)
            {
                return fileInfo;
            }
        }

        return _wikiFiles.GetFileInfo(relativePath);
    }

    private static bool LooksLikePageRequest(string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
        {
            return true;
        }

        return ShouldAppendHtmlExtension(relativePath);
    }

    private static bool ShouldAppendHtmlExtension(string relativePath)
    {
        var extension = Path.GetExtension(relativePath);
        return string.IsNullOrEmpty(extension) || !WikiStaticExtensions.Contains(extension);
    }

    /// <summary>
    /// Shared Quartz chrome (CSS/JS/fonts/icons) is not in the access manifest, so slug ACL must not
    /// block it. Only the site-wide static/ bundle and root-level assets count as chrome; assets nested
    /// under content folders are treated as page attachments and go through <see cref="AssetOwnerSlug"/>.
    /// </summary>
    private static bool IsQuartzChromeAsset(string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
        {
            return false;
        }

        if (MgOnlyStaticFiles.Any(f => relativePath.Equals(f, StringComparison.OrdinalIgnoreCase)))
        {
            return false;
        }

        if (relativePath.StartsWith("static/", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        // Root-level assets (index.css, prescript.js, postscript.js, favicon, default og-image, …).
        if (!relativePath.Contains('/', StringComparison.Ordinal))
        {
            return IsStaticAssetFile(relativePath);
        }

        return false;
    }

    /// <summary>True for files served verbatim (images, css, js, fonts, …); false for HTML page requests.</summary>
    private static bool IsStaticAssetFile(string relativePath)
    {
        var extension = Path.GetExtension(relativePath);
        return !string.IsNullOrEmpty(extension)
            && WikiStaticExtensions.Contains(extension)
            && !extension.Equals(".html", StringComparison.OrdinalIgnoreCase)
            && !extension.Equals(".htm", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Maps a nested content asset to the slug of its owning page. <c>foo/bar-og-image.webp</c> belongs
    /// to page <c>foo/bar</c>; any other nested file inherits the ACL of its containing folder page.
    /// </summary>
    private static string AssetOwnerSlug(string relativePath)
    {
        var dir = relativePath.Contains('/', StringComparison.Ordinal)
            ? relativePath[..relativePath.LastIndexOf('/')]
            : string.Empty;

        var fileStem = Path.GetFileNameWithoutExtension(relativePath);
        if (fileStem.EndsWith(OgImageMarker, StringComparison.OrdinalIgnoreCase))
        {
            var pageStem = fileStem[..^OgImageMarker.Length];
            var ownerSlug = string.IsNullOrEmpty(dir) ? pageStem : $"{dir}/{pageStem}";
            return ownerSlug.Trim('/');
        }

        return dir.Trim('/');
    }

    private async Task ServeNotFoundAsync(HttpContext context)
    {
        var notFound = _wikiFiles?.GetFileInfo("404.html");
        if (notFound?.Exists == true && !string.IsNullOrEmpty(notFound.PhysicalPath))
        {
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            context.Response.ContentType = "text/html; charset=utf-8";
            context.Response.Headers.CacheControl = "no-cache";
            await context.Response.SendFileAsync(notFound.PhysicalPath, context.RequestAborted);
            return;
        }

        context.Response.StatusCode = StatusCodes.Status404NotFound;
    }

    /// <summary>
    /// Kestrel keeps commas in Path, but some clients split at ',' into QueryString — merge when needed.
    /// </summary>
    private static string GetWikiSubPath(HttpContext context, string? remainder)
    {
        var sub = Uri.UnescapeDataString((remainder ?? string.Empty).TrimStart('/'));
        var qs = context.Request.QueryString.Value;
        if (!string.IsNullOrEmpty(qs) && qs.StartsWith(",", StringComparison.Ordinal))
        {
            sub += Uri.UnescapeDataString(qs);
        }

        return sub;
    }

    private static string SlugFromRelativePath(string relativePath)
    {
        var slug = relativePath;
        if (slug.EndsWith(".html", StringComparison.OrdinalIgnoreCase))
        {
            slug = slug[..^5];
        }

        return slug;
    }

    private void EnsureWikiProvider()
    {
        var webRoot = _environment.WebRootPath ?? "";
        if (_accessDeniedHtmlPath is null)
        {
            var deniedPath = Path.Combine(webRoot, "wiki-access-denied.html");
            if (File.Exists(deniedPath))
            {
                _accessDeniedHtmlPath = deniedPath;
            }
        }

        var wikiRoot = Path.GetFullPath(Path.Combine(webRoot, "wiki"));
        if (!Directory.Exists(wikiRoot))
        {
            _wikiFiles = null;
            return;
        }

        var indexPath = Path.Combine(wikiRoot, "index.html");
        var deployStamp = File.Exists(indexPath) ? File.GetLastWriteTimeUtc(indexPath).Ticks : 0L;
        if (_wikiFiles is not null && deployStamp == _wikiDeployStamp)
        {
            return;
        }

        _wikiDeployStamp = deployStamp;
        _wikiFiles = new PhysicalFileProvider(wikiRoot);
    }

    /// <summary>
    /// HTML must always revalidate so a rebuild cannot pair stale markup with deleted CSS hashes.
    /// Fingerprinted CSS/JS can be cached immutably — the filename changes when content changes.
    /// </summary>
    private static void ApplyWikiCacheHeaders(
        HttpContext context,
        string relativePath,
        string fileName,
        bool aclGated)
    {
        if (IsWikiHtmlRequest(relativePath, fileName))
        {
            context.Response.Headers.CacheControl = "no-cache";
            return;
        }

        if (ContentHashedAssetRegex.IsMatch(fileName))
        {
            context.Response.Headers.CacheControl = "public, max-age=31536000, immutable";
            return;
        }

        // ACL-gated responses depend on the active character — never cache across sessions.
        context.Response.Headers.CacheControl = aclGated ? "private, no-store" : "private, max-age=300";
    }

    private static bool IsWikiHtmlRequest(string relativePath, string fileName)
    {
        if (fileName.Equals("404.html", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (relativePath.EndsWith(".html", StringComparison.OrdinalIgnoreCase)
            || relativePath.EndsWith(".htm", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return ShouldAppendHtmlExtension(relativePath) && !IsStaticAssetFile(relativePath);
    }

    private async Task ServeAccessDeniedAsync(HttpContext context)
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        context.Response.Headers.CacheControl = "private, no-store";

        if (!string.IsNullOrEmpty(_accessDeniedHtmlPath) && File.Exists(_accessDeniedHtmlPath))
        {
            context.Response.ContentType = "text/html; charset=utf-8";
            await context.Response.SendFileAsync(_accessDeniedHtmlPath, context.RequestAborted);
            return;
        }

        context.Response.ContentType = "text/html; charset=utf-8";
        await context.Response.WriteAsync(
            """
            <!DOCTYPE html><html lang="pl"><head><meta charset="utf-8"><title>Brak dostępu</title></head>
            <body data-wiki-access-denied="1" style="font-family:Georgia,serif;background:#4a3828;color:#f8f4ec;padding:2rem;text-align:center">
            <div style="max-width:32rem;margin:auto;padding:2rem;background:#f3ebe0;color:#2c2218;border:2px solid #c9a227;border-radius:8px">
            <h1 style="color:#5c3d1e">Brak dostępu do tej strony</h1>
            <p style="color:#3a2f24">Ta część wiki jest niedostępna dla Twojej postaci.</p>
            <p><a href="/wiki/świat-i-zasady/" style="color:#fff;background:#7a5c2e;padding:0.5rem 1rem;text-decoration:none;border-radius:4px">Wróć do: Świat i zasady</a></p>
            </div></body></html>
            """,
            context.RequestAborted);
    }

    private async Task ServeAccessDiagnosticsAsync(HttpContext context, string? userName, bool isAdminOrMg)
    {
        if (!_environment.IsDevelopment())
        {
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            return;
        }

        if (!(context.User.Identity?.IsAuthenticated ?? false))
        {
            var returnUrl = Uri.EscapeDataString(context.Request.Path + context.Request.QueryString);
            context.Response.Redirect($"/Account/Login?returnUrl={returnUrl}");
            return;
        }

        var diagnostics = await _wikiAccess.GetAccessDiagnosticsAsync(userName, isAdminOrMg);
        context.Response.ContentType = "application/json; charset=utf-8";
        context.Response.Headers.CacheControl = "private, no-store";
        await context.Response.WriteAsJsonAsync(diagnostics, context.RequestAborted);
    }

    private static bool IsAdminOrMg(ClaimsPrincipal user) =>
        user.IsInRole(SD.Role_Admin) || user.IsInRole(SD.Role_GameMaster);
}
