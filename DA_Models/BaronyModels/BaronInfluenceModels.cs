using DA_Common.Barony;

namespace DA_Models.BaronyModels
{
    public class BaronInfluenceModifierDTO
    {
        public int Id { get; set; }
        public int BaronyId { get; set; }
        public string Source { get; set; } = string.Empty;
        public PpbVector Additive { get; set; } = new();
        public string? FormulaText { get; set; }
        public string? Description { get; set; }
    }

    /// <summary>Single row in the Baron Card influence table.</summary>
    public sealed class BaronInfluenceRow
    {
        public string Source { get; set; } = string.Empty;
        public PpbVector Values { get; set; } = new();
        public bool IsSystem { get; set; }
        public int? ModifierId { get; set; }
        public string? Formula { get; set; }
        public string? Description { get; set; }
    }
}
