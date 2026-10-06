namespace DA_Models.ChatModels
{
    public class ChatMessageDTO
    {
        public long Id { get; set; }

        public long ConversationId { get; set; }

        public string SenderUserId { get; set; } = string.Empty;

        public string SenderName { get; set; } = string.Empty;

        public string? SenderImageUrl { get; set; }

        public bool SentAsGameMaster { get; set; }

        public string Content { get; set; } = string.Empty;

        public DateTime CreatedUtc { get; set; }

        /// <summary>Resolved per reader, so it is set when the DTO leaves the repository, not stored.</summary>
        public bool IsMine { get; set; }
    }
}
