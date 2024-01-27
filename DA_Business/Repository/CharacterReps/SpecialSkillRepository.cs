using AutoMapper;
using DA_Business.Repository.CharacterReps.IRepository;
using DA_DataAccess.Data;
using DA_Models.CharacterModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DA_Business.Repository.CharacterReps
{
    public class SpecialSkillRepository : ISpecialSkillRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly IMapper _mapper;

        public SpecialSkillRepository(ApplicationDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }
        public async Task<SpecialSkillDTO> Create(SpecialSkillDTO objDTO)
        {
            throw new NotImplementedException();
        }

        public async Task<int> Delete(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<SpecialSkillDTO>> GetAll()
        {
            throw new NotImplementedException();
        }

        public async Task<SpecialSkillDTO> GetById(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<SpecialSkillDTO> Update(SpecialSkillDTO objDTO)
        {
            throw new NotImplementedException();
        }
    }
}
