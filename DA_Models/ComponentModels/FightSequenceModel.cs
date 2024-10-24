using Abp.Collections.Extensions;
using DA_Common;
using DA_DataAccess.CharacterClasses;
using DA_Models.CharacterModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DA_Common.SD;
using static MudBlazor.Colors;

namespace DA_Models.ComponentModels
{
    public class FighterModel {
        public string Name { get; set; } = string.Empty;
        public BattlePropertyModel Props { get; set; } = null;
        public ICollection<TraitDTO>? States { get; set; } = new List<TraitDTO>();
        public HealthModel Health { get; set; } = null;
        public int PainResistance { get; set; } = 0;
        public int Balance { get; set; } = 0;
        public int Lifting { get; set; } = 0;

        public Tuple<int, string> Roll { get; set; } = new Tuple<int, string>(0, string.Empty);

        public string OldStates { get; set; } = string.Empty;
        public string NewStates { get; set; } = string.Empty;
        public int ActionLeft { get; set; } = 2;
    }

    public class FightSequenceModel
    {
        public FightSequenceModel(DateModel date)
        {
            Date = Date;
        }
        public DateModel Date { get; set; } = new(1, 1);
        // input variables

        public FighterModel Attacker { get; set; } = new();
        public FighterModel Defender { get; set; } = new();

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
        private string TestConditionIfHit { get; set; } = string.Empty;

        // public variables

        public bool IsShieldDefenceAllowed { get; set; } = true;
        public bool IsParryDefenceAllowed { get; set; } = true;

        // return variables

        public string WoundSeverity { get; set; } = string.Empty;
        public RichText ResultStringMG { get; set; } = new();
        public List<WoundDTO> NewWounds { get; set; } = new List<WoundDTO>();   

        // functions

