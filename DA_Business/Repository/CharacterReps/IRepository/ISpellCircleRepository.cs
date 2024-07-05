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
        public Task<SpellCircle> Create(SpellCircle objDTO);

        public Task<SpellCircle> Update(SpellCircle objDTO);
        public Task<int> Delete(int id);

        public Task<SpellCircle> GetById(int id);
        public Task<IEnumerable<SpellCircle>> GetAll(int profId);

    }
}
