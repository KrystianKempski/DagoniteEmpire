using DA_Common.Barony;
using DA_DataAccess.BaronyData;
using DA_DataAccess.Data;
using Microsoft.EntityFrameworkCore;

namespace DA_Business.Repository.BaronyRepos;

/// <summary>
/// Polish strings for Darkhold Relations seeded from <c>DarkholdSeed.json</c>.
/// English is the snapshot default; Polish is written at seed time when UI culture is PL,
/// and backfilled for existing Darkhold baronies on load.
/// </summary>
public static class DarkholdRelationLocalization
{
    public const string DirectVassalModifierPl = "bezpośredni wasal";

    public sealed record Entry(
        string Name,
        string? TitlePl,
        string? DescriptionPl);

    // All must be declared before ByName — static fields initialize in declaration order.
    public static readonly Entry[] All =
    [
        new(
            Name: "Millena Canterill",
            TitlePl: "Baronet",
            DescriptionPl:
                "Po śmierci głowy rodu — Rydana Canterilla, zwanego Kozłem — wdowa przejęła jego majątek. Rydan zostawił cztery córki i żadnego syna, więc to kobiety rządzą teraz domem Brązowych Canterillów. Stara matrona cieszy się dobrymi stosunkami ze swoim dalekim kuzynem, markizem Argewaldem, i nikt jeszcze nie próbował wypierać ich z ziem. Baroneta Millena zazdrośnie strzeże cnoty córek, ostrzegając każdego zalotnika, że w razie małżeństwa będzie musiał wyrzec się własnego nazwiska. Dotąd nie brakowało chętnych mężczyzn, lecz żaden nie zyskał przychylności wybrednej matrony. Brązowi Canterillowie trzymają południowo-zachodnie ziemie wraz z wsią Durbale, dworem i wieżą obronną."),
        new(
            Name: "Jochim Bullewyn",
            TitlePl: "Baronet",
            DescriptionPl:
                "Zamożny ród jak na standardy Darkhold. Baronet jest sprytny i ostrożny — człowiek rozsądny z nosem do zysku, choć nie słynie z nadmiernej skorumpowania ani nadużywania władzy. Ród pochodzi z centralnego Imperium i zdobył ziemie w Darkhold w nagrodę za służbę w Wojnie Kildradzkiej. Dom trzyma wsię Raven's Claw i okoliczne ziemie; dwór Bullewynów stoi w niej, choć rodzina utrzymuje też rezydencję w mieście Darkhold."),
        new(
            Name: "Olgred Trur",
            TitlePl: "Wicehrabia Brie",
            DescriptionPl:
                "Krew Kildradu. Jego ojciec Omir oddał Brie Hardwinowi Greatwingowi po krótkim oblężeniu w zamian za pokój i brak łupiestwa, potem rządził ostrożnie przez trzydzieści lat — lubiany przez pospólstwo, ściskany przez ambitnych sąsiadów (w tym Darkhold za czasów poprzedników Thaddeusa Direbolta). Po śmierci Omira Olgred podniósł podatki, zaostrzył prawo, zajmował dobra dłużników, powiększył gwardię, odebrał ziemię opuszczonemu Darkhold i wypowiedział wojnę baronowi Corlinowi Werdhogowi z Klin. Wczesne zyski runęły, gdy Werdhog wynajął wschodnią kompanię najemników Scarlet Desire, która nocą uderzyła na Olgreda, wycięła połowę jego gwardii i go odpędziła. Najeździ trwają. Wasal markiza Argewalda Canterilla z Totham; płaci daniny i prowadzi wojny w granicach prawa, więc Argewald nie interweniuje."),
        new(
            Name: "Arienna Coler",
            TitlePl: "Baronowa Thyruswill",
            DescriptionPl:
                "Wdowa po lordzie Ludenie Canterillu, który umarł bezdzietnie i zostawił jej wszystko — ku wściekłości jego rodu. Thyruswill stoi na półwyspie Moonlake, gdzie Hardwin Greatwing kiedyś splądrował i spalił Moonhall po gorzkim oblężeniu; Luden nadzorował jego odbudowę pod Turderweldem z Canterillów. Arienna, niegdyś konkubina Ludena, a po śmierci Herdwig jego żona, to ostra polityczka, która utrzymała baronię mimo procesów Canterillów, zyskując łaskę margrabiego Hardwina i łamiąc ich embargo. Zalotnicy się tłoczą; gra ich jednych przeciw drugim i odmawia ponownego małżeństwa. Płaci najwyższą daninę pokojową (30%) i słucha suzerena, lecz markiz Argewald Canterill wciąż wstrzymuje tytuł wicebaronowej."),
        new(
            Name: "Dyron Greatwing",
            TitlePl: "Baron Hurtbow",
            DescriptionPl:
                "Wysoki, chudy i mało skłonny do ambicji — znany z inteligencji i miłości do nauki, logiki i gier. Młody brat stryjeczny margrabiego Hardwina Greatwinga, mianowany na Hurtbow po dekadach dalijskiej wojny na skraju Irredale: feud Walcha Hurtmere'a, chłopskie powstanie dla Imperium, nieudana dyplomacja zakładników, śmierć Luciusa Greatwinga w zasadzce i brutalne kampanie Urglima Mad Bulla, które tylko pogłębiły nienawiść. Po upadku Urglima sąsiedzi — przez lorda Eraca Mertyna — kupili kruchy pokój z Daliszyznami za jego ciało, dobra i przyznanie porażki. Herby Dyrona dzielą Greatwinga na zielone pole i czarnego lwa; dopiero zaczyna osiadać w twardej, ubogiej baronii granicznej."),
        new(
            Name: "Durisug Dag'Thorak",
            TitlePl: "Markiz (Książę Górski) Groundfall",
            DescriptionPl:
                "Twardy, nieugięty krasnoludzki lord Szarych Gór i Jadowitej Przełęczy — jedynej łatwej bramy z Solonych Bagien do Marchii. Klan Dag'Thorak (Tarcza Dagonitów) trzyma Groundfall od ponad siedmiu stuleci; stali się wasalami Kildradu w Wielkim Głodzie w zamian za żywność i medycynę, zachowali własne prawo, język, wiarę i trybut krasnoludzkiej broni, a później złożyli hołd Hardwinowi Greatwingowi na niezmienionych warunkach, gdy ten wziął trzech synów Durisuga w zakład. Bezpośredni wasal margrabiego: płaci podatki i daniny wojenne, lecz zachowuje specjalne prawa dla krasnoludów we Wschodniej Marchii. Bez fortu Groundfall łupieżcy jaszczuroludzi i bagienne bestie rozlałyby się pięćdziesiąt mil w głąb lądu."),
        new(
            Name: "Turen Koltberg",
            TitlePl: "Baronet Um",
            DescriptionPl:
                "Jeden z trzech walczących braci-baronetów Koltbergów w jałowym Dołku Koltberg na wschód od Darkhold — dostępnym tylko stromymi, goblinami nawiedzanymi ścieżkami przez Szare Góry. Um to jego wioska i kurza „siedziba”. Oficjalnie wasal markiza Argewalda Canterilla, który nigdy nie pobiera podatków ani nie wzywa ich na wojnę: w dole nie ma rud, mało drewna i cienka gleba. Bracia — synowie Willisa Grubego z różnych matek — toczą pijane „wojny” kijami i spirytusem z piołunu, po czym tracą zainteresowanie, zanim cokolwiek rozstrzygną. Mało szanowani i zwykle ignorowani w polityce Marchii."),
        new(
            Name: "Joral Uberdorf",
            TitlePl: "Baronet Mudside",
            DescriptionPl: null),
        new(
            Name: "Konrad Eisen",
            TitlePl: "Baronet Glossop",
            DescriptionPl: null),
        new(
            Name: "Milwarn Jettenborg",
            TitlePl: "Baronet New Bradford",
            DescriptionPl: null),
        new(
            Name: "Roderick Vael",
            TitlePl: "Baronet Naporia",
            DescriptionPl: null),
        new(
            Name: "Dunna Koltberg",
            TitlePl: "Baronet Holdywag",
            DescriptionPl:
                "Baronet Holdywag, trzeci z kłótliwych braci z dołu. Ta sama nędza, te same „oblężenia” kijem i spirytusem, te same puste roszczenia do rządzenia doliną. Panowie Marchii zostawiają Koltbergów w spokoju, póki nie urośnie tam coś wartego opodatkowania — czego nigdy nie było."),
        new(
            Name: "Will the Stammerer",
            TitlePl: "Baronet Arg",
            DescriptionPl:
                "Baronet Arg w tym samym ubogim dole co Turen i Dunna. Roszczy sobie całą dolinę z krwi; walczy z bracioma w groteskowych bójkach wiejskich zamiast w prawdziwych kampaniach. Dom Koltberg słynie z nędzy, dzikości i pogłosek o kazirodztwie — wystające uszy, kłaczaste twarze, garbite sylwetki. Jak i jego krewni, jest wasalem Argewalda Canterilla tylko z miana."),
        new(
            Name: "Jora Bullewyn",
            TitlePl: "Dziedzic",
            DescriptionPl: "Dziedzic i syn Jochima Bullewyna."),
        new(
            Name: "Umbra Bullewyn",
            TitlePl: "Dama",
            DescriptionPl: "Matka rodu. Żona Jochima."),
        new(
            Name: "Callor Bullewyn",
            TitlePl: "Syn",
            DescriptionPl: "Syn Jochima i Umbry."),
        new(
            Name: "Ranel Bullewyn",
            TitlePl: "Córka",
            DescriptionPl: "Córka Jochima i Umbry."),
        new(
            Name: "Mereya Bullewyn",
            TitlePl: "Córka",
            DescriptionPl: "Najmłodsza córka Jochima i Umbry."),
        new(
            Name: "Terren Wynch",
            TitlePl: "Przyjaciel rodziny",
            DescriptionPl: "Przyjaciel rodu Bullewynów."),
        new(
            Name: "Ellya Canterill",
            TitlePl: "Dziedziczka",
            DescriptionPl: "Najstarsza córka i dziedziczka Milleny Canterill."),
        new(
            Name: "Laurane Canterill",
            TitlePl: "Córka",
            DescriptionPl: "Córka Milleny Canterill."),
        new(
            Name: "Dyanna Canterill",
            TitlePl: "Córka",
            DescriptionPl: "Córka Milleny Canterill."),
        new(
            Name: "Nysah Canterill",
            TitlePl: "Córka",
            DescriptionPl: "Najmłodsza córka Milleny Canterill."),
        new(
            Name: "Dorran Carner",
            TitlePl: "Zalotnik",
            DescriptionPl: "Zalotnik stara się o rękę jednej z córek Canterillów."),
        new(
            Name: "Lanard Apperford",
            TitlePl: "Zalotnik",
            DescriptionPl: "Zalotnik stara się o rękę jednej z córek Canterillów."),
    ];

