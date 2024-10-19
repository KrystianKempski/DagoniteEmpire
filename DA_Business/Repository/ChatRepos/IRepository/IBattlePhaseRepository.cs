using DA_DataAccess.Chat;
using DA_DataAccess;
using DA_Models.CharacterModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DA_Models.ChatModels;

namespace DA_Business.Repository.CharacterReps.IRepository
{
    public interface IBattlePhaseRepository
    {
        Task<BattlePhaseDTO> Create(BattlePhaseDTO objDTO);

        Task<int> Delete(int id);
        Task<IEnumerable<BattlePhaseDTO>> GetAllForChapter(int? chapter = null);

        Task<BattlePhaseDTO?> GetCurrentForChapter(int chapterId);

        Task<BattlePhaseDTO> GetById(int id);

        Task<BattlePhaseDTO> Update(BattlePhaseDTO objDTO);
    }
}
