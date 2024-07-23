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
        //private readonly ApplicationDbContext _db;
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
                //handle profession skills
                if (!obj.ActiveSkills.IsNullOrEmpty())
                {
                    var activeSkills = await contex.ProfessionSkills.ToListAsync();
                    activeSkills.ForEach(t =>
                    {
                        if (obj.ActiveSkills.Any(nt => nt.Id == t.Id))
                        {
                            var untracked = obj.ActiveSkills.FirstOrDefault(nt => nt.Id == t.Id);
                            obj.ActiveSkills.Remove(untracked);
                            obj.ActiveSkills.Add(t);
                        }
                    });

                    var passiveSkills = await contex.ProfessionSkills.ToListAsync();
                    passiveSkills.ForEach(t =>
                    {
                        if (obj.PassiveSkills.Any(nt => nt.Id == t.Id))
                        {
                            var untracked = obj.PassiveSkills.FirstOrDefault(nt => nt.Id == t.Id);
                            obj.PassiveSkills.Remove(untracked);
                            obj.PassiveSkills.Add(t);
                        }
                    });
                }

                var addedObj = await contex.Professions.AddAsync(obj);
                await contex.SaveChangesAsync();

                return _mapper.Map<Profession, ProfessionDTO>(addedObj.Entity);
            }
            catch (Exception ex)
            {
                throw new RepositoryErrorException("Error in Profession Repository Create");
            }
                
        }

        public async Task<int> Delete(int id)
        {
            try
            {
                using var contex = await _db.CreateDbContextAsync();
                var obj = await contex.Professions.Include(u=>u.ActiveSkills).Include(u => u.PassiveSkills).FirstOrDefaultAsync(u => u.Id == id);
                if (obj is not null)
                {

                    if(!obj.ActiveSkills.IsNullOrEmpty())
                    {
                        foreach (var s in obj.ActiveSkills)
                        {
                            contex.ProfessionSkills.Remove(s);
                        }
                    }
                    if (!obj.PassiveSkills.IsNullOrEmpty())
                    {
                        foreach (var s in obj.PassiveSkills)
                        {
                            contex.ProfessionSkills.Remove(s);
                        }
                    }
                    contex.Professions.Remove(obj);
                    await contex.SaveChangesAsync();
                }
            return 0;
            }
            catch (Exception ex)
            {
                throw new RepositoryErrorException("Error in Profession Repository Delete");
            }
            return 0;
        }

        public async Task<IEnumerable<ProfessionDTO>> GetAll()
        {
            using var contex = await _db.CreateDbContextAsync();
            return _mapper.Map<IEnumerable<Profession>, IEnumerable<ProfessionDTO>>(contex.Professions.Include(u => u.ActiveSkills).Include(p => p.PassiveSkills).Include(p => p.SpellCircles));
        }

        public async Task<IEnumerable<ProfessionDTO>> GetAllApproved()
        {
            using var contex = await _db.CreateDbContextAsync();
            return _mapper.Map<IEnumerable<Profession>, IEnumerable<ProfessionDTO>>(contex.Professions.Include(u => u.ActiveSkills).Include(p => p.PassiveSkills).Include(p => p.SpellCircles).Where(t=>t.IsApproved == true));
        }

        public async Task<ProfessionDTO> GetById(int id)
        {
            using var contex = await _db.CreateDbContextAsync();
            var obj = await contex.Professions
                .Include(u => u.ActiveSkills)
                .Include(p => p.PassiveSkills)
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
                    .Include(u => u.ActiveSkills)
                    .Include(p => p.PassiveSkills)
                    .Include(p => p.SpellCircles).ThenInclude(c=>c.SpellSlots)
                    .FirstOrDefaultAsync(u => u.Id == objDTO.Id);
                if (obj is not null)
                {
                    var updateProfession = _mapper.Map<ProfessionDTO, Profession>(objDTO);
                    // Update parent
                    contex.Entry(obj).CurrentValues.SetValues(updateProfession);

                    // Delete active skills 
                    if (obj.ActiveSkills is not null)
                    {
                        foreach (var existingChild in obj.ActiveSkills.ToList())
                        {
                            if (!updateProfession.ActiveSkills.Any(c => c.Id == existingChild.Id))
                            {
                                contex.ProfessionSkills.Remove(existingChild);
                            }
                        }
                    }
                    // Update and Insert ActiveSkills
                    if (updateProfession.ActiveSkills is not null)
                    {
                        foreach (var childTrait in updateProfession.ActiveSkills)
                        {
                            ProfessionSkill? existingChild;
                            if (!obj.ActiveSkills.IsNullOrEmpty())
                            {
                                existingChild = obj.ActiveSkills.FirstOrDefault(c => c.Id == childTrait.Id && c.Id != default(int));
                            }
                            else
                            {
                                obj.ActiveSkills = new List<ProfessionSkill>();
                                existingChild = null;
                            }

                            if (existingChild != null)
                                // Update bonus
                                contex.Entry(existingChild).CurrentValues.SetValues(childTrait);
                            else
                                // Insert bonus
                                obj.ActiveSkills.Add(childTrait);
                        }
                    }
                    // Update and Insert ActiveSkills
                    if (updateProfession.PassiveSkills is not null)
                    {
                        foreach (var childTrait in updateProfession.PassiveSkills)
                        {
                            ProfessionSkill? existingChild;
                            if (!obj.PassiveSkills.IsNullOrEmpty())
                            {
                                existingChild = obj.PassiveSkills.FirstOrDefault(c => c.Id == childTrait.Id && c.Id != default(int));
                            }
                            else
                            {
                                obj.PassiveSkills = new List<ProfessionSkill>();
                                existingChild = null;
                            }

                            if (existingChild != null)
                                // Update bonus
                                contex.Entry(existingChild).CurrentValues.SetValues(childTrait);
                            else
                                // Insert bonus
                                obj.PassiveSkills.Add(childTrait);
                        }
                    }

                    // Delete spell circles
                    if (obj.SpellCircles is not null)
                    {
                        foreach (var existingChild in obj.SpellCircles.ToList())
                        {
                            //delete those who is not in updated proffestion
                            if (!updateProfession.SpellCircles.Any(c => c.Id == existingChild.Id))
                            {
                                if(existingChild.SpellSlots is not null)
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
                        foreach (var childTrait in updateProfession.SpellCircles)
                        {
                            SpellCircle? existingChild;
                            if (!obj.SpellCircles.IsNullOrEmpty())
                            {
                                existingChild = obj.SpellCircles.FirstOrDefault(c => c.Id == childTrait.Id && c.Id != default(int));
                            }
                            else
                            {
                                obj.SpellCircles = new List<SpellCircle>();
                                existingChild = null;
                            }

                            if (existingChild != null)
                            {
                                // Update Circle
                                contex.Entry(existingChild).CurrentValues.SetValues(childTrait);

                                // Update Slot
                                // Delete spell Slot
                                if (obj.SpellCircles is not null)
                                {
                                    foreach (var existingSlot in existingChild.SpellSlots.ToList())
                                    {
                                        if (!childTrait.SpellSlots.Any(c => c.Id == existingSlot.Id))
                                        {

                                            contex.SpellSlots.Remove(existingSlot);
                                        }

                                    }
                                }
                                // Update and Insert spell Slot
                                if (childTrait.SpellSlots is not null)
                                {
                                    foreach (var spellSlot in childTrait.SpellSlots)
                                    {
                                        SpellSlot? existingSlot;
                                        if (!existingChild.SpellSlots.IsNullOrEmpty())
                                        {
                                            existingSlot = existingChild.SpellSlots.FirstOrDefault(c => c.Id == spellSlot.Id && c.Id != default(int));
                                        }
                                        else
                                        {
                                            existingChild.SpellSlots = new List<SpellSlot>();
                                            existingSlot = null;
                                        }

                                        if (existingSlot != null)
                                        {
                                            // Update slot
                                            contex.Entry(existingSlot).CurrentValues.SetValues(spellSlot);
                                            // update spell
                                            Spell existingSpell = contex.Spells.Include(t => t.SpellSlots).FirstOrDefault(c => c.Id == spellSlot.SpellId && c.Id != default(int));
                                            if (existingSpell != null)
                                                contex.Entry(existingSpell).CurrentValues.SetValues(spellSlot.Spell);
                                            else
                                                existingSlot.Spell = spellSlot.Spell;
                                        }
                                        else
                                            // Insert SpellSlot
                                            existingChild.SpellSlots.Add(spellSlot);
                                    }
                                }
                            }
                            else
                                // Insert bonus
                                obj.SpellCircles.Add(childTrait);
                        }
                    }
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
                throw new RepositoryErrorException("Error in Profession Repository Update");
            }
        }
    }
}
