namespace DA_Common.Barony.Battle
{
    /// <summary>
    /// In-battle weapon set toggle: active slot 1 uses Weapon1Key, slot 2 uses Weapon2Key.
    /// Shields apply only when the active weapon is one-handed.
    /// </summary>
    public static class BattleWeaponSwap
    {
        public const int MinSlot = 1;
        public const int MaxSlot = 2;

        public static bool CanSwap(string? weapon2Key) =>
            !string.IsNullOrWhiteSpace(weapon2Key);

        public static string? ActiveWeaponKey(string? weapon1Key, string? weapon2Key, int activeSlot) =>
            activeSlot == 2 ? weapon2Key : weapon1Key;

        public static string? ActiveWeaponQuality(string? weapon1Quality, string? weapon2Quality, int activeSlot) =>
            activeSlot == 2 ? weapon2Quality : weapon1Quality;

        /// <summary>Shield counts only when the wielded weapon is one-handed.</summary>
        public static string? ActiveShieldKey(string? weapon1Key, string? weapon2Key, string? shieldKey, int activeSlot)
        {
            var weapon = UnitWeaponCatalog.Find(ActiveWeaponKey(weapon1Key, weapon2Key, activeSlot));
            if (weapon is null || !weapon.OneHanded)
                return null;
            return string.IsNullOrWhiteSpace(shieldKey) ? null : shieldKey;
        }

        public static bool HasActiveShield(string? weapon1Key, string? weapon2Key, string? shieldKey, int activeSlot) =>
            ActiveShieldKey(weapon1Key, weapon2Key, shieldKey, activeSlot) is not null;

        public static int OtherSlot(int activeSlot) => activeSlot == 2 ? 1 : 2;

        /// <summary>After battle, normalize loadout keys so Weapon1 matches the active slot.</summary>
        public static (string? Weapon1Key, string? Weapon2Key, string? Weapon1Quality, string? Weapon2Quality) NormalizeToActiveSlot(
            string? weapon1Key,
            string? weapon2Key,
            string? weapon1Quality,
            string? weapon2Quality,
            int activeSlot)
        {
            if (activeSlot != 2 || !CanSwap(weapon2Key))
                return (weapon1Key, weapon2Key, weapon1Quality, weapon2Quality);
            return (weapon2Key, weapon1Key, weapon2Quality, weapon1Quality);
        }

        public static string DescribeActiveLoadout(string? weapon1Key, string? weapon2Key, string? shieldKey, int activeSlot)
        {
            var weapon = UnitWeaponCatalog.Find(ActiveWeaponKey(weapon1Key, weapon2Key, activeSlot));
            var name = weapon?.DisplayName() ?? ActiveWeaponKey(weapon1Key, weapon2Key, activeSlot) ?? "?";
            var activeShieldKey = ActiveShieldKey(weapon1Key, weapon2Key, shieldKey, activeSlot);
            if (activeShieldKey is null)
                return name;
            var shieldName = UnitArmorCatalog.Find(activeShieldKey)?.DisplayName() ?? activeShieldKey;
            return $"{name} + {shieldName}";
        }

        public static string DescribeSwap(
            string? weapon1Key,
            string? weapon2Key,
            string? shieldKey,
            int fromSlot,
            int toSlot) =>
            $"{DescribeActiveLoadout(weapon1Key, weapon2Key, shieldKey, fromSlot)} → {DescribeActiveLoadout(weapon1Key, weapon2Key, shieldKey, toSlot)}";
    }
}
