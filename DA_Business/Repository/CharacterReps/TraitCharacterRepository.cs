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
    public class TraitCharacterRepository : ITraitRepository<TraitCharacterDTO>
    {
       // private readonly ApplicationDbContext _db;

        private readonly IDbContextFactory<ApplicationDbContext> _db;
        private readonly IMapper _mapper;

        public TraitCharacterRepository(IDbContextFactory<ApplicationDbContext> db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }
        public async Task<TraitCharacterDTO> Create(TraitCharacterDTO objDTO)
        {
            try
            {
                using var contex = await _db.CreateDbContextAsync();
                var obj = _mapper.Map<TraitCharacterDTO, TraitCharacter>(objDTO);
                var addedObj = await contex.TraitsCharacter.AddAsync(obj);
                await contex.SaveChangesAsync();

                return _mapper.Map<TraitCharacter, TraitCharacterDTO>(addedObj.Entity);
            }
            catch (Exception ex)
            {
                throw new RepositoryErrorException("Error in Trait-Adv Repository Create: " + ex.Message); ;
            }
                
        }
        //public async Task<TraitCharacterDTO> Add(TraitCharacterDTO objDTO,int Id)
        //{
        //    try
        //    {
        //        using var contex = await _db.CreateDbContextAsync();
        //        var character = await contex.Characters.Include(u=>u.TraitsCharacter).FirstOrDefaultAsync(c => c.Id == Id);
        //        if (character == null)
        //            return null;
        //        var obj = _mapper.Map<TraitCharacterDTO, TraitCharacter>(objDTO);
        //        if(character.TraitsCharacter is null)
        //            character.TraitsCharacter = new List<TraitCharacter>();
        //        character.TraitsCharacter.Add(obj);

        //        await contex.SaveChangesAsync();

        //        return _mapper.Map<TraitCharacter, TraitCharacterDTO>(obj);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new RepositoryErrorException("Error in Trait-Adv Repository Create");
        //    }

        //}

        public async Task<int> Delete(int id)
        {
            try
            {
                using var contex = await _db.CreateDbContextAsync();
                var obj = await contex.TraitsCharacter.Include(t=>t.Bonuses).FirstOrDefaultAsync(u => u.Id == id && u.TraitApproved == false);
                if (obj != null)
                {
                    contex.TraitsCharacter.Remove(obj);
                    await contex.SaveChangesAsync();
                }
                return 0;
            }
            catch (Exception ex)
            {
                throw new RepositoryErrorException("Error in Trait-Adv Repository Delete: " + ex.Message);
            }
        }

        public async Task<IEnumerable<TraitCharacterDTO>> GetAll(int? charId =null)
        {

            using var contex = await _db.CreateDbContextAsync();
            if (charId == null || charId < 1)
                return _mapper.Map<IEnumerable<TraitCharacter>, IEnumerable<TraitCharacterDTO>>(contex.TraitsCharacter.Include(u => u.Bonuses));
           return _mapper.Map<IEnumerable<TraitCharacter>, IEnumerable<TraitCharacterDTO>>(contex.TraitsCharacter.Include(u => u.Bonuses).Where(u => u.CharacterId == charId));
        }

        public async Task<IEnumerable<TraitCharacterDTO>> GetAllApproved(bool addUnique = false)
        {

            using var contex = await _db.CreateDbContextAsync();
            if (addUnique)
                return _mapper.Map<IEnumerable<TraitCharacter>, IEnumerable<TraitCharacterDTO>>(contex.TraitsCharacter.Include(u => u.Bonuses).Where(t => t.TraitApproved == true));
                    
            return _mapper.Map<IEnumerable<TraitCharacter>, IEnumerable<TraitCharacterDTO>>(contex.TraitsCharacter.Include(u => u.Bonuses).Where(t=>t.TraitApproved==true && t.IsUnique == false));
        }


        public async Task<TraitCharacterDTO> GetById(int id)
        {

            using var contex = await _db.CreateDbContextAsync();
            var obj = await contex.TraitsCharacter.Include(u=>u.Bonuses).FirstOrDefaultAsync(u => u.Id == id);
            if (obj != null)
            {
                return _mapper.Map<TraitCharacter, TraitCharacterDTO>(obj);
            }
            return new TraitCharacterDTO();
        }

        public async Task<TraitCharacterDTO> Update(TraitCharacterDTO objDTO)
        {
            try
            {
                var newTrait = _mapper.Map<TraitCharacterDTO, TraitCharacter>(objDTO);
                using var contex = await _db.CreateDbContextAsync();
                var obj = await contex.TraitsCharacter.Include(t=>t.Bonuses).FirstOrDefaultAsync(u => u.Id == objDTO.Id);
                if (obj is not null)
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

                    var addedObj =  contex.TraitsCharacter.Update(obj);
                    await contex.SaveChangesAsync();
                    return _mapper.Map<TraitCharacter, TraitCharacterDTO>(addedObj.Entity);
                }
                else
                {
                    var addedObj = await contex.TraitsCharacter.AddAsync(newTrait);
                    await contex.SaveChangesAsync();

                    return _mapper.Map<TraitCharacter, TraitCharacterDTO>(addedObj.Entity);
                }
            }
            catch (Exception ex)
            {
                throw new RepositoryErrorException("Error in Trait-Race Repository Update: " + ex.Message); ;
            }
        }
    }
}
