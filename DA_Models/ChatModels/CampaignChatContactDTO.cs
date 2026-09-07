namespace DA_Models.ChatModels
{
    public class CampaignChatContactDTO
    {
        /// <summary>null = party channel.</summary>
        public int? PeerCharacterId { get; set; }
        public string DisplayName { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public bool IsPartyChannel { get; set; }
        public bool IsGameMaster { get; set; }
        public int UnreadCount { get; set; }
        public string? LastMessagePreview { get; set; }
        public DateTime? LastMessageDate { get; set; }
    }
}
