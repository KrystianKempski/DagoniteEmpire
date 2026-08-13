using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using DA_DataAccess.BaronyData;
using DA_DataAccess.Data;
using Microsoft.EntityFrameworkCore;

namespace DA_Business.Repository.BaronyRepos
{
    /// <summary>
    /// Seeds the full starting state of the <c>Darkhold</c> player barony from an embedded
    /// snapshot (terrain map, fiefs/domains, terrain improvements, lord's seat, courtiers and
    /// vassal/neighbor relations). Original snapshot ids are remapped onto the freshly created
    /// barony. Idempotent: skips when the barony already has terrain domains.
    /// </summary>
    public static class DarkholdSeeder
    {
        public const string BaronyName = "Darkhold";

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
            var relRows = seed.BaronyRelations
                .Select(s => (s.Id, Entity: new BaronyRelation
                {
                    BaronyId = baronyId,
                    Category = s.Category,
                    GroupName = s.GroupName,
                    Name = s.Name,
                    Title = s.Title,
                    Age = s.Age,
                    Description = s.Description,
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
                        Description = s.Description,
                        Value = s.Value,
                        SortOrder = s.SortOrder,
                    }));
            }

            await ctx.SaveChangesAsync();
        }

        private static int? Remap(IReadOnlyDictionary<int, int> map, int? oldId) =>
            oldId is int id && map.TryGetValue(id, out var newId) ? newId : null;

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
    }
}
