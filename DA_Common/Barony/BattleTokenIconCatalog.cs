using System;
using System.Collections.Generic;
using System.Linq;

namespace DA_Common.Barony
{
    public sealed record BattleTokenIcon(string Key, string Label, string Path);

    /// <summary>Token icons for Barony Battle Map (paths relative to wwwroot).</summary>
    public static class BattleTokenIconCatalog
    {
        public static IReadOnlyList<BattleTokenIcon> All { get; } = new BattleTokenIcon[]
        {
            new("shield", "Shield", "icons/shield.svg"),
            new("breastplate", "Breastplate", "icons/breastplate.svg"),
            new("armor", "Armor vest", "icons/armor-vest.svg"),
            new("leather", "Leather armor", "icons/leather-armor.svg"),
            new("sword", "Sword", "icons/axe-sword.svg"),
            new("clash", "Sword clash", "icons/sword-clash.svg"),
            new("spear", "Spear", "icons/spear-feather.svg"),
            new("flail", "Flail", "icons/flail.svg"),
            new("archer", "Archer", "icons/archer.svg"),
            new("crossbow", "Crossbow", "icons/crossbow.svg"),
            new("helm", "Helm", "icons/barbute.svg"),
            new("elf-helm", "Elf helm", "icons/elf-helmet.svg"),
            new("black-knight-helm", "Black knight helm", "icons/black-knight-helm.svg"),
            new("brutal-helm", "Brutal helm", "icons/brutal-helm.svg"),
            new("horned-helm", "Horned helm", "icons/horned-helm.svg"),
            new("horse", "Horse", "icons/horse-head.svg"),
            new("cavalry", "Cavalry", "icons/cavalry.svg"),
            new("mounted-knight", "Mounted knight", "icons/mounted-knight.svg"),
            new("cowled", "Hooded", "icons/cowled.svg"),
            new("skull", "Skull", "icons/death-skull.svg"),
            new("bolt", "Spell", "icons/bolt-spell-cast.svg"),
        };

        /// <summary>Removed from the picker; still resolved so existing tokens keep their art.</summary>
        private static readonly IReadOnlyDictionary<string, string> LegacyPaths =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["axe"] = "icons/axe-in-stump.svg",
                ["tower"] = "icons/stone-tower.svg",
                ["banner"] = "icons/imperial.svg",
            };

        public static BattleTokenIcon? Find(string? key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return null;
            return All.FirstOrDefault(i => i.Key.Equals(key.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        public static string? PathFor(string? key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return null;
            var trimmed = key.Trim();
            var found = Find(trimmed);
            if (found is not null)
                return found.Path;
            return LegacyPaths.TryGetValue(trimmed, out var path) ? path : null;
        }
    }
}
