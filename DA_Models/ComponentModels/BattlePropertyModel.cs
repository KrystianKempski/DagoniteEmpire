﻿using Abp.Collections.Extensions;
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

namespace DA_Models.ComponentModels
{
    public class BattlePropertyModel : ParameterModel<BattlePropertyDTO>
    {

        public BattlePropertyModel(AllParamsModel allParams) : base(allParams)
        {
            foreach (var prop in SD.BattleProperty.All)
            {
                Set(prop, new BattlePropertyDTO() { Name = prop, BaseBonus = 0 });
            }
            foreach (var prop in SD.WeaponQuality.All)
            {
                Set(prop, new BattlePropertyDTO() { Name = prop, BaseBonus = 0 });
            }
        }


        public async Task CalculateBattleStats()
        {
            try
            {

                string slotTypeMain = _allParams.Character.WeaponSet == 1 ? SD.SlotType.WeaponMain2 : SD.SlotType.WeaponMain1;
                string slotTypeOff = _allParams.Character.WeaponSet == 1 ? SD.SlotType.WeaponOff2 : SD.SlotType.WeaponOff1;
                bool isParrying = false;
                //clear BattleProperies values;
                foreach (var prop in GetAllArray())
                {
                    prop.BaseBonus = 0;
                    prop.GearBonus = 0;
                }


                // get currently used weapon
                EquipmentDTO? WeaponUsed = null;
                //get currently used shield
                EquipmentDTO? ShieldUsed = null;
                //get currently used armor
                EquipmentDTO? ArmorUsed = null;

                foreach (var slot in _allParams.EquipmentSlots)
                {
                    if (slot.SlotType == SD.SlotType.WeaponMain1 || slot.SlotType == SD.SlotType.WeaponMain2)
                    {
                        if (slot.SlotType != slotTypeMain)
                            continue;
                        WeaponUsed = slot.Equipment;
                    }
                    if (slot.SlotType == SD.SlotType.WeaponOff1 || slot.SlotType == SD.SlotType.WeaponOff2){
                        if(slot.SlotType != slotTypeOff || slot.Equipment.EquipmentType != SD.EquipmentType.Shield)
                            continue;
                        else
                            ShieldUsed = slot.Equipment;
                    }
                    if (slot.SlotType == SD.SlotType.Body)
                    {
                        ArmorUsed = slot.Equipment;
                    }

                    if (slot.IsEquipped)
                        AddWeaponQualityFromEquipment(slot.Equipment);
                }

                isParrying = Get(SD.WeaponQuality.Parrying).GearBonus>0;

                // calculate base propertiers 
                foreach (var prop in GetAll())
                {
                    //if (prop.Value.SumBonus == 0) continue;

                    switch (prop.Key)
                    {
                        case SD.WeaponQuality.Parrying:
                            Get(SD.BattleProperty.DefenceParry).GearBonus += prop.Value.SumBonus;
                            break;
                        case SD.WeaponQuality.Fast:
                            Get(SD.BattleProperty.AttackDodge).GearBonus += prop.Value.SumBonus;
                            Get(SD.BattleProperty.AttackParry).GearBonus += prop.Value.SumBonus;
                            break;
                        case SD.WeaponQuality.Slow:
                            Get(SD.BattleProperty.AttackDodge).GearBonus -= prop.Value.SumBonus;
                            Get(SD.BattleProperty.AttackParry).GearBonus -= prop.Value.SumBonus;
                            break;
                        case SD.WeaponQuality.Heavy:
                            Get(SD.BattleProperty.AttackParry).GearBonus += 3 * prop.Value.SumBonus;
                            Get(SD.BattleProperty.AttackShield).GearBonus += prop.Value.SumBonus;
                            Get(SD.BattleProperty.AttackArmor).GearBonus += prop.Value.SumBonus;
                            Get(SD.BattleProperty.DefenceDodge).GearBonus -= prop.Value.SumBonus;
                            break;
                        case SD.WeaponQuality.Devastating:
                            Get(SD.BattleProperty.DamageBonus).GearBonus += prop.Value.SumBonus;
                            break;
                        case SD.WeaponQuality.Weak:
                            Get(SD.BattleProperty.DamageBonus).GearBonus -= prop.Value.SumBonus;
                            break;
                        case SD.WeaponQuality.Precise:
                            Get(SD.BattleProperty.AttackBase).GearBonus += prop.Value.SumBonus;
                            break;
                        case SD.WeaponQuality.Bulky:
                            Get(SD.BattleProperty.AttackBase).GearBonus -= prop.Value.SumBonus;
                            break;
                        case SD.WeaponQuality.ShieldDestructive:
                            Get(SD.BattleProperty.AttackShield).GearBonus += prop.Value.SumBonus;
                            break;
                        case SD.WeaponQuality.Armor:
                            Get(SD.BattleProperty.ArmorClass).GearBonus += prop.Value.SumBonus;
                            break;
                        case SD.WeaponQuality.ArmorDefenceBonus:
                            if (ArmorUsed is not null)
                                Get(SD.BattleProperty.DefenceArmor).GearBonus += prop.Value.SumBonus;
                            break;
                        case SD.WeaponQuality.ShieldDefenceBonus:
                            if (ShieldUsed is not null)
                                Get(SD.BattleProperty.DefenceShield).GearBonus += prop.Value.SumBonus;
                            break;
                        case SD.WeaponQuality.ArmorPenalty:
                            var dodge = Get(SD.BattleProperty.DefenceDodge);
                            dodge.GearBonus -= prop.Value.SumBonus;
                            //add bane to skills which are difficult to use with armor
                            foreach (var skill in SD.SpecialSkills.ArmorPenaltySkills)
                                AddGearBonusToSpecialSkill(skill, -prop.Value.SumBonus);
                            break;
                        case SD.BattleProperty.AttackBase:
                            if (WeaponUsed is not null)
                            {
                                Get(SD.BattleProperty.AttackBase).BaseBonus = _allParams.SpecialSkills.Get(WeaponUsed.RelatedSkill).SumBonus;
                            }
                            break;
                        case SD.BattleProperty.AttackDodge:
                            Get(SD.BattleProperty.AttackDodge).BaseBonus = Get(SD.BattleProperty.AttackBase).BaseBonus;
                            break;
                        case SD.BattleProperty.AttackArmor:
                            Get(SD.BattleProperty.AttackArmor).BaseBonus = Get(SD.BattleProperty.AttackBase).BaseBonus;
                            break;
                        case SD.BattleProperty.AttackShield:
                            Get(SD.BattleProperty.AttackShield).BaseBonus = Get(SD.BattleProperty.AttackBase).BaseBonus;
                            break;
                        case SD.BattleProperty.AttackParry:
                                Get(SD.BattleProperty.AttackParry).BaseBonus = Get(SD.BattleProperty.AttackBase).BaseBonus;
                            break;
                        case SD.BattleProperty.ArmorClass:
                            if (ArmorUsed is not null)
                                Get(SD.BattleProperty.ArmorClass).BaseBonus += prop.Value.SumBonus;
                            break;
                        case SD.BattleProperty.DefenceDodge:
                            Get(SD.BattleProperty.DefenceDodge).BaseBonus = _allParams.SpecialSkills.Get(SD.SpecialSkills.Acrobatics.Dodge).SumBonus;
                            break;
                        case SD.BattleProperty.DefenceArmor:
                            if (ArmorUsed is not null)
                                Get(SD.BattleProperty.DefenceArmor).BaseBonus = _allParams.SpecialSkills.Get(SD.SpecialSkills.Athletics.Armor).SumBonus;
                            break;
                        case SD.BattleProperty.DefenceShield:
                            if (ShieldUsed is not null)
                                Get(SD.BattleProperty.DefenceShield).BaseBonus = _allParams.SpecialSkills.Get(SD.SpecialSkills.Melee.Shields).SumBonus;
                            break;
                        case SD.BattleProperty.DefenceParry:
                            if (WeaponUsed is not null &&  isParrying)
                            {
                                Get(SD.BattleProperty.DefenceParry).BaseBonus = _allParams.SpecialSkills.Get(WeaponUsed.RelatedSkill).SumBonus;
                            }
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                ;
            }
        }

        public void AddWeaponQualityFromEquipment(EquipmentDTO? equip)
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
                        battleProp = Get(bonus.FeatureName);
                        if (battleProp is not null)
                        {
                            battleProp.GearBonus += bonus.BonusValue;
                        }
                    }
                }
            }
        }

        public IEnumerable<BattlePropertyDTO> GetWeaponQualityListFromItem(EquipmentDTO? equip)
        {
            ICollection<BattlePropertyDTO> weapons = new List<BattlePropertyDTO>();
            if (equip is null || equip.Traits is null)
            {
                return weapons;
            }
            BattlePropertyDTO? battleProp = null;
            foreach (var trait in equip.Traits)
            {
                foreach (var bonus in trait.Bonuses)
                {
                    if (bonus.FeatureType == SD.FeatureWeaponQuality && bonus.FeatureName.IsNullOrEmpty() == false && SD.WeaponQuality.All.Contains(bonus.FeatureName))
                    {
                        battleProp = new();
                        battleProp.Name = bonus.FeatureName;
                        battleProp.GearBonus = bonus.BonusValue;
                        weapons.Add(battleProp);
                    }
                }
            }

            return weapons;

        }

        private void AddGearBonusToSpecialSkill(string skillName, int value)
        {
            var skill = _allParams.SpecialSkills.Get(skillName);
            if (skill != null)
                skill.GearBonus += value;
        }
    }
}