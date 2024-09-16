﻿using Syncfusion.Blazor.RichTextEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using static MudBlazor.CategoryTypes;
using static Npgsql.PostgresTypes.PostgresCompositeType;

namespace DA_Common
{
    public static class SD
    {
        public const string Role_Admin = "Admin";
        public const string Role_GameMaster = "GameMaster";
        public const string Role_HeroPlayer = "HeroPlayer";
        public const string Role_DukePlayer = "DukePlayer";

        public const string FeatureAttribute = "Attribute";
        public const string FeatureBaseSkill = "BaseSkill";
        public const string FeatureSpecialSkill = "SpecialSkill";
        public const string FeatureWeaponQuality = "WeaponQuality";
        public const string FeatureDukeTraits = "DukeTraits";
        public const string FeatureOther = "Other";

        public const string TraitType_Advantage = "Advantage";
        public const string TraitType_Race = "Race";
        public const string TraitType_Gear = "Gear";
        public const string TraitType_Unique = "Unique";

        public const string NPCName_GameMaster = "Game Master";
        // Weapon qualities
  

        public readonly struct WeaponQuality
        {
            public const string Fast = "Fast";
            public const string Slow = "Slow";
            public const string Parrying = "Parrying";
            public const string ShieldDestructive = "Shield destructive";
            public const string ArmorPiercing = "Armor piercing";
            public const string Long = "Long";
            public const string Heavy = "Heavy";
            public const string Devastating = "Devastating";
            public const string Weak = "Weak";
            public const string Stunning = "Stunning";
            public const string Stumbling = "Stumbling";
            public const string Snatching = "Snatching";
            public const string Disarming = "Disarming";
            public const string Armor = "Armor";
            public const string ArmorDefenceBonus = "Armor defence bonus";
            public const string ArmorPenalty = "Armor Penalty";
            public const string Durability = "Durability";
            public const string ShieldDefenceBonus = "Shield defence bonus";
            public const string Bulky = "Bulky";
            public const string Precise = "Precise";
            public const string Range = "Range";
            public const string Light = "Light";
            public const string Reload = "Reload";

            public static readonly string[] All = { Fast, Slow, Parrying, ShieldDestructive, ArmorPiercing, Long, Heavy, Devastating,
                Weak, Stunning, Stumbling, Snatching,Disarming, Armor, ArmorDefenceBonus,ArmorPenalty ,Durability ,ShieldDefenceBonus,Bulky ,Precise,Range,Light,Reload };
        }

        public readonly struct BattleProperty
        {
            public const string AttackBase = "AttackBase";
            public const string AttackDodge = "AttackDodge";
            public const string AttackArmor = "AttackArmor";
            public const string AttackShield = "AttackShield";
            public const string AttackParry = "AttackParry";
            public const string DamageBonus = "DamageBonus";

            public const string ArmorClass = "ArmorClass";
            public const string DefenceDodge = "DefenceDodge";
            public const string DefenceArmor = "DefenceArmor";
            public const string DefenceShield = "DefenceShield";
            public const string DefenceParry = "DefenceParry";
            public static readonly string[] All = { AttackBase, AttackDodge, AttackArmor, AttackShield, AttackParry,DamageBonus,
                                                    ArmorClass, DefenceDodge, DefenceArmor, DefenceShield, DefenceParry };
        }

        public readonly struct Attributes
        {
            public const string Strength = "Strength";
            public const string Dexterity = "Dexterity";
            public const string Endurance = "Endurance";
            public const string Intelligence = "Intelligence";
            public const string Instinct = "Instinct";
            public const string Willpower = "Willpower";
            public const string Charisma = "Charisma";
            public static readonly string[] All = { Strength, Dexterity, Endurance, Intelligence, Instinct, Willpower, Charisma };
        }
        public readonly struct BaseSkills
        {

            public const string Melee = "Melee";
            public const string Shooting = "Shooting";
            public const string Acrobatics = "Acrobatics";
            public const string SleightOfHands = "Sleight of hands";
            public const string Athletics = "Athletics";
            public const string Talk = "Talk";
            public const string Deceit = "Deceit";
            public const string Perception = "Perception";
            public const string Knowledge = "Knowledge";
            public const string Craft = "Craft";
            public const string Survival = "Survival";
            public const string AnimalHandle = "Animal handle";
            public const string Medicine = "Medicine";
            public static readonly string[] All = { Melee, Shooting, Acrobatics, SleightOfHands, Athletics, Talk, Deceit, Perception, Knowledge, Craft, Survival, AnimalHandle, Medicine };
        }

        

