using AutoMapper;
using DA_Business.Repository.CharacterReps.IRepository;
using DA_DataAccess.CharacterClasses;
using DA_DataAccess.Chat;
using DA_DataAccess.Data;
using DA_Models.CharacterModels;
using DA_Models.ChatModels;
using DagoniteEmpire.Exceptions;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Net.NetworkInformation;

namespace DA_Business.Repository.ChatRepos
{
    public class BattlePhaseRepository : IBattlePhaseRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _db;
        private readonly IMapper _mapper;

        public BattlePhaseRepository(IDbContextFactory<ApplicationDbContext> db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }


        public async Task<BattlePhaseDTO> Create(BattlePhaseDTO objDTO)
        {
            try
            {
                using var contex = await _db.CreateDbContextAsync();
                var obj = _mapper.Map<BattlePhaseDTO, BattlePhase>(objDTO);
                var addedObj = await contex.BattlePhases.AddAsync(obj);
                await contex.SaveChangesAsync();
                return _mapper.Map<BattlePhase, BattlePhaseDTO>(addedObj.Entity);
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
                var obj = await contex.BattlePhases.FirstOrDefaultAsync(u => u.Id == id);
                if (obj != null)
                {
                    
                    contex.BattlePhases.Remove(obj);
                    return contex.SaveChanges();
                }
            }
            catch (Exception ex) { throw new RepositoryErrorException("Error in" + System.Reflection.MethodBase.GetCurrentMethod().Name); }
            return 0;
        }

        public async Task<BattlePhaseDTO?> GetCurrentForChapter(int chapterId)
        {
            try
            {
                using var contex = await _db.CreateDbContextAsync();
                if (chapterId < 1)
                    return null;

                var obj = contex.BattlePhases.FirstOrDefault(u => u.ChapterId == chapterId && u.BattleOngoing);
                if (obj != null)
                    return _mapper.Map<BattlePhase, BattlePhaseDTO>(obj);
                else
                    return null;
            }
            catch (Exception ex) { throw new RepositoryErrorException("Error in" + System.Reflection.MethodBase.GetCurrentMethod().Name); }
        }

        public async Task<IEnumerable<BattlePhaseDTO>> GetAllForChapter(int? chapterId = null)
        {
            try
            {
                using var contex = await _db.CreateDbContextAsync();
                if (chapterId == null || chapterId < 1)
                    return _mapper.Map<IEnumerable<BattlePhase>, IEnumerable<BattlePhaseDTO>>(contex.BattlePhases);

                var obj = contex.BattlePhases.Where(u => u.ChapterId == chapterId);
                if (obj != null && obj.Any())
                    return _mapper.Map<IEnumerable<BattlePhase>, IEnumerable<BattlePhaseDTO>>(obj);
            }
            catch (Exception ex) { throw new RepositoryErrorException("Error in" + System.Reflection.MethodBase.GetCurrentMethod().Name); }

            return new List<BattlePhaseDTO>();
        }

        public async Task<BattlePhaseDTO> GetById(int id)
        {
            try
            {
                using var contex = await _db.CreateDbContextAsync();
                var obj = await contex.BattlePhases.FirstOrDefaultAsync(u => u.Id == id);
                if (obj != null)
                {
                    return _mapper.Map<BattlePhase, BattlePhaseDTO>(obj);
                }
            }
            catch (Exception ex) { throw new RepositoryErrorException("Error in" + System.Reflection.MethodBase.GetCurrentMethod().Name); }
            return new BattlePhaseDTO();
        }

        public async Task<BattlePhaseDTO> Update(BattlePhaseDTO objDTO)
        {
            try
            {
                using var contex = await _db.CreateDbContextAsync();
                var obj = await contex.BattlePhases.FirstOrDefaultAsync(u => u.Id == objDTO.Id);
                if (obj is not null)
                {
                    var updatedObj = _mapper.Map<BattlePhaseDTO, BattlePhase>(objDTO);

                    // Update parent
                    contex.Entry(obj).CurrentValues.SetValues(updatedObj);
                    await contex.SaveChangesAsync();
                    return _mapper.Map<BattlePhase, BattlePhaseDTO>(obj);
                }
                else
                {
                    obj = _mapper.Map<BattlePhaseDTO, BattlePhase>(objDTO);
                    var addedObj = contex.BattlePhases.Add(obj);
                    await contex.SaveChangesAsync();

                    return _mapper.Map<BattlePhase, BattlePhaseDTO>(addedObj.Entity);
                }
            }
            catch (Exception ex)
            {
                throw new RepositoryErrorException("Error in" + System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
    }
}
