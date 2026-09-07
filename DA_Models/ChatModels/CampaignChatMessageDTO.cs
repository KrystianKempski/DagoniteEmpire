namespace DA_Models.ChatModels
{
    public class CampaignChatMessageDTO
    {
        public long Id { get; set; }
        public int CampaignId { get; set; }
        public int SenderCharacterId { get; set; }
        public string SenderName { get; set; } = string.Empty;
        public string? SenderImageUrl { get; set; }
        public int? RecipientCharacterId { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public bool IsMine { get; set; }
    }
}
