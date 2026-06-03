using DA_Business.Services.Interfaces;
using DA_Business.Services.Wiki;
using Microsoft.AspNetCore.Http;

namespace DA_Business.Services;

public class WikiViewAsService : IWikiViewAsService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public WikiViewAsService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? GetViewAsCharacterName()
    {
        var context = _httpContextAccessor.HttpContext;
        if (context is null)
        {
            return null;
        }

        return context.Request.Cookies.TryGetValue(WikiViewAsConstants.CookieName, out var value)
            && !string.IsNullOrWhiteSpace(value)
            ? value.Trim()
            : null;
    }

    public bool IsPreviewActive() => !string.IsNullOrWhiteSpace(GetViewAsCharacterName());

    public void SetViewAs(string? npcName)
    {
        var context = _httpContextAccessor.HttpContext
            ?? throw new InvalidOperationException("No HTTP context");

        if (string.IsNullOrWhiteSpace(npcName))
        {
            ClearViewAs();
            return;
        }

        context.Response.Cookies.Append(
            WikiViewAsConstants.CookieName,
            npcName.Trim(),
            new CookieOptions
            {
                Path = WikiViewAsConstants.CookiePath,
                HttpOnly = true,
                Secure = context.Request.IsHttps,
                SameSite = SameSiteMode.Lax,
                IsEssential = true,
                MaxAge = TimeSpan.FromHours(8),
            });
    }

    public void ClearViewAs()
    {
        var context = _httpContextAccessor.HttpContext;
        if (context is null)
        {
            return;
        }

        context.Response.Cookies.Delete(WikiViewAsConstants.CookieName, new CookieOptions
        {
            Path = WikiViewAsConstants.CookiePath,
        });
    }
}
