using DA_Scribe.Configuration;
using DA_DataAccess.Chat;
using DA_DataAccess.Data;
using DA_DataAccess.Scribe;
using DA_Scribe.Diagnostics;
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
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IChunkService _chunkService;
        private readonly IDocumentParserService _documentParser;
        private readonly ILogger<ScribeService> _logger;
        private readonly ScribeOptions _options;

        public ScribeService(
            IDbContextFactory<ApplicationDbContext> contextFactory,
            IEmbeddingService embeddingService,
            IHttpClientFactory httpClientFactory,
            IChunkService chunkService,
            IDocumentParserService documentParser,
            ILogger<ScribeService> logger,
            IOptions<ScribeOptions> options)
        {
            _contextFactory = contextFactory;
            _embeddingService = embeddingService;
            _httpClientFactory = httpClientFactory;
            _chunkService = chunkService;
            _documentParser = documentParser;
            _logger = logger;
            _options = options.Value;
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
            using var activity = ScribeTelemetry.ActivitySource.StartActivity("scribe.search");
            activity?.SetTag("scribe.search.character_id", characterId);
            activity?.SetTag("scribe.search.campaign_id", campaignId);
            activity?.SetTag("scribe.search.top_k", topK);
            activity?.SetTag("scribe.search.is_gm", isGameMaster);
            activity?.SetTag("scribe.search.query_length", query?.Length ?? 0);

            var queryEmbedding = await _embeddingService.GetEmbeddingAsync(query!, cancellationToken);
            var results = await SearchInternalAsync(queryEmbedding, userId, characterId, campaignId, topK, isGameMaster, cancellationToken);
            activity?.SetTag("scribe.search.result_count", results.Count);
            return results;
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

            await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
            context.ScribeMemories.Add(memory);
            await context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Document {FileName} ingested: memory {MemoryId} with {ChunkCount} chunks",
                fileName,
                memory.Id,
                scribeChunks.Count);

            return memory.Id;
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

            await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
            context.ScribeMemories.Add(memory);
            await context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Content '{Title}' ingested: memory {MemoryId} with {ChunkCount} chunks",
                title,
                memory.Id,
                scribeChunks.Count);

            return memory.Id;
        }

        public async Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var embeddingAvailable = await _embeddingService.IsAvailableAsync(cancellationToken);
                var llmAvailable = await IsChatModelAvailableAsync(cancellationToken);

                return embeddingAvailable && llmAvailable;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "SCRIBE availability check failed");
                return false;
            }
        }

        private async Task<bool> IsChatModelAvailableAsync(CancellationToken ct)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("scribe-ollama-health");
                client.BaseAddress ??= new Uri(_options.Ollama.BaseUrl);
                using var response = await client.GetAsync("/api/tags", ct);
                if (!response.IsSuccessStatusCode) return false;
                var content = await response.Content.ReadAsStringAsync(ct);
                return content.Contains(_options.Ollama.ChatModel, StringComparison.OrdinalIgnoreCase);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Ollama chat model availability check failed");
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
            string userId,
            CancellationToken cancellationToken = default)
        {
            await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
            
            return await context.ScribeConversations
                .Include(c => c.Messages.OrderBy(m => m.Timestamp))
                .FirstOrDefaultAsync(c => c.Id == conversationId && c.UserId == userId, cancellationToken);
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
            string userId,
            CancellationToken cancellationToken = default)
        {
            await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
            
            var conversation = await context.ScribeConversations
                .Include(c => c.Messages)
                .FirstOrDefaultAsync(c => c.Id == conversationId && c.UserId == userId, cancellationToken);
            
            if (conversation != null)
            {
                context.ScribeConversations.Remove(conversation);
                await context.SaveChangesAsync(cancellationToken);
                
                _logger.LogInformation("Deleted conversation {ConversationId} for user {UserId}", conversationId, userId);
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
        
        // ==========================================
        // Post Ingestion (Chapter Threads)
        // ==========================================
        
        public async Task<int> IngestChapterPostsAsync(
            int chapterId,
            bool reindexExisting = false,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Ingesting posts from chapter {ChapterId}, reindex={Reindex}", chapterId, reindexExisting);
            
            await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
            
            // Load chapter with posts and character info
            var chapter = await context.Chapters
                .Include(c => c.Posts)
                    .ThenInclude(p => p.Character)
                .Include(c => c.Characters)
                .Include(c => c.Campaign)
                .FirstOrDefaultAsync(c => c.Id == chapterId, cancellationToken);
            
            if (chapter == null)
            {
                _logger.LogWarning("Chapter {ChapterId} not found", chapterId);
                return 0;
            }
            
            // Get already indexed post IDs
            var indexedPostIds = reindexExisting 
                ? new HashSet<int>()
                : (await context.ScribeMemories
                    .Where(m => m.SourceChapterId == chapterId && m.SourcePostId != null && m.Type == MemoryType.Post)
                    .Select(m => m.SourcePostId!.Value)
                    .ToListAsync(cancellationToken))
                    .ToHashSet();
            
            // If reindexing, delete old memories for this chapter's posts
            if (reindexExisting)
            {
                var oldMemories = await context.ScribeMemories
                    .Include(m => m.Chunks)
                    .Where(m => m.SourceChapterId == chapterId && m.Type == MemoryType.Post)
                    .ToListAsync(cancellationToken);
                
                if (oldMemories.Any())
                {
                    context.ScribeMemories.RemoveRange(oldMemories);
                    await context.SaveChangesAsync(cancellationToken);
                    _logger.LogInformation("Deleted {Count} old post memories for chapter {ChapterId}", oldMemories.Count, chapterId);
                }
            }
            
            int ingestedCount = 0;
            var characterIdsInChapter = chapter.Characters.Select(c => c.Id).ToList();

            var orderedPosts = chapter.Posts.OrderBy(p => p.CreatedDate).ToList();
            var batchSize = Math.Clamp(_options.Ingest.BatchSize, 1, 500);
            var batchDelayMs = Math.Clamp(_options.Ingest.BatchDelayMs, 0, 60_000);
            var processedInBatch = 0;

            // Process posts in chronological order
            foreach (var post in orderedPosts)
            {
                cancellationToken.ThrowIfCancellationRequested();
                
                // Skip already indexed posts
                if (indexedPostIds.Contains(post.Id))
                    continue;
                
                // Skip very short posts (less than 50 chars of plain text)
                var plainText = StripHtml(post.Content);
                if (plainText.Length < 50)
                {
                    _logger.LogDebug("Skipping short post {PostId} ({Length} chars)", post.Id, plainText.Length);
                    continue;
                }
                
                try
                {
                    await IngestPostInternalAsync(
                        post, 
                        chapter, 
                        characterIdsInChapter, 
                        context, 
                        cancellationToken);
                    
                    ingestedCount++;
                    processedInBatch++;

                    if (processedInBatch >= batchSize)
                    {
                        _logger.LogInformation(
                            "Ingest progress: chapter {ChapterId} {Done}/{Total} posts done",
                            chapterId, ingestedCount, orderedPosts.Count);
                        processedInBatch = 0;
                        if (batchDelayMs > 0)
                            await Task.Delay(batchDelayMs, cancellationToken);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to ingest post {PostId}", post.Id);
                }
            }
            
            _logger.LogInformation(
                "Ingested {Count} posts from chapter {ChapterId} '{ChapterName}'",
                ingestedCount, chapterId, chapter.Name);
            
            return ingestedCount;
        }
        
        public async Task<int> IngestCampaignPostsAsync(
            int campaignId,
            bool reindexExisting = false,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Ingesting posts from campaign {CampaignId}, reindex={Reindex}", campaignId, reindexExisting);
            
            await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
            
            var chapterIds = await context.Chapters
                .Where(c => c.CampaignId == campaignId)
                .Select(c => c.Id)
                .ToListAsync(cancellationToken);
            
            int totalIngested = 0;
            var interChapterDelayMs = Math.Clamp(_options.Ingest.InterChapterDelayMs, 0, 60_000);

            for (int i = 0; i < chapterIds.Count; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var chapterId = chapterIds[i];
                totalIngested += await IngestChapterPostsAsync(chapterId, reindexExisting, cancellationToken);

                if (interChapterDelayMs > 0 && i < chapterIds.Count - 1)
                    await Task.Delay(interChapterDelayMs, cancellationToken);
            }
            
            _logger.LogInformation(
                "Ingested {Count} posts from {ChapterCount} chapters in campaign {CampaignId}",
                totalIngested, chapterIds.Count, campaignId);
            
            return totalIngested;
        }
        
        public async Task<int?> IngestPostAsync(
            int postId,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Ingesting single post {PostId}", postId);
            
            await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
            
            // Check if already indexed
            var exists = await context.ScribeMemories
                .AnyAsync(m => m.SourcePostId == postId && m.Type == MemoryType.Post, cancellationToken);
            
            if (exists)
            {
                _logger.LogDebug("Post {PostId} already indexed, skipping", postId);
                return null;
            }
            
            // Load post with all needed navigation properties
            var post = await context.Posts
                .Include(p => p.Character)
                .Include(p => p.Chapter)
                    .ThenInclude(c => c!.Characters)
                .Include(p => p.Chapter)
                    .ThenInclude(c => c!.Campaign)
                .FirstOrDefaultAsync(p => p.Id == postId, cancellationToken);
            
            if (post?.Chapter == null)
            {
                _logger.LogWarning("Post {PostId} or its chapter not found", postId);
                return null;
            }
            
            // Skip very short posts
            var plainText = StripHtml(post.Content);
            if (plainText.Length < 50)
            {
                _logger.LogDebug("Post {PostId} too short ({Length} chars), skipping", postId, plainText.Length);
                return null;
            }
            
            var characterIdsInChapter = post.Chapter.Characters.Select(c => c.Id).ToList();
            
            var memoryId = await IngestPostInternalAsync(
                post, 
                post.Chapter, 
                characterIdsInChapter, 
                context, 
                cancellationToken);
            
            _logger.LogInformation("Ingested post {PostId} as memory {MemoryId}", postId, memoryId);
            return memoryId;
        }
        
        public async Task<bool> IsPostIndexedAsync(
            int postId,
            CancellationToken cancellationToken = default)
        {
            await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
            
            return await context.ScribeMemories
                .AnyAsync(m => m.SourcePostId == postId && m.Type == MemoryType.Post, cancellationToken);
        }

        public async Task RemovePostAsync(
            int postId,
            CancellationToken cancellationToken = default)
        {
            await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

            var memories = await context.ScribeMemories
                .Include(m => m.Chunks)
                .Where(m => m.SourcePostId == postId && m.Type == MemoryType.Post)
                .ToListAsync(cancellationToken);

            if (memories.Count == 0)
                return;

            context.ScribeMemories.RemoveRange(memories);
            await context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Removed {Count} memory record(s) for post {PostId} from SCRIBE", memories.Count, postId);
        }

        public async Task<int?> ReindexPostAsync(
            int postId,
            CancellationToken cancellationToken = default)
        {
            await RemovePostAsync(postId, cancellationToken);
            return await IngestPostAsync(postId, cancellationToken);
        }
        
        /// <summary>
        /// Internal method to ingest a single post
        /// </summary>
        private async Task<int> IngestPostInternalAsync(
            DA_DataAccess.Chat.Post post,
            DA_DataAccess.Chat.Chapter chapter,
            IList<int> characterIdsInChapter,
            ApplicationDbContext context,
            CancellationToken cancellationToken)
        {
            var plainText = StripHtml(post.Content);
            var characterName = post.AlternativeName ?? post.Character?.NPCName ?? "Narrator";
            var isNpc = post.AlternativeName != null || (post.Character?.NPCType != DA_Common.SD.NPCType.PC);
            
            // Build title
            var title = $"{characterName}: {(plainText.Length > 80 ? plainText[..80] + "..." : plainText)}";
            
            // Determine which characters have access to this post
            // All characters in the chapter at the time can see it
            var accessCharacterIds = characterIdsInChapter.ToList();
            
            // The posting character definitely has access
            if (post.CharacterId > 0 && !accessCharacterIds.Contains(post.CharacterId))
            {
                accessCharacterIds.Add(post.CharacterId);
            }
            
            // Build context-rich content for embedding
            var contentForEmbedding = BuildPostContentForEmbedding(post, chapter, characterName);
            
            // Create memory
            var memory = new ScribeMemory
            {
                Title = title,
                Content = contentForEmbedding,
                Type = MemoryType.Post,
                SourcePostId = post.Id,
                SourceChapterId = chapter.Id,
                SourceCampaignId = chapter.CampaignId,
                CharacterIds = accessCharacterIds,
                IsPublic = false, // Posts are only visible to characters in the chapter
                IsGmOnly = false,
                CreatedAt = DateTime.UtcNow
            };
            
            context.ScribeMemories.Add(memory);
            await context.SaveChangesAsync(cancellationToken);
            
            // Chunk and embed
            var chunks = _chunkService.ChunkText(contentForEmbedding, maxTokens: 400, overlapTokens: 50);
            var scribeChunks = new List<ScribeChunk>();
            
            for (int i = 0; i < chunks.Count; i++)
            {
                var chunkText = chunks[i];
                
                try
                {
                    var embedding = await _embeddingService.GetEmbeddingAsync(chunkText, cancellationToken);
                    
                    var scribeChunk = new ScribeChunk
                    {
                        ScribeMemoryId = memory.Id,
                        Content = chunkText,
                        Embedding = new Pgvector.Vector(embedding),
                        ChunkIndex = i,
                        TokenCount = chunkText.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length * 4 / 3,
                        CampaignId = chapter.CampaignId,
                        ChapterId = chapter.Id,
                        MemoryType = MemoryType.Post,
                        IsPublic = false,
                        IsGmOnly = false,
                        CharacterIds = accessCharacterIds
                    };
                    
                    scribeChunks.Add(scribeChunk);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to embed chunk {Index} of post {PostId}", i, post.Id);
                }
            }
            
            if (scribeChunks.Any())
            {
                context.ScribeChunks.AddRange(scribeChunks);
                await context.SaveChangesAsync(cancellationToken);
            }
            
            return memory.Id;
        }
        
        /// <summary>
        /// Build content for embedding with context about the post
        /// </summary>
        private static string BuildPostContentForEmbedding(
            DA_DataAccess.Chat.Post post,
            DA_DataAccess.Chat.Chapter chapter,
            string characterName)
        {
            var sb = new System.Text.StringBuilder();
            
            // Add context header
            sb.AppendLine($"[Kampania: {chapter.Campaign?.Name ?? "Nieznana"}]");
            sb.AppendLine($"[Rozdział: {chapter.Name}]");
            
            if (!string.IsNullOrEmpty(chapter.Place))
                sb.AppendLine($"[Miejsce: {chapter.Place}]");
            
            if (!string.IsNullOrEmpty(chapter.DayTime))
                sb.AppendLine($"[Czas: {chapter.DayTime}]");
            
            sb.AppendLine($"[Postać: {characterName}]");
            sb.AppendLine($"[Data: {post.CreatedDate:yyyy-MM-dd HH:mm}]");
            sb.AppendLine();
            
            // Add the actual content
            sb.AppendLine(StripHtml(post.Content));
            
            return sb.ToString();
        }
        
        /// <summary>
        /// Strip HTML tags from content
        /// </summary>
        private static string StripHtml(string html)
        {
            if (string.IsNullOrEmpty(html))
                return string.Empty;
            
            // Remove script and style blocks completely
            var result = System.Text.RegularExpressions.Regex.Replace(
                html, 
                @"<(script|style)[^>]*>[\s\S]*?</\1>", 
                string.Empty, 
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            
            // Replace <br> and <p> with newlines
            result = System.Text.RegularExpressions.Regex.Replace(
                result, 
                @"<br\s*/?>|</p>|</div>", 
                "\n", 
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            
            // Remove all remaining HTML tags
            result = System.Text.RegularExpressions.Regex.Replace(result, @"<[^>]+>", string.Empty);
            
            // Decode HTML entities
            result = System.Net.WebUtility.HtmlDecode(result);
            
            // Normalize whitespace
            result = System.Text.RegularExpressions.Regex.Replace(result, @"\s+", " ");
            result = System.Text.RegularExpressions.Regex.Replace(result, @"\n\s*\n", "\n\n");
            
            return result.Trim();
        }
    }
}
