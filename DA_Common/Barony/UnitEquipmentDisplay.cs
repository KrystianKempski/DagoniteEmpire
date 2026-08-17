using DA_Common.Localization;

namespace DA_Common.Barony
{
    /// <summary>Display-time localization for unit equipment catalog names and labels.</summary>
    public static class UnitEquipmentDisplay
    {
        public static string DisplayName(this UnitWeaponDef weapon) => LocCatalog.Name(weapon?.Name);
        public static string DisplayName(this UnitArmorDef armor) => LocCatalog.Name(armor?.Name);
        public static string DisplayName(this UnitMountDef mount) => LocCatalog.Name(mount?.Name);

        /// <summary>Localized weapon type (e.g. "Swords", "Crossbows").</summary>
        public static string WeaponTypeLabel(string? weaponType) => LocCatalog.Name(weaponType);

        /// <summary>Localized armor-tier title (e.g. "Simple armor", "Heavy armor").</summary>
        public static string ArmorTierLabel(string? title) => LocCatalog.Name(title);
    }
}
