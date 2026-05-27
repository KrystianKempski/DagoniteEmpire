using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DA_Scribe.Entities
{
    /// <summary>
    /// A single message in a SCRIBE conversation
    /// </summary>
    public class ScribeMessage
    {
        [Key]
        public int Id { get; set; }
        
        /// <summary>
        /// The conversation this message belongs to
        /// </summary>
        [ForeignKey(nameof(Conversation))]
        public int ConversationId { get; set; }
        
        /// <summary>
        /// Navigation property
        /// </summary>
        public virtual ScribeConversation? Conversation { get; set; }
        
        /// <summary>
        /// Role: "user" or "assistant"
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string Role { get; set; } = "user";
        
        /// <summary>
        /// Message content
        /// </summary>
        [Required]
        public string Content { get; set; } = string.Empty;
        
        /// <summary>
        /// When the message was sent
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// IDs of chunks that were used to generate this response (for assistant messages).
        /// Stored as comma-separated values.
        /// </summary>
        [MaxLength(500)]
        public string? SourceChunkIds { get; set; }
        
        /// <summary>
        /// Model used to generate this response (for assistant messages)
        /// </summary>
        [MaxLength(100)]
        public string? ModelUsed { get; set; }
        
        /// <summary>
        /// Time taken to generate response in milliseconds
        /// </summary>
        public int? GenerationTimeMs { get; set; }
        
        // Helper property
        
        /// <summary>
        /// Get chunk IDs used for this response
        /// </summary>
        [NotMapped]
        public IEnumerable<int> ChunkIds
        {
            get
            {
                if (string.IsNullOrEmpty(SourceChunkIds))
                    return Enumerable.Empty<int>();
                
                return SourceChunkIds
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => int.TryParse(s.Trim(), out var id) ? id : 0)
                    .Where(id => id > 0);
            }
            set
            {
                SourceChunkIds = value?.Any() == true 
                    ? string.Join(",", value) 
                    : null;
            }
        }
    }
}
