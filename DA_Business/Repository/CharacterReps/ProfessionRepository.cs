using Abp.Collections.Extensions;
using AutoMapper;
using Castle.MicroKernel.Registration;
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
using DagoniteEmpire.Exceptions;
using System.Diagnostics;
using Abp.Extensions;
using Castle.MicroKernel.Internal;

namespace DA_Business.Repository.CharacterReps
{
    public class ProfessionRepository : IProfessionRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _db;
        private readonly IMapper _mapper;

        public ProfessionRepository(IDbContextFactory<ApplicationDbContext> db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }
        public async Task<ProfessionDTO> Create(ProfessionDTO objDTO)
        {
            try
            {
                using var contex = await _db.CreateDbContextAsync();
                var obj = _mapper.Map<ProfessionDTO, Profession>(objDTO);


                var addedObj = await contex.Professions.AddAsync(obj);
                await contex.SaveChangesAsync();

                return _mapper.Map<Profession, ProfessionDTO>(addedObj.Entity);
            }
            catch (Exception ex)
            {
                throw new RepositoryErrorException("Error in Profession Repository Create: " + ex.Message);
            }
                
        }

        public async Task<int> Delete(int id)
        {
            try
            {
                using var contex = await _db.CreateDbContextAsync();
                var obj = await contex.Professions.FirstOrDefaultAsync(u => u.Id == id);
                if (obj is not null)
                {
                    contex.Professions.Remove(obj);
                    await contex.SaveChangesAsync();
                }
            return 0;
            }
            catch (Exception ex)
            {
                throw new RepositoryErrorException("Error in Profession Repository Delete: " + ex.Message);
            }
        }

        public async Task<IEnumerable<ProfessionDTO>> GetAll()
        {
            using var contex = await _db.CreateDbContextAsync();
            return _mapper.Map<IEnumerable<Profession>, IEnumerable<ProfessionDTO>>(contex.Professions.Include(p => p.SpellCircles));
        }

        public async Task<IEnumerable<ProfessionDTO>> GetAllApproved()
        {
            using var contex = await _db.CreateDbContextAsync();
            return _mapper.Map<IEnumerable<Profession>, IEnumerable<ProfessionDTO>>(contex.Professions.Include(p => p.SpellCircles).Where(t=>t.IsApproved == true));
        }

        public async Task<ProfessionDTO> GetById(int id)
        {
            using var contex = await _db.CreateDbContextAsync();
            var obj = await contex.Professions
                .Include(p => p.SpellCircles).ThenInclude(p=>p.SpellSlots).ThenInclude(p=>p.Spell).FirstOrDefaultAsync(u => u.Id == id);
            if (obj != null)
            {
                return _mapper.Map<Profession, ProfessionDTO>(obj);
            }
            return new ProfessionDTO();
        }

