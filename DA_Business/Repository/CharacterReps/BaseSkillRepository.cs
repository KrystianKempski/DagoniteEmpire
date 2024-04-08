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
    public class BaseSkillRepository : IBaseSkillRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly IMapper _mapper;

        public BaseSkillRepository(ApplicationDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }
        public async Task<BaseSkillDTO> Create(BaseSkillDTO objDTO)
        {
            try
            {
                var obj = _mapper.Map<BaseSkillDTO, BaseSkill>(objDTO);
                var addedObj = _db.BaseSkills.Add(obj);
                await _db.SaveChangesAsync();

                return _mapper.Map<BaseSkill, BaseSkillDTO>(addedObj.Entity);
            }
            catch (Exception ex)
            {
                ;
            }
            return null;
        }

        public async Task<int> Delete(int id)
        {
            var obj = await _db.BaseSkills.FirstOrDefaultAsync(u => u.Id == id);
            if (obj != null)
            {
                _db.BaseSkills.Remove(obj);
                return _db.SaveChanges();
            }
            return 0;
        }

        public async Task<IEnumerable<BaseSkillDTO>> GetAll(int? charId = null)
        {
            if (charId == null || charId < 1)
                return _mapper.Map<IEnumerable<BaseSkill>, IEnumerable<BaseSkillDTO>>(_db.BaseSkills/*.Include(u => u.TraitBonusRelated)*/);
            return _mapper.Map<IEnumerable<BaseSkill>, IEnumerable<BaseSkillDTO>>(_db.BaseSkills./*Include(u => u.TraitBonusRelated).*/Where(u => u.CharacterId == charId).OrderBy(u => u.Index));
        }

        public async Task<BaseSkillDTO> GetById(int id)
        {
            var obj = await _db.BaseSkills./*Include(u => u.TraitBonusRelated).*/FirstOrDefaultAsync(u => u.Id == id);
            if (obj != null)
            {
                return _mapper.Map<BaseSkill, BaseSkillDTO>(obj);
            }
            return new BaseSkillDTO();
        }

        public async Task<BaseSkillDTO> Update(BaseSkillDTO objDTO)
        {
            var obj = await _db.BaseSkills.FirstOrDefaultAsync(u => u.Id == objDTO.Id);
            if (obj != null)
            {
                obj.Name = objDTO.Name;
                obj.CharacterId = objDTO.CharacterId;        
                obj.OtherBonuses = objDTO.OtherBonuses;        
                obj.RaceBonus = objDTO.RaceBonus;  
                obj.BaseBonus = objDTO.BaseBonus;
                obj.GearBonus = objDTO.GearBonus;
                obj.TraitBonus = objDTO.TraitBonus;
                obj.TempBonuses = objDTO.TempBonuses;
                obj.RelatedAttribute1 = objDTO.RelatedAttribute1;
                obj.RelatedAttribute2 = objDTO.RelatedAttribute2;
                obj.Index = objDTO.Index;
                _db.BaseSkills.Update(obj);
                await _db.SaveChangesAsync();
                return _mapper.Map<BaseSkill,BaseSkillDTO>(obj);    
            }
            return objDTO;
        }
    }
}
