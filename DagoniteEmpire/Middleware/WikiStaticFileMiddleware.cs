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
                context.Response.StatusCode = StatusCodes.Status404NotFound;
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
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                return;
            }
        }

        if (relativePath.Equals("static/contentIndex.json", StringComparison.OrdinalIgnoreCase)
            && !isAdminOrMg)
        {
            await ServeFilteredJson(
                context,
                "static/contentIndex.json",
                async json => await _wikiAccess.FilterContentIndexAsync(userName, false, json) ?? "{}");
            return;
        }

        if (relativePath.Equals("static/encryptedContentIndex.json", StringComparison.OrdinalIgnoreCase)
            && !isAdminOrMg)
        {
            await ServeFilteredJson(
                context,
                "static/encryptedContentIndex.json",
                async json => await _wikiAccess.FilterEncryptedContentIndexAsync(userName, false, json));
            return;
        }

        if (relativePath.Equals("sitemap.xml", StringComparison.OrdinalIgnoreCase)
            && !isAdminOrMg)
        {
            await ServeFilteredXml(
                context,
                "sitemap.xml",
                xml => _wikiAccess.FilterSitemapAsync(userName, false, xml));
            return;
        }

        var fileInfo = _wikiFiles.GetFileInfo(relativePath);
        if (!fileInfo.Exists)
        {
            var directoryInfo = _wikiFiles.GetDirectoryContents(relativePath);
            if (directoryInfo.Exists)
            {
                fileInfo = _wikiFiles.GetFileInfo($"{relativePath.TrimEnd('/')}/index.html");
                if (fileInfo.Exists)
                {
                    var indexSlug = relativePath.TrimEnd('/');
                    if (!isAdminOrMg && !await _wikiAccess.CanAccessSlug(userName, isAdminOrMg, indexSlug))
                    {
                        context.Response.StatusCode = StatusCodes.Status404NotFound;
                        return;
                    }
                }
            }
        }

        if (!fileInfo.Exists || fileInfo.IsDirectory)
        {
            context.Response.StatusCode = StatusCodes.Status404NotFound;
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
        if (_wikiFiles is not null)
        {
            return;
        }

        var wikiRoot = Path.GetFullPath(Path.Combine(_environment.WebRootPath ?? "", "wiki"));
        if (!Directory.Exists(wikiRoot))
        {
            return;
        }

        _wikiFiles = new PhysicalFileProvider(wikiRoot);
    }

    private static bool IsAdminOrMg(ClaimsPrincipal user) =>
        user.IsInRole(SD.Role_Admin) || user.IsInRole(SD.Role_GameMaster);
}
