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
                "+5% Food", "Salted fish & meat", "Mine - Salt", "salt deposit");
            g.BonusPercent[Ppb.Food] = 5.0m;
            }
            { var g = G("fish-meat-salted", TradeGoodSection.Food, "Salted fish & meat", "Fish and meat cured with salt; keeps well for storage and export.",
                "+1 Defense, +1 Production", "", "Meat saltery (salt + fish/meat)", "salt deposit + fish");
            g.BonusAdditive[Ppb.Defense] = 1.0m;
            g.BonusAdditive[Ppb.Production] = 1.0m;
            }
            { var g = G("cattle", TradeGoodSection.Food, "Cattle", "Herds on pasture; meat, hides, wool, and draft power.",
                "+1 Economy, +1 Production", "Meat, hides, cheese", "Pastures (cattle)", "fertile land (3+)");
            g.BonusAdditive[Ppb.Economy] = 1.0m;
            g.BonusAdditive[Ppb.Production] = 1.0m;
            }
            { var g = G("cheese", TradeGoodSection.Food, "Cheese", "Dairy from pastoral herds; staple food and local trade.",
                "+1 Culture, +1 Economy", "", "Cheese dairy", "cattle");
            g.BonusAdditive[Ppb.Culture] = 1.0m;
            g.BonusAdditive[Ppb.Economy] = 1.0m;
            }
            { var g = G("honey-wax", TradeGoodSection.Wood, "Honey & wax", "Honey and beeswax from apiaries; sweetener, candles, and crafts.",
                "+1 Loyalty, +1 Science", "", "Apiary", "rich forest (3+)");
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
                "+1 Production, +1 Economy", "Cloth", "Sheep pastures", "fertile terrain (2+)");
            g.BonusAdditive[Ppb.Production] = 1.0m;
            g.BonusAdditive[Ppb.Economy] = 1.0m;
            }
            { var g = G("flax-hemp", TradeGoodSection.Fibers, "Flax & hemp", "Fiber for linen, rope, and sails.",
                "+1 Production, +1 Stability", "Cloth", "Flax farm", "fertile terrain (2+)");
            g.BonusAdditive[Ppb.Production] = 1.0m;
            g.BonusAdditive[Ppb.Stability] = 1.0m;
            }
            { var g = G("cloth", TradeGoodSection.Fibers, "Cloth", "Finished cloth for clothing, uniforms, and export.",
                "+1 Economy, +1 Culture", "Dyed cloth", "Weaver's workshop", "flax / wool / cotton");
            g.BonusAdditive[Ppb.Economy] = 1.0m;
            g.BonusAdditive[Ppb.Culture] = 1.0m;
            }
            { var g = G("cotton", TradeGoodSection.Fibers, "Cotton", "Raw cotton; grows in warm, fertile fields.",
                "+1 Production, +1 Loyalty", "Dyed cloth", "Cotton farm", "fertile land (3+), warm climate");
            g.BonusAdditive[Ppb.Production] = 1.0m;
            g.BonusAdditive[Ppb.Loyalty] = 1.0m;
            }
            { var g = G("leather", TradeGoodSection.Fibers, "Leather", "Tanned hides for footwear, straps, and light armor.",
                "+1 Production, +1 Defense", "Light armor", "Tannery", "cattle");
            g.BonusAdditive[Ppb.Production] = 1.0m;
            g.BonusAdditive[Ppb.Defense] = 1.0m;
            }
            { var g = G("furs", TradeGoodSection.Fibers, "Furs", "Furs from hunts and cold country; luxury and winter wear.",
                "+1 Culture, +1 Intelligence", "", "Import", null);
            g.BonusAdditive[Ppb.Culture] = 1.0m;
            g.BonusAdditive[Ppb.Intelligence] = 1.0m;
            }
            { var g = G("dyes", TradeGoodSection.Crafts, "Dyes", "Dyes and fixatives for cloth and banners.",
                "+1 Culture, +1 Production", "Dyed cloth", "Dyer's workshop", "dye source (woad etc.)");
            g.BonusAdditive[Ppb.Culture] = 1.0m;
            g.BonusAdditive[Ppb.Production] = 1.0m;
            }
            { var g = G("silk", TradeGoodSection.Fibers, "Silk", "Silk thread and fabric; expensive luxury import.",
                "+2 Culture", "", "Import", null);
            g.BonusAdditive[Ppb.Culture] = 2.0m;
            }
            { var g = G("soft-metals", TradeGoodSection.Mines, "Soft metals (Cu, Sn, Pb)", "Copper, tin, and lead for tools, alloys, and simple weapons.",
                "+1 Production", "Simple weapons (poor quality)", "Mine - soft metals", "soft metals deposit");
            g.BonusAdditive[Ppb.Production] = 1.0m;
            }
            { var g = G("iron", TradeGoodSection.Mines, "Iron", "Iron ore and blooms; foundation of tools and military arms.",
                "+1 Production, +1 Defense", "Military weapons, normal quality, Blacksmith Workshop", "Mine - Iron", "iron deposit");
            g.BonusAdditive[Ppb.Production] = 1.0m;
            g.BonusAdditive[Ppb.Defense] = 1.0m;
            }
            { var g = G("silver", TradeGoodSection.Mines, "Silver", "Silver for coinage, jewelry, and treasury.",
                "+1 Economy, +1 Culture", "Mint, jeweler", "Mine - Silver", "silver deposit");
            g.BonusAdditive[Ppb.Economy] = 1.0m;
            g.BonusAdditive[Ppb.Culture] = 1.0m;
            }
            { var g = G("gold", TradeGoodSection.Mines, "Gold", "Precious metal for coins, regalia, and prestige.",
                "+1 Economy, +1 Culture", "Mint, jeweler", "Mine - Gold", "gold deposit");
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
                "+1 Production, +1 Economy", "Pottery workshop, brickyard", "Clay pit", "clay deposit");
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
                "+1 Culture, +1 Magic", "", "Sawmill - Elven alder", "elven alder");
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
                "+1 Defense, +1 Production", "Light cavalry", "Stables", "fertile land");
            g.BonusAdditive[Ppb.Defense] = 1.0m;
            g.BonusAdditive[Ppb.Production] = 1.0m;
            }
            { var g = G("war-horses", TradeGoodSection.Military, "War horses", "Trained chargers for medium and heavy cavalry.",
                "+2 Defense", "Medium and heavy cavalry", "Stables", null);
            g.BonusAdditive[Ppb.Defense] = 2.0m;
            }
            { var g = G("noble-horses", TradeGoodSection.Military, "Noble horses", "Show and tourney breeds.",
                "+1 Culture, +1 Loyalty", "", "Stables", null);
            g.BonusAdditive[Ppb.Culture] = 1.0m;
            g.BonusAdditive[Ppb.Loyalty] = 1.0m;
            }
            { var g = G("sulfur", TradeGoodSection.Mines, "Sulfur", "Sulfur from mines; gunpowder and alchemy.",
                "+1 Production, +1% Food", "Firearms", "Mine - Sulfur", "sulfur deposit");
            g.BonusAdditive[Ppb.Production] = 1.0m;
            g.BonusPercent[Ppb.Food] = 1.0m;
            }
            { var g = G("saltpeter", TradeGoodSection.Crafts, "Saltpeter", "Saltpeter for gunpowder; often produced near towns.",
                "+1 Production, +1% Food", "Firearms", "Saltpeter works", "town");
            g.BonusAdditive[Ppb.Production] = 1.0m;
            g.BonusPercent[Ppb.Food] = 1.0m;
            }
            { var g = G("access-arms-military", TradeGoodSection.Military, "Military weapons", "Standing access to military-grade arms: swords, polearms, war bows.",
                "+2 Defense", "Units: military weapons", "Blacksmith Workshop", "iron deposit");
            g.BonusAdditive[Ppb.Defense] = 2.0m;
            }
            { var g = G("access-arms-firearms", TradeGoodSection.Military, "Firearms", "Access to powder weapons: muskets and arquebuses.",
                "+2 Defense", "Units: powder weapons", "Gunsmith workshop", "sulfur, saltpeter");
            g.BonusAdditive[Ppb.Defense] = 2.0m;
            }
            { var g = G("access-armor-light", TradeGoodSection.Military, "Light armor", "Leather, caps, and padded protection.",
                "+2 Defense", "Light armor", "Tannery", "leather");
            g.BonusAdditive[Ppb.Defense] = 2.0m;
            }
            { var g = G("access-armor-medium", TradeGoodSection.Military, "Medium armor", "Mail, cuirasses, and brigandine.",
                "+2 Defense", "Medium armor", "Armorer (plate worker)", "leather, iron");
            g.BonusAdditive[Ppb.Defense] = 2.0m;
            }
            { var g = G("access-armor-heavy", TradeGoodSection.Military, "Heavy armor", "Plate and partial plate harness.",
                "+2 Defense", "Heavy armor", "Armorer (plate worker)", "leather, iron");
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
                "+2 Defense", "Good-quality weapons", "Mine - Dagoferryt", "dagoferryt deposit");
            g.BonusAdditive[Ppb.Defense] = 2.0m;
            }
            { var g = G("elf-forest-goods", TradeGoodSection.Exotic, "Elven forest crafts", "Finished luxury goods from distant elven woods.",
                "+1 Culture, +1 Magic", "", "Import", null);
            g.BonusAdditive[Ppb.Culture] = 1.0m;
            g.BonusAdditive[Ppb.Magic] = 1.0m;
            }

        }

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
