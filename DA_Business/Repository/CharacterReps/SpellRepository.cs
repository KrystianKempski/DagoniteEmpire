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
    public class SpellRepository : ISpellRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _db;
        private readonly IMapper _mapper;

        public SpellRepository(IDbContextFactory<ApplicationDbContext> db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }
        public async Task<Spell> Create(Spell objDTO)
        {
            try
            {
                using var contex = await _db.CreateDbContextAsync();
                var addedObj = await contex.Spells.AddAsync(objDTO);
                await contex.SaveChangesAsync();

                return addedObj.Entity;
            }
            catch (Exception ex)
            {
                throw new RepositoryErrorException("Error in Spell Repository Create");
            }                
        }

        public async Task<int> Delete(int id)
        {
            try
            {
                using var contex = await _db.CreateDbContextAsync();
                var obj = await contex.Spells.FirstOrDefaultAsync(u => u.Id == id);
                if (obj != null)
                {
                    contex.Spells.Remove(obj);
                    await contex.SaveChangesAsync();
                }
                return 0;
            }
            catch (Exception ex)
            {
                throw new RepositoryErrorException("Error in Spell Repository Delete"); ;
            }
        }

        public async Task<IEnumerable<Spell>> GetAll(int lvl)
        {

            using var contex = await _db.CreateDbContextAsync();

            var result = await contex.Spells.Where(u => u.IsApproved && u.Level <= lvl).ToListAsync();

            return result;
        }

        public async Task<Spell> GetById(int id)
        {

            using var contex = await _db.CreateDbContextAsync();
            var obj = await contex.Spells.FirstOrDefaultAsync(u => u.Id == id);
            if (obj != null)
            {
                return obj;
            }
            return new Spell();
        }

        public async Task<Spell> Update(Spell objDTO)
        {
            try
            {
                var newSpell = objDTO;
                using var contex = await _db.CreateDbContextAsync();
                var obj = await contex.Spells.FirstOrDefaultAsync(u => u.Id == objDTO.Id);
                if (obj is not null)
                {
                    // Update parent
                    contex.Entry(obj).CurrentValues.SetValues(objDTO);

                   
                    var addedObj =  contex.Spells.Update(obj);
                    await contex.SaveChangesAsync();
                    return addedObj.Entity;
                }
                else
                {
                    var addedObj = await contex.Spells.AddAsync(newSpell);
                    await contex.SaveChangesAsync();

                    return addedObj.Entity;
                }
            }
            catch (Exception ex)
            {
                throw new RepositoryErrorException("Error in Spell Repository Update");
            }
        }
    }
}
