using DA_Models.BaronyModels;
using System.Threading.Tasks;

namespace DA_Business.Repository.BaronyRepos
{
    public interface IBaronyBattleMapRepository
    {
        Task<BaronyBattleMapDTO> GetOrCreate(int baronyId);
        Task<BaronyBattleMapDTO> Update(BaronyBattleMapDTO dto);
        Task<bool> IsActive(int baronyId);
        Task SetActive(int baronyId, bool active);
    }
}
