using DA_Models.CharacterModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DA_Business.Repository.CharacterReps.IRepository
{
    public interface ITraitAdvRepository
    {
        public Task<TraitAdvDTO> Create(TraitAdvDTO objDTO);

        public Task<TraitAdvDTO> Update(TraitAdvDTO objDTO);
        public Task<int> Delete(int id);

        public Task<TraitAdvDTO> GetById(int id);
        public Task<IEnumerable<TraitAdvDTO>> GetAll(int? baseId = null);

        public Task<IEnumerable<TraitAdvDTO>> GetAllApproved();
    }
}
