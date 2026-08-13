using System.ComponentModel.DataAnnotations;

namespace DA_DataAccess.BaronyData
{
    /// <summary>
    /// One tactical battle map per barony. Cells, tokens, turn state and log are JSON blobs
    /// so the grid can evolve without schema changes (same pattern as chapter BattleMap).
    /// </summary>
    public class BaronyBattleMap
    {
        [Key]
        public int Id { get; set; }

        public int BaronyId { get; set; }

        /// <summary>When false, Baron players do not see the Battle Map tab.</summary>
        public bool IsActive { get; set; }

        /// <summary>"setup" (deploy/paint) or "battle" (turn order + movement).</summary>
        public string Phase { get; set; } = "setup";

        public int Width { get; set; } = 20;
        public int Height { get; set; } = 16;

        public string CellsJson { get; set; } = "[]";
        public string TokensJson { get; set; } = "[]";
        public string TurnStateJson { get; set; } = "{}";
        public string LogJson { get; set; } = "[]";
        public string TalliesJson { get; set; } = "[]";
        public string XpSummaryJson { get; set; } = "null";
    }
}
