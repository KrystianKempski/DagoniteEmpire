using DA_Models.ChatModels;
using System.Threading.Tasks;

namespace DA_Business.Repository.CharacterReps.IRepository
{
    public interface IBattleMapRepository
    {
        /// <summary>Returns the map for a chapter, creating a default one if none exists yet.</summary>
        Task<BattleMapDTO> GetOrCreateForChapter(int chapterId, int campaignId);

        Task<BattleMapDTO> Update(BattleMapDTO objDTO);

        Task<int> Delete(int id);
    }
}
