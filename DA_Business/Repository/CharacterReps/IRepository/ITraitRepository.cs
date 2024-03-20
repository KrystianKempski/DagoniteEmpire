using DA_Models.CharacterModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DA_Business.Repository.CharacterReps.IRepository
{
    public interface ITraitRepository
    {
        public Task<TraitDTO> Create(TraitDTO objDTO);

        public Task<TraitDTO> Update(TraitDTO objDTO);
        public Task<int> Delete(int id);

        public Task<TraitDTO> GetById(int id);
        public Task<IEnumerable<TraitDTO>> GetAll(int? baseId = null);
    }
}
