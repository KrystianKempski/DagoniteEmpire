using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using DA_Common.Barony;
using DA_DataAccess.BaronyData;
using DA_DataAccess.Data;
using Microsoft.EntityFrameworkCore;

namespace DA_Business.Repository.BaronyRepos
{
    /// <summary>
    /// Seeds the full starting state of the <c>Darkhold</c> player barony from an embedded
    /// snapshot (terrain map, fiefs/domains, terrain improvements, lord's seat, courtiers,
    /// vassal/neighbor relations, army units and a ready tactical battle map). Original
    /// snapshot ids are remapped onto the freshly created barony. Idempotent: skips when the
    /// barony already has terrain domains.
    /// </summary>
    public static class DarkholdSeeder
    {
        public const string BaronyName = "Darkhold";
        public const int TerrainMapWidth = 11;
        public const int TerrainMapHeight = 10;

        private const string ResourceSuffix = "DarkholdSeed.json";

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            NumberHandling = JsonNumberHandling.AllowReadingFromString
                             | JsonNumberHandling.AllowNamedFloatingPointLiterals,
        };

        public static bool IsDarkhold(string? baronyName) =>
            string.Equals(baronyName?.Trim(), BaronyName, StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Applies the Darkhold snapshot to <paramref name="baronyId"/>. The baron's own
        /// person/lord name is personalised with <paramref name="baronName"/>.
        /// </summary>
        public static async Task SeedAsync(ApplicationDbContext ctx, int baronyId, string baronName)
        {
            var barony = await ctx.Baronies.FirstOrDefaultAsync(b => b.Id == baronyId);
            if (barony is null)
                return;

            barony.TerrainMapWidth = TerrainMapWidth;
            barony.TerrainMapHeight = TerrainMapHeight;

            // Idempotency guard — a seeded barony always has at least one terrain domain.
            if (await ctx.TerrainMapDomains.AnyAsync(d => d.BaronyId == baronyId))
                return;

            var seed = LoadSeed();
            if (seed is null)
                return;

            // --- Domains ---
            var domainMap = new Dictionary<int, int>();
            var domainRows = seed.TerrainMapDomains
                .Select(s => (s.Id, Entity: new TerrainMapDomain
                {
                    BaronyId = baronyId,
                    Name = s.Name,
                    LordName = s.IsPrimary ? baronName : s.LordName,
                    ColorHex = s.ColorHex,
                    IsPrimary = s.IsPrimary,
                    SortOrder = s.SortOrder,
                }))
                .ToList();
            ctx.TerrainMapDomains.AddRange(domainRows.Select(r => r.Entity));
            await ctx.SaveChangesAsync();
            foreach (var r in domainRows) domainMap[r.Id] = r.Entity.Id;

            // --- Fiefs (SeniorDomainId -> domain) ---
            var fiefMap = new Dictionary<int, int>();
            var fiefRows = seed.Fiefs
                .Select(s => (s.Id, Entity: new Fief
                {
                    BaronyId = baronyId,
                    Name = s.IsBaronDemesne ? $"Lord {baronName}" : s.Name,
                    LiegeName = s.IsBaronDemesne ? baronName : s.LiegeName,
                    IsBaronDemesne = s.IsBaronDemesne,
                    IsDomainDefault = s.IsDomainDefault,
                    SeniorDomainId = Remap(domainMap, s.SeniorDomainId),
                    ColorHex = s.ColorHex,
                    BonusMultiplier = s.BonusMultiplier,
                }))
                .ToList();
            ctx.Fiefs.AddRange(fiefRows.Select(r => r.Entity));
            await ctx.SaveChangesAsync();
            foreach (var r in fiefRows) fiefMap[r.Id] = r.Entity.Id;

            // --- Terrain tiles (FiefId -> fief, MapDomainId -> domain) ---
            var tileMap = new Dictionary<int, int>();
            var tileRows = seed.TerrainTiles
                .Select(s => (s.Id, Entity: new TerrainTile
                {
                    BaronyId = baronyId,
                    MapId = s.MapId,
                    X = s.X,
                    Y = s.Y,
                    BaseType = s.BaseType,
                    FeaturesMask = s.FeaturesMask,
                    Fertility = s.Fertility,
                    Resource = s.Resource,
                    FiefId = Remap(fiefMap, s.FiefId),
                    MapDomainId = Remap(domainMap, s.MapDomainId),
                    Comment = s.Comment,
                }))
                .ToList();
            ctx.TerrainTiles.AddRange(tileRows.Select(r => r.Entity));
            await ctx.SaveChangesAsync();
            foreach (var r in tileRows) tileMap[r.Id] = r.Entity.Id;

            // --- Terrain improvements (TileId -> tile; TemplateId is a global catalog id) ---
            var improvements = seed.TerrainImprovements
                .Select(s => new TerrainImprovement
                {
                    BaronyId = baronyId,
                    TileId = Remap(tileMap, s.TileId),
                    TemplateId = s.TemplateId,
                    Name = s.Name,
                    AdditiveJson = s.AdditiveJson,
                    PercentJson = s.PercentJson,
                    Description = s.Description,
                    FormulaText = s.FormulaText,
                    IsActive = s.IsActive,
                    InactiveReason = s.InactiveReason,
                    IconUrl = s.IconUrl,
                    Population = s.Population,
                    HasPalisade = s.HasPalisade,
                });
            ctx.TerrainImprovements.AddRange(improvements);

            // --- Courtiers (available advisor pool) ---
            var availMap = new Dictionary<int, int>();
            var availRows = seed.AvailableAdvisors
                .Select(s => (s.Id, Entity: new AvailableAdvisor
                {
                    BaronyId = baronyId,
                    Name = s.Name,
                    Description = s.Description,
                    SkillsJson = s.SkillsJson,
                    SheetJson = s.SheetJson,
                }))
                .ToList();
            ctx.AvailableAdvisors.AddRange(availRows.Select(r => r.Entity));
            await ctx.SaveChangesAsync();
            foreach (var r in availRows) availMap[r.Id] = r.Entity.Id;

            // --- Advisors / offices (AvailableAdvisorId -> courtier; personalise baron) ---
            var advisorMap = new Dictionary<int, int>();
            var advisorRows = seed.Advisors
                .Select(s => (s.Id, Entity: new Advisor
                {
                    BaronyId = baronyId,
                    OfficeType = s.OfficeType,
                    Title = s.Title,
                    PersonName = s.IsBaron ? baronName : s.PersonName,
                    AvailableAdvisorId = Remap(availMap, s.AvailableAdvisorId),
                    IsBaron = s.IsBaron,
                    SkillsJson = s.SkillsJson,
                    SignificantSkillsJson = s.SignificantSkillsJson,
                    AdditiveJson = s.AdditiveJson,
                    PercentJson = s.PercentJson,
                    FormulaText = s.FormulaText,
                    Description = s.Description,
                    UpkeepGold = s.UpkeepGold,
                }))
                .ToList();
            ctx.Advisors.AddRange(advisorRows.Select(r => r.Entity));
            await ctx.SaveChangesAsync();
            foreach (var r in advisorRows) advisorMap[r.Id] = r.Entity.Id;

            // --- Lord's seat ---
            var seatSeed = seed.BaronySeats.FirstOrDefault();
            if (seatSeed is not null)
            {
                var seat = new BaronySeat
                {
                    BaronyId = baronyId,
                    Name = seatSeed.Name,
                    GridWidth = seatSeed.GridWidth,
                    GridHeight = seatSeed.GridHeight,
                    ActiveLevelsJson = seatSeed.ActiveLevelsJson,
                };
                ctx.BaronySeats.Add(seat);
                await ctx.SaveChangesAsync();

                // Rooms (SeatId -> seat, OccupantAdvisorId -> advisor, PurposeTemplateId global)
                var roomMap = new Dictionary<int, int>();
                var roomRows = seed.SeatRooms
                    .Select(s => (s.Id, Entity: new SeatRoom
                    {
                        SeatId = seat.Id,
                        Name = s.Name,
                        Level = s.Level,
                        GridX = s.GridX,
                        GridY = s.GridY,
                        GridW = s.GridW,
                        GridH = s.GridH,
                        Material = s.Material,
                        PrestigeMultiplier = s.PrestigeMultiplier,
                        Status = s.Status,
                        AdditiveJson = s.AdditiveJson,
                        PercentJson = s.PercentJson,
                        PurposeTemplateId = s.PurposeTemplateId,
                        OccupantAdvisorId = Remap(advisorMap, s.OccupantAdvisorId),
                        OccupantCustom = s.OccupantCustom,
                        SortOrder = s.SortOrder,
                    }))
                    .ToList();
                ctx.SeatRooms.AddRange(roomRows.Select(r => r.Entity));
                await ctx.SaveChangesAsync();
                foreach (var r in roomRows) roomMap[r.Id] = r.Entity.Id;

                if (seed.SeatRoomTraits.Count > 0)
                {
                    ctx.SeatRoomTraits.AddRange(seed.SeatRoomTraits
                        .Where(s => roomMap.ContainsKey(s.RoomId))
                        .Select(s => new SeatRoomTrait
                        {
                            RoomId = roomMap[s.RoomId],
                            Kind = s.Kind,
                            Text = s.Text,
                            SortOrder = s.SortOrder,
                        }));
                }

                ctx.SeatTiles.AddRange(seed.SeatTiles
                    .Select(s => new SeatTile
                    {
                        SeatId = seat.Id,
                        Level = s.Level,
                        X = s.X,
                        Y = s.Y,
                        Kind = s.Kind,
                    }));
            }

            // --- Relations (Vassals + Neighbors only; FiefId -> fief) ---
            var relMap = new Dictionary<int, int>();
            var isPolish = DarkholdOpeningCouncilTopics.IsPolish;
            var relRows = seed.BaronyRelations
                .Select(s => (s.Id, Entity: new BaronyRelation
                {
                    BaronyId = baronyId,
                    Category = s.Category,
                    GroupName = s.GroupName,
                    Name = s.Name,
                    Title = isPolish
                        ? DarkholdRelationLocalization.LocalizeTitle(s.Name, s.Title)
                        : s.Title,
                    Age = s.Age,
                    Description = isPolish
                        ? DarkholdRelationLocalization.LocalizeDescription(s.Name, s.Description)
                        : s.Description,
                    TroopCount = s.TroopCount,
                    RelationDescription = s.RelationDescription,
                    Notes = s.Notes,
                    MarksJson = s.MarksJson,
                    SortOrder = s.SortOrder,
                    FiefId = Remap(fiefMap, s.FiefId),
                }))
                .ToList();
            ctx.BaronyRelations.AddRange(relRows.Select(r => r.Entity));
            await ctx.SaveChangesAsync();
            foreach (var r in relRows) relMap[r.Id] = r.Entity.Id;

            if (seed.BaronyRelationModifiers.Count > 0)
            {
                ctx.BaronyRelationModifiers.AddRange(seed.BaronyRelationModifiers
                    .Where(s => relMap.ContainsKey(s.RelationId))
                    .Select(s => new BaronyRelationModifier
                    {
                        RelationId = relMap[s.RelationId],
                        Description = DarkholdRelationLocalization.LocalizeModifierDescription(s.Description),
                        Value = s.Value,
                        SortOrder = s.SortOrder,
                    }));
            }

            // --- Army units (CaptainAvailableAdvisorId -> courtier). Keep old ids for token remap. ---
            var unitMap = new Dictionary<int, int>();
            if (seed.BaronyUnits.Count > 0)
            {
                var nowUtc = DateTime.UtcNow;
                var unitRows = seed.BaronyUnits
                    .Select(s => (s.Id, Entity: new BaronyUnit
                    {
                        BaronyId = baronyId,
                        Name = s.Name,
                        Status = s.Status,
                        TroopCount = s.TroopCount,
                        RecruitSelectionKey = s.RecruitSelectionKey,
                        TrainingTypeKey = s.TrainingTypeKey,
                        RaceKey = s.RaceKey,
                        Wage = s.Wage,
                        UpkeepFood = s.UpkeepFood,
                        UpkeepDefense = s.UpkeepDefense,
                        Build = s.Build,
                        Agility = s.Agility,
                        Will = s.Will,
                        Perception = s.Perception,
                        AttrPenaltyBuild = s.AttrPenaltyBuild,
                        AttrPenaltyAgility = s.AttrPenaltyAgility,
                        AttrOtherBuild = s.AttrOtherBuild,
                        AttrOtherAgility = s.AttrOtherAgility,
                        AttrOtherWill = s.AttrOtherWill,
                        AttrOtherPerception = s.AttrOtherPerception,
                        SkillsJson = s.SkillsJson,
                        SkillOtherJson = s.SkillOtherJson,
                        CombatOtherJson = s.CombatOtherJson,
                        SkillOtherSourcesJson = s.SkillOtherSourcesJson,
                        AttrOtherSourcesJson = s.AttrOtherSourcesJson,
                        Weapon1Key = s.Weapon1Key,
                        Weapon2Key = s.Weapon2Key,
                        ArmorKey = s.ArmorKey,
                        ShieldKey = s.ShieldKey,
                        MountKey = s.MountKey,
                        Weapon1Quality = s.Weapon1Quality,
                        Weapon2Quality = s.Weapon2Quality,
                        DefenseSkillKey = s.DefenseSkillKey,
                        CommanderAttack = s.CommanderAttack,
                        CommanderDefense = s.CommanderDefense,
                        CaptainAvailableAdvisorId = Remap(availMap, s.CaptainAvailableAdvisorId),
                        OtherAttack = s.OtherAttack,
                        OtherDefense = s.OtherDefense,
                        OtherDamage = s.OtherDamage,
                        OtherMove = s.OtherMove,
                        OtherArmor = s.OtherArmor,
                        OtherHp = s.OtherHp,
                        RemainingPd = s.RemainingPd,
                        Discipline = s.Discipline,
                        MaxBaseSkillAtGraduation = s.MaxBaseSkillAtGraduation,
                        FreeAttributePoints = s.FreeAttributePoints,
                        CurrentHp = s.CurrentHp,
                        LogJson = s.LogJson,
                        CreatedAtUtc = nowUtc,
                        UpdatedAtUtc = nowUtc,
                    }))
                    .ToList();
                ctx.BaronyUnits.AddRange(unitRows.Select(r => r.Entity));
                await ctx.SaveChangesAsync();
                foreach (var r in unitRows) unitMap[r.Id] = r.Entity.Id;
            }

            // --- Tactical battle map (unit ids inside token/tally JSON remapped to the new units) ---
            var mapSeed = seed.BaronyBattleMaps.FirstOrDefault();
            if (mapSeed is not null)
            {
                ctx.BaronyBattleMaps.Add(new BaronyBattleMap
                {
                    BaronyId = baronyId,
                    IsActive = mapSeed.IsActive,
                    Phase = mapSeed.Phase,
                    Width = mapSeed.Width,
                    Height = mapSeed.Height,
                    CellsJson = mapSeed.CellsJson,
                    TokensJson = RemapUnitIds(mapSeed.TokensJson, unitMap),
                    TurnStateJson = mapSeed.TurnStateJson,
                    LogJson = mapSeed.LogJson,
                    TalliesJson = RemapUnitIds(mapSeed.TalliesJson, unitMap),
                    XpSummaryJson = mapSeed.XpSummaryJson,
                });
            }

            // --- Court audiences (opening petitions awaiting the new baron) ---
            SeedOpeningAudiences(ctx, baronyId, barony.TurnNumber);

            // --- First Council session: urgent advisor agenda ---
            SeedOpeningCouncil(ctx, barony);

            await ctx.SaveChangesAsync();
        }

        private static void SeedOpeningCouncil(ApplicationDbContext ctx, Barony barony)
        {
            var now = DateTime.UtcNow;
            var turnNumber = barony.TurnNumber;

            foreach (var (topic, index) in DarkholdOpeningCouncilTopics.All.Select((t, i) => (t, i)))
            {
                ctx.BaronAudiences.Add(new BaronAudience
                {
                    BaronyId = barony.Id,
                    Title = DarkholdOpeningCouncilTopics.FormatTitle(topic),
                    PetitionerName = topic.SpeakerName,
                    Kind = BaronAudienceKind.Council,
                    Status = BaronAudienceStatus.Scheduled,
                    TurnNumber = turnNumber,
                    CreatedAtUtc = now.AddMilliseconds(index),
                    UpdatedAtUtc = now.AddMilliseconds(index),
                    Exchanges =
                    {
                        new BaronAudienceExchange
                        {
                            Body = DarkholdOpeningCouncilTopics.FormatExchangeBody(topic),
                            IsFromPetitioner = true,
                            SpeakerName = topic.SpeakerName,
                            TurnNumber = turnNumber,
                            SortOrder = 0,
                            CreatedAtUtc = now.AddMilliseconds(index),
                        },
                    },
                });
            }
        }

        private static void SeedOpeningAudiences(ApplicationDbContext ctx, int baronyId, int turnNumber)
        {
            var now = DateTime.UtcNow;
            var isPolish = DarkholdOpeningCouncilTopics.IsPolish;
            (string TitleEn, string TitlePl, string PetitionerEn, string PetitionerPl, string PetitionerIcon, string BodyEn, string BodyPl)[] petitions =
            {
                (
                    "The neighbour's cow on my field",
                    "Krowa sąsiada na mojej łące",
                    "Tuiw Dun, peasant",
                    "Tuiw Dun, chłop",
                    "farmer",
                    "My lord! My neighbour's cow grazes on my field. When I tried to milk her in return, he set upon me and bruised my arms with a rope. Many times I have asked him to keep his cow off my meadow, but he does not watch her at all and lets her wander wherever she pleases. I beg for justice — for me and against my neighbour. He is called Mieon.",
                    "Milordzie! Krowa sąsiada pasie się na moim polu. Kiedy chciałem ją w zamian wydoić, napadł na mnie i obił mi ramiona powrozem. Wiele razy prosiłem go, by jego krowa nie wypasała się na mojej łące, lecz on w ogóle jej nie pilnuje i pozwala jej łazić, gdzie chce. Proszę o sprawiedliwość — dla mnie i przeciw sąsiadowi. Zwie się Mieon."
                ),
                (
                    "A witch at the forest's edge",
                    "Wiedźma na skraju lasu",
                    "Bostri Trueore, peasant",
                    "Bostri Trueore, chłop",
                    "farmer",
                    "My lord, I do not know whether you are aware, but a witch dwells at the edge of the forest. People tolerate her, for she is a skilled herbalist and helps with their troubles — sometimes she delivers a difficult birth, or saves a cow from death. But she is a witch! If you fear Thyrus and the Inquisition, you ought to drive her out. She casts spells and curses. If someone looks at her the wrong way, or refuses her what she asks, she may lay a dreadful curse upon them!",
                    "Panie, nie wiem, czy Wam wiadomo, ale na skraju lasu mieszka wiedźma. Ludzie ją tolerują, bo jest dobrą zielarką i pomaga w kłopotach — czasem odbierze trudny poród albo uratuje krowę od śmierci. Ale to wiedźma! Jeśli boicie się Thyrusa i inkwizycji, powinniście ją przegnać. Rzuca czary i klątwy. Jeśli ktoś spojrzy na nią krzywo albo nie da jej tego, czego zażąda, może rzucić na niego straszliwe przekleństwo!"
                ),
                (
                    "The innkeeper waters down his ale",
                    "Karczmarz rozwadnia piwo",
                    "Horos Phassar, local craftsman",
                    "Horos Phassar, miejscowy rzemieślnik",
                    "hammer",
                    "My lord. Congratulations on your appointment. I wish to offer my respects and my pledge of loyalty. There is also a rather serious matter. Such things are punished harshly in more civilised places. You see, the local innkeeper waters down his beer. You may say that everyone waters it a little and that this is the norm — but my lord, he goes far beyond all measure! This cannot be allowed. I pray you, reprimand him and put an end to this thievery. I assure you that all of Darkhold will be grateful to you for evermore.",
                    "Milordzie. Gratuluję nominacji. Pragnę złożyć wyrazy szacunku i zapewnić o mojej lojalności. Jest też pewna dość poważna sprawa. W bardziej cywilizowanych miejscach takie rzeczy są surowo karane. Otóż miejscowy karczmarz rozwadnia piwo. Powiecie, że każdy trochę rozwadnia i to norma, ale panie, on przekracza wszelkie granice! Tak być nie może. Proszę, upomnijcie go i ukróćcie ten złodziejski proceder. Zapewniam, że całe Darkhold będzie wam wdzięczne po wsze czasy."
                ),
                (
                    "My child has gone missing",
                    "Moje dziecko zaginęło",
                    "Annyte Trapp, the forester's wife",
                    "Annyte Trapp, żona leśnika",
                    "horn",
                    "Baron! My child has gone missing. Please! Help me! My little Riff. He was playing by the house; I took my eyes off him for but a moment, and he vanished — gone! Perhaps someone has taken him! I beg you, order everyone questioned and send guards to search! Perhaps he is lost in the forest and cannot find his way home!",
                    "Baronie! Moje dziecko zaginęło. Proszę, pomóżcie mi! Mój mały Riff. Bawił się przy domostwie, na chwilę spuściłam go z oczu i zniknął — przepadł! Może ktoś go porwał! Błagam, każcie wszystkich przepytać i wyślijcie strażników na poszukiwania! Może zabłądził w lesie i nie potrafi znaleźć drogi do domu!"
                ),
                (
                    "Patronage for the shrine of Orados",
                    "Patronat nad kapliczką Oradosa",
                    "Brother Squall",
                    "Brat Szkwał",
                    "sun-priest",
                    "My lord, I am a humble servant of Orados, God of Tides, of storms, waves and shores. The previous baron permitted me to found a shrine here in Darkhold. I believe Orados holds this land especially dear, as one may see in the strength and wildness of the weather here. Yet the local folk are not kindly disposed toward my god, for the storm they so dread is his blessing. I ask that you take my shrine under your patronage and attend me at mass. The Allfather will surely look upon you more favourably — and who knows, perhaps he will even temper his wrath and let the ships be spared the storms that so often visit the coast of Darkhold.",
                    "Panie, jestem skromnym sługą Oradosa, Boga Pływów, sztormów, fal i wybrzeży. Poprzedni baron zezwolił mi założyć w Darkhold kapliczkę. Wierzę, że Orados szczególnie umiłował tę krainę, co widać po sile i nieokiełznaniu tutejszej pogody. Jednak okoliczna ludność nie jest przychylna mojemu bogu, skoro sztorm, którego tak się boi, jest jego łaską. Proszę, obejmijcie patronatem moją kaplicę i odwiedźcie mnie podczas mszy. Wszechojciec z pewnością spojrzy na was przychylniej — a kto wie, może nawet utemperuje swój gniew i pozwoli statkom uchować się przed sztormami, które tak często nawiedzają wybrzeże Darkhold."
                ),
                (
                    "A gift of wine and a marriage proposal",
                    "Beczka wina i propozycja małżeństwa",
                    "Baronet Jochim Bullewyn",
                    "Baronet Jochim, głowa rodu Bullewynów, szlachcic z Darkhold",
                    "banner",
                    "Noble liege! Congratulations on your appointment! I am certain you will manage splendidly in your new office. Please accept, toward our future cooperation, this cask of my very finest wine! May its exquisite taste caress your palate and gladden your stomach and your head. Allow me to present my most beloved daughter! She is very pretty, obedient and clever — and her dowry is most handsome! She would make an excellent wife, but I keep my little flower only for the worthiest of husbands. — the noble winked knowingly — Perhaps you might call on us one of these days? My wife makes a superb turkey. (The daughter looked her stated age. She was not unpleasant to behold — pretty, even — though hardly a great beauty. She seemed amiable and had a lovely smile.)",
                    "Szlachetny seniorze! Gratuluję nominacji! Jestem pewien, że doskonale poradzicie sobie na nowym stanowisku. Przyjmijcie, na poczet naszej przyszłej współpracy, tę oto beczkę mojego najprzedniejszego wina! Niechaj jego wytworny smak pieści wasze podniebienie i raduje żołądek oraz głowę. Pozwólcie, że przedstawię wam moją najukochańszą córkę! Jest bardzo ładna, posłuszna i mądra, a i posag ma znamienity! Świetnie nadawałaby się na żonę, lecz chowam mój kwiatuszek tylko dla najgodniejszych mężów. — szlachcic mrugnął porozumiewawczo — Może zajdziecie do nas w odwiedziny któregoś dnia? Moja żona robi wyborną potrawkę z indyka. (Córka wyglądała na podany wiek — nie była brzydka, nawet ładna, choć pięknością raczej nie była. Sprawiała sympatyczne wrażenie i miała ładny uśmiech.)"
                ),
                (
                    "A vassal's homage",
                    "Hołd lenny",
                    "Baronetess Millena Canterill",
                    "Baroneta Millena Canterill, głowa rodu Canterillów Brązowych",
                    "banner",
                    "Greetings, baron. I am Millena Canterill, your vassal. Congratulations on your ennoblement. I pay you homage and assure you of my obedience. — she gave a perfunctory bow — I hope you contrive to rule longer than the previous baron did.",
                    "Witajcie, baronie. Jestem Millena Canterill, wasza wasalka. Gratuluję nobilitacji. Składam hołd i zapewniam o moim posłuszeństwie. — skłoniła się zdawkowo — Mam nadzieję, że uda się wam rządzić dłużej niż poprzedniemu baronowi."
                ),
            };

            foreach (var p in petitions)
            {
                ctx.BaronAudiences.Add(new BaronAudience
                {
                    BaronyId = baronyId,
                    Title = isPolish ? p.TitlePl : p.TitleEn,
                    PetitionerName = isPolish ? p.PetitionerPl : p.PetitionerEn,
                    PetitionerIcon = p.PetitionerIcon,
                    Kind = DA_Common.Barony.BaronAudienceKind.Audience,
                    Status = DA_Common.Barony.BaronAudienceStatus.Scheduled,
                    TurnNumber = turnNumber,
                    CreatedAtUtc = now,
                    UpdatedAtUtc = now,
                    Exchanges = new List<BaronAudienceExchange>
                    {
                        new BaronAudienceExchange
                        {
                            Body = isPolish ? p.BodyPl : p.BodyEn,
                            IsFromPetitioner = true,
                            TurnNumber = turnNumber,
                            SortOrder = 0,
                            CreatedAtUtc = now,
                        },
                    },
                });
            }
        }

        private static int? Remap(IReadOnlyDictionary<int, int> map, int? oldId) =>
            oldId is int id && map.TryGetValue(id, out var newId) ? newId : null;

        // Rewrites each object's numeric "unitId" (in a JSON array) from snapshot id to seeded id.
        private static string RemapUnitIds(string json, IReadOnlyDictionary<int, int> unitMap)
        {
            if (unitMap.Count == 0 || string.IsNullOrWhiteSpace(json))
                return json;
            JsonNode? node;
            try { node = JsonNode.Parse(json); }
            catch { return json; }
            if (node is not JsonArray arr)
                return json;
            foreach (var el in arr)
            {
                if (el is JsonObject obj
                    && obj.TryGetPropertyValue("unitId", out var v)
                    && v is JsonValue jv && jv.TryGetValue<int>(out var oldId)
                    && unitMap.TryGetValue(oldId, out var newId))
                {
                    obj["unitId"] = newId;
                }
            }
            return arr.ToJsonString();
        }

        private static SeedDocument? LoadSeed()
        {
            var assembly = typeof(DarkholdSeeder).Assembly;
            var name = assembly.GetManifestResourceNames()
                .FirstOrDefault(n => n.EndsWith(ResourceSuffix, StringComparison.Ordinal));
            if (name is null)
                return null;

            using var stream = assembly.GetManifestResourceStream(name);
            if (stream is null)
                return null;

            return JsonSerializer.Deserialize<SeedDocument>(stream, JsonOptions);
        }

        // ---- Snapshot DTOs (mirror the exported DB columns) ----

        private sealed class SeedDocument
        {
            public List<DomainSeed> TerrainMapDomains { get; set; } = new();
            public List<FiefSeed> Fiefs { get; set; } = new();
            public List<TileSeed> TerrainTiles { get; set; } = new();
            public List<ImprovementSeed> TerrainImprovements { get; set; } = new();
            public List<SeatSeed> BaronySeats { get; set; } = new();
            public List<RoomSeed> SeatRooms { get; set; } = new();
            public List<RoomTraitSeed> SeatRoomTraits { get; set; } = new();
            public List<SeatTileSeed> SeatTiles { get; set; } = new();
            public List<AvailableAdvisorSeed> AvailableAdvisors { get; set; } = new();
            public List<AdvisorSeed> Advisors { get; set; } = new();
            public List<RelationSeed> BaronyRelations { get; set; } = new();
            public List<RelationModifierSeed> BaronyRelationModifiers { get; set; } = new();
            public List<UnitSeed> BaronyUnits { get; set; } = new();
            public List<BattleMapSeed> BaronyBattleMaps { get; set; } = new();
        }

        private sealed class DomainSeed
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public string LordName { get; set; } = string.Empty;
            public string ColorHex { get; set; } = "#888888";
            public bool IsPrimary { get; set; }
            public int SortOrder { get; set; }
        }

