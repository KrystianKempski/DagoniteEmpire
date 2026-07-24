using DA_Common.Barony;

namespace DA_Models.BaronyModels
{
    public class BaronyDTO
    {
        public int Id { get; set; }
        public int CharacterId { get; set; }
        public string Name { get; set; } = "Nowa Baronia";
        public int Size { get; set; }

        public int Year { get; set; } = 625;
        public int Month { get; set; } = 1;
        public int TurnNumber { get; set; } = 1;
        public string Season { get; set; } = "Winter";

        public decimal TreasuryGold { get; set; }
        public decimal BaronPurseGold { get; set; }
        public decimal FoodInGranaries { get; set; }

        /// <summary>Cumulative stocks for Food, Gold, Production, Science, Magic, Culture, Intelligence, Defense.</summary>
        public PpbVector ResourceStocks { get; set; } = new();

        /// <summary>Income from the previous turn (editable on turn 1 for MG starting grants).</summary>
        public PpbVector PreviousTurnIncome { get; set; } = new();

        public int Unrest { get; set; }

        /// <summary>Raw 2d6 at turn start (2–12).</summary>
        public int ConjunctureDice { get; set; } = 7;

        /// <summary>MG fortune modifier added to the dice (war −3, good harvest +2, …).</summary>
        public int ConjunctureModifier { get; set; }

        /// <summary>Effective conjuncture this turn: Dice + Modifier.</summary>
        public int Conjuncture => ConjunctureDice + ConjunctureModifier;

        /// <summary>Share of gross gold income paid to the senior (default 15%). MG-editable on Budget.</summary>
        public decimal LiegeTributePercent { get; set; } = 15m;

        /// <summary>Share of village gold on vassal fiefs kept by the baron (default 15%). MG-editable on Budget.</summary>
        public decimal VassalTributePercent { get; set; } = 15m;

        public int Prestige { get; set; }
        public int Honor { get; set; }
        public int Fear { get; set; }

        public PpbVector BaseParameters { get; set; } = new();

        public string? Notes { get; set; }

        /// <summary>Player marked the current turn as finished; MG may resolve.</summary>
        public bool PlayerTurnReady { get; set; }
    }

    /// <summary>Lightweight barony row for MG list / selector.</summary>
    public class BaronyListItemDTO
    {
        public int Id { get; set; }
        public int CharacterId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string BaronName { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }

    public class AdvisorDTO
    {
        public int Id { get; set; }
        public int BaronyId { get; set; }
        public int? AvailableAdvisorId { get; set; }
        public string OfficeType { get; set; } = DA_Common.Barony.OfficeType.Custom;
        public string Title { get; set; } = string.Empty;
        public string PersonName { get; set; } = string.Empty;
        public bool IsBaron { get; set; }
        public PpbVector Skills { get; set; } = new();
        /// <summary>Administrative skills that affect barony parameters (up to 4).</summary>
        public List<Ppb> SignificantSkills { get; set; } = new();
        public PpbVector Additive { get; set; } = new();
        public PpbVector Percent { get; set; } = new();
        public string? FormulaText { get; set; }
        /// <summary>Office flavor text (custom offices). Core offices use <see cref="DA_Common.Barony.OfficeDescriptions"/>.</summary>
        public string? Description { get; set; }
        /// <summary>Assigned person's bio (from Available Advisors pool; not persisted on Advisor).</summary>
        public string? PersonDescription { get; set; }
        public decimal UpkeepGold { get; set; }
    }

    public class AvailableAdvisorDTO
    {
        public int Id { get; set; }
        public int BaronyId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public PpbVector Skills { get; set; } = new();
    }

    public class BaronyBuildingDTO
    {
        public int Id { get; set; }
        public int BaronyId { get; set; }
        public int? TemplateId { get; set; }

        /// <summary>When set, marks a starter/core city building seeded from the Buildings catalog.</summary>
        public string? CoreKey { get; set; }

        public string Name { get; set; } = string.Empty;
        public string Kind { get; set; } = DA_Common.Barony.BuildingKind.Building;
        public PpbVector Additive { get; set; } = new();
        public PpbVector Percent { get; set; } = new();
        public string? Description { get; set; }

        /// <summary>Display-only population (e.g. town row in City Buildings).</summary>
        public int Population { get; set; }
    }

