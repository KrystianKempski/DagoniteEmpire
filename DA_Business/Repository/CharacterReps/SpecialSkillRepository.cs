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
    public class SpecialSkillRepository : ISpecialSkillRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly IMapper _mapper;

        public SpecialSkillRepository(ApplicationDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }
        public async Task<SpecialSkillDTO> Create(SpecialSkillDTO objDTO)
        {
            var obj = _mapper.Map<SpecialSkillDTO, SpecialSkill>(objDTO);
            var addedObj = _db.SpecialSkills.Add(obj);
            await _db.SaveChangesAsync();

            return _mapper.Map<SpecialSkill, SpecialSkillDTO>(addedObj.Entity);
        }

        public async Task<int> Delete(int id)
        {
            var obj = await _db.SpecialSkills.FirstOrDefaultAsync(u => u.Id == id);
            if (obj != null)
            {
                _db.SpecialSkills.Remove(obj);
                return _db.SaveChanges();
            }
            return 0;
        }

        public async Task<IEnumerable<SpecialSkillDTO>> GetAll(int? charId = null)
        {
            if (charId == null || charId < 1)
                return _mapper.Map<IEnumerable<SpecialSkill>, IEnumerable<SpecialSkillDTO>>(_db.SpecialSkills/*.Include(u => u.TraitBonusRelated)*/);
           return _mapper.Map<IEnumerable<SpecialSkill>, IEnumerable<SpecialSkillDTO>>(_db.SpecialSkills/*.Include(u => u.TraitBonusRelated)*/.Where(u => u.CharacterId == charId).OrderBy(u=>u.Index));
        }

        public async Task<SpecialSkillDTO> GetById(int id)
        {
            var obj = await _db.SpecialSkills./*Include(u => u.TraitBonusRelated).*/FirstOrDefaultAsync(u => u.Id == id);
            if (obj != null)
            {
                return _mapper.Map<SpecialSkill, SpecialSkillDTO>(obj);
            }
            return new SpecialSkillDTO();
        }

        public async Task<SpecialSkillDTO> Update(SpecialSkillDTO objDTO)
        {
            var obj = await _db.SpecialSkills.FirstOrDefaultAsync(u => u.Id == objDTO.Id);
            if (obj != null)
            {
                obj.Name = objDTO.Name;    
                obj.CharacterId = objDTO.CharacterId;       
                obj.OtherBonuses = objDTO.OtherBonuses;        
                obj.RaceBonus = objDTO.RaceBonus;  
                obj.BaseBonus = objDTO.BaseBonus;
                obj.GearBonus = objDTO.GearBonus;
                obj.TempBonuses = objDTO.TempBonuses;
                obj.RelatedBaseSkillName = objDTO.RelatedBaseSkillName;
                obj.ChosenAttribute = objDTO.ChosenAttribute;
                obj.Editable = objDTO.Editable;
                obj.Index = objDTO.Index; 
                _db.SpecialSkills.Update(obj);
                await _db.SaveChangesAsync();
                return _mapper.Map<SpecialSkill, SpecialSkillDTO>(obj);
            }
            else
            {
                obj = _mapper.Map<SpecialSkillDTO, SpecialSkill>(objDTO);
                var addedObj = _db.SpecialSkills.Add(obj);
                await _db.SaveChangesAsync();

                return _mapper.Map<SpecialSkill, SpecialSkillDTO>(addedObj.Entity);
            }
        }
    }
}
