using AutoMapper;
using DA_Business.Repository.CharacterReps.IRepository;
using DA_DataAccess.CharacterClasses;
using DA_DataAccess.Data;
using DA_Models.CharacterModels;
using DagoniteEmpire.Exceptions;
using Microsoft.EntityFrameworkCore;
using Attribute = DA_DataAccess.CharacterClasses.Attribute;

namespace DA_Business.Repository.CharacterReps
{
    public class AttributeRepository : IAttributeRepository
    {

        private readonly IDbContextFactory<ApplicationDbContext> _db;
        private readonly IMapper _mapper;

        public AttributeRepository(IDbContextFactory<ApplicationDbContext> db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<AttributeDTO> Create(AttributeDTO objDTO)
        {
            try
            {
                using var contex = await _db.CreateDbContextAsync();
                var obj = _mapper.Map<AttributeDTO, Attribute>(objDTO);
                var addedObj = await contex.Attributes.AddAsync(obj);           
                await contex.SaveChangesAsync();
                return _mapper.Map<Attribute, AttributeDTO>(addedObj.Entity);
            }
            catch (Exception ex) { throw new RepositoryErrorException("Error in" + System.Reflection.MethodBase.GetCurrentMethod().Name); }
        }

        public async Task<int> Delete(int id)
        {
            using var contex = await _db.CreateDbContextAsync();
            var obj = await contex.Attributes.FirstOrDefaultAsync(u => u.Id == id);
            if (obj is not null)
            {
                contex.Attributes.Remove(obj);
                await contex.SaveChangesAsync();
            }
            return 0;
        }

        public async Task<IEnumerable<AttributeDTO>> GetAll(int? charId = null)
        {
            using var contex = await _db.CreateDbContextAsync();
            if (charId == null || charId < 1)
                return _mapper.Map<IEnumerable<Attribute>, IEnumerable<AttributeDTO>>(contex.Attributes/*.Include(u => u.TraitBonusRelated)*/);
            try
            {
                var obj = contex.Attributes./*Include(u => u.TraitBonusRelated).*/Where(u => u.CharacterId == charId).OrderBy(u => u.Index).ToList();
                if (obj != null && obj.Any())
                    return _mapper.Map<IEnumerable<Attribute>, IEnumerable<AttributeDTO>>(obj);
            }
            catch (Exception ex) { 
                throw new RepositoryErrorException("Error in" + System.Reflection.MethodBase.GetCurrentMethod().Name); 
            }


            return new List<AttributeDTO>();
        }

        public async Task<AttributeDTO> GetById(int id)
        {
            using var contex = await _db.CreateDbContextAsync();
            var obj = await contex.Attributes/*.Include(u => u.TraitBonusRelated)*/.FirstOrDefaultAsync(u => u.Id == id);
            if (obj != null)
            {
                return _mapper.Map<Attribute, AttributeDTO>(obj);
            }
            return new AttributeDTO();
        }

        public async Task<AttributeDTO> Update(AttributeDTO objDTO)
        {

            using var contex = await _db.CreateDbContextAsync();
            var obj = await contex.Attributes.FirstOrDefaultAsync(u => u.Id == objDTO.Id);
            if (obj != null)
            {
                //obj.CharacterId = objDTO.CharacterId;        //is it nessesary?
                //obj.OtherBonuses = objDTO.OtherBonuses;        //is it nessesary?
                //obj.RaceBonus = objDTO.RaceBonus;  //is it nessesary?
                //obj.BaseBonus = objDTO.BaseBonus;
                //obj.HealthBonus = objDTO.HealthBonus;
                //obj.GearBonus = objDTO.GearBonus;
                //obj.Index = objDTO.Index;
                //await contex.Attributes.Update(obj);
                //await contex.Attributes.SaveChangesAsync();
                //return _mapper.Map<Attribute, AttributeDTO>(obj);

                // Update parent
                contex.Entry(obj).CurrentValues.SetValues(objDTO);
                await contex.SaveChangesAsync();
                return _mapper.Map<Attribute, AttributeDTO>(obj);
            }
            return objDTO;
        }
    }
}
