using DA_Business.Services.Interfaces;
using DA_Common.Barony;
using DA_DataAccess.BaronyData;
using DA_DataAccess.Data;
using DA_Models.BaronyModels;

namespace DA_Business.Services
{
    /// <summary>No-op chronicle for tests and seeding paths that have no user context.</summary>
    public class NullBaronyLogService : IBaronyLogService
    {
        public Task Log(int baronyId, string category, string summary, string? details = null,
            string? entityType = null, int? entityId = null, bool important = false) => Task.CompletedTask;

        public Task Attach(ApplicationDbContext ctx, Barony barony, string category, string summary,
            string? details = null, string? entityType = null, int? entityId = null,
            bool important = false, BaronyLogStamp? stamp = null) => Task.CompletedTask;

        public Task<IReadOnlyList<BaronyLogTurnDTO>> GetTurns(int baronyId) =>
            Task.FromResult<IReadOnlyList<BaronyLogTurnDTO>>(new List<BaronyLogTurnDTO>());

        public Task<IReadOnlyList<BaronyLogEntryDTO>> GetEntries(int baronyId, int turnNumber, BaronyLogFilterDTO? filter = null) =>
            Task.FromResult<IReadOnlyList<BaronyLogEntryDTO>>(new List<BaronyLogEntryDTO>());

        public Task<IReadOnlyList<string>> GetActors(int baronyId) =>
            Task.FromResult<IReadOnlyList<string>>(new List<string>());

        public Task<bool> Delete(int entryId) => Task.FromResult(false);
    }
}
