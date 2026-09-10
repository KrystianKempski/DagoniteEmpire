using System.Collections.Generic;

namespace DA_Models.BaronyModels
{
    /// <summary>Single chronicle entry shown in the barony log tab.</summary>
    public class BaronyLogEntryDTO
    {
        public int Id { get; set; }
        public int BaronyId { get; set; }
        public int TurnNumber { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public string Season { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Actor { get; set; } = string.Empty;
        public string ActorRole { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
        public string? Details { get; set; }
        public string? EntityType { get; set; }
        public int? EntityId { get; set; }
        public bool IsImportant { get; set; }
        public DateTime CreatedAtUtc { get; set; }

        public bool HasDetails => !string.IsNullOrWhiteSpace(Details);
    }

    /// <summary>Chronicle entries of one turn, newest turn first in the UI.</summary>
    public class BaronyLogTurnDTO
    {
        public int TurnNumber { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public string Season { get; set; } = string.Empty;
        public int EntryCount { get; set; }
        public List<BaronyLogEntryDTO> Entries { get; set; } = new();
    }

    /// <summary>Optional narrowing of the chronicle view.</summary>
    public class BaronyLogFilterDTO
    {
        public HashSet<string> Categories { get; set; } = new();
        public string? Actor { get; set; }
        public string? Search { get; set; }
        public bool ImportantOnly { get; set; }
    }
}
