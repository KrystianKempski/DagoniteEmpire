namespace DA_Common.Barony
{
    /// <summary>
    /// Maps unit weapons / armor / shields to trade-good access keys
    /// (<c>access-arms-*</c>, <c>access-armor-*</c>). Simple weapons need no trade good.
    /// </summary>
    public static class UnitEquipmentTradeAccess
    {
        public const string MilitaryArms = "access-arms-military";
        public const string Firearms = "access-arms-firearms";
        public const string LightArmor = "access-armor-light";
        public const string MediumArmor = "access-armor-medium";
        public const string HeavyArmor = "access-armor-heavy";
        public const string Horses = "horses";
        public const string WarHorses = "war-horses";

        /// <summary>Required trade-good key, or null when the item needs no strategic access.</summary>
        public static string? RequiredGoodKey(UnitWeaponDef w) => w.Category.ToLowerInvariant() switch
        {
            "military" => MilitaryArms,
            "powder" => Firearms,
            _ => null, // simple
        };

        /// <summary>
        /// Shields and body armor follow the Excel tier groupings
        /// (Simple / Medium / Heavy), not raw <see cref="UnitArmorDef.ArmorClass"/>.
        /// </summary>
        public static string? RequiredGoodKey(UnitArmorDef a)
        {
            foreach (var (title, keys) in UnitArmorCatalog.ExcelTiers)
            {
                if (!keys.Any(k => string.Equals(k, a.Key, StringComparison.OrdinalIgnoreCase)))
                    continue;
                return title switch
                {
                    "Simple armor" => LightArmor,
                    "Medium armor" => MediumArmor,
                    "Heavy armor" => HeavyArmor,
                    _ => null,
                };
            }

            // Fallback by armor class if not listed in Excel tiers.
            return a.ArmorClass.ToLowerInvariant() switch
            {
                "light" => LightArmor,
                "medium" => MediumArmor,
                "heavy" => HeavyArmor,
                _ => LightArmor,
            };
        }

        public static bool HasAccess(UnitWeaponDef w, TradeGoodAvailabilitySnapshot? availability) =>
            HasAccess(RequiredGoodKey(w), availability);

        public static bool HasAccess(UnitArmorDef a, TradeGoodAvailabilitySnapshot? availability) =>
            HasAccess(RequiredGoodKey(a), availability);

        public static bool HasAccess(UnitMountDef m, TradeGoodAvailabilitySnapshot? availability) =>
            HasAccess(m.RequiredTradeGoodKey, availability);

        public static bool HasAccess(string? requiredGoodKey, TradeGoodAvailabilitySnapshot? availability)
        {
            if (string.IsNullOrWhiteSpace(requiredGoodKey))
                return true;
            // Null snapshot = UI not loaded yet; do not grey rows until availability is known.
            // Server callers must pass a resolved snapshot.
            if (availability is null)
                return true;
            return availability.IsAvailable(requiredGoodKey);
        }

        public static string LackReason(UnitWeaponDef w) =>
            LackReason(RequiredGoodKey(w), UnitWeaponCatalog.CategoryLabel(w.Category));

        public static string LackReason(UnitArmorDef a)
        {
            var key = RequiredGoodKey(a);
            var label = TradeGoodsCatalog.Find(key)?.Name
                ?? key
                ?? "armor access";
            return $"{a.Name}: need trade access to {label}.";
        }

        public static string LackReason(UnitMountDef m)
        {
            var label = TradeGoodsCatalog.Find(m.RequiredTradeGoodKey)?.Name
                ?? m.RequiredTradeGoodKey;
            return $"{m.Name}: need trade access to {label}.";
        }

        private static string LackReason(string? goodKey, string fallbackLabel)
        {
            var label = TradeGoodsCatalog.Find(goodKey)?.Name ?? fallbackLabel;
            return $"Need trade access to {label}.";
        }

        public static bool MeetsWeapon(
            UnitWeaponDef w,
            int build,
            int agility,
            TradeGoodAvailabilitySnapshot? availability,
            out string reason)
        {
            if (!UnitEquipmentRequirements.MeetsWeapon(w, build, agility, out reason))
                return false;
            if (!HasAccess(w, availability))
            {
                reason = LackReason(w);
                return false;
            }
            reason = string.Empty;
            return true;
        }

        public static bool MeetsArmor(
            UnitArmorDef a,
            int build,
            int armorSkill,
            TradeGoodAvailabilitySnapshot? availability,
            out string reason)
        {
            if (!UnitEquipmentRequirements.MeetsArmor(a, build, armorSkill, out reason))
                return false;
            if (!HasAccess(a, availability))
            {
                reason = LackReason(a);
                return false;
            }
            reason = string.Empty;
            return true;
        }

        public static bool MeetsMount(
            UnitMountDef m,
            int ridingSkill,
            TradeGoodAvailabilitySnapshot? availability,
            out string reason)
        {
            if (!UnitEquipmentRequirements.MeetsMount(m, ridingSkill, out reason))
                return false;
            if (!HasAccess(m, availability))
            {
                reason = LackReason(m);
                return false;
            }
            reason = string.Empty;
            return true;
        }

        public static string? FirstEligibleWeaponKey(
            int build,
            int agility,
            TradeGoodAvailabilitySnapshot? availability) =>
            UnitWeaponCatalog.All
                .FirstOrDefault(w => MeetsWeapon(w, build, agility, availability, out _))
                ?.Key;
    }
}