        public readonly struct SpecialSkills
        {
            public readonly struct Melee
            {
                public const string Heavy = "Heavy weapons";
                public const string Swords = "Swords and sabres";
                public const string Fencing = "Fencing weapons";
                public const string Light = "Light weapons";
                public const string Shields = "Shields";
                public const string Polearms = "Polearms";
            };
            public readonly struct Shooting
            {
                public const string Bows = "Bows";
                public const string Crossbows = "Crossbows";
                public const string Throwing = "Throwing weapons";
                public const string Slingshots = "Slingshots";
                public const string Javelins = "Javelins";
                public const string Firearms = "Firearms";
                public const string Grenades = "Grenades";
            }
            public readonly struct Acrobatics
            {
                public const string Jumping = "Jumping";
                public const string Climbing = "Climbing";
                public const string Balance = "Balance";
                public const string Running = "Running";
                public const string Dodge = "Dodge";
            }
            public readonly struct SleightOfHands
            {
                public const string Pickpocketing = "Pickpocketing";
                public const string Lockpicking = "Lockpicking";
                public const string DisarmingTraps = "Disarming traps";
                public const string Tricks = "Tricks";
                public const string Handcraft = "Handcraft";
            }
            public readonly struct Athletics
            {
                public const string Wrestling = "Wrestling";
                public const string Lifting = "Lifting";
                public const string Armor = "Armor";
                public const string Threatening = "Threatening";
                public const string PainResistance = "Pain Resistance";
                public const string Swimming = "Swimming";
            }

            public static readonly string[] ArmorPenaltySkills = { Acrobatics.Jumping, Acrobatics.Climbing, Acrobatics.Balance, Acrobatics.Running, Acrobatics.Dodge,
                                                                SleightOfHands.Pickpocketing, "Sneak",Athletics.Swimming};
        }
        public readonly struct EquipmentType
        {
            public const string Other = "Other";
            public const string WeaponMelee = "WeaponMelee";
            public const string WeaponRanged = "WeaponRanged";
            public const string Shield = "Shield";
            public const string Face = "Face";
            public const string Throat = "Throat";
            public const string Body = "Body";
            public const string Hands = "Hands";
            public const string Waist = "Waist";
            public const string Feet = "Feet";
            public const string Head = "Head";
            public const string Shoulders = "Shoulders";
            public const string Torso = "Torso";
            public const string Arms = "Arms";
            public const string Rings = "Rings";
            public static readonly string[] All = { Other, WeaponMelee, WeaponRanged, Shield, Face, Throat, Body, Hands, Waist, Feet, Head, Shoulders, Torso, Arms, Rings };
        }
        public readonly struct SlotType
        {
            public const string Other = "Other";
            public const string WeaponMain1 = "WeaponMain1";
            public const string WeaponOff1 = "WeaponOff1";
            public const string WeaponMain2 = "WeaponMain2";
            public const string WeaponOff2 = "WeaponOff2";
            public const string Shield = "Shield";
            public const string Face = "Face";
            public const string Throat = "Throat";
            public const string Body = "Body";
            public const string Hands = "Hands";
            public const string Waist = "Waist";
            public const string Feet = "Feet";
            public const string Head = "Head";
            public const string Shoulders = "Shoulders";
            public const string Torso = "Torso";
            public const string Arms = "Arms";
            public const string Ring1 = "Ring1";
            public const string Ring2 = "Ring2";
            public static readonly string[] All = { Other, WeaponMain1, WeaponOff1, WeaponMain2, WeaponOff2, Shield, Face, Throat, Body, Hands, Waist, Feet, Head, Shoulders, Torso, Arms, Ring1,Ring2 };
        }

