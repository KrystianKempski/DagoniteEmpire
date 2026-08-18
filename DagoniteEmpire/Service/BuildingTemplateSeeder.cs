using System.Text.Json;
using DA_Common.Barony;
using DA_DataAccess.BaronyData;

namespace DagoniteEmpire.Service
{
    /// <summary>Global building / terrain-improvement catalog (shared by all baronies).</summary>
    public static class BuildingTemplateSeeder
    {
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

        public static IReadOnlyList<BuildingTemplate> CreateAll()
        {
            var list = new List<BuildingTemplate>();
            void Add(
                string name,
                int lordship,
                string kind,
                decimal productionCost = 0m,
                decimal goldCost = 0m,
                string? description = null,
                PpbVector? additive = null,
                string? terrainRequirement = null)
            {
                var fx = additive ?? new PpbVector();
                fx.EnsureSize();
                list.Add(new BuildingTemplate
                {
                    Name = name,
                    RequiredLordshipLevel = lordship,
                    Kind = kind,
                    ProductionCost = productionCost,
                    GoldCost = goldCost,
                    Description = description,
                    TerrainRequirement = terrainRequirement,
                    EffectAdditiveJson = JsonSerializer.Serialize(fx, JsonOptions),
                    EffectPercentJson = JsonSerializer.Serialize(new PpbVector(), JsonOptions),
                });
            }

            static PpbVector Fx(
                decimal? food = null, decimal? economy = null, decimal? production = null,
                decimal? loyalty = null, decimal? stability = null, decimal? law = null,
                decimal? corruption = null, decimal? science = null, decimal? magic = null,
                decimal? culture = null, decimal? intelligence = null, decimal? defense = null,
                decimal? treasury = null)
            {
                var v = new PpbVector();
                if (food.HasValue) v[Ppb.Food] = food.Value;
                if (economy.HasValue) v[Ppb.Economy] = economy.Value;
                if (production.HasValue) v[Ppb.Production] = production.Value;
                if (loyalty.HasValue) v[Ppb.Loyalty] = loyalty.Value;
                if (stability.HasValue) v[Ppb.Stability] = stability.Value;
                if (law.HasValue) v[Ppb.Law] = law.Value;
                if (corruption.HasValue) v[Ppb.Corruption] = corruption.Value;
                if (science.HasValue) v[Ppb.Science] = science.Value;
                if (magic.HasValue) v[Ppb.Magic] = magic.Value;
                if (culture.HasValue) v[Ppb.Culture] = culture.Value;
                if (intelligence.HasValue) v[Ppb.Intelligence] = intelligence.Value;
                if (defense.HasValue) v[Ppb.Defense] = defense.Value;
                if (treasury.HasValue) v[Ppb.Treasury] = treasury.Value;
                return v;
            }

            const string B = BuildingKind.Building;
            const string I = BuildingKind.Improvement;

            Add("Alchemist's Workshop", 1, B, 80, 160,
                "A learned alchemist's workshop—research plus simple potions and ingredients for sale.",
                Fx(economy: 1, production: 1, science: 2, magic: 1, treasury: -5));

            Add("Bakery", 1, B, 50, 50,
                "A large bakery producing bread for the whole town.",
                Fx(economy: 1, loyalty: 2, stability: 2, treasury: 5));

            Add("Barracks", 1, B, 200, 100,
                "Barracks for housing and training professional soldiers. Can billet up to 3 military units and provides a free city guard unit.",
                Fx(food: -1, stability: 6, law: 4, corruption: 1, defense: 20, treasury: -60));

            Add("Black Market", 1, B, 50, 50,
                "A shady venue for illegal or stolen goods. Causes trouble but generates significant income.",
                Fx(economy: 2, production: 1, stability: -3, law: -4, corruption: -1, defense: 5, treasury: 20));

            Add("Brewery", 1, B, 50, 80,
                "A town brewery that malts grain into ale and beer for markets, inns, and garrisons. Reliable drink lifts spirits and keeps the common folk loyal when harvests are thin.\nProduces Beer",
                Fx(economy: 1, loyalty: 2, stability: 2, treasury: 10));

            Add("Brickyard", 1, B, 100, 40,
                "Kilns and drying yards that fire clay into durable brick for walls, chimneys, and paved courts. Requires clay from a pit; finished brick opens masonry works that timber alone cannot match.\nProduces Bricks",
                Fx(economy: 1, production: 4, defense: 4));

            Add("Bridge (Building)", 1, B, 200, 200,
                "Inside the city. Eases moving people and goods across the river—a good defensive point or an easy target during attack.",
                Fx(economy: 2, production: 2, loyalty: 1, stability: 3, defense: 3, treasury: -2));

            Add("Builders' Guild", 1, B, 150, 150,
                "Allows building one extra structure per turn, plus bridges and complex works requiring an architect. Upkeep: -5.",
                Fx(economy: 2, production: 10, law: 1, science: 5, culture: 3, defense: 2, treasury: -5));

            Add("Candlemaker", 1, B, 40, 40,
                "A workshop that renders beeswax and tallow into candles for homes, halls, and shrines. It does not create a trade good by itself, but turns Honey & Wax into everyday light and ritual goods.",
                Fx(economy: 2, science: 2, culture: 2, treasury: 20));

            Add("Cemetery", 1, B, 50, 30,
                "A place to bury the dead; may prevent corpses piling up outside the city and the epidemics that follow.",
                Fx(stability: 1));

            Add("Cheese dairy", 1, B, 80, 50,
                "A dairy house where milk from cattle herds is turned into cheese for storage and sale. Needs cattle nearby; the dairy adds culture and comfort beyond raw meat and hides.\nProduces Cheese",
                Fx(food: 0.5m, economy: 1, loyalty: 1, stability: 1, culture: 1, treasury: 20));

            Add("City Moat", 1, B, 200, 80,
                "A moat improves defense but may stink when used as an open sewer.",
                Fx(stability: 1, defense: 5, treasury: -1));

            Add("Cobbled Streets", 1, B, 30, 10,
                "Muddy gravel roads replaced with fine cobblestone. Improves city life. Cost: 30 production and 10 gold per 1 community. Side effect: -community/2.",
                Fx(economy: 1, production: 1, loyalty: 1, stability: 1, magic: 1, defense: 1, treasury: 1));

            Add("Dance Hall", 1, B, 40, 40,
                "A large plank hall where crowds can dance until dawn—always warm and dry.",
                Fx(loyalty: 2, stability: 1, magic: 1, treasury: -2));

            Add("Defensive Granary", 1, B, 30, 15,
                "A stone building with a flat roof, canopy, reinforced gates, stone stockpile, and signal fire— a fallback strongpoint for several farms or a small hamlet. Increases barony storage by 5. Can shelter a community of size 3 for a few days.",
                Fx(loyalty: 1, defense: 3, treasury: -2));

            Add("Dyer's workshop", 1, B, 120, 100,
                "Vats and drying racks for coloring cloth, banners, and uniforms. Needs woad, madder, or weld; the smell and runoff bother neighbors, but dyed goods command far better prices than plain weave.\nProduces Dyes",
                Fx(economy: 3, production: 1, loyalty: -2, stability: -2, law: -1, culture: 3, treasury: 30));

            Add("Fletcher's Workshop", 1, B, 40, 80,
                "Produces bows from hunting pieces to longbows for the army.",
                Fx(economy: 2, production: 3, science: 2, defense: 5));

            Add("Glassworks", 1, B, 100, 150,
                "A hot furnace turning sand and cullet into window glass, vessels, and lenses. Costly to build and fuel, but glass marks a town as cultured and useful to scholars.\nProduces Glass",
                Fx(economy: 2, production: 1, science: 2, culture: 2, defense: 1, treasury: 30));

            Add("Granary", 1, B, 50, 30,
                "Stores food and distributes it in lean months. Hoarding much food in one place can be dangerous… Capacity: 20 food; 1 food point may carry to the next year.",
                Fx(food: 1, economy: 1, stability: 2, defense: -1, treasury: -3));

            Add("Gunsmith workshop", 1, B, 200, 300,
                "A rare, expensive shop for muskets, arquebuses, and powder fittings. Needs sulfur and saltpeter in the supply chain; once running, it arms units with powder weapons.\nProduces Firearms",
                Fx(economy: 1, production: 3, loyalty: 1, science: 2, culture: 1, defense: 8));

            Add("Herbalist / Alchemist's Workshop", 1, B, 80, 60,
                "A herbalist's bench and simple alchemist's gear for salves, simples, and field reagents. Draws on dense woods or gardens for roots and herbs that heal, research, and trade.\nProduces Herbs & Roots",
                Fx(stability: 2, science: 1));

            Add("Inn", 1, B, 80, 120,
                "A proper inn for travelers and townsfolk—better rooms, better drink, and louder talk.",
                Fx(economy: 4, loyalty: 6, stability: 2, law: -1, intelligence: 5, treasury: 15));

            Add("Landfill", 1, B, 100, 30,
                "Usually outside the walls; necessary when city population and technology exceed a certain level.",
                Fx(stability: 2, law: -1));

            Add("Large Temple (Thyrus)", 3, B, 500, 2000,
                "A temple to Thyrus. Bonuses to law, corruption, and defense.",
                Fx(economy: 8, production: -8, loyalty: 10, stability: 15, law: 8, corruption: -3, science: 10, magic: 8, culture: 25, defense: 25, treasury: 25));

            Add("Market Square", 1, B, 30, 40,
                "A small paved place where local producers and nearby merchants can exchange goods.",
                Fx(economy: 3, production: 3, treasury: 10));

            Add("Marketplace", 1, B, 50, 50,
                "A crowded square of merchants, buyers, and pickpockets—the upgraded market of the town.",
                Fx(food: 1, economy: 5, production: 2, law: -2, corruption: 0.5m, science: 2, culture: 3, defense: 3, treasury: 5));

            Add("Meat saltery (salt + fish/meat)", 1, B, 40, 40,
                "A salting house where fish and meat are cured for winter stores and long-distance carts. Needs salt plus a catch or slaughter; the result keeps far longer than fresh flesh.\nProduces Salted Fish & Meat",
                Fx(food: 0.5m, economy: 1, loyalty: 1, defense: 2, treasury: 10));

            Add("Medium Temple (Thyrus)", 2, B, 130, 350,
                "A temple to Thyrus. Bonuses to law, corruption, and defense.",
                Fx(economy: 4, production: -3, loyalty: 5, stability: 7, law: 3, corruption: -1, science: 5, magic: 3, culture: 10, defense: 10, treasury: 10));

            Add("Mill", 1, B, 40, 50,
                "Bulk grain milling; millers often double as brokers.",
                Fx(food: 1, economy: 3, production: 2, law: -1, treasury: 5));

            Add("Orphanage", 1, B, 150, 100,
                "A home for street children, unwanted youths, and bastards—proof of the ruler's mercy.",
                Fx(food: -1, loyalty: 2, stability: 2, treasury: -10));

            Add("Palace", 2, B, 0, 0,
                "The lord’s residence and seat of local rule—reception halls, chambers, and the daily business of the domain.",
                Fx(economy: 2, production: 2));

            Add("Palisade", 1, B, 100, 50,
                "A few meters of sharpened stakes encircling the settlement.",
                Fx(loyalty: 2, stability: 1, defense: 8));

            Add("Paper mill", 1, B, 150, 150,
                "Beaters and drying lofts that turn pulp into paper for ledgers, letters, and scriptoria. Supports administration, scholarship, and intelligence work far beyond parchment alone.\nProduces Paper",
                Fx(economy: 3, production: 1, loyalty: 1, stability: 1, law: 1, science: 3, magic: 1, culture: 3, intelligence: 1, treasury: 20));

            Add("Park Grove", 1, B, 50, 60,
                "A place to rest, meet, hold simple gatherings, and enjoy light recreation.",
                Fx(loyalty: 1, science: 1, treasury: -1));

            Add("Physician's House", 1, B, 15, 30,
                "Where the poor can receive care—costly, but it earns the gratitude of the populace.",
                Fx(loyalty: 4, stability: 3, treasury: -10));

            Add("Pier", 1, B, 15, 30,
                "A small pier for fishing boats and light trade craft.",
                Fx(food: 0.5m, economy: 2, defense: 2, treasury: 3));

            Add("Potter's workshop", 1, B, 100, 60,
                "Wheels and kilns for pots, tiles, and glazed wares from local clay. Everyday ceramics improve household life and local trade without needing imported luxuries.\nProduces Ceramics",
                Fx(economy: 2, production: 2, loyalty: 1, culture: 1, treasury: 20));

            Add("Prison", 1, B, 150, 100,
                "A small dungeon that stops thugs and thieves; even has a modest torture chamber, though a skilled jailer matters most. Also deters crime and aids defense.",
                Fx(stability: 2, law: 1, corruption: -0.5m, defense: 1, treasury: -5));

            Add("Saltpeter works", 1, B, 40, 40,
                "Beds and sheds where saltpeter is scraped and refined near towns. Unpleasant neighbors and poor loyalty, but essential—with sulfur—for firearms and powder.\nProduces Saltpeter",
                Fx(economy: 1, loyalty: -3, stability: -1, defense: 3));

            Add("Shrine", 1, B, 30, 60,
                "A place of worship; bonuses depend on the god honored.",
                Fx(treasury: -2));

            Add("Small Arena", 1, B, 100, 40,
                "A small fighting ring with stands—entertainment that also stokes aggression.",
                Fx(economy: 2, loyalty: 3, stability: 2, law: -1, science: 1, culture: 1, defense: 3, treasury: 2));

            Add("Small Ocean-River Port", 1, B, 60, 120,
                "A small port that can handle even larger vessels. Generates trade, tolls, gossip, and frustrated sailors. Allows goods from distant lands.",
                Fx(food: 1, economy: 7, production: 2, stability: -2, law: -2, corruption: 1, science: 5, culture: 5, intelligence: 5, defense: 5, treasury: 10));

            Add("Small Temple (Thyrus)", 1, B, 60, 150,
                "A temple to Thyrus. Bonuses to law, corruption, and defense.",
                Fx(economy: 2, production: -1, loyalty: 2, stability: 3, law: 1, corruption: -0.5m, science: 2, magic: 1, culture: 5, defense: 4, treasury: 4));

            Add("Spy Den", 1, B, 20, 150,
                "Spy Den.",
                Fx(loyalty: 2, law: -1, corruption: 2, science: 12, defense: 5, treasury: -5));

            Add("Steward's Building", 1, B, 40, 60,
                "Steward's hut. Locals come here with their affairs before the authorities.",
                Fx(stability: 3, law: 3, corruption: 2, science: 2, magic: 2, intelligence: 2, treasury: -15));

            Add("Tannery", 1, B, 60, 80,
                "A foul but necessary yard where hides become leather for boots, straps, and light armor. The stench hurts stability and law, yet armies and crafts cannot do without it.\nProduces Leather & Light Armor",
                Fx(economy: 2, production: 1, stability: -1, law: -1, defense: 1, treasury: 2));

            Add("Tavern", 1, B, 40, 80,
                "A humble tavern. Meeting place for the peasantry and a rest stop for the few traveling merchants. Can be upgraded to an inn.",
                Fx(economy: 2, loyalty: 3, intelligence: 3));

            Add("Town Garrison", 1, B, 80, 100,
                "A town guard post that keeps order and helps defend the city in wartime.",
                Fx(stability: 3, law: 2, defense: 6, treasury: -25));

            Add("Trade Pier", 1, B, 40, 80,
                "A small pier for merchant and fishing vessels. Generates trade, tolls, gossip, and frustrated sailors. Allows goods from nearby lands.",
                Fx(food: 0.5m, economy: 4, production: 1, law: -1, corruption: 3, science: 1, culture: 2, defense: 3, treasury: 5));

            Add("Watchtower", 1, B, 20, 10,
                "A watchtower on the barony border for spotting enemies or lighting signal fires. A network needs towers roughly 5–10 miles apart on hills, or 5 miles on flat ground. Upkeep: -1.",
                Fx(defense: 2, treasury: -1));

            Add("Weaver's workshop", 1, B, 120, 120,
                "Looms that turn flax, wool, or cotton into finished cloth for clothing and trade. The heart of the textile chain—feeds dyers and raises both economy and culture.\nProduces Cloth",
                Fx(economy: 3, production: 2, loyalty: 1, stability: 1, culture: 2, defense: 2, treasury: 30));

            Add("Well", 1, B, 10, 8,
                "Essential for life; its absence imposes penalties.");

            Add("Wooden City Wall", 1, B, 300, 120,
                "A low wall that can repel small forces and keep out unwanted elements; gives a sense of safety.",
                Fx(loyalty: 1, stability: 3, law: 1, defense: 10));

            Add("Amber gatherer", 1, I, 40, 30,
                "Coastal gatherers and beachcombers collect fossil resin washed up by storms and tides. The work is seasonal and exposed, but amber sells well to jewelers and temple craftsmen.\nProduces Amber",
                Fx(economy: 2, loyalty: 0, stability: -2, law: -1, magic: 1, culture: 2, defense: -2, treasury: 30));

            Add("Apiary", 1, I, 50, 20,
                "Beehives set among flowering meadows or forest clearings. Beekeepers harvest honey for food and trade, and beeswax for candles, seals, and crafts.\nProduces Honey & Wax",
                Fx(food: 0.5m, economy: 1, loyalty: 1, stability: 1, science: 1, treasury: 10));

            Add("Bridge (Improvement)", 1, I, 200, 200,
                "Outside the city. Eases moving people and goods across the river—a good defensive point or an easy target during attack. Can be built alongside other improvements.",
                Fx(economy: 1, production: 1, loyalty: 1, stability: 2));

            Add("Clay pit", 1, I, 50, 40,
                "Open pits where laborers dig clay for bricks, tiles, and pottery. Hard outdoor work that scars the land, but supplies the raw material every ceramic craft depends on.\nProduces Clay",
                Fx(food: -0.3m, economy: 2, production: 4, stability: -2, law: -2, defense: 2, treasury: 10),
                terrainRequirement: "Clay");

            Add("Cotton farm", 1, I, 40, 20,
                "Warm-climate fields growing cotton for yarn and cloth. Needs fertile ground and heat; the crop feeds weavers and dyers once harvested.\nProduces Cotton",
                Fx(economy: 1, production: 1, defense: -1, treasury: 20));

            Add("Defensive Earthwork", 1, I, 100, 50,
                "Defensive Earthwork.",
                Fx(loyalty: 2, stability: 1, defense: 8));

            Add("Farm", 1, I, 40, 20,
                "Peasant fields and huts on moderately fertile land, giving adequate yields and some labor. Distance from the city, flammability, and exposure make them vulnerable.",
                Fx(food: 1.5m, economy: 0.5m, production: 1, defense: -1, treasury: 10));

            Add("Farm - bountiful", 1, I, 40, 20,
                "Peasant fields and huts on exceptionally rich soil, giving outstanding yields and labor. Distance from the city, flammability, and exposure make them vulnerable.",
                Fx(food: 3, economy: 1, production: 1, defense: -1, treasury: 20));

            Add("Farm - fertile", 1, I, 40, 20,
                "Peasant fields and huts on fertile soil, giving strong yields and labor. Distance from the city, flammability, and exposure make them vulnerable.",
                Fx(food: 2, economy: 1, production: 1, defense: -1, treasury: 15));

            Add("Farm - poor fertility", 1, I, 40, 20,
                "Peasant fields and huts on poorly fertile soil, yielding meager crops with little labor. Distance from the city, flammability, and exposure make them vulnerable.",
                Fx(food: 0.8m, production: 0.5m, defense: -1, treasury: 8));

            Add("Fishing Pier", 1, I, 50, 30,
                "Boats and fisher huts providing food from the catch. Requires coast or river. If the tile also has a Fishery deposit: +1 Food and +10 Treasury. Distance from the city makes it vulnerable.",
                Fx(food: 1, economy: 1, defense: -0.5m, treasury: 10));

            Add("Flax farm", 1, I, 40, 20,
                "Fields of flax (and often hemp nearby) retted and dressed into fiber for linen, rope, and sails. A sturdy rural improvement that feeds the weaver's workshop and shipyards alike.\nProduces Flax & Hemp",
                Fx(economy: 1, production: 2, defense: -1, treasury: 15));

            Add("Hunter's Lodge", 1, I, 25, 20,
                "A lodge for hunters who gather furs and meat and scout the nearby woods.",
                Fx(food: 0.7m, economy: 0.5m, production: 0.5m, defense: 3, treasury: 5));

            Add("Hunter's Lodge - Furs", 1, I, 25, 20,
                "A lodge built over a rich fur territory. Trappers and hunters harvest pelts for trade and warm the barony's coffers. Produces Furs.\n+10 Treasury, +2 Culture bonus from the fur deposit.",
                Fx(food: 0.7m, economy: 0.5m, production: 0.5m, defense: 3, treasury: 15, culture: 2));

            Add("Mine - Dagoferryt", 1, I, 200, 120,
                "A hard, dangerous mine for dagoferryt—the rare metal prized for the finest blades and armor. Extraction is costly, but the ore unlocks good-quality weapons beyond ordinary ironwork.\nProduces Dagoferryt",
                Fx(food: -0.3m, economy: 4, production: 8, stability: -3, law: -2, defense: 8, treasury: 80),
                terrainRequirement: "Mine");

            Add("Mine - Gold", 3, I, 120, 100,
                "Deep workings that yield gold for coin, tribute, and prestige. The ore also feeds a jeweler's bench: finished ornaments and regalia turn raw bullion into culture and court display.\nProduces Gold",
                Fx(food: -0.3m, economy: 6, production: 3, stability: -3, law: -6, corruption: -3, science: 3, culture: 2, treasury: 200),
                terrainRequirement: "Mine");

            Add("Mine - Iron", 1, I, 150, 80,
                "Iron pits and bloomery work that feed forges across the barony. Without iron there is no military smithing, blacksmith workshop expansion, or solid tools for builders.\nProduces Iron",
                Fx(food: -0.3m, economy: 3, production: 6, stability: -3, law: -1, defense: 5, treasury: 50),
                terrainRequirement: "Mine");

            Add("Mine - precious gems (luxury)", 3, I, 120, 120,
                "A luxury dig for precious and ornamental stones. Greed follows the haul, but gems sold to jewelers and temples bring enormous treasury and prestige.\nProduces Gemstones",
                Fx(food: -0.3m, economy: 7, production: 1, stability: -3, law: -7, corruption: -4, science: 3, defense: 3, treasury: 300),
                terrainRequirement: "Quarry");

            Add("Mine - Salt", 2, I, 120, 100,
                "Salt workings or brine pans that preserve food and underwrite long-distance trade. Salt unlocks salting houses and keeps armies and towns fed through lean seasons.\nProduces Salt",
                Fx(food: 1, economy: 5, production: 1, stability: -3, law: -2, corruption: -1, science: 1, defense: 2, treasury: 100),
                terrainRequirement: "Salt");

            Add("Mine - Silver", 2, I, 120, 120,
                "Silver veins for coinage, plate, and jewelry. Bullion can go to a mint or to a jeweler for ornaments that raise a court's standing.\nProduces Silver",
                Fx(food: -0.3m, economy: 4, production: 2, stability: -3, law: -4, corruption: -1, science: 1, culture: 1, defense: 1, treasury: 100),
                terrainRequirement: "Mine");

            Add("Mine - soft metals", 1, I, 100, 50,
                "Workings for copper, tin, lead, and similar soft metals. Essential for tools, alloys, fittings, and simple poor-quality weapons when iron is scarce.\nProduces Soft Metals (Cu, Sn, Pb)",
                Fx(food: -0.3m, economy: 2, production: 5, stability: -3, law: -1, defense: 3, treasury: 30),
                terrainRequirement: "Mine");

            Add("Mine - Sulfur", 1, I, 80, 60,
                "A foul sulfur mine—bad air and brittle ground, but vital for alchemy and gunpowder. Together with saltpeter it unlocks firearms and powder weapons for the army.\nProduces Sulfur",
                Fx(economy: 3, loyalty: -1, stability: -3, defense: 3, treasury: 40),
                terrainRequirement: "Sulfur");

            Add("Pastures (cattle)", 1, I, 60, 150,
                "Open grazing for cattle herds that supply meat, hides, and draft strength. Gold cost is high because the herd itself must be bought and stocked—pasture alone does not create cattle.\nRequires trade access to Cattle (breeding stock via treaty, import, or MG). Once built, this site becomes a local source.\nProduces Cattle",
                Fx(food: 0.5m, economy: 2, production: 2, defense: -2, treasury: 20));

            Add("Quarry - common stone", 1, I, 80, 40,
                "A quarry for ordinary building stone—walls, foundations, and roads. Not as hard as granite, but enough for larger buildings and town defenses.\nProduces Building Stone",
                Fx(food: -0.3m, economy: 1, production: 4, stability: -3, law: -2, defense: 2, treasury: 10),
                terrainRequirement: "Quarry");

            Add("Quarry - Granite", 1, I, 100, 50,
                "Hard granite cut for towers, keeps, and serious fortification. Slow and heavy work, but the stone shrugs off siege better than timber or soft rock.\nProduces Granite",
                Fx(food: -0.3m, economy: 1, production: 6, stability: -3, law: -2, defense: 4, treasury: 20),
                terrainRequirement: "Quarry");

            Add("Quarry - Obsidian", 1, I, 100, 70,
                "A volcanic-glass quarry for sharp tools, ritual blades, and rare craft. Dangerous footing and scarce deposits, yet prized by smiths and mystics alike.\nProduces Obsidian",
                Fx(food: -0.3m, economy: 2, production: 4, stability: -3, law: -2, defense: 4, treasury: 50),
                terrainRequirement: "Obsidian");

            Add("Quarry - Tarnit", 1, I, 200, 100,
                "Extraction of tarnit, the heaviest and finest stone for top-tier fortresses. Enormous effort and cost, but nothing else matches it for the strongest walls.\nProduces Tarnit",
                Fx(food: -0.5m, economy: 2, production: 10, stability: -5, law: -2, defense: 8, treasury: 40),
                terrainRequirement: "Quarry");

            Add("Sawmill - common", 1, I, 50, 30,
                "Can only be built in forest; supplies timber for building, crafts, and fuel for harsh northern winters.",
                Fx(food: 0.5m, economy: 1, production: 3, treasury: 10),
                terrainRequirement: "Forest");

            Add("Sawmill - Elven alder", 1, I, 60, 40,
                "A forest mill cutting rare elven alder—superb for bows and luxury carving. Scarce timber brings high prices, culture, and intrigue wherever it is traded.\nProduces Elven Alder",
                Fx(economy: 6, production: 4, stability: -2, law: -4, culture: 2, intelligence: 4, defense: 3, treasury: 150),
                terrainRequirement: "Forest");

            Add("Sawmill - Ironwood", 1, I, 60, 40,
                "Millwork on ironwood: dense, rare trunks that yield superior shafts and structures. Hard on saws and crews, but the timber supports good-quality weapons and elite crafts.\nProduces Ironwood",
                Fx(economy: 3, production: 4, stability: -2, law: -1, defense: 1, treasury: 40),
                terrainRequirement: "Forest");

            Add("Sawmill - Shipbuilding wood", 1, I, 60, 40,
                "A coastal or river mill dressing dense ship timber for hulls, masts, and yards. Without it, shipyards cannot lay proper keels or expand fleets.\nProduces Shipbuilding Timber",
                Fx(economy: 3, production: 4, defense: 2, treasury: 20),
                terrainRequirement: "Forest");

            Add("Sheep pastures", 1, I, 60, 100,
                "Pastures stocked with sheep for wool, meat, and manure on milder slopes. Cheaper than cattle herds, yet the fleece feeds the entire cloth chain.\nRequires trade access to Sheep (breeding stock via treaty, import, or MG). Once built, this site becomes a local source.\nProduces Sheep & Wool",
                Fx(food: 1, economy: 1, production: 1, defense: -2, treasury: 15));

            Add("Unfortified Village", 1, I, 300, 300,
                "A rural settlement without walls: homes, common fields, and local crafts. Soft target in war, but cheap to found.");

            Add("Vineyard", 1, I, 250, 200,
                "Terraced vines on warm hills, tended for wine, feasts, and export. Production cost is high because vines take years to mature before they give a real harvest.\nProduces Wine",
                Fx(food: 0.5m, economy: 2, loyalty: 2, stability: 2, culture: 2, defense: -1, treasury: 40));

            Add("Horse Stud (regular)", 2, I, 80, 200,
                "A stud farm breeding ordinary riding stock for patrols and light cavalry. Gold cost covers buying and stocking the herd—pasture alone does not create horses.\nRequires trade access to Horses (breeding stock via treaty, import, or MG). Once built, this site becomes a local source.\nProduces Horses",
                Fx(food: -1, economy: 3, production: 2, defense: -3, treasury: 50));

            Add("Horse Stud (military)", 3, I, 120, 250,
                "A war-horse stud training chargers for medium and heavy cavalry. Demanding feed and handlers; mounts must be bred and drilled for battle.\nRequires trade access to War horses (breeding stock via treaty, import, or MG). Once built, this site becomes a local source.\nProduces War Horses",
                Fx(food: -2, economy: 3, production: 1, defense: 1, treasury: 50));

            Add("Horse Stud (noble)", 4, I, 200, 500,
                "A prestigious stud for show and tourney breeds favored by the richest lords. Costly to maintain, but the mounts bring culture and prestige.\nRequires trade access to Noble horses (breeding stock via treaty, import, or MG). Once built, this site becomes a local source.\nProduces Noble Horses",
                Fx(food: -2, economy: 5, loyalty: -1, stability: -1, culture: 4, defense: -2, treasury: 70));

            Add("Smithy", 1, B, 40, 60,
                "A small forge for tools, fittings, and simple weapons when soft metals or iron are available. Can be expanded into a full Forge for military-grade arms.\nProduces Simple Weapons",
                Fx(economy: 1, production: 3, science: 1, defense: 3));

            Add("Forge", 2, B, 100, 150,
                "A large military forge for swords, polearms, and war-grade blades. Requires iron or dagoferrite in the supply chain.\nProduces Military Weapons",
                Fx(economy: 3, production: 6, science: 3, defense: 7));

            Add("Armorer", 2, B, 100, 150,
                "A specialized workshop fitting mail, brigandine, and cuirasses. Needs iron or dagoferrite (and leather for straps and padding).\nProduces Medium Armor",
                Fx(economy: 2, production: 3, law: 1, defense: 5));

            Add("Plate Workshop", 3, B, 300, 400,
                "A master plate shop and armorers' guild hall for full and partial plate harness. Requires iron or dagoferrite and skilled plate-workers.\nProduces Heavy Armor",
                Fx(economy: 4, production: 8, defense: 12));

            Add("Farm (Dye plant)", 1, I, 40, 20,
                "Fields planted with the medieval dye triad—woad (blue), madder (red), and weld (yellow). Any of the three crops unlocks a dyer's workshop and feeds its vats.\nProduces Woad, Madder & Weld",
                Fx(economy: 2, production: 2, defense: -1, treasury: 20));

            return list;
        }

        /// <summary>English description strings used as resx keys for Polish UI copy.</summary>
        public static IEnumerable<string> LocalizationKeys() =>
            CreateAll()
                .Select(t => t.Description)
                .Where(d => !string.IsNullOrWhiteSpace(d))
                .Select(d => d!);
    }
}
