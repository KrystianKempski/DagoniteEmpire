using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using DA_Scribe.Models;
using DA_Scribe.Services.Interfaces;
using DA_DataAccess.Data;
using DagoniteEmpire.Service.Scribe;
using System.Text.Json;

namespace DagoniteEmpire.Service
{
    /// <summary>
    /// API controller for SCRIBE import and management operations
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "GameMaster,Admin")]
    [EnableRateLimiting("scribe-query")]
    public class ScribeController : ControllerBase
    {
        private readonly IScribeService _scribeService;
        private readonly IScribeAgentService _agentService;
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
        private readonly ILogger<ScribeController> _logger;

        public ScribeController(
            IScribeService scribeService,
            IScribeAgentService agentService,
            IDbContextFactory<ApplicationDbContext> contextFactory,
            ILogger<ScribeController> logger)
        {
            _scribeService = scribeService;
            _agentService = agentService;
            _contextFactory = contextFactory;
            _logger = logger;
        }

        private bool IsAdmin => User.IsInRole("Admin");
        private string CurrentUserName => User.Identity?.Name ?? string.Empty;

        /// <summary>
        /// Confirms that the current user owns the campaign (is its GM) or has the Admin role.
        /// Returns null when access is granted, or a ForbidResult/NotFoundResult to return otherwise.
        /// </summary>
        private async Task<ActionResult?> EnsureCampaignAccessAsync(int campaignId, CancellationToken cancellationToken)
        {
            if (IsAdmin)
                return null;

            await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
            var owner = await context.Campaigns
                .Where(c => c.Id == campaignId)
                .Select(c => (string?)c.GameMaster)
                .FirstOrDefaultAsync(cancellationToken);

            if (owner is null)
                return NotFound($"Campaign {campaignId} not found");

            if (!string.Equals(owner, CurrentUserName, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning(
                    "User {User} attempted to access campaign {CampaignId} owned by {Owner}",
                    CurrentUserName, campaignId, owner);
                return Forbid();
            }

            return null;
        }

        private async Task<ActionResult?> EnsureChapterAccessAsync(int chapterId, CancellationToken cancellationToken)
        {
            if (IsAdmin)
                return null;

            await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
            var record = await context.Chapters
                .Where(ch => ch.Id == chapterId)
                .Select(ch => new { ch.CampaignId, Owner = ch.Campaign != null ? ch.Campaign.GameMaster : null })
                .FirstOrDefaultAsync(cancellationToken);

            if (record is null)
                return NotFound($"Chapter {chapterId} not found");

            if (!string.Equals(record.Owner, CurrentUserName, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning(
                    "User {User} attempted to access chapter {ChapterId} from campaign {CampaignId} owned by {Owner}",
                    CurrentUserName, chapterId, record.CampaignId, record.Owner);
                return Forbid();
            }

            return null;
        }

        /// <summary>
        /// Import pre-processed chunks from the Python extraction script
        /// </summary>
        /// <param name="campaignId">Campaign ID to associate imported content with</param>
        /// <param name="file">JSON file with ScribeImportData structure</param>
        [HttpPost("import/{campaignId}")]
        [RequestSizeLimit(100_000_000)] // 100MB limit for large imports
        [EnableRateLimiting("scribe-ingest")]
        public async Task<ActionResult<ScribeImportResult>> ImportBatch(
            int campaignId,
            IFormFile file,
            CancellationToken cancellationToken)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file provided");
            }

            if (!file.FileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest("File must be a JSON file");
            }

            var access = await EnsureCampaignAccessAsync(campaignId, cancellationToken);
            if (access is not null) return access;

            try
            {
                _logger.LogInformation(
                    "Starting SCRIBE import for campaign {CampaignId} from file {FileName} ({Size} bytes)",
                    campaignId,
                    file.FileName,
                    file.Length);

                // Parse the JSON file
                using var stream = file.OpenReadStream();
                var importData = await JsonSerializer.DeserializeAsync<ScribeImportData>(
                    stream,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true },
                    cancellationToken);

                if (importData == null)
                {
                    return BadRequest("Invalid JSON structure");
                }

                _logger.LogInformation(
                    "Parsed import data: {ChunkCount} chunks from campaign '{Campaign}'",
                    importData.Chunks.Count,
                    importData.Metadata.Campaign);

                // Character name to ID mapping
                // TODO: Load from database based on campaign
                var characterNameToIdMap = await GetCharacterMappingAsync(campaignId, cancellationToken);

                // Perform the import
                var result = await _scribeService.ImportBatchAsync(
                    importData,
                    campaignId,
                    characterNameToIdMap,
                    cancellationToken);

                if (result.Success)
                {
                    return Ok(result);
                }
                else
                {
                    return StatusCode(207, result); // Partial success
                }
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to parse import JSON");
                return BadRequest($"Invalid JSON: {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Import failed");
                return StatusCode(500, new ScribeImportResult
                {
                    Success = false,
                    Message = $"Import failed: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Import pre-processed chunks from JSON body (for smaller imports or API clients)
        /// </summary>
        [HttpPost("import-json/{campaignId}")]
        [EnableRateLimiting("scribe-ingest")]
        public async Task<ActionResult<ScribeImportResult>> ImportBatchJson(
            int campaignId,
            [FromBody] ScribeImportData importData,
            CancellationToken cancellationToken)
        {
            if (importData?.Chunks == null || importData.Chunks.Count == 0)
            {
                return BadRequest("No chunks provided");
            }

            var access = await EnsureCampaignAccessAsync(campaignId, cancellationToken);
            if (access is not null) return access;

            try
            {
                var characterNameToIdMap = await GetCharacterMappingAsync(campaignId, cancellationToken);
                
                var result = await _scribeService.ImportBatchAsync(
                    importData,
                    campaignId,
                    characterNameToIdMap,
                    cancellationToken);

                return result.Success ? Ok(result) : StatusCode(207, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Import failed");
                return StatusCode(500, new ScribeImportResult
                {
                    Success = false,
                    Message = $"Import failed: {ex.Message}"
                });
            }
        }

        // TEST ENDPOINT REMOVED - use /import/{campaignId} with proper authentication
        
        /// <summary>
        /// Check if SCRIBE services (Ollama) are available
        /// </summary>
        [HttpGet("status")]
        [AllowAnonymous]
        public async Task<ActionResult<object>> GetStatus(CancellationToken cancellationToken)
        {
            var isAvailable = await _scribeService.IsAvailableAsync(cancellationToken);
            
            return Ok(new
            {
                Available = isAvailable,
                CheckedAt = DateTime.UtcNow
            });
        }

        /// <summary>
        /// Query SCRIBE with a question (RAG pipeline)
        /// </summary>
        [HttpPost("query")]
        [Authorize] // Any authenticated user can query
        public async Task<ActionResult<ScribeQueryResult>> Query(
            [FromBody] ScribeQueryRequest request,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Query))
            {
                return BadRequest("Query is required");
            }

            try
            {
                // Server-side validation of GameMaster status - don't trust client
                var isGameMaster = User.IsInRole("GameMaster") || User.IsInRole("Admin");
                var userId = User.Identity?.Name ?? "anonymous";

                _logger.LogInformation(
                    "SCRIBE query (agent): '{Query}' for campaign {CampaignId}, character {CharacterId}, user={User}, GM={IsGM}",
                    request.Query, request.CampaignId, request.CharacterId, userId, isGameMaster);

                var agentResult = await _agentService.InvokeAsync(new Scribe.ScribeAgentRequest
                {
                    Question = request.Query,
                    UserId = userId,
                    CharacterId = request.CharacterId,
                    CampaignId = request.CampaignId,
                    IsGameMaster = isGameMaster,
                }, cancellationToken);

                // Map agent result to the legacy ScribeQueryResult shape so the existing UI keeps working
                return Ok(new ScribeQueryResult
                {
                    Response = agentResult.Response,
                    GenerationTimeMs = agentResult.GenerationTimeMs,
                    ModelUsed = agentResult.ModelUsed,
                    ConversationId = agentResult.ConversationId,
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Query failed for user {User}", User.Identity?.Name);
                return StatusCode(500, new ScribeQueryResult
                {
                    Response = "Wystąpił błąd podczas przetwarzania zapytania."
                });
            }
        }

        /// <summary>
        /// Search for similar chunks without generating response (for debugging/GM tools)
        /// </summary>
        [HttpPost("search")]
        [Authorize(Roles = "GameMaster,Admin")] // Only GM/Admin can use raw search
        public async Task<ActionResult<IList<ScribeSearchResult>>> Search(
            [FromBody] ScribeQueryRequest request,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Query))
            {
                return BadRequest("Query is required");
            }

            // Server-side: user is guaranteed to be GM/Admin due to [Authorize] attribute
            var isGameMaster = true;
            var userId = User.Identity?.Name ?? "anonymous";

            var results = await _scribeService.SearchAsync(
                request.Query,
                userId,
                request.CharacterId,
                request.CampaignId,
                request.TopK ?? 5,
                isGameMaster: isGameMaster,
                cancellationToken);

            return Ok(results);
        }

        /// <summary>
        /// Get character name to ID mapping for a campaign
        /// </summary>
        /// <summary>
        /// Builds a name -> id lookup for every character bound to the campaign, using both the
        /// player-facing UserName (player characters) and NPCName (when the character is an NPC).
        /// Case-insensitive. Replaces the previous hardcoded mapping.
        /// </summary>
        private async Task<Dictionary<string, int>> GetCharacterMappingAsync(
            int campaignId,
            CancellationToken cancellationToken)
        {
            await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

            var characters = await context.Campaigns
                .Where(c => c.Id == campaignId)
                .SelectMany(c => c.Characters)
                .Select(ch => new { ch.Id, ch.UserName, ch.NPCName })
                .ToListAsync(cancellationToken);

            var mapping = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (var ch in characters)
            {
                if (!string.IsNullOrWhiteSpace(ch.UserName))
                    mapping[ch.UserName] = ch.Id;
                if (!string.IsNullOrWhiteSpace(ch.NPCName))
                    mapping[ch.NPCName] = ch.Id;
            }

            _logger.LogDebug(
                "Resolved {Count} character name aliases for campaign {CampaignId}",
                mapping.Count, campaignId);

            return mapping;
        }
        
        // ==========================================
        // Post Ingestion Endpoints
        // ==========================================
        
        /// <summary>
        /// Ingest all posts from a chapter into SCRIBE
        /// </summary>
        /// <param name="chapterId">Chapter ID to ingest posts from</param>
        /// <param name="reindex">If true, re-process posts that were already indexed</param>
        [HttpPost("ingest/chapter/{chapterId}")]
        [EnableRateLimiting("scribe-ingest")]
        public async Task<ActionResult<IngestResult>> IngestChapterPosts(
            int chapterId,
            [FromQuery] bool reindex = false,
            CancellationToken cancellationToken = default)
        {
            var access = await EnsureChapterAccessAsync(chapterId, cancellationToken);
            if (access is not null) return access;

            try
            {
                _logger.LogInformation(
                    "Starting post ingestion for chapter {ChapterId}, reindex={Reindex}",
                    chapterId, reindex);
                
                var count = await _scribeService.IngestChapterPostsAsync(chapterId, reindex, cancellationToken);
                
                return Ok(new IngestResult
                {
                    Success = true,
                    PostsIngested = count,
                    Message = $"Zindeksowano {count} postów z rozdziału."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to ingest posts from chapter {ChapterId}", chapterId);
                return StatusCode(500, new IngestResult
                {
                    Success = false,
                    Message = $"Błąd podczas indeksowania: {ex.Message}"
                });
            }
        }
        
        /// <summary>
        /// Ingest all posts from all chapters of a campaign into SCRIBE
        /// </summary>
        /// <param name="campaignId">Campaign ID to ingest posts from</param>
        /// <param name="reindex">If true, re-process posts that were already indexed</param>
        [HttpPost("ingest/campaign/{campaignId}")]
        [EnableRateLimiting("scribe-ingest")]
        public async Task<ActionResult<IngestResult>> IngestCampaignPosts(
            int campaignId,
            [FromQuery] bool reindex = false,
            CancellationToken cancellationToken = default)
        {
            var access = await EnsureCampaignAccessAsync(campaignId, cancellationToken);
            if (access is not null) return access;

            try
            {
                _logger.LogInformation(
                    "Starting post ingestion for campaign {CampaignId}, reindex={Reindex}",
                    campaignId, reindex);
                
                var count = await _scribeService.IngestCampaignPostsAsync(campaignId, reindex, cancellationToken);
                
                return Ok(new IngestResult
                {
                    Success = true,
                    PostsIngested = count,
                    Message = $"Zindeksowano {count} postów z kampanii."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to ingest posts from campaign {CampaignId}", campaignId);
                return StatusCode(500, new IngestResult
                {
                    Success = false,
                    Message = $"Błąd podczas indeksowania: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Agentic query - lets the LLM autonomously call tools (memory search, character lookup, chapter listing)
        /// to answer questions. Slower than /query but richer reasoning.
        /// </summary>
        [HttpPost("agent/query")]
        [Authorize]
        public async Task<ActionResult<ScribeAgentResult>> AgentQuery(
            [FromBody] ScribeQueryRequest request,
            [FromServices] IScribeAgentService agent,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Query))
                return BadRequest("Query is required");

            var isGameMaster = User.IsInRole("GameMaster") || User.IsInRole("Admin");
            var userId = User.Identity?.Name ?? "anonymous";

            try
            {
                var result = await agent.InvokeAsync(new ScribeAgentRequest
                {
                    Question = request.Query,
                    UserId = userId,
                    CharacterId = request.CharacterId,
                    CampaignId = request.CampaignId,
                    IsGameMaster = isGameMaster,
                }, cancellationToken);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Agent query failed for user {User}", userId);
                return StatusCode(500, new ScribeAgentResult
                {
                    Response = "Wystąpił błąd podczas przetwarzania zapytania przez agenta."
                });
            }
        }
    }
    
    /// <summary>
    /// Result of post ingestion operation
    /// </summary>
    public class IngestResult
    {
        public bool Success { get; set; }
        public int PostsIngested { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}