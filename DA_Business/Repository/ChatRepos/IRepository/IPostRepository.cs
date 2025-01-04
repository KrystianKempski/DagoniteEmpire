using DA_DataAccess.Chat;
using DA_DataAccess;
using DA_Models.CharacterModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DA_Business.Repository.CharacterReps.IRepository
{
    public interface IPostRepository
    {
        Task<PostDTO> Create(PostDTO objDTO);

        Task<int> Delete(int id);
        Task<IEnumerable<PostDTO>> GetAll(int? chapterId = null);

        Task<PostDTO> GetById(int id);

        Task<PostDTO> Update(PostDTO objDTO);

        Task<IEnumerable<PostDTO>> GetPage(int chapterId, int postPerPage, int pageNum);
        Task<int> GetPostCount(int chapterId);
        Task<int> GetCharacterPostCount(int characterId, DateTime? From = null, DateTime? To = null);
        Task<DateTime> GetCharacterLastPostDate(int characterId);
        Task<int> GetCharacterLastPostChapter(int characterId);
    }
}
