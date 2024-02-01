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
            var obj = _mapper.Map<BaseSkillDTO, BaseSkill>(objDTO);
            var addedObj = _db.BaseSkills.Add(obj);
            await _db.SaveChangesAsync();

            return _mapper.Map<BaseSkill, BaseSkillDTO>(addedObj.Entity);
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
                return _mapper.Map<IEnumerable<BaseSkill>, IEnumerable<BaseSkillDTO>>(_db.BaseSkills);
            return _mapper.Map<IEnumerable<BaseSkill>, IEnumerable<BaseSkillDTO>>(_db.BaseSkills.Where(u => u.CharacterId == charId));
        }

        public async Task<BaseSkillDTO> GetById(int id)
        {
            var obj = await _db.BaseSkills.FirstOrDefaultAsync(u => u.Id == id);
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
                obj.CharacterId = objDTO.CharacterId;        //is it nessesary?
                obj.OtherBonuses = objDTO.OtherBonuses;        //is it nessesary?
                obj.RaceBonus = objDTO.RaceBonus;  //is it nessesary?
                obj.BaseBonus = objDTO.BaseBonus;
                obj.GearBonus = objDTO.GearBonus;
                obj.TempBonuses = objDTO.TempBonuses;
                _db.BaseSkills.Update(obj);
                await _db.SaveChangesAsync();
                return _mapper.Map<BaseSkill,BaseSkillDTO>(obj);    
            }
            return objDTO;
        }
    }
}
