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
                // Navigation properties (Race, Profession) are ignored by AutoMapper

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
                throw new RepositoryErrorException("Error in"+ System.Reflection.MethodBase.GetCurrentMethod().Name , ex); 
            }
        }

        public async Task<int> Delete(int id)
        {
            try
            {
                using var contex = await _db.CreateDbContextAsync();
                var obj = await contex.Characters
                    .Include(c => c.EquipmentSlots)
                        .ThenInclude(e => e.Equipment)
                        .ThenInclude(t => t.Traits)
                    .AsSplitQuery()
                    .FirstOrDefaultAsync(u => u.Id == id);
                if (obj is null)
                    return 0;

                var raceId = obj.RaceId;
                var professionId = obj.ProfessionId;
                var unapprovedEquipmentIds = (obj.EquipmentSlots ?? Enumerable.Empty<EquipmentSlot>())
                    .Where(s => s.Equipment is { IsApproved: false })
                    .Select(s => s.EquipmentID)
                    .Distinct()
                    .ToList();

                var draftTraits = await contex.TraitsCharacter
                    .Where(c => c.CharacterId == id && c.TraitApproved == false)
                    .ToListAsync();
                contex.TraitsCharacter.RemoveRange(draftTraits);

                if (obj.EquipmentSlots is not null)
                {
                    foreach (var slot in obj.EquipmentSlots.ToList())
                        contex.EquipmentSlots.Remove(slot);
                }

                // Drop the character before any shared Race/Profession row. Demo clones
                // (and draft characters) reuse those FKs; deleting the race first hits
                // FK_Characters_Races_RaceId while this row — and any sibling — still points at it.
                contex.Characters.Remove(obj);
                var changes = await contex.SaveChangesAsync();

                changes += await DeleteOrphanUnapprovedRaceAsync(contex, raceId);
                changes += await DeleteOrphanUnapprovedProfessionAsync(contex, professionId);
                changes += await DeleteOrphanUnapprovedEquipmentAsync(contex, unapprovedEquipmentIds);
                return changes;
            }
            catch (Exception ex) {
                 throw new RepositoryErrorException("Error in" + System.Reflection.MethodBase.GetCurrentMethod().Name , ex );
            }
        }

        private static async Task<int> DeleteOrphanUnapprovedRaceAsync(ApplicationDbContext contex, int? raceId)
        {
            if (raceId is not int id)
                return 0;
            if (await contex.Characters.AnyAsync(c => c.RaceId == id))
                return 0;

            var race = await contex.Races.Include(c => c.Traits)
                .FirstOrDefaultAsync(u => u.Id == id && u.RaceApproved == false);
            if (race is null)
                return 0;

            if (!race.Traits.IsNullOrEmpty())
            {
                foreach (var trait in race.Traits.Where(t => t.TraitApproved == false).ToList())
                    contex.TraitsRace.Remove(trait);
            }
            contex.Races.Remove(race);
            return await contex.SaveChangesAsync();
        }

        private static async Task<int> DeleteOrphanUnapprovedProfessionAsync(ApplicationDbContext contex, int professionId)
        {
            if (professionId < 1)
                return 0;
            if (await contex.Characters.AnyAsync(c => c.ProfessionId == professionId))
                return 0;

            var profession = await contex.Professions
                .FirstOrDefaultAsync(u => u.Id == professionId && u.IsApproved == false && u.IsUniversal == false);
            if (profession is null)
                return 0;

            var draftTraits = await contex.TraitsProfession
                .Where(u => u.ProfessionId == professionId && u.TraitApproved == false)
                .ToListAsync();
            contex.TraitsProfession.RemoveRange(draftTraits);
            contex.Professions.Remove(profession);
            return await contex.SaveChangesAsync();
        }

        private static async Task<int> DeleteOrphanUnapprovedEquipmentAsync(
            ApplicationDbContext contex, IReadOnlyCollection<int> equipmentIds)
        {
            if (equipmentIds.Count == 0)
                return 0;

            var changes = 0;
            foreach (var equipmentId in equipmentIds)
            {
                if (equipmentId < 1)
                    continue;
                if (await contex.EquipmentSlots.AnyAsync(s => s.EquipmentID == equipmentId))
                    continue;

                var equi = await contex.Equipment.Include(e => e.Traits)
                    .FirstOrDefaultAsync(e => e.Id == equipmentId && e.IsApproved == false);
                if (equi is null)
                    continue;

                if (equi.Traits is not null)
                {
                    foreach (var trait in equi.Traits.Where(t => t.TraitApproved == false).ToList())
                        contex.TraitsEquipment.Remove(trait);
                }
                contex.Equipment.Remove(equi);
                changes += await contex.SaveChangesAsync();
            }
            return changes;
        }

        public async Task<IEnumerable<CharacterDTO>> GetAll(int? id=null, bool fullIncludes = false)
        {
            using var contex = await _db.CreateDbContextAsync();
            var query = contex.Characters.AsNoTracking().Where(u => u.NPCName != SD.GameMaster_NPCName);

            if (id is > 0)
            {
                query = query.Where(u => u.Id == id);
            }

            if (fullIncludes)
            {
                query = query
                    .Include(r => r.Race)
                    .Include(r => r.Profession)
                    .Include(r => r.EquipmentSlots)
                    .AsSplitQuery();
            }

            return _mapper.Map<IEnumerable<Character>, IEnumerable<CharacterDTO>>(await query.ToListAsync());
        }

        public async Task<CharacterDTO> GetById(int id, bool fullIncludes = false)
        {
            using var contex = await _db.CreateDbContextAsync();
            Character? obj;
            if (fullIncludes)
            {
                obj = await contex.Characters
                    .AsNoTracking()
                    .Include(r => r.Race)
                    .Include(r => r.Profession)
                    .Include(r => r.Attributes)
                    .Include(r => r.BaseSkills)
                    .Include(r => r.SpecialSkills)
                    .Include(r => r.EquipmentSlots!)
                        .ThenInclude(u => u.Equipment)
                        .ThenInclude(b => b!.Traits!)
                        .ThenInclude(b => b.Bonuses)
                    .AsSplitQuery()
                    .FirstOrDefaultAsync(u => u.Id == id);
            }
            else
            {
                obj = await contex.Characters
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Id == id);
            }

            if (obj != null)
            {
                var dto = _mapper.Map<Character, CharacterDTO>(obj);
                if (fullIncludes)
                    CharacterSkillRelations.Wire(dto);
                return dto;
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
                    .AsNoTracking()
                    .Include(r => r.Race)
                    .Include(r => r.Profession)
                    .Include(r => r.Attributes)
                    .Include(r => r.BaseSkills)
                    .Include(r => r.SpecialSkills)
                    .Include(r => r.EquipmentSlots)
                        .ThenInclude(u => u.Equipment)
                        .ThenInclude(b => b.Traits)
                        .ThenInclude(b => b.Bonuses)
                    .AsSplitQuery()
                    .FirstOrDefaultAsync(u => u.NPCName == npcName);
            }
            else
            {
                obj = await contex.Characters
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.NPCName == npcName);
            }
            if (obj != null)
            {
                var dto = _mapper.Map<Character, CharacterDTO>(obj);
                if (fullIncludes)
                    CharacterSkillRelations.Wire(dto);
                return dto;
            }
            return new CharacterDTO();
        }
        
        public async Task<IEnumerable<CharacterDTO>> GetAllForUser(string userName, bool fullIncludes = false)
        {
            using var contex = await _db.CreateDbContextAsync();
            if (userName == null || userName.Length<3)
                return new List<CharacterDTO>();

            var query = contex.Characters.AsNoTracking().Where(u => u.UserName == userName);
            if (fullIncludes)
            {
                query = query
                    .Include(r => r.Race)
                    .Include(r => r.Profession)
                    .Include(r => r.EquipmentSlots)
                    .AsSplitQuery();
            }

            return _mapper.Map<IEnumerable<Character>, IEnumerable<CharacterDTO>>(await query.ToListAsync());
        }

        public async Task<bool> CheckIfCharacterBelongToUser(string userName, int characterId)
        {
            using var contex = await _db.CreateDbContextAsync();
            return await contex.Characters
                .AsNoTracking()
                .AnyAsync(u => u.Id == characterId && u.UserName == userName);
        }

        public async Task<CharacterDTO> Update(CharacterDTO objDTO)
        {
            try
            {
                using var contex = await _db.CreateDbContextAsync();
                var obj = await contex.Characters
                    .Include(u => u.EquipmentSlots)
                        .ThenInclude(e => e.Equipment)
                        .ThenInclude(e => e.Traits)
                        .ThenInclude(t => t.Bonuses)
                    .AsSplitQuery()
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
                throw new RepositoryErrorException("Error in" + System.Reflection.MethodBase.GetCurrentMethod().Name , ex); 
            }
        }

        public async Task<IEnumerable<CharacterDTO>> GetAllApproved(string? userName = null, bool fullIncludes = false)
        {
            using var contex = await _db.CreateDbContextAsync();
            var query = contex.Characters.AsNoTracking().Where(u => u.IsApproved == true);

            if (userName is { Length: >= 2 })
            {
                query = query.Where(u => u.UserName == userName);
            }

            if (fullIncludes)
            {
                query = query
                    .Include(r => r.Race)
                    .Include(r => r.Profession)
                    .Include(r => r.EquipmentSlots)
                    .AsSplitQuery();
            }

            return _mapper.Map<IEnumerable<Character>, IEnumerable<CharacterDTO>>(await query.ToListAsync());
        }
        public async Task<string> GetPortraitUrl(int id)
        {
            using var contex = await _db.CreateDbContextAsync();
            var obj = await contex.Characters.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);
            if (obj != null)
            {
                return obj.ImageUrl;
            }
            return string.Empty;
        }

    }
}
