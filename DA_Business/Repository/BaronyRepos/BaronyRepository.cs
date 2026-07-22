using System.Text.Json;
using DA_Business.Repository.CharacterReps.IRepository;
using DA_Common;
using DA_Common.Barony;
using DA_DataAccess.BaronyData;
using DA_DataAccess.CharacterClasses;
using DA_DataAccess.Data;
using DA_Models.BaronyModels;
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
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

        public BaronyRepository(IDbContextFactory<ApplicationDbContext> db)
        {
            _db = db;
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

        public async Task<BaronyDTO> CreateForCharacter(int characterId, string name, string? notes = null)
        {
            try
            {
                using var ctx = await _db.CreateDbContextAsync();

                var character = await ctx.Characters.AsNoTracking()
                    .FirstOrDefaultAsync(c => c.Id == characterId);
                if (character is null)
                    throw new RepositoryErrorException("Character not found.");
                if (character.NPCType != SD.NPCType.Duke)
                    throw new RepositoryErrorException("Only Duke-type characters can be assigned as baron.");
                if (!character.IsApproved)
                    throw new RepositoryErrorException("Character must be approved before becoming a baron.");

                var existing = await ctx.Baronies.FirstOrDefaultAsync(b => b.CharacterId == characterId);
                if (existing is not null)
                    throw new RepositoryErrorException("This character already has a barony.");

                var entity = new Barony
                {
                    CharacterId = characterId,
                    Name = string.IsNullOrWhiteSpace(name) ? "Nowa Baronia" : name.Trim(),
                    Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim(),
                    BaseParametersJson = Ser(new PpbVector()),
                    ResourceStocksJson = Ser(new PpbVector()),
                    PreviousTurnIncomeJson = Ser(new PpbVector()),
                };
                var added = await ctx.Baronies.AddAsync(entity);
                await ctx.SaveChangesAsync();

                SeedDefaultAdvisors(ctx, added.Entity.Id, character.NPCName ?? "Baron");
                SeedTerrainGrid(ctx, added.Entity.Id);
                SeedPrimaryMapDomain(ctx, added.Entity.Id, added.Entity.Name, character.NPCName ?? "Baron");
                SeedPrimaryFief(ctx, added.Entity.Id, character.NPCName ?? "Baron");
                SeniorHousesSeeder.EnsureForBarony(ctx, added.Entity.Id);
                OrganizationsSeeder.EnsureForBarony(ctx, added.Entity.Id);
                VassalsFromFiefsSeeder.EnsureForBarony(ctx, added.Entity.Id);
                await ctx.SaveChangesAsync();

                return ToDTO(added.Entity);
            }
            catch (RepositoryErrorException) { throw; }
            catch (System.Exception ex) { throw Err(ex, nameof(CreateForCharacter)); }
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
                    .Select(e => ToImprovementDto(e, tiles, taxRates))
                    .ToList();

                VassalsFromFiefsSeeder.EnsureForBarony(ctx, baronyId);
                SeniorHousesSeeder.EnsureForBarony(ctx, baronyId);
                await ctx.SaveChangesAsync();

                return new BaronyOverviewDTO
                {
                    Barony = ToDTO(barony),
                    Advisors = (await ctx.Advisors.AsNoTracking().Where(x => x.BaronyId == baronyId).ToListAsync()).Select(ToDTO).ToList(),
                    Buildings = (await ctx.BaronyBuildings.AsNoTracking().Where(x => x.BaronyId == baronyId).ToListAsync()).Select(ToDTO).ToList(),
                    SocialRelations = (await ctx.SocialGroupRelations.AsNoTracking().Where(x => x.BaronyId == baronyId).ToListAsync()).Select(ToDTO).ToList(),
                    Improvements = improvementDtos,
                    Decrees = (await ctx.Decrees.AsNoTracking().Where(x => x.BaronyId == baronyId).ToListAsync()).Select(ToDTO).ToList(),
                    Events = (await ctx.BaronyEvents.AsNoTracking().Where(x => x.BaronyId == baronyId).ToListAsync()).Select(ToDTO).ToList(),
                    Relations = (await ctx.BaronyRelations.AsNoTracking().Include(x => x.Modifiers).Where(x => x.BaronyId == baronyId).ToListAsync()).Select(ToDTO).ToList(),
                    Seat = await EnsureSeatDtoAsync(ctx, baronyId),
                    SeatPurposeTemplates = await LoadPurposeTemplatesAsync(ctx, baronyId),
                    CommunityModifiers = (await ctx.CommunityModifiers.AsNoTracking().Where(x => x.BaronyId == baronyId).ToListAsync()).Select(ToDTO).ToList(),
                    Fiefs = (await ctx.Fiefs.AsNoTracking().Where(x => x.BaronyId == baronyId).ToListAsync()).Select(ToDTO).ToList(),
                    Tiles = tiles.Select(ToDTO).ToList(),
                    Projects = (await ctx.BaronyProjects.AsNoTracking().Where(x => x.BaronyId == baronyId).ToListAsync()).Select(ToDTO).ToList(),
                    ResourceSources = (await ctx.BaronyResourceSources.AsNoTracking().Where(x => x.BaronyId == baronyId).ToListAsync()).Select(ToDTO).ToList(),
                    PurseSources = (await ctx.BaronPurseSources.AsNoTracking().Where(x => x.BaronyId == baronyId).ToListAsync()).Select(ToDTO).ToList(),
                };
            }
            catch (System.Exception ex) { throw Err(ex, nameof(GetOverview)); }
        }

        // ---------------- Advisors ----------------
        public async Task<List<AdvisorDTO>> GetAdvisors(int baronyId) =>
            await GetList(ctx => ctx.Advisors, baronyId, ToDTO, nameof(GetAdvisors));

        public async Task<List<AvailableAdvisorDTO>> GetAvailableAdvisors(int baronyId) =>
            await GetList(ctx => ctx.AvailableAdvisors, baronyId, ToDTO, nameof(GetAvailableAdvisors));

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
                    e = ToEntity(dto);
                    ctx.AvailableAdvisors.Add(e);
                }
                else
                {
                    ApplyAvailableAdvisor(e, dto);
                }
                await ctx.SaveChangesAsync();
                return ToDTO(e);
            }
            catch (System.Exception ex) { throw Err(ex, nameof(SaveAvailableAdvisor)); }
        }

        public Task<int> DeleteAvailableAdvisor(int id) =>
            Delete(ctx => ctx.AvailableAdvisors, id, nameof(DeleteAvailableAdvisor));

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
                else { ApplyDecree(e, dto); }
                await ctx.SaveChangesAsync();
                return ToDTO(e);
            }
            catch (System.Exception ex) { throw Err(ex, nameof(SaveDecree)); }
        }

        public Task<int> DeleteDecree(int id) => Delete(ctx => ctx.Decrees, id, nameof(DeleteDecree));

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
                VassalsFromFiefsSeeder.EnsureForBarony(ctx, baronyId);
                SeniorHousesSeeder.EnsureForBarony(ctx, baronyId);
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

        // ---------------- Baron time (JC) ----------------
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
                        + $"{BaronTimeRules.RequiredManagementJc} JC causes management penalties "
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
                    .OrderByDescending(t => t.UpdatedAtUtc)
                    .ThenByDescending(t => t.Id)
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
                    ApplyLetterMessage(e, dto);
                    e.UpdatedAtUtc = now;
                }

                var thread = await ctx.BaronLetterThreads.FirstOrDefaultAsync(t => t.Id == e.ThreadId);
                if (thread is not null)
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
                VassalsFromFiefsSeeder.EnsureForBarony(ctx, e.BaronyId);
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
                var existing = await ctx.TerrainTiles
                    .Where(x => x.BaronyId == baronyId)
                    .ToListAsync();
                var existingCoords = existing.Select(t => (t.X, t.Y)).ToHashSet();
                var added = false;

                for (var y = 0; y < TerrainMapGrid.Size; y++)
                {
                    for (var x = 0; x < TerrainMapGrid.Size; x++)
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

                if (added)
                    await ctx.SaveChangesAsync();

                return await ctx.TerrainTiles.AsNoTracking()
                    .Where(x => x.BaronyId == baronyId)
                    .Select(x => ToDTO(x))
                    .ToListAsync();
            }
            catch (System.Exception ex) { throw Err(ex, nameof(EnsureTerrainGrid)); }
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
                return entities.Select(e => ToImprovementDto(e, tiles, taxRates)).ToList();
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
                return ToImprovementDto(e, tile, taxRates);
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
                if (e is null) { e = ToEntity(dto); ctx.BaronyProjects.Add(e); }
                else { ApplyProject(e, dto); }
                await ctx.SaveChangesAsync();
                return ToDTO(e);
            }
            catch (System.Exception ex) { throw Err(ex, nameof(SaveProject)); }
        }

        public Task<int> DeleteProject(int id) => Delete(ctx => ctx.BaronyProjects, id, nameof(DeleteProject));

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
                if (project.Status is ProjectStatus.Completed or ProjectStatus.Cancelled)
                    throw new InvalidOperationException("This project cannot accept resources.");

                var barony = await ctx.Baronies.FirstOrDefaultAsync(x => x.Id == project.BaronyId)
                    ?? throw new InvalidOperationException("Barony not found.");

                var dto = ToDTO(project);
                var stocks = ResourceCatalog.Slice(De(barony.ResourceStocksJson));
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
                if (dto.Status != ProjectStatus.Draft)
                    throw new InvalidOperationException("Only draft projects can have allocations cleared.");
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

            const string chancellorDescription =
                "One of the barony's most important offices. The chancellor manages relations between the ruler " +
                "and both vassals and liege lords, reads the loyalty and mood of subjects toward the government, " +
                "and shapes opinion by many means. The office also oversees cultural development. A chancellor may " +
                "rule through love—easing conflicts and appealing to reason—through fear, threats, and harsh " +
                "punishment for disobedience, or through a blend of both approaches.";

            const string guardCaptainDescription =
                "The Guard Captain is essential in the smallest administrative units. Keeper of the law, " +
                "commander, and guardian of the baron's person and lands. Later, most of these duties pass to " +
                "the general, border warden, chief judge, and others—but until the barony grows into a " +
                "principality, the Guard Captain alone can handle them.";

            const string stewardDescription =
                "The Steward oversees everything tied to revenue, construction, provisions, and tax collection. " +
                "From harvests and storehouses to new works and the flow of coin into the treasury, this office " +
                "keeps the barony fed, built, and solvent.";

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
            for (var y = 0; y < TerrainMapGrid.Size; y++)
            {
                for (var x = 0; x < TerrainMapGrid.Size; x++)
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
                Year = e.Year,
                Month = e.Month,
                TurnNumber = e.TurnNumber,
                Season = e.Season,
                TreasuryGold = e.TreasuryGold,
                BaronPurseGold = e.BaronPurseGold,
                FoodInGranaries = e.FoodInGranaries,
                ResourceStocks = stocks,
                PreviousTurnIncome = De(e.PreviousTurnIncomeJson),
                Unrest = e.Unrest,
                Prestige = e.Prestige,
                Honor = e.Honor,
                Fear = e.Fear,
                BaseParameters = De(e.BaseParametersJson),
                Notes = e.Notes,
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
            e.Year = d.Year;
            e.Month = d.Month;
            e.TurnNumber = d.TurnNumber;
            e.Season = d.Season;
            e.BaronPurseGold = d.BaronPurseGold;
            e.Unrest = d.Unrest;
            e.Prestige = d.Prestige;
            e.Honor = d.Honor;
            e.Fear = d.Fear;
            e.BaseParametersJson = Ser(d.BaseParameters);
            e.Notes = d.Notes;

            var stocks = ResourceCatalog.Slice(d.ResourceStocks);
            // Keep Food/Gold scalars and vector in sync (Budget may update scalars only).
            stocks[Ppb.Food] = d.FoodInGranaries;
            stocks[Ppb.Treasury] = d.TreasuryGold;
            e.FoodInGranaries = stocks[Ppb.Food];
            e.TreasuryGold = stocks[Ppb.Treasury];
            e.ResourceStocksJson = Ser(stocks);
            e.PreviousTurnIncomeJson = Ser(ResourceCatalog.Slice(d.PreviousTurnIncome));
        }

        // ---------------- Mapping: Advisor ----------------
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
            e.FormulaText = d.FormulaText; e.Description = d.Description; e.UpkeepGold = d.UpkeepGold;
        }

        private static AvailableAdvisorDTO ToDTO(AvailableAdvisor e) => new()
        {
            Id = e.Id,
            BaronyId = e.BaronyId,
            Name = e.Name,
            Description = e.Description,
            Skills = De(e.SkillsJson),
        };

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
            e.Name = d.Name;
            e.Description = d.Description;
            e.SkillsJson = Ser(d.Skills);
        }

        // ---------------- Mapping: Building ----------------
        private static BaronyBuildingDTO ToDTO(BaronyBuilding e) => new()
        {
            Id = e.Id, BaronyId = e.BaronyId, TemplateId = e.TemplateId, Name = e.Name, Kind = e.Kind,
            Additive = De(e.AdditiveJson), Percent = De(e.PercentJson), Description = e.Description,
        };

        private static BaronyBuilding ToEntity(BaronyBuildingDTO d) { var e = new BaronyBuilding(); ApplyBuilding(e, d); e.Id = d.Id; return e; }

        private static void ApplyBuilding(BaronyBuilding e, BaronyBuildingDTO d)
        {
            e.BaronyId = d.BaronyId; e.TemplateId = d.TemplateId; e.Name = d.Name; e.Kind = d.Kind;
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
            Season = e.Season ?? "Winter",
            SeenByBaron = e.SeenByBaron,
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
            e.Season = string.IsNullOrWhiteSpace(d.Season) ? "Winter" : d.Season;
            e.SeenByBaron = d.SeenByBaron;
            e.SortOrder = d.SortOrder;
            e.SentAtUtc = d.SentAtUtc;
            if (d.CreatedAtUtc != default)
                e.CreatedAtUtc = d.CreatedAtUtc;
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
            Id = e.Id, BaronyId = e.BaronyId, MapId = e.MapId, X = e.X, Y = e.Y, BaseType = e.BaseType,
            FeaturesMask = e.FeaturesMask, Fertility = e.Fertility, Resource = e.Resource,
            FiefId = e.FiefId, MapDomainId = e.MapDomainId, Comment = e.Comment,
        };

        private static TerrainTile ToEntity(TerrainTileDTO d) { var e = new TerrainTile(); ApplyTile(e, d); e.Id = d.Id; return e; }

        private static void ApplyTile(TerrainTile e, TerrainTileDTO d)
        {
            e.BaronyId = d.BaronyId; e.MapId = d.MapId; e.X = d.X; e.Y = d.Y; e.BaseType = d.BaseType;
            e.FeaturesMask = d.FeaturesMask; e.Fertility = d.Fertility; e.Resource = d.Resource;
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

            ApplySettlementFormulas(dto, fertility, taxRates);
        }

        private static void ApplySettlementFormulas(
            TerrainImprovementDTO dto,
            int fertility,
            TownTaxRates? taxRates = null)
        {
            var taxes = taxRates ?? TownTaxRates.Defaults;
            if (IsVillage(dto.Name))
            {
                dto.Additive = VillagePpbFormulas.Compute(dto.Population, fertility, dto.HasPalisade, taxes);
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
            TownTaxRates taxRates)
        {
            TerrainTile? tile = null;
            if (e.TileId is int tid)
                tile = tiles.FirstOrDefault(t => t.Id == tid);
            return ToImprovementDto(e, tile, taxRates);
        }

        private static TerrainImprovementDTO ToImprovementDto(
            TerrainImprovement e,
            TerrainTile? tile,
            TownTaxRates taxRates)
        {
            var dto = ToDTO(e);
            ApplySettlementFormulas(dto, tile?.Fertility ?? TerrainFertility.Unknown, taxRates);
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
                Status = e.Status,
                TurnsRemaining = e.TurnsRemaining,
                Notes = e.Notes,
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
            e.Status = d.Status;
            e.TurnsRemaining = d.TurnsRemaining;
            e.Notes = d.Notes;
        }

        private static PpbVector MergeLegacyCost(PpbVector goldProduction, PpbVector materials)
        {
            var merged = ProjectCostCatalog.SliceGoldProduction(goldProduction);
            foreach (var info in ProjectCostCatalog.Materials)
                merged[info.Key] = materials[info.Key];
            return merged;
        }

        // ---------------- Mapping: Resource source ----------------
        private static BaronyResourceSourceDTO ToDTO(BaronyResourceSource e) => new()
        {
            Id = e.Id,
            BaronyId = e.BaronyId,
            Name = e.Name,
            Description = e.Description,
            Additive = De(e.AdditiveJson),
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
            Id = e.Id, Name = e.Name, RequiredLordshipLevel = e.RequiredLordshipLevel, Kind = e.Kind,
            GoldCost = e.GoldCost, ProductionCost = e.ProductionCost,
            EffectAdditive = De(e.EffectAdditiveJson), EffectPercent = De(e.EffectPercentJson),
            Description = e.Description, TerrainRequirement = e.TerrainRequirement,
        };

        private static BuildingTemplate ToEntity(BuildingTemplateDTO d) { var e = new BuildingTemplate(); ApplyTemplate(e, d); e.Id = d.Id; return e; }

        private static void ApplyTemplate(BuildingTemplate e, BuildingTemplateDTO d)
        {
            e.Name = d.Name; e.RequiredLordshipLevel = d.RequiredLordshipLevel; e.Kind = d.Kind;
            e.GoldCost = d.GoldCost; e.ProductionCost = d.ProductionCost;
            e.EffectAdditiveJson = Ser(d.EffectAdditive); e.EffectPercentJson = Ser(d.EffectPercent);
            e.Description = d.Description; e.TerrainRequirement = d.TerrainRequirement;
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
            e.AdditiveJson = Ser(d.Additive);
            e.PercentJson = Ser(d.Percent);
            e.IsUniversal = d.IsUniversal;
            e.BaronyId = d.IsUniversal ? null : d.BaronyId;
            e.SortOrder = d.SortOrder;
        }
    }
}
