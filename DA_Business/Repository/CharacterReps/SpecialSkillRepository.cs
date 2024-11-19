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
using static DA_Common.SD;

namespace DA_Business.Repository.CharacterReps
{
    public class SpecialSkillRepository : ISpecialSkillRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _db;
        private readonly IMapper _mapper;

        public SpecialSkillRepository(IDbContextFactory<ApplicationDbContext> db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }
        public async Task<SpecialSkillDTO> Create(SpecialSkillDTO objDTO)
        {
            using var contex = await _db.CreateDbContextAsync();
            var obj = _mapper.Map<SpecialSkillDTO, SpecialSkill>(objDTO);
            var addedObj = await contex.SpecialSkills.AddAsync(obj);
            await contex.SaveChangesAsync();

            return _mapper.Map<SpecialSkill, SpecialSkillDTO>(addedObj.Entity);
        }

        public async Task<int> Delete(int id)
        {
            using var contex = await _db.CreateDbContextAsync();
            var obj = await contex.SpecialSkills.FirstOrDefaultAsync(u => u.Id == id);
            if (obj != null)
            {
                contex.SpecialSkills.Remove(obj);
                await contex.SaveChangesAsync();
            }
            return 0;
        }

        public async Task<IDictionary<string, SpecialSkillDTO>> GetAll(int? charId = null)
        {
            try
            {
                List<SpecialSkill> obj;
                using var contex = await _db.CreateDbContextAsync();
                if (charId == null || charId < 1)
                {
                    obj = contex.SpecialSkills.ToList();
                }
                else
                {
                    obj = contex.SpecialSkills.Where(u => u.CharacterId == charId).OrderBy(u => u.Index).ToList();
                }

                if (obj != null && obj.Any())
                {
                    var list = _mapper.Map<IEnumerable<SpecialSkill>, IEnumerable<SpecialSkillDTO>>(obj);
                    IDictionary<string, SpecialSkillDTO> result = new Dictionary<string, SpecialSkillDTO>();
                    foreach (var atr in list)
                    {
                        result[atr.Name] = atr;
                    }
                    return result;
                }
            }
            catch (Exception ex)
            {
                throw new RepositoryErrorException("Error in" + System.Reflection.MethodBase.GetCurrentMethod().Name +": " + ex.Message);
            }

            return new Dictionary<string, SpecialSkillDTO>();
        }

        public async Task<IEnumerable<SpecialSkillDTO>> GetAllFromGroup(int charId ,string baseSkillName)
        {
            try
            {
                List<SpecialSkill> obj;
                using var contex = await _db.CreateDbContextAsync();
                if (charId == null || charId < 1 || baseSkillName == string.Empty)
                {
                    return new List<SpecialSkillDTO>();
                }
                else
                {
                    obj = contex.SpecialSkills.Where(u => u.CharacterId == charId && u.RelatedBaseSkillName == baseSkillName).OrderBy(u => u.Index).ToList();
                    return _mapper.Map<IEnumerable<SpecialSkill>, IEnumerable<SpecialSkillDTO>>(obj);
                }               
            }
            catch (Exception ex)
            {
                throw new RepositoryErrorException("Error in" + System.Reflection.MethodBase.GetCurrentMethod().Name + ": " + ex.Message);
            }
        }

        public async Task<SpecialSkillDTO> GetById(int id)
        {
            using var contex = await _db.CreateDbContextAsync();
            var obj = await contex.SpecialSkills.FirstOrDefaultAsync(u => u.Id == id);
            if (obj != null)
            {
                return _mapper.Map<SpecialSkill, SpecialSkillDTO>(obj);
            }
            return new SpecialSkillDTO();
        }

        public async Task<SpecialSkillDTO> Update(SpecialSkillDTO objDTO)
        {
            using var contex = await _db.CreateDbContextAsync();
            var obj = await contex.SpecialSkills.FirstOrDefaultAsync(u => u.Id == objDTO.Id);
            if (obj != null)
            {
                // Update parent
                contex.Entry(obj).CurrentValues.SetValues(objDTO);
                await contex.SaveChangesAsync();
                return _mapper.Map<SpecialSkill, SpecialSkillDTO>(obj);
            }
            else
            {
                obj = _mapper.Map<SpecialSkillDTO, SpecialSkill>(objDTO);
                var addedObj = await contex.SpecialSkills.AddAsync(obj);
                await contex.SaveChangesAsync();

                return _mapper.Map<SpecialSkill, SpecialSkillDTO>(addedObj.Entity);
            }
        }
        public async Task Delete(SpecialSkillDTO objDTO)
        {
            using var contex = await _db.CreateDbContextAsync();
            var obj = await contex.SpecialSkills.FirstOrDefaultAsync(u => u.Id == objDTO.Id);
            if (obj != null)
            {
                contex.SpecialSkills.Remove(obj);
                await contex.SaveChangesAsync();
               
            }
        }
    }
}
