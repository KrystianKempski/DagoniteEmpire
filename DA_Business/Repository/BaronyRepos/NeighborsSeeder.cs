using DA_Common.Barony;
using DA_DataAccess.BaronyData;
using DA_DataAccess.Data;
using Microsoft.EntityFrameworkCore;

namespace DA_Business.Repository.BaronyRepos
{
    /// <summary>Default Neighbors contacts seeded for every barony (Eastern March neighbours).</summary>
    public static class NeighborsSeeder
    {
        public sealed record SeedEntry(
            string GroupName,
            string Name,
            string Title,
            int? Age,
            string Description,
            int TroopCount,
            int SortOrder);

        public static readonly IReadOnlyList<SeedEntry> Defaults =
        [
            new(
                GroupName: "House Greywarden",
                Name: "Balon Greywarden",
                Title: "Viscount of Easthills",
                Age: null,
                Description:
                    "Proud cousin of Marquis Myrton Greyward. Easthills is a rainy, mountainous viscounty of poor soil and few farmers—dependent on food from other provinces, yet rich in ores and unusually populous and wealthy for such barren land. Balon is a stern, strong lord whose judgments are fair and whose lands stay safe, but he must share power with Inquisitor Zurwick (who oversees the dagonite mine with two hundred monks, eighty of them paladins, and is raising an Order fortress) and Baronet Prywald Collyhors, a former commoner who won silver mines and a title by betraying Kildrad’s war chest to the Empire. The Greywards honour their promise to Collyhors yet treat him as a turncoat; he remains the richest man in the province.",
                TroopCount: 0,
                SortOrder: 0),
            new(
                GroupName: "House Blackfyre",
                Name: "Huel Blackfyre",
                Title: "Baron of Gillamoor",
                Age: null,
                Description:
                    "Knighted at fourteen after the Battle of the Pass and granted Gillamoor by Marquis Myrton Greyward for later service against rebels and orcs. A warlike, unscrupulous expander: he seized Dolburg from Ryher Tarwin while Tarwin fought the Vorgowelds, and took land from Blackhammer after Lord Orewald the Childless died. He keeps nearly two hundred well-equipped soldiers—one of the larger private armies in this part of the March—and presses claims ever more boldly. He sought all of Blackhammer by diplomacy; Myrton refused, and the old dwarf hold still looks too hard to storm.",
                TroopCount: 200,
                SortOrder: 1),
            new(
                GroupName: "House Tarwin",
                Name: "Ryher Tarwin",
                Title: "Baron of Bredow",
                Age: null,
                Description:
                    "Son of Ralys Tarwin, who received fertile, trade-rich Bredow from Myrton Greyward for valour in the conquest. The town once sat on routes to dwarf holds, the free cities of the east, and Frampland, with good peace and commerce among the Vorgowelds—until Ryher cheated them with bad goods, false tolls, and clipped coin, then publicly executed a shaman sent to collect what was owed. Clans Thunderfoot, Eternal Storm, and Coldhands declared war; Vorgoweld embargo and myto strangled the roads. He has won battles, but war and blockade have gutted his subjects and treasury, turning a prosperous trading domain into a militarised ruin.",
                TroopCount: 0,
                SortOrder: 2),
            new(
                GroupName: "House Blackward",
                Name: "Urven Blackward",
                Title: "Baron of Orlyn",
                Age: null,
                Description:
                    "Legitimised bastard of a Greyward cadet line, distant kin to Myrton. Some seventy percent of Orlyn is dense wildwood; the barony exports timber—mahogany, ironwood, and dark oak—and forest goods. Control of nearly half the Orlyn forest lets him host the “Royal Hunts,” invited across the Eastern March; he competes for that prestige with Juri Swald of the Darklyn woods across the vale. An adroit, merciful ruler who ordered the cut without stripping the forest, harsh on poachers and bandits, kind to the poor and orphans. Popular among woodcutters and foresters; Haga’s cult dominates his domain, though he is said to favour Erastil.",
                TroopCount: 0,
                SortOrder: 3),
            new(
                GroupName: "House Trur",
                Name: "Olgred Trur",
                Title: "Viscount of Brie",
                Age: null,
                Description:
                    "Of Kildrad blood. His father Omir yielded Brie to Hardwin Greatwing after a short siege in exchange for peace and no sack, then ruled cautiously for thirty years—loved by commoners, pressed by ambitious neighbours (including Darkhold under Thaddeus Direbolt’s predecessors). After Omir’s death Olgred raised taxes, hardened the law, seized debtors’ goods, enlarged the guard, retook land from vacant Darkhold, and made war on Baron Corlin Werdhog of Klin. Early gains collapsed when Werdhog hired the eastern sellsword company Scarlet Desire, who night-attacked Olgred, butchered half his guard, and drove him off. Raids continue. Vassal of Marquis Argewald Canterill of Totham; he pays his dues and keeps his wars within the law, so Argewald does not intervene.",
                TroopCount: 0,
                SortOrder: 4),
            new(
                GroupName: "House Coler",
                Name: "Arienna Coler",
                Title: "Baroness of Thyruswill",
                Age: 32,
                Description:
                    "Widow of Lord Luden Canterill, who died childless and left her everything—to his house’s fury. Thyruswill stands on the Moonlake peninsula where Hardwin Greatwing once sacked and burned Moonhall after a bitter siege; Luden oversaw its rebuilding under Turderweld of the Canterills. Arienna, once Luden’s concubine and later his wife after Herdwig’s death, is a sharp politician who kept the barony despite Canterill lawsuits by winning Margrave Hardwin’s favour and breaking their embargo. Suitors flock; she plays them against each other and refuses to remarry. She pays the highest peace-fee (30%) and obeys her liege, yet Marquis Argewald Canterill still withholds the viscountess’s title.",
                TroopCount: 0,
                SortOrder: 5),
            new(
                GroupName: "House Greatwing",
                Name: "Dyron Greatwing",
                Title: "Baron of Hurtbow",
                Age: null,
                Description:
                    "Tall, thin, and little given to ambition—known for intelligence and a love of learning, logic, and games. Young nephew of Margrave Hardwin Greatwing, appointed to Hurtbow after decades of Dalish war on the Irredale fringe: Walch Hurtmere’s feud, peasant revolt for the Empire, failed hostage diplomacy, Lucius Greatwing’s death in ambush, and Urglim Mad Bull’s brutal campaigns that only deepened the hatred. After Urglim fell, neighbours—through Lord Erac Mertyn—bought a fragile peace with the Dalish for his body, goods, and an admission of defeat. Dyron’s arms split Greatwing with a green field and black lion; he is only beginning to settle into a hard, impoverished border barony.",
                TroopCount: 0,
                SortOrder: 6),
            new(
                GroupName: "Clan Dag'Thorak",
                Name: "Durisug Dag'Thorak",
                Title: "Marquis (Mountain Prince) of Groundfall",
                Age: null,
                Description:
                    "Hard, unyielding dwarf lord of the Gray Mountains and the Venomous Pass—the only easy gate from the Salt Marshes into the March. Clan Dag'Thorak (Dagonite Shield) has held Groundfall for over seven centuries; they became Kildrad’s vassals in the Great Hunger in exchange for food and medicine, kept their own law, tongue, faith, and a tribute of dwarf-wrought arms, and later swore to Hardwin Greatwing on unchanged terms after he took three of Durisug’s sons hostage. Direct vassal of the Margrave: he pays tax and war-dues, yet keeps special rights for dwarves in the Eastern March. Without Groundfall’s fort, lizardfolk raiders and marsh-beasts would spill fifty miles inland.",
                TroopCount: 0,
                SortOrder: 7),
            new(
                GroupName: "House Koltberg",
                Name: "Turen Koltberg",
                Title: "Baronet of Um",
                Age: null,
                Description:
                    "One of three feuding Koltberg baronet-brothers in the barren Koltberg Hollow east of Darkhold—reached only by steep goblin-haunted paths through the Gray Mountains. Um is his village and chicken-coop “seat.” Officially a vassal of Marquis Argewald Canterill, who never collects tax or summons them to war: the hollow has no ores, little timber, and thin soil. The brothers—sons of Willis the Stout by different mothers—wage drunken “wars” with clubs and wormwood spirits, then lose interest before anything is decided. Little respected and usually ignored in March politics.",
                TroopCount: 0,
                SortOrder: 8),
            new(
                GroupName: "House Koltberg",
                Name: "Will the Stammerer",
                Title: "Baronet of Arg",
                Age: null,
                Description:
                    "Koltberg baronet of Arg in the same poor hollow as Turen and Dunna. Claims the whole vale by blood; fights his half-brothers in farcical village brawls rather than true campaigns. House Koltberg is known for poverty, wildness, and inbreeding rumours—outstanding ears, mangy faces, hunched frames. Like his kin he is a vassal of Argewald Canterill in name only.",
                TroopCount: 0,
                SortOrder: 9),
            new(
                GroupName: "House Koltberg",
                Name: "Dunna Koltberg",
                Title: "Baronet of Holdywag",
                Age: null,
                Description:
                    "Koltberg baronet of Holdywag, third of the quarrelsome brothers of the hollow. Same poverty, same club-and-spirit “sieges,” same empty claim to rule the vale. March lords leave the Koltbergs alone until something worth taxing grows there—which never has.",
                TroopCount: 0,
                SortOrder: 10),
            new(
                GroupName: "House Mertyn",
                Name: "Erac Mertyn",
                Title: "Count of Willow Hill",
                Age: 52,
                Description:
                    "Heir of an old house always loyal to the Greatwings. Willow Hill buffers the March against the wild folk of Irredale Forest. Erac is a seasoned ranger and warrior—master of the hunt and protector of the deep woods—who has fought Dalish, greenskins, and ogrillons for decades. His network of watchtowers carries warning so civilians and stock can reach the walls. For loyalty the Mertyns hold a count’s title, a hereditary Master of Scouts dignity, and a seat on Margrave Hardwin’s council. To the south lie Greatling baronet villages forever squabbling for Hardwin’s favour.",
                TroopCount: 0,
                SortOrder: 11),
            new(
                GroupName: "House Hastwyck",
                Name: "Dramon Hastwyck",
                Title: "Baron of Forestedge",
                Age: null,
                Description:
                    "Younger son of Count Keven Hastwyck of Dawntree, who carved Forestedge from his own lands. Dramon raised a sizable seat with a loan from his father and unexplained funds, then—backed by Dawntree—grew the barony fast, absorbing nearby villages. He sought Hurtbow, but Hardwin gave it elsewhere. Ambitious and able; rumour says he will not leave his brother’s inheritance alone and means to contest Dawntree itself against their father’s will.",
                TroopCount: 0,
                SortOrder: 12),
            new(
                GroupName: "House Grann",
                Name: "Jerald Grann",
                Title: "Viscount of Brodlow",
                Age: null,
                Description:
                    "Has fought the goblins and hobgoblins of Goblin Skull Hill for over twenty years and lost two sons in its tunnels. Decade after decade the “green crusades” of local lords bleed out underground; the last was seven years ago, and the vermin have swollen again. Said to hate goblins so fiercely that once, disarmed and surrounded, he tore them apart and gouged out their eyes with his fingers. Brodlow lives by timber export and horse-breeding—mounts needed for constant counter-raids.",
                TroopCount: 0,
                SortOrder: 13),
            new(
                GroupName: "House Rollford",
                Name: "Aren Rollford",
                Title: "Count of King's Peace",
                Age: null,
                Description:
                    "From an old Kildrad house that has held these lands beyond memory. Thirty years ago the truce between Kildrad and the Empire was struck here; war never reached the county, leaving it richer and more peopled than much of the Eastern March. The Rollfords alone trade with the closed elves of Lorathrien—crafts, timber, and forest fruits—so seniors shield them from predatory neighbours. In return Aren must stay neutral in private wars. Dignified and slim; fond of Lorathrien jewelry and dress.",
                TroopCount: 0,
                SortOrder: 14),
            new(
                GroupName: "House Clifford",
                Name: "Erwald Clifford",
                Title: "Baron of Earthcliff",
                Age: null,
                Description:
                    "Twin of Parweld of Newcliff. Their father Morweld, unable to name the elder, split his fertile grain-and-cattle plains: Erwald took the east with the lesser town and ancestral seat; Parweld the west and larger Newcliff—on pain that if the brothers quarrelled, all would pass to their sister Adryana. After two years of peace they fell out; for a year and a half neither has won clear advantage, and the once-rich pastures suffer for it.",
                TroopCount: 0,
                SortOrder: 15),
            new(
                GroupName: "House Clifford",
                Name: "Parweld Clifford",
                Title: "Baron of Newcliff",
                Age: null,
                Description:
                    "Twin of Erwald of Earthcliff. Holds the western share of Morweld Clifford’s divided domain—the larger town of Newcliff on the same fertile plains of grain and cattle. Locked in the same fraternal war for over a year and a half; neither twin will yield, and the countryside pays the price.",
                TroopCount: 0,
                SortOrder: 16),
            new(
                GroupName: "House Edgerton",
                Name: "Darrin Edgerton",
                Title: "Baron of Keatlow",
                Age: null,
                Description:
                    "Youngest son of Justan Edgerton who outlived three elder brothers and turned Keatlow from a mercantile barony into a military one. As a sellsword captain he banked gold and favours, then carved land and a rich village from Summerhall (Lord Highhill) and later an industrial scrap with a village from Lonehill (Brenn Penrose). Ambitious and warlike; likely already eyeing the next neighbour.",
                TroopCount: 0,
                SortOrder: 17),
        ];

