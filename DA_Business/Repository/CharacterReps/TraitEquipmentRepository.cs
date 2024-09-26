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
    public class TraitEquipmentRepository : ITraitRepository<TraitEquipmentDTO>
    {
        private readonly IDbContextFactory<ApplicationDbContext> _db;
        private readonly IMapper _mapper;

        public TraitEquipmentRepository(IDbContextFactory<ApplicationDbContext> db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }
        public async Task<TraitEquipmentDTO> Create(TraitEquipmentDTO objDTO)
        {
            try
            {
                using var contex = await _db.CreateDbContextAsync();
                var obj = _mapper.Map<TraitEquipmentDTO, TraitEquipment>(objDTO);
                var addedObj = await contex.TraitsEquipment.AddAsync(obj);
                await contex.SaveChangesAsync();

                return _mapper.Map<TraitEquipment, TraitEquipmentDTO>(addedObj.Entity);
            }
            catch (Exception ex)
            {
                throw new RepositoryErrorException("Error in Trait-Equipment Repository Create");
            }
                
}

        public async Task<int> Delete(int id)
        {
            try
            {
                using var contex = await _db.CreateDbContextAsync();
                var obj = await contex.TraitsEquipment.Include(t=>t.Bonuses).FirstOrDefaultAsync(u => u.Id == id && u.TraitApproved == false);
                if (obj != null)
                {
                    contex.TraitsEquipment.Remove(obj);
                    await contex.SaveChangesAsync();
                }
                return 0;
            }
            catch (Exception ex)
            {
                throw new RepositoryErrorException("Error in Trait-Equipment Repository Delete"); ;
            }
        }

        public async Task<IEnumerable<TraitEquipmentDTO>> GetAll(int? charId =null)
        {
            using var contex = await _db.CreateDbContextAsync();
            if (charId == null || charId < 1)
                return _mapper.Map<IEnumerable<TraitEquipment>, IEnumerable<TraitEquipmentDTO>>(contex.TraitsEquipment.Include(u => u.Bonuses));
           return _mapper.Map<IEnumerable<TraitEquipment>, IEnumerable<TraitEquipmentDTO>>(contex.TraitsEquipment.Include(u => u.Bonuses).Where(u => u.Equipment.FirstOrDefault(c=>c.Id == charId)!=null));
        }

        public async Task<IEnumerable<TraitEquipmentDTO>> GetAllApproved(bool addUnique = false)
        {
            using var contex = await _db.CreateDbContextAsync();
            if (addUnique)
                return _mapper.Map<IEnumerable<TraitEquipment>, IEnumerable<TraitEquipmentDTO>>(contex.TraitsEquipment.Include(u => u.Bonuses).Where(t => t.TraitApproved == true));
                    
            return _mapper.Map<IEnumerable<TraitEquipment>, IEnumerable<TraitEquipmentDTO>>(contex.TraitsEquipment.Include(u => u.Bonuses).Where(t=>t.TraitApproved==true && t.IsUnique == false));
        }

        public async Task<TraitEquipmentDTO> GetById(int id)
        {
            using var contex = await _db.CreateDbContextAsync();
            var obj = await contex.TraitsEquipment.Include(u=>u.Bonuses).FirstOrDefaultAsync(u => u.Id == id);
            if (obj != null)
            {
                return _mapper.Map<TraitEquipment, TraitEquipmentDTO>(obj);
            }
            return new TraitEquipmentDTO();
        }

        public async Task<TraitEquipmentDTO> Update(TraitEquipmentDTO objDTO)
        {
            try
            {
                var newTrait = _mapper.Map<TraitEquipmentDTO, TraitEquipment>(objDTO);
                using var contex = await _db.CreateDbContextAsync();
                var obj = await contex.TraitsEquipment.Include(t=>t.Bonuses).FirstOrDefaultAsync(u => u.Id == objDTO.Id);
                if (obj is not null)
                {
                    obj.Name = newTrait.Name;    
                    obj.Descr = newTrait.Descr;
                    obj.Index = newTrait.Index;
                    obj.TraitType = newTrait.TraitType;
                    obj.TraitValue = newTrait.TraitValue;
                    obj.TraitApproved = newTrait.TraitApproved;
                    obj.IsUnique = newTrait.IsUnique;

                    // Delete trait bonuses
                    if (!obj.Bonuses.IsNullOrEmpty())
                    {
                        foreach (var existingChild in obj.Bonuses.ToList())
                        {
                            if (!newTrait.Bonuses.Any(c => c.Id == existingChild.Id))
                            {
                               contex.Bonuses.Remove(existingChild);
                            }
                        }
                    }

                    // Update and Insert bonuses
                    if (newTrait.Bonuses is not null)
                    {
                        foreach (var childTrait in newTrait.Bonuses)
                        {
                            Bonus? existingChild;
                            if (!obj.Bonuses.IsNullOrEmpty())
                            {
                                existingChild = obj.Bonuses
                               .Where(c => c.Id == childTrait.Id && c.Id != default(int))
                               .SingleOrDefault();
                            }
                            else
                            {
                                obj.Bonuses = new List<Bonus>();
                                existingChild = null;
                            }

                            if (existingChild != null)
                                // Update bonus
                                contex.Entry(existingChild).CurrentValues.SetValues(childTrait);
                            else
                                // Insert bonus
                                obj.Bonuses.Add(childTrait);
                        }
                    }

                    var addedObj =  contex.TraitsEquipment.Update(obj);
                    await contex.SaveChangesAsync();
                    return _mapper.Map<TraitEquipment, TraitEquipmentDTO>(addedObj.Entity);
                }
                else
                {
                    var addedObj = await contex.TraitsEquipment.AddAsync(newTrait);
                    await contex.SaveChangesAsync();

                    return _mapper.Map<TraitEquipment, TraitEquipmentDTO>(addedObj.Entity);
                }
            }
            catch (Exception ex)
            {
                throw new RepositoryErrorException("Error in Trait-Equipment Repository Update");
            }
        }
    }
}
