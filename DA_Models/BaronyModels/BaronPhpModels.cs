using DA_Common.Barony;

namespace DA_Models.BaronyModels
{
    public class BaronPhpSourceDTO
    {
        public int Id { get; set; }
        public int BaronyId { get; set; }
        public string Source { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int Prestige { get; set; }
        public int Honor { get; set; }
        public int Fear { get; set; }
    }

    /// <summary>Prestige / Honor / Fear totals from one source row.</summary>
    public readonly record struct PhpTotals(int Prestige, int Honor, int Fear)
    {
        public static PhpTotals Zero => new(0, 0, 0);

        public PhpTotals Add(int prestige, int honor, int fear) =>
            new(Prestige + prestige, Honor + honor, Fear + fear);

        public PhpTotals Add(PhpTotals other) =>
            Add(other.Prestige, other.Honor, other.Fear);
    }

    /// <summary>Single row in the Baron Card PHP table.</summary>
    public sealed class BaronPhpRow
    {
        public string Source { get; set; } = string.Empty;
        public int Prestige { get; set; }
        public int Honor { get; set; }
        public int Fear { get; set; }
        public bool IsSystem { get; set; }
        public int? SourceId { get; set; }
        public string? Description { get; set; }
    }
}
