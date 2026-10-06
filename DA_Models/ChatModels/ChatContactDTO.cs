namespace DA_Models.ChatModels
{
    /// <summary>
    /// One row of the chat list. Covers both an existing thread and a peer you may write to but never
    /// have — conversations are only created when the first message is sent, so
    /// <see cref="ConversationId"/> is null until then.
    /// </summary>
    public class ChatContactDTO
    {
        public long? ConversationId { get; set; }

        public bool IsPartyChannel { get; set; }

        /// <summary>Set for party channels, so the thread can be created on first open.</summary>
        public int? CampaignId { get; set; }

        /// <summary>Peer account for a direct thread; null when the peer is the Game Master role.</summary>
        public string? PeerUserId { get; set; }

        public bool PeerIsGameMaster { get; set; }

        public string DisplayName { get; set; } = string.Empty;

        public string? ImageUrl { get; set; }

        public int UnreadCount { get; set; }

        public string? LastMessagePreview { get; set; }

        public DateTime? LastMessageUtc { get; set; }
    }
}
