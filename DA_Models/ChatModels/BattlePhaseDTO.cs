namespace DA_Models.ChatModels
{
    public class BattlePhaseDTO
    {
        public BattlePhaseDTO() { }
        public BattlePhaseDTO(int campId, int chapterId)
        {
            CampaignId = campId;
            ChapterId = chapterId;
            BattleOngoing = true;
        }

        public int Id { get; set; }
        public int Name { get; set; }
        public int ChapterId { get; set; }
        public int CampaignId { get; set; }
        public int CurrentTurn { get; set; } = 1;
        public bool BattleOngoing { get; set; } = false;
    }
}
