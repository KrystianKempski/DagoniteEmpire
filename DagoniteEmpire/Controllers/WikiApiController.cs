using DA_Business.Services.Interfaces;
using DA_Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DagoniteEmpire.Controllers;

[ApiController]
[Route("api/wiki")]
public class WikiApiController : ControllerBase
{
    private readonly IWikiAccessService _wikiAccess;

    public WikiApiController(IWikiAccessService wikiAccess)
    {
        _wikiAccess = wikiAccess;
    }

    /// <summary>Used by wiki iframe to block navigation before Quartz shows a 404 page.</summary>
    [HttpGet("access")]
    public async Task<IActionResult> CheckAccess([FromQuery] string? slug)
    {
        if (string.IsNullOrWhiteSpace(slug))
        {
            return BadRequest(new { allowed = false, error = "slug required" });
        }

        slug = slug.Trim().TrimStart('/');

        if (_wikiAccess.IsAnonymousPublicPath(slug))
        {
            return Ok(new { allowed = true });
        }

        if (!(User.Identity?.IsAuthenticated ?? false))
        {
            return Ok(new { allowed = false });
        }

        var userName = User.Identity?.Name;
        var isAdminOrMg = User.IsInRole(SD.Role_Admin) || User.IsInRole(SD.Role_GameMaster);
        if (await _wikiAccess.ShouldBypassAccessChecksAsync(userName, isAdminOrMg))
        {
            return Ok(new { allowed = true, bypass = true });
        }

        var allowed = await _wikiAccess.CanAccessSlug(userName, isAdminOrMg, slug);
        return Ok(new { allowed });
    }
}
