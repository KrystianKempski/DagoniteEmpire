using Abp.Collections.Extensions;
using AutoMapper;
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

namespace DA_Business.Repository.CharacterReps
{
    public class TraitAdvRepository : ITraitAdvRepository
    {
       // private readonly ApplicationDbContext _db;

        private readonly IDbContextFactory<ApplicationDbContext> _db;
        private readonly IMapper _mapper;

        public TraitAdvRepository(IDbContextFactory<ApplicationDbContext> db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }
        public async Task<TraitAdvDTO> Create(TraitAdvDTO objDTO)
        {
            try
            {
                using var contex = await _db.CreateDbContextAsync();
                var obj = _mapper.Map<TraitAdvDTO, TraitAdv>(objDTO);
                var addedObj = await contex.TraitsAdv.AddAsync(obj);
                await contex.SaveChangesAsync();

                return _mapper.Map<TraitAdv, TraitAdvDTO>(addedObj.Entity);
            }
            catch (Exception ex)
            {
                ;
            }
            return null;
                
}

        public async Task<int> Delete(int id)
        {
            try
            {
                using var contex = await _db.CreateDbContextAsync();
                var obj = await contex.TraitsAdv.Include(t=>t.Bonuses).FirstOrDefaultAsync(u => u.Id == id && u.TraitApproved == false);
                if (obj != null)
                {
                    contex.TraitsAdv.Remove(obj);
                    await contex.SaveChangesAsync();
                }
                return 0;
            }
            catch (Exception ex)
            {
                ;
            }
            return 0;
        }

        public async Task<IEnumerable<TraitAdvDTO>> GetAll(int? charId =null)
        {

            using var contex = await _db.CreateDbContextAsync();
            if (charId == null || charId < 1)
                return _mapper.Map<IEnumerable<TraitAdv>, IEnumerable<TraitAdvDTO>>(contex.TraitsAdv.Include(u => u.Bonuses));
           return _mapper.Map<IEnumerable<TraitAdv>, IEnumerable<TraitAdvDTO>>(contex.TraitsAdv.Include(u => u.Bonuses).Where(u => u.Characters.FirstOrDefault(c=>c.Id == charId)!=null));
        }

        public async Task<IEnumerable<TraitAdvDTO>> GetAllApproved(bool addUnique = false)
        {

            using var contex = await _db.CreateDbContextAsync();
            if (addUnique)
                return _mapper.Map<IEnumerable<TraitAdv>, IEnumerable<TraitAdvDTO>>(contex.TraitsAdv.Include(u => u.Bonuses).Where(t => t.TraitApproved == true));
                    
            return _mapper.Map<IEnumerable<TraitAdv>, IEnumerable<TraitAdvDTO>>(contex.TraitsAdv.Include(u => u.Bonuses).Where(t=>t.TraitApproved==true && t.IsUnique == false));
        }

        public async Task<TraitAdvDTO> GetById(int id)
        {

            using var contex = await _db.CreateDbContextAsync();
            var obj = await contex.TraitsAdv.Include(u=>u.Bonuses).FirstOrDefaultAsync(u => u.Id == id);
            if (obj != null)
            {
                return _mapper.Map<TraitAdv, TraitAdvDTO>(obj);
            }
            return new TraitAdvDTO();
        }

        public async Task<TraitAdvDTO> Update(TraitAdvDTO objDTO)
        {
            try
            {
                var newTrait = _mapper.Map<TraitAdvDTO, TraitAdv>(objDTO);
                using var contex = await _db.CreateDbContextAsync();
                var obj = await contex.TraitsAdv.Include(t=>t.Bonuses).FirstOrDefaultAsync(u => u.Id == objDTO.Id);
                if (obj is not null)
                {
                    obj.Name = newTrait.Name;    
                    obj.Descr = newTrait.Descr;
                    obj.SummaryDescr = newTrait.SummaryDescr;
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

                    var addedObj =  contex.TraitsAdv.Update(obj);
                    await contex.SaveChangesAsync();
                    return _mapper.Map<TraitAdv, TraitAdvDTO>(addedObj.Entity);
                }
                else
                {
                    var addedObj = await contex.TraitsAdv.AddAsync(newTrait);
                    await contex.SaveChangesAsync();

                    return _mapper.Map<TraitAdv, TraitAdvDTO>(addedObj.Entity);
                }
            }
            catch (Exception ex)
            {
                ;
            }
            return null;
        }
    }
}
