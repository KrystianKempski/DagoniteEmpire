using DA_Common.Barony;

namespace DagoniteEmpire.Pages.Barony.Components
{
    /// <summary>One row in a Resources-tab cumulative table.</summary>
    public sealed class ResourceTableRow
    {
        public string Label { get; set; } = string.Empty;
        public string? Description { get; set; }
        public PpbVector Values { get; set; } = new();
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
        public int? SourceId { get; set; }
        public bool IsSystem { get; set; }
    }
}