    public class SocialGroupRelationDTO
    {
        public int Id { get; set; }
        public int BaronyId { get; set; }
        public string Group { get; set; } = string.Empty;
        public int RelationLevel { get; set; }
        public int? InfluencePercent { get; set; }
        public bool? IsActive { get; set; }
        public int? TaxPercent { get; set; }
        public PpbVector Additive { get; set; } = new();
        public PpbVector Percent { get; set; } = new();
        public string? FormulaText { get; set; }
    }

    public class DecreeDTO
    {
        public int Id { get; set; }
        public int BaronyId { get; set; }
        public string Name { get; set; } = string.Empty;
        public PpbVector Additive { get; set; } = new();
        public PpbVector Percent { get; set; } = new();
        public string? Description { get; set; }
        public string? FormulaText { get; set; }

        /// <summary>When false, PPB is excluded from totals.</summary>
        public bool IsActive { get; set; } = true;
    }

    public class BaronyEventDTO
    {
        public int Id { get; set; }
        public int BaronyId { get; set; }
        public string Name { get; set; } = string.Empty;

        /// <summary>First turn on which the event applies (inclusive).</summary>
        public int StartTurn { get; set; } = 1;

        /// <summary>Last turn (inclusive). Null = ongoing / no end.</summary>
        public int? EndTurn { get; set; }

        public PpbVector Additive { get; set; } = new();
        public PpbVector Percent { get; set; } = new();
        public string? Description { get; set; }

        public bool IsActiveOnTurn(int turn) =>
            turn >= StartTurn && (EndTurn is null || turn <= EndTurn.Value);

        public string TurnRangeLabel =>
            EndTurn is int end ? $"{StartTurn}–{end}" : $"{StartTurn}–∞";
    }

    public class CommunityModifierDTO
    {
        public int Id { get; set; }
        public int BaronyId { get; set; }
        public string Source { get; set; } = string.Empty;
        public PpbVector Additive { get; set; } = new();
        public PpbVector Percent { get; set; } = new();
        public string? FormulaText { get; set; }
    }

    public class BaronyResourceSourceDTO
    {
        public int Id { get; set; }
        public int BaronyId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }

        /// <summary>Resource deltas. Positive = income, negative = expense.</summary>
        public PpbVector Additive { get; set; } = new();
    }

    public class BaronPurseSourceDTO
    {
        public int Id { get; set; }
        public int BaronyId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }

