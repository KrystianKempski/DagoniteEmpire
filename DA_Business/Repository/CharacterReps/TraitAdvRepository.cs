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
    public class TraitAdvRepository : ITraitAdvRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly IMapper _mapper;

        public TraitAdvRepository(ApplicationDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }
        public async Task<TraitAdvDTO> Create(TraitAdvDTO objDTO)
        {
            try
            {

                var obj = _mapper.Map<TraitAdvDTO, TraitAdv>(objDTO);
                var addedObj = _db.TraitsAdv.Add(obj);
                await _db.SaveChangesAsync();

                return _mapper.Map<TraitAdv, TraitAdvDTO>(addedObj.Entity);
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
                var obj = await _db.TraitsAdv.FirstOrDefaultAsync(u => u.Id == id);
            if (obj != null)
            {
                _db.TraitsAdv.Remove(obj);
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

        public async Task<IEnumerable<TraitAdvDTO>> GetAll(int? charId =null)
        {
            if (charId == null || charId < 1)
                return _mapper.Map<IEnumerable<TraitAdv>, IEnumerable<TraitAdvDTO>>(_db.TraitsAdv.Include(u => u.Bonuses));
           return _mapper.Map<IEnumerable<TraitAdv>, IEnumerable<TraitAdvDTO>>(_db.TraitsAdv.Include(u => u.Bonuses).Where(u => u.Characters.FirstOrDefault(c=>c.Id == charId)!=null));
        }

        public async Task<IEnumerable<TraitAdvDTO>> GetAllApproved()
        {
            return _mapper.Map<IEnumerable<TraitAdv>, IEnumerable<TraitAdvDTO>>(_db.TraitsAdv.Include(u => u.Bonuses).Where(t=>t.TraitApproved==true));
        }

        public async Task<TraitAdvDTO> GetById(int id)
        {
            var obj = await _db.TraitsAdv.Include(u=>u.Bonuses).FirstOrDefaultAsync(u => u.Id == id);
            if (obj != null)
            {
                return _mapper.Map<TraitAdv, TraitAdvDTO>(obj);
            }
            return new TraitAdvDTO();
        }

        public async Task<TraitAdvDTO> Update(TraitAdvDTO objDTO)
        {
            try
            {
                var obj = await _db.TraitsAdv.FirstOrDefaultAsync(u => u.Id == objDTO.Id);
                if (obj != null)
                {
                    obj.Name = objDTO.Name;    
                   // obj.CharacterId = objDTO.CharacterId;       
                    obj.Descr = objDTO.Descr;
                    obj.SummaryDescr = objDTO.SummaryDescr;
                    obj.Index = objDTO.Index;  
                    obj.TraitType = objDTO.TraitType;
                    obj.TraitValue = objDTO.TraitValue;
                    obj.TraitApproved = objDTO.TraitApproved;
                    obj.IsUnique = objDTO.IsUnique; 
                    _db.TraitsAdv.Update(obj);
                    await _db.SaveChangesAsync();
                    return _mapper.Map<TraitAdv, TraitAdvDTO>(obj);
                }
                else
                {
                    obj = _mapper.Map<TraitAdvDTO, TraitAdv>(objDTO);
                    var addedObj = _db.TraitsAdv.Add(obj);
                    await _db.SaveChangesAsync();

                    return _mapper.Map<TraitAdv, TraitAdvDTO>(addedObj.Entity);
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
