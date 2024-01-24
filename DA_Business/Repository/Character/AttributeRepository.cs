using AutoMapper;
using DA_Business.Repository.IRepository.Character;
using DA_DataAccess.Data;
using DA_Models.CharacterModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DA_Business.Repository.Character
{
    public class AttributeRepository : IAttributeRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly IMapper _mapper;

        public AttributeRepository(ApplicationDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }
        public async Task<AttributeDTO> Create(AttributeDTO objDTO)
        {
            throw new NotImplementedException();
        }

        public async Task<int> Delete(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<AttributeDTO>> GetAll()
        {
            throw new NotImplementedException();
        }

        public async Task<AttributeDTO> GetById(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<AttributeDTO> Update(AttributeDTO objDTO)
        {
            throw new NotImplementedException();
        }
    }
}