        public readonly struct WoundSeverity
        {
            public const string Light = "Light";
            public const string Moderate = "Moderate";
            public const string Heavy = "Heavy";
            public const string Critical = "Critical";
            public const string Deadly = "Deadly";
            public static readonly string[] All = { Light, Moderate, Heavy, Critical, Deadly };
        }
        public readonly struct WoundLocation
        {
            public const string Head = "Head";
            public const string Neck = "Neck";
            public const string MainArm = "Main arm";
            public const string OffArm = "Off arm";
            public const string MainHand = "Main hand";
            public const string OffHand = "Off hand";
            public const string Back = "Back";
            public const string LeftLeg = "Left Leg";
            public const string RightLeg = "Right Leg";
            public const string Face = "Face";
            public const string Body = "Body";
            public static readonly string[] All = { Head, Neck, MainArm, OffArm, MainHand, OffHand, Back, LeftLeg, Body, RightLeg, Face, Body };
        }
        public enum WoundLocationEnum
        {
            Head, Neck, MainArm, OffArm, MainHand, OffHand, Back, LeftLeg, RightLeg, Face, Body
        }
        public static readonly string[,] WoundAttributes = { 
            { SD.Attributes.Instinct, SD.Attributes.Intelligence, }, //Head
            { SD.Attributes.Endurance, SD.Attributes.Dexterity, }, //Neck
            { SD.Attributes.Dexterity, SD.Attributes.Strength, }, //Main arm
            { SD.Attributes.Dexterity, SD.Attributes.Strength, }, //OffArm
            { SD.Attributes.Dexterity, SD.Attributes.Strength, }, //MainHand
            { SD.Attributes.Dexterity, SD.Attributes.Strength, }, //OffHand
            { SD.Attributes.Dexterity, SD.Attributes.Strength, }, //Back
            { SD.Attributes.Dexterity, SD.Attributes.Endurance, }, //LeftLeg
            { SD.Attributes.Dexterity, SD.Attributes.Endurance, }, //RightLeg
            { SD.Attributes.Charisma, SD.Attributes.Instinct, }, //Face
            { SD.Attributes.Strength, SD.Attributes.Endurance, }, //Body
        };

        //circle 0  1  2  3  4  5  6  7  8  9
        public static readonly int[,,] SpellsPerDay = {
          {
            //WIZARD
             { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },               // lvl 0
             { 4, 2, 0, 0, 0, 0, 0, 0, 0, 0 },               // lvl 1
             { 4, 3, 2, 1, 0, 0, 0, 0, 0, 0 },               // lvl 2
             { 4, 4, 3, 3, 2, 0, 0, 0, 0, 0 },               // lvl 3
             { 4, 4, 4, 4, 3, 2, 1, 0, 0, 0 },               // lvl 4
             { 4, 4, 4, 4, 4, 4, 3, 2, 1, 0 },               // lvl 5
             { 4, 4, 4, 4, 4, 4, 4, 3, 3, 2 },               // lvl 6
             { 4, 4, 4, 4, 4, 4, 4, 4, 4, 4 },               // lvl 7
          }
            ,
          {
            //SORCERER
            {  0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },               // lvl 0
            { 99, 4, 0, 0, 0, 0, 0, 0, 0, 0 },               // lvl 1
            { 99, 6, 4, 0, 0, 0, 0, 0, 0, 0 },               // lvl 2
            { 99, 6, 6, 5, 3, 0, 0, 0, 0, 0 },               // lvl 3
            { 99, 6, 6, 6, 6, 4, 0, 0, 0, 0 },               // lvl 4
            { 99, 6, 6, 6, 6, 6, 5, 3, 0, 0 },               // lvl 5
            { 99, 6, 6, 6, 6, 6, 6, 6, 5, 3 },               // lvl 6
            { 99, 6, 6, 6, 6, 6, 6, 6, 6, 6,},               // lvl 7
          }
        } ;
      //circle 0  1  2  3  4  5  6  7  8  9
        public static readonly int[,,] SpellsKnown = {
          {
            //WIZARD
             { 4, 5, 0, 0, 0, 0, 0, 0, 0, 0 },               // lvl 1
             { 4, 5, 4, 2, 0, 0, 0, 0, 0, 0 },               // lvl 2
             { 4, 5, 4, 4, 4, 0, 0, 0, 0, 0 },               // lvl 3
             { 4, 5, 4, 4, 4, 4, 2, 0, 0, 0 },               // lvl 4
             { 4, 4, 4, 4, 4, 4, 4, 4, 2, 0 },               // lvl 5
             { 4, 4, 4, 4, 4, 4, 4, 4, 4, 4 },               // lvl 6
             { 4, 4, 4, 4, 4, 4, 4, 4, 4, 6 },               // lvl 7
          }
            ,
          {
            //SORCERER
            { 5, 2, 0, 0, 0, 0, 0, 0, 0, 0 },               // lvl 1
            { 6, 4, 2, 0, 0, 0, 0, 0, 0, 0 },               // lvl 2
            { 8, 5, 3, 2, 1, 0, 0, 0, 0, 0 },               // lvl 3
            { 9, 5, 5, 4, 3, 2, 0, 0, 0, 0 },               // lvl 4
            { 9, 5, 5, 4, 4, 4, 3, 2, 0, 0 },               // lvl 5
            { 9, 5, 5, 4, 4, 4, 3, 3, 2, 1 },               // lvl 6
            { 9, 5, 5, 4, 4, 4, 3, 3, 3, 3,},               // lvl 7
          }
        };
        //circle 0  1  2  3  4  5  6  7  8  9
        public static readonly int[,] AbilityModifBonusSpell = {
            { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },               // +0
            { 0, 1, 0, 0, 0, 0, 0, 0, 0, 0 },               // +1
            { 0, 1, 1, 0, 0, 0, 0, 0, 0, 0 },               // +2
            { 0, 1, 1, 1, 0, 0, 0, 0, 0, 0 },               // +3
            { 0, 1, 1, 1, 1, 0, 0, 0, 0, 0 },               // +4
            { 0, 2, 1, 1, 1, 1, 0, 0, 0, 0 },               // +5
            { 0, 2, 2, 1, 1, 1, 1, 0, 0, 0 },               // +6
            { 0, 2, 2, 2, 1, 1, 1, 1, 0, 0 },               // +7
            { 0, 2, 2, 2, 2, 1, 1, 1, 1, 0,},               // +8
            { 0, 3, 2, 2, 2, 2, 1, 1, 1, 1,},               // +9
        };

