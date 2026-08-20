using System;
using System.Collections.Generic;
using System.Linq;

namespace DA_Common.Barony
{
    public sealed record PetitionerIcon(string Key, string Label, string Path);

    /// <summary>
    /// Icons for audience petitioners: person, caste, profession, or purpose of the visit.
    /// Paths are relative to wwwroot.
    /// </summary>
    public static class PetitionerIconCatalog
    {
        public const string DefaultPath = "icons/people.svg";

        public static IReadOnlyList<PetitionerIcon> All { get; } = new PetitionerIcon[]
        {
            // People & caste
            new("people", "Commoner", "icons/people.svg"),
            new("farmer", "Farmer", "icons/farmer.svg"),
            new("hood", "Hooded", "icons/hood.svg"),
            new("cowled", "Cowled", "icons/cowled.svg"),
            new("robe", "Robed", "icons/robe.svg"),
            new("cultist", "Cultist", "icons/cultist.svg"),
            new("executioner", "Executioner", "icons/executioner-hood.svg"),
            new("queen-crown", "Royalty", "icons/queen-crown.svg"),
            new("jewel-crown", "Lord", "icons/jewel-crown.svg"),
            new("ring", "Noble", "icons/big-diamond-ring.svg"),
            new("dwarf-helm", "Dwarf", "icons/dwarf-helmet.svg"),
            new("elf-helm", "Elf", "icons/elf-helmet.svg"),
            new("goblin", "Goblin", "icons/goblin.svg"),
            new("spectacles", "Scholar", "icons/spectacles.svg"),

            // Warriors & offices
            new("helm", "Soldier", "icons/barbute.svg"),
            new("black-knight", "Dark knight", "icons/black-knight-helm.svg"),
            new("brutal-helm", "Brute", "icons/brutal-helm.svg"),
            new("horned-helm", "Barbarian", "icons/horned-helm.svg"),
            new("mounted-knight", "Knight", "icons/mounted-knight.svg"),
            new("cavalry", "Rider", "icons/cavalry.svg"),
            new("archer", "Archer", "icons/archer.svg"),
            new("crossbow", "Crossbow", "icons/crossbow.svg"),
            new("shield", "Guard", "icons/shield.svg"),
            new("breastplate", "Man-at-arms", "icons/breastplate.svg"),
            new("swords", "Warrior", "icons/axe-sword.svg"),
            new("crossed-swords", "Duelist", "icons/crossed-swords.svg"),

            // Craft & trade
            new("anvil-impact", "Smith", "icons/anvil-impact.svg"),
            new("anvil", "Forge", "icons/anvil.svg"),
            new("hammer", "Craftsman", "icons/hammer.svg"),
            new("stone-crafting", "Stonemason", "icons/stone-crafting.svg"),
            new("freemasonry", "Guild", "icons/freemasonry.svg"),
            new("hatchet", "Woodsman", "icons/hatchet.svg"),
            new("axe-stump", "Woodcutter", "icons/axe-in-stump-black.svg"),
            new("mine-wagon", "Miner", "icons/mine-wagon-black.svg"),
            new("trade", "Merchant", "icons/trade.svg"),
            new("coins", "Money", "icons/two-coins.svg"),
            new("crate", "Trader", "icons/wooden-crate.svg"),
            new("abacus", "Steward", "icons/abacus.svg"),
            new("scales", "Judge", "icons/scales.svg"),
            new("justice", "Law", "icons/justice.svg"),

            // Faith, arts, learning
            new("sun-priest", "Priest", "icons/sun-priest.svg"),
            new("chalice", "Courtier", "icons/jeweled-chalice.svg"),
            new("quill-ink", "Scribe", "icons/quill-ink.svg"),
            new("quill", "Clerk", "icons/quill.svg"),
            new("lyre", "Bard", "icons/lyre.svg"),
            new("herbs", "Herbalist", "icons/herbs-bundle.svg"),
            new("apothecary", "Healer", "icons/apothecary.svg"),
            new("wand", "Mage", "icons/crystal-wand.svg"),
            new("spell", "Spellcaster", "icons/bolt-spell-cast.svg"),
            new("sparkles", "Mystic", "icons/sparkles.svg"),

            // Purpose of visit
            new("scroll", "Petition", "icons/tied-scroll.svg"),
            new("papers", "Documents", "icons/papers.svg"),
            new("wax-seal", "Official", "icons/wax-seal.svg"),
            new("banner", "Herald", "icons/vertical-banner.svg"),
            new("hands", "Diplomat", "icons/shaking-hands.svg"),
            new("dove", "Peace envoy", "icons/peace-dove.svg"),
            new("compass", "Traveler", "icons/compass.svg"),
            new("horn", "Hunter", "icons/hunting-horn-black.svg"),
            new("horse", "Horseman", "icons/horse-head.svg"),
            new("sickle", "Harvest", "icons/sickle.svg"),
            new("scythe", "Reaper", "icons/scythe.svg"),
            new("wheat", "Grain", "icons/wheat.svg"),
            new("stein", "Innkeeper", "icons/beer-stein.svg"),
            new("wine", "Feast", "icons/wine-glass.svg"),
            new("fishing", "Fisher", "icons/fishing.svg"),
            new("cow", "Herder", "icons/cow.svg"),
            new("village", "Villager", "icons/village.svg"),
            new("gate", "Townsman", "icons/medieval-gate.svg"),
            new("heart", "Plea", "icons/heart.svg"),
            new("fist", "Anger", "icons/fist.svg"),
            new("skull", "Death", "icons/death-skull.svg"),
            new("hourglass", "Urgent", "icons/hourglass.svg"),
            new("unknown", "Unknown", "icons/uncertainty.svg"),
        };

        public static PetitionerIcon? Find(string? keyOrPath)
        {
            if (string.IsNullOrWhiteSpace(keyOrPath))
                return null;
            var t = Normalize(keyOrPath);
            return All.FirstOrDefault(i =>
                i.Key.Equals(t, StringComparison.OrdinalIgnoreCase)
                || i.Path.Equals(t, StringComparison.OrdinalIgnoreCase));
        }

        public static string PathFor(string? keyOrPath)
        {
            if (string.IsNullOrWhiteSpace(keyOrPath))
                return DefaultPath;
            var t = Normalize(keyOrPath);
            var found = Find(t);
            if (found is not null)
                return found.Path;
            return t.StartsWith("icons/", StringComparison.OrdinalIgnoreCase) ? t : DefaultPath;
        }

        private static string Normalize(string value)
        {
            var t = value.Trim().TrimStart('/');
            if (t.StartsWith("wwwroot/", StringComparison.OrdinalIgnoreCase))
                t = t["wwwroot/".Length..];
            return t;
        }
    }
}
