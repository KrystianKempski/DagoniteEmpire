using System;
using DA_Models.ComponentModels;

namespace DA_Models.ChatModels
{
    public class BattleEventDTO
    {
        public int Id { get; set; }
        public int BattlePhaseId { get; set; }
        public int ChapterId { get; set; }
        public int CampaignId { get; set; }
        public int TurnNumber { get; set; }
        public BattleTurnEventKind Kind { get; set; }
        public BattleEventImportance Importance { get; set; }
        public string TargetName { get; set; } = string.Empty;
        public string? CausedBy { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}