    private static readonly IReadOnlyDictionary<string, Entry> ByName =
        All.ToDictionary(e => e.Name, StringComparer.OrdinalIgnoreCase);

    public static string LocalizeTitle(string name, string title) =>
        TryGet(name, out var entry) && entry.TitlePl is not null ? entry.TitlePl : title;

    public static string LocalizeDescription(string name, string description) =>
        TryGet(name, out var entry) && entry.DescriptionPl is not null ? entry.DescriptionPl : description;

    public static string LocalizeModifierDescription(string description) =>
        DarkholdOpeningCouncilTopics.IsPolish
        && string.Equals(description, RelationVassalDefaults.DirectVassalModifier, StringComparison.OrdinalIgnoreCase)
            ? DirectVassalModifierPl
            : description;

    /// <summary>
    /// Backfills Polish relation text for an existing Darkhold barony when UI culture is PL.
    /// </summary>
    public static void EnsurePolishForBarony(ApplicationDbContext ctx, int baronyId, string? baronyName)
    {
        if (!DarkholdOpeningCouncilTopics.IsPolish || !DarkholdSeeder.IsDarkhold(baronyName))
            return;

        var relations = ctx.BaronyRelations
            .Include(r => r.Modifiers)
            .Where(r => r.BaronyId == baronyId)
            .ToList();

        foreach (var relation in relations)
        {
            if (!TryGet(relation.Name, out var entry))
                continue;

            if (entry.TitlePl is not null)
                relation.Title = entry.TitlePl;

            if (entry.DescriptionPl is not null)
                relation.Description = entry.DescriptionPl;

            foreach (var modifier in relation.Modifiers)
                modifier.Description = LocalizeModifierDescription(modifier.Description);
        }
    }

    private static bool TryGet(string? name, out Entry entry)
    {
        entry = null!;
        return !string.IsNullOrWhiteSpace(name) && ByName.TryGetValue(name.Trim(), out entry);
    }
}
