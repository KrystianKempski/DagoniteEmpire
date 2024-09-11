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
        }
        public CharacterDTO Character { get; set; } = new CharacterDTO();
        public RaceDTO CurrentRace { get; set; } = new RaceDTO();
        public ProfessionDTO Profession { get; set; } = new ProfessionDTO();
        public IDictionary<string,AttributeDTO> Attributes { get; set; } =new Dictionary<string, AttributeDTO>();
        public IEnumerable<BaseSkillDTO> BaseSkills { get; set; } = Enumerable.Empty<BaseSkillDTO>();
        //public ICollection<SpecialSkillDTO> SpecialSkills { get; set; } = new HashSet<SpecialSkillDTO>();

        public SpecialSkillModel SpecialSkills { get; set; }
        public ICollection<TraitAdvDTO> TraitsAdv { get; set; } = new List<TraitAdvDTO>();
        public ICollection<TraitDTO> Traits { get; set; } = new List<TraitDTO>();
        public ICollection<RaceDTO> Races { get; set; } = new List<RaceDTO>();
        public ICollection<EquipmentSlotDTO> EquipmentSlots { get; set; } = new List<EquipmentSlotDTO>();
        public BattlePropertyModel BattleProperties { get; set; }


    public bool IsAdminOrMG { get; set; } = false;

        public async Task AllTraitsChange()
        {
            AdvTraitsChange();
            RaceTraitsChange();
            GearChange();
        }

        public async Task TraitsChange(string? TraitType)
        {
            if (TraitType.IsNullOrEmpty())
                throw new Exception("No trait type");

            if(TraitType == SD.TraitType_Advantage)
                await AdvTraitsChange();
            else if (TraitType == SD.TraitType_Race)
                await RaceTraitsChange();
            else if(TraitType == SD.TraitType_Gear)
                await GearChange();
        }

        public async Task AdvTraitsChange()
        {
            IEnumerable<FeatureDTO>[] allFeatures = { Attributes.Values.ToArray(), BaseSkills, SpecialSkills.GetAllArray() };

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
            await CalculateTraits(Traits, SD.TraitType_Advantage);
        }

        public async Task RaceTraitsChange()
        {
            IEnumerable<FeatureDTO>[] allFeatures = { Attributes.Values.ToArray(), BaseSkills, SpecialSkills.GetAllArray() };

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
            await CalculateTraits(CurrentRace.Traits.Cast<TraitDTO>().ToList(), SD.TraitType_Race);
        }

        public async Task GearChange()
        {
            IEnumerable<FeatureDTO>[] allFeatures = { Attributes.Values.ToArray(), BaseSkills, SpecialSkills.GetAllArray() };

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
                    await CalculateTraits(slot.Equipment.Traits.Cast<TraitDTO>().ToList(), SD.TraitType_Gear);
                }
            }
            await BattleProperties. CalculateBattleStats();
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
                            feature = Attributes[bonus.FeatureName];
                            break;
                        case SD.FeatureBaseSkill:
                            feature = BaseSkills.FirstOrDefault(u => u.Name == bonus.FeatureName);
                            break;
                        case SD.FeatureSpecialSkill:
                            feature = SpecialSkills.Get(bonus.FeatureName);
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
                        int newVal = (int)feature.GetType().GetProperty(bonusName).GetValue(feature, null) + bonus.BonusValue;
                        feature.GetType().GetProperty(bonusName).SetValue(feature, newVal);
                    }
                }
            }
        }
    }
}
