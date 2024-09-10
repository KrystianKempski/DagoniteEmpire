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

        public async Task<IDictionary<string,AttributeDTO>> GetAll(int? charId = null)
        {
            try
            {
                List<Attribute> obj;
                using var contex = await _db.CreateDbContextAsync();
                if (charId == null || charId < 1)
                {
                    obj = contex.Attributes.ToList();
                }
                else
                {
                    obj = contex.Attributes.Where(u => u.CharacterId == charId).OrderBy(u => u.Index).ToList();
                }     
           
                if (obj != null && obj.Any())
                {
                    var list = _mapper.Map<IEnumerable<Attribute>, IEnumerable<AttributeDTO>>(obj);
                    IDictionary<string, AttributeDTO> result = new Dictionary<string, AttributeDTO>();
                    foreach (var atr in list)
                    {
                        result[atr.Name] = atr;
                    }
                    return result;
                }
            }
            catch (Exception ex) { 
                throw new RepositoryErrorException("Error in" + System.Reflection.MethodBase.GetCurrentMethod().Name); 
            }

            return new Dictionary<string, AttributeDTO>();
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
                // Update parent
                contex.Entry(obj).CurrentValues.SetValues(objDTO);
                await contex.SaveChangesAsync();
                return _mapper.Map<Attribute, AttributeDTO>(obj);
            }
            return objDTO;
        }
    }
}
