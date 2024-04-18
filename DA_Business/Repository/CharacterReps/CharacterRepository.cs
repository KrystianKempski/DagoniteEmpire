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
using System.Diagnostics;
using DagoniteEmpire.Exceptions;

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
            var obj = _mapper.Map<CharacterDTO, Character>(objDTO);
            using var contex = await _db.CreateDbContextAsync();
            try
            {
                obj.Race = null;

                //handle traits Adv
                var traits = await contex.TraitsAdv.ToListAsync();
                traits.ForEach(t =>
                {
                    if (obj.TraitsAdv.Any(nt => nt.Id == t.Id))
                    {
                        var untracked = obj.TraitsAdv.FirstOrDefault(nt => nt.Id == t.Id);
                        obj.TraitsAdv.Remove(untracked);
                        obj.TraitsAdv.Add(t);
                    }
                });

                var addedObj = contex.Characters.Add(obj);
                await contex.SaveChangesAsync();
                return _mapper.Map<Character, CharacterDTO>(addedObj.Entity);
            }
            catch (Exception ex)
            {
                throw new RepositoryErrorException("Error in Character Repository Create");
            }
        }

        public async Task<int> Delete(int id)
        {
            try
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
            }
            catch (Exception ex)
            {
                throw new RepositoryErrorException("Error in Character Repository Create");
            }
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
                                {
                                    var detachedTrait = contex.TraitsAdv.Include(t => t.Bonuses).Include(c => c.Characters).FirstOrDefault(c => c.Id == existingChild.Id && c.Id != default(int));
                                    if(detachedTrait != null && !detachedTrait.Characters.IsNullOrEmpty() && detachedTrait.Characters.Contains(obj))
                                    {
                                        detachedTrait.Characters.Remove(obj);
                                        contex.Traits.Update(detachedTrait);
                                    }
                                }
                                else
                                    contex.TraitsAdv.Remove(existingChild);
                            }

                        }
                    }

                    // Update and Insert traits
                    if (updatedChar.TraitsAdv is not null)
                    {
                        foreach (var trait in updatedChar.TraitsAdv)
                        {
                            TraitAdv? existingTrait = null;
                            if (obj.TraitsAdv is not null)
                            {
                                existingTrait = obj.TraitsAdv
                                    .FirstOrDefault(c => c.Id == trait.Id && c.Id != default(int));
                            }
                            else 
                            {   
                                obj.TraitsAdv = new List<TraitAdv>();
                            }

                            if (existingTrait == null)
                            {
                                existingTrait = contex.TraitsAdv.Include(t => t.Bonuses).Include(c=>c.Characters).FirstOrDefault(c => c.Id == trait.Id && c.Id != default(int));
                            }

                            if (existingTrait is not null)
                            {
                                if (existingTrait.TraitApproved && obj.TraitsAdv.Contains(existingTrait) == false)
                                {
                                    if (existingTrait.Characters is null)
                                        existingTrait.Characters = new List<Character>();
                                    existingTrait.Characters.Add(obj);
                                    contex.Traits.Update(existingTrait);
                                }
                                else
                                {
                                    // Update trait
                                    contex.Entry(existingTrait).CurrentValues.SetValues(trait);
                                    // update bonuses

                                    // Delete trait bonuses
                                    if (!existingTrait.Bonuses.IsNullOrEmpty())
                                    {
                                        foreach (var existingChildBonus in existingTrait.Bonuses.ToList())
                                        {
                                            if (!trait.Bonuses.Any(c => c.Id == existingChildBonus.Id))
                                            {
                                                contex.Bonuses.Remove(existingChildBonus);
                                            }
                                        }
                                    }

                                    // Update and Insert bonuses
                                    if (trait.Bonuses is not null)
                                    {
                                        foreach (var childBonus in trait.Bonuses)
                                        {
                                            Bonus? existingChildBonus;
                                            if (!existingTrait.Bonuses.IsNullOrEmpty())
                                            {
                                                existingChildBonus = existingTrait.Bonuses
                                               .FirstOrDefault(c => c.Id == childBonus.Id && c.Id != default(int));
                                            }
                                            else
                                            {
                                                existingTrait.Bonuses = new List<Bonus>();
                                                existingChildBonus = null;
                                            }

                                            if (existingChildBonus != null)
                                                // Update bonus
                                                contex.Entry(existingChildBonus).CurrentValues.SetValues(childBonus);
                                            else
                                                // Insert bonus
                                                existingTrait.Bonuses.Add(childBonus);
                                        }
                                    }
                                }
                            }
                            else
                                // Insert trait
                                obj.TraitsAdv.Add(trait);
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
                throw new RepositoryErrorException("Error in Character Repository Update");
            }
        }

    }
}
