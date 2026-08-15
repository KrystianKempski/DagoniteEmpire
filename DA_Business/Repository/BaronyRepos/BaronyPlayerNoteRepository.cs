using DA_DataAccess.BaronyData;
using DA_DataAccess.Data;
using DA_Models.BaronyModels;
using DagoniteEmpire.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace DA_Business.Repository.BaronyRepos
{
    /// <summary>
    /// Private baron planning notes (journal / sticky / reminders). Own DbContexts via factory,
    /// manual entity ↔ DTO mapping (same pattern as BaronyBattleMapRepository).
    /// </summary>
    public sealed class BaronyPlayerNoteRepository : IBaronyPlayerNoteRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _db;

        public BaronyPlayerNoteRepository(IDbContextFactory<ApplicationDbContext> db)
        {
            _db = db;
        }

        public async Task<List<BaronyPlayerNoteDTO>> GetNotes(int baronyId, string? noteType = null, string ownerScope = "player")
        {
            try
            {
                using var ctx = await _db.CreateDbContextAsync();
                var query = ctx.BaronyPlayerNotes.AsNoTracking()
                    .Where(n => n.BaronyId == baronyId && n.OwnerScope == ownerScope);
                if (!string.IsNullOrWhiteSpace(noteType))
                    query = query.Where(n => n.NoteType == noteType);

                var list = await query
                    .OrderBy(n => n.SortOrder)
                    .ThenBy(n => n.Id)
                    .ToListAsync();
                return list.Select(ToDTO).ToList();
            }
            catch (System.Exception ex) { throw Err(ex, nameof(GetNotes)); }
        }

        public async Task<BaronyPlayerNoteDTO> GetOrCreateJournal(int baronyId, int currentTurn, string ownerScope = "player")
        {
            try
            {
                using var ctx = await _db.CreateDbContextAsync();
                var journal = await ctx.BaronyPlayerNotes
                    .FirstOrDefaultAsync(n => n.BaronyId == baronyId
                                              && n.OwnerScope == ownerScope
                                              && n.NoteType == BaronyPlayerNoteType.Journal);
                if (journal is not null)
                    return ToDTO(journal);

                journal = new BaronyPlayerNote
                {
                    BaronyId = baronyId,
                    OwnerScope = ownerScope,
                    NoteType = BaronyPlayerNoteType.Journal,
                    BodyHtml = "<p></p>",
                    CreatedTurn = currentTurn,
                    CreatedAtUtc = DateTime.UtcNow,
                    UpdatedAtUtc = DateTime.UtcNow,
                };
                ctx.BaronyPlayerNotes.Add(journal);
                await ctx.SaveChangesAsync();
                return ToDTO(journal);
            }
            catch (System.Exception ex) { throw Err(ex, nameof(GetOrCreateJournal)); }
        }

        public async Task<BaronyPlayerNoteDTO> Upsert(BaronyPlayerNoteDTO dto)
        {
            try
            {
                using var ctx = await _db.CreateDbContextAsync();
                BaronyPlayerNote? entity = dto.Id > 0
                    ? await ctx.BaronyPlayerNotes.FirstOrDefaultAsync(n => n.Id == dto.Id)
                    : null;

                if (entity is null)
                {
                    entity = new BaronyPlayerNote
                    {
                        BaronyId = dto.BaronyId,
                        OwnerScope = string.IsNullOrWhiteSpace(dto.OwnerScope)
                            ? BaronyPlayerNoteOwnerScope.Player
                            : dto.OwnerScope,
                        CreatedTurn = dto.CreatedTurn,
                        CreatedAtUtc = DateTime.UtcNow,
                    };
                    ctx.BaronyPlayerNotes.Add(entity);
                }

                entity.NoteType = string.IsNullOrWhiteSpace(dto.NoteType)
                    ? BaronyPlayerNoteType.Journal
                    : dto.NoteType;
                entity.Title = dto.Title;
                entity.BodyHtml = dto.BodyHtml;
                entity.Color = dto.Color;
                entity.DueTurn = dto.DueTurn;
                entity.IsDone = dto.IsDone;
                entity.SortOrder = dto.SortOrder;
                entity.UpdatedAtUtc = DateTime.UtcNow;

                await ctx.SaveChangesAsync();
                return ToDTO(entity);
            }
            catch (System.Exception ex) { throw Err(ex, nameof(Upsert)); }
        }

        public async Task DeleteNote(int id)
        {
            try
            {
                using var ctx = await _db.CreateDbContextAsync();
                var entity = await ctx.BaronyPlayerNotes.FirstOrDefaultAsync(n => n.Id == id);
                if (entity is null)
                    return;
                ctx.BaronyPlayerNotes.Remove(entity);
                await ctx.SaveChangesAsync();
            }
            catch (System.Exception ex) { throw Err(ex, nameof(DeleteNote)); }
        }

        public async Task<int> GetDueReminderCount(int baronyId, string ownerScope = "player")
        {
            try
            {
                using var ctx = await _db.CreateDbContextAsync();
                var turn = await ctx.Baronies
                    .Where(b => b.Id == baronyId)
                    .Select(b => (int?)b.TurnNumber)
                    .FirstOrDefaultAsync() ?? 0;

                return await ctx.BaronyPlayerNotes.AsNoTracking()
                    .CountAsync(n => n.BaronyId == baronyId
                                     && n.OwnerScope == ownerScope
                                     && n.NoteType == BaronyPlayerNoteType.Reminder
                                     && !n.IsDone
                                     && n.DueTurn != null
                                     && n.DueTurn <= turn);
            }
            catch (System.Exception ex) { throw Err(ex, nameof(GetDueReminderCount)); }
        }

        private static BaronyPlayerNoteDTO ToDTO(BaronyPlayerNote e) => new()
        {
            Id = e.Id,
            BaronyId = e.BaronyId,
            OwnerScope = e.OwnerScope,
            NoteType = e.NoteType,
            Title = e.Title,
            BodyHtml = e.BodyHtml,
            Color = e.Color,
            DueTurn = e.DueTurn,
            CreatedTurn = e.CreatedTurn,
            IsDone = e.IsDone,
            SortOrder = e.SortOrder,
            CreatedAtUtc = e.CreatedAtUtc,
            UpdatedAtUtc = e.UpdatedAtUtc,
        };

        private static RepositoryErrorException Err(System.Exception ex, string method) =>
            new RepositoryErrorException("Error in " + method + ": " + ex.Message, ex);
    }
}
