using AutoMapper;
using DA_Business.Repository.CharacterReps.IRepository;
using DA_DataAccess.CharacterClasses;
using DA_DataAccess.Chat;
using DA_DataAccess.Data;
using DA_Models.CharacterModels;
using DagoniteEmpire.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace DA_Business.Repository.ChatRepos
{
    public class CampaignRepository : ICampaignRepository
    {
        //private readonly ApplicationDbContext _db;
        private readonly IDbContextFactory<ApplicationDbContext> _db;
        private readonly IMapper _mapper;

        public CampaignRepository(IDbContextFactory<ApplicationDbContext> db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<CampaignDTO> Create(CampaignDTO objDTO)
        {
            try
            {
                using var contex = await _db.CreateDbContextAsync();
                var obj = _mapper.Map<CampaignDTO, Campaign>(objDTO);

                foreach(var cha in obj.Characters)
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

                var addedObj = await contex.Campaigns.AddAsync(obj);
                await contex.SaveChangesAsync();
                return _mapper.Map<Campaign, CampaignDTO>(addedObj.Entity);
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
                var obj = await contex.Campaigns.FirstOrDefaultAsync(u => u.Id == id);
                if (obj != null)
                {
                    contex.Campaigns.Remove(obj);
                    return await contex.SaveChangesAsync();
                }
            }
            catch (Exception ex) { 
                throw new RepositoryErrorException("Error in" + System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
            return 0;
        }

        public async Task<IEnumerable<CampaignDTO>> GetAll(int? characterId = null)
        {
            try
            {

                using var contex = await _db.CreateDbContextAsync();
                if (characterId==null)
                    return _mapper.Map<IEnumerable<Campaign>, IEnumerable<CampaignDTO>>(contex.Campaigns.Include(a => a.Characters));

                var obj = contex.Campaigns.Include(a=>a.Characters).Where(u => u.Characters.FirstOrDefault(a=>a.Id==characterId)!=null).OrderBy(u => u.CreatedDate);
                if (obj != null && obj.Any())
                    return _mapper.Map<IEnumerable<Campaign>, IEnumerable<CampaignDTO>>(obj);
                else
                    return new List<CampaignDTO>();
               

            }
            catch (Exception ex) { 
                throw new RepositoryErrorException("Error in" + System.Reflection.MethodBase.GetCurrentMethod().Name); 
            }
        }

        public async Task<CampaignDTO> GetById(int id)
        {
            try
            {
                using var contex = await _db.CreateDbContextAsync();
                var obj = await contex.Campaigns.Include(a => a.Characters).FirstOrDefaultAsync(u => u.Id == id);
                if (obj != null)
                {
                    return _mapper.Map<Campaign, CampaignDTO>(obj);
                }
            }
            catch (Exception ex) {
                throw new RepositoryErrorException("Error in" + System.Reflection.MethodBase.GetCurrentMethod().Name); 
            }
            return new CampaignDTO();
        }

        public async Task<CampaignDTO> Update(CampaignDTO objDTO)
        {
            try
            {
                using var contex = await _db.CreateDbContextAsync();
                var obj = await contex.Campaigns.FirstOrDefaultAsync(u => u.Id == objDTO.Id);
                if (obj != null)
                {
                    obj.IsFinished = objDTO.IsFinished;
                    obj.Description = objDTO.Description;
                    obj.Name = objDTO.Name;

                    contex.Campaigns.Update(obj);
                    await contex.SaveChangesAsync();
                    return _mapper.Map<Campaign, CampaignDTO>(obj);
                }
            }
            catch (Exception ex) {
                throw new RepositoryErrorException("Error in" + System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
            return objDTO;
        }
    }
}
