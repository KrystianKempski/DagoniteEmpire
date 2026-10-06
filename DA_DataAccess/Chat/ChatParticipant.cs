using System.ComponentModel.DataAnnotations.Schema;

namespace DA_DataAccess.Chat
{
    /// <summary>
    /// A side of a direct thread. Party threads have no rows here — their membership is the campaign
    /// roster read at query time, so adding a character to a campaign cannot leave a stale participant
    /// list behind.
    /// </summary>
    public class ChatParticipant
    {
        public long Id { get; set; }

        public long ConversationId { get; set; }
        [ForeignKey(nameof(ConversationId))]
        public virtual ChatConversation? Conversation { get; set; }

        /// <summary>Null exactly when <see cref="IsGameMasterRole"/> is set.</summary>
        public string? UserId { get; set; }
        [ForeignKey(nameof(UserId))]
        public virtual ApplicationUser? User { get; set; }

        /// <summary>
        /// The Game Master side is a role, not an account: whoever holds the role reads and answers the
        /// same thread, so handing the campaign to another GM keeps the history intact. Read state stays
        /// per account in <see cref="ChatReadState"/>.
        /// </summary>
        public bool IsGameMasterRole { get; set; }
    }
}
