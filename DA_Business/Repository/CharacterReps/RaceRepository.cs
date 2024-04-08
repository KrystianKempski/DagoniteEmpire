using Abp.Collections.Extensions;
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
    public class RaceRepository : IRaceRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly IMapper _mapper;

        public RaceRepository(ApplicationDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }
        public async Task<RaceDTO> Create(RaceDTO objDTO)
        {
            try
            {

                var obj = _mapper.Map<RaceDTO, Race>(objDTO);
                var addedObj = _db.Races.Add(obj);
                _db.SaveChangesAsync();

                return _mapper.Map<Race, RaceDTO>(addedObj.Entity);
            }
            catch (Exception ex)
            {
                ;
            }
            return null;
                
}

        public async Task<int> Delete(int id)
        {
            try
            {
                var obj = await _db.Races.FirstOrDefaultAsync(u => u.Id == id);
            if (obj != null)
            {
                _db.Races.Remove(obj);
                return _db.SaveChanges();
            }
            return 0;
            }
            catch (Exception ex)
            {
                ;
            }
            return 0;
        }

        public async Task<IEnumerable<RaceDTO>> GetAll(int? charId = null)
        {
            if (charId == null || charId < 1)
                return _mapper.Map<IEnumerable<Race>, IEnumerable<RaceDTO>>(_db.Races.Include(u => u.Traits));
           return _mapper.Map<IEnumerable<Race>, IEnumerable<RaceDTO>>(_db.Races.Include(u => u.Traits).Where(u => u.CharacterId == charId).OrderBy(u=>u.Index));
        }

        public async Task<IEnumerable<RaceDTO>> GetAllApproved()
        {
            return _mapper.Map<IEnumerable<Race>, IEnumerable<RaceDTO>>(_db.Races.Include(u => u.Traits).Where(t=>t.RaceApproved == true));
        }

        public async Task<RaceDTO> GetById(int id)
        {
            var obj = await _db.Races.Include(u => u.Traits).FirstOrDefaultAsync(u => u.Id == id);
            if (obj != null)
            {
                return _mapper.Map<Race, RaceDTO>(obj);
            }
            return new RaceDTO();
        }

        public async Task<RaceDTO> Update(RaceDTO objDTO)
        {
            try
            {
                var obj = await _db.Races.FirstOrDefaultAsync(u => u.Id == objDTO.Id);
                if (obj != null)
                {
                    obj.Name = objDTO.Name;    
                    obj.CharacterId = objDTO.CharacterId;       
                    obj.Description = objDTO.Description;        
                    obj.Index = objDTO.Index;
                    obj.RaceApproved = objDTO.RaceApproved;
                    _db.Races.Update(obj);

                    await _db.SaveChangesAsync();
                    return _mapper.Map<Race, RaceDTO>(obj);
                }
                else
                {
                    obj = _mapper.Map<RaceDTO, Race>(objDTO);
                    var addedObj = _db.Races.Add(obj);
                    await _db.SaveChangesAsync();

                    return _mapper.Map<Race, RaceDTO>(addedObj.Entity);
                }
            }
            catch (Exception ex)
            {
                ;
            }
            return null;
        }
    }
}
