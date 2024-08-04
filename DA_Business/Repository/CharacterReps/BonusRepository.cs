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
    public class BonusRepository : IBonusRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _db;
        private readonly IMapper _mapper;

        public BonusRepository(IDbContextFactory<ApplicationDbContext> db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }
        public async Task<BonusDTO> Create(BonusDTO objDTO)
        {
            try
            {
                using var contex = await _db.CreateDbContextAsync();
                var obj = _mapper.Map<BonusDTO, Bonus>(objDTO);
                var addedObj = contex.Bonuses.Add(obj);
                await contex.SaveChangesAsync();

                return _mapper.Map<Bonus, BonusDTO>(addedObj.Entity);
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
            var obj = await contex.Bonuses.FirstOrDefaultAsync(u => u.Id == id);
            if (obj != null)
            {
                contex.Bonuses.Remove(obj);
                return contex.SaveChanges();
            }
            return 0;
        }

        public async Task<IEnumerable<BonusDTO>> GetAll(int? traitId = null)
        {
            using var contex = await _db.CreateDbContextAsync();
            if (traitId == null || traitId < 1)
                return _mapper.Map<IEnumerable<Bonus>, IEnumerable<BonusDTO>>(contex.Bonuses);
           return _mapper.Map<IEnumerable<Bonus>, IEnumerable<BonusDTO>>(contex.Bonuses.Where(u => u.TraitId == traitId).OrderBy(u=>u.Index));
        }

        public async Task<BonusDTO> GetById(int id)
        {
            using var contex = await _db.CreateDbContextAsync();
            var obj = await contex.Bonuses.FirstOrDefaultAsync(u => u.Id == id);
            if (obj != null)
            {
                return _mapper.Map<Bonus, BonusDTO>(obj);
            }
            return new BonusDTO();
        }

        public async Task<BonusDTO> Update(BonusDTO objDTO)
        {
            using var contex = await _db.CreateDbContextAsync();
            var obj = await contex.Bonuses.FirstOrDefaultAsync(u => u.Id == objDTO.Id);
            if (obj != null)
            {

                obj.FeatureType = objDTO.FeatureType;    
                obj.FeatureName = objDTO.FeatureName;       
                obj.BonusValue = objDTO.BonusValue;        
                obj.Description = objDTO.Description;     
                obj.Index = objDTO.Index;     
                obj.TraitId = objDTO.TraitId;
                contex.Bonuses.Update(obj);
                await contex.SaveChangesAsync();
                return _mapper.Map<Bonus, BonusDTO>(obj);
            }
            else
            {
                obj = _mapper.Map<BonusDTO, Bonus>(objDTO);
                var addedObj = contex.Bonuses.Add(obj);
                await contex.SaveChangesAsync();

                return _mapper.Map<Bonus, BonusDTO>(addedObj.Entity);
            }
        }
    }
}