        /// <summary>Gold delta. Positive = income, negative = expense.</summary>
        public decimal Amount { get; set; }
    }

    public class FiefDTO
    {
        public int Id { get; set; }
        public int BaronyId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string LiegeName { get; set; } = string.Empty;
        public bool IsBaronDemesne { get; set; }
        public bool IsDomainDefault { get; set; }
        public int? SeniorDomainId { get; set; }
        public string ColorHex { get; set; } = "#4d7ea8";
        public decimal BonusMultiplier { get; set; } = 1.0m;
    }

    public class TerrainTileDTO
    {
        public int Id { get; set; }
        public int BaronyId { get; set; }
        public int MapId { get; set; } = 1;
        public int X { get; set; }
        public int Y { get; set; }
        public string BaseType { get; set; } = DA_Common.Barony.TerrainBaseType.Plains;
        /// <summary>Feature bit flags (TerrainFeature).</summary>
        public int FeaturesMask { get; set; }
        public int Fertility { get; set; }
        public string? Resource { get; set; }
        public int? FiefId { get; set; }
        public int? MapDomainId { get; set; }
        public string? Comment { get; set; }

        public bool HasFeature(int flag) => DA_Common.Barony.TerrainFeature.Has(FeaturesMask, flag);
    }

    public class TerrainMapDomainDTO
    {
        public int Id { get; set; }
        public int BaronyId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string LordName { get; set; } = string.Empty;
        public string ColorHex { get; set; } = "#888888";
        public bool IsPrimary { get; set; }
        public int SortOrder { get; set; }
    }

    public class TerrainImprovementDTO
    {
        public int Id { get; set; }
        public int BaronyId { get; set; }
        public int? TileId { get; set; }
        public int? TemplateId { get; set; }
        public string Name { get; set; } = string.Empty;
        public PpbVector Additive { get; set; } = new();
        public PpbVector Percent { get; set; } = new();
        public string? Description { get; set; }
        public string? FormulaText { get; set; }

        /// <summary>When false, PPB is excluded from totals and shown struck through.</summary>
        public bool IsActive { get; set; } = true;

        /// <summary>Why inactive — shown on Domain Panel name hover.</summary>
        public string? InactiveReason { get; set; }

        /// <summary>Optional icon override for custom map improvements.</summary>
        public string? IconUrl { get; set; }

        /// <summary>Settlement population (villages and towns).</summary>
        public int Population { get; set; }

        /// <summary>Village palisade (+5 Def, +3 Stab, +1 Law).</summary>
        public bool HasPalisade { get; set; }
    }

    public class BaronyProjectDTO
    {
        public int Id { get; set; }
        public int BaronyId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string OutputKind { get; set; } = DA_Common.Barony.ProjectOutputKind.DecreeOrTechnology;
        public PpbVector CostGoldProduction { get; set; } = new();
        public PpbVector CostMaterials { get; set; } = new();
        public string AllowedCostModes { get; set; } = DA_Common.Barony.ProjectAllowedCostModes.PlayerChoice;
        public string? SelectedCostMode { get; set; }
        public PpbVector ResultAdditive { get; set; } = new();
        public PpbVector ResultPercent { get; set; } = new();
        public PpbVector Allocated { get; set; } = new();
        public string ResultDescription { get; set; } = string.Empty;
        public string Status { get; set; } = DA_Common.Barony.ProjectStatus.Draft;
        public int TurnsRemaining { get; set; }
        public string? Notes { get; set; }

        /// <summary>When set, this project is a map construction on that terrain tile.</summary>
        public int? TileId { get; set; }

        /// <summary>Building/improvement catalog entry used for map construction costs &amp; effects.</summary>
        public int? BuildingTemplateId { get; set; }

        /// <summary>Army unit linked to a Unit Training project.</summary>
        public int? UnitId { get; set; }

        /// <summary>True while a map construction project is still active (shows crane on the tile).</summary>
        public bool IsActiveMapConstruction =>
            TileId is > 0
            && Status is not (DA_Common.Barony.ProjectStatus.Completed or DA_Common.Barony.ProjectStatus.Cancelled);

        /// <summary>True when both Gold+Production and Materials must be paid together.</summary>
        public bool IsCombinedCost =>
            string.Equals(
                AllowedCostModes,
                DA_Common.Barony.ProjectAllowedCostModes.Combined,
                StringComparison.OrdinalIgnoreCase);

        /// <summary>Active payment track after GM rules and player selection.</summary>
        public string EffectiveCostMode => ResolveEffectiveCostMode();

        public IReadOnlyList<PpbInfo> ActiveCostColumns =>
            IsCombinedCost || EffectiveCostMode == DA_Common.Barony.ProjectCostMode.Combined
                ? ProjectCostCatalog.CombinedActiveColumns(CostGoldProduction, CostMaterials)
                : EffectiveCostMode == DA_Common.Barony.ProjectCostMode.GoldProduction
                    ? ProjectCostCatalog.GoldProduction
                    : ProjectCostCatalog.Materials;

        public PpbVector GetActiveCost() =>
            IsCombinedCost || EffectiveCostMode == DA_Common.Barony.ProjectCostMode.Combined
                ? ProjectCostCatalog.MergeTracks(CostGoldProduction, CostMaterials)
                : EffectiveCostMode == DA_Common.Barony.ProjectCostMode.GoldProduction
                    ? ProjectCostCatalog.SliceGoldProduction(CostGoldProduction)
                    : ProjectCostCatalog.SliceMaterials(CostMaterials);

        public List<string> GetSelectableCostModes()
        {
            var hasGoldProduction = ProjectCostCatalog.HasRequirement(CostGoldProduction);
            var hasMaterials = ProjectCostCatalog.HasRequirement(CostMaterials);

            return AllowedCostModes switch
            {
                DA_Common.Barony.ProjectAllowedCostModes.Combined =>
                    hasGoldProduction || hasMaterials
                        ? new List<string> { DA_Common.Barony.ProjectCostMode.Combined }
                        : new List<string>(),
                DA_Common.Barony.ProjectAllowedCostModes.GoldProductionOnly when hasGoldProduction =>
                    new List<string> { DA_Common.Barony.ProjectCostMode.GoldProduction },
                DA_Common.Barony.ProjectAllowedCostModes.MaterialsOnly when hasMaterials =>
                    new List<string> { DA_Common.Barony.ProjectCostMode.Materials },
                _ => BuildChoiceModes(hasGoldProduction, hasMaterials),
            };
        }

        public bool HasAllocationOnActiveTrack() =>
            ActiveCostColumns.Any(info => Allocated[info.Key] > 0m);

        public bool CanSwitchCostMode =>
            !IsCombinedCost
            && Status is not (ProjectStatus.Completed or ProjectStatus.Cancelled)
            && GetSelectableCostModes().Count > 1
            && !HasAllocationOnActiveTrack();

        /// <summary>Procent alokacji względem kosztu aktywnej ścieżki (0-100).</summary>
        public int AllocationPercent
        {
            get
            {
                var cost = GetActiveCost();
                var ratios = new List<decimal>();
                foreach (var info in ActiveCostColumns)
                {
                    var required = cost[info.Key];
                    if (required <= 0m)
                        continue;
                    ratios.Add(Math.Clamp(Allocated[info.Key] / required, 0m, 1m));
                }

                if (ratios.Count == 0)
                    return ProjectCostCatalog.HasRequirement(cost) || ActiveCostColumns.Count > 0 ? 0 : 100;
                return (int)Math.Round(ratios.Average() * 100m);
            }
        }

        /// <summary>Remaining cost on the active payment track(s).</summary>
        public PpbVector RemainingCost()
        {
            var cost = GetActiveCost();
            var v = new PpbVector();
            foreach (var info in ActiveCostColumns)
                v[info.Key] = Math.Max(0m, cost[info.Key] - Allocated[info.Key]);
            return v;
        }

        public bool HasRemainingCost =>
            ActiveCostColumns.Any(info => RemainingCost()[info.Key] > 0m);

        public bool CanAcceptAllocation =>
            Status is not (ProjectStatus.Completed or ProjectStatus.Cancelled) && HasRemainingCost;

        public bool HasAnyAllocation =>
            ResourceCatalog.All.Any(info => Allocated[info.Key] > 0m);

        public bool CanClearAllocation =>
            Status == ProjectStatus.Draft && HasAnyAllocation;

        /// <summary>
        /// Negative Resources balance row: allocated + remaining on active track.
        /// Only when at least one resource was allocated; otherwise no stock impact.
        /// </summary>
        public PpbVector ResourcesBalanceImpact()
        {
            var v = new PpbVector();
            if (!HasAnyAllocation)
                return v;
            if (Status is ProjectStatus.Completed or ProjectStatus.Cancelled)
                return v;

            var remaining = RemainingCost();
            foreach (var info in ResourceCatalog.All)
                v[info.Key] -= Allocated[info.Key] + remaining[info.Key];
            return v;
        }

        private string ResolveEffectiveCostMode()
        {
            if (IsCombinedCost)
                return DA_Common.Barony.ProjectCostMode.Combined;

            var selectable = GetSelectableCostModes();
            if (selectable.Count == 0)
                return DA_Common.Barony.ProjectCostMode.GoldProduction;
            if (selectable.Count == 1)
                return selectable[0];
            if (!string.IsNullOrWhiteSpace(SelectedCostMode) && selectable.Contains(SelectedCostMode))
                return SelectedCostMode;
            return selectable[0];
        }

        private static List<string> BuildChoiceModes(bool hasGoldProduction, bool hasMaterials)
        {
            var modes = new List<string>();
            if (hasGoldProduction)
                modes.Add(DA_Common.Barony.ProjectCostMode.GoldProduction);
            if (hasMaterials)
                modes.Add(DA_Common.Barony.ProjectCostMode.Materials);
            return modes;
        }
    }

    public class BaronyRelationModifierDTO
    {
        public int Id { get; set; }
        public string Description { get; set; } = string.Empty;
        public int Value { get; set; }
        public int SortOrder { get; set; }
    }

    public class BaronyRelationDTO
    {
        public int Id { get; set; }
        public int BaronyId { get; set; }
        public string Category { get; set; } = DA_Common.Barony.RelationCategory.Acquaintances;
        public string GroupName { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public int? Age { get; set; }
        public string Description { get; set; } = string.Empty;
        public int TroopCount { get; set; }
        public string RelationDescription { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public int SortOrder { get; set; }
        /// <summary>Terrain fief this Vassals contact is synced from (null = manual entry).</summary>
        public int? FiefId { get; set; }
        public List<BaronyRelationModifierDTO> Modifiers { get; set; } = new();

        /// <summary>Sum of modifier values, clamped to −200…200.</summary>
        public int Attitude
        {
            get
            {
                var sum = Modifiers?.Sum(m => m.Value) ?? 0;
                return Math.Clamp(sum, -200, 200);
            }
        }

        public string AttitudeLabel => DA_Common.Barony.RelationCategory.AttitudeLabel(Attitude);
    }

    public class BuildingTemplateDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int RequiredLordshipLevel { get; set; }
        public string Kind { get; set; } = DA_Common.Barony.BuildingKind.Building;
        public decimal GoldCost { get; set; }
        public decimal ProductionCost { get; set; }
        public PpbVector EffectAdditive { get; set; } = new();
        public PpbVector EffectPercent { get; set; } = new();
        public string? Description { get; set; }
        public string? TerrainRequirement { get; set; }
    }

    public class BaronySeatDTO
    {
        public int Id { get; set; }
        public int BaronyId { get; set; }
        public string Name { get; set; } = "Lord's Seat";
        public int GridWidth { get; set; } = 12;
        public int GridHeight { get; set; } = 8;
        /// <summary>Active floor levels, sorted ascending. Always at least ground (0).</summary>
        public List<int> ActiveLevels { get; set; } = new() { SeatFloorLevel.Ground };
        public List<SeatRoomDTO> Rooms { get; set; } = new();
        public List<SeatTileDTO> Tiles { get; set; } = new();
    }

    public class SeatTileDTO
    {
        public int Id { get; set; }
        public int SeatId { get; set; }
        public int Level { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public string Kind { get; set; } = SeatTileKind.Ground;
    }

    public class SeatRoomTraitDTO
    {
        public int Id { get; set; }
        public string Kind { get; set; } = SeatRoomTraitKind.Advantage;
        public string Text { get; set; } = string.Empty;
        public int SortOrder { get; set; }
    }

    public class SeatRoomDTO
    {
        public int Id { get; set; }
        public int SeatId { get; set; }
        public int BaronyId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Level { get; set; }
        public int GridX { get; set; }
        public int GridY { get; set; }
        public int GridW { get; set; } = 1;
        public int GridH { get; set; } = 1;
        public string Material { get; set; } = SeatRoomMaterial.Stone;
        public decimal PrestigeMultiplier { get; set; } = 1m;
        public string Status { get; set; } = SeatRoomStatus.Active;
        public PpbVector Additive { get; set; } = new();
        public PpbVector Percent { get; set; } = new();
        public int? PurposeTemplateId { get; set; }
        public int? OccupantAdvisorId { get; set; }
        public string OccupantCustom { get; set; } = string.Empty;
        public int SortOrder { get; set; }
        public List<SeatRoomTraitDTO> Traits { get; set; } = new();

        public int TileCount => Math.Max(0, GridW) * Math.Max(0, GridH);

        public string SizeCategory => SeatRoomSizeCategory.FromTileCount(TileCount);

        public bool IsRuin => string.Equals(Status, SeatRoomStatus.Ruin, StringComparison.Ordinal);

        public bool ContributesPpb => !IsRuin;
    }

    public class SeatRoomPurposeAssignmentDTO
    {
        public int? PurposeTemplateId { get; set; }
        public int? OccupantAdvisorId { get; set; }
        public string? OccupantCustom { get; set; }
    }

    public class SeatPurposeTemplateDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string MinSizeCategory { get; set; } = SeatRoomSizeCategory.Small;
        public string WhoOccupies { get; set; } = string.Empty;
        public int SleepCapacity { get; set; }
        public decimal AdditivePrestige { get; set; }
        public PpbVector Additive { get; set; } = new();
        public PpbVector Percent { get; set; } = new();
        public bool IsUniversal { get; set; } = true;
        public int? BaronyId { get; set; }
        public int SortOrder { get; set; }
    }

    /// <summary>Result of MG resolving a barony turn.</summary>
    public class TurnResolveReportDTO
    {
        public int BaronyId { get; set; }
        public int PreviousTurnNumber { get; set; }
        public int NewTurnNumber { get; set; }
        public string NewSeason { get; set; } = string.Empty;
        public int NewYear { get; set; }
        public int NewMonth { get; set; }
        public PpbVector AppliedIncome { get; set; } = new();
        public List<string> CompletedProjects { get; set; } = new();
        public int Size { get; set; }
        public int ControlDc { get; set; }
        public int SettlementPopulation { get; set; }
        public bool LoyaltyTestRan { get; set; }
        public decimal Loyalty { get; set; }
        public decimal Stability { get; set; }
        public int? LoyaltyD20 { get; set; }
        public int? LoyaltyTestResult { get; set; }
        public int UnrestBefore { get; set; }
        public int UnrestAfter { get; set; }
        public int UnrestDelta { get; set; }
        public int NewConjunctureDice { get; set; }
        public int ConjunctureModifier { get; set; }
        public string SummaryText { get; set; } = string.Empty;
    }
}
