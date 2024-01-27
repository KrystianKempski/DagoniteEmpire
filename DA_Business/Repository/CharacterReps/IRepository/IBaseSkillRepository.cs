using DA_Models.CharacterModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DA_Business.Repository.CharacterReps.IRepository
{
    public interface IBaseSkillRepository
    {
        public Task<BaseSkillDTO> Create(BaseSkillDTO objDTO);

        public Task<BaseSkillDTO> Update(BaseSkillDTO objDTO);
        public Task<int> Delete(int id);

        public Task<BaseSkillDTO> GetById(int id);
        public Task<IEnumerable<BaseSkillDTO>> GetAll();
    }
}
