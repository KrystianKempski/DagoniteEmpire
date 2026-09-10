using DA_Business.Repository.CharacterReps.IRepository;
using DA_Business.Services.Interfaces;
using DA_Common.Barony;
using DA_Common.Notifications;
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
        private readonly IGameNotificationQueue _notifications;
        private readonly IBaronyLogService _log;
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

        public BaronyBattleMapRepository(
            IDbContextFactory<ApplicationDbContext> db,
            IGameNotificationQueue notifications,
            IBaronyLogService log)
        {
            _db = db;
            _notifications = notifications;
            _log = log;
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

                // Captured before the overwrite so we can tell a real turn change from the many
                // saves that only move tokens around.
                var previousPhase = obj.Phase;
                var previousTurnState = DeserializeTurnState(obj.TurnStateJson);
                var previousActive = obj.IsActive;

                obj.IsActive = dto.IsActive;
                obj.Phase = string.IsNullOrWhiteSpace(dto.Phase) ? BaronyBattlePhases.Setup : dto.Phase;
                obj.Width = FixedWidth;
                obj.Height = FixedHeight;
                TrimToSize(dto);
                obj.CellsJson = JsonSerializer.Serialize(dto.Cells ?? new(), JsonOptions);
                obj.TokensJson = JsonSerializer.Serialize(dto.Tokens ?? new(), JsonOptions);
                obj.TurnStateJson = JsonSerializer.Serialize(dto.TurnState ?? new(), JsonOptions);
                obj.LogJson = JsonSerializer.Serialize(dto.Log ?? new(), JsonOptions);
                obj.TalliesJson = JsonSerializer.Serialize(dto.Tallies ?? new(), JsonOptions);
                obj.XpSummaryJson = JsonSerializer.Serialize(dto.XpSummary, JsonOptions);
                await ctx.SaveChangesAsync();

                await ChronicleBattleMilestones(obj.BaronyId, dto, previousActive, previousPhase, previousTurnState);
                NotifyIfTurnAdvanced(dto, previousPhase, previousTurnState);

                return ToDTO(obj);
            }
            catch (System.Exception ex)
            {
                throw new RepositoryErrorException("Error in " + nameof(Update) + ": " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Only battle milestones reach the chronicle. Every token drag saves the whole map, so
        /// logging each save would bury the turn in noise.
        /// </summary>
        private async Task ChronicleBattleMilestones(
            int baronyId,
            BaronyBattleMapDTO dto,
            bool previousActive,
            string? previousPhase,
            BaronyBattleTurnStateDTO? previousTurnState)
        {
            if (previousActive != dto.IsActive)
            {
                await _log.Log(
                    baronyId, BaronyLogCategory.Battle,
                    dto.IsActive ? "Battle started." : "Battle ended.",
                    important: true);
                return;
            }

            if (!dto.IsActive)
                return;

            if (!string.Equals(previousPhase, dto.Phase, StringComparison.Ordinal))
            {
                await _log.Log(
                    baronyId, BaronyLogCategory.Battle,
                    $"Battle phase: {previousPhase} → {dto.Phase}.");
                return;
            }

            var round = dto.TurnState?.Round ?? 0;
            if (round > 0 && round != (previousTurnState?.Round ?? 0))
                await _log.Log(baronyId, BaronyLogCategory.Battle, $"Battle round {round} begins.");
        }

        private static BaronyBattleTurnStateDTO? DeserializeTurnState(string? json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return null;

            try
            {
                return JsonSerializer.Deserialize<BaronyBattleTurnStateDTO>(json, JsonOptions);
            }
            catch (JsonException)
            {
                return null;
            }
        }

        /// <summary>
        /// Raises a "your move" notification only when the acting side really changed. Dragging a
        /// token saves the whole map, so without comparing the turn pointer every drag would ping
        /// somebody's phone.
        /// </summary>
        private void NotifyIfTurnAdvanced(
            BaronyBattleMapDTO dto,
            string? previousPhase,
            BaronyBattleTurnStateDTO? previous)
        {
            var state = dto.TurnState;
            if (state is null || dto.Phase != BaronyBattlePhases.Battle)
                return;

            // Combat damage is resolved by the Game Master in one step; nobody is prompted for input.
            if (state.SubPhase == BaronyBattleSubPhases.Combat)
                return;

            var pointerUnchanged = previous is not null
                && previousPhase == dto.Phase
                && previous.SubPhase == state.SubPhase
                && previous.Round == state.Round
                && previous.CurrentIndex == state.CurrentIndex;
            if (pointerUnchanged)
                return;

            // Movement goes unit by unit; attack planning opens for all of the baron's units at once.
            BaronyBattleTokenDTO? active = null;
            if (state.SubPhase == BaronyBattleSubPhases.Movement)
            {
                if (state.CurrentIndex < 0 || state.CurrentIndex >= state.InitiativeOrder.Count)
                    return;

                var activeId = state.InitiativeOrder[state.CurrentIndex];
                active = dto.Tokens?.FirstOrDefault(t => t.Id == activeId);
                if (active is null)
                    return;
            }

            _notifications.Enqueue(new BattleTurnAdvanced(
                dto.BaronyId,
                state.Round,
                state.SubPhase,
                active?.Label,
                active?.IsEnemy ?? false));
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
                Tallies = Deserialize<List<BaronyBattleXpTallyDTO>>(entity.TalliesJson) ?? new(),
                XpSummary = Deserialize<BaronyBattleXpSummaryDTO>(entity.XpSummaryJson),
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
            TalliesJson = JsonSerializer.Serialize(dto.Tallies ?? new(), JsonOptions),
            XpSummaryJson = JsonSerializer.Serialize(dto.XpSummary, JsonOptions),
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
