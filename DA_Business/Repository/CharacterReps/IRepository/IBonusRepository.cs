using DA_Models.CharacterModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DA_Business.Repository.CharacterReps.IRepository
{
    public interface IBonusRepository
    {
        public Task<BonusDTO> Create(BonusDTO objDTO);

        public Task<BonusDTO> Update(BonusDTO objDTO);
        public Task<int> Delete(int id);

        public Task<BonusDTO> GetById(int id);
        public Task<IEnumerable<BonusDTO>> GetAll(int? baseId = null);
    }
}
