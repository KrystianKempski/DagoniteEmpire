using DA_Common.Barony;
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

        /// <summary>Player marks (or clears) ready-to-end-turn for MG resolve.</summary>
        Task<BaronyDTO> SetPlayerTurnReady(int baronyId, bool ready);

        /// <summary>
        /// Apply end-of-turn: income, project ticks/completions, unrest check, calendar, conjuncture.
        /// <paramref name="expectedIncome"/> / loyalty / stability / population come from Domain Panel calc.
        /// </summary>
        Task<TurnResolveReportDTO> ResolveTurn(
            int baronyId,
            PpbVector expectedIncome,
            decimal loyaltyFinal,
            decimal stabilityFinal,
            int settlementPopulation);

        // --- Advisors / urzędy ---
        Task<List<AdvisorDTO>> GetAdvisors(int baronyId);
        Task<AdvisorDTO> SaveAdvisor(AdvisorDTO dto);
        Task<int> DeleteAdvisor(int id);
        Task<List<AvailableAdvisorDTO>> GetAvailableAdvisors(int baronyId);
        Task<AvailableAdvisorDTO> SaveAvailableAdvisor(AvailableAdvisorDTO dto);
        Task<int> DeleteAvailableAdvisor(int id);

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

        // --- Relacje ---
        Task<List<BaronyRelationDTO>> GetRelations(int baronyId);
        Task<BaronyRelationDTO> SaveRelation(BaronyRelationDTO dto);
        Task<int> DeleteRelation(int id);
        Task SaveRelationNotes(int relationId, string? notes);

        // --- Lord's Seat ---
        Task<BaronySeatDTO> EnsureSeat(int baronyId);
        Task<BaronySeatDTO?> GetSeat(int baronyId);
        Task<BaronySeatDTO> SaveSeat(BaronySeatDTO dto);
        Task<SeatRoomDTO> SaveSeatRoom(SeatRoomDTO dto);
        Task<int> DeleteSeatRoom(int id);
        Task SetSeatRoomPurpose(int roomId, int? purposeTemplateId, int? occupantAdvisorId = null, string? occupantCustom = null);
        Task SetSeatTile(int seatId, int level, int x, int y, string? kind);
        Task SaveSeatActiveLevels(int seatId, IReadOnlyList<int> levels);
        Task<List<SeatPurposeTemplateDTO>> GetSeatPurposeTemplates(int baronyId);
        Task<SeatPurposeTemplateDTO> SaveSeatPurposeTemplate(SeatPurposeTemplateDTO dto);
        Task<int> DeleteSeatPurposeTemplate(int id);

        // --- Kary/bonusy społeczności ---
        Task<List<CommunityModifierDTO>> GetCommunityModifiers(int baronyId);
        Task<CommunityModifierDTO> SaveCommunityModifier(CommunityModifierDTO dto);
        Task<int> DeleteCommunityModifier(int id);

        // --- Baron Card influence ---
        Task<List<BaronInfluenceModifierDTO>> GetBaronInfluenceModifiers(int baronyId);
        Task<BaronInfluenceModifierDTO> SaveBaronInfluenceModifier(BaronInfluenceModifierDTO dto);
        Task<int> DeleteBaronInfluenceModifier(int id);

        // --- Baron Card PHP (Prestige / Honor / Fear) ---
        Task<List<BaronPhpSourceDTO>> GetBaronPhpSources(int baronyId);
        Task<BaronPhpSourceDTO> SaveBaronPhpSource(BaronPhpSourceDTO dto);
        Task<int> DeleteBaronPhpSource(int id);

        // --- Baron Card artifacts ---
        Task<List<BaronArtifactDTO>> GetBaronArtifacts(int baronyId);
        Task<BaronArtifactDTO> SaveBaronArtifact(BaronArtifactDTO dto);
        Task<int> DeleteBaronArtifact(int id);

        // --- Baron Card time (BT) ---
        Task EnsureBaronTimeDefaults(int baronyId);
        Task<List<BaronTimeModifierDTO>> GetBaronTimeModifiers(int baronyId);
        Task<BaronTimeModifierDTO> SaveBaronTimeModifier(BaronTimeModifierDTO dto);
        Task<int> DeleteBaronTimeModifier(int id);
        Task<List<BaronTimeActionDTO>> GetBaronTimeActions(int baronyId);
        Task<BaronTimeActionDTO> SaveBaronTimeAction(BaronTimeActionDTO dto);
        Task<int> DeleteBaronTimeAction(int id);

        // --- Baron letters (threads + messages) ---
        Task<List<BaronLetterThreadDTO>> GetLetterThreads(int baronyId);
        Task<BaronLetterThreadDTO> SaveLetterThread(BaronLetterThreadDTO dto);
        Task<int> DeleteLetterThread(int id);
        Task<BaronLetterMessageDTO> SaveLetterMessage(BaronLetterMessageDTO dto);
        Task<int> DeleteLetterMessage(int id);
        Task MarkLetterThreadSeenByBaron(int threadId);
        Task MarkLetterThreadSeenByGm(int threadId);
        /// <summary>Unread inbound letters for the baron of this barony.</summary>
        Task<BaronLetterInboxBadgeDTO> GetLetterInboxBadgeForBaron(int baronyId);
        /// <summary>Unread outbound (baron→) letters across all baronies — for MG/Admin FAB.</summary>
        Task<BaronLetterInboxBadgeDTO> GetLetterInboxBadgeForGm();

        // --- Baron audiences (petitioner dialogues) ---
        Task<List<BaronAudienceDTO>> GetAudiences(int baronyId);
        Task<BaronAudienceDTO> SaveAudience(BaronAudienceDTO dto);
        Task<int> DeleteAudience(int id);
        Task<BaronAudienceDTO> EnsureCouncilSession(int baronyId, int turnNumber, int year, string season);
        Task<BaronAudienceExchangeDTO> SaveAudienceExchange(BaronAudienceExchangeDTO dto);
        Task<int> DeleteAudienceExchange(int id);
        /// <summary>MG: mark deferred. Spawns a continuation on next Resolve Turn.</summary>
        Task<BaronAudienceDTO> DeferAudience(int audienceId);
        Task<BaronAudienceDTO> ResolveAudience(int audienceId, string? gmSummary, string? outcomeNotes);
        Task<BaronAudienceDTO> DismissAudience(int audienceId, string? gmSummary = null);

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
        /// <summary>Expand/shrink map edges. Positive = add tiles, negative = remove.</summary>
        Task<(int Width, int Height)> ResizeTerrainMap(
            int baronyId, int deltaLeft, int deltaRight, int deltaTop, int deltaBottom);
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
        Task<BaronyProjectDTO> AllocateProjectResources(int projectId, PpbVector amounts);
        Task<BaronyProjectDTO> ClearProjectAllocations(int projectId);
        Task<BaronyProjectDTO> SetProjectCostMode(int projectId, string mode);
        Task<int> DeleteProject(int id);

        // --- Army units ---
        Task<List<BaronyUnitDTO>> GetUnits(int baronyId);
        Task<BaronyUnitDTO> SaveUnit(BaronyUnitDTO dto);
        Task<int> DeleteUnit(int id);
        Task<StartUnitTrainingResult> StartUnitTraining(StartUnitTrainingRequest request);
        Task<StartUnitReinforceResult> StartUnitReinforce(StartUnitReinforceRequest request);
        Task<StartUnitChangeEquipmentResult> StartUnitChangeEquipment(StartUnitChangeEquipmentRequest request);
        /// <summary>MG: force a training unit to Active and complete its training project.</summary>
        Task<BaronyUnitDTO> ActivateUnit(int unitId);

        // --- Resources balance custom sources ---
        Task<List<BaronyResourceSourceDTO>> GetResourceSources(int baronyId);
        Task<BaronyResourceSourceDTO> SaveResourceSource(BaronyResourceSourceDTO dto);
        Task<int> DeleteResourceSource(int id);

        // --- Baron purse ledger ---
        Task<List<BaronPurseSourceDTO>> GetPurseSources(int baronyId);
        Task<BaronPurseSourceDTO> SavePurseSource(BaronPurseSourceDTO dto);
        Task<int> DeletePurseSource(int id);

        // --- Towary strategiczne (dostępność w baronii) ---
        /// <summary>MG override keys stored on the barony (not derived production/treaty).</summary>
        Task<HashSet<string>> GetTradeGoodMgOverrideKeys(int baronyId);
        Task SetTradeGoodMgOverrideKeys(int baronyId, IReadOnlyCollection<string> keys);

        /// <summary>Derived availability: produced ∪ treaty-received ∪ MG override.</summary>
        Task<TradeGoodAvailabilitySnapshot> GetTradeGoodAvailability(int baronyId);

        Task<string> GetLuxuryGoodsAccessKey(int baronyId);
        Task SetLuxuryGoodsAccessKey(int baronyId, string key);

        Task<List<BaronyTradeTreaty>> GetTradeTreaties(int baronyId);
        Task SaveTradeTreaties(int baronyId, IReadOnlyList<BaronyTradeTreaty> treaties);
        Task<HashSet<string>> GetBlockedTradeLordKeys(int baronyId);
        Task SetBlockedTradeLordKeys(int baronyId, IReadOnlyCollection<string> lordKeys);

        // --- Katalog budynków/ulepszeń (globalny) ---
        Task<List<BuildingTemplateDTO>> GetBuildingTemplates();
        Task<BuildingTemplateDTO> SaveBuildingTemplate(BuildingTemplateDTO dto);
        Task<int> DeleteBuildingTemplate(int id);
    }
}
