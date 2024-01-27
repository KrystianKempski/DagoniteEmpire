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
    public class BaseSkillRepository : IBaseSkillRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly IMapper _mapper;

        public BaseSkillRepository(ApplicationDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }
        public async Task<BaseSkillDTO> Create(BaseSkillDTO objDTO)
        {
            throw new NotImplementedException();
        }

        public async Task<int> Delete(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<BaseSkillDTO>> GetAll()
        {
            throw new NotImplementedException();
        }

        public async Task<BaseSkillDTO> GetById(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<BaseSkillDTO> Update(BaseSkillDTO objDTO)
        {
            throw new NotImplementedException();
        }
    }
}
