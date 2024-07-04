using Syncfusion.Blazor.RichTextEditor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

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
        public const string WeaponQuality_Fast = "Fast";
        public const string WeaponQuality_Slow = "Slow";
        public const string WeaponQuality_Parrying = "Parrying";
        public const string WeaponQuality_ShieldDestructive = "Shield destructive";
        public const string WeaponQuality_ArmorPiercing = "Armor piercing";
        public const string WeaponQuality_Long = "Long";
        public const string WeaponQuality_Heavy = "Heavy";
        public const string WeaponQuality_Devastating = "Devastating";
        public const string WeaponQuality_Weak = "Weak";
        public const string WeaponQuality_Stunning = "Stunning";
        public const string WeaponQuality_Stumbling = "Stumbling";
        public const string WeaponQuality_Snatching = "Snatching";
        public const string WeaponQuality_Armor = "Armor";
        public const string WeaponQuality_ArmorDefenceBonus = "Armor defence bonus";
        public const string WeaponQuality_ArmorBane = "ArmorBane";
        public const string WeaponQuality_Durability = "Durability";
        public const string WeaponQuality_ShieldDefenceBonus = "Shield defence bonus";
        public const string WeaponQuality_Bulky = "Bulky";
        public const string WeaponQuality_Precise = "Precise";
        public const string WeaponQuality_Range  = "Range";
        public const string WeaponQuality_Ligh = "Ligh";

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

    }
    public static class MyIcon
    {
        public const string Bookmark = "icons/bookmarklet.svg";
        public const string Scroll = "icons/scroll.svg";
        public const string Quill = "icons/quill.svg";


    }

    public enum EquipmentType
    {
        Other,
        Armor,
        WeaponMelee,
        WeaponRanged,
    }
}
