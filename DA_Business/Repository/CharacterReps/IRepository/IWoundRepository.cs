using DA_Models.CharacterModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DA_Business.Repository.CharacterReps.IRepository
{
    public interface IWoundRepository
    {
        public Task<WoundDTO> Create(WoundDTO objDTO);

        public Task<WoundDTO> Update(WoundDTO objDTO);
        public Task<int> Delete(int id);

        public Task<WoundDTO> GetById(int id);
        public Task<IEnumerable<WoundDTO>> GetAll(int? charId = null);
    }
}
