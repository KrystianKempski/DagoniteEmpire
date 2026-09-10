using DA_Common.Barony;
using DA_DataAccess.BaronyData;
using DA_DataAccess.Data;
using DA_Models.BaronyModels;

namespace DA_Business.Services.Interfaces
{
    /// <summary>
    /// Barony chronicle: records what the baron and the Game Master did, grouped by turn.
    /// Implementations must never throw — a failed log entry may not break the logged action.
    /// </summary>
    public interface IBaronyLogService
    {
        /// <summary>Writes a single short entry in its own transaction.</summary>
        Task Log(
            int baronyId,
            string category,
            string summary,
            string? details = null,
            string? entityType = null,
            int? entityId = null,
            bool important = false);

        /// <summary>
        /// Queues an entry inside a caller-owned context; the caller's <c>SaveChangesAsync</c>
        /// persists it together with the change being logged.
        /// </summary>
        Task Attach(
            ApplicationDbContext ctx,
            Barony barony,
            string category,
            string summary,
            string? details = null,
            string? entityType = null,
            int? entityId = null,
            bool important = false,
            BaronyLogStamp? stamp = null);

        /// <summary>Turn headers (newest first) with entry counts, without loading entries.</summary>
        Task<IReadOnlyList<BaronyLogTurnDTO>> GetTurns(int baronyId);

        /// <summary>Entries of a single turn, oldest first, optionally filtered.</summary>
        Task<IReadOnlyList<BaronyLogEntryDTO>> GetEntries(int baronyId, int turnNumber, BaronyLogFilterDTO? filter = null);

        /// <summary>Distinct actors that ever wrote to this barony's chronicle.</summary>
        Task<IReadOnlyList<string>> GetActors(int baronyId);

        /// <summary>Game Master removal of a single entry.</summary>
        Task<bool> Delete(int entryId);
    }
}
