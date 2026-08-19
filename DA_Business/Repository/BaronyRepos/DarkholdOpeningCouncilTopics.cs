namespace DA_Business.Repository.BaronyRepos;

/// <summary>
/// Opening Council agenda for a freshly seeded Darkhold barony.
/// English is what the seeder writes; Polish is kept ready for localisation.
/// </summary>
public static class DarkholdOpeningCouncilTopics
{
    public sealed record Topic(
        string TitleEn,
        string TitlePl,
        string SpeakerName,
        string BodyEn,
        string BodyPl);

    public static readonly Topic[] All =
    {
        new(
            TitleEn: "Kobold raid on a farm",
            TitlePl: "Atak koboldów na farmę",
            SpeakerName: "Sir Loren Birely",
            BodyEn:
                "My lord. Three weeks past, kobolds struck a farm and stripped it bare. They do not raid the same place twice, and we cannot yet say from which cave they came — the eastern hills are riddled with tunnels. Lord Ekhard vanished half a year ago on an expedition into those same mountains; my own search found his camp before a tunnel, kobold sign, and a recent cave-in. Whether this band is the same tribe that took him, I cannot swear. What I can swear is that they will return. I ask leave to send scouts under Anwan, quietly, and to ready the guard for war against a smaller foe.",
            BodyPl:
                "Milordzie. Trzy tygodnie temu koboldy uderzyły na farmę i doszczętnie ją splądrowały. Nie atakują dwa razy w tym samym miejscu, a my wciąż nie wiemy, z której jaskini wyszły — wschodnie wzgórza są pełne tuneli. Lord Ekhard zniknął pół roku temu podczas wyprawy w te same góry; moja ekspedycja znalazła jego obóz przed tunelem, ślady koboldów i niedawny zawał. Czy to to samo plemię, które go pochłonęło — nie przysięgnę. Przysięgnę natomiast, że wrócą. Proszę o zgodę, by Anwan poprowadził ciche rozpoznanie, a gwardia przygotowała się do walki z mniejszym przeciwnikiem."),

        new(
            TitleEn: "The debt to our senior",
            TitlePl: "Dług u seniora",
            SpeakerName: "Albus Durdwale",
            BodyEn:
                "My lord. Darkhold still owes Marquis Canterill two hundred imperials — a burden first taken under Baron Mirren Credd, and grown with every lean season since. Interest accrues. Until the debt is settled in earnest, Totham treats us as a vassal who has forgotten his manners. I can arrange repayment as swiftly as the treasury allows, but I need your word on how much we send this quarter, and whether we dare ask the Marquis for mercy.",
            BodyPl:
                "Milordzie. Darkhold nadal winien jest markizowi Canterillowi dwieście imperiali — dług zaciągnięty jeszcze za barona Mirrena Credda, który rósł z każdym chudym sezonem. Odsetki ciągle narastają. Dopóki nie uregulujemy go na serio, Totham traktuje nas jak wasala, który zapomniał manier. Mogę ustawić spłatę tak szybko, jak pozwoli skarbiec, lecz potrzebuję Waszego słowa: ile wysyłamy w tym kwartale i czy ośmielimy się prosić markiza o łaskę."),

        new(
            TitleEn: "Paper, ink, and the luxury toll",
            TitlePl: "Papier, atrament i myto na luksusy",
            SpeakerName: "Albus Durdwale",
            BodyEn:
                "My lord — a smaller matter, yet it bites daily. We are nearly out of paper and ink. Totham has raised the toll on luxury goods until Darkhold settles its debt; the old allotment of paper that every Canterill vassal once received has simply stopped. Without it, decrees, ledgers and letters grind to a halt. We may beg the Marquis to ease the toll, or buy dearer stock from Thyruswill and swallow the loss. Either way, I need a decision before the last sheets run out.",
            BodyPl:
                "Milordzie — sprawa mniejsza, a gryzie codziennie. Kończy nam się papier i atrament. Totham podniosło myto na towary luksusowe, póki Darkhold nie ureguluje długu; dawny przydział papieru, który dostawał każdy wasal Canterilla, po prostu ustał. Bez tego dekrety, księgi i listy stają w miejscu. Możemy błagać markiza o złagodzenie myta albo kupić drożej w Thyruswill i przełknąć stratę. Tak czy inaczej, decyzja potrzebna zanim skończą się ostatnie arkusze."),

        new(
            TitleEn: "The beast of Ravenclaw Wood",
            TitlePl: "Bestia z lasu Kruczego Szponu",
            SpeakerName: "Merdred Igrus",
            BodyEn:
                "My lord. Since last summer the peasants of Ravenclaw have complained of a monster in the neighbouring wood. It takes livestock and, they say, children. Hunters and woodcutters have vanished without a trace. One witness swore it was a dragon and died of his wounds before a proper beast-master could examine him — and we have no such master here, only hunters who know how to flee. The last report is a week old: one hunter escaped when he felt himself hunted; his companion did not return. The attacks began about a month ago. Shall we gather what the hunters know and ride out on a hunt?",
            BodyPl:
                "Milordzie. Od zeszłego lata chłopi z Kruczego Szponu narzekają na potwora w okolicznym lesie. Porywa zwierzęta i — jak twierdzą — dzieci. Myśliwi i drwale giną bez śladu. Jeden świadek przysięgał, że to smok, i zmarł od ran, zanim mógł go zbadać znawca bestii — a takiego u nas nie ma, tylko myśliwi, którzy wiedzą, jak uciekać. Ostatni raport sprzed tygodnia: jeden myśliwy zbiegł, gdy poczuł, że ktoś na niego poluje; towarzysz nie wrócił. Ataki zaczęły się około miesiąca temu. Mam zebrać wieści od myśliwych i urządzić polowanie?"),

        new(
            TitleEn: "Lands stolen by Brie — the circle of Haga",
            TitlePl: "Ziemie zabrane przez Brie — krąg Hagi",
            SpeakerName: "Merdred Igrus",
            BodyEn:
                "My lord. During the year without a baron, Viscount Olgreb of Brie seized a stretch of our land that holds a holy circle of Haga. The folk of Darkhold once prayed and kept their festivals there; now Brie forbids them entry, and of late he has raised a sawmill in the sacred wood. Those trees should not fall to ignorant axes — and even without the sacrilege, the land is ours. I ask what you intend: swift recovery, careful pressure, or an embassy first. Either way, we should know how the sawmill and its workers are guarded before we move.",
            BodyPl:
                "Milordzie. W roku bez barona wicehrabia Olgreb z Brie zagarnął kawał naszej ziemi ze świętym kręgiem Hagi. Lud Darkhold odprawiał tam modły i święta; teraz Brie zabrania wstępu, a niedawno postawił tartak w świętym lesie. Te drzewa nie powinny padać pod toporami ignorantów — a nawet bez świętokradztwa ziemia wciąż jest nasza. Pytam o Wasz zamiar: szybkie odzyskanie, ostrożny nacisk, czy najpierw poselstwo. Tak czy inaczej, zanim ruszymy, trzeba wiedzieć, jak chroniony jest tartak i jego ludzie."),

        new(
            TitleEn: "The pirate wreck on the eastern cliffs",
            TitlePl: "Wrak piratów na wschodnich klifach",
            SpeakerName: "Sir Loren Birely",
            BodyEn:
                "My lord. Last spring a pirate drakkar out of Erude was wrecked on the cliffs east of Darkhold. The survivors gave us some trouble for a time, but Baron Ekhard dealt with them. He never settled the matter of the wreck itself, however. Against all expectation it has not broken apart and still clings to the rocks. Some manner of drowners have nested inside it — lured, most likely, by the corpses of the castaways. The fishermen who pass that way complain the drowners lie in wait for them; several have not come home at all. Perhaps something ought to be done about that wreck.",
            BodyPl:
                "Milordzie. Ostatniej wiosny na klifach na wschód od Darkhold rozbił się drakkar piratów z Erude. Rozbitkowie sprawiali nam później trochę kłopotów, lecz baron Ekhard uporał się z nimi. Nie zdążył jednak rozwiązać problemu samego wraku. Ten — wbrew wszelkim przewidywaniom — nie rozleciał się i wciąż trzyma się na skałach. W jego wnętrzu zalęgły się jakieś utopce, zwabione zapewne zwłokami rozbitków. Rybacy, którzy tamtędy przepływają, narzekają, że utopce na nich czyhają; kilku nawet nie wróciło do domostw. Może trzeba by coś zrobić z tym wrakiem."),
    };

    public static string FormatExchangeBody(Topic topic) => IsPolish ? topic.BodyPl : topic.BodyEn;

    public static string FormatTitle(Topic topic) => IsPolish ? topic.TitlePl : topic.TitleEn;

    /// <summary>True when the current request's UI culture is Polish — drives which
    /// language the seeder writes for both council topics and opening audiences.</summary>
    internal static bool IsPolish =>
        System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName
            .Equals("pl", StringComparison.OrdinalIgnoreCase);
}
