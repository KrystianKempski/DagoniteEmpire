using System.ComponentModel.DataAnnotations;

namespace DA_DataAccess.BaronyData
{
    /// <summary>
    /// Private planning note owned by the baron player (never shown to the MG).
    /// A single table backs the "Notes" tab: journal entries, sticky notes and
    /// turn-based reminders are distinguished by <see cref="NoteType"/>.
    /// </summary>
    public class BaronyPlayerNote
    {
        [Key]
        public int Id { get; set; }

        public int BaronyId { get; set; }

        /// <summary>"journal" (long rich text), "sticky" (board card) or "reminder" (turn timer).</summary>
        public string NoteType { get; set; } = "journal";

        public string? Title { get; set; }

        /// <summary>Rich-text body (Quill HTML) for journal and sticky notes.</summary>
        public string? BodyHtml { get; set; }

        /// <summary>Sticky-note colour key.</summary>
        public string? Color { get; set; }

        /// <summary>Reminder target turn; the note is due when the barony reaches it.</summary>
        public int? DueTurn { get; set; }

        /// <summary>Turn number when the note was created.</summary>
        public int CreatedTurn { get; set; }

        /// <summary>Reminder dismissed / sticky archived.</summary>
        public bool IsDone { get; set; }

        public int SortOrder { get; set; }

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
    }
}
