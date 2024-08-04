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
    public class ProfessionSkillRepository : IProfessionSkillRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _db;
        public ProfessionSkillRepository(IDbContextFactory<ApplicationDbContext> db)
        {
            _db = db;
        }
        public async Task<ProfessionSkill> Create(ProfessionSkill objDTO)
        {
            try
            {
                using var contex = await _db.CreateDbContextAsync();
               
                var addedObj = await contex.ProfessionSkills.AddAsync(objDTO);
                await contex.SaveChangesAsync();

                return addedObj.Entity;
            }
            catch (Exception ex)
            {
                throw new RepositoryErrorException("Error in ProfessionSkill Repository Create");
            }

        }       

        public async Task<int> Delete(int id)
        {
            try
            {
                using var contex = await _db.CreateDbContextAsync();

                var obj = await contex.ProfessionSkills.FirstOrDefaultAsync(u => u.Id == id);
                if (obj is not null)
                {
                    contex.ProfessionSkills.Remove(obj);
                    await contex.SaveChangesAsync();
                }
            return 0;
            }
            catch (Exception ex)
            {
                throw new RepositoryErrorException("Error in ProfessionSkill Repository Delete");
            }
        }

        public async Task<IEnumerable<ProfessionSkill>> GetAll()
        {
            using var contex = await _db.CreateDbContextAsync();

            return contex.ProfessionSkills;
        }

        public async Task<IEnumerable<ProfessionSkill>> GetAllApproved()
        {
            using var contex = await _db.CreateDbContextAsync();
            return contex.ProfessionSkills.Where(t=>t.IsApproved == true);
        }

        public async Task<ProfessionSkill> GetById(int id)
        {
            using var contex = await _db.CreateDbContextAsync();
            var obj = await contex.ProfessionSkills.FirstOrDefaultAsync(u => u.Id == id);
            if (obj != null)
            {
                return obj;
            }
            return new ProfessionSkill();
        }

        public async Task<ProfessionSkill> Update(ProfessionSkill objDTO)
        {
            try
            {
                using var contex = await _db.CreateDbContextAsync();
                var obj = await contex.ProfessionSkills.FirstOrDefaultAsync(u => u.Id == objDTO.Id);
                if (obj is not null)
                {
                    // Update parent
                    contex.Entry(obj).CurrentValues.SetValues(objDTO);
                    await contex.SaveChangesAsync();
                    return obj;
                }
                else
                {
                    var addedObj = contex.ProfessionSkills.Add(objDTO);
                    await contex.SaveChangesAsync();
                    return addedObj.Entity;
                }
            }
            catch (Exception ex)
            {
                throw new RepositoryErrorException("Error in ProfessionSkill Repository Update");
            }
        }
    }
}
