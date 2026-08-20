using System.Text.Json;
using DA_Business.Repository.CharacterReps.IRepository;
using DA_Common;
using DA_Common.Barony;
using DA_Common.Localization;
using DA_DataAccess.BaronyData;
using DA_DataAccess.CharacterClasses;
using DA_DataAccess.Chat;
using DA_DataAccess.Data;
using DA_Models.BaronyModels;
using DA_Models.CharacterModels;
using DagoniteEmpire.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace DA_Business.Repository.BaronyRepos
{
    /// <summary>
    /// Repozytorium warstwy baronii. Wzorem BattleMapRepository operuje na własnych kontekstach
    /// z IDbContextFactory i ręcznie mapuje encje ↔ DTO (wektory PPB trzymane jako JSON).
    /// </summary>
    public class BaronyRepository : IBaronyRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _db;
        private readonly ICharacterRepository _characters;
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

        public BaronyRepository(IDbContextFactory<ApplicationDbContext> db, ICharacterRepository characters)
        {
            _db = db;
            _characters = characters;
        }

        // ---------------- JSON helpers ----------------
        private static string Ser(PpbVector? v)
        {
            (v ?? new PpbVector()).EnsureSize();
            return JsonSerializer.Serialize(v ?? new PpbVector(), JsonOptions);
        }

        private static PpbVector De(string? json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return new PpbVector();
            try
            {
                var v = JsonSerializer.Deserialize<PpbVector>(json, JsonOptions) ?? new PpbVector();
                v.EnsureSize();
                return v;
            }
            catch
            {
                return new PpbVector();
            }
        }

        private static RepositoryErrorException Err(System.Exception ex, string method) =>
            new RepositoryErrorException("Error in " + method + ": " + ex.Message, ex);

        // ---------------- Barony ----------------
        public async Task<BaronyDTO?> GetByCharacterId(int characterId)
        {
            try
            {
                using var ctx = await _db.CreateDbContextAsync();
                var e = await ctx.Baronies.AsNoTracking().FirstOrDefaultAsync(b => b.CharacterId == characterId);
                return e is null ? null : ToDTO(e);
            }
            catch (System.Exception ex) { throw Err(ex, nameof(GetByCharacterId)); }
        }

        public async Task<BaronyDTO?> GetById(int id)
        {
            try
            {
                using var ctx = await _db.CreateDbContextAsync();
                var e = await ctx.Baronies.AsNoTracking().FirstOrDefaultAsync(b => b.Id == id);
                return e is null ? null : ToDTO(e);
            }
            catch (System.Exception ex) { throw Err(ex, nameof(GetById)); }
        }

        public async Task<List<BaronyListItemDTO>> GetAllSummaries()
        {
            try
            {
                using var ctx = await _db.CreateDbContextAsync();
                return await ctx.Baronies.AsNoTracking()
                    .Join(
                        ctx.Characters.AsNoTracking(),
                        b => b.CharacterId,
                        c => c.Id,
                        (b, c) => new BaronyListItemDTO
                        {
                            Id = b.Id,
                            CharacterId = b.CharacterId,
                            Name = b.Name,
                            BaronName = c.NPCName ?? string.Empty,
                            Notes = b.Notes,
                        })
                    .OrderBy(b => b.Name)
                    .ToListAsync();
            }
            catch (System.Exception ex) { throw Err(ex, nameof(GetAllSummaries)); }
        }

        public async Task<BaronyDTO> CreateForCharacter(int characterId, string name, string? notes = null, string? seedProfile = null)
        {
            try
            {
                using var ctx = await _db.CreateDbContextAsync();

                var character = await ctx.Characters.FirstOrDefaultAsync(c => c.Id == characterId);
                if (character is null)
                    throw new RepositoryErrorException(Loc.T("Character not found."));
                if (character.NPCType != SD.NPCType.Duke)
                    throw new RepositoryErrorException(Loc.T("Only Duke-type characters can be assigned as baron."));
                if (!character.IsApproved)
                    throw new RepositoryErrorException(Loc.T("Character must be approved before becoming a baron."));

                var existing = await ctx.Baronies.FirstOrDefaultAsync(b => b.CharacterId == characterId);
                if (existing is not null)
                    throw new RepositoryErrorException(Loc.T("This character already has a barony."));

                var entity = new Barony
                {
                    CharacterId = characterId,
                    Name = string.IsNullOrWhiteSpace(name) ? "New Barony" : name.Trim(),
                    Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim(),
                    TerrainMapWidth = TerrainMapGrid.DefaultSize,
                    TerrainMapHeight = TerrainMapGrid.DefaultSize,
                    Year = 625,
                    Month = 3,
                    TurnNumber = 1,
                    Season = "Spring",
                    ConjunctureDice = RollService.Roll2d6().Sum,
                    ConjunctureModifier = 0,
                    LiegeTributePercent = FiefTributeFormulas.DefaultPercent,
                    VassalTributePercent = FiefTributeFormulas.DefaultPercent,
                    BaseParametersJson = Ser(new PpbVector()),
                    ResourceStocksJson = Ser(new PpbVector()),
                    PreviousTurnIncomeJson = Ser(new PpbVector()),
                    PreviousTurnStockJson = Ser(new PpbVector()),
                };
                var added = await ctx.Baronies.AddAsync(entity);
                await ctx.SaveChangesAsync();

                var baronName = character.NPCName ?? "Baron";
                var profile = (seedProfile ?? string.Empty).Trim().ToLowerInvariant();
                if (string.IsNullOrWhiteSpace(profile))
                    profile = DarkholdSeeder.IsDarkhold(added.Entity.Name) ? "darkhold" : "custom";

                if (profile == "darkhold")
                {
                    // Darkhold ships a hand-authored starting state (terrain, fiefs, seat,
                    // courtiers, vassals + neighbors). Senior houses / organizations stay generic.
                    SeniorHousesSeeder.EnsureForBarony(ctx, added.Entity.Id);
                    OrganizationsSeeder.EnsureForBarony(ctx, added.Entity.Id);
                    await DarkholdSeeder.SeedAsync(ctx, added.Entity.Id, baronName);
                    await EnsureStarterCityBuildingsAsync(ctx, added.Entity.Id);
                    SeedStarterUnits(ctx, added.Entity.Id);
                }
                else if (profile == "custom")
                {
                    // Custom profile intentionally starts blank in all seeded barony sections.
                }
                else
                {
                    throw new RepositoryErrorException(Loc.T("Unsupported barony seed profile: {0}", seedProfile ?? string.Empty));
                }

                // Permanent work-calendar decrees exist for every barony (Darkhold + custom).
                PermanentDecreesSeeder.EnsureForBarony(ctx, added.Entity.Id);

                ApplyStarterResourceStocks(added.Entity);
                SeedBaronCampaign(ctx, character, added.Entity);
                await ctx.SaveChangesAsync();

                return ToDTO(added.Entity);
            }
            catch (RepositoryErrorException) { throw; }
            catch (System.Exception ex) { throw Err(ex, nameof(CreateForCharacter)); }
        }

        private static void ApplyStarterResourceStocks(Barony barony)
        {
            var stocks = new PpbVector
            {
                [Ppb.Food] = 5m,
                [Ppb.Production] = 50m,
                [Ppb.Defense] = 50m,
                [Ppb.Treasury] = 100m,
            };

            barony.FoodInGranaries = stocks[Ppb.Food];
            barony.TreasuryGold = stocks[Ppb.Treasury];
            barony.ResourceStocksJson = Ser(stocks);
        }

        /// <summary>
        /// Each new barony gets a dedicated campaign with the baron character as a member.
        /// </summary>
        private static void SeedBaronCampaign(ApplicationDbContext ctx, Character character, Barony barony)
        {
            var baronName = string.IsNullOrWhiteSpace(character.NPCName) ? "Baron" : character.NPCName.Trim();
            var description = string.IsNullOrWhiteSpace(barony.Notes)
                ? $"Campaign for the barony of {barony.Name} (baron: {baronName})."
                : barony.Notes.Trim();

            ctx.Campaigns.Add(new Campaign
            {
                Name = barony.Name,
                Description = description,
                CreatedDate = DateTime.Now,
                Characters = new List<Character> { character },
            });
        }

        public async Task<BaronyDTO> UpdateBarony(BaronyDTO dto)
        {
            try
            {
                using var ctx = await _db.CreateDbContextAsync();
                var e = await ctx.Baronies.FirstOrDefaultAsync(b => b.Id == dto.Id);
                if (e is null)
                {
                    var created = ToEntity(dto);
                    var added = ctx.Baronies.Add(created);
                    await ctx.SaveChangesAsync();
                    return ToDTO(added.Entity);
                }
                ApplyBarony(e, dto);
                await ctx.SaveChangesAsync();
                return ToDTO(e);
            }
            catch (System.Exception ex) { throw Err(ex, nameof(UpdateBarony)); }
        }

        public async Task<BaronyDTO> SetPlayerTurnReady(int baronyId, bool ready)
        {
            try
            {
                using var ctx = await _db.CreateDbContextAsync();
                var e = await ctx.Baronies.FirstOrDefaultAsync(b => b.Id == baronyId)
                    ?? throw new InvalidOperationException("Barony not found.");
                e.PlayerTurnReady = ready;
                await ctx.SaveChangesAsync();
                return ToDTO(e);
            }
            catch (System.Exception ex) when (ex is not InvalidOperationException)
            {
                throw Err(ex, nameof(SetPlayerTurnReady));
            }
        }

        public async Task<HashSet<string>> GetTradeGoodMgOverrideKeys(int baronyId)
        {
            try
            {
                using var ctx = await _db.CreateDbContextAsync();
                var json = await ctx.Baronies.AsNoTracking()
                    .Where(b => b.Id == baronyId)
                    .Select(b => b.AvailableTradeGoodsJson)
                    .FirstOrDefaultAsync();
                return TradeGoodAvailability.NormalizeOverrideKeys(ParseTradeGoodKeys(json));
            }
            catch (System.Exception ex) { throw Err(ex, nameof(GetTradeGoodMgOverrideKeys)); }
        }

        public async Task SetTradeGoodMgOverrideKeys(int baronyId, IReadOnlyCollection<string> keys)
        {
            try
            {
                var normalized = TradeGoodAvailability.NormalizeOverrideKeys(keys)
                    .OrderBy(k => k, StringComparer.OrdinalIgnoreCase)
                    .ToList();

                using var ctx = await _db.CreateDbContextAsync();
                var e = await ctx.Baronies.FirstOrDefaultAsync(b => b.Id == baronyId)
                    ?? throw new InvalidOperationException("Barony not found.");
                e.AvailableTradeGoodsJson = JsonSerializer.Serialize(normalized, JsonOptions);
                await ctx.SaveChangesAsync();
            }
            catch (System.Exception ex) when (ex is not InvalidOperationException)
            {
                throw Err(ex, nameof(SetTradeGoodMgOverrideKeys));
            }
        }

        public async Task<TradeGoodAvailabilitySnapshot> GetTradeGoodAvailability(int baronyId)
        {
            try
            {
                using var ctx = await _db.CreateDbContextAsync();
                var barony = await ctx.Baronies.AsNoTracking()
                    .FirstOrDefaultAsync(b => b.Id == baronyId)
                    ?? throw new InvalidOperationException("Barony not found.");
                return await ResolveTradeGoodAvailabilityAsync(ctx, baronyId, barony);
            }
            catch (System.Exception ex) when (ex is not InvalidOperationException)
            {
                throw Err(ex, nameof(GetTradeGoodAvailability));
            }
        }

        private async Task<TradeGoodAvailabilitySnapshot> ResolveTradeGoodAvailabilityAsync(
            ApplicationDbContext ctx,
            int baronyId,
            Barony barony)
        {
            var buildingNames = await ctx.BaronyBuildings.AsNoTracking()
                .Where(b => b.BaronyId == baronyId)
                .Select(b => b.Name)
                .ToListAsync();

            var improvementNames = await LoadPrimaryDomainActiveImprovementNamesAsync(ctx, baronyId);
            var facilityNames = buildingNames.Concat(improvementNames);
            var treaties = ParseTradeTreaties(barony.TradeTreatiesJson);
            var overrides = TradeGoodAvailability.NormalizeOverrideKeys(ParseTradeGoodKeys(barony.AvailableTradeGoodsJson));
            return TradeGoodAvailability.Resolve(facilityNames, treaties, overrides);
        }

        private static void EnsureUnitGearMeetsRequirements(
            BaronyUnitDTO unit,
            UnitWeaponDef weapon1,
            UnitWeaponDef? weapon2,
            UnitArmorDef? armor,
            UnitArmorDef? shield,
            UnitMountDef? mount,
            TradeGoodAvailabilitySnapshot availability)
        {
            var build = unit.EffectiveBuild;
            var agility = unit.EffectiveAgility;
            var skillTotals = UnitStatHelper.BuildSkillTotals(unit);
            var armorSkill = skillTotals.GetValueOrDefault(UnitSkillKey.ArmorSkill);
            var ridingSkill = skillTotals.GetValueOrDefault(UnitSkillKey.Riding);

            if (!UnitEquipmentTradeAccess.MeetsWeapon(weapon1, build, agility, availability, out var w1Why))
                throw new InvalidOperationException(w1Why);
            if (weapon2 is not null
                && !UnitEquipmentTradeAccess.MeetsWeapon(weapon2, build, agility, availability, out var w2Why))
                throw new InvalidOperationException(w2Why);
            if (armor is not null
                && !UnitEquipmentTradeAccess.MeetsArmor(armor, build, armorSkill, availability, out var arWhy))
                throw new InvalidOperationException(arWhy);
            if (shield is not null
                && !UnitEquipmentTradeAccess.MeetsArmor(shield, build, armorSkill, availability, out var shWhy))
                throw new InvalidOperationException(shWhy);
            if (mount is not null
                && !UnitEquipmentTradeAccess.MeetsMount(mount, ridingSkill, availability, out var mtWhy))
                throw new InvalidOperationException(mtWhy);
        }

        private static async Task<List<string>> LoadPrimaryDomainActiveImprovementNamesAsync(
            ApplicationDbContext ctx,
            int baronyId)
        {
            var primaryDomainId = await ctx.TerrainMapDomains.AsNoTracking()
                .Where(d => d.BaronyId == baronyId && d.IsPrimary)
                .Select(d => (int?)d.Id)
                .FirstOrDefaultAsync();

            var tileQuery = ctx.TerrainTiles.AsNoTracking().Where(t => t.BaronyId == baronyId);
            if (primaryDomainId is int pid)
                tileQuery = tileQuery.Where(t => t.MapDomainId == pid);

            var playerTileIds = await tileQuery.Select(t => t.Id).ToListAsync();
            var tileIdSet = playerTileIds.ToHashSet();

            return (await ctx.TerrainImprovements.AsNoTracking()
                    .Where(x => x.BaronyId == baronyId && x.IsActive)
                    .ToListAsync())
                .Where(e => e.TileId is int tid && tileIdSet.Contains(tid))
                .SelectMany(e => TradeGoodAvailability.FacilityNamesFromMapImprovement(e.Name, e.Description))
                .ToList();
        }

        public async Task<string> GetLuxuryGoodsAccessKey(int baronyId)
        {
            try
            {
                using var ctx = await _db.CreateDbContextAsync();
                var key = await ctx.Baronies.AsNoTracking()
                    .Where(b => b.Id == baronyId)
                    .Select(b => b.LuxuryGoodsAccessKey)
                    .FirstOrDefaultAsync();
                return LuxuryGoodsAccessCatalog.Find(key).Key;
            }
            catch (System.Exception ex) { throw Err(ex, nameof(GetLuxuryGoodsAccessKey)); }
        }

        public async Task SetLuxuryGoodsAccessKey(int baronyId, string key)
        {
            try
            {
                var tier = LuxuryGoodsAccessCatalog.Find(key);
                using var ctx = await _db.CreateDbContextAsync();
                var e = await ctx.Baronies.FirstOrDefaultAsync(b => b.Id == baronyId)
                    ?? throw new InvalidOperationException("Barony not found.");
                e.LuxuryGoodsAccessKey = tier.Key;
                await ctx.SaveChangesAsync();
            }
            catch (System.Exception ex) when (ex is not InvalidOperationException)
            {
                throw Err(ex, nameof(SetLuxuryGoodsAccessKey));
            }
        }

        public async Task<List<BaronyTradeTreaty>> GetTradeTreaties(int baronyId)
        {
            try
            {
                using var ctx = await _db.CreateDbContextAsync();
                var json = await ctx.Baronies.AsNoTracking()
                    .Where(b => b.Id == baronyId)
                    .Select(b => b.TradeTreatiesJson)
                    .FirstOrDefaultAsync();
                return ParseTradeTreaties(json);
            }
            catch (System.Exception ex) { throw Err(ex, nameof(GetTradeTreaties)); }
        }

        public async Task SaveTradeTreaties(int baronyId, IReadOnlyList<BaronyTradeTreaty> treaties)
        {
            try
            {
                var normalized = NormalizeTradeTreaties(treaties);
                using var ctx = await _db.CreateDbContextAsync();
                var e = await ctx.Baronies.FirstOrDefaultAsync(b => b.Id == baronyId)
                    ?? throw new InvalidOperationException("Barony not found.");
                e.TradeTreatiesJson = JsonSerializer.Serialize(normalized, JsonOptions);
                await ctx.SaveChangesAsync();
            }
            catch (System.Exception ex) when (ex is not InvalidOperationException)
            {
                throw Err(ex, nameof(SaveTradeTreaties));
            }
        }

        public async Task<HashSet<string>> GetBlockedTradeLordKeys(int baronyId)
        {
            try
            {
                using var ctx = await _db.CreateDbContextAsync();
                var json = await ctx.Baronies.AsNoTracking()
                    .Where(b => b.Id == baronyId)
                    .Select(b => b.BlockedTradeLordKeysJson)
                    .FirstOrDefaultAsync();
                return ParseLordKeySet(json);
            }
            catch (System.Exception ex) { throw Err(ex, nameof(GetBlockedTradeLordKeys)); }
        }

        public async Task SetBlockedTradeLordKeys(int baronyId, IReadOnlyCollection<string> lordKeys)
        {
            try
            {
                var normalized = (lordKeys ?? Array.Empty<string>())
                    .Where(k => !string.IsNullOrWhiteSpace(k))
                    .Select(k => k.Trim())
                    .Where(k => KnownLordsCatalog.FindByKey(k) is not null)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(k => k, StringComparer.OrdinalIgnoreCase)
                    .ToList();

                using var ctx = await _db.CreateDbContextAsync();
                var e = await ctx.Baronies.FirstOrDefaultAsync(b => b.Id == baronyId)
                    ?? throw new InvalidOperationException("Barony not found.");
                e.BlockedTradeLordKeysJson = JsonSerializer.Serialize(normalized, JsonOptions);
                await ctx.SaveChangesAsync();
            }
            catch (System.Exception ex) when (ex is not InvalidOperationException)
            {
                throw Err(ex, nameof(SetBlockedTradeLordKeys));
            }
        }

        private static HashSet<string> ParseLordKeySet(string? json)
        {
            var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (string.IsNullOrWhiteSpace(json))
                return set;
            try
            {
                var list = JsonSerializer.Deserialize<List<string>>(json, JsonOptions);
                if (list is null)
                    return set;
                foreach (var key in list)
                {
                    if (!string.IsNullOrWhiteSpace(key) && KnownLordsCatalog.FindByKey(key) is not null)
                        set.Add(key.Trim());
                }
            }
            catch
            {
                /* ignore corrupt json */
            }

            return set;
        }

        public async Task<IReadOnlyDictionary<string, string>> GetKnownLordNotes(int baronyId)
        {
            try
            {
                using var ctx = await _db.CreateDbContextAsync();
                var json = await ctx.Baronies.AsNoTracking()
                    .Where(b => b.Id == baronyId)
                    .Select(b => b.KnownLordNotesJson)
                    .FirstOrDefaultAsync();
                return ParseKnownLordNotes(json);
            }
            catch (System.Exception ex) { throw Err(ex, nameof(GetKnownLordNotes)); }
        }

        public async Task SaveKnownLordNote(int baronyId, string lordKey, string? notes)
        {
            try
            {
                var key = lordKey?.Trim() ?? "";
                if (string.IsNullOrWhiteSpace(key) || KnownLordsCatalog.FindByKey(key) is null)
                    throw new InvalidOperationException("Unknown lord.");

                using var ctx = await _db.CreateDbContextAsync();
                var e = await ctx.Baronies.FirstOrDefaultAsync(b => b.Id == baronyId)
                    ?? throw new InvalidOperationException("Barony not found.");

                var map = ParseKnownLordNotes(e.KnownLordNotesJson).ToDictionary(
                    k => k.Key,
                    v => v.Value,
                    StringComparer.OrdinalIgnoreCase);

                if (string.IsNullOrWhiteSpace(notes))
                    map.Remove(key);
                else
                    map[key] = notes.Trim();

                e.KnownLordNotesJson = JsonSerializer.Serialize(map, JsonOptions);
                await ctx.SaveChangesAsync();
            }
            catch (System.Exception ex) when (ex is not InvalidOperationException)
            {
                throw Err(ex, nameof(SaveKnownLordNote));
            }
        }

        private static Dictionary<string, string> ParseKnownLordNotes(string? json)
        {
            var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (string.IsNullOrWhiteSpace(json))
                return map;

            try
            {
                var parsed = JsonSerializer.Deserialize<Dictionary<string, string>>(json, JsonOptions);
                if (parsed is null)
                    return map;

                foreach (var (key, value) in parsed)
                {
                    if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(value))
                        continue;
                    if (KnownLordsCatalog.FindByKey(key) is null)
                        continue;
                    map[key.Trim()] = value.Trim();
                }
            }
            catch
            {
                /* ignore corrupt json */
            }

            return map;
        }

        public async Task<IReadOnlyDictionary<string, IReadOnlyList<BaronyCharacterMarkDTO>>> GetKnownLordMarks(int baronyId)
        {
            try
            {
                using var ctx = await _db.CreateDbContextAsync();
                var json = await ctx.Baronies.AsNoTracking()
                    .Where(b => b.Id == baronyId)
                    .Select(b => b.KnownLordMarksJson)
                    .FirstOrDefaultAsync();
                return ParseKnownLordMarks(json)
                    .ToDictionary(
                        k => k.Key,
                        v => (IReadOnlyList<BaronyCharacterMarkDTO>)v.Value,
                        StringComparer.OrdinalIgnoreCase);
            }
            catch (System.Exception ex) { throw Err(ex, nameof(GetKnownLordMarks)); }
        }

        public async Task SaveKnownLordMarks(int baronyId, string lordKey, IReadOnlyList<BaronyCharacterMarkDTO> marks)
        {
            try
            {
                var key = lordKey?.Trim() ?? "";
                if (string.IsNullOrWhiteSpace(key) || KnownLordsCatalog.FindByKey(key) is null)
                    throw new InvalidOperationException("Unknown lord.");

                using var ctx = await _db.CreateDbContextAsync();
                var e = await ctx.Baronies.FirstOrDefaultAsync(b => b.Id == baronyId)
                    ?? throw new InvalidOperationException("Barony not found.");

                var map = ParseKnownLordMarks(e.KnownLordMarksJson).ToDictionary(
                    k => k.Key,
                    v => v.Value,
                    StringComparer.OrdinalIgnoreCase);

                var normalized = BaronyCharacterMarkDTO.NormalizeList(marks);
                if (normalized.Count == 0)
                    map.Remove(key);
                else
                    map[key] = normalized;

                e.KnownLordMarksJson = JsonSerializer.Serialize(map, JsonOptions);
                await ctx.SaveChangesAsync();
            }
            catch (System.Exception ex) when (ex is not InvalidOperationException)
            {
                throw Err(ex, nameof(SaveKnownLordMarks));
            }
        }

        private static Dictionary<string, List<BaronyCharacterMarkDTO>> ParseKnownLordMarks(string? json)
        {
            var map = new Dictionary<string, List<BaronyCharacterMarkDTO>>(StringComparer.OrdinalIgnoreCase);
            if (string.IsNullOrWhiteSpace(json))
                return map;

            try
            {
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.ValueKind != JsonValueKind.Object)
                    return map;

                foreach (var prop in doc.RootElement.EnumerateObject())
                {
                    if (string.IsNullOrWhiteSpace(prop.Name) || KnownLordsCatalog.FindByKey(prop.Name) is null)
                        continue;

                    var marks = ParseLordMarkValue(prop.Value);
                    if (marks.Count == 0)
                        continue;

                    map[prop.Name.Trim()] = marks;
                }
            }
            catch
            {
                /* ignore corrupt json */
            }

            return map;
        }

        private static List<BaronyCharacterMarkDTO> ParseLordMarkValue(JsonElement value)
        {
            if (value.ValueKind == JsonValueKind.Array)
            {
                var list = JsonSerializer.Deserialize<List<BaronyCharacterMarkDTO>>(value.GetRawText(), JsonOptions);
                return BaronyCharacterMarkDTO.NormalizeList(list);
            }

            if (value.ValueKind == JsonValueKind.Object)
            {
                var single = JsonSerializer.Deserialize<BaronyCharacterMarkDTO>(value.GetRawText(), JsonOptions);
                return BaronyCharacterMarkDTO.NormalizeList(single is null ? null : new[] { single });
            }

            return new List<BaronyCharacterMarkDTO>();
        }

        private static List<BaronyCharacterMarkDTO> ParseRelationMarks(string? json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return new List<BaronyCharacterMarkDTO>();

            try
            {
                var list = JsonSerializer.Deserialize<List<BaronyCharacterMarkDTO>>(json, JsonOptions);
                return BaronyCharacterMarkDTO.NormalizeList(list);
            }
            catch
            {
                return new List<BaronyCharacterMarkDTO>();
            }
        }

        private static string SerMarks(IReadOnlyList<BaronyCharacterMarkDTO>? marks) =>
            JsonSerializer.Serialize(BaronyCharacterMarkDTO.NormalizeList(marks), JsonOptions);

        private static List<BaronyTradeTreaty> ParseTradeTreaties(string? json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return new List<BaronyTradeTreaty>();
            try
            {
                return JsonSerializer.Deserialize<List<BaronyTradeTreaty>>(json, JsonOptions) ?? new List<BaronyTradeTreaty>();
            }
            catch
            {
                return new List<BaronyTradeTreaty>();
            }
        }

        private static List<BaronyTradeTreaty> NormalizeTradeTreaties(IReadOnlyList<BaronyTradeTreaty> treaties)
        {
            var knownGoods = new HashSet<string>(
                TradeGoodsCatalog.All.Select(g => g.Key),
                StringComparer.OrdinalIgnoreCase);

            var result = new List<BaronyTradeTreaty>();
            foreach (var treaty in treaties)
            {
                if (string.IsNullOrWhiteSpace(treaty.CounterpartyLordKey))
                    continue;

                if (KnownLordsCatalog.FindByKey(treaty.CounterpartyLordKey) is null)
                    continue;

                var copy = new BaronyTradeTreaty
                {
                    Id = string.IsNullOrWhiteSpace(treaty.Id) ? Guid.NewGuid().ToString("N") : treaty.Id.Trim(),
                    CounterpartyLordKey = treaty.CounterpartyLordKey.Trim(),
                    Title = string.IsNullOrWhiteSpace(treaty.Title) ? null : treaty.Title.Trim(),
                    IsApproved = treaty.IsApproved,
                    Paragraphs = MigrateParagraphsToPerSeat(treaty, knownGoods),
                };

                copy.Paragraphs = copy.Paragraphs
                    .Where(p => KnownLordsCatalog.FindByKey(p.LordKey) is not null)
                    .ToList();

                if (copy.Paragraphs.Count == 0)
                    continue;

                if (!copy.Paragraphs.Any(p => p.IsDestination))
                {
                    var dest = copy.Paragraphs[^1];
                    dest.IsDestination = true;
                    dest.CustomsGoldPerTurn = 0m;
                    dest.LordKey = copy.CounterpartyLordKey;
                }

                copy.CounterpartyLordKey = copy.Paragraphs.First(p => p.IsDestination).LordKey;
                result.Add(copy);
            }

            return result;
        }

        /// <summary>
        /// New model: one paragraph per seat. Legacy: goods on one paragraph + TransitLegs list → expand.
        /// </summary>
        private static List<TradeTreatyParagraph> MigrateParagraphsToPerSeat(
            BaronyTradeTreaty treaty,
            HashSet<string> knownGoods)
        {
            static List<string> CleanGoods(IEnumerable<string> keys, HashSet<string> known) =>
                keys.Where(known.Contains)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(k => k, StringComparer.OrdinalIgnoreCase)
                    .ToList();

            if (treaty.Paragraphs.Any(p => !string.IsNullOrWhiteSpace(p.LordKey)) &&
                treaty.Paragraphs.All(p => p.TransitLegs.Count == 0))
            {
                return treaty.Paragraphs.Select(p => new TradeTreatyParagraph
                {
                    LordKey = p.LordKey.Trim(),
                    IsDestination = p.IsDestination ||
                                    string.Equals(p.LordKey, treaty.CounterpartyLordKey, StringComparison.OrdinalIgnoreCase),
                    CustomsGoldPerTurn = p.IsDestination ||
                                         string.Equals(p.LordKey, treaty.CounterpartyLordKey, StringComparison.OrdinalIgnoreCase)
                        ? 0m
                        : (p.CustomsGoldPerTurn < 0m ? 0m : p.CustomsGoldPerTurn),
                    SweetenerGoldPerTurn = p.SweetenerGoldPerTurn,
                    BaronyGrantsGoodKeys = CleanGoods(p.BaronyGrantsGoodKeys, knownGoods),
                    CounterpartyGrantsGoodKeys = CleanGoods(p.CounterpartyGrantsGoodKeys, knownGoods),
                }).ToList();
            }

            var legacyLegs = treaty.Paragraphs.SelectMany(p => p.TransitLegs).ToList();
            var goodsSource = treaty.Paragraphs.FirstOrDefault(p =>
                p.BaronyGrantsGoodKeys.Count > 0 || p.CounterpartyGrantsGoodKeys.Count > 0)
                ?? treaty.Paragraphs.FirstOrDefault();

            var result = new List<TradeTreatyParagraph>();
            foreach (var leg in legacyLegs)
            {
                if (KnownLordsCatalog.FindByKey(leg.LordKey) is null)
                    continue;
                result.Add(new TradeTreatyParagraph
                {
                    LordKey = leg.LordKey.Trim(),
                    IsDestination = false,
                    CustomsGoldPerTurn = leg.CustomsGoldPerTurn < 0m ? 0m : leg.CustomsGoldPerTurn,
                    SweetenerGoldPerTurn = 0m,
                    BaronyGrantsGoodKeys = new List<string>(),
                    CounterpartyGrantsGoodKeys = new List<string>(),
                });
            }

            result.Add(new TradeTreatyParagraph
            {
                LordKey = treaty.CounterpartyLordKey.Trim(),
                IsDestination = true,
                CustomsGoldPerTurn = 0m,
                SweetenerGoldPerTurn = goodsSource?.SweetenerGoldPerTurn ?? 0m,
                BaronyGrantsGoodKeys = CleanGoods(goodsSource?.BaronyGrantsGoodKeys ?? Enumerable.Empty<string>(), knownGoods),
                CounterpartyGrantsGoodKeys = CleanGoods(goodsSource?.CounterpartyGrantsGoodKeys ?? Enumerable.Empty<string>(), knownGoods),
            });

            return result;
        }

        private static HashSet<string> ParseTradeGoodKeys(string? json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            try
            {
                var list = JsonSerializer.Deserialize<List<string>>(json, JsonOptions) ?? new List<string>();
                return new HashSet<string>(
                    list.Where(s => !string.IsNullOrWhiteSpace(s)).Select(s => s.Trim()),
                    StringComparer.OrdinalIgnoreCase);
            }
            catch
            {
                return new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            }
        }

        public async Task<TurnResolveReportDTO> ResolveTurn(
            int baronyId,
            PpbVector expectedIncome,
            decimal loyaltyFinal,
            decimal stabilityFinal,
            int settlementPopulation)
        {
            try
            {
                using var ctx = await _db.CreateDbContextAsync();
                var barony = await ctx.Baronies.FirstOrDefaultAsync(b => b.Id == baronyId)
                    ?? throw new InvalidOperationException("Barony not found.");

                var report = new TurnResolveReportDTO
                {
                    BaronyId = baronyId,
                    PreviousTurnNumber = barony.TurnNumber,
                    Loyalty = loyaltyFinal,
                    Stability = stabilityFinal,
                    SettlementPopulation = Math.Max(0, settlementPopulation),
                    UnrestBefore = barony.Unrest,
                };

                // 1) Rebuild Resource Balance for the new turn:
                //    snapshot opening stock → wipe all ledger sources → apply income → (projects add grants).
                var income = ResourceCatalog.Slice(expectedIncome);
                var stocks = ResourceCatalog.Slice(De(barony.ResourceStocksJson));
                stocks[Ppb.Food] = barony.FoodInGranaries;
                stocks[Ppb.Treasury] = barony.TreasuryGold;
                var openingStock = ResourceCatalog.Slice(stocks);

                var oldSources = await ctx.BaronyResourceSources
                    .Where(s => s.BaronyId == baronyId)
                    .ToListAsync();
                if (oldSources.Count > 0)
                    ctx.BaronyResourceSources.RemoveRange(oldSources);

                foreach (var info in ResourceCatalog.All)
                    stocks[info.Key] += income[info.Key];
                stocks = ResourceCatalog.Slice(stocks);
                barony.PreviousTurnStockJson = Ser(openingStock);
                barony.PreviousTurnIncomeJson = Ser(income);
                barony.ResourceStocksJson = Ser(stocks);
                barony.FoodInGranaries = stocks[Ppb.Food];
                barony.TreasuryGold = stocks[Ppb.Treasury];
                report.AppliedIncome = income.Clone();

                // 2) Tick / complete funded projects and apply their results
                var projects = await ctx.BaronyProjects
                    .Where(p => p.BaronyId == baronyId)
                    .ToListAsync();
                var templates = await ctx.BuildingTemplates.AsNoTracking().ToListAsync();
                var templateById = templates.ToDictionary(t => t.Id);

                var nextCal = BaronyCalendarFormulas.AdvanceOneTurn(
                    barony.Year, barony.Month, barony.TurnNumber, barony.Season);
                var effectStartTurn = nextCal.TurnNumber;

                foreach (var project in projects)
                {
                    var dto = ToDTO(project);
                    if (ProjectStatus.IsTerminal(dto.Status))
                    {
                        if (!ProjectStatus.IsCompleted(dto.Status))
                            continue;

                        // Already granted — never re-apply on later Resolves.
                        if (HasProjectResultsApplied(dto.Notes))
                            continue;

                        // One-time: grant only when the project completes (non-terminal path below).
                        // Never repair-stack stocks on later turns; just stamp the marker if missing.
                        if (IsOneTimeResourcesKind(dto.OutputKind ?? ""))
                        {
                            dto.Notes = MarkProjectResultsApplied(dto.Notes);
                            ApplyProject(project, dto);
                            continue;
                        }

                        if (!IsRepairableCompletedKind(dto.OutputKind))
                            continue;

                        ApplyProject(project, dto);
                        report.CompletedProjects.Add(dto.Name + " (results applied)");
                        var repair = await ApplyCompletedProjectResultsAsync(
                            ctx, barony, dto, templateById, stocks, effectStartTurn);
                        if (repair.Applied)
                        {
                            dto.Notes = MarkProjectResultsApplied(dto.Notes);
                            ApplyProject(project, dto);
                        }

                        if (repair.Notes.Count > 0)
                            report.ProjectResults.AddRange(repair.Notes);
                        else
                            report.ProjectResults.Add($"{dto.Name}: completed (no further effect).");
                        continue;
                    }

                    // Draft: not accepted yet — never tick turns.
                    if (dto.Status == ProjectStatus.Draft)
                        continue;

                    // Resource allocation: wait until fully funded; then start (In progress) and tick.
                    if (ProjectStatus.IsResourceAllocation(dto.Status))
                    {
                        if (dto.HasRemainingCost)
                            continue;
                        dto.Status = ProjectStatus.InProgress;
                    }
                    else if (dto.HasRemainingCost)
                    {
                        continue;
                    }

                    dto.TurnsRemaining = Math.Max(0, dto.TurnsRemaining - 1);
                    if (dto.TurnsRemaining > 0)
                    {
                        ApplyProject(project, dto);
                        continue;
                    }

                    dto.Status = ProjectStatus.Completed;
                    dto.TurnsRemaining = 0;
                    ApplyProject(project, dto);
                    report.CompletedProjects.Add(dto.Name);

                    var finish = await ApplyCompletedProjectResultsAsync(
                        ctx, barony, dto, templateById, stocks, effectStartTurn);
                    if (finish.Applied)
                    {
                        dto.Notes = MarkProjectResultsApplied(dto.Notes);
                        ApplyProject(project, dto);
                    }

                    if (finish.Notes.Count > 0)
                        report.ProjectResults.AddRange(finish.Notes);
                    else
                        report.ProjectResults.Add($"{dto.Name}: completed (no further effect).");
                }

                // Re-sync stocks after one-time resource grants from projects
                stocks = ResourceCatalog.Slice(stocks);
                barony.ResourceStocksJson = Ser(stocks);
                barony.FoodInGranaries = stocks[Ppb.Food];
                barony.TreasuryGold = stocks[Ppb.Treasury];

                // 3) Size from primary-domain tiles
                var primaryDomainId = await ctx.TerrainMapDomains.AsNoTracking()
                    .Where(d => d.BaronyId == baronyId && d.IsPrimary)
                    .Select(d => (int?)d.Id)
                    .FirstOrDefaultAsync();
                var size = primaryDomainId is int pid
                    ? await ctx.TerrainTiles.CountAsync(t => t.BaronyId == baronyId && t.MapDomainId == pid)
                    : await ctx.TerrainTiles.CountAsync(t => t.BaronyId == baronyId);
                barony.Size = size;
                report.Size = size;

                // 4) Loyalty / unrest when Stability ≤ 0
                var controlDc = ControlDcFormulas.ControlDc(size, report.SettlementPopulation);
                report.ControlDc = controlDc;
                if (stabilityFinal <= 0m)
                {
                    var d20 = RollService.RollD20().Sum;
                    var test = ControlDcFormulas.TestResult(loyaltyFinal, d20, controlDc);
                    var delta = ControlDcFormulas.UnrestDelta(test, controlDc);
                    report.LoyaltyTestRan = true;
                    report.LoyaltyD20 = d20;
                    report.LoyaltyTestResult = test;
                    report.UnrestDelta = delta;
                    barony.Unrest = UnrestPpbFormulas.Clamp(barony.Unrest + delta);
                }

                report.UnrestAfter = barony.Unrest;

                // 5) Advance calendar (same step precomputed for project event dating)
                var yearAdvanced = BaronyCalendarFormulas.IsNewYearTransition(barony.Season);
                barony.Year = nextCal.Year;
                barony.Month = nextCal.Month;
                barony.TurnNumber = nextCal.TurnNumber;
                barony.Season = nextCal.Season;
                report.NewTurnNumber = nextCal.TurnNumber;
                report.NewSeason = nextCal.Season;
                report.NewYear = nextCal.Year;
                report.NewMonth = nextCal.Month;
                report.YearAdvanced = yearAdvanced;

                // 5b) New year (Winter → Spring): baron + relation characters age +1
                if (yearAdvanced)
                {
                    var aging = await AgeCharactersForNewYearAsync(ctx, barony);
                    report.BaronAgeIncremented = aging.BaronAged;
                    report.RelationsAged = aging.RelationsAged;
                }

                // 6) New conjuncture
                var conj = RollService.Roll2d6();
                barony.ConjunctureDice = conj.Sum;
                report.NewConjunctureDice = conj.Sum;
                report.ConjunctureModifier = barony.ConjunctureModifier;

                // 7) Reset Baron's Time allocation for the new turn
                await ResetBaronTimeForNewTurnAsync(ctx, baronyId);

                // 8) Letter inbound quotas / awaiting-reply unlock via advanced TurnNumber
                //    (see BaronLetterRules.CountsAsReceivedThisTurn / BaronAwaitingReplyThisTurn).

                // 8b) Deferred audiences → new turn copies (last exchange carried forward)
                await AdvanceDeferredAudiencesAsync(ctx, baronyId, nextCal.TurnNumber);

                // 8c) Council: archive any still-open session for the ending turn
                await AdvanceCouncilSessionsAsync(ctx, baronyId);

                // 9) Depleted units regenerate troops toward full strength
                report.UnitTroopRegenerations = await RegenerateDepletedUnitsAsync(ctx, baronyId);

                // 9b) Unit peacetime actions: Training XP + partial demobilization
                var battleActive = await IsBaronyBattleInProgressAsync(ctx, baronyId);
                report.UnitActionResults = await ApplyUnitActionsOnResolveAsync(ctx, baronyId, battleActive);

                // 10) Clear ready flag
                barony.PlayerTurnReady = false;

                await ctx.SaveChangesAsync();
                report.SummaryText = BuildTurnSummary(report);
                return report;
            }
            catch (System.Exception ex) when (ex is not InvalidOperationException)
            {
                throw Err(ex, nameof(ResolveTurn));
            }
        }

        /// <summary>
        /// Clears spent Baron's Time actions for a new turn and restores default management spend.
        /// Percent modifiers are kept (lasting effects).
        /// </summary>
        private static async Task ResetBaronTimeForNewTurnAsync(ApplicationDbContext ctx, int baronyId)
        {
            var actions = await ctx.BaronTimeActions
                .Where(a => a.BaronyId == baronyId)
                .ToListAsync();

            var toRemove = actions.Where(a => !a.IsSystem).ToList();
            if (toRemove.Count > 0)
                ctx.BaronTimeActions.RemoveRange(toRemove);

            var management = actions.FirstOrDefault(a =>
                a.IsSystem && a.Kind == BaronTimeActionKind.Management);
            if (management is null)
            {
                ctx.BaronTimeActions.Add(new BaronTimeAction
                {
                    BaronyId = baronyId,
                    Name = BaronTimeRules.ManagementActionName,
                    Kind = BaronTimeActionKind.Management,
                    CostJc = BaronTimeRules.RequiredManagementJc,
                    Description =
                        "Essential governance each turn. Spending fewer than "
                        + $"{BaronTimeRules.RequiredManagementJc} BT causes management penalties "
                        + "(stability, loyalty, income, etc.).",
                    SortOrder = 0,
                    IsSystem = true,
                });
            }
            else
            {
                management.CostJc = BaronTimeRules.RequiredManagementJc;
                management.Name = BaronTimeRules.ManagementActionName;
                management.Kind = BaronTimeActionKind.Management;
            }
        }

        /// <summary>
        /// Understrength units regain <see cref="UnitRules.TroopRegenPerTurn"/> troops per Resolve Turn
        /// (capped at full strength). Updates CurrentHp when the unit was at its previous Max HP.
        /// </summary>
        private static async Task<List<string>> RegenerateDepletedUnitsAsync(ApplicationDbContext ctx, int baronyId)
        {
            var notes = new List<string>();
            var units = await ctx.BaronyUnits
                .Where(u => u.BaronyId == baronyId
                    && u.Status != UnitStatus.Disbanded)
                .ToListAsync();

            foreach (var unit in units)
            {
                var full = unit.MaxTroopCount > 0 ? unit.MaxTroopCount : UnitRules.DefaultTroopCount;
                if (unit.TroopCount >= full)
                    continue;

                var before = unit.TroopCount;
                var after = UnitCasualtyFormulas.Regenerate(before, full);
                if (after <= before)
                    continue;

                var dto = ToUnitDTO(unit);
                var oldMax = UnitStatHelper.Compute(dto).MaxHp;
                var wasAtMax = unit.CurrentHp >= oldMax;

                unit.TroopCount = after;
                unit.UpdatedAtUtc = DateTime.UtcNow;
                dto.TroopCount = after;
                var newMax = UnitStatHelper.Compute(dto).MaxHp;
                unit.CurrentHp = wasAtMax ? newMax : Math.Min(unit.CurrentHp, newMax);
                unit.DefenseSkillKey = dto.DefenseSkillKey;

                notes.Add($"{unit.Name}: {before} → {after}/{full}");
            }

            return notes;
        }

        private static async Task<bool> IsBaronyBattleInProgressAsync(ApplicationDbContext ctx, int baronyId)
        {
            var phase = await ctx.BaronyBattleMaps.AsNoTracking()
                .Where(m => m.BaronyId == baronyId)
                .Select(m => m.Phase)
                .FirstOrDefaultAsync();
            return string.Equals(phase, BaronyBattlePhases.Battle, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Apply Training XP and Partial demobilization for units with peacetime actions.
        /// Domain bonuses are live in Domain Panel (not applied here). Battle suppresses XP.
        /// </summary>
        private async Task<List<string>> ApplyUnitActionsOnResolveAsync(
            ApplicationDbContext ctx, int baronyId, bool battleSuppresses)
        {
            var notes = new List<string>();
            var units = await ctx.BaronyUnits
                .Where(u => u.BaronyId == baronyId
                    && u.Status == UnitStatus.Active
                    && u.CurrentAction != null
                    && u.CurrentAction != "")
                .ToListAsync();
            if (units.Count == 0)
                return notes;

            var barony = await ctx.Baronies.AsNoTracking().FirstOrDefaultAsync(b => b.Id == baronyId);
            CharacterDTO? baronCharacter = null;
            if (barony is { CharacterId: > 0 })
                baronCharacter = await _characters.GetById(barony.CharacterId, fullIncludes: true);

            foreach (var unit in units)
            {
                var action = UnitActionKind.Normalize(unit.CurrentAction);
                if (action == UnitActionKind.None)
                    continue;

                if (UnitActionKind.GrantsTrainingXp(action))
                {
                    if (battleSuppresses)
                    {
                        notes.Add($"{unit.Name}: {Loc.T("Training XP skipped (battle in progress).")}");
                    }
                    else
                    {
                        var (kind, command, strategy) = await ResolveCaptainTrainingSkillsAsync(
                            ctx, unit, baronCharacter);
                        var xp = UnitActionFormulas.TrainingXp(
                            kind, command, strategy, unit.ActionTrainingJc, battleSuppresses: false);
                        if (xp > 0)
                        {
                            unit.RemainingPd += xp;
                            unit.UpdatedAtUtc = DateTime.UtcNow;
                            notes.Add($"{unit.Name}: +{xp} XP ({UnitActionKind.DisplayName(action)})");
                            AppendUnitLog(unit, "action", Loc.T("Training: +{0} XP", xp), xp);
                        }
                        else
                        {
                            notes.Add($"{unit.Name}: {Loc.T("Training yielded 0 XP (no captain or skills).")}");
                        }
                    }
                }
                // Partial demobilization is a live Domain Panel upkeep modifier (×½), not a resolve effect.
            }

            return notes;
        }

        private async Task<(UnitCaptainKind Kind, int Command, int Strategy)> ResolveCaptainTrainingSkillsAsync(
            ApplicationDbContext ctx, BaronyUnit unit, CharacterDTO? baronCharacter)
        {
            if (unit.CaptainIsBaron)
            {
                var (cmd, strat) = CharacterCommandStrategy(baronCharacter);
                return (UnitCaptainKind.Baron, cmd, strat);
            }

            if (unit.CaptainAvailableAdvisorId is not int captainId || captainId <= 0)
                return (UnitCaptainKind.None, 0, 0);

            var person = await ctx.AvailableAdvisors.AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == captainId && a.BaronyId == unit.BaronyId);
            if (person is null)
                return (UnitCaptainKind.None, 0, 0);

            if (person.CharacterId is int linkedId && linkedId > 0)
            {
                var character = await _characters.GetById(linkedId, fullIncludes: true);
                var (cmd, strat) = CharacterCommandStrategy(character);
                return (UnitCaptainKind.LinkedCharacter, cmd, strat);
            }

            var sheet = DeserializeCourtSheet(person.SheetJson);
            var command = sheet.GetMain(CourtMainSkill.Command) + sheet.GetMainOtherSum(CourtMainSkill.Command);
            var strategy = sheet.GetSecondary(CourtSecondarySkill.StrategyTactics);
            return (UnitCaptainKind.CourtSheet, command, strategy);
        }

        private static (int Command, int Strategy) CharacterCommandStrategy(CharacterDTO? character)
        {
            if (character is null)
                return (0, 0);
            CharacterSkillRelations.Wire(character);
            return (SpecialSkill(character, UnitActionFormulas.CharacterCommandSkill),
                SpecialSkill(character, UnitActionFormulas.CharacterStrategySkill));
        }

        private static int SpecialSkill(CharacterDTO character, string name)
        {
            if (character.SpecialSkills is null)
                return 0;
            foreach (var s in character.SpecialSkills)
            {
                if (string.Equals(s.Name, name, StringComparison.OrdinalIgnoreCase))
                    return (int)Math.Floor((decimal)s.SumBonus);
            }
            return 0;
        }

        private static void AppendUnitLog(BaronyUnit unit, string kind, string text, int? xpDelta = null)
        {
            var log = DeUnitLog(unit.LogJson);
            log.Insert(0, new BaronyUnitLogEntryDTO
            {
                Id = Guid.NewGuid().ToString("N"),
                UtcAt = DateTime.UtcNow,
                Kind = kind,
                Text = text,
                XpDelta = xpDelta,
            });
            if (log.Count > 80)
                log = log.Take(80).ToList();
            unit.LogJson = SerUnitLog(log);
        }

        private sealed record ProjectApplyResult(List<string> Notes, bool Applied);

        /// <summary>
        /// Applies the finished project's OutputKind effect (unit training/reinforce, event, decree, building, resources).
        /// </summary>
        private static async Task<ProjectApplyResult> ApplyCompletedProjectResultsAsync(
            ApplicationDbContext ctx,
            Barony barony,
            BaronyProjectDTO project,
            IReadOnlyDictionary<int, BuildingTemplate> templateById,
            PpbVector stocks,
            int effectStartTurn)
        {
            var notes = new List<string>();
            var kind = project.OutputKind?.Trim() ?? "";

            if (IsUnitTrainingKind(kind))
            {
                if (ResolveProjectUnitId(project) is int unitId)
                {
                    var unit = await ctx.BaronyUnits.FirstOrDefaultAsync(u => u.Id == unitId && u.BaronyId == barony.Id);
                    if (unit is not null)
                    {
                        var was = unit.Status;
                        unit.Status = UnitStatus.Active;
                        unit.MaxBaseSkillAtGraduation = 0;
                        unit.UpdatedAtUtc = DateTime.UtcNow;
                        if (unit.CurrentHp <= 0)
                        {
                            var dto = ToUnitDTO(unit);
                            unit.CurrentHp = ComputeUnitCombat(dto).MaxHp;
                        }

                        if (project.UnitId is null or <= 0)
                            project.UnitId = unit.Id;

                        notes.Add(
                            $"Training complete: {unit.Name} ({was} → {UnitStatus.Active}).");
                        return new ProjectApplyResult(notes, Applied: true);
                    }

                    notes.Add($"{project.Name}: training project has no matching unit #{unitId}.");
                }
                else
                {
                    notes.Add($"{project.Name}: Unit Training finished but UnitId is missing.");
                }

                return new ProjectApplyResult(notes, Applied: false);
            }

            if (IsUnitReinforceKind(kind))
            {
                if (ResolveProjectUnitId(project) is int unitId)
                {
                    var unit = await ctx.BaronyUnits.FirstOrDefaultAsync(u => u.Id == unitId && u.BaronyId == barony.Id);
                    if (unit is not null)
                    {
                        var add = ResolveReinforceTroopAdd(project, unit);
                        var before = unit.TroopCount;
                        if (add > 0)
                        {
                            var dto = ToUnitDTO(unit);
                            var oldMax = UnitStatHelper.Compute(dto).MaxHp;
                            var wasAtMax = unit.CurrentHp >= oldMax;
                            var full = unit.MaxTroopCount > 0 ? unit.MaxTroopCount : UnitRules.DefaultTroopCount;
                            unit.TroopCount = Math.Clamp(
                                unit.TroopCount + add,
                                0,
                                full);
                            unit.UpdatedAtUtc = DateTime.UtcNow;
                            dto.TroopCount = unit.TroopCount;
                            var newMax = UnitStatHelper.Compute(dto).MaxHp;
                            unit.CurrentHp = wasAtMax ? newMax : Math.Min(unit.CurrentHp, newMax);
                            unit.DefenseSkillKey = dto.DefenseSkillKey;
                        }

                        if (project.UnitId is null or <= 0)
                            project.UnitId = unit.Id;

                        var fullLabel = unit.MaxTroopCount > 0 ? unit.MaxTroopCount : UnitRules.DefaultTroopCount;
                        notes.Add(
                            $"Reinforce complete: {unit.Name} troops {before} → {unit.TroopCount}/{fullLabel}"
                            + (add > 0 ? $" (+{add})." : " (no troops added — check project notes)."));
                        return new ProjectApplyResult(notes, Applied: add > 0);
                    }

                    notes.Add($"{project.Name}: reinforce project has no matching unit #{unitId}.");
                }
                else
                {
                    notes.Add($"{project.Name}: Unit Reinforce finished but UnitId is missing.");
                }

                return new ProjectApplyResult(notes, Applied: false);
            }

            if (IsUnitChangeEquipmentKind(kind))
            {
                if (ResolveProjectUnitId(project) is int equipUnitId)
                {
                    var unit = await ctx.BaronyUnits.FirstOrDefaultAsync(u =>
                        u.Id == equipUnitId && u.BaronyId == barony.Id);
                    if (unit is not null)
                    {
                        if (!TryApplyChangeEquipmentFromNotes(project, unit, out var loadoutNote))
                        {
                            notes.Add($"{project.Name}: change-equipment notes incomplete — loadout not updated.");
                            return new ProjectApplyResult(notes, Applied: false);
                        }
                        AddUnitLogEntry(
                            unit,
                            kind: "equipment",
                            text: $"Equipment changed: {loadoutNote}.");

                        if (project.UnitId is null or <= 0)
                            project.UnitId = unit.Id;

                        notes.Add($"Change equipment complete: {unit.Name} — {loadoutNote}.");
                        return new ProjectApplyResult(notes, Applied: true);
                    }

                    notes.Add($"{project.Name}: change-equipment project has no matching unit #{equipUnitId}.");
                }
                else
                {
                    notes.Add($"{project.Name}: Unit Change Equipment finished but UnitId is missing.");
                }

                return new ProjectApplyResult(notes, Applied: false);
            }

            if (IsOneTimeResourcesKind(kind))
            {
                var grant = ResourceCatalog.Slice(project.ResultAdditive);
                if (grant.IsEmpty)
                {
                    var hadNonResource = PpbCatalog.All.Any(info =>
                        !ResourceCatalog.Contains(info.Key) && project.ResultAdditive[info.Key] != 0m);
                    notes.Add(hadNonResource
                        ? $"{project.Name}: one-time resources need values in cumulative resources (Food, Gold, Production, …). Loyalty/Stability/etc. are ignored for stock grants."
                        : $"{project.Name}: one-time resources finished but Expected output (+) has no cumulative resources.");
                    return new ProjectApplyResult(notes, Applied: false);
                }

                foreach (var info in ResourceCatalog.All)
                    stocks[info.Key] += grant[info.Key];

                var name = string.IsNullOrWhiteSpace(project.Name) ? "Project grant" : project.Name.Trim();
                ctx.BaronyResourceSources.Add(new BaronyResourceSource
                {
                    BaronyId = barony.Id,
                    Name = name,
                    Description = string.IsNullOrWhiteSpace(project.ResultDescription)
                        ? $"One-time resources from project completed at Resolve Turn {effectStartTurn}."
                        : project.ResultDescription.Trim(),
                    AdditiveJson = Ser(grant),
                    IsTurnEphemeral = false,
                    VisibleOnTurn = null,
                });

                var parts = ResourceCatalog.All
                    .Where(info => grant[info.Key] != 0m)
                    .Select(info => $"{info.ShortEn} {PpbFormat.Additive(grant[info.Key])}");
                notes.Add(
                    $"One-time resources → stocks & Resource Balance “{name}”: "
                    + string.Join(", ", parts) + ".");
                return new ProjectApplyResult(notes, Applied: true);
            }

            if (string.Equals(kind, ProjectOutputKind.DecreeOrTechnology, StringComparison.OrdinalIgnoreCase)
                || kind.Contains("Decree", StringComparison.OrdinalIgnoreCase)
                || kind.Contains("Technology", StringComparison.OrdinalIgnoreCase))
            {
                var name = string.IsNullOrWhiteSpace(project.Name) ? "Completed project" : project.Name.Trim();
                var additive = project.ResultAdditive.Clone();
                var percent = project.ResultPercent.Clone();
                ctx.Decrees.Add(new Decree
                {
                    BaronyId = barony.Id,
                    Name = name,
                    Description = string.IsNullOrWhiteSpace(project.ResultDescription)
                        ? $"From completed project “{name}”."
                        : project.ResultDescription.Trim(),
                    FormulaText = null,
                    AdditiveJson = Ser(additive),
                    PercentJson = Ser(percent),
                    IsActive = true,
                });

                var hasPpb = PpbCatalog.All.Any(info =>
                    additive[info.Key] != 0m || percent[info.Key] != 0m);
                notes.Add(
                    hasPpb
                        ? $"Decree / technology added to Domain Panel: {name}."
                        : $"Decree / technology added to Domain Panel: {name} (no PPB modifiers set on the project).");
                return new ProjectApplyResult(notes, Applied: true);
            }

            if (string.Equals(kind, ProjectOutputKind.Event, StringComparison.OrdinalIgnoreCase))
            {
                var name = string.IsNullOrWhiteSpace(project.Name) ? "Completed project" : project.Name.Trim();
                ctx.BaronyEvents.Add(new BaronyEvent
                {
                    BaronyId = barony.Id,
                    Name = name,
                    Description = string.IsNullOrWhiteSpace(project.ResultDescription)
                        ? $"From completed project “{name}”."
                        : project.ResultDescription,
                    StartTurn = Math.Max(1, effectStartTurn),
                    EndTurn = null,
                    AdditiveJson = Ser(project.ResultAdditive),
                    PercentJson = Ser(project.ResultPercent),
                });
                notes.Add($"Event added to Domain Panel: {name} (from turn {effectStartTurn}, ongoing).");
                return new ProjectApplyResult(notes, Applied: true);
            }

            BuildingTemplate? template = null;
            if (project.BuildingTemplateId is int tid && templateById.TryGetValue(tid, out var t))
                template = t;

            if (project.TileId is int tileId && tileId > 0)
            {
                var existing = await ctx.TerrainImprovements
                    .FirstOrDefaultAsync(i => i.BaronyId == barony.Id && i.TileId == tileId);
                var tile = await ctx.TerrainTiles.AsNoTracking()
                    .FirstOrDefaultAsync(t => t.Id == tileId && t.BaronyId == barony.Id);

                var catalogName = template?.Name?.Trim();
                if (string.IsNullOrWhiteSpace(catalogName))
                    catalogName = null;

                var mapKind = !string.IsNullOrWhiteSpace(template?.MapPinKind)
                    ? template!.MapPinKind!.Trim()
                    : TerrainImprovementCatalogMap.MapKindFromCatalogTemplateName(catalogName)
                      ?? MapImprovement.Custom;
                var name = mapKind;
                // Match MG brush: Name = map kind (icons); Description = catalog name (Domain Panel label).
                // Village/Town place names are unknown here — keep catalog name so the label stays useful.
                var description = catalogName
                    ?? (!string.IsNullOrWhiteSpace(project.ResultDescription)
                        ? project.ResultDescription.Trim()
                        : template?.Description);

                var additive = template is not null ? De(template.EffectAdditiveJson) : project.ResultAdditive.Clone();
                var percent = template is not null ? De(template.EffectPercentJson) : project.ResultPercent.Clone();

                if (string.Equals(mapKind, MapImprovement.Village, StringComparison.OrdinalIgnoreCase)
                    && tile is not null)
                {
                    var population = existing?.Population ?? 0;
                    var hasPalisade = existing?.HasPalisade ?? false;
                    additive = VillagePpbFormulas.Compute(
                        population, tile.Fertility, hasPalisade, TownTaxRates.Defaults, barony.Season);
                    percent = new PpbVector();
                }
                else if (string.Equals(mapKind, MapImprovement.FishingHarbor, StringComparison.OrdinalIgnoreCase)
                         && tile is not null
                         && TerrainImprovementCatalogMap.HasFisheryBonus(tile.Resource))
                {
                    additive[Ppb.Food] += TerrainImprovementCatalogMap.FishingHarborFisheryFoodBonus;
                    additive[Ppb.Treasury] += TerrainImprovementCatalogMap.FishingHarborFisheryTreasuryBonus;
                }

                var iconUrl = !string.IsNullOrWhiteSpace(template?.IconUrl)
                    ? template!.IconUrl!.Trim()
                    : null;

                if (existing is null)
                {
                    ctx.TerrainImprovements.Add(new TerrainImprovement
                    {
                        BaronyId = barony.Id,
                        TileId = tileId,
                        TemplateId = template?.Id,
                        Name = name,
                        Description = description,
                        FormulaText = template?.Description,
                        AdditiveJson = Ser(additive),
                        PercentJson = Ser(percent),
                        IconUrl = iconUrl,
                        IsActive = true,
                    });
                }
                else
                {
                    existing.TemplateId = template?.Id ?? existing.TemplateId;
                    existing.Name = name;
                    existing.Description = description;
                    existing.FormulaText = template?.Description ?? existing.FormulaText;
                    existing.AdditiveJson = Ser(additive);
                    existing.PercentJson = Ser(percent);
                    existing.IconUrl = iconUrl ?? existing.IconUrl;
                    existing.IsActive = true;
                    existing.InactiveReason = null;
                }

                notes.Add($"Map improvement placed: {catalogName ?? name} ({mapKind}, tile #{tileId}).");
                return new ProjectApplyResult(notes, Applied: true);
            }

            // Map catalog improvements must not fall through into city buildings when TileId is missing.
            if (template is not null
                && string.Equals(template.Kind, BuildingKind.Improvement, StringComparison.OrdinalIgnoreCase)
                && (TerrainImprovementCatalogMap.MapKindFromCatalogTemplateName(template.Name) is not null
                    || !string.IsNullOrWhiteSpace(template.MapPinKind)))
            {
                notes.Add(
                    $"{project.Name}: catalog improvement “{template.Name}” requires a map tile (TileId) and was not applied.");
                return new ProjectApplyResult(notes, Applied: false);
            }

            if (string.Equals(kind, ProjectOutputKind.Building, StringComparison.OrdinalIgnoreCase)
                || string.Equals(kind, ProjectOutputKind.Improvement, StringComparison.OrdinalIgnoreCase))
            {
                var name = template?.Name
                    ?? (string.IsNullOrWhiteSpace(project.Name) ? "Building" : project.Name.Trim());
                var additive = template is not null ? De(template.EffectAdditiveJson) : project.ResultAdditive.Clone();
                var percent = template is not null ? De(template.EffectPercentJson) : project.ResultPercent.Clone();
                var buildingKind = template is not null && !string.IsNullOrWhiteSpace(template.Kind)
                    ? template.Kind.Trim()
                    : (string.Equals(kind, ProjectOutputKind.Improvement, StringComparison.OrdinalIgnoreCase)
                        ? BuildingKind.Improvement
                        : BuildingKind.Building);
                var description = !string.IsNullOrWhiteSpace(template?.Description)
                    ? template!.Description
                    : (!string.IsNullOrWhiteSpace(project.ResultDescription)
                        ? project.ResultDescription
                        : null);
                ctx.BaronyBuildings.Add(new BaronyBuilding
                {
                    BaronyId = barony.Id,
                    TemplateId = template?.Id,
                    Name = name,
                    Kind = buildingKind,
                    Description = description,
                    AdditiveJson = Ser(additive),
                    PercentJson = Ser(percent),
                });
                notes.Add($"{kind} added: {name}.");
                return new ProjectApplyResult(notes, Applied: true);
            }

            notes.Add($"{project.Name}: completed with unhandled output type “{kind}”.");
            return new ProjectApplyResult(notes, Applied: false);
        }

        private static bool IsOneTimeResourcesKind(string kind) =>
            string.Equals(kind, ProjectOutputKind.OneTimeResources, StringComparison.OrdinalIgnoreCase)
            || kind.Contains("One-time", StringComparison.OrdinalIgnoreCase)
            || kind.Contains("One time", StringComparison.OrdinalIgnoreCase);

        private static bool IsUnitTrainingKind(string kind) =>
            string.Equals(kind, ProjectOutputKind.UnitTraining, StringComparison.OrdinalIgnoreCase)
            || (kind.Contains("Training", StringComparison.OrdinalIgnoreCase)
                && !kind.Contains("Reinforce", StringComparison.OrdinalIgnoreCase));

        private static bool IsUnitReinforceKind(string kind) =>
            string.Equals(kind, ProjectOutputKind.UnitReinforce, StringComparison.OrdinalIgnoreCase)
            || (kind.Contains("Reinforce", StringComparison.OrdinalIgnoreCase)
                && !kind.Contains("Equipment", StringComparison.OrdinalIgnoreCase)
                && !kind.Contains("Change", StringComparison.OrdinalIgnoreCase));

        private static bool IsUnitChangeEquipmentKind(string kind) =>
            string.Equals(kind, ProjectOutputKind.UnitChangeEquipment, StringComparison.OrdinalIgnoreCase)
            || (kind.Contains("Change Equipment", StringComparison.OrdinalIgnoreCase)
                || kind.Contains("Re-equip", StringComparison.OrdinalIgnoreCase)
                || kind.Contains("ChangeEquipment", StringComparison.OrdinalIgnoreCase));

        /// <summary>
        /// Completed projects of these kinds can have results re-applied once if a prior resolve
        /// marked them done without granting the effect.
        /// One-time resources are excluded: they must grant only on the Resolve that completes them
        /// (re-running them would stack stocks every turn).
        /// </summary>
        private static bool IsRepairableCompletedKind(string? kind)
        {
            var k = kind?.Trim() ?? "";
            return IsUnitTrainingKind(k) || IsUnitReinforceKind(k) || IsUnitChangeEquipmentKind(k);
        }

        private static int? ResolveProjectUnitId(BaronyProjectDTO project)
        {
            if (project.UnitId is int id && id > 0)
                return id;

            // "Adds N troops to unit #ID (...)." / "Activates unit #ID (...)."
            foreach (var text in new[] { project.ResultDescription, project.Description })
            {
                if (string.IsNullOrWhiteSpace(text))
                    continue;
                var idx = text.IndexOf("unit #", StringComparison.OrdinalIgnoreCase);
                if (idx < 0)
                    continue;
                var parsed = ReadIntAt(text, idx + "unit #".Length);
                if (parsed > 0)
                    return parsed;
            }

            return null;
        }

        private const string ProjectResultsAppliedMarker = "ResultsApplied=1";

        private static bool HasProjectResultsApplied(string? notes) =>
            !string.IsNullOrEmpty(notes)
            && notes.Contains(ProjectResultsAppliedMarker, StringComparison.OrdinalIgnoreCase);

        private static string MarkProjectResultsApplied(string? notes)
        {
            if (HasProjectResultsApplied(notes))
                return notes!;
            return string.IsNullOrWhiteSpace(notes)
                ? ProjectResultsAppliedMarker
                : notes.TrimEnd() + "; " + ProjectResultsAppliedMarker;
        }

        /// <summary>
        /// On Winter → Spring, increment the baron's Character.Age and every relation with a set Age.
        /// </summary>
        private static async Task<(bool BaronAged, int RelationsAged)> AgeCharactersForNewYearAsync(
            ApplicationDbContext ctx, Barony barony)
        {
            var baronAged = false;
            if (barony.CharacterId > 0)
            {
                var character = await ctx.Characters.FirstOrDefaultAsync(c => c.Id == barony.CharacterId);
                if (character is not null)
                {
                    character.Age += 1;
                    baronAged = true;
                }
            }

            var relations = await ctx.BaronyRelations
                .Where(r => r.BaronyId == barony.Id && r.Age != null)
                .ToListAsync();
            foreach (var relation in relations)
                relation.Age = relation.Age!.Value + 1;

            return (baronAged, relations.Count);
        }

        private static string BuildTurnSummary(TurnResolveReportDTO r)
        {
            var lines = new List<string>
            {
                $"Turn {r.PreviousTurnNumber} resolved → Turn {r.NewTurnNumber} ({r.NewSeason} {r.NewYear}).",
                $"Resource income applied. Size {r.Size}. Control DC {r.ControlDc} (population {r.SettlementPopulation}).",
            };
            if (r.YearAdvanced)
            {
                var ageBits = new List<string>();
                if (r.BaronAgeIncremented)
                    ageBits.Add("baron +1");
                if (r.RelationsAged > 0)
                    ageBits.Add($"{r.RelationsAged} relation(s) +1");
                lines.Add(ageBits.Count > 0
                    ? $"New year: ages increased ({string.Join(", ", ageBits)})."
                    : "New year: calendar advanced (no ages to update).");
            }
            if (r.CompletedProjects.Count > 0)
            {
                lines.Add("Completed projects: " + string.Join(", ", r.CompletedProjects) + ".");
                foreach (var detail in r.ProjectResults)
                    lines.Add("  • " + detail);
            }
            else
            {
                lines.Add("No projects completed.");
            }

            if (r.UnitTroopRegenerations.Count > 0)
                lines.Add("Troop recovery (+" + UnitRules.TroopRegenPerTurn + "/turn): "
                    + string.Join("; ", r.UnitTroopRegenerations) + ".");

            if (r.UnitActionResults.Count > 0)
                lines.Add("Unit actions: " + string.Join("; ", r.UnitActionResults) + ".");

            if (r.LoyaltyTestRan)
            {
                lines.Add(
                    $"Loyalty test: {PpbFormat.Number(r.Loyalty)} + d20({r.LoyaltyD20}) − DC {r.ControlDc} = {r.LoyaltyTestResult}. "
                    + $"Unrest {r.UnrestBefore} → {r.UnrestAfter} (Δ {r.UnrestDelta}).");
            }
            else
            {
                lines.Add($"Stability {PpbFormat.Number(r.Stability)} > 0 — no loyalty test. Unrest stays {r.UnrestAfter}.");
            }

            lines.Add(
                $"New conjuncture dice: {r.NewConjunctureDice}"
                + (r.ConjunctureModifier != 0
                    ? $" (modifier {r.ConjunctureModifier:+0;-0}, effective {r.NewConjunctureDice + r.ConjunctureModifier})"
                    : "")
                + ".");
            return string.Join("\n", lines);
        }

        public async Task<BaronyOverviewDTO?> GetOverview(int baronyId)
        {
            try
            {
                using var ctx = await _db.CreateDbContextAsync();
                var barony = await ctx.Baronies.AsNoTracking().FirstOrDefaultAsync(b => b.Id == baronyId);
                if (barony is null)
                    return null;

                var primaryDomainId = await ctx.TerrainMapDomains.AsNoTracking()
                    .Where(d => d.BaronyId == baronyId && d.IsPrimary)
                    .Select(d => (int?)d.Id)
                    .FirstOrDefaultAsync();

                var tiles = await ctx.TerrainTiles.AsNoTracking()
                    .Where(x => x.BaronyId == baronyId)
                    .ToListAsync();

                // Domain Panel / PPB: only improvements on tiles inside the player's primary domain.
                var playerTileIds = primaryDomainId is int pid
                    ? tiles.Where(t => t.MapDomainId == pid).Select(t => t.Id).ToHashSet()
                    : new HashSet<int>();

                var improvements = (await ctx.TerrainImprovements.AsNoTracking()
                        .Where(x => x.BaronyId == baronyId)
                        .ToListAsync())
                    .Where(e => e.TileId is int tid && playerTileIds.Contains(tid))
                    .ToList();

                var taxRates = TownTaxRates.FromRelations(
                    (await ctx.SocialGroupRelations.AsNoTracking()
                        .Where(x => x.BaronyId == baronyId)
                        .ToListAsync())
                    .Select(r => (r.Group, r.TaxPercent)));

                var improvementDtos = improvements
                    .Select(e => ToImprovementDto(e, tiles, taxRates, barony.Season))
                    .ToList();

                SeniorHousesSeeder.EnsureForBarony(ctx, baronyId);
                OrganizationsSeeder.EnsureForBarony(ctx, baronyId);
                PermanentDecreesSeeder.EnsureForBarony(ctx, baronyId);
                DarkholdRelationLocalization.EnsurePolishForBarony(ctx, baronyId, barony.Name);
                await EnsureCoreOfficeDescriptionsAsync(ctx, baronyId);
                await EnsureStarterCityBuildingsAsync(ctx, baronyId);
                await RefreshLinkedCourtiersAsync(ctx, baronyId);
                await ctx.SaveChangesAsync();

                var availableAdvisors = (await ctx.AvailableAdvisors.AsNoTracking()
                        .Where(x => x.BaronyId == baronyId)
                        .ToListAsync())
                    .Select(ToDTO)
                    .ToList();
                var personById = availableAdvisors.ToDictionary(a => a.Id);

                var advisors = (await ctx.Advisors.AsNoTracking()
                        .Where(x => x.BaronyId == baronyId)
                        .ToListAsync())
                    .Select(e => ToAdvisorDto(e, personById))
                    .ToList();

                return new BaronyOverviewDTO
                {
                    Barony = ToDTO(barony),
                    Advisors = advisors,
                    AvailableAdvisors = availableAdvisors,
                    Buildings = (await ctx.BaronyBuildings.AsNoTracking().Where(x => x.BaronyId == baronyId).ToListAsync()).Select(ToDTO).ToList(),
                    SocialRelations = (await ctx.SocialGroupRelations.AsNoTracking().Where(x => x.BaronyId == baronyId).ToListAsync()).Select(ToDTO).ToList(),
                    Improvements = improvementDtos,
                    Decrees = (await ctx.Decrees.AsNoTracking().Where(x => x.BaronyId == baronyId).ToListAsync()).Select(ToDTO).ToList(),
                    Events = (await ctx.BaronyEvents.AsNoTracking().Where(x => x.BaronyId == baronyId).ToListAsync()).Select(ToDTO).ToList(),
                    Audiences = await LoadAudienceDtosAsync(ctx, baronyId),
                    Relations = (await ctx.BaronyRelations.AsNoTracking().Include(x => x.Modifiers).Where(x => x.BaronyId == baronyId).ToListAsync()).Select(ToDTO).ToList(),
                    Seat = await EnsureSeatDtoAsync(ctx, baronyId),
                    SeatPurposeTemplates = await LoadPurposeTemplatesAsync(ctx, baronyId),
                    Artifacts = (await ctx.BaronArtifacts.AsNoTracking()
                            .Where(x => x.BaronyId == baronyId)
                            .OrderBy(x => x.SortOrder)
                            .ThenBy(x => x.Id)
                            .ToListAsync())
                        .Select(ToDTO)
                        .ToList(),
                    CommunityModifiers = (await ctx.CommunityModifiers.AsNoTracking().Where(x => x.BaronyId == baronyId).ToListAsync()).Select(ToDTO).ToList(),
                    Fiefs = (await ctx.Fiefs.AsNoTracking().Where(x => x.BaronyId == baronyId).ToListAsync()).Select(ToDTO).ToList(),
                    Tiles = tiles.Select(ToDTO).ToList(),
                    Projects = (await ctx.BaronyProjects.AsNoTracking().Where(x => x.BaronyId == baronyId).ToListAsync()).Select(ToDTO).ToList(),
                    ResourceSources = (await ctx.BaronyResourceSources.AsNoTracking().Where(x => x.BaronyId == baronyId).ToListAsync()).Select(ToDTO).ToList(),
                    PurseSources = (await ctx.BaronPurseSources.AsNoTracking().Where(x => x.BaronyId == baronyId).ToListAsync()).Select(ToDTO).ToList(),
                    Units = (await ctx.BaronyUnits.AsNoTracking()
                            .Where(x => x.BaronyId == baronyId)
                            .OrderBy(x => x.Name)
                            .ThenBy(x => x.Id)
                            .ToListAsync())
                        .Select(ToUnitDTO)
                        .ToList(),
                };
            }
            catch (System.Exception ex) { throw Err(ex, nameof(GetOverview)); }
        }

        // ---------------- Advisors ----------------
        public async Task<List<AdvisorDTO>> GetAdvisors(int baronyId)
        {
            try
            {
                using var ctx = await _db.CreateDbContextAsync();
                await EnsureCoreOfficeDescriptionsAsync(ctx, baronyId);
                await RefreshLinkedCourtiersAsync(ctx, baronyId);
                await ctx.SaveChangesAsync();

                var personById = (await ctx.AvailableAdvisors.AsNoTracking()
                        .Where(x => x.BaronyId == baronyId)
                        .ToListAsync())
                    .ToDictionary(a => a.Id, ToDTO);

                return (await ctx.Advisors.AsNoTracking()
                        .Where(x => x.BaronyId == baronyId)
                        .ToListAsync())
                    .Select(e => ToAdvisorDto(e, personById))
                    .ToList();
            }
            catch (System.Exception ex) { throw Err(ex, nameof(GetAdvisors)); }
        }

        public async Task<List<AvailableAdvisorDTO>> GetAvailableAdvisors(int baronyId)
        {
            try
            {
                using var ctx = await _db.CreateDbContextAsync();
                await RefreshLinkedCourtiersAsync(ctx, baronyId);
                await EnsureCourtSheetCommanderCxAsync(ctx, baronyId);
                await ctx.SaveChangesAsync();
                return (await ctx.AvailableAdvisors.AsNoTracking()
                        .Where(x => x.BaronyId == baronyId)
                        .OrderBy(x => x.Name)
                        .ThenBy(x => x.Id)
                        .ToListAsync())
                    .Select(ToDTO)
                    .ToList();
            }
            catch (System.Exception ex) { throw Err(ex, nameof(GetAvailableAdvisors)); }
        }

        public async Task<AvailableAdvisorDTO> AttachCharacterAsCourtier(int baronyId, int characterId)
        {
            try
            {
                using var ctx = await _db.CreateDbContextAsync();
                var barony = await ctx.Baronies.AsNoTracking().FirstOrDefaultAsync(b => b.Id == baronyId)
                    ?? throw new InvalidOperationException(Loc.T("Barony not found."));
                if (barony.CharacterId == characterId)
                    throw new InvalidOperationException(Loc.T("The baron character is already tied to this barony."));

                var already = await ctx.AvailableAdvisors.AsNoTracking()
                    .AnyAsync(a => a.CharacterId == characterId);
                if (already)
                    throw new InvalidOperationException(Loc.T("This character is already attached as a courtier somewhere."));

                var character = await _characters.GetById(characterId, fullIncludes: true);
                if (character is null || character.Id <= 0)
                    throw new InvalidOperationException(Loc.T("Character not found."));

                var skills = CharacterBaronySkillPpb.FromCharacter(character);
                var sheet = CommanderCxFormulas.BuildCharacterCommanderSheet(null, character);
                var e = new AvailableAdvisor
                {
                    BaronyId = baronyId,
                    CharacterId = characterId,
                    Name = CharacterDisplayName(character),
                    Description = null,
                    SheetJson = JsonSerializer.Serialize(sheet, JsonOptions),
                    SkillsJson = Ser(skills),
                };
                ctx.AvailableAdvisors.Add(e);
                await ctx.SaveChangesAsync();
                return ToDTO(e);
            }
            catch (InvalidOperationException) { throw; }
            catch (System.Exception ex) { throw Err(ex, nameof(AttachCharacterAsCourtier)); }
        }

        public async Task<HashSet<int>> GetAttachedCourtierCharacterIds()
        {
            try
            {
                using var ctx = await _db.CreateDbContextAsync();
                var ids = await ctx.AvailableAdvisors.AsNoTracking()
                    .Where(a => a.CharacterId != null)
                    .Select(a => a.CharacterId!.Value)
                    .ToListAsync();
                return ids.ToHashSet();
            }
            catch (System.Exception ex) { throw Err(ex, nameof(GetAttachedCourtierCharacterIds)); }
        }

        public async Task<CourtCharacterSheet> SaveBaronCommanderSheet(int baronyId, CourtCharacterSheet sheet)
        {
            try
            {
                using var ctx = await _db.CreateDbContextAsync();
                var barony = await ctx.Baronies.FirstOrDefaultAsync(b => b.Id == baronyId)
                    ?? throw new InvalidOperationException(Loc.T("Barony not found."));

                var character = await _characters.GetById(barony.CharacterId, fullIncludes: true);
                var saved = CommanderCxFormulas.BuildCharacterCommanderSheet(sheet, character);
                barony.CommanderSheetJson = JsonSerializer.Serialize(saved, JsonOptions);
                await ctx.SaveChangesAsync();
                return saved;
            }
            catch (InvalidOperationException) { throw; }
            catch (System.Exception ex) { throw Err(ex, nameof(SaveBaronCommanderSheet)); }
        }

        /// <summary>Restore catalog office flavor text when assignment overwrote Description with a person bio.</summary>
        private static async Task EnsureCoreOfficeDescriptionsAsync(ApplicationDbContext ctx, int baronyId)
        {
            var core = await ctx.Advisors
                .Where(a => a.BaronyId == baronyId && !a.IsBaron)
                .ToListAsync();
            foreach (var a in core)
            {
                var catalog = OfficeDescriptions.For(a.OfficeType);
                if (catalog is null)
                    continue;
                if (!string.Equals(a.Description, catalog, StringComparison.Ordinal))
                    a.Description = catalog;
            }
        }

        /// <summary>
        /// Insert missing starter city buildings from the Buildings catalog (CoreKey + TemplateId).
        /// </summary>
        private static async Task EnsureStarterCityBuildingsAsync(ApplicationDbContext ctx, int baronyId)
        {
            var existingKeys = await ctx.BaronyBuildings
                .AsNoTracking()
                .Where(b => b.BaronyId == baronyId && b.CoreKey != null && b.CoreKey != "")
                .Select(b => b.CoreKey!)
                .ToListAsync();
            var have = new HashSet<string>(existingKeys, StringComparer.OrdinalIgnoreCase);

            var missingKeys = CoreCityBuildingKey.All.Where(k => !have.Contains(k)).ToList();
            if (missingKeys.Count == 0)
                return;

            var catalogNames = missingKeys.Select(CoreCityBuildingKey.CatalogName).ToList();
            var templates = await ctx.BuildingTemplates
                .AsNoTracking()
                .Where(t => catalogNames.Contains(t.Name))
                .ToListAsync();
            var byName = templates.ToDictionary(t => t.Name, StringComparer.OrdinalIgnoreCase);

            foreach (var key in missingKeys)
            {
                var name = CoreCityBuildingKey.CatalogName(key);
                if (!byName.TryGetValue(name, out var template))
                    continue;

                ctx.BaronyBuildings.Add(new BaronyBuilding
                {
                    BaronyId = baronyId,
                    TemplateId = template.Id,
                    CoreKey = key,
                    Name = template.Name,
                    Kind = BuildingKind.Building,
                    Description = template.Description,
                    AdditiveJson = template.EffectAdditiveJson,
                    PercentJson = string.IsNullOrWhiteSpace(template.EffectPercentJson)
                        ? "{}"
                        : template.EffectPercentJson,
                });
            }
        }

        /// <summary>
        /// Seeds City Watch + Baron's Guard once on barony create (not Ensure / GetOverview).
        /// </summary>
        private static void SeedStarterUnits(ApplicationDbContext ctx, int baronyId)
        {
            var now = DateTime.UtcNow;
            foreach (var dto in StarterUnitsSeeder.CreateDefaults(baronyId))
            {
                var entity = ToUnitEntity(dto);
                entity.CreatedAtUtc = now;
                entity.UpdatedAtUtc = now;
                ctx.BaronyUnits.Add(entity);
            }
        }

        public async Task<AdvisorDTO> SaveAdvisor(AdvisorDTO dto)
        {
            try
            {
                using var ctx = await _db.CreateDbContextAsync();
                var e = dto.Id > 0 ? await ctx.Advisors.FirstOrDefaultAsync(x => x.Id == dto.Id) : null;
                if (e is null)
                {
                    e = ToEntity(dto);
                    ctx.Advisors.Add(e);
                }
                else
                {
                    ApplyAdvisor(e, dto);
                }
                await ctx.SaveChangesAsync();
                return ToDTO(e);
            }
            catch (System.Exception ex) { throw Err(ex, nameof(SaveAdvisor)); }
        }

        public Task<int> DeleteAdvisor(int id) => Delete(ctx => ctx.Advisors, id, nameof(DeleteAdvisor));

        public async Task<AvailableAdvisorDTO> SaveAvailableAdvisor(AvailableAdvisorDTO dto)
        {
            try
            {
                using var ctx = await _db.CreateDbContextAsync();
                var e = dto.Id > 0 ? await ctx.AvailableAdvisors.FirstOrDefaultAsync(x => x.Id == dto.Id) : null;
                if (e is null)
                {
                    // Character links go through AttachCharacterAsCourtier only.
                    dto.CharacterId = null;
                    e = ToEntity(dto);
                    ctx.AvailableAdvisors.Add(e);
                    await ctx.SaveChangesAsync();
                }
                else if (e.CharacterId is > 0)
                {
                    e.Description = dto.Description;
                    var character = await _characters.GetById(e.CharacterId.Value, fullIncludes: true);
                    if (character is not null && character.Id > 0)
                    {
                        e.Name = CharacterDisplayName(character);
                        e.SkillsJson = Ser(CharacterBaronySkillPpb.FromCharacter(character));
                    }

                    var sheet = CommanderCxFormulas.BuildCharacterCommanderSheet(
                        dto.Sheet ?? DeserializeCourtSheet(e.SheetJson),
                        character);
                    e.SheetJson = JsonSerializer.Serialize(sheet, JsonOptions);

                    var skillsJson = e.SkillsJson;
                    var offices = await ctx.Advisors
                        .Where(a => a.AvailableAdvisorId == e.Id)
                        .ToListAsync();
                    foreach (var advisor in offices)
                    {
                        advisor.SkillsJson = skillsJson;
                        advisor.PersonName = e.Name;
                    }
                    await ResyncUnitsForCaptainAsync(ctx, e.Id, e.BaronyId);
                    await ctx.SaveChangesAsync();
                }
                else
                {
                    dto.CharacterId = null;
                    ApplyAvailableAdvisor(e, dto);
                    var skillsJson = e.SkillsJson;
                    var linked = await ctx.Advisors
                        .Where(a => a.AvailableAdvisorId == e.Id)
                        .ToListAsync();
                    foreach (var advisor in linked)
                        advisor.SkillsJson = skillsJson;
                    await ResyncUnitsForCaptainAsync(ctx, e.Id, e.BaronyId);
                    await ctx.SaveChangesAsync();
                }
                return ToDTO(e);
            }
            catch (System.Exception ex) { throw Err(ex, nameof(SaveAvailableAdvisor)); }
        }

        public async Task<int> DeleteAvailableAdvisor(int id)
        {
            try
            {
                using var ctx = await _db.CreateDbContextAsync();
                var linked = await ctx.Advisors
                    .Where(a => a.AvailableAdvisorId == id)
                    .ToListAsync();
                foreach (var advisor in linked)
                    advisor.AvailableAdvisorId = null;

                var captained = await ctx.BaronyUnits
                    .Where(u => u.CaptainAvailableAdvisorId == id)
                    .ToListAsync();
                foreach (var unit in captained)
                {
                    unit.CaptainAvailableAdvisorId = null;
                    var dto = ToUnitDTO(unit);
                    var ca = 0;
                    var cd = 0;
                    var oa = dto.OtherAttack;
                    var od = dto.OtherDefense;
                    var odm = dto.OtherDamage;
                    var om = dto.OtherMove;
                    var oar = dto.OtherArmor;
                    var oh = dto.OtherHp;
                    UnitCommanderSync.ClearCaptainBonuses(
                        ref ca, ref cd, ref oa, ref od, ref odm, ref om, ref oar, ref oh, dto.CombatOther);
                    dto.CommanderAttack = ca;
                    dto.CommanderDefense = cd;
                    dto.OtherAttack = oa;
                    dto.OtherDefense = od;
                    dto.OtherDamage = odm;
                    dto.OtherMove = om;
                    dto.OtherArmor = oar;
                    dto.OtherHp = oh;
                    ApplyUnit(unit, dto);
                    unit.UpdatedAtUtc = DateTime.UtcNow;
                }

                var entity = await ctx.AvailableAdvisors.FirstOrDefaultAsync(x => x.Id == id);
                if (entity is null)
                    return 0;
                ctx.AvailableAdvisors.Remove(entity);
                await ctx.SaveChangesAsync();
                return 1;
            }
            catch (System.Exception ex) { throw Err(ex, nameof(DeleteAvailableAdvisor)); }
        }

        // ---------------- Buildings ----------------
        public async Task<List<BaronyBuildingDTO>> GetBuildings(int baronyId) =>
            await GetList(ctx => ctx.BaronyBuildings, baronyId, ToDTO, nameof(GetBuildings));

        public async Task<BaronyBuildingDTO> SaveBuilding(BaronyBuildingDTO dto)
        {
            try
            {
                using var ctx = await _db.CreateDbContextAsync();
                var e = dto.Id > 0 ? await ctx.BaronyBuildings.FirstOrDefaultAsync(x => x.Id == dto.Id) : null;
                if (e is null) { e = ToEntity(dto); ctx.BaronyBuildings.Add(e); }
                else { ApplyBuilding(e, dto); }
                await ctx.SaveChangesAsync();
                return ToDTO(e);
            }
            catch (System.Exception ex) { throw Err(ex, nameof(SaveBuilding)); }
        }

        public Task<int> DeleteBuilding(int id) => Delete(ctx => ctx.BaronyBuildings, id, nameof(DeleteBuilding));

        // ---------------- Social relations ----------------
        public async Task<List<SocialGroupRelationDTO>> GetSocialRelations(int baronyId) =>
            await GetList(ctx => ctx.SocialGroupRelations, baronyId, ToDTO, nameof(GetSocialRelations));

        public async Task<SocialGroupRelationDTO> SaveSocialRelation(SocialGroupRelationDTO dto)
        {
            try
            {
                using var ctx = await _db.CreateDbContextAsync();
                var e = dto.Id > 0 ? await ctx.SocialGroupRelations.FirstOrDefaultAsync(x => x.Id == dto.Id) : null;
                if (e is null) { e = ToEntity(dto); ctx.SocialGroupRelations.Add(e); }
                else { ApplySocial(e, dto); }
                await ctx.SaveChangesAsync();
                return ToDTO(e);
            }
            catch (System.Exception ex) { throw Err(ex, nameof(SaveSocialRelation)); }
        }

        public Task<int> DeleteSocialRelation(int id) => Delete(ctx => ctx.SocialGroupRelations, id, nameof(DeleteSocialRelation));

        // ---------------- Decrees ----------------
        public async Task<List<DecreeDTO>> GetDecrees(int baronyId) =>
            await GetList(ctx => ctx.Decrees, baronyId, ToDTO, nameof(GetDecrees));

        public async Task<DecreeDTO> SaveDecree(DecreeDTO dto)
        {
            try
            {
                using var ctx = await _db.CreateDbContextAsync();
                var e = dto.Id > 0 ? await ctx.Decrees.FirstOrDefaultAsync(x => x.Id == dto.Id) : null;
                if (e is null) { e = ToEntity(dto); ctx.Decrees.Add(e); }
                else
                {
                    // Permanent decrees keep catalog name/PPB; only IsActive (and free-text description) may change.
                    if (PermanentDecreesSeeder.IsPermanent(e.Name))
                    {
                        e.IsActive = dto.IsActive;
                        if (!string.IsNullOrWhiteSpace(dto.Description))
                            e.Description = dto.Description.Trim();
                    }
                    else
                    {
                        ApplyDecree(e, dto);
                    }
                }
                PermanentDecreesSeeder.ApplyMutualExclusivity(ctx, e);
                await ctx.SaveChangesAsync();
                return ToDTO(e);
            }
            catch (System.Exception ex) { throw Err(ex, nameof(SaveDecree)); }
        }

        public async Task<int> DeleteDecree(int id)
        {
            try
            {
                using var ctx = await _db.CreateDbContextAsync();
                var e = await ctx.Decrees.FirstOrDefaultAsync(x => x.Id == id);
                if (e is null)
                    return 0;
                if (PermanentDecreesSeeder.IsPermanent(e.Name))
                    throw new RepositoryErrorException(Loc.T("This decree is permanent and cannot be removed."));
                ctx.Decrees.Remove(e);
                await ctx.SaveChangesAsync();
                return 1;
            }
            catch (RepositoryErrorException) { throw; }
            catch (System.Exception ex) { throw Err(ex, nameof(DeleteDecree)); }
        }

        // ---------------- Events ----------------
        public async Task<List<BaronyEventDTO>> GetEvents(int baronyId) =>
            await GetList(ctx => ctx.BaronyEvents, baronyId, ToDTO, nameof(GetEvents));

        public async Task<BaronyEventDTO> SaveEvent(BaronyEventDTO dto)
        {
            try
            {
                using var ctx = await _db.CreateDbContextAsync();
                var e = dto.Id > 0 ? await ctx.BaronyEvents.FirstOrDefaultAsync(x => x.Id == dto.Id) : null;
                if (e is null) { e = ToEntity(dto); ctx.BaronyEvents.Add(e); }
                else { ApplyEvent(e, dto); }
                await ctx.SaveChangesAsync();
                return ToDTO(e);
            }
            catch (System.Exception ex) { throw Err(ex, nameof(SaveEvent)); }
        }

        public Task<int> DeleteEvent(int id) => Delete(ctx => ctx.BaronyEvents, id, nameof(DeleteEvent));

        // ---------------- Relations ----------------
        public async Task<List<BaronyRelationDTO>> GetRelations(int baronyId)
        {
            try
            {
                using var ctx = await _db.CreateDbContextAsync();
                var barony = await ctx.Baronies.AsNoTracking().FirstOrDefaultAsync(b => b.Id == baronyId);
                SeniorHousesSeeder.EnsureForBarony(ctx, baronyId);
                OrganizationsSeeder.EnsureForBarony(ctx, baronyId);
                DarkholdRelationLocalization.EnsurePolishForBarony(ctx, baronyId, barony?.Name);
                await ctx.SaveChangesAsync();

                var list = await ctx.BaronyRelations.AsNoTracking()
                    .Include(x => x.Modifiers)
                    .Where(x => x.BaronyId == baronyId)
                    .ToListAsync();
                return list.Select(ToDTO).ToList();
            }
            catch (System.Exception ex) { throw Err(ex, nameof(GetRelations)); }
        }

        public async Task<BaronyRelationDTO> SaveRelation(BaronyRelationDTO dto)
        {
            try
            {
                using var ctx = await _db.CreateDbContextAsync();
                BaronyRelation e;
                if (dto.Id > 0)
                {
                    e = await ctx.BaronyRelations.Include(x => x.Modifiers).FirstOrDefaultAsync(x => x.Id == dto.Id)
                        ?? throw new InvalidOperationException($"Relation {dto.Id} not found.");
                    ApplyRelation(e, dto);
                    ctx.BaronyRelationModifiers.RemoveRange(e.Modifiers);
                    e.Modifiers.Clear();
                }
                else
                {
                    e = new BaronyRelation();
                    ApplyRelation(e, dto);
                    ctx.BaronyRelations.Add(e);
                }

                foreach (var m in (dto.Modifiers ?? new()).OrderBy(x => x.SortOrder))
                {
                    e.Modifiers.Add(new BaronyRelationModifier
                    {
                        Description = m.Description ?? "",
                        Value = m.Value,
                        SortOrder = m.SortOrder,
                    });
                }

                await ctx.SaveChangesAsync();
                return ToDTO(e);
            }
            catch (System.Exception ex) when (ex is not InvalidOperationException)
            {
                throw Err(ex, nameof(SaveRelation));
            }
        }

        public Task<int> DeleteRelation(int id) =>
            Delete(ctx => ctx.BaronyRelations, id, nameof(DeleteRelation));

        public async Task SaveRelationNotes(int relationId, string? notes)
        {
            try
            {
                using var ctx = await _db.CreateDbContextAsync();
                var e = await ctx.BaronyRelations.FirstOrDefaultAsync(x => x.Id == relationId)
                    ?? throw new InvalidOperationException($"Relation {relationId} not found.");
                e.Notes = notes;
                await ctx.SaveChangesAsync();
            }
            catch (System.Exception ex) when (ex is not InvalidOperationException)
            {
                throw Err(ex, nameof(SaveRelationNotes));
            }
        }

        public async Task SaveRelationMarks(int relationId, IReadOnlyList<BaronyCharacterMarkDTO> marks)
        {
            try
            {
                using var ctx = await _db.CreateDbContextAsync();
                var e = await ctx.BaronyRelations.FirstOrDefaultAsync(x => x.Id == relationId)
                    ?? throw new InvalidOperationException($"Relation {relationId} not found.");

                e.MarksJson = SerMarks(marks);
                await ctx.SaveChangesAsync();
            }
            catch (System.Exception ex) when (ex is not InvalidOperationException)
            {
                throw Err(ex, nameof(SaveRelationMarks));
            }
        }

        // ---------------- Lord's Seat ----------------
        public async Task<BaronySeatDTO> EnsureSeat(int baronyId)
        {
            try
            {
                using var ctx = await _db.CreateDbContextAsync();
                var seat = await ctx.BaronySeats
                    .Include(s => s.Rooms)
                    .ThenInclude(r => r.Traits)
                    .Include(s => s.Tiles)
                    .FirstOrDefaultAsync(s => s.BaronyId == baronyId);

                if (seat is null)
                {
                    seat = new BaronySeat { BaronyId = baronyId, ActiveLevelsJson = "[0]" };
                    ctx.BaronySeats.Add(seat);
                    await ctx.SaveChangesAsync();
                }

                return ToDTO(seat, baronyId);
            }
            catch (System.Exception ex) { throw Err(ex, nameof(EnsureSeat)); }
        }

        public async Task<BaronySeatDTO?> GetSeat(int baronyId)
        {
            try
            {
                using var ctx = await _db.CreateDbContextAsync();
                return await LoadSeatDtoAsync(ctx, baronyId);
            }
            catch (System.Exception ex) { throw Err(ex, nameof(GetSeat)); }
        }

        public async Task<BaronySeatDTO> SaveSeat(BaronySeatDTO dto)
        {
            try
            {
                using var ctx = await _db.CreateDbContextAsync();
                var e = dto.Id > 0
                    ? await ctx.BaronySeats.FirstOrDefaultAsync(x => x.Id == dto.Id)
                    : await ctx.BaronySeats.FirstOrDefaultAsync(x => x.BaronyId == dto.BaronyId);

                if (e is null)
                {
                    e = new BaronySeat { BaronyId = dto.BaronyId };
                    ctx.BaronySeats.Add(e);
                }

                ApplySeat(e, dto);
                await ctx.SaveChangesAsync();
                dto.Id = e.Id;
                return await EnsureSeat(dto.BaronyId);
            }
            catch (System.Exception ex) { throw Err(ex, nameof(SaveSeat)); }
        }

        public async Task<SeatRoomDTO> SaveSeatRoom(SeatRoomDTO dto)
        {
            try
            {
                using var ctx = await _db.CreateDbContextAsync();
                SeatRoom e;
                if (dto.Id > 0)
                {
                    e = await ctx.SeatRooms.Include(x => x.Traits).FirstOrDefaultAsync(x => x.Id == dto.Id)
                        ?? throw new InvalidOperationException($"Seat room {dto.Id} not found.");
                    ApplySeatRoom(e, dto);
                    ctx.SeatRoomTraits.RemoveRange(e.Traits);
                    e.Traits.Clear();
                }
                else
                {
                    e = new SeatRoom();
                    ApplySeatRoom(e, dto);
                    ctx.SeatRooms.Add(e);
                }

                foreach (var t in (dto.Traits ?? new()).OrderBy(x => x.SortOrder))
                {
                    e.Traits.Add(new SeatRoomTrait
                    {
                        Kind = t.Kind ?? SeatRoomTraitKind.Advantage,
                        Text = t.Text ?? "",
                        SortOrder = t.SortOrder,
                    });
                }

                await ctx.SaveChangesAsync();
                var baronyId = await ctx.BaronySeats.AsNoTracking()
                    .Where(s => s.Id == e.SeatId)
                    .Select(s => s.BaronyId)
                    .FirstAsync();
                return ToDTO(e, baronyId);
            }
            catch (System.Exception ex) when (ex is not InvalidOperationException)
            {
                throw Err(ex, nameof(SaveSeatRoom));
            }
        }

        public Task<int> DeleteSeatRoom(int id) => Delete(ctx => ctx.SeatRooms, id, nameof(DeleteSeatRoom));

        public async Task SetSeatTile(int seatId, int level, int x, int y, string? kind)
        {
            try
            {
                level = SeatFloorLevel.Clamp(level);
                using var ctx = await _db.CreateDbContextAsync();
                var seat = await ctx.BaronySeats.AsNoTracking().FirstOrDefaultAsync(s => s.Id == seatId)
                    ?? throw new InvalidOperationException($"Seat {seatId} not found.");

                if (x < 0 || y < 0 || x >= seat.GridWidth || y >= seat.GridHeight)
                    throw new InvalidOperationException("Tile is outside the seat grid.");

                var existing = await ctx.SeatTiles
                    .FirstOrDefaultAsync(t => t.SeatId == seatId && t.Level == level && t.X == x && t.Y == y);

                if (string.IsNullOrWhiteSpace(kind))
                {
                    if (existing is not null)
                    {
                        ctx.SeatTiles.Remove(existing);
                        await ctx.SaveChangesAsync();
                    }

                    return;
                }

                if (!SeatTileKind.IsKnown(kind))
                    throw new InvalidOperationException($"Unknown tile kind '{kind}'.");

                var normalized = SeatTileKind.Normalize(kind);

                if (existing is null)
                {
                    ctx.SeatTiles.Add(new SeatTile
                    {
                        SeatId = seatId,
                        Level = level,
                        X = x,
                        Y = y,
                        Kind = normalized,
                    });
                }
                else
                {
                    existing.Kind = normalized;
                }

                await ctx.SaveChangesAsync();
            }
            catch (System.Exception ex) when (ex is not InvalidOperationException)
            {
                throw Err(ex, nameof(SetSeatTile));
            }
        }

        public async Task SaveSeatActiveLevels(int seatId, IReadOnlyList<int> levels)
        {
            try
            {
                using var ctx = await _db.CreateDbContextAsync();
                var seat = await ctx.BaronySeats.FirstOrDefaultAsync(s => s.Id == seatId)
                    ?? throw new InvalidOperationException($"Seat {seatId} not found.");

                var normalized = NormalizeActiveLevels(levels);
                var previous = ParseActiveLevels(seat.ActiveLevelsJson);
                var removed = previous.Where(l => !normalized.Contains(l)).ToList();

                if (removed.Count > 0)
                {
                    var roomsOnRemoved = await ctx.SeatRooms.AsNoTracking()
                        .AnyAsync(r => r.SeatId == seatId && removed.Contains(r.Level));
                    if (roomsOnRemoved)
                        throw new InvalidOperationException("Cannot remove a level that still has chambers.");

                    var tiles = await ctx.SeatTiles
                        .Where(t => t.SeatId == seatId && removed.Contains(t.Level))
                        .ToListAsync();
                    if (tiles.Count > 0)
                        ctx.SeatTiles.RemoveRange(tiles);
                }

                seat.ActiveLevelsJson = JsonSerializer.Serialize(normalized);
                await ctx.SaveChangesAsync();
            }
            catch (System.Exception ex) when (ex is not InvalidOperationException)
            {
                throw Err(ex, nameof(SaveSeatActiveLevels));
            }
        }

        public async Task SetSeatRoomPurpose(
            int roomId,
            int? purposeTemplateId,
            int? occupantAdvisorId = null,
            string? occupantCustom = null)
        {
            try
            {
                using var ctx = await _db.CreateDbContextAsync();
                var room = await ctx.SeatRooms.FirstOrDefaultAsync(x => x.Id == roomId)
                    ?? throw new InvalidOperationException($"Seat room {roomId} not found.");

                var baronyId = await ctx.BaronySeats.AsNoTracking()
                    .Where(s => s.Id == room.SeatId)
                    .Select(s => s.BaronyId)
                    .FirstAsync();

                if (purposeTemplateId is int pid)
                {
                    var template = await ctx.SeatPurposeTemplates.AsNoTracking()
                        .FirstOrDefaultAsync(t => t.Id == pid)
                        ?? throw new InvalidOperationException($"Purpose template {pid} not found.");
                    if (!template.IsUniversal && template.BaronyId != baronyId)
                        throw new InvalidOperationException("Purpose template is not available for this barony.");

                    var tileCount = Math.Max(0, room.GridW) * Math.Max(0, room.GridH);
                    if (!SeatRoomSizeCategory.MeetsMinimum(tileCount, template.MinSizeCategory))
                        throw new InvalidOperationException(
                            $"Room size ({SeatRoomSizeCategory.FromTileCount(tileCount)}) is below required {template.MinSizeCategory}.");
                }

                var custom = string.IsNullOrWhiteSpace(occupantCustom) ? null : occupantCustom.Trim();
                if (purposeTemplateId is null)
                {
                    room.PurposeTemplateId = null;
                    room.OccupantAdvisorId = null;
                    room.OccupantCustom = string.Empty;
                }
                else if (custom is not null)
                {
                    room.PurposeTemplateId = purposeTemplateId;
                    room.OccupantAdvisorId = null;
                    room.OccupantCustom = custom;
                }
                else if (occupantAdvisorId is int aid)
                {
                    var advisor = await ctx.Advisors.AsNoTracking()
                        .FirstOrDefaultAsync(a => a.Id == aid && a.BaronyId == baronyId)
                        ?? throw new InvalidOperationException("Selected occupant is not available for this barony.");
                    if (string.IsNullOrWhiteSpace(advisor.PersonName))
                        throw new InvalidOperationException("Selected occupant has no assigned person.");

                    room.PurposeTemplateId = purposeTemplateId;
                    room.OccupantAdvisorId = aid;
                    room.OccupantCustom = string.Empty;
                }
                else
                {
                    room.PurposeTemplateId = purposeTemplateId;
                    room.OccupantAdvisorId = null;
                    room.OccupantCustom = string.Empty;
                }

                await ctx.SaveChangesAsync();
            }
            catch (System.Exception ex) when (ex is not InvalidOperationException)
            {
                throw Err(ex, nameof(SetSeatRoomPurpose));
            }
        }

        public async Task<List<SeatPurposeTemplateDTO>> GetSeatPurposeTemplates(int baronyId)
        {
            try
            {
                using var ctx = await _db.CreateDbContextAsync();
                return await LoadPurposeTemplatesAsync(ctx, baronyId);
            }
            catch (System.Exception ex) { throw Err(ex, nameof(GetSeatPurposeTemplates)); }
        }

        public async Task<SeatPurposeTemplateDTO> SaveSeatPurposeTemplate(SeatPurposeTemplateDTO dto)
        {
            try
            {
                using var ctx = await _db.CreateDbContextAsync();
                var e = dto.Id > 0
                    ? await ctx.SeatPurposeTemplates.FirstOrDefaultAsync(x => x.Id == dto.Id)
                    : null;
                if (e is null)
                {
                    e = new SeatPurposeTemplate();
                    ctx.SeatPurposeTemplates.Add(e);
                }

                ApplyPurposeTemplate(e, dto);
                await ctx.SaveChangesAsync();
                return ToDTO(e);
            }
            catch (System.Exception ex) { throw Err(ex, nameof(SaveSeatPurposeTemplate)); }
        }

        public Task<int> DeleteSeatPurposeTemplate(int id) =>
            Delete(ctx => ctx.SeatPurposeTemplates, id, nameof(DeleteSeatPurposeTemplate));

        // ---------------- Community modifiers ----------------
        public async Task<List<CommunityModifierDTO>> GetCommunityModifiers(int baronyId) =>
            await GetList(ctx => ctx.CommunityModifiers, baronyId, ToDTO, nameof(GetCommunityModifiers));

        public async Task<CommunityModifierDTO> SaveCommunityModifier(CommunityModifierDTO dto)
        {
            try
            {
                using var ctx = await _db.CreateDbContextAsync();
                var e = dto.Id > 0 ? await ctx.CommunityModifiers.FirstOrDefaultAsync(x => x.Id == dto.Id) : null;
                if (e is null) { e = ToEntity(dto); ctx.CommunityModifiers.Add(e); }
                else { ApplyCommunity(e, dto); }
                await ctx.SaveChangesAsync();
                return ToDTO(e);
            }
            catch (System.Exception ex) { throw Err(ex, nameof(SaveCommunityModifier)); }
        }

        public Task<int> DeleteCommunityModifier(int id) => Delete(ctx => ctx.CommunityModifiers, id, nameof(DeleteCommunityModifier));

        // ---------------- Baron Card influence ----------------
        public async Task<List<BaronInfluenceModifierDTO>> GetBaronInfluenceModifiers(int baronyId) =>
            await GetList(ctx => ctx.BaronInfluenceModifiers, baronyId, ToDTO, nameof(GetBaronInfluenceModifiers));

        public async Task<BaronInfluenceModifierDTO> SaveBaronInfluenceModifier(BaronInfluenceModifierDTO dto)
        {
            try
            {
                using var ctx = await _db.CreateDbContextAsync();
                var e = dto.Id > 0 ? await ctx.BaronInfluenceModifiers.FirstOrDefaultAsync(x => x.Id == dto.Id) : null;
                if (e is null) { e = ToEntity(dto); ctx.BaronInfluenceModifiers.Add(e); }
                else { ApplyBaronInfluence(e, dto); }
                await ctx.SaveChangesAsync();
                return ToDTO(e);
            }
            catch (System.Exception ex) { throw Err(ex, nameof(SaveBaronInfluenceModifier)); }
        }

        public Task<int> DeleteBaronInfluenceModifier(int id) =>
            Delete(ctx => ctx.BaronInfluenceModifiers, id, nameof(DeleteBaronInfluenceModifier));

        // ---------------- Baron PHP sources ----------------
        public async Task<List<BaronPhpSourceDTO>> GetBaronPhpSources(int baronyId) =>
            await GetList(ctx => ctx.BaronPhpSources, baronyId, ToDTO, nameof(GetBaronPhpSources));

        public async Task<BaronPhpSourceDTO> SaveBaronPhpSource(BaronPhpSourceDTO dto)
        {
            try
            {
                using var ctx = await _db.CreateDbContextAsync();
                var e = dto.Id > 0 ? await ctx.BaronPhpSources.FirstOrDefaultAsync(x => x.Id == dto.Id) : null;
                if (e is null) { e = ToEntity(dto); ctx.BaronPhpSources.Add(e); }
                else { ApplyBaronPhp(e, dto); }
                await ctx.SaveChangesAsync();
                return ToDTO(e);
            }
            catch (System.Exception ex) { throw Err(ex, nameof(SaveBaronPhpSource)); }
        }

        public Task<int> DeleteBaronPhpSource(int id) =>
            Delete(ctx => ctx.BaronPhpSources, id, nameof(DeleteBaronPhpSource));

        // ---------------- Baron artifacts ----------------
        public async Task<List<BaronArtifactDTO>> GetBaronArtifacts(int baronyId) =>
            await GetList(ctx => ctx.BaronArtifacts, baronyId, ToDTO, nameof(GetBaronArtifacts));

        public async Task<BaronArtifactDTO> SaveBaronArtifact(BaronArtifactDTO dto)
        {
            try
            {
                using var ctx = await _db.CreateDbContextAsync();
                var e = dto.Id > 0 ? await ctx.BaronArtifacts.FirstOrDefaultAsync(x => x.Id == dto.Id) : null;
                if (e is null) { e = ToEntity(dto); ctx.BaronArtifacts.Add(e); }
                else { ApplyBaronArtifact(e, dto); }
                await ctx.SaveChangesAsync();
                return ToDTO(e);
            }
            catch (System.Exception ex) { throw Err(ex, nameof(SaveBaronArtifact)); }
        }

        public Task<int> DeleteBaronArtifact(int id) =>
            Delete(ctx => ctx.BaronArtifacts, id, nameof(DeleteBaronArtifact));

        // ---------------- Baron time (BT) ----------------
        public async Task EnsureBaronTimeDefaults(int baronyId)
        {
            try
            {
                using var ctx = await _db.CreateDbContextAsync();
                var hasManagement = await ctx.BaronTimeActions.AnyAsync(a =>
                    a.BaronyId == baronyId
                    && a.IsSystem
                    && a.Kind == BaronTimeActionKind.Management);

                if (hasManagement)
                    return;

                ctx.BaronTimeActions.Add(new BaronTimeAction
                {
                    BaronyId = baronyId,
                    Name = BaronTimeRules.ManagementActionName,
                    Kind = BaronTimeActionKind.Management,
                    CostJc = BaronTimeRules.RequiredManagementJc,
                    Description =
                        "Essential governance each turn. Spending fewer than "
                        + $"{BaronTimeRules.RequiredManagementJc} BT causes management penalties "
                        + "(stability, loyalty, income, etc.).",
                    SortOrder = 0,
                    IsSystem = true,
                });
                await ctx.SaveChangesAsync();
            }
            catch (System.Exception ex) { throw Err(ex, nameof(EnsureBaronTimeDefaults)); }
        }

        public async Task<List<BaronTimeModifierDTO>> GetBaronTimeModifiers(int baronyId) =>
            await GetList(ctx => ctx.BaronTimeModifiers, baronyId, ToDTO, nameof(GetBaronTimeModifiers));

        public async Task<BaronTimeModifierDTO> SaveBaronTimeModifier(BaronTimeModifierDTO dto)
        {
            try
            {
                using var ctx = await _db.CreateDbContextAsync();
                var e = dto.Id > 0 ? await ctx.BaronTimeModifiers.FirstOrDefaultAsync(x => x.Id == dto.Id) : null;
                if (e is null) { e = ToEntity(dto); ctx.BaronTimeModifiers.Add(e); }
                else { ApplyBaronTimeModifier(e, dto); }
                await ctx.SaveChangesAsync();
                return ToDTO(e);
            }
            catch (System.Exception ex) { throw Err(ex, nameof(SaveBaronTimeModifier)); }
        }

        public Task<int> DeleteBaronTimeModifier(int id) =>
            Delete(ctx => ctx.BaronTimeModifiers, id, nameof(DeleteBaronTimeModifier));

        public async Task<List<BaronTimeActionDTO>> GetBaronTimeActions(int baronyId) =>
            await GetList(ctx => ctx.BaronTimeActions, baronyId, ToDTO, nameof(GetBaronTimeActions));

        public async Task<BaronTimeActionDTO> SaveBaronTimeAction(BaronTimeActionDTO dto)
        {
            try
            {
                using var ctx = await _db.CreateDbContextAsync();
                var e = dto.Id > 0 ? await ctx.BaronTimeActions.FirstOrDefaultAsync(x => x.Id == dto.Id) : null;
                if (e is null) { e = ToEntity(dto); ctx.BaronTimeActions.Add(e); }
                else { ApplyBaronTimeAction(e, dto); }
                await ctx.SaveChangesAsync();
                return ToDTO(e);
            }
            catch (System.Exception ex) { throw Err(ex, nameof(SaveBaronTimeAction)); }
        }

        public async Task<int> DeleteBaronTimeAction(int id)
        {
            try
            {
                using var ctx = await _db.CreateDbContextAsync();
                var e = await ctx.BaronTimeActions.FirstOrDefaultAsync(x => x.Id == id);
                if (e is null) return 0;
                if (e.IsSystem)
                    throw new InvalidOperationException("Cannot delete the system Barony management action.");
                ctx.BaronTimeActions.Remove(e);
                await ctx.SaveChangesAsync();
                return id;
            }
            catch (System.Exception ex) { throw Err(ex, nameof(DeleteBaronTimeAction)); }
        }

        // ---------------- Baron letters (threads + messages) ----------------
        public async Task<List<BaronLetterThreadDTO>> GetLetterThreads(int baronyId)
        {
            try
            {
                using var ctx = await _db.CreateDbContextAsync();
                var threads = await ctx.BaronLetterThreads.AsNoTracking()
                    .Where(t => t.BaronyId == baronyId)
                    .OrderBy(t => t.CreatedAtUtc)
                    .ThenBy(t => t.Id)
                    .ToListAsync();

                var threadIds = threads.Select(t => t.Id).ToList();
                var messages = threadIds.Count == 0
                    ? new List<BaronLetterMessage>()
                    : await ctx.BaronLetterMessages.AsNoTracking()
                        .Where(m => threadIds.Contains(m.ThreadId))
                        .OrderBy(m => m.SortOrder)
                        .ThenBy(m => m.Id)
                        .ToListAsync();

                var byThread = messages.GroupBy(m => m.ThreadId)
                    .ToDictionary(g => g.Key, g => g.Select(ToDTO).ToList());

                return threads.Select(t =>
                {
                    var dto = ToDTO(t);
                    dto.Messages = byThread.TryGetValue(t.Id, out var msgs) ? msgs : new List<BaronLetterMessageDTO>();
                    return dto;
                }).ToList();
            }
            catch (System.Exception ex) { throw Err(ex, nameof(GetLetterThreads)); }
        }

        public async Task<BaronLetterThreadDTO> SaveLetterThread(BaronLetterThreadDTO dto)
        {
            try
            {
                using var ctx = await _db.CreateDbContextAsync();
                var now = DateTime.UtcNow;
                var e = dto.Id > 0
                    ? await ctx.BaronLetterThreads.FirstOrDefaultAsync(x => x.Id == dto.Id)
                    : null;

                if (e is null)
                {
                    e = ToEntity(dto);
                    e.CreatedAtUtc = now;
                    e.UpdatedAtUtc = now;
                    ctx.BaronLetterThreads.Add(e);
                }
                else
                {
                    ApplyLetterThread(e, dto);
                    e.UpdatedAtUtc = now;
                }

                await ctx.SaveChangesAsync();
                var result = ToDTO(e);
                result.Messages = dto.Messages ?? new List<BaronLetterMessageDTO>();
                return result;
            }
            catch (System.Exception ex) { throw Err(ex, nameof(SaveLetterThread)); }
        }

        public async Task<int> DeleteLetterThread(int id)
        {
            try
            {
                using var ctx = await _db.CreateDbContextAsync();
                var e = await ctx.BaronLetterThreads.FirstOrDefaultAsync(x => x.Id == id);
                if (e is null) return 0;
                ctx.BaronLetterThreads.Remove(e);
                await ctx.SaveChangesAsync();
                return id;
            }
            catch (System.Exception ex) { throw Err(ex, nameof(DeleteLetterThread)); }
        }

        public async Task<BaronLetterMessageDTO> SaveLetterMessage(BaronLetterMessageDTO dto)
        {
            try
            {
                using var ctx = await _db.CreateDbContextAsync();
                var now = DateTime.UtcNow;
                var e = dto.Id > 0
                    ? await ctx.BaronLetterMessages.FirstOrDefaultAsync(x => x.Id == dto.Id)
                    : null;

                if (e is null)
                {
                    e = ToEntity(dto);
                    e.CreatedAtUtc = now;
                    e.UpdatedAtUtc = now;
                    if (e.SortOrder <= 0)
                    {
                        var max = await ctx.BaronLetterMessages
                            .Where(m => m.ThreadId == e.ThreadId)
                            .Select(m => (int?)m.SortOrder)
                            .MaxAsync() ?? 0;
                        e.SortOrder = max + 1;
                    }
                    ctx.BaronLetterMessages.Add(e);
                }
                else
                {
                    // Never let a stale autosave demote a delivered letter back to Draft.
                    var incomingDraft = string.Equals(
                        dto.Status, BaronLetterStatus.Draft, StringComparison.OrdinalIgnoreCase);
                    var existingSent = string.Equals(
                        e.Status, BaronLetterStatus.Sent, StringComparison.OrdinalIgnoreCase);
                    if (incomingDraft && existingSent)
                        return ToDTO(e);

                    ApplyLetterMessage(e, dto);
                    e.UpdatedAtUtc = now;
                }

                // Draft autosave must not bump thread activity — that reordered the sidebar on click.
                var bumpThreadActivity = !string.Equals(
                    e.Status, BaronLetterStatus.Draft, StringComparison.OrdinalIgnoreCase);

                var thread = await ctx.BaronLetterThreads.FirstOrDefaultAsync(t => t.Id == e.ThreadId);
                if (thread is not null && bumpThreadActivity)
                    thread.UpdatedAtUtc = now;

                await ctx.SaveChangesAsync();
                return ToDTO(e);
            }
            catch (System.Exception ex) { throw Err(ex, nameof(SaveLetterMessage)); }
        }

        public Task<int> DeleteLetterMessage(int id) =>
            Delete(ctx => ctx.BaronLetterMessages, id, nameof(DeleteLetterMessage));

        public async Task MarkLetterThreadSeenByBaron(int threadId)
        {
            try
            {
                using var ctx = await _db.CreateDbContextAsync();
                var unread = await ctx.BaronLetterMessages
                    .Where(m => m.ThreadId == threadId
                        && !m.SeenByBaron
                        && m.IsInbound
                        && m.Status != BaronLetterStatus.Draft)
                    .ToListAsync();

                if (unread.Count == 0)
                    return;

                foreach (var m in unread)
                    m.SeenByBaron = true;

                await ctx.SaveChangesAsync();
            }
            catch (System.Exception ex) { throw Err(ex, nameof(MarkLetterThreadSeenByBaron)); }
        }

        public async Task MarkLetterThreadSeenByGm(int threadId)
        {
            try
            {
                using var ctx = await _db.CreateDbContextAsync();
                var unread = await ctx.BaronLetterMessages
                    .Where(m => m.ThreadId == threadId
                        && !m.SeenByGm
                        && !m.IsInbound
                        && m.Status != BaronLetterStatus.Draft)
                    .ToListAsync();

                if (unread.Count == 0)
                    return;

                foreach (var m in unread)
                    m.SeenByGm = true;

                await ctx.SaveChangesAsync();
            }
            catch (System.Exception ex) { throw Err(ex, nameof(MarkLetterThreadSeenByGm)); }
        }

        public async Task<BaronLetterInboxBadgeDTO> GetLetterInboxBadgeForBaron(int baronyId)
        {
            try
            {
                using var ctx = await _db.CreateDbContextAsync();
                var rows = await (
                    from m in ctx.BaronLetterMessages.AsNoTracking()
                    join t in ctx.BaronLetterThreads.AsNoTracking() on m.ThreadId equals t.Id
                    where t.BaronyId == baronyId
                        && !m.SeenByBaron
                        && m.IsInbound
                        && m.Status != BaronLetterStatus.Draft
                    orderby (m.SentAtUtc ?? m.UpdatedAtUtc) descending, m.Id descending
                    select new { m.ThreadId, t.BaronyId }
                ).ToListAsync();

                var latest = rows.FirstOrDefault();
                return new BaronLetterInboxBadgeDTO
                {
                    UnreadCount = rows.Count,
                    LatestThreadId = latest?.ThreadId,
                    BaronyId = latest?.BaronyId ?? baronyId,
                };
            }
            catch (System.Exception ex) { throw Err(ex, nameof(GetLetterInboxBadgeForBaron)); }
        }

        public async Task<BaronLetterInboxBadgeDTO> GetLetterInboxBadgeForGm()
        {
            try
            {
                using var ctx = await _db.CreateDbContextAsync();
                var rows = await (
                    from m in ctx.BaronLetterMessages.AsNoTracking()
                    join t in ctx.BaronLetterThreads.AsNoTracking() on m.ThreadId equals t.Id
                    join b in ctx.Baronies.AsNoTracking() on t.BaronyId equals b.Id
                    where !m.SeenByGm
                        && !m.IsInbound
                        && m.Status != BaronLetterStatus.Draft
                    orderby (m.SentAtUtc ?? m.UpdatedAtUtc) descending, m.Id descending
                    select new { m.ThreadId, t.BaronyId, b.CharacterId }
                ).ToListAsync();

                var latest = rows.FirstOrDefault();
                return new BaronLetterInboxBadgeDTO
                {
                    UnreadCount = rows.Count,
                    LatestThreadId = latest?.ThreadId,
                    BaronyId = latest?.BaronyId,
                    CharacterId = latest?.CharacterId,
                };
            }
            catch (System.Exception ex) { throw Err(ex, nameof(GetLetterInboxBadgeForGm)); }
        }

        // ---------------- Baron audiences ----------------
        public async Task<List<BaronAudienceDTO>> GetAudiences(int baronyId)
        {
            try
            {
                using var ctx = await _db.CreateDbContextAsync();
                return await LoadAudienceDtosAsync(ctx, baronyId);
            }
            catch (System.Exception ex) { throw Err(ex, nameof(GetAudiences)); }
        }

        public async Task<BaronAudienceDTO> SaveAudience(BaronAudienceDTO dto)
        {
            try
            {
                using var ctx = await _db.CreateDbContextAsync();
                var now = DateTime.UtcNow;
                BaronAudience e;
                if (dto.Id > 0)
                {
                    e = await ctx.BaronAudiences.FirstOrDefaultAsync(x => x.Id == dto.Id)
                        ?? throw new InvalidOperationException("Audience not found.");
                    ApplyAudience(e, dto);
                    e.UpdatedAtUtc = now;
                }
                else
                {
                    e = ToEntity(dto);
                    e.CreatedAtUtc = now;
                    e.UpdatedAtUtc = now;
                    if (e.TurnNumber <= 0)
                    {
                        var barony = await ctx.Baronies.AsNoTracking()
                            .FirstOrDefaultAsync(b => b.Id == dto.BaronyId);
                        e.TurnNumber = barony?.TurnNumber ?? 1;
                    }

                    if (string.IsNullOrWhiteSpace(e.Status))
                        e.Status = BaronAudienceStatus.Scheduled;

                    ctx.BaronAudiences.Add(e);
                }

                await ctx.SaveChangesAsync();
                var result = ToDTO(e);
                result.Exchanges = dto.Exchanges ?? new List<BaronAudienceExchangeDTO>();
                return result;
            }
            catch (System.Exception ex) when (ex is not InvalidOperationException)
            {
                throw Err(ex, nameof(SaveAudience));
            }
        }

        public async Task<int> DeleteAudience(int id)
        {
            try
            {
                using var ctx = await _db.CreateDbContextAsync();
                var e = await ctx.BaronAudiences.FirstOrDefaultAsync(x => x.Id == id);
                if (e is null) return 0;
                ctx.BaronAudiences.Remove(e);
                await ctx.SaveChangesAsync();
                return 1;
            }
            catch (System.Exception ex) { throw Err(ex, nameof(DeleteAudience)); }
        }

        public async Task<BaronAudienceExchangeDTO> SaveAudienceExchange(BaronAudienceExchangeDTO dto)
        {
            try
            {
                using var ctx = await _db.CreateDbContextAsync();
                var audience = await ctx.BaronAudiences.FirstOrDefaultAsync(a => a.Id == dto.AudienceId)
                    ?? throw new InvalidOperationException("Audience not found.");

                if (BaronAudienceStatus.IsClosed(audience.Status)
                    || string.Equals(audience.Status, BaronAudienceStatus.Deferred, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException("This audience is closed or deferred.");

                var now = DateTime.UtcNow;
                BaronAudienceExchange e;
                if (dto.Id > 0)
                {
                    e = await ctx.BaronAudienceExchanges.FirstOrDefaultAsync(x => x.Id == dto.Id)
                        ?? throw new InvalidOperationException("Exchange not found.");
                    ApplyAudienceExchange(e, dto);
                }
                else
                {
                    e = ToEntity(dto);
                    e.CreatedAtUtc = now;
                    if (e.SortOrder <= 0)
                    {
                        var max = await ctx.BaronAudienceExchanges
                            .Where(x => x.AudienceId == dto.AudienceId)
                            .Select(x => (int?)x.SortOrder)
                            .MaxAsync() ?? 0;
                        e.SortOrder = max + 1;
                    }

                    if (e.TurnNumber <= 0)
                    {
                        var barony = await ctx.Baronies.AsNoTracking()
                            .FirstOrDefaultAsync(b => b.Id == audience.BaronyId);
                        e.TurnNumber = barony?.TurnNumber ?? audience.TurnNumber;
                    }

                    ctx.BaronAudienceExchanges.Add(e);
                }

                if (string.Equals(audience.Status, BaronAudienceStatus.Scheduled, StringComparison.OrdinalIgnoreCase))
                    audience.Status = BaronAudienceStatus.InProgress;
                audience.UpdatedAtUtc = now;

                await ctx.SaveChangesAsync();
                return ToDTO(e);
            }
            catch (System.Exception ex) when (ex is not InvalidOperationException)
            {
                throw Err(ex, nameof(SaveAudienceExchange));
            }
        }

        public Task<int> DeleteAudienceExchange(int id) =>
            Delete(ctx => ctx.BaronAudienceExchanges, id, nameof(DeleteAudienceExchange));

        public async Task<BaronAudienceDTO> DeferAudience(int audienceId)
        {
            try
            {
                using var ctx = await _db.CreateDbContextAsync();
                var e = await ctx.BaronAudiences.FirstOrDefaultAsync(a => a.Id == audienceId)
                    ?? throw new InvalidOperationException("Audience not found.");
                if (BaronAudienceStatus.IsClosed(e.Status))
                    throw new InvalidOperationException("Closed audiences cannot be deferred.");

                e.Status = BaronAudienceStatus.Deferred;
                e.UpdatedAtUtc = DateTime.UtcNow;
                await ctx.SaveChangesAsync();
                return await LoadAudienceDtoAsync(ctx, e.Id);
            }
            catch (System.Exception ex) when (ex is not InvalidOperationException)
            {
                throw Err(ex, nameof(DeferAudience));
            }
        }

        public async Task<BaronAudienceDTO> ResolveAudience(int audienceId, string? gmSummary, string? outcomeNotes)
        {
            try
            {
                using var ctx = await _db.CreateDbContextAsync();
                var e = await ctx.BaronAudiences.FirstOrDefaultAsync(a => a.Id == audienceId)
                    ?? throw new InvalidOperationException("Audience not found.");

                e.Status = BaronAudienceStatus.Resolved;
                e.GmSummary = (gmSummary ?? "").Trim();
                e.OutcomeNotes = (outcomeNotes ?? "").Trim();
                e.ClosedAtUtc = DateTime.UtcNow;
                e.UpdatedAtUtc = e.ClosedAtUtc.Value;
                await ctx.SaveChangesAsync();
                return await LoadAudienceDtoAsync(ctx, e.Id);
            }
            catch (System.Exception ex) when (ex is not InvalidOperationException)
            {
                throw Err(ex, nameof(ResolveAudience));
            }
        }

        public async Task<BaronAudienceDTO> DismissAudience(int audienceId, string? gmSummary = null)
        {
            try
            {
                using var ctx = await _db.CreateDbContextAsync();
                var e = await ctx.BaronAudiences.FirstOrDefaultAsync(a => a.Id == audienceId)
                    ?? throw new InvalidOperationException("Audience not found.");

                e.Status = BaronAudienceStatus.Dismissed;
                if (!string.IsNullOrWhiteSpace(gmSummary))
                    e.GmSummary = gmSummary.Trim();
                e.ClosedAtUtc = DateTime.UtcNow;
                e.UpdatedAtUtc = e.ClosedAtUtc.Value;
                await ctx.SaveChangesAsync();
                return await LoadAudienceDtoAsync(ctx, e.Id);
            }
            catch (System.Exception ex) when (ex is not InvalidOperationException)
            {
                throw Err(ex, nameof(DismissAudience));
            }
        }

        private static async Task<BaronAudienceDTO> LoadAudienceDtoAsync(ApplicationDbContext ctx, int id)
        {
            var e = await ctx.BaronAudiences.AsNoTracking().FirstAsync(a => a.Id == id);
            var dto = ToDTO(e);
            dto.Exchanges = await ctx.BaronAudienceExchanges.AsNoTracking()
                .Where(x => x.AudienceId == id)
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.Id)
                .Select(x => ToDTO(x))
                .ToListAsync();
            return dto;
        }

        private static async Task<List<BaronAudienceDTO>> LoadAudienceDtosAsync(ApplicationDbContext ctx, int baronyId)
        {
            var audiences = await ctx.BaronAudiences.AsNoTracking()
                .Where(a => a.BaronyId == baronyId)
                .OrderByDescending(a => a.TurnNumber)
                .ThenByDescending(a => a.UpdatedAtUtc)
                .ThenByDescending(a => a.Id)
                .ToListAsync();
            if (audiences.Count == 0)
                return new List<BaronAudienceDTO>();

            var ids = audiences.Select(a => a.Id).ToList();
            var exchanges = await ctx.BaronAudienceExchanges.AsNoTracking()
                .Where(x => ids.Contains(x.AudienceId))
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.Id)
                .ToListAsync();
            var byAudience = exchanges
                .GroupBy(x => x.AudienceId)
                .ToDictionary(g => g.Key, g => g.Select(ToDTO).ToList());

            return audiences.Select(a =>
            {
                var dto = ToDTO(a);
                dto.Exchanges = byAudience.TryGetValue(a.Id, out var list)
                    ? list
                    : new List<BaronAudienceExchangeDTO>();
                return dto;
            }).ToList();
        }

        /// <summary>
        /// Deferred audiences become archived continuity sources; a new audience is opened
        /// on the new turn with the last exchange copied as the opening line.
        /// </summary>
        private static async Task AdvanceDeferredAudiencesAsync(
            ApplicationDbContext ctx,
            int baronyId,
            int newTurnNumber)
        {
            var deferred = await ctx.BaronAudiences
                .Where(a => a.BaronyId == baronyId
                    && a.Status == BaronAudienceStatus.Deferred
                    && a.Kind != BaronAudienceKind.Council)
                .ToListAsync();
            if (deferred.Count == 0)
                return;

            var continuedIds = await ctx.BaronAudiences
                .Where(a => a.BaronyId == baronyId && a.ContinuedFromAudienceId != null)
                .Select(a => a.ContinuedFromAudienceId!.Value)
                .Distinct()
                .ToListAsync();
            var continuedSet = continuedIds.ToHashSet();

            deferred = deferred.Where(a => !continuedSet.Contains(a.Id)).ToList();
            if (deferred.Count == 0)
                return;

            var ids = deferred.Select(a => a.Id).ToList();
            var exchanges = await ctx.BaronAudienceExchanges
                .Where(x => ids.Contains(x.AudienceId))
                .ToListAsync();
            var lastMap = exchanges
                .GroupBy(x => x.AudienceId)
                .ToDictionary(
                    g => g.Key,
                    g => g.OrderByDescending(x => x.SortOrder).ThenByDescending(x => x.Id).First());

            var now = DateTime.UtcNow;
            foreach (var old in deferred)
            {
                var neu = new BaronAudience
                {
                    BaronyId = baronyId,
                    Title = old.Title,
                    PetitionerName = old.PetitionerName,
                    PetitionerIcon = old.PetitionerIcon,
                    Kind = BaronAudienceKind.Normalize(old.Kind),
                    Status = BaronAudienceStatus.Scheduled,
                    TurnNumber = newTurnNumber,
                    ContinuedFromAudienceId = old.Id,
                    GmSummary = "",
                    OutcomeNotes = "",
                    AdditiveJson = old.AdditiveJson,
                    PercentJson = old.PercentJson,
                    Prestige = old.Prestige,
                    Honor = old.Honor,
                    Fear = old.Fear,
                    CreatedAtUtc = now,
                    UpdatedAtUtc = now,
                };
                ctx.BaronAudiences.Add(neu);
                await ctx.SaveChangesAsync();

                if (lastMap.TryGetValue(old.Id, out var last) && !string.IsNullOrWhiteSpace(last.Body))
                {
                    ctx.BaronAudienceExchanges.Add(new BaronAudienceExchange
                    {
                        AudienceId = neu.Id,
                        Body = last.Body,
                        IsFromPetitioner = last.IsFromPetitioner,
                        TurnNumber = newTurnNumber,
                        SortOrder = 1,
                        CreatedAtUtc = now,
                    });
                    neu.Status = BaronAudienceStatus.InProgress;
                    neu.UpdatedAtUtc = now;
                    await ctx.SaveChangesAsync();
                }
            }
        }

        /// <summary>
        /// Close open Council sessions for the ending turn. The GM adds each new turn's
        /// topics manually now — no blank "Council session" placeholder is auto-created.
        /// </summary>
        private static async Task AdvanceCouncilSessionsAsync(
            ApplicationDbContext ctx,
            int baronyId)
        {
            var now = DateTime.UtcNow;
            var open = await ctx.BaronAudiences
                .Where(a => a.BaronyId == baronyId
                    && a.Kind == BaronAudienceKind.Council
                    && (a.Status == BaronAudienceStatus.Scheduled
                        || a.Status == BaronAudienceStatus.InProgress))
                .ToListAsync();

            foreach (var session in open)
            {
                session.Status = BaronAudienceStatus.Resolved;
                session.ClosedAtUtc = now;
                session.UpdatedAtUtc = now;
                if (string.IsNullOrWhiteSpace(session.GmSummary))
                    session.GmSummary = "Closed at end of turn.";
            }

            await ctx.SaveChangesAsync();
        }

        // ---------------- Offices influence ----------------
        public async Task<List<AdvisorInfluenceModifierDTO>> GetAdvisorInfluenceModifiers(int baronyId)
        {
            try
            {
                using var ctx = await _db.CreateDbContextAsync();
                var advisorIds = await ctx.Advisors.AsNoTracking()
                    .Where(x => x.BaronyId == baronyId)
                    .Select(x => x.Id)
                    .ToListAsync();
                if (advisorIds.Count == 0)
                    return new List<AdvisorInfluenceModifierDTO>();

                var rows = await ctx.AdvisorInfluenceModifiers.AsNoTracking()
                    .Where(x => advisorIds.Contains(x.AdvisorId))
                    .ToListAsync();
                return rows.Select(ToDTO).ToList();
            }
            catch (System.Exception ex) { throw Err(ex, nameof(GetAdvisorInfluenceModifiers)); }
        }

        public async Task<AdvisorInfluenceModifierDTO> SaveAdvisorInfluenceModifier(AdvisorInfluenceModifierDTO dto)
        {
            try
            {
                using var ctx = await _db.CreateDbContextAsync();
                var e = dto.Id > 0 ? await ctx.AdvisorInfluenceModifiers.FirstOrDefaultAsync(x => x.Id == dto.Id) : null;
                if (e is null) { e = ToEntity(dto); ctx.AdvisorInfluenceModifiers.Add(e); }
                else { ApplyAdvisorInfluence(e, dto); }
                await ctx.SaveChangesAsync();
                return ToDTO(e);
            }
            catch (System.Exception ex) { throw Err(ex, nameof(SaveAdvisorInfluenceModifier)); }
        }

        public Task<int> DeleteAdvisorInfluenceModifier(int id) =>
            Delete(ctx => ctx.AdvisorInfluenceModifiers, id, nameof(DeleteAdvisorInfluenceModifier));

        // ---------------- Fiefs ----------------
        public async Task<List<FiefDTO>> GetFiefs(int baronyId) =>
            await GetList(ctx => ctx.Fiefs, baronyId, ToDTO, nameof(GetFiefs));

        public async Task<FiefDTO> SaveFief(FiefDTO dto)
        {
            try
            {
                using var ctx = await _db.CreateDbContextAsync();
                var e = dto.Id > 0 ? await ctx.Fiefs.FirstOrDefaultAsync(x => x.Id == dto.Id) : null;
                if (e is null) { e = ToEntity(dto); ctx.Fiefs.Add(e); }
                else { ApplyFief(e, dto); }
                await ctx.SaveChangesAsync();
                return ToDTO(e);
            }
            catch (System.Exception ex) { throw Err(ex, nameof(SaveFief)); }
        }

        public async Task<int> DeleteFief(int id)
        {
            try
            {
                using var ctx = await _db.CreateDbContextAsync();
                var e = await ctx.Fiefs.FirstOrDefaultAsync(x => x.Id == id);
                if (e is null)
                    return 0;

                var linked = await ctx.BaronyRelations
                    .Include(r => r.Modifiers)
                    .Where(r => r.FiefId == id)
                    .ToListAsync();
                if (linked.Count > 0)
                    ctx.BaronyRelations.RemoveRange(linked);

                ctx.Fiefs.Remove(e);
                await ctx.SaveChangesAsync();
                return id;
            }
            catch (System.Exception ex) { throw Err(ex, nameof(DeleteFief)); }
        }

        // ---------------- Tiles ----------------
        public async Task<List<TerrainTileDTO>> GetTiles(int baronyId) =>
            await GetList(ctx => ctx.TerrainTiles, baronyId, ToDTO, nameof(GetTiles));

        public async Task<List<TerrainTileDTO>> EnsureTerrainGrid(int baronyId)
        {
            try
            {
                using var ctx = await _db.CreateDbContextAsync();
                var barony = await ctx.Baronies.FirstOrDefaultAsync(b => b.Id == baronyId)
                    ?? throw new InvalidOperationException("Barony not found.");

                var width = TerrainMapGrid.ClampDimension(
                    barony.TerrainMapWidth > 0 ? barony.TerrainMapWidth : TerrainMapGrid.DefaultSize);
                var height = TerrainMapGrid.ClampDimension(
                    barony.TerrainMapHeight > 0 ? barony.TerrainMapHeight : TerrainMapGrid.DefaultSize);
                if (barony.TerrainMapWidth != width || barony.TerrainMapHeight != height)
                {
                    barony.TerrainMapWidth = width;
                    barony.TerrainMapHeight = height;
                }

                var existing = await ctx.TerrainTiles
                    .Where(x => x.BaronyId == baronyId)
                    .ToListAsync();
                var existingCoords = existing.Select(t => (t.X, t.Y)).ToHashSet();
                var added = false;

                for (var y = 0; y < height; y++)
                {
                    for (var x = 0; x < width; x++)
                    {
                        if (existingCoords.Contains((x, y)))
                            continue;

                        ctx.TerrainTiles.Add(new TerrainTile
                        {
                            BaronyId = baronyId,
                            X = x,
                            Y = y,
                            BaseType = TerrainBaseType.Plains,
                            FeaturesMask = 0,
                            Fertility = TerrainFertility.Unknown,
                        });
                        added = true;
                    }
                }

                if (added || ctx.ChangeTracker.HasChanges())
                    await ctx.SaveChangesAsync();

                return await ctx.TerrainTiles.AsNoTracking()
                    .Where(x => x.BaronyId == baronyId)
                    .Select(x => ToDTO(x))
                    .ToListAsync();
            }
            catch (System.Exception ex) { throw Err(ex, nameof(EnsureTerrainGrid)); }
        }

        /// <summary>
        /// Expand or shrink the terrain map from each edge.
        /// Positive deltas add tiles; negative deltas remove them.
        /// Existing tiles shift when growing/shrinking from left or top.
        /// </summary>
        public async Task<(int Width, int Height)> ResizeTerrainMap(
            int baronyId,
            int deltaLeft,
            int deltaRight,
            int deltaTop,
            int deltaBottom)
        {
            try
            {
                using var strategyCtx = await _db.CreateDbContextAsync();
                var strategy = strategyCtx.Database.CreateExecutionStrategy();
                return await strategy.ExecuteAsync(async () =>
                {
                    using var ctx = await _db.CreateDbContextAsync();
                    await using var tx = await ctx.Database.BeginTransactionAsync();

                    var barony = await ctx.Baronies.FirstOrDefaultAsync(b => b.Id == baronyId)
                        ?? throw new InvalidOperationException("Barony not found.");

                    var oldW = TerrainMapGrid.ClampDimension(
                        barony.TerrainMapWidth > 0 ? barony.TerrainMapWidth : TerrainMapGrid.DefaultSize);
                    var oldH = TerrainMapGrid.ClampDimension(
                        barony.TerrainMapHeight > 0 ? barony.TerrainMapHeight : TerrainMapGrid.DefaultSize);
                    var newW = oldW + deltaLeft + deltaRight;
                    var newH = oldH + deltaTop + deltaBottom;
                    if (!TerrainMapGrid.IsValidDimension(newW) || !TerrainMapGrid.IsValidDimension(newH))
                    {
                        throw new InvalidOperationException(
                            $"Map size must be between {TerrainMapGrid.MinSize} and {TerrainMapGrid.MaxSize} "
                            + $"(requested {newW}×{newH}).");
                    }

                    if (deltaLeft == 0 && deltaRight == 0 && deltaTop == 0 && deltaBottom == 0)
                    {
                        await tx.CommitAsync();
                        return (oldW, oldH);
                    }

                    var tiles = await ctx.TerrainTiles.Where(t => t.BaronyId == baronyId).ToListAsync();
                    var removeIds = tiles
                        .Where(t =>
                        {
                            var nx = t.X + deltaLeft;
                            var ny = t.Y + deltaTop;
                            return nx < 0 || ny < 0 || nx >= newW || ny >= newH;
                        })
                        .Select(t => t.Id)
                        .ToList();

                    if (removeIds.Count > 0)
                    {
                        var removeSet = removeIds.ToHashSet();
                        var improvements = await ctx.TerrainImprovements
                            .Where(i => i.BaronyId == baronyId && i.TileId != null && removeSet.Contains(i.TileId.Value))
                            .ToListAsync();
                        if (improvements.Count > 0)
                            ctx.TerrainImprovements.RemoveRange(improvements);

                        var projects = await ctx.BaronyProjects
                            .Where(p => p.BaronyId == baronyId && p.TileId != null && removeSet.Contains(p.TileId.Value))
                            .ToListAsync();
                        foreach (var p in projects)
                            p.TileId = null;

                        ctx.TerrainTiles.RemoveRange(tiles.Where(t => removeSet.Contains(t.Id)));
                        await ctx.SaveChangesAsync();
                        tiles = await ctx.TerrainTiles.Where(t => t.BaronyId == baronyId).ToListAsync();
                    }

                    // Two-phase shift avoids unique (BaronyId, X, Y) collisions mid-update.
                    const int shiftPark = 100_000;
                    if (deltaLeft != 0 || deltaTop != 0)
                    {
                        foreach (var t in tiles)
                        {
                            t.X += shiftPark;
                            t.Y += shiftPark;
                        }
                        await ctx.SaveChangesAsync();

                        foreach (var t in tiles)
                        {
                            t.X = t.X - shiftPark + deltaLeft;
                            t.Y = t.Y - shiftPark + deltaTop;
                        }
                        await ctx.SaveChangesAsync();
                    }

                    var existingCoords = tiles.Select(t => (t.X, t.Y)).ToHashSet();
                    for (var y = 0; y < newH; y++)
                    {
                        for (var x = 0; x < newW; x++)
                        {
                            if (existingCoords.Contains((x, y)))
                                continue;
                            ctx.TerrainTiles.Add(new TerrainTile
                            {
                                BaronyId = baronyId,
                                X = x,
                                Y = y,
                                BaseType = TerrainBaseType.Plains,
                                FeaturesMask = 0,
                                Fertility = TerrainFertility.Unknown,
                            });
                        }
                    }

                    barony.TerrainMapWidth = newW;
                    barony.TerrainMapHeight = newH;
                    await ctx.SaveChangesAsync();
                    await tx.CommitAsync();
                    return (newW, newH);
                });
            }
            catch (System.Exception ex) { throw Err(ex, nameof(ResizeTerrainMap)); }
        }

        public async Task<TerrainTileDTO> SaveTile(TerrainTileDTO dto)
        {
            try
            {
                using var ctx = await _db.CreateDbContextAsync();
                var e = dto.Id > 0
                    ? await ctx.TerrainTiles.FirstOrDefaultAsync(x => x.Id == dto.Id)
                    : await ctx.TerrainTiles.FirstOrDefaultAsync(x =>
                        x.BaronyId == dto.BaronyId && x.X == dto.X && x.Y == dto.Y);
                if (e is null) { e = ToEntity(dto); ctx.TerrainTiles.Add(e); }
                else { ApplyTile(e, dto); }
                await ctx.SaveChangesAsync();
                return ToDTO(e);
            }
            catch (System.Exception ex) { throw Err(ex, nameof(SaveTile)); }
        }

        public Task<int> DeleteTile(int id) => Delete(ctx => ctx.TerrainTiles, id, nameof(DeleteTile));

        // ---------------- Map domains ----------------
        public async Task<List<TerrainMapDomainDTO>> GetMapDomains(int baronyId)
        {
            try
            {
                using var ctx = await _db.CreateDbContextAsync();
                return await ctx.TerrainMapDomains.AsNoTracking()
                    .Where(x => x.BaronyId == baronyId)
                    .OrderBy(x => x.IsPrimary ? 0 : 1)
                    .ThenBy(x => x.SortOrder)
                    .ThenBy(x => x.Name)
                    .Select(x => ToDTO(x))
                    .ToListAsync();
            }
            catch (System.Exception ex) { throw Err(ex, nameof(GetMapDomains)); }
        }

        public async Task<TerrainMapDomainDTO> SaveMapDomain(TerrainMapDomainDTO dto)
        {
            try
            {
                using var ctx = await _db.CreateDbContextAsync();
                var e = dto.Id > 0
                    ? await ctx.TerrainMapDomains.FirstOrDefaultAsync(x => x.Id == dto.Id)
                    : null;
                if (e is null) { e = ToEntity(dto); ctx.TerrainMapDomains.Add(e); }
                else { ApplyMapDomain(e, dto); }
                await ctx.SaveChangesAsync();
                return ToDTO(e);
            }
            catch (System.Exception ex) { throw Err(ex, nameof(SaveMapDomain)); }
        }

        public Task<int> DeleteMapDomain(int id) =>
            Delete(ctx => ctx.TerrainMapDomains, id, nameof(DeleteMapDomain));

        // ---------------- Improvements ----------------
        public async Task<List<TerrainImprovementDTO>> GetImprovements(int baronyId)
        {
            try
            {
                using var ctx = await _db.CreateDbContextAsync();
                var entities = await ctx.TerrainImprovements.AsNoTracking()
                    .Where(x => x.BaronyId == baronyId)
                    .ToListAsync();
                var tiles = await ctx.TerrainTiles.AsNoTracking()
                    .Where(x => x.BaronyId == baronyId)
                    .ToListAsync();
                var taxRates = TownTaxRates.FromRelations(
                    (await ctx.SocialGroupRelations.AsNoTracking()
                        .Where(x => x.BaronyId == baronyId)
                        .ToListAsync())
                    .Select(r => (r.Group, r.TaxPercent)));
                var season = await ctx.Baronies.AsNoTracking()
                    .Where(b => b.Id == baronyId)
                    .Select(b => b.Season)
                    .FirstOrDefaultAsync();
                return entities.Select(e => ToImprovementDto(e, tiles, taxRates, season)).ToList();
            }
            catch (System.Exception ex) { throw Err(ex, nameof(GetImprovements)); }
        }

        public async Task<TerrainImprovementDTO> SaveImprovement(TerrainImprovementDTO dto)
        {
            try
            {
                using var ctx = await _db.CreateDbContextAsync();
                await ApplySettlementFormulasAsync(ctx, dto);
                var e = dto.Id > 0 ? await ctx.TerrainImprovements.FirstOrDefaultAsync(x => x.Id == dto.Id) : null;
                if (e is null) { e = ToEntity(dto); ctx.TerrainImprovements.Add(e); }
                else { ApplyImprovement(e, dto); }
                await ctx.SaveChangesAsync();

                var tile = dto.TileId is int tid
                    ? await ctx.TerrainTiles.AsNoTracking().FirstOrDefaultAsync(t => t.Id == tid)
                    : null;
                var taxRates = TownTaxRates.FromRelations(
                    (await ctx.SocialGroupRelations.AsNoTracking()
                        .Where(x => x.BaronyId == dto.BaronyId)
                        .ToListAsync())
                    .Select(r => (r.Group, r.TaxPercent)));
                var season = await ctx.Baronies.AsNoTracking()
                    .Where(b => b.Id == dto.BaronyId)
                    .Select(b => b.Season)
                    .FirstOrDefaultAsync();
                return ToImprovementDto(e, tile, taxRates, season);
            }
            catch (System.Exception ex) { throw Err(ex, nameof(SaveImprovement)); }
        }

        public Task<int> DeleteImprovement(int id) => Delete(ctx => ctx.TerrainImprovements, id, nameof(DeleteImprovement));

        // ---------------- Projects ----------------
        public async Task<List<BaronyProjectDTO>> GetProjects(int baronyId) =>
            await GetList(ctx => ctx.BaronyProjects, baronyId, ToDTO, nameof(GetProjects));

        public async Task<BaronyProjectDTO> SaveProject(BaronyProjectDTO dto)
        {
            try
            {
                using var ctx = await _db.CreateDbContextAsync();
                var e = dto.Id > 0 ? await ctx.BaronyProjects.FirstOrDefaultAsync(x => x.Id == dto.Id) : null;
                var previousStatus = e?.Status;
                if (e is null) { e = ToEntity(dto); ctx.BaronyProjects.Add(e); }
                else { ApplyProject(e, dto); }

                // MG manually marking a project Completed should still grant its OutputKind result.
                var becameCompleted =
                    string.Equals(dto.Status, ProjectStatus.Completed, StringComparison.OrdinalIgnoreCase)
                    && !string.Equals(previousStatus, ProjectStatus.Completed, StringComparison.OrdinalIgnoreCase);
                if (becameCompleted)
                {
                    var barony = await ctx.Baronies.FirstOrDefaultAsync(b => b.Id == e.BaronyId)
                        ?? throw new InvalidOperationException("Barony not found.");
                    var stocks = ResourceCatalog.Slice(De(barony.ResourceStocksJson));
                    stocks[Ppb.Food] = barony.FoodInGranaries;
                    stocks[Ppb.Treasury] = barony.TreasuryGold;
                    var templates = await ctx.BuildingTemplates.AsNoTracking().ToListAsync();
                    var appliedDto = ToDTO(e);
                    var finish = await ApplyCompletedProjectResultsAsync(
                        ctx, barony, appliedDto, templates.ToDictionary(t => t.Id), stocks, barony.TurnNumber);
                    if (finish.Applied)
                        appliedDto.Notes = MarkProjectResultsApplied(appliedDto.Notes);
                    if (appliedDto.UnitId is int restoredUnitId && restoredUnitId > 0)
                        e.UnitId = restoredUnitId;
                    e.Notes = appliedDto.Notes;
                    stocks = ResourceCatalog.Slice(stocks);
                    barony.ResourceStocksJson = Ser(stocks);
                    barony.FoodInGranaries = stocks[Ppb.Food];
                    barony.TreasuryGold = stocks[Ppb.Treasury];
                }

                await ctx.SaveChangesAsync();
                return ToDTO(e);
            }
            catch (System.Exception ex) { throw Err(ex, nameof(SaveProject)); }
        }

        public Task<int> DeleteProject(int id) => Delete(ctx => ctx.BaronyProjects, id, nameof(DeleteProject));

        // ---------------- Army units ----------------
        public async Task<List<BaronyUnitDTO>> GetUnits(int baronyId)
        {
            try
            {
                using var ctx = await _db.CreateDbContextAsync();
                var units = await ctx.BaronyUnits.AsNoTracking()
                    .Where(u => u.BaronyId == baronyId)
                    .OrderBy(u => u.Name)
                    .ThenBy(u => u.Id)
                    .ToListAsync();
                var projects = await ctx.BaronyProjects.AsNoTracking()
                    .Where(p => p.BaronyId == baronyId && p.UnitId != null
                        && p.Status != ProjectStatus.Completed
                        && p.Status != ProjectStatus.Cancelled
                        && p.Status != "Completed"
                        && p.Status != "Cancelled")
                    .ToListAsync();
                var byUnit = projects
                    .Where(p => p.UnitId is int)
                    .GroupBy(p => p.UnitId!.Value)
                    .ToDictionary(g => g.Key, g => g.OrderByDescending(p => p.Id).First());

                var captainIds = units
                    .Where(u => u.CaptainAvailableAdvisorId is int)
                    .Select(u => u.CaptainAvailableAdvisorId!.Value)
                    .Distinct()
                    .ToList();
                var captainNames = captainIds.Count == 0
                    ? new Dictionary<int, string>()
                    : await ctx.AvailableAdvisors.AsNoTracking()
                        .Where(a => a.BaronyId == baronyId && captainIds.Contains(a.Id))
                        .ToDictionaryAsync(a => a.Id, a => a.Name);

                return units.Select(u =>
                {
                    var dto = ToUnitDTO(u);
                    if (byUnit.TryGetValue(u.Id, out var proj))
                    {
                        dto.TrainingProjectId = proj.Id;
                        dto.TrainingTurnsRemaining = proj.TurnsRemaining;
                        dto.OpenProjectOutputKind = proj.OutputKind;
                        dto.OpenProjectStatus = proj.Status;
                    }
                    if (u.CaptainIsBaron)
                        dto.CaptainName = Loc.T("Baron");
                    else if (u.CaptainAvailableAdvisorId is int cid
                        && captainNames.TryGetValue(cid, out var cname))
                        dto.CaptainName = cname;
                    return dto;
                }).ToList();
            }
            catch (System.Exception ex) { throw Err(ex, nameof(GetUnits)); }
        }

        public async Task<BaronyUnitDTO> SaveUnit(BaronyUnitDTO dto)
        {
            try
            {
                using var ctx = await _db.CreateDbContextAsync();
                await EnforceCaptainAssignmentAsync(ctx, dto);
                await SyncUnitCommanderBonusesAsync(ctx, dto);

                var e = dto.Id > 0
                    ? await ctx.BaronyUnits.FirstOrDefaultAsync(x => x.Id == dto.Id)
                    : null;
                if (e is null)
                {
                    e = ToUnitEntity(dto);
                    e.CreatedAtUtc = DateTime.UtcNow;
                    e.UpdatedAtUtc = e.CreatedAtUtc;
                    ctx.BaronyUnits.Add(e);
                }
                else
                {
                    ApplyUnit(e, dto);
                    e.UpdatedAtUtc = DateTime.UtcNow;
                }
                await ctx.SaveChangesAsync();
                return ToUnitDTO(e);
            }
            catch (System.Exception ex) { throw Err(ex, nameof(SaveUnit)); }
        }

        public Task<int> DeleteUnit(int id) => Delete(ctx => ctx.BaronyUnits, id, nameof(DeleteUnit));

        public async Task<BaronyUnitDTO> ActivateUnit(int unitId)
        {
            try
            {
                if (unitId <= 0)
                    throw new InvalidOperationException("Unit is required.");

                using var ctx = await _db.CreateDbContextAsync();
                var unit = await ctx.BaronyUnits.FirstOrDefaultAsync(u => u.Id == unitId)
                    ?? throw new InvalidOperationException("Unit not found.");

                if (string.Equals(unit.Status, UnitStatus.Active, StringComparison.OrdinalIgnoreCase))
                    return ToUnitDTO(unit);

                if (string.Equals(unit.Status, UnitStatus.Disbanded, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException("Disbanded units cannot be activated.");

                unit.Status = UnitStatus.Active;
                // Graduation cap no longer applies — clear so UI/saves don't re-enforce it.
                unit.MaxBaseSkillAtGraduation = 0;
                unit.UpdatedAtUtc = DateTime.UtcNow;
                if (unit.CurrentHp <= 0)
                {
                    var preview = ToUnitDTO(unit);
                    unit.CurrentHp = ComputeUnitCombat(preview).MaxHp;
                }

                var openProjects = await ctx.BaronyProjects
                    .Where(p => p.BaronyId == unit.BaronyId
                                && p.UnitId == unit.Id
                                && p.Status != ProjectStatus.Completed
                                && p.Status != ProjectStatus.Cancelled)
                    .ToListAsync();
                foreach (var project in openProjects)
                {
                    project.Status = ProjectStatus.Completed;
                    project.TurnsRemaining = 0;
                    if (string.IsNullOrWhiteSpace(project.ResultDescription))
                        project.ResultDescription = $"Activates unit #{unit.Id} ({unit.Name}).";
                }

                await ctx.SaveChangesAsync();

                var dto = ToUnitDTO(unit);
                if (openProjects.Count > 0)
                {
                    dto.TrainingProjectId = openProjects[0].Id;
                    dto.TrainingTurnsRemaining = 0;
                }
                return dto;
            }
            catch (System.Exception ex) { throw Err(ex, nameof(ActivateUnit)); }
        }

        public async Task<StartUnitTrainingResult> StartUnitTraining(StartUnitTrainingRequest request)
        {
            try
            {
                if (request.BaronyId <= 0)
                    throw new InvalidOperationException("Barony is required.");
                if (string.IsNullOrWhiteSpace(request.Name))
                    throw new InvalidOperationException("Unit name is required.");

                var recruit = UnitRecruitSelectionCatalog.Find(request.RecruitSelectionKey)
                    ?? throw new InvalidOperationException("Unknown recruit selection.");
                var training = UnitTrainingTypeCatalog.Find(request.TrainingTypeKey)
                    ?? throw new InvalidOperationException("Unknown training type.");
                var weapon1 = UnitWeaponCatalog.Find(request.Weapon1Key);
                var weapon2 = UnitWeaponCatalog.Find(request.Weapon2Key);
                var armor = UnitArmorCatalog.Find(request.ArmorKey);
                var shield = UnitArmorCatalog.Find(request.ShieldKey);
                var mount = UnitMountCatalog.Find(request.MountKey);
                if (weapon1 is null)
                    throw new InvalidOperationException("Primary weapon is required.");

                var costs = UnitTrainingCostFormulas.Compute(
                    recruit, training, weapon1, weapon2, armor, shield, mount,
                    new UnitEquipmentPayModes(
                        UnitEquipmentAcquireMode.Normalize(request.Weapon1AcquireMode),
                        weapon2 is null
                            ? UnitEquipmentAcquireMode.Craft
                            : UnitEquipmentAcquireMode.Normalize(request.Weapon2AcquireMode),
                        armor is null
                            ? UnitEquipmentAcquireMode.Craft
                            : UnitEquipmentAcquireMode.Normalize(request.ArmorAcquireMode),
                        shield is null
                            ? UnitEquipmentAcquireMode.Craft
                            : UnitEquipmentAcquireMode.Normalize(request.ShieldAcquireMode),
                        mount is null
                            ? UnitEquipmentAcquireMode.Craft
                            : UnitEquipmentAcquireMode.Normalize(request.MountAcquireMode)),
                    request.AccelerateTurns);

                using var ctx = await _db.CreateDbContextAsync();
                var barony = await ctx.Baronies.AsNoTracking().FirstOrDefaultAsync(b => b.Id == request.BaronyId)
                    ?? throw new InvalidOperationException("Barony not found.");
                var gearQuality = UnitWeaponQuality.Normalize(barony.DefaultUnitWeaponQuality);
                var availability = await ResolveTradeGoodAvailabilityAsync(ctx, request.BaronyId, barony);

                var now = DateTime.UtcNow;
                var attr = recruit.AttributeScore;
                var unit = new BaronyUnit
                {
                    BaronyId = request.BaronyId,
                    Name = request.Name.Trim(),
                    Status = UnitStatus.Training,
                    TroopCount = UnitRules.DefaultTroopCount,
                    MaxTroopCount = UnitRules.DefaultTroopCount,
                    RecruitSelectionKey = recruit.Key,
                    TrainingTypeKey = training.Key,
                    RaceKey = string.IsNullOrWhiteSpace(request.RaceKey)
                        ? UnitRaceKey.Human
                        : request.RaceKey.Trim(),
                    Wage = costs.Wage,
                    UpkeepFood = UnitRules.DefaultUpkeepFood,
                    UpkeepDefense = 0, // Derived from gear market gold each turn.
                    Build = Math.Max(attr, request.Build ?? attr),
                    Agility = Math.Max(attr, request.Agility ?? attr),
                    Will = Math.Max(attr, request.Will ?? attr),
                    Perception = Math.Max(attr, request.Perception ?? attr),
                    SkillsJson = SerIntDict(UnitSkillDefaults.CreateSkillBase(request.SkillBase)),
                    SkillOtherJson = SerIntDict(SyncSkillOtherCopy(request.SkillOther, request.SkillOtherSources)),
                    SkillOtherSourcesJson = SerCombatOther(request.SkillOtherSources),
                    CombatOtherJson = SerCombatOther(request.CombatOther),
                    Weapon1Key = weapon1.Key,
                    Weapon2Key = weapon2?.Key,
                    ArmorKey = armor?.Key,
                    ShieldKey = shield?.Key,
                    MountKey = mount?.Key,
                    Weapon1Quality = gearQuality,
                    Weapon2Quality = UnitWeaponQuality.Normal,
                    DefenseSkillKey = UnitSkillKey.Dodges,
                    RemainingPd = Math.Clamp(request.RemainingPd ?? costs.Pd, 0, costs.Pd),
                    Discipline = Math.Clamp(
                        request.Discipline ?? costs.StartingDiscipline,
                        UnitRules.DisciplineMin,
                        UnitRules.DisciplineMax),
                    MaxBaseSkillAtGraduation = costs.MaxBaseSkill,
                    FreeAttributePoints = Math.Clamp(
                        request.FreeAttributePoints ?? costs.FreeAttributePoints,
                        0,
                        costs.FreeAttributePoints),
                    CreatedAtUtc = now,
                    UpdatedAtUtc = now,
                };
                ApplyCombatOtherFromMap(unit, request.CombatOther);
                ApplyAttrOtherFromMap(unit, request.AttrOtherSources);

                var unitDtoPreview = ToUnitDTO(unit);
                // Apply armor agility penalty to the attribute penalty column (Excel Ks).
                unit.AttrPenaltyAgility = (armor?.AgilityPenalty ?? 0) + (shield?.AgilityPenalty ?? 0);
                unitDtoPreview.AttrPenaltyAgility = unit.AttrPenaltyAgility;
                EnsureUnitGearMeetsRequirements(unitDtoPreview, weapon1, weapon2, armor, shield, mount, availability);
                var combatPreview = ComputeUnitCombat(unitDtoPreview);
                unit.DefenseSkillKey = unitDtoPreview.DefenseSkillKey;
                unit.CurrentHp = combatPreview.MaxHp;
                AddUnitLogEntry(
                    unit,
                    kind: "created",
                    text: $"Unit created: {unit.Name} ({recruit.Name}, {training.Name}).");

                ctx.BaronyUnits.Add(unit);
                await ctx.SaveChangesAsync();

                var goldCost = new PpbVector();
                goldCost[Ppb.Treasury] = costs.GoldTotal;
                goldCost[Ppb.Production] = costs.Production;

                var defenseCost = new PpbVector();
                defenseCost[Ppb.Defense] = costs.DefenseTotal;

                var hasGoldTrack = costs.GoldTotal > 0 || costs.Production > 0;
                var hasDefTrack = costs.DefenseTotal > 0;
                // Unit training is the Combined exception: gold+production and Defense can be required together.
                var allowed = (hasGoldTrack, hasDefTrack) switch
                {
                    (true, true) => ProjectAllowedCostModes.Combined,
                    (false, true) => ProjectAllowedCostModes.MaterialsOnly,
                    _ => ProjectAllowedCostModes.GoldProductionOnly,
                };
                var selectedMode = (hasGoldTrack, hasDefTrack) switch
                {
                    (true, true) => ProjectCostMode.Combined,
                    (false, true) => ProjectCostMode.Materials,
                    _ => ProjectCostMode.GoldProduction,
                };

                var recruitNote = recruit.EventEffect is not null
                    ? $" Adds event “{recruit.EventEffect.Name}” ({recruit.EventEffect.DurationTurns} turns)."
                    : string.IsNullOrWhiteSpace(recruit.Notes) ? string.Empty : $" {recruit.Notes}";

                var project = new BaronyProject
                {
                    BaronyId = request.BaronyId,
                    Name = $"Train: {unit.Name}",
                    Description = $"Unit training — {recruit.Name}, {training.Name}.{recruitNote}",
                    OutputKind = ProjectOutputKind.UnitTraining,
                    UnitId = unit.Id,
                    Status = ProjectStatus.ResourceAllocation,
                    TurnsRemaining = costs.Turns,
                    AllowedCostModes = allowed,
                    SelectedCostMode = selectedMode,
                    CostGoldProductionJson = Ser(ProjectCostCatalog.SliceGoldProduction(goldCost)),
                    CostMaterialsJson = Ser(ProjectCostCatalog.SliceMaterials(defenseCost)),
                    CostJson = Ser(MergeLegacyCost(goldCost, defenseCost)),
                    ResultJson = "{}",
                    ResultPercentJson = "{}",
                    AllocatedJson = "{}",
                    ResultDescription = $"Activates unit #{unit.Id} ({unit.Name}).",
                    Notes = BuildGearDefenseNote(request),
                };

                // Zero-cost express: still a draft project so MG/player can see it; auto-complete when turns=0 and no cost on fund.
                if (!hasGoldTrack && !hasDefTrack && costs.Turns <= 0)
                {
                    unit.Status = UnitStatus.Active;
                    unit.MaxBaseSkillAtGraduation = 0;
                    unit.UpdatedAtUtc = DateTime.UtcNow;
                    project.Status = ProjectStatus.Completed;
                    project.TurnsRemaining = 0;
                }

                if (recruit.EventEffect is { } fx)
                {
                    var startTurn = Math.Max(1, barony.TurnNumber);
                    var additive = new PpbVector();
                    additive[Ppb.Loyalty] = fx.Loyalty;
                    additive[Ppb.Stability] = fx.Stability;
                    ctx.BaronyEvents.Add(new BaronyEvent
                    {
                        BaronyId = request.BaronyId,
                        Name = fx.Name,
                        Description = fx.Description
                            ?? $"From recruiting {unit.Name} ({recruit.Name}).",
                        StartTurn = startTurn,
                        EndTurn = startTurn + Math.Max(1, fx.DurationTurns) - 1,
                        AdditiveJson = Ser(additive),
                        PercentJson = "{}",
                    });
                }

                ctx.BaronyProjects.Add(project);
                await ctx.SaveChangesAsync();

                return new StartUnitTrainingResult
                {
                    Unit = ToUnitDTO(unit),
                    Project = ToDTO(project),
                };
            }
            catch (System.Exception ex) when (ex is not InvalidOperationException)
            {
                throw Err(ex, nameof(StartUnitTraining));
            }
        }

        public async Task<StartUnitReinforceResult> StartUnitReinforce(StartUnitReinforceRequest request)
        {
            try
            {
                if (request.BaronyId <= 0)
                    throw new InvalidOperationException("Barony is required.");
                if (request.UnitId <= 0)
                    throw new InvalidOperationException("Unit is required.");

                using var ctx = await _db.CreateDbContextAsync();
                _ = await ctx.Baronies.AsNoTracking().FirstOrDefaultAsync(b => b.Id == request.BaronyId)
                    ?? throw new InvalidOperationException("Barony not found.");

                var unit = await ctx.BaronyUnits.FirstOrDefaultAsync(u =>
                        u.Id == request.UnitId && u.BaronyId == request.BaronyId)
                    ?? throw new InvalidOperationException("Unit not found.");

                if (!string.Equals(unit.Status, UnitStatus.Active, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException("Only Active units can be reinforced.");

                var full = unit.MaxTroopCount > 0 ? unit.MaxTroopCount : UnitRules.DefaultTroopCount;
                var missing = full - unit.TroopCount;
                if (missing <= 0)
                    throw new InvalidOperationException("Unit is already at full strength.");

                var openProject = await ctx.BaronyProjects.AsNoTracking()
                    .AnyAsync(p => p.BaronyId == request.BaronyId
                        && p.UnitId == unit.Id
                        && p.Status != ProjectStatus.Completed
                        && p.Status != ProjectStatus.Cancelled);
                if (openProject)
                    throw new InvalidOperationException("This unit already has an open project.");

                var n = request.TroopCount > 0
                    ? Math.Clamp(request.TroopCount, 1, missing)
                    : missing;

                var payModes = new UnitEquipmentPayModes(
                    UnitEquipmentAcquireMode.Normalize(request.Weapon1AcquireMode),
                    UnitEquipmentAcquireMode.Normalize(request.Weapon2AcquireMode),
                    UnitEquipmentAcquireMode.Normalize(request.ArmorAcquireMode),
                    UnitEquipmentAcquireMode.Normalize(request.ShieldAcquireMode),
                    UnitEquipmentAcquireMode.Normalize(request.MountAcquireMode));

                var costs = UnitReinforceCostFormulas.Compute(
                    unit.TroopCount,
                    UnitWeaponCatalog.Find(unit.Weapon1Key),
                    UnitWeaponCatalog.Find(unit.Weapon2Key),
                    UnitArmorCatalog.Find(unit.ArmorKey),
                    UnitArmorCatalog.Find(unit.ShieldKey),
                    UnitMountCatalog.Find(unit.MountKey),
                    payModes,
                    n,
                    full);

                if (costs.TroopCount <= 0)
                    throw new InvalidOperationException("Nothing to reinforce.");

                var goldCost = new PpbVector();
                goldCost[Ppb.Treasury] = costs.GoldTotal;
                goldCost[Ppb.Production] = costs.Production;

                var defenseCost = new PpbVector();
                defenseCost[Ppb.Defense] = costs.DefenseTotal;

                var hasGoldTrack = costs.GoldTotal > 0 || costs.Production > 0;
                var hasDefTrack = costs.DefenseTotal > 0;
                var allowed = (hasGoldTrack, hasDefTrack) switch
                {
                    (true, true) => ProjectAllowedCostModes.Combined,
                    (false, true) => ProjectAllowedCostModes.MaterialsOnly,
                    _ => ProjectAllowedCostModes.GoldProductionOnly,
                };
                var selectedMode = (hasGoldTrack, hasDefTrack) switch
                {
                    (true, true) => ProjectCostMode.Combined,
                    (false, true) => ProjectCostMode.Materials,
                    _ => ProjectCostMode.GoldProduction,
                };

                var recruit = UnitRecruitSelectionCatalog.SelectedVolunteers;
                var training = UnitTrainingTypeCatalog.Standard;

                var project = new BaronyProject
                {
                    BaronyId = request.BaronyId,
                    Name = $"Reinforce: {unit.Name}",
                    Description =
                        $"Replenish {costs.TroopCount} troops for {unit.Name} "
                        + $"({recruit.Name} + {training.Name}; gear at {UnitRules.ReinforceGearSalvagePercent}% salvage, scaled).",
                    OutputKind = ProjectOutputKind.UnitReinforce,
                    UnitId = unit.Id,
                    Status = ProjectStatus.ResourceAllocation,
                    TurnsRemaining = costs.Turns,
                    AllowedCostModes = allowed,
                    SelectedCostMode = selectedMode,
                    CostGoldProductionJson = Ser(ProjectCostCatalog.SliceGoldProduction(goldCost)),
                    CostMaterialsJson = Ser(ProjectCostCatalog.SliceMaterials(defenseCost)),
                    CostJson = Ser(MergeLegacyCost(goldCost, defenseCost)),
                    ResultJson = "{}",
                    ResultPercentJson = "{}",
                    AllocatedJson = "{}",
                    ResultDescription = $"Adds {costs.TroopCount} troops to unit #{unit.Id} ({unit.Name}).",
                    Notes = BuildReinforceNote(costs.TroopCount, payModes),
                };

                ctx.BaronyProjects.Add(project);
                await ctx.SaveChangesAsync();

                return new StartUnitReinforceResult
                {
                    Unit = ToUnitDTO(unit),
                    Project = ToDTO(project),
                };
            }
            catch (System.Exception ex) when (ex is not InvalidOperationException)
            {
                throw Err(ex, nameof(StartUnitReinforce));
            }
        }

        public async Task<StartUnitChangeEquipmentResult> StartUnitChangeEquipment(StartUnitChangeEquipmentRequest request)
        {
            try
            {
                if (request.BaronyId <= 0)
                    throw new InvalidOperationException("Barony is required.");
                if (request.UnitId <= 0)
                    throw new InvalidOperationException("Unit is required.");

                using var ctx = await _db.CreateDbContextAsync();
                var barony = await ctx.Baronies.AsNoTracking().FirstOrDefaultAsync(b => b.Id == request.BaronyId)
                    ?? throw new InvalidOperationException("Barony not found.");

                var unit = await ctx.BaronyUnits.FirstOrDefaultAsync(u =>
                        u.Id == request.UnitId && u.BaronyId == request.BaronyId)
                    ?? throw new InvalidOperationException("Unit not found.");

                if (!string.Equals(unit.Status, UnitStatus.Active, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException("Only Active units can change equipment.");

                var openProject = await ctx.BaronyProjects.AsNoTracking()
                    .AnyAsync(p => p.BaronyId == request.BaronyId
                        && p.UnitId == unit.Id
                        && p.Status != ProjectStatus.Completed
                        && p.Status != ProjectStatus.Cancelled);
                if (openProject)
                    throw new InvalidOperationException("This unit already has an open project.");

                var weapon1 = UnitWeaponCatalog.Find(request.Weapon1Key)
                    ?? throw new InvalidOperationException("Primary weapon is required.");
                var weapon2 = UnitWeaponCatalog.Find(request.Weapon2Key);
                var armor = UnitArmorCatalog.Find(request.ArmorKey);
                var shield = UnitArmorCatalog.Find(request.ShieldKey);
                var mount = UnitMountCatalog.Find(request.MountKey);

                var availability = await ResolveTradeGoodAvailabilityAsync(ctx, request.BaronyId, barony);
                var unitDto = ToUnitDTO(unit);
                EnsureUnitGearMeetsRequirements(unitDto, weapon1, weapon2, armor, shield, mount, availability);

                var sameLoadout =
                    string.Equals(unit.Weapon1Key, weapon1.Key, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(unit.Weapon2Key ?? "", weapon2?.Key ?? "", StringComparison.OrdinalIgnoreCase)
                    && string.Equals(unit.ArmorKey ?? "", armor?.Key ?? "", StringComparison.OrdinalIgnoreCase)
                    && string.Equals(unit.ShieldKey ?? "", shield?.Key ?? "", StringComparison.OrdinalIgnoreCase)
                    && string.Equals(unit.MountKey ?? "", mount?.Key ?? "", StringComparison.OrdinalIgnoreCase);
                if (sameLoadout)
                    throw new InvalidOperationException("Choose a different loadout than the unit already has.");

                var payModes = new UnitEquipmentPayModes(
                    UnitEquipmentAcquireMode.Normalize(request.Weapon1AcquireMode),
                    weapon2 is null
                        ? UnitEquipmentAcquireMode.Craft
                        : UnitEquipmentAcquireMode.Normalize(request.Weapon2AcquireMode),
                    armor is null
                        ? UnitEquipmentAcquireMode.Craft
                        : UnitEquipmentAcquireMode.Normalize(request.ArmorAcquireMode),
                    shield is null
                        ? UnitEquipmentAcquireMode.Craft
                        : UnitEquipmentAcquireMode.Normalize(request.ShieldAcquireMode),
                    mount is null
                        ? UnitEquipmentAcquireMode.Craft
                        : UnitEquipmentAcquireMode.Normalize(request.MountAcquireMode));

                var full = unit.MaxTroopCount > 0 ? unit.MaxTroopCount : UnitRules.DefaultTroopCount;
                var costs = UnitChangeEquipmentCostFormulas.Compute(
                    weapon1, weapon2, armor, shield, mount, payModes, unit.TroopCount, full);

                var goldCost = new PpbVector();
                goldCost[Ppb.Treasury] = costs.Gold;
                goldCost[Ppb.Production] = costs.Production;

                var defenseCost = new PpbVector();
                defenseCost[Ppb.Defense] = costs.Defense;

                var hasGoldTrack = costs.Gold > 0 || costs.Production > 0;
                var hasDefTrack = costs.Defense > 0;
                var allowed = (hasGoldTrack, hasDefTrack) switch
                {
                    (true, true) => ProjectAllowedCostModes.Combined,
                    (false, true) => ProjectAllowedCostModes.MaterialsOnly,
                    _ => ProjectAllowedCostModes.GoldProductionOnly,
                };
                var selectedMode = (hasGoldTrack, hasDefTrack) switch
                {
                    (true, true) => ProjectCostMode.Combined,
                    (false, true) => ProjectCostMode.Materials,
                    _ => ProjectCostMode.GoldProduction,
                };

                // Quality is MG barony policy / existing unit value — not chosen on re-equip.
                var quality = UnitWeaponQuality.Normalize(unit.Weapon1Quality);

                var gearSummary = string.Join(", ", new[]
                {
                    weapon1.Name,
                    weapon2?.Name,
                    armor?.Name,
                    shield?.Name,
                }.Where(s => !string.IsNullOrWhiteSpace(s)));

                var project = new BaronyProject
                {
                    BaronyId = request.BaronyId,
                    Name = $"Change equipment: {unit.Name}",
                    Description =
                        $"Re-equip {unit.Name} ({unit.TroopCount}/{full} troops): {gearSummary}. "
                        + "Gear paid Craft / Buy / Defense like the unit generator; cost scaled by troop count.",
                    // Same funding flow as Unit Reinforce (Resource allocation + Combined tracks).
                    OutputKind = ProjectOutputKind.UnitChangeEquipment,
                    UnitId = unit.Id,
                    Status = ProjectStatus.ResourceAllocation,
                    TurnsRemaining = costs.Turns,
                    AllowedCostModes = allowed,
                    SelectedCostMode = selectedMode,
                    CostGoldProductionJson = Ser(ProjectCostCatalog.SliceGoldProduction(goldCost)),
                    CostMaterialsJson = Ser(ProjectCostCatalog.SliceMaterials(defenseCost)),
                    CostJson = Ser(MergeLegacyCost(goldCost, defenseCost)),
                    ResultJson = "{}",
                    ResultPercentJson = "{}",
                    AllocatedJson = "{}",
                    ResultDescription = $"Changes equipment on unit #{unit.Id} ({unit.Name}).",
                    Notes = BuildChangeEquipmentNote(
                        weapon1.Key, weapon2?.Key, armor?.Key, shield?.Key, mount?.Key, quality, payModes),
                };

                ctx.BaronyProjects.Add(project);
                await ctx.SaveChangesAsync();

                return new StartUnitChangeEquipmentResult
                {
                    Unit = ToUnitDTO(unit),
                    Project = ToDTO(project),
                };
            }
            catch (System.Exception ex) when (ex is not InvalidOperationException)
            {
                throw Err(ex, nameof(StartUnitChangeEquipment));
            }
        }

        public async Task<BaronyProjectDTO> SetProjectCostMode(int projectId, string mode)
        {
            try
            {
                using var ctx = await _db.CreateDbContextAsync();
                var project = await ctx.BaronyProjects.FirstOrDefaultAsync(x => x.Id == projectId)
                    ?? throw new InvalidOperationException("Project not found.");

                var dto = ToDTO(project);
                if (dto.Status is ProjectStatus.Completed or ProjectStatus.Cancelled)
                    throw new InvalidOperationException("This project cannot change payment method.");
                if (!dto.GetSelectableCostModes().Contains(mode))
                    throw new InvalidOperationException("This payment method is not available for the project.");
                if (!dto.CanSwitchCostMode)
                    throw new InvalidOperationException("Payment method is locked after the first allocation.");

                dto.SelectedCostMode = mode;
                ApplyProject(project, dto);
                await ctx.SaveChangesAsync();
                return ToDTO(project);
            }
            catch (System.Exception ex) { throw Err(ex, nameof(SetProjectCostMode)); }
        }

        public async Task<BaronyProjectDTO> AllocateProjectResources(int projectId, PpbVector amounts)
        {
            try
            {
                using var ctx = await _db.CreateDbContextAsync();
                var project = await ctx.BaronyProjects.FirstOrDefaultAsync(x => x.Id == projectId)
                    ?? throw new InvalidOperationException("Project not found.");
                if (project.Status is ProjectStatus.Completed or ProjectStatus.Cancelled
                    || string.Equals(project.Status, "Completed", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(project.Status, "Cancelled", StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException("This project cannot accept resources.");

                var barony = await ctx.Baronies.FirstOrDefaultAsync(x => x.Id == project.BaronyId)
                    ?? throw new InvalidOperationException("Barony not found.");

                var dto = ToDTO(project);
                var stocks = ResourceCatalog.Slice(De(barony.ResourceStocksJson));
                stocks[Ppb.Food] = barony.FoodInGranaries;
                stocks[Ppb.Treasury] = barony.TreasuryGold;
                var toAdd = ResourceCatalog.Slice(amounts);
                var activeCost = dto.GetActiveCost();
                var activeKeys = dto.ActiveCostColumns.Select(x => x.Key).ToHashSet();
                var any = false;

                foreach (var info in ResourceCatalog.All)
                {
                    var add = toAdd[info.Key];
                    if (add <= 0m)
                        continue;

                    if (!activeKeys.Contains(info.Key))
                        throw new InvalidOperationException(
                            $"Cannot allocate {info.NameEn} while paying with {dto.EffectiveCostMode}.");

                    var remaining = Math.Max(0m, activeCost[info.Key] - dto.Allocated[info.Key]);
                    if (add > remaining)
                        throw new InvalidOperationException(
                            $"Cannot allocate more {info.NameEn} than remaining ({PpbFormat.Number(remaining)}).");
                    if (add > stocks[info.Key])
                        throw new InvalidOperationException(
                            $"Not enough {info.NameEn} in stock ({PpbFormat.Number(stocks[info.Key])} available).");

                    dto.Allocated[info.Key] += add;
                    stocks[info.Key] -= add;
                    any = true;
                }

                if (!any)
                    throw new InvalidOperationException("Enter at least one resource amount to allocate.");

                if (string.IsNullOrWhiteSpace(dto.SelectedCostMode))
                    dto.SelectedCostMode = dto.EffectiveCostMode;

                // Fully funded allocation phase → work may begin (turns tick from next Resolve).
                if (ProjectStatus.IsResourceAllocation(dto.Status) && !dto.HasRemainingCost)
                    dto.Status = ProjectStatus.InProgress;

                ApplyProject(project, dto);
                stocks = ResourceCatalog.Slice(stocks);
                barony.ResourceStocksJson = Ser(stocks);
                barony.FoodInGranaries = stocks[Ppb.Food];
                barony.TreasuryGold = stocks[Ppb.Treasury];

                await ctx.SaveChangesAsync();
                return ToDTO(project);
            }
            catch (System.Exception ex) when (ex is not InvalidOperationException)
            {
                throw Err(ex, nameof(AllocateProjectResources));
            }
        }

        public async Task<BaronyProjectDTO> ClearProjectAllocations(int projectId)
        {
            try
            {
                using var ctx = await _db.CreateDbContextAsync();
                var project = await ctx.BaronyProjects.FirstOrDefaultAsync(x => x.Id == projectId)
                    ?? throw new InvalidOperationException("Project not found.");

                var barony = await ctx.Baronies.FirstOrDefaultAsync(x => x.Id == project.BaronyId)
                    ?? throw new InvalidOperationException("Barony not found.");

                var dto = ToDTO(project);
                if (dto.Status != ProjectStatus.Draft
                    && !ProjectStatus.IsResourceAllocation(dto.Status))
                    throw new InvalidOperationException(
                        "Only draft or resource-allocation projects can have allocations cleared.");
                if (!dto.HasAnyAllocation)
                    throw new InvalidOperationException("This project has no allocated resources.");

                var stocks = ResourceCatalog.Slice(De(barony.ResourceStocksJson));
                foreach (var info in ResourceCatalog.All)
                {
                    var amount = dto.Allocated[info.Key];
                    if (amount <= 0m)
                        continue;
                    stocks[info.Key] += amount;
                    dto.Allocated[info.Key] = 0m;
                }

                dto.SelectedCostMode = null;
                ApplyProject(project, dto);
                stocks = ResourceCatalog.Slice(stocks);
                barony.ResourceStocksJson = Ser(stocks);
                barony.FoodInGranaries = stocks[Ppb.Food];
                barony.TreasuryGold = stocks[Ppb.Treasury];

                await ctx.SaveChangesAsync();
                return ToDTO(project);
            }
            catch (System.Exception ex) when (ex is not InvalidOperationException)
            {
                throw Err(ex, nameof(ClearProjectAllocations));
            }
        }

        // ---------------- Resource sources ----------------
        public async Task<List<BaronyResourceSourceDTO>> GetResourceSources(int baronyId) =>
            await GetList(ctx => ctx.BaronyResourceSources, baronyId, ToDTO, nameof(GetResourceSources));

        public async Task<BaronyResourceSourceDTO> SaveResourceSource(BaronyResourceSourceDTO dto)
        {
            try
            {
                using var ctx = await _db.CreateDbContextAsync();
                var e = dto.Id > 0 ? await ctx.BaronyResourceSources.FirstOrDefaultAsync(x => x.Id == dto.Id) : null;
                if (e is null) { e = ToEntity(dto); ctx.BaronyResourceSources.Add(e); }
                else { ApplyResourceSource(e, dto); }
                await ctx.SaveChangesAsync();
                return ToDTO(e);
            }
            catch (System.Exception ex) { throw Err(ex, nameof(SaveResourceSource)); }
        }

        public Task<int> DeleteResourceSource(int id) =>
            Delete(ctx => ctx.BaronyResourceSources, id, nameof(DeleteResourceSource));

        // ---------------- Baron purse sources ----------------
        public async Task<List<BaronPurseSourceDTO>> GetPurseSources(int baronyId) =>
            await GetList(ctx => ctx.BaronPurseSources, baronyId, ToDTO, nameof(GetPurseSources));

        public async Task<BaronPurseSourceDTO> SavePurseSource(BaronPurseSourceDTO dto)
        {
            try
            {
                using var ctx = await _db.CreateDbContextAsync();
                var e = dto.Id > 0 ? await ctx.BaronPurseSources.FirstOrDefaultAsync(x => x.Id == dto.Id) : null;
                if (e is null) { e = ToEntity(dto); ctx.BaronPurseSources.Add(e); }
                else { ApplyPurseSource(e, dto); }
                await ctx.SaveChangesAsync();
                return ToDTO(e);
            }
            catch (System.Exception ex) { throw Err(ex, nameof(SavePurseSource)); }
        }

        public Task<int> DeletePurseSource(int id) =>
            Delete(ctx => ctx.BaronPurseSources, id, nameof(DeletePurseSource));

        // ---------------- Building templates (global) ----------------
        public async Task<List<BuildingTemplateDTO>> GetBuildingTemplates()
        {
            try
            {
                using var ctx = await _db.CreateDbContextAsync();
                var list = await ctx.BuildingTemplates.AsNoTracking().OrderBy(x => x.Name).ToListAsync();
                return list.Select(ToDTO).ToList();
            }
            catch (System.Exception ex) { throw Err(ex, nameof(GetBuildingTemplates)); }
        }

        public async Task<BuildingTemplateDTO> SaveBuildingTemplate(BuildingTemplateDTO dto)
        {
            try
            {
                using var ctx = await _db.CreateDbContextAsync();
                var e = dto.Id > 0 ? await ctx.BuildingTemplates.FirstOrDefaultAsync(x => x.Id == dto.Id) : null;
                if (e is null) { e = ToEntity(dto); ctx.BuildingTemplates.Add(e); }
                else { ApplyTemplate(e, dto); }
                await ctx.SaveChangesAsync();
                return ToDTO(e);
            }
            catch (System.Exception ex) { throw Err(ex, nameof(SaveBuildingTemplate)); }
        }

        public Task<int> DeleteBuildingTemplate(int id) => Delete(ctx => ctx.BuildingTemplates, id, nameof(DeleteBuildingTemplate));

        // ---------------- Generic list/delete helpers ----------------
        private async Task<List<TDto>> GetList<TEntity, TDto>(
            Func<ApplicationDbContext, DbSet<TEntity>> set,
            int baronyId,
            Func<TEntity, TDto> map,
            string method) where TEntity : class
        {
            try
            {
                using var ctx = await _db.CreateDbContextAsync();
                var query = set(ctx).AsNoTracking();
                var filtered = query.Where(BuildBaronyPredicate<TEntity>(baronyId));
                var list = await filtered.ToListAsync();
                return list.Select(map).ToList();
            }
            catch (System.Exception ex) { throw Err(ex, method); }
        }

        private static System.Linq.Expressions.Expression<Func<TEntity, bool>> BuildBaronyPredicate<TEntity>(int baronyId)
        {
            var param = System.Linq.Expressions.Expression.Parameter(typeof(TEntity), "x");
            var prop = System.Linq.Expressions.Expression.Property(param, "BaronyId");
            var constant = System.Linq.Expressions.Expression.Constant(baronyId);
            var body = System.Linq.Expressions.Expression.Equal(prop, constant);
            return System.Linq.Expressions.Expression.Lambda<Func<TEntity, bool>>(body, param);
        }

        private async Task<int> Delete<TEntity>(
            Func<ApplicationDbContext, DbSet<TEntity>> set,
            int id,
            string method) where TEntity : class
        {
            try
            {
                using var ctx = await _db.CreateDbContextAsync();
                var dbSet = set(ctx);
                var e = await dbSet.FindAsync(id);
                if (e is null)
                    return 0;
                dbSet.Remove(e);
                return await ctx.SaveChangesAsync();
            }
            catch (System.Exception ex) { throw Err(ex, method); }
        }

        private static void SeedDefaultAdvisors(ApplicationDbContext ctx, int baronyId, string baronName)
        {
            var baronAdd = new PpbVector();
            baronAdd[Ppb.Loyalty] = 2;
            baronAdd[Ppb.Stability] = 1;

            var baronPct = new PpbVector();
            baronPct[Ppb.Culture] = 5;

            var chancellorAdd = new PpbVector();
            var chancellorPct = new PpbVector();
            var captainAdd = new PpbVector();
            var captainPct = new PpbVector();
            var stewardAdd = new PpbVector();
            var stewardPct = new PpbVector();

            var chancellorSkills = new PpbVector();
            var captainSkills = new PpbVector();
            var stewardSkills = new PpbVector();

            const string chancellorDescription = OfficeDescriptions.Chancellor;
            const string guardCaptainDescription = OfficeDescriptions.GuardCaptain;
            const string stewardDescription = OfficeDescriptions.Steward;

            var chancellorSignificant = AdvisorSignificantSkills.DefaultForOffice(OfficeType.Chancellor);
            var captainSignificant = AdvisorSignificantSkills.DefaultForOffice(OfficeType.GuardCaptain);
            var stewardSignificant = AdvisorSignificantSkills.DefaultForOffice(OfficeType.Steward);

            ctx.Advisors.AddRange(
                new Advisor
                {
                    BaronyId = baronyId,
                    OfficeType = OfficeType.Baron,
                    Title = "Baron",
                    PersonName = baronName,
                    IsBaron = true,
                    AdditiveJson = Ser(baronAdd),
                    PercentJson = Ser(baronPct),
                    FormulaText = "Extrapolated from baron character skills [TO BE COMPLETED]",
                },
                new Advisor
                {
                    BaronyId = baronyId,
                    OfficeType = OfficeType.Chancellor,
                    Title = "Chancellor",
                    PersonName = "",
                    SkillsJson = Ser(chancellorSkills),
                    SignificantSkillsJson = AdvisorSignificantSkills.Serialize(chancellorSignificant),
                    AdditiveJson = Ser(chancellorAdd),
                    PercentJson = Ser(chancellorPct),
                    UpkeepGold = 15,
                    Description = chancellorDescription,
                },
                new Advisor
                {
                    BaronyId = baronyId,
                    OfficeType = OfficeType.GuardCaptain,
                    Title = "Guard Captain",
                    PersonName = "",
                    SkillsJson = Ser(captainSkills),
                    SignificantSkillsJson = AdvisorSignificantSkills.Serialize(captainSignificant),
                    AdditiveJson = Ser(captainAdd),
                    PercentJson = Ser(captainPct),
                    UpkeepGold = 12,
                    Description = guardCaptainDescription,
                },
                new Advisor
                {
                    BaronyId = baronyId,
                    OfficeType = OfficeType.Steward,
                    Title = "Steward",
                    PersonName = "",
                    SkillsJson = Ser(stewardSkills),
                    SignificantSkillsJson = AdvisorSignificantSkills.Serialize(stewardSignificant),
                    AdditiveJson = Ser(stewardAdd),
                    PercentJson = Ser(stewardPct),
                    UpkeepGold = 12,
                    Description = stewardDescription,
                });
        }

        private static void SeedTerrainGrid(ApplicationDbContext ctx, int baronyId)
        {
            for (var y = 0; y < TerrainMapGrid.DefaultSize; y++)
            {
                for (var x = 0; x < TerrainMapGrid.DefaultSize; x++)
                {
                    ctx.TerrainTiles.Add(new TerrainTile
                    {
                        BaronyId = baronyId,
                        X = x,
                        Y = y,
                        BaseType = TerrainBaseType.Plains,
                        FeaturesMask = 0,
                        Fertility = TerrainFertility.Unknown,
                    });
                }
            }
        }

        private static void SeedPrimaryMapDomain(ApplicationDbContext ctx, int baronyId, string baronyName, string lordName)
        {
            ctx.TerrainMapDomains.Add(new TerrainMapDomain
            {
                BaronyId = baronyId,
                Name = baronyName,
                LordName = lordName,
                ColorHex = "#9c7a33",
                IsPrimary = true,
                SortOrder = 0,
            });
        }

        private static void SeedPrimaryFief(ApplicationDbContext ctx, int baronyId, string baronName)
        {
            ctx.Fiefs.Add(new Fief
            {
                BaronyId = baronyId,
                Name = $"Lord {baronName}",
                LiegeName = baronName,
                IsBaronDemesne = true,
                ColorHex = "#4d7ea8",
                BonusMultiplier = 1.0m,
            });
        }

        // ---------------- Mapping: Barony ----------------
        private static BaronyDTO ToDTO(Barony e)
        {
            var stocks = De(e.ResourceStocksJson);
            // Food / Gold scalars remain the source of truth for Budget / Domain Panel.
            stocks[Ppb.Food] = e.FoodInGranaries;
            stocks[Ppb.Treasury] = e.TreasuryGold;

            return new()
            {
                Id = e.Id,
                CharacterId = e.CharacterId,
                Name = e.Name,
                Size = e.Size,
                TerrainMapWidth = TerrainMapGrid.ClampDimension(
                    e.TerrainMapWidth > 0 ? e.TerrainMapWidth : TerrainMapGrid.DefaultSize),
                TerrainMapHeight = TerrainMapGrid.ClampDimension(
                    e.TerrainMapHeight > 0 ? e.TerrainMapHeight : TerrainMapGrid.DefaultSize),
                Year = e.Year,
                Month = e.Month,
                TurnNumber = e.TurnNumber,
                Season = e.Season,
                TreasuryGold = e.TreasuryGold,
                BaronPurseGold = e.BaronPurseGold,
                FoodInGranaries = e.FoodInGranaries,
                ResourceStocks = stocks,
                PreviousTurnStock = ResourceCatalog.Slice(De(e.PreviousTurnStockJson)),
                PreviousTurnIncome = ResourceCatalog.Slice(De(e.PreviousTurnIncomeJson)),
                Unrest = e.Unrest,
                ConjunctureDice = e.ConjunctureDice,
                ConjunctureModifier = e.ConjunctureModifier,
                DefaultUnitWeaponQuality = UnitWeaponQuality.Normalize(e.DefaultUnitWeaponQuality),
                LiegeTributePercent = e.LiegeTributePercent,
                VassalTributePercent = e.VassalTributePercent,
                Prestige = e.Prestige,
                Honor = e.Honor,
                Fear = e.Fear,
                BaseParameters = De(e.BaseParametersJson),
                Notes = e.Notes,
                TradeGoodMgOverrideKeys = TradeGoodAvailability.NormalizeOverrideKeys(ParseTradeGoodKeys(e.AvailableTradeGoodsJson))
                    .OrderBy(k => k, StringComparer.OrdinalIgnoreCase)
                    .ToList(),
                LuxuryGoodsAccessKey = LuxuryGoodsAccessCatalog.Find(e.LuxuryGoodsAccessKey).Key,
                TradeTreaties = ParseTradeTreaties(e.TradeTreatiesJson),
                PlayerTurnReady = e.PlayerTurnReady,
                CommanderSheet = DeserializeCourtSheet(e.CommanderSheetJson),
            };
        }

        private static Barony ToEntity(BaronyDTO d)
        {
            var e = new Barony();
            ApplyBarony(e, d);
            e.Id = d.Id;
            return e;
        }

        private static void ApplyBarony(Barony e, BaronyDTO d)
        {
            e.CharacterId = d.CharacterId;
            e.Name = d.Name;
            e.Size = d.Size;
            e.TerrainMapWidth = TerrainMapGrid.ClampDimension(
                d.TerrainMapWidth > 0 ? d.TerrainMapWidth : TerrainMapGrid.DefaultSize);
            e.TerrainMapHeight = TerrainMapGrid.ClampDimension(
                d.TerrainMapHeight > 0 ? d.TerrainMapHeight : TerrainMapGrid.DefaultSize);
            e.Year = d.Year;
            e.Month = d.Month;
            e.TurnNumber = d.TurnNumber;
            e.Season = d.Season;
            e.BaronPurseGold = d.BaronPurseGold;
            e.Unrest = UnrestPpbFormulas.Clamp(d.Unrest);
            e.ConjunctureDice = d.ConjunctureDice;
            e.ConjunctureModifier = d.ConjunctureModifier;
            e.DefaultUnitWeaponQuality = UnitWeaponQuality.Normalize(d.DefaultUnitWeaponQuality);
            e.LiegeTributePercent = FiefTributeFormulas.ClampPercent(d.LiegeTributePercent);
            e.VassalTributePercent = FiefTributeFormulas.ClampPercent(d.VassalTributePercent);
            e.Prestige = d.Prestige;
            e.Honor = d.Honor;
            e.Fear = d.Fear;
            e.BaseParametersJson = Ser(d.BaseParameters);
            e.Notes = d.Notes;
            e.PlayerTurnReady = d.PlayerTurnReady;

            var stocks = ResourceCatalog.Slice(d.ResourceStocks);
            // Keep Food/Gold scalars and vector in sync (Budget may update scalars only).
            stocks[Ppb.Food] = d.FoodInGranaries;
            stocks[Ppb.Treasury] = d.TreasuryGold;
            e.FoodInGranaries = stocks[Ppb.Food];
            e.TreasuryGold = stocks[Ppb.Treasury];
            e.ResourceStocksJson = Ser(stocks);
            e.PreviousTurnIncomeJson = Ser(ResourceCatalog.Slice(d.PreviousTurnIncome));
            e.PreviousTurnStockJson = Ser(ResourceCatalog.Slice(d.PreviousTurnStock));
        }

        // ---------------- Mapping: Advisor ----------------
        private static AdvisorDTO ToAdvisorDto(Advisor e, IReadOnlyDictionary<int, AvailableAdvisorDTO> personById)
        {
            var dto = ToDTO(e);
            if (e.AvailableAdvisorId is int aid && personById.TryGetValue(aid, out var person))
                dto.PersonDescription = person.Description;
            // Prefer catalog text for core offices (Description may still be a stale person bio until Ensure runs).
            var catalog = OfficeDescriptions.For(e.OfficeType);
            if (catalog is not null)
                dto.Description = catalog;
            return dto;
        }

        private static AdvisorDTO ToDTO(Advisor e) => new()
        {
            Id = e.Id, BaronyId = e.BaronyId, OfficeType = e.OfficeType, Title = e.Title,
            PersonName = e.PersonName, IsBaron = e.IsBaron, AvailableAdvisorId = e.AvailableAdvisorId,
            Skills = De(e.SkillsJson), SignificantSkills = AdvisorSignificantSkills.Deserialize(e.SignificantSkillsJson),
            Additive = De(e.AdditiveJson),
            Percent = De(e.PercentJson), FormulaText = e.FormulaText, Description = e.Description, UpkeepGold = e.UpkeepGold,
        };

        private static Advisor ToEntity(AdvisorDTO d) { var e = new Advisor(); ApplyAdvisor(e, d); e.Id = d.Id; return e; }

        private static void ApplyAdvisor(Advisor e, AdvisorDTO d)
        {
            e.BaronyId = d.BaronyId; e.OfficeType = d.OfficeType; e.Title = d.Title; e.PersonName = d.PersonName;
            e.IsBaron = d.IsBaron;
            e.AvailableAdvisorId = d.AvailableAdvisorId;
            e.SkillsJson = Ser(d.Skills);
            e.SignificantSkillsJson = AdvisorSignificantSkills.Serialize(d.SignificantSkills);
            e.AdditiveJson = Ser(d.Additive); e.PercentJson = Ser(d.Percent);
            e.FormulaText = d.FormulaText;
            // Never persist person bios as office Description for core offices.
            e.Description = OfficeDescriptions.For(d.OfficeType) ?? d.Description;
            e.UpkeepGold = d.UpkeepGold;
        }

        private static AvailableAdvisorDTO ToDTO(AvailableAdvisor e)
        {
            if (e.CharacterId is > 0)
            {
                // Commander progress lives in SheetJson; Domain Skills come from SkillsJson.
                var sheet = DeserializeCourtSheet(e.SheetJson);
                return new AvailableAdvisorDTO
                {
                    Id = e.Id,
                    BaronyId = e.BaronyId,
                    Name = e.Name,
                    Description = e.Description,
                    CharacterId = e.CharacterId,
                    Sheet = sheet,
                    Skills = De(e.SkillsJson),
                };
            }

            var courtSheet = CommanderCxFormulas.EnsureCourtSheetCx(DeserializeCourtSheet(e.SheetJson));
            return new AvailableAdvisorDTO
            {
                Id = e.Id,
                BaronyId = e.BaronyId,
                Name = e.Name,
                Description = e.Description,
                CharacterId = e.CharacterId,
                Sheet = courtSheet,
                Skills = CourtPpbFormulas.ComputeTotal(courtSheet),
            };
        }

        private static AvailableAdvisor ToEntity(AvailableAdvisorDTO d)
        {
            var e = new AvailableAdvisor();
            ApplyAvailableAdvisor(e, d);
            e.Id = d.Id;
            return e;
        }

        private static void ApplyAvailableAdvisor(AvailableAdvisor e, AvailableAdvisorDTO d)
        {
            e.BaronyId = d.BaronyId;
            e.CharacterId = d.CharacterId is > 0 ? d.CharacterId : null;
            e.Name = d.Name;
            e.Description = d.Description;

            if (e.CharacterId is > 0)
            {
                // Preserve commander tree in SheetJson; Domain Skills stay on SkillsJson.
                var existingSheet = DeserializeCourtSheet(e.SheetJson);
                if (d.Sheet is not null)
                {
                    existingSheet.CommanderXp = d.Sheet.CommanderXp;
                    existingSheet.UnlockedCommanderAbilities =
                        d.Sheet.UnlockedCommanderAbilities?.ToList() ?? new List<string>();
                }
                existingSheet.Normalize();
                e.SheetJson = JsonSerializer.Serialize(existingSheet, JsonOptions);
                if (d.Skills is not null && !d.Skills.IsEmpty)
                    e.SkillsJson = Ser(d.Skills);
                return;
            }

            var sheet = CommanderCxFormulas.EnsureCourtSheetCx(d.Sheet ?? CourtCharacterSheet.CreateDefault());
            sheet.Normalize();
            e.SheetJson = JsonSerializer.Serialize(sheet, JsonOptions);
            e.SkillsJson = Ser(CourtPpbFormulas.ComputeTotal(sheet));
        }

        private static string CharacterDisplayName(CharacterDTO character)
        {
            if (!string.IsNullOrWhiteSpace(character.NPCName))
                return character.NPCName.Trim();
            if (!string.IsNullOrWhiteSpace(character.UserName))
                return character.UserName.Trim();
            return $"Character #{character.Id}";
        }

        private async Task RefreshLinkedCourtiersAsync(ApplicationDbContext ctx, int baronyId)
        {
            var linked = await ctx.AvailableAdvisors
                .Where(a => a.BaronyId == baronyId && a.CharacterId != null)
                .ToListAsync();
            if (linked.Count == 0)
                return;

            foreach (var e in linked)
            {
                var character = await _characters.GetById(e.CharacterId!.Value, fullIncludes: true);
                if (character is null || character.Id <= 0)
                    continue;

                e.Name = CharacterDisplayName(character);
                var skills = CharacterBaronySkillPpb.FromCharacter(character);
                e.SkillsJson = Ser(skills);

                var sheet = CommanderCxFormulas.BuildCharacterCommanderSheet(
                    DeserializeCourtSheet(e.SheetJson), character);
                e.SheetJson = JsonSerializer.Serialize(sheet, JsonOptions);

                var offices = await ctx.Advisors
                    .Where(a => a.AvailableAdvisorId == e.Id)
                    .ToListAsync();
                foreach (var office in offices)
                {
                    office.SkillsJson = e.SkillsJson;
                    office.PersonName = e.Name;
                }
            }
        }

        private static async Task EnsureCourtSheetCommanderCxAsync(ApplicationDbContext ctx, int baronyId)
        {
            var courtOnly = await ctx.AvailableAdvisors
                .Where(a => a.BaronyId == baronyId && a.CharacterId == null)
                .ToListAsync();
            foreach (var e in courtOnly)
            {
                var sheet = DeserializeCourtSheet(e.SheetJson);
                if (!CommanderCxFormulas.EnsureMinimumPool(sheet, CommanderCxFormulas.BaseCxFromCourtSheet(sheet)))
                    continue;
                e.SheetJson = JsonSerializer.Serialize(sheet, JsonOptions);
            }
        }

        private static CourtCharacterSheet DeserializeCourtSheet(string? json)
        {
            if (string.IsNullOrWhiteSpace(json) || json is "{}" or "null")
                return CourtCharacterSheet.CreateDefault();
            try
            {
                var sheet = JsonSerializer.Deserialize<CourtCharacterSheet>(json, JsonOptions)
                            ?? CourtCharacterSheet.CreateDefault();
                sheet.Normalize();
                return sheet;
            }
            catch
            {
                return CourtCharacterSheet.CreateDefault();
            }
        }

        // ---------------- Mapping: Building ----------------
        private static BaronyBuildingDTO ToDTO(BaronyBuilding e) => new()
        {
            Id = e.Id, BaronyId = e.BaronyId, TemplateId = e.TemplateId, CoreKey = e.CoreKey,
            Name = e.Name, Kind = e.Kind,
            Additive = De(e.AdditiveJson), Percent = De(e.PercentJson), Description = e.Description,
        };

        private static BaronyBuilding ToEntity(BaronyBuildingDTO d) { var e = new BaronyBuilding(); ApplyBuilding(e, d); e.Id = d.Id; return e; }

        private static void ApplyBuilding(BaronyBuilding e, BaronyBuildingDTO d)
        {
            e.BaronyId = d.BaronyId; e.TemplateId = d.TemplateId; e.CoreKey = d.CoreKey;
            e.Name = d.Name; e.Kind = d.Kind;
            e.AdditiveJson = Ser(d.Additive); e.PercentJson = Ser(d.Percent); e.Description = d.Description;
        }

        // ---------------- Mapping: Social ----------------
        private static SocialGroupRelationDTO ToDTO(SocialGroupRelation e) => new()
        {
            Id = e.Id, BaronyId = e.BaronyId, Group = e.Group, RelationLevel = e.RelationLevel,
            InfluencePercent = e.InfluencePercent, IsActive = e.IsActive, TaxPercent = e.TaxPercent,
            Additive = De(e.AdditiveJson), Percent = De(e.PercentJson), FormulaText = e.FormulaText,
        };

        private static SocialGroupRelation ToEntity(SocialGroupRelationDTO d) { var e = new SocialGroupRelation(); ApplySocial(e, d); e.Id = d.Id; return e; }

        private static void ApplySocial(SocialGroupRelation e, SocialGroupRelationDTO d)
        {
            e.BaronyId = d.BaronyId;
            e.Group = SocialGroup.NormalizeKey(d.Group);
            e.RelationLevel = d.RelationLevel;
            e.InfluencePercent = d.InfluencePercent;
            e.IsActive = d.IsActive;
            e.TaxPercent = d.TaxPercent;
            var additive = SocialGroupPpbFormulas.ComputeAdditive(e.Group, e.RelationLevel);
            var percent = SocialGroupPpbFormulas.ComputePercent(e.Group, e.RelationLevel);
            e.AdditiveJson = Ser(additive);
            e.PercentJson = Ser(percent);
            e.FormulaText = d.FormulaText;
        }

        // ---------------- Mapping: Decree ----------------
        private static DecreeDTO ToDTO(Decree e) => new()
        {
            Id = e.Id, BaronyId = e.BaronyId, Name = e.Name, Additive = De(e.AdditiveJson),
            Percent = De(e.PercentJson), Description = e.Description, FormulaText = e.FormulaText,
            IsActive = e.IsActive,
        };

        private static Decree ToEntity(DecreeDTO d) { var e = new Decree(); ApplyDecree(e, d); e.Id = d.Id; return e; }

        private static void ApplyDecree(Decree e, DecreeDTO d)
        {
            e.BaronyId = d.BaronyId; e.Name = d.Name; e.AdditiveJson = Ser(d.Additive);
            e.PercentJson = Ser(d.Percent); e.Description = d.Description; e.FormulaText = d.FormulaText;
            e.IsActive = d.IsActive;
        }

        // ---------------- Mapping: Event ----------------
        private static BaronyEventDTO ToDTO(BaronyEvent e) => new()
        {
            Id = e.Id, BaronyId = e.BaronyId, Name = e.Name,
            StartTurn = e.StartTurn, EndTurn = e.EndTurn,
            Additive = De(e.AdditiveJson), Percent = De(e.PercentJson), Description = e.Description,
        };

        private static BaronyEvent ToEntity(BaronyEventDTO d) { var e = new BaronyEvent(); ApplyEvent(e, d); e.Id = d.Id; return e; }

        private static void ApplyEvent(BaronyEvent e, BaronyEventDTO d)
        {
            e.BaronyId = d.BaronyId; e.Name = d.Name;
            e.StartTurn = d.StartTurn; e.EndTurn = d.EndTurn;
            e.AdditiveJson = Ser(d.Additive); e.PercentJson = Ser(d.Percent); e.Description = d.Description;
        }

        // ---------------- Mapping: Relation ----------------
        private static BaronyRelationDTO ToDTO(BaronyRelation e) => new()
        {
            Id = e.Id,
            BaronyId = e.BaronyId,
            Category = e.Category ?? "",
            GroupName = e.GroupName ?? "",
            Name = e.Name ?? "",
            Title = e.Title ?? "",
            Age = e.Age,
            Description = e.Description ?? "",
            TroopCount = e.TroopCount,
            RelationDescription = e.RelationDescription ?? "",
            Notes = e.Notes,
            Marks = ParseRelationMarks(e.MarksJson),
            SortOrder = e.SortOrder,
            FiefId = e.FiefId,
            Modifiers = (e.Modifiers ?? new List<BaronyRelationModifier>())
                .OrderBy(m => m.SortOrder)
                .Select(m => new BaronyRelationModifierDTO
                {
                    Id = m.Id,
                    Description = m.Description ?? "",
                    Value = m.Value,
                    SortOrder = m.SortOrder,
                }).ToList(),
        };

        private static void ApplyRelation(BaronyRelation e, BaronyRelationDTO d)
        {
            e.BaronyId = d.BaronyId;
            e.Category = d.Category ?? "";
            e.GroupName = d.GroupName ?? "";
            e.Name = d.Name ?? "";
            e.Title = d.Title ?? "";
            e.Age = d.Age;
            e.Description = d.Description ?? "";
            e.TroopCount = d.TroopCount;
            e.RelationDescription = d.RelationDescription ?? "";
            e.Notes = d.Notes;
            e.MarksJson = SerMarks(d.Marks);
            e.SortOrder = d.SortOrder;
            e.FiefId = d.FiefId;
        }

        // ---------------- Mapping: Community ----------------
        private static CommunityModifierDTO ToDTO(CommunityModifier e) => new()
        {
            Id = e.Id, BaronyId = e.BaronyId, Source = e.Source, Additive = De(e.AdditiveJson),
            Percent = De(e.PercentJson), FormulaText = e.FormulaText,
        };

        private static CommunityModifier ToEntity(CommunityModifierDTO d) { var e = new CommunityModifier(); ApplyCommunity(e, d); e.Id = d.Id; return e; }

        private static void ApplyCommunity(CommunityModifier e, CommunityModifierDTO d)
        {
            e.BaronyId = d.BaronyId; e.Source = d.Source; e.AdditiveJson = Ser(d.Additive);
            e.PercentJson = Ser(d.Percent); e.FormulaText = d.FormulaText;
        }

        // ---------------- Mapping: Baron influence ----------------
        private static BaronInfluenceModifierDTO ToDTO(BaronInfluenceModifier e) => new()
        {
            Id = e.Id,
            BaronyId = e.BaronyId,
            Source = e.Source,
            Additive = De(e.AdditiveJson),
            FormulaText = e.FormulaText,
            Description = e.Description,
        };

        private static BaronInfluenceModifier ToEntity(BaronInfluenceModifierDTO d)
        {
            var e = new BaronInfluenceModifier();
            ApplyBaronInfluence(e, d);
            e.Id = d.Id;
            return e;
        }

        private static void ApplyBaronInfluence(BaronInfluenceModifier e, BaronInfluenceModifierDTO d)
        {
            e.BaronyId = d.BaronyId;
            e.Source = d.Source;
            e.AdditiveJson = Ser(d.Additive);
            e.FormulaText = d.FormulaText;
            e.Description = d.Description;
        }

        // ---------------- Mapping: Baron PHP source ----------------
        private static BaronPhpSourceDTO ToDTO(BaronPhpSource e) => new()
        {
            Id = e.Id,
            BaronyId = e.BaronyId,
            Source = e.Source,
            Description = e.Description,
            Prestige = e.Prestige,
            Honor = e.Honor,
            Fear = e.Fear,
        };

        private static BaronPhpSource ToEntity(BaronPhpSourceDTO d)
        {
            var e = new BaronPhpSource();
            ApplyBaronPhp(e, d);
            e.Id = d.Id;
            return e;
        }

        private static void ApplyBaronPhp(BaronPhpSource e, BaronPhpSourceDTO d)
        {
            e.BaronyId = d.BaronyId;
            e.Source = d.Source ?? "";
            e.Description = d.Description;
            e.Prestige = d.Prestige;
            e.Honor = d.Honor;
            e.Fear = d.Fear;
        }

        // ---------------- Mapping: Baron artifact ----------------
        private static BaronArtifactDTO ToDTO(BaronArtifact e) => new()
        {
            Id = e.Id,
            BaronyId = e.BaronyId,
            Name = e.Name ?? "",
            Kind = e.Kind ?? BaronArtifactKind.Other,
            Origin = e.Origin ?? BaronArtifactOrigin.Acquired,
            Prestige = e.Prestige,
            Honor = e.Honor,
            Fear = e.Fear,
            Additive = De(e.AdditiveJson),
            Percent = De(e.PercentJson),
            SeatRoomId = e.SeatRoomId,
            Description = e.Description,
            SortOrder = e.SortOrder,
        };

        private static BaronArtifact ToEntity(BaronArtifactDTO d)
        {
            var e = new BaronArtifact();
            ApplyBaronArtifact(e, d);
            e.Id = d.Id;
            return e;
        }

        private static void ApplyBaronArtifact(BaronArtifact e, BaronArtifactDTO d)
        {
            e.BaronyId = d.BaronyId;
            e.Name = d.Name ?? "";
            e.Kind = string.IsNullOrWhiteSpace(d.Kind) ? BaronArtifactKind.Other : d.Kind.Trim();
            e.Origin = string.IsNullOrWhiteSpace(d.Origin) ? BaronArtifactOrigin.Acquired : d.Origin.Trim();
            e.Prestige = d.Prestige;
            e.Honor = d.Honor;
            e.Fear = d.Fear;
            e.AdditiveJson = Ser(d.Additive);
            e.PercentJson = Ser(d.Percent);
            e.SeatRoomId = d.SeatRoomId;
            e.Description = d.Description;
            e.SortOrder = d.SortOrder;
        }

        // ---------------- Mapping: Baron time ----------------
        private static BaronTimeModifierDTO ToDTO(BaronTimeModifier e) => new()
        {
            Id = e.Id,
            BaronyId = e.BaronyId,
            Source = e.Source ?? "",
            Percent = e.Percent,
            Description = e.Description,
            SortOrder = e.SortOrder,
        };

        private static BaronTimeModifier ToEntity(BaronTimeModifierDTO d)
        {
            var e = new BaronTimeModifier();
            ApplyBaronTimeModifier(e, d);
            e.Id = d.Id;
            return e;
        }

        private static void ApplyBaronTimeModifier(BaronTimeModifier e, BaronTimeModifierDTO d)
        {
            e.BaronyId = d.BaronyId;
            e.Source = d.Source ?? "";
            e.Percent = d.Percent;
            e.Description = d.Description;
            e.SortOrder = d.SortOrder;
        }

        private static BaronTimeActionDTO ToDTO(BaronTimeAction e) => new()
        {
            Id = e.Id,
            BaronyId = e.BaronyId,
            Name = e.Name ?? "",
            Kind = e.Kind ?? BaronTimeActionKind.Other,
            CostJc = e.CostJc,
            Description = e.Description,
            SortOrder = e.SortOrder,
            IsSystem = e.IsSystem,
        };

        private static BaronTimeAction ToEntity(BaronTimeActionDTO d)
        {
            var e = new BaronTimeAction();
            ApplyBaronTimeAction(e, d);
            e.Id = d.Id;
            return e;
        }

        private static void ApplyBaronTimeAction(BaronTimeAction e, BaronTimeActionDTO d)
        {
            e.BaronyId = d.BaronyId;
            e.Name = d.Name ?? "";
            e.Kind = string.IsNullOrWhiteSpace(d.Kind) ? BaronTimeActionKind.Other : d.Kind.Trim();
            e.CostJc = d.CostJc;
            e.Description = d.Description;
            e.SortOrder = d.SortOrder;
            e.IsSystem = d.IsSystem;
        }

        // ---------------- Mapping: Baron letter threads / messages ----------------
        private static BaronLetterThreadDTO ToDTO(BaronLetterThread e) => new()
        {
            Id = e.Id,
            BaronyId = e.BaronyId,
            Title = e.Title ?? "",
            RelationId = e.RelationId,
            CorrespondentName = e.CorrespondentName ?? "",
            CorrespondentTitle = e.CorrespondentTitle,
            CorrespondentCategory = e.CorrespondentCategory,
            ReplyRegion = e.ReplyRegion ?? BaronLetterReplyRegion.EasternMarch,
            CreatedAtUtc = e.CreatedAtUtc,
            UpdatedAtUtc = e.UpdatedAtUtc,
        };

        private static BaronLetterMessageDTO ToDTO(BaronLetterMessage e) => new()
        {
            Id = e.Id,
            ThreadId = e.ThreadId,
            BodyHtml = e.BodyHtml ?? "",
            Status = e.Status ?? BaronLetterStatus.Draft,
            IsInbound = e.IsInbound,
            TurnNumber = e.TurnNumber,
            Year = e.Year,
            Month = e.Month,
            Day = e.Day,
            Season = e.Season ?? "Spring",
            SeenByBaron = e.SeenByBaron,
            SeenByGm = e.SeenByGm,
            SortOrder = e.SortOrder,
            CreatedAtUtc = e.CreatedAtUtc,
            UpdatedAtUtc = e.UpdatedAtUtc,
            SentAtUtc = e.SentAtUtc,
        };

        private static BaronLetterThread ToEntity(BaronLetterThreadDTO d)
        {
            var e = new BaronLetterThread();
            ApplyLetterThread(e, d);
            e.Id = d.Id;
            return e;
        }

        private static BaronLetterMessage ToEntity(BaronLetterMessageDTO d)
        {
            var e = new BaronLetterMessage();
            ApplyLetterMessage(e, d);
            e.Id = d.Id;
            return e;
        }

        private static void ApplyLetterThread(BaronLetterThread e, BaronLetterThreadDTO d)
        {
            e.BaronyId = d.BaronyId;
            e.Title = d.Title?.Trim() ?? "";
            e.RelationId = d.RelationId;
            e.CorrespondentName = d.CorrespondentName?.Trim() ?? "";
            e.CorrespondentTitle = string.IsNullOrWhiteSpace(d.CorrespondentTitle) ? null : d.CorrespondentTitle.Trim();
            e.CorrespondentCategory = d.CorrespondentCategory;
            e.ReplyRegion = string.IsNullOrWhiteSpace(d.ReplyRegion)
                ? BaronLetterReplyRegion.EasternMarch
                : d.ReplyRegion.Trim();
            if (d.CreatedAtUtc != default)
                e.CreatedAtUtc = d.CreatedAtUtc;
        }

        private static void ApplyLetterMessage(BaronLetterMessage e, BaronLetterMessageDTO d)
        {
            e.ThreadId = d.ThreadId;
            e.BodyHtml = d.BodyHtml ?? "";
            e.Status = string.IsNullOrWhiteSpace(d.Status) ? BaronLetterStatus.Draft : d.Status.Trim();
            e.IsInbound = d.IsInbound;
            e.TurnNumber = d.TurnNumber;
            e.Year = d.Year;
            e.Month = d.Month;
            e.Day = d.Day;
            e.Season = string.IsNullOrWhiteSpace(d.Season) ? "Spring" : d.Season;
            e.SeenByBaron = d.SeenByBaron;
            e.SeenByGm = d.SeenByGm;
            e.SortOrder = d.SortOrder;
            e.SentAtUtc = d.SentAtUtc;
            if (d.CreatedAtUtc != default)
                e.CreatedAtUtc = d.CreatedAtUtc;
        }

        // ---------------- Mapping: Baron audiences ----------------
        private static BaronAudienceDTO ToDTO(BaronAudience e) => new()
        {
            Id = e.Id,
            BaronyId = e.BaronyId,
            Title = e.Title ?? "",
            PetitionerName = e.PetitionerName ?? "",
            PetitionerIcon = e.PetitionerIcon,
            AssignedAdvisorName = e.AssignedAdvisorName,
            Kind = BaronAudienceKind.Normalize(e.Kind),
            Status = e.Status ?? BaronAudienceStatus.Scheduled,
            TurnNumber = e.TurnNumber,
            ContinuedFromAudienceId = e.ContinuedFromAudienceId,
            GmSummary = e.GmSummary ?? "",
            OutcomeNotes = e.OutcomeNotes ?? "",
            Additive = De(e.AdditiveJson),
            Percent = De(e.PercentJson),
            Prestige = e.Prestige,
            Honor = e.Honor,
            Fear = e.Fear,
            CreatedAtUtc = e.CreatedAtUtc,
            UpdatedAtUtc = e.UpdatedAtUtc,
            ClosedAtUtc = e.ClosedAtUtc,
        };

        private static BaronAudienceExchangeDTO ToDTO(BaronAudienceExchange e) => new()
        {
            Id = e.Id,
            AudienceId = e.AudienceId,
            Body = e.Body ?? "",
            IsFromPetitioner = e.IsFromPetitioner,
            SpeakerName = e.SpeakerName,
            IsResourceChange = e.IsResourceChange,
            Additive = De(e.AdditiveJson),
            Prestige = e.Prestige,
            Honor = e.Honor,
            Fear = e.Fear,
            TurnNumber = e.TurnNumber,
            SortOrder = e.SortOrder,
            CreatedAtUtc = e.CreatedAtUtc,
        };

        private static BaronAudience ToEntity(BaronAudienceDTO d)
        {
            var e = new BaronAudience();
            ApplyAudience(e, d);
            return e;
        }

        private static BaronAudienceExchange ToEntity(BaronAudienceExchangeDTO d)
        {
            var e = new BaronAudienceExchange();
            ApplyAudienceExchange(e, d);
            return e;
        }

        private static void ApplyAudience(BaronAudience e, BaronAudienceDTO d)
        {
            e.BaronyId = d.BaronyId;
            e.Title = (d.Title ?? "").Trim();
            e.PetitionerName = (d.PetitionerName ?? "").Trim();
            e.PetitionerIcon = string.IsNullOrWhiteSpace(d.PetitionerIcon) ? null : d.PetitionerIcon.Trim();
            e.AssignedAdvisorName = string.IsNullOrWhiteSpace(d.AssignedAdvisorName) ? null : d.AssignedAdvisorName.Trim();
            e.Kind = BaronAudienceKind.Normalize(d.Kind);
            e.Status = string.IsNullOrWhiteSpace(d.Status)
                ? BaronAudienceStatus.Scheduled
                : d.Status.Trim();
            e.TurnNumber = d.TurnNumber;
            e.ContinuedFromAudienceId = d.ContinuedFromAudienceId;
            e.GmSummary = d.GmSummary ?? "";
            e.OutcomeNotes = d.OutcomeNotes ?? "";
            e.AdditiveJson = Ser(d.Additive);
            e.PercentJson = Ser(d.Percent);
            e.Prestige = d.Prestige;
            e.Honor = d.Honor;
            e.Fear = d.Fear;
            e.ClosedAtUtc = d.ClosedAtUtc;
        }

        private static void ApplyAudienceExchange(BaronAudienceExchange e, BaronAudienceExchangeDTO d)
        {
            e.AudienceId = d.AudienceId;
            e.Body = d.Body ?? "";
            e.IsFromPetitioner = d.IsFromPetitioner;
            e.SpeakerName = string.IsNullOrWhiteSpace(d.SpeakerName) ? null : d.SpeakerName.Trim();
            e.IsResourceChange = d.IsResourceChange;
            e.AdditiveJson = Ser(d.Additive);
            e.Prestige = d.Prestige;
            e.Honor = d.Honor;
            e.Fear = d.Fear;
            e.TurnNumber = d.TurnNumber;
            e.SortOrder = d.SortOrder;
        }

        // ---------------- Mapping: Advisor influence ----------------
        private static AdvisorInfluenceModifierDTO ToDTO(AdvisorInfluenceModifier e) => new()
        {
            Id = e.Id,
            AdvisorId = e.AdvisorId,
            Source = e.Source,
            Additive = De(e.AdditiveJson),
            FormulaText = e.FormulaText,
            Description = e.Description,
            CostGold = e.CostGold,
        };

        private static AdvisorInfluenceModifier ToEntity(AdvisorInfluenceModifierDTO d)
        {
            var e = new AdvisorInfluenceModifier();
            ApplyAdvisorInfluence(e, d);
            e.Id = d.Id;
            return e;
        }

        private static void ApplyAdvisorInfluence(AdvisorInfluenceModifier e, AdvisorInfluenceModifierDTO d)
        {
            e.AdvisorId = d.AdvisorId;
            e.Source = d.Source;
            e.AdditiveJson = Ser(d.Additive);
            e.FormulaText = d.FormulaText;
            e.Description = d.Description;
            e.CostGold = d.CostGold;
        }

        // ---------------- Mapping: Fief ----------------
        private static FiefDTO ToDTO(Fief e) => new()
        {
            Id = e.Id, BaronyId = e.BaronyId, Name = e.Name, LiegeName = e.LiegeName,
            IsBaronDemesne = e.IsBaronDemesne, IsDomainDefault = e.IsDomainDefault,
            SeniorDomainId = e.SeniorDomainId,
            ColorHex = e.ColorHex, BonusMultiplier = e.BonusMultiplier,
        };

        private static Fief ToEntity(FiefDTO d) { var e = new Fief(); ApplyFief(e, d); e.Id = d.Id; return e; }

        private static void ApplyFief(Fief e, FiefDTO d)
        {
            e.BaronyId = d.BaronyId; e.Name = d.Name; e.LiegeName = d.LiegeName;
            e.IsBaronDemesne = d.IsBaronDemesne; e.IsDomainDefault = d.IsDomainDefault;
            e.SeniorDomainId = d.SeniorDomainId;
            e.ColorHex = string.IsNullOrWhiteSpace(d.ColorHex) ? "#4d7ea8" : d.ColorHex;
            e.BonusMultiplier = d.BonusMultiplier;
        }

        // ---------------- Mapping: Tile ----------------
        private static TerrainTileDTO ToDTO(TerrainTile e) => new()
        {
            Id = e.Id, BaronyId = e.BaronyId, MapId = e.MapId, X = e.X, Y = e.Y,
            BaseType = TerrainBaseType.CanonicalNameOrRaw(e.BaseType),
            FeaturesMask = e.FeaturesMask, Fertility = e.Fertility,
            Resource = string.IsNullOrWhiteSpace(e.Resource) ? e.Resource : TerrainResource.CanonicalNameOrRaw(e.Resource),
            FiefId = e.FiefId, MapDomainId = e.MapDomainId, Comment = e.Comment,
        };

        private static TerrainTile ToEntity(TerrainTileDTO d) { var e = new TerrainTile(); ApplyTile(e, d); e.Id = d.Id; return e; }

        private static void ApplyTile(TerrainTile e, TerrainTileDTO d)
        {
            e.BaronyId = d.BaronyId; e.MapId = d.MapId; e.X = d.X; e.Y = d.Y;
            e.BaseType = TerrainBaseType.CanonicalNameOrRaw(d.BaseType);
            e.FeaturesMask = d.FeaturesMask; e.Fertility = d.Fertility;
            e.Resource = string.IsNullOrWhiteSpace(d.Resource) ? d.Resource : TerrainResource.CanonicalNameOrRaw(d.Resource);
            e.FiefId = d.FiefId; e.MapDomainId = d.MapDomainId; e.Comment = d.Comment;
        }

        // ---------------- Mapping: Map domain ----------------
        private static TerrainMapDomainDTO ToDTO(TerrainMapDomain e) => new()
        {
            Id = e.Id, BaronyId = e.BaronyId, Name = e.Name, LordName = e.LordName, ColorHex = e.ColorHex,
            IsPrimary = e.IsPrimary, SortOrder = e.SortOrder,
        };

        private static TerrainMapDomain ToEntity(TerrainMapDomainDTO d)
        {
            var e = new TerrainMapDomain();
            ApplyMapDomain(e, d);
            e.Id = d.Id;
            return e;
        }

        private static void ApplyMapDomain(TerrainMapDomain e, TerrainMapDomainDTO d)
        {
            e.BaronyId = d.BaronyId; e.Name = d.Name; e.LordName = d.LordName; e.ColorHex = d.ColorHex;
            e.IsPrimary = d.IsPrimary; e.SortOrder = d.SortOrder;
        }

        // ---------------- Mapping: Improvement ----------------
        private static async Task ApplySettlementFormulasAsync(ApplicationDbContext ctx, TerrainImprovementDTO dto)
        {
            if (!IsVillage(dto.Name) && !IsTown(dto.Name))
                return;

            var fertility = TerrainFertility.Unknown;
            if (dto.TileId is int tid)
            {
                var tileFertility = await ctx.TerrainTiles.AsNoTracking()
                    .Where(t => t.Id == tid)
                    .Select(t => (int?)t.Fertility)
                    .FirstOrDefaultAsync();
                if (tileFertility is int f)
                    fertility = f;
            }

            var taxRates = TownTaxRates.FromRelations(
                (await ctx.SocialGroupRelations.AsNoTracking()
                    .Where(x => x.BaronyId == dto.BaronyId)
                    .ToListAsync())
                .Select(r => (r.Group, r.TaxPercent)));

            var season = await ctx.Baronies.AsNoTracking()
                .Where(b => b.Id == dto.BaronyId)
                .Select(b => b.Season)
                .FirstOrDefaultAsync();

            ApplySettlementFormulas(dto, fertility, taxRates, season);
        }

        private static void ApplySettlementFormulas(
            TerrainImprovementDTO dto,
            int fertility,
            TownTaxRates? taxRates = null,
            string? season = null)
        {
            var taxes = taxRates ?? TownTaxRates.Defaults;
            if (IsVillage(dto.Name))
            {
                dto.Additive = VillagePpbFormulas.Compute(dto.Population, fertility, dto.HasPalisade, taxes, season);
                dto.Percent = new PpbVector();
                dto.FormulaText = VillagePpbFormulas.CatalogDescription;
            }
            else if (IsTown(dto.Name))
            {
                dto.HasPalisade = false;
                dto.Additive = TownPpbFormulas.Compute(dto.Population, taxes);
                dto.Percent = new PpbVector();
                dto.FormulaText = TownPpbFormulas.CatalogDescription;
            }
        }

        private static bool IsVillage(string? name) =>
            string.Equals(name, MapImprovement.Village, StringComparison.OrdinalIgnoreCase);

        private static bool IsTown(string? name) =>
            string.Equals(name, MapImprovement.Town, StringComparison.OrdinalIgnoreCase);

        private static TerrainImprovementDTO ToImprovementDto(
            TerrainImprovement e,
            IEnumerable<TerrainTile> tiles,
            TownTaxRates taxRates,
            string? season = null)
        {
            TerrainTile? tile = null;
            if (e.TileId is int tid)
                tile = tiles.FirstOrDefault(t => t.Id == tid);
            return ToImprovementDto(e, tile, taxRates, season);
        }

        private static TerrainImprovementDTO ToImprovementDto(
            TerrainImprovement e,
            TerrainTile? tile,
            TownTaxRates taxRates,
            string? season = null)
        {
            var dto = ToDTO(e);
            ApplySettlementFormulas(dto, tile?.Fertility ?? TerrainFertility.Unknown, taxRates, season);
            return dto;
        }

        private static TerrainImprovementDTO ToDTO(TerrainImprovement e) => new()
        {
            Id = e.Id, BaronyId = e.BaronyId, TileId = e.TileId, TemplateId = e.TemplateId, Name = e.Name,
            Additive = De(e.AdditiveJson), Percent = De(e.PercentJson), Description = e.Description, FormulaText = e.FormulaText,
            IsActive = e.IsActive, InactiveReason = e.InactiveReason, IconUrl = e.IconUrl,
            Population = e.Population, HasPalisade = e.HasPalisade,
        };

        private static TerrainImprovement ToEntity(TerrainImprovementDTO d) { var e = new TerrainImprovement(); ApplyImprovement(e, d); e.Id = d.Id; return e; }

        private static void ApplyImprovement(TerrainImprovement e, TerrainImprovementDTO d)
        {
            e.BaronyId = d.BaronyId; e.TileId = d.TileId; e.TemplateId = d.TemplateId; e.Name = d.Name;
            e.AdditiveJson = Ser(d.Additive); e.PercentJson = Ser(d.Percent); e.Description = d.Description; e.FormulaText = d.FormulaText;
            e.IsActive = d.IsActive; e.InactiveReason = d.InactiveReason; e.IconUrl = d.IconUrl;
            e.Population = d.Population; e.HasPalisade = d.HasPalisade;
        }

        // ---------------- Mapping: Project ----------------
        private static BaronyProjectDTO ToDTO(BaronyProject e)
        {
            var goldProduction = De(e.CostGoldProductionJson);
            var materials = De(e.CostMaterialsJson);
            if (!ProjectCostCatalog.HasRequirement(goldProduction) && !ProjectCostCatalog.HasRequirement(materials))
                ProjectCostCatalog.SplitLegacyCost(De(e.CostJson), out goldProduction, out materials);

            return new BaronyProjectDTO
            {
                Id = e.Id,
                BaronyId = e.BaronyId,
                Name = e.Name,
                Description = e.Description,
                OutputKind = e.OutputKind,
                CostGoldProduction = goldProduction,
                CostMaterials = materials,
                AllowedCostModes = string.IsNullOrWhiteSpace(e.AllowedCostModes)
                    ? DA_Common.Barony.ProjectAllowedCostModes.PlayerChoice
                    : e.AllowedCostModes,
                SelectedCostMode = e.SelectedCostMode,
                ResultAdditive = De(e.ResultJson),
                ResultPercent = De(e.ResultPercentJson),
                Allocated = De(e.AllocatedJson),
                ResultDescription = e.ResultDescription,
                HideResultFromBaron = e.HideResultFromBaron,
                Status = e.Status,
                TurnsRemaining = e.TurnsRemaining,
                Notes = e.Notes,
                TileId = e.TileId,
                BuildingTemplateId = e.BuildingTemplateId,
                UnitId = e.UnitId,
            };
        }

        private static BaronyProject ToEntity(BaronyProjectDTO d) { var e = new BaronyProject(); ApplyProject(e, d); e.Id = d.Id; return e; }

        private static void ApplyProject(BaronyProject e, BaronyProjectDTO d)
        {
            e.BaronyId = d.BaronyId;
            e.Name = d.Name;
            e.Description = d.Description ?? string.Empty;
            e.OutputKind = string.IsNullOrWhiteSpace(d.OutputKind)
                ? DA_Common.Barony.ProjectOutputKind.DecreeOrTechnology
                : d.OutputKind.Trim();
            e.CostGoldProductionJson = Ser(ProjectCostCatalog.SliceGoldProduction(d.CostGoldProduction));
            e.CostMaterialsJson = Ser(ProjectCostCatalog.SliceMaterials(d.CostMaterials));
            e.AllowedCostModes = string.IsNullOrWhiteSpace(d.AllowedCostModes)
                ? DA_Common.Barony.ProjectAllowedCostModes.PlayerChoice
                : d.AllowedCostModes.Trim();
            e.SelectedCostMode = string.IsNullOrWhiteSpace(d.SelectedCostMode) ? null : d.SelectedCostMode.Trim();
            e.CostJson = Ser(MergeLegacyCost(d.CostGoldProduction, d.CostMaterials));
            e.ResultJson = Ser(d.ResultAdditive);
            e.ResultPercentJson = Ser(d.ResultPercent);
            e.AllocatedJson = Ser(ResourceCatalog.Slice(d.Allocated));
            e.ResultDescription = d.ResultDescription;
            e.HideResultFromBaron = d.HideResultFromBaron;
            e.Status = d.Status;
            e.TurnsRemaining = d.TurnsRemaining;
            e.Notes = d.Notes;
            // Never wipe map-construction links on partial updates that omit them.
            if (d.TileId is > 0)
                e.TileId = d.TileId;
            else if (d.Id <= 0)
                e.TileId = null;

            if (d.BuildingTemplateId is > 0)
                e.BuildingTemplateId = d.BuildingTemplateId;
            else if (d.Id <= 0)
                e.BuildingTemplateId = null;

            e.UnitId = d.UnitId is > 0 ? d.UnitId : null;
        }

        private static PpbVector MergeLegacyCost(PpbVector goldProduction, PpbVector materials)
        {
            var merged = ProjectCostCatalog.SliceGoldProduction(goldProduction);
            foreach (var info in ProjectCostCatalog.Materials)
                merged[info.Key] = materials[info.Key];
            return merged;
        }

        // ---------------- Mapping: Army unit ----------------
        private static BaronyUnitDTO ToUnitDTO(BaronyUnit e) => new()
        {
            Id = e.Id,
            BaronyId = e.BaronyId,
            Name = e.Name,
            Status = e.Status,
            TroopCount = e.TroopCount,
            MaxTroopCount = e.MaxTroopCount > 0 ? e.MaxTroopCount : UnitRules.DefaultTroopCount,
            RecruitSelectionKey = e.RecruitSelectionKey,
            TrainingTypeKey = e.TrainingTypeKey,
            RaceKey = string.IsNullOrWhiteSpace(e.RaceKey) ? UnitRaceKey.Human : e.RaceKey,
            Wage = e.Wage,
            UpkeepFood = e.UpkeepFood,
            UpkeepDefense = e.UpkeepDefense,
            Build = e.Build,
            Agility = e.Agility,
            Will = e.Will,
            Perception = e.Perception,
            AttrPenaltyBuild = e.AttrPenaltyBuild,
            AttrPenaltyAgility = e.AttrPenaltyAgility,
            AttrOtherBuild = e.AttrOtherBuild,
            AttrOtherAgility = e.AttrOtherAgility,
            AttrOtherWill = e.AttrOtherWill,
            AttrOtherPerception = e.AttrOtherPerception,
            AttrOtherSources = DeCombatOther(e.AttrOtherSourcesJson),
            SkillBase = DeIntDict(e.SkillsJson),
            SkillOther = DeIntDict(e.SkillOtherJson),
            CombatOther = DeCombatOther(e.CombatOtherJson),
            SkillOtherSources = DeCombatOther(e.SkillOtherSourcesJson),
            Weapon1Key = e.Weapon1Key,
            Weapon2Key = e.Weapon2Key,
            ArmorKey = e.ArmorKey,
            ShieldKey = e.ShieldKey,
            MountKey = e.MountKey,
            Weapon1Quality = e.Weapon1Quality,
            Weapon2Quality = e.Weapon2Quality,
            DefenseSkillKey = e.DefenseSkillKey,
            CommanderAttack = e.CommanderAttack,
            CommanderDefense = e.CommanderDefense,
            CaptainAvailableAdvisorId = e.CaptainAvailableAdvisorId,
            CaptainIsBaron = e.CaptainIsBaron,
            CurrentAction = e.CurrentAction ?? string.Empty,
            ActionTrainingJc = e.ActionTrainingJc,
            ActionDemobilizeTroops = e.ActionDemobilizeTroops,
            OtherAttack = e.OtherAttack,
            OtherDefense = e.OtherDefense,
            OtherDamage = e.OtherDamage,
            OtherMove = e.OtherMove,
            OtherArmor = e.OtherArmor,
            OtherHp = e.OtherHp,
            RemainingPd = e.RemainingPd,
            Discipline = e.Discipline,
            MaxBaseSkillAtGraduation = e.MaxBaseSkillAtGraduation,
            FreeAttributePoints = e.FreeAttributePoints,
            CurrentHp = e.CurrentHp,
            CreatedAtUtc = e.CreatedAtUtc,
            UpdatedAtUtc = e.UpdatedAtUtc,
            Log = DeUnitLog(e.LogJson),
        };

        private static BaronyUnit ToUnitEntity(BaronyUnitDTO d)
        {
            var e = new BaronyUnit();
            ApplyUnit(e, d);
            e.Id = d.Id;
            return e;
        }

        private static void ApplyUnit(BaronyUnit e, BaronyUnitDTO d)
        {
            // Keep DefenseSkillKey in sync with auto-pick (highest eligible vs gear).
            _ = UnitStatHelper.Compute(d);

            e.BaronyId = d.BaronyId;
            e.Name = d.Name?.Trim() ?? string.Empty;
            e.Status = string.IsNullOrWhiteSpace(d.Status) ? UnitStatus.Training : d.Status.Trim();
            e.MaxTroopCount = Math.Clamp(
                d.MaxTroopCount > 0 ? d.MaxTroopCount : UnitRules.DefaultTroopCount,
                1,
                UnitRules.AbsoluteMaxTroopCount);
            e.TroopCount = Math.Clamp(d.TroopCount, 0, e.MaxTroopCount);
            e.RecruitSelectionKey = d.RecruitSelectionKey ?? string.Empty;
            e.TrainingTypeKey = d.TrainingTypeKey ?? string.Empty;
            e.RaceKey = string.IsNullOrWhiteSpace(d.RaceKey) ? UnitRaceKey.Human : d.RaceKey.Trim();
            e.Wage = d.Wage;
            e.UpkeepFood = d.UpkeepFood;
            e.UpkeepDefense = d.UpkeepDefense;
            e.Build = d.Build;
            e.Agility = d.Agility;
            e.Will = d.Will;
            e.Perception = d.Perception;
            e.AttrPenaltyBuild = d.AttrPenaltyBuild;
            e.AttrPenaltyAgility = d.AttrPenaltyAgility;
            SyncAttrOtherFromSources(d);
            e.AttrOtherBuild = d.AttrOtherBuild;
            e.AttrOtherAgility = d.AttrOtherAgility;
            e.AttrOtherWill = d.AttrOtherWill;
            e.AttrOtherPerception = d.AttrOtherPerception;
            e.AttrOtherSourcesJson = SerCombatOther(d.AttrOtherSources);
            e.SkillsJson = SerIntDict(d.SkillBase);
            SyncSkillOtherFromSources(d);
            e.SkillOtherJson = SerIntDict(d.SkillOther);
            SyncCombatOtherTotals(d);
            e.CombatOtherJson = SerCombatOther(d.CombatOther);
            e.SkillOtherSourcesJson = SerCombatOther(d.SkillOtherSources);
            e.Weapon1Key = string.IsNullOrWhiteSpace(d.Weapon1Key) ? null : d.Weapon1Key.Trim();
            e.Weapon2Key = string.IsNullOrWhiteSpace(d.Weapon2Key) ? null : d.Weapon2Key.Trim();
            e.ArmorKey = string.IsNullOrWhiteSpace(d.ArmorKey) ? null : d.ArmorKey.Trim();
            e.ShieldKey = string.IsNullOrWhiteSpace(d.ShieldKey) ? null : d.ShieldKey.Trim();
            e.MountKey = string.IsNullOrWhiteSpace(d.MountKey) ? null : d.MountKey.Trim();
            e.Weapon1Quality = string.IsNullOrWhiteSpace(d.Weapon1Quality)
                ? UnitWeaponQuality.Normal
                : d.Weapon1Quality.Trim();
            e.Weapon2Quality = string.IsNullOrWhiteSpace(d.Weapon2Quality)
                ? UnitWeaponQuality.Normal
                : d.Weapon2Quality.Trim();
            e.DefenseSkillKey = string.IsNullOrWhiteSpace(d.DefenseSkillKey)
                ? UnitSkillKey.Dodges
                : d.DefenseSkillKey.Trim();
            e.CommanderAttack = d.CommanderAttack;
            e.CommanderDefense = d.CommanderDefense;
            e.CaptainAvailableAdvisorId = d.CaptainIsBaron
                ? null
                : (d.CaptainAvailableAdvisorId is int cap && cap > 0 ? cap : null);
            e.CaptainIsBaron = d.CaptainIsBaron;
            e.CurrentAction = UnitActionKind.Normalize(d.CurrentAction);
            e.ActionTrainingJc = UnitActionFormulas.ClampJc(d.ActionTrainingJc);
            e.ActionDemobilizeTroops = Math.Max(0, d.ActionDemobilizeTroops);
            if (!UnitActionKind.IsPartialDemobilization(e.CurrentAction))
                e.ActionDemobilizeTroops = 0;
            if (!UnitActionKind.GrantsTrainingXp(e.CurrentAction) || !e.CaptainIsBaron)
                e.ActionTrainingJc = 0;
            e.OtherAttack = d.OtherAttack;
            e.OtherDefense = d.OtherDefense;
            e.OtherDamage = d.OtherDamage;
            e.OtherMove = d.OtherMove;
            e.OtherArmor = d.OtherArmor;
            e.OtherHp = d.OtherHp;
            e.RemainingPd = d.RemainingPd;
            e.Discipline = Math.Clamp(d.Discipline, UnitRules.DisciplineMin, UnitRules.DisciplineMax);
            e.MaxBaseSkillAtGraduation = d.MaxBaseSkillAtGraduation;
            e.FreeAttributePoints = Math.Max(0, d.FreeAttributePoints);
            e.CurrentHp = d.CurrentHp;
            e.CreatedAtUtc = d.CreatedAtUtc;
            e.UpdatedAtUtc = d.UpdatedAtUtc;
            e.LogJson = SerUnitLog(d.Log);
        }

        private static async Task EnforceCaptainAssignmentAsync(ApplicationDbContext ctx, BaronyUnitDTO dto)
        {
            if (dto.CaptainIsBaron)
            {
                dto.CaptainAvailableAdvisorId = null;
                dto.CaptainName = Loc.T("Baron");

                var otherBaronLed = await ctx.BaronyUnits
                    .Where(u => u.BaronyId == dto.BaronyId && u.CaptainIsBaron && u.Id != dto.Id)
                    .ToListAsync();
                foreach (var other in otherBaronLed)
                {
                    other.CaptainIsBaron = false;
                    other.UpdatedAtUtc = DateTime.UtcNow;
                }
                return;
            }

            if (dto.CaptainAvailableAdvisorId is not int captainId || captainId <= 0)
            {
                dto.CaptainAvailableAdvisorId = null;
                dto.CaptainName = null;
                return;
            }

            var person = await ctx.AvailableAdvisors.AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == captainId && a.BaronyId == dto.BaronyId);
            if (person is null)
                throw new InvalidOperationException("Captain must be a court person of this barony.");

            dto.CaptainName = person.Name;

            var others = await ctx.BaronyUnits
                .Where(u => u.BaronyId == dto.BaronyId
                            && u.CaptainAvailableAdvisorId == captainId
                            && u.Id != dto.Id)
                .ToListAsync();
            foreach (var other in others)
            {
                other.CaptainAvailableAdvisorId = null;
                other.CaptainIsBaron = false;
                other.CommanderAttack = 0;
                other.CommanderDefense = 0;
                // Strip Commander combat-other entries and resum.
                var map = DeCombatOther(other.CombatOtherJson);
                var ca = 0;
                var cd = 0;
                var oa = other.OtherAttack;
                var od = other.OtherDefense;
                var odm = other.OtherDamage;
                var om = other.OtherMove;
                var oar = other.OtherArmor;
                var oh = other.OtherHp;
                UnitCommanderSync.ClearCaptainBonuses(
                    ref ca, ref cd, ref oa, ref od, ref odm, ref om, ref oar, ref oh, map);
                other.CommanderAttack = ca;
                other.CommanderDefense = cd;
                other.OtherAttack = oa;
                other.OtherDefense = od;
                other.OtherDamage = odm;
                other.OtherMove = om;
                other.OtherArmor = oar;
                other.OtherHp = oh;
                other.CombatOtherJson = SerCombatOther(map);
                other.UpdatedAtUtc = DateTime.UtcNow;
            }
        }

        private static async Task SyncUnitCommanderBonusesAsync(ApplicationDbContext ctx, BaronyUnitDTO dto)
        {
            dto.CombatOther ??= new Dictionary<string, List<UnitCombatModifierEntry>>(StringComparer.OrdinalIgnoreCase);
            CourtCharacterSheet? sheet = null;
            if (dto.CaptainAvailableAdvisorId is int captainId && captainId > 0)
            {
                var person = await ctx.AvailableAdvisors.AsNoTracking()
                    .FirstOrDefaultAsync(a => a.Id == captainId && a.BaronyId == dto.BaronyId);
                if (person is not null)
                    sheet = DeserializeCourtSheet(person.SheetJson);
            }

            var hasMount = !string.IsNullOrWhiteSpace(dto.MountKey);
            var hasShield = !string.IsNullOrWhiteSpace(dto.ShieldKey);
            var ca = dto.CommanderAttack;
            var cd = dto.CommanderDefense;
            var oa = dto.OtherAttack;
            var od = dto.OtherDefense;
            var odm = dto.OtherDamage;
            var om = dto.OtherMove;
            var oar = dto.OtherArmor;
            var oh = dto.OtherHp;
            UnitCommanderSync.ApplyCaptainBonuses(
                ref ca, ref cd, ref oa, ref od, ref odm, ref om, ref oar, ref oh,
                dto.CombatOther, sheet, hasMount, hasShield);
            dto.CommanderAttack = ca;
            dto.CommanderDefense = cd;
            dto.OtherAttack = oa;
            dto.OtherDefense = od;
            dto.OtherDamage = odm;
            dto.OtherMove = om;
            dto.OtherArmor = oar;
            dto.OtherHp = oh;
        }

        private static async Task ResyncUnitsForCaptainAsync(ApplicationDbContext ctx, int captainId, int baronyId)
        {
            if (captainId <= 0)
                return;
            var person = await ctx.AvailableAdvisors.AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == captainId && a.BaronyId == baronyId);
            var sheet = person is null ? null : DeserializeCourtSheet(person.SheetJson);
            var units = await ctx.BaronyUnits
                .Where(u => u.BaronyId == baronyId && u.CaptainAvailableAdvisorId == captainId)
                .ToListAsync();
            foreach (var unit in units)
            {
                var dto = ToUnitDTO(unit);
                var hasMount = !string.IsNullOrWhiteSpace(dto.MountKey);
                var hasShield = !string.IsNullOrWhiteSpace(dto.ShieldKey);
                var ca = dto.CommanderAttack;
                var cd = dto.CommanderDefense;
                var oa = dto.OtherAttack;
                var od = dto.OtherDefense;
                var odm = dto.OtherDamage;
                var om = dto.OtherMove;
                var oar = dto.OtherArmor;
                var oh = dto.OtherHp;
                UnitCommanderSync.ApplyCaptainBonuses(
                    ref ca, ref cd, ref oa, ref od, ref odm, ref om, ref oar, ref oh,
                    dto.CombatOther, sheet, hasMount, hasShield);
                dto.CommanderAttack = ca;
                dto.CommanderDefense = cd;
                dto.OtherAttack = oa;
                dto.OtherDefense = od;
                dto.OtherDamage = odm;
                dto.OtherMove = om;
                dto.OtherArmor = oar;
                dto.OtherHp = oh;
                ApplyUnit(unit, dto);
                unit.UpdatedAtUtc = DateTime.UtcNow;
            }
        }

        private static string? BuildGearDefenseNote(StartUnitTrainingRequest request)
        {
            var parts = new List<string>();
            void Add(string slot, string? itemKey, string mode)
            {
                if (string.IsNullOrWhiteSpace(itemKey)) return;
                var m = UnitEquipmentAcquireMode.Normalize(mode);
                if (m == UnitEquipmentAcquireMode.Craft) return;
                parts.Add($"{slot}={UnitEquipmentAcquireMode.Label(m)}");
            }

            Add("W1", request.Weapon1Key, request.Weapon1AcquireMode);
            Add("W2", request.Weapon2Key, request.Weapon2AcquireMode);
            Add("armor", request.ArmorKey, request.ArmorAcquireMode);
            Add("shield", request.ShieldKey, request.ShieldAcquireMode);
            Add("mount", request.MountKey, request.MountAcquireMode);
            return parts.Count == 0
                ? null
                : $"Gear acquire: {string.Join(", ", parts)} (Buy = Mkt gold; Defense = 2×Mkt).";
        }

        private static string BuildReinforceNote(int troopCount, UnitEquipmentPayModes pay)
        {
            var parts = new List<string>
            {
                $"ReinforceTroops={troopCount}",
                $"W1={UnitEquipmentAcquireMode.Label(pay.Weapon1)}",
                $"W2={UnitEquipmentAcquireMode.Label(pay.Weapon2)}",
                $"armor={UnitEquipmentAcquireMode.Label(pay.Armor)}",
                $"shield={UnitEquipmentAcquireMode.Label(pay.Shield)}",
                $"mount={UnitEquipmentAcquireMode.Label(pay.Mount)}",
            };
            return string.Join("; ", parts)
                + $". People = Selected volunteers + Standard × N/{UnitRules.DefaultTroopCount}; "
                + $"gear = {UnitRules.ReinforceGearSalvagePercent}% salvage × same scale.";
        }

        private static string BuildChangeEquipmentNote(
            string weapon1Key,
            string? weapon2Key,
            string? armorKey,
            string? shieldKey,
            string? mountKey,
            string weapon1Quality,
            UnitEquipmentPayModes pay)
        {
            var parts = new List<string>
            {
                "ChangeEquipment=1",
                $"W1Key={weapon1Key}",
                $"W2Key={weapon2Key ?? ""}",
                $"ArmorKey={armorKey ?? ""}",
                $"ShieldKey={shieldKey ?? ""}",
                $"MountKey={mountKey ?? ""}",
                $"Qual={weapon1Quality}",
                $"W1={UnitEquipmentAcquireMode.Label(pay.Weapon1)}",
                $"W2={UnitEquipmentAcquireMode.Label(pay.Weapon2)}",
                $"armor={UnitEquipmentAcquireMode.Label(pay.Armor)}",
                $"shield={UnitEquipmentAcquireMode.Label(pay.Shield)}",
                $"mount={UnitEquipmentAcquireMode.Label(pay.Mount)}",
            };
            return string.Join("; ", parts);
        }

        private static bool TryApplyChangeEquipmentFromNotes(
            BaronyProjectDTO project,
            BaronyUnit unit,
            out string loadoutNote)
        {
            loadoutNote = string.Empty;
            var w1Key = ReadNoteValue(project.Notes, "W1Key=");
            if (string.IsNullOrWhiteSpace(w1Key) || UnitWeaponCatalog.Find(w1Key) is null)
                return false;

            var w2Key = NullIfEmpty(ReadNoteValue(project.Notes, "W2Key="));
            var armorKey = NullIfEmpty(ReadNoteValue(project.Notes, "ArmorKey="));
            var shieldKey = NullIfEmpty(ReadNoteValue(project.Notes, "ShieldKey="));
            var mountKey = NullIfEmpty(ReadNoteValue(project.Notes, "MountKey="));
            var quality = ReadNoteValue(project.Notes, "Qual=");
            if (string.IsNullOrWhiteSpace(quality)
                || !UnitWeaponQuality.All.Contains(quality, StringComparer.OrdinalIgnoreCase))
                quality = UnitWeaponQuality.Normal;

            if (w2Key is not null && UnitWeaponCatalog.Find(w2Key) is null)
                w2Key = null;
            if (armorKey is not null && UnitArmorCatalog.Find(armorKey) is null)
                armorKey = null;
            if (shieldKey is not null && UnitArmorCatalog.Find(shieldKey) is null)
                shieldKey = null;
            if (mountKey is not null && UnitMountCatalog.Find(mountKey) is null)
                mountKey = null;

            var armor = UnitArmorCatalog.Find(armorKey);
            var shield = UnitArmorCatalog.Find(shieldKey);

            var dto = ToUnitDTO(unit);
            var oldMax = UnitStatHelper.Compute(dto).MaxHp;
            var wasAtMax = unit.CurrentHp >= oldMax;

            unit.Weapon1Key = w1Key.Trim();
            unit.Weapon2Key = w2Key;
            unit.ArmorKey = armorKey;
            unit.ShieldKey = shieldKey;
            unit.MountKey = mountKey;
            unit.Weapon1Quality = quality;
            unit.Weapon2Quality = UnitWeaponQuality.Normal;
            unit.AttrPenaltyAgility = (armor?.AgilityPenalty ?? 0) + (shield?.AgilityPenalty ?? 0);
            unit.UpdatedAtUtc = DateTime.UtcNow;

            dto = ToUnitDTO(unit);
            dto.AttrPenaltyAgility = unit.AttrPenaltyAgility;
            var newMax = UnitStatHelper.Compute(dto).MaxHp;
            unit.CurrentHp = wasAtMax ? newMax : Math.Min(unit.CurrentHp, newMax);
            unit.DefenseSkillKey = dto.DefenseSkillKey;

            loadoutNote = string.Join(", ", new[]
            {
                UnitWeaponCatalog.Find(unit.Weapon1Key)?.Name ?? unit.Weapon1Key,
                unit.Weapon2Key is null ? null : UnitWeaponCatalog.Find(unit.Weapon2Key)?.Name ?? unit.Weapon2Key,
                unit.ArmorKey is null ? null : UnitArmorCatalog.Find(unit.ArmorKey)?.Name ?? unit.ArmorKey,
                unit.ShieldKey is null ? null : UnitArmorCatalog.Find(unit.ShieldKey)?.Name ?? unit.ShieldKey,
                unit.MountKey is null ? null : UnitMountCatalog.Find(unit.MountKey)?.Name ?? unit.MountKey,
            }.Where(s => !string.IsNullOrWhiteSpace(s)));
            return true;
        }

        private static string? NullIfEmpty(string? value) =>
            string.IsNullOrWhiteSpace(value) ? null : value.Trim();

        private static string? ReadNoteValue(string? notes, string prefix)
        {
            if (string.IsNullOrWhiteSpace(notes) || string.IsNullOrEmpty(prefix))
                return null;
            var idx = notes.IndexOf(prefix, StringComparison.OrdinalIgnoreCase);
            if (idx < 0)
                return null;
            var start = idx + prefix.Length;
            var end = notes.IndexOf(';', start);
            var raw = end < 0 ? notes[start..] : notes[start..end];
            return raw.Trim().TrimEnd('.');
        }

        /// <summary>
        /// Troops to add when a reinforce project completes.
        /// Prefers Notes (<c>ReinforceTroops=N</c>), then ResultDescription / Description, then fill-to-full.
        /// </summary>
        private static int ResolveReinforceTroopAdd(BaronyProjectDTO project, BaronyUnit unit)
        {
            var fromNotes = ReadReinforceTroops(project.Notes);
            if (fromNotes > 0)
                return ClampReinforceAdd(fromNotes, unit.TroopCount, unit.MaxTroopCount);

            var fromResult = ReadLeadingIntAfter(project.ResultDescription, "Adds ");
            if (fromResult > 0)
                return ClampReinforceAdd(fromResult, unit.TroopCount, unit.MaxTroopCount);

            var fromDesc = ReadLeadingIntAfter(project.Description, "Replenish ");
            if (fromDesc > 0)
                return ClampReinforceAdd(fromDesc, unit.TroopCount, unit.MaxTroopCount);

            var full = unit.MaxTroopCount > 0 ? unit.MaxTroopCount : UnitRules.DefaultTroopCount;
            var missing = full - unit.TroopCount;
            return Math.Max(0, missing);
        }

        private static int ClampReinforceAdd(int add, int currentTroops, int maxTroopCount)
        {
            var full = maxTroopCount > 0 ? maxTroopCount : UnitRules.DefaultTroopCount;
            return Math.Clamp(add, 0, Math.Max(0, full - currentTroops));
        }

        private static int ReadReinforceTroops(string? notes)
        {
            if (string.IsNullOrWhiteSpace(notes))
                return 0;

            const string prefix = "ReinforceTroops=";
            var idx = notes.IndexOf(prefix, StringComparison.OrdinalIgnoreCase);
            if (idx < 0)
                return 0;

            return ReadIntAt(notes, idx + prefix.Length);
        }

        private static int ReadLeadingIntAfter(string? text, string marker)
        {
            if (string.IsNullOrWhiteSpace(text) || string.IsNullOrEmpty(marker))
                return 0;
            var idx = text.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
            if (idx < 0)
                return 0;
            return ReadIntAt(text, idx + marker.Length);
        }

        private static int ReadIntAt(string text, int start)
        {
            while (start < text.Length && char.IsWhiteSpace(text[start]))
                start++;
            var end = start;
            while (end < text.Length && char.IsDigit(text[end]))
                end++;
            if (end > start && int.TryParse(text.AsSpan(start, end - start), out var n) && n > 0)
                return n;
            return 0;
        }

        private static string SerIntDict(Dictionary<string, int>? map)
        {
            map ??= new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            return JsonSerializer.Serialize(map, JsonOptions);
        }

        private static Dictionary<string, int> DeIntDict(string? json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            try
            {
                var dict = JsonSerializer.Deserialize<Dictionary<string, int>>(json, JsonOptions)
                    ?? new Dictionary<string, int>();
                return new Dictionary<string, int>(dict, StringComparer.OrdinalIgnoreCase);
            }
            catch
            {
                return new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            }
        }

        private static void SyncCombatOtherTotals(BaronyUnitDTO d)
        {
            d.CombatOther ??= new Dictionary<string, List<UnitCombatModifierEntry>>(StringComparer.OrdinalIgnoreCase);
            var (atk, def, dmg, mov, arm, hp) = UnitCombatOtherFormulas.SumAll(d.CombatOther);
            d.OtherAttack = atk;
            d.OtherDefense = def;
            d.OtherDamage = dmg;
            d.OtherMove = mov;
            d.OtherArmor = arm;
            d.OtherHp = hp;
        }

        private static void SyncSkillOtherFromSources(BaronyUnitDTO d)
        {
            d.SkillOther ??= new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            d.SkillOtherSources ??= new Dictionary<string, List<UnitCombatModifierEntry>>(StringComparer.OrdinalIgnoreCase);
            UnitCombatOtherFormulas.ApplySkillOtherTotals(d.SkillOtherSources, d.SkillOther);
        }

        private static void SyncAttrOtherFromSources(BaronyUnitDTO d)
        {
            d.AttrOtherSources ??= new Dictionary<string, List<UnitCombatModifierEntry>>(StringComparer.OrdinalIgnoreCase);
            UnitCombatOtherFormulas.ApplyAttrOtherTotals(
                d.AttrOtherSources,
                out var build, out var agility, out var will, out var perception);
            d.AttrOtherBuild = build;
            d.AttrOtherAgility = agility;
            d.AttrOtherWill = will;
            d.AttrOtherPerception = perception;
        }

        private static void ApplyAttrOtherFromMap(
            BaronyUnit e,
            Dictionary<string, List<UnitCombatModifierEntry>>? map)
        {
            map ??= new Dictionary<string, List<UnitCombatModifierEntry>>(StringComparer.OrdinalIgnoreCase);
            e.AttrOtherSourcesJson = SerCombatOther(map);
            UnitCombatOtherFormulas.ApplyAttrOtherTotals(
                map, out var build, out var agility, out var will, out var perception);
            e.AttrOtherBuild = build;
            e.AttrOtherAgility = agility;
            e.AttrOtherWill = will;
            e.AttrOtherPerception = perception;
        }

        private static Dictionary<string, int> SyncSkillOtherCopy(
            Dictionary<string, int>? skillOther,
            Dictionary<string, List<UnitCombatModifierEntry>>? sources)
        {
            var result = new Dictionary<string, int>(
                skillOther ?? new Dictionary<string, int>(),
                StringComparer.OrdinalIgnoreCase);
            UnitCombatOtherFormulas.ApplySkillOtherTotals(
                sources ?? new Dictionary<string, List<UnitCombatModifierEntry>>(StringComparer.OrdinalIgnoreCase),
                result);
            return result;
        }

        private static void ApplyCombatOtherFromMap(
            BaronyUnit e,
            Dictionary<string, List<UnitCombatModifierEntry>>? map)
        {
            map ??= new Dictionary<string, List<UnitCombatModifierEntry>>(StringComparer.OrdinalIgnoreCase);
            e.CombatOtherJson = SerCombatOther(map);
            var (atk, def, dmg, mov, arm, hp) = UnitCombatOtherFormulas.SumAll(map);
            e.OtherAttack = atk;
            e.OtherDefense = def;
            e.OtherDamage = dmg;
            e.OtherMove = mov;
            e.OtherArmor = arm;
            e.OtherHp = hp;
        }

        private static string SerCombatOther(Dictionary<string, List<UnitCombatModifierEntry>>? map)
        {
            map ??= new Dictionary<string, List<UnitCombatModifierEntry>>(StringComparer.OrdinalIgnoreCase);
            var clean = new Dictionary<string, List<UnitCombatModifierEntry>>(StringComparer.OrdinalIgnoreCase);
            foreach (var (key, list) in map)
            {
                if (string.IsNullOrWhiteSpace(key) || list is null || list.Count == 0)
                    continue;
                clean[key.Trim()] = list
                    .Select(m => new UnitCombatModifierEntry
                    {
                        Label = (m.Label ?? string.Empty).Trim(),
                        Value = m.Value,
                    })
                    .Where(m => !string.IsNullOrWhiteSpace(m.Label) || m.Value != 0)
                    .ToList();
            }
            return JsonSerializer.Serialize(clean, JsonOptions);
        }

        private static Dictionary<string, List<UnitCombatModifierEntry>> DeCombatOther(string? json)
        {
            var result = new Dictionary<string, List<UnitCombatModifierEntry>>(StringComparer.OrdinalIgnoreCase);
            if (string.IsNullOrWhiteSpace(json))
                return result;
            try
            {
                var dict = JsonSerializer.Deserialize<Dictionary<string, List<UnitCombatModifierEntry>>>(json, JsonOptions);
                if (dict is null) return result;
                foreach (var (key, list) in dict)
                {
                    if (string.IsNullOrWhiteSpace(key) || list is null) continue;
                    result[key.Trim()] = list
                        .Select(m => new UnitCombatModifierEntry
                        {
                            Label = m.Label ?? string.Empty,
                            Value = m.Value,
                        })
                        .ToList();
                }
            }
            catch
            {
                // ignore corrupt JSON
            }
            return result;
        }

        private static string SerUnitLog(List<BaronyUnitLogEntryDTO>? log)
        {
            log ??= new();
            var clean = log
                .Where(e => e is not null)
                .Select(e => new BaronyUnitLogEntryDTO
                {
                    Id = string.IsNullOrWhiteSpace(e.Id) ? Guid.NewGuid().ToString("N") : e.Id.Trim(),
                    UtcAt = e.UtcAt == default ? DateTime.UtcNow : e.UtcAt,
                    Kind = string.IsNullOrWhiteSpace(e.Kind) ? "system" : e.Kind.Trim(),
                    Text = (e.Text ?? string.Empty).Trim(),
                    XpDelta = e.XpDelta,
                    Note = string.IsNullOrWhiteSpace(e.Note) ? null : e.Note.Trim(),
                })
                .ToList();
            if (clean.Count > 200)
                clean = clean[^200..];
            return JsonSerializer.Serialize(clean, JsonOptions);
        }

        private static void AddUnitLogEntry(
            BaronyUnit unit,
            string kind,
            string text,
            int? xpDelta = null,
            string? note = null)
        {
            var log = DeUnitLog(unit.LogJson);
            log.Add(new BaronyUnitLogEntryDTO
            {
                Id = Guid.NewGuid().ToString("N"),
                UtcAt = DateTime.UtcNow,
                Kind = string.IsNullOrWhiteSpace(kind) ? "system" : kind.Trim(),
                Text = (text ?? string.Empty).Trim(),
                XpDelta = xpDelta,
                Note = string.IsNullOrWhiteSpace(note) ? null : note.Trim(),
            });
            unit.LogJson = SerUnitLog(log);
        }

        private static List<BaronyUnitLogEntryDTO> DeUnitLog(string? json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return new();
            try
            {
                var list = JsonSerializer.Deserialize<List<BaronyUnitLogEntryDTO>>(json, JsonOptions) ?? new();
                var normalized = list
                    .Where(e => e is not null)
                    .Select(e => new BaronyUnitLogEntryDTO
                    {
                        Id = string.IsNullOrWhiteSpace(e.Id) ? Guid.NewGuid().ToString("N") : e.Id.Trim(),
                        UtcAt = e.UtcAt,
                        Kind = string.IsNullOrWhiteSpace(e.Kind) ? "system" : e.Kind.Trim(),
                        Text = e.Text ?? string.Empty,
                        XpDelta = e.XpDelta,
                        Note = e.Note,
                    })
                    .OrderBy(e => e.UtcAt)
                    .ToList();
                if (normalized.Count > 200)
                    normalized = normalized[^200..];
                return normalized;
            }
            catch
            {
                return new();
            }
        }

        internal static UnitCombatTotals ComputeUnitCombat(BaronyUnitDTO dto) =>
            UnitStatHelper.Compute(dto);

        // ---------------- Mapping: Resource source ----------------
        private static BaronyResourceSourceDTO ToDTO(BaronyResourceSource e) => new()
        {
            Id = e.Id,
            BaronyId = e.BaronyId,
            Name = e.Name,
            Description = e.Description,
            Additive = De(e.AdditiveJson),
            IsTurnEphemeral = e.IsTurnEphemeral,
            VisibleOnTurn = e.VisibleOnTurn,
        };

        private static BaronyResourceSource ToEntity(BaronyResourceSourceDTO d)
        {
            var e = new BaronyResourceSource();
            ApplyResourceSource(e, d);
            e.Id = d.Id;
            return e;
        }

        private static void ApplyResourceSource(BaronyResourceSource e, BaronyResourceSourceDTO d)
        {
            e.BaronyId = d.BaronyId;
            e.Name = d.Name;
            e.Description = d.Description;
            e.AdditiveJson = Ser(ResourceCatalog.Slice(d.Additive));
            e.IsTurnEphemeral = d.IsTurnEphemeral;
            e.VisibleOnTurn = d.IsTurnEphemeral ? d.VisibleOnTurn : null;
        }

        // ---------------- Mapping: Baron purse source ----------------
        private static BaronPurseSourceDTO ToDTO(BaronPurseSource e) => new()
        {
            Id = e.Id,
            BaronyId = e.BaronyId,
            Name = e.Name,
            Description = e.Description,
            Amount = e.Amount,
        };

        private static BaronPurseSource ToEntity(BaronPurseSourceDTO d)
        {
            var e = new BaronPurseSource();
            ApplyPurseSource(e, d);
            e.Id = d.Id;
            return e;
        }

        private static void ApplyPurseSource(BaronPurseSource e, BaronPurseSourceDTO d)
        {
            e.BaronyId = d.BaronyId;
            e.Name = d.Name;
            e.Description = d.Description;
            e.Amount = d.Amount;
        }

        // ---------------- Mapping: Template ----------------
        private static BuildingTemplateDTO ToDTO(BuildingTemplate e) => new()
        {
            Id = e.Id, Name = e.Name, IsCustom = e.IsCustom, RequiredLordshipLevel = e.RequiredLordshipLevel, Kind = e.Kind,
            GoldCost = e.GoldCost, ProductionCost = e.ProductionCost,
            EffectAdditive = De(e.EffectAdditiveJson), EffectPercent = De(e.EffectPercentJson),
            Description = e.Description, TerrainRequirement = e.TerrainRequirement,
            MapPinKind = e.MapPinKind, IconUrl = e.IconUrl,
        };

        private static BuildingTemplate ToEntity(BuildingTemplateDTO d) { var e = new BuildingTemplate(); ApplyTemplate(e, d); e.Id = d.Id; return e; }

        private static void ApplyTemplate(BuildingTemplate e, BuildingTemplateDTO d)
        {
            e.Name = d.Name; e.IsCustom = d.IsCustom; e.RequiredLordshipLevel = d.RequiredLordshipLevel; e.Kind = d.Kind;
            e.GoldCost = d.GoldCost; e.ProductionCost = d.ProductionCost;
            e.EffectAdditiveJson = Ser(d.EffectAdditive); e.EffectPercentJson = Ser(d.EffectPercent);
            e.Description = d.Description; e.TerrainRequirement = d.TerrainRequirement;
            e.MapPinKind = d.MapPinKind; e.IconUrl = d.IconUrl;
        }

        // ---------------- Mapping: Lord's Seat ----------------
        private static async Task<BaronySeatDTO?> LoadSeatDtoAsync(ApplicationDbContext ctx, int baronyId)
        {
            var seat = await ctx.BaronySeats.AsNoTracking()
                .Include(s => s.Rooms)
                .ThenInclude(r => r.Traits)
                .Include(s => s.Tiles)
                .FirstOrDefaultAsync(s => s.BaronyId == baronyId);
            return seat is null ? null : ToDTO(seat, baronyId);
        }

        private static async Task<BaronySeatDTO> EnsureSeatDtoAsync(ApplicationDbContext ctx, int baronyId)
        {
            var existing = await LoadSeatDtoAsync(ctx, baronyId);
            if (existing is not null)
                return existing;

            var seat = new BaronySeat { BaronyId = baronyId, ActiveLevelsJson = "[0]" };
            ctx.BaronySeats.Add(seat);
            await ctx.SaveChangesAsync();
            return ToDTO(seat, baronyId);
        }

        private static async Task<List<SeatPurposeTemplateDTO>> LoadPurposeTemplatesAsync(ApplicationDbContext ctx, int baronyId) =>
            (await ctx.SeatPurposeTemplates.AsNoTracking()
                .Where(t => t.IsUniversal || t.BaronyId == baronyId)
                .OrderBy(t => t.SortOrder)
                .ThenBy(t => t.Name)
                .ToListAsync())
            .Select(ToDTO)
            .ToList();

        private static BaronySeatDTO ToDTO(BaronySeat e, int baronyId) => new()
        {
            Id = e.Id,
            BaronyId = baronyId,
            Name = e.Name ?? "Lord's Seat",
            GridWidth = e.GridWidth,
            GridHeight = e.GridHeight,
            ActiveLevels = ParseActiveLevels(e.ActiveLevelsJson),
            Rooms = (e.Rooms ?? new List<SeatRoom>())
                .OrderBy(r => r.Level)
                .ThenBy(r => r.SortOrder)
                .ThenBy(r => r.Name)
                .Select(r => ToDTO(r, baronyId))
                .ToList(),
            Tiles = (e.Tiles ?? new List<SeatTile>())
                .OrderBy(t => t.Level)
                .ThenBy(t => t.Y)
                .ThenBy(t => t.X)
                .Select(ToDTO)
                .ToList(),
        };

        private static SeatTileDTO ToDTO(SeatTile e) => new()
        {
            Id = e.Id,
            SeatId = e.SeatId,
            Level = e.Level,
            X = e.X,
            Y = e.Y,
            Kind = SeatTileKind.Normalize(e.Kind),
        };

        private static SeatRoomDTO ToDTO(SeatRoom e, int baronyId) => new()
        {
            Id = e.Id,
            SeatId = e.SeatId,
            BaronyId = baronyId,
            Name = e.Name ?? "",
            Level = SeatFloorLevel.Clamp(e.Level),
            GridX = e.GridX,
            GridY = e.GridY,
            GridW = e.GridW,
            GridH = e.GridH,
            Material = e.Material ?? SeatRoomMaterial.Stone,
            PrestigeMultiplier = e.PrestigeMultiplier,
            Status = e.Status ?? SeatRoomStatus.Active,
            Additive = De(e.AdditiveJson),
            Percent = De(e.PercentJson),
            PurposeTemplateId = e.PurposeTemplateId,
            OccupantAdvisorId = e.OccupantAdvisorId,
            OccupantCustom = e.OccupantCustom ?? "",
            SortOrder = e.SortOrder,
            Traits = (e.Traits ?? new List<SeatRoomTrait>())
                .OrderBy(t => t.SortOrder)
                .Select(t => new SeatRoomTraitDTO
                {
                    Id = t.Id,
                    Kind = t.Kind ?? SeatRoomTraitKind.Advantage,
                    Text = t.Text ?? "",
                    SortOrder = t.SortOrder,
                }).ToList(),
        };

        private static SeatPurposeTemplateDTO ToDTO(SeatPurposeTemplate e) => new()
        {
            Id = e.Id,
            Name = e.Name ?? "",
            Description = e.Description ?? "",
            MinSizeCategory = e.MinSizeCategory ?? SeatRoomSizeCategory.Small,
            WhoOccupies = e.WhoOccupies ?? "",
            SleepCapacity = e.SleepCapacity,
            AdditivePrestige = e.AdditivePrestige,
            AdditiveHonor = e.AdditiveHonor,
            AdditiveFear = e.AdditiveFear,
            Additive = De(e.AdditiveJson),
            Percent = De(e.PercentJson),
            IsUniversal = e.IsUniversal,
            BaronyId = e.BaronyId,
            SortOrder = e.SortOrder,
        };

        private static void ApplySeat(BaronySeat e, BaronySeatDTO d)
        {
            e.BaronyId = d.BaronyId;
            e.Name = string.IsNullOrWhiteSpace(d.Name) ? "Lord's Seat" : d.Name.Trim();
            e.GridWidth = Math.Max(1, d.GridWidth);
            e.GridHeight = Math.Max(1, d.GridHeight);
            // Active levels are managed via SaveSeatActiveLevels.
        }

        private static void ApplySeatRoom(SeatRoom e, SeatRoomDTO d)
        {
            e.SeatId = d.SeatId;
            e.Name = d.Name ?? "";
            e.Level = SeatFloorLevel.Clamp(d.Level);
            e.GridX = Math.Max(0, d.GridX);
            e.GridY = Math.Max(0, d.GridY);
            e.GridW = Math.Max(0, d.GridW);
            e.GridH = Math.Max(0, d.GridH);
            e.Material = string.IsNullOrWhiteSpace(d.Material) ? SeatRoomMaterial.Stone : d.Material;
            e.PrestigeMultiplier = d.PrestigeMultiplier <= 0 ? 1m : d.PrestigeMultiplier;
            e.Status = string.IsNullOrWhiteSpace(d.Status) ? SeatRoomStatus.Active : d.Status;
            e.AdditiveJson = Ser(d.Additive);
            e.PercentJson = Ser(d.Percent);
            e.PurposeTemplateId = d.PurposeTemplateId;
            e.OccupantAdvisorId = d.OccupantAdvisorId;
            e.OccupantCustom = d.OccupantCustom ?? "";
            e.SortOrder = d.SortOrder;
        }

        private static List<int> ParseActiveLevels(string? json)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(json))
                {
                    var parsed = JsonSerializer.Deserialize<List<int>>(json);
                    if (parsed is { Count: > 0 })
                        return NormalizeActiveLevels(parsed);
                }
            }
            catch
            {
                // fall through
            }

            return new List<int> { SeatFloorLevel.Ground };
        }

        private static List<int> NormalizeActiveLevels(IEnumerable<int>? levels)
        {
            var set = new SortedSet<int>();
            foreach (var level in levels ?? Enumerable.Empty<int>())
            {
                if (SeatFloorLevel.IsValid(level))
                    set.Add(level);
            }

            if (set.Count == 0)
                set.Add(SeatFloorLevel.Ground);

            return set.ToList();
        }

        private static void ApplyPurposeTemplate(SeatPurposeTemplate e, SeatPurposeTemplateDTO d)
        {
            e.Name = d.Name ?? "";
            e.Description = d.Description ?? "";
            e.MinSizeCategory = string.IsNullOrWhiteSpace(d.MinSizeCategory)
                ? SeatRoomSizeCategory.Small
                : d.MinSizeCategory;
            e.WhoOccupies = d.WhoOccupies ?? "";
            e.SleepCapacity = Math.Max(0, d.SleepCapacity);
            e.AdditivePrestige = d.AdditivePrestige;
            e.AdditiveHonor = d.AdditiveHonor;
            e.AdditiveFear = d.AdditiveFear;
            e.AdditiveJson = Ser(d.Additive);
            e.PercentJson = Ser(d.Percent);
            e.IsUniversal = d.IsUniversal;
            e.BaronyId = d.IsUniversal ? null : d.BaronyId;
            e.SortOrder = d.SortOrder;
        }
    }
}
