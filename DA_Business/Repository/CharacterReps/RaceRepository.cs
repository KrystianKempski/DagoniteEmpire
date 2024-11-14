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
    public class RaceRepository : IRaceRepository
    {
        //private readonly ApplicationDbContext _db;
        private readonly IDbContextFactory<ApplicationDbContext> _db;
        private readonly IMapper _mapper;

        public RaceRepository(IDbContextFactory<ApplicationDbContext> db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }
        public async Task<RaceDTO> Create(RaceDTO objDTO)
        {
            try
            {
                using var contex = await _db.CreateDbContextAsync();
                var obj = _mapper.Map<RaceDTO, Race>(objDTO);
                //handle traitsRace
                var traits = await contex.TraitsRace.ToListAsync();
                traits.ForEach(t =>
                {
                    if (obj.Traits.Any(nt => nt.Id == t.Id))
                    {
                        var untracked = obj.Traits.FirstOrDefault(nt => nt.Id == t.Id);
                        if(untracked != null)
                        {
                            obj.Traits.Remove(untracked);
                            obj.Traits.Add(t);
                        }
                    }
                });

                var addedObj = await contex.Races.AddAsync(obj);
                await contex.SaveChangesAsync();

                return _mapper.Map<Race, RaceDTO>(addedObj.Entity);
            }
            catch (Exception ex)
            {
                throw new RepositoryErrorException("Error in Race Repository Create: " + ex.Message); ;
            }                
        }

        public async Task<int> Delete(int id)
        {
            try
            {
                using var contex = await _db.CreateDbContextAsync();
                var obj = await contex.Races.Include(u=>u.Traits).FirstOrDefaultAsync(u => u.Id == id);
                
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
                    contex.Races.Remove(obj);
                    await contex.SaveChangesAsync();
                }
            return 0;
            }
            catch (Exception ex)
            {
                throw new RepositoryErrorException("Error in Race Repository Delete: " + ex.Message); ;
            }
        }

        public async Task<IEnumerable<RaceDTO>> GetAll()
        {
            using var contex = await _db.CreateDbContextAsync();

           //var obj = _mapper.Map<IEnumerable<Race>, IEnumerable<RaceDTO>>(contex.Races.Include(u => u.Traits).ThenInclude(b => b.Bonuses));


            return _mapper.Map<IEnumerable<Race>, IEnumerable<RaceDTO>>(contex.Races.Include(u => u.Traits).ThenInclude(b => b.Bonuses));
            //if (charId == null || charId < 1)
          //  return _mapper.Map<IEnumerable<Race>, IEnumerable<RaceDTO>>(_db.Races.AsNoTracking().Include(u => u.Traits).ThenInclude(b=>b.Bonuses));
          // return _mapper.Map<IEnumerable<Race>, IEnumerable<RaceDTO>>(_db.Races.Include(u => u.Traits).Where(u => u.Characters.FirstOrDefault(c=>c.Id ==charId) != null).OrderBy(u=>u.Index));
        }

        public async Task<IEnumerable<RaceDTO>> GetAllApproved()
        {
            using var contex = await _db.CreateDbContextAsync();
            return _mapper.Map<IEnumerable<Race>, IEnumerable<RaceDTO>>(contex.Races.Include(u => u.Traits).ThenInclude(b => b.Bonuses).Where(t=>t.RaceApproved == true));
        }

        public async Task<RaceDTO> GetById(int id)
        {
            using var contex = await _db.CreateDbContextAsync();
            var obj = await contex.Races.Include(u => u.Traits).ThenInclude(b => b.Bonuses).FirstOrDefaultAsync(u => u.Id == id);
            if (obj != null)
            {
                return _mapper.Map<Race, RaceDTO>(obj);
            }
            return new RaceDTO();
        }

        public async Task<RaceDTO> Update(RaceDTO objDTO)
        {
            try
            {
                using var contex = await _db.CreateDbContextAsync();
                var obj = await contex.Races.Include(r=>r.Traits).ThenInclude(b => b.Bonuses).FirstOrDefaultAsync(u => u.Id == objDTO.Id);
                if (obj is not null)
                {
                    var updatedRace = _mapper.Map<RaceDTO, Race>(objDTO);

                    // Update parent
                    contex.Entry(obj).CurrentValues.SetValues(updatedRace);


                    // Delete race traits
                    if (obj.Traits is not null)
                    {
                        foreach (var existingChild in obj.Traits.ToList())
                        {
                            if (!updatedRace.Traits.Any(c => c.Id == existingChild.Id))
                            {
                                if (existingChild.TraitApproved == true)
                                {
                                    var detachedTrait = contex.TraitsRace.Include(t => t.Bonuses).Include(c => c.Races).FirstOrDefault(c => c.Id == existingChild.Id && c.Id != default(int));
                                    if (detachedTrait == null || detachedTrait.Races.IsNullOrEmpty() || !detachedTrait.Races.Contains(obj))
                                        continue;
                                    detachedTrait.Races.Remove(obj);
                                    contex.TraitsRace.Update(detachedTrait);
                                }
                                else
                                    contex.TraitsRace.Remove(existingChild);
                            }
                        }
                    }

                    // Update and Insert traits
                    if (updatedRace.Traits is not null)
                    {
                        foreach (var trait in updatedRace.Traits)
                        {
                            TraitRace? existingTrait = null;
                            if (obj.Traits is not null)
                            {
                                existingTrait = obj.Traits
                                    .FirstOrDefault(c => c.Id == trait.Id && c.Id != default(int));
                            }
                            else
                            {
                                obj.Traits = new List<TraitRace>();
                            }

                            if (existingTrait == null)
                            {
                                existingTrait = contex.TraitsRace.Include(t => t.Bonuses).Include(c => c.Races).FirstOrDefault(c => c.Id == trait.Id && c.Id != default(int));
                            }

                            if (existingTrait is not null)
                            {
                                if (existingTrait.TraitApproved && obj.Traits.Contains(existingTrait) == false)
                                {
                                    if (existingTrait.Races is null)
                                        existingTrait.Races = new List<Race>();
                                    existingTrait.Races.Add(obj);
                                    contex.Traits.Update(existingTrait);
                                }
                                else
                                {
                                    // Update trait
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
                    return _mapper.Map<Race, RaceDTO>(obj);
                }
                else
                {
                    obj = _mapper.Map<RaceDTO, Race>(objDTO);

                    var addedObj = contex.Races.Add(obj);
                    await contex.SaveChangesAsync();

                    return _mapper.Map<Race, RaceDTO>(addedObj.Entity);
                }
            }
            catch (Exception ex)
            {
                throw new RepositoryErrorException("Error in Race Repository Update: " + ex.Message);
            }
        }
    }
}
