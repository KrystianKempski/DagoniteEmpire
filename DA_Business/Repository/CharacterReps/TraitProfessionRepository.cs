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
    public class TraitProfessionRepository : ITraitRepository<TraitProfessionDTO>
    {
        private readonly IDbContextFactory<ApplicationDbContext> _db;
        private readonly IMapper _mapper;

        public TraitProfessionRepository(IDbContextFactory<ApplicationDbContext> db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }
        public async Task<TraitProfessionDTO> Create(TraitProfessionDTO objDTO)
        {
            try
            {
                using var contex = await _db.CreateDbContextAsync();
                var obj = _mapper.Map<TraitProfessionDTO, TraitProfession>(objDTO);
                var addedObj = await contex.TraitsProfession.AddAsync(obj);
                await contex.SaveChangesAsync();

                return _mapper.Map<TraitProfession, TraitProfessionDTO>(addedObj.Entity);
            }
            catch (Exception ex)
            {
                throw new RepositoryErrorException("Error in Trait Profession Repository Create");
            }
        }

        public async Task<TraitProfessionDTO> Add(TraitProfessionDTO objDTO, int Id)
        {

            using var contex = await _db.CreateDbContextAsync();
            var profession = await contex.Professions.FirstOrDefaultAsync(c => c.Id == Id);
            if (profession == null)
                return null;
            var obj = _mapper.Map<TraitProfessionDTO, TraitProfession>(objDTO);
            obj.ProfessionId = Id;

            var addedObj = await contex.TraitsProfession.AddAsync(obj);
            await contex.SaveChangesAsync();

            return _mapper.Map<TraitProfession, TraitProfessionDTO>(addedObj.Entity);
        }

        public async Task<int> Delete(int id)
        {
            try
            {
                using var contex = await _db.CreateDbContextAsync();
                var obj = await contex.TraitsProfession.Include(t=>t.Bonuses).FirstOrDefaultAsync(u => u.Id == id && u.TraitApproved == false);
                if (obj != null)
                {
                    if (obj.TraitApproved == true)
                        return 0;
                    contex.TraitsProfession.Remove(obj);
                    await contex.SaveChangesAsync();
                }
                return 0;
            }
            catch (Exception ex)
            {
                throw new RepositoryErrorException("Error in Trait-Profession Repository Delete"); ;
            }
        }

        public async Task<IEnumerable<TraitProfessionDTO>> GetAll(int? profId =null)
        {
            using var contex = await _db.CreateDbContextAsync();
            if (profId == null || profId < 1)
                return _mapper.Map<IEnumerable<TraitProfession>, IEnumerable<TraitProfessionDTO>>(contex.TraitsProfession.Include(u => u.Bonuses));
            return _mapper.Map<IEnumerable<TraitProfession>, IEnumerable<TraitProfessionDTO>>(contex.TraitsProfession.Include(u => u.Bonuses).Where(u => u.ProfessionId == profId));
        }

        public async Task<IEnumerable<TraitProfessionDTO>> GetAllApproved(bool addUnique = false)
        {
            using var contex = await _db.CreateDbContextAsync();
            if (addUnique)
                return _mapper.Map<IEnumerable<TraitProfession>, IEnumerable<TraitProfessionDTO>>(contex.TraitsProfession.Include(u => u.Bonuses).Where(t => t.TraitApproved == true));

            return _mapper.Map<IEnumerable<TraitProfession>, IEnumerable<TraitProfessionDTO>>(contex.TraitsProfession.Include(u => u.Bonuses).Where(t => t.TraitApproved == true && t.IsUnique == false));
        }

        public async Task<TraitProfessionDTO> GetById(int id)
        {

            using var contex = await _db.CreateDbContextAsync();
            var obj = await contex.TraitsProfession.Include(u=>u.Bonuses).FirstOrDefaultAsync(u => u.Id == id);
            if (obj != null)
            {
                return _mapper.Map<TraitProfession, TraitProfessionDTO>(obj);
            }
            return new TraitProfessionDTO();
        }

        public async Task<TraitProfessionDTO> Update(TraitProfessionDTO objDTO)
        {
            try
            {
                var newTrait = _mapper.Map<TraitProfessionDTO, TraitProfession>(objDTO);
                using var contex = await _db.CreateDbContextAsync();
                var obj = await contex.TraitsProfession.Include(t=>t.Bonuses).FirstOrDefaultAsync(u => u.Id == objDTO.Id);
                if (obj is not null)
                {
                    //obj.Name = newTrait.Name;    
                    //obj.Descr = newTrait.Descr;
                    //obj.Index = newTrait.Index;
                    //obj.TraitType = newTrait.TraitType;
                    //obj.TraitValue = newTrait.TraitValue;
                    //obj.TraitApproved = newTrait.TraitApproved;
                    //obj.IsUnique = newTrait.IsUnique;

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


                    contex.Entry(obj).CurrentValues.SetValues(objDTO);

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

                    var addedObj =  contex.TraitsProfession.Update(obj);
                    await contex.SaveChangesAsync();
                    return _mapper.Map<TraitProfession, TraitProfessionDTO>(addedObj.Entity);
                }
                else
                {
                    var addedObj = await contex.TraitsProfession.AddAsync(newTrait);
                    await contex.SaveChangesAsync();

                    return _mapper.Map<TraitProfession, TraitProfessionDTO>(addedObj.Entity);
                }
            }
            catch (Exception ex)
            {
                throw new RepositoryErrorException("Error in Trait-Profession Repository Update");
            }
        }
    }
}
