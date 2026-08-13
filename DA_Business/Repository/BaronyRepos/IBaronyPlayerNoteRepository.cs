using DA_Models.BaronyModels;

namespace DA_Business.Repository.BaronyRepos
{
    public interface IBaronyPlayerNoteRepository
    {
        Task<List<BaronyPlayerNoteDTO>> GetNotes(int baronyId, string? noteType = null);

        /// <summary>Returns the single journal note for the barony, creating an empty one if needed.</summary>
        Task<BaronyPlayerNoteDTO> GetOrCreateJournal(int baronyId, int currentTurn);

        Task<BaronyPlayerNoteDTO> Upsert(BaronyPlayerNoteDTO dto);

        Task DeleteNote(int id);

        /// <summary>Count of active reminders whose target turn has been reached (barony turn resolved internally).</summary>
        Task<int> GetDueReminderCount(int baronyId);
    }
}
