using DA_Common;
using DA_Models.CharacterModels;
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
        public IDictionary<string, BattlePropertyDTO> BattleProperties { get; set; }

        //public Dictionary<string, int> BattleStats { get; set; } = new();
        public void InitBattleProperties()
        { 
            foreach (var prop in SD.BattleProperty.All)
            {
                BattleProperties[prop] = new BattlePropertyDTO() { Name = prop, BaseBonus = 0 };
            }
            foreach (var prop in SD.WeaponQuality.All)
            {
                BattleProperties[prop] = new BattlePropertyDTO() { Name = prop, BaseBonus = 0 };
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
                            if(bonus.FeatureName is not null)
                                feature = BattleProperties[bonus.FeatureName];
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

        private void GetWeaponQualityFromEquipment(EquipmentDTO equip)
        {
            if (equip is null || equip.Traits is null)
            {
                return;
            }
            BattlePropertyDTO? battleProp;
            foreach (var trait in equip.Traits)
            {
                foreach (var bonus in trait.Bonuses)
                {
                    if (bonus.FeatureType == SD.FeatureWeaponQuality && bonus.FeatureName is not null)
                    {
                        battleProp = BattleProperties[bonus.FeatureName];
                        if(battleProp is not null)
                            battleProp.BaseBonus += bonus.BonusValue;
                    }
                }
            }
        }

        private void AddGearBonusToSpecialSkill(string skillName, int value)
        {
            var skill = SpecialSkills.FirstOrDefault(s => s.Name == skillName);
            skill.GearBonus += value;
        }
        private void ClearGearBonusToSpecialSkill(string skillName)
        {
            var skill = SpecialSkills.FirstOrDefault(s => s.Name == skillName);
            skill.GearBonus=0;
        }
        public async Task CalculateBattleStats()
        {
            string slotTypeMain = Character.WeaponSet == 1 ? SD.SlotType.WeaponMain2 : SD.SlotType.WeaponMain1;
            string slotTypeOff = Character.WeaponSet == 1 ? SD.SlotType.WeaponOff2 : SD.SlotType.WeaponOff1;



            //clear BattleProperies values;
            foreach (var prop in BattleProperties)
            {
                prop.Value.BaseBonus = 0;
                prop.Value.GearBonus = 0;
            }
            //// clear Battle stats bonuses to skills
            foreach (var skill in SpecialSkills)
            {
                if (skill.GearBonus != 0)
                {
                    skill.GearBonus = 0;
                }
            }
            //recalcutate gear traits
            foreach (var slot in EquipmentSlots)
            {
                if (slot.Equipment is not null && slot.Equipment.Traits != null && slot.IsEquipped)
                {
                    CalculateTraits(slot.Equipment.Traits.Cast<TraitDTO>().ToList(), SD.TraitType_Gear);
                }
            }

            EquipmentDTO? WeaponUsed = Character?.EquipmentSlots?.FirstOrDefault(s => s.SlotType == slotTypeMain)?.Equipment;
            if(WeaponUsed is not null)
                GetWeaponQualityFromEquipment(WeaponUsed);
            WeaponUsed = Character?.EquipmentSlots?.FirstOrDefault(s => s.SlotType == slotTypeOff)?.Equipment;
            if (WeaponUsed is not null)
                GetWeaponQualityFromEquipment(WeaponUsed);

            // calculate base propertiers 
            foreach (var prop in BattleProperties)
            {
                if (prop.Value.SumBonus == 0) continue;

                switch (prop.Key)
                {
                    case SD.WeaponQuality.Parrying:
                        BattleProperties[SD.BattleProperty.DefenceDodge].GearBonus += prop.Value.SumBonus;
                        break;
                    case SD.WeaponQuality.Fast:
                        BattleProperties[SD.BattleProperty.AttackDodge].GearBonus += prop.Value.SumBonus;
                        BattleProperties[SD.BattleProperty.AttackParry].GearBonus += prop.Value.SumBonus;
                        break;
                    case SD.WeaponQuality.Slow:
                        BattleProperties[SD.BattleProperty.AttackDodge].GearBonus -= prop.Value.SumBonus;
                        BattleProperties[SD.BattleProperty.AttackParry].GearBonus -= prop.Value.SumBonus;
                        break;
                    case SD.WeaponQuality.Heavy:
                        BattleProperties[SD.BattleProperty.AttackParry].GearBonus += 3 * prop.Value.SumBonus;
                        BattleProperties[SD.BattleProperty.AttackShield].GearBonus += prop.Value.SumBonus;
                        BattleProperties[SD.BattleProperty.AttackArmor].GearBonus += prop.Value.SumBonus;
                        BattleProperties[SD.BattleProperty.DefenceDodge].GearBonus -= prop.Value.SumBonus;
                        break;
                    case SD.WeaponQuality.Devastating:
                        BattleProperties[SD.BattleProperty.DamageBonus].GearBonus += prop.Value.SumBonus;
                        break;
                    case SD.WeaponQuality.Weak:
                        BattleProperties[SD.BattleProperty.DamageBonus].GearBonus -= prop.Value.SumBonus;
                        break;
                    case SD.WeaponQuality.Precise:
                        BattleProperties[SD.BattleProperty.AttackBase].GearBonus += prop.Value.SumBonus;
                        break;
                    case SD.WeaponQuality.Bulky:
                        BattleProperties[SD.BattleProperty.AttackBase].GearBonus -= prop.Value.SumBonus;
                        break;
                    case SD.WeaponQuality.ArmorDefenceBonus:
                        BattleProperties[SD.BattleProperty.DefenceArmor].GearBonus += prop.Value.SumBonus;
                        break;
                    case SD.WeaponQuality.ShieldDefenceBonus:
                        BattleProperties[SD.BattleProperty.DefenceShield].GearBonus += prop.Value.SumBonus;
                        break;
                    case SD.WeaponQuality.ArmorBane:
                        BattleProperties[SD.BattleProperty.DefenceDodge].GearBonus -= prop.Value.SumBonus;
                        foreach (var skill in SD.SpecialSkills.ArmorBaneSkills)
                        {
                            AddGearBonusToSpecialSkill(skill, -prop.Value.SumBonus);
                        }
                        break;
                }
            }



        }

    }
}
