using DA_Common.Barony;
using DA_DataAccess.BaronyData;
using DA_DataAccess.Data;
using Microsoft.EntityFrameworkCore;

namespace DA_Business.Repository.BaronyRepos
{
    /// <summary>Default Organizations contacts seeded for every barony (EN/PL at write time).</summary>
    public static class OrganizationsSeeder
    {
        public sealed record SeedEntry(
            string GroupNameEn,
            string GroupNamePl,
            string Name,
            string TitleEn,
            string TitlePl,
            int? Age,
            string DescriptionEn,
            string DescriptionPl,
            int SortOrder)
        {
            public string GroupName => BaronyCulture.IsPolish ? GroupNamePl : GroupNameEn;
            public string Title => BaronyCulture.IsPolish ? TitlePl : TitleEn;
            public string Description => BaronyCulture.IsPolish ? DescriptionPl : DescriptionEn;
        }

        public static readonly IReadOnlyList<SeedEntry> Defaults =
        [
            new(
                GroupNameEn: "The Inquisition",
                GroupNamePl: "Inkwizycja",
                Name: "Gideon Sloane",
                TitleEn: "Senior Inquisitor of the Eastern March",
                TitlePl: "Starszy Inkwizytor Marchii Wschodniej",
                Age: null,
                DescriptionEn:
                    "First Inquisitor of the March and senior over three regional inquisitors. Mild-mannered and soft-spoken in well-cut burgundy and black, with three old scars across his face—yet his office is feared: dagonite dust, heresy, forbidden cults, and investigations into the mighty all fall under the Inquisition’s eye. He answers ultimately to the Emperor; locally, a single word from him can freeze a city or a noble house.",
                DescriptionPl:
                    "Pierwszy Inkwizytor Marchii i zwierzchnik trzech regionalnych inkwizytorów. Łagodny i cichy w dobrze skrojonym burguńsko-czarnym stroju, z trzema starymi bliznami na twarzy — a jednak jego urząd budzi lęk: proch dagonitowy, herezja, zakazane kulty i śledztwa przeciw możnym leżą w oku Inkwizycji. Ostatecznie odpowiada przed Cesarzem; lokalnie jedno jego słowo może zamrozić miasto albo ród szlachecki.",
                SortOrder: 0),
            new(
                GroupNameEn: "Order of Thunder",
                GroupNamePl: "Zakon Gromu",
                Name: "Jorenth von Egilburry",
                TitleEn: "Komtur of the Eastern March",
                TitlePl: "Komtur Marchii Wschodniej",
                Age: null,
                DescriptionEn:
                    "Highest commander of the Most Holy Order of Thyrus Thunderwielder in the Eastern March—chaplain-knights, hospitallers, and heavy cavalry under the cry *Let the Thunder Fall!* Grave and measured in duty, he answers for the Order’s honor in the region and can place even a Trial Knight on a path that binds faith, Empire, and secrets best kept behind cloister walls.",
                DescriptionPl:
                    "Najwyższy dowódca Najświętszego Zakonu Thyrusa Gromowładnego we Wschodniej Marchii — rycerze-kapelani, szpitalnicy i ciężka jazda pod okrzykiem *Niech spadnie Grom!* Surowy i miarodajny w obowiązku, odpowiada za honor Zakonu w regionie i może skierować nawet Rycerza Próby na ścieżkę łączącą wiarę, Imperium i tajemnice, które najlepiej zostawić za murami klasztoru.",
                SortOrder: 1),
            new(
                GroupNameEn: "Imperial Administration",
                GroupNamePl: "Administracja Imperialna",
                Name: "Ernhold Tourraine",
                TitleEn: "Emperor of the Thyrotan Empire",
                TitlePl: "Cesarz Imperium Thyrotańskiego",
                Age: null,
                DescriptionEn:
                    "Reigning Tourraine and heir of Jergon’s line, seated in Dagareth amid the High Council of electors, officers of treasury, war, and spies, the Chancellor, Arcymagister, Grand Inquisitor, and the Archpriest of Thyrus. His word is final, yet he seldom overrides a united Council. From the capital he sets taxes, law, and the Empire’s recent appetite for conquest—most recently the reduction of Kildrad to a march shared among his generals.",
                DescriptionPl:
                    "Panujący Tourraine i dziedzic linii Jergona, zasiadający w Dagareth wśród Wysokiej Rady elektorów, urzędników skarbu, wojny i szpiegów, Kanclerza, Arcymagistra, Wielkiego Inkwizytora i Arcykapłana Thyrusa. Jego słowo jest ostateczne, lecz rzadko łamie jednomyślną Radę. Ze stolicy ustala podatki, prawo i niedawny apetyt Imperium na podbój — ostatnio sprowadzenie Kildradu do marchii dzielonej między jego generałów.",
                SortOrder: 2),
            new(
                GroupNameEn: "Mage Guild",
                GroupNamePl: "Gildia Magów",
                Name: "Cassian Orthas",
                TitleEn: "Arcymagister",
                TitlePl: "Arcymagister",
                Age: 163,
                DescriptionEn:
                    "Head of Magna Gilda Arcana Magistanem—the Empire’s arcane authority, a campus-city of seven colleges plus the Eighth that hunts forbidden arts. Bound to the Emperor and free of the Inquisition’s yoke, the Guild controls who may wield high magic and how dagonite dust is allotted to its ranks. On the High Council the Arcymagister speaks for learning; in the marches, a guild seal can open doors—or close them forever.",
                DescriptionPl:
                    "Głowa Magna Gilda Arcana Magistanem — arcanej władzy Imperium, miasta-kampusu siedmiu kolegiów oraz Ósmego, które ściga sztuki zakazane. Związana z Cesarzem i wolna od jarzma Inkwizycji, Gildia kontroluje, kto może władać wysoką magią i jak proch dagonitowy rozdzielany jest w jej szeregach. W Wysokiej Radzie Arcymagister przemawia za nauką; w marchiach pieczęć gildii potrafi otworzyć drzwi — albo zamknąć je na zawsze.",
                SortOrder: 3),
        ];

        /// <summary>
        /// Adds any missing default organization relations for <paramref name="baronyId"/>.
        /// When UI culture is Polish, also refreshes seeded titles/descriptions to PL.
        /// </summary>
        public static void EnsureForBarony(ApplicationDbContext ctx, int baronyId)
        {
            var existing = ctx.BaronyRelations
                .Where(r => r.BaronyId == baronyId && r.Category == RelationCategory.Organizations)
                .ToList();

            var byName = existing
                .GroupBy(r => r.Name, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

            foreach (var entry in Defaults)
            {
                if (byName.TryGetValue(entry.Name, out var relation))
                {
                    ApplyLocalizedFields(relation, entry);
                    continue;
                }

                ctx.BaronyRelations.Add(new BaronyRelation
                {
                    BaronyId = baronyId,
                    Category = RelationCategory.Organizations,
                    GroupName = entry.GroupName,
                    Name = entry.Name,
                    Title = entry.Title,
                    Age = entry.Age,
                    Description = entry.Description,
                    TroopCount = 0,
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

        private static void ApplyLocalizedFields(BaronyRelation relation, SeedEntry entry)
        {
            if (!BaronyCulture.IsPolish)
                return;

            relation.GroupName = entry.GroupNamePl;
            relation.Title = entry.TitlePl;
            relation.Description = entry.DescriptionPl;
            relation.SortOrder = entry.SortOrder;
        }
    }
}
