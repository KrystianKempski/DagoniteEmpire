﻿using AutoMapper;
using DA_Business.Repository.CharacterReps.IRepository;
using DA_DataAccess.CharacterClasses;
using DA_DataAccess.Data;
using DA_Models.CharacterModels;
using DagoniteEmpire.Exceptions;
using Microsoft.EntityFrameworkCore;
using Attribute = DA_DataAccess.CharacterClasses.Attribute;

namespace DA_Business.Repository.CharacterReps
{
    public class AttributeRepository : IAttributeRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly IMapper _mapper;

        public AttributeRepository(ApplicationDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<AttributeDTO> Create(AttributeDTO objDTO)
        {
            var obj = _mapper.Map<AttributeDTO, Attribute>(objDTO);
            var addedObj = _db.Attributes.Add(obj);
            try
            {
                await _db.SaveChangesAsync();
            }
            catch (Exception ex) { throw new RepositoryErrorException("Error in" + System.Reflection.MethodBase.GetCurrentMethod().Name); }
            return _mapper.Map<Attribute, AttributeDTO>(addedObj.Entity);
        }

        public async Task<int> Delete(int id)
        {
            var obj = await _db.Attributes.FirstOrDefaultAsync(u => u.Id == id);
            if (obj != null)
            {
                _db.Attributes.Remove(obj);
                return _db.SaveChanges();
            }
            return 0;
        }

        public async Task<IEnumerable<AttributeDTO>> GetAll(int? charId = null)
        {
            if(charId == null || charId < 1)
                return _mapper.Map<IEnumerable<Attribute>, IEnumerable<AttributeDTO>>(_db.Attributes/*.Include(u => u.TraitBonusRelated)*/);
            try
            {
                var obj = _db.Attributes./*Include(u => u.TraitBonusRelated).*/Where(u => u.CharacterId == charId).OrderBy(u => u.Index);
                if (obj != null && obj.Any())
                    return _mapper.Map<IEnumerable<Attribute>, IEnumerable<AttributeDTO>>(obj);
            }
            catch (Exception ex) { throw new RepositoryErrorException("Error in" + System.Reflection.MethodBase.GetCurrentMethod().Name); }


            return new List<AttributeDTO>();
        }

        public async Task<AttributeDTO> GetById(int id)
        {
            var obj = await _db.Attributes/*.Include(u => u.TraitBonusRelated)*/.FirstOrDefaultAsync(u => u.Id == id);
            if (obj != null)
            {
                return _mapper.Map<Attribute, AttributeDTO>(obj);
            }
            return new AttributeDTO();
        }

        public async Task<AttributeDTO> Update(AttributeDTO objDTO)
        {
            var obj = await _db.Attributes.FirstOrDefaultAsync(u => u.Id == objDTO.Id);
            if (obj != null)
            {
                obj.CharacterId = objDTO.CharacterId;        //is it nessesary?
                obj.OtherBonuses = objDTO.OtherBonuses;        //is it nessesary?
                obj.RaceBonus = objDTO.RaceBonus;  //is it nessesary?
                obj.BaseBonus = objDTO.BaseBonus;
                obj.HealthBonus = objDTO.HealthBonus;
                obj.GearBonus = objDTO.GearBonus;
                obj.Index = objDTO.Index;
                _db.Attributes.Update(obj);
                await _db.SaveChangesAsync();
                return _mapper.Map<Attribute, AttributeDTO>(obj);
            }
            return objDTO;
        }
    }
}
