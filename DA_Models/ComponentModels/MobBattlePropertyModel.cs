﻿using DA_Common;
using DA_DataAccess.CharacterClasses;
using DA_Models.CharacterModels;
using DA_Models.ComponentInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DA_Models.ComponentModels
{
    public class MobBattlePropertyModel : BattlePropertyModel
    {
        public MobBattlePropertyModel(MobDTO? mob) : base(null)
        {
           Mob=mob;
        }
        private MobDTO? Mob;

        public override void CalculateBattleStats()
        {
            try
            {
                if (Mob is null)
                    return;
                
                //clear BattleProperies values;
                foreach (var prop in GetAllArray())
                {
                    prop.BaseBonus = 0;
                    prop.GearBonus = 0;
                }
                if(Mob.MainWeapon is not null)
                {
                    MainWeaponUsed = Mob.MainWeapon;
                    AddWeaponQualityFromEquipment(MainWeaponUsed);
                }
                if (Mob.OffWeapon is not null) { 
                    OffWeaponUsed = Mob.OffWeapon;
                    AddWeaponQualityFromEquipment(OffWeaponUsed);
                }
                if (Mob.ShieldWeapon is not null) { 
                    ShieldUsed = Mob.ShieldWeapon;
                    AddWeaponQualityFromEquipment(ShieldUsed);
                }
                if (Mob.Armor is not null){
                    ArmorUsed = Mob.Armor;
                    AddWeaponQualityFromEquipment(ArmorUsed);
                }

                bool isParrying = false;
                var parrySkill = Get(SD.WeaponQuality.Parrying);
                if (parrySkill is not null)
                    isParrying = parrySkill.GearBonus > 0;

                // calculate base propertiers 
                foreach (var prop in GetAll())
                {
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
                            Get(SD.BattleProperty.DefenceDodge).GearBonus -= prop.Value.SumBonus;
                            break;
                        case SD.BattleProperty.AttackBase:
                            Get(SD.BattleProperty.AttackBase).BaseBonus = Mob.AttackSkillValue;                            
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
                            Get(SD.BattleProperty.DefenceDodge).BaseBonus = Mob.DodgeSkillValue;
                            break;
                        case SD.BattleProperty.DefenceArmor:
                            if (ArmorUsed is not null)
                                Get(SD.BattleProperty.DefenceArmor).BaseBonus = Mob.ArmorSkillValue;
                            break;
                        case SD.BattleProperty.DefenceShield:
                            if (ShieldUsed is not null)
                                Get(SD.BattleProperty.DefenceShield).BaseBonus = Mob.ShieldSkillValue;
                            break;
                        case SD.BattleProperty.DefenceParry:
                            if (isParrying)                                
                                Get(SD.BattleProperty.DefenceParry).BaseBonus = Mob.ParrySkillValue;
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                ;
            }
        }
    }
}