        /// <summary>
        /// Adds any missing default neighbor relations for <paramref name="baronyId"/>.
        /// Idempotent: skips entries whose Name already exists in Neighbors for that barony.
        /// </summary>
        public static void EnsureForBarony(ApplicationDbContext ctx, int baronyId)
        {
            var existingNames = ctx.BaronyRelations
                .Where(r => r.BaronyId == baronyId && r.Category == RelationCategory.Neighbors)
                .Select(r => r.Name)
                .ToList();

            foreach (var entry in Defaults)
            {
                if (existingNames.Any(n => string.Equals(n, entry.Name, StringComparison.OrdinalIgnoreCase)))
                    continue;

                ctx.BaronyRelations.Add(new BaronyRelation
                {
                    BaronyId = baronyId,
                    Category = RelationCategory.Neighbors,
                    GroupName = entry.GroupName,
                    Name = entry.Name,
                    Title = entry.Title,
                    Age = entry.Age,
                    Description = entry.Description,
                    TroopCount = entry.TroopCount,
                    RelationDescription = string.Empty,
                    Notes = null,
                    SortOrder = entry.SortOrder,
                });
            }
        }

        public static async Task EnsureForAllBaroniesAsync(ApplicationDbContext ctx)
        {
            var baronyIds = await ctx.Baronies.AsNoTracking().Select(b => b.Id).ToListAsync();
            foreach (var id in baronyIds)
                EnsureForBarony(ctx, id);
            await ctx.SaveChangesAsync();
        }
    }
}
