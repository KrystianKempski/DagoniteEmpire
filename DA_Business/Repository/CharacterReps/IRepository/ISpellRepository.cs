using DA_DataAccess.CharacterClasses;
using DA_Models.CharacterModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DA_Business.Repository.CharacterReps.IRepository
{
    public interface ISpellRepository
    {
        public Task<Spell> Create(Spell objDTO);

        public Task<Spell> Update(Spell objDTO);
        public Task<int> Delete(int id);

        public Task<Spell> GetById(int id);
        public Task<IEnumerable<Spell>> GetAll(int lvl=0);

    }
}
