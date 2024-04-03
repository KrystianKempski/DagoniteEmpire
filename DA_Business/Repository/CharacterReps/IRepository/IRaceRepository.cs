using DA_DataAccess.CharacterClasses;
using DA_Models.CharacterModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DA_Business.Repository.CharacterReps.IRepository
{
    public interface IRaceRepository
    {
        public Task<RaceDTO> Create(RaceDTO objDTO);

        public Task<RaceDTO> Update(RaceDTO objDTO);
        public Task<int> Delete(int id);

        public Task<RaceDTO> GetById(int id);
        public Task<IEnumerable<RaceDTO>> GetAll(int? baseId = null);

        public Task<IEnumerable<RaceDTO>> GetAllApproved();
    }
}
