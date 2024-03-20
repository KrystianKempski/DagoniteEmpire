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
        private readonly ApplicationDbContext _db;
        private readonly IMapper _mapper;

        public BonusRepository(ApplicationDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }
        public async Task<BonusDTO> Create(BonusDTO objDTO)
        {
            var obj = _mapper.Map<BonusDTO, Bonus>(objDTO);
            var addedObj = _db.Bonuses.Add(obj);
            await _db.SaveChangesAsync();

            return _mapper.Map<Bonus, BonusDTO>(addedObj.Entity);
        }

        public async Task<int> Delete(int id)
        {
            var obj = await _db.Bonuses.FirstOrDefaultAsync(u => u.Id == id);
            if (obj != null)
            {
                _db.Bonuses.Remove(obj);
                return _db.SaveChanges();
            }
            return 0;
        }

        public async Task<IEnumerable<BonusDTO>> GetAll(int? traitId = null)
        {
            if (traitId == null || traitId < 1)
                return _mapper.Map<IEnumerable<Bonus>, IEnumerable<BonusDTO>>(_db.Bonuses);
           return _mapper.Map<IEnumerable<Bonus>, IEnumerable<BonusDTO>>(_db.Bonuses.Where(u => u.TraitId == traitId).OrderBy(u=>u.Index));
        }

        public async Task<BonusDTO> GetById(int id)
        {
            var obj = await _db.Bonuses.FirstOrDefaultAsync(u => u.Id == id);
            if (obj != null)
            {
                return _mapper.Map<Bonus, BonusDTO>(obj);
            }
            return new BonusDTO();
        }

        public async Task<BonusDTO> Update(BonusDTO objDTO)
        {
            var obj = await _db.Bonuses.FirstOrDefaultAsync(u => u.Id == objDTO.Id);
            if (obj != null)
            {

                obj.FeatureType = objDTO.FeatureType;    
                obj.FeatureName = objDTO.FeatureName;       
                obj.BonusValue = objDTO.BonusValue;        
                obj.Description = objDTO.Description;     
                obj.Index = objDTO.Index;     
                obj.TraitId = objDTO.TraitId;  
                _db.Bonuses.Update(obj);
                await _db.SaveChangesAsync();
                return _mapper.Map<Bonus, BonusDTO>(obj);
            }
            else
            {
                obj = _mapper.Map<BonusDTO, Bonus>(objDTO);
                var addedObj = _db.Bonuses.Add(obj);
                await _db.SaveChangesAsync();

                return _mapper.Map<Bonus, BonusDTO>(addedObj.Entity);
            }
        }
    }
}
