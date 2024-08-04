using Abp.Collections.Extensions;
using AutoMapper;
using DA_Business.Repository.CharacterReps.IRepository;
using DA_DataAccess.CharacterClasses;
using DA_DataAccess.Data;
using DA_Models.CharacterModels;
using DagoniteEmpire.Exceptions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DA_Business.Repository.CharacterReps
{
    public class SpellSlotRepository : ISpellSlotRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _db;

        public SpellSlotRepository(IDbContextFactory<ApplicationDbContext> db)
        {
            _db = db;
        }
        public async Task<SpellSlot> Create(SpellSlot objDTO)
        {
            try
            {
                using var contex = await _db.CreateDbContextAsync();
                var addedObj = await contex.SpellSlots.AddAsync(objDTO);
                await contex.SaveChangesAsync();

                return addedObj.Entity;
            }
            catch (Exception ex)
            {
                throw new RepositoryErrorException("Error in SpellSlot Repository Create");
            }                
        }

        public async Task<int> Delete(int id)
        {
            try
            {
                using var contex = await _db.CreateDbContextAsync();
                var obj = await contex.SpellSlots.FirstOrDefaultAsync(u => u.Id == id);
                if (obj != null)
                {
                    contex.SpellSlots.Remove(obj);
                    await contex.SaveChangesAsync();
                }
                return 0;
            }
            catch (Exception ex)
            {
                throw new RepositoryErrorException("Error in SpellSlot Repository Delete"); ;
            }
        }

        public async Task<IEnumerable<SpellSlot>> GetAll(int? spellCircleId =null)
        {
            using var contex = await _db.CreateDbContextAsync();
            if (spellCircleId == null || spellCircleId < 1)
                return contex.SpellSlots;
           return contex.SpellSlots.Where(u => u.SpellCircleId== spellCircleId);
        }

        public async Task<SpellSlot> GetById(int id)
        {
            using var contex = await _db.CreateDbContextAsync();
            var obj = await contex.SpellSlots.FirstOrDefaultAsync(u => u.Id == id);
            if (obj != null)
            {
                return obj;
            }
            return new SpellSlot();
        }

        public async Task<SpellSlot> Update(SpellSlot objDTO)
        {
            try
            {
                using var contex = await _db.CreateDbContextAsync();
                var obj = await contex.SpellSlots.Include(t=>t.Spell).FirstOrDefaultAsync(u => u.Id == objDTO.Id);
                if (obj is not null)
                {
                    //// Update parent
                    //contex.Entry(obj).CurrentValues.SetValues(objDTO);

                    // // Delete spell
                    //if (obj.Spell is not null)
                    //{
                    //    if (obj.Spell.IsApproved == true)
                    //    {
                    //        obj.Spell = null;
                    //        obj.SpellId = 0;
                    //    }
                    //    else
                    //        contex.Spells.Remove(obj.Spell);
                    //}

                    //// Update and Insert bonuses
                    //if (objDTO.Spell is not null)
                    //{
                    //    Spell? existingChild;
                    //    existingChild = await contex.Spells.Include(t => t.SpellSlots).FirstOrDefaultAsync(u => u.Id == objDTO.Spell.Id);
                    //    if (existingChild != null)
                    //    {
                    //        obj.SpellId = existingChild.Id;
                    //        obj.Spell = existingChild;
                    //    }
                    //    else
                    //    {
                    //       var spell =  await contex.Spells.AddAsync(objDTO.Spell);
                    //       obj.Spell = spell.Entity;
                    //    }
                    //} 

                    // Update slot
                    // update spell
                    Spell existingSpell = contex.Spells.Include(t => t.SpellSlots).FirstOrDefault(c => c.Id == obj.SpellId && c.Id != default(int));

                    // remove spell
                    if ((objDTO.SpellId is null || objDTO.SpellId == 0) &&  existingSpell is not null)
                    {
                        if (existingSpell.IsApproved && existingSpell.SpellSlots is not null && existingSpell.SpellSlots.Any())
                        {
                            existingSpell.SpellSlots.Remove(obj);
                        }
                        else
                        {
                            contex.Spells.Remove(existingSpell);
                        }
                    }

                    contex.Entry(obj).CurrentValues.SetValues(objDTO);
                    if (obj.Spell is not null && objDTO.Spell is not null)
                    {

                        contex.Entry(obj.Spell).CurrentValues.SetValues(objDTO.Spell);
                    //else
                        obj.Spell = objDTO.Spell;
                    }
                    else
                    {
                        obj.Spell = null;
                        obj.SpellId = default(int);
                    }

                    var addedObj =  contex.SpellSlots.Update(obj);
                    await contex.SaveChangesAsync();
                    return addedObj.Entity;
                }
                else
                {
                    var addedObj = await contex.SpellSlots.AddAsync(objDTO);
                    await contex.SaveChangesAsync();

                    return addedObj.Entity;
                }
            }
            catch (Exception ex)
            {
                throw new RepositoryErrorException("Error in SpellSlot Repository Update");
            }
        }
    }
}
