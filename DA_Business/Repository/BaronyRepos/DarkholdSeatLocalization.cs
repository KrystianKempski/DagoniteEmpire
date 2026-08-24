using System.Text.Json;
using DA_Common.Barony;
using DA_DataAccess.BaronyData;
using DA_DataAccess.Data;
using Microsoft.EntityFrameworkCore;

namespace DA_Business.Repository.BaronyRepos;

/// <summary>
/// Polish Lord's Seat chamber names and starting artifacts for Darkhold.
/// English remains the snapshot default; Polish is written at seed time when UI culture is PL,
/// and backfilled for existing Darkhold baronies on load. Artifacts are seeded for every
/// Darkhold barony (new and existing) so they can be placed in chambers.
/// </summary>
public static class DarkholdSeatLocalization
{
    public const string SeatNameEn = "Lord's Seat";
    public const string SeatNamePl = "Siedziba lorda";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public sealed record RoomEntry(int Level, int GridX, int GridY, string NameEn, string NamePl);

    public sealed record ArtifactEntry(
        string NameEn,
        string NamePl,
        string Kind,
        string Origin,
        int Prestige,
        int Honor,
        int Fear,
        string DescriptionEn,
        string DescriptionPl,
        int? SeedRoomId,
        int PlaceLevel,
        int PlaceX,
        int PlaceY,
        (Ppb Key, decimal Value)[] Additive,
        int SortOrder);

    public static readonly RoomEntry[] Rooms =
    [
        new(0, 12, 3, "North lean-to big chamber", "Duża komnata przybudówki północnej"),
        new(0, 15, 7, "North-east tower - lower level, south chamber, ", "Wieża północno-wschodnia — parter, komnata południowa"),
        new(0, 15, 10, "East tower - lower big chamber", "Wieża wschodnia — duża komnata parteru"),
        new(0, 13, 14, "East wall lean-to", "Przybudówka przy wschodnim murze"),
        new(0, 9, 16, "Souther lean-to", "Przybudówka południowa"),
        new(0, 9, 9, "Courtyard barrack", "Koszary na dziedzińcu"),
        new(0, 8, 6, "Shed", "Szopa"),
        new(1, 12, 3, "Left chamber on top of stone  lean-on", "Lewa komnata nad kamienną przybudówką"),
        new(1, 12, 5, "Right chamber on top of stone  lean-on", "Prawa komnata nad kamienną przybudówką"),
        new(1, 15, 5, "North-east tower, middle level, north chamber", "Wieża północno-wschodnia — piętro, komnata północna"),
        new(1, 15, 12, "East tower, middle level south chamber", "Wieża wschodnia — piętro, komnata południowa"),
        new(2, 15, 5, "North east tower, top level, north chamber", "Wieża północno-wschodnia — najwyższe piętro, komnata północna"),
        new(2, 15, 10, "East tower - top level", "Wieża wschodnia — najwyższe piętro"),
        new(0, 15, 5, "North-east tower, lowel level, north chamber", "Wieża północno-wschodnia — parter, komnata północna"),
        new(1, 15, 7, "North-east tower, middle level, south chamber", "Wieża północno-wschodnia — piętro, komnata południowa"),
        new(1, 15, 10, "East tower, middle level north", "Wieża wschodnia — piętro, komnata północna"),
        new(2, 15, 7, "North east tower, top level, south chamber", "Wieża północno-wschodnia — najwyższe piętro, komnata południowa"),
        new(2, 15, 12, "North east tower, top level, south chamber", "Wieża wschodnia — najwyższe piętro, komnata południowa"),
        new(3, 15, 5, "Top of north-east tower", "Szczyt wieży północno-wschodniej"),
        new(3, 15, 10, "Top of east tower", "Szczyt wieży wschodniej"),
    ];

