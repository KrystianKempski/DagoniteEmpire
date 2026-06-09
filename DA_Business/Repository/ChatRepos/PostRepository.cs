using AutoMapper;
using DA_Business.Repository.CharacterReps.IRepository;
using DA_DataAccess.Chat;
using DA_DataAccess.Data;
using DA_Models.CharacterModels;
using DagoniteEmpire.Exceptions;
using Microsoft.EntityFrameworkCore;

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

        private IQueryable<Post> ReadPostsWithCharacter(IQueryable<Post> query) =>
            query.AsNoTracking().Include(u => u.Character);

        public async Task<PostDTO> Create(PostDTO objDTO)
        {
            try
            {
                using var contex = await _db.CreateDbContextAsync();
                var obj = _mapper.Map<PostDTO, Post>(objDTO);
                var addedObj = contex.Posts.Add(obj);

                await contex.SaveChangesAsync();
                return _mapper.Map<Post, PostDTO>(addedObj.Entity);
            }
            catch (Exception ex)
            {
                throw new RepositoryErrorException("Error in" + System.Reflection.MethodBase.GetCurrentMethod()!.Name, ex);
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
            catch (Exception ex) { throw new RepositoryErrorException("Error in" + System.Reflection.MethodBase.GetCurrentMethod()!.Name, ex); }
            return 0;
        }

        public async Task<IEnumerable<PostDTO>> GetAll(int? chapterId = null)
        {
            try
            {
                using var contex = await _db.CreateDbContextAsync();
                var query = contex.Posts.AsQueryable();
                if (chapterId is > 0)
                {
                    query = query.Where(u => u.ChapterId == chapterId);
                }

                var posts = await ReadPostsWithCharacter(query.OrderBy(u => u.CreatedDate)).ToListAsync();
                return _mapper.Map<IEnumerable<Post>, IEnumerable<PostDTO>>(posts);
            }
            catch (Exception ex)
            {
                throw new RepositoryErrorException("Error in" + System.Reflection.MethodBase.GetCurrentMethod()!.Name, ex);
            }
        }

        public async Task<IEnumerable<PostDTO>> GetPage(int chapterId, int postPerPage, int pageNum)
        {
            try
            {
                using var contex = await _db.CreateDbContextAsync();
                if (chapterId == 0 || postPerPage < 1) throw new RepositoryErrorException("Error in" + System.Reflection.MethodBase.GetCurrentMethod()!.Name);

                int skip = postPerPage * (pageNum - 1);
                var posts = await ReadPostsWithCharacter(
                        contex.Posts
                            .Where(u => u.ChapterId == chapterId)
                            .OrderBy(u => u.CreatedDate)
                            .Skip(skip)
                            .Take(postPerPage))
                    .ToListAsync();

                return _mapper.Map<IEnumerable<Post>, IEnumerable<PostDTO>>(posts);
            }
            catch (Exception ex)
            {
                throw new RepositoryErrorException("Error in" + System.Reflection.MethodBase.GetCurrentMethod()!.Name, ex);
            }
        }

        public async Task<int> GetPostCount(int chapterId)
        {
            try
            {
                using var contex = await _db.CreateDbContextAsync();
                if (chapterId == 0) throw new RepositoryErrorException("Error in" + System.Reflection.MethodBase.GetCurrentMethod()!.Name);

                return await contex.Posts
                    .AsNoTracking()
                    .CountAsync(u => u.ChapterId == chapterId);
            }
            catch (Exception ex)
            {
                throw new RepositoryErrorException("Error in" + System.Reflection.MethodBase.GetCurrentMethod()!.Name, ex);
            }
        }

        public async Task<int> GetCharacterPostCount(int characterId)
        {
            try
            {
                using var contex = await _db.CreateDbContextAsync();
                if (characterId == 0) throw new RepositoryErrorException("Error in" + System.Reflection.MethodBase.GetCurrentMethod()!.Name);

                return await contex.Posts
                    .AsNoTracking()
                    .CountAsync(u => u.CharacterId == characterId);
            }
            catch (Exception ex)
            {
                throw new RepositoryErrorException("Error in" + System.Reflection.MethodBase.GetCurrentMethod()!.Name, ex);
            }
        }

        public async Task<DateTime> GetCharacterLastPostDate(int characterId)
        {
            try
            {
                using var contex = await _db.CreateDbContextAsync();
                if (characterId == 0) throw new RepositoryErrorException("Error in" + System.Reflection.MethodBase.GetCurrentMethod()!.Name);

                return await contex.Posts
                    .AsNoTracking()
                    .Where(u => u.CharacterId == characterId)
                    .OrderByDescending(p => p.CreatedDate)
                    .Select(p => p.CreatedDate)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                throw new RepositoryErrorException("Error in" + System.Reflection.MethodBase.GetCurrentMethod()!.Name, ex);
            }
        }

        public async Task<int> GetCharacterPostCount(int characterId, DateTime? From = null, DateTime? To = null)
        {
            try
            {
                using var contex = await _db.CreateDbContextAsync();
                if (characterId == 0) throw new RepositoryErrorException("Error in" + System.Reflection.MethodBase.GetCurrentMethod()!.Name);

                var query = contex.Posts.AsNoTracking().Where(u => u.CharacterId == characterId);
                if (From is not null && To is null)
                {
                    query = query.Where(u => u.CreatedDate >= From);
                }
                else if (From is not null && To is not null)
                {
                    query = query.Where(u => u.CreatedDate >= From && u.CreatedDate <= To);
                }

                return await query.CountAsync();
            }
            catch (Exception ex)
            {
                throw new RepositoryErrorException("Error in" + System.Reflection.MethodBase.GetCurrentMethod()!.Name, ex);
            }
        }

        public async Task<int> GetCharacterLastPostChapter(int characterId)
        {
            try
            {
                using var contex = await _db.CreateDbContextAsync();
                if (characterId == 0) throw new RepositoryErrorException("Error in" + System.Reflection.MethodBase.GetCurrentMethod()!.Name);

                return await contex.Posts
                    .AsNoTracking()
                    .Where(u => u.CharacterId == characterId)
                    .OrderByDescending(p => p.CreatedDate)
                    .Select(p => p.ChapterId)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                throw new RepositoryErrorException("Error in" + System.Reflection.MethodBase.GetCurrentMethod()!.Name, ex);
            }
        }

        public async Task<PostDTO> GetById(int id)
        {
            try
            {
                using var contex = await _db.CreateDbContextAsync();
                var obj = await ReadPostsWithCharacter(contex.Posts.Where(u => u.Id == id))
                    .FirstOrDefaultAsync();
                if (obj != null)
                {
                    return _mapper.Map<Post, PostDTO>(obj);
                }
            }
            catch (Exception ex) { throw new RepositoryErrorException("Error in" + System.Reflection.MethodBase.GetCurrentMethod()!.Name, ex); }
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
            catch (Exception ex) { throw new RepositoryErrorException("Error in" + System.Reflection.MethodBase.GetCurrentMethod()!.Name, ex); }
            return objDTO;
        }
    }
}
