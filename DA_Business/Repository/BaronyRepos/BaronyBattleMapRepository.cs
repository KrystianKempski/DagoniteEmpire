using DA_Business.Repository.CharacterReps.IRepository;
using DA_DataAccess.BaronyData;
using DA_DataAccess.Data;
using DA_Models.BaronyModels;
using DagoniteEmpire.Exceptions;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace DA_Business.Repository.BaronyRepos
{
    public sealed class BaronyBattleMapRepository : IBaronyBattleMapRepository
    {
        public const int FixedWidth = 20;
        public const int FixedHeight = 16;

        private readonly IDbContextFactory<ApplicationDbContext> _db;
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

        public BaronyBattleMapRepository(IDbContextFactory<ApplicationDbContext> db)
        {
            _db = db;
        }

        public async Task<BaronyBattleMapDTO> GetOrCreate(int baronyId)
        {
            try
            {
                using var ctx = await _db.CreateDbContextAsync();
                var obj = await ctx.BaronyBattleMaps.AsNoTracking()
                    .FirstOrDefaultAsync(u => u.BaronyId == baronyId);
                if (obj is not null)
                    return ToDTO(obj);

                var entity = new BaronyBattleMap
                {
                    BaronyId = baronyId,
                    IsActive = false,
                    Phase = BaronyBattlePhases.Setup,
                    Width = FixedWidth,
                    Height = FixedHeight,
                };
                var added = await ctx.BaronyBattleMaps.AddAsync(entity);
                await ctx.SaveChangesAsync();
                return ToDTO(added.Entity);
            }
            catch (System.Exception ex)
            {
                throw new RepositoryErrorException("Error in " + nameof(GetOrCreate), ex);
            }
        }

        public async Task<BaronyBattleMapDTO> Update(BaronyBattleMapDTO dto)
        {
            try
            {
                using var ctx = await _db.CreateDbContextAsync();
                var obj = await ctx.BaronyBattleMaps.FirstOrDefaultAsync(u => u.Id == dto.Id)
                          ?? await ctx.BaronyBattleMaps.FirstOrDefaultAsync(u => u.BaronyId == dto.BaronyId);

                if (obj is null)
                {
                    obj = ToEntity(dto);
                    obj.Width = FixedWidth;
                    obj.Height = FixedHeight;
                    var added = ctx.BaronyBattleMaps.Add(obj);
                    await ctx.SaveChangesAsync();
                    return ToDTO(added.Entity);
                }

                obj.IsActive = dto.IsActive;
                obj.Phase = string.IsNullOrWhiteSpace(dto.Phase) ? BaronyBattlePhases.Setup : dto.Phase;
                obj.Width = FixedWidth;
                obj.Height = FixedHeight;
                TrimToSize(dto);
                obj.CellsJson = JsonSerializer.Serialize(dto.Cells ?? new(), JsonOptions);
                obj.TokensJson = JsonSerializer.Serialize(dto.Tokens ?? new(), JsonOptions);
                obj.TurnStateJson = JsonSerializer.Serialize(dto.TurnState ?? new(), JsonOptions);
                obj.LogJson = JsonSerializer.Serialize(dto.Log ?? new(), JsonOptions);
                await ctx.SaveChangesAsync();
                return ToDTO(obj);
            }
            catch (System.Exception ex)
            {
                throw new RepositoryErrorException("Error in " + nameof(Update) + ": " + ex.Message, ex);
            }
        }

        public async Task<bool> IsActive(int baronyId)
        {
            try
            {
                using var ctx = await _db.CreateDbContextAsync();
                return await ctx.BaronyBattleMaps.AsNoTracking()
                    .AnyAsync(u => u.BaronyId == baronyId && u.IsActive);
            }
            catch
            {
                return false;
            }
        }

        public async Task SetActive(int baronyId, bool active)
        {
            var map = await GetOrCreate(baronyId);
            map.IsActive = active;
            await Update(map);
        }

        private static BaronyBattleMapDTO ToDTO(BaronyBattleMap entity)
        {
            var dto = new BaronyBattleMapDTO
            {
                Id = entity.Id,
                BaronyId = entity.BaronyId,
                IsActive = entity.IsActive,
                Phase = string.IsNullOrWhiteSpace(entity.Phase) ? BaronyBattlePhases.Setup : entity.Phase,
                Width = FixedWidth,
                Height = FixedHeight,
                Cells = Deserialize<List<BaronyBattleCellDTO>>(entity.CellsJson) ?? new(),
                Tokens = Deserialize<List<BaronyBattleTokenDTO>>(entity.TokensJson) ?? new(),
                TurnState = Deserialize<BaronyBattleTurnStateDTO>(entity.TurnStateJson) ?? new(),
                Log = Deserialize<List<BaronyBattleLogEntryDTO>>(entity.LogJson) ?? new(),
            };
            TrimToSize(dto);
            return dto;
        }

        private static void TrimToSize(BaronyBattleMapDTO dto)
        {
            dto.Width = FixedWidth;
            dto.Height = FixedHeight;
            dto.Cells?.RemoveAll(c => c.X < 0 || c.Y < 0 || c.X >= FixedWidth || c.Y >= FixedHeight);
            if (dto.Tokens is null)
                return;
            foreach (var token in dto.Tokens)
            {
                int size = Math.Clamp(token.Size <= 0 ? 1 : token.Size, 1, 3);
                token.X = Math.Clamp(token.X, 0, Math.Max(0, FixedWidth - size));
                token.Y = Math.Clamp(token.Y, 0, Math.Max(0, FixedHeight - size));
            }
        }

        private static BaronyBattleMap ToEntity(BaronyBattleMapDTO dto) => new()
        {
            Id = dto.Id,
            BaronyId = dto.BaronyId,
            IsActive = dto.IsActive,
            Phase = string.IsNullOrWhiteSpace(dto.Phase) ? BaronyBattlePhases.Setup : dto.Phase,
            Width = FixedWidth,
            Height = FixedHeight,
            CellsJson = JsonSerializer.Serialize(dto.Cells ?? new(), JsonOptions),
            TokensJson = JsonSerializer.Serialize(dto.Tokens ?? new(), JsonOptions),
            TurnStateJson = JsonSerializer.Serialize(dto.TurnState ?? new(), JsonOptions),
            LogJson = JsonSerializer.Serialize(dto.Log ?? new(), JsonOptions),
        };

        private static T? Deserialize<T>(string? json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return default;
            try { return JsonSerializer.Deserialize<T>(json, JsonOptions); }
            catch { return default; }
        }
    }
}
