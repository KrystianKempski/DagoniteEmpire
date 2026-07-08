using System;

namespace DA_DataAccess.Chat
{
    /// <summary>
    /// A single significant event that happened during a battle (wound, unconsciousness,
    /// death, state change, bleeding pain test, ...). Persisted so turn/battle summaries and
    /// the battle log survive page reloads. <see cref="Kind"/> and <see cref="Importance"/>
    /// are stored as ints mirroring the DA_Models enums.
    /// </summary>
    public class BattleEvent
    {
        public int Id { get; set; }
        public int BattlePhaseId { get; set; }
        public int ChapterId { get; set; }
        public int CampaignId { get; set; }
        public int TurnNumber { get; set; }
        public int Kind { get; set; }
        public int Importance { get; set; }
        public string TargetName { get; set; } = string.Empty;
        public string? CausedBy { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}
