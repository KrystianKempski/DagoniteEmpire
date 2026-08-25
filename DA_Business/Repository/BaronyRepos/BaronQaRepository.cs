using DA_DataAccess.BaronyData;
using DA_DataAccess.Data;
using DA_Models.BaronyModels;
using DagoniteEmpire.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace DA_Business.Repository.BaronyRepos
{
    public sealed class BaronQaRepository : IBaronQaRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _db;

        public BaronQaRepository(IDbContextFactory<ApplicationDbContext> db)
        {
            _db = db;
        }

        public async Task<List<BaronQaThreadDTO>> GetThreads(int baronyId)
        {
            try
            {
                using var ctx = await _db.CreateDbContextAsync();
                return await LoadThreadDtosAsync(ctx, baronyId);
            }
            catch (Exception ex) { throw Err(ex, nameof(GetThreads)); }
        }

        public async Task<BaronQaThreadDTO> SaveThread(BaronQaThreadDTO dto)
        {
            try
            {
                using var ctx = await _db.CreateDbContextAsync();
                var now = DateTime.UtcNow;
                BaronQaThread e;
                if (dto.Id > 0)
                {
                    e = await ctx.BaronQaThreads.FirstOrDefaultAsync(t => t.Id == dto.Id)
                        ?? throw new InvalidOperationException("QA thread not found.");
                    e.Title = (dto.Title ?? "").Trim();
                    e.UpdatedAtUtc = now;
                }
                else
                {
                    e = new BaronQaThread
                    {
                        BaronyId = dto.BaronyId,
                        Title = (dto.Title ?? "").Trim(),
                        CreatedTurn = dto.CreatedTurn,
                        CreatedAtUtc = now,
                        UpdatedAtUtc = now,
                    };
                    ctx.BaronQaThreads.Add(e);
                }

                await ctx.SaveChangesAsync();
                return await LoadThreadDtoAsync(ctx, e.Id);
            }
            catch (Exception ex) when (ex is not InvalidOperationException)
            {
                throw Err(ex, nameof(SaveThread));
            }
        }

        public async Task<int> DeleteThread(int threadId)
        {
            try
            {
                using var ctx = await _db.CreateDbContextAsync();
                var e = await ctx.BaronQaThreads.FirstOrDefaultAsync(t => t.Id == threadId);
                if (e is null)
                    return 0;
                ctx.BaronQaThreads.Remove(e);
                await ctx.SaveChangesAsync();
                return 1;
            }
            catch (Exception ex) { throw Err(ex, nameof(DeleteThread)); }
        }

        public async Task<BaronQaMessageDTO> SaveMessage(BaronQaMessageDTO dto)
        {
            try
            {
                using var ctx = await _db.CreateDbContextAsync();
                var thread = await ctx.BaronQaThreads.FirstOrDefaultAsync(t => t.Id == dto.ThreadId)
                    ?? throw new InvalidOperationException("QA thread not found.");

                var now = DateTime.UtcNow;
                BaronQaMessage e;
                if (dto.Id > 0)
                {
                    e = await ctx.BaronQaMessages.FirstOrDefaultAsync(m => m.Id == dto.Id)
                        ?? throw new InvalidOperationException("QA message not found.");
                    e.Body = (dto.Body ?? "").Trim();
                    e.SpeakerName = string.IsNullOrWhiteSpace(dto.SpeakerName) ? null : dto.SpeakerName.Trim();
                }
                else
                {
                    var maxSort = await ctx.BaronQaMessages
                        .Where(m => m.ThreadId == dto.ThreadId)
                        .Select(m => (int?)m.SortOrder)
                        .MaxAsync() ?? 0;
                    e = new BaronQaMessage
                    {
                        ThreadId = dto.ThreadId,
                        Body = (dto.Body ?? "").Trim(),
                        IsFromGm = dto.IsFromGm,
                        SpeakerName = string.IsNullOrWhiteSpace(dto.SpeakerName) ? null : dto.SpeakerName.Trim(),
                        TurnNumber = dto.TurnNumber,
                        SortOrder = maxSort + 1,
                        CreatedAtUtc = now,
                        SeenByBaron = !dto.IsFromGm,
                        SeenByGm = dto.IsFromGm,
                    };
                    ctx.BaronQaMessages.Add(e);
                }

                thread.UpdatedAtUtc = now;
                await ctx.SaveChangesAsync();
                return ToMessageDto(e);
            }
            catch (Exception ex) when (ex is not InvalidOperationException)
            {
                throw Err(ex, nameof(SaveMessage));
            }
        }

        public async Task MarkThreadSeen(int threadId, bool asGm)
        {
            try
            {
                using var ctx = await _db.CreateDbContextAsync();
                var messages = await ctx.BaronQaMessages.Where(m => m.ThreadId == threadId).ToListAsync();
                var changed = false;
                foreach (var m in messages)
                {
                    if (asGm)
                    {
                        if (!m.SeenByGm) { m.SeenByGm = true; changed = true; }
                    }
                    else if (!m.SeenByBaron)
                    {
                        m.SeenByBaron = true;
                        changed = true;
                    }
                }
                if (changed)
                    await ctx.SaveChangesAsync();
            }
            catch (Exception ex) { throw Err(ex, nameof(MarkThreadSeen)); }
        }

        private static async Task<List<BaronQaThreadDTO>> LoadThreadDtosAsync(ApplicationDbContext ctx, int baronyId)
        {
            var threads = await ctx.BaronQaThreads.AsNoTracking()
                .Where(t => t.BaronyId == baronyId)
                .OrderByDescending(t => t.UpdatedAtUtc)
                .ThenByDescending(t => t.Id)
                .ToListAsync();
            if (threads.Count == 0)
                return new List<BaronQaThreadDTO>();

            var ids = threads.Select(t => t.Id).ToList();
            var messages = await ctx.BaronQaMessages.AsNoTracking()
                .Where(m => ids.Contains(m.ThreadId))
                .OrderBy(m => m.SortOrder)
                .ThenBy(m => m.Id)
                .ToListAsync();
            var byThread = messages.GroupBy(m => m.ThreadId)
                .ToDictionary(g => g.Key, g => g.Select(ToMessageDto).ToList());

            return threads.Select(t =>
            {
                var dto = ToThreadDto(t);
                dto.Messages = byThread.TryGetValue(t.Id, out var list) ? list : new List<BaronQaMessageDTO>();
                return dto;
            }).ToList();
        }

        private static async Task<BaronQaThreadDTO> LoadThreadDtoAsync(ApplicationDbContext ctx, int id)
        {
            var t = await ctx.BaronQaThreads.AsNoTracking().FirstAsync(x => x.Id == id);
            var dto = ToThreadDto(t);
            dto.Messages = (await ctx.BaronQaMessages.AsNoTracking()
                    .Where(m => m.ThreadId == id)
                    .OrderBy(m => m.SortOrder)
                    .ThenBy(m => m.Id)
                    .ToListAsync())
                .Select(ToMessageDto)
                .ToList();
            return dto;
        }

        private static BaronQaThreadDTO ToThreadDto(BaronQaThread e) => new()
        {
            Id = e.Id,
            BaronyId = e.BaronyId,
            Title = e.Title,
            CreatedTurn = e.CreatedTurn,
            CreatedAtUtc = e.CreatedAtUtc,
            UpdatedAtUtc = e.UpdatedAtUtc,
        };

        private static BaronQaMessageDTO ToMessageDto(BaronQaMessage e) => new()
        {
            Id = e.Id,
            ThreadId = e.ThreadId,
            Body = e.Body,
            IsFromGm = e.IsFromGm,
            SpeakerName = e.SpeakerName,
            TurnNumber = e.TurnNumber,
            SortOrder = e.SortOrder,
            CreatedAtUtc = e.CreatedAtUtc,
            SeenByBaron = e.SeenByBaron,
            SeenByGm = e.SeenByGm,
        };

        private static RepositoryErrorException Err(Exception ex, string name) =>
            new("Error in " + name, ex);
    }
}
