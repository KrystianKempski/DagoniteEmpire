namespace DA_Models.BaronyModels
{
    public class BaronQaThreadDTO
    {
        public int Id { get; set; }
        public int BaronyId { get; set; }
        public string Title { get; set; } = string.Empty;
        public int CreatedTurn { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime UpdatedAtUtc { get; set; }
        public List<BaronQaMessageDTO> Messages { get; set; } = new();

        public BaronQaMessageDTO? LastMessage =>
            Messages.OrderByDescending(m => m.SortOrder).ThenByDescending(m => m.Id).FirstOrDefault();

        public bool HasUnreadForBaron => Messages.Any(m => m.IsFromGm && !m.SeenByBaron);
        public bool HasUnreadForGm => Messages.Any(m => !m.IsFromGm && !m.SeenByGm);
    }

    public class BaronQaMessageDTO
    {
        public int Id { get; set; }
        public int ThreadId { get; set; }
        public string Body { get; set; } = string.Empty;
        public bool IsFromGm { get; set; }
        public string? SpeakerName { get; set; }
        public int TurnNumber { get; set; }
        public int SortOrder { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public bool SeenByBaron { get; set; } = true;
        public bool SeenByGm { get; set; } = true;
    }
}
