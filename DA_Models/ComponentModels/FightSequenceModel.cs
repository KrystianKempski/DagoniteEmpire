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
        // input variables
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

        // select variables
        public string AttackAction { get; set; } = string.Empty;
        public string AttackLocation { get; set; } = string.Empty;
        public string DefenceType { get; set; } = string.Empty;


        // private variables
        private bool IsHit { get; set; } = false;
        private int AttackValue { get; set; } = 0;
        private int DefenceValue { get; set; } = 0;
        private int HitValue { get; set; } = 0;
        private int AdditionalDamage { get; set; } = 0;
        private int DamageDelt { get; set; } = 0;
        private string WoundSeverity { get; set; } = string.Empty;
        private bool IsShieldDefenceAllowed { get; set; } = true;
        private bool IsParryDefenceAllowed { get; set; } = true;
        private string TestConditionIfHit { get; set; } = string.Empty;
        private Tuple<int, string> AttackerRoll { get; set; } = new Tuple<int, string>(0, string.Empty);
        private Tuple<int, string> DefenderRoll { get; set; } = new Tuple<int, string>(0, string.Empty);
        private bool UpdateAttackerNeeded { get; set; } = false;
        private bool UpdateDefenderNeeded { get; set; } = false;

        // return variables
        public RichText ResultStringMG { get; set; } = new();

        public string AttackerOldStates { get; set; } = string.Empty;
        public string DefenderOldStates { get; set; } = string.Empty;
        public string AttackerNewStates { get; set; } = string.Empty;
        public string DefenderNewStates { get; set; } = string.Empty;

        // functions

        public void AddAttacker(AllParamsModel allParams)
        {
            AttackerProps = allParams.BattleProperties;
            AttackerStates = allParams.Character?.TraitsCharacter?.Where(t => t.TraitType == SD.TraitType_Temporary).ToList();
            AttackerHealth = allParams.Health;
            AttackerName =  allParams.Character.NPCName;
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
                    var statesParams = state.Split(":");
                    trait.Name = statesParams[0];
                    trait.Level = SD.GetTempStatesLevel(trait.Name);
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

        public void CalculateAndWriteAttack()
        {
            /// Get bonus from states
            WriteBonusesFromStates();
            /// Get bonus from attack type
            WriteWhoAttacksWhoAndHow();            
            /// Add dice rolls  and sum up attack
            WriteDiceRollsAndAttackSummary();
            /// Calculate damage
            if (IsHit)
            {
                /// Damage calculation
                WriteDamageSummary();
                /// Calculate wound
                CalculateAndAddWound();
                /// Test possible states
                WriteAndCalculatePossibleStates();
            }
        }

        public void WriteWhoAttacksWhoAndHow()
        {
            int AttackCurrValue = 0;
            int DefenceCurrValue = 0;
            string defenceString = string.Empty;
            string attackString = string.Empty;
            switch (DefenceType)
            {
                default:
                case SD.DefenceType.Dodge:
                    AttackCurrValue += AttackerProps.Get(SD.BattleProperty.AttackDodge).SumBonus;
                    DefenceCurrValue += DefenderProps.Get(SD.BattleProperty.DefenceDodge).SumBonus;
                    defenceString = SD.DefenceType.Dodge.ToLower();
                    break;
                case SD.DefenceType.Parry:
                    AttackCurrValue = AttackerProps.Get(SD.BattleProperty.AttackParry).SumBonus;
                    DefenceCurrValue += DefenderProps.Get(SD.BattleProperty.DefenceParry).SumBonus;
                    defenceString = SD.DefenceType.Parry.ToLower();
                    break;
                case SD.DefenceType.Shield:
                    AttackCurrValue += AttackerProps.Get(SD.BattleProperty.AttackShield).SumBonus;
                    DefenceCurrValue += DefenderProps.Get(SD.BattleProperty.DefenceShield).SumBonus;
                    defenceString = "deflect with shield";
                    break;
                case SD.DefenceType.Armor:
                    AttackValue += AttackerProps.Get(SD.BattleProperty.AttackArmor).SumBonus;
                    DefenceCurrValue += DefenderProps.Get(SD.BattleProperty.DefenceArmor).SumBonus;
                    defenceString = "deflect with armor";
                    break;
            }

            attackString += $"{SD.BonusText(AttackCurrValue)} ";
            AttackValue += AttackCurrValue;
            AttackCurrValue = 0;

            /// Get bonus from attack action
            switch (AttackAction)
            {
                default:
                case SD.AttackAction.Cautious:
                    AttackCurrValue += -3;
                    TraitCharacterDTO cautious = new TraitCharacterDTO(true);
                    AttackerNewStates += SD.TempStates.Cautious + ":1" + ", ";
                    attackString += "cautiously";
                    break;
                case SD.AttackAction.Targeted:
                    switch (AttackLocation)
                    {
                        default:
                        case SD.WoundLocation.Head:
                            AttackCurrValue += -5;
                            AdditionalDamage += 8;
                            TestConditionIfHit += SD.TempStates.Stumbled + ", ";
                            break;
                        case SD.WoundLocation.Neck:
                            AttackCurrValue += -6;
                            AdditionalDamage += 9;
                            if (AttackerProps.Get(SD.WeaponQuality.Snatching).SumBonus > 0)
                                TestConditionIfHit += SD.TempStates.Snatched + ", ";
                            else
                                TestConditionIfHit += SD.TempStates.Bleeding + ", ";
                            break;
                        case SD.WoundLocation.Face:
                            AttackCurrValue += -6;
                            AdditionalDamage += 10;
                            TestConditionIfHit += SD.TempStates.Blinded + ", ";
                            break;
                        case SD.WoundLocation.MainHand:
                        case SD.WoundLocation.MainArm:
                        case SD.WoundLocation.OffArm:
                        case SD.WoundLocation.OffHand:
                            AttackCurrValue += -2;
                            if (AttackerProps.Get(SD.WeaponQuality.Snatching).SumBonus > 0)
                                TestConditionIfHit += SD.TempStates.Snatched + ", ";
                            break;
                        case SD.WoundLocation.Body:
                            break;
                        case SD.WoundLocation.Back:
                            break;
                        case SD.WoundLocation.LeftLeg:
                        case SD.WoundLocation.RightLeg:
                            AttackCurrValue += -2;
                            if (AttackerProps.Get(SD.WeaponQuality.Snatching).SumBonus > 0)
                                TestConditionIfHit += SD.TempStates.Snatched + ", ";
                            else
                                TestConditionIfHit += SD.TempStates.Stumbled + ", ";
                            break;
                    }
                    attackString += $"aiming at the {AttackLocation.ToLower()}";
                    break;
                case SD.AttackAction.Charge:
                    AttackCurrValue += 5;
                    AdditionalDamage += 3;
                    attackString += "charging";
                    break;
                case SD.AttackAction.Raging:
                    AttackCurrValue += 7;
                    AdditionalDamage += 3;
                    AttackerNewStates += SD.TempStates.Unbalanced + ":1" + ", ";
                    attackString += "furiously!";
                    break;
                case SD.AttackAction.Strong:
                    AttackCurrValue += 5;
                    attackString += "with all strength";
                    break;
            }
            AttackValue += AttackCurrValue;
            DefenceValue += DefenceCurrValue;
            attackString += SD.BonusText(AttackCurrValue);
            defenceString += SD.BonusText(DefenceCurrValue);

            // add weapon bonus if exists
            AttackCurrValue = AttackerProps.Get(SD.WeaponQuality.Precise).SumBonus;
            if (AttackCurrValue > 0)
            {
                AttackValue += AttackCurrValue;
                attackString += ", with precise weapon" + SD.BonusText(AttackCurrValue);
            }
            AttackCurrValue = AttackerProps.Get(SD.WeaponQuality.Bulky).SumBonus;
            if (AttackCurrValue > 0)
            {
                AttackValue -= AttackCurrValue;
                attackString += ", with crude weapon" + SD.BonusText(-AttackCurrValue);
            }


            ResultStringMG += $"{AttackerName} {AttackerOldStates} attacks {attackString}, {DefenderName} {DefenderOldStates} tries to {defenceString}.";
        }

        public void WriteBonusesFromStates()
        {
            int AttackCurrValue = 0;
            int DefenceCurrValue = 0;
            string defenceString = string.Empty;
            string attackString = string.Empty;
            // attacker
            if (AttackerStates is not null && AttackerStates.Any())
            {
                AttackerOldStates = "(";
                foreach (var state in AttackerStates)
                {
                    AttackCurrValue = 0;
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
                            AttackCurrValue += -state.Level;
                            break;
                        case SD.TempStates.Snatched:
                            AttackCurrValue += -state.Level;
                            break;
                        case SD.TempStates.Blinded:
                            AttackCurrValue += -state.Level;
                            break;
                        case SD.TempStates.Invisible:
                            AttackCurrValue += state.Level;
                            break;
                        case SD.TempStates.Flanking:
                            AttackCurrValue += state.Level;
                            IsShieldDefenceAllowed = false;
                            break;
                    }
                    AttackValue += AttackCurrValue;
                    attackString = state.Name + SD.BonusText(AttackCurrValue);

                    AttackValue += AttackCurrValue;
                    attackString = state.Name + SD.BonusText(AttackCurrValue);
                    AttackerOldStates += $"{attackString}, ";
                }
                AttackerOldStates = AttackerOldStates.Remove(AttackerOldStates.Length - 2);  // remove last chars
                AttackerOldStates +=")";
            }

            if (DefenderStates is not null && DefenderStates.Any())
            {
                DefenderOldStates = "(";
                // defender
                foreach (var state in DefenderStates)
                {

                    DefenceCurrValue = 0;
                    defenceString = "";
                    switch (state.Name)
                    {
                        case SD.TempStates.Stunned:
                            DefenceCurrValue += -state.Level;
                            break;
                        case SD.TempStates.Unaware:
                            DefenceCurrValue += -state.Level;
                            break;
                        case SD.TempStates.FullDefence:
                            DefenceCurrValue += state.Level;
                            break;
                        case SD.TempStates.Surrounded:
                            DefenceCurrValue += 2 * state.TraitValue;
                            break;
                        case SD.TempStates.Disarmed:
                            IsParryDefenceAllowed = false;
                            break;
                        case SD.TempStates.Bleeding:
                            break;
                        case SD.TempStates.Unbalanced:
                            DefenceCurrValue += -state.Level;
                            break;
                        case SD.TempStates.Cautious:
                            DefenceCurrValue += state.Level;
                            break;
                        case SD.TempStates.Stumbled:
                            DefenceCurrValue += -state.Level;
                            break;
                        case SD.TempStates.Snatched:
                            DefenceCurrValue += -state.Level;
                            break;
                        case SD.TempStates.Blinded:
                            DefenceCurrValue += -state.Level;
                            break;
                        case SD.TempStates.Invisible:
                            DefenceCurrValue += state.Level;
                            break;
                        case SD.TempStates.Flanking:
                            break;
                    }
                    DefenceValue += DefenceCurrValue;
                    defenceString = state.Name + SD.BonusText(DefenceCurrValue);
                    DefenderOldStates += $"{defenceString}, ";
                }
                DefenderOldStates = DefenderOldStates.Remove(DefenderOldStates.Length - 2);  // remove last chars
                DefenderOldStates += ")";
            }
        }
        public void WriteDiceRollsAndAttackSummary()
        {
            string attackString = string.Empty;
            AttackerRoll = SD.RollDice();
            DefenderRoll = SD.RollDice();
            ResultStringMG.NewLine();
            ResultStringMG += $"{AttackerName} roll: {AttackerRoll.Item2}, {DefenderName} roll: {DefenderRoll.Item2}";
            AttackValue += AttackerRoll.Item1;
            DefenceValue += DefenderRoll.Item1;
            HitValue = AttackValue - DefenceValue;
            if (HitValue >= 0) IsHit = true;
            attackString = IsHit ? "Hit!" : "Miss!";
            ResultStringMG.NewLine();
            ResultStringMG += $" {AttackerName} summary: {RichText.BoldText(AttackValue)}, {DefenderName} summary: {RichText.BoldText(DefenceValue)}. {attackString}";
            if(IsHit == false)
            {
                ResultStringMG.EndText();
            }
        }
        public void WriteDamageSummary()
        {
            if (IsHit == false) return; 

            string attackString = string.Empty;
            // damage from attack
            ResultStringMG.NewLine();
            ResultStringMG += $"Damage dealt from attack: {HitValue}";
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
            DamageDelt = (AttackValue - DefenceValue) - dmgDeflected + AdditionalDamage + dmgfromWeaponQuality;
            WoundSeverity = SD.WoundSeverityFromDmg(DamageDelt);
            ResultStringMG.NewLine();
            ResultStringMG += $"Summary damage: {DamageDelt} - {RichText.BoldText(WoundSeverity)} wound";
            if(AttackLocation.IsNullOrEmpty() == false ){
                ResultStringMG += $" to{AttackLocation}";
            }
        }
        public void CalculateAndAddWound()
        {
            // Prain resistance roll
            int DC = SD.DCFromWoundSeverity(WoundSeverity);
            var painResRoll = SD.MakeRollTest(DC, DefenderPainResistance);
            ResultStringMG.NewLine();
            ResultStringMG += $"Pain resistance test: {painResRoll.Item2}";

            //create wound
            WoundDTO newWound = new();
            newWound.DateStart = Date;
            newWound.IsIgnored = painResRoll.Item1;
            newWound.Description = ResultStringMG.ToString();
            if (AttackAction.IsNullOrEmpty())
            {
                Random rnd = new Random();
                int location = rnd.Next(0, SD.WoundLocation.All.Length - 1);
                AttackAction = SD.WoundLocation.All[location];
            }
            newWound.Location = AttackAction;
            newWound.Value = SD.GetValueFromSeverity(WoundSeverity);

            DefenderHealth.AddWound(newWound);
        }
        public void WriteAndCalculatePossibleStates()
        {
            int DC = 0;
            foreach (var stateTest in TestConditionIfHit.Split(", "))
            {
                Tuple<bool, string> result = new Tuple<bool, string>(false, string.Empty);
                int duration = 0;
                switch (stateTest)
                {
                    case SD.TempStates.Stumbled:
                        DC = AttackerProps.Get(SD.WeaponQuality.Stumbling).SumBonus + HitValue;
                        result = SD.MakeRollTest(DC, Math.Max(DefenderBalance, DefenderLifting));
                        duration = 99;
                        break;
                    case SD.TempStates.Stunned:
                        DC = AttackerProps.Get(SD.WeaponQuality.Stunning).SumBonus + HitValue;
                        result = SD.MakeRollTest(DC, DefenderPainResistance);
                        duration = Math.Max(1, DC / 10);
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
                ResultStringMG.NewLine();
                ResultStringMG += $"Test against {stateTest}: {result.Item2}";
                if (result.Item1)
                {
                    DefenderNewStates += $"{stateTest}:{duration}";
                }
            }
            ResultStringMG.EndText();
        }
    }
}
