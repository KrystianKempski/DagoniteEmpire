using DA_Business.Services.Interfaces;
using DA_Common;
using DA_Common.Barony;
using DA_DataAccess.BaronyData;
using DA_DataAccess.Data;
using DA_Models.BaronyModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DA_Business.Services
{
    /// <inheritdoc cref="IBaronyLogService"/>
    public class BaronyLogService : IBaronyLogService
    {
        private const int SummaryMaxLength = 400;

        private readonly IDbContextFactory<ApplicationDbContext> _db;
        private readonly IUserService _userService;
        private readonly ILogger<BaronyLogService> _logger;

        private (string Actor, string Role)? _actor;

        public BaronyLogService(
            IDbContextFactory<ApplicationDbContext> db,
            IUserService userService,
            ILogger<BaronyLogService> logger)
        {
            _db = db;
            _userService = userService;
            _logger = logger;
        }

        public async Task Log(
            int baronyId,
            string category,
            string summary,
            string? details = null,
            string? entityType = null,
            int? entityId = null,
            bool important = false)
        {
            if (baronyId <= 0 || string.IsNullOrWhiteSpace(summary))
                return;

            try
            {
                using var ctx = await _db.CreateDbContextAsync();
                var barony = await ctx.Baronies.AsNoTracking().FirstOrDefaultAsync(b => b.Id == baronyId);
                if (barony is null)
                    return;

                ctx.BaronyLogEntries.Add(await BuildEntry(
                    barony, category, summary, details, entityType, entityId, important));
                await ctx.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Barony chronicle entry skipped for barony {BaronyId}.", baronyId);
            }
        }

        public async Task Attach(
            ApplicationDbContext ctx,
            Barony barony,
            string category,
            string summary,
            string? details = null,
            string? entityType = null,
            int? entityId = null,
            bool important = false,
            BaronyLogStamp? stamp = null)
        {
            if (barony is null || string.IsNullOrWhiteSpace(summary))
                return;

            try
            {
                ctx.BaronyLogEntries.Add(await BuildEntry(
                    barony, category, summary, details, entityType, entityId, important, stamp));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Barony chronicle entry skipped for barony {BaronyId}.", barony.Id);
            }
        }

        public async Task<IReadOnlyList<BaronyLogTurnDTO>> GetTurns(int baronyId)
        {
            using var ctx = await _db.CreateDbContextAsync();
            var turns = await ctx.BaronyLogEntries.AsNoTracking()
                .Where(e => e.BaronyId == baronyId)
                .GroupBy(e => e.TurnNumber)
                .Select(g => new
                {
                    TurnNumber = g.Key,
                    Count = g.Count(),
                    Year = g.Max(e => e.Year),
                    Month = g.Max(e => e.Month),
                    Season = g.Max(e => e.Season),
                })
                .OrderByDescending(g => g.TurnNumber)
                .ToListAsync();

            return turns.Select(t => new BaronyLogTurnDTO
            {
                TurnNumber = t.TurnNumber,
                Year = t.Year,
                Month = t.Month,
                Season = t.Season ?? string.Empty,
                EntryCount = t.Count,
            }).ToList();
        }

        public async Task<IReadOnlyList<BaronyLogEntryDTO>> GetEntries(
            int baronyId, int turnNumber, BaronyLogFilterDTO? filter = null)
        {
            using var ctx = await _db.CreateDbContextAsync();
            var query = ctx.BaronyLogEntries.AsNoTracking()
                .Where(e => e.BaronyId == baronyId && e.TurnNumber == turnNumber);

            if (filter is not null)
            {
                if (filter.Categories.Count > 0)
                    query = query.Where(e => filter.Categories.Contains(e.Category));
                if (!string.IsNullOrWhiteSpace(filter.Actor))
                    query = query.Where(e => e.Actor == filter.Actor);
                if (filter.ImportantOnly)
                    query = query.Where(e => e.IsImportant);
            }

            var entries = await query
                .OrderBy(e => e.CreatedAtUtc)
                .ThenBy(e => e.Id)
                .ToListAsync();

            // Free-text search runs in memory: a single turn holds few entries and this keeps the
            // query provider-agnostic (ILike is PostgreSQL-only).
            if (!string.IsNullOrWhiteSpace(filter?.Search))
            {
                var needle = filter.Search.Trim();
                entries = entries
                    .Where(e => e.Summary.Contains(needle, StringComparison.OrdinalIgnoreCase)
                                || (e.Details?.Contains(needle, StringComparison.OrdinalIgnoreCase) ?? false))
                    .ToList();
            }

            return entries.Select(ToDTO).ToList();
        }

        public async Task<IReadOnlyList<string>> GetActors(int baronyId)
        {
            using var ctx = await _db.CreateDbContextAsync();
            return await ctx.BaronyLogEntries.AsNoTracking()
                .Where(e => e.BaronyId == baronyId && e.Actor != "")
                .Select(e => e.Actor)
                .Distinct()
                .OrderBy(a => a)
                .ToListAsync();
        }

        public async Task<bool> Delete(int entryId)
        {
            using var ctx = await _db.CreateDbContextAsync();
            var entry = await ctx.BaronyLogEntries.FirstOrDefaultAsync(e => e.Id == entryId);
            if (entry is null)
                return false;

            ctx.BaronyLogEntries.Remove(entry);
            await ctx.SaveChangesAsync();
            return true;
        }

        private async Task<BaronyLogEntry> BuildEntry(
            Barony barony,
            string category,
            string summary,
            string? details,
            string? entityType,
            int? entityId,
            bool important,
            BaronyLogStamp? stamp = null)
        {
            var (actor, role) = await ResolveActor();
            var text = summary.Trim();
            if (text.Length > SummaryMaxLength)
                text = text[..(SummaryMaxLength - 1)] + "…";

            return new BaronyLogEntry
            {
                BaronyId = barony.Id,
                TurnNumber = stamp?.TurnNumber ?? barony.TurnNumber,
                Year = stamp?.Year ?? barony.Year,
                Month = stamp?.Month ?? barony.Month,
                Season = stamp?.Season ?? barony.Season ?? string.Empty,
                Category = BaronyLogCategory.Normalize(category),
                Actor = actor,
                ActorRole = role,
                Summary = text,
                Details = string.IsNullOrWhiteSpace(details) ? null : details,
                EntityType = entityType,
                EntityId = entityId,
                IsImportant = important,
                CreatedAtUtc = DateTime.UtcNow,
            };
        }

        /// <summary>Resolved once per circuit; reading session storage on every entry would be costly.</summary>
        private async Task<(string Actor, string Role)> ResolveActor()
        {
            if (_actor is { } cached)
                return cached;

            var resolved = (Actor: "System", Role: BaronyLogActorRole.System);
            try
            {
                var user = await _userService.GetUserInfo();
                if (user is not null && !string.IsNullOrWhiteSpace(user.UserName))
                {
                    var role = user.IsAdminOrMG == true
                        ? BaronyLogActorRole.GameMaster
                        : user.Role == SD.Role_DukePlayer
                            ? BaronyLogActorRole.Baron
                            : BaronyLogActorRole.System;
                    resolved = (user.UserName!, role);
                }
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Chronicle actor unresolved; falling back to System.");
            }

            _actor = resolved;
            return resolved;
        }

        private static BaronyLogEntryDTO ToDTO(BaronyLogEntry e) => new()
        {
            Id = e.Id,
            BaronyId = e.BaronyId,
            TurnNumber = e.TurnNumber,
            Year = e.Year,
            Month = e.Month,
            Season = e.Season,
            Category = e.Category,
            Actor = e.Actor,
            ActorRole = e.ActorRole,
            Summary = e.Summary,
            Details = e.Details,
            EntityType = e.EntityType,
            EntityId = e.EntityId,
            IsImportant = e.IsImportant,
            CreatedAtUtc = e.CreatedAtUtc,
        };
    }
}
