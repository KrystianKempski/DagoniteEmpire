using AutoMapper;
using DA_Business.Repository.CharacterReps.IRepository;
using DA_DataAccess.Data;
using DA_DataAccess.CharacterClasses;
using DA_Models.CharacterModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Attribute = DA_DataAccess.CharacterClasses.Attribute;

namespace DA_Business.Repository.CharacterReps
{
    public class CharacterRepository : ICharacterRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly IMapper _mapper;

        public CharacterRepository(ApplicationDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }
        public async Task<CharacterDTO> Create(CharacterDTO objDTO)
        {
            var obj = _mapper.Map<CharacterDTO, Character>(objDTO);
            var addedObj = _db.Characters.Add(obj);
            await _db.SaveChangesAsync();

            return _mapper.Map<Character, CharacterDTO>(addedObj.Entity);
        }

        public async Task<int> Delete(int id)
        {
            var obj = await _db.Characters.FirstOrDefaultAsync(u => u.Id == id);
            if (obj != null)
            {
                _db.Characters.Remove(obj);
                return _db.SaveChanges();
            }
            return 0;
        }

        public async Task<IEnumerable<CharacterDTO>> GetAll(int? id=null)
        {
            if(id == null || id < 1)
                return _mapper.Map<IEnumerable<Character>, IEnumerable<CharacterDTO>>(_db.Characters);
            return _mapper.Map<IEnumerable<Character>, IEnumerable<CharacterDTO>>(_db.Characters.Where(u=>u.Id==id));
        }

        public async Task<CharacterDTO> GetById(int id)
        {
            var obj = await _db.Characters.FirstOrDefaultAsync(u => u.Id == id);
            if (obj != null)
            {
                return _mapper.Map<Character, CharacterDTO>(obj);
            }
            return new CharacterDTO();
        }

        public async Task<CharacterDTO> Update(CharacterDTO objDTO)
        {
            var obj = await _db.Characters.FirstOrDefaultAsync(u => u.Id == objDTO.Id);
            if (obj != null)
            {
                obj.Attributes = _mapper.Map<ICollection<AttributeDTO>, ICollection<Attribute>>(objDTO.Attributes);
                obj.BaseSkills = _mapper.Map<ICollection<BaseSkillDTO>, ICollection<BaseSkill>>(objDTO.BaseSkills);
                obj.SpecialSkills = _mapper.Map<ICollection<SpecialSkillDTO>, ICollection<SpecialSkill>>(objDTO.SpecialSkills);

                obj.Age = objDTO.Age;
                obj.NPCName = objDTO.NPCName;
                obj.Race = objDTO.Race;
                obj.Class = objDTO.Class;
                _db.Characters.Update(obj);
                await _db.SaveChangesAsync();
               // return _mapper.Map<Object, CharacterDTO>(obj);
            }
            return objDTO;
        }

        private IEnumerable<Attribute> GetAttributes(int charId)
        {
            var attributes = new List<Attribute>()
            {
                new Attribute()
                {
                    CharacterId= charId,
                    Name = "Strength",
                    BaseBonus = 6,
                    GearBonus = 0,
                    HealthBonus = 0,
                    RaceBonus = 0,
                    OtherBonuses = { },
                },
                new Attribute()
                {
                    CharacterId= charId,
                    Name = "Dexterity",
                    BaseBonus = 6,
                    GearBonus = 0,
                    HealthBonus = 0,
                    RaceBonus = 0,
                    OtherBonuses = { },
                },
                new Attribute()
                {
                    CharacterId= charId,
                    Name = "Endurance",
                    BaseBonus = 6,
                    GearBonus = 0,
                    HealthBonus = 0,
                    RaceBonus = 0,
                    OtherBonuses = { },
                },
                new Attribute()
                {
                    CharacterId= charId,
                    Name = "Intelligence",
                    BaseBonus = 6,
                    GearBonus = 0,
                    HealthBonus = 0,
                    RaceBonus = 0,
                    OtherBonuses = { },
                },
                new Attribute()
                {
                    CharacterId= charId,
                    Name = "Instinct",
                    BaseBonus = 6,
                    GearBonus = 0,
                    HealthBonus = 0,
                    RaceBonus = 0,
                    OtherBonuses = { },
                },
                new Attribute()
                {
                    CharacterId= charId,
                    Name = "Willpower",
                    BaseBonus = 6,
                    GearBonus = 0,
                    HealthBonus = 0,
                    RaceBonus = 0,
                    OtherBonuses = { },
                },
                new Attribute()
                {
                    CharacterId= charId,
                    Name = "Charisma",
                    BaseBonus = 6,
                    GearBonus = 0,
                    HealthBonus = 0,
                    RaceBonus = 0,
                    OtherBonuses = { },
                },
            };
            return attributes;
        }
    }
}
