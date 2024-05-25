using AutoMapper;
using DA_Business.Repository.CharacterReps.IRepository;
using DA_DataAccess.CharacterClasses;
using DA_DataAccess.Chat;
using DA_DataAccess.Data;
using DA_Models.CharacterModels;
using DagoniteEmpire.Exceptions;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.Metrics;
using System.Net.NetworkInformation;

namespace DA_Business.Repository.ChatRepos
{
    public class ChapterRepository : IChapterRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _db;
        private readonly IMapper _mapper;

        public ChapterRepository(IDbContextFactory<ApplicationDbContext> db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }


        public async Task<ChapterDTO> Create(ChapterDTO objDTO)
        {
            try
            {
                using var contex = await _db.CreateDbContextAsync();
                var obj = _mapper.Map<ChapterDTO, Chapter>(objDTO);

                foreach (var cha in obj.Characters)
                {
                    cha.Profession = null;
                    cha.Race = null;
                }

                var characterts = await contex.Characters.ToListAsync();
                characterts.ForEach(t =>
                {
                    if (obj.Characters.Any(nt => nt.Id == t.Id))
                    {
                        var untracked = obj.Characters.FirstOrDefault(nt => nt.Id == t.Id);
                        obj.Characters.Remove(untracked);
                        obj.Characters.Add(t);
                    }
                });
                var posts = await contex.Posts.ToListAsync();
                posts.ForEach(t =>
                {
                    if (obj.Posts.Any(nt => nt.Id == t.Id))
                    {
                        var untracked = obj.Posts.FirstOrDefault(nt => nt.Id == t.Id);
                        obj.Posts.Remove(untracked);
                        obj.Posts.Add(t);
                    }
                });

                var addedObj = await contex.Chapters.AddAsync(obj);
                await contex.SaveChangesAsync();
                return _mapper.Map<Chapter, ChapterDTO>(addedObj.Entity);
            }
            catch (Exception ex)
            {
                throw new RepositoryErrorException("Error in" + System.Reflection.MethodBase.GetCurrentMethod().Name);
            }

        }

        public async Task<int> Delete(int id)
        {
            try
            {
                using var contex = await _db.CreateDbContextAsync();
                var obj = await contex.Chapters.Include(a => a.Posts).FirstOrDefaultAsync(u => u.Id == id);
                if (obj != null)
                {
                    if (obj.Posts != null && obj.Posts.Any())
                    {
                        foreach(var post in obj.Posts) 
                        {
                            contex.Posts.Remove(post);
                        }
                    }
                    contex.Chapters.Remove(obj);
                    return contex.SaveChanges();
                }
            }
            catch (Exception ex) { throw new RepositoryErrorException("Error in" + System.Reflection.MethodBase.GetCurrentMethod().Name);}
            return 0;
        }

        public async Task<IEnumerable<ChapterDTO>> GetAll(int? campaignId = null)
        {
            try
            {
                using var contex = await _db.CreateDbContextAsync();
                if (campaignId == null || campaignId < 1)
                    return _mapper.Map<IEnumerable<Chapter>, IEnumerable<ChapterDTO>>(contex.Chapters.Include(a => a.Characters).Include(a=>a.Posts));
            
                var obj = contex.Chapters.Include(a => a.Characters).Include(a => a.Posts).Where(u => u.CampaignId == campaignId).OrderBy(u => u.CreatedDate);
                if (obj != null && obj.Any())
                    return _mapper.Map<IEnumerable<Chapter>, IEnumerable<ChapterDTO>>(obj);
            }
            catch (Exception ex) { throw new RepositoryErrorException("Error in" + System.Reflection.MethodBase.GetCurrentMethod().Name); }

            return new List<ChapterDTO>();
        }

        public async Task<ChapterDTO> GetById(int id)
        {
            try
            {
                using var contex = await _db.CreateDbContextAsync();
                var obj = await contex.Chapters.Include(a => a.Characters).Include(a => a.Posts).FirstOrDefaultAsync(u => u.Id == id);
                if (obj != null)
                {
                    return _mapper.Map<Chapter, ChapterDTO>(obj);
                }
            }
            catch (Exception ex) { throw new RepositoryErrorException("Error in" + System.Reflection.MethodBase.GetCurrentMethod().Name); }
            return new ChapterDTO();
        }

        public async Task<ChapterDTO> Update(ChapterDTO objDTO)
        {
            try
            {
                using var contex = await _db.CreateDbContextAsync();
                var obj = await contex.Chapters.FirstOrDefaultAsync(u => u.Id == objDTO.Id);
                if (obj != null)
                {
                    obj.Day = objDTO.Day;
                    obj.Place = objDTO.Day;
                    obj.IsFinished = objDTO.IsFinished;
                    obj.Description = objDTO.Description;
                    obj.Name = objDTO.Name;

                    contex.Chapters.Update(obj);
                    await contex.SaveChangesAsync();
                    return _mapper.Map<Chapter, ChapterDTO>(obj);
                }
            }
            catch (Exception ex) { throw new RepositoryErrorException("Error in" + System.Reflection.MethodBase.GetCurrentMethod().Name); }
            return objDTO;
        }
    }
}
