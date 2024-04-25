using DA_DataAccess.CharacterClasses;
using DA_Models.CharacterModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DA_Business.Repository.CharacterReps.IRepository
{
    public interface IProfessionSkillRepository
    {
        public Task<ProfessionSkill> Create(ProfessionSkill objDTO);

        public Task<ProfessionSkill> Update(ProfessionSkill objDTO);
        public Task<int> Delete(int id);

        public Task<ProfessionSkill> GetById(int id);
        public Task<IEnumerable<ProfessionSkill>> GetAll();

        public Task<IEnumerable<ProfessionSkill>> GetAllApproved();
    }
}