    /// <summary>Seed room ids from <c>DarkholdSeed.json</c> (Throne 9, Study 26, Armory 25).</summary>
    public static readonly ArtifactEntry[] Artifacts =
    [
        new(
            NameEn: "Sword of the Direbolts",
            NamePl: "Miecz Direboltów",
            Kind: BaronArtifactKind.Weapon,
            Origin: BaronArtifactOrigin.Inherited,
            Prestige: 8,
            Honor: 2,
            Fear: 1,
            DescriptionEn:
                "Ancestral arming sword of House Direbolt, still hung above the high seat. The pommel bears a worn raven, and the blade is kept oiled even though no baron has drawn it in years.",
            DescriptionPl:
                "Rodowy miecz domu Direbolt, wciąż zawieszony nad tronem. Głowica nosi wytartego kruka, a głownia jest oliwiona, choć żaden baron nie dobył jej od lat.",
            SeedRoomId: 9,
            PlaceLevel: 0,
            PlaceX: 12,
            PlaceY: 3,
            Additive: [(Ppb.Law, 1m)],
            SortOrder: 0),
        new(
            NameEn: "Kildrad campaign tapestry",
            NamePl: "Gobelin z wojny kildradzkiej",
            Kind: BaronArtifactKind.Tapestry,
            Origin: BaronArtifactOrigin.Inherited,
            Prestige: 6,
            Honor: 0,
            Fear: 0,
            DescriptionEn:
                "A long wool hanging that shows Hardwin Greatwing's banners over the salt marshes. It was a gift to Darkhold after the War of Kildrad and still smells faintly of cedar chests.",
            DescriptionPl:
                "Długi wełniany arras ze sztandarami Hardwina Greatwinga nad solnymi bagnami. Dar dla Darkhold po Wojnie Kildradzkiej; wciąż pachnie cedrowymi skrzyniami.",
            SeedRoomId: 9,
            PlaceLevel: 0,
            PlaceX: 12,
            PlaceY: 3,
            Additive: [(Ppb.Culture, 2m)],
            SortOrder: 1),
        new(
            NameEn: "Portrait of Baron Ekhard",
            NamePl: "Portret barona Ekharda",
            Kind: BaronArtifactKind.Painting,
            Origin: BaronArtifactOrigin.Inherited,
            Prestige: 4,
            Honor: 1,
            Fear: 0,
            DescriptionEn:
                "Oil portrait of the last baron, painted the winter before he vanished into the eastern hills. The eyes follow visitors across the study.",
            DescriptionPl:
                "Olejny portret ostatniego barona, malowany zimą zanim zniknął we wschodnich wzgórzach. Oczy śledzą gości przez gabinet.",
            SeedRoomId: 26,
            PlaceLevel: 2,
            PlaceX: 15,
            PlaceY: 7,
            Additive: [(Ppb.Culture, 1m)],
            SortOrder: 2),
        new(
            NameEn: "Carta of Darkhold",
            NamePl: "Karta Darkhold",
            Kind: BaronArtifactKind.Book,
            Origin: BaronArtifactOrigin.Acquired,
            Prestige: 3,
            Honor: 0,
            Fear: 0,
            DescriptionEn:
                "Bound copy of the barony's grants, tolls and boundary oaths. Albus keeps a working ledger; this is the fair copy shown to envoys.",
            DescriptionPl:
                "Oprawiony zbiór nadan, myt i przysiąg granicznych baronii. Albus trzyma robocze księgi; to czystopis pokazywany posłom.",
            SeedRoomId: 26,
            PlaceLevel: 2,
            PlaceX: 15,
            PlaceY: 7,
            Additive: [(Ppb.Stability, 1m), (Ppb.Science, 1m)],
            SortOrder: 3),
        new(
            NameEn: "Mail of Groundfall",
            NamePl: "Kolczuga z Groundfall",
            Kind: BaronArtifactKind.Armor,
            Origin: BaronArtifactOrigin.Gift,
            Prestige: 5,
            Honor: 0,
            Fear: 1,
            DescriptionEn:
                "Dwarf-forged hauberk from Durisug Dag'Thorak, presented when Darkhold last sent grain through the pass. Too fine for daily watch; it hangs in the armory as proof of the pact.",
            DescriptionPl:
                "Krasnoludzka kolczuga od Durisuga Dag'Thoraka, dar gdy Darkhold ostatnio posłało zboże przez przełęcz. Zbyt dobra na codzienną wartę; wisi w zbrojowni jako dowód paktu.",
            SeedRoomId: 25,
            PlaceLevel: 1,
            PlaceX: 15,
            PlaceY: 10,
            Additive: [(Ppb.Defense, 1m)],
            SortOrder: 4),
        new(
            NameEn: "Wreck-iron trophy",
            NamePl: "Trofeum z wraku",
            Kind: BaronArtifactKind.Trophy,
            Origin: BaronArtifactOrigin.Won,
            Prestige: 4,
            Honor: 0,
            Fear: 2,
            DescriptionEn:
                "A twisted prow-hook taken from the Erude drakkar that wrecked on the cliffs. Baron Ekhard meant to hang it in the hall; it still sits in store.",
            DescriptionPl:
                "Wypaczony hak z dziobu erudzkiego drakkara, który rozbił się na klifach. Baron Ekhard chciał powiesić go w hali; wciąż leży w składzie.",
            SeedRoomId: null,
            PlaceLevel: -1,
            PlaceX: 0,
            PlaceY: 0,
            Additive: [(Ppb.Loyalty, 1m)],
            SortOrder: 5),
        new(
            NameEn: "Silver salt cellar",
            NamePl: "Srebrna solniczka",
            Kind: BaronArtifactKind.DisplayPiece,
            Origin: BaronArtifactOrigin.Bought,
            Prestige: 3,
            Honor: 0,
            Fear: 0,
            DescriptionEn:
                "A small silver cellar from Thyruswill, bought when salt still moved cheaply through Moonlake. Waiting for a chamber fine enough to show it.",
            DescriptionPl:
                "Niewielka srebrna solniczka z Thyruswill, kupiona gdy sól szła tanio przez Moonlake. Czeka na komnatę dość godną, by ją wystawić.",
            SeedRoomId: null,
            PlaceLevel: -1,
            PlaceX: 0,
            PlaceY: 0,
            Additive: [(Ppb.Economy, 1m)],
            SortOrder: 6),
    ];

