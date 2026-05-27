using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DA_Scribe.Entities
{
    /// <summary>
    /// A conversation session with SCRIBE.
    /// Tracks chat history for contextual follow-up questions.
    /// </summary>
    public class ScribeConversation
    {
        [Key]
        public int Id { get; set; }
        
        /// <summary>
        /// User ID (ASP.NET Identity)
        /// </summary>
        [Required]
        [MaxLength(450)]
        public string UserId { get; set; } = string.Empty;
        
        /// <summary>
        /// If the user was querying as a specific character
        /// </summary>
        public int? CharacterId { get; set; }
        
        /// <summary>
        /// Campaign context for the conversation (if any)
        /// </summary>
        public int? CampaignId { get; set; }
        
        /// <summary>
        /// When the conversation started
        /// </summary>
        public DateTime StartedAt { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// When the last message was sent
        /// </summary>
        public DateTime? LastMessageAt { get; set; }
        
        /// <summary>
        /// Optional title/summary of the conversation
        /// </summary>
        [MaxLength(200)]
        public string? Title { get; set; }
        
        /// <summary>
        /// Messages in this conversation
        /// </summary>
        public virtual ICollection<ScribeMessage> Messages { get; set; } = new List<ScribeMessage>();
    }
}
