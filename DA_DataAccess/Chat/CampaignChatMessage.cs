using DA_DataAccess.CharacterClasses;
using System.ComponentModel.DataAnnotations.Schema;

namespace DA_DataAccess.Chat
{
    public class CampaignChatMessage
    {
        public long Id { get; set; }

        public int CampaignId { get; set; }
        [ForeignKey(nameof(CampaignId))]
        public virtual Campaign? Campaign { get; set; }

        public int SenderCharacterId { get; set; }
        [ForeignKey(nameof(SenderCharacterId))]
        public virtual Character? SenderCharacter { get; set; }

        /// <summary>null = party channel (all campaign players + GM).</summary>
        public int? RecipientCharacterId { get; set; }
        [ForeignKey(nameof(RecipientCharacterId))]
        public virtual Character? RecipientCharacter { get; set; }

        public string Content { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public bool IsDeleted { get; set; }
    }
}
