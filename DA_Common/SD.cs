using Syncfusion.Blazor.RichTextEditor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DA_Common
{
    public static class SD
    {
        public const string Role_Admin = "Admin";
        public const string Role_GameMaster= "GameMaster";
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

        public const string Strength = "Strength";
        public const string Dexterity = "Dexterity";
        public const string Endurance = "Endurance";
        public const string Intelligence = "Intelligence";
        public const string Instinct = "Instinct";
        public const string Willpower = "Willpower";
        public const string Charisma = "Charisma";
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

     //circle 0  1  2  3  4  5  6  7  8  9
        public static readonly int[,] SpellsKnown = { 
            { 4, 2, 0, 0, 0, 0, 0, 0, 0, 0 },               // lvl 0
            { 4, 3, 2, 1, 0, 0, 0, 0, 0, 0 },               // lvl 1
            { 4, 4, 3, 3, 2, 0, 0, 0, 0, 0 },               // lvl 2
            { 4, 4, 4, 4, 3, 2, 1, 0, 0, 0 },               // lvl 3
            { 4, 3, 2, 1, 0, 0, 0, 0, 0, 0 },               // lvl 4
            { 4, 4, 4, 4, 4, 3, 3, 2, 0, 0 },               // lvl 5
            { 4, 4, 4, 4, 4, 4, 3, 3, 2, 1 },               // lvl 6
            { 4, 4, 4, 4, 4, 4, 4, 4, 4, 4,},               // lvl 7
        } ;

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

    public enum SpellcasterType
    {
        Wizard,
        Sorcerer
    }
}
