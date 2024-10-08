using AutoMapper;
using DA_Business.Repository.CharacterReps.IRepository;
using DA_DataAccess.Data;
using DA_DataAccess.CharacterClasses;
using DA_Models.CharacterModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Attribute = DA_DataAccess.CharacterClasses.Attribute;
using System.Diagnostics.Metrics;
using Abp.Collections.Extensions;
using System.Diagnostics;
using DagoniteEmpire.Exceptions;
using DA_Common;

namespace DA_Business.Repository.CharacterReps
{
    public class MobRepository : IMobRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _db;
        private readonly IMapper _mapper;

        public MobRepository(IDbContextFactory<ApplicationDbContext> db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }
        public async Task<MobDTO> Create(MobDTO objDTO)
        {
            var obj = _mapper.Map<MobDTO, Mob>(objDTO);
            using var contex = await _db.CreateDbContextAsync();
            try
            {
                var addedObj = contex.Mobs.Add(obj);
                await contex.SaveChangesAsync();
                return _mapper.Map<Mob, MobDTO>(addedObj.Entity);
            }
            catch (Exception ex) {
                throw new RepositoryErrorException("Error in"+ System.Reflection.MethodBase.GetCurrentMethod().Name); 
            }
        }

        public async Task<int> Delete(int id)
        {
            try
            {
                using var contex = await _db.CreateDbContextAsync();
                var obj = await contex.Mobs.FirstOrDefaultAsync(u => u.Id == id);
                if (obj is not null)
                {
                    contex.Mobs.Remove(obj);                   
                }
                return await contex.SaveChangesAsync();
            }
            catch (Exception ex) {
                 throw new RepositoryErrorException("Error in" + System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        public async Task<IEnumerable<MobDTO>> GetAll()
        {
            using var contex = await _db.CreateDbContextAsync();
             return _mapper.Map<IEnumerable<Mob>, IEnumerable<MobDTO>>(contex.Mobs);
        }

        public async Task<MobDTO> GetById(int id)
        {
            using var contex = await _db.CreateDbContextAsync();
            var obj = await contex.Mobs.FirstOrDefaultAsync(u => u.Id == id);
            if (obj != null)
            {
                return _mapper.Map<Mob, MobDTO>(obj); ;
            }
            return new MobDTO();
        }
        
        public async Task<IEnumerable<MobDTO>> GetAllForCampaign(int campaignId)
        {
            //try
            //{
            //    using var contex = await _db.CreateDbContextAsync();
            //    if (campaignId == null || campaignId == 0)
            //        return new List<CharacterDTO>();
            //    return _mapper.Map<IEnumerable<Character>, IEnumerable<CharacterDTO>>(contex.Characters.Include(r => r.Race).Include(r => r.Race).Include(r => r.Profession).Include(r => r.Equipment)
            //        .Where(u => u.Campaigns != null && u.Campaigns.FirstOrDefault(c => c.Id == campaignId) != null));
            //}
            //catch (Exception ex)
            //{
            //    throw new RepositoryErrorException("Error in" + System.Reflection.MethodBase.GetCurrentMethod().Name);
            //}
            return null;
        }

        public async Task<MobDTO> Update(MobDTO objDTO)
        {
            try
            {
                using var contex = await _db.CreateDbContextAsync();
                var obj = await contex.Mobs.FirstOrDefaultAsync(u => u.Id == objDTO.Id);

                if (obj != null)
                {
                    var updatedChar = _mapper.Map<MobDTO, Mob>(objDTO);
                    // Update character built-in types
                    contex.Entry(obj).CurrentValues.SetValues(objDTO);                           

                    var result = contex.Mobs.Update(obj);
                    await contex.SaveChangesAsync();
                    return _mapper.Map<Mob, MobDTO>(result.Entity);
                }
                else
                {
                    obj = _mapper.Map<MobDTO, Mob>(objDTO);
                    var addedObj = contex.Mobs.Add(obj);
                    await contex.SaveChangesAsync();
                    return _mapper.Map<Mob, MobDTO>(addedObj.Entity);
                }
            }
            catch (Exception ex) { 
                throw new RepositoryErrorException("Error in" + System.Reflection.MethodBase.GetCurrentMethod().Name); 
            }
        }
    }
}
