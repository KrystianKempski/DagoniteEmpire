using DA_Common.Barony;

namespace DA_Models.BaronyModels
{
    /// <summary>Komplet danych baronii wykorzystywany przez Panel Domeny i strony pochodne.</summary>
    public class BaronyOverviewDTO
    {
        public BaronyDTO Barony { get; set; } = new();
        public List<AdvisorDTO> Advisors { get; set; } = new();
        public List<AvailableAdvisorDTO> AvailableAdvisors { get; set; } = new();
        public List<BaronyBuildingDTO> Buildings { get; set; } = new();
        public List<SocialGroupRelationDTO> SocialRelations { get; set; } = new();
        public List<TerrainImprovementDTO> Improvements { get; set; } = new();
        public List<DecreeDTO> Decrees { get; set; } = new();
        public List<BaronyEventDTO> Events { get; set; } = new();
        public List<CommunityModifierDTO> CommunityModifiers { get; set; } = new();
        public List<FiefDTO> Fiefs { get; set; } = new();
        public List<TerrainTileDTO> Tiles { get; set; } = new();
        public List<BaronyProjectDTO> Projects { get; set; } = new();
        public List<BaronyRelationDTO> Relations { get; set; } = new();
        public BaronySeatDTO? Seat { get; set; }
        public List<SeatPurposeTemplateDTO> SeatPurposeTemplates { get; set; } = new();
        public List<BaronyResourceSourceDTO> ResourceSources { get; set; } = new();
        public List<BaronPurseSourceDTO> PurseSources { get; set; } = new();
        public List<BaronyUnitDTO> Units { get; set; } = new();

        /// <summary>Suma wpływów addytywnych z dostarczonych modyfikatorów.</summary>
        public static PpbVector SumAdditive(IEnumerable<(PpbVector Additive, PpbVector Percent)> rows)
            => PpbVector.Sum(rows.Select(r => r.Additive));

        /// <summary>Suma wpływów procentowych z dostarczonych modyfikatorów.</summary>
        public static PpbVector SumPercent(IEnumerable<(PpbVector Additive, PpbVector Percent)> rows)
            => PpbVector.Sum(rows.Select(r => r.Percent));
    }
}
