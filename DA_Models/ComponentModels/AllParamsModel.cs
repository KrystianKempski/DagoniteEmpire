using DA_Common;
using DA_Models.CharacterModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MudBlazor.CategoryTypes;

namespace DA_Models.ComponentModels
{
    public class AllParamsModel
    {
        public AllParamsModel()
        {
            InitBattleProperties();
        }
        public CharacterDTO Character { get; set; } = new CharacterDTO();
        public RaceDTO CurrentRace { get; set; } = new RaceDTO();
        public ProfessionDTO Profession { get; set; } = new ProfessionDTO();
        public IEnumerable<AttributeDTO> Attributes { get; set; } = Enumerable.Empty<AttributeDTO>();
        public IEnumerable<BaseSkillDTO> BaseSkills { get; set; } = Enumerable.Empty<BaseSkillDTO>();
        public ICollection<SpecialSkillDTO> SpecialSkills { get; set; } = new HashSet<SpecialSkillDTO>();
        public ICollection<TraitAdvDTO> TraitsAdv { get; set; } = new List<TraitAdvDTO>();
        public ICollection<TraitDTO> Traits { get; set; } = new List<TraitDTO>();
        public ICollection<RaceDTO> Races { get; set; } = new List<RaceDTO>();
        public ICollection<EquipmentSlotDTO> EquipmentSlots { get; set; } = new List<EquipmentSlotDTO>();
        public ICollection<BattlePropertyDTO> BattleProperties { get; set; } = new List<BattlePropertyDTO>();
        public void InitBattleProperties()
        {
            BattleProperties = new List<BattlePropertyDTO>();
            foreach (var prop in SD.BattleProperty.All)
            {
                BattleProperties.Add(new BattlePropertyDTO() { Name = prop, BaseBonus = 0 });
            }
            foreach (var prop in SD.WeaponQuality.All)
            {
                BattleProperties.Add(new BattlePropertyDTO() { Name = prop, BaseBonus = 0 });
            }
        }

    public bool IsAdminOrMG { get; set; } = false;

        public async Task TraitsChange()
        {
            IEnumerable<FeatureDTO>[] allFeatures = { Attributes, BaseSkills, SpecialSkills };
            bool sum = false;

            //clear all traits bonuses
            foreach (var feat in allFeatures)
            {
                foreach (var obj in feat)
                {
                    if (obj.TraitBonus != 0)
                    {
                        obj.TraitBonus = 0;
                        sum = true;
                    }
                    if (obj.RaceBonus != 0)
                    {
                        obj.RaceBonus = 0;
                        sum = true;
                    }
                    if (obj.GearBonus != 0)
                    {
                        obj.GearBonus = 0;
                        sum = true;
                    }
                    if (sum)
                    {
                        sum = false;
                    }
                }
            }

            // calculate all traits adv
            CalculateTraits(Traits, SD.TraitType_Advantage);

            // calculate all race traits
            CalculateTraits(CurrentRace.Traits.Cast<TraitDTO>().ToList(), SD.TraitType_Race);

            foreach (var slot in EquipmentSlots)
            {
                if (slot.Equipment is not null && slot.Equipment.Traits != null && slot.IsEquipped)
                {
                    CalculateTraits(slot.Equipment.Traits.Cast<TraitDTO>().ToList(), SD.TraitType_Gear);
                }
            }
        }

        private async Task CalculateTraits(ICollection<TraitDTO> traits, string TraitType)
        {
            FeatureDTO? feature = null;
            // calculate all traits adv
            string bonusName;
            switch (TraitType)
            {
                case SD.TraitType_Advantage: bonusName = nameof(feature.TraitBonus); break;
                case SD.TraitType_Gear: bonusName = nameof(feature.GearBonus); break;
                case SD.TraitType_Race: bonusName = nameof(feature.RaceBonus); break;
                default: bonusName = string.Empty; break;
            }

            foreach (var trait in traits)
            {
                if (trait.TraitType != SD.TraitType_Advantage)
                    Character.TraitBalance += trait.TraitValue;

                foreach (var bonus in trait.Bonuses)
                {
                    switch (bonus.FeatureType)
                    {
                        case SD.FeatureAttribute:
                            feature = Attributes.FirstOrDefault(u => u.Name == bonus.FeatureName);
                            break;
                        case SD.FeatureBaseSkill:
                            feature = BaseSkills.FirstOrDefault(u => u.Name == bonus.FeatureName);
                            break;
                        case SD.FeatureSpecialSkill:
                            feature = SpecialSkills.FirstOrDefault(u => u.Name == bonus.FeatureName);
                            break;
                        case SD.FeatureWeaponQuality:
                            feature = BattleProperties.FirstOrDefault(u => u.Name == bonus.FeatureName);
                            break;
                        default:
                            feature = null;
                            break;
                    }
                    if (feature != null)
                    {
                        int newVal = (int)feature.GetType().GetProperty(bonusName).GetValue(feature, null) + bonus.BonusValue;
                        feature.GetType().GetProperty(bonusName).SetValue(feature, newVal);
                    }
                }
            }
        }

    }
}
