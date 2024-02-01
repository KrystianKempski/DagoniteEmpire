﻿using AutoMapper;
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
                return _mapper.Map<IEnumerable<SpecialSkill>, IEnumerable<SpecialSkillDTO>>(_db.SpecialSkills);
            return _mapper.Map<IEnumerable<SpecialSkill>, IEnumerable<SpecialSkillDTO>>(_db.SpecialSkills.Where(u => u.CharacterId == charId));
        }

        public async Task<SpecialSkillDTO> GetById(int id)
        {
            var obj = await _db.SpecialSkills.FirstOrDefaultAsync(u => u.Id == id);
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
                obj.CharacterId = objDTO.CharacterId;        //is it nessesary?
                obj.OtherBonuses = objDTO.OtherBonuses;        //is it nessesary?
                obj.RaceBonus = objDTO.RaceBonus;  //is it nessesary?
                obj.BaseBonus = objDTO.BaseBonus;
                obj.AtributeId = objDTO.AtributeId;
                obj.GearBonus = objDTO.GearBonus;
                obj.TempBonuses = objDTO.TempBonuses;
                _db.SpecialSkills.Update(obj);
                await _db.SaveChangesAsync();
                return _mapper.Map<SpecialSkill, SpecialSkillDTO>(obj);
            }
            return objDTO;
        }
    }
}
