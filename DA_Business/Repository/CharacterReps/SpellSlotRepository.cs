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
        private readonly IMapper _mapper;

        public SpellSlotRepository(IDbContextFactory<ApplicationDbContext> db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
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
                var newSpell = objDTO;
                using var contex = await _db.CreateDbContextAsync();
                var obj = await contex.SpellSlots.Include(t=>t.Spell).FirstOrDefaultAsync(u => u.Id == objDTO.Id);
                if (obj is not null)
                {
                    // Update parent
                    contex.Entry(obj).CurrentValues.SetValues(objDTO);

                    // Delete trait bonuses
                    if (!obj.Spell.IsNullOrEmpty())
                    {
                        if (!newSpell.Spell.Any(c => c.Id == obj.Spell.First().Id))
                        {
                            contex.Spells.Remove(obj.Spell.First());
                        }
                    }

                    // Update and Insert bonuses
                    if (newSpell.Spell is not null)
                    {
                        foreach (var childTrait in newSpell.Spell)
                        {
                            Spell? existingChild;
                            if (!obj.Spell.IsNullOrEmpty())
                            {
                                existingChild = obj.Spell
                               .Where(c => c.Id == childTrait.Id && c.Id != default(int))
                               .SingleOrDefault();
                            }
                            else
                            {
                                obj.Spell = new List<Spell>();
                                existingChild = null;
                            }

                            if (existingChild != null)
                                // Update bonus
                                contex.Entry(existingChild).CurrentValues.SetValues(childTrait);
                            else
                                // Insert bonus
                                obj.Spell.Add(childTrait);
                        }
                    }

                    var addedObj =  contex.SpellSlots.Update(obj);
                    await contex.SaveChangesAsync();
                    return addedObj.Entity;
                }
                else
                {
                    var addedObj = await contex.SpellSlots.AddAsync(newSpell);
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
