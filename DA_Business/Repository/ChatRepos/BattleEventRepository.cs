using AutoMapper;
using DA_Business.Repository.CharacterReps.IRepository;
using DA_DataAccess.Chat;
using DA_DataAccess.Data;
using DA_Models.ChatModels;
using DagoniteEmpire.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace DA_Business.Repository.ChatRepos
{
    public class BattleEventRepository : IBattleEventRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _db;
        private readonly IMapper _mapper;

        public BattleEventRepository(IDbContextFactory<ApplicationDbContext> db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<BattleEventDTO> Create(BattleEventDTO objDTO)
        {
            try
            {
                using var contex = await _db.CreateDbContextAsync();
                var obj = _mapper.Map<BattleEventDTO, BattleEvent>(objDTO);
                var addedObj = await contex.BattleEvents.AddAsync(obj);
                await contex.SaveChangesAsync();
                return _mapper.Map<BattleEvent, BattleEventDTO>(addedObj.Entity);
            }
            catch (Exception ex)
            {
                throw new RepositoryErrorException("Error in" + System.Reflection.MethodBase.GetCurrentMethod().Name, ex);
            }
        }

        public async Task<IEnumerable<BattleEventDTO>> GetByBattlePhase(int battlePhaseId)
        {
            try
            {
                using var contex = await _db.CreateDbContextAsync();
                if (battlePhaseId < 1)
                    return new List<BattleEventDTO>();

                var obj = await contex.BattleEvents.AsNoTracking()
                    .Where(u => u.BattlePhaseId == battlePhaseId)
                    .OrderBy(u => u.TurnNumber)
                    .ThenBy(u => u.Id)
                    .ToListAsync();
                return _mapper.Map<IEnumerable<BattleEvent>, IEnumerable<BattleEventDTO>>(obj);
            }
            catch (Exception ex)
            {
                throw new RepositoryErrorException("Error in" + System.Reflection.MethodBase.GetCurrentMethod().Name, ex);
            }
        }

        public async Task<IEnumerable<BattleEventDTO>> GetByChapter(int chapterId)
        {
            try
            {
                using var contex = await _db.CreateDbContextAsync();
                if (chapterId < 1)
                    return new List<BattleEventDTO>();

                var obj = await contex.BattleEvents.AsNoTracking()
                    .Where(u => u.ChapterId == chapterId)
                    .OrderBy(u => u.BattlePhaseId)
                    .ThenBy(u => u.TurnNumber)
                    .ThenBy(u => u.Id)
                    .ToListAsync();
                return _mapper.Map<IEnumerable<BattleEvent>, IEnumerable<BattleEventDTO>>(obj);
            }
            catch (Exception ex)
            {
                throw new RepositoryErrorException("Error in" + System.Reflection.MethodBase.GetCurrentMethod().Name, ex);
            }
        }

        public async Task<int> DeleteByBattlePhase(int battlePhaseId)
        {
            try
            {
                using var contex = await _db.CreateDbContextAsync();
                var objs = await contex.BattleEvents.Where(u => u.BattlePhaseId == battlePhaseId).ToListAsync();
                if (objs.Any())
                {
                    contex.BattleEvents.RemoveRange(objs);
                    return await contex.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                throw new RepositoryErrorException("Error in" + System.Reflection.MethodBase.GetCurrentMethod().Name, ex);
            }
            return 0;
        }
    }
}
