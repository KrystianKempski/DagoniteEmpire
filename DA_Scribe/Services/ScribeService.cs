using DA_Scribe.Configuration;
using DA_Scribe.Entities;
using DA_Scribe.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DA_Scribe.Services
{
    /// <summary>
    /// Main orchestration service for SCRIBE - implements RAG pipeline
    /// </summary>
    public class ScribeService : IScribeService
    {
        private readonly IEmbeddingService _embeddingService;
        private readonly ILLMService _llmService;
        private readonly IChunkService _chunkService;
        private readonly IDocumentParserService _documentParser;
        private readonly ILogger<ScribeService> _logger;
        private readonly ScribeOptions _options;

        public ScribeService(
            IEmbeddingService embeddingService,
            ILLMService llmService,
            IChunkService chunkService,
            IDocumentParserService documentParser,
            ILogger<ScribeService> logger,
            IOptions<ScribeOptions> options)
        {
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

        private Task<IList<ScribeSearchResult>> SearchInternalAsync(
            float[] queryEmbedding,
            string userId,
            int? characterId,
            int? campaignId,
            int topK,
            CancellationToken cancellationToken)
        {
            // TODO: Implement actual vector search using pgvector
            // For now, return empty results
            _logger.LogDebug("Vector search not yet implemented - returning empty results");
            
            IList<ScribeSearchResult> results = new List<ScribeSearchResult>();
            return Task.FromResult(results);
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
    }
}
