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
using System.Diagnostics.Metrics;
using Abp.Collections.Extensions;

namespace DA_Business.Repository.CharacterReps
{
    public class CharacterRepository : ICharacterRepository
    {
        //private readonly ApplicationDbContext _db;
        private readonly IDbContextFactory<ApplicationDbContext> _db;
        private readonly IMapper _mapper;

        public CharacterRepository(IDbContextFactory<ApplicationDbContext> db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }
        public async Task<CharacterDTO> Create(CharacterDTO objDTO)
        {
            var objMapped = _mapper.Map<CharacterDTO, Character>(objDTO);
            using var contex = await _db.CreateDbContextAsync();
            var obj = await contex.Characters.Include(u => u.TraitsAdv).FirstOrDefaultAsync(c=>c.Id == objMapped.Id);
            try
            {


                if (obj is null)
                {


                    // handle race
                    //if (obj.RaceId > 0)
                    //{
                    //    obj.Race = contex.Races.FirstOrDefault(r => r.Id == obj.RaceId);
                    //}
                    //else
                    //{
                    //    obj.Race = null;
                    //    obj.RaceId = 0;
                    //}
                    //handle traitsAdv
                    objMapped.Race = null;

                    var traits = await contex.TraitsAdv.ToListAsync();
                    traits.ForEach(t =>
                    {
                        if (obj.TraitsAdv.Any(ncht => ncht.Id == t.Id))
                        {
                            var untracked = obj.TraitsAdv.FirstOrDefault(ncht => ncht.Id == t.Id);
                            obj.TraitsAdv.Remove(untracked);
                            obj.TraitsAdv.Add(t);
                        }
                    });
                }

                var addedObj = contex.Characters.Add(objMapped);
                await contex.SaveChangesAsync();
                return _mapper.Map<Character, CharacterDTO>(addedObj.Entity);
            }
            catch (Exception ex)
            {
                ;
            }

            return null;
        }

        public async Task<int> Delete(int id)
        {
            using var contex = await _db.CreateDbContextAsync();
            var obj = await contex.Characters.FirstOrDefaultAsync(u => u.Id == id);
            var race = await contex.Races.FirstOrDefaultAsync(u => u.Id == obj.RaceId && u.RaceApproved == false);
            if (race is not null)
            {
                contex.Races.Remove(race);
            }

            if (obj is not null)
            {
                contex.Characters.Remove(obj);
            }
            return contex.SaveChanges();
            return 0;
        }

        public async Task<IEnumerable<CharacterDTO>> GetAll(int? id=null)
        {
            using var contex = await _db.CreateDbContextAsync();
            if (id == null || id < 1)
                return _mapper.Map<IEnumerable<Character>, IEnumerable<CharacterDTO>>(contex.Characters.Include(r => r.TraitsAdv).Include(r => r.Race));
            return _mapper.Map<IEnumerable<Character>, IEnumerable<CharacterDTO>>(contex.Characters.Include(r => r.TraitsAdv).Include(r => r.Race).Where(u=>u.Id==id));
        }

        public async Task<CharacterDTO> GetById(int id)
        {
            using var contex = await _db.CreateDbContextAsync();
            var obj = await contex.Characters.Include(t => t.TraitsAdv).ThenInclude(b=>b.Bonuses).Include(r=>r.Race).FirstOrDefaultAsync(u => u.Id == id);
            if (obj != null)
            {
                return _mapper.Map<Character, CharacterDTO>(obj);
            }
            return new CharacterDTO();
        }
        public async Task<IEnumerable<CharacterDTO>> GetAllForUser(string userName)
        {
            using var contex = await _db.CreateDbContextAsync();
            if (userName == null || userName.Length<3)
                return null;
            return _mapper.Map<IEnumerable<Character>, IEnumerable<CharacterDTO>>(contex.Characters.Include(r => r.Race).Include(r => r.Race).Where(u => u.UserName == userName));
        }

        public async Task<CharacterDTO> Update(CharacterDTO objDTO)
        {
            try
            {
                using var contex = await _db.CreateDbContextAsync();
                var obj = await contex.Characters.Include(u => u.TraitsAdv).FirstOrDefaultAsync(u => u.Id == objDTO.Id);
                if (obj != null)
                {
                    var updatedChar = _mapper.Map<CharacterDTO, Character>(objDTO);
                    var traits = await contex.TraitsAdv.ToListAsync();

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

                    // Delete adv traits
                    if(obj.TraitsAdv is not null)
                    {
                        foreach (var existingChild in obj.TraitsAdv.ToList())
                        {
                            if (!updatedChar.TraitsAdv.Any(c => c.Id == existingChild.Id))
                            {
                                if (existingChild.TraitApproved == true)
                                    obj.TraitsAdv.Remove(existingChild);
                                else
                                    contex.TraitsAdv.Remove(existingChild);
                            }

                        }
                    }

                    // Update and Insert traits
                    if (updatedChar.TraitsAdv is not null)
                    {
                        foreach (var childTrait in updatedChar.TraitsAdv)
                        {
                            TraitAdv? existingChild;
                            if (!obj.TraitsAdv.IsNullOrEmpty())
                            {
                                existingChild = obj.TraitsAdv
                               .Where(c => c.Id == childTrait.Id && c.Id != default(int))
                               .SingleOrDefault();
                            }
                            else
                            {
                                obj.TraitsAdv = new List<TraitAdv>();
                                existingChild = null;
                            }

                            if (existingChild != null)
                            {
                                // Update trait
                                contex.Entry(existingChild).CurrentValues.SetValues(childTrait);
                                // update bonuses

                                // Delete trait bonuses
                                if (!existingChild.Bonuses.IsNullOrEmpty())
                                {
                                    foreach (var existingChildBonus in existingChild.Bonuses.ToList())
                                    {
                                        if (!childTrait.Bonuses.Any(c => c.Id == existingChildBonus.Id))
                                        {
                                            contex.Bonuses.Remove(existingChildBonus);
                                        }
                                    }
                                }

                                // Update and Insert bonuses
                                if (childTrait.Bonuses is not null)
                                {
                                    foreach (var childTraitBonus in childTrait.Bonuses)
                                    {
                                        Bonus? existingChildTrait;
                                        if (!existingChild.Bonuses.IsNullOrEmpty())
                                        {
                                            existingChildTrait = existingChild.Bonuses
                                           .Where(c => c.Id == childTraitBonus.Id && c.Id != default(int))
                                           .SingleOrDefault();
                                        }
                                        else
                                        {
                                            existingChild.Bonuses = new List<Bonus>();
                                            existingChildTrait = null;
                                        }

                                        if (existingChildTrait != null)
                                            // Update bonus
                                            contex.Entry(existingChildTrait).CurrentValues.SetValues(childTraitBonus);
                                        else
                                            // Insert bonus
                                            existingChild.Bonuses.Add(childTraitBonus);
                                    }
                                }
                            }
                            else
                                // Insert trait
                                obj.TraitsAdv.Add(childTrait);
                        }
                    }


                    var result = contex.Characters.Update(obj);
                    await contex.SaveChangesAsync();
                    return _mapper.Map<Character, CharacterDTO>(result.Entity);
                }else
                    return objDTO;
            }
            catch (Exception ex)
            {
                ;
            }
            return null;
        }

    }
}
