using Abp.Collections.Extensions;
using DA_Common;
using DA_Models.CharacterModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DA_Common.SD;

namespace DA_Models.ComponentModels
{
    public class FightSequenceModel
    {
        public FightSequenceModel(DateModel date)
        {
            Date = Date;
        }
        public DateModel Date { get; set; } = new(1, 1);
        public string AttackerName { get; set; } = string.Empty;
        public string DefenderName { get; set; } = string.Empty;
        public BattlePropertyModel AttackerProps { get; set; } = null;
        public BattlePropertyModel DefenderProps { get; set; } = null;
        public ICollection<TraitCharacterDTO>? AttackerStates { get; set; } = new List<TraitCharacterDTO>();
        public ICollection<TraitCharacterDTO>? DefenderStates { get; set; } = new List<TraitCharacterDTO>();
        public HealthModel AttackerHealth { get; set; } = null;
        public HealthModel DefenderHealth { get; set; } = null;
        public int AttackerPainResistance { get; set; } = 0;
        public int DefenderPainResistance { get; set; } = 0;
        public int AttackerBalance { get; set; } = 0;
        public int DefenderBalance { get; set; } = 0;
        public int AttackerLifting { get; set; } = 0;
        public int DefenderLifting { get; set; } = 0;
        public string AttackAction { get; set; } = string.Empty;
        public string AttackLocation { get; set; } = string.Empty;
        public string DefenceType { get; set; } = string.Empty;
        public bool IsHit { get; set; } = false;
        public int DamageDelt { get; set; } = 0;
        public bool IsShieldDefenceAllowed { get; set; } = true;
        public bool IsParryDefenceAllowed { get; set; } = true;
        public string TestConditionIfHit { get; set; } = string.Empty;
        public Tuple<int,string> AttackerRoll { get; set; } = new Tuple<int,string>(0, string.Empty);
        public Tuple<int, string> DefenderRoll { get; set; } = new Tuple<int, string>(0, string.Empty);

        public bool UpdateAttackerNeeded { get; set; } = false;
        public bool UpdateDefenderNeeded { get; set; } = false;
       
        public string ResultStringMG { get; set; } = string.Empty;       

        public string AttackerNewStates { get; set; } = string.Empty;
        public string DefenderNewStates { get; set; } = string.Empty;
        public string DefenderNewWounds { get; set; } = string.Empty;

        public void AddAttacker(AllParamsModel allParams)
        {
            AttackerProps = allParams.BattleProperties;
            AttackerStates = allParams.Character?.TraitsCharacter?.Where(t => t.TraitType == SD.TraitType_Temporary).ToList();
            AttackerHealth = allParams.Health;
            AttackerName = allParams.Character.NPCName;
            AttackerPainResistance = allParams.SpecialSkills.Get(SD.SpecialSkills.Athletics.PainResistance).SumBonus;
            AttackerLifting = allParams.SpecialSkills.Get(SD.SpecialSkills.Athletics.Lifting).SumBonus;
            AttackerBalance = allParams.SpecialSkills.Get(SD.SpecialSkills.Acrobatics.Balance).SumBonus;
        }
        public void AddAttacker(MobDTO mob)
        {
            try
            {
                AttackerProps = mob.BattleProperties;
                var states = mob.States.Split(", ");
                AttackerStates = new List<TraitCharacterDTO>();
                foreach (var state in states)
                {
                    var trait = new TraitCharacterDTO(true);
                    var statesParams = mob.States.Split(":");
                    trait.Name = statesParams[0];
                    trait.TraitValue = Int32.Parse(statesParams[1]);
                    AttackerStates.Add(trait);
                }
                AttackerHealth = new MobHealthModel(mob);
                AttackerName = mob.Name;
                AttackerPainResistance = mob.PainResSkillValue;
                AttackerLifting = mob.AttackSkillValue;
                AttackerBalance = mob.DodgeSkillValue;
            }
            catch (Exception ex)
            {
                ;
            }
        }
        public void AddDefender(AllParamsModel allParams)
        {
            DefenderProps = allParams.BattleProperties;
            DefenderStates = allParams.Character?.TraitsCharacter?.Where(t => t.TraitType == SD.TraitType_Temporary).ToList();
            DefenderHealth = allParams.Health;
            DefenderName = allParams.Character.NPCName;
            DefenderPainResistance = allParams.SpecialSkills.Get(SD.SpecialSkills.Athletics.PainResistance).SumBonus;
            DefenderLifting = allParams.SpecialSkills.Get(SD.SpecialSkills.Athletics.Lifting).SumBonus;
            DefenderBalance = allParams.SpecialSkills.Get(SD.SpecialSkills.Acrobatics.Balance).SumBonus;
        }
        public void AddDefender(MobDTO mob)
        {
            try
            {
                DefenderProps = mob.BattleProperties;
                var states = mob.States.Split(", ");
                DefenderStates = new List<TraitCharacterDTO>();
                foreach (var state in states)
                {
                    var trait = new TraitCharacterDTO(true);
                    var statesParams = mob.States.Split(":");
                    trait.Name = statesParams[0];
                    trait.TraitValue = Int32.Parse(statesParams[1]);
                    DefenderStates.Add(trait);
                }
                DefenderHealth = new MobHealthModel(mob);
                DefenderName = mob.Name;
                DefenderPainResistance = mob.PainResSkillValue;
                DefenderLifting = mob.AttackSkillValue;
                DefenderBalance = mob.DodgeSkillValue;
            }
            catch (Exception ex)
            {
                ;
            }
        }
        