        private sealed class FiefSeed
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public string LiegeName { get; set; } = string.Empty;
            public bool IsBaronDemesne { get; set; }
            public bool IsDomainDefault { get; set; }
            public int? SeniorDomainId { get; set; }
            public string ColorHex { get; set; } = "#4d7ea8";
            public decimal BonusMultiplier { get; set; } = 1.0m;
        }

        private sealed class TileSeed
        {
            public int Id { get; set; }
            public int MapId { get; set; } = 1;
            public int X { get; set; }
            public int Y { get; set; }
            public string BaseType { get; set; } = string.Empty;
            public int FeaturesMask { get; set; }
            public int Fertility { get; set; }
            public string? Resource { get; set; }
            public int? FiefId { get; set; }
            public int? MapDomainId { get; set; }
            public string? Comment { get; set; }
        }

        private sealed class ImprovementSeed
        {
            public int Id { get; set; }
            public int? TileId { get; set; }
            public int? TemplateId { get; set; }
            public string Name { get; set; } = string.Empty;
            public string AdditiveJson { get; set; } = "{}";
            public string PercentJson { get; set; } = "{}";
            public string? Description { get; set; }
            public string? FormulaText { get; set; }
            public bool IsActive { get; set; } = true;
            public string? InactiveReason { get; set; }
            public string? IconUrl { get; set; }
            public int Population { get; set; }
            public bool HasPalisade { get; set; }
        }

        private sealed class SeatSeed
        {
            public int Id { get; set; }
            public string Name { get; set; } = "Lord's Seat";
            public int GridWidth { get; set; } = 12;
            public int GridHeight { get; set; } = 8;
            public string ActiveLevelsJson { get; set; } = "[0]";
        }

        private sealed class RoomSeed
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public int Level { get; set; }
            public int GridX { get; set; }
            public int GridY { get; set; }
            public int GridW { get; set; } = 1;
            public int GridH { get; set; } = 1;
            public string Material { get; set; } = string.Empty;
            public decimal PrestigeMultiplier { get; set; } = 1m;
            public string Status { get; set; } = string.Empty;
            public string AdditiveJson { get; set; } = string.Empty;
            public string PercentJson { get; set; } = string.Empty;
            public int? PurposeTemplateId { get; set; }
            public int? OccupantAdvisorId { get; set; }
            public string OccupantCustom { get; set; } = string.Empty;
            public int SortOrder { get; set; }
        }

        private sealed class RoomTraitSeed
        {
            public int Id { get; set; }
            public int RoomId { get; set; }
            public string Kind { get; set; } = string.Empty;
            public string Text { get; set; } = string.Empty;
            public int SortOrder { get; set; }
        }

        private sealed class SeatTileSeed
        {
            public int Id { get; set; }
            public int Level { get; set; }
            public int X { get; set; }
            public int Y { get; set; }
            public string Kind { get; set; } = string.Empty;
        }

        private sealed class AvailableAdvisorSeed
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public string? Description { get; set; }
            public string SkillsJson { get; set; } = "{}";
            public string SheetJson { get; set; } = "{}";
        }

        private sealed class AdvisorSeed
        {
            public int Id { get; set; }
            public string OfficeType { get; set; } = string.Empty;
            public string Title { get; set; } = string.Empty;
            public string PersonName { get; set; } = string.Empty;
            public bool IsBaron { get; set; }
            public int? AvailableAdvisorId { get; set; }
            public string SkillsJson { get; set; } = "{}";
            public string SignificantSkillsJson { get; set; } = "[]";
            public string AdditiveJson { get; set; } = "{}";
            public string PercentJson { get; set; } = "{}";
            public string? FormulaText { get; set; }
            public string? Description { get; set; }
            public decimal UpkeepGold { get; set; }
        }

        private sealed class RelationSeed
        {
            public int Id { get; set; }
            public string Category { get; set; } = string.Empty;
            public string GroupName { get; set; } = string.Empty;
            public string Name { get; set; } = string.Empty;
            public string Title { get; set; } = string.Empty;
            public int? Age { get; set; }
            public string Description { get; set; } = string.Empty;
            public int TroopCount { get; set; }
            public string RelationDescription { get; set; } = string.Empty;
            public string? Notes { get; set; }
            public string MarksJson { get; set; } = "[]";
            public int SortOrder { get; set; }
            public int? FiefId { get; set; }
        }

        private sealed class RelationModifierSeed
        {
            public int Id { get; set; }
            public int RelationId { get; set; }
            public string Description { get; set; } = string.Empty;
            public int Value { get; set; }
            public int SortOrder { get; set; }
        }

        private sealed class UnitSeed
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public string Status { get; set; } = string.Empty;
            public int TroopCount { get; set; }
            public string RecruitSelectionKey { get; set; } = string.Empty;
            public string TrainingTypeKey { get; set; } = string.Empty;
            public string RaceKey { get; set; } = string.Empty;
            public int Wage { get; set; }
            public decimal UpkeepFood { get; set; }
            public int UpkeepDefense { get; set; }
            public int Build { get; set; }
            public int Agility { get; set; }
            public int Will { get; set; }
            public int Perception { get; set; }
            public int AttrPenaltyBuild { get; set; }
            public int AttrPenaltyAgility { get; set; }
            public int AttrOtherBuild { get; set; }
            public int AttrOtherAgility { get; set; }
            public int AttrOtherWill { get; set; }
            public int AttrOtherPerception { get; set; }
            public string SkillsJson { get; set; } = "{}";
            public string SkillOtherJson { get; set; } = "{}";
            public string CombatOtherJson { get; set; } = "{}";
            public string SkillOtherSourcesJson { get; set; } = "{}";
            public string AttrOtherSourcesJson { get; set; } = "{}";
            public string? Weapon1Key { get; set; }
            public string? Weapon2Key { get; set; }
            public string? ArmorKey { get; set; }
            public string? ShieldKey { get; set; }
            public string? MountKey { get; set; }
            public string Weapon1Quality { get; set; } = string.Empty;
            public string Weapon2Quality { get; set; } = string.Empty;
            public string DefenseSkillKey { get; set; } = string.Empty;
            public int CommanderAttack { get; set; }
            public int CommanderDefense { get; set; }
            public int? CaptainAvailableAdvisorId { get; set; }
            public int OtherAttack { get; set; }
            public int OtherDefense { get; set; }
            public int OtherDamage { get; set; }
            public int OtherMove { get; set; }
            public int OtherArmor { get; set; }
            public int OtherHp { get; set; }
            public int RemainingPd { get; set; }
            public int Discipline { get; set; } = 1;
            public int MaxBaseSkillAtGraduation { get; set; }
            public int FreeAttributePoints { get; set; }
            public int CurrentHp { get; set; }
            public string LogJson { get; set; } = "[]";
        }

        private sealed class BattleMapSeed
        {
            public bool IsActive { get; set; }
            public string Phase { get; set; } = "setup";
            public int Width { get; set; } = 20;
            public int Height { get; set; } = 16;
            public string CellsJson { get; set; } = "[]";
            public string TokensJson { get; set; } = "[]";
            public string TurnStateJson { get; set; } = "{}";
            public string LogJson { get; set; } = "[]";
            public string TalliesJson { get; set; } = "[]";
            public string XpSummaryJson { get; set; } = "null";
        }
    }
}
