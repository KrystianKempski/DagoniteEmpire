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
    public class CharacterRepository : ICharacterRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly IMapper _mapper;

        public CharacterRepository(ApplicationDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }
        public async Task<CharacterDTO> Create(CharacterDTO objDTO)
        {
            throw new NotImplementedException();
        }

        public async Task<int> Delete(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<CharacterDTO>> GetAll()
        {
            throw new NotImplementedException();
        }

        public async Task<CharacterDTO> GetById(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<CharacterDTO> Update(CharacterDTO objDTO)
        {
            throw new NotImplementedException();
        }
    }
}
