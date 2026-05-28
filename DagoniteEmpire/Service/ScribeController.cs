using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using DA_Scribe.Models;
using DA_Scribe.Services.Interfaces;
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

        /// <summary>
        /// TEST ONLY - Import without auth (remove in production)
        /// </summary>
        [HttpPost("test-import/{campaignId}")]
        [AllowAnonymous]
        public async Task<ActionResult<ScribeImportResult>> TestImport(
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
                _logger.LogInformation("TEST IMPORT: {ChunkCount} chunks for campaign {CampaignId}", 
                    importData.Chunks.Count, campaignId);
                    
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
                _logger.LogError(ex, "Test import failed");
                return StatusCode(500, new ScribeImportResult
                {
                    Success = false,
                    Message = $"Import failed: {ex.Message}"
                });
            }
        }
        
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
        [AllowAnonymous] // TODO: Change to proper auth for production
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
                _logger.LogInformation(
                    "SCRIBE query: '{Query}' for campaign {CampaignId}, character {CharacterId}, GM={IsGM}",
                    request.Query, request.CampaignId, request.CharacterId, request.IsGameMaster);

                var result = await _scribeService.QueryAsync(
                    request.Query,
                    request.UserId ?? "anonymous",
                    request.CharacterId,
                    request.CampaignId,
                    conversationId: null,
                    isGameMaster: request.IsGameMaster,
                    cancellationToken: cancellationToken);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Query failed");
                return StatusCode(500, new ScribeQueryResult
                {
                    Response = $"Błąd: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Search for similar chunks without generating response (for debugging)
        /// </summary>
        [HttpPost("search")]
        [AllowAnonymous]
        public async Task<ActionResult<IList<ScribeSearchResult>>> Search(
            [FromBody] ScribeQueryRequest request,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Query))
            {
                return BadRequest("Query is required");
            }

            var results = await _scribeService.SearchAsync(
                request.Query,
                request.UserId ?? "anonymous",
                request.CharacterId,
                request.CampaignId,
                request.TopK ?? 5,
                isGameMaster: request.IsGameMaster,
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
    }
}
