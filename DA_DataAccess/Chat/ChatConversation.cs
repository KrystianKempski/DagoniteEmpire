namespace DA_DataAccess.Chat
{
    public enum ChatConversationKind
    {
        /// <summary>One thread per pair of accounts, independent of campaigns.</summary>
        Direct = 0,

        /// <summary>One thread per campaign, open to the whole roster and the Game Masters.</summary>
        CampaignParty = 1,
    }

    /// <summary>
    /// A chat thread between accounts. Chat deliberately knows nothing about characters: a player
    /// with heroes in three campaigns still has exactly one thread per peer, so the unread badge
    /// and push notifications cannot depend on which character happens to be selected.
    /// </summary>
    public class ChatConversation
    {
        public long Id { get; set; }

        public ChatConversationKind Kind { get; set; }

        /// <summary>Set for <see cref="ChatConversationKind.CampaignParty"/>, null for direct threads.</summary>
        public int? CampaignId { get; set; }
        public virtual Campaign? Campaign { get; set; }

        /// <summary>
        /// Deterministic identity of the thread, unique across both kinds — see <see cref="ChatPairKey"/>.
        /// Non-nullable on purpose: a plain unique index then covers direct and party threads alike,
        /// without provider-specific filtered indexes (the tests run on SQLite, production on Npgsql).
        /// </summary>
        public string PairKey { get; set; } = string.Empty;

        public DateTime CreatedUtc { get; set; }

        /// <summary>Denormalized so the conversation list sorts without touching the messages.</summary>
        public DateTime LastMessageUtc { get; set; }

        public virtual ICollection<ChatParticipant> Participants { get; set; } = new List<ChatParticipant>();
    }
}
