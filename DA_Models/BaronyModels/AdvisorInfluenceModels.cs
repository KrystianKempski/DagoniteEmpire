using DA_Common.Barony;

namespace DA_Models.BaronyModels
{
    public class AdvisorInfluenceModifierDTO
    {
        public int Id { get; set; }
        public int AdvisorId { get; set; }
        public string Source { get; set; } = string.Empty;
        public PpbVector Additive { get; set; } = new();
        public string? FormulaText { get; set; }
        public string? Description { get; set; }
        public decimal CostGold { get; set; }
    }

    public enum AdvisorInfluenceSystemKind
    {
        None,
        Skills,
    }

    /// <summary>Single row in an advisor office influence table.</summary>
    public sealed class AdvisorInfluenceRow
    {
        public string Source { get; set; } = string.Empty;
        public PpbVector Values { get; set; } = new();
        public bool IsSystem { get; set; }
        public AdvisorInfluenceSystemKind SystemKind { get; set; }
        public int? ModifierId { get; set; }
        public string? Formula { get; set; }
        public string? Description { get; set; }
        public decimal Cost { get; set; }
    }
}
