using DA_Scribe.Configuration;
using DA_DataAccess.Data;
using DA_DataAccess.Scribe;
using DA_Scribe.Models;
using DA_Scribe.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Pgvector.EntityFrameworkCore;

namespace DA_Scribe.Services
{
    /// <summary>
    /// Main orchestration service for SCRIBE - implements RAG pipeline
    /// </summary>
    public class ScribeService : IScribeService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
        private readonly IEmbeddingService _embeddingService;
        private readonly ILLMService _llmService;
        private readonly IChunkService _chunkService;
        private readonly IDocumentParserService _documentParser;
        private readonly ILogger<ScribeService> _logger;
        private readonly ScribeOptions _options;

        public ScribeService(
            IDbContextFactory<ApplicationDbContext> contextFactory,
            IEmbeddingService embeddingService,
            ILLMService llmService,
            IChunkService chunkService,
            IDocumentParserService documentParser,
            ILogger<ScribeService> logger,
            IOptions<ScribeOptions> options)
        {
            _contextFactory = contextFactory;
            _embeddingService = embeddingService;
            _llmService = llmService;
            _chunkService = chunkService;
            _documentParser = documentParser;
            _logger = logger;
            _options = options.Value;
        }

        public async Task<ScribeQueryResult> QueryAsync(
            string query,
            string userId,
            int? characterId = null,
            int? campaignId = null,
            int? conversationId = null,
            CancellationToken cancellationToken = default)
        {
            var startTime = DateTime.UtcNow;
            _logger.LogInformation("SCRIBE query from user {UserId}: {Query}", userId, query);

            try
            {
                // Step 1: Generate embedding for query
                var queryEmbedding = await _embeddingService.GetEmbeddingAsync(query, cancellationToken);

                // Step 2: Search for relevant chunks (TODO: implement vector search)
                var searchResults = await SearchInternalAsync(
                    queryEmbedding, 
                    userId, 
                    characterId, 
                    campaignId, 
                    _options.Search.TopK,
                    cancellationToken);

                // Step 3: Check access restrictions
                var accessRestricted = false;
                string? accessMessage = null;
                
                if (!searchResults.Any())
                {
                    // No results found - could be access restriction or no data
                    accessRestricted = true;
                    accessMessage = "Nie znaleziono informacji na ten temat w archiwum.";
                }

                // Step 4: Build context from retrieved chunks
                var contextChunks = searchResults.Select(r => r.Chunk.Content).ToList();

                // Step 5: Generate response using LLM
                string response;
                if (contextChunks.Any())
                {
                    response = await _llmService.GenerateResponseAsync(
                        query, 
                        contextChunks, 
                        cancellationToken: cancellationToken);
                }
                else
                {
                    response = "Przepraszam, nie znalazłem informacji na ten temat w archiwum przygód. " +
                               "Możliwe, że te informacje nie zostały jeszcze zindeksowane lub Twoja postać nie była przy tym obecna.";
                }

                var duration = (int)(DateTime.UtcNow - startTime).TotalMilliseconds;

                return new ScribeQueryResult
                {
                    Response = response,
                    Sources = searchResults,
                    GenerationTimeMs = duration,
                    ModelUsed = _llmService.ModelName,
                    AccessRestricted = accessRestricted,
                    AccessMessage = accessMessage
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing SCRIBE query");
                
                return new ScribeQueryResult
                {
                    Response = "Przepraszam, wystąpił błąd podczas przetwarzania pytania. Spróbuj ponownie później.",
                    Sources = new List<ScribeSearchResult>(),
                    GenerationTimeMs = (int)(DateTime.UtcNow - startTime).TotalMilliseconds,
                    AccessRestricted = false
                };
            }
        }

        public async IAsyncEnumerable<string> QueryStreamAsync(
            string query,
            string userId,
            int? characterId = null,
            int? campaignId = null,
            int? conversationId = null,
            [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            // Get embedding and search
            var queryEmbedding = await _embeddingService.GetEmbeddingAsync(query, cancellationToken);
            var searchResults = await SearchInternalAsync(
                queryEmbedding, userId, characterId, campaignId, _options.Search.TopK, cancellationToken);

            var contextChunks = searchResults.Select(r => r.Chunk.Content).ToList();

            // Stream response
            await foreach (var token in _llmService.GenerateResponseStreamAsync(
                query, contextChunks, cancellationToken: cancellationToken))
            {
                yield return token;
            }
        }

        public async Task<IList<ScribeSearchResult>> SearchAsync(
            string query,
            string userId,
            int? characterId = null,
            int? campaignId = null,
            int topK = 5,
            CancellationToken cancellationToken = default)
        {
            var queryEmbedding = await _embeddingService.GetEmbeddingAsync(query, cancellationToken);
            return await SearchInternalAsync(queryEmbedding, userId, characterId, campaignId, topK, cancellationToken);
        }

        private async Task<IList<ScribeSearchResult>> SearchInternalAsync(
            float[] queryEmbedding,
            string userId,
            int? characterId,
            int? campaignId,
            int topK,
            CancellationToken cancellationToken)
        {
            await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
            
            var queryVector = new Pgvector.Vector(queryEmbedding);
            
            // Build query with vector similarity search
            var query = context.ScribeChunks
                .Include(c => c.ScribeMemory)
                .AsQueryable();
            
            // Filter by campaign if specified
            if (campaignId.HasValue)
            {
                query = query.Where(c => c.CampaignId == campaignId.Value);
            }
            
            // Access control: show chunks that are either:
            // 1. Public (world knowledge, rules)
            // 2. Character was present (their POV or they witnessed it)
            // 3. GM-only content for GM users (TODO: check user role)
            var charIdStr = characterId?.ToString() ?? "";
            if (characterId.HasValue)
            {
                // CharacterIdsJson can be: "3" or "1,2,3" - check for exact match or in list
                query = query.Where(c => 
                    c.IsPublic || 
                    c.CharacterIdsJson == charIdStr ||
                    c.CharacterIdsJson!.StartsWith(charIdStr + ",") ||
                    c.CharacterIdsJson!.EndsWith("," + charIdStr) ||
                    c.CharacterIdsJson!.Contains("," + charIdStr + ","));
            }
            else
            {
                // No character context - show all content (for GM/testing)
                // TODO: In production, check if user is GM
            }
            
            // Order by cosine distance (smaller = more similar) and take top K
            var results = await query
                .OrderBy(c => c.Embedding!.CosineDistance(queryVector))
                .Take(topK)
                .Select(c => new 
                {
                    Chunk = c,
                    Distance = c.Embedding!.CosineDistance(queryVector)
                })
                .ToListAsync(cancellationToken);
            
            _logger.LogDebug(
                "Vector search found {Count} results for campaign {CampaignId}, character {CharacterId}",
                results.Count, campaignId, characterId);
            
            // Convert to ScribeSearchResult with similarity score
            IList<ScribeSearchResult> searchResults = results
                .Select(r => new ScribeSearchResult
                {
                    Chunk = r.Chunk,
                    Similarity = (float)(1.0 - r.Distance), // Convert distance to similarity
                    Memory = r.Chunk.ScribeMemory
                })
                .ToList();
            
            return searchResults;
        }

        public async Task<int> IngestDocumentAsync(
            Stream stream,
            string fileName,
            int campaignId,
            IEnumerable<int>? characterIds = null,
            bool isPublic = false,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Ingesting document: {FileName} for campaign {CampaignId}", fileName, campaignId);

            // Step 1: Parse document
            var parsed = await _documentParser.ParseWordDocumentAsync(stream, fileName, cancellationToken);

            // Step 2: Create memory entry
            var memory = new ScribeMemory
            {
                Title = parsed.Title ?? Path.GetFileNameWithoutExtension(fileName),
                Content = parsed.Content,
                Type = MemoryType.Document,
                SourceCampaignId = campaignId,
                SourceDocumentName = fileName,
                IsPublic = isPublic,
                CharacterIds = characterIds ?? Enumerable.Empty<int>(),
                CreatedAt = DateTime.UtcNow
            };

            // Step 3: Chunk content
            var chunks = _chunkService.ChunkText(
                parsed.Content,
                _options.Chunking.MaxTokensPerChunk,
                _options.Chunking.OverlapTokens);

            _logger.LogInformation("Document {FileName} split into {ChunkCount} chunks", fileName, chunks.Count);

            // Step 4: Generate embeddings for each chunk
            var scribeChunks = new List<ScribeChunk>();
            for (int i = 0; i < chunks.Count; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                
                var embedding = await _embeddingService.GetEmbeddingAsync(chunks[i], cancellationToken);
                
                scribeChunks.Add(new ScribeChunk
                {
                    Content = chunks[i],
                    Embedding = new Pgvector.Vector(embedding),
                    ChunkIndex = i,
                    TokenCount = _chunkService.EstimateTokenCount(chunks[i]),
                    CampaignId = campaignId,
                    MemoryType = MemoryType.Document,
                    IsPublic = isPublic,
                    CharacterIds = characterIds ?? Enumerable.Empty<int>()
                });
            }

            memory.Chunks = scribeChunks;

            // TODO: Save to database
            _logger.LogInformation(
                "Document {FileName} ingested: {ChunkCount} chunks with embeddings (DB save pending)",
                fileName, 
                scribeChunks.Count);

            // Return placeholder ID (actual implementation would return DB-generated ID)
            return 0;
        }

        public async Task<int> IngestContentAsync(
            string title,
            string content,
            MemoryType type,
            int campaignId,
            IEnumerable<int>? characterIds = null,
            bool isPublic = false,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Ingesting content: {Title} for campaign {CampaignId}", title, campaignId);

            // Create memory
            var memory = new ScribeMemory
            {
                Title = title,
                Content = content,
                Type = type,
                SourceCampaignId = campaignId,
                IsPublic = isPublic,
                CharacterIds = characterIds ?? Enumerable.Empty<int>(),
                CreatedAt = DateTime.UtcNow
            };

            // Chunk and embed
            var chunks = _chunkService.ChunkText(content, _options.Chunking.MaxTokensPerChunk);
            
            var scribeChunks = new List<ScribeChunk>();
            for (int i = 0; i < chunks.Count; i++)
            {
                var embedding = await _embeddingService.GetEmbeddingAsync(chunks[i], cancellationToken);
                
                scribeChunks.Add(new ScribeChunk
                {
                    Content = chunks[i],
                    Embedding = new Pgvector.Vector(embedding),
                    ChunkIndex = i,
                    TokenCount = _chunkService.EstimateTokenCount(chunks[i]),
                    CampaignId = campaignId,
                    MemoryType = type,
                    IsPublic = isPublic,
                    CharacterIds = characterIds ?? Enumerable.Empty<int>()
                });
            }

            memory.Chunks = scribeChunks;

            // TODO: Save to database
            return 0;
        }

        public async Task<int> GenerateChapterSummaryAsync(
            int chapterId,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Generating summary for chapter {ChapterId}", chapterId);

            // TODO: Implement chapter summary generation
            // 1. Load chapter posts
            // 2. Concatenate content
            // 3. Generate summary using LLM
            // 4. Create memory entry
            // 5. Chunk and embed

            await Task.CompletedTask;
            return 0;
        }

        public async Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var embeddingAvailable = await _embeddingService.IsAvailableAsync(cancellationToken);
                var llmAvailable = await _llmService.IsAvailableAsync(cancellationToken);
                
                return embeddingAvailable && llmAvailable;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "SCRIBE availability check failed");
                return false;
            }
        }

        public async Task<ScribeImportResult> ImportBatchAsync(
            ScribeImportData importData,
            int campaignId,
            Dictionary<string, int>? characterNameToIdMap = null,
            CancellationToken cancellationToken = default)
        {
            var startTime = DateTime.UtcNow;
            var result = new ScribeImportResult();
            var errors = new List<string>();
            
            _logger.LogInformation(
                "Starting batch import for campaign {CampaignId}: {ChunkCount} chunks from {Campaign}",
                campaignId,
                importData.Chunks.Count,
                importData.Metadata.Campaign);

            // Group chunks by document (act) for creating memories
            var chunksByDocument = importData.Chunks
                .GroupBy(c => c.DocumentPath ?? c.ActNumber ?? "unknown")
                .ToList();
            
            _logger.LogInformation("Grouped into {DocumentCount} documents", chunksByDocument.Count);

            foreach (var docGroup in chunksByDocument)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    // Determine memory type from chunk types
                    var chunkTypes = docGroup.Select(c => c.ChunkType).Distinct().ToList();
                    var memoryType = DetermineMemoryType(chunkTypes);
                    
                    // Get all characters present in this document
                    var allCharacterNames = docGroup
                        .SelectMany(c => c.CharactersPresent)
                        .Distinct()
                        .ToList();
                    
                    // Map character names to IDs
                    var characterIds = MapCharacterNamesToIds(allCharacterNames, characterNameToIdMap);
                    
                    // Create a memory for this document
                    var documentTitle = GenerateDocumentTitle(docGroup.Key, docGroup.First());
                    var combinedContent = string.Join("\n\n", docGroup.Select(c => c.Content));
                    
                    var memory = new ScribeMemory
                    {
                        Title = documentTitle,
                        Content = combinedContent,
                        Type = memoryType,
                        SourceCampaignId = campaignId,
                        SourceDocumentName = docGroup.Key,
                        CharacterIds = characterIds,
                        IsPublic = memoryType == MemoryType.World || memoryType == MemoryType.Rules,
                        CreatedAt = DateTime.UtcNow
                    };

                    // Process each chunk - embed and create ScribeChunk
                    var scribeChunks = new List<ScribeChunk>();
                    int chunkIndex = 0;
                    
                    foreach (var chunk in docGroup)
                    {
                        try
                        {
                            // Generate embedding for the plain text content
                            var embedding = await _embeddingService.GetEmbeddingAsync(
                                chunk.ContentPlain, 
                                cancellationToken);
                            
                            // Map this chunk's characters
                            var chunkCharacterIds = MapCharacterNamesToIds(
                                chunk.CharactersPresent, 
                                characterNameToIdMap);
                            
                            var scribeChunk = new ScribeChunk
                            {
                                Content = chunk.Content, // Keep annotated content
                                Embedding = new Pgvector.Vector(embedding),
                                ChunkIndex = chunkIndex++,
                                TokenCount = chunk.WordCount * 4 / 3, // Rough estimate
                                CampaignId = campaignId,
                                MemoryType = memoryType,
                                IsPublic = memory.IsPublic,
                                CharacterIds = chunkCharacterIds,
                                // Store additional metadata in extended properties if needed
                            };
                            
                            scribeChunks.Add(scribeChunk);
                            result.ChunksImported++;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed to process chunk {ChunkId}", chunk.Id);
                            errors.Add($"Chunk {chunk.Id}: {ex.Message}");
                            result.ChunksFailed++;
                        }
                    }

                    memory.Chunks = scribeChunks;
                    result.MemoriesCreated++;
                    
                    // Save memory to database
                    await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
                    context.ScribeMemories.Add(memory);
                    await context.SaveChangesAsync(cancellationToken);
                    
                    _logger.LogDebug(
                        "Saved memory '{Title}' with {ChunkCount} chunks to database",
                        memory.Title,
                        scribeChunks.Count);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to process document {Document}", docGroup.Key);
                    errors.Add($"Document {docGroup.Key}: {ex.Message}");
                }
            }

            result.Success = result.ChunksFailed == 0 && errors.Count == 0;
            result.Duration = DateTime.UtcNow - startTime;
            result.Errors = errors;
            result.Message = $"Imported {result.ChunksImported} chunks into {result.MemoriesCreated} memories in {result.Duration.TotalSeconds:F1}s";

            _logger.LogInformation(
                "Batch import complete: {ChunksImported} imported, {ChunksFailed} failed, {MemoriesCreated} memories in {Duration}s",
                result.ChunksImported,
                result.ChunksFailed,
                result.MemoriesCreated,
                result.Duration.TotalSeconds);

            return result;
        }

        private static MemoryType DetermineMemoryType(IList<string> chunkTypes)
        {
            if (chunkTypes.Contains("world"))
                return MemoryType.World;
            if (chunkTypes.Contains("rules"))
                return MemoryType.Rules;
            if (chunkTypes.Contains("character"))
                return MemoryType.Character;
            if (chunkTypes.Contains("combat"))
                return MemoryType.Event;
            
            return MemoryType.Event;
        }

        private static List<int> MapCharacterNamesToIds(
            IEnumerable<string> characterNames,
            Dictionary<string, int>? nameToIdMap)
        {
            if (nameToIdMap == null || !characterNames.Any())
                return new List<int>();
            
            return characterNames
                .Where(name => nameToIdMap.ContainsKey(name))
                .Select(name => nameToIdMap[name])
                .Distinct()
                .ToList();
        }

        private static string GenerateDocumentTitle(string documentPath, ScribeImportChunk firstChunk)
        {
            // Try to generate a meaningful title
            if (!string.IsNullOrEmpty(firstChunk.SceneTitle))
                return firstChunk.SceneTitle;
            
            if (!string.IsNullOrEmpty(firstChunk.ActNumber))
            {
                var actPart = $"Akt {firstChunk.ActNumber}";
                
                // Extract title from document path
                var pathParts = documentPath.Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);
                var fileName = pathParts.LastOrDefault() ?? documentPath;
                
                // Remove act number prefix and extension
                var title = System.Text.RegularExpressions.Regex.Replace(
                    fileName, 
                    @"^[Aa]kt\s*\d+(?:\.\d+)?\s*", 
                    "");
                title = System.IO.Path.GetFileNameWithoutExtension(title);
                
                if (!string.IsNullOrWhiteSpace(title))
                    return $"{actPart}: {title}";
                
                return actPart;
            }
            
            return System.IO.Path.GetFileNameWithoutExtension(documentPath);
        }
    }
}
