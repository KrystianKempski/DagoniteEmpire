using System.Text;
using DA_Business.Services.Interfaces;
using DA_Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DagoniteEmpire.Service
{
    [ApiController]
    [Route("api/gm-panel")]
    [Authorize(Roles = SD.Role_Admin + "," + SD.Role_GameMaster)]
    public class GmPanelController : ControllerBase
    {
        private readonly ICampaignSummaryService _summaryService;

        public GmPanelController(ICampaignSummaryService summaryService)
        {
            _summaryService = summaryService;
        }

        [HttpGet("download")]
        public async Task<IActionResult> Download([FromQuery] int? chapterId, [FromQuery] int? campaignId)
        {
            if (chapterId is > 0)
            {
                var result = await _summaryService.GenerateChapterSummaryAsync(chapterId.Value);
                if (result is null)
                    return NotFound();

                return File(
                    Encoding.UTF8.GetBytes(result.Content),
                    "text/plain; charset=utf-8",
                    result.FileName);
            }

            if (campaignId is > 0)
            {
                var result = await _summaryService.GenerateCampaignSummaryAsync(campaignId.Value);
                if (result is null)
                    return NotFound();

                return File(
                    Encoding.UTF8.GetBytes(result.Content),
                    "text/plain; charset=utf-8",
                    result.FileName);
            }

            return BadRequest("Podaj chapterId lub campaignId.");
        }
    }
}