        public void MakeAttack()
        {
            int AttackValue = 0;
            int AllAttackValue = 0;
            int DefenceValue = 0;
            int AllDefenceValue = 0;
            int AdditionalDamage = 0;
            int HitValue = 0;

            string defenceString = string.Empty;
            string attackString = string.Empty;

            /// Get bonus from attack type
            switch (DefenceType)
            {
                default:
                case SD.DefenceType.Dodge:
                    AllAttackValue += AttackerProps.Get(SD.BattleProperty.AttackDodge).SumBonus;
                    AllDefenceValue += DefenderProps.Get(SD.BattleProperty.DefenceDodge).SumBonus;
                    defenceString = SD.DefenceType.Dodge.ToLower();
                    break;
                case SD.DefenceType.Parry:
                    AllAttackValue += AttackerProps.Get(SD.BattleProperty.AttackParry).SumBonus;
                    AllDefenceValue += DefenderProps.Get(SD.BattleProperty.DefenceParry).SumBonus;
                    defenceString = SD.DefenceType.Parry.ToLower();
                    break;
                case SD.DefenceType.Shield:
                    AllAttackValue += AttackerProps.Get(SD.BattleProperty.AttackShield).SumBonus;
                    AllDefenceValue += DefenderProps.Get(SD.BattleProperty.DefenceShield).SumBonus;
                    defenceString = "deflect with shield";
                    break;
                case SD.DefenceType.Armor:
                    AllAttackValue += AttackerProps.Get(SD.BattleProperty.AttackArmor).SumBonus;
                    AllDefenceValue += DefenderProps.Get(SD.BattleProperty.DefenceArmor).SumBonus;
                    defenceString = "deflect with armor";
                    break;
            }
            

            /// Get bonus from attack action
            switch (AttackAction)
            {
                default:
                case SD.AttackAction.Cautious:
                    AttackValue += -3;
                    TraitCharacterDTO cautious = new TraitCharacterDTO(true);
                    AttackerNewStates += SD.TempStates.Cautious + ":1" + ", ";
                    attackString = "cautiously";
                    break;
                case SD.AttackAction.Targeted:
                    switch (AttackLocation)
                    {
                        default:
                        case SD.WoundLocation.Head:
                            AttackValue += -5;
                            AdditionalDamage += 8;
                            TestConditionIfHit += SD.TempStates.Stumbled + ", ";
                            break;
                        case SD.WoundLocation.Neck:
                            AttackValue += -6;
                            AdditionalDamage += 9;                            
                            if(AttackerProps.Get(SD.WeaponQuality.Snatching).SumBonus > 0)
                                TestConditionIfHit += SD.TempStates.Snatched + ", ";
                            else
                                TestConditionIfHit += SD.TempStates.Bleeding + ", ";
                            break;
                        case SD.WoundLocation.Face:
                            AttackValue += -6;
                            AdditionalDamage += 10;
                            TestConditionIfHit += SD.TempStates.Blinded + ", ";
                            break;
                        case SD.WoundLocation.MainHand:
                        case SD.WoundLocation.MainArm:
                        case SD.WoundLocation.OffArm:
                        case SD.WoundLocation.OffHand:
                            AttackValue += -2;
                            if (AttackerProps.Get(SD.WeaponQuality.Snatching).SumBonus > 0)
                                TestConditionIfHit += SD.TempStates.Snatched + ", ";
                            break;
                        case SD.WoundLocation.Body:
                            break;
                        case SD.WoundLocation.Back:
                            break;                        
                        case SD.WoundLocation.LeftLeg:
                        case SD.WoundLocation.RightLeg:
                            AttackValue += -2;
                            if (AttackerProps.Get(SD.WeaponQuality.Snatching).SumBonus > 0)
                                TestConditionIfHit += SD.TempStates.Snatched + ", ";
                            else
                                TestConditionIfHit += SD.TempStates.Stumbled + ", ";
                            break;
                    }
                    attackString = $"aiming at the {AttackLocation.ToLower()}";
                    break;
                case SD.AttackAction.Charge:
                    AttackValue += 5;
                    AdditionalDamage += 3;
                    attackString = "charging";
                    break;
                case SD.AttackAction.Raging:
                    AttackValue += 7;
                    AdditionalDamage += 3;
                    AttackerNewStates += SD.TempStates.Unbalanced + ":1" + ", ";
                    attackString = "furiously!";
                    break;
                case SD.AttackAction.Strong:
                    AttackValue += 5;
                    attackString = "with all strength";
                    break;
            }
            AllAttackValue += AttackValue;
            AllDefenceValue += DefenceValue;
            attackString += SD.BonusText(AttackValue);
            defenceString += SD.BonusText(DefenceValue);

            // add weapon bonus if exists
            AttackValue = AttackerProps.Get(SD.WeaponQuality.Precise).SumBonus;
            if (AttackValue > 0)
            {
                AllAttackValue += AttackValue;
                attackString += ", with precise weapon" + SD.BonusText(AttackValue);
            }
            AttackValue = AttackerProps.Get(SD.WeaponQuality.Bulky).SumBonus;
            if (AttackValue > 0)
            {
                AllAttackValue -= AttackValue;
                attackString += ", with crude weapon" + SD.BonusText(-AttackValue);
            }


            ResultStringMG += $"({AttackerName} attacks {attackString}, {DefenderName} tries to {defenceString}.";

            /// Get bonus from states
            // attacker
            if (AttackerStates is not null && AttackerStates.Any())
            {
                ResultStringMG += $" \r{AttackerName} is :";

                foreach (var state in AttackerStates)
                {
                    AttackValue = 0;
                    attackString = "";
                    switch (state.Name)
                    {
                        case SD.TempStates.Stunned:
                        case SD.TempStates.Unaware:
                        case SD.TempStates.FullDefence:
                            //cannot attack! error!
                            break;
                        case SD.TempStates.Surrounded:
                        case SD.TempStates.Disarmed:
                        case SD.TempStates.Bleeding:
                        case SD.TempStates.Unbalanced:
                        case SD.TempStates.Cautious:
                            //does nothing
                            break;
                        case SD.TempStates.Stumbled:
                            AttackValue += -state.Level;
                            break;
                        case SD.TempStates.Snatched:
                            AttackValue += -state.Level;
                            break;
                        case SD.TempStates.Blinded:
                            AttackValue += -state.Level;
                            break;
                        case SD.TempStates.Invisible:
                            AttackValue += state.Level;
                            break;
                        case SD.TempStates.Flanking:
                            AttackValue += state.Level;
                            IsShieldDefenceAllowed = false;
                            break;
                    }
                    AllAttackValue += AttackValue;
                    attackString = state.Name + SD.BonusText(AttackValue);
                    ResultStringMG += $"{attackString}, ";
                }
            }

            if (DefenderStates is not null && DefenderStates.Any())
            {
                ResultStringMG += $"\r{DefenderName} is: ";

                // defender
                foreach (var state in DefenderStates)
                {

                    DefenceValue = 0;
                    defenceString = "";
                    switch (state.Name)
                    {
                        case SD.TempStates.Stunned:
                            DefenceValue += -state.Level;
                            break;
                        case SD.TempStates.Unaware:
                            DefenceValue += -state.Level;
                            break;
                        case SD.TempStates.FullDefence:
                            DefenceValue += state.Level;
                            break;
                        case SD.TempStates.Surrounded:
                            DefenceValue += 2 * state.TraitValue;
                            break;
                        case SD.TempStates.Disarmed:
                            IsParryDefenceAllowed = false;
                            break;
                        case SD.TempStates.Bleeding:
                            break;
                        case SD.TempStates.Unbalanced:
                            DefenceValue += -state.Level;
                            break;
                        case SD.TempStates.Cautious:
                            DefenceValue += state.Level;
                            break;
                        case SD.TempStates.Stumbled:
                            DefenceValue += -state.Level;
                            break;
                        case SD.TempStates.Snatched:
                            DefenceValue += -state.Level;
                            break;
                        case SD.TempStates.Blinded:
                            DefenceValue += -state.Level;
                            break;
                        case SD.TempStates.Invisible:
                            DefenceValue += state.Level;
                            break;
                        case SD.TempStates.Flanking:
                            break;
                    }
                    AllDefenceValue += DefenceValue;
                    defenceString = state.Name + SD.BonusText(DefenceValue);
                    ResultStringMG += $"{defenceString}, ";
                }
            }

            /// Add dice rolls 
            AttackerRoll = SD.RollDice();
            DefenderRoll = SD.RollDice();
            ResultStringMG += $" \r{AttackerName} roll: {AttackerRoll.Item2}, {DefenderName} roll: {DefenderRoll.Item2}";
            AllAttackValue += AttackerRoll.Item1;
            AllDefenceValue += DefenderRoll.Item1;
            HitValue = AllAttackValue - DefenceValue;
            if (HitValue >= 0) IsHit = true;
            attackString = IsHit ? "Hit!" : "Miss!";
            ResultStringMG += $" \r{AttackerName} sum: {AllAttackValue}, {DefenderName} sum: {AllDefenceValue}. {attackString}";

            /// Calculate damage
            if (IsHit)
            {
                // damage from attack
                ResultStringMG += $" \rDamage dealt from attack: {HitValue}";
                int dmgDeflected = DefenderProps.Get(SD.BattleProperty.ArmorClass).SumBonus - AttackerProps.Get(SD.WeaponQuality.ArmorPiercing).SumBonus;
                if (dmgDeflected > 0)
                {
                    attackString = $", deflected by armor: {dmgDeflected} ";
                }
                else
                {
                    attackString = "";
                    dmgDeflected = 0;
                }
                ResultStringMG += $"{attackString}";
                // damage from weapon qualities
                int dmgfromWeaponQuality = AttackerProps.Get(SD.WeaponQuality.Devastating).SumBonus;
                if (dmgfromWeaponQuality > 0)
                {
                    attackString = $", from devastating weapon: {dmgfromWeaponQuality}";
                }
                else
                {
                    dmgfromWeaponQuality = AttackerProps.Get(SD.WeaponQuality.Weak).SumBonus;
                    if (dmgfromWeaponQuality > 0)
                    {
                        dmgfromWeaponQuality = -dmgfromWeaponQuality;
                        attackString = $", from weak weapon: {dmgfromWeaponQuality}";
                    }
                }
                ResultStringMG += $"{attackString}";
                // damage from actions (rage, charge)
                if (AdditionalDamage != 0)
                {
                    attackString = $", from action: {AdditionalDamage}";
                }
                ResultStringMG += $"{attackString}";
                DamageDelt = (AllAttackValue - AllDefenceValue) - dmgDeflected + AdditionalDamage + dmgfromWeaponQuality;
                string woundSeverity = SD.WoundSeverityFromDmg(DamageDelt);
                ResultStringMG += $"\rSummary damage: {DamageDelt} - {woundSeverity} wound to {AttackLocation}";
                /// Prain resistance roll
                int DC = SD.DCFromWoundSeverity(woundSeverity);
                var painResRoll =  SD.MakeRollTest(DC, DefenderPainResistance);
                ResultStringMG += $"\rPain resistance test: {painResRoll.Item2}";

                /// Add wound

                WoundDTO newWound = new();
                newWound.DateStart = Date;
                newWound.IsIgnored = painResRoll.Item1;
                newWound.Description = ResultStringMG;
                if (AttackAction.IsNullOrEmpty())
                {
                    Random rnd = new Random();
                    int location = rnd.Next(0, SD.WoundLocation.All.Length-1);
                    AttackAction = SD.WoundLocation.All[location];
                }
                newWound.Location = AttackAction;
                newWound.Value = SD.GetValueFromSeverity(woundSeverity);

                DefenderHealth.AddWound(newWound);

                /// test possible states

                foreach (var stateTest in TestConditionIfHit.Split(", "))
                {
                    Tuple<bool, string> result = new Tuple<bool, string>(false,string.Empty);
                    int duration = 0;
                    switch (stateTest)
                    {
                        case SD.TempStates.Stumbled:
                            DC = AttackerProps.Get(SD.WeaponQuality.Stumbling).SumBonus + HitValue;
                            result = SD.MakeRollTest(DC, Math.Max(DefenderBalance,DefenderLifting));
                            duration = 99;                           
                            break;
                        case SD.TempStates.Stunned:
                            DC = AttackerProps.Get(SD.WeaponQuality.Stunning).SumBonus + HitValue;
                            result = SD.MakeRollTest(DC, DefenderPainResistance);
                            duration =Math.Max(1, DC /10);
                            break;
                        case SD.TempStates.Snatched:
                            DC = AttackerProps.Get(SD.WeaponQuality.Snatching).SumBonus + HitValue;
                            result = SD.MakeRollTest(DC, Math.Max(DefenderBalance, DefenderLifting));
                            duration = 99;
                            break;
                        case SD.TempStates.Bleeding:
                            DC = HitValue;
                            result = SD.MakeRollTest(DC, DefenderPainResistance);
                            duration = 99;
                            break;
                        case SD.TempStates.Blinded:
                            DC = HitValue;
                            result = SD.MakeRollTest(DC, DefenderPainResistance);
                            duration = Math.Max(1, DC / 10);
                            break;
                        default: continue;
                    }
                    ResultStringMG += $"\rTest against {stateTest}: {result.Item2}";
                    if (result.Item1)
                    {
                        DefenderNewStates += $"{stateTest}:{duration}";
                    }
                }
            }
        }
    }
}
