using DA_Common;
using DA_DataAccess.CharacterClasses;
using DA_Models.ComponentModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace DA_Models.CharacterModels
{
    public class ProfessionDTO
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string RelatedAttributeName { get; set; } = string.Empty;
        public int ClassLevel { get; set; } = 1;

        public int MaxFocusPoints {
            get
            {
                int res = ProfessionSkillRoll * 4 + 4;
                if (res < 4)
                    res = 4;
                return res;
            }
        }
        public int CurrentFocusPoints { get; set; } = 0;

        public ICollection<CharacterDTO>? Characters { get; set; } = new List<CharacterDTO>();

        public ICollection<TraitProfessionDTO>? Traits { get; set; }

        public SpellcasterType CasterType { get; set; } = SpellcasterType.None;

        public ICollection<SpellCircleDTO>? SpellCircles { get;set; }


        public bool IsApproved { get; set; } = false;

        public bool IsUniversal { get; set; } = false;
        // dto
        public int ProfessionSkillRoll { get; set; } = 0;


        public AttributeDTO? RelatedAttribute { get; set; }

        public ICollection<TraitCharacterDTO> TraitsToAdd = new List<TraitCharacterDTO>();
        public ICollection<TraitDTO> TraitsToDelete = new List<TraitDTO>();

        public ProfessionDTO()
        {
            Traits = new List<TraitProfessionDTO>()
            {
                //active
                new TraitProfessionDTO(){ Level = 1, Index=0},
                new TraitProfessionDTO(){ Level = 1, Index=1},
                new TraitProfessionDTO(){ Level = 2, Index=2},
                new TraitProfessionDTO(){ Level = 2, Index=3},
                new TraitProfessionDTO(){ Level = 3, Index=4},
                new TraitProfessionDTO(){ Level = 3, Index=5},
                new TraitProfessionDTO(){ Level = 4, Index=6},
                new TraitProfessionDTO(){ Level = 4, Index=7},
                new TraitProfessionDTO(){ Level = 5, Index=8},
                new TraitProfessionDTO(){ Level = 5, Index=9},
                new TraitProfessionDTO(){ Level = 6, Index=10},
                new TraitProfessionDTO(){ Level = 6, Index=11},
                new TraitProfessionDTO(){ Level = 7, Index=12},
                new TraitProfessionDTO(){ Level = 7, Index=13},
                // passive
                new TraitProfessionDTO(){ Level = 1, Index=0, IsActiveSkill = false},
                new TraitProfessionDTO(){ Level = 2, Index=1, IsActiveSkill = false},
                new TraitProfessionDTO(){ Level = 3, Index=2, IsActiveSkill = false},
                new TraitProfessionDTO(){ Level = 4, Index=3, IsActiveSkill = false},
                new TraitProfessionDTO(){ Level = 5, Index=4, IsActiveSkill = false},
                new TraitProfessionDTO(){ Level = 6, Index=5, IsActiveSkill = false},
                new TraitProfessionDTO(){ Level = 7, Index=6, IsActiveSkill = false},
            };
            Description = string.Empty;
        }
        public void CalculateClassParams(IDictionary<string, AttributeDTO> attributes)
        {
            if (string.IsNullOrEmpty(RelatedAttributeName))
                return;
            var attr = attributes[RelatedAttributeName];
            if (attr is null)
                return;
            RelatedAttribute = attr;
            ProfessionSkillRoll = attr.ModifierAbsolute  + ClassLevel;
        }

        public void AddPropertyListener(AttributeDTO attr)
        {
            if (attr == null) return;
            RelatedAttribute = attr;
        }


        public string AddSpellCircle(int newLevel)
        {
            try
            {
                if (newLevel > 9)
                     return "Max spell circle level reached";
                if (newLevel < 1)
                    return "Too lower circle level";
                if (SpellCircles is null)
                    return "";
                if (SpellCircles.FirstOrDefault(c => c.Level == newLevel) != null)
                    return "";

                SpellCircleDTO circle = new();
                circle.ProfessionId = Id;
                circle.Profession = this;
                circle.Level = newLevel;
                circle.CalculateSpells();
                
                SpellCircles.Add(circle);
                return "Circle " + newLevel.ToString() + "added";
            }
            catch (WarningException ex)
            {
                return ex.ToString();
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        public void StartProfessionPage(AllParamsModel allParams)
        {
            if (Traits is null || allParams is null)
            {
                return;
            }

            foreach (var skill in Traits.Where(u => u.IsActiveSkill))
            {
                skill.UseCount = GetSkillUseCount(skill, allParams);
                skill.IsInUse = skill.UseCount > 0;
            }
        }

        public int GetSkillUseCount(TraitProfessionDTO skill, AllParamsModel allParams)
        {
            var pending = TraitsToAdd?.Count(t => t.Name == skill.Name) ?? 0;
            var persisted = allParams.TraitsTemporary?.Count(t => t.Name == skill.Name) ?? 0;
            return pending + persisted;
        }

        public int GetMaxSkillUseCount(TraitProfessionDTO skill, AllParamsModel allParams)
        {
            if (skill.Cost <= 0)
            {
                return GetSkillUseCount(skill, allParams);
            }

            var current = GetSkillUseCount(skill, allParams);
            return current + (CurrentFocusPoints / skill.Cost);
        }

        public string SetSkillUseCount(TraitProfessionDTO skill, int newCount, AllParamsModel allParams)
        {
            if (string.IsNullOrEmpty(skill.Name))
            {
                return "";
            }

            if (newCount < 0)
            {
                newCount = 0;
            }

            var current = GetSkillUseCount(skill, allParams);
            if (newCount == current)
            {
                skill.UseCount = newCount;
                skill.IsInUse = newCount > 0;
                return "";
            }

            if (newCount > current)
            {
                var toAdd = newCount - current;
                var costNeeded = toAdd * skill.Cost;
                if (CurrentFocusPoints < costNeeded)
                {
                    return "Not enough focus points";
                }

                for (var i = 0; i < toAdd; i++)
                {
                    var tempSkill = new TraitCharacterDTO(skill, true)
                    {
                        IsRemovable = false
                    };
                    TraitsToAdd.Add(tempSkill);
                }

                CurrentFocusPoints -= costNeeded;
            }
            else
            {
                var toRemove = current - newCount;
                for (var i = 0; i < toRemove; i++)
                {
                    RemoveOneSkillUse(skill, allParams);
                }
            }

            skill.UseCount = newCount;
            skill.IsInUse = newCount > 0;
            return "";
        }

        private void RemoveOneSkillUse(TraitProfessionDTO skill, AllParamsModel allParams)
        {
            var pending = TraitsToAdd?.FirstOrDefault(t => t.Name == skill.Name);
            if (pending is not null)
            {
                TraitsToAdd.Remove(pending);
                CurrentFocusPoints += skill.Cost;
                return;
            }

            var deleteSkill = allParams.TraitsTemporary?.FirstOrDefault(u => u.Name == skill.Name);
            if (deleteSkill is null)
            {
                return;
            }

            if (TraitsToDelete.FirstOrDefault(u => u.Id == deleteSkill.Id) is null)
            {
                TraitsToDelete.Add(deleteSkill);
            }

            allParams.TraitsTemporary?.Remove(deleteSkill);
            CurrentFocusPoints += skill.Cost;
        }

        private void UnCheckSkill(TraitProfessionDTO skill, AllParamsModel allParams)
        {
            SetSkillUseCount(skill, 0, allParams);
        }

        public string Rest(AllParamsModel AllParams)
        {
            if (CasterType != SpellcasterType.None)
            {
                if (SpellCircles is not null)
                {

                    foreach (var circle in SpellCircles)
                    {
                        circle.CalculateSpells();
                        if (circle.SpellSlots is not null)
                        {
                            foreach (var slot in circle.SpellSlots)
                            {
                                if (CasterType == SpellcasterType.Sorcerer)
                                {
                                    slot.Prepared = circle.PerDay;
                                }
                                else
                                {
                                    slot.Prepared = 0;
                                }
                            }
                        }
                    }
                }
            }
            foreach (var skill in Traits.Where(u => u.IsActiveSkill == true))
            {
                UnCheckSkill(skill, AllParams);
            }
            CurrentFocusPoints = MaxFocusPoints;
            return "";
        }
    }
}
