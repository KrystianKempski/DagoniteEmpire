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
    }
}
