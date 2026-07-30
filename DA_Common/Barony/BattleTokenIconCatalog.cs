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
            new("axe", "Axe", "icons/axe-in-stump.svg"),
            new("spear", "Spear", "icons/spear-feather.svg"),
            new("helm", "Helm", "icons/barbute.svg"),
            new("elf-helm", "Elf helm", "icons/elf-helmet.svg"),
            new("horse", "Horse", "icons/horse-head.svg"),
            new("tower", "Tower", "icons/stone-tower.svg"),
            new("banner", "Imperial", "icons/imperial.svg"),
            new("cowled", "Hooded", "icons/cowled.svg"),
            new("skull", "Skull", "icons/death-skull.svg"),
            new("bolt", "Spell", "icons/bolt-spell-cast.svg"),
        };

        public static BattleTokenIcon? Find(string? key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return null;
            return All.FirstOrDefault(i => i.Key.Equals(key.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        public static string? PathFor(string? key) => Find(key)?.Path;
    }
}
