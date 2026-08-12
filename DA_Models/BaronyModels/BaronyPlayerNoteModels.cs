namespace DA_Models.BaronyModels
{
    /// <summary>DTO for a private baron planning note (journal / sticky / reminder).</summary>
    public class BaronyPlayerNoteDTO
    {
        public int Id { get; set; }
        public int BaronyId { get; set; }
        public string NoteType { get; set; } = "journal";
        public string? Title { get; set; }
        public string? BodyHtml { get; set; }
        public string? Color { get; set; }
        public int? DueTurn { get; set; }
        public int CreatedTurn { get; set; }
        public bool IsDone { get; set; }
        public int SortOrder { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime UpdatedAtUtc { get; set; }
    }

    /// <summary>Well-known <see cref="BaronyPlayerNoteDTO.NoteType"/> values.</summary>
    public static class BaronyPlayerNoteType
    {
        public const string Journal = "journal";
        public const string Sticky = "sticky";
        public const string Reminder = "reminder";
    }
}
