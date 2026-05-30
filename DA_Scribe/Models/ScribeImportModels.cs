using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace DA_Scribe.Models
{
    /// <summary>
    /// Root object for importing pre-processed campaign data from external tools
    /// </summary>
    public class ScribeImportData
    {
        [JsonPropertyName("metadata")]
        public ScribeImportMetadata Metadata { get; set; } = new();
        
        [JsonPropertyName("chunks")]
        public List<ScribeImportChunk> Chunks { get; set; } = new();
    }
    
    /// <summary>
    /// Metadata about the import batch
    /// </summary>
    public class ScribeImportMetadata
    {
        [JsonPropertyName("campaign")]
        public string Campaign { get; set; } = string.Empty;
        
        [JsonPropertyName("processed_at")]
        public DateTime? ProcessedAt { get; set; }
        
        [JsonPropertyName("stats")]
        public ScribeImportStats? Stats { get; set; }
    }
    
    /// <summary>
    /// Statistics about the extracted data
    /// </summary>
    public class ScribeImportStats
    {
        [JsonPropertyName("total_documents")]
        public int TotalDocuments { get; set; }
        
        [JsonPropertyName("total_chunks")]
        public int TotalChunks { get; set; }
        
        [JsonPropertyName("all_characters")]
        public List<string> AllCharacters { get; set; } = new();
        
        [JsonPropertyName("all_npcs")]
        public List<string> AllNpcs { get; set; } = new();
        
        [JsonPropertyName("all_locations")]
        public List<string> AllLocations { get; set; } = new();
    }
    
    /// <summary>
    /// A pre-processed chunk ready for embedding and storage
    /// </summary>
    public class ScribeImportChunk
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;
        
        [JsonPropertyName("document_path")]
        public string? DocumentPath { get; set; }
        
        [JsonPropertyName("act_number")]
        public string? ActNumber { get; set; }
        
        [JsonPropertyName("scene_title")]
        public string? SceneTitle { get; set; }
        
        /// <summary>
        /// Content with character annotations like [Tomin]text
        /// </summary>
        [JsonPropertyName("content")]
        public string Content { get; set; } = string.Empty;
        
        /// <summary>
        /// Plain text content without annotations (for embedding)
        /// </summary>
        [JsonPropertyName("content_plain")]
        public string ContentPlain { get; set; } = string.Empty;
        
        /// <summary>
        /// Point-of-view characters in this chunk
        /// </summary>
        [JsonPropertyName("pov_characters")]
        public List<string> PovCharacters { get; set; } = new();
        
        /// <summary>
        /// All player characters present/mentioned
        /// </summary>
        [JsonPropertyName("characters_present")]
        public List<string> CharactersPresent { get; set; } = new();
        
        /// <summary>
        /// Non-player characters mentioned
        /// </summary>
        [JsonPropertyName("npcs_mentioned")]
        public List<string> NpcsMentioned { get; set; } = new();
        
        /// <summary>
        /// Locations mentioned in this chunk
        /// </summary>
        [JsonPropertyName("locations")]
        public List<string> Locations { get; set; } = new();
        
        /// <summary>
        /// Items, equipment, or possessions mentioned
        /// </summary>
        [JsonPropertyName("items_mentioned")]
        public List<string> ItemsMentioned { get; set; } = new();
        
        /// <summary>
        /// In-game date (e.g., "4 Erastus")
        /// </summary>
        [JsonPropertyName("date_in_game")]
        public string? DateInGame { get; set; }
        
        [JsonPropertyName("has_dialogue")]
        public bool HasDialogue { get; set; }
        
        [JsonPropertyName("has_combat")]
        public bool HasCombat { get; set; }
        
        [JsonPropertyName("has_game_mechanics")]
        public bool HasGameMechanics { get; set; }
        
        /// <summary>
        /// Primary type: narrative, dialogue, combat, mechanics, summary, world, rules, character
        /// </summary>
        [JsonPropertyName("chunk_type")]
        public string ChunkType { get; set; } = "narrative";
        
        [JsonPropertyName("word_count")]
        public int WordCount { get; set; }
    }
    
    /// <summary>
    /// Result of a batch import operation
    /// </summary>
    public class ScribeImportResult
    {
        public bool Success { get; set; }
        public int ChunksImported { get; set; }
        public int ChunksFailed { get; set; }
        public int MemoriesCreated { get; set; }
        public TimeSpan Duration { get; set; }
        public List<string> Errors { get; set; } = new();
        public string? Message { get; set; }
    }

    /// <summary>
    /// Request for querying SCRIBE
    /// </summary>
    public class ScribeQueryRequest
    {
        /// <summary>
        /// The question to ask
        /// </summary>
        [Required]
        [StringLength(4000, MinimumLength = 1)]
        public string Query { get; set; } = string.Empty;
        
        /// <summary>
        /// User making the request (for logging)
        /// </summary>
        public string? UserId { get; set; }
        
        /// <summary>
        /// Character context - filters results to what this character knows
        /// </summary>
        public int? CharacterId { get; set; }
        
        /// <summary>
        /// Campaign to search in
        /// </summary>
        public int? CampaignId { get; set; }
        
        /// <summary>
        /// Number of chunks to retrieve (default 5)
        /// </summary>
        [Range(1, 20)]
        public int? TopK { get; set; }
        
        /// <summary>
        /// Whether the user is a Game Master (has full access)
        /// </summary>
        public bool IsGameMaster { get; set; } = false;
    }
}
