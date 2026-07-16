using DA_Models.BaronyModels;

namespace DA_Business.Repository.CharacterReps.IRepository
{
    public interface IBaronyRepository
    {
        // --- Barony ---
        Task<BaronyDTO?> GetByCharacterId(int characterId);
        Task<BaronyDTO?> GetById(int id);
        Task<List<BaronyListItemDTO>> GetAllSummaries();
        Task<BaronyDTO> CreateForCharacter(int characterId, string name, string? notes = null);
        Task<BaronyDTO> UpdateBarony(BaronyDTO dto);
        Task<BaronyOverviewDTO?> GetOverview(int baronyId);

        // --- Advisors / urzędy ---
        Task<List<AdvisorDTO>> GetAdvisors(int baronyId);
        Task<AdvisorDTO> SaveAdvisor(AdvisorDTO dto);
        Task<int> DeleteAdvisor(int id);

        // --- Budynki miasta ---
        Task<List<BaronyBuildingDTO>> GetBuildings(int baronyId);
        Task<BaronyBuildingDTO> SaveBuilding(BaronyBuildingDTO dto);
        Task<int> DeleteBuilding(int id);

        // --- Relacje społeczne ---
        Task<List<SocialGroupRelationDTO>> GetSocialRelations(int baronyId);
        Task<SocialGroupRelationDTO> SaveSocialRelation(SocialGroupRelationDTO dto);
        Task<int> DeleteSocialRelation(int id);

        // --- Dekrety ---
        Task<List<DecreeDTO>> GetDecrees(int baronyId);
        Task<DecreeDTO> SaveDecree(DecreeDTO dto);
        Task<int> DeleteDecree(int id);

        // --- Wydarzenia ---
        Task<List<BaronyEventDTO>> GetEvents(int baronyId);
        Task<BaronyEventDTO> SaveEvent(BaronyEventDTO dto);
        Task<int> DeleteEvent(int id);

        // --- Kary/bonusy społeczności ---
        Task<List<CommunityModifierDTO>> GetCommunityModifiers(int baronyId);
        Task<CommunityModifierDTO> SaveCommunityModifier(CommunityModifierDTO dto);
        Task<int> DeleteCommunityModifier(int id);

        // --- Baron Card influence ---
        Task<List<BaronInfluenceModifierDTO>> GetBaronInfluenceModifiers(int baronyId);
        Task<BaronInfluenceModifierDTO> SaveBaronInfluenceModifier(BaronInfluenceModifierDTO dto);
        Task<int> DeleteBaronInfluenceModifier(int id);

        // --- Offices influence ---
        Task<List<AdvisorInfluenceModifierDTO>> GetAdvisorInfluenceModifiers(int baronyId);
        Task<AdvisorInfluenceModifierDTO> SaveAdvisorInfluenceModifier(AdvisorInfluenceModifierDTO dto);
        Task<int> DeleteAdvisorInfluenceModifier(int id);

        // --- Lenna ---
        Task<List<FiefDTO>> GetFiefs(int baronyId);
        Task<FiefDTO> SaveFief(FiefDTO dto);
        Task<int> DeleteFief(int id);

        // --- Pola terenu ---
        Task<List<TerrainTileDTO>> GetTiles(int baronyId);
        Task<List<TerrainTileDTO>> EnsureTerrainGrid(int baronyId);
        Task<TerrainTileDTO> SaveTile(TerrainTileDTO dto);
        Task<int> DeleteTile(int id);

        // --- Map domains ---
        Task<List<TerrainMapDomainDTO>> GetMapDomains(int baronyId);
        Task<TerrainMapDomainDTO> SaveMapDomain(TerrainMapDomainDTO dto);
        Task<int> DeleteMapDomain(int id);

        // --- Ulepszenia terenu ---
        Task<List<TerrainImprovementDTO>> GetImprovements(int baronyId);
        Task<TerrainImprovementDTO> SaveImprovement(TerrainImprovementDTO dto);
        Task<int> DeleteImprovement(int id);

        // --- Projekty ---
        Task<List<BaronyProjectDTO>> GetProjects(int baronyId);
        Task<BaronyProjectDTO> SaveProject(BaronyProjectDTO dto);
        Task<int> DeleteProject(int id);

        // --- Katalog budynków/ulepszeń (globalny) ---
        Task<List<BuildingTemplateDTO>> GetBuildingTemplates();
        Task<BuildingTemplateDTO> SaveBuildingTemplate(BuildingTemplateDTO dto);
        Task<int> DeleteBuildingTemplate(int id);
    }
}
