using AutoMapper;
using DA_Business.Repository.CharacterReps.IRepository;
using DA_DataAccess.CharacterClasses;
using DA_DataAccess.Chat;
using DA_DataAccess.Data;
using DA_Models.CharacterModels;
using DagoniteEmpire.Exceptions;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.Metrics;

namespace DA_Business.Repository.ChatRepos
{
    public class PostRepository : IPostRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _db;
        private readonly IMapper _mapper;

        public PostRepository(IDbContextFactory<ApplicationDbContext> db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<PostDTO> Create(PostDTO objDTO)
        {
            
            try
            {
                using var contex = await _db.CreateDbContextAsync();
                var obj = _mapper.Map<PostDTO, Post>(objDTO);
                obj.Character = null;
                obj.Chapter = null;
                var addedObj = contex.Posts.Add(obj);

                await contex.SaveChangesAsync();
                return _mapper.Map<Post, PostDTO>(addedObj.Entity);
            }
            catch (Exception ex) { 
                throw new RepositoryErrorException("Error in" + System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
            
        }

        public async Task<int> Delete(int id)
        {
            try
            {
                using var contex = await _db.CreateDbContextAsync();
                var obj = await contex.Posts.FirstOrDefaultAsync(u => u.Id == id);
                if (obj != null)
                {
                    contex.Posts.Remove(obj);
                    return contex.SaveChanges();
                }
            }
            catch (Exception ex) { throw new RepositoryErrorException("Error in" + System.Reflection.MethodBase.GetCurrentMethod().Name);}
            return 0;
        }

        public async Task<IEnumerable<PostDTO>> GetAll(int? chapterId = null)
        {
            try
            {
                using var contex = await _db.CreateDbContextAsync();
                if (chapterId == null || chapterId < 1)
                    return _mapper.Map<IEnumerable<Post>, IEnumerable<PostDTO>>(contex.Posts.Include(u => u.Character));
            
                
                var obj = contex.Posts.Include(u=>u.Character).Where(u => u.ChapterId  == chapterId).OrderBy(u => u.CreatedDate);
                if (obj != null && obj.Any())
                    return _mapper.Map<IEnumerable<Post>, IEnumerable<PostDTO>>(obj);
            }
            catch (Exception ex) {
                throw new RepositoryErrorException("Error in" + System.Reflection.MethodBase.GetCurrentMethod().Name);
            }

            return new List<PostDTO>();
        }

        public async Task<IEnumerable<PostDTO>> GetPage(int chapterId, int postPerPage, int pageNum )
        {
            try
            {
                using var contex = await _db.CreateDbContextAsync();
                if(chapterId == 0 || postPerPage <1) throw new RepositoryErrorException("Error in" + System.Reflection.MethodBase.GetCurrentMethod().Name);

                int skip = postPerPage * (pageNum-1);
                var obj = contex.Posts.Include(u => u.Character).Where(u => u.ChapterId == chapterId).OrderBy(u => u.CreatedDate).Skip(skip).Take(postPerPage);
                if (obj != null && obj.Any())
                    return _mapper.Map<IEnumerable<Post>, IEnumerable<PostDTO>>(obj);
            }
            catch (Exception ex)
            {
                throw new RepositoryErrorException("Error in" + System.Reflection.MethodBase.GetCurrentMethod().Name);
            }

            return new List<PostDTO>();
        }
        public async Task<int> GetPostCount(int chapterId)
        {
            try
            {
                using var contex = await _db.CreateDbContextAsync();
                if (chapterId == 0) throw new RepositoryErrorException("Error in" + System.Reflection.MethodBase.GetCurrentMethod().Name);

                int count = contex.Posts.Where(u => u.ChapterId == chapterId).Count();

                return count;
            }
            catch (Exception ex)
            {
                throw new RepositoryErrorException("Error in" + System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        public async Task<PostDTO> GetById(int id)
        {
            try
            {
                using var contex = await _db.CreateDbContextAsync();
                var obj = await contex.Posts.Include(u => u.Character).FirstOrDefaultAsync(u => u.Id == id);
                if (obj != null)
                {
                    return _mapper.Map<Post, PostDTO>(obj);
                }
            }
            catch (Exception ex) { throw new RepositoryErrorException("Error in" + System.Reflection.MethodBase.GetCurrentMethod().Name); }
            return new PostDTO();
        }

        public async Task<PostDTO> Update(PostDTO objDTO)
        {
            try
            {
                using var contex = await _db.CreateDbContextAsync();
                var obj = await contex.Posts.FirstOrDefaultAsync(u => u.Id == objDTO.Id);
                if (obj != null)
                {
                    obj.Content = objDTO.Content;
                    obj.CreatedDate = objDTO.CreatedDate;

                    contex.Posts.Update(obj);
                    await contex.SaveChangesAsync();
                    return _mapper.Map<Post, PostDTO>(obj);
                }
            }
            catch (Exception ex) { throw new RepositoryErrorException("Error in" + System.Reflection.MethodBase.GetCurrentMethod().Name); }
            return objDTO;
        }
    }
}
