using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Pgvector;

namespace DA_DataAccess.Scribe
{
    /// <summary>
    /// Chunked and embedded content for vector similarity search.
    /// Each ScribeMemory is split into multiple chunks for better retrieval.
    /// </summary>
    public class ScribeChunk
    {
        [Key]
        public int Id { get; set; }
        
        /// <summary>
        /// The memory this chunk belongs to
        /// </summary>
        [ForeignKey(nameof(ScribeMemory))]
        public int ScribeMemoryId { get; set; }
        
        /// <summary>
        /// Navigation property to parent memory
        /// </summary>
        public virtual ScribeMemory? ScribeMemory { get; set; }
        
        /// <summary>
        /// The text content of this chunk (typically 300-500 tokens)
        /// </summary>
        [Required]
        public string Content { get; set; } = string.Empty;
        
        /// <summary>
        /// Vector embedding for similarity search.
        /// Using nomic-embed-text: 768 dimensions
        /// </summary>
        [Column(TypeName = "vector(768)")]
        public Vector? Embedding { get; set; }
        
        /// <summary>
        /// Order of this chunk within the parent memory (for context reconstruction)
        /// </summary>
        public int ChunkIndex { get; set; } = 0;
        
        /// <summary>
        /// Token count of this chunk (for reference)
        /// </summary>
        public int TokenCount { get; set; } = 0;
        
        // Denormalized metadata for efficient filtering during search
        
        /// <summary>
        /// Campaign ID (denormalized from memory for faster queries)
        /// </summary>
        public int? CampaignId { get; set; }
        
        /// <summary>
        /// Chapter ID (denormalized from memory for faster queries)
        /// </summary>
        public int? ChapterId { get; set; }
        
        /// <summary>
        /// Memory type (denormalized for filtering)
        /// </summary>
        public MemoryType MemoryType { get; set; }
        
        /// <summary>
        /// Character IDs with access (denormalized for access control in queries).
        /// Stored as comma-separated values.
        /// </summary>
        [MaxLength(1000)]
        public string? CharacterIdsJson { get; set; }
        
        /// <summary>
        /// If true, chunk is accessible to all players
        /// </summary>
        public bool IsPublic { get; set; } = false;
        
        /// <summary>
        /// If true, chunk is only visible to GM
        /// </summary>
        public bool IsGmOnly { get; set; } = false;
        
        // Helper property
        
        /// <summary>
        /// Get character IDs that have access to this chunk
        /// </summary>
        [NotMapped]
        public IEnumerable<int> CharacterIds
        {
            get
            {
                if (string.IsNullOrEmpty(CharacterIdsJson))
                    return Enumerable.Empty<int>();
                
                return CharacterIdsJson
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => int.TryParse(s.Trim(), out var id) ? id : 0)
                    .Where(id => id > 0);
            }
            set
            {
                CharacterIdsJson = value?.Any() == true 
                    ? string.Join(",", value) 
                    : null;
            }
        }
    }
}
