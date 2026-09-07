using DA_DataAccess.CharacterClasses;
using System.ComponentModel.DataAnnotations.Schema;

namespace DA_DataAccess.Chat
{
    /// <summary>
    /// Read watermark for a (reader, peer) pair. PeerCharacterId null = party channel.
    /// </summary>
    public class CampaignChatRead
    {
        public long Id { get; set; }

        public int CampaignId { get; set; }
        [ForeignKey(nameof(CampaignId))]
        public virtual Campaign? Campaign { get; set; }

        public int CharacterId { get; set; }
        [ForeignKey(nameof(CharacterId))]
        public virtual Character? Character { get; set; }

        /// <summary>null = party channel.</summary>
        public int? PeerCharacterId { get; set; }

        public DateTime LastReadDate { get; set; }
    }
}
