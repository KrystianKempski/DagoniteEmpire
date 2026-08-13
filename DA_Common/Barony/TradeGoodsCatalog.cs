namespace DA_Common.Barony
{
    public static class TradeGoodSection
    {
        public const string Food = "Zywnosc";
        public const string Fibers = "Wloki";
        public const string Mines = "Kopaliny";
        public const string Wood = "Drewno";
        public const string Crafts = "Rzemioslo";
        public const string Exotic = "Egzotyczne";
        public const string Military = "Bron";

        public static readonly IReadOnlyList<(string Key, string LabelEn)> All = new (string, string)[]
        {
            (TradeGoodSection.Food, "Food"),
            (TradeGoodSection.Fibers, "Fibers & hides"),
            (TradeGoodSection.Mines, "Mines & stone"),
            (TradeGoodSection.Wood, "Wood & forest"),
            (TradeGoodSection.Crafts, "Craft goods"),
            (TradeGoodSection.Exotic, "Exotic imports"),
            (TradeGoodSection.Military, "Arms & horses"),
        };
    }

    public sealed class TradeGoodEntry
    {
        public required string Key { get; init; }
        public required string SectionKey { get; init; }
        public required string Name { get; init; }
        public required string Description { get; init; }
        public required string BonusDisplay { get; init; }
        public required string Unlocks { get; init; }
        public required string ProductionBuilding { get; init; }
        public string? Requirements { get; init; }
        public string IconUrl { get; set; } = "/icons/wooden-crate.svg";
        public string ColorHex { get; set; } = "#888888";
        public PpbVector BonusAdditive { get; init; } = new();
        public PpbVector BonusPercent { get; init; } = new();
    }

    public static class TradeGoodsCatalog
    {
        private static readonly List<TradeGoodEntry> _all = new();
        public static IReadOnlyList<TradeGoodEntry> All => _all;

        private static TradeGoodEntry G(
            string key, string section, string name, string description,
            string bonusDisplay, string unlocks, string productionBuilding, string? requirements)
        {
            var g = new TradeGoodEntry
            {
                Key = key, SectionKey = section, Name = name, Description = description,
                BonusDisplay = bonusDisplay, Unlocks = unlocks, ProductionBuilding = productionBuilding,
                Requirements = requirements,
            };
            _all.Add(g);
            return g;
        }

        static TradeGoodsCatalog()
        {

            { var g = G("salt", TradeGoodSection.Mines, "Salt", "Salt from mines or brine works; preserves food and supports long-distance trade.",
                "+5% Food", "Meat saltery", "Mine - Salt", "salt deposit");
            g.BonusPercent[Ppb.Food] = 5.0m;
            }
            { var g = G("fish-meat-salted", TradeGoodSection.Food, "Salted fish & meat", "Fish and meat cured with salt; keeps well for storage and export.",
                "+1 Defense, +1 Production", "", "Meat saltery (salt + fish/meat)", "salt deposit + fish");
            g.BonusAdditive[Ppb.Defense] = 1.0m;
            g.BonusAdditive[Ppb.Production] = 1.0m;
            }
            { var g = G("cattle", TradeGoodSection.Food, "Cattle", "Herds on pasture; meat, hides, wool, and draft power.",
                "+1 Economy, +1 Production", "Cheese dairy, Tannery", "Pastures (cattle)", "fertile land (3+)");
            g.BonusAdditive[Ppb.Economy] = 1.0m;
            g.BonusAdditive[Ppb.Production] = 1.0m;
            }
            { var g = G("cheese", TradeGoodSection.Food, "Cheese", "Dairy from pastoral herds; staple food and local trade.",
                "+1 Culture, +1 Economy", "", "Cheese dairy", "cattle");
            g.BonusAdditive[Ppb.Culture] = 1.0m;
            g.BonusAdditive[Ppb.Economy] = 1.0m;
            }
            { var g = G("honey-wax", TradeGoodSection.Wood, "Honey & wax", "Honey and beeswax from apiaries; sweetener, candles, and crafts.",
                "+1 Loyalty, +1 Science", "Candlemaker", "Apiary", "rich forest (3+)");
            g.BonusAdditive[Ppb.Loyalty] = 1.0m;
            g.BonusAdditive[Ppb.Science] = 1.0m;
            }
            { var g = G("wine", TradeGoodSection.Food, "Wine", "Wine from vineyards; drink, feasts, and trade in warm hills.",
                "+1 Intelligence, +1 Stability", "", "Vineyard", "fertile hills, warm climate (4+)");
            g.BonusAdditive[Ppb.Intelligence] = 1.0m;
            g.BonusAdditive[Ppb.Stability] = 1.0m;
            }
            { var g = G("olive-oil", TradeGoodSection.Exotic, "Olive oil", "Cooking oil from olives; lamps and preserved foods.",
                "+1 Culture, +1 Stability", "", "Import", null);
            g.BonusAdditive[Ppb.Culture] = 1.0m;
            g.BonusAdditive[Ppb.Stability] = 1.0m;
            }
            { var g = G("beer", TradeGoodSection.Food, "Beer", "Ale and beer from grain; everyday drink in towns and garrisons.",
                "+1 Stability, +1 Loyalty", "", "Brewery", "grain");
            g.BonusAdditive[Ppb.Stability] = 1.0m;
            g.BonusAdditive[Ppb.Loyalty] = 1.0m;
            }
            { var g = G("wool", TradeGoodSection.Fibers, "Wool", "Raw wool from sheep; spun into yarn and cloth.",
                "+1 Production, +1 Economy", "", "Sheep pastures", "fertile terrain (2+)");
            g.BonusAdditive[Ppb.Production] = 1.0m;
            g.BonusAdditive[Ppb.Economy] = 1.0m;
            }
            { var g = G("sheep", TradeGoodSection.Fibers, "Sheep", "Breeding flocks for wool, meat, and manure. Stock must be obtained before pastures can be founded.",
                "+1 Food, +1 Loyalty", "Sheep pastures", "Sheep pastures", "fertile terrain (2+)");
            g.BonusAdditive[Ppb.Food] = 1.0m;
            g.BonusAdditive[Ppb.Loyalty] = 1.0m;
            }
            { var g = G("flax-hemp", TradeGoodSection.Fibers, "Flax & hemp", "Fiber for linen, rope, and sails.",
                "+1 Production, +1 Stability", "", "Flax farm", "fertile terrain (2+)");
            g.BonusAdditive[Ppb.Production] = 1.0m;
            g.BonusAdditive[Ppb.Stability] = 1.0m;
            }
            { var g = G("cloth", TradeGoodSection.Fibers, "Cloth", "Finished cloth for clothing, uniforms, and export.",
                "+1 Economy, +1 Culture", "", "Weaver's workshop", "flax / wool / cotton");
            g.BonusAdditive[Ppb.Economy] = 1.0m;
            g.BonusAdditive[Ppb.Culture] = 1.0m;
            }
            { var g = G("cotton", TradeGoodSection.Fibers, "Cotton", "Raw cotton; grows in warm, fertile fields.",
                "+1 Production, +1 Loyalty", "", "Cotton farm", "fertile land (3+), warm climate");
            g.BonusAdditive[Ppb.Production] = 1.0m;
            g.BonusAdditive[Ppb.Loyalty] = 1.0m;
            }
            { var g = G("woad", TradeGoodSection.Fibers, "Woad", "Blue dye from woad leaves (Isatis tinctoria)—the staple blue of medieval cloth.",
                "+1 Culture", "Dyer's workshop", "Farm (Dye plant)", "fertile land (2+)");
            g.BonusAdditive[Ppb.Culture] = 1.0m;
            }
            { var g = G("madder", TradeGoodSection.Fibers, "Madder", "Red dye from madder root (Rubia tinctorum)—rich crimsons for cloth and banners.",
                "+1 Production", "Dyer's workshop", "Farm (Dye plant)", "fertile land (2+)");
            g.BonusAdditive[Ppb.Production] = 1.0m;
            }
            { var g = G("weld", TradeGoodSection.Fibers, "Weld", "Yellow dye from weld (Reseda luteola)—bright mordant yellows and greens when mixed.",
                "+1 Economy", "Dyer's workshop", "Farm (Dye plant)", "fertile land (2+)");
            g.BonusAdditive[Ppb.Economy] = 1.0m;
            }
            { var g = G("leather", TradeGoodSection.Fibers, "Leather", "Tanned hides for footwear, straps, and light armor.",
                "+1 Production, +1 Defense", "Light armor (units)", "Tannery", "cattle");
            g.BonusAdditive[Ppb.Production] = 1.0m;
            g.BonusAdditive[Ppb.Defense] = 1.0m;
            }
            { var g = G("furs", TradeGoodSection.Fibers, "Furs", "Furs from hunts and cold country; luxury and winter wear.",
                "+1 Culture, +1 Intelligence", "", "Hunter's Lodge - Furs", "furs deposit");
            g.BonusAdditive[Ppb.Culture] = 1.0m;
            g.BonusAdditive[Ppb.Intelligence] = 1.0m;
            }
            { var g = G("dyes", TradeGoodSection.Crafts, "Dyes", "Dyes and fixatives for cloth and banners.",
                "+1 Culture, +1 Production", "", "Dyer's workshop", "woad / madder / weld");
            g.BonusAdditive[Ppb.Culture] = 1.0m;
            g.BonusAdditive[Ppb.Production] = 1.0m;
            }
            { var g = G("silk", TradeGoodSection.Fibers, "Silk", "Silk thread and fabric; expensive luxury import.",
                "+2 Culture", "", "Import", null);
            g.BonusAdditive[Ppb.Culture] = 2.0m;
            }
            { var g = G("soft-metals", TradeGoodSection.Mines, "Soft metals (Cu, Sn, Pb)", "Copper, tin, and lead for tools, alloys, and simple weapons.",
                "+1 Production", "Smithy", "Mine - soft metals", "soft metals deposit");
            g.BonusAdditive[Ppb.Production] = 1.0m;
            }
            { var g = G("iron", TradeGoodSection.Mines, "Iron", "Iron ore and blooms; foundation of tools and military arms.",
                "+1 Production, +1 Defense", "Forge, Armorer, Plate Workshop", "Mine - Iron", "iron deposit");
            g.BonusAdditive[Ppb.Production] = 1.0m;
            g.BonusAdditive[Ppb.Defense] = 1.0m;
            }
            { var g = G("silver", TradeGoodSection.Mines, "Silver", "Silver for coinage, jewelry, and treasury.",
                "+1 Economy, +1 Culture", "Mint, Jeweler", "Mine - Silver", "silver deposit");
            g.BonusAdditive[Ppb.Economy] = 1.0m;
            g.BonusAdditive[Ppb.Culture] = 1.0m;
            }
            { var g = G("gold", TradeGoodSection.Mines, "Gold", "Precious metal for coins, regalia, and prestige.",
                "+1 Economy, +1 Culture", "Mint, Jeweler", "Mine - Gold", "gold deposit");
            g.BonusAdditive[Ppb.Economy] = 1.0m;
            g.BonusAdditive[Ppb.Culture] = 1.0m;
            }
            { var g = G("gemstones", TradeGoodSection.Mines, "Gemstones", "Precious and ornamental stones.",
                "+1 Culture, +1 Magic", "Jeweler", "Mine - precious gems (luxury)", null);
            g.BonusAdditive[Ppb.Culture] = 1.0m;
            g.BonusAdditive[Ppb.Magic] = 1.0m;
            }
            { var g = G("building-stone", TradeGoodSection.Mines, "Building stone", "Common stone for walls, houses, and roads.",
                "+1 Production, +1 Defense", "Larger buildings, walls", "Quarry - common stone", "stone deposit");
            g.BonusAdditive[Ppb.Production] = 1.0m;
            g.BonusAdditive[Ppb.Defense] = 1.0m;
            }
            { var g = G("granite", TradeGoodSection.Mines, "Granite", "Hard stone for towers and fortress walls.",
                "+2 Defense", "Keeps, towers, defensive walls", "Quarry - Granite", "granite deposit");
            g.BonusAdditive[Ppb.Defense] = 2.0m;
            }
            { var g = G("clay", TradeGoodSection.Mines, "Clay", "Clay from pits; raw material for bricks and pottery.",
                "+1 Production, +1 Economy", "Potter's workshop, Brickyard", "Clay pit", "clay deposit");
            g.BonusAdditive[Ppb.Production] = 1.0m;
            g.BonusAdditive[Ppb.Economy] = 1.0m;
            }
            { var g = G("bricks", TradeGoodSection.Crafts, "Bricks", "Fired brick for durable masonry.",
                "+1 Production, +1 Defense", "Masonry buildings and walls", "Brickyard", "clay deposit");
            g.BonusAdditive[Ppb.Production] = 1.0m;
            g.BonusAdditive[Ppb.Defense] = 1.0m;
            }
            { var g = G("ceramics", TradeGoodSection.Crafts, "Ceramics", "Pots, tiles, and glazed wares.",
                "+1 Culture, +1 Loyalty", "", "Potter's workshop", "clay deposit");
            g.BonusAdditive[Ppb.Culture] = 1.0m;
            g.BonusAdditive[Ppb.Loyalty] = 1.0m;
            }
            { var g = G("shipbuilding-wood", TradeGoodSection.Wood, "Shipbuilding timber", "Timber for hulls, masts, and shipyards.",
                "+1 Production, +1 Defense", "Shipyards", "Sawmill - Shipbuilding wood", "shipbuilding wood");
            g.BonusAdditive[Ppb.Production] = 1.0m;
            g.BonusAdditive[Ppb.Defense] = 1.0m;
            }
            { var g = G("ironwood", TradeGoodSection.Wood, "Ironwood", "Very hard, rare timber for superior crafts and structures.",
                "+1 Production, +1 Defense", "Weapons (good quality)", "Sawmill - Ironwood", "ironwood forest");
            g.BonusAdditive[Ppb.Production] = 1.0m;
            g.BonusAdditive[Ppb.Defense] = 1.0m;
            }
            { var g = G("elven-alder", TradeGoodSection.Wood, "Elven alder", "Rare bow- and luxury-grade wood from deep forests.",
                "+1 Culture, +1 Magic", "Superior bows", "Sawmill - Elven alder", "elven alder");
            g.BonusAdditive[Ppb.Culture] = 1.0m;
            g.BonusAdditive[Ppb.Magic] = 1.0m;
            }
            { var g = G("herbs-roots", TradeGoodSection.Wood, "Herbs & roots", "Medicinal and alchemical plants from woods and gardens.",
                "+1 Science, +1 Magic", "", "Herbalist / Alchemist's Workshop", "dense forest");
            g.BonusAdditive[Ppb.Science] = 1.0m;
            g.BonusAdditive[Ppb.Magic] = 1.0m;
            }
            { var g = G("paper", TradeGoodSection.Crafts, "Paper", "Paper for records, letters, and scriptoria.",
                "+1 Science, +1 Intelligence", "Scriptorium", "Paper mill", null);
            g.BonusAdditive[Ppb.Science] = 1.0m;
            g.BonusAdditive[Ppb.Intelligence] = 1.0m;
            }
            { var g = G("glass", TradeGoodSection.Crafts, "Glass", "Glass for windows, vessels, and instruments.",
                "+1 Culture, +1 Science", "", "Glassworks", "sand/gravel pit");
            g.BonusAdditive[Ppb.Culture] = 1.0m;
            g.BonusAdditive[Ppb.Science] = 1.0m;
            }
            { var g = G("spices", TradeGoodSection.Exotic, "Spices", "Pepper, ginger, cinnamon, and similar distant imports.",
                "+2 Economy", "", "Import", null);
            g.BonusAdditive[Ppb.Economy] = 2.0m;
            }
            { var g = G("sugar", TradeGoodSection.Exotic, "Sugar", "Cane sugar; costly sweetener and preserves.",
                "+1 Culture, +1 Loyalty", "", "Import", null);
            g.BonusAdditive[Ppb.Culture] = 1.0m;
            g.BonusAdditive[Ppb.Loyalty] = 1.0m;
            }
            { var g = G("amber", TradeGoodSection.Crafts, "Amber", "Fossil resin from shores; jewelry and charms.",
                "+1 Culture, +1 Magic", "Jeweler", "Amber gatherer", "amber coast");
            g.BonusAdditive[Ppb.Culture] = 1.0m;
            g.BonusAdditive[Ppb.Magic] = 1.0m;
            }
            { var g = G("ivory", TradeGoodSection.Exotic, "Ivory & walrus bone", "Carving stock for luxury goods and ornament.",
                "+1 Culture, +1 Production", "", "Import", null);
            g.BonusAdditive[Ppb.Culture] = 1.0m;
            g.BonusAdditive[Ppb.Production] = 1.0m;
            }
            { var g = G("horses", TradeGoodSection.Military, "Horses", "Riding stock and light cavalry mounts.",
                "+1 Defense, +1 Production", "Light cavalry", "Horse Stud (regular)", "fertile land");
            g.BonusAdditive[Ppb.Defense] = 1.0m;
            g.BonusAdditive[Ppb.Production] = 1.0m;
            }
            { var g = G("war-horses", TradeGoodSection.Military, "War horses", "Trained chargers for medium and heavy cavalry.",
                "+2 Defense", "Medium and heavy cavalry", "Horse Stud (military)", null);
            g.BonusAdditive[Ppb.Defense] = 2.0m;
            }
            { var g = G("noble-horses", TradeGoodSection.Military, "Noble horses", "Show and tourney breeds.",
                "+1 Culture, +1 Loyalty", "", "Horse Stud (noble)", null);
            g.BonusAdditive[Ppb.Culture] = 1.0m;
            g.BonusAdditive[Ppb.Loyalty] = 1.0m;
            }
            { var g = G("sulfur", TradeGoodSection.Mines, "Sulfur", "Sulfur from mines; gunpowder and alchemy.",
                "+1 Production, +1% Food", "Gunsmith workshop", "Mine - Sulfur", "sulfur deposit");
            g.BonusAdditive[Ppb.Production] = 1.0m;
            g.BonusPercent[Ppb.Food] = 1.0m;
            }
            { var g = G("saltpeter", TradeGoodSection.Crafts, "Saltpeter", "Saltpeter for gunpowder; often produced near towns.",
                "+1 Production, +1% Food", "Gunsmith workshop", "Saltpeter works", "town");
            g.BonusAdditive[Ppb.Production] = 1.0m;
            g.BonusPercent[Ppb.Food] = 1.0m;
            }
            { var g = G("access-arms-military", TradeGoodSection.Military, "Military weapons", "Standing access to military-grade arms: swords, polearms, war bows.",
                "+2 Defense", "Units: military weapons", "Forge", "iron deposit");
            g.BonusAdditive[Ppb.Defense] = 2.0m;
            }
            { var g = G("access-arms-firearms", TradeGoodSection.Military, "Firearms", "Access to powder weapons: muskets and arquebuses.",
                "+2 Defense", "Units: powder weapons", "Gunsmith workshop", "sulfur, saltpeter");
            g.BonusAdditive[Ppb.Defense] = 2.0m;
            }
            { var g = G("access-armor-light", TradeGoodSection.Military, "Light armor", "Leather, caps, and padded protection.",
                "+2 Defense", "Units: light armor", "Tannery", "leather");
            g.BonusAdditive[Ppb.Defense] = 2.0m;
            }
            { var g = G("access-armor-medium", TradeGoodSection.Military, "Medium armor", "Mail, cuirasses, and brigandine.",
                "+2 Defense", "Units: medium armor", "Armorer", "leather, iron");
            g.BonusAdditive[Ppb.Defense] = 2.0m;
            }
            { var g = G("access-armor-heavy", TradeGoodSection.Military, "Heavy armor", "Plate and partial plate harness.",
                "+2 Defense", "Units: heavy armor", "Plate Workshop", "leather, iron");
            g.BonusAdditive[Ppb.Defense] = 2.0m;
            }
            { var g = G("obsidian", TradeGoodSection.Mines, "Obsidian", "Volcanic glass; sharp tools and ritual blades.",
                "+1 Magic, +1 Production", "", "Quarry - Obsidian", "obsidian deposit");
            g.BonusAdditive[Ppb.Magic] = 1.0m;
            g.BonusAdditive[Ppb.Production] = 1.0m;
            }
            { var g = G("tarnit", TradeGoodSection.Mines, "Tarnit", "Rare stone prized for the strongest fortifications.",
                "+1 Magic, +1 Defense", "Top-tier fortifications", "Quarry - Tarnit", "tarnit deposit");
            g.BonusAdditive[Ppb.Magic] = 1.0m;
            g.BonusAdditive[Ppb.Defense] = 1.0m;
            }
            { var g = G("dagoferryt", TradeGoodSection.Mines, "Dagoferryt", "Rare metal for the finest weapons and armor.",
                "+2 Defense", "Weapons (good quality)", "Mine - Dagoferryt", "dagoferryt deposit");
            g.BonusAdditive[Ppb.Defense] = 2.0m;
            }
            { var g = G("elf-forest-goods", TradeGoodSection.Exotic, "Elven forest crafts", "Finished luxury goods from distant elven woods.",
                "+1 Culture, +1 Magic", "", "Import", null);
            g.BonusAdditive[Ppb.Culture] = 1.0m;
            g.BonusAdditive[Ppb.Magic] = 1.0m;
            }

            ApplyVisuals();
        }

        private static void ApplyVisuals()
        {
            void Set(string key, string icon, string color)
            {
                var g = Find(key);
                if (g is null)
                    return;
                g.IconUrl = icon.StartsWith('/') ? icon : "/icons/" + icon;
                g.ColorHex = color;
            }

            // Mines / stone — align with TerrainResource where possible
            Set("soft-metals", "copper.svg", "#e67e22");
            Set("iron", "metal-bar.svg", "#2e86c1");
            Set("silver", "metal-bar.svg", "#c0c7ce");
            Set("gold", "metal-bar.svg", "#d4af37");
            Set("dagoferryt", "metal-bar.svg", "#8e44ad");
            Set("building-stone", "stone-block-stroke.svg", "#f4f1ea");
            Set("granite", "stone-block.svg", "#7f8c8d");
            Set("tarnit", "stone-block.svg", "#9b59b6");
            Set("obsidian", "silex.svg", "#1c1c1c");
            Set("sulfur", "silex.svg", "#e6c200");
            Set("clay", "coal-pile.svg", "#c4783a");
            Set("salt", "coal-pile-stroke.svg", "#f8f8ff");
            Set("gemstones", "emerald.svg", "#9b59b6");

            // Wood
            Set("shipbuilding-wood", "wood-pile.svg", "#5c3317");
            Set("ironwood", "wood-pile.svg", "#2471a3");
            Set("elven-alder", "oak.svg", "#d4af37");
            Set("herbs-roots", "apothecary.svg", "#3d8b57");
            Set("honey-wax", "honeycomb.svg", "#d4a017");

            // Food
            Set("fish-meat-salted", "fishing.svg", "#1a9bb5");
            Set("cattle", "cow.svg", "#8b6914");
            Set("cheese", "cheese-wedge.svg", "#e8c547");
            Set("wine", "grapes.svg", "#722f37");
            Set("beer", "beer-stein.svg", "#c9a227");
            Set("olive-oil", "porcelain-vase.svg", "#9caf00");

            // Fibers
            Set("wool", "wool.svg", "#e8e0d0");
            Set("sheep", "wool.svg", "#c4b59a");
            Set("flax-hemp", "flax.svg", "#6d8f6a");
            Set("cloth", "rolled-cloth.svg", "#5b7c99");
            Set("cotton", "cotton-flower.svg", "#f5f0e6");
            Set("woad", "three-leaves.svg", "#2e6bb0");
            Set("madder", "root-tip.svg", "#a83c3c");
            Set("weld", "vine-flower.svg", "#d4a017");
            Set("leather", "rolled-cloth.svg", "#8b4513");
            Set("furs", "animal-hide.svg", "#5c4033");
            Set("silk", "sparkles.svg", "#c9a0dc");

            // Crafts
            Set("dyes", "powder-bag.svg", "#9b2d5a");
            Set("bricks", "brick-pile.svg", "#b55239");
            Set("ceramics", "porcelain-vase.svg", "#a0522d");
            Set("paper", "papers.svg", "#d9cfb8");
            Set("glass", "wine-glass.svg", "#7ec8e3");
            Set("amber", "emerald.svg", "#ffbf00");
            Set("saltpeter", "powder-bag.svg", "#e8e8e0");

            // Exotic
            Set("spices", "powder-bag.svg", "#c45c26");
            Set("sugar", "sugar-cane.svg", "#f2e6c8");
            Set("ivory", "ivory-tusks.svg", "#f5f0e1");
            Set("elf-forest-goods", "elf-helmet.svg", "#2e8b57");

            // Arms & horses
            Set("horses", "horse-head.svg", "#8b6914");
            Set("war-horses", "horse-head.svg", "#4a3728");
            Set("noble-horses", "horse-head.svg", "#d4af37");
            Set("access-arms-military", "axe-sword.svg", "#5a6a7a");
            Set("access-arms-firearms", "musket.svg", "#3d3d3d");
            Set("access-armor-light", "leather-armor.svg", "#a67c52");
            Set("access-armor-medium", "armor-vest.svg", "#6e7f8d");
            Set("access-armor-heavy", "breastplate.svg", "#4a5560");
        }

        public static string IconUrl(string? key) => Find(key)?.IconUrl ?? "/icons/wooden-crate.svg";

        public static string ColorHex(string? key) => Find(key)?.ColorHex ?? "#888888";

        /// <summary>Near-white mask fills that still use CSS drop-shadow outlines.</summary>
        public static bool NeedsDarkOutline(string? key) =>
            string.Equals(key, "silver", StringComparison.OrdinalIgnoreCase)
            || string.Equals(key, "wool", StringComparison.OrdinalIgnoreCase)
            || string.Equals(key, "sheep", StringComparison.OrdinalIgnoreCase)
            || string.Equals(key, "cotton", StringComparison.OrdinalIgnoreCase)
            || string.Equals(key, "paper", StringComparison.OrdinalIgnoreCase)
            || string.Equals(key, "sugar", StringComparison.OrdinalIgnoreCase)
            || string.Equals(key, "ivory", StringComparison.OrdinalIgnoreCase)
            || string.Equals(key, "saltpeter", StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Ready-made SVG with black stroke — must render as &lt;img&gt;, not a CSS mask.
        /// </summary>
        public static bool UsesBakedStrokeIcon(string? key) =>
            string.Equals(key, "building-stone", StringComparison.OrdinalIgnoreCase)
            || string.Equals(key, "salt", StringComparison.OrdinalIgnoreCase);

        public static TradeGoodEntry? Find(string? key) =>
            string.IsNullOrWhiteSpace(key) ? null : _all.FirstOrDefault(x => string.Equals(x.Key, key, StringComparison.OrdinalIgnoreCase));

        public static IEnumerable<TradeGoodEntry> BySection(string sectionKey) =>
            _all.Where(x => x.SectionKey == sectionKey);
    }

    public static class TradeGoodsBonusAggregator
    {
        public static void Sum(IEnumerable<TradeGoodEntry> goods, out PpbVector additive, out PpbVector percent)
        {
            additive = new PpbVector();
            percent = new PpbVector();
            foreach (var g in goods)
            {
                additive.AddInPlace(g.BonusAdditive);
                percent.AddInPlace(g.BonusPercent);
            }
        }
    }
}
