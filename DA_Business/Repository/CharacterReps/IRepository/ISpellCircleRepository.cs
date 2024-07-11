using DA_DataAccess.CharacterClasses;
using DA_Models.CharacterModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DA_Business.Repository.CharacterReps.IRepository
{
    public interface ISpellCircleRepository
    {
        public Task<SpellCircleDTO> Create(SpellCircleDTO objDTO);

        public Task<SpellCircleDTO> Update(SpellCircleDTO objDTO);
        public Task<int> Delete(int id);

        public Task<SpellCircleDTO> GetById(int id);
        public Task<IEnumerable<SpellCircleDTO>> GetAll(int profId);

    }
}
