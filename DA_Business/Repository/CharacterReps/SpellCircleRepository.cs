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
    public class SpellCircleRepository : ISpellCircleRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _db;
        private readonly IMapper _mapper;

        public SpellCircleRepository(IDbContextFactory<ApplicationDbContext> db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }
        public async Task<SpellCircleDTO> Create(SpellCircleDTO objDTO)
        {
            try
            {
                using var contex = await _db.CreateDbContextAsync();
                var obj = _mapper.Map<SpellCircleDTO, SpellCircle>(objDTO);
                var addedObj = await contex.SpellCircles.AddAsync(obj);
                await contex.SaveChangesAsync();

                return _mapper.Map <SpellCircle, SpellCircleDTO>(addedObj.Entity);
            }
            catch (Exception ex)
            {
                throw new RepositoryErrorException("Error in SpellCircle Repository Create");
            }
        }       

        public async Task<int> Delete(int id)
        {
            try
            {
                using var contex = await _db.CreateDbContextAsync();

                var obj = await contex.SpellCircles.FirstOrDefaultAsync(u => u.Id == id);
                if (obj is not null)
                {
                    contex.SpellCircles.Remove(obj);
                    await contex.SaveChangesAsync();
                }
            return 0;
            }
            catch (Exception ex)
            {
                throw new RepositoryErrorException("Error in SpellCircles Repository Delete");
            }
        }

        public async Task<IEnumerable<SpellCircleDTO>> GetAll(int profId)
        {
            using var contex = await _db.CreateDbContextAsync();

            return _mapper.Map<IEnumerable<SpellCircle>, IEnumerable<SpellCircleDTO>>(contex.SpellCircles.Where(u => u.ProfessionId == profId));
    
        }

        public async Task<SpellCircleDTO> GetById(int id)
        {
            using var contex = await _db.CreateDbContextAsync();
            var obj = await contex.SpellCircles.FirstOrDefaultAsync(u => u.Id == id);
            if (obj != null)
            {
                return _mapper.Map<SpellCircle, SpellCircleDTO>(obj);
            }
            return new SpellCircleDTO();
        }

        public async Task<SpellCircleDTO> Update(SpellCircleDTO objDTO)
        {
            try
            {

                var newCircle = _mapper.Map<SpellCircleDTO, SpellCircle>(objDTO);
                using var contex = await _db.CreateDbContextAsync();
                var obj = await contex.SpellCircles.FirstOrDefaultAsync(u => u.Id == objDTO.Id);
                if (obj is not null)
                {
                    // Update parent
                    contex.Entry(obj).CurrentValues.SetValues(objDTO);
                    await contex.SaveChangesAsync();
                    return _mapper.Map<SpellCircle, SpellCircleDTO>(obj);
                }
                else
                {
                    var addedObj = contex.SpellCircles.Add(newCircle);
                    await contex.SaveChangesAsync();
                    return _mapper.Map<SpellCircle, SpellCircleDTO>(addedObj.Entity);
                }
            }
            catch (Exception ex)
            {
                throw new RepositoryErrorException("Error in SpellCircle Repository Update");
            }
        }

    }
}
