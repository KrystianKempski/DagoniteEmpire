using System.ComponentModel.DataAnnotations;

namespace DA_DataAccess.BaronyData
{
    /// <summary>Shared baron ↔ MG question thread on the Notes page.</summary>
    public class BaronQaThread
    {
        [Key]
        public int Id { get; set; }
        public int BaronyId { get; set; }

        public string Title { get; set; } = string.Empty;

        /// <summary>Barony turn when the thread was opened.</summary>
        public int CreatedTurn { get; set; }

        public DateTime CreatedAtUtc { get; set; }
        public DateTime UpdatedAtUtc { get; set; }

        public List<BaronQaMessage> Messages { get; set; } = new();
    }

    /// <summary>One message in a <see cref="BaronQaThread"/>.</summary>
    public class BaronQaMessage
    {
        [Key]
        public int Id { get; set; }
        public int ThreadId { get; set; }

        public string Body { get; set; } = string.Empty;

        /// <summary>True = Game Master side; false = baron.</summary>
        public bool IsFromGm { get; set; }

        public string? SpeakerName { get; set; }

        public int TurnNumber { get; set; }
        public int SortOrder { get; set; }

        public DateTime CreatedAtUtc { get; set; }

        public bool SeenByBaron { get; set; } = true;
        public bool SeenByGm { get; set; } = true;

        public BaronQaThread? Thread { get; set; }
    }
}
