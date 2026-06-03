using System.Security.Claims;
using DA_Business.Services.Interfaces;
using DA_Common;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;

namespace DagoniteEmpire.Middleware;

/// <summary>
/// Serves the pre-built Quartz site from wwwroot/wiki under /wiki/* with per-slug access control.
/// </summary>
public class WikiStaticFileMiddleware : IMiddleware
{
    public const string WikiUrlPrefix = "/wiki";
    private static readonly string[] MgOnlyStaticFiles =
    [
        "static/wiki-access.json",
        "static/wiki-parties.json",
    ];

    private readonly IWebHostEnvironment _environment;
    private readonly IWikiAccessService _wikiAccess;
    private readonly FileExtensionContentTypeProvider _contentTypes = new();
    private PhysicalFileProvider? _wikiFiles;
    private string? _accessDeniedHtmlPath;

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

        var subPath = remainder.Value ?? string.Empty;
        if (subPath is "" or "/")
        {
            await next(context);
            return;
        }

        EnsureWikiProvider();
        if (_wikiFiles is null)
        {
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            return;
        }

        var relativePath = subPath.TrimStart('/');
        var slug = SlugFromRelativePath(relativePath);
        var isAdminOrMg = IsAdminOrMg(context.User);
        var userName = context.User.Identity?.Name;

        if (MgOnlyStaticFiles.Any(f =>
                relativePath.Equals(f, StringComparison.OrdinalIgnoreCase)))
        {
            if (!isAdminOrMg)
            {
                await ServeAccessDeniedAsync(context);
                return;
            }
        }
        else if (!_wikiAccess.IsAnonymousPublicPath(slug))
        {
            if (!(context.User.Identity?.IsAuthenticated ?? false))
            {
                var returnUrl = Uri.EscapeDataString(context.Request.Path + context.Request.QueryString);
                context.Response.Redirect($"/Account/Login?returnUrl={returnUrl}");
                return;
            }

            if (!await _wikiAccess.CanAccessSlug(userName, isAdminOrMg, slug))
            {
                await ServeAccessDeniedAsync(context);
                return;
            }
        }

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
        context.Response.Headers.CacheControl = "private, max-age=300";
        await context.Response.SendFileAsync(fileInfo.PhysicalPath, context.RequestAborted);
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

        if (!Path.HasExtension(relativePath))
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

        var extension = Path.GetExtension(relativePath);
        return string.IsNullOrEmpty(extension)
            || extension.Equals(".html", StringComparison.OrdinalIgnoreCase);
    }

    private async Task ServeNotFoundAsync(HttpContext context)
    {
        if (IsIframeRequest(context))
        {
            await ServeIframeBlockedNotifyAsync(context, StatusCodes.Status404NotFound);
            return;
        }

        var notFound = _wikiFiles?.GetFileInfo("404.html");
        if (notFound?.Exists == true && !string.IsNullOrEmpty(notFound.PhysicalPath))
        {
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            context.Response.ContentType = "text/html; charset=utf-8";
            context.Response.Headers.CacheControl = "private, no-store";
            await context.Response.SendFileAsync(notFound.PhysicalPath, context.RequestAborted);
            return;
        }

        context.Response.StatusCode = StatusCodes.Status404NotFound;
    }

    private static bool IsIframeRequest(HttpContext context) =>
        string.Equals(
            context.Request.Headers["Sec-Fetch-Dest"].FirstOrDefault(),
            "iframe",
            StringComparison.OrdinalIgnoreCase);

    private static async Task ServeIframeBlockedNotifyAsync(HttpContext context, int statusCode)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "text/html; charset=utf-8";
        context.Response.Headers.CacheControl = "private, no-store";
        await context.Response.WriteAsync(IframeBlockedNotifyHtml, context.RequestAborted);
    }

    private const string IframeBlockedNotifyHtml =
        """
        <!DOCTYPE html><html lang="pl"><head><meta charset="utf-8"><title>Brak dostępu</title></head>
        <body data-wiki-access-denied="1"><script>
        (function(){try{if(window.parent!==window)window.parent.postMessage({type:'dagonite-wiki-blocked'},window.location.origin);}catch(e){}})();
        </script></body></html>
        """;

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

        if (_wikiFiles is not null)
        {
            return;
        }

        var wikiRoot = Path.GetFullPath(Path.Combine(webRoot, "wiki"));
        if (!Directory.Exists(wikiRoot))
        {
            return;
        }

        _wikiFiles = new PhysicalFileProvider(wikiRoot);
    }

    private async Task ServeAccessDeniedAsync(HttpContext context)
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        context.Response.Headers.CacheControl = "private, no-store";

        if (IsIframeRequest(context))
        {
            await ServeIframeBlockedNotifyAsync(context, StatusCodes.Status403Forbidden);
            return;
        }

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

    private static bool IsAdminOrMg(ClaimsPrincipal user) =>
        user.IsInRole(SD.Role_Admin) || user.IsInRole(SD.Role_GameMaster);
}
