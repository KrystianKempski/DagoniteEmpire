using System;
using System.Text;
using System.Text.RegularExpressions;
using MudBlazor;
using MudBlazor.Charts;

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

        public const string GameMaster_NPCName = "Game Master";
        public const string GameMaster_Portrait = "../images/gm_avatar.webp";
        public const string PostNoPortrait = "__no_portrait__";

        /// <summary>HttpOnly cookie so wiki static middleware can read the active hero (Blazor session storage is not available there).</summary>
        public const string WikiSelectedCharacterCookie = "dagonite_wiki_character_id";

        public static string Portrait = "portraits";
        public static string Icon = "icons";
        public static string PostImage = "postImages";
        public const string DefaultCharacterImage = "/images/def-char-img.webp";
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

        public readonly struct FightModifiers
        {
            /// <summary>+3 attack from behind (fight dialog checkbox). Part of full flanking (+5 with Surrounded 2v1). Not a persisted state.</summary>
            public const int FlankingAttackBonus = 3;

            /// <summary>Defence penalty per extra attacker (fight dialog Surrounded). Not a persisted state.</summary>
            public const int SurroundedDefencePenaltyPerExtra = 2;

            public const int SurroundedArmorPenaltyPerExtra = 1;

            /// <summary>Extra damage added to a confirmed critical hit.</summary>
            public const int CriticalHitDamageBonus = 8;

            /// <summary>Defence penalty applied to a Dead or Unconscious defender.</summary>
            public const int IncapacitatedDefencePenalty = 20;

            /// <summary>Added to the hit margin to form the DC of Bleeding / Blinded checks.</summary>
            public const int WoundConditionCheckOffset = 8;

            /// <summary>Base DC (before wound overflow) of the on-hit Unconscious check.</summary>
            public const int UnconsciousCheckBaseDc = 20;

            /// <summary>Base DC (before wound overflow) of the on-hit Dead check.</summary>
            public const int DeadCheckBaseDc = 30;

            /// <summary>
            /// When a mob's total wounds reach MaxWounds + this value (or more), the mob dies automatically.
            /// </summary>
            public const int MobDeathOverflowThreshold = 8;

            /// <summary>DC points per turn of resulting state duration: duration = max(1, DC / this).</summary>
            public const int StateDurationDcTier = 10;
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
                public static readonly string[] All = { Bows, Crossbows, Throwing, Slingshots, Javelins, Grenades, };
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
                public static readonly string[] All = { Pickpocketing, Lockpicking, DisarmingTraps, Tricks, Handcraft, };
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
            public const string WeaponMelee = "Weapon melee";
            public const string WeaponRanged = "Weapon ranged";
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
            public static readonly string[] All = { Other, WeaponMain1, WeaponOff1, WeaponMain2, WeaponOff2, Shield, Face, Throat, Body, Hands, Waist, Feet, Head, Shoulders, Torso, Arms, Ring1, Ring2 };
        }

        public readonly struct BasicWeaponsMelee
        {
            public const string Unarmed = "Unarmed";
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
            public const string Khopesh = "Khopesh";
            public const string Whip = "Whip";
            public const string WarClub = "War club";
            public const string Bardiche = "Bardiche";
            public const string LanceCavalry = "Lance, cavalry";
            public const string LanceInfantry = "Lance, infantry";
            public const string Greatsword = "Greatsword";
            public const string Halberd = "Halberd";
            public const string Billhook = "Billhook";
            public const string MainGauche = "Main gauche";
            public const string Staff = "Staff";
            public static readonly string[] All = { Unarmed, Dagger, LongSword, BattleAxe, Pickaxe, Mace, Morningstar, ShorSpear, Rapier, TwoHandedFlail, Warhammer, Greataxe, Poleaxe, Sarissa, Khopesh, Whip, WarClub, Bardiche, LanceCavalry, LanceInfantry, Greatsword, Halberd, Billhook, MainGauche, Staff };
        }
        public readonly struct BasicWeaponsShooting
        {
            public const string CrossbowLight = "Crossbow, light";
            public const string CrossbowHeavy = "Crossbow, heavy";
            public const string BowSimple = "Bow, simple";
            public const string Longbow = "Longbow";
            public const string Slingshot = "Slingshot";
            public const string Musket = "Musket";
            public const string Javelin = "Javelin";

            public static readonly string[] All = { CrossbowLight, CrossbowHeavy, BowSimple, Longbow, Slingshot, Musket, Javelin };
        }

        public readonly struct BasicShields
        {
            public const string WoodenBuckler = "Wooden buckler";
            public const string MetalBuckler = "Metal buckler";
            public const string WoodenShield = "Wooden shield";
            public const string MetalShield = "Metal shield";
            public const string BigWoodenShield = "Big wooden shield";
            public const string BigMetalShield = "Big metal shield";
            public const string Pavise = "Pavise";

            public static readonly string[] All = { WoodenBuckler, MetalBuckler, WoodenShield, MetalShield, BigWoodenShield, BigMetalShield, Pavise };
        }

        public readonly struct BasicArmors
        {
            public const string LightLeatherArmor = "Light leather armor";
            public const string LeatherScaleArmor = "Leather scale armor";
            public const string SteelScaleArmor = "Steel scale armor";
            public const string HalfPlate = "Half plate";
            public const string FullPlate = "Full plate";

            public static readonly string[] All = { LightLeatherArmor, LeatherScaleArmor, SteelScaleArmor, HalfPlate, FullPlate };
        }

        /// <summary>Items seeded by DbInitializer — used as templates when creating custom equipment.</summary>
        public readonly struct BasicEquipment
        {
            public static readonly string[] All =
            {
                BasicArmors.LightLeatherArmor, BasicArmors.LeatherScaleArmor, BasicArmors.SteelScaleArmor,
                BasicArmors.HalfPlate, BasicArmors.FullPlate,
                BasicShields.WoodenBuckler, BasicShields.MetalBuckler, BasicShields.WoodenShield,
                BasicShields.MetalShield, BasicShields.BigWoodenShield, BasicShields.BigMetalShield,
                BasicWeaponsMelee.Dagger, BasicWeaponsMelee.LongSword, BasicWeaponsMelee.BattleAxe,
                BasicWeaponsMelee.Pickaxe, BasicWeaponsMelee.Mace, BasicWeaponsMelee.Morningstar,
                BasicWeaponsMelee.ShorSpear, BasicWeaponsMelee.Rapier, BasicWeaponsMelee.TwoHandedFlail,
                BasicWeaponsMelee.Warhammer, BasicWeaponsMelee.Greataxe, BasicWeaponsMelee.Poleaxe,
                BasicWeaponsMelee.Sarissa, BasicWeaponsMelee.Khopesh, BasicWeaponsMelee.Whip,
                BasicWeaponsMelee.WarClub, BasicWeaponsMelee.Bardiche, BasicWeaponsMelee.LanceCavalry,
                BasicWeaponsMelee.LanceInfantry, BasicWeaponsMelee.Greatsword, BasicWeaponsMelee.Halberd,
                BasicWeaponsMelee.Billhook, BasicWeaponsMelee.MainGauche, BasicWeaponsMelee.Staff,
                BasicWeaponsShooting.CrossbowLight, BasicWeaponsShooting.CrossbowHeavy,
                BasicWeaponsShooting.BowSimple, BasicWeaponsShooting.Longbow, BasicWeaponsShooting.Slingshot,
                BasicWeaponsShooting.Musket, BasicWeaponsShooting.Javelin,
                BasicShields.Pavise,
            };

            public static readonly string[] TemplateTypes =
            {
                EquipmentType.WeaponMelee, EquipmentType.WeaponRanged, EquipmentType.Shield, EquipmentType.Body,
            };
        }



        public readonly struct Condition
        {
            public const string Nutrition = "Nutrition";
            public const string Cleanliness = "Cleanliness";
            public const string Wellbeing = "Well-being";
            public const string Rest = "Rest";
            public const string GeneralHealth = "General health";
            public static readonly string[] All = { Nutrition, Cleanliness, Wellbeing, Rest, GeneralHealth };
        }

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
        };
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

            public static string GetDayOfWeek(int day, int month)
            {
                int days = day;
                for (int i = 0; i < month - 1; i++)
                {
                    days += Months[i].Days;
                }
                int dayOfWeek = days % 7;

                return AllWeek[dayOfWeek];
            }

            public static string GetDate(int day, int month, int year = 0)
            {
                if (day == 0 || month == 0)
                    return "";
                while (day > Months[month - 1].Days)
                {
                    day = day - Months[month - 1].Days;
                    month++;
                }

                string dayOfWeek = GetDayOfWeek(day, month);
                string dayNum;
                if (day == 1)
                    dayNum = "1st";
                else if (day == 2)
                {
                    dayNum = "2nd";
                }
                else
                {
                    dayNum = day.ToString() + "th";
                }
                if (year > 0)
                    return dayOfWeek + ", " + dayNum + " of " + Months[month - 1].Name + ", year " + year.ToString();

                return dayOfWeek + ", " + dayNum + " of " + Months[month - 1].Name;
            }

            public Calendar()
            {
            }

        }

        public static readonly Dictionary<string, int> DifficultyLevel
             = new Dictionary<string, int>
         {
            { "Effortless", 5 },
            { "Simple", 8 },
            { "Straightforward", 12 },
            { "Demanding", 16 },
            { "Hard", 20 },
            { "Challanging", 25 },
            { "Very hard", 30 },
            { "Nearly impossible", 35 },
         };

        public static string GetDifficultyName(int diffLevel)
        {
            if (diffLevel <= 5) return "Effortless";
            if (diffLevel <= 8) return "Simple";
            if (diffLevel <= 12) return "Straightforward";
            if (diffLevel <= 16) return "Demanding";
            if (diffLevel <= 20) return "Hard";
            if (diffLevel <= 25) return "Challanging";
            if (diffLevel <= 30) return "Very hard";
            return "Nearly impossible";
        }

        public static Tuple<int, string> RollDice()
        {
            var roll = RollService.RollDice();
            return Tuple.Create(roll.Sum, roll.Text);
        }

        public static Tuple<bool, string> MakeRollTest(int DC, int skill)
        {
            var result = RollService.MakeRollTest(DC, skill);
            return Tuple.Create(result.Success, result.Text);
        }

        public static Tuple<bool, string> MakeRollTestPlain(int DC, int skill)
        {
            var result = MakeRollTest(DC, skill);
            var plain = result.Item2
                .Replace("<strong>", "", StringComparison.Ordinal)
                .Replace("</strong>", "", StringComparison.Ordinal);
            return Tuple.Create(result.Item1, plain);
        }

        /// <summary>Fight sequence roll test: plain totals, bold Success!/Fail! only.</summary>
        public static Tuple<bool, string> MakeRollTestForFight(int DC, int skill)
        {
            var result = MakeRollTestPlain(DC, skill);
            var text = result.Item2
                .Replace("Success!", RichText.BoldText("Success!"), StringComparison.Ordinal)
                .Replace("Fail!", RichText.BoldText("Fail!"), StringComparison.Ordinal);
            return Tuple.Create(result.Item1, text);
        }

        public static Tuple<bool, string> MakeOppositeRollTest(string name1, int skill1, string name2, int skill2)
        {
            var result = RollService.MakeOppositeRollTest(name1, skill1, name2, skill2);
            return Tuple.Create(result.FirstSideWins, result.Text);
        }

        public static Tuple<bool, string> MakeOppositeRollTest(string name1, List<Pair<string, int>> bonuses1, string name2, List<Pair<string, int>> bonuses2)
        {
            var result = RollService.MakeOppositeRollTest(name1, bonuses1, name2, bonuses2);
            return Tuple.Create(result.FirstSideWins, result.Text);
        }


        public static string BonusText(int value)
        {
            string res = " (";
            res += value >= 0 ? $"+{value.ToString()}" : value.ToString();
            res += ")";
            return res;
        }

        public static class Languages
        {
            public const string CategoryHuman = "Human languages";
            public const string CategoryRacial = "Racial";
            public const string CategoryExotic = "Exotic";

            // Every character knows the common language for free; it does not count toward the language slot pool.
            public const string CommonLanguageName = "wspólny";

            // A character always knows at least one language, so the pool can never drop below 1.
            public static int GetMaxSlots(int linguisticsValue) => Math.Max(1, 1 + linguisticsValue / 3);

            public static bool IsCommon(string? name) =>
                string.Equals(name, CommonLanguageName, StringComparison.OrdinalIgnoreCase);
        }
    }
    public class Pair<T, U>
    {
        public Pair()
        {
        }

        public Pair(T first, U second)
        {
            this.First = first;
            this.Second = second;
        }

        public T First { get; set; } = default!;
        public U Second { get; set; } = default!;
    };
    public static class MyIcon
    {
        public const string Bookmark = "icons/bookmarklet.svg";
        public const string BookmarkWhite = "icons/bookmarklet_white.svg";
        public const string Scroll = "icons/scroll.svg";
        public const string ScrollWhite = "icons/scroll_white.svg";
        public const string Quill = "icons/quill.svg";
        public const string Anvil_white = "icons/anvil_white.svg";
        public const string Helm_white = "icons/barbute_white.svg";
        public const string Anvil = "icons/anvil.svg";
        public const string Helm = "icons/barbute.svg";
        public const string Chest = "icons/chest.svg";
        public const string JewelCrownWhite = "icons/jewel-crown_white.svg";
        public const string JewelCrown = "icons/jewel-crown.svg";
        public const string Compass = "icons/compass.svg";
        public const string TwoCoins = "icons/two-coins.svg";
        public const string Trade = "icons/trade.svg";
        public const string WoodenCrate = "icons/wooden-crate.svg";
        public const string WoodCabinBlack = "icons/wood-cabin-black.svg";
        public const string GearHammer = "icons/gear-hammer.svg";
        public const string Crane = "icons/crane.svg";
        public const string WaxSeal = "icons/wax-seal.svg";
        public const string TiedScroll = "icons/tied-scroll.svg";
        public const string Goblin = "icons/goblin.svg";
        public const string Attack = "icons/sword-clash.svg";
        public const string AttackWhite = "icons/sword-clash-white.svg";
        public const string Unaware = "icons/unaware.svg";
        public const string Stunned = "icons/stunned.svg";
        public const string Snatched = "icons/snatched.svg";
        public const string Disarmed = "icons/disarmed.svg";
        public const string Stumbled = "icons/stumbled.svg";
        public const string Blinded = "icons/blinded.svg";
        public const string Invisible = "icons/invisible.svg";
        public const string Surrounded = "icons/surrounded.svg";
        public const string Unbalanced = "icons/unbalanced.svg";
        public const string Cautious = "icons/cautious.svg";
        public const string NoTurn = "icons/hourglass.svg";
        public const string HalfAction = "icons/half-action.svg";
        public const string FullDefence = "icons/full-defence.svg";
        public const string Bleeding = "icons/bleeding.svg";
        public const string Unconscious = "icons/unconscious.svg";
        public const string Dead = "icons/death-skull.svg";
        public const string TendedWound = "icons/tended-wound.svg";
        public const string FreshWound = "icons/fresh-wound.svg";
        public const string CustomIcon = "icons/uncertainty.svg";
        public const string Ability = "icons/bolt-spell-cast.svg";
        public const string Imperial = "icons/imperial.svg";
        public const string Tallar = "icons/tallar.svg";
        public const string Haller = "icons/haller.svg";
        public const string Copper = "icons/copper.svg";
        public const string Dice = "icons/dices.svg";
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
    public enum TurnLeft
    {
        No = 0,
        Half =1,
        Whole =2
    }

    public class Wounds
    {
        public readonly struct Severity
        {
            public const string Scars = "Scars";
            public const string Light = "Light";
            public const string Moderate = "Moderate";
            public const string Heavy = "Heavy";
            public const string Critical = "Critical";
            public const string Deadly = "Deadly";
            public static readonly string[] All = { Scars, Light, Moderate, Heavy, Critical, Deadly };
        }
        public readonly struct Location
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
            public static readonly string[] All = { Head, Neck, MainArm, OffArm, MainHand, OffHand, Back, LeftLeg, RightLeg, Face, Body };
        }
        public enum LocationEnum
        {
            Head, Neck, MainArm, OffArm, MainHand, OffHand, Back, LeftLeg, RightLeg, Face, Body
        }

        public static string RandomLocation()
        {
            Random rnd = new Random();
            int roll = rnd.Next(1, 100);
            if      (roll < 2) return Location.Face;
            else if (roll < 3) return Location.Neck;
            else if (roll < 6) return Location.Head;
            else if (roll < 16) return Location.MainArm;
            else if (roll < 26) return Location.OffArm;
            else if (roll < 31) return Location.MainHand;
            else if (roll < 36) return Location.OffHand;
            else if (roll < 46) return Location.LeftLeg;
            else if (roll < 56) return Location.RightLeg;
            else return Location.Body;
        }
        public static readonly string[,] Attributes = {
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
        public static string SeverityFromDmg(int value)
        {
            if (value <= 0)
                return "no";
            else if (value > 0 && value < 5)
                return Severity.Light;
            else if (value < 9)
                return Severity.Moderate;
            else if (value < 15)
                return Severity.Heavy;
            else if (value < 25)
                return Severity.Critical;
            else if (value >= 25)
                return Severity.Deadly;
            else
                return "";
        }

        /// <summary>
        /// Wounds at Light severity or higher are included in battle turn summaries.
        /// Scars, "no" wound, and empty severity are omitted.
        /// </summary>
        public static bool IsReportableInTurnSummary(string? severity)
        {
            if (string.IsNullOrWhiteSpace(severity))
                return false;

            var normalized = severity.Trim();
            if (string.Equals(normalized, "no", StringComparison.OrdinalIgnoreCase))
                return false;

            return normalized is Severity.Light or Severity.Moderate or Severity.Heavy
                or Severity.Critical or Severity.Deadly;
        }

        public static int DCFromSeverity(string value)
        {
            switch (value)
            {
                case Wounds.Severity.Light: return 7;
                case Wounds.Severity.Moderate: return 14;
                case Wounds.Severity.Heavy: return 21;
                case Wounds.Severity.Critical: return 28;
            }
            return 0;
        }
        public static int GetValueFromSeverity(string severity)
        {
            switch (severity)
            {
                case Wounds.Severity.Light: return 1;
                case Wounds.Severity.Moderate: return 3;
                case Wounds.Severity.Heavy: return 9;
                case Wounds.Severity.Critical: return 18;
                case Wounds.Severity.Deadly: return 25;
                default: return 0;
            }
        }

        /// <summary>
        /// Effective wound penalty applied to a health pool. A successful pain-resistance test
        /// (<paramref name="isIgnored"/> = true) reduces the penalty — same rules as <c>WoundDTO.Penalty</c>.
        /// </summary>
        public static int GetPenaltyFromValue(int value, bool isIgnored)
        {
            if (value > 0 && value < 3)
                return isIgnored ? 0 : 1;
            if (value >= 3 && value < 9)
                return isIgnored ? 1 : 3;
            if (value >= 9 && value < 18)
                return isIgnored ? 3 : 7;
            if (value >= 18 && value < 25)
                return isIgnored ? 5 : 12;
            if (value >= 25)
                return 20;

            return 0;
        }

        public static int GetPenaltyFromSeverity(string severity, bool isIgnored) =>
            GetPenaltyFromValue(GetValueFromSeverity(severity), isIgnored);
        public static string GetIcon(bool isTended)
        {
            return isTended ? MyIcon.TendedWound : MyIcon.FreshWound;
        }
    }

    public class ProtectedStorageKeys
    {
        public const string SelectedCharacterId = "SelectedCharacterId";
        public const string UserName = "UserName";
        public const string UserId = "UserId";
        public const string IsAdminOrMG = "IsAdminOrMG";
        public const string CharacterMG = "CharacterMG";
        public const string IsAuthenticated = "IsAuthenticated";
        public const string Role = "Role";
        public const string IsInited = "IsInited";
        public static readonly string[] All = { SelectedCharacterId, UserName, UserId, IsAdminOrMG, CharacterMG, IsAuthenticated, Role, IsInited };
    }; 

    public class States
    {
        public enum Level
        {
            Stunned = 10,
            Stumbled = 5,
            Snatched = 5,
            Disarmed = 0,
            Blinded = 8,
            Unaware = 10,
            Invisible = 5,
            Surrounded = 2,
            Unbalanced = 7,
            Cautious = 2,
            FullDefence = 5,
            Bleeding = 0,
            Unconscious = 20,
            Dead = 99,
            NoTurn = 0,
            HalfTurn = 0,
        }

        /// <summary>Canonical turn-count durations for combat states (kept here so fight code and the seeder agree).</summary>
        public readonly struct Duration
        {
            /// <summary>Lasts a single round.</summary>
            public const int SingleTurn = 1;

            /// <summary>Persists until the fighter acts it off (stands up, is released, bleeding is tended, ...).</summary>
            public const int UntilResolved = 99;

            /// <summary>Effectively permanent (death).</summary>
            public const int Permanent = 999;

            /// <summary>Default bleeding window when no explicit duration is given.</summary>
            public const int BleedingDefault = 10;
        }

        public readonly struct Names
        {
            public const string Stunned = "Stunned";
            public const string Stumbled = "Stumbled";
            public const string Snatched = "Snatched";
            public const string Disarmed = "Disarmed";
            public const string Blinded = "Blinded";
            public const string Unaware = "Unaware";
            public const string Invisible = "Invisible";
            public const string Surrounded = "Surrounded";
            public const string Unbalanced = "Unbalanced";
            public const string Cautious = "Cautious";
            public const string FullDefence = "Full defence";
            public const string Bleeding = "Bleeding";
            public const string Unconscious = "Unconscious";
            public const string Dead = "Dead";
            public const string NoTurn = "No turn";
            public const string HalfTurn = "Half turn";
            public static readonly string[] All = { Stunned, Stumbled, Snatched, Disarmed, Blinded, Unaware, Invisible, Surrounded, Unbalanced, Cautious, FullDefence, Bleeding, Unconscious, Dead, NoTurn ,HalfTurn};
        }
        public static int GetLevel(string name)
        {
            switch (name)
            {
                case Names.Stunned: return (int)Level.Stunned;
                case Names.Stumbled: return (int)Level.Stumbled;
                case Names.Snatched: return (int)Level.Snatched;
                case Names.Disarmed: return (int)Level.Disarmed;
                case States.Names.Blinded: return (int)Level.Blinded;
                case Names.Unaware: return (int)Level.Unaware;
                case Names.Invisible: return (int)Level.Invisible;
                case Names.Surrounded: return (int)Level.Surrounded;
                case Names.Unbalanced: return (int)Level.Unbalanced;
                case Names.Cautious: return (int)Level.Cautious;
                case Names.FullDefence: return (int)Level.FullDefence;
                case Names.Bleeding: return (int)Level.Bleeding;
                case Names.Unconscious: return (int)Level.Unconscious;
                case Names.Dead: return (int)Level.Dead;
                case Names.NoTurn: return (int)Level.NoTurn;
                case Names.HalfTurn: return (int)Level.HalfTurn;

            }
            return 0;
        }
        public static string GetIcon(string name)
        {
            switch (name)
            {
                case Names.Stunned: return MyIcon.Stunned;
                case Names.Stumbled: return MyIcon.Stumbled;
                case Names.Snatched: return MyIcon.Snatched;
                case Names.Disarmed: return MyIcon.Disarmed;
                case Names.Blinded: return MyIcon.Blinded;
                case Names.Unaware: return MyIcon.Unaware;
                case Names.Invisible: return MyIcon.Invisible;
                case Names.Surrounded: return MyIcon.Surrounded;
                case Names.Unbalanced: return MyIcon.Unbalanced;
                case Names.Cautious: return MyIcon.Cautious;
                case Names.FullDefence: return MyIcon.FullDefence;
                case Names.Bleeding: return MyIcon.Bleeding;
                case Names.Unconscious: return MyIcon.Unconscious;
                case Names.Dead: return MyIcon.Dead;
                case Names.NoTurn: return MyIcon.NoTurn;
                case Names.HalfTurn: return MyIcon.HalfAction;
                default: return MyIcon.CustomIcon;
            }
        }

    }

    public class RichText
    {
        public const string QuoteBlockClass = "rich-text-quote";
        public const string QuoteBlockOpenTag = $"<blockquote class=\"{QuoteBlockClass}\"><p>";
        public const string QuoteBlockCloseTag = "</p></blockquote>";

        public string AllText { get => _allText; set => _allText = value; }
        private string _allText;
        private string backgroundColor = "#eaeaea";
        private string textColor = "black";
        public RichText()
        {
            _allText = $"<div style=\"background-color: {backgroundColor};color: {textColor};\"><blockquote><p>";

        }

        public void EndText()
        {
            _allText += "</p></blockquote></div><br>";
        }

        /// <summary>Converts RichText blocks to Quill-compatible blockquote HTML.</summary>
        public string ToQuillHtml() => ToQuillHtml(_allText);

        public static string ToQuillHtml(string? richHtml)
        {
            if (string.IsNullOrEmpty(richHtml))
                return string.Empty;

            const string prefix = "<div style=\"background-color: #eaeaea;color: black;\"><blockquote><p>";
            const string suffix = "</p></blockquote></div><br>";
            var result = new StringBuilder();
            var remaining = richHtml;
            var converted = false;

            while (true)
            {
                var start = remaining.IndexOf(prefix, StringComparison.Ordinal);
                if (start < 0)
                    break;

                var end = remaining.IndexOf(suffix, start, StringComparison.Ordinal);
                if (end < 0)
                    break;

                if (start > 0)
                    result.Append(remaining[..start]);

                var inner = remaining.Substring(start + prefix.Length, end - start - prefix.Length);
                result.Append(QuoteBlockOpenTag).Append(inner).Append(QuoteBlockCloseTag);
                remaining = remaining.Substring(end + suffix.Length);
                converted = true;
            }

            if (!converted)
                return richHtml;

            result.Append(remaining);
            return result.ToString();
        }

        private static string WrapInQuoteBlock(string? html)
        {
            if (string.IsNullOrEmpty(html))
                return string.Empty;

            if (html.Contains(QuoteBlockClass, StringComparison.Ordinal)
                || html.Contains("<blockquote", StringComparison.OrdinalIgnoreCase))
                return html;

            return $"<blockquote class=\"{QuoteBlockClass}\">{html}</blockquote>";
        }

        /// <summary>Quote block HTML for pasting a saved roll batch into a thread post.</summary>
        public static string ToThreadPostQuillHtml(string? richHtml) =>
            WrapInQuoteBlock(ToQuillHtml(richHtml));

        /// <summary>Italic Quill HTML for displaying a fight sequence in the roll dialog editor.</summary>
        public static string ToFightQuillHtml(string? richHtml) => WrapQuillParagraphs(richHtml, wrapInner: static c => $"<em>{c}</em>");

        /// <summary>Paragraph HTML for Quill load + format-as-quote (no blockquote wrapper).</summary>
        public static string ToPlainEditorHtml(string? richHtml)
        {
            var html = ToQuillHtml(richHtml);
            if (string.IsNullOrEmpty(html))
                return string.Empty;

            return html
                .Replace($"<blockquote class=\"{QuoteBlockClass}\">", string.Empty, StringComparison.Ordinal)
                .Replace("</blockquote>", string.Empty, StringComparison.Ordinal);
        }

        /// <summary>Italic paragraph HTML for in-dialog Quill editors (no blockquote; Quill drops blockquotes on load).</summary>
        public static string ToFightEditorHtml(string? richHtml)
        {
            var html = ToFightQuillHtml(richHtml);
            if (string.IsNullOrEmpty(html))
                return string.Empty;

            return html
                .Replace($"<blockquote class=\"{QuoteBlockClass}\">", string.Empty, StringComparison.Ordinal)
                .Replace("</blockquote>", string.Empty, StringComparison.Ordinal);
        }

        /// <summary>Italic fight sequence in a quote block for pasting into a thread post.</summary>
        public static string ToThreadFightPostQuillHtml(string? richHtml) =>
            WrapInQuoteBlock(ToFightQuillHtml(richHtml));

        /// <summary>Merges multiple &lt;p&gt; blocks into one continuous line for a single blockquote.</summary>
        public static string CollapseToSingleParagraph(string? html)
        {
            if (string.IsNullOrEmpty(html))
                return string.Empty;

            var parts = new List<string>();
            var remaining = html;

            while (remaining.Contains("<p>", StringComparison.Ordinal))
            {
                var start = remaining.IndexOf("<p>", StringComparison.Ordinal);
                var end = remaining.IndexOf("</p>", start, StringComparison.Ordinal);
                if (end < 0)
                    break;

                var inner = StripEmWrapper(remaining.Substring(start + 3, end - start - 3).Trim());
                if (inner.Length > 0)
                    parts.Add(inner);

                remaining = remaining.Substring(end + 4);
            }

            if (parts.Count == 0)
                return html;

            var merged = string.Join(" ", parts);
            merged = Regex.Replace(merged, @"\s+", " ");

            var wrapInEm = html.Contains("<em>", StringComparison.OrdinalIgnoreCase);
            return wrapInEm ? $"<p><em>{merged}</em></p>" : $"<p>{merged}</p>";
        }

        private static string StripEmWrapper(string inner)
        {
            const string open = "<em>";
            const string close = "</em>";
            var trimmed = inner.Trim();
            if (trimmed.StartsWith(open, StringComparison.OrdinalIgnoreCase)
                && trimmed.EndsWith(close, StringComparison.OrdinalIgnoreCase))
            {
                return trimmed[open.Length..^close.Length].Trim();
            }

            return trimmed;
        }

        private static string WrapQuillParagraphs(string? richHtml, Func<string, string> wrapInner)
        {
            var inner = ToQuillHtml(richHtml);
            if (string.IsNullOrEmpty(inner))
                return string.Empty;

            var result = new StringBuilder();
            var remaining = inner;
            var wrappedAny = false;

            while (remaining.Contains("<p>", StringComparison.Ordinal))
            {
                var start = remaining.IndexOf("<p>", StringComparison.Ordinal);
                var end = remaining.IndexOf("</p>", start, StringComparison.Ordinal);
                if (end < 0)
                    break;

                if (!wrappedAny && start > 0)
                    result.Append(remaining[..start]);

                var paragraph = remaining.Substring(start + 3, end - start - 3);
                result.Append("<p>").Append(wrapInner(paragraph)).Append("</p>");
                remaining = remaining.Substring(end + 4);
                wrappedAny = true;
            }

            if (!wrappedAny)
                return inner;

            if (remaining.Length > 0)
                result.Append(remaining);

            return result.ToString();
        }

        public void NewLine()
        {
            _allText += "</p>";
            _allText += "<p>";
        }
        public static string BoldText(string text)
        {
            return "<strong>" + text + "</strong>";
        }
        public static string BoldText(int num)
        {
            return "<strong>" + num.ToString() + "</strong>";
        }
        public static string NumToStr(int value)
        {
            if (value < 0)
            {
                return $"{value}";
            }
            else
            {
                return $"+{value}";
            }
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
