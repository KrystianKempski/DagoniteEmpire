using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;

namespace DagoniteEmpire.Controllers;

// Sets the UI culture cookie and reloads. Blazor Server establishes culture per circuit,
// so switching language requires a full navigation rather than a live in-circuit change.
[Route("[controller]/[action]")]
public class CultureController : Controller
{
    [HttpGet]
    public IActionResult Set(string? culture, string? redirectUri)
    {
        if (!string.IsNullOrWhiteSpace(culture))
        {
            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1), Path = "/" });
        }

        return LocalRedirect(string.IsNullOrWhiteSpace(redirectUri) ? "/" : redirectUri);
    }
}
