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
        /// Domain Panel list: baron row from Baron Card influence + non-baron advisors from DB.
        /// </summary>
        public static List<AdvisorDTO> AdvisorsForDomainPanel(
            IEnumerable<AdvisorDTO>? advisors,
            BaronyDTO barony,
            CharacterDTO? character,
            IEnumerable<BaronInfluenceModifierDTO>? baronModifiers)
        {
            var offices = OrderAdvisors(advisors?.Where(a => !a.IsBaron)).ToList();
            var existingBaron = advisors?.FirstOrDefault(a => a.IsBaron);
            offices.Insert(0, BuildBaronAdvisorRow(barony, character, baronModifiers, existingBaron));
            return offices;
        }

        public static AdvisorDTO BuildBaronAdvisorRow(
            BaronyDTO barony,
            CharacterDTO? character,
            IEnumerable<BaronInfluenceModifierDTO>? baronModifiers,
            AdvisorDTO? existingBaronAdvisor = null)
        {
            var skillInfluence = InfluenceFromSkills(character);
            var skillBasedAdditive = BaronSkillAdditiveToAdvisor(skillInfluence);
            var skillBasedPercent = BaronFoodPercentToAdvisor(skillInfluence[Ppb.Food]);
            var customAdditive = PpbVector.Sum((baronModifiers ?? Enumerable.Empty<BaronInfluenceModifierDTO>())
                .Select(m => m.Additive));
            skillBasedAdditive.AddInPlace(customAdditive);

            var name = !string.IsNullOrWhiteSpace(character?.NPCName)
                ? character!.NPCName
                : !string.IsNullOrWhiteSpace(existingBaronAdvisor?.PersonName)
                    ? existingBaronAdvisor!.PersonName
                    : "Baron";

            return new AdvisorDTO
            {
                Id = existingBaronAdvisor?.Id ?? 0,
                BaronyId = barony.Id,
                OfficeType = OfficeType.Baron,
                Title = "Baron",
                PersonName = name,
                IsBaron = true,
                Additive = skillBasedAdditive,
                Percent = skillBasedPercent,
                FormulaText = "Food skill: +X% to all PPB except Gold, and −X% Corruption; additive: +Stability/Loyalty/Law/Science/Magic/Culture/Intelligence and −Corruption from baron skills.",
            };
        }

        private static PpbVector BaronSkillAdditiveToAdvisor(PpbVector skillInfluence)
        {
            var v = new PpbVector();
            v.EnsureSize();
            v[Ppb.Stability] = skillInfluence[Ppb.Stability];
            v[Ppb.Loyalty] = skillInfluence[Ppb.Loyalty];
            v[Ppb.Law] = skillInfluence[Ppb.Law];
            v[Ppb.Science] = skillInfluence[Ppb.Science];
            v[Ppb.Magic] = skillInfluence[Ppb.Magic];
            v[Ppb.Culture] = skillInfluence[Ppb.Culture];
            v[Ppb.Intelligence] = skillInfluence[Ppb.Intelligence];
            v[Ppb.Corruption] = -skillInfluence[Ppb.Corruption];
            return v;
        }

        private static PpbVector BaronFoodPercentToAdvisor(decimal foodSkill)
        {
            var v = new PpbVector();
            v.EnsureSize();
            foreach (Ppb key in Enum.GetValues<Ppb>())
            {
                if (key == Ppb.Treasury)
                    continue;
                if (key == Ppb.Corruption)
                {
                    v[key] = -foodSkill;
                    continue;
                }
                v[key] = foodSkill;
            }
            return v;
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

        /// <summary>Fixed city buildings present in every barony (not stored in DB).</summary>
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
                    Kind = BuildingKind.Building,
                    Additive = A(production: 2, loyalty: 2, stability: 3, law: 1, corruption: 1,
                        science: 1, culture: 1, intelligence: 1, defense: 1, treasury: 15),
                    Description = "Upkeep: -15 gold per turn.",
                },
                new BaronyBuildingDTO
                {
                    BaronyId = baronyId,
                    Name = "Inn and Lodging",
                    Kind = BuildingKind.Building,
                    Additive = A(treasury: 1),
                    Description = "Roadside inn and lodgings for travelers.",
                },
                new BaronyBuildingDTO
                {
                    BaronyId = baronyId,
                    Name = "Ruler's Seat",
                    Kind = BuildingKind.Building,
                    Additive = A(food: 1, economy: -0.5m, production: 1, loyalty: 1, stability: 2, law: 2,
                        corruption: 2, science: 1, culture: 1, defense: 2, treasury: 5),
                    Description = "Upkeep: -30 gold per turn. Treasury income about +4–5.",
                },
            };
        }

        /// <summary>Core buildings plus map towns and catalog instances saved for the barony.</summary>
        public static List<PpbModifierRow> CityBuildingSectionRows(
            int baronyId,
            IEnumerable<BaronyBuildingDTO> saved,
            IEnumerable<TerrainImprovementDTO>? improvements = null)
        {
            var rows = CoreCityBuildings(baronyId)
                .Select(b => Row(b.Name, b.Additive, b.Percent, null, b.Description))
                .ToList();
            rows.AddRange(TownPopulationRows(improvements));
            rows.AddRange(BuildingRows(saved));
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
            IEnumerable<TerrainImprovementDTO>? improvements = null)
        {
            var vectors = CoreCityBuildings(baronyId).Select(b => b.Additive)
                .Concat(ActiveTowns(improvements).Select(t => t.Additive))
                .Concat(saved.Select(b => b.Additive));
            return PpbVector.Sum(vectors);
        }

        public static PpbVector SumCityBuildingsPercent(
            int baronyId,
            IEnumerable<BaronyBuildingDTO> saved,
            IEnumerable<TerrainImprovementDTO>? improvements = null)
        {
            var vectors = CoreCityBuildings(baronyId).Select(b => b.Percent)
                .Concat(ActiveTowns(improvements).Select(t => t.Percent))
                .Concat(saved.Select(b => b.Percent));
            return PpbVector.Sum(vectors);
        }

        public static int SumCityPopulation(
            int baronyId,
            IEnumerable<BaronyBuildingDTO> saved,
            IEnumerable<TerrainImprovementDTO>? improvements = null) =>
            CoreCityBuildings(baronyId).Sum(b => b.Population)
            + ActiveTowns(improvements).Sum(t => t.Population)
            + saved.Sum(b => b.Population);

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
            int unrest)
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

            return new List<PpbModifierRow>
            {
                Row(CommunitySource.Hunger, HungerPpbFormulas.ComputeAdditive(hunger), HungerPpbFormulas.ComputePercent(hunger)),
                Row(CommunitySource.Crime, CrimePpbFormulas.ComputeAdditive(crime), CrimePpbFormulas.ComputePercent(crime)),
                Row(CommunitySource.Corruption, CorruptionPpbFormulas.ComputeAdditive(corruption), CorruptionPpbFormulas.ComputePercent(corruption)),
                Row(CommunitySource.Unrest, UnrestPpbFormulas.ComputeAdditive(unrestValue), UnrestPpbFormulas.ComputePercent(unrestValue)),
            };
        }

        /// <summary>Additive sum of rows (for section totals).</summary>
        public static PpbVector SumAdditive(IEnumerable<PpbModifierRow> rows)
            => PpbVector.Sum(rows.Select(r => r.Additive));

        /// <summary>Percent sum of rows.</summary>
        public static PpbVector SumPercent(IEnumerable<PpbModifierRow> rows)
            => PpbVector.Sum(rows.Select(r => r.Percent));

        /// <summary>
        /// Simplified section "glance vector" for chips in collapsed headers:
        /// additive sum + percent sum (informational only).
        /// </summary>
        public static PpbVector SectionGlance(IEnumerable<PpbModifierRow> rows)
        {
            var list = rows.ToList();
            var glance = SumAdditive(list);
            glance.AddInPlace(SumPercent(list));
            return glance;
        }

        /// <summary>Grand total of all sections against PPB base values.</summary>
        public static PpbVector GrandTotal(BaronyOverviewDTO ov)
        {
            var advisorRows = AdvisorRows(ov.Advisors);
            var buildingRows = BuildingRows(ov.Buildings);
            var socialRows = SocialRows(ov.Barony.Id, ov.SocialRelations);
            var improvementRows = ImprovementRows(ov.Improvements);
            var decreeRows = DecreeRows(ov.Decrees);
            var eventRows = EventRows(ov.Events, ov.Barony.TurnNumber);
            var communityRows = CommunityRows(
                advisorRows, buildingRows, socialRows, improvementRows, decreeRows, eventRows,
                ov.Barony.Unrest);

            var allRows = new List<PpbModifierRow>();
            allRows.AddRange(advisorRows);
            allRows.AddRange(buildingRows);
            allRows.AddRange(socialRows);
            allRows.AddRange(improvementRows);
            allRows.AddRange(decreeRows);
            allRows.AddRange(eventRows);
            allRows.AddRange(communityRows);

            var additive = SumAdditive(allRows);
            var percent = SumPercent(allRows);
            return PpbMath.Summarize(ov.Barony.BaseParameters, additive, percent);
        }

        // --- Baron Card: influence on barony ---

        public static List<BaronInfluenceRow> BuildInfluenceRows(
            CharacterDTO? character,
            int prestige,
            int honor,
            IEnumerable<BaronInfluenceModifierDTO>? customModifiers)
        {
            var rows = new List<BaronInfluenceRow>
            {
                new()
                {
                    Source = BaronInfluenceSource.FromSkills,
                    Values = InfluenceFromSkills(character),
                    IsSystem = true,
                    Description = BaronSkillPpbFormulas.CatalogDescription,
                    ValueTooltip = BaronSkillPpbFormulas.ExplainAdditive,
                },
                new()
                {
                    Source = BaronInfluenceSource.FromPrestigeHonor,
                    Values = InfluenceFromPrestigeHonor(prestige, honor),
                    IsSystem = true,
                    Formula = "Bonuses from prestige and honor [TO BE COMPLETED]",
                },
            };

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

        public static PpbVector InfluenceFromPrestigeHonor(int prestige, int honor)
        {
            var values = new PpbVector();
            values[Ppb.Loyalty] = prestige / 2;
            values[Ppb.Culture] = prestige / 3;
            values[Ppb.Stability] = honor / 2;
            values[Ppb.Law] = honor / 3;
            values[Ppb.Defense] = honor / 4;
            return values;
        }

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
            var rows = new List<AdvisorInfluenceRow>
            {
                new()
                {
                    Source = AdvisorInfluenceSource.FromSkills,
                    Values = advisor.Skills.Clone(),
                    IsSystem = true,
                    SystemKind = AdvisorInfluenceSystemKind.Skills,
                    Formula = "Administrative skills of the office holder [TO BE COMPLETED]",
                },
            };

            foreach (var modifier in customModifiers ?? Enumerable.Empty<AdvisorInfluenceModifierDTO>())
            {
                rows.Add(new AdvisorInfluenceRow
                {
                    Source = modifier.Source,
                    Values = modifier.Additive,
                    IsSystem = false,
                    ModifierId = modifier.Id,
                    Formula = modifier.FormulaText,
                    Description = modifier.Description,
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
                if (row.SystemKind == AdvisorInfluenceSystemKind.Skills && significantSkills is not null)
                    sum.AddInPlace(AdvisorSignificantSkills.MaskToSignificant(row.Values, significantSkills));
                else
                    sum.AddInPlace(row.Values);
            }
            return sum;
        }

        public static void SyncAdvisorAdditive(AdvisorDTO advisor, IEnumerable<AdvisorInfluenceModifierDTO>? customModifiers)
        {
            var significant = EffectiveSignificantSkills(advisor);
            var total = AdvisorSignificantSkills.MaskToSignificant(advisor.Skills, significant);
            foreach (var modifier in customModifiers ?? Enumerable.Empty<AdvisorInfluenceModifierDTO>())
                total.AddInPlace(modifier.Additive);
            advisor.Additive = total;
        }
    }
}
