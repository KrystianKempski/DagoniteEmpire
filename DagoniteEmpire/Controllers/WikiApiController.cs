using System.Security.Claims;
using DA_Business.Services.Interfaces;
using DA_Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DagoniteEmpire.Controllers;

[ApiController]
[Route("api/wiki")]
[Authorize]
public class WikiApiController : ControllerBase
{
    private readonly IWikiViewAsService _viewAs;

    public WikiApiController(IWikiViewAsService viewAs)
    {
        _viewAs = viewAs;
    }

    [HttpPost("view-as")]
    [Authorize(Roles = SD.Role_Admin + "," + SD.Role_GameMaster)]
    public IActionResult SetViewAs([FromBody] WikiViewAsRequest? request)
    {
        if (string.IsNullOrWhiteSpace(request?.NpcName))
        {
            _viewAs.ClearViewAs();
            return Ok(new { active = false });
        }

        _viewAs.SetViewAs(request.NpcName.Trim());
        return Ok(new { active = true, npcName = request.NpcName.Trim() });
    }

    [HttpDelete("view-as")]
    [Authorize(Roles = SD.Role_Admin + "," + SD.Role_GameMaster)]
    public IActionResult ClearViewAs()
    {
        _viewAs.ClearViewAs();
        return Ok(new { active = false });
    }

    [HttpGet("view-as")]
    [Authorize(Roles = SD.Role_Admin + "," + SD.Role_GameMaster)]
    public IActionResult GetViewAs()
    {
        var name = _viewAs.GetViewAsCharacterName();
        return Ok(new { active = !string.IsNullOrWhiteSpace(name), npcName = name });
    }

    public sealed class WikiViewAsRequest
    {
        public string? NpcName { get; set; }
    }
}