    private static readonly Dictionary<(int Level, int X, int Y), RoomEntry> RoomsByLayout =
        Rooms.ToDictionary(r => (r.Level, r.GridX, r.GridY));

    public static string LocalizeSeatName(string? name) =>
        DarkholdOpeningCouncilTopics.IsPolish && IsEnglishSeatName(name) ? SeatNamePl : (name ?? SeatNameEn);

    public static string LocalizeRoomName(int level, int gridX, int gridY, string? current)
    {
        if (!RoomsByLayout.TryGetValue((level, gridX, gridY), out var entry))
            return current ?? string.Empty;
        if (DarkholdOpeningCouncilTopics.IsPolish && IsEnglishRoomName(current, entry))
            return entry.NamePl;
        return string.IsNullOrWhiteSpace(current) ? entry.NameEn : current;
    }

    public static void SeedArtifacts(
        ApplicationDbContext ctx,
        int baronyId,
        IReadOnlyDictionary<int, int> seedRoomMap)
    {
        foreach (var item in Artifacts)
        {
            ctx.BaronArtifacts.Add(ToEntity(baronyId, item, ResolveSeedRoomId(item, seedRoomMap)));
        }
    }

    /// <summary>
    /// Polish chamber names (when UI is PL) and missing starting artifacts for an existing Darkhold.
    /// </summary>
    public static void EnsureForBarony(ApplicationDbContext ctx, int baronyId, string? baronyName)
    {
        if (!DarkholdSeeder.IsDarkhold(baronyName))
            return;

        var seat = ctx.BaronySeats
            .Include(s => s.Rooms)
            .FirstOrDefault(s => s.BaronyId == baronyId);
        if (seat is null)
            return;

        if (DarkholdOpeningCouncilTopics.IsPolish)
        {
            if (IsEnglishSeatName(seat.Name))
                seat.Name = SeatNamePl;

            foreach (var room in seat.Rooms)
            {
                if (!RoomsByLayout.TryGetValue((room.Level, room.GridX, room.GridY), out var entry))
                    continue;
                if (IsEnglishRoomName(room.Name, entry))
                    room.Name = entry.NamePl;
            }
        }

        var existing = ctx.BaronArtifacts.Where(a => a.BaronyId == baronyId).ToList();
        if (existing.Count == 0)
        {
            var rooms = seat.Rooms.ToList();
            foreach (var item in Artifacts)
                ctx.BaronArtifacts.Add(ToEntity(baronyId, item, ResolveLayoutRoomId(item, rooms)));
            return;
        }

        if (!DarkholdOpeningCouncilTopics.IsPolish)
            return;

        foreach (var artifact in existing)
        {
            var entry = Artifacts.FirstOrDefault(a =>
                string.Equals(artifact.Name, a.NameEn, StringComparison.OrdinalIgnoreCase));
            if (entry is null)
                continue;
            artifact.Name = entry.NamePl;
            if (string.Equals(artifact.Description, entry.DescriptionEn, StringComparison.Ordinal))
                artifact.Description = entry.DescriptionPl;
        }
    }

    private static BaronArtifact ToEntity(int baronyId, ArtifactEntry item, int? seatRoomId)
    {
        var add = new PpbVector();
        foreach (var (key, value) in item.Additive)
            add[key] = value;

        return new BaronArtifact
        {
            BaronyId = baronyId,
            Name = DarkholdOpeningCouncilTopics.IsPolish ? item.NamePl : item.NameEn,
            Kind = item.Kind,
            Origin = item.Origin,
            Prestige = item.Prestige,
            Honor = item.Honor,
            Fear = item.Fear,
            AdditiveJson = JsonSerializer.Serialize(add, JsonOptions),
            PercentJson = JsonSerializer.Serialize(new PpbVector(), JsonOptions),
            SeatRoomId = seatRoomId,
            Description = DarkholdOpeningCouncilTopics.IsPolish ? item.DescriptionPl : item.DescriptionEn,
            SortOrder = item.SortOrder,
        };
    }

    private static int? ResolveSeedRoomId(ArtifactEntry item, IReadOnlyDictionary<int, int> seedRoomMap)
    {
        if (item.SeedRoomId is int seedId && seedRoomMap.TryGetValue(seedId, out var id))
            return id;
        return null;
    }

    private static int? ResolveLayoutRoomId(ArtifactEntry item, IReadOnlyList<SeatRoom> rooms)
    {
        if (item.SeedRoomId is null || item.PlaceLevel < 0)
            return null;
        return rooms.FirstOrDefault(r =>
            r.Level == item.PlaceLevel && r.GridX == item.PlaceX && r.GridY == item.PlaceY)?.Id;
    }

    private static bool IsEnglishSeatName(string? name) =>
        string.Equals((name ?? "").Trim(), SeatNameEn, StringComparison.OrdinalIgnoreCase);

    private static bool IsEnglishRoomName(string? current, RoomEntry entry) =>
        string.Equals((current ?? "").Trim(), entry.NameEn.Trim(), StringComparison.OrdinalIgnoreCase);
}
