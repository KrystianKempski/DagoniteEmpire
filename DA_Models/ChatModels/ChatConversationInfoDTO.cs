namespace DA_Models.ChatModels
{
    /// <summary>Header for a thread opened straight from a push deep link, before the list is loaded.</summary>
    public class ChatConversationInfoDTO
    {
        public long ConversationId { get; set; }

        public bool IsPartyChannel { get; set; }

        public string DisplayName { get; set; } = string.Empty;

        public string? ImageUrl { get; set; }
    }
}
