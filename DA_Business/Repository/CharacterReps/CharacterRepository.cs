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


                // Update and Insert equimpment
                if (obj.EquipmentSlots is not null)
                {
                    foreach (var slot in obj.EquipmentSlots)
                    {
                        // Insert slot
                        if (slot.Equipment.Id != slot.EquipmentID && slot.Equipment.Id != 0)
                        {
                            slot.EquipmentID = slot.Equipment.Id;
                            slot.Equipment = null;
                        }
                        //obj.EquipmentSlots.Add(slot);
                    }
                }

                var addedObj = contex.Characters.Add(obj);
                await contex.SaveChangesAsync();
                return _mapper.Map<Character, CharacterDTO>(addedObj.Entity);
            }
            catch (Exception ex) {
                throw new RepositoryErrorException("Error in"+ System.Reflection.MethodBase.GetCurrentMethod().Name + ": " + ex.Message); 
            }
        }

        public async Task<int> Delete(int id)
        {
            try
            {
                using var contex = await _db.CreateDbContextAsync();
                //delete traits adv
                var obj = await contex.Characters.Include(c=>c.EquipmentSlots).ThenInclude(e=>e.Equipment).ThenInclude(t=>t.Traits).FirstOrDefaultAsync(u => u.Id == id);
                var traits = contex.TraitsCharacter.Where(c=>c.CharacterId == id && c.TraitApproved == false);
                foreach(var trait in traits)
                {
                    contex.TraitsCharacter.Remove(trait);
                }

                //delete race
                var race = await contex.Races.Include(c => c.Traits).FirstOrDefaultAsync(u => u.Id == obj.RaceId && u.RaceApproved == false);
                if (race is not null)
                {
                    if (!race.Traits.IsNullOrEmpty())
                        race.Traits.Where(t=>t.TraitApproved==false).ToList().ForEach(t =>contex.TraitsRace.Remove(t));
                    contex.Races.Remove(race);
                }
                //delete equipment slots
                if (!obj.EquipmentSlots.IsNullOrEmpty())
                {
                    foreach(var slot in obj.EquipmentSlots)
                    {
                        var equi = slot.Equipment;
                        if (equi.IsApproved == false)
                        {
                            if(equi.Traits is not null)
                            {
                                foreach(var trait in equi.Traits)
                                {
                                    if (trait.TraitApproved == false)
                                        contex.TraitsEquipment.Remove(trait);
                                }
                            }
                            contex.Equipment.Remove(equi);
                        }
                        contex.EquipmentSlots.Remove(slot);
                    }
                }
                await contex.SaveChangesAsync();
                
                //delete class traits
                var traitsProfession = await contex.TraitsProfession.Where(u => u.ProfessionId == obj.ProfessionId).ToListAsync();
                if (!traitsProfession.IsNullOrEmpty())
                {
                    foreach (var trait in traitsProfession)
                    {
                        if (trait.TraitApproved == false)
                            contex.TraitsProfession.Remove(trait);
                    }
                }
                await contex.SaveChangesAsync();
                //datele class
                var profession = await contex.Professions.FirstOrDefaultAsync(u => u.Id == obj.ProfessionId && u.IsApproved == false && u.IsUniversal == false);
                if (profession is not null)
                {
                    contex.Professions.Remove(profession);
                }
                //await contex.SaveChangesAsync();
                return await contex.SaveChangesAsync();
            }
            catch (Exception ex) {
                 throw new RepositoryErrorException("Error in" + System.Reflection.MethodBase.GetCurrentMethod().Name + ": " + ex.Message );
            }
        }

        public async Task<IEnumerable<CharacterDTO>> GetAll(int? id=null, bool fullIncludes = false)
        {
            using var contex = await _db.CreateDbContextAsync();
            if (fullIncludes)
            {

                if (id == null || id < 1)
                    return _mapper.Map<IEnumerable<Character>, IEnumerable<CharacterDTO>>(contex.Characters
                        .Include(r => r.Race)
                        .Include(r => r.Profession)
                        .Include(r=>r.EquipmentSlots)
                        .Where(u=>u.NPCName != SD.GameMaster_NPCName));
                return _mapper.Map<IEnumerable<Character>, IEnumerable<CharacterDTO>>(contex.Characters
                    .Include(r => r.Race)
                    .Include(r => r.Profession)
                    .Include(r => r.EquipmentSlots)
                    .Where(u=>u.Id == id && u.NPCName != SD.GameMaster_NPCName));
            }
            else
            {
                if (id == null || id < 1)
                    return _mapper.Map<IEnumerable<Character>, IEnumerable<CharacterDTO>>(contex.Characters
                        .Where(u => u.NPCName != SD.GameMaster_NPCName));
                return _mapper.Map<IEnumerable<Character>, IEnumerable<CharacterDTO>>(contex.Characters
                    .Where(u => u.Id == id && u.NPCName != SD.GameMaster_NPCName));
            }
        }

        public async Task<CharacterDTO> GetById(int id, bool fullIncludes = false)
        {
            using var contex = await _db.CreateDbContextAsync();
            Character? obj = null;
            if (fullIncludes)
            {
                obj = await contex.Characters
                .Include(r => r.Race)
                .Include(r => r.Profession)
                .Include(r => r.EquipmentSlots)?.ThenInclude(u => u.Equipment)?.ThenInclude(b => b.Traits)?.ThenInclude(b => b.Bonuses)
                .FirstOrDefaultAsync(u => u.Id == id);
            }
            else
            {
                obj = await contex.Characters
                .FirstOrDefaultAsync(u => u.Id == id);
            }
            if (obj != null)
            {
                var res = _mapper.Map<Character, CharacterDTO>(obj);
                return res;
            }
            return new CharacterDTO();
        }
        public async Task<CharacterDTO> GetByName(string npcName, bool fullIncludes = false)
        {
            using var contex = await _db.CreateDbContextAsync();
            Character? obj = null;
            if (fullIncludes)
            {
                obj = await contex.Characters
                .Include(r => r.Race)
                .Include(r => r.Profession)
                .Include(r => r.EquipmentSlots).ThenInclude(u => u.Equipment).ThenInclude(b => b.Traits)?.ThenInclude(b => b.Bonuses)
                .FirstOrDefaultAsync(u => u.NPCName == npcName);
            }
            else
            {
                obj = await contex.Characters
               .FirstOrDefaultAsync(u => u.NPCName == npcName);
            }
            if (obj != null)
            {
                return _mapper.Map<Character, CharacterDTO>(obj);
            }
            return new CharacterDTO();
        }
        
        public async Task<IEnumerable<CharacterDTO>> GetAllForUser(string userName, bool fullIncludes = false)
        {
            using var contex = await _db.CreateDbContextAsync();
            if (userName == null || userName.Length<3)
                return new List<CharacterDTO>();
            if (fullIncludes)
            {
                return _mapper.Map<IEnumerable<Character>, IEnumerable<CharacterDTO>>(contex.Characters
                .Include(r => r.Race)
                .Include(r => r.Profession)
                .Include(r => r.EquipmentSlots)
                .Where(u => u.UserName == userName));
            }
            else
            {
                return _mapper.Map<IEnumerable<Character>, IEnumerable<CharacterDTO>>(contex.Characters
                .Where(u => u.UserName == userName));
            }
        }
        public async Task<IEnumerable<CharacterDTO>> GetAllForCampaign(int campaignId, bool fullIncludes = false)
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
                var obj = await contex.Characters
                    .Include(u=>u.EquipmentSlots).ThenInclude(e => e.Equipment).ThenInclude(e=>e.Traits).ThenInclude(t=>t.Bonuses)
                    .FirstOrDefaultAsync(u => u.Id == objDTO.Id);


                if (obj != null)
                {
                    var updatedChar = _mapper.Map<CharacterDTO, Character>(objDTO);
                    var traits = await contex.TraitsCharacter.ToListAsync();
                    // Update character built-in types
                    contex.Entry(obj).CurrentValues.SetValues(objDTO);


                    /// UPDATE EQUIPMENT

                    // Delete equipment
                    if (obj.EquipmentSlots is not null)
                    {
                        foreach (var existingSlot in obj.EquipmentSlots.ToList())
                        {
                            if (!updatedChar.EquipmentSlots.Any(s=>s.Id == existingSlot.Id))
                            {
                                if (existingSlot.Equipment.IsApproved == true)
                                {
                                    var detachedEquipment = contex.Equipment.FirstOrDefault(e => e.Id == existingSlot.Equipment.Id);
                                    if (detachedEquipment != null && !detachedEquipment.EquipmentSlots.IsNullOrEmpty() && detachedEquipment.EquipmentSlots.Contains(existingSlot))
                                    {
                                        detachedEquipment.EquipmentSlots.Remove(existingSlot);
                                        contex.Equipment.Update(detachedEquipment);
                                    }
                                }
                                else
                                {
                                    contex.Equipment.Remove(existingSlot.Equipment);
                                }
                            }
                        }
                    }

                    // Update and Insert equimpment
                    if (updatedChar.EquipmentSlots is not null)
                    {
                        foreach (var slot in updatedChar.EquipmentSlots)
                        {
                            // Update built-in type members
                            EquipmentSlot? existingSlot = null;
                            if (obj.EquipmentSlots is not null)
                            {
                                existingSlot = obj.EquipmentSlots
                                    .FirstOrDefault(c => c.Id == slot.Id && c.Id != default(int));
                            }
                            else
                            {
                                obj.EquipmentSlots = new List<EquipmentSlot>();
                            }

                            if (existingSlot == null)
                            {
                                existingSlot = contex.EquipmentSlots
                                    ?.Include(e=>e.Equipment)?.ThenInclude(t => t.Traits)?.ThenInclude(t => t.Bonuses)
                                    .Include(c => c.Character)
                                    .FirstOrDefault(c => c.Id == slot.Id && c.Id != default(int));
                            }

                            if (existingSlot is not null)
                            {
                                Equipment? existingItem = null;
                                existingItem = contex.Equipment
                                    ?.Include(e => e.Traits)?.ThenInclude(t => t.Bonuses)
                                    ?.Include(c => c.EquipmentSlots)
                                    ?.FirstOrDefault(c => c.Id == slot.EquipmentID && c.Id != default(int));

                                //contex.Entry(existingSlot).CurrentValues.SetValues(slot);
                                existingSlot.Count = slot.Count;
                                existingSlot.SlotType = slot.SlotType;
                                existingSlot.IsEquipped = slot.IsEquipped;

                                if (existingItem is not null)
                                {
                                    if (existingItem.IsApproved && obj.EquipmentSlots.Contains(existingSlot) == false)
                                    {
                                        if (existingItem.EquipmentSlots is null)
                                            existingItem.EquipmentSlots = new List<EquipmentSlot>();
                                        existingItem.EquipmentSlots.Add(slot);
                                        contex.Equipment.Update(existingItem);
                                    }
                                    else
                                    {
                                        //contex.Equipment.Update(existingEqu);
                                        // Update and Insert traits

                                        // Update built-in type members
                                        contex.Entry(existingItem).CurrentValues.SetValues(slot.Equipment);


                                        // Delete equipment traits
                                        if (existingItem.Traits is not null)
                                        {
                                            foreach (var existingChild in existingItem.Traits.ToList())
                                            {
                                                if (!slot.Equipment.Traits.Any(c => c.Id == existingChild.Id))
                                                {
                                                    if (existingChild.TraitApproved == true)
                                                    {
                                                        var detachedTrait = contex.TraitsEquipment.Include(t => t.Bonuses).Include(c => c.Equipment).FirstOrDefault(c => c.Id == existingChild.Id && c.Id != default(int));
                                                        if (detachedTrait != null && !detachedTrait.Equipment.IsNullOrEmpty() && detachedTrait.Equipment.Contains(existingItem))
                                                        {
                                                            detachedTrait.Equipment.Remove(existingItem);
                                                            contex.TraitsEquipment.Update(detachedTrait);
                                                        }
                                                    }
                                                    else
                                                        contex.TraitsEquipment.Remove(existingChild);
                                                }
                                            }
                                        }


                                        // Update and Insert traits
                                        if (slot.Equipment.Traits is not null)
                                        {
                                            foreach (var trait in slot.Equipment.Traits)
                                            {
                                                TraitEquipment? existingTrait = null;
                                                if (existingItem.Traits is not null)
                                                {
                                                    existingTrait = existingItem.Traits
                                                        .FirstOrDefault(c => c.Id == trait.Id && c.Id != default(int));
                                                }
                                                else
                                                {
                                                    existingItem.Traits = new List<TraitEquipment>();
                                                }

                                                if (existingTrait == null)
                                                {
                                                    existingTrait = contex.TraitsEquipment
                                                        .Include(t => t.Bonuses)
                                                        .Include(c => c.Equipment)
                                                        .FirstOrDefault(c => c.Id == trait.Id && c.Id != default(int));
                                                }

                                                if (existingTrait is not null)
                                                {
                                                    if (existingTrait.TraitApproved && existingItem.Traits.Contains(existingTrait) == false)
                                                    {
                                                        if (existingTrait.Equipment is null)
                                                            existingTrait.Equipment = new List<Equipment>();
                                                        existingTrait.Equipment.Add(existingItem);
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
                                                    existingItem.Traits.Add(trait);
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                // Insert slot
                                //slot.CharacterID = obj.Id;
                                if (slot.Equipment.Id != slot.EquipmentID && slot.Equipment.Id != 0)
                                {
                                    slot.EquipmentID = slot.Equipment.Id;
                                    slot.Equipment = null;
                                }
                                obj.EquipmentSlots.Add(slot);
                            }
                        }
                    }

                    var result = contex.Characters.Update(obj);
                    await contex.SaveChangesAsync();
                    return _mapper.Map<Character, CharacterDTO>(result.Entity);
                }else
                    return objDTO;
            }
            catch (Exception ex) { 
                throw new RepositoryErrorException("Error in" + System.Reflection.MethodBase.GetCurrentMethod().Name + ": " + ex.Message); 
            }
        }

        public async Task<IEnumerable<CharacterDTO>> GetAllApproved(string? userName = null, bool fullIncludes = false)
        {
            using var contex = await _db.CreateDbContextAsync();
            if (fullIncludes)
            {

            if (userName == null || userName.Length < 2)
                return _mapper.Map<IEnumerable<Character>, IEnumerable<CharacterDTO>>(contex.Characters.Include(r => r.Race).Include(r => r.Race).Include(r => r.Profession).Include(r => r.EquipmentSlots).Where(u =>u.IsApproved == true));
            return _mapper.Map<IEnumerable<Character>, IEnumerable<CharacterDTO>>(contex.Characters.Include(r => r.Race).Include(r => r.Race).Include(r => r.Profession).Include(r => r.EquipmentSlots).Where(u => u.UserName == userName && u.IsApproved == true));
            }
            else
            {
                if (userName == null || userName.Length < 2)
                    return _mapper.Map<IEnumerable<Character>, IEnumerable<CharacterDTO>>(contex.Characters.Where(u => u.IsApproved == true));
                return _mapper.Map<IEnumerable<Character>, IEnumerable<CharacterDTO>>(contex.Characters.Where(u => u.UserName == userName && u.IsApproved == true));
            }

        }
        public async Task<string> GetPortraitUrl(int id)
        {
            using var contex = await _db.CreateDbContextAsync();
            var obj = await contex.Characters.FirstOrDefaultAsync(u => u.Id == id);
            if (obj != null)
            {
                return obj.ImageUrl;
            }
            return string.Empty;
        }

    }
}
