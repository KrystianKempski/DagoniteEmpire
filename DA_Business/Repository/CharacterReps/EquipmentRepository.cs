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

namespace DA_Business.Repository.CharacterReps
{
    public class EquipmentRepository : IEquipmentRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _db;
        private readonly IMapper _mapper;

        public EquipmentRepository(IDbContextFactory<ApplicationDbContext> db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }
        public async Task<EquipmentDTO> Create(EquipmentDTO objDTO)
        {
            try
            {
                using var contex = await _db.CreateDbContextAsync();
                var obj = _mapper.Map<EquipmentDTO, Equipment>(objDTO);
                //handle traitsEquipment
                var traits = await contex.TraitsEquipment.ToListAsync();
                traits.ForEach(t =>
                {
                    if (obj.Traits.Any(nt => nt.Id == t.Id))
                    {
                        var untracked = obj.Traits.FirstOrDefault(nt => nt.Id == t.Id);
                        obj.Traits.Remove(untracked);
                        obj.Traits.Add(t);
                    }
                });

                var addedObj = await contex.Equipment.AddAsync(obj);
                await contex.SaveChangesAsync();

                return _mapper.Map<Equipment, EquipmentDTO>(addedObj.Entity);
            }
            catch (Exception ex)
            {
                throw new RepositoryErrorException("Error in Equipment Repository Create");
            }                
        }

        public async Task<int> Delete(int id)
        {
            try
            {
                using var contex = await _db.CreateDbContextAsync();
                var obj = await contex.Equipment.Include(u=>u.Traits).FirstOrDefaultAsync(u => u.Id == id);
                if (obj is not null)
                {
                    var traits = obj.Traits;
                    foreach(var t in traits)
                    {
                        if (t.TraitApproved == false)
                        {
                            contex.Traits.Remove(t);
                        }
                    }
                    contex.Equipment.Remove(obj);
                    await contex.SaveChangesAsync();
                }
            return 0;
            }
            catch (Exception ex)
            {
                throw new RepositoryErrorException("Error in Equipment Repository Delete");
            }
            return 0;
        }

        public async Task<IEnumerable<EquipmentDTO>> GetAll(int? charId = null)
        {
            using var contex = await _db.CreateDbContextAsync();
            if (charId == null || charId < 1)
                return _mapper.Map<IEnumerable<Equipment>, IEnumerable<EquipmentDTO>>(contex.Equipment.Include(u => u.Traits).ThenInclude(b => b.Bonuses));
            return _mapper.Map<IEnumerable<Equipment>, IEnumerable<EquipmentDTO>>(contex.Equipment.Include(u => u.Traits).ThenInclude(b => b.Bonuses).Where(u => u.Characters.FirstOrDefault(c => c.Id == charId) != null));
        }

        public async Task<IEnumerable<EquipmentDTO>> GetAllApproved()
        {
            using var contex = await _db.CreateDbContextAsync();
            return _mapper.Map<IEnumerable<Equipment>, IEnumerable<EquipmentDTO>>(contex.Equipment.Include(u => u.Traits).ThenInclude(b => b.Bonuses).Where(t=>t.IsApproved == true));
        }

        public async Task<EquipmentDTO> GetById(int id)
        {
            using var contex = await _db.CreateDbContextAsync();
            var obj = await contex.Equipment.Include(u => u.Traits).ThenInclude(b => b.Bonuses).FirstOrDefaultAsync(u => u.Id == id);
            if (obj != null)
            {
                return _mapper.Map<Equipment, EquipmentDTO>(obj);
            }
            return new EquipmentDTO();
        }

        public async Task<EquipmentDTO> Update(EquipmentDTO objDTO)
        {
            try
            {
                using var contex = await _db.CreateDbContextAsync();
                var obj = await contex.Equipment.Include(r=>r.Traits).ThenInclude(b => b.Bonuses).FirstOrDefaultAsync(u => u.Id == objDTO.Id);
                if (obj is not null)
                {
                    var updatedEquipment = _mapper.Map<EquipmentDTO, Equipment>(objDTO);

                    // Update built-in type members
                    contex.Entry(obj).CurrentValues.SetValues(updatedEquipment);


                    // Delete equipment traits
                    if (obj.Traits is not null)
                    {
                        foreach (var existingChild in obj.Traits.ToList())
                        {
                            if (!updatedEquipment.Traits.Any(c => c.Id == existingChild.Id))
                            {
                                if (existingChild.TraitApproved == true)
                                {
                                    var detachedTrait = contex.TraitsEquipment.Include(t => t.Bonuses).Include(c => c.Equipment).FirstOrDefault(c => c.Id == existingChild.Id && c.Id != default(int));
                                    if (detachedTrait != null && !detachedTrait.Equipment.IsNullOrEmpty() && detachedTrait.Equipment.Contains(obj))
                                    {
                                        detachedTrait.Equipment.Remove(obj);
                                        contex.TraitsEquipment.Update(detachedTrait);
                                    }
                                }
                                else
                                    contex.TraitsEquipment.Remove(existingChild);
                            }
                        }
                    }


                    // Update and Insert traits
                    if (updatedEquipment.Traits is not null)
                    {
                        foreach (var trait in updatedEquipment.Traits)
                        {
                            TraitEquipment? existingTrait = null;
                            if (obj.Traits is not null)
                            {
                                existingTrait = obj.Traits
                                    .FirstOrDefault(c => c.Id == trait.Id && c.Id != default(int));
                            }
                            else
                            {
                                obj.Traits = new List<TraitEquipment>();
                            }

                            if (existingTrait == null)
                            {
                                existingTrait = contex.TraitsEquipment.Include(t => t.Bonuses).Include(c => c.Equipment).FirstOrDefault(c => c.Id == trait.Id && c.Id != default(int));
                            }

                            if (existingTrait is not null)
                            {
                                if (existingTrait.TraitApproved && obj.Traits.Contains(existingTrait) == false)
                                {
                                    if (existingTrait.Equipment is null)
                                        existingTrait.Equipment = new List<Equipment>();
                                    existingTrait.Equipment.Add(obj);
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
                                obj.Traits.Add(trait);
                        }
                    }
                    await contex.SaveChangesAsync();
                    return _mapper.Map<Equipment, EquipmentDTO>(obj);
                }
                else
                {
                    obj = _mapper.Map<EquipmentDTO, Equipment>(objDTO);

                    var addedObj = contex.Equipment.Add(obj);
                    await contex.SaveChangesAsync();

                    return _mapper.Map<Equipment, EquipmentDTO>(addedObj.Entity);
                }
            }
            catch (Exception ex)
            {
                throw new RepositoryErrorException("Error in Equipment Repository Update");
            }
            return null;
        }
    }
}
