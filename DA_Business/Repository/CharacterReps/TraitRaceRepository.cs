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
    public class TraitRaceRepository : ITraitRaceRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly IMapper _mapper;

        public TraitRaceRepository(ApplicationDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }
        public async Task<TraitRaceDTO> Create(TraitRaceDTO objDTO)
        {
            try
            {
                //How to use EF 6 many-to-many with IDbContextFactory in Blazor Server
                var obj = _mapper.Map<TraitRaceDTO, TraitRace>(objDTO);
                var addedObj = _db.TraitsRace.Add(obj);
                await _db.SaveChangesAsync();

                return _mapper.Map<TraitRace, TraitRaceDTO>(addedObj.Entity);
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
                var obj = await _db.TraitsRace.FirstOrDefaultAsync(u => u.Id == id);
            if (obj != null)
            {
                _db.TraitsRace.Remove(obj);
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

        public async Task<IEnumerable<TraitRaceDTO>> GetAll(int? raceId = null)
        {
            if (raceId == null || raceId < 1)
                return _mapper.Map<IEnumerable<TraitRace>, IEnumerable<TraitRaceDTO>>(_db.TraitsRace.Include(u => u.Bonuses));
           return _mapper.Map<IEnumerable<TraitRace>, IEnumerable<TraitRaceDTO>>(_db.TraitsRace.Include(u => u.Bonuses).Where(u => u.Races.FirstOrDefault(r=>r.Id == raceId) != null));
        }

        public async Task<IEnumerable<TraitRaceDTO>> GetAllApproved()
        {
            return _mapper.Map<IEnumerable<TraitRace>, IEnumerable<TraitRaceDTO>>(_db.TraitsRace.Include(u => u.Bonuses).Where(t=>t.TraitApproved==true));
        }

        public async Task<TraitRaceDTO> GetById(int id)
        {
            var obj = await _db.TraitsRace.Include(u=>u.Bonuses).FirstOrDefaultAsync(u => u.Id == id);
            if (obj != null)
            {
                return _mapper.Map<TraitRace, TraitRaceDTO>(obj);
            }
            return new TraitRaceDTO();
        }

        public async Task<TraitRaceDTO> Update(TraitRaceDTO objDTO)
        {
            try
            {
                var obj = await _db.TraitsRace.FirstOrDefaultAsync(u => u.Id == objDTO.Id);
                if (obj != null)
                {
                    obj.Name = objDTO.Name;
                    //obj.RaceId = objDTO.RaceId;
                    obj.Descr = objDTO.Descr;
                    obj.SummaryDescr = objDTO.SummaryDescr;
                    obj.Index = objDTO.Index;
                    obj.TraitType = objDTO.TraitType;
                    obj.TraitValue = objDTO.TraitValue;
                    obj.TraitApproved = objDTO.TraitApproved;
                    obj.IsUnique = objDTO.IsUnique;
                    //obj = _mapper.Map<TraitRaceDTO, TraitRace>(objDTO);
                    var updatedObj = _db.TraitsRace.Update(obj);
                    await _db.SaveChangesAsync();
                    return _mapper.Map<TraitRace, TraitRaceDTO>(updatedObj.Entity);
                }
                else
                {
                    obj = _mapper.Map<TraitRaceDTO, TraitRace>(objDTO);
                    var addedObj = _db.TraitsRace.Add(obj);
                    await _db.SaveChangesAsync();

                    return _mapper.Map<TraitRace, TraitRaceDTO>(addedObj.Entity);
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
