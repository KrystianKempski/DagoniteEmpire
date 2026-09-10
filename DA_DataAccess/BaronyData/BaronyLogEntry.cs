using System.ComponentModel.DataAnnotations;

namespace DA_DataAccess.BaronyData
{
    /// <summary>
    /// Chronicle of everything the baron and the Game Master did in a barony, grouped by turn.
    /// Entries are short one-liners; only the turn resolve entry carries a full report in
    /// <see cref="Details"/>. Writing a log entry must never break the action being logged.
    /// </summary>
    public class BaronyLogEntry
    {
        [Key]
        public int Id { get; set; }

        public int BaronyId { get; set; }

        /// <summary>Turn the action happened in (grouping key in the UI).</summary>
        public int TurnNumber { get; set; }

        public int Year { get; set; }
        public int Month { get; set; }
        public string Season { get; set; } = string.Empty;

        /// <summary>See <c>BaronyLogCategory</c> — Resources, Projects, Army, TurnResolve, …</summary>
        public string Category { get; set; } = string.Empty;

        /// <summary>User name of whoever triggered the action, or "System".</summary>
        public string Actor { get; set; } = string.Empty;

        /// <summary>"Baron", "GM" or "System".</summary>
        public string ActorRole { get; set; } = string.Empty;

        /// <summary>Short one-line description of the action.</summary>
        [MaxLength(400)]
        public string Summary { get; set; } = string.Empty;

        /// <summary>Full report; only filled for turn resolve entries.</summary>
        public string? Details { get; set; }

        /// <summary>Optional origin of the entry, e.g. "Project", "BaronyUnit".</summary>
        public string? EntityType { get; set; }

        public int? EntityId { get; set; }

        /// <summary>Highlighted in the chronicle (turn resolve, battles, events).</summary>
        public bool IsImportant { get; set; }

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    }
}
