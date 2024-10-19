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
    public interface IChapterRepository
    {
        Task<ChapterDTO> Create(ChapterDTO objDTO);

        Task<int> Delete(int id);
        Task<IEnumerable<ChapterDTO>> GetAll(int? campaignId = null);

        Task<ChapterDTO> GetById(int id);

        Task<ChapterDTO> Update(ChapterDTO objDTO);
    }
}
