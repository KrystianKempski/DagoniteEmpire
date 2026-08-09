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
            PersonDescription = source.PersonDescription,
            UpkeepGold = source.UpkeepGold,
        };

        /// <summary>Office flavor for tooltips / Offices page (catalog for core, stored text for custom).</summary>
        public static string? ResolveOfficeDescription(AdvisorDTO advisor)
            => OfficeDescriptions.For(advisor.OfficeType)
               ?? (string.IsNullOrWhiteSpace(advisor.Description) ? null : advisor.Description.Trim());

        /// <summary>Assigned person's bio (Available Advisors pool).</summary>
        public static string? ResolvePersonDescription(AdvisorDTO advisor)
            => string.IsNullOrWhiteSpace(advisor.PersonDescription) ? null : advisor.PersonDescription.Trim();

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
                        ? $" Skill PPB applied at {factorPct}% ({managementJc}/{BaronTimeRules.RequiredManagementJc} management BT)."
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

        /// <summary>
        /// Starter city buildings in display order.
        /// Prefer saved rows with <see cref="CoreCityBuildingKey"/>; fall back to Buildings catalog templates.
        /// </summary>
        public static IReadOnlyList<BaronyBuildingDTO> EffectiveCoreCityBuildings(
            int baronyId,
            IEnumerable<BaronyBuildingDTO>? saved,
            IEnumerable<BuildingTemplateDTO>? catalog = null)
        {
            var byKey = (saved ?? Enumerable.Empty<BaronyBuildingDTO>())
                .Where(b => !string.IsNullOrWhiteSpace(b.CoreKey))
                .GroupBy(b => b.CoreKey!, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

            var catalogByName = (catalog ?? Enumerable.Empty<BuildingTemplateDTO>())
                .Where(tpl => !string.IsNullOrWhiteSpace(tpl.Name))
                .GroupBy(tpl => tpl.Name, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

            var list = new List<BaronyBuildingDTO>(CoreCityBuildingKey.All.Length);
            foreach (var key in CoreCityBuildingKey.All)
            {
                if (byKey.TryGetValue(key, out var row))
                {
                    list.Add(row);
                    continue;
                }

                var catalogName = CoreCityBuildingKey.CatalogName(key);
                if (!catalogByName.TryGetValue(catalogName, out var template))
                    continue;

                list.Add(FromStarterCatalogTemplate(baronyId, key, template));
            }

            return list;
        }

        public static BaronyBuildingDTO FromStarterCatalogTemplate(
            int baronyId,
            string coreKey,
            BuildingTemplateDTO template) => new()
        {
            BaronyId = baronyId,
            TemplateId = template.Id > 0 ? template.Id : null,
            CoreKey = coreKey,
            Name = template.Name,
            Kind = BuildingKind.Building,
            Description = template.Description,
            Additive = template.EffectAdditive.Clone(),
            Percent = template.EffectPercent.Clone(),
        };

        /// <summary>Saved buildings that are not starter/core rows (catalog / custom adds).</summary>
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

        public static bool IsFarm(TerrainImprovementDTO improvement) =>
            string.Equals(improvement.Name, MapImprovement.Farm, StringComparison.OrdinalIgnoreCase);

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

        public static List<PpbModifierRow> ImprovementRows(
            IEnumerable<TerrainImprovementDTO> improvements,
            IEnumerable<TerrainTileDTO>? tiles = null,
            IEnumerable<FiefDTO>? fiefs = null,
            decimal vassalTributePercent = FiefTributeFormulas.DefaultPercent,
            string? season = null)
        {
            var tilesById = (tiles ?? Enumerable.Empty<TerrainTileDTO>())
                .ToDictionary(t => t.Id);
            var fiefsById = (fiefs ?? Enumerable.Empty<FiefDTO>())
                .ToDictionary(f => f.Id);
            var rate = FiefTributeFormulas.ClampPercent(vassalTributePercent);

            return improvements
                .Where(ShowsOnDomainPanel)
                .Where(i => i.IsActive)
                .OrderByDescending(IsVillage)
                .ThenBy(ImprovementDisplayLabel, StringComparer.OrdinalIgnoreCase)
                .Select(i =>
                {
                    var additive = ApplyVassalVillageGoldShare(i, tilesById, fiefsById, rate, out var note);
                    additive = ApplyWinterFarmFoodHalt(i, additive, season, out var winterNote);
                    var formula = i.FormulaText;
                    foreach (var extra in new[] { note, winterNote })
                    {
                        if (string.IsNullOrWhiteSpace(extra))
                            continue;
                        formula = string.IsNullOrWhiteSpace(formula) ? extra : $"{formula}\n\n{extra}";
                    }
                    return Row(ImprovementDisplayLabel(i), additive, i.Percent, formula, i.Description);
                })
                .ToList();
        }

        /// <summary>
        /// Map farms contribute no Food in Winter (stored yield is kept; Domain Panel / income zeroes it).
        /// </summary>
        public static PpbVector ApplyWinterFarmFoodHalt(
            TerrainImprovementDTO improvement,
            PpbVector additive,
            string? season,
            out string? note)
        {
            note = null;
            if (!IsFarm(improvement) || BaronyCalendarFormulas.FarmsProduceFood(season))
                return additive;

            var clone = additive.Clone();
            if (clone[Ppb.Food] == 0m)
                return clone;

            clone[Ppb.Food] = 0m;
            note = "Farm food yield is 0 in Winter — survive on granary stocks.";
            return clone;
        }

        /// <summary>
        /// Positive Gold from active villages (after vassal fief share). Used on Budget as Fief income.
        /// </summary>
        public static decimal VillageGoldIncome(
            IEnumerable<TerrainImprovementDTO> improvements,
            IEnumerable<TerrainTileDTO>? tiles = null,
            IEnumerable<FiefDTO>? fiefs = null,
            decimal vassalTributePercent = FiefTributeFormulas.DefaultPercent)
        {
            var tilesById = (tiles ?? Enumerable.Empty<TerrainTileDTO>())
                .ToDictionary(t => t.Id);
            var fiefsById = (fiefs ?? Enumerable.Empty<FiefDTO>())
                .ToDictionary(f => f.Id);
            var rate = FiefTributeFormulas.ClampPercent(vassalTributePercent);

            var sum = improvements
                .Where(ShowsOnDomainPanel)
                .Where(i => i.IsActive && IsVillage(i))
                .Sum(i =>
                {
                    var additive = ApplyVassalVillageGoldShare(i, tilesById, fiefsById, rate, out _);
                    return Math.Max(0m, additive[Ppb.Treasury]);
                });
            return PpbFormat.Round(sum);
        }

        /// <summary>
        /// Villages on vassal fiefs: baron keeps only <paramref name="vassalTributePercent"/>% of Treasury gold.
        /// </summary>
        public static PpbVector ApplyVassalVillageGoldShare(
            TerrainImprovementDTO improvement,
            IReadOnlyDictionary<int, TerrainTileDTO> tilesById,
            IReadOnlyDictionary<int, FiefDTO> fiefsById,
            decimal vassalTributePercent,
            out string? note)
        {
            note = null;
            var additive = improvement.Additive.Clone();
            if (!IsVillage(improvement))
                return additive;

            TerrainTileDTO? tile = null;
            if (improvement.TileId is int tid)
                tilesById.TryGetValue(tid, out tile);
            if (!IsVassalFiefTile(tile, fiefsById))
                return additive;

            var full = additive[Ppb.Treasury];
            var kept = FiefTributeFormulas.ApplyVassalShare(full, vassalTributePercent);
            additive[Ppb.Treasury] = kept;
            note = FiefTributeFormulas.ExplainVassalShare(full, vassalTributePercent, kept);
            return additive;
        }

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

        public static string OwnershipTooltip(bool isVassalFief, decimal vassalTributePercent = FiefTributeFormulas.DefaultPercent) =>
            isVassalFief
                ? $"On a vassal’s fief — baron keeps {FiefTributeFormulas.ClampPercent(vassalTributePercent):0.#}% of village gold."
                : "On the baron’s demesne — full village gold.";


        public static List<PpbModifierRow> DecreeRows(IEnumerable<DecreeDTO> decrees)
            => decrees
                .Where(d => d.IsActive)
                .Select(d => Row(d.Name, d.Additive, d.Percent, d.FormulaText, d.Description))
                .ToList();

        /// <summary>
        /// Always-active combined trade / luxury / treaty row under Decrees and Technologies.
        /// </summary>
        public static List<PpbModifierRow> TradeGoodDomainRows(
            IEnumerable<BaronyBuildingDTO>? buildings,
            IEnumerable<TerrainImprovementDTO>? improvements,
            IEnumerable<BaronyTradeTreaty>? treaties,
            IEnumerable<string>? mgOverrideKeys,
            string? luxuryAccessKey)
        {
            var facilityNames = (buildings ?? Enumerable.Empty<BaronyBuildingDTO>())
                .Select(b => b.Name)
                .Concat(
                    (improvements ?? Enumerable.Empty<TerrainImprovementDTO>())
                        .Where(i => i.IsActive)
                        .SelectMany(i => TradeGoodAvailability.FacilityNamesFromMapImprovement(i.Name, i.Description)));
            var treatyList = (treaties ?? Enumerable.Empty<BaronyTradeTreaty>()).ToList();
            var availability = TradeGoodAvailability.Resolve(facilityNames, treatyList, mgOverrideKeys);
            return TradeGoodAvailability.DomainPanelBonusParts(availability, treatyList, luxuryAccessKey)
                .Select(p => Row(p.Label, p.Additive, p.Percent, note: p.Note))
                .ToList();
        }

        /// <summary>Decrees (active) plus derived trade-good bonuses for the Decrees section.</summary>
        public static List<PpbModifierRow> DecreeSectionRows(
            IEnumerable<DecreeDTO> decrees,
            IEnumerable<BaronyBuildingDTO>? buildings,
            IEnumerable<TerrainImprovementDTO>? improvements,
            IEnumerable<BaronyTradeTreaty>? treaties,
            IEnumerable<string>? mgOverrideKeys,
            string? luxuryAccessKey)
        {
            var rows = DecreeRows(decrees);
            rows.AddRange(TradeGoodDomainRows(buildings, improvements, treaties, mgOverrideKeys, luxuryAccessKey));
            return rows;
        }

        public static List<PpbModifierRow> EventRows(IEnumerable<BaronyEventDTO> events, int currentTurn)
            => events
                .Where(e => e.IsActiveOnTurn(currentTurn))
                .Select(e => Row(e.Name, e.Additive, e.Percent, null, e.Description))
                .ToList();

        /// <summary>
        /// Non-cumulative PPB from this turn's audiences (Economy, Loyalty, Stability, Law, Corruption).
        /// Shown as a synthetic Events row named <see cref="BaronAudiencePpb.SummaryRowName"/>.
        /// </summary>
        public static List<PpbModifierRow> AudienceEventRows(IEnumerable<BaronAudienceDTO>? audiences, int currentTurn)
        {
            var add = new PpbVector();
            var pct = new PpbVector();
            foreach (var a in audiences ?? Enumerable.Empty<BaronAudienceDTO>())
            {
                if (!BaronAudiencePpb.ContributesToTurn(a.TurnNumber, a.Status, currentTurn))
                    continue;
                add.AddInPlace(a.Additive);
                pct.AddInPlace(a.Percent);
            }

            add = BaronAudiencePpb.SliceNonCumulative(add);
            pct = BaronAudiencePpb.SliceNonCumulative(pct);
            if (add.IsEmpty && pct.IsEmpty)
                return new List<PpbModifierRow>();

            return new List<PpbModifierRow>
            {
                Row(
                    BaronAudiencePpb.SummaryRowName,
                    add,
                    pct,
                    formula: "Sum of audience grants (non-cumulative PPB)",
                    note: "From Audiences this turn"),
            };
        }

        /// <summary>Cumulative PPB slice from this turn's audiences (for Project Summary).</summary>
        public static void AudienceCumulativeTotals(
            IEnumerable<BaronAudienceDTO>? audiences,
            int currentTurn,
            out PpbVector additive,
            out PpbVector percent)
        {
            additive = new PpbVector();
            percent = new PpbVector();
            foreach (var a in audiences ?? Enumerable.Empty<BaronAudienceDTO>())
            {
                if (!BaronAudiencePpb.ContributesToTurn(a.TurnNumber, a.Status, currentTurn))
                    continue;
                additive.AddInPlace(a.Additive);
                percent.AddInPlace(a.Percent);
            }

            additive = BaronAudiencePpb.SliceCumulative(additive);
            percent = BaronAudiencePpb.SliceCumulative(percent);
        }

        /// <summary>
        /// Active units only: wage (Gold), food upkeep, defense upkeep as negative Additive.
        /// Training units do not count until graduation.
        /// </summary>
        public static List<PpbModifierRow> ArmyRows(IEnumerable<BaronyUnitDTO>? units)
            => (units ?? Enumerable.Empty<BaronyUnitDTO>())
                .Where(u => u.IsActive)
                .OrderBy(u => u.Name, StringComparer.OrdinalIgnoreCase)
                .ThenBy(u => u.Id)
                .Select(u =>
                {
                    var upkeep = UnitUpkeepFormulas.Compute(
                        u.Wage, u.UpkeepFood, u.UpkeepDefense,
                        u.Weapon1Key, u.Weapon2Key, u.ArmorKey, u.ShieldKey, u.MountKey);
                    var additive = new PpbVector();
                    additive[Ppb.Treasury] = -upkeep.Gold;
                    additive[Ppb.Food] = -upkeep.Food;
                    additive[Ppb.Defense] = -upkeep.Defense;
                    return Row(
                        u.Name,
                        additive,
                        new PpbVector(),
                        formula: UnitUpkeepFormulas.Explain(upkeep),
                        note: $"{u.TroopCount} troops");
                })
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
            IEnumerable<PpbModifierRow> armyRows,
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
            preCommunity.AddRange(armyRows);

            var preFinal = SummarizeSections(preCommunity);
            var foodFinal = preFinal[Ppb.Food];
            var hunger = HungerPpbFormulas.FromFoodBalance(foodFinal);
            var corruptionFinal = preFinal[Ppb.Corruption];
            var corruption = CorruptionPpbFormulas.FromCorruptionBalance(corruptionFinal);
            var unrestValue = Math.Max(0m, unrest);

            var hungerRow = Row(
                CommunitySource.Hunger,
                HungerPpbFormulas.ComputeAdditive(hunger),
                HungerPpbFormulas.ComputePercent(hunger),
                HungerPpbFormulas.FormulaSummary(foodFinal, hunger),
                HungerPpbFormulas.CatalogDescription);
            var unrestRow = Row(
                CommunitySource.Unrest,
                UnrestPpbFormulas.ComputeAdditive(unrestValue),
                UnrestPpbFormulas.ComputePercent(unrestValue),
                UnrestPpbFormulas.FormulaSummary(unrestValue),
                UnrestPpbFormulas.CatalogDescription);
            var corruptionRow = Row(
                CommunitySource.Corruption,
                CorruptionPpbFormulas.ComputeAdditive(corruption),
                CorruptionPpbFormulas.ComputePercent(corruption),
                CorruptionPpbFormulas.FormulaSummary(corruptionFinal, corruption),
                CorruptionPpbFormulas.CatalogDescription);

            // Crime = max(0, −Final Law). Crime does not modify Law, so Final Law =
            // Law after Hunger + Unrest (and other non-Crime rows that touch Law).
            var beforeCrime = new List<PpbModifierRow>(preCommunity) { hungerRow, unrestRow };
            var lawFinal = SummarizeSections(beforeCrime)[Ppb.Law];
            var crime = CrimePpbFormulas.FromLawBalance(lawFinal);
            var crimeRow = Row(
                CommunitySource.Crime,
                CrimePpbFormulas.ComputeAdditive(crime),
                CrimePpbFormulas.ComputePercent(crime),
                CrimePpbFormulas.FormulaSummary(lawFinal, crime),
                CrimePpbFormulas.CatalogDescription);

            // Economy conjuncture uses Domain Final Economy after other Community rows
            // (Hunger/Crime/Corruption/Unrest). That row does not modify Economy, so no loop.
            var beforeEconomy = new List<PpbModifierRow>(preCommunity)
            {
                hungerRow,
                crimeRow,
                corruptionRow,
                unrestRow,
            };
            var economyE = SummarizeSections(beforeEconomy)[Ppb.Economy];

            return new List<PpbModifierRow>
            {
                hungerRow,
                crimeRow,
                corruptionRow,
                unrestRow,
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
        /// Additive values that percent modifiers scale: positive per row only.
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
                    var v = row.Additive[info.Key];
                    if (v > 0m)
                        sum[info.Key] += v;
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
            var improvementRows = ImprovementRows(
                ov.Improvements, ov.Tiles, ov.Fiefs, ov.Barony.VassalTributePercent, ov.Barony.Season);
            var decreeRows = DecreeSectionRows(
                ov.Decrees,
                ov.Buildings,
                ov.Improvements,
                ov.Barony.TradeTreaties,
                ov.Barony.TradeGoodMgOverrideKeys,
                ov.Barony.LuxuryGoodsAccessKey);
            var tradeGoodRows = TradeGoodDomainRows(
                ov.Buildings,
                ov.Improvements,
                ov.Barony.TradeTreaties,
                ov.Barony.TradeGoodMgOverrideKeys,
                ov.Barony.LuxuryGoodsAccessKey);
            var eventRows = EventRows(ov.Events, ov.Barony.TurnNumber);
            eventRows.AddRange(AudienceEventRows(ov.Audiences, ov.Barony.TurnNumber));
            var armyRows = ArmyRows(ov.Units);
            var settlementPop = SumSettlementPopulation(ov.Barony.Id, ov.Buildings, ov.Improvements);
            var communityRows = CommunityRows(
                advisorRows, buildingRows, socialRows, improvementRows, decreeRows, eventRows, armyRows,
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
            allRows.AddRange(armyRows);
            allRows.AddRange(communityRows);

            return new DomainPanelRowSet
            {
                Advisors = advisors,
                AdvisorRows = advisorRows,
                BuildingRows = buildingRows,
                SocialRows = socialRows,
                ImprovementRows = improvementRows,
                DecreeRows = decreeRows,
                TradeGoodRows = tradeGoodRows,
                EventRows = eventRows,
                ArmyRows = armyRows,
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

        /// <summary>
        /// Domain Panel Final Gold income before expenses, used as the base for liege tribute.
        /// </summary>
        public static decimal GrossGoldIncome(DomainPanelRowSet panel)
        {
            var pos = panel.AllRows.Sum(r => Math.Max(0m, r.Additive[Ppb.Treasury]));
            var neg = panel.AllRows.Sum(r => Math.Max(0m, -r.Additive[Ppb.Treasury]));
            var additiveNet = pos - neg;
            var goldFinal = panel.GrandTotal[Ppb.Treasury];
            // Remainder so Income − Expenses (before tribute) reconciles exactly to Domain Panel Final.
            var scaling = goldFinal - additiveNet;
            return PpbFormat.Round(pos + Math.Max(0m, scaling));
        }

        /// <summary>
        /// Expected resource delta for HUD / Resources: Domain Panel Final minus liege tribute on Gold.
        /// Audience cumulative grants are applied to stocks immediately (Resource Balance), not here.
        /// </summary>
        public static PpbVector ExpectedResourceIncome(
            BaronyOverviewDTO ov,
            CharacterDTO? character = null,
            IEnumerable<BaronInfluenceModifierDTO>? baronModifiers = null,
            IEnumerable<AdvisorInfluenceModifierDTO>? advisorModifiers = null,
            int managementJc = BaronTimeRules.RequiredManagementJc)
        {
            var panel = BuildDomainPanelRows(ov, character, baronModifiers, advisorModifiers, managementJc);
            var expected = ResourceCatalog.Slice(panel.GrandTotal);
            var gross = GrossGoldIncome(panel);
            var tribute = FiefTributeFormulas.ComputeTribute(gross, ov.Barony.LiegeTributePercent);
            expected[Ppb.Treasury] = PpbFormat.Round(expected[Ppb.Treasury] - tribute);
            return ResourceCatalog.Slice(expected);
        }

        public sealed class DomainPanelRowSet
        {
            public List<AdvisorDTO> Advisors { get; init; } = new();
            public List<PpbModifierRow> AdvisorRows { get; init; } = new();
            public List<PpbModifierRow> BuildingRows { get; init; } = new();
            public List<PpbModifierRow> SocialRows { get; init; } = new();
            public List<PpbModifierRow> ImprovementRows { get; init; } = new();
            public List<PpbModifierRow> DecreeRows { get; init; } = new();
            public List<PpbModifierRow> TradeGoodRows { get; init; } = new();
            public List<PpbModifierRow> EventRows { get; init; } = new();
            public List<PpbModifierRow> ArmyRows { get; init; } = new();
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
                        $"Skill PPB scaled by management BT. "
                        + $"{managementJc}/{BaronTimeRules.RequiredManagementJc} BT = {factorPct}% of From Skills. "
                        + "Penalty values are rounded to whole numbers.",
                    Formula =
                        $"management BT {managementJc}/{BaronTimeRules.RequiredManagementJc} → {factorPct}% skills",
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

        /// <summary>Effective skill PPB after management BT factor (0–100%).</summary>
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
        /// Lord's Seat contribution to PHP. For each active room with a purpose:
        /// Prestige / Honor / Fear = Σ (chamber multiplier × purpose additive value).
        /// The same chamber multiplier applies to all three metrics.
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
            decimal honor = 0m;
            decimal fear = 0m;
            foreach (var room in seat.Rooms)
            {
                if (room.IsRuin)
                    continue;
                if (room.PurposeTemplateId is not int pid || !purposes.TryGetValue(pid, out var purpose))
                    continue;

                var mult = room.PrestigeMultiplier;
                prestige += mult * purpose.AdditivePrestige;
                honor += mult * purpose.AdditiveHonor;
                fear += mult * purpose.AdditiveFear;
            }

            return new PhpTotals(
                Prestige: (int)Math.Round(prestige, MidpointRounding.AwayFromZero),
                Honor: (int)Math.Round(honor, MidpointRounding.AwayFromZero),
                Fear: (int)Math.Round(fear, MidpointRounding.AwayFromZero));
        }

        public static List<BaronPhpRow> BuildPhpRows(
            PhpTotals seatContribution,
            PhpTotals itemsContribution,
            IEnumerable<BaronPhpSourceDTO>? customSources,
            PhpTotals adventuresContribution = default)
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
                        "Sum of Prestige, Honor and Fear from Lord's Seat chambers "
                        + "(purpose additive value × chamber multiplier).",
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
                new()
                {
                    Source = BaronPhpSourceLabel.FromAdventures,
                    Prestige = adventuresContribution.Prestige,
                    Honor = adventuresContribution.Honor,
                    Fear = adventuresContribution.Fear,
                    IsSystem = true,
                    Description =
                        "Prestige, Honor and Fear granted through baronial audiences "
                        + "(petitioner adventures). Deferred and dismissed audiences are excluded.",
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

        /// <summary>Lifetime PHP for Baron Card (active + resolved; not deferred/dismissed).</summary>
        public static PhpTotals AudiencePhpContribution(IEnumerable<BaronAudienceDTO>? audiences)
        {
            int prestige = 0, honor = 0, fear = 0;
            foreach (var a in audiences ?? Enumerable.Empty<BaronAudienceDTO>())
            {
                if (!BaronAudiencePpb.ContributesToPhp(a.Status))
                    continue;
                prestige += a.Prestige;
                honor += a.Honor;
                fear += a.Fear;
            }

            return new PhpTotals(prestige, honor, fear);
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

        // --- Baron Card: Time (BT) ---

        /// <summary>
        /// Attribute score used for BT pool (same base as health: SumAbsolute).
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
        /// BT pool: (Endurance + Willpower) × 10, then ± percent modifiers.
        /// Management = sum of Management actions; Adventure weeks = Adventure BT / 25.
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

        public static string FormatBt(int value) => $"{value} BT";

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
