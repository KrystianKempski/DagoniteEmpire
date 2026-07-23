using DA_Common;
using DA_Common.Barony;
using DA_Models.BaronyModels;
using DA_Models.CharacterModels;

namespace DagoniteEmpire.Pages.Barony
{
    /// <summary>
    /// Helper calculations for the Domain Panel: builds PPB table rows,
    /// section sums and total summary.
    /// Note: exact formulas will be added later - currently using:
    /// (base + Σ additive) * (1 + Σ percent/100).
    /// </summary>
    public static class BaronyCalc
    {
        public static PpbModifierRow Row(string label, PpbVector additive, PpbVector percent, string? formula = null, string? note = null)
            => new() { Label = label, Additive = additive ?? new PpbVector(), Percent = percent ?? new PpbVector(), Formula = formula, Note = note };

        public static List<PpbModifierRow> AdvisorRows(IEnumerable<AdvisorDTO> advisors)
            => advisors.Select(a => Row(
                    $"{AdvisorRoleLabel(a)} - {DisplayName(a)}",
                    a.Additive, a.Percent, a.FormulaText, a.Description))
                .ToList();

        public static List<AdvisorDTO> OrderAdvisors(IEnumerable<AdvisorDTO>? advisors)
        {
            var list = advisors?.ToList() ?? new List<AdvisorDTO>();
            return list
                .OrderBy(RankAdvisor)
                .ThenBy(a => a.Title, StringComparer.OrdinalIgnoreCase)
                .ThenBy(a => a.PersonName, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        public static string AdvisorRoleLabel(AdvisorDTO advisor)
        {
            if (!string.IsNullOrWhiteSpace(advisor.Title))
                return advisor.Title;

            return advisor.OfficeType switch
            {
                OfficeType.Baron => "Baron",
                OfficeType.Chancellor => "Chancellor",
                OfficeType.GuardCaptain => "Guard Captain",
                OfficeType.Steward => "Steward",
                _ => "Advisor",
            };
        }

        public static PpbVector SumAdvisorAdditive(IEnumerable<AdvisorDTO> advisors)
            => PpbVector.Sum(advisors.Select(a => a.Additive));

        public static PpbVector SumAdvisorPercent(IEnumerable<AdvisorDTO> advisors)
            => PpbVector.Sum(advisors.Select(a => a.Percent));

        /// <summary>
        /// Domain Panel list: baron row from character skills + non-baron advisors with skill mapping applied.
        /// </summary>
        public static List<AdvisorDTO> AdvisorsForDomainPanel(
            IEnumerable<AdvisorDTO>? advisors,
            BaronyDTO barony,
            CharacterDTO? character,
            IEnumerable<BaronInfluenceModifierDTO>? baronModifiers,
            IEnumerable<AdvisorInfluenceModifierDTO>? advisorModifiers = null,
            int managementJc = BaronTimeRules.RequiredManagementJc)
        {
            var modsByAdvisor = (advisorModifiers ?? Enumerable.Empty<AdvisorInfluenceModifierDTO>())
                .GroupBy(m => m.AdvisorId)
                .ToDictionary(g => g.Key, g => (IEnumerable<AdvisorInfluenceModifierDTO>)g.ToList());

            var offices = OrderAdvisors(advisors?.Where(a => !a.IsBaron))
                .Select(a =>
                {
                    var row = CloneAdvisorForPanel(a);
                    var mods = modsByAdvisor.GetValueOrDefault(a.Id);
                    ApplyAdvisorSkillInfluence(row, mods);
                    ApplyOfficeGoldCostToPanelRow(row, mods);
                    return row;
                })
                .ToList();

            var existingBaron = advisors?.FirstOrDefault(a => a.IsBaron);
            offices.Insert(0, BuildBaronAdvisorRow(
                barony, character, baronModifiers, existingBaron, managementJc));
            return offices;
        }

        private static AdvisorDTO CloneAdvisorForPanel(AdvisorDTO source) => new()
        {
            Id = source.Id,
            BaronyId = source.BaronyId,
            AvailableAdvisorId = source.AvailableAdvisorId,
            OfficeType = source.OfficeType,
            Title = source.Title,
            PersonName = source.PersonName,
            IsBaron = source.IsBaron,
            Skills = source.Skills.Clone(),
            SignificantSkills = source.SignificantSkills.ToList(),
            FormulaText = source.FormulaText,
            Description = source.Description,
            UpkeepGold = source.UpkeepGold,
        };

        public static AdvisorDTO BuildBaronAdvisorRow(
            BaronyDTO barony,
            CharacterDTO? character,
            IEnumerable<BaronInfluenceModifierDTO>? baronModifiers,
            AdvisorDTO? existingBaronAdvisor = null,
            int managementJc = BaronTimeRules.RequiredManagementJc)
        {
            // Same skill-unit total as Baron Card (skills ± management + PHP + custom sources),
            // then Domain Panel Additive/Percent from the skill→PPB formulas.
            var influenceRows = BuildInfluenceRows(
                character,
                barony.Prestige,
                barony.Honor,
                barony.Fear,
                baronModifiers,
                managementJc);
            var totalSkills = SumInfluenceRows(influenceRows);

            var name = !string.IsNullOrWhiteSpace(character?.NPCName)
                ? character!.NPCName
                : !string.IsNullOrWhiteSpace(existingBaronAdvisor?.PersonName)
                    ? existingBaronAdvisor!.PersonName
                    : "Baron";

            var factor = BaronTimeRules.ManagementSkillFactor(managementJc);
            var factorPct = decimal.Round(factor * 100m, 0, MidpointRounding.AwayFromZero);

            return new AdvisorDTO
            {
                Id = existingBaronAdvisor?.Id ?? 0,
                BaronyId = barony.Id,
                OfficeType = OfficeType.Baron,
                Title = "Baron",
                PersonName = name,
                IsBaron = true,
                Additive = BaronSkillPpbFormulas.MapToAdvisorAdditive(totalSkills),
                Percent = BaronSkillPpbFormulas.MapToAdvisorPercent(totalSkills),
                Description = BaronSkillPpbFormulas.BaronAdvisorNameTooltip
                    + (factor < 1m
                        ? $" Skill PPB applied at {factorPct}% ({managementJc}/{BaronTimeRules.RequiredManagementJc} management JC)."
                        : ""),
            };
        }

        private static int RankAdvisor(AdvisorDTO advisor)
        {
            if (advisor.IsBaron)
                return 0;

            return advisor.OfficeType switch
            {
                OfficeType.Chancellor => 1,
                OfficeType.GuardCaptain => 2,
                OfficeType.Steward => 3,
                _ => 10,
            };
        }

        private static string DisplayName(AdvisorDTO advisor)
            => string.IsNullOrWhiteSpace(advisor.PersonName) ? "Unassigned" : advisor.PersonName;

        public static List<PpbModifierRow> BuildingRows(IEnumerable<BaronyBuildingDTO> buildings)
            => buildings.Select(b => Row(b.Name, b.Additive, b.Percent, null, b.Description)).ToList();

        /// <summary>Default fixed city buildings. MG overrides are stored in BaronyBuildings via CoreKey.</summary>
        public static IReadOnlyList<BaronyBuildingDTO> CoreCityBuildings(int baronyId)
        {
            static PpbVector A(
                decimal? food = null, decimal? economy = null, decimal? production = null,
                decimal? loyalty = null, decimal? stability = null, decimal? law = null,
                decimal? corruption = null, decimal? science = null, decimal? magic = null,
                decimal? culture = null, decimal? intelligence = null, decimal? defense = null,
                decimal? treasury = null)
            {
                var v = new PpbVector();
                if (food.HasValue) v[Ppb.Food] = food.Value;
                if (economy.HasValue) v[Ppb.Economy] = economy.Value;
                if (production.HasValue) v[Ppb.Production] = production.Value;
                if (loyalty.HasValue) v[Ppb.Loyalty] = loyalty.Value;
                if (stability.HasValue) v[Ppb.Stability] = stability.Value;
                if (law.HasValue) v[Ppb.Law] = law.Value;
                if (corruption.HasValue) v[Ppb.Corruption] = corruption.Value;
                if (science.HasValue) v[Ppb.Science] = science.Value;
                if (magic.HasValue) v[Ppb.Magic] = magic.Value;
                if (culture.HasValue) v[Ppb.Culture] = culture.Value;
                if (intelligence.HasValue) v[Ppb.Intelligence] = intelligence.Value;
                if (defense.HasValue) v[Ppb.Defense] = defense.Value;
                if (treasury.HasValue) v[Ppb.Treasury] = treasury.Value;
                return v;
            }

            return new[]
            {
                new BaronyBuildingDTO
                {
                    BaronyId = baronyId,
                    Name = "Steward's Building",
                    CoreKey = CoreCityBuildingKey.StewardsBuilding,
                    Kind = BuildingKind.Building,
                    Additive = A(
                        stability: 3, law: 3, corruption: 2,
                        science: 2, culture: 2, intelligence: 2,
                        defense: 5, treasury: -15),
                    Description =
                        "Steward's hut. Locals come here with their affairs before the authorities. "
                        + "Officials and tax collectors hold office here. Can be upgraded to a town hall.",
                },
                new BaronyBuildingDTO
                {
                    BaronyId = baronyId,
                    Name = "Tavern",
                    CoreKey = CoreCityBuildingKey.Tavern,
                    Kind = BuildingKind.Building,
                    Additive = A(economy: 2, intelligence: 3, loyalty: 3),
                    Description =
                        "A humble tavern. Meeting place for the peasantry and a rest stop for the few "
                        + "traveling merchants. Can be upgraded to an inn.",
                },
                new BaronyBuildingDTO
                {
                    BaronyId = baronyId,
                    Name = "Market Square",
                    CoreKey = CoreCityBuildingKey.MarketSquare,
                    Kind = BuildingKind.Building,
                    Additive = A(economy: 3, production: 3, treasury: 10),
                    Description =
                        "A small paved place where local producers and nearby merchants can exchange goods. "
                        + "Can be upgraded to a marketplace.",
                },
            };
        }


        /// <summary>Core defaults merged with per-barony MG overrides (matched by CoreKey).</summary>
        public static IReadOnlyList<BaronyBuildingDTO> EffectiveCoreCityBuildings(
            int baronyId,
            IEnumerable<BaronyBuildingDTO>? saved)
        {
            var overrides = (saved ?? Enumerable.Empty<BaronyBuildingDTO>())
                .Where(b => !string.IsNullOrWhiteSpace(b.CoreKey))
                .GroupBy(b => b.CoreKey!, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

            return CoreCityBuildings(baronyId).Select(def =>
            {
                if (!overrides.TryGetValue(def.CoreKey!, out var ov))
                    return def;

                return new BaronyBuildingDTO
                {
                    Id = ov.Id,
                    BaronyId = baronyId,
                    CoreKey = def.CoreKey,
                    TemplateId = null,
                    Name = string.IsNullOrWhiteSpace(ov.Name) ? def.Name : ov.Name,
                    Kind = BuildingKind.Building,
                    Description = ov.Description,
                    Additive = ov.Additive.Clone(),
                    Percent = ov.Percent.Clone(),
                    Population = ov.Population,
                };
            }).ToList();
        }

        /// <summary>Saved buildings that are not core overrides (catalog / custom adds).</summary>
        public static IEnumerable<BaronyBuildingDTO> ExtraCityBuildings(IEnumerable<BaronyBuildingDTO>? saved)
            => (saved ?? Enumerable.Empty<BaronyBuildingDTO>())
                .Where(b => string.IsNullOrWhiteSpace(b.CoreKey));

                public static IReadOnlyDictionary<int, SeatPurposeTemplateDTO> PurposeLookup(
            IEnumerable<SeatPurposeTemplateDTO>? templates) =>
            (templates ?? Enumerable.Empty<SeatPurposeTemplateDTO>())
                .ToDictionary(t => t.Id);

        public static (PpbVector Additive, PpbVector Percent) SeatRoomEffectivePpb(
            SeatRoomDTO room,
            IReadOnlyDictionary<int, SeatPurposeTemplateDTO> purposes)
        {
            if (!room.ContributesPpb)
                return (new PpbVector(), new PpbVector());

            var add = room.Additive.Clone();
            var pct = room.Percent.Clone();
            if (room.PurposeTemplateId is int pid && purposes.TryGetValue(pid, out var purpose))
            {
                add = PpbVector.Sum(new[] { add, purpose.Additive });
                pct = PpbVector.Sum(new[] { pct, purpose.Percent });
            }

            return (add, pct);
        }

        public static PpbModifierRow LordsSeatSummaryRow(
            IEnumerable<SeatRoomDTO> rooms,
            IReadOnlyDictionary<int, SeatPurposeTemplateDTO> purposes)
        {
            var active = rooms.Where(r => r.ContributesPpb).ToList();
            if (active.Count == 0)
            {
                return Row("Lord's Seat", new PpbVector(), new PpbVector(), null,
                    "No active chambers. Ruins and unassigned rooms contribute nothing until restored.");
            }

            var add = PpbVector.Sum(active.Select(r => SeatRoomEffectivePpb(r, purposes).Additive));
            var pct = PpbVector.Sum(active.Select(r => SeatRoomEffectivePpb(r, purposes).Percent));
            var lines = active.Select(r =>
            {
                var purposeName = r.PurposeTemplateId is int pid && purposes.TryGetValue(pid, out var p)
                    ? p.Name
                    : "Unassigned";
                return $"• {r.Name} — {purposeName} ({r.SizeCategory}, {r.TileCount} tiles)";
            });
            return Row("Lord's Seat", add, pct, null, string.Join("\n", lines));
        }

        public static List<PpbModifierRow> LordsSeatDetailRows(
            IEnumerable<SeatRoomDTO> rooms,
            IReadOnlyDictionary<int, SeatPurposeTemplateDTO> purposes) =>
            rooms
                .OrderBy(r => r.SortOrder)
                .ThenBy(r => r.Name, StringComparer.OrdinalIgnoreCase)
                .Select(r =>
                {
                    var (add, pct) = SeatRoomEffectivePpb(r, purposes);
                    var purposeName = r.PurposeTemplateId is int pid && purposes.TryGetValue(pid, out var p)
                        ? p.Name
                        : "—";
                    var label = r.IsRuin ? $"{r.Name} (ruin)" : r.Name;
                    var desc = $"{purposeName} · {r.SizeCategory} · {r.Material}";
                    if (r.IsRuin)
                        desc = "Ruin — excluded from PPB.\n" + desc;
                    return Row(label, add, pct, null, desc);
                })
                .ToList();

        /// <summary>Core buildings plus map towns and catalog instances saved for the barony.</summary>
        public static List<PpbModifierRow> CityBuildingSectionRows(
            int baronyId,
            IEnumerable<BaronyBuildingDTO> saved,
            IEnumerable<TerrainImprovementDTO>? improvements = null,
            BaronySeatDTO? seat = null,
            IEnumerable<SeatPurposeTemplateDTO>? purposeTemplates = null)
        {
            var purposes = PurposeLookup(purposeTemplates);
            var rows = EffectiveCoreCityBuildings(baronyId, saved)
                .Select(b => Row(b.Name, b.Additive, b.Percent, null, b.Description))
                .ToList();
            if (seat is not null)
                rows.Add(LordsSeatSummaryRow(seat.Rooms, purposes));
            rows.AddRange(TownPopulationRows(improvements));
            rows.AddRange(BuildingRows(ExtraCityBuildings(saved)));
            return rows;
        }

        public static IEnumerable<TerrainImprovementDTO> ActiveTowns(IEnumerable<TerrainImprovementDTO>? improvements) =>
            (improvements ?? Enumerable.Empty<TerrainImprovementDTO>())
                .Where(i => IsTown(i) && i.IsActive);

        public static bool IsTown(TerrainImprovementDTO improvement) =>
            string.Equals(improvement.Name, MapImprovement.Town, StringComparison.OrdinalIgnoreCase);

        public static bool IsVillage(TerrainImprovementDTO improvement) =>
            string.Equals(improvement.Name, MapImprovement.Village, StringComparison.OrdinalIgnoreCase);

        public static string TownPopulationLabel(TerrainImprovementDTO town) =>
            TownPpbFormulas.PopulationRowLabel(ImprovementDisplayLabel(town));

        public static List<PpbModifierRow> TownPopulationRows(IEnumerable<TerrainImprovementDTO>? improvements) =>
            ActiveTowns(improvements)
                .OrderBy(TownPopulationLabel, StringComparer.OrdinalIgnoreCase)
                .Select(t => Row(TownPopulationLabel(t), t.Additive, t.Percent, t.FormulaText, t.Description))
                .ToList();

        public static PpbVector SumCityBuildings(
            int baronyId,
            IEnumerable<BaronyBuildingDTO> saved,
            IEnumerable<TerrainImprovementDTO>? improvements = null,
            BaronySeatDTO? seat = null,
            IEnumerable<SeatPurposeTemplateDTO>? purposeTemplates = null)
        {
            var purposes = PurposeLookup(purposeTemplates);
            var vectors = EffectiveCoreCityBuildings(baronyId, saved).Select(b => b.Additive)
                .Concat(ActiveTowns(improvements).Select(t => t.Additive))
                .Concat(ExtraCityBuildings(saved).Select(b => b.Additive));
            if (seat is not null)
            {
                vectors = vectors.Concat(
                    seat.Rooms.Where(r => r.ContributesPpb)
                        .Select(r => SeatRoomEffectivePpb(r, purposes).Additive));
            }

            return PpbVector.Sum(vectors);
        }

        public static PpbVector SumCityBuildingsPercent(
            int baronyId,
            IEnumerable<BaronyBuildingDTO> saved,
            IEnumerable<TerrainImprovementDTO>? improvements = null,
            BaronySeatDTO? seat = null,
            IEnumerable<SeatPurposeTemplateDTO>? purposeTemplates = null)
        {
            var purposes = PurposeLookup(purposeTemplates);
            var vectors = EffectiveCoreCityBuildings(baronyId, saved).Select(b => b.Percent)
                .Concat(ActiveTowns(improvements).Select(t => t.Percent))
                .Concat(ExtraCityBuildings(saved).Select(b => b.Percent));
            if (seat is not null)
            {
                vectors = vectors.Concat(
                    seat.Rooms.Where(r => r.ContributesPpb)
                        .Select(r => SeatRoomEffectivePpb(r, purposes).Percent));
            }

            return PpbVector.Sum(vectors);
        }

        public static int SumCityPopulation(
            int baronyId,
            IEnumerable<BaronyBuildingDTO> saved,
            IEnumerable<TerrainImprovementDTO>? improvements = null) =>
            EffectiveCoreCityBuildings(baronyId, saved).Sum(b => b.Population)
            + ActiveTowns(improvements).Sum(t => t.Population)
            + ExtraCityBuildings(saved).Sum(b => b.Population);

        /// <summary>Towns, city buildings, and villages — population for Economy conjuncture.</summary>
        public static int SumSettlementPopulation(
            int baronyId,
            IEnumerable<BaronyBuildingDTO> saved,
            IEnumerable<TerrainImprovementDTO>? improvements = null) =>
            SumCityPopulation(baronyId, saved, improvements)
            + ActiveVillages(improvements).Sum(v => v.Population);

        public static IEnumerable<TerrainImprovementDTO> ActiveVillages(IEnumerable<TerrainImprovementDTO>? improvements) =>
            (improvements ?? Enumerable.Empty<TerrainImprovementDTO>())
                .Where(i => IsVillage(i) && i.IsActive);

        public static List<PpbModifierRow> SocialRows(int baronyId, IEnumerable<SocialGroupRelationDTO> relations)
            => SocialGroupSectionRows(baronyId, relations)
                .Where(r => r.IsActive)
                .Select(r => Row(
                    $"{r.Group} ({r.RelationDisplay})",
                    r.Additive,
                    r.Percent))
                .ToList();

        /// <summary>Fixed social groups merged with saved relation data.</summary>
        public static IReadOnlyList<SocialGroupRow> SocialGroupSectionRows(int baronyId, IEnumerable<SocialGroupRelationDTO> saved)
        {
            var byGroup = new Dictionary<string, SocialGroupRelationDTO>(StringComparer.OrdinalIgnoreCase);
            foreach (var relation in saved)
            {
                var key = SocialGroup.NormalizeKey(relation.Group);
                byGroup.TryAdd(key, relation);
            }

            return SocialGroup.All.Select(group =>
            {
                byGroup.TryGetValue(group, out var db);
                var relationScore = db?.RelationLevel ?? 0;
                return new SocialGroupRow
                {
                    Id = db?.Id ?? 0,
                    BaronyId = baronyId,
                    Group = group,
                    InfluencePercent = db?.InfluencePercent ?? SocialGroup.DefaultInfluence(group),
                    IsActive = db?.IsActive ?? SocialGroup.DefaultIsActive(group),
                    RelationScore = relationScore,
                    TaxPercent = (int)(db?.TaxPercent ?? SocialGroup.DefaultTax(group)),
                    Additive = SocialGroupPpbFormulas.ComputeAdditive(group, relationScore),
                    Percent = SocialGroupPpbFormulas.ComputePercent(group, relationScore),
                };
            }).ToList();
        }

        public static void ApplyComputedPpb(SocialGroupRelationDTO dto)
        {
            dto.Additive = SocialGroupPpbFormulas.ComputeAdditive(dto.Group, dto.RelationLevel);
            dto.Percent = SocialGroupPpbFormulas.ComputePercent(dto.Group, dto.RelationLevel);
        }

        public static SocialGroupRelationDTO ToRelationDto(SocialGroupRow row)
        {
            var dto = new SocialGroupRelationDTO
            {
                Id = row.Id,
                BaronyId = row.BaronyId,
                Group = row.Group,
                RelationLevel = row.RelationScore,
                InfluencePercent = row.InfluencePercent,
                IsActive = row.IsActive,
                TaxPercent = row.TaxPercent,
            };
            ApplyComputedPpb(dto);
            return dto;
        }

        public static PpbVector SumSocialGroupsAdditive(int baronyId, IEnumerable<SocialGroupRelationDTO> saved)
            => PpbVector.Sum(SocialGroupSectionRows(baronyId, saved).Where(r => r.IsActive).Select(r => r.Additive));

        public static PpbVector SumSocialGroupsPercent(int baronyId, IEnumerable<SocialGroupRelationDTO> saved)
            => PpbVector.Sum(SocialGroupSectionRows(baronyId, saved).Where(r => r.IsActive).Select(r => r.Percent));

        public static int SumSocialInfluencePercent(int baronyId, IEnumerable<SocialGroupRelationDTO> saved)
            => SocialGroupSectionRows(baronyId, saved).Where(r => r.IsActive).Sum(r => r.InfluencePercent);

        public static List<PpbModifierRow> ImprovementRows(IEnumerable<TerrainImprovementDTO> improvements)
            => improvements
                .Where(ShowsOnDomainPanel)
                .Where(i => i.IsActive)
                .OrderByDescending(IsVillage)
                .ThenBy(ImprovementDisplayLabel, StringComparer.OrdinalIgnoreCase)
                .Select(i => Row(ImprovementDisplayLabel(i), i.Additive, i.Percent, i.FormulaText, i.Description))
                .ToList();

        /// <summary>
        /// Towns are map markers only for now.
        /// Overview already limits improvements to the player's primary domain.
        /// </summary>
        public static bool ShowsOnDomainPanel(TerrainImprovementDTO improvement) =>
            !string.Equals(improvement.Name, MapImprovement.Town, StringComparison.OrdinalIgnoreCase);

        public static string ImprovementDisplayLabel(TerrainImprovementDTO improvement)
        {
            // Village/Town: place name. Farm/Mine/Sawmill/…: catalog template name stored in Description.
            if (!string.IsNullOrWhiteSpace(improvement.Description)
                && MapImprovement.IsKnown(improvement.Name))
                return improvement.Description.Trim();

            return improvement.Name;
        }

        /// <summary>
        /// True when the tile belongs to a vassal fief (not baron demesne / domain default).
        /// Tiles without a fief count as direct baronial land.
        /// </summary>
        public static bool IsVassalFiefTile(TerrainTileDTO? tile, IReadOnlyDictionary<int, FiefDTO> fiefsById)
        {
            if (tile?.FiefId is not int fiefId || !fiefsById.TryGetValue(fiefId, out var fief))
                return false;
            return !TerrainMapVisuals.UsesLordTitle(fief);
        }

        public static string OwnershipLabel(bool isVassalFief) =>
            isVassalFief ? "Vassal" : "Demesne";

        public static string OwnershipTooltip(bool isVassalFief) =>
            isVassalFief
                ? "On a vassal’s fief — only half of treasury income goes to the baron."
                : "On the baron’s demesne — full treasury income.";


        public static List<PpbModifierRow> DecreeRows(IEnumerable<DecreeDTO> decrees)
            => decrees
                .Where(d => d.IsActive)
                .Select(d => Row(d.Name, d.Additive, d.Percent, d.FormulaText, d.Description))
                .ToList();

        public static List<PpbModifierRow> EventRows(IEnumerable<BaronyEventDTO> events, int currentTurn)
            => events
                .Where(e => e.IsActiveOnTurn(currentTurn))
                .Select(e => Row(e.Name, e.Additive, e.Percent, null, e.Description))
                .ToList();

        public static List<PpbModifierRow> CommunityRows(IEnumerable<CommunityModifierDTO> mods)
            => mods.Select(m => Row(m.Source, m.Additive, m.Percent, m.FormulaText)).ToList();

        public static List<PpbModifierRow> CommunityRows(
            IEnumerable<PpbModifierRow> advisorRows,
            IEnumerable<PpbModifierRow> buildingRows,
            IEnumerable<PpbModifierRow> socialRows,
            IEnumerable<PpbModifierRow> improvementRows,
            IEnumerable<PpbModifierRow> decreeRows,
            IEnumerable<PpbModifierRow> eventRows,
            int unrest,
            int settlementPopulation,
            int conjunctureDice,
            int conjunctureModifier)
        {
            var preCommunity = new List<PpbModifierRow>();
            preCommunity.AddRange(advisorRows);
            preCommunity.AddRange(buildingRows);
            preCommunity.AddRange(socialRows);
            preCommunity.AddRange(improvementRows);
            preCommunity.AddRange(decreeRows);
            preCommunity.AddRange(eventRows);

            var preAdd = SumAdditive(preCommunity);
            var hunger = HungerPpbFormulas.FromFoodBalance(preAdd[Ppb.Food]);
            var crime = CrimePpbFormulas.FromLawBalance(preAdd[Ppb.Law]);
            var corruption = CorruptionPpbFormulas.FromCorruptionBalance(preAdd[Ppb.Corruption]);
            var unrestValue = Math.Max(0m, unrest);
            var economyE = preAdd[Ppb.Economy];

            return new List<PpbModifierRow>
            {
                Row(CommunitySource.Hunger, HungerPpbFormulas.ComputeAdditive(hunger), HungerPpbFormulas.ComputePercent(hunger)),
                Row(CommunitySource.Crime, CrimePpbFormulas.ComputeAdditive(crime), CrimePpbFormulas.ComputePercent(crime)),
                Row(CommunitySource.Corruption, CorruptionPpbFormulas.ComputeAdditive(corruption), CorruptionPpbFormulas.ComputePercent(corruption)),
                Row(CommunitySource.Unrest, UnrestPpbFormulas.ComputeAdditive(unrestValue), UnrestPpbFormulas.ComputePercent(unrestValue)),
                Row(
                    CommunitySource.Economy,
                    EconomyConjunctureFormulas.ComputeAdditive(
                        economyE, conjunctureDice, conjunctureModifier),
                    EconomyConjunctureFormulas.ComputePercentVector(
                        economyE, settlementPopulation, conjunctureDice, conjunctureModifier),
                    EconomyConjunctureFormulas.FormulaSummary(
                        economyE, settlementPopulation, conjunctureDice, conjunctureModifier),
                    EconomyConjunctureFormulas.CatalogDescription),
            };
        }

        /// <summary>Additive sum of rows (for section totals).</summary>
        public static PpbVector SumAdditive(IEnumerable<PpbModifierRow> rows)
            => PpbVector.Sum(rows.Select(r => r.Additive));

        /// <summary>Percent sum of rows.</summary>
        public static PpbVector SumPercent(IEnumerable<PpbModifierRow> rows)
            => PpbVector.Sum(rows.Select(r => r.Percent));

        /// <summary>
        /// Additive values that percent modifiers scale: positive per row (negative for Corruption).
        /// </summary>
        public static PpbVector SumScalableAdditive(IEnumerable<PpbModifierRow> rows)
        {
            var sum = new PpbVector();
            foreach (var row in rows)
            {
                if (row.Additive is null)
                    continue;
                foreach (var info in PpbCatalog.All)
                {
                    var key = info.Key;
                    var v = row.Additive[key];
                    if (key == Ppb.Corruption)
                    {
                        if (v < 0m)
                            sum[key] += v;
                    }
                    else if (v > 0m)
                    {
                        sum[key] += v;
                    }
                }
            }
            return sum;
        }

        /// <summary>Grand total from all Domain Panel section rows.</summary>
        public static PpbVector SummarizeSections(IEnumerable<PpbModifierRow> rows)
        {
            var list = rows.ToList();
            return PpbMath.Summarize(
                SumAdditive(list),
                SumScalableAdditive(list),
                SumPercent(list));
        }

        /// <summary>
        /// Barony Summary table rows: global Σ additive / Σ percent, scalable, percent effect, Final.
        /// </summary>
        public static List<PpbModifierRow> BuildBaronySummaryRows(DomainPanelRowSet panel)
        {
            var all = panel.AllRows;
            var additive = SumAdditive(all);
            var percent = SumPercent(all);
            var scalable = SumScalableAdditive(all);
            var percentEffect = new PpbVector();
            scalable.EnsureSize();
            percent.EnsureSize();
            percentEffect.EnsureSize();
            for (int i = 0; i < PpbCatalog.Count; i++)
                percentEffect.Values[i] = PpbFormat.Round(scalable.Values[i] * (percent.Values[i] / 100m));

            return new List<PpbModifierRow>
            {
                new() { Label = "Σ additive (all sections)", Additive = additive },
                new() { Label = "Σ percent (all sections)", Percent = percent },
                new() { Label = "Scalable additive", Additive = scalable },
                new() { Label = "Percent effect", Additive = percentEffect },
                new() { Label = "Final Value", Additive = panel.GrandTotal },
            };
        }

        /// <summary>
        /// Collapsed-header chips: additive section sum only (do not mix % points into PPB units).
        /// </summary>
        public static PpbVector SectionGlance(IEnumerable<PpbModifierRow> rows)
            => SumAdditive(rows);

        /// <summary>
        /// Same section rows as Domain Panel (including core buildings + towns under City and Buildings).
        /// Used by Domain Panel summary, Resources expected income, and Budget turn gold.
        /// </summary>
        public static DomainPanelRowSet BuildDomainPanelRows(
            BaronyOverviewDTO ov,
            CharacterDTO? character = null,
            IEnumerable<BaronInfluenceModifierDTO>? baronModifiers = null,
            IEnumerable<AdvisorInfluenceModifierDTO>? advisorModifiers = null,
            int managementJc = BaronTimeRules.RequiredManagementJc)
        {
            var advisors = AdvisorsForDomainPanel(
                ov.Advisors, ov.Barony, character, baronModifiers, advisorModifiers, managementJc);
            var advisorRows = AdvisorRows(advisors);
            var buildingRows = CityBuildingSectionRows(
                ov.Barony.Id, ov.Buildings, ov.Improvements, ov.Seat, ov.SeatPurposeTemplates);
            var socialRows = SocialRows(ov.Barony.Id, ov.SocialRelations);
            var improvementRows = ImprovementRows(ov.Improvements);
            var decreeRows = DecreeRows(ov.Decrees);
            var eventRows = EventRows(ov.Events, ov.Barony.TurnNumber);
            var settlementPop = SumSettlementPopulation(ov.Barony.Id, ov.Buildings, ov.Improvements);
            var communityRows = CommunityRows(
                advisorRows, buildingRows, socialRows, improvementRows, decreeRows, eventRows,
                ov.Barony.Unrest,
                settlementPop,
                ov.Barony.ConjunctureDice,
                ov.Barony.ConjunctureModifier);

            var allRows = new List<PpbModifierRow>();
            allRows.AddRange(advisorRows);
            allRows.AddRange(buildingRows);
            allRows.AddRange(socialRows);
            allRows.AddRange(improvementRows);
            allRows.AddRange(decreeRows);
            allRows.AddRange(eventRows);
            allRows.AddRange(communityRows);

            return new DomainPanelRowSet
            {
                Advisors = advisors,
                AdvisorRows = advisorRows,
                BuildingRows = buildingRows,
                SocialRows = socialRows,
                ImprovementRows = improvementRows,
                DecreeRows = decreeRows,
                EventRows = eventRows,
                CommunityRows = communityRows,
                AllRows = allRows,
                GrandTotal = SummarizeSections(allRows),
            };
        }

        /// <summary>
        /// Domain Panel only: fold office upkeep (+ bonus source gold costs) into the office row's Gold.
        /// Not used by Offices sync — upkeep stays a separate field there.
        /// </summary>
        private static void ApplyOfficeGoldCostToPanelRow(
            AdvisorDTO advisor,
            IEnumerable<AdvisorInfluenceModifierDTO>? customModifiers)
        {
            if (advisor.IsBaron)
                return;

            var cost = PpbFormat.Round(TotalOfficeCost(advisor, customModifiers));
            advisor.UpkeepGold = cost;
            if (cost != 0m)
                advisor.Additive[Ppb.Treasury] -= cost;
        }

        /// <summary>Grand total of all Domain Panel sections (same Gold as Resources / Budget turn balance).</summary>
        public static PpbVector GrandTotal(
            BaronyOverviewDTO ov,
            CharacterDTO? character = null,
            IEnumerable<BaronInfluenceModifierDTO>? baronModifiers = null,
            IEnumerable<AdvisorInfluenceModifierDTO>? advisorModifiers = null,
            int managementJc = BaronTimeRules.RequiredManagementJc)
            => BuildDomainPanelRows(ov, character, baronModifiers, advisorModifiers, managementJc).GrandTotal;

        public sealed class DomainPanelRowSet
        {
            public List<AdvisorDTO> Advisors { get; init; } = new();
            public List<PpbModifierRow> AdvisorRows { get; init; } = new();
            public List<PpbModifierRow> BuildingRows { get; init; } = new();
            public List<PpbModifierRow> SocialRows { get; init; } = new();
            public List<PpbModifierRow> ImprovementRows { get; init; } = new();
            public List<PpbModifierRow> DecreeRows { get; init; } = new();
            public List<PpbModifierRow> EventRows { get; init; } = new();
            public List<PpbModifierRow> CommunityRows { get; init; } = new();
            public List<PpbModifierRow> AllRows { get; init; } = new();
            public PpbVector GrandTotal { get; init; } = new();
        }

        // --- Baron Card: influence on barony ---

        public static List<BaronInfluenceRow> BuildInfluenceRows(
            CharacterDTO? character,
            int prestige,
            int honor,
            int fear,
            IEnumerable<BaronInfluenceModifierDTO>? customModifiers,
            int managementJc = BaronTimeRules.RequiredManagementJc)
        {
            var skills = InfluenceFromSkills(character);
            var rows = new List<BaronInfluenceRow>
            {
                new()
                {
                    Source = BaronInfluenceSource.FromSkills,
                    Values = skills,
                    IsSystem = true,
                    Description = BaronSkillPpbFormulas.CatalogDescription,
                    ValueTooltip = BaronSkillPpbFormulas.ExplainAdditive,
                },
            };

            var penalty = ManagementSkillPenalty(skills, managementJc);
            if (!penalty.IsEmpty)
            {
                var factor = BaronTimeRules.ManagementSkillFactor(managementJc);
                var factorPct = decimal.Round(factor * 100m, 0, MidpointRounding.AwayFromZero);
                rows.Add(new BaronInfluenceRow
                {
                    Source = BaronInfluenceSource.FromManagementTime,
                    Values = penalty,
                    IsSystem = true,
                    Description =
                        $"Skill PPB scaled by management JC. "
                        + $"{managementJc}/{BaronTimeRules.RequiredManagementJc} JC = {factorPct}% of From Skills. "
                        + "Penalty values are rounded to whole numbers.",
                    Formula =
                        $"management JC {managementJc}/{BaronTimeRules.RequiredManagementJc} → {factorPct}% skills",
                });
            }

            rows.Add(new BaronInfluenceRow
            {
                Source = BaronInfluenceSource.FromPrestigeHonor,
                Values = InfluenceFromPrestigeHonorFear(prestige, honor, fear),
                IsSystem = true,
                Description = BaronReputationTiers.DescribeActiveTiers(prestige, honor, fear),
                Formula = "Reputation tier bonuses from Prestige, Honor, and Fear",
            });

            foreach (var modifier in customModifiers ?? Enumerable.Empty<BaronInfluenceModifierDTO>())
            {
                rows.Add(new BaronInfluenceRow
                {
                    Source = modifier.Source,
                    Values = modifier.Additive,
                    IsSystem = false,
                    ModifierId = modifier.Id,
                    Formula = modifier.FormulaText,
                    Description = modifier.Description,
                });
            }

            return rows;
        }

        public static PpbVector SumInfluenceRows(IEnumerable<BaronInfluenceRow> rows)
        {
            var sum = new PpbVector();
            foreach (var row in rows)
                sum.AddInPlace(row.Values);
            return sum;
        }

        public static int ManagementJcSpent(IEnumerable<BaronTimeActionDTO>? actions) =>
            (actions ?? Enumerable.Empty<BaronTimeActionDTO>())
                .Where(a => string.Equals(a.Kind, BaronTimeActionKind.Management, StringComparison.OrdinalIgnoreCase))
                .Sum(a => a.CostJc);

        /// <summary>Scale each PPB component by factor and round to whole numbers.</summary>
        public static PpbVector ScalePpbToIntegers(PpbVector source, decimal factor)
        {
            var result = new PpbVector();
            if (source is null)
                return result;

            source.EnsureSize();
            for (int i = 0; i < PpbCatalog.Count; i++)
            {
                result.Values[i] = decimal.Round(
                    source.Values[i] * factor,
                    0,
                    MidpointRounding.AwayFromZero);
            }

            return result;
        }

        /// <summary>Effective skill PPB after management JC factor (0–100%).</summary>
        public static PpbVector ApplyManagementSkillFactor(PpbVector skills, int managementJc) =>
            ScalePpbToIntegers(skills ?? new PpbVector(), BaronTimeRules.ManagementSkillFactor(managementJc));

        /// <summary>
        /// Integer penalty so that From Skills + penalty ≈ skills × (managementJc/100).
        /// Each component: Round(full × (factor − 1)).
        /// </summary>
        public static PpbVector ManagementSkillPenalty(PpbVector fullSkills, int managementJc)
        {
            var factor = BaronTimeRules.ManagementSkillFactor(managementJc);
            if (factor >= 1m || fullSkills is null)
                return new PpbVector();

            return ScalePpbToIntegers(fullSkills, factor - 1m);
        }

        public static PpbVector InfluenceFromSkills(CharacterDTO? character)
        {
            if (character is null)
                return new PpbVector();

            CharacterSkillRelations.Wire(character);

            decimal Special(string name)
            {
                if (character.SpecialSkills is null)
                    return 0m;
                foreach (var s in character.SpecialSkills)
                {
                    if (string.Equals(s.Name, name, StringComparison.OrdinalIgnoreCase))
                        return s.SumBonus;
                }
                return 0m;
            }

            decimal Base(string name)
            {
                if (character.BaseSkills is null)
                    return 0m;
                foreach (var s in character.BaseSkills)
                {
                    if (string.Equals(s.Name, name, StringComparison.OrdinalIgnoreCase))
                        return s.SumBonus;
                }
                return 0m;
            }

            decimal Attr(string name)
            {
                if (character.Attributes is null)
                    return 0m;
                foreach (var a in character.Attributes)
                {
                    if (string.Equals(a.Name, name, StringComparison.OrdinalIgnoreCase))
                        return a.ModifierAbsolute;
                }
                return 0m;
            }

            return BaronSkillPpbFormulas.Compute(Special, Base, Attr);
        }

        public static PpbVector InfluenceFromPrestigeHonorFear(int prestige, int honor, int fear) =>
            BaronReputationTiers.InfluenceFromScores(prestige, honor, fear);

        // --- Baron Card: Prestige / Honor / Fear ---

        /// <summary>
        /// Lord's Seat contribution to PHP. Prestige = Σ (multiplier × purpose AdditivePrestige)
        /// for active rooms with a purpose. Honor/Fear reserved for future purpose fields.
        /// </summary>
        public static PhpTotals SeatPhpContribution(
            BaronySeatDTO? seat,
            IEnumerable<SeatPurposeTemplateDTO>? purposeTemplates)
        {
            if (seat?.Rooms is null || seat.Rooms.Count == 0)
                return PhpTotals.Zero;

            var purposes = (purposeTemplates ?? Enumerable.Empty<SeatPurposeTemplateDTO>())
                .ToDictionary(p => p.Id);

            decimal prestige = 0m;
            foreach (var room in seat.Rooms)
            {
                if (room.IsRuin)
                    continue;
                if (room.PurposeTemplateId is not int pid || !purposes.TryGetValue(pid, out var purpose))
                    continue;

                prestige += room.PrestigeMultiplier * purpose.AdditivePrestige;
            }

            return new PhpTotals(
                Prestige: (int)Math.Round(prestige, MidpointRounding.AwayFromZero),
                Honor: 0,
                Fear: 0);
        }

        public static List<BaronPhpRow> BuildPhpRows(
            PhpTotals seatContribution,
            PhpTotals itemsContribution,
            IEnumerable<BaronPhpSourceDTO>? customSources)
        {
            var rows = new List<BaronPhpRow>
            {
                new()
                {
                    Source = BaronPhpSourceLabel.FromSeat,
                    Prestige = seatContribution.Prestige,
                    Honor = seatContribution.Honor,
                    Fear = seatContribution.Fear,
                    IsSystem = true,
                    Description =
                        "Sum of chamber prestige from Lord's Seat (purpose prestige × chamber multiplier). "
                        + "Honor and Fear from the seat will appear when purpose templates define them.",
                },
                new()
                {
                    Source = BaronPhpSourceLabel.FromItems,
                    Prestige = itemsContribution.Prestige,
                    Honor = itemsContribution.Honor,
                    Fear = itemsContribution.Fear,
                    IsSystem = true,
                    Description =
                        "Trophies, treasures and artifacts: each item's Prestige / Honor / Fear "
                        + "× the chamber prestige multiplier of the room where it is displayed "
                        + "(×1 if not placed).",
                },
            };

            foreach (var source in customSources ?? Enumerable.Empty<BaronPhpSourceDTO>())
            {
                rows.Add(new BaronPhpRow
                {
                    Source = source.Source,
                    Prestige = source.Prestige,
                    Honor = source.Honor,
                    Fear = source.Fear,
                    IsSystem = false,
                    SourceId = source.Id,
                    Description = source.Description,
                });
            }

            return rows;
        }

        /// <summary>
        /// Artifacts contribution: Σ (base PHP × chamber PrestigeMultiplier).
        /// Unplaced items use multiplier 1.
        /// </summary>
        public static PhpTotals ArtifactsPhpContribution(
            IEnumerable<BaronArtifactDTO>? artifacts,
            BaronySeatDTO? seat)
        {
            var rooms = seat?.Rooms?
                .Where(r => !r.IsRuin)
                .ToDictionary(r => r.Id)
                ?? new Dictionary<int, SeatRoomDTO>();

            decimal prestige = 0m, honor = 0m, fear = 0m;
            foreach (var item in artifacts ?? Enumerable.Empty<BaronArtifactDTO>())
            {
                var mult = 1m;
                if (item.SeatRoomId is int roomId && rooms.TryGetValue(roomId, out var room))
                    mult = room.PrestigeMultiplier <= 0 ? 1m : room.PrestigeMultiplier;

                prestige += item.Prestige * mult;
                honor += item.Honor * mult;
                fear += item.Fear * mult;
            }

            return new PhpTotals(
                Prestige: (int)Math.Round(prestige, MidpointRounding.AwayFromZero),
                Honor: (int)Math.Round(honor, MidpointRounding.AwayFromZero),
                Fear: (int)Math.Round(fear, MidpointRounding.AwayFromZero));
        }

        /// <summary>Effective chamber multiplier for an artifact (1 if unplaced / ruin / missing).</summary>
        public static decimal ArtifactChamberBonus(BaronArtifactDTO item, BaronySeatDTO? seat)
        {
            if (item.SeatRoomId is not int roomId || seat?.Rooms is null)
                return 1m;

            var room = seat.Rooms.FirstOrDefault(r => r.Id == roomId && !r.IsRuin);
            if (room is null)
                return 1m;

            return room.PrestigeMultiplier <= 0 ? 1m : room.PrestigeMultiplier;
        }

        public static string ArtifactRoomLabel(BaronArtifactDTO item, BaronySeatDTO? seat)
        {
            if (item.SeatRoomId is not int roomId || seat?.Rooms is null)
                return "—";

            var room = seat.Rooms.FirstOrDefault(r => r.Id == roomId);
            return room is null ? "—" : (string.IsNullOrWhiteSpace(room.Name) ? $"Room #{room.Id}" : room.Name);
        }

        /// <summary>Chamber name with prestige multiplier, e.g. <c>Great Hall ×1.5</c>.</summary>
        public static string ArtifactLocationLabel(BaronArtifactDTO item, BaronySeatDTO? seat)
        {
            var roomName = ArtifactRoomLabel(item, seat);
            if (roomName == "—")
                return "—";

            var mult = ArtifactChamberBonus(item, seat);
            return $"{roomName} ×{mult:0.##}";
        }

        public static string ArtifactLocationTooltip(BaronArtifactDTO item, BaronySeatDTO? seat)
        {
            if (item.SeatRoomId is not int roomId || seat?.Rooms is null)
            {
                return "Not placed in a Lord's Seat chamber. Prestige, Honor and Fear use ×1 "
                    + "(no chamber multiplier).";
            }

            var room = seat.Rooms.FirstOrDefault(r => r.Id == roomId);
            if (room is null)
            {
                return "Chamber not found. Prestige, Honor and Fear use ×1.";
            }

            var name = string.IsNullOrWhiteSpace(room.Name) ? $"Room #{room.Id}" : room.Name;
            var mult = room.IsRuin
                ? 1m
                : (room.PrestigeMultiplier <= 0 ? 1m : room.PrestigeMultiplier);
            var size = room.SizeCategory;
            var capacity = BaronArtifactCapacity.LimitLabel(size);
            var lines = new List<string>
            {
                $"Location: {name}",
                $"Size: {size} (artifact capacity {capacity})",
                $"Chamber prestige multiplier: ×{mult:0.##}",
                "This multiplier is applied to the item's Prestige, Honor and Fear.",
            };
            if (room.IsRuin)
                lines.Add("This chamber is a ruin — multiplier treated as ×1.");
            return string.Join("\n", lines);
        }

        /// <summary>
        /// How many artifacts already occupy a chamber (optionally excluding one being edited).
        /// </summary>
        public static int ArtifactCountInRoom(
            IEnumerable<BaronArtifactDTO>? artifacts,
            int roomId,
            int? excludeArtifactId = null)
        {
            return (artifacts ?? Enumerable.Empty<BaronArtifactDTO>())
                .Count(a => a.SeatRoomId == roomId
                    && (excludeArtifactId is null || a.Id != excludeArtifactId.Value));
        }

        /// <summary>
        /// Returns an error message if placing into the room would exceed capacity; otherwise null.
        /// </summary>
        public static string? ArtifactCapacityError(
            SeatRoomDTO? room,
            IEnumerable<BaronArtifactDTO>? artifacts,
            int? excludeArtifactId = null)
        {
            if (room is null || room.IsRuin)
                return "Choose an active chamber, or leave location empty.";

            var max = BaronArtifactCapacity.MaxForSize(room.SizeCategory);
            if (max is null)
                return null;

            var used = ArtifactCountInRoom(artifacts, room.Id, excludeArtifactId);
            if (used >= max.Value)
            {
                return $"{room.Name} is full ({used}/{max} artifacts for {room.SizeCategory} chambers). "
                    + "Remove an item or choose another room.";
            }

            return null;
        }

        public static PhpTotals SumPhpRows(IEnumerable<BaronPhpRow>? rows)
        {
            var total = PhpTotals.Zero;
            if (rows is null)
                return total;

            foreach (var row in rows)
                total = total.Add(row.Prestige, row.Honor, row.Fear);

            return total;
        }

        // --- Baron Card: Time (JC) ---

        /// <summary>
        /// Attribute score used for JC pool (same base as health: SumAbsolute).
        /// </summary>
        public static int AttributeSumAbsolute(CharacterDTO? character, string attributeName)
        {
            if (character?.Attributes is null)
                return 0;

            foreach (var a in character.Attributes)
            {
                if (string.Equals(a.Name, attributeName, StringComparison.OrdinalIgnoreCase))
                    return a.SumAbsolute;
            }

            return 0;
        }

        /// <summary>
        /// JC pool: (Endurance + Willpower) × 10, then ± percent modifiers.
        /// Management = sum of Management actions; Adventure weeks = Adventure JC / 25.
        /// </summary>
        public static BaronTimeBudget BuildTimeBudget(
            CharacterDTO? character,
            IEnumerable<BaronTimeModifierDTO>? modifiers,
            IEnumerable<BaronTimeActionDTO>? actions)
        {
            var endurance = AttributeSumAbsolute(character, SD.Attributes.Endurance);
            var willpower = AttributeSumAbsolute(character, SD.Attributes.Willpower);
            var baseJc = (endurance + willpower) * BaronTimeRules.AttributeFactor;

            var modList = modifiers?.ToList() ?? new List<BaronTimeModifierDTO>();
            var percent = modList.Sum(m => m.Percent);
            var totalJc = (int)Math.Round(
                baseJc * (1m + percent / 100m),
                MidpointRounding.AwayFromZero);

            var actionList = actions?.ToList() ?? new List<BaronTimeActionDTO>();
            var spent = actionList.Sum(a => a.CostJc);
            var management = actionList
                .Where(a => string.Equals(a.Kind, BaronTimeActionKind.Management, StringComparison.OrdinalIgnoreCase))
                .Sum(a => a.CostJc);
            var adventure = actionList
                .Where(a => string.Equals(a.Kind, BaronTimeActionKind.Adventure, StringComparison.OrdinalIgnoreCase))
                .Sum(a => a.CostJc);
            var weeks = adventure / (decimal)BaronTimeRules.WeeklyExpeditionJc;

            return new BaronTimeBudget(
                Endurance: endurance,
                Willpower: willpower,
                BaseJc: baseJc,
                ModifierPercent: percent,
                TotalJc: totalJc,
                SpentJc: spent,
                RemainingJc: totalJc - spent,
                ManagementJc: management,
                AdventureJc: adventure,
                ExpeditionWeeks: weeks);
        }

        public static string FormatJc(int value) => $"{value} JC";

        public static string FormatPercent(decimal percent) =>
            percent > 0 ? $"+{percent:0.##}%"
            : percent < 0 ? $"{percent:0.##}%"
            : "±0%";

        // --- Offices: advisor influence on barony ---

        public static List<AdvisorDTO> OrderOffices(IEnumerable<AdvisorDTO>? advisors)
            => OrderAdvisors(advisors?.Where(a => !a.IsBaron));

        public static decimal SumOfficeUpkeep(
            IEnumerable<AdvisorDTO>? advisors,
            IEnumerable<AdvisorInfluenceModifierDTO>? modifiers = null)
        {
            var modList = modifiers?.ToList() ?? new List<AdvisorInfluenceModifierDTO>();
            return OrderOffices(advisors).Sum(a =>
                TotalOfficeCost(a, modList.Where(m => m.AdvisorId == a.Id)));
        }

        public static decimal TotalOfficeCost(
            AdvisorDTO advisor,
            IEnumerable<AdvisorInfluenceModifierDTO>? customModifiers)
            => advisor.UpkeepGold + (customModifiers?.Sum(m => m.CostGold) ?? 0m);

        public static string OfficeSectionTitle(AdvisorDTO advisor)
            => $"{AdvisorRoleLabel(advisor)} - Skills";

        public static bool IsCoreOffice(AdvisorDTO advisor)
            => !advisor.IsBaron && OfficeType.Core.Contains(advisor.OfficeType);

        public static bool IsOfficeAssigned(AdvisorDTO advisor)
            => advisor.AvailableAdvisorId is > 0;

        public static IReadOnlyList<Ppb> EffectiveSignificantSkills(AdvisorDTO advisor)
        {
            if (advisor.SignificantSkills.Count > 0)
                return advisor.SignificantSkills;
            return AdvisorSignificantSkills.DefaultForOffice(advisor.OfficeType);
        }

        public static List<AdvisorInfluenceRow> BuildAdvisorInfluenceRows(
            AdvisorDTO advisor,
            IEnumerable<AdvisorInfluenceModifierDTO>? customModifiers)
        {
            var rows = new List<AdvisorInfluenceRow>();

            if (IsOfficeAssigned(advisor))
            {
                rows.Add(new AdvisorInfluenceRow
                {
                    Source = AdvisorInfluenceSource.FromSkills,
                    Values = advisor.Skills.Clone(),
                    IsSystem = true,
                    SystemKind = AdvisorInfluenceSystemKind.Skills,
                    Description = "Administrative skills of the office holder. "
                        + "Only significant (active) skills affect barony PPB in the Domain Panel.",
                });
            }

            foreach (var modifier in customModifiers ?? Enumerable.Empty<AdvisorInfluenceModifierDTO>())
            {
                rows.Add(new AdvisorInfluenceRow
                {
                    Source = modifier.Source,
                    Values = modifier.Additive,
                    IsSystem = false,
                    ModifierId = modifier.Id,
                    Formula = modifier.FormulaText,
                    Description = string.IsNullOrWhiteSpace(modifier.Description)
                        ? "Skill-unit bonus. Counts toward the office skill total (active skills only), "
                          + "then Domain Panel Additive/Percent from the skill→PPB formulas."
                        : modifier.Description,
                    Cost = modifier.CostGold,
                });
            }

            return rows;
        }

        public static PpbVector SumAdvisorInfluenceRows(
            IEnumerable<AdvisorInfluenceRow> rows,
            IEnumerable<Ppb>? significantSkills = null)
        {
            var sum = new PpbVector();
            foreach (var row in rows)
            {
                var values = significantSkills is not null
                    ? AdvisorSignificantSkills.MaskToSignificant(row.Values, significantSkills)
                    : row.Values;
                sum.AddInPlace(values);
            }
            return sum;
        }

        /// <summary>
        /// Domain Panel office row: sum skills + bonus sources (skill units), mask to active skills,
        /// then map that total through the skill→PPB Additive/Percent formulas.
        /// </summary>
        public static void ApplyAdvisorSkillInfluence(
            AdvisorDTO advisor,
            IEnumerable<AdvisorInfluenceModifierDTO>? customModifiers)
        {
            if (advisor.IsBaron)
                return;

            var active = EffectiveSignificantSkills(advisor);
            var totalSkills = SumAdvisorInfluenceRows(
                BuildAdvisorInfluenceRows(advisor, customModifiers),
                active);

            advisor.Additive = BaronSkillPpbFormulas.MapToAdvisorAdditive(totalSkills);
            advisor.Percent = BaronSkillPpbFormulas.MapToAdvisorPercent(totalSkills);
        }

        public static void SyncAdvisorAdditive(
            AdvisorDTO advisor,
            IEnumerable<AdvisorInfluenceModifierDTO>? customModifiers)
            => ApplyAdvisorSkillInfluence(advisor, customModifiers);

        public static string? ExplainOfficeAdvisorAdditive(AdvisorDTO advisor, Ppb key)
        {
            if (advisor.IsBaron)
                return null;

            string? skillTip = null;
            if (IsOfficeAssigned(advisor) && IsActiveSkill(advisor, key))
                skillTip = BaronSkillPpbFormulas.ExplainAdvisorAdditive(key);

            if (key == Ppb.Treasury && advisor.UpkeepGold != 0m)
            {
                var upkeep = $"Office upkeep: −{PpbFormat.Number(advisor.UpkeepGold)} gold.";
                return skillTip is null ? upkeep : $"{skillTip}\n{upkeep}";
            }

            return skillTip;
        }

        public static string? ExplainOfficeAdvisorPercent(AdvisorDTO advisor, Ppb key)
        {
            if (advisor.IsBaron || !IsOfficeAssigned(advisor) || !IsActiveSkill(advisor, key))
                return null;
            return BaronSkillPpbFormulas.ExplainAdvisorPercent(key);
        }

        private static bool IsActiveSkill(AdvisorDTO advisor, Ppb key)
            => EffectiveSignificantSkills(advisor).Contains(key);
    }
}
