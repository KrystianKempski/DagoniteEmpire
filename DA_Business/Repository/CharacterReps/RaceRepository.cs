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
                //var traits = await contex.TraitsRace.ToListAsync();
                //traits.ForEach(t =>
                //{
                //    if (obj.Traits.Any(nt => nt.Id == t.Id))
                //    {
                //        var untracked = obj.Traits.FirstOrDefault(nt => nt.Id == t.Id);
                //        obj.Traits.Remove(untracked);
                //        obj.Traits.Add(t);
                //    }
                //});

                var addedObj = await contex.Races.AddAsync(obj);
                await contex.SaveChangesAsync();

                return _mapper.Map<Race, RaceDTO>(addedObj.Entity);
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
                ;
            }
            return 0;
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
                    //var traits = await _db.TraitsRace.ToListAsync();

                    // Update parent
                    contex.Entry(obj).CurrentValues.SetValues(updatedRace);


                    // Delete race traits
                    if (!obj.Traits.IsNullOrEmpty())
                    {
                        foreach (var existingChild in obj.Traits.ToList())
                        {
                            if (!updatedRace.Traits.Any(c => c.Id == existingChild.Id))
                            {
                                if (existingChild.TraitApproved == true)
                                    obj.Traits.Remove(existingChild);
                                else
                                    contex.Traits.Remove(existingChild);
                            }
                        }
                    }

                    // Update and Insert characters
                    if (updatedRace.Traits is not null)
                    {
                        foreach (var childTrait in updatedRace.Traits)
                        {
                            TraitRace? existingChild;
                            if (!obj.Traits.IsNullOrEmpty())
                            {
                                existingChild = obj.Traits
                               .Where(c => c.Id == childTrait.Id && c.Id != default(int))
                               .SingleOrDefault();
                            }
                            else
                            {
                                obj.Traits = new List<TraitRace>();
                                existingChild = null;
                            }

                            if (existingChild != null)
                            {


                                // Update trait
                                contex.Entry(existingChild).CurrentValues.SetValues(childTrait);
                                // update bonuses

                                // Delete trait bonuses
                                if (!existingChild.Bonuses.IsNullOrEmpty())
                                {
                                    foreach (var existingChildBonus in existingChild.Bonuses.ToList())
                                    {
                                        if (!childTrait.Bonuses.Any(c => c.Id == existingChildBonus.Id))
                                        {
                                            contex.Bonuses.Remove(existingChildBonus);
                                        }
                                    }
                                }

                                // Update and Insert bonuses
                                if (childTrait.Bonuses is not null)
                                {
                                    foreach (var childTraitBonus in childTrait.Bonuses)
                                    {
                                        Bonus? existingChildTrait;
                                        if (!existingChild.Bonuses.IsNullOrEmpty())
                                        {
                                            existingChildTrait = existingChild.Bonuses
                                           .Where(c => c.Id == childTraitBonus.Id && c.Id != default(int))
                                           .SingleOrDefault();
                                        }
                                        else
                                        {
                                            existingChild.Bonuses = new List<Bonus>();
                                            existingChildTrait = null;
                                        }

                                        if (existingChildTrait != null)
                                            // Update bonus
                                            contex.Entry(existingChildTrait).CurrentValues.SetValues(childTraitBonus);
                                        else
                                            // Insert bonus
                                            existingChild.Bonuses.Add(childTraitBonus);
                                    }
                                }


                            }
                            else
                                // Insert trait
                                obj.Traits.Add(childTrait);
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
                ;
            }
            return null;
        }
    }
}
