using DA_Models.BaronyModels;

namespace DA_Business.Repository.BaronyRepos
{
    public interface IBaronQaRepository
    {
        Task<List<BaronQaThreadDTO>> GetThreads(int baronyId);
        Task<BaronQaThreadDTO> SaveThread(BaronQaThreadDTO dto);
        Task<int> DeleteThread(int threadId);
        Task<BaronQaMessageDTO> SaveMessage(BaronQaMessageDTO dto);
        Task MarkThreadSeen(int threadId, bool asGm);
        /// <summary>Unread GM answers across the baronies this account owns.</summary>
        Task<BaronQaInboxBadgeDTO> GetInboxBadgeForBaron(IReadOnlyCollection<int> baronyIds);
        /// <summary>Unread player questions across every barony — the GM answers for all of them.</summary>
        Task<BaronQaInboxBadgeDTO> GetInboxBadgeForGm();
    }
}