        public async Task<ProfessionDTO> Update(ProfessionDTO objDTO)
        {
            try
            {
                using var contex = await _db.CreateDbContextAsync();
                var obj = await contex.Professions
                    .Include(p => p.SpellCircles).ThenInclude(c => c.SpellSlots).ThenInclude(s => s.Spell)
                    .FirstOrDefaultAsync(u => u.Id == objDTO.Id);
                if (obj is not null)
                {
                    var updateProfession = _mapper.Map<ProfessionDTO, Profession>(objDTO);
                    // Update parent
                    contex.Entry(obj).CurrentValues.SetValues(updateProfession);

                    // Delete spell circles
                    if (obj.SpellCircles is not null)
                    {
                        foreach (var existingChild in obj.SpellCircles.ToList())
                        {
                            //delete those who is not in updated profession
                            if (!updateProfession.SpellCircles.Any(c => c.Id == existingChild.Id))
                            {
                                if (existingChild.SpellSlots is not null)
                                {
                                    foreach (var existingSlot in existingChild.SpellSlots)
                                    {
                                        if (!updateProfession.SpellCircles.Any(c => c.Id == existingChild.Id))
                                        {
                                            contex.SpellCircles.Remove(existingChild);
                                        }
                                    }
                                }
                                contex.SpellCircles.Remove(existingChild);
                            }
                        }
                    }
                    // Update and Insert spell circles
                    if (updateProfession.SpellCircles is not null)
                    {
                        foreach (var newCircle in updateProfession.SpellCircles)
                        {
                            SpellCircle? existingCircle;
                            if (!obj.SpellCircles.IsNullOrEmpty())
                            {
                                existingCircle = obj.SpellCircles.FirstOrDefault(c => c.Id == newCircle.Id && c.Id != default(int));
                            }
                            else
                            {
                                obj.SpellCircles = new List<SpellCircle>();
                                existingCircle = null;
                            }

                            if (existingCircle != null)
                            {

                                // Delete spell Slot
                                if (obj.SpellCircles is not null)
                                {

                                    // Update Circle
                                    contex.Entry(existingCircle).CurrentValues.SetValues(newCircle);

                                    // update slots
                                    foreach (var existingSlot in existingCircle.SpellSlots.ToList())
                                    {
                                        if (!newCircle.SpellSlots.Any(c => c.Id == existingSlot.Id))
                                        {
                                            contex.SpellSlots.Remove(existingSlot);
                                        }
                                        // Delete spells
                                        if (!newCircle.SpellSlots.Any(c => c.SpellId == existingSlot.SpellId))
                                        {
                                            if (existingSlot.Spell?.IsApproved == true)
                                            {
                                                var detachedSpell = contex.Spells.Include(t => t.SpellSlots).FirstOrDefault(c => c.Id == existingSlot.SpellId && c.Id != default(int));
                                                if (detachedSpell == null || detachedSpell.SpellSlots.IsNullOrEmpty() || detachedSpell.SpellSlots?.Contains(existingSlot) == false)
                                                    continue;
                                                detachedSpell.SpellSlots?.Remove(existingSlot);
                                                contex.Spells.Update(detachedSpell);
                                                contex.SpellSlots.Update(existingSlot);
                                            }
                                            else if (existingSlot.Spell is not null)
                                            {
                                                contex.Spells.Remove(existingSlot.Spell);
                                            }
                                        }

                                    }

                                    // Update Slot
                                }
                                // Update and Insert spell Slot
                                if (newCircle.SpellSlots is not null)
                                {
                                    foreach (var spellSlot in newCircle.SpellSlots)
                                    {
                                        SpellSlot? existingSlot;
                                        if (!existingCircle.SpellSlots.IsNullOrEmpty())
                                        {
                                            existingSlot = existingCircle.SpellSlots.FirstOrDefault(c => c.Id == spellSlot.Id && c.Id != default(int));
                                        }
                                        else
                                        {
                                            existingCircle.SpellSlots = new List<SpellSlot>();
                                            existingSlot = null;
                                        }

                                        if (existingSlot != null)
                                        {
                                            // Update slot
                                            contex.Entry(existingSlot).CurrentValues.SetValues(spellSlot);
                                            // update spell

                                            Spell? existingSpell = contex.Spells.Include(t => t.SpellSlots).FirstOrDefault(c => c.Id == spellSlot.SpellId && c.Id != default(int));

                                            if (existingSpell is not null)
                                                contex.Entry(existingSpell).CurrentValues.SetValues(spellSlot.Spell);
                                            else
                                            {
                                                if (spellSlot.Spell is not null)
                                                {
                                                    spellSlot.Spell.SpellSlots = new HashSet<SpellSlot>();
                                                    spellSlot.Spell.SpellSlots.Add(existingSlot);
                                                    contex.Spells.Add(spellSlot.Spell);
                                                }
                                            }
                                        }
                                        else
                                            // Insert SpellSlot
                                            existingCircle.SpellSlots.Add(spellSlot);
                                    }
                                }
                            }
                            else
                            {
                                if (newCircle.SpellSlots is not null)
                                {
                                    // Remove unnessesary existing spells
                                    foreach (var slot in newCircle.SpellSlots)
                                    {
                                        if (slot.Spell is not null && slot.Spell.IsApproved == true && contex.Spells.FirstOrDefault(s => s.Id == slot.SpellId && s.Id != default(int)) != null)
                                        {
                                            slot.Spell = null;
                                        }
                                    }
                                    obj.SpellCircles.Add(newCircle);
                                }
                            }
                        }
                    }
                    //contex.Professions.Update(obj);
                    await contex.SaveChangesAsync();
                    return _mapper.Map<Profession, ProfessionDTO>(obj);
                }
                else
                {
                    obj = _mapper.Map<ProfessionDTO, Profession>(objDTO);
                    var addedObj = contex.Professions.Add(obj);
                    await contex.SaveChangesAsync();
                    return _mapper.Map<Profession, ProfessionDTO>(addedObj.Entity);
                }
            }
            catch (Exception ex)
            {
                throw new RepositoryErrorException("Error in Profession Repository Update: " + ex.Message);
            }
        }
    }
}
