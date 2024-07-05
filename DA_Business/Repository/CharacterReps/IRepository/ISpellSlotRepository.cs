using DA_DataAccess.CharacterClasses;
using DA_Models.CharacterModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DA_Business.Repository.CharacterReps.IRepository
{
    public interface ISpellSlotRepository
    {
        public Task<SpellSlot> Create(SpellSlot objDTO);

        public Task<SpellSlot> Update(SpellSlot objDTO);
        public Task<int> Delete(int id);

        public Task<SpellSlot> GetById(int id);
        public Task<IEnumerable<SpellSlot>> GetAll(int? circleId = null);

    }
}
