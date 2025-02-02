﻿using Abp.Collections.Extensions;
using AutoMapper;
using DA_Business.Repository.CharacterReps.IRepository;
using DA_DataAccess.CharacterClasses;
using DA_DataAccess.Data;
using DA_Models.CharacterModels;
using DagoniteEmpire.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DA_Business.Repository.CharacterReps
{
    public class TraitRaceRepository : ITraitRepository<TraitRaceDTO>
    {
        private readonly IDbContextFactory<ApplicationDbContext> _db;
        private readonly IMapper _mapper;
        private readonly IJSRuntime _jsRuntime;

        public TraitRaceRepository(IDbContextFactory<ApplicationDbContext> db, IMapper mapper, IJSRuntime jsRuntime)
        {
            _db = db;
            _mapper = mapper;
            _jsRuntime = jsRuntime;
        }
        public async Task<TraitRaceDTO> Create(TraitRaceDTO objDTO)
        {
            try
            {
                using var contex = await _db.CreateDbContextAsync();
                //How to use EF 6 many-to-many with IDbContextFactory in Blazor Server
                var obj = _mapper.Map<TraitRaceDTO, TraitRace>(objDTO);
                var addedObj = contex.TraitsRace.Add(obj);
                await contex.SaveChangesAsync();

                return _mapper.Map<TraitRace, TraitRaceDTO>(addedObj.Entity);
            }
            catch (Exception ex)
            {

                throw new RepositoryErrorException("Error in Trait-race Repository Create: " + ex.Message);
            }
                
        }

        public async Task<TraitRaceDTO> Add(TraitRaceDTO objDTO, int Id)
        {
            using var contex = await _db.CreateDbContextAsync();
            var race = await contex.Races.Include(u => u.Traits).FirstOrDefaultAsync(c => c.Id == Id);
            if (race == null)
                return null;
            var obj = _mapper.Map<TraitRaceDTO, TraitRace>(objDTO);
            race.Traits.Add(obj);
            await contex.SaveChangesAsync();

            return _mapper.Map<TraitRace, TraitRaceDTO>(obj);
        }

        public async Task<int> Delete(int id)
        {
            try
            {
                using var contex = await _db.CreateDbContextAsync();
                var obj = await contex.TraitsRace.FirstOrDefaultAsync(u => u.Id == id && u.TraitApproved == false);
                if (obj != null)
                {
                    contex.TraitsRace.Remove(obj);
                    await contex.SaveChangesAsync();
                }
                return 0;
            }
            catch (Exception ex)
            {
                throw new RepositoryErrorException("Error in Trait-race Repository delete: " + ex.Message);
            }
        }

        public async Task<IEnumerable<TraitRaceDTO>> GetAll(int? raceId = null)
        {
            using var contex = await _db.CreateDbContextAsync();
            if (raceId == null || raceId < 1)
                return _mapper.Map<IEnumerable<TraitRace>, IEnumerable<TraitRaceDTO>>(contex.TraitsRace.Include(u => u.Bonuses));
           return _mapper.Map<IEnumerable<TraitRace>, IEnumerable<TraitRaceDTO>>(contex.TraitsRace.Include(u => u.Bonuses).Where(u => u.Races.FirstOrDefault(r=>r.Id == raceId) != null));
        }

        public async Task<IEnumerable<TraitRaceDTO>> GetAllApproved(bool addUnique = false)
        {
            using var contex = await _db.CreateDbContextAsync();
            if (addUnique)
                return _mapper.Map<IEnumerable<TraitRace>, IEnumerable<TraitRaceDTO>>(contex.TraitsRace.Include(u => u.Bonuses).Where(t => t.TraitApproved == true));
            return _mapper.Map<IEnumerable<TraitRace>, IEnumerable<TraitRaceDTO>>(contex.TraitsRace.Include(u => u.Bonuses).Where(t=>t.TraitApproved==true && t.IsUnique == false));
        }

        public async Task<TraitRaceDTO> GetById(int id)
        {
            using var contex = await _db.CreateDbContextAsync();
            var obj = await contex.TraitsRace.Include(u=>u.Bonuses).FirstOrDefaultAsync(u => u.Id == id);
            if (obj != null)
            {
                return _mapper.Map<TraitRace, TraitRaceDTO>(obj);
            }
            return new TraitRaceDTO();
        }

        public async Task<TraitRaceDTO> Update(TraitRaceDTO objDTO)
        {
            try
            {
                using var contex = await _db.CreateDbContextAsync();
                var newTrait = _mapper.Map<TraitRaceDTO, TraitRace>(objDTO);
                var obj = await contex.TraitsRace.FirstOrDefaultAsync(u => u.Id == objDTO.Id);
                if (obj != null)
                {
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

                    var updatedObj = contex.TraitsRace.Update(obj);
                    await contex.SaveChangesAsync();
                    return _mapper.Map<TraitRace, TraitRaceDTO>(updatedObj.Entity);
                }
                else
                {
                   
                    var addedObj = await contex.TraitsRace.AddAsync(newTrait);
                    await contex.SaveChangesAsync();

                    return _mapper.Map<TraitRace, TraitRaceDTO>(addedObj.Entity);
                }
            }
            catch (Exception ex)
            {
                throw new RepositoryErrorException("Error in Trait-race Repository Update: " + ex.Message); ;
            }
        }

    }
}
