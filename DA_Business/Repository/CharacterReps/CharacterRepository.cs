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
using DA_Common;

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
                obj.Profession = null;

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
                //handle equipment
                var equipment = await contex.Equipment.ToListAsync();
                equipment.ForEach(e =>
                {
                    if (obj.Equipment.Any(nt => nt.Id == e.Id))
                    {
                        var untracked = obj.Equipment.FirstOrDefault(nt => nt.Id == e.Id);
                        obj.Equipment.Remove(untracked);
                        obj.Equipment.Add(e);
                    }
                });

                var addedObj = contex.Characters.Add(obj);
                await contex.SaveChangesAsync();
                return _mapper.Map<Character, CharacterDTO>(addedObj.Entity);
            }
            catch (Exception ex) {throw new RepositoryErrorException("Error in"+ System.Reflection.MethodBase.GetCurrentMethod().Name); }
        }

        public async Task<int> Delete(int id)
        {
            try
            {
                using var contex = await _db.CreateDbContextAsync();
                //delete traits adv
                var obj = await contex.Characters.Include(c=>c.TraitsAdv).Include(c=>c.Equipment).FirstOrDefaultAsync(u => u.Id == id);
                if (!obj.TraitsAdv.IsNullOrEmpty())
                {
                    obj.TraitsAdv.Where(t=>t.TraitApproved == false).ToList().ForEach(t => contex.TraitsAdv.Remove(t));
                }
                //delete race
                var race = await contex.Races.Include(c => c.Traits).FirstOrDefaultAsync(u => u.Id == obj.RaceId && u.RaceApproved == false);
                if (race is not null)
                {
                    if (!race.Traits.IsNullOrEmpty())
                        race.Traits.ToList().ForEach(t =>contex.TraitsRace.Remove(t));
                    contex.Races.Remove(race);
                }
                //delete equipments
                if (!obj.TraitsAdv.IsNullOrEmpty())
                {
                    obj.Equipment.ToList().ForEach(e =>
                    {
                        var equi = contex.Equipment.Include(c => c.Traits).ThenInclude(t=>t.Bonuses).FirstOrDefaultAsync(u => u.Id == obj.Id && u.IsApproved == false).Result;
                        equi.Traits.Where(t => t.TraitApproved == false).ToList().ForEach(t => contex.TraitsEquipment.Remove(t));
                        contex.Equipment.Remove(equi);
                    });
                }

                
                //datele class
                var profession = await contex.Professions.Include(s=>s.ActiveSkills).Include(u=>u.PassiveSkills).FirstOrDefaultAsync(u => u.Id == obj.ProfessionId && u.IsApproved == false && u.IsUniversal == false);
                if (profession is not null)
                {
                    if (!profession.ActiveSkills.IsNullOrEmpty())
                        profession.ActiveSkills.ToList().ForEach(s =>contex.ProfessionSkills.Remove(s));
                    if (!profession.PassiveSkills.IsNullOrEmpty())
                        profession.PassiveSkills.ToList().ForEach(s =>contex.ProfessionSkills.Remove(s));
                    contex.Professions.Remove(profession);
                }

                return contex.SaveChanges();
            }
            catch (Exception ex) { throw new RepositoryErrorException("Error in" + System.Reflection.MethodBase.GetCurrentMethod().Name); }
        }

        public async Task<IEnumerable<CharacterDTO>> GetAll(int? id=null)
        {
            using var contex = await _db.CreateDbContextAsync();
            if (id == null || id < 1)
                return _mapper.Map<IEnumerable<Character>, IEnumerable<CharacterDTO>>(contex.Characters.Include(r => r.TraitsAdv).Include(r => r.Race).Include(r => r.Profession).Include(r=>r.Equipment).Where(u=>u.NPCName != SD.NPCName_GameMaster));
            return _mapper.Map<IEnumerable<Character>, IEnumerable<CharacterDTO>>(contex.Characters.Include(r => r.TraitsAdv).Include(r => r.Race).Include(r => r.Profession).Include(r => r.Equipment).Where(u=>u.Id == id && u.NPCName != SD.NPCName_GameMaster));
        }

        public async Task<CharacterDTO> GetById(int id)
        {
            using var contex = await _db.CreateDbContextAsync();
            var obj = await contex.Characters.Include(t => t.TraitsAdv).ThenInclude(b=>b.Bonuses).Include(r=>r.Race).Include(r => r.Profession).Include(r => r.Equipment).FirstOrDefaultAsync(u => u.Id == id);
            if (obj != null)
            {
                return _mapper.Map<Character, CharacterDTO>(obj);
            }
            return new CharacterDTO();
        }
        public async Task<CharacterDTO> GetByName(string npcName)
        {
            using var contex = await _db.CreateDbContextAsync();
            var obj = await contex.Characters.Include(t => t.TraitsAdv).ThenInclude(b => b.Bonuses).Include(r => r.Race).Include(r => r.Profession).Include(r => r.Equipment).FirstOrDefaultAsync(u => u.NPCName == npcName);
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
                return new List<CharacterDTO>();
            return _mapper.Map<IEnumerable<Character>, IEnumerable<CharacterDTO>>(contex.Characters.Include(r => r.Race).Include(r => r.Race).Include(r => r.Profession).Include(r=>r.Equipment).Where(u => u.UserName == userName));
        }
        public async Task<IEnumerable<CharacterDTO>> GetAllForCampaign(int campaignId)
        {
            //try
            //{
            //    using var contex = await _db.CreateDbContextAsync();
            //    if (campaignId == null || campaignId == 0)
            //        return new List<CharacterDTO>();
            //    return _mapper.Map<IEnumerable<Character>, IEnumerable<CharacterDTO>>(contex.Characters.Include(r => r.Race).Include(r => r.Race).Include(r => r.Profession).Include(r => r.Equipment)
            //        .Where(u => u.Campaigns != null && u.Campaigns.FirstOrDefault(c => c.Id == campaignId) != null));
            //}
            //catch (Exception ex)
            //{
            //    throw new RepositoryErrorException("Error in" + System.Reflection.MethodBase.GetCurrentMethod().Name);
            //}
            return null;
        }

        public async Task<CharacterDTO> Update(CharacterDTO objDTO)
        {
            try
            {
                using var contex = await _db.CreateDbContextAsync();
                var obj = await contex.Characters.Include(u => u.TraitsAdv).ThenInclude(t => t.Bonuses).Include(u=>u.Equipment).ThenInclude(e=>e.Traits).ThenInclude(t=>t.Bonuses).FirstOrDefaultAsync(u => u.Id == objDTO.Id);


                if (obj != null)
                {
                    var updatedChar = _mapper.Map<CharacterDTO, Character>(objDTO);
                    var traits = await contex.TraitsAdv.ToListAsync();

                    obj.Age = updatedChar.Age;
                    obj.NPCName = updatedChar.NPCName;
                    obj.IsApproved = updatedChar.IsApproved;
                    obj.Description = updatedChar.Description;
                    obj.CurrentExpPoints = updatedChar.CurrentExpPoints;
                    obj.UsedExpPoints = updatedChar.UsedExpPoints;
                    obj.AttributePoints = updatedChar.AttributePoints;
                    obj.NPCType = updatedChar.NPCType;
                    obj.ImageUrl = updatedChar.ImageUrl;
                    obj.TraitBalance = updatedChar.TraitBalance;
                    obj.RaceId = updatedChar.RaceId;
                    obj.ProfessionId = updatedChar.ProfessionId;

                    /// UPDATE TRAITS

                    // Delete adv traits
                    if (obj.TraitsAdv is not null)
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
                                    // adding already existing, approved trait to character
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
                    /// UPDATE EQUIPMENT

                    // Delete equimpment
                    if (obj.Equipment is not null)
                    {
                        foreach (var existingChild in obj.Equipment.ToList())
                        {
                            if (!updatedChar.Equipment.Any(c => c.Id == existingChild.Id))
                            {
                                if (existingChild.IsApproved == true)
                                {
                                    //detach approved/non-deleting trait from character
                                    var detachedEquipment = contex.Equipment.Include(t => t.Traits).Include(c => c.Characters).FirstOrDefault(c => c.Id == existingChild.Id && c.Id != default(int));
                                    if (detachedEquipment != null && !detachedEquipment.Characters.IsNullOrEmpty() && detachedEquipment.Characters.Contains(obj))
                                    {
                                        detachedEquipment.Characters.Remove(obj);
                                        contex.Equipment.Update(detachedEquipment);
                                    }
                                }
                                else
                                    contex.Equipment.Remove(existingChild);
                            }

                        }
                    }

                    // Update and Insert equimpment
                    if (updatedChar.Equipment is not null)
                    {
                        foreach (var equ in updatedChar.Equipment)
                        {
                            Equipment? existingEqu = null;
                            if (obj.Equipment is not null)
                            {
                                existingEqu = obj.Equipment
                                    .FirstOrDefault(c => c.Id == equ.Id && c.Id != default(int));
                            }
                            else
                            {
                                obj.Equipment = new List<Equipment>();
                            }

                            if (existingEqu == null)
                            {
                                existingEqu = contex.Equipment.Include(t => t.Traits).Include(c => c.Characters).FirstOrDefault(c => c.Id == equ.Id && c.Id != default(int));
                            }

                            if (existingEqu is not null)
                            {
                                if (existingEqu.IsApproved && obj.Equipment.Contains(existingEqu) == false)
                                {
                                    if (existingEqu.Characters is null)
                                        existingEqu.Characters = new List<Character>();
                                    existingEqu.Characters.Add(obj);
                                    contex.Equipment.Update(existingEqu);
                                }
                                else
                                {
                                    //contex.Equipment.Update(existingEqu);
                                    // Update and Insert traits

                                    // Update built-in type members
                                    contex.Entry(existingEqu).CurrentValues.SetValues(equ);


                                    // Delete equipment traits
                                    if (existingEqu.Traits is not null)
                                    {
                                        foreach (var existingChild in existingEqu.Traits.ToList())
                                        {
                                            if (!equ.Traits.Any(c => c.Id == existingChild.Id))
                                            {
                                                if (existingChild.TraitApproved == true)
                                                {
                                                    var detachedTrait = contex.TraitsEquipment.Include(t => t.Bonuses).Include(c => c.Equipment).FirstOrDefault(c => c.Id == existingChild.Id && c.Id != default(int));
                                                    if (detachedTrait != null && !detachedTrait.Equipment.IsNullOrEmpty() && detachedTrait.Equipment.Contains(existingEqu))
                                                    {
                                                        detachedTrait.Equipment.Remove(existingEqu);
                                                        contex.TraitsEquipment.Update(detachedTrait);
                                                    }
                                                }
                                                else
                                                    contex.TraitsEquipment.Remove(existingChild);
                                            }
                                        }
                                    }


                                    // Update and Insert traits
                                    if (equ.Traits is not null)
                                    {
                                        foreach (var trait in equ.Traits)
                                        {
                                            TraitEquipment? existingTrait = null;
                                            if (existingEqu.Traits is not null)
                                            {
                                                existingTrait = existingEqu.Traits
                                                    .FirstOrDefault(c => c.Id == trait.Id && c.Id != default(int));
                                            }
                                            else
                                            {
                                                existingEqu.Traits = new List<TraitEquipment>();
                                            }

                                            if (existingTrait == null)
                                            {
                                                existingTrait = contex.TraitsEquipment.Include(t => t.Bonuses).Include(c => c.Equipment).FirstOrDefault(c => c.Id == trait.Id && c.Id != default(int));
                                            }

                                            if (existingTrait is not null)
                                            {
                                                if (existingTrait.TraitApproved && existingEqu.Traits.Contains(existingTrait) == false)
                                                {
                                                    if (existingTrait.Equipment is null)
                                                        existingTrait.Equipment = new List<Equipment>();
                                                    existingTrait.Equipment.Add(existingEqu);
                                                    contex.Traits.Update(existingTrait);
                                                }
                                                else
                                                {
                                                    // Update trait built-in types
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
                                                existingEqu.Traits.Add(trait);
                                        }
                                    }
                                }
                            }
                            else
                                // Insert trait
                                obj.Equipment.Add(equ);
                        }
                    }
                   




                    var result = contex.Characters.Update(obj);
                    await contex.SaveChangesAsync();
                    return _mapper.Map<Character, CharacterDTO>(result.Entity);
                }else
                    return objDTO;
            }
            catch (Exception ex) { throw new RepositoryErrorException("Error in" + System.Reflection.MethodBase.GetCurrentMethod().Name); }
        }

    }
}
