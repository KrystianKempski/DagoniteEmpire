using Abp.Collections.Extensions;
using Castle.Core;
using DA_Common;
using DA_DataAccess.CharacterClasses;
using DA_Models.CharacterModels;
using DA_Models.ComponentInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DA_Common.SD;
using static MudBlazor.CategoryTypes;


namespace DA_Models.ComponentModels
{
    public class AllParamsModel
    {
        public AllParamsModel()
        {
            BattleProperties = new BattlePropertyModel(this);
            SpecialSkills = new SpecialSkillModel(this);
            Attributes = new AttributesModel(this);
            Health = new HealthModel(this);
        }
        public CharacterDTO Character { get; set; } = new CharacterDTO();
        public RaceDTO CurrentRace { get; set; } = new RaceDTO();
        public ProfessionDTO Profession { get; set; } = new ProfessionDTO();
        public AttributesModel Attributes { get; set; }
        public IEnumerable<BaseSkillDTO> BaseSkills { get; set; } = Enumerable.Empty<BaseSkillDTO>();
        public SpecialSkillModel SpecialSkills { get; set; }
        public ICollection<TraitDTO> TraitsCharacter { get; set; } = new List<TraitDTO>();
        public ICollection<TraitDTO> TraitsTemporary { get; set; } = new List<TraitDTO>();
        public ICollection<TraitDTO> TraitsProfession { get; set; } = new List<TraitDTO>();
        public ICollection<RaceDTO> Races { get; set; } = new List<RaceDTO>();
        public ICollection<LanguageDTO> Languages { get; set; } = new List<LanguageDTO>();
        public IEnumerable<LanguageDTO> AvailableLanguages { get; set; } = Enumerable.Empty<LanguageDTO>();
        public ICollection<EquipmentSlotDTO> EquipmentSlots { get; set; } = new List<EquipmentSlotDTO>();
        public BattlePropertyModel BattleProperties { get; set; }
        public HealthModel Health { get; set; }
        public ICollection<TraitDTO> TraitsToDelete { get; set; } = new List<TraitDTO>();
        public ICollection<EquipmentSlotDTO> EquipmentSlotsToDelete { get; set; } = new List<EquipmentSlotDTO>();
        public ICollection<SpecialSkillDTO> SpecialSkillsToDelete { get; set; } = new List<SpecialSkillDTO>();
        public string States =>
            CombatStateString.Format(TraitsTemporary.Select(t => new CombatStateEntry(t.Name, t.TraitValue)));

        /// <summary>
        /// Extra special-skill TempBonuses applied after temporary traits
        /// (e.g. baron Prestige/Honor/Fear reputation). Not persisted.
        /// </summary>
        public IReadOnlyDictionary<string, int> ExternalSpecialSkillTempBonuses { get; set; } =
            new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        public bool IsAdminOrMG { get; set; } = false;

        public int GetMaxLanguageSlots()
        {
            // Language slots use only the permanent ("stałe") value of Linguistics.
            // SumAbsolute excludes temporary states (TempBonuses) and wounds (HealthBonus).
            var linguistics = SpecialSkills.Get("Linguistics");
            return SD.Languages.GetMaxSlots(linguistics?.SumAbsolute ?? 0);
        }

        // The common language is known for free and does not consume a language slot.
        public int GetUsedLanguageSlots() =>
            Languages.Count(l => !SD.Languages.IsCommon(l.Name));

        // Ensures every character knows the common language ("wspólny").
        public void EnsureCommonLanguage()
        {
            if (Languages.Any(l => SD.Languages.IsCommon(l.Name)))
                return;

            var common = AvailableLanguages.FirstOrDefault(l => SD.Languages.IsCommon(l.Name));
            if (common is not null)
                Languages.Add(common);
        }

        public void AllTraitsChange()
        {
            CharTraitsChange();
            TempTraitsChange();
            RaceTraitsChange();
            GearTraitChange();
            ProfTraitsChange();
        }

        public void TraitsChange(string? TraitType = null)
        {
            if (TraitType.IsNullOrEmpty())
                throw new Exception("No trait type");

            if(TraitType == SD.TraitType_Character)
                CharTraitsChange();
            else if (TraitType == SD.TraitType_Race)
                RaceTraitsChange();
            else if(TraitType == SD.TraitType_Gear)
                GearTraitChange();
            else if (TraitType == SD.TraitType_Temporary)
                TempTraitsChange();
            else if (TraitType == SD.TraitType_Profession)
                ProfTraitsChange();
        }

        /// <summary>Adds <see cref="ExternalSpecialSkillTempBonuses"/> into special-skill TempBonuses.</summary>
        public void ApplyExternalSpecialSkillTempBonuses()
        {
            if (ExternalSpecialSkillTempBonuses is null || ExternalSpecialSkillTempBonuses.Count == 0)
                return;

            foreach (var (name, value) in ExternalSpecialSkillTempBonuses)
            {
                if (value == 0 || string.IsNullOrWhiteSpace(name))
                    continue;
                var skill = SpecialSkills.Get(name);
                if (skill is not null)
                    skill.TempBonuses += value;
            }
        }

        public IEnumerable<FeatureDTO>[] GetAllFeatures()
        {
            IEnumerable<FeatureDTO>[] allFeatures = { Attributes.GetAllArray(), BaseSkills, SpecialSkills.GetAllArray(), BattleProperties.GetAllArray() };
            return allFeatures;
        }

