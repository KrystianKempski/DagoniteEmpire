using DA_Models.CharacterModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DA_Business.Repository.CharacterReps.IRepository
{
    public interface ITraitRepository<T>
    {
        public Task<T> Create(T objDTO);

        public Task<T> Update(T objDTO);
        public Task<int> Delete(int id);

        public Task<T> GetById(int id);
        public Task<IEnumerable<T>> GetAll(int? charId = null);

        public Task<IEnumerable<T>> GetAllApproved(bool addUnique = false);
    }
}
