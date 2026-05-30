using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using DA_Scribe.Models;
using DA_Scribe.Services.Interfaces;
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
    public class ScribeController : ControllerBase
    {
        private readonly IScribeService _scribeService;
        private readonly ILogger<ScribeController> _logger;

        public ScribeController(IScribeService scribeService, ILogger<ScribeController> logger)
        {
            _scribeService = scribeService;
            _logger = logger;
        }

        /// <summary>
        /// Import pre-processed chunks from the Python extraction script
        /// </summary>
        /// <param name="campaignId">Campaign ID to associate imported content with</param>
        /// <param name="file">JSON file with ScribeImportData structure</param>
        [HttpPost("import/{campaignId}")]
        [RequestSizeLimit(100_000_000)] // 100MB limit for large imports
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
        public async Task<ActionResult<ScribeImportResult>> ImportBatchJson(
            int campaignId,
            [FromBody] ScribeImportData importData,
            CancellationToken cancellationToken)
        {
            if (importData?.Chunks == null || importData.Chunks.Count == 0)
            {
                return BadRequest("No chunks provided");
            }

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
                    "SCRIBE query: '{Query}' for campaign {CampaignId}, character {CharacterId}, user={User}, GM={IsGM}",
                    request.Query, request.CampaignId, request.CharacterId, userId, isGameMaster);

                var result = await _scribeService.QueryAsync(
                    request.Query,
                    userId,
                    request.CharacterId,
                    request.CampaignId,
                    conversationId: null,
                    isGameMaster: isGameMaster,
                    cancellationToken: cancellationToken);

                return Ok(result);
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
        private Task<Dictionary<string, int>> GetCharacterMappingAsync(
            int campaignId, 
            CancellationToken cancellationToken)
        {
            // TODO: Load from database - for now use hardcoded mapping for Kraina Możliwości
            var mapping = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
            {
                // Active characters - IDs should match database
                { "Udar", 1 },
                { "Tomin", 2 },
                { "Granit", 3 },
                { "Sir Cedrick", 4 },
                
                // Archived characters
                { "Sharu", 5 },
                { "Bjorn", 6 },
                { "Orion", 7 },
                { "Roolf", 8 },
            };
            
            return Task.FromResult(mapping);
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
        public async Task<ActionResult<IngestResult>> IngestChapterPosts(
            int chapterId,
            [FromQuery] bool reindex = false,
            CancellationToken cancellationToken = default)
        {
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
        public async Task<ActionResult<IngestResult>> IngestCampaignPosts(
            int campaignId,
            [FromQuery] bool reindex = false,
            CancellationToken cancellationToken = default)
        {
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