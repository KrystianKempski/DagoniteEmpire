using DA_Models.CharacterModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DA_Business.Repository.CharacterReps.IRepository
{
    public interface ITraitRaceRepository
    {
        public Task<TraitRaceDTO> Create(TraitRaceDTO objDTO);

        public Task<TraitRaceDTO> Update(TraitRaceDTO objDTO, int raceId);
        public Task<int> Delete(int id);

        public Task<TraitRaceDTO> GetById(int id);
        public Task<IEnumerable<TraitRaceDTO>> GetAll(int? raceId = null);

        public Task<IEnumerable<TraitRaceDTO>> GetAllApproved(bool addUnique=false);
    }
}
