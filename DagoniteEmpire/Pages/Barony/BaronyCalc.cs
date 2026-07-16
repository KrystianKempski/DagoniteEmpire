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
            var influence = SumInfluenceRows(
                BuildInfluenceRows(character, barony.Prestige, barony.Honor, baronModifiers));

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
                Additive = influence,
                Percent = existingBaronAdvisor?.Percent.Clone() ?? new PpbVector(),
                FormulaText = "Baron Card: skills, prestige/honor, and custom sources",
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

        /// <summary>Core buildings plus catalog instances saved for the barony.</summary>
        public static List<PpbModifierRow> CityBuildingSectionRows(int baronyId, IEnumerable<BaronyBuildingDTO> saved)
        {
            var rows = CoreCityBuildings(baronyId)
                .Select(b => Row(b.Name, b.Additive, b.Percent, null, b.Description))
                .ToList();
            rows.AddRange(BuildingRows(saved));
            return rows;
        }

        public static PpbVector SumCityBuildings(int baronyId, IEnumerable<BaronyBuildingDTO> saved)
        {
            var vectors = CoreCityBuildings(baronyId).Select(b => b.Additive)
                .Concat(saved.Select(b => b.Additive));
            return PpbVector.Sum(vectors);
        }

        public static PpbVector SumCityBuildingsPercent(int baronyId, IEnumerable<BaronyBuildingDTO> saved)
        {
            var vectors = CoreCityBuildings(baronyId).Select(b => b.Percent)
                .Concat(saved.Select(b => b.Percent));
            return PpbVector.Sum(vectors);
        }

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
            => improvements.Select(i => Row(i.Name, i.Additive, i.Percent, i.FormulaText, i.Description)).ToList();

        public static List<PpbModifierRow> DecreeRows(IEnumerable<DecreeDTO> decrees)
            => decrees.Select(d => Row(d.Name, d.Additive, d.Percent, d.FormulaText, d.Description)).ToList();

        public static List<PpbModifierRow> EventRows(IEnumerable<BaronyEventDTO> events)
            => events.Where(e => e.IsActive).Select(e => Row(e.Name, e.Additive, e.Percent, null, e.Description)).ToList();

        public static List<PpbModifierRow> CommunityRows(IEnumerable<CommunityModifierDTO> mods)
            => mods.Select(m => Row(m.Source, m.Additive, m.Percent, m.FormulaText)).ToList();

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
            var allRows = new List<PpbModifierRow>();
            allRows.AddRange(AdvisorRows(ov.Advisors));
            allRows.AddRange(BuildingRows(ov.Buildings));
            allRows.AddRange(SocialRows(ov.Barony.Id, ov.SocialRelations));
            allRows.AddRange(ImprovementRows(ov.Improvements));
            allRows.AddRange(DecreeRows(ov.Decrees));
            allRows.AddRange(EventRows(ov.Events));
            allRows.AddRange(CommunityRows(ov.CommunityModifiers));

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
                    Formula = "Extrapolated from baron attributes and skills [TO BE COMPLETED]",
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
            var values = new PpbVector();
            if (character is null)
                return values;

            if (character.Attributes is not null)
            {
                foreach (var attr in character.Attributes)
                {
                    var bonus = attr.SumBonus;
                    if (bonus == 0)
                        continue;

                    switch (attr.Name)
                    {
                        case SD.Attributes.Charisma:
                            values[Ppb.Loyalty] += bonus;
                            values[Ppb.Culture] += bonus / 2;
                            break;
                        case SD.Attributes.Intelligence:
                            values[Ppb.Science] += bonus;
                            values[Ppb.Economy] += bonus / 2;
                            break;
                        case SD.Attributes.Strength:
                            values[Ppb.Defense] += bonus;
                            values[Ppb.Production] += bonus / 2;
                            break;
                        case SD.Attributes.Endurance:
                            values[Ppb.Food] += bonus / 2;
                            values[Ppb.Stability] += bonus;
                            break;
                        case SD.Attributes.Willpower:
                            values[Ppb.Law] += bonus;
                            values[Ppb.Stability] += bonus / 2;
                            break;
                        case SD.Attributes.Instinct:
                            values[Ppb.Intelligence] += bonus;
                            break;
                        case SD.Attributes.Dexterity:
                            values[Ppb.Law] += bonus / 2;
                            values[Ppb.Corruption] -= bonus / 3;
                            break;
                    }
                }
            }

            if (character.BaseSkills is not null)
            {
                foreach (var skill in character.BaseSkills)
                {
                    var bonus = skill.SumBonus;
                    if (bonus == 0)
                        continue;

                    switch (skill.Name)
                    {
                        case SD.BaseSkills.Talk:
                            values[Ppb.Loyalty] += bonus / 2;
                            values[Ppb.Culture] += bonus;
                            break;
                        case SD.BaseSkills.Knowledge:
                            values[Ppb.Science] += bonus;
                            break;
                        case SD.BaseSkills.Craft:
                            values[Ppb.Production] += bonus;
                            break;
                        case SD.BaseSkills.Survival:
                            values[Ppb.Food] += bonus;
                            break;
                        case SD.BaseSkills.Deceit:
                            values[Ppb.Corruption] += bonus / 2;
                            values[Ppb.Intelligence] += bonus / 2;
                            break;
                        case SD.BaseSkills.Medicine:
                            values[Ppb.Food] += bonus / 2;
                            values[Ppb.Stability] += bonus / 2;
                            break;
                    }
                }
            }

            return values;
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
