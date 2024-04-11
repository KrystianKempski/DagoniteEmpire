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
            // handle race
            if (obj.RaceId > 0)
            {
                obj.Race = _db.Races.FirstOrDefault(r => r.Id == obj.RaceId);
            }
            else
            {
                obj.Race = null;
                obj.RaceId = 0;
            }
            //handle traitsAdv

            var traits = await _db.TraitsAdv.ToListAsync();
            traits.ForEach(t =>
            {
                if (obj.TraitsAdv.Any(ncht => ncht.Id == t.Id))
                {
                    var untracked = obj.TraitsAdv.FirstOrDefault(ncht => ncht.Id == t.Id);
                    obj.TraitsAdv.Remove(untracked);
                    obj.TraitsAdv.Add(t);
                }
            });


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
                return _mapper.Map<IEnumerable<Character>, IEnumerable<CharacterDTO>>(_db.Characters.Include(r => r.TraitsAdv).Include(r => r.Race));
            return _mapper.Map<IEnumerable<Character>, IEnumerable<CharacterDTO>>(_db.Characters.Include(r => r.TraitsAdv).Include(r => r.Race).Where(u=>u.Id==id));
        }

        public async Task<CharacterDTO> GetById(int id)
        {
            var obj = await _db.Characters.Include(t => t.TraitsAdv).Include(r=>r.Race).FirstOrDefaultAsync(u => u.Id == id);
            if (obj != null)
            {
                return _mapper.Map<Character, CharacterDTO>(obj);
            }
            return new CharacterDTO();
        }
        public async Task<IEnumerable<CharacterDTO>> GetAllForUser(string userName)
        {
            if (userName == null || userName.Length<3)
                return null;
            return _mapper.Map<IEnumerable<Character>, IEnumerable<CharacterDTO>>(_db.Characters.Include(r => r.Race).Include(r => r.Race).Where(u => u.UserName == userName));
        }

        public async Task<CharacterDTO> Update(CharacterDTO objDTO)
        {
            var obj = await _db.Characters.FirstOrDefaultAsync(u => u.Id == objDTO.Id);
            if (obj != null)
            {
                var updatedChar = _mapper.Map<CharacterDTO, Character>(objDTO);
                var traits = await _db.TraitsAdv.ToListAsync();

                obj.Age = updatedChar.Age;
                obj.NPCName = updatedChar.NPCName;
                obj.Description = updatedChar.Description;
                obj.Class = updatedChar.Class;
                obj.CurrentExpPoints = updatedChar.CurrentExpPoints;
                obj.UsedExpPoints = updatedChar.UsedExpPoints;
                obj.AttributePoints = updatedChar.AttributePoints;
                obj.NPCType = updatedChar.NPCType;
                obj.ImageUrl = updatedChar.ImageUrl;
                obj.TraitBalance = updatedChar.TraitBalance;
                obj.RaceId = updatedChar.RaceId;
               //obj.TraitsAdv = updatedChar.TraitsAdv;



                // race
                // handle race
                if (obj.RaceId > 0)
                {
                    obj.Race = _db.Races.FirstOrDefault(r => r.Id == obj.RaceId);
                }

                //handle traitsAdv
                    
                foreach (var t in traits)
                {
                    if (updatedChar.TraitsAdv.Any(ut => ut.Id == t.Id)
                        && !obj.TraitsAdv.Any(ot => ot.Id == t.Id))
                    {
                        obj.TraitsAdv.Add(t);
                    }
                }

                //foreach(var t in traits)
                //{
                //    if (obj.TraitsAdv.Any(ncht => ncht.Id == t.Id))
                //    {
                //        var untracked = obj.TraitsAdv.FirstOrDefault(ncht => ncht.Id == t.Id);
                //        obj.TraitsAdv.Remove(untracked);
                //        obj.TraitsAdv.Add(t);
                //    }
                //}

                //traits.ForEach(t =>
                //{
                //    if (obj.TraitsAdv.Any(ncht => ncht.Id == t.Id))
                //    {
                //        var untracked = obj.TraitsAdv.FirstOrDefault(ncht => ncht.Id == t.Id);
                //        obj.TraitsAdv.Remove(untracked);
                //        obj.TraitsAdv.Add(t);
                //    }
                //});

                _db.Characters.Update(obj);
                await _db.SaveChangesAsync();
            }
            return objDTO;
        }

    }
}
