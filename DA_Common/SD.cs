using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using MudBlazor;
using MudBlazor.Charts;
using Syncfusion.Blazor.RichTextEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using static MudBlazor.CategoryTypes;
using static MudBlazor.Colors;
using static MudBlazor.Icons.Custom;
using static Npgsql.PostgresTypes.PostgresCompositeType;
using static System.Formats.Asn1.AsnWriter;
using static System.Runtime.InteropServices.JavaScript.JSType;

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

        public const string TraitType_Character = "Character";
        public const string TraitType_Temporary = "Temporary";
        public const string TraitType_Race = "Race";
        public const string TraitType_Gear = "Gear";
        public const string TraitType_Profession = "Profession";
        public const string TraitType_Unique = "Unique";

        public const string NPCName_GameMaster = "Game Master";
        public readonly struct NPCType
        {
            public const string Hero = "Hero";
            public const string Duke = "Duke";
            public const string PC = "PC";
        }

        // Weapon qualities

        public const string WeaponParametersDescr = "Weapon parameters";
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
        public readonly struct AttackAction
        {
            public const string Normal = "Normal";
            public const string Cautious = "Cautious";
            public const string Raging = "Raging";
            public const string Strong = "Strong";
            public const string Targeted = "Targeted";
            public const string Charge = "Charge";
            public static readonly string[] All = { Normal, Cautious, Raging, Strong, Targeted, Charge };
        }

        public readonly struct DefenceType
        {
            public const string Dodge = "Dodge";
            public const string Parry = "Parry";
            public const string Shield = "Shield";
            public const string Armor = "Armor";
            public static readonly string[] All = { Dodge, Parry, Shield, Armor };
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
                public const string Unarmed = "Unarmed";                
                public static readonly string[] All = { Heavy, Swords, Fencing, Light, Shields, Polearms, Unarmed };
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
                public static readonly string[] All = { Bows, Crossbows, Throwing, Slingshots, Javelins, Grenades,  };
            }
            public readonly struct Acrobatics
            {
                public const string Jumping = "Jumping";
                public const string Climbing = "Climbing";
                public const string Balance = "Balance";
                public const string Running = "Running";
                public const string Dodge = "Dodge";
                public static readonly string[] All = { Jumping, Climbing, Balance, Running, Dodge, };
            }
            public readonly struct SleightOfHands
            {
                public const string Pickpocketing = "Pickpocketing";
                public const string Lockpicking = "Lockpicking";
                public const string DisarmingTraps = "Disarming traps";
                public const string Tricks = "Tricks";
                public const string Handcraft = "Handcraft";
                public static readonly string[] All = { Pickpocketing, Lockpicking, DisarmingTraps, Tricks, Handcraft,  };
            }
            public readonly struct Athletics
            {
                public const string Wrestling = "Wrestling";
                public const string Lifting = "Lifting";
                public const string Armor = "Armor";
                public const string Threatening = "Threatening";
                public const string PainResistance = "Pain Resistance";
                public const string Swimming = "Swimming";
                public static readonly string[] All = { Wrestling, Lifting, Armor, Threatening, PainResistance, Swimming, };
            }

            public static readonly string[] ArmorPenaltySkills = { Acrobatics.Jumping, Acrobatics.Climbing, Acrobatics.Balance, Acrobatics.Running, Acrobatics.Dodge,
                                                                SleightOfHands.Pickpocketing, "Sneak",Athletics.Swimming};
            public static readonly string[] All =
            {   Shooting.Bows, Shooting.Crossbows, Shooting.Throwing, Shooting.Slingshots, Shooting.Javelins, Shooting.Grenades,
                Acrobatics.Jumping, Acrobatics.Climbing, Acrobatics.Balance, Acrobatics.Running, Acrobatics.Dodge,
                SleightOfHands.Pickpocketing, SleightOfHands.Lockpicking, SleightOfHands.DisarmingTraps, SleightOfHands.Tricks, SleightOfHands.Handcraft,
                Athletics.Wrestling, Athletics.Lifting, Athletics.Armor, Athletics.Threatening, Athletics.PainResistance, Athletics.Swimming,

            };
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

        public readonly struct BasicWeaponsMelee
        {
            public const string Fists = "Fists";
            public const string Dagger = "Dagger";
            public const string LongSword = "Long sword";
            public const string BattleAxe = "Battle axe";
            public const string Pickaxe = "Pickaxe";
            public const string Mace = "Mace";
            public const string Morningstar = "Morningstar";
            public const string ShorSpear = "Short spear";
            public const string Rapier = "Rapier";
            public const string TwoHandedFlail = "Two-handed flail";
            public const string Warhammer = "Warhammer";
            public const string Greataxe = "Greataxe";
            public const string Poleaxe = "Poleaxe";
            public const string Sarissa = "Sarissa";
            public static readonly string[] All = { Fists,Dagger, LongSword, BattleAxe, Pickaxe, Mace, Morningstar, ShorSpear, Rapier, TwoHandedFlail, Warhammer, Greataxe, Poleaxe, Sarissa };
        }
        public readonly struct BasicWeaponsShooting
        {
            public const string CrossbowLight = "Crossbow, light";
            public const string CrossbowHeavy = "Crossbow, heavy";
            public const string BowSimple = "Bow, simple";
            public const string Longbow = "Longbow";
            public const string Slingshot = "Slingshot";

            public static readonly string[] All = { CrossbowLight, CrossbowHeavy, BowSimple, Longbow, Slingshot };
        }

        public readonly struct BasicShields
        {
            public const string WoodenBuckler = "Wooden buckler";
            public const string MetalBuckler = "Metal buckler";
            public const string WoodenShield = "Wooden shield";
            public const string MetalShield = "Metal shield";
            public const string BigWoodenShield = "Big wooden shield";
            public const string BigMetalShield = "Big metal shield";

            public static readonly string[] All = { WoodenBuckler, MetalBuckler, WoodenShield, MetalShield, BigWoodenShield, BigMetalShield };
        }

        public readonly struct BasicArmors
        {
            public const string LightLeatherArmor = "Light leather armor";
            public const string LeatherScaleArmor = "Leather scale armor";
            public const string StealScaleArmor = "Steal scale armor";
            public const string HalfPlate = "Half plate";
            public const string FullPlate = "Full plate";

            public static readonly string[] All = { LightLeatherArmor, LeatherScaleArmor, StealScaleArmor, HalfPlate, FullPlate };
        }

        public readonly struct WoundSeverity
        {
            public const string Scars = "Scars";
            public const string Light = "Light";
            public const string Moderate = "Moderate";
            public const string Heavy = "Heavy";
            public const string Critical = "Critical";
            public const string Deadly = "Deadly";
            public static readonly string[] All = { Scars, Light, Moderate, Heavy, Critical, Deadly };
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

        public readonly struct Condition
        {
            public const string Nutrition = "Nutrition";
            public const string Cleanliness = "Cleanliness";
            public const string Wellbeing = "Well-being";
            public const string Rest = "Rest";
            public const string GeneralHealth = "General health";
            public static readonly string[] All = { Nutrition, Cleanliness, Wellbeing, Rest, GeneralHealth };
        }

        public readonly struct TempStates
        {
            public const string Stunned = "Stunned";
            public const string Stumbled = "Stumbled";
            public const string Snatched = "Snatched";
            public const string Disarmed = "Disarmed";
            public const string Blinded = "Blinded";
            public const string Unaware = "Unaware";
            public const string Invisible = "Invisible";
            public const string Flanking = "Flanking";
            public const string Surrounded = "Surrounded";
            public const string Unbalanced = "Unbalanced";
            public const string Cautious = "Cautious";
            public const string FullDefence = "Full defence";
            public const string Bleeding = "Bleeding";

            public static readonly string[] All = { Stunned, Stumbled, Snatched, Disarmed, Blinded, Unaware, Invisible, Flanking, Surrounded, Unbalanced, Cautious, FullDefence, Bleeding };
        }

        
        //public readonly struct TempStatesLevel
        //{
        //    public const int Stunned = 10;
        //    public const int Stumbled = 5;
        //    public const int Snatched = 5;
        //    public const int Disarmed = 0;
        //    public const int Blinded = 8;
        //    public const int Unaware = 10;
        //    public const int Invisible = 5;
        //    public const int Flanking = 3;
        //    public const int Surrounded = 2;
        //    public const int Unbalanced = 7;
        //    public const int Cautious = 2;
        //    public const int FullDefence = 5;
        //    public const int Bleeding = 0;

        //    public static readonly int[] All = { Stunned, Stumbled, Snatched, Disarmed, Blinded, Unaware, Invisible, Flanking, Surrounded, Unbalanced, Cautious, FullDefence };
        //}



        public readonly struct ProfessionSkills
        {
            public const string DoubleWeaponFighting = "Double weapon fighting";
            public const string GreaterDoubleWeaponFighting = "Greater double weapon fighting";
            public const string MightyGrip = "Mighty grip";
            public const string WizardMagic = "Wizard magic";
            public const string SorcererMagic = "Sorcerer magic";
        }

        //circ 0  1  2  3  4  5  6  7  8  9
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
                new() {Name = "Abadius",Number = 1,Season = "Winter",Days = 31 },
                new() { Name = "Calistril", Number = 2, Season = "Winter", Days = 28},
                new() { Name = "Pharast", Number = 3, Season = "Spring", Days = 31},
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
                    return dayOfWeek + ", " + dayNum + " of " + Months[month - 1].Name + ", year " + year.ToString();

                return dayOfWeek + ", " + dayNum + " of " + Months[month- 1];
            }

            public Calendar()
            {
            }

        }
        public static Tuple<int, string> RollDice()
        {
            int result = 0;
            string text = string.Empty;
            Random rnd = new Random();
            int[] dice = { 1, 1, 1 };
            if (true)
            {
                dice[0] = rnd.Next(1, 6);  // creates a number between 1 and 6
                dice[1] = rnd.Next(1, 6);
                dice[2] = rnd.Next(1, 6);

                result = dice.Sum();
                text = $"(3d6: {dice[0].ToString()}+{dice[1].ToString()}+{dice[2].ToString()}={RichText.BoldText(result)})";
            }
            return Tuple.Create(result, text);
        }
        public static Tuple<bool, string> MakeRollTest(int DC,int skill)
        {
            bool result = false;
            string text = string.Empty;
            var roll = RollDice();
            result = skill+roll.Item1 >= DC;
            var sucess = result ? "Sucess!" : "Fail!";
            text = $"{skill} + {roll.Item2} is {skill + roll.Item1} vs DC: {DC}. {RichText.BoldText(sucess)}";

            return Tuple.Create(result, text);
        }

        public static string BonusText(int value)
        {
            string res = " (";
            res += value >= 0 ? $"+{value.ToString()}" : value.ToString();
            res += ")";
            return res;
        }
        public static string WoundSeverityFromDmg(int value)
        {
            if (value <= 0)
                return "no";
            else if (value > 0 && value < 5)
                return SD.WoundSeverity.Light;
            else if (value < 9)
                return SD.WoundSeverity.Moderate;
            else if (value < 15)
                return SD.WoundSeverity.Heavy;
            else if (value < 25)
                return SD.WoundSeverity.Critical;
            else if (value >= 25)
                return SD.WoundSeverity.Deadly;
            else
                return "";
        }
        public static int DCFromWoundSeverity(string value)
        {
            switch (value)
            {
                case SD.WoundSeverity.Light: return 7;
                case SD.WoundSeverity.Moderate: return 14;
                case SD.WoundSeverity.Heavy: return 21;
                case SD.WoundSeverity.Critical: return 28;
            }    
            return 0;
        }
        public static int GetValueFromSeverity(string severity)
        {
            switch (severity)
            {
                case SD.WoundSeverity.Light: return 1;
                case SD.WoundSeverity.Moderate: return 3;
                case SD.WoundSeverity.Heavy: return 9;
                case SD.WoundSeverity.Critical: return 18;
                case SD.WoundSeverity.Deadly: return 25;
                default: return 0;
            }
        }
        public static int GetTempStatesLevel(string name)
        {
            switch (name)
            {
                case SD.TempStates.Stunned: return (int)TempStatesLevel.Stunned;
                case SD.TempStates.Stumbled: return (int)TempStatesLevel.Stumbled;
                case SD.TempStates.Snatched: return (int)TempStatesLevel.Snatched;
                case SD.TempStates.Disarmed: return (int)TempStatesLevel.Disarmed;
                case SD.TempStates.Blinded: return (int)TempStatesLevel.Blinded;
                case SD.TempStates.Unaware: return (int)TempStatesLevel.Unaware;
                case SD.TempStates.Invisible: return (int)TempStatesLevel.Invisible;
                case SD.TempStates.Flanking: return (int)TempStatesLevel.Flanking;
                case SD.TempStates.Surrounded: return (int)TempStatesLevel.Surrounded;
                case SD.TempStates.Unbalanced: return (int)TempStatesLevel.Unbalanced;
                case SD.TempStates.Cautious: return (int)TempStatesLevel.Cautious;
                case SD.TempStates.FullDefence: return (int)TempStatesLevel.FullDefence;
                case SD.TempStates.Bleeding: return (int)TempStatesLevel.Bleeding;
            }
            return 0;
        }
        
    }
    public static class MyIcon
    {
        public const string Bookmark = "icons/bookmarklet.svg";
        public const string BookmarkWhite = "icons/bookmarklet_white.svg";
        public const string Scroll = "icons\\scroll.svg";
        public const string Quill = "icons/quill.svg";
        public const string Anvil_white = "icons/anvil_white.svg";
        public const string Helm_white = "icons/barbute_white.svg";
        public const string Anvil = "icons/anvil.svg";
        public const string Helm = "icons/barbute.svg";
        public const string Chest = "icons/chest.svg";
        public const string Goblin = "icons\\goblin.svg";
        public const string Attack = "icons/sword-clash.svg";
    }

    public enum Relation
    {
        Teammate,
        Ally,
        Neutral,
        Enemy,
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

    public enum Nutrition
    {
        BalancedDiet = 2,
        Fueled =0,
        Hungry=-2,
        Starving=-4,
        Malnourished=-8,
    }
    public enum Cleanliness
    {
        Groomed = 2,
        Clean = 0,
        Dirty = -2,
        Filthy = -4,
        Defiled = -8,
    }
    public enum Wellbeing
    {
        Joyous = 2,
        Content = 0,
        Worried = -2,
        Despaired = -4,
        Broken = -8,
    }
    public enum Rest
    {
        WellRested = 2,
        Rested = 0,
        Tired = -2,
        Exhausted = -4,
        LastBreath = -8,
    }
    public enum GeneralHealth
    {
        GreatHealth = 2,
        Stable = 0,
        Unwell = -2,
        Sick = -4,
        Dying = -8,
    }
    public enum TempStatesLevel
    {
        Stunned = 10,
        Stumbled = 5,
        Snatched = 5,
        Disarmed = 0,
        Blinded = 8,
        Unaware = 10,
        Invisible = 5,
        Flanking = 3,
        Surrounded = 2,
        Unbalanced = 7,
        Cautious = 2,
        FullDefence = 5,
        Bleeding = 0,
    }

    public class RichText
    {
        public string AllText { get => _allText; set => _allText = value; }
        private string _allText;
        public RichText()
        {
            _allText = "<p><em>(";
        }

        public void EndText()
        {
            _allText += ")</em></em>";
        }
        public void NewLine()
        {
            _allText += "</em></em>";
            _allText += "<p><em>";
        }
        public static string BoldText(string text)
        {
            return "<strong>" + text + "</strong>";
        }
        public static string BoldText(int num)
        {
            return "<strong>" + num.ToString() + "</strong>";
        }

        public static RichText operator+(RichText left, RichText right)
        {
            left._allText += right._allText;
            return left;
        }
        public static RichText operator +(RichText left, string right)
        {
            left._allText += right;
            return left;
        }
        public override string ToString()
        {
            return _allText;
        }
    }

}
