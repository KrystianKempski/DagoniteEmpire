namespace DA_DataAccess.Chat
{
    /// <summary>
    /// Persisted tactical battle map for a chapter. Cells and tokens are stored as JSON blobs
    /// so the grid can evolve without schema changes.
    /// </summary>
    public class BattleMap
    {
        public int Id { get; set; }
        public int ChapterId { get; set; }
        public int CampaignId { get; set; }
        public int Width { get; set; } = 10;
        public int Height { get; set; } = 10;

        /// <summary>JSON array of cells that have a color and/or centered text.</summary>
        public string CellsJson { get; set; } = "[]";

        /// <summary>JSON array of movable tokens (character/mob markers).</summary>
        public string TokensJson { get; set; } = "[]";
    }
}