        public static FighterModel? AddFighter( AllParamsModel allParams)
        {
            try
            {
                FighterModel fighter = new();
                fighter.Props = allParams.BattleProperties;
                fighter.States = allParams.TraitsTemporary;
                fighter.Health = allParams.Health;
                fighter.Name =  allParams.Character.NPCName;
                fighter.PainResistance = allParams.SpecialSkills.Get(SD.SpecialSkills.Athletics.PainResistance).SumBonus;
                fighter.Lifting = allParams.SpecialSkills.Get(SD.SpecialSkills.Athletics.Lifting).SumBonus;
                fighter.Balance = allParams.SpecialSkills.Get(SD.SpecialSkills.Acrobatics.Balance).SumBonus;
                return fighter;
            }
            catch (Exception ex)
            {
                ;
            }
            return null;
        }
        public static FighterModel? AddFighter(MobDTO mob)
        {
            try
            {
                FighterModel fighter = new();
                fighter.Props = mob.BattleProperties;
                var states = mob.States.Split(", ");
                fighter.States = new List<TraitDTO>();
                foreach (var state in states)
                {
                    if (state.IsNullOrEmpty()) continue;
                    var statesParams = state.Split(":");
                    var trait = StateSeeder.GetStateDTO(statesParams[0], false, Int32.Parse(statesParams[1]));
                    fighter.States.Add(trait);
                }
                fighter.Health = new MobHealthModel(mob);
                fighter.Name = mob.Name;
                fighter.PainResistance = mob.PainResSkillValue;
                fighter.Lifting = mob.AttackSkillValue - mob.CurrentWounds/2;
                fighter.Balance = mob.DodgeSkillValue - mob.CurrentWounds / 2;
                return fighter;
            }
            catch (Exception ex)
            {
                ;
            }
            return null;
        }

       
        public void CalculateAndWriteAttack()
        {
            ClearRoll();           
            /// Get bonus from states
            WriteBonusesFromStates();
            /// Get bonus from attack type
            WriteWhoAttacksWhoAndHow();
            if (AttackLocation.IsNullOrEmpty() == false)
                WriteLocationOfAttack();
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

        public void ClearRoll()
        {
            ResultStringMG = new();
            AttackValue = 0;
            DefenceValue = 0;
            AdditionalDamage = 0;
            DamageDelt = 0;
            TestConditionIfHit = string.Empty;
            HitValue = 0;
            IsHit = false;
            Attacker.NewStates  = string.Empty;
            Defender.NewStates  = string.Empty;
            WoundSeverity = string.Empty;
            Attacker.Roll = new Tuple<int, string>(0, string.Empty);
            Defender.Roll  = new Tuple<int, string>(0, string.Empty);
            NewWounds = new List<WoundDTO>();
            if (Attacker.Props is null || Defender.Props is null)
            {
                ResultStringMG += $"Error! No Attacker or Defender properties are loaded";
                return;
            }
        }

        public void WriteLocationOfAttack()
        {
            int AttackCurrValue = 0;
            string attackString = string.Empty;
            switch (AttackLocation)
            {
                default:
                case Wounds.Location.Head:
                    AttackCurrValue += -5;
                    AdditionalDamage += 8;
                    TestConditionIfHit += States.Names.Stunned + ", ";
                    break;
                case Wounds.Location.Neck:
                    AttackCurrValue += -6;
                    AdditionalDamage += 9;
                    if (Attacker.Props.Get(SD.WeaponQuality.Snatching).SumBonus > 0)
                        TestConditionIfHit += States.Names.Snatched + ", ";
                    else
                        TestConditionIfHit += States.Names.Bleeding + ", ";
                    break;
                case Wounds.Location.Face:
                    AttackCurrValue += -6;
                    AdditionalDamage += 10;
                    TestConditionIfHit += States.Names.Blinded + ", ";
                    break;
                case Wounds.Location.MainHand:
                case Wounds.Location.MainArm:
                case Wounds.Location.OffArm:
                case Wounds.Location.OffHand:
                    AttackCurrValue += -2;
                    if (Attacker.Props.Get(SD.WeaponQuality.Snatching).SumBonus > 0)
                        TestConditionIfHit += States.Names.Snatched + ", ";
                    break;
                case Wounds.Location.Body:
                    break;
                case Wounds.Location.Back:
                    break;
                case Wounds.Location.LeftLeg:
                case Wounds.Location.RightLeg:
                    AttackCurrValue += -2;
                    if (Attacker.Props.Get(SD.WeaponQuality.Snatching).SumBonus > 0)
                        TestConditionIfHit += States.Names.Snatched + ", ";
                    else
                        TestConditionIfHit += States.Names.Stumbled + ", ";
                    break;                   
            }
            ResultStringMG.NewLine();
            ResultStringMG += $"Attack is aimed at the {RichText.BoldText(AttackLocation.ToLower())} {SD.BonusText(AttackCurrValue)}";
            AttackValue += AttackCurrValue;
        }

        public void WriteWhoAttacksWhoAndHow()
        {
            int AttackCurrValue = 0;
            int DefenceCurrValue = 0;
            string defenceString = string.Empty;
            string weaponString = string.Empty;
            string woundString = string.Empty;
            int weaponCurrValue = 0;
            string attackString = string.Empty;
            switch (DefenceType)
            {
                default:
                case SD.DefenceType.Dodge:
                    weaponCurrValue += Attacker.Props.Get(SD.BattleProperty.AttackDodge).SumBonus;
                    DefenceCurrValue += Defender.Props.Get(SD.BattleProperty.DefenceDodge).SumBonus;                   
                    defenceString = SD.DefenceType.Dodge.ToLower();
                    break;
                case SD.DefenceType.Parry:
                    weaponCurrValue = Attacker.Props.Get(SD.BattleProperty.AttackParry).SumBonus;
                    DefenceCurrValue += Defender.Props.Get(SD.BattleProperty.DefenceParry).SumBonus;
                    defenceString = SD.DefenceType.Parry.ToLower();
                    break;
                case SD.DefenceType.Shield:
                    weaponCurrValue += Attacker.Props.Get(SD.BattleProperty.AttackShield).SumBonus;
                    DefenceCurrValue += Defender.Props.Get(SD.BattleProperty.DefenceShield).SumBonus;
                    defenceString = "deflect with shield";
                    break;
                case SD.DefenceType.Armor:
                    weaponCurrValue += Attacker.Props.Get(SD.BattleProperty.AttackArmor).SumBonus;
                    DefenceCurrValue += Defender.Props.Get(SD.BattleProperty.DefenceArmor).SumBonus;
                    defenceString = "deflect with armor";
                    break;
            }

            weaponString += $"using {Attacker.Props.MainWeaponUsed.Name} {SD.BonusText(weaponCurrValue)} ";
            AttackValue += weaponCurrValue;
            AttackCurrValue = 0;

            /// Get bonus from attack action
            switch (AttackAction)
            {
                default:
                case SD.AttackAction.Normal:
                    AttackCurrValue = 0;
                    Attacker.ActionLeft -= 1;
                    break;
                case SD.AttackAction.Cautious:
                    AttackCurrValue = -3;
                    TraitCharacterDTO cautious = new TraitCharacterDTO(true);
                    if(Attacker.NewStates.Contains(States.Names.Cautious) == false ||
                       Attacker.OldStates.Contains(States.Names.Cautious) == false)  // dont add it twice
                        Attacker.NewStates += States.Names.Cautious + ":1, ";  // add new state
                    attackString += "cautiously";
                    Attacker.ActionLeft -= 1;
                    break;                
                case SD.AttackAction.Charge:
                    AttackCurrValue = 5;
                    AdditionalDamage += 3;
                    attackString += "charging";
                    Attacker.ActionLeft -= 2;
                    break;
                case SD.AttackAction.Raging:
                    AttackCurrValue = 7;
                    AdditionalDamage += 3;
                    if (Attacker.NewStates.Contains(States.Names.Unbalanced) == false ||
                        Attacker.OldStates.Contains(States.Names.Unbalanced) == false)
                        Attacker.NewStates += States.Names.Unbalanced + ":1, ";
                    attackString += "furiously!";
                    Attacker.ActionLeft -= 2;
                    break;
                case SD.AttackAction.Strong:
                    AttackCurrValue = 5;
                    attackString += "with all strength";
                    Attacker.ActionLeft -= 2;
                    break;
            }
            if(Attacker.ActionLeft == 1)
            {
                Attacker.NewStates += $"{States.Names.HalfTurn}:1, ";
            }else if(Attacker.ActionLeft < 1)
            {
                Attacker.NewStates = Attacker.NewStates.Replace($"{States.Names.HalfTurn}:1, ", string.Empty);
                Attacker.NewStates += $"{States.Names.NoTurn}:1, ";
            }

            AttackValue += AttackCurrValue;
            DefenceValue += DefenceCurrValue;
            attackString += AttackCurrValue == 0 ? "" : SD.BonusText(AttackCurrValue);
            defenceString += SD.BonusText(DefenceCurrValue);

            // add weapon bonus if exists
            AttackCurrValue = Attacker.Props.Get(SD.WeaponQuality.Precise).SumBonus;
            if (AttackCurrValue > 0)
            {
                AttackValue += AttackCurrValue;
                attackString += ", with precise weapon" + SD.BonusText(AttackCurrValue);
            }
            AttackCurrValue = Attacker.Props.Get(SD.WeaponQuality.Bulky).SumBonus;
            if (AttackCurrValue > 0)
            {
                AttackValue -= AttackCurrValue;
                attackString += ", with crude weapon" + SD.BonusText(-AttackCurrValue);
            }
            string attackerStatesString = GetStatesString(Attacker.OldStates);
            string defenderStatesString = GetStatesString(Defender.OldStates);

            ResultStringMG += $"({RichText.BoldText(Attacker.Name)}{attackerStatesString} attacks {attackString} {weaponString}, {RichText.BoldText(Defender.Name)}{defenderStatesString} tries to {defenceString}.";
        }

        public void WriteBonusesFromStates()
        {
            int AttackCurrValue = 0;
            int DefenceCurrValue = 0;
            string defenceString = string.Empty;
            string attackString = string.Empty;
            // attacker
            if (Attacker.States is not null && Attacker.States.Any())
            {
                Attacker.OldStates = string.Empty;
                foreach (var state in Attacker.States)
                {
                    AttackCurrValue = 0;
                    switch (state.Name)
                    {
                        case States.Names.Stunned:
                        case States.Names.Unaware:
                        case States.Names.FullDefence:
                        case States.Names.Unconscious:
                        case States.Names.Dead:
                            //cannot attack! error!
                            break;
                        case States.Names.Surrounded:    
                        case States.Names.Bleeding:
                        case States.Names.Unbalanced:
                        case States.Names.Cautious:
                            //does nothing
                            break;
                        case States.Names.Stumbled:
                            AttackCurrValue += -(int)States.Level.Stumbled;
                            break;
                        case States.Names.Snatched:
                            AttackCurrValue += -(int)States.Level.Snatched;
                            break;
                        case States.Names.Blinded:
                            AttackCurrValue += -(int)States.Level.Blinded;
                            break;
                        case States.Names.Invisible:
                            AttackCurrValue += (int)States.Level.Invisible;
                            break;
                        case States.Names.Flanking:
                            AttackCurrValue += (int)States.Level.Flanking;
                            IsShieldDefenceAllowed = false;
                            break;
                        case States.Names.Disarmed:
                            Attacker.Props.MainWeaponUsed = Fists();
                            Attacker.Props.CalculateBattleStats();
                            break;
                    }
                    AttackValue += AttackCurrValue;
                    Attacker.OldStates += AttackCurrValue == 0 ? string.Empty : $"{state.Name} {SD.BonusText(AttackCurrValue)}, ";
                }
                if(Attacker.OldStates.Length < 5)                
                {
                    Attacker.OldStates = string.Empty;
                }
                Attacker.ActionLeft = 2;
                if (Attacker.OldStates.Contains(States.Names.HalfTurn))
                {
                    Attacker.ActionLeft = (int)TurnLeft.Half;
                }
                if (Attacker.OldStates.Contains(States.Names.NoTurn))
                {
                    Attacker.ActionLeft = (int)TurnLeft.No;
                }
            }

            if (Defender.States is not null && Defender.States.Any())
            {
                Defender.OldStates = string.Empty;
                // defender
                foreach (var state in Defender.States)
                {
                    DefenceCurrValue = 0;
                    switch (state.Name)
                    {
                        case States.Names.Dead:
                            DefenceCurrValue += -20;
                            break;
                        case States.Names.Unconscious:
                            DefenceCurrValue += -20;
                            break;
                        case States.Names.Stunned:
                            DefenceCurrValue += -(int)States.Level.Stunned;
                            break;
                        case States.Names.Unaware:
                            DefenceCurrValue += -(int)States.Level.Unaware;
                            break;
                        case States.Names.FullDefence:
                            DefenceCurrValue += (int)States.Level.FullDefence;
                            break;
                        case States.Names.Surrounded:
                            DefenceCurrValue -= (int)States.Level.Surrounded * (state.TraitValue-1);
                            break;
                        case States.Names.Disarmed:
                            IsParryDefenceAllowed = false;
                            break;
                        case States.Names.Bleeding:
                            break;
                        case States.Names.Unbalanced:
                            DefenceCurrValue += -(int)States.Level.Unbalanced;
                            break;
                        case States.Names.Cautious:
                            DefenceCurrValue += (int)States.Level.Cautious;
                            break;
                        case States.Names.Stumbled:
                            DefenceCurrValue += -(int)States.Level.Stumbled;
                            break;
                        case States.Names.Snatched:
                            DefenceCurrValue += -(int)States.Level.Snatched;
                            break;
                        case States.Names.Blinded:
                            DefenceCurrValue += -(int)States.Level.Blinded;
                            break;
                        case States.Names.Invisible:
                            DefenceCurrValue += (int)States.Level.Invisible;
                            break;
                        case States.Names.Flanking:
                            break;
                    }
                    DefenceValue += DefenceCurrValue;
                    Defender.OldStates += DefenceCurrValue == 0 ? "" : $"{state.Name} {SD.BonusText(DefenceCurrValue)}, ";
                }
                if(Defender.OldStates.Length <5)
                {
                    Defender.OldStates = string.Empty;
                }
                Defender.ActionLeft = 0;
                if (Defender.OldStates.Contains(States.Names.HalfTurn))
                {
                    Defender.ActionLeft = (int)TurnLeft.Half;
                }
                if (Defender.OldStates.Contains(States.Names.NoTurn))
                {
                    Defender.ActionLeft = (int)TurnLeft.No;
                }
            }
        }
        public void WriteDiceRollsAndAttackSummary()
        {
            string attackString = string.Empty;
            Attacker.Roll = SD.RollDice();
            Defender.Roll = SD.RollDice();
            ResultStringMG.NewLine();
            ResultStringMG += $"{Attacker.Name} roll: {Attacker.Roll.Item2}, {Defender.Name} roll: {Defender.Roll.Item2}";
            AttackValue += Attacker.Roll.Item1;
            DefenceValue += Defender.Roll.Item1;
            HitValue = AttackValue - DefenceValue;
            if (HitValue >= 0) IsHit = true;
            attackString = IsHit ? "Hit!" : "Miss!";
            ResultStringMG.NewLine();
            ResultStringMG += $" {Attacker.Name} summary: {RichText.BoldText(AttackValue)}, {Defender.Name} summary: {RichText.BoldText(DefenceValue)}. {attackString}";
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
            int dmgDeflected = Defender.Props.Get(SD.BattleProperty.ArmorClass).SumBonus - Attacker.Props.Get(SD.WeaponQuality.ArmorPiercing).SumBonus;
            if (dmgDeflected > 0)
            {
                attackString = $", deflected by armor: -{dmgDeflected} ";
            }
            else
            {
                attackString = "";
                dmgDeflected = 0;
            }
            ResultStringMG += $"{attackString}";
            attackString = string.Empty;
            // damage from weapon qualities
            int dmgfromWeaponQuality = Attacker.Props.Get(SD.WeaponQuality.Devastating).SumBonus;
            if (dmgfromWeaponQuality > 0)
            {
                attackString = $", from devastating weapon: {dmgfromWeaponQuality}";
            }
            else
            {
                dmgfromWeaponQuality = Attacker.Props.Get(SD.WeaponQuality.Weak).SumBonus;
                if (dmgfromWeaponQuality > 0)
                {
                    dmgfromWeaponQuality = -dmgfromWeaponQuality;
                    attackString = $", from weak weapon: {dmgfromWeaponQuality}";
                }
            }
            ResultStringMG += $"{attackString}";
            attackString = string.Empty;
            // damage from actions (rage, charge)
            if (AdditionalDamage != 0)
            {
                attackString = $", from action: {AdditionalDamage}";
            }
            ResultStringMG += $"{attackString}";
            DamageDelt = (AttackValue - DefenceValue) - dmgDeflected + AdditionalDamage + dmgfromWeaponQuality;
            if (DamageDelt < 0) DamageDelt = 0;
            WoundSeverity = Wounds.SeverityFromDmg(DamageDelt);
            ResultStringMG.NewLine();
            ResultStringMG += $"Summary damage: {DamageDelt} - {RichText.BoldText(WoundSeverity + " wound")}";
            if(AttackLocation.IsNullOrEmpty() == false ){
                ResultStringMG += $" to {AttackLocation.ToLower()}";
            }
        }
        public void CalculateAndAddWound()
        {
            if (DamageDelt <= 0) return;
            // Prain resistance roll
            int DC = Wounds.DCFromSeverity(WoundSeverity);
            var painResRoll = SD.MakeRollTest(DC, Defender.PainResistance);
            if(DC != 0) 
            { 
                ResultStringMG.NewLine();
                ResultStringMG += $"Pain resistance test: {painResRoll.Item2}";
            }

            //create wound
            WoundDTO newWound = new();
            newWound.DateStart = Date;
            newWound.IsIgnored = painResRoll.Item1;
            newWound.Description = $"Wound inflicted by {Attacker.Name} after {AttackAction} attack.";
            if (AttackLocation.IsNullOrEmpty())
            {
                Random rnd = new Random();
                int location = rnd.Next(0, Wounds.Location.All.Length - 1);
                newWound.Location = Wounds.RandomLocation(); //Wounds.Location.Body;  
            }
            else
            {
                newWound.Location = AttackLocation;
            }
            newWound.Value = Wounds.GetValueFromSeverity(WoundSeverity);
            NewWounds.Add(newWound);

            if ((painResRoll.Item1 == false && WoundSeverity == Wounds.Severity.Critical)  ||
                Defender.Health.CurrentWounds >= Defender.Health.MaxWounds && WoundSeverity != Wounds.Severity.Deadly)
            {
                TestConditionIfHit += States.Names.Unconscious + ", ";
            }else if(WoundSeverity == Wounds.Severity.Deadly)
            {
                TestConditionIfHit += States.Names.Dead + ", ";
            }

        }
        public void WriteAndCalculatePossibleStates()
        {

            if(Defender.NewStates.Contains(States.Names.Unconscious) == false)
            {
                int DC = 0;
                foreach (var stateTest in TestConditionIfHit.Split(", "))
                {
                    Tuple<bool, string> result = new Tuple<bool, string>(false, string.Empty);
                    int duration = 0;
                    switch (stateTest)
                    {
                        case States.Names.Stumbled:
                            DC = Attacker.Props.Get(SD.WeaponQuality.Stumbling).SumBonus + DamageDelt;
                            result = SD.MakeRollTest(DC, Math.Max(Defender.Balance, Defender.Lifting));
                            duration = 99;
                            break;
                        case States.Names.Stunned:
                            DC = Attacker.Props.Get(SD.WeaponQuality.Stunning).SumBonus + DamageDelt;
                            result = SD.MakeRollTest(DC, Defender.PainResistance);
                            duration = Math.Max(1, DC / 10);
                            break;
                        case States.Names.Snatched:
                            DC = Attacker.Props.Get(SD.WeaponQuality.Snatching).SumBonus + DamageDelt;
                            result = SD.MakeRollTest(DC, Math.Max(Defender.Balance, Defender.Lifting));
                            duration = 99;
                            break;
                        case States.Names.Bleeding:
                            DC = HitValue;
                            result = SD.MakeRollTest(DC, Defender.PainResistance);
                            duration = 99;
                            break;
                        case States.Names.Blinded:
                            DC = HitValue;
                            result = SD.MakeRollTest(DC, Defender.PainResistance);
                            duration = Math.Max(1, DC / 10);
                            break;
                        case States.Names.Unconscious:
                            DC = 20 + Defender.Health.CurrentWounds - Defender.Health.MaxWounds;
                            result = SD.MakeRollTest(DC, Defender.PainResistance);
                            duration = Math.Max(1, DC / 10);
                            break;
                        case States.Names.Dead:
                            DC = 30 + Defender.Health.CurrentWounds - Defender.Health.MaxWounds;
                            result = SD.MakeRollTest(DC, Defender.PainResistance);
                            duration = Math.Max(1, DC / 10);
                            break;
                        default: continue;
                    }
                    ResultStringMG.NewLine();
                    ResultStringMG += $"Test against {stateTest}: {result.Item2}";
                    if (result.Item1 == false)
                    {
                        if(stateTest == States.Names.Dead)
                        {
                            Defender.NewStates = "Dead:999, ";
                            Defender.States = new List<TraitDTO>();
                            break; 
                        }
                        if (Defender.OldStates.Contains(stateTest))
                        {
                            var stateDTO = Defender?.States?.FirstOrDefault(s => s.Name == stateTest);
                            if (stateDTO is not null)
                            {
                                if(stateDTO.TraitValue < duration)
                                {
                                    stateDTO.TraitValue = duration;
                                }
                            }
                            else
                                throw new Exception();

                        }
                        else
                            Defender.NewStates += $"{stateTest}:{duration}, ";
                    }
                }
            }
            ResultStringMG.EndText();
        }

        public string SelectBestDefence()
        {
            if (Attacker.Props is null || Defender.Props is null) return "";
            int difference = 0, differenceMin = 100;
            string BestType = string.Empty;
            foreach(var defenceType in SD.DefenceType.All)
            {
                if (defenceType == SD.DefenceType.Shield && (Defender.Props.ShieldUsed is null || IsShieldDefenceAllowed == false)) continue;
                if (defenceType == SD.DefenceType.Armor && Defender.Props.ArmorUsed is null) continue;
                if (defenceType == SD.DefenceType.Parry && IsParryDefenceAllowed == false) continue;
                switch (defenceType)
                {
                    default:
                    case SD.DefenceType.Dodge:                        
                        difference =  Attacker.Props.Get(SD.BattleProperty.AttackDodge).SumBonus - Defender.Props.Get(SD.BattleProperty.DefenceDodge).SumBonus;
                        break;
                    case SD.DefenceType.Parry:
                        difference = Attacker.Props.Get(SD.BattleProperty.AttackParry).SumBonus - Defender.Props.Get(SD.BattleProperty.DefenceParry).SumBonus;
                        break;
                    case SD.DefenceType.Shield:
                        difference = Attacker.Props.Get(SD.BattleProperty.AttackShield).SumBonus - Defender.Props.Get(SD.BattleProperty.DefenceShield).SumBonus;
                        break;
                    case SD.DefenceType.Armor:
                        difference= Attacker.Props.Get(SD.BattleProperty.AttackArmor).SumBonus - Defender.Props.Get(SD.BattleProperty.DefenceArmor).SumBonus;
                        break;
                }
                if (difference < differenceMin)
                {
                    differenceMin = difference;
                    BestType = defenceType;
                }
            } 
            return BestType;
        }

        public static EquipmentDTO? Fists()
        {
            var item = new EquipmentDTO()
            {
                Name = SD.BasicWeaponsMelee.Fists,
                EquipmentType = SD.EquipmentType.WeaponMelee,
                Description = "Just your fists and feets",
                ShortDescr = "Just your fists and feets",
                RelatedSkill = SD.SpecialSkills.Melee.Unarmed,
                IsTwoHanded = true,
                Weight = 0.0m,
                Price = 0.0m,
                Traits = new List<TraitEquipmentDTO>()
                        {
                            new TraitEquipmentDTO(){
                                Descr = "",
                                Name = SD.WeaponParametersDescr,
                                TraitType = SD.TraitType_Gear,
                                Bonuses = new List<BonusDTO>()
                                {
                                    new BonusDTO{
                                        BonusValue = 2,
                                        FeatureType = SD.FeatureWeaponQuality,
                                        FeatureName = SD.WeaponQuality.Weak,
                                    },
                                }
                            }
                        },
                IsApproved = true,
            };
            return item;
        }

        private string GetStatesString(string oldStates)
        {
            string res = string.Empty;
            if (oldStates.Length > 4)
            {
                res = oldStates;
                res = $" ({res.Remove(res.Length - 2)})";
            }
            return res;
        }
    }
}