        public struct Month
        {
            public string Name;
            public int Number;
            public string Season;
            public int Days;
        }

        public readonly struct Calendar
        {
            public const int StartYear = 625;

            public static readonly Month[] Months = { 
                new(){Name = "Abadius",Number = 1,Season = "Winter",Days = 31 },
                new() { Name = "Calistril ", Number = 2, Season = "Winter", Days = 28},
                new() { Name = "Pharast ", Number = 3, Season = "Spring", Days = 31},
                new() { Name = "Gozran", Number = 4, Season = "Spring", Days = 30},
                new() { Name = "Desnus", Number = 5, Season = "Spring", Days = 31},
                new() { Name = "Sarenith", Number = 6, Season = "Summer", Days = 30},
                new() { Name = "Erastus", Number = 7, Season = "Summer", Days = 31},
                new() { Name = "Arodus", Number = 8, Season = "Summer", Days = 31},
                new() { Name = "Rova", Number = 9, Season = "Fall", Days = 30},
                new() { Name = "Lamashan", Number = 10, Season = "Fall", Days = 31},
                new() { Name = "Neth", Number = 11, Season = "Fall", Days = 30},
                new() { Name = "Kuthona", Number = 12, Season = "Winter", Days = 31}
            };

            public const string Moonday = "Moonday";
            public const string Toilday = "Toilday";
            public const string Wealday = "Wealday";
            public const string Oathday = "Oathday";
            public const string Fireday = "Fireday";
            public const string Starday = "Starday";
            public const string Sunday = "Sunday";

            public static readonly string[] AllWeek = { Moonday, Toilday, Wealday, Oathday, Fireday, Starday, Sunday };

            public static string GetDayOfWeek( int day, int month)
            {
                int days = day;
                for (int i = 0; i < month-1; i++)
                {
                    days += Months[i].Days;
                }
                int dayOfWeek = days % 7;

                return AllWeek[dayOfWeek];
            }

            public static string GetDate( int day, int month,int year = 0)
            {
                if (day == 0 || month == 0)
                    return "";
                while(day > Months[month - 1].Days)
                {
                    day = day - Months[month - 1].Days;
                    month++;
                }

                string dayOfWeek =  GetDayOfWeek(day, month);
                string dayNum;
                if (day == 1)
                    dayNum = "1st";
                else if(day == 2)
                {
                    dayNum = "2nd";
                }
                else
                {
                    dayNum = day.ToString() + "th";
                }
                if(year > 0)
                    return dayOfWeek + ", " + dayNum + " of " + Months[month + 1].Name + ", year " + year.ToString();

                return dayOfWeek + ", " + dayNum + " of " + Months[month + 1];
            }

            public Calendar()
            {
            }

        }

    }
    public static class MyIcon
    {
        public const string Bookmark = "icons/bookmarklet.svg";
        public const string Scroll = "icons/scroll.svg";
        public const string Quill = "icons/quill.svg";
        public const string Anvil_white = "icons/anvil_white.svg";
        public const string Helm_white = "icons/barbute_white.svg";
        public const string Anvil = "icons/anvil.svg";
        public const string Helm = "icons/barbute.svg";
    }


    public enum SpellcasterType
    {
        Wizard,
        Sorcerer,
        None,
    }

    public enum EquippedItems
    {
        Other,
        WeaponMain1,
        WeaponOff1,
        WeaponMain2,
        WeaponOff2,
        Shield, 
        Face,
        Throat, 
        Body, 
        Hands, 
        Waist, 
        Feet,
        Head,
        Shoulders,
        Torso, 
        Arms,
        Ring1,
        Ring2
    }
}
