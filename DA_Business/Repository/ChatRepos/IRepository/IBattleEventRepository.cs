using System.Collections.Generic;
using System.Threading.Tasks;
using DA_Models.ChatModels;

namespace DA_Business.Repository.CharacterReps.IRepository
{
    public interface IBattleEventRepository
    {
        Task<BattleEventDTO> Create(BattleEventDTO objDTO);

        /// <summary>All events of a single battle, ordered chronologically.</summary>
        Task<IEnumerable<BattleEventDTO>> GetByBattlePhase(int battlePhaseId);

        /// <summary>All events ever recorded for a chapter, ordered chronologically.</summary>
        Task<IEnumerable<BattleEventDTO>> GetByChapter(int chapterId);

        Task<int> DeleteByBattlePhase(int battlePhaseId);
    }
}