        public void CharTraitsChange()
        {
            IEnumerable<FeatureDTO>[] allFeatures = GetAllFeatures();

            //clear all traits bonuses
            foreach (var feat in allFeatures)
            {
                foreach (var obj in feat)
                {
                    if (obj.TraitBonus != 0)
                    {
                        obj.TraitBonus = 0;
                    }
                }
            }

            // calculate all traits adv
            CalculateTraits(TraitsCharacter, SD.TraitType_Character);

            // recalculate trait balance
            Character.TraitBalance = 0;
            foreach (var trait in TraitsCharacter)
            {
                Character.TraitBalance += trait.TraitValue;
            }
        }

        public void TempTraitsChange()
        {
            IEnumerable<FeatureDTO>[] allFeatures = GetAllFeatures();

            //clear all traits bonuses
            foreach (var feat in allFeatures)
            {
                foreach (var obj in feat)
                {
                    obj.TempBonuses = 0;
                }
            }
            // calculate all traits adv
            CalculateTraits(TraitsTemporary, SD.TraitType_Temporary);
            ApplyExternalSpecialSkillTempBonuses();
        }

        public void RaceTraitsChange()
        {
            IEnumerable<FeatureDTO>[] allFeatures = GetAllFeatures();

            //clear all traits bonuses
            foreach (var feat in allFeatures)
            {
                foreach (var obj in feat)
                {
                    if (obj.RaceBonus != 0)
                    {
                        obj.RaceBonus = 0;
                    }
                }
            }

            // calculate all race traits
            CalculateTraits(CurrentRace.Traits.Cast<TraitDTO>().ToList(), SD.TraitType_Race);
        }

        public void GearTraitChange()
        {
            IEnumerable<FeatureDTO>[] allFeatures = GetAllFeatures();

            //clear all traits bonuses
            foreach (var feat in allFeatures)
            {
                foreach (var obj in feat)
                {
                    if (obj.GearBonus != 0)
                    {
                        obj.GearBonus = 0;
                    }
                }
            }

            // calculate all gear traits in equipped items
            foreach (var slot in EquipmentSlots)
            {
                if (slot.Equipment is not null && slot.Equipment.Traits != null && slot.IsEquipped)
                {
                    CalculateTraits(slot.Equipment.Traits.Cast<TraitDTO>().ToList(), SD.TraitType_Gear);
                }
            }
            BattleProperties.CalculateBattleStats();
        }
        public void ProfTraitsChange()
        {
            IEnumerable<FeatureDTO>[] allFeatures = GetAllFeatures();

            //clear all traits bonuses
            foreach (var feat in allFeatures)
            {
                foreach (var obj in feat)
                {
                    if (obj.OtherBonuses != 0)
                    {
                        obj.OtherBonuses = 0;
                    }
                }
            }

            // calculate all gear traits in equipped items
            if(Profession.Traits is not null && Profession.Traits.Any())
            {
                var traits = Profession.Traits.Where(t => t.IsActiveSkill == false).Cast<TraitDTO>().ToList();
                CalculateTraits(traits, SD.TraitType_Profession);

                // add spell circles
                int spellCircle = 0;
                foreach(var trait in traits)
                {
                    if (trait.Name.StartsWith(SD.ProfessionSkills.WizardMagic) || trait.Name.StartsWith(SD.ProfessionSkills.SorcererMagic))
                        if (trait.Level > spellCircle) spellCircle = trait.Level;
                }
                if (spellCircle > 0) {
                    for(int c = 1; c <= spellCircle; c++)
                    {
                        Profession.AddSpellCircle(c);
                    }
                }
            }
        }

        private void CalculateTraits(ICollection<TraitDTO> traits, string TraitType)
        {
            FeatureDTO? feature = null;
            // calculate all traits adv
            string bonusName;
            switch (TraitType)
            {
                case SD.TraitType_Character: bonusName = nameof(feature.TraitBonus); break;
                case SD.TraitType_Gear: bonusName = nameof(feature.GearBonus); break;
                case SD.TraitType_Race: bonusName = nameof(feature.RaceBonus); break;
                case SD.TraitType_Profession: bonusName = nameof(feature.OtherBonuses); break;
                case SD.TraitType_Temporary: bonusName = nameof(feature.TempBonuses); break;
                default: bonusName = string.Empty; break;
            }

            Profession.CasterType = SpellcasterType.None;
            foreach (var trait in traits)
            {
                if (trait.TraitType == SD.TraitType_Character)
                    Character.TraitBalance += trait.TraitValue;
                else if (trait.TraitType == SD.TraitType_Profession)
                {
                    //determine caster type
                    if (trait.Name.StartsWith(SD.ProfessionSkills.WizardMagic))
                        Profession.CasterType = SpellcasterType.Wizard;
                    else if (trait.Name.StartsWith(SD.ProfessionSkills.SorcererMagic))
                        Profession.CasterType = SpellcasterType.Sorcerer;
                }

                foreach (var bonus in trait.Bonuses)
                {
                    switch (bonus.FeatureType)
                    {
                        case SD.FeatureAttribute:
                            feature = Attributes.Get(bonus.FeatureName);
                            break;
                        case SD.FeatureBaseSkill:
                            feature = BaseSkills.FirstOrDefault(u => u.Name == bonus.FeatureName);
                            break;
                        case SD.FeatureSpecialSkill:
                            feature = SpecialSkills.Get(bonus?.FeatureName);
                            break;
                        case SD.FeatureWeaponQuality:
                            if(bonus.FeatureName is not null)
                                feature = BattleProperties.Get(bonus.FeatureName);
                            break;
                        default:
                            feature = null;
                            break;
                    }
                    if (feature != null)
                    {
                        var newVal = (int?)feature?.GetType()?.GetProperty(bonusName)?.GetValue(feature, null) + bonus.BonusValue;
                        if(newVal is not null)
                            feature?.GetType()?.GetProperty(bonusName)?.SetValue(feature, newVal);
                    }
                }
            }
        }
    }
}
