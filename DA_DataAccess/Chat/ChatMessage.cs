using System.ComponentModel.DataAnnotations.Schema;

namespace DA_DataAccess.Chat
{
    /// <summary>A message in a <see cref="ChatConversation"/>, authored by an account.</summary>
    public class ChatMessage
    {
        public long Id { get; set; }

        public long ConversationId { get; set; }
        [ForeignKey(nameof(ConversationId))]
        public virtual ChatConversation? Conversation { get; set; }

        public string SenderUserId { get; set; } = string.Empty;
        [ForeignKey(nameof(SenderUserId))]
        public virtual ApplicationUser? SenderUser { get; set; }

        /// <summary>
        /// True when the author wrote with the Game Master role rather than as themselves, so the
        /// message renders as "Game Master" no matter which GM account typed it.
        /// </summary>
        public bool SentAsGameMaster { get; set; }

        public string Content { get; set; } = string.Empty;

        public DateTime CreatedUtc { get; set; }

        public bool IsDeleted { get; set; }
    }
}
