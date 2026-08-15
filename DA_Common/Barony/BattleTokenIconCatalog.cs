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
            new("shield", "Tarcza", "icons/shield.svg"),
            new("breastplate", "Napierśnik", "icons/breastplate.svg"),
            new("armor", "Kamizelka pancerna", "icons/armor-vest.svg"),
            new("leather", "Skórzana zbroja", "icons/leather-armor.svg"),
            new("sword", "Miecz", "icons/axe-sword.svg"),
            new("clash", "Starcie mieczy", "icons/sword-clash.svg"),
            new("spear", "Włócznia", "icons/spear-feather.svg"),
            new("flail", "Cep bojowy", "icons/flail.svg"),
            new("archer", "Łucznik", "icons/archer.svg"),
            new("crossbow", "Kusza", "icons/crossbow.svg"),
            new("helm", "Hełm", "icons/barbute.svg"),
            new("elf-helm", "Elfi hełm", "icons/elf-helmet.svg"),
            new("black-knight-helm", "Hełm czarnego rycerza", "icons/black-knight-helm.svg"),
            new("brutal-helm", "Brutalny hełm", "icons/brutal-helm.svg"),
            new("horned-helm", "Rogaty hełm", "icons/horned-helm.svg"),
            new("horse", "Koń", "icons/horse-head.svg"),
            new("cavalry", "Kawaleria", "icons/cavalry.svg"),
            new("mounted-knight", "Rycerz konny", "icons/mounted-knight.svg"),
            new("cowled", "W kapturze", "icons/cowled.svg"),
            new("skull", "Czaszka", "icons/death-skull.svg"),
            new("bolt", "Zaklęcie", "icons/bolt-spell-cast.svg"),
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
