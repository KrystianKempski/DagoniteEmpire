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
            bool isGameMaster = false,
            CancellationToken cancellationToken = default)
        {
            var startTime = DateTime.UtcNow;
            _logger.LogInformation("SCRIBE query from user {UserId}, character {CharacterId}, GM={IsGM}: {Query}", 
                userId, characterId, isGameMaster, query);

            try
            {
                // Step 0: Create or get conversation
                int activeConversationId;
                if (conversationId.HasValue)
                {
                    activeConversationId = conversationId.Value;
                }
                else
                {
                    var newConversation = await CreateConversationAsync(
                        userId, campaignId, characterId, null, cancellationToken);
                    activeConversationId = newConversation.Id;
                }
                
                // Save user message
                await SaveMessageAsync(activeConversationId, "user", query, cancellationToken: cancellationToken);
                
                // Step 1: Generate embedding for query
                var queryEmbedding = await _embeddingService.GetEmbeddingAsync(query, cancellationToken);

                // Step 2: Search for relevant chunks with access control
                var searchResults = await SearchInternalAsync(
                    queryEmbedding, 
                    userId, 
                    characterId, 
                    campaignId, 
                    _options.Search.TopK,
                    isGameMaster,
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
                
                // Save assistant response
                var chunkIds = searchResults.Select(r => r.Chunk.Id).ToList();
                await SaveMessageAsync(
                    activeConversationId, 
                    "assistant", 
                    response, 
                    chunkIds, 
                    _llmService.ModelName, 
                    duration, 
                    cancellationToken);

                return new ScribeQueryResult
                {
                    Response = response,
                    Sources = searchResults,
                    GenerationTimeMs = duration,
                    ModelUsed = _llmService.ModelName,
                    AccessRestricted = accessRestricted,
                    AccessMessage = accessMessage,
                    ConversationId = activeConversationId
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
            bool isGameMaster = false,
            [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var startTime = DateTime.UtcNow;
            
            // Create or get conversation
            int activeConversationId;
            if (conversationId.HasValue)
            {
                activeConversationId = conversationId.Value;
            }
            else
            {
                var newConversation = await CreateConversationAsync(
                    userId, campaignId, characterId, null, cancellationToken);
                activeConversationId = newConversation.Id;
            }
            
            // Save user message
            await SaveMessageAsync(activeConversationId, "user", query, cancellationToken: cancellationToken);
            
            // Get embedding and search
            var queryEmbedding = await _embeddingService.GetEmbeddingAsync(query, cancellationToken);
            var searchResults = await SearchInternalAsync(
                queryEmbedding, userId, characterId, campaignId, _options.Search.TopK, isGameMaster, cancellationToken);

            var contextChunks = searchResults.Select(r => r.Chunk.Content).ToList();
            
            // Collect full response for saving
            var responseBuilder = new System.Text.StringBuilder();

            // Stream response
            await foreach (var token in _llmService.GenerateResponseStreamAsync(
                query, contextChunks, cancellationToken: cancellationToken))
            {
                responseBuilder.Append(token);
                yield return token;
            }
            
            // Save assistant response after streaming completes
            var duration = (int)(DateTime.UtcNow - startTime).TotalMilliseconds;
            var chunkIds = searchResults.Select(r => r.Chunk.Id).ToList();
            await SaveMessageAsync(
                activeConversationId, 
                "assistant", 
                responseBuilder.ToString(), 
                chunkIds, 
                _llmService.ModelName, 
                duration, 
                cancellationToken);
        }

        public async Task<IList<ScribeSearchResult>> SearchAsync(
            string query,
            string userId,
            int? characterId = null,
            int? campaignId = null,
            int topK = 5,
            bool isGameMaster = false,
            CancellationToken cancellationToken = default)
        {
            var queryEmbedding = await _embeddingService.GetEmbeddingAsync(query, cancellationToken);
            return await SearchInternalAsync(queryEmbedding, userId, characterId, campaignId, topK, isGameMaster, cancellationToken);
        }

        private async Task<IList<ScribeSearchResult>> SearchInternalAsync(
            float[] queryEmbedding,
            string userId,
            int? characterId,
            int? campaignId,
            int topK,
            bool isGameMaster,
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
            
            // Access control logic:
            // 1. GM sees everything (including IsGmOnly)
            // 2. Players see: Public content + content where their character was present
            // 3. Character must be in the campaign to see campaign-specific content
            
            if (isGameMaster)
            {
                // GM has full access - no filtering needed
                _logger.LogDebug("GM access granted - showing all content");
            }
            else if (characterId.HasValue)
            {
                // Check if character is in the requested campaign
                bool characterInCampaign = true;
                if (campaignId.HasValue)
                {
                    characterInCampaign = await context.Characters
                        .Where(c => c.Id == characterId.Value)
                        .SelectMany(c => c.Campaigns!)
                        .AnyAsync(camp => camp.Id == campaignId.Value, cancellationToken);
                    
                    if (!characterInCampaign)
                    {
                        _logger.LogWarning(
                            "Character {CharacterId} not in campaign {CampaignId} - access restricted",
                            characterId, campaignId);
                        
                        // Character not in this campaign - only show public world knowledge
                        query = query.Where(c => c.IsPublic && !c.IsGmOnly);
                    }
                }
                
                if (characterInCampaign)
                {
                    // Character is in campaign - show public content + content they witnessed
                    var charIdStr = characterId.Value.ToString();
                    
                    // Filter: Public OR character was present, but never GM-only content
                    query = query.Where(c => 
                        !c.IsGmOnly && (
                            c.IsPublic || 
                            c.CharacterIdsJson == charIdStr ||
                            c.CharacterIdsJson!.StartsWith(charIdStr + ",") ||
                            c.CharacterIdsJson!.EndsWith("," + charIdStr) ||
                            c.CharacterIdsJson!.Contains("," + charIdStr + ",")));
                }
            }
            else
            {
                // No character context and not GM - only public content
                _logger.LogDebug("No character context - showing public content only");
                query = query.Where(c => c.IsPublic && !c.IsGmOnly);
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
                "Vector search found {Count} results for campaign {CampaignId}, character {CharacterId}, GM={IsGM}",
                results.Count, campaignId, characterId, isGameMaster);
            
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
        
        // ==========================================
        // Conversation History
        // ==========================================
        
        public async Task<IList<ScribeConversation>> GetConversationsAsync(
            string userId,
            int? campaignId = null,
            int limit = 20,
            CancellationToken cancellationToken = default)
        {
            await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
            
            var query = context.ScribeConversations
                .Where(c => c.UserId == userId)
                .AsQueryable();
            
            if (campaignId.HasValue)
            {
                query = query.Where(c => c.CampaignId == campaignId.Value);
            }
            
            return await query
                .OrderByDescending(c => c.LastMessageAt ?? c.StartedAt)
                .Take(limit)
                .ToListAsync(cancellationToken);
        }
        
        public async Task<ScribeConversation?> GetConversationAsync(
            int conversationId,
            CancellationToken cancellationToken = default)
        {
            await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
            
            return await context.ScribeConversations
                .Include(c => c.Messages.OrderBy(m => m.Timestamp))
                .FirstOrDefaultAsync(c => c.Id == conversationId, cancellationToken);
        }
        
        public async Task<ScribeConversation> CreateConversationAsync(
            string userId,
            int? campaignId = null,
            int? characterId = null,
            string? title = null,
            CancellationToken cancellationToken = default)
        {
            await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
            
            var conversation = new ScribeConversation
            {
                UserId = userId,
                CampaignId = campaignId,
                CharacterId = characterId,
                Title = title,
                StartedAt = DateTime.UtcNow
            };
            
            context.ScribeConversations.Add(conversation);
            await context.SaveChangesAsync(cancellationToken);
            
            _logger.LogInformation(
                "Created conversation {ConversationId} for user {UserId}, campaign {CampaignId}",
                conversation.Id, userId, campaignId);
            
            return conversation;
        }
        
        public async Task DeleteConversationAsync(
            int conversationId,
            CancellationToken cancellationToken = default)
        {
            await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
            
            var conversation = await context.ScribeConversations
                .Include(c => c.Messages)
                .FirstOrDefaultAsync(c => c.Id == conversationId, cancellationToken);
            
            if (conversation != null)
            {
                context.ScribeConversations.Remove(conversation);
                await context.SaveChangesAsync(cancellationToken);
                
                _logger.LogInformation("Deleted conversation {ConversationId}", conversationId);
            }
        }
        
        /// <summary>
        /// Save a message to conversation and update LastMessageAt
        /// </summary>
        private async Task SaveMessageAsync(
            int conversationId,
            string role,
            string content,
            IEnumerable<int>? chunkIds = null,
            string? modelUsed = null,
            int? generationTimeMs = null,
            CancellationToken cancellationToken = default)
        {
            await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
            
            var message = new ScribeMessage
            {
                ConversationId = conversationId,
                Role = role,
                Content = content,
                Timestamp = DateTime.UtcNow,
                SourceChunkIds = chunkIds != null ? string.Join(",", chunkIds) : null,
                ModelUsed = modelUsed,
                GenerationTimeMs = generationTimeMs
            };
            
            context.ScribeMessages.Add(message);
            
            // Update conversation's LastMessageAt
            var conversation = await context.ScribeConversations
                .FirstOrDefaultAsync(c => c.Id == conversationId, cancellationToken);
            
            if (conversation != null)
            {
                conversation.LastMessageAt = message.Timestamp;
                
                // Auto-generate title from first user message if not set
                if (string.IsNullOrEmpty(conversation.Title) && role == "user")
                {
                    conversation.Title = content.Length > 50 
                        ? content[..50] + "..." 
                        : content;
                }
            }
            
            await context.SaveChangesAsync(cancellationToken);
        }
    }
}
