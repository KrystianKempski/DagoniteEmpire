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
            Canonicalize(obj);
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
                    obj = await contex.SpecialSkills.AsNoTracking().ToListAsync();
                }
                else
                {
                    obj = await contex.SpecialSkills.AsNoTracking().Where(u => u.CharacterId == charId).OrderBy(u => u.Index).ToListAsync();
                }

                if (obj != null && obj.Any())
                {
                    var list = _mapper.Map<IEnumerable<SpecialSkill>, IEnumerable<SpecialSkillDTO>>(obj);
                    IDictionary<string, SpecialSkillDTO> result = new Dictionary<string, SpecialSkillDTO>();
                    foreach (var atr in list)
                    {
                        Canonicalize(atr);
                        result[atr.Name] = atr;
                    }
                    return result;
                }
            }
            catch (Exception ex)
            {
                throw new RepositoryErrorException("Error in" + System.Reflection.MethodBase.GetCurrentMethod().Name , ex);
            }

            return new Dictionary<string, SpecialSkillDTO>();
        }

        public async Task<IEnumerable<SpecialSkillDTO>> GetAllFromGroup(int charId ,string baseSkillName)
        {
            try
            {
                List<SpecialSkill> obj;
                using var contex = await _db.CreateDbContextAsync();
                if (charId < 1 || baseSkillName == string.Empty)
                {
                    return new List<SpecialSkillDTO>();
                }
                else
                {
                    var canonicalBase = BaseSkills.Canonical(baseSkillName);
                    obj = await contex.SpecialSkills.AsNoTracking().Where(u => u.CharacterId == charId).OrderBy(u => u.Index).ToListAsync();
                    var list = _mapper.Map<IEnumerable<SpecialSkill>, IEnumerable<SpecialSkillDTO>>(obj).ToList();
                    foreach (var dto in list)
                        Canonicalize(dto);
                    return list.Where(s => BaseSkills.Canonical(s.RelatedBaseSkillName) == canonicalBase).ToList();
                }               
            }
            catch (Exception ex)
            {
                throw new RepositoryErrorException("Error in" + System.Reflection.MethodBase.GetCurrentMethod().Name , ex);
            }
        }

        public async Task<SpecialSkillDTO> GetById(int id)
        {
            using var contex = await _db.CreateDbContextAsync();
            var obj = await contex.SpecialSkills.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);
            if (obj != null)
            {
                var dto = _mapper.Map<SpecialSkill, SpecialSkillDTO>(obj);
                Canonicalize(dto);
                return dto;
            }
            return new SpecialSkillDTO();
        }

        public async Task<SpecialSkillDTO> Update(SpecialSkillDTO objDTO)
        {
            using var contex = await _db.CreateDbContextAsync();
            var obj = await contex.SpecialSkills.FirstOrDefaultAsync(u => u.Id == objDTO.Id);
            if (obj != null)
            {
                Canonicalize(objDTO);
                // Update parent
                contex.Entry(obj).CurrentValues.SetValues(objDTO);
                await contex.SaveChangesAsync();
                return _mapper.Map<SpecialSkill, SpecialSkillDTO>(obj);
            }
            else
            {
                Canonicalize(objDTO);
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

        private static void Canonicalize(SpecialSkill obj)
        {
            obj.Name = SpecialSkills.Canonical(obj.Name);
            if (!string.IsNullOrEmpty(obj.RelatedBaseSkillName))
                obj.RelatedBaseSkillName = BaseSkills.Canonical(obj.RelatedBaseSkillName);
            if (!string.IsNullOrEmpty(obj.RelatedAttribute1))
                obj.RelatedAttribute1 = Attributes.Canonical(obj.RelatedAttribute1);
            if (!string.IsNullOrEmpty(obj.RelatedAttribute2))
                obj.RelatedAttribute2 = Attributes.Canonical(obj.RelatedAttribute2);
            if (!string.IsNullOrEmpty(obj.ChosenAttribute))
                obj.ChosenAttribute = Attributes.Canonical(obj.ChosenAttribute);
        }

        private static void Canonicalize(SpecialSkillDTO obj)
        {
            obj.Name = SpecialSkills.Canonical(obj.Name);
            if (!string.IsNullOrEmpty(obj.RelatedBaseSkillName))
                obj.RelatedBaseSkillName = BaseSkills.Canonical(obj.RelatedBaseSkillName);
            if (!string.IsNullOrEmpty(obj.RelatedAttribute1))
                obj.RelatedAttribute1 = Attributes.Canonical(obj.RelatedAttribute1);
            if (!string.IsNullOrEmpty(obj.RelatedAttribute2))
                obj.RelatedAttribute2 = Attributes.Canonical(obj.RelatedAttribute2);
            if (!string.IsNullOrEmpty(obj.ChosenAttribute))
                obj.ChosenAttribute = Attributes.Canonical(obj.ChosenAttribute);
        }
    }
}
