using System.ComponentModel.DataAnnotations;

namespace DA_DataAccess.BaronyData
{
    /// <summary>Reusable MG template for audience hall narrative events.</summary>
    public class BaronyHallEventTemplate
    {
        [Key]
        public int Id { get; set; }

        public int BaronyId { get; set; }

        /// <summary>Template label in the MG list (not shown to the baron).</summary>
        public string Name { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public string Body { get; set; } = string.Empty;

        /// <summary><see cref="DA_Common.Barony.BaronyHallEventTriggerKind"/> default.</summary>
        public string TriggerKind { get; set; } = DA_Common.Barony.BaronyHallEventTriggerKind.Manual;

        public string IconPath { get; set; } = "icons/vertical-banner.svg";

        public string ConsequenceAdditiveJson { get; set; } = "{}";
        public string ConsequencePercentJson { get; set; } = "{}";
        public int? ConsequenceEndTurn { get; set; }

        public int SortOrder { get; set; }

        public DateTime CreatedAtUtc { get; set; }
        public DateTime UpdatedAtUtc { get; set; }
    }
}
