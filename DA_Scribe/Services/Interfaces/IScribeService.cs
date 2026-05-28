using DA_DataAccess.Scribe;
using DA_Scribe.Models;

namespace DA_Scribe.Services.Interfaces
{
    /// <summary>
    /// Result of a SCRIBE search query
    /// </summary>
    public class ScribeSearchResult
    {
        /// <summary>
        /// The chunk that matched
        /// </summary>
        public required ScribeChunk Chunk { get; set; }
        
        /// <summary>
        /// Similarity score (0-1, higher is more similar)
        /// </summary>
        public float Similarity { get; set; }
        
        /// <summary>
        /// The parent memory
        /// </summary>
        public ScribeMemory? Memory { get; set; }
    }
    
    /// <summary>
    /// Result of a SCRIBE query (RAG response)
    /// </summary>
    public class ScribeQueryResult
    {
        /// <summary>
        /// The generated response
        /// </summary>
        public string Response { get; set; } = string.Empty;
        
        /// <summary>
        /// Chunks that were used to generate the response
        /// </summary>
        public IList<ScribeSearchResult> Sources { get; set; } = new List<ScribeSearchResult>();
        
        /// <summary>
        /// Time taken to generate response in milliseconds
        /// </summary>
        public int GenerationTimeMs { get; set; }
        
        /// <summary>
        /// Model used for generation
        /// </summary>
        public string? ModelUsed { get; set; }
        
        /// <summary>
        /// If access was restricted
        /// </summary>
        public bool AccessRestricted { get; set; }
        
        /// <summary>
        /// Message if access was restricted
        /// </summary>
        public string? AccessMessage { get; set; }
        
        /// <summary>
        /// Conversation ID (for continuing the conversation)
        /// </summary>
        public int? ConversationId { get; set; }
    }
    
    /// <summary>
    /// Main orchestration service for SCRIBE - the AI memory system
    /// </summary>
    public interface IScribeService
    {
        /// <summary>
        /// Query SCRIBE with a natural language question
        /// </summary>
        /// <param name="query">User's question</param>
        /// <param name="userId">Current user ID</param>
        /// <param name="characterId">Optional character ID for access control</param>
        /// <param name="campaignId">Optional campaign ID to scope search</param>
        /// <param name="conversationId">Optional existing conversation ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Generated response with sources</returns>
        Task<ScribeQueryResult> QueryAsync(
            string query,
            string userId,
            int? characterId = null,
            int? campaignId = null,
            int? conversationId = null,
            bool isGameMaster = false,
            CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Query SCRIBE with streaming response
        /// </summary>
        IAsyncEnumerable<string> QueryStreamAsync(
            string query,
            string userId,
            int? characterId = null,
            int? campaignId = null,
            int? conversationId = null,
            bool isGameMaster = false,
            CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Search for relevant chunks without generating a response
        /// </summary>
        /// <param name="query">Search query</param>
        /// <param name="userId">Current user ID</param>
        /// <param name="characterId">Optional character ID</param>
        /// <param name="campaignId">Optional campaign ID</param>
        /// <param name="topK">Number of results to return</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task<IList<ScribeSearchResult>> SearchAsync(
            string query,
            string userId,
            int? characterId = null,
            int? campaignId = null,
            int topK = 5,
            bool isGameMaster = false,
            CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Ingest a document into SCRIBE
        /// </summary>
        /// <param name="stream">Document stream</param>
        /// <param name="fileName">Original filename</param>
        /// <param name="campaignId">Campaign to associate with</param>
        /// <param name="characterIds">Characters who have access to this content</param>
        /// <param name="isPublic">If true, all players can access</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Created memory ID</returns>
        Task<int> IngestDocumentAsync(
            Stream stream,
            string fileName,
            int campaignId,
            IEnumerable<int>? characterIds = null,
            bool isPublic = false,
            CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Ingest text content directly
        /// </summary>
        /// <param name="title">Title of the memory</param>
        /// <param name="content">Text content</param>
        /// <param name="type">Type of memory</param>
        /// <param name="campaignId">Campaign ID</param>
        /// <param name="characterIds">Characters with access</param>
        /// <param name="isPublic">If public</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Created memory ID</returns>
        Task<int> IngestContentAsync(
            string title,
            string content,
            MemoryType type,
            int campaignId,
            IEnumerable<int>? characterIds = null,
            bool isPublic = false,
            CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Generate a summary for a chapter
        /// </summary>
        /// <param name="chapterId">Chapter to summarize</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Generated summary memory ID</returns>
        Task<int> GenerateChapterSummaryAsync(
            int chapterId,
            CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Check if SCRIBE services are available
        /// </summary>
        Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Import pre-processed chunks from external extraction tool (e.g., Python script)
        /// </summary>
        /// <param name="importData">Pre-processed import data with chunks</param>
        /// <param name="campaignId">Campaign to associate with</param>
        /// <param name="characterNameToIdMap">Map of character names to database IDs</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Import result with statistics</returns>
        Task<ScribeImportResult> ImportBatchAsync(
            ScribeImportData importData,
            int campaignId,
            Dictionary<string, int>? characterNameToIdMap = null,
            CancellationToken cancellationToken = default);
        
        // ==========================================
        // Conversation History
        // ==========================================
        
        /// <summary>
        /// Get user's conversations
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="campaignId">Optional campaign filter</param>
        /// <param name="limit">Max conversations to return</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task<IList<ScribeConversation>> GetConversationsAsync(
            string userId,
            int? campaignId = null,
            int limit = 20,
            CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Get a specific conversation with messages
        /// </summary>
        /// <param name="conversationId">Conversation ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task<ScribeConversation?> GetConversationAsync(
            int conversationId,
            CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Create a new conversation
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="campaignId">Optional campaign context</param>
        /// <param name="characterId">Optional character context</param>
        /// <param name="title">Optional title</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task<ScribeConversation> CreateConversationAsync(
            string userId,
            int? campaignId = null,
            int? characterId = null,
            string? title = null,
            CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Delete a conversation
        /// </summary>
        /// <param name="conversationId">Conversation to delete</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task DeleteConversationAsync(
            int conversationId,
            CancellationToken cancellationToken = default);
    }
}
