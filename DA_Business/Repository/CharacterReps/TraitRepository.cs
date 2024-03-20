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
    public class TraitRepository : ITraitRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly IMapper _mapper;

        public TraitRepository(ApplicationDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }
        public async Task<TraitDTO> Create(TraitDTO objDTO)
        {
            var obj = _mapper.Map<TraitDTO, Trait>(objDTO);
            var addedObj = _db.Traits.Add(obj);
            await _db.SaveChangesAsync();

            return _mapper.Map<Trait, TraitDTO>(addedObj.Entity);
        }

        public async Task<int> Delete(int id)
        {
            var obj = await _db.Traits.FirstOrDefaultAsync(u => u.Id == id);
            if (obj != null)
            {
                _db.Traits.Remove(obj);
                return _db.SaveChanges();
            }
            return 0;
        }

        public async Task<IEnumerable<TraitDTO>> GetAll(int? charId = null)
        {
            if (charId == null || charId < 1)
                return _mapper.Map<IEnumerable<Trait>, IEnumerable<TraitDTO>>(_db.Traits);
           return _mapper.Map<IEnumerable<Trait>, IEnumerable<TraitDTO>>(_db.Traits.Where(u => u.CharacterId == charId).OrderBy(u=>u.Index));
        }

        public async Task<TraitDTO> GetById(int id)
        {
            var obj = await _db.Traits.FirstOrDefaultAsync(u => u.Id == id);
            if (obj != null)
            {
                return _mapper.Map<Trait, TraitDTO>(obj);
            }
            return new TraitDTO();
        }

        public async Task<TraitDTO> Update(TraitDTO objDTO)
        {
            var obj = await _db.Traits.FirstOrDefaultAsync(u => u.Id == objDTO.Id);
            if (obj != null)
            {
                obj.Name = objDTO.Name;    
                obj.CharacterId = objDTO.CharacterId;       
                obj.Descr = objDTO.Descr;        
                obj.Index = objDTO.Index;  
                _db.Traits.Update(obj);
                await _db.SaveChangesAsync();
                return _mapper.Map<Trait, TraitDTO>(obj);
            }
            else
            {
                obj = _mapper.Map<TraitDTO, Trait>(objDTO);
                var addedObj = _db.Traits.Add(obj);
                await _db.SaveChangesAsync();

                return _mapper.Map<Trait, TraitDTO>(addedObj.Entity);
            }
        }
    }
}
