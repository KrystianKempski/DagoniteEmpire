using System.ComponentModel.DataAnnotations.Schema;

namespace DA_DataAccess.Chat
{
    /// <summary>
    /// Read watermark for one account in one thread. Kept apart from <see cref="ChatParticipant"/> so the
    /// Game Master role can be a single participant while each GM account still tracks its own unread
    /// count — otherwise one GM opening a thread would clear the badge for the others.
    /// </summary>
    public class ChatReadState
    {
        public long Id { get; set; }

        public long ConversationId { get; set; }
        [ForeignKey(nameof(ConversationId))]
        public virtual ChatConversation? Conversation { get; set; }

        public string UserId { get; set; } = string.Empty;
        [ForeignKey(nameof(UserId))]
        public virtual ApplicationUser? User { get; set; }

        public DateTime LastReadUtc { get; set; }
    }
}
