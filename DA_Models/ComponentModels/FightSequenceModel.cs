using Abp.Collections.Extensions;
using DA_Common;
using DA_Common.Localization;
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
        public BattlePropertyModel Props { get; set; } = null!;
        public ICollection<TraitDTO>? States { get; set; } = new List<TraitDTO>();
        public HealthModel Health { get; set; } = null!;
        public int PainResistance { get; set; } = 0;
        public int Balance { get; set; } = 0;
        public int Lifting { get; set; } = 0;

        public Tuple<int, string> Roll { get; set; } = new Tuple<int, string>(0, string.Empty);

        public string OldStates { get; set; } = string.Empty;
        public string NewStates { get; set; } = string.Empty;
        public int ActionLeft { get; set; } = 2;

        public List<Pair<string, int>> AdditionalBonuses = new List<Pair<string, int>>();
    }

    public class FightSequenceModel
    {
        public FightSequenceModel(DateModel date)
        {
            Date = date;
        }
        public DateModel Date { get; set; } = new(1, 1);
        // input variables

        public FighterModel Attacker { get; set; } = new();
        public FighterModel Defender { get; set; } = new();

        // select variables
        public string AttackAction { get; set; } = string.Empty;
        public string AttackLocation { get; set; } = string.Empty;
        public string DefenceType { get; set; } = string.Empty;

        /// <summary>Number of attackers vs defender this roll (0 = not surrounded). Set from fight dialog only.</summary>
        public int SurroundedAttackerCount { get; set; } = 0;

        /// <summary>Attack from behind (+3); part of full flanking with Surrounded. Not persisted.</summary>
        public bool IsAttackerFlanking { get; set; } = false;


        // private variables

        /// <summary>Shortest "Name (+/-N), " modifier entry; a shorter accumulator means nothing meaningful was written.</summary>
        private const int MinFormattedStateStringLength = 5;

        private bool IsHit { get; set; } = false;
        private bool IsCriticalHit { get; set; } = false;
        private bool IsCriticalDefense { get; set; } = false;
        private int CriticalHitDamageBonus { get; set; } = 0;
        private int AttackValue { get; set; } = 0;
        private int DefenceValue { get; set; } = 0;
        private int HitValue { get; set; } = 0;
        private int AdditionalDamage { get; set; } = 0;
        private int DamageDelt { get; set; } = 0;
        private string TestConditionIfHit { get; set; } = string.Empty;
        private MobWoundOverflowResult _pendingMobOverflow = MobWoundOverflowResult.None;

        // public variables

        public bool IsShieldDefenceAllowed { get; set; } = true;
        public bool IsParryDefenceAllowed { get; set; } = true;

        // return variables

        public string WoundSeverity { get; set; } = string.Empty;
        public RichText ResultStringMG { get; set; } = new();
        public List<WoundDTO> NewWounds { get; set; } = new List<WoundDTO>();
        public int AppliedMobDamage { get; set; }
        public int IgnoredMobDamage { get; set; } 

        // functions

        /// <summary>State duration in turns derived from a check DC (at least one turn).</summary>
        private static int DurationFromDc(int dc) =>
            Math.Max(States.Duration.SingleTurn, dc / SD.FightModifiers.StateDurationDcTier);

        private static List<TraitDTO> FightCalculationStates(IEnumerable<TraitDTO>? states) =>
            (states ?? []).Where(s => s.Name is not ("Flanking" or States.Names.Surrounded)).ToList();

        public static FighterModel? AddFighter(AllParamsModel allParams)
        {
            if (allParams?.Character is null || allParams.BattleProperties is null || allParams.Health is null)
                return null;

            FighterModel fighter = new();
            fighter.Props = allParams.BattleProperties;
            fighter.States = FightCalculationStates(allParams.TraitsTemporary);
            fighter.Health = allParams.Health;
            fighter.Name = allParams.Character.NPCName;
            fighter.PainResistance = allParams.SpecialSkills.Get(SD.SpecialSkills.Athletics.PainResistance).SumBonus;
            fighter.Lifting = allParams.SpecialSkills.Get(SD.SpecialSkills.Athletics.Lifting).SumBonus;
            fighter.Balance = allParams.SpecialSkills.Get(SD.SpecialSkills.Acrobatics.Balance).SumBonus;
            return fighter;
        }

        public static FighterModel? AddFighter(MobDTO mob)
        {
            if (mob is null)
                return null;

            if (mob.BattleProperties is null)
            {
                mob.BattleProperties = new MobBattlePropertyModel(mob);
                mob.BattleProperties.CalculateBattleStats();
            }

            FighterModel fighter = new();
            fighter.Props = mob.BattleProperties;
            fighter.States = FightCalculationStates(ParseMobStates(mob.States));
            fighter.Health = new MobHealthModel(mob);
            fighter.Name = mob.Name;
            fighter.PainResistance = mob.PainResSkillValue;
            var hpPenalty = MobHealthModel.CombatPenalty(mob.CurrentWounds, mob.MaxWounds);
            fighter.Lifting = mob.AttackSkillValue - hpPenalty;
            fighter.Balance = mob.DodgeSkillValue - hpPenalty;
            return fighter;
        }

        private static List<TraitDTO> ParseMobStates(string? statesString) =>
            CombatStateString.Parse(statesString)
                .Select(state => (TraitDTO)StateSeeder.GetStateDTO(state.Name, false, state.Duration))
                .ToList();

        private static int GetActionLeftFromStates(ICollection<TraitDTO>? states, int defaultActionLeft)
        {
            if (states is null || !states.Any())
                return defaultActionLeft;

            if (states.Any(s => s.Name == States.Names.NoTurn))
                return (int)TurnLeft.No;

            if (states.Any(s => s.Name == States.Names.HalfTurn))
                return (int)TurnLeft.Half;

            return defaultActionLeft;
        }

        public static string MergeMobStates(string? existingStates, string? newStates) =>
            CombatStateString.Merge(existingStates, newStates);

        /// <summary>Outcome of applying wound overflow rules to a mob defender.</summary>
        public readonly record struct MobWoundOverflowResult(bool IsDead, bool IsUnconscious, string NewStates)
        {
            public static MobWoundOverflowResult None => new(false, false, string.Empty);
        }

        /// <summary>
        /// Mobs automatically lose consciousness when wounds exceed max health, and die when wounds
        /// reach max health + <see cref="FightModifiers.MobDeathOverflowThreshold"/> or more.
        /// </summary>
        public static MobWoundOverflowResult EvaluateMobWoundOverflow(int projectedWounds, int maxWounds)
        {
            if (projectedWounds >= maxWounds + FightModifiers.MobDeathOverflowThreshold)
            {
                return new MobWoundOverflowResult(
                    true,
                    false,
                    CombatStateString.Add(null, States.Names.Dead, States.Duration.Permanent));
            }

            if (projectedWounds > maxWounds)
            {
                return new MobWoundOverflowResult(
                    false,
                    true,
                    CombatStateString.Add(null, States.Names.Unconscious, States.Duration.UntilResolved));
            }

            return MobWoundOverflowResult.None;
        }

        private bool IsMobDefender() => Defender.Health is MobHealthModel;

        public void UpdateDefenceFlags()
        {
            if (Defender.Props is null)
            {
                IsParryDefenceAllowed = false;
                IsShieldDefenceAllowed = true;
                return;
            }

            IsParryDefenceAllowed = Defender.Props.Get(SD.BattleProperty.DefenceParry)?.GearBonus > 0;
            IsShieldDefenceAllowed = Defender.Props.ShieldUsed is not null;
        }

        /// <summary>
        /// Resolves DefenceType when unset (e.g. MudSelect not touched). Uses SelectBestDefence, then Dodge.
        /// </summary>
        public void EnsureDefenceType()
        {
            if (!string.IsNullOrEmpty(DefenceType))
                return;

            if (Defender.Props is null)
            {
                DefenceType = SD.DefenceType.Dodge;
                return;
            }

            UpdateDefenceFlags();
            var best = SelectBestDefence();
            DefenceType = string.IsNullOrEmpty(best) ? SD.DefenceType.Dodge : best;
        }

       
        public string CalculateAndWriteAttack()
        {
            try
            {

                ClearRoll();
                EnsureDefenceType();
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
                return string.Empty;
            }
            catch(Exception ex)
            {
                return ex.Message;
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
            _pendingMobOverflow = MobWoundOverflowResult.None;
            HitValue = 0;
            IsHit = false;
            IsCriticalHit = false;
            IsCriticalDefense = false;
            CriticalHitDamageBonus = 0;
            Attacker.NewStates  = string.Empty;
            Defender.NewStates  = string.Empty;
            Attacker.OldStates = string.Empty;
            Defender.OldStates = string.Empty;
            WoundSeverity = string.Empty;
            AppliedMobDamage = 0;
            IgnoredMobDamage = 0;
            Attacker.Roll = new Tuple<int, string>(0, string.Empty);
            Defender.Roll  = new Tuple<int, string>(0, string.Empty);
            NewWounds = new List<WoundDTO>();
            if (Attacker.Props is null || Defender.Props is null)
            {
                ResultStringMG += Loc.T("Error! No Attacker or Defender properties are loaded");
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
            ResultStringMG += Loc.T("Attack is aimed at the {0} {1}", RichText.BoldText(AttackLocation.ToLower()), SD.BonusText(AttackCurrValue));
            AttackValue += AttackCurrValue;
        }

        public void WriteWhoAttacksWhoAndHow()
        {
            int AttackCurrValue = 0;
            int DefenceCurrValue = 0;
            string defenceString = string.Empty;
            string weaponString = string.Empty;
            string weaponModifierString = string.Empty;
            int weaponCurrValue = 0;
            string actionDescription = string.Empty;
            int actionBonus = 0;

            var surroundedPenalty = ApplySurroundedDefencePenalty();

            switch (DefenceType)
            {
                default:
                case SD.DefenceType.Dodge:
                    weaponCurrValue += Attacker.Props.Get(SD.BattleProperty.AttackDodge).SumBonus;
                    DefenceCurrValue += Defender.Props.Get(SD.BattleProperty.DefenceDodge).SumBonus;                   
                    defenceString = Loc.T(SD.DefenceType.Dodge.ToLower());
                    break;
                case SD.DefenceType.Parry:
                    weaponCurrValue = Attacker.Props.Get(SD.BattleProperty.AttackParry).SumBonus;
                    DefenceCurrValue += Defender.Props.Get(SD.BattleProperty.DefenceParry).SumBonus;
                    defenceString = Loc.T(SD.DefenceType.Parry.ToLower());
                    break;
                case SD.DefenceType.Shield:
                    weaponCurrValue += Attacker.Props.Get(SD.BattleProperty.AttackShield).SumBonus;
                    DefenceCurrValue += Defender.Props.Get(SD.BattleProperty.DefenceShield).SumBonus;
                    defenceString = Loc.T("deflect with shield");
                    break;
                case SD.DefenceType.Armor:
                    weaponCurrValue += Attacker.Props.Get(SD.BattleProperty.AttackArmor).SumBonus;
                    DefenceCurrValue += Defender.Props.Get(SD.BattleProperty.DefenceArmor).SumBonus;
                    defenceString = Loc.T("deflect with armor");
                    break;
            }

            if (Attacker?.Props?.MainWeaponUsed?.Name is null)
                throw new Exception("Attacker weapon is missing");
            weaponString += Loc.T("using {0} {1}", Attacker.Props.MainWeaponUsed.Name, SD.BonusText(weaponCurrValue)) + " ";
            AttackValue += weaponCurrValue;
            AttackCurrValue = 0;

            /// Get bonus from attack action
            switch (AttackAction)
            {
                default:
                case SD.AttackAction.Normal:
                    actionBonus = 0;
                    Attacker.ActionLeft -= 1;
                    break;
                case SD.AttackAction.Cautious:
                    actionBonus = -3;
                    if(Attacker.NewStates.Contains(States.Names.Cautious) == false ||
                       Attacker.OldStates.Contains(States.Names.Cautious) == false)  // dont add it twice
                        Attacker.NewStates += $"{States.Names.Cautious}:{States.Duration.SingleTurn}, ";  // add new state
                    actionDescription = Loc.T("cautiously");
                    Attacker.ActionLeft -= 1;
                    break;                
                case SD.AttackAction.Charge:
                    actionBonus = 5;
                    AdditionalDamage += 3;
                    actionDescription = Loc.T("charging");
                    Attacker.ActionLeft -= 2;
                    break;
                case SD.AttackAction.Raging:
                    actionBonus = 7;
                    AdditionalDamage += 3;
                    if (Attacker.NewStates.Contains(States.Names.Unbalanced) == false ||
                        Attacker.OldStates.Contains(States.Names.Unbalanced) == false)
                        Attacker.NewStates += $"{States.Names.Unbalanced}:{States.Duration.SingleTurn}, ";
                    actionDescription = Loc.T("furiously!");
                    Attacker.ActionLeft -= 2;
                    break;
                case SD.AttackAction.Strong:
                    actionBonus = 5;
                    actionDescription = Loc.T("with all strength");
                    Attacker.ActionLeft -= 2;
                    break;
            }
            if(Attacker.ActionLeft == 1)
            {
                Attacker.NewStates += $"{States.Names.HalfTurn}:{States.Duration.SingleTurn}, ";
            }else if(Attacker.ActionLeft < 1)
            {
                Attacker.NewStates = Attacker.NewStates.Replace($"{States.Names.HalfTurn}:{States.Duration.SingleTurn}, ", string.Empty);
                Attacker.NewStates += $"{States.Names.NoTurn}:{States.Duration.SingleTurn}, ";
            }

            AttackValue += actionBonus;
            DefenceValue += DefenceCurrValue;
            defenceString += SD.BonusText(DefenceCurrValue);

            var actionDisplay = string.Empty;
            if (!string.IsNullOrEmpty(actionDescription))
                actionDisplay = $" {RichText.BoldText(actionDescription)}{SD.BonusText(actionBonus)}";

            if (IsAttackerFlanking)
            {
                AttackValue += SD.FightModifiers.FlankingAttackBonus;
                IsShieldDefenceAllowed = false;
                actionDisplay += $" {RichText.BoldText(Loc.T("flanking"))}{SD.BonusText(SD.FightModifiers.FlankingAttackBonus)}";
            }

            // add weapon bonus if exists
            AttackCurrValue = Attacker.Props.Get(SD.WeaponQuality.Precise).SumBonus;
            if (AttackCurrValue > 0)
            {
                AttackValue += AttackCurrValue;
                weaponModifierString += ", " + Loc.T("with precise weapon") + SD.BonusText(AttackCurrValue);
            }
            AttackCurrValue = Attacker.Props.Get(SD.WeaponQuality.Bulky).SumBonus;
            if (AttackCurrValue > 0)
            {
                AttackValue -= AttackCurrValue;
                weaponModifierString += ", " + Loc.T("with crude weapon") + SD.BonusText(-AttackCurrValue);
            }
            string attackerStatesString = GetStatesString(Attacker.OldStates);
            string defenderStatesString = GetStatesString(Defender.OldStates);
            attackerStatesString += GetAdditionalBonusString(true);
            defenderStatesString += GetAdditionalBonusString(false);
            if (surroundedPenalty > 0)
                defenderStatesString += FormatSurroundedModifier(surroundedPenalty);
            if(attackerStatesString.Length > 0) attackerStatesString = $"({attackerStatesString})";
            if (defenderStatesString.Length > 0) defenderStatesString = $"({defenderStatesString})";

            ResultStringMG += Loc.T("{0} {1} attacks{2} {3}{4}, {5} {6} tries to {7}.", RichText.BoldText(Attacker.Name), attackerStatesString, actionDisplay, weaponString, weaponModifierString, RichText.BoldText(Defender.Name), defenderStatesString, defenceString);
        }

        public static int GetSurroundedDefencePenalty(string defenceType, int attackerCount)
        {
            var extraAttackers = Math.Max(0, attackerCount - 1);
            if (extraAttackers == 0)
                return 0;

            var penaltyPerExtra = string.Equals(defenceType, SD.DefenceType.Armor, StringComparison.Ordinal)
                ? SD.FightModifiers.SurroundedArmorPenaltyPerExtra
                : SD.FightModifiers.SurroundedDefencePenaltyPerExtra;
            return penaltyPerExtra * extraAttackers;
        }

        private int ApplySurroundedDefencePenalty()
        {
            if (SurroundedAttackerCount <= 1)
                return 0;

            var defenceType = DefenceType;
            if (string.IsNullOrEmpty(defenceType))
                defenceType = SD.DefenceType.Dodge;

            var penalty = GetSurroundedDefencePenalty(defenceType, SurroundedAttackerCount);
            if (penalty <= 0)
                return 0;

            DefenceValue -= penalty;
            return penalty;
        }

        private string FormatSurroundedModifier(int penalty)
        {
            var surroundedLabel = RichText.BoldText(LocCatalog.Name(States.Names.Surrounded));
            var label = string.Equals(DefenceType, SD.DefenceType.Armor, StringComparison.Ordinal)
                ? $"{surroundedLabel} {RichText.BoldText(Loc.T("armored"))}"
                : surroundedLabel;
            return $" {label}{SD.BonusText(-penalty)}";
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
                        case States.Names.Disarmed:
                            Attacker.Props.MainWeaponUsed = Unarmed();
                            Attacker.Props.CalculateBattleStats();
                            break;
                    }
                    AttackValue += AttackCurrValue;
                    Attacker.OldStates += AttackCurrValue == 0 ? string.Empty : $"{state.Name} {SD.BonusText(AttackCurrValue)}, ";
                }
                if(Attacker.OldStates.Length < MinFormattedStateStringLength)                
                {
                    Attacker.OldStates = string.Empty;
                }
            }

            Attacker.ActionLeft = GetActionLeftFromStates(Attacker.States, (int)TurnLeft.Whole);

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
                            DefenceCurrValue += -SD.FightModifiers.IncapacitatedDefencePenalty;
                            break;
                        case States.Names.Unconscious:
                            DefenceCurrValue += -SD.FightModifiers.IncapacitatedDefencePenalty;
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
                    }
                    DefenceValue += DefenceCurrValue;
                    Defender.OldStates += DefenceCurrValue == 0 ? "" : $"{state.Name} {SD.BonusText(DefenceCurrValue)}, ";
                }
                if(Defender.OldStates.Length < MinFormattedStateStringLength)
                {
                    Defender.OldStates = string.Empty;
                }
                Defender.ActionLeft = GetActionLeftFromStates(Defender.States, 0);
            }
        }
        public void WriteDiceRollsAndAttackSummary()
        {
            string attackString = string.Empty;
            Attacker.Roll = SD.RollDice();
            Defender.Roll = SD.RollDice();
            IsCriticalHit = RollService.IsCriticalSuccess(Attacker.Roll.Item1);
            IsCriticalDefense = RollService.IsCriticalSuccess(Defender.Roll.Item1);
            ResultStringMG.NewLine();
            ResultStringMG += Loc.T("{0} roll: {1}, {2} roll: {3}", Attacker.Name, RichText.BoldText(Attacker.Roll.Item2), Defender.Name, RichText.BoldText(Defender.Roll.Item2));
            AttackValue += Attacker.Roll.Item1;
            DefenceValue += Defender.Roll.Item1;
            HitValue = AttackValue - DefenceValue;
            if (IsCriticalDefense && IsCriticalHit == false)
            {
                IsHit = false;
            }
            else if (IsCriticalHit)
            {
                IsHit = true;
                CriticalHitDamageBonus = SD.FightModifiers.CriticalHitDamageBonus;
            }
            else if (HitValue >= 0)
            {
                IsHit = true;
            }
            if (IsCriticalHit)
                attackString = Loc.T("Critical Hit!");
            else if (IsCriticalDefense)
                attackString = Loc.T("Critical Defence!");
            else
                attackString = IsHit ? Loc.T("Hit!") : Loc.T("Miss!");
            ResultStringMG.NewLine();
            ResultStringMG += Loc.T(" {0} summary: {1}, {2} summary: {3}. {4}", Attacker.Name, RichText.BoldText(AttackValue.ToString()), Defender.Name, RichText.BoldText(DefenceValue.ToString()), RichText.BoldText(attackString));
            if(IsHit == false)
            {
                WriteNewStatesSummary();
                ResultStringMG.EndText();
            }
        }
        public void WriteDamageSummary()
        {
            if (IsHit == false) return; 

            string attackString = string.Empty;
            // damage from attack
            ResultStringMG.NewLine();
            ResultStringMG += Loc.T("Damage dealt from attack: {0}", HitValue);
            int dmgDeflected = Defender.Props.Get(SD.BattleProperty.ArmorClass).SumBonus - Attacker.Props.Get(SD.WeaponQuality.ArmorPiercing).SumBonus;
            if (dmgDeflected > 0)
            {
                attackString = ", " + Loc.T("deflected by armor: -{0}", dmgDeflected) + " ";
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
                attackString = ", " + Loc.T("from devastating weapon: {0}", dmgfromWeaponQuality);
            }
            else
            {
                dmgfromWeaponQuality = Attacker.Props.Get(SD.WeaponQuality.Weak).SumBonus;
                if (dmgfromWeaponQuality > 0)
                {
                    dmgfromWeaponQuality = -dmgfromWeaponQuality;
                    attackString = ", " + Loc.T("from weak weapon: {0}", dmgfromWeaponQuality);
                }
            }
            ResultStringMG += $"{attackString}";
            attackString = string.Empty;
            // damage from actions (rage, charge)
            if (AdditionalDamage != 0)
            {
                attackString = ", " + Loc.T("from action: {0}", AdditionalDamage);
            }
            ResultStringMG += $"{attackString}";
            attackString = string.Empty;
            if (CriticalHitDamageBonus > 0)
            {
                attackString = ", " + Loc.T("from critical hit: +{0}", CriticalHitDamageBonus);
            }
            ResultStringMG += $"{attackString}";
            DamageDelt = (AttackValue - DefenceValue) - dmgDeflected + AdditionalDamage + dmgfromWeaponQuality + CriticalHitDamageBonus;
            if (DamageDelt < 0) DamageDelt = 0;
            WoundSeverity = Wounds.SeverityFromDmg(DamageDelt);
            ResultStringMG.NewLine();
            ResultStringMG += Loc.T("Summary damage: {0} - {1}", DamageDelt, RichText.BoldText(Loc.T("{0} wound", WoundSeverity)));
            if (AttackLocation.IsNullOrEmpty() == false)
                ResultStringMG += " " + Loc.T("to {0}", RichText.BoldText(AttackLocation.ToLower()));
        }
        public void CalculateAndAddWound()
        {
            if (DamageDelt <= 0) return;
            // Prain resistance roll
            int DC = Wounds.DCFromSeverity(WoundSeverity);
            var painResRoll = SD.MakeRollTestForFight(DC, Defender.PainResistance);
            if(DC != 0) 
            { 
                ResultStringMG.NewLine();
                ResultStringMG += Loc.T("Pain resistance test: {0}", painResRoll.Item2);
            }

            //create wound
            WoundDTO newWound = new();
            newWound.DateStart = Date;
            newWound.IsIgnored = painResRoll.Item1;
            newWound.Description = Loc.T("Wound inflicted by {0} after {1} attack.", Attacker.Name, AttackAction);
            if (AttackLocation.IsNullOrEmpty())
            {
                newWound.Location = Wounds.RandomLocation();
            }
            else
            {
                newWound.Location = AttackLocation;
            }
            newWound.Value = Wounds.GetValueFromSeverity(WoundSeverity);
            NewWounds.Add(newWound);

            if (IsMobDefender())
            {
                AppliedMobDamage = MobHealthModel.ApplyIncomingDamage(DamageDelt, painResRoll.Item1);
                IgnoredMobDamage = DamageDelt - AppliedMobDamage;
                var projectedWounds = Defender.Health.CurrentWounds + AppliedMobDamage;
                ResultStringMG.NewLine();
                if (IgnoredMobDamage > 0)
                {
                    ResultStringMG +=
                        $"Mob damage: {AppliedMobDamage} applied ({IgnoredMobDamage} ignored). {MobHealthModel.FormatHpLog(projectedWounds, Defender.Health.MaxWounds)}";
                }
                else
                {
                    ResultStringMG +=
                        $"Mob damage: {AppliedMobDamage} applied. {MobHealthModel.FormatHpLog(projectedWounds, Defender.Health.MaxWounds)}";
                }
                _pendingMobOverflow = EvaluateMobWoundOverflow(projectedWounds, Defender.Health.MaxWounds);
                return;
            }

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
            if (_pendingMobOverflow.IsDead && IsMobDefender())
            {
                Defender.NewStates = _pendingMobOverflow.NewStates;
                Defender.States = new List<TraitDTO>();
                ResultStringMG.NewLine();
                ResultStringMG += Loc.T("Mob is dead — wounds exceed maximum health by 8 or more.");
                _pendingMobOverflow = MobWoundOverflowResult.None;
                WriteNewStatesSummary();
                ResultStringMG.EndText();
                return;
            }

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
                            result = SD.MakeRollTestForFight(DC, Math.Max(Defender.Balance, Defender.Lifting));
                            duration = States.Duration.UntilResolved;  // until standing up
                            break;
                        case States.Names.Stunned:
                            DC = Attacker.Props.Get(SD.WeaponQuality.Stunning).SumBonus + DamageDelt;
                            result = SD.MakeRollTestForFight(DC, Defender.PainResistance);
                            duration = DurationFromDc(DC);
                            break;
                        case States.Names.Snatched:
                            DC = Attacker.Props.Get(SD.WeaponQuality.Snatching).SumBonus + DamageDelt;
                            result = SD.MakeRollTestForFight(DC, Math.Max(Defender.Balance, Defender.Lifting));
                            duration = States.Duration.UntilResolved; // until release from grabbed
                            break;
                        case States.Names.Bleeding:
                            DC = HitValue + SD.FightModifiers.WoundConditionCheckOffset;
                            result = SD.MakeRollTestForFight(DC, Defender.PainResistance);
                            duration = States.Duration.UntilResolved; // until stop bleeding
                            break;
                        case States.Names.Blinded:
                            DC = HitValue + SD.FightModifiers.WoundConditionCheckOffset;
                            result = SD.MakeRollTestForFight(DC, Defender.PainResistance);
                            duration = DurationFromDc(DC);
                            break;
                        case States.Names.Unconscious:
                            DC = SD.FightModifiers.UnconsciousCheckBaseDc + Defender.Health.CurrentWounds - Defender.Health.MaxWounds;
                            result = SD.MakeRollTestForFight(DC, Defender.PainResistance);
                            duration = DurationFromDc(DC);
                            break;
                        case States.Names.Dead:
                            DC = SD.FightModifiers.DeadCheckBaseDc + Defender.Health.CurrentWounds - Defender.Health.MaxWounds;
                            result = SD.MakeRollTestForFight(DC, Defender.PainResistance);
                            duration = DurationFromDc(DC);
                            break;
                        default: continue;
                    }
                    ResultStringMG.NewLine();
                    ResultStringMG += Loc.T("Test against {0}: {1}", stateTest, result.Item2);
                    if (result.Item1 == false)
                    {
                        if(stateTest == States.Names.Dead)
                        {
                            Defender.NewStates = CombatStateString.Add(null, States.Names.Dead, States.Duration.Permanent);
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

            if (_pendingMobOverflow.IsUnconscious && IsMobDefender()
                && CombatStateString.HasState(Defender.NewStates, States.Names.Unconscious) == false)
            {
                Defender.NewStates = CombatStateString.Merge(Defender.NewStates, _pendingMobOverflow.NewStates);
                ResultStringMG.NewLine();
                ResultStringMG += Loc.T("Mob loses consciousness — wounds exceed maximum health.");
                _pendingMobOverflow = MobWoundOverflowResult.None;
            }

            WriteNewStatesSummary();
            ResultStringMG.EndText();
        }

        private void WriteNewStatesSummary()
        {
            var attackerStates = FormatNewStatesForDisplay(Attacker.NewStates);
            if (!string.IsNullOrEmpty(attackerStates))
            {
                ResultStringMG.NewLine();
                ResultStringMG += Loc.T("{0} new states: {1}", Attacker.Name, attackerStates);
            }

            var defenderStates = FormatNewStatesForDisplay(Defender.NewStates);
            if (!string.IsNullOrEmpty(defenderStates))
            {
                ResultStringMG.NewLine();
                ResultStringMG += Loc.T("{0} new states: {1}", Defender.Name, defenderStates);
            }
        }

        private static string FormatNewStatesForDisplay(string newStates)
        {
            if (string.IsNullOrWhiteSpace(newStates))
                return string.Empty;

            var parts = CombatStateString.Parse(newStates)
                .Select(entry => entry.Name)
                .Where(name => name != States.Names.NoTurn && name != States.Names.HalfTurn)
                .Select(RichText.BoldText);

            return string.Join(", ", parts);
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

        public static EquipmentDTO? Unarmed()
        {
            var item = new EquipmentDTO()
            {
                Name = SD.BasicWeaponsMelee.Unarmed,
                EquipmentType = SD.EquipmentType.WeaponMelee,
                Description = "Punches, kicks, bites, and other unarmed attacks",
                ShortDescr = "Punches, kicks, bites, and other unarmed attacks",
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
                res = $"{res.Remove(res.Length - 2)}";
            }
            return res;
        }
        private string GetAdditionalBonusString(bool isAttacker)
        {
            var fighter = isAttacker ? Attacker : Defender;
            if (fighter.AdditionalBonuses is null || fighter.AdditionalBonuses.Count == 0)
                return string.Empty;

            var res = string.Empty;
            foreach (var item in fighter.AdditionalBonuses)
            {
                if (isAttacker)
                    AttackValue += item.Second;
                else
                    DefenceValue += item.Second;

                res += $"{item.First} ({RichText.NumToStr(item.Second)}), ";
            }

            return res.Length >= 2
                ? $" {res.Remove(res.Length - 2)}"
                : string.Empty;
        }
    }
}
