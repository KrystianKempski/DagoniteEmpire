using DA_Models.CharacterModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DA_Business.Repository.CharacterReps.IRepository
{
    public interface ISpecialSkillRepository
    {
        public Task<SpecialSkillDTO> Create(SpecialSkillDTO objDTO);

        public Task<SpecialSkillDTO> Update(SpecialSkillDTO objDTO);
        public Task<int> Delete(int id);

        public Task<SpecialSkillDTO> GetById(int id);
        public Task<IDictionary<string, SpecialSkillDTO>> GetAll(int? baseId = null);
        public Task<IEnumerable<SpecialSkillDTO>> GetAllFromGroup(int charId, string baseSkillName);

        public Task Delete(SpecialSkillDTO objDTO);
    }
}
