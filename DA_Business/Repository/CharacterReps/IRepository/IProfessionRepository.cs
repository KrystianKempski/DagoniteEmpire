using DA_DataAccess.CharacterClasses;
using DA_Models.CharacterModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DA_Business.Repository.CharacterReps.IRepository
{
    public interface IProfessionRepository
    {
        public Task<ProfessionDTO> Create(ProfessionDTO objDTO);

        public Task<ProfessionDTO> Update(ProfessionDTO objDTO);
        public Task<int> Delete(int id);

        public Task<ProfessionDTO> GetById(int id);
        public Task<IEnumerable<ProfessionDTO>> GetAll();

        public Task<IEnumerable<ProfessionDTO>> GetAllApproved();
    }
}
