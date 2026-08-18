using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using DA_Common.Localization;
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

        /// <summary>Hidden Identity account used by the public "Try baron" demo (DukePlayer role, no interactive login).</summary>
        public const string DemoBaronUserName = "DemoBaron";
        public const string DemoBaronEmail = "demo-baron@dagonite.local";
        /// <summary>Hidden Identity account used by the public "Try Game Master" demo (GameMaster role, no interactive login).</summary>
        public const string DemoGmUserName = "DemoGM";
        public const string DemoGmEmail = "demo-gm@dagonite.local";
        /// <summary>Source character cloned per demo session (seeded by <c>EnsureGenericDemoBaronAsync</c>).</summary>
        public const string DemoBaronSourceCharacterName = "Aldric Emberfall";
        /// <summary>Owner of the persistent demo-baron template character; kept distinct from <see cref="DemoBaronUserName"/> so it never collides with per-session clones.</summary>
        public const string DemoBaronTemplateUserName = "DemoBaronTemplate";
        /// <summary>Abandoned demo sessions older than this are swept from the database.</summary>
        public static readonly TimeSpan DemoSessionTtl = TimeSpan.FromMinutes(2);

        /// <summary>True for any hidden demo account (baron or GM), used to isolate demo sessions in shared UI.</summary>
        public static bool IsDemoUserName(string? userName) =>
            userName == DemoBaronUserName || userName == DemoGmUserName;

        /// <summary>
        /// Real Admin/GM who may list and open every character. Hidden demo accounts never qualify —
        /// they may only use the throwaway baron cloned for their browser session.
        /// </summary>
        public static bool HasGlobalCharacterAccess(string? userName, bool isAdminOrMg) =>
            isAdminOrMg && !IsDemoUserName(userName);

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

            /// <summary>
            /// Maps a stored weapon-quality name (English key or legacy Polish alias) to the canonical English key.
            /// Unknown values are returned trimmed, unchanged.
            /// </summary>
            public static string Canonical(string? name) => CatalogKey.Resolve(name, CanonicalMap);

            private static readonly Dictionary<string, string> CanonicalMap = CatalogKey.BuildMap(All, new Dictionary<string, string>
            {
                // Wiki „Zasady walki” / Broń — cechy (plus rodzaj żeński z przykładów)
                ["szybki"] = Fast,
                ["szybka"] = Fast,
                ["powolny"] = Slow,
                ["powolna"] = Slow,
                ["parujący"] = Parrying,
                ["parująca"] = Parrying,
                ["niszczący tarczę"] = ShieldDestructive,
                ["niszcząca tarcze"] = ShieldDestructive,
                ["przebijający"] = ArmorPiercing,
                ["przebijająca"] = ArmorPiercing,
                ["przebijająca pancerz"] = ArmorPiercing,
                ["długi"] = Long,
                ["długa"] = Long,
                ["ciężki"] = Heavy,
                ["ciężka"] = Heavy,
                ["druzgocący"] = Devastating,
                ["druzgocąca"] = Devastating,
                ["niszczycielska"] = Devastating,
                ["słaby"] = Weak,
                ["słaba"] = Weak,
                ["ogłuszający"] = Stunning,
                ["ogłuszająca"] = Stunning,
                ["potykający"] = Stumbling,
                ["potykająca"] = Stumbling,
                ["przewracająca"] = Stumbling,
                ["pochwycająca"] = Snatching,
                ["pochwycający"] = Snatching,
                ["szarpiąca"] = Snatching,
                ["rozbrajająca"] = Disarming,
                ["rozbrajający"] = Disarming,
                ["pancerz"] = Armor,
                ["ochrona"] = Armor,
                ["bonus obrony pancerzem"] = ArmorDefenceBonus,
                ["premia do obrony pancerza"] = ArmorDefenceBonus,
                ["kara (akrobatyka)"] = ArmorPenalty,
                ["kara do akrobatyki"] = ArmorPenalty,
                ["kara pancerza"] = ArmorPenalty,
                ["trwałość"] = Durability,
                ["bonus obrony"] = ShieldDefenceBonus,
                ["premia do obrony tarczą"] = ShieldDefenceBonus,
                ["niewygodny"] = Bulky,
                ["niewygodna"] = Bulky,
                ["nieporęczna"] = Bulky,
                ["celny"] = Precise,
                ["celna"] = Precise,
                ["precyzyjna"] = Precise,
                ["zasięg"] = Range,
                ["lekka"] = Light,
                ["przeładowanie"] = Reload,
            });
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

            public static string Canonical(string? name) => CatalogKey.Resolve(name, CanonicalMap);

            private static readonly Dictionary<string, string> CanonicalMap = CatalogKey.BuildMap(All, new Dictionary<string, string>
            {
                ["normalny"] = Normal,
                ["ostrozny"] = Cautious,
                ["ostrożny"] = Cautious,
                ["szalony"] = Raging,
                ["silny"] = Strong,
                ["celowany"] = Targeted,
                ["szarza"] = Charge,
                ["szarża"] = Charge,
            });
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

            /// <summary>
            /// Maps a stored attribute name (English key or legacy Polish alias) to the canonical English key.
            /// Unknown values are returned trimmed, unchanged.
            /// </summary>
            public static string Canonical(string? name) => CatalogKey.Resolve(name, CanonicalMap);

            private static readonly Dictionary<string, string> CanonicalMap = CatalogKey.BuildMap(All, new Dictionary<string, string>
            {
                ["siła"] = Strength,
                ["zręczność"] = Dexterity,
                ["wytrzymałość"] = Endurance,
                ["inteligencja"] = Intelligence,
                ["instynkt"] = Instinct,
                ["siła woli"] = Willpower,
                ["charyzma"] = Charisma,
            });
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

            /// <summary>
            /// Maps a stored base-skill name (English key or legacy Polish alias) to the canonical English key.
            /// Unknown values are returned trimmed, unchanged.
            /// </summary>
            public static string Canonical(string? name) => CatalogKey.Resolve(name, CanonicalMap);

            private static readonly Dictionary<string, string> CanonicalMap = CatalogKey.BuildMap(All, new Dictionary<string, string>
            {
                ["walka wręcz"] = Melee,
                ["strzelanie"] = Shooting,
                ["strzelectwo"] = Shooting,
                ["akrobatyka"] = Acrobatics,
                ["zręczność rąk"] = SleightOfHands,
                ["zwinne dłonie"] = SleightOfHands,
                ["atletyka"] = Athletics,
                ["rozmowa"] = Talk,
                ["oszustwo"] = Deceit,
                ["podstęp"] = Deceit,
                ["percepcja"] = Perception,
                ["spostrzegawczość"] = Perception,
                ["wiedza"] = Knowledge,
                ["rzemiosło"] = Craft,
                ["przetrwanie"] = Survival,
                ["obchodzenie się ze zwierzętami"] = AnimalHandle,
                ["obchodzenie ze zwierzętami"] = AnimalHandle,
                ["medycyna"] = Medicine,
            });
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
                public const string Exotic = "Exotic weapons";
                public static readonly string[] All = { Heavy, Swords, Fencing, Light, Shields, Polearms, Unarmed, Exotic };
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
                public static readonly string[] All = { Bows, Crossbows, Throwing, Slingshots, Javelins, Firearms, Grenades };
            }
            public readonly struct Acrobatics
            {
                public const string Jumping = "Jumping";
                public const string Climbing = "Climbing";
                public const string Balance = "Balance";
                public const string Running = "Running";
                public const string Dodge = "Dodge";
                public static readonly string[] All = { Jumping, Climbing, Balance, Running, Dodge };
            }
            public readonly struct SleightOfHands
            {
                public const string Pickpocketing = "Pickpocketing";
                public const string Lockpicking = "Lockpicking";
                public const string DisarmingTraps = "Disarming traps";
                public const string Tricks = "Tricks";
                public const string Handcraft = "Handcraft";
                public static readonly string[] All = { Pickpocketing, Lockpicking, DisarmingTraps, Tricks, Handcraft };
            }
            public readonly struct Athletics
            {
                public const string Wrestling = "Wrestling";
                public const string Lifting = "Lifting";
                public const string Armor = "Armor";
                public const string Threatening = "Threatening";
                public const string PainResistance = "Pain Resistance";
                public const string Swimming = "Swimming";
                public static readonly string[] All = { Wrestling, Lifting, Armor, Threatening, PainResistance, Swimming };
            }
            public readonly struct Talk
            {
                public const string Persuasion = "Persuasion";
                public const string Bluff = "Bluff";
                public const string Acting = "Acting";
                public const string PublicSpeech = "Public speech";
                public const string Inspire = "Inspire";
                public const string Diplomacy = "Diplomacy";
                public const string Trade = "Trade";
                public static readonly string[] All = { Persuasion, Bluff, Acting, PublicSpeech, Inspire, Diplomacy, Trade };
            }
            public readonly struct Deceit
            {
                public const string Sneak = "Sneak";
                public const string Gambling = "Gambling";
                public const string DirtyTricks = "Dirty tricks";
                public const string Investigation = "Investigation";
                public const string Disguise = "Disguise";
                public const string Intimidate = "Intimidate";
                public static readonly string[] All = { Sneak, Gambling, DirtyTricks, Investigation, Disguise, Intimidate };
            }
            public readonly struct Perception
            {
                public const string Observation = "Observation";
                public const string SenseMotives = "Sense motives";
                public const string Hearing = "Hearing";
                public const string Smell = "Smell";
                public const string Vigilance = "Vigilance";
                public static readonly string[] All = { Observation, SenseMotives, Hearing, Smell, Vigilance };
            }
            public readonly struct Knowledge
            {
                public const string HistoryAndReligion = "History and religion";
                public const string Beasts = "Beasts";
                public const string Linguistics = "Linguistics";
                public const string RacesAndNations = "Races and nations";
                public const string Geography = "Geography";
                public const string PlantsAndMushrooms = "Plants and mushrooms";
                public const string Heraldry = "Heraldry";
                public const string MathematicsAndLogic = "Mathematics and logic";
                public const string AlchemyAndPhysics = "Alchemy and physics";
                public const string StrategyAndTactics = "Strategy and tactics";
                public static readonly string[] All = { HistoryAndReligion, Beasts, Linguistics, RacesAndNations, Geography, PlantsAndMushrooms, Heraldry, MathematicsAndLogic, AlchemyAndPhysics, StrategyAndTactics };
            }
            public readonly struct Craft
            {
                public const string ArchitectureAndStonemasonry = "Architecture and stonemasonry";
                public const string GeologyAndMining = "Geology and mining";
                public const string MetallurgyAndBlacksmithing = "Metallurgy and blacksmithing";
                public const string EngineeringAndGunsmithing = "Engineering and gunsmithing";
                public const string ShipbuildingAndCarpentry = "Shipbuilding and carpentry";
                public const string FineArts = "Fine arts";
                public static readonly string[] All = { ArchitectureAndStonemasonry, GeologyAndMining, MetallurgyAndBlacksmithing, EngineeringAndGunsmithing, ShipbuildingAndCarpentry, FineArts };
            }
            public readonly struct Survival
            {
                public const string Tracking = "Tracking";
                public const string SenseOfDirection = "Sense of direction";
                public const string Trapping = "Trapping";
                public const string WildernessKnowledge = "Wilderness knowledge";
                public const string Sailing = "Sailing";
                public static readonly string[] All = { Tracking, SenseOfDirection, Trapping, WildernessKnowledge, Sailing };
            }
            public readonly struct Medicine
            {
                public const string Surgery = "Surgery";
                public const string TendWounds = "Tend wounds";
                public const string Diseases = "Diseases";
                public const string TendBeasts = "Tend beasts";
                public const string PoisonsAndVenoms = "Poisons and venoms";
                public const string Torture = "Torture";
                public static readonly string[] All = { Surgery, TendWounds, Diseases, TendBeasts, PoisonsAndVenoms, Torture };
            }
            public readonly struct AnimalHandle
            {
                public const string Training = "Training";
                public const string Taming = "Taming";
                public const string Riding = "Riding";
                public const string AnimalsCare = "Animals care";
                public static readonly string[] All = { Training, Taming, Riding, AnimalsCare };
            }

            public static readonly string[] ArmorPenaltySkills = { Acrobatics.Jumping, Acrobatics.Climbing, Acrobatics.Balance, Acrobatics.Running, Acrobatics.Dodge,
                                                                SleightOfHands.Pickpocketing, Deceit.Sneak, Athletics.Swimming };
            public static readonly string[] All = Melee.All
                .Concat(Shooting.All)
                .Concat(Acrobatics.All)
                .Concat(SleightOfHands.All)
                .Concat(Athletics.All)
                .Concat(Talk.All)
                .Concat(Deceit.All)
                .Concat(Perception.All)
                .Concat(Knowledge.All)
                .Concat(Craft.All)
                .Concat(Survival.All)
                .Concat(Medicine.All)
                .Concat(AnimalHandle.All)
                .ToArray();

            /// <summary>
            /// Maps a stored special-skill name (English key or legacy Polish alias) to the canonical English key.
            /// Unknown values (including custom editable skill names) are returned trimmed, unchanged.
            /// </summary>
            public static string Canonical(string? name) => CatalogKey.Resolve(name, CanonicalMap);

            private static readonly Dictionary<string, string> CanonicalMap = CatalogKey.BuildMap(All, new Dictionary<string, string>
            {
                ["broń ciężka"] = Melee.Heavy,
                ["ciężka broń"] = Melee.Heavy,
                ["miecze i szable"] = Melee.Swords,
                ["broń fechtunkowa"] = Melee.Fencing,
                ["broń lekka"] = Melee.Light,
                ["lekka broń"] = Melee.Light,
                ["tarcze"] = Melee.Shields,
                ["broń drzewcowa"] = Melee.Polearms,
                ["bez broni"] = Melee.Unarmed,
                ["broń egzotyczna"] = Melee.Exotic,
                ["egzotyczna"] = Melee.Exotic,
                ["łuki"] = Shooting.Bows,
                ["kusze"] = Shooting.Crossbows,
                ["broń miotana"] = Shooting.Throwing,
                ["proce"] = Shooting.Slingshots,
                ["oszczepy"] = Shooting.Javelins,
                ["broń palna"] = Shooting.Firearms,
                ["granaty"] = Shooting.Grenades,
                ["skoki"] = Acrobatics.Jumping,
                ["wspinaczka"] = Acrobatics.Climbing,
                ["równowaga"] = Acrobatics.Balance,
                ["bieg"] = Acrobatics.Running,
                ["unik"] = Acrobatics.Dodge,
                ["kieszonkostwo"] = SleightOfHands.Pickpocketing,
                ["otwieranie zamków"] = SleightOfHands.Lockpicking,
                ["rozbrajanie pułapek"] = SleightOfHands.DisarmingTraps,
                ["sztuczki"] = SleightOfHands.Tricks,
                ["rękodzieło"] = SleightOfHands.Handcraft,
                ["zapasy"] = Athletics.Wrestling,
                ["podnoszenie"] = Athletics.Lifting,
                ["pancerz"] = Athletics.Armor,
                ["grożenie"] = Athletics.Threatening,
                ["odporność na ból"] = Athletics.PainResistance,
                ["pływanie"] = Athletics.Swimming,
                ["perswazja"] = Talk.Persuasion,
                ["blef"] = Talk.Bluff,
                ["aktorstwo"] = Talk.Acting,
                ["przemawianie publiczne"] = Talk.PublicSpeech,
                ["inspirowanie"] = Talk.Inspire,
                ["dyplomacja"] = Talk.Diplomacy,
                ["handel"] = Talk.Trade,
                ["skradanie"] = Deceit.Sneak,
                ["hazard"] = Deceit.Gambling,
                ["brudne zagrania"] = Deceit.DirtyTricks,
                ["śledztwo"] = Deceit.Investigation,
                ["maskowanie"] = Deceit.Disguise,
                ["zastraszanie"] = Deceit.Intimidate,
                ["obserwacja"] = Perception.Observation,
                ["wyczucie motywów"] = Perception.SenseMotives,
                ["słuch"] = Perception.Hearing,
                ["węch"] = Perception.Smell,
                ["czujność"] = Perception.Vigilance,
                ["historia i religia"] = Knowledge.HistoryAndReligion,
                ["bestie"] = Knowledge.Beasts,
                ["lingwistyka"] = Knowledge.Linguistics,
                ["rasy i narody"] = Knowledge.RacesAndNations,
                ["geografia"] = Knowledge.Geography,
                ["rośliny i grzyby"] = Knowledge.PlantsAndMushrooms,
                ["heraldyka"] = Knowledge.Heraldry,
                ["matematyka i logika"] = Knowledge.MathematicsAndLogic,
                ["alchemia i fizyka"] = Knowledge.AlchemyAndPhysics,
                ["strategia i taktyka"] = Knowledge.StrategyAndTactics,
                ["architektura i kamieniarstwo"] = Craft.ArchitectureAndStonemasonry,
                ["geologia i górnictwo"] = Craft.GeologyAndMining,
                ["metalurgia i kowalstwo"] = Craft.MetallurgyAndBlacksmithing,
                ["inżynieria i rusznikarstwo"] = Craft.EngineeringAndGunsmithing,
                ["szkutnictwo i ciesielstwo"] = Craft.ShipbuildingAndCarpentry,
                ["sztuki piękne"] = Craft.FineArts,
                ["tropienie"] = Survival.Tracking,
                ["orientacja w terenie"] = Survival.SenseOfDirection,
                ["pułapkowanie"] = Survival.Trapping,
                ["wiedza o dziczy"] = Survival.WildernessKnowledge,
                ["żeglarstwo"] = Survival.Sailing,
                ["chirurgia"] = Medicine.Surgery,
                ["opieka nad ranami"] = Medicine.TendWounds,
                ["choroby"] = Medicine.Diseases,
                ["leczenie zwierząt"] = Medicine.TendBeasts,
                ["trucizny i jady"] = Medicine.PoisonsAndVenoms,
                ["tortury"] = Medicine.Torture,
                ["szkolenie"] = AnimalHandle.Training,
                ["oswajanie"] = AnimalHandle.Taming,
                ["jazda konna"] = AnimalHandle.Riding,
                ["pielęgnacja zwierząt"] = AnimalHandle.AnimalsCare,
            });
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

            /// <summary>
            /// Maps a stored equipment-type name (English key or legacy Polish alias) to the canonical English key.
            /// Unknown values are returned trimmed, unchanged.
            /// </summary>
            public static string Canonical(string? name) => CatalogKey.Resolve(name, CanonicalMap);

            private static readonly Dictionary<string, string> CanonicalMap = CatalogKey.BuildMap(All, new Dictionary<string, string>
            {
                ["inne"] = Other,
                ["broń wręcz"] = WeaponMelee,
                ["bron wrecz"] = WeaponMelee,
                ["broń dystansowa"] = WeaponRanged,
                ["tarcza"] = Shield,
                ["twarz"] = Face,
                ["szyja"] = Throat,
                ["ciało"] = Body,
                ["dłonie"] = Hands,
                ["pas"] = Waist,
                ["stopy"] = Feet,
                ["głowa"] = Head,
                ["ramiona"] = Shoulders,
                ["tułów"] = Torso,
                ["ręce"] = Arms,
                ["pierścienie"] = Rings,
            });
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

            /// <summary>Catalog item names including Unarmed and seeded gear (templates in <see cref="All"/> omit Unarmed).</summary>
            public static readonly string[] Names = BasicWeaponsMelee.All
                .Concat(BasicWeaponsShooting.All)
                .Concat(BasicShields.All)
                .Concat(BasicArmors.All)
                .Concat(new[] { "Bandage", "Wound balm", "Rope" })
                .Distinct()
                .ToArray();

            /// <summary>Seeded English description / short-description strings (display keys).</summary>
            public static readonly string[] CatalogDescriptions =
            {
                "20 feet of strong rope",
                "A truly devastating weapon",
                "Axe head on long pole",
                "Basic weapon of all soldiers",
                "Best protection there is",
                "Bigger for better protection",
                "Black powder firearm",
                "Common tool of hunters",
                "Curved exotic blade",
                "Easy to use and slow to reload",
                "Fast and elegant weapon",
                "Flexible reach weapon",
                "For dressing wounds",
                "Good for penetrating armor",
                "Good protection of solid steel",
                "Heavy and slow, but easy to knock down an opponent",
                "Heavy bludgeoning weapon",
                "Heavy infantry lance",
                "Heavy pole axe with long reach",
                "Helps with healing wounds. 20 doses, +2 to tending wounds. 1 dose for light and medium wounds, 2 for heavy, and 4 for critical",
                "Hooked polearm for pulling and tripping",
                "Lance designed for mounted combat",
                "Large stationary shield",
                "Large stationary shield. −2 to fight tests; can provide full cover in some situations",
                "Large two-handed sword",
                "Light but sturdy",
                "Main tool of all adventurers",
                "Military archers' primary weapon",
                "Offers good protection and mobility",
                "One handed and good way to stun opponent",
                "Parrying dagger for off-hand use",
                "Powerful but slow",
                "Powerful weapon that can easily stun the enemy",
                "Punches, kicks, bites, and other unarmed attacks",
                "Simple and deadly",
                "Simple but effective",
                "Simple wooden staff",
                "Simple, wooden shield",
                "Small and deadly",
                "Small, but better than nothing",
                "Strong, metal shield",
                "Thrown spear",
                "Versatile polearm with axe blade",
                "Very long spear",
                "Weapon of heavily armed knights",
            };

            /// <summary>
            /// Maps a stored catalog item name (English key or legacy Polish alias) to the canonical English key.
            /// Unknown values (custom player names) are returned trimmed, unchanged.
            /// </summary>
            public static string Canonical(string? name) => CatalogKey.Resolve(name, CanonicalMap);

            /// <summary>Canonical English key when <paramref name="name"/> is a catalog item; otherwise the trimmed original.</summary>
            public static string CanonicalNameOrRaw(string? name)
            {
                var canonical = Canonical(name);
                return Names.Contains(canonical) ? canonical : (name?.Trim() ?? string.Empty);
            }

            private static readonly Dictionary<string, string> CanonicalMap = CatalogKey.BuildMap(Names, new Dictionary<string, string>
            {
                ["bez broni"] = BasicWeaponsMelee.Unarmed,
                ["pięści"] = BasicWeaponsMelee.Unarmed,
                ["sztylet"] = BasicWeaponsMelee.Dagger,
                ["długi miecz"] = BasicWeaponsMelee.LongSword,
                ["miecz długi"] = BasicWeaponsMelee.LongSword,
                ["topór bojowy"] = BasicWeaponsMelee.BattleAxe,
                ["kilof"] = BasicWeaponsMelee.Pickaxe,
                ["buława"] = BasicWeaponsMelee.Mace,
                ["morgenstern"] = BasicWeaponsMelee.Morningstar,
                ["krótka włócznia"] = BasicWeaponsMelee.ShorSpear,
                ["rapier"] = BasicWeaponsMelee.Rapier,
                ["cep dwuręczny"] = BasicWeaponsMelee.TwoHandedFlail,
                ["młot bojowy"] = BasicWeaponsMelee.Warhammer,
                ["wielki topór"] = BasicWeaponsMelee.Greataxe,
                ["nadziak"] = BasicWeaponsMelee.Poleaxe,
                ["sarissa"] = BasicWeaponsMelee.Sarissa,
                ["chopesz"] = BasicWeaponsMelee.Khopesh,
                ["bicz"] = BasicWeaponsMelee.Whip,
                ["maczuga bojowa"] = BasicWeaponsMelee.WarClub,
                ["berdysz"] = BasicWeaponsMelee.Bardiche,
                ["lanca kawaleryjska"] = BasicWeaponsMelee.LanceCavalry,
                ["lanca piechoty"] = BasicWeaponsMelee.LanceInfantry,
                ["miecz dwuręczny"] = BasicWeaponsMelee.Greatsword,
                ["halabarda"] = BasicWeaponsMelee.Halberd,
                ["gizarma"] = BasicWeaponsMelee.Billhook,
                ["lewak"] = BasicWeaponsMelee.MainGauche,
                ["kostur"] = BasicWeaponsMelee.Staff,
                ["kusza lekka"] = BasicWeaponsShooting.CrossbowLight,
                ["lekka kusza"] = BasicWeaponsShooting.CrossbowLight,
                ["kusza ciężka"] = BasicWeaponsShooting.CrossbowHeavy,
                ["ciężka kusza"] = BasicWeaponsShooting.CrossbowHeavy,
                ["łuk prosty"] = BasicWeaponsShooting.BowSimple,
                ["długi łuk"] = BasicWeaponsShooting.Longbow,
                ["proca"] = BasicWeaponsShooting.Slingshot,
                ["muszkiet"] = BasicWeaponsShooting.Musket,
                ["oszczep"] = BasicWeaponsShooting.Javelin,
                ["puklerz drewniany"] = BasicShields.WoodenBuckler,
                ["drewniany puklerz"] = BasicShields.WoodenBuckler,
                ["puklerz metalowy"] = BasicShields.MetalBuckler,
                ["metalowy puklerz"] = BasicShields.MetalBuckler,
                ["tarcza drewniana"] = BasicShields.WoodenShield,
                ["drewniana tarcza"] = BasicShields.WoodenShield,
                ["tarcza metalowa"] = BasicShields.MetalShield,
                ["metalowa tarcza"] = BasicShields.MetalShield,
                ["duża tarcza drewniana"] = BasicShields.BigWoodenShield,
                ["duża tarcza metalowa"] = BasicShields.BigMetalShield,
                ["pawęż"] = BasicShields.Pavise,
                ["lekka zbroja skórzana"] = BasicArmors.LightLeatherArmor,
                ["lekka skórzana zbroja"] = BasicArmors.LightLeatherArmor,
                ["zbroja z łusek skórzanych"] = BasicArmors.LeatherScaleArmor,
                ["zbroja z łusek stalowych"] = BasicArmors.SteelScaleArmor,
                ["półpancerz"] = BasicArmors.HalfPlate,
                ["pełna zbroja płytowa"] = BasicArmors.FullPlate,
                ["bandaż"] = "Bandage",
                ["balsam na rany"] = "Wound balm",
                ["maść na rany"] = "Wound balm",
                ["lina"] = "Rope",
            });
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
        public const string Windmill = "icons/windmill.svg";
        public const string WindmillBlack = "icons/windmill-black.svg";
        public const string TwoCoins = "icons/two-coins.svg";
        public const string Trade = "icons/trade.svg";
        public const string AxeSword = "icons/axe-sword.svg";
        public const string VerticalBanner = "icons/vertical-banner.svg";
        public const string People = "icons/people.svg";
        public const string ShakingHands = "icons/shaking-hands.svg";
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
        public const string StoneThrone = "icons/stone-throne.svg";
        public const string Abacus = "icons/abacus.svg";
        public const string ElfHelmet = "icons/elf-helmet.svg";
        public const string JeweledChalice = "icons/jeweled-chalice.svg";
        public const string QuillInk = "icons/quill-ink.svg";
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

            public static string Canonical(string? name) => CatalogKey.Resolve(name, CanonicalMap);

            private static readonly Dictionary<string, string> CanonicalMap = CatalogKey.BuildMap(All, new Dictionary<string, string>
            {
                ["glowa"] = Head,
                ["głowa"] = Head,
                ["szyja"] = Neck,
                ["reka glowna"] = MainArm,
                ["ręka główna"] = MainArm,
                ["reka pomocnicza"] = OffArm,
                ["ręka pomocnicza"] = OffArm,
                ["dlon glowna"] = MainHand,
                ["dłoń główna"] = MainHand,
                ["dlon pomocnicza"] = OffHand,
                ["dłoń pomocnicza"] = OffHand,
                ["plecy"] = Back,
                ["noga lewa"] = LeftLeg,
                ["noga prawa"] = RightLeg,
                ["twarz"] = Face,
                ["cialo"] = Body,
                ["ciało"] = Body,
            });
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
        /// <summary>Opaque demo-session token; present only while inside the "Try baron" demo.</summary>
        public const string DemoToken = "DemoToken";
        public static readonly string[] All = { SelectedCharacterId, UserName, UserId, IsAdminOrMG, CharacterMG, IsAuthenticated, Role, IsInited, DemoToken };
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

            /// <summary>Maps a stored state name (English key or legacy Polish alias) to the canonical English key.</summary>
            public static string Canonical(string? name) => CatalogKey.Resolve(name, CanonicalMap);

            private static readonly Dictionary<string, string> CanonicalMap = CatalogKey.BuildMap(All, new Dictionary<string, string>
            {
                ["ogluszony"] = Stunned,
                ["ogłuszony"] = Stunned,
                ["potkniety"] = Stumbled,
                ["potknięty"] = Stumbled,
                ["pochwycony"] = Snatched,
                ["rozbrojony"] = Disarmed,
                ["oslepiony"] = Blinded,
                ["oślepiony"] = Blinded,
                ["nieswiadomy"] = Unaware,
                ["nieświadomy"] = Unaware,
                ["niewidzialny"] = Invisible,
                ["otoczony"] = Surrounded,
                ["niezrownowazony"] = Unbalanced,
                ["niezrównoważony"] = Unbalanced,
                ["ostrozny"] = Cautious,
                ["ostrożny"] = Cautious,
                ["pelna obrona"] = FullDefence,
                ["pełna obrona"] = FullDefence,
                ["krwawiacy"] = Bleeding,
                ["krwawiący"] = Bleeding,
                ["nieprzytomny"] = Unconscious,
                ["martwy"] = Dead,
                ["brak tury"] = NoTurn,
                ["pol tury"] = HalfTurn,
                ["pół tury"] = HalfTurn,
            });
        }

        /// <summary>Seeded English temporary-state descriptions (display keys).</summary>
        public static readonly string[] CatalogDescriptions =
        {
            "This character can't do anything this turn",
            "This character is dazed, it cannot perform any actions and its defence is impaired",
            "This character is dead...",
            "This character is excluded from any fight",
            "This character is seriously bleeding. It gets one wound every turn, untill 10 round or the wound is taken care of",
            "This character is surrounded by enemies. For every other enemy attacking this character there is added penalty to defence equal to 2",
            "This character is unaware of it's enemies. This causes penalty to defence equal to 10. Unaware characters become aware after first attack",
            "This character lost its balance. For remainging turn he have penalty of 7 to defence",
            "This character lost its balance, and lies on the ground. To get up it needs to use action (or two if in heavy armor)",
            "This character lost its sight. This causes penalty to defence equal 8, unless there is other way to see incoming attacks. This character can attack with penalty equal to 5",
            "This character lost it primary weapon",
            "This character still have one action this turn",
            "This character was captured. It cannot move or use captured limb until it gets free",
            "This character went in full defence. It gets bonus to all kinds of defence equal to 5, but it cannot attack or make any actions",
            "This character went in semi-defencive state. It gets bonus to all kinds of defence equal to 2",
            "This character cannot be seen, but enemies are aware of its presence. This causes bonus to attack equal to 5, and bonus defence equal to 5",
        };
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
