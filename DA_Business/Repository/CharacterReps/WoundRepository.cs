using AutoMapper;
using DA_Business.Repository.CharacterReps.IRepository;
using DA_DataAccess.CharacterClasses;
using DA_DataAccess.Data;
using DA_Models.CharacterModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DA_Business.Repository.CharacterReps
{
    public class WoundRepository : IWoundRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _db;
        private readonly IMapper _mapper;

        public WoundRepository(IDbContextFactory<ApplicationDbContext> db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }
        public async Task<WoundDTO> Create(WoundDTO objDTO)
        {
            try
            {
                using var contex = await _db.CreateDbContextAsync();
                var obj = _mapper.Map<WoundDTO, Wound>(objDTO);
                var addedObj = await contex.Wounds.AddAsync(obj);
                await contex.SaveChangesAsync();

                return _mapper.Map<Wound, WoundDTO>(addedObj.Entity);
            }
            catch (Exception ex)
            {
                ;
            }
            return null;
        }

        public async Task<int> Delete(int id)
        {
            using var contex = await _db.CreateDbContextAsync();
            var obj = await contex.Wounds.FirstOrDefaultAsync(u => u.Id == id);
            if (obj != null)
            {
                contex.Wounds.Remove(obj);
                return contex.SaveChanges();
            }
            return 0;
        }

        public async Task<IEnumerable<WoundDTO>> GetAll(int? charId = null)
        {
            try
            {
                using var contex = await _db.CreateDbContextAsync();

                if (contex.Wounds is null || contex.Wounds.Any() == false)
                    return new List<WoundDTO>();
                if (charId == null || charId < 1)
                    return _mapper.Map<IEnumerable<Wound>, IEnumerable<WoundDTO>>(contex.Wounds).Where(u =>u.IsCondition == false);
                return _mapper.Map<IEnumerable<Wound>, IEnumerable<WoundDTO>>(contex.Wounds.Where(u => u.CharacterId == charId && u.IsCondition == false));
            }
            catch (Exception ex)
            {
                ;
            }
            return new List<WoundDTO>();
        }
        public async Task<IEnumerable<ConditionDTO>> GetAllCond(int? charId = null)
        {
            try
            {
                using var contex = await _db.CreateDbContextAsync();

                if (contex.Wounds is null || contex.Wounds.Any() == false)
                    return new List<ConditionDTO>();
                if (charId == null || charId < 1)
                    return _mapper.Map<IEnumerable<Wound>, IEnumerable<ConditionDTO>>(contex.Wounds).Where(u => u.IsCondition == false);
                return _mapper.Map<IEnumerable<Wound>, IEnumerable<ConditionDTO>>(contex.Wounds.Where(u => u.CharacterId == charId && u.IsCondition == true));
            }
            catch (Exception ex)
            {
                ;
            }
            return new List<ConditionDTO>();
        }

        public async Task<WoundDTO> GetById(int id)
        {
            using var contex = await _db.CreateDbContextAsync();
            var obj = await contex.Wounds.FirstOrDefaultAsync(u => u.Id == id);
            if (obj != null)
            {
                return _mapper.Map<Wound, WoundDTO>(obj);
            }
            return new WoundDTO();
        }

        public async Task<WoundDTO> Update(WoundDTO objDTO)
        {
            using var contex = await _db.CreateDbContextAsync();
            var obj = await contex.Wounds.FirstOrDefaultAsync(u => u.Id == objDTO.Id);
            if (obj != null)
            {
                // Update parent
                contex.Entry(obj).CurrentValues.SetValues(objDTO);
                contex.Wounds.Update(obj);
                await contex.SaveChangesAsync();
                return _mapper.Map<Wound, WoundDTO>(obj);
            }
            else
            {
                obj = _mapper.Map<WoundDTO, Wound>(objDTO);
                var addedObj = await contex.Wounds.AddAsync(obj);
                await contex.SaveChangesAsync();
            }
            return objDTO;
        }
    }
}
