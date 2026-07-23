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

            Add("Cemetery", 1, B, 50, 30,
                "A place to bury the dead; may prevent corpses piling up outside the city and the epidemics that follow.",
                Fx(stability: 1));
            Add("Black Market", 1, B, 50, 50,
                "A shady venue for illegal or stolen goods. Causes trouble but generates significant income.",
                Fx(economy: 2, production: 1, stability: -3, law: -4, corruption: -1, defense: 5, treasury: 20));
            Add("Physician's House", 1, B, 15, 30,
                "Where the poor can receive care—costly, but it earns the gratitude of the populace.",
                Fx(loyalty: 4, stability: 3, treasury: -10));
            Add("Farm - poor fertility", 1, I, 40, 20,
                "Peasant fields and huts on poorly fertile soil, yielding meager crops with little labor. Distance from the city, flammability, and exposure make them vulnerable.",
                Fx(food: 0.8m, production: 0.5m, defense: -1, treasury: 8));
            Add("Farm", 1, I, 40, 20,
                "Peasant fields and huts on moderately fertile land, giving adequate yields and some labor. Distance from the city, flammability, and exposure make them vulnerable.",
                Fx(food: 1.5m, economy: 0.5m, production: 1, defense: -1, treasury: 10));
            Add("Farm - fertile", 1, I, 40, 20,
                "Peasant fields and huts on fertile soil, giving strong yields and labor. Distance from the city, flammability, and exposure make them vulnerable.",
                Fx(food: 2, economy: 1, production: 1, defense: -1, treasury: 15));
            Add("Farm - bountiful", 1, I, 40, 20,
                "Peasant fields and huts on exceptionally rich soil, giving outstanding yields and labor. Distance from the city, flammability, and exposure make them vulnerable.",
                Fx(food: 3, economy: 1, production: 1, defense: -1, treasury: 20));
            Add("City Moat", 1, B, 200, 80,
                "A moat improves defense but may stink when used as an open sewer.",
                Fx(stability: 1, defense: 5, treasury: -1));
            Add("Tannery", 1, B, 60, 80,
                "The stench is foul, but it supplies quality leather for subjects and the army.",
                Fx(economy: 2, production: 1, stability: -1, law: -1, defense: 1, treasury: 2));
            Add("Town Garrison", 1, B, 80, 100,
                "A town guard post that keeps order and helps defend the city in wartime. "
                + "Provides one city guard unit. Can be upgraded to barracks.",
                Fx(stability: 3, law: 2, defense: 6, treasury: -25));
            Add("Steward's Building", 1, B, 40, 60,
                "Steward's hut. Locals come here with their affairs before the authorities. "
                + "Officials and tax collectors hold office here. Can be upgraded to a town hall.",
                Fx(stability: 3, law: 3, corruption: 2,
                    science: 2, magic: 2, intelligence: 2, treasury: -15));
            Add("Market Square", 1, B, 30, 40,
                "A small paved place where local producers and nearby merchants can exchange goods. "
                + "Can be upgraded to a marketplace.",
                Fx(economy: 3, production: 3, treasury: 10));
            Add("Quarry - Granite", 1, I, 100, 50,
                "Extracts granite—excellent for fortifications.",
                Fx(food: -0.3m, economy: 1, production: 6, stability: -3, law: -2, defense: 4, treasury: 20),
                terrainRequirement: "Quarry");
            Add("Quarry - common stone", 1, I, 80, 40,
                "Extracts common local stone—not as hard as granite, but good for castles.",
                Fx(food: -0.3m, economy: 1, production: 4, stability: -3, law: -2, defense: 2, treasury: 10),
                terrainRequirement: "Quarry");
            Add("Quarry - Obsidian", 1, I, 100, 70,
                "Extracts obsidian, useful for certain tools and weapons.",
                Fx(food: -0.3m, economy: 2, production: 4, stability: -3, law: -2, defense: 4, treasury: 50),
                terrainRequirement: "Obsidian");
            Add("Quarry - Tarnit", 1, I, 200, 100,
                "Very heavy; extraction takes great effort. The best (and most expensive) building material for fortresses.",
                Fx(food: -0.5m, economy: 2, production: 10, stability: -5, law: -2, defense: 8, treasury: 40),
                terrainRequirement: "Quarry");
            Add("Clay pit", 1, I, 50, 40,
                "Extracts clay at scale—the basic material for bricks and ceramics.",
                Fx(food: -0.3m, economy: 2, production: 4, stability: -2, law: -2, defense: 2, treasury: 10),
                terrainRequirement: "Clay");
            Add("Shrine", 1, B, 30, 60,
                "A place of worship; bonuses depend on the god honored.",
                Fx(treasury: -2));
            Add("Tavern", 1, B, 40, 80,
                "A humble tavern. Meeting place for the peasantry and a rest stop for the few traveling merchants. Can be upgraded to an inn.",
                Fx(economy: 2, intelligence: 3, loyalty: 3));
            Add("Inn", 1, B, 80, 120,
                "A proper inn for travelers and townsfolk—better rooms, better drink, and louder talk.",
                Fx(economy: 4, loyalty: 6, stability: 2, law: -1, intelligence: 5, treasury: 15));
            Add("Mine - precious gems (luxury)", 3, I, 120, 120,
                "Mines extremely valuable gemstones.",
                Fx(food: -0.3m, economy: 7, production: 1, stability: -3, law: -7, corruption: -4, science: 3, defense: 3, treasury: 300),
                terrainRequirement: "Quarry");
            Add("Mine - soft metals", 1, I, 100, 50,
                "Mines soft metals—copper, lead, tin, and the like. Provides income and materials essential for construction.",
                Fx(food: -0.3m, economy: 2, production: 5, stability: -3, law: -1, defense: 3, treasury: 30),
                terrainRequirement: "Mine");
            Add("Mine - Silver", 2, I, 120, 120,
                "Mines silver and copper ores. Good income, but greed and legal problems follow.",
                Fx(food: -0.3m, economy: 4, production: 2, stability: -3, law: -4, corruption: -1, science: 1, culture: 1, defense: 1, treasury: 100),
                terrainRequirement: "Mine");
            Add("Mine - Salt", 2, I, 120, 100,
                "A salt mine.",
                Fx(food: 1, economy: 5, production: 1, stability: -3, law: -2, corruption: -1, science: 1, defense: 2, treasury: 100),
                terrainRequirement: "Salt");
            Add("Mine - Gold", 3, I, 120, 100,
                "A gold mine. Excellent income, but greed and legal trouble follow.",
                Fx(food: -0.3m, economy: 6, production: 3, stability: -3, law: -6, corruption: -3, science: 3, culture: 2, treasury: 200),
                terrainRequirement: "Mine");
            Add("Mine - Iron", 1, I, 150, 80,
                additive: Fx(food: -0.3m, economy: 3, production: 6, stability: -3, law: -1, defense: 5, treasury: 50),
                terrainRequirement: "Mine");
            Add("Mine - Dagoferryt", 1, I, 200, 120,
                "The best metal for forging weapons and armor.",
                Fx(food: -0.3m, economy: 4, production: 8, stability: -3, law: -2, defense: 8, treasury: 80),
                terrainRequirement: "Mine");
            Add("Small Arena", 1, B, 100, 40,
                "A small fighting ring with stands—entertainment that also stokes aggression.",
                Fx(economy: 2, loyalty: 3, stability: 2, law: -1, science: 1, culture: 1, defense: 3, treasury: 2));
            Add("Small Brewery", 1, B, 60, 60,
                "A small brewery that lifts spirits and health and provides modest income. Sufficient for a population up to 15 communities.",
                Fx(economy: 1, loyalty: 2, stability: 2, treasury: 10));
            Add("Mill", 1, B, 40, 50,
                "Bulk grain milling; millers often double as brokers.",
                Fx(food: 1, economy: 3, production: 2, law: -1, treasury: 5));
            Add("Pier", 1, B, 15, 30,
                "A small pier for fishing boats and light trade craft.",
                Fx(food: 0.5m, economy: 2, defense: 2, treasury: 3));
            Add("Bridge (Improvement)", 1, I, 200, 200,
                "Outside the city. Eases moving people and goods across the river—a good defensive point or an easy target during attack. Can be built alongside other improvements.",
                Fx(economy: 1, production: 1, loyalty: 1, stability: 2));
            Add("Bridge (Building)", 1, B, 200, 200,
                "Inside the city. Eases moving people and goods across the river—a good defensive point or an easy target during attack.",
                Fx(economy: 2, production: 2, loyalty: 1, stability: 3, defense: 3, treasury: -2));
            Add("Wooden City Wall", 1, B, 300, 120,
                "A low wall that can repel small forces and keep out unwanted elements; gives a sense of safety.",
                Fx(loyalty: 1, stability: 3, law: 1, defense: 10));
            Add("Unfortified Village", 1, I, 300, 300,
                VillagePpbFormulas.CatalogDescription);
            Add("Palace", 2, B, additive: Fx(economy: 2, production: 2));
            Add("Park Grove", 1, B, 50, 60,
                "A place to rest, meet, hold simple gatherings, and enjoy light recreation.",
                Fx(loyalty: 1, science: 1, treasury: -1));
            Add("Bakery", 1, B, 50, 50,
                "A large bakery producing bread for the whole town.",
                Fx(economy: 1, loyalty: 2, stability: 2, treasury: 5));
            Add("Alchemist's Workshop", 1, B, 80, 160,
                "A learned alchemist's workshop—research plus simple potions and ingredients for sale.",
                Fx(economy: 1, production: 1, science: 2, magic: 1, treasury: -5));
            Add("Trade Pier", 1, B, 40, 80,
                "A small pier for merchant and fishing vessels. Generates trade, tolls, gossip, and frustrated sailors. Allows goods from nearby lands.",
                Fx(food: 0.5m, economy: 4, production: 1, law: -1, science: 1, culture: 2, defense: 3, corruption: 3, treasury: 5));
            Add("Small Ocean-River Port", 1, B, 60, 120,
                "A small port that can handle even larger vessels. Generates trade, tolls, gossip, and frustrated sailors. Allows goods from distant lands.",
                Fx(food: 1, economy: 7, production: 2, stability: -2, law: -2, corruption: 1, science: 5, culture: 5, intelligence: 5, defense: 5, treasury: 10));
            Add("Fishing Pier", 1, I, 50, 30,
                "Boats and fisher huts providing food from the catch. Requires coast or river. If the tile also has a Fishery deposit: +1 Food and +10 Treasury. Distance from the city makes it vulnerable.",
                Fx(food: 1, economy: 1, defense: -0.5m, treasury: 10));
            Add("Marketplace", 1, B, 50, 50,
                "A crowded square of merchants, buyers, and pickpockets—the upgraded market of the town.",
                Fx(food: 1, economy: 5, production: 2, law: -2, corruption: 0.5m, science: 2, culture: 3, defense: 3, treasury: 5));
            Add("Orphanage", 1, B, 150, 100,
                "A home for street children, unwanted youths, and bastards—proof of the ruler's mercy.",
                Fx(food: -1, loyalty: 2, stability: 2, treasury: -10));
            Add("Granary", 1, B, 50, 30,
                "Stores food and distributes it in lean months. Hoarding much food in one place can be dangerous… Capacity: 20 food; 1 food point may carry to the next year.",
                Fx(food: 1, economy: 1, stability: 2, defense: -1, treasury: -3));
            Add("Stables", 1, B, 100, 40,
                "Stables for army, ruler, and wealthy citizens' horses. Required for cavalry (holds 50 army horses).",
                Fx(production: 1, defense: 1, treasury: -5));
            Add("Dance Hall", 1, B, 40, 40,
                "A large plank hall where crowds can dance until dawn—always warm and dry.",
                Fx(loyalty: 2, stability: 1, magic: 1, treasury: -2));
            Add("Sawmill - Ironwood", 1, I, 60, 40,
                additive: Fx(economy: 3, production: 4, stability: -2, law: -1, defense: 1, treasury: 40),
                terrainRequirement: "Forest");
            Add("Sawmill - common", 1, I, 50, 30,
                "Can only be built in forest; supplies timber for building, crafts, and fuel for harsh northern winters.",
                Fx(food: 0.5m, economy: 1, production: 3, treasury: 10),
                terrainRequirement: "Forest");
            Add("Sawmill - Elven alder", 1, I, 60, 40,
                "Elven alder is an extremely rare timber—superb for bows and highly prized decorative wood.",
                Fx(economy: 6, production: 4, stability: -2, law: -4, culture: 2, intelligence: 4, defense: 3, treasury: 150),
                terrainRequirement: "Forest");
            Add("Blacksmith Workshop", 1, B, 60, 100,
                "A small smithy—a master and apprentices at one forge. Enough for a baron to equip light infantry.",
                Fx(economy: 2, production: 5, science: 2, defense: 5));
            Add("Watchtower", 1, B, 20, 10,
                "A watchtower on the barony border for spotting enemies or lighting signal fires. A network needs towers roughly 5–10 miles apart on hills, or 5 miles on flat ground. Upkeep: -1.",
                Fx(defense: 2, treasury: -1));
            Add("Prison", 1, B, 150, 100,
                "A small dungeon that stops thugs and thieves; even has a modest torture chamber, though a skilled jailer matters most. Also deters crime and aids defense.",
                Fx(stability: 2, law: 1, corruption: -0.5m, defense: 1, treasury: -5));
            Add("Cobbled Streets", 1, B, 30, 10,
                "Muddy gravel roads replaced with fine cobblestone. Improves city life. Cost: 30 production and 10 gold per 1 community. Side effect: -community/2.",
                Fx(economy: 1, production: 1, loyalty: 1, stability: 1, magic: 1, defense: 1, treasury: 1));
            Add("Landfill", 1, B, 100, 30,
                "Usually outside the walls; necessary when city population and technology exceed a certain level.",
                Fx(stability: 2, law: -1));
            Add("Defensive Earthwork", 1, I, 100, 50,
                additive: Fx(loyalty: 2, stability: 1, defense: 8));
            Add("Herbalist", 1, B, 80, 60,
                "An herbalist selling healing herbs and brews.",
                Fx(stability: 2, science: 1));
            Add("Palisade", 1, B, 100, 50,
                "A few meters of sharpened stakes encircling the settlement.",
                Fx(loyalty: 2, stability: 1, defense: 8));
            Add("Hunter's Lodge", 1, I, 25, 20,
                "A lodge for hunters who gather furs and meat and scout the nearby woods.",
                Fx(food: 0.7m, economy: 0.5m, production: 0.5m, defense: 3, treasury: 5));
            Add("Well", 1, B, 10, 8,
                "Essential for life; its absence imposes penalties.");
            Add("Fletcher's Workshop", 1, B, 40, 80,
                "Produces bows from hunting pieces to longbows for the army.",
                Fx(economy: 2, production: 3, science: 2, defense: 5));
            Add("Defensive Granary", 1, B, 30, 15,
                "A stone building with a flat roof, canopy, reinforced gates, stone stockpile, and signal fire— a fallback strongpoint for several farms or a small hamlet. Increases barony storage by 5. Can shelter a community of size 3 for a few days.",
                Fx(loyalty: 1, defense: 3, treasury: -2));
            Add("Small Temple (Thyrus)", 1, B, 60, 150,
                "A temple to Thyrus. Bonuses to law, corruption, and defense.",
                Fx(economy: 2, production: -1, loyalty: 2, stability: 3, law: 1, corruption: -0.5m, science: 2, magic: 1, culture: 5, defense: 4, treasury: 4));
            Add("Medium Temple (Thyrus)", 2, B, 130, 350,
                "A temple to Thyrus. Bonuses to law, corruption, and defense.",
                Fx(economy: 4, production: -3, loyalty: 5, stability: 7, law: 3, corruption: -1, science: 5, magic: 3, culture: 10, defense: 10, treasury: 10));
            Add("Large Temple (Thyrus)", 3, B, 500, 2000,
                "A temple to Thyrus. Bonuses to law, corruption, and defense.",
                Fx(economy: 8, production: -8, loyalty: 10, stability: 15, law: 8, corruption: -3, science: 10, magic: 8, culture: 25, defense: 25, treasury: 25));
            Add("Spy Den", 1, B, 20, 150,
                @"A spy network letting the baron hire spies (for gold or intelligence). Each mission costs extra. Examples: listen for rumors (5 imp./20 intel), steal information (15 imp./50 intel), plant a spy (40–100 imp.), sabotage (40–80 imp.). No assassinations. Upkeep: -5.",
                Fx(loyalty: 2, law: -1, corruption: 2, science: 12, defense: 5, treasury: -5));
            Add("Barracks", 1, B, 200, 100,
                "Barracks for housing and training professional soldiers. Can billet up to 3 military units and provides a free city guard unit.",
                Fx(food: -1, stability: 6, law: 4, corruption: 1, defense: 20, treasury: -60));
            Add("Builders' Guild", 1, B, 150, 150,
                "Allows building one extra structure per turn, plus bridges and complex works requiring an architect. Upkeep: -5.",
                Fx(economy: 2, production: 10, law: 1, science: 5, culture: 3, defense: 2, treasury: -5));

            return list;
        }
    }
}
