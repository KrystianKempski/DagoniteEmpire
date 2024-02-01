using DA_Models.CharacterModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DA_Business.Repository.CharacterReps.IRepository
{
    public interface ICharacterRepository
    {
        public Task<CharacterDTO> Create(CharacterDTO objDTO);

        public Task<CharacterDTO> Update(CharacterDTO objDTO);
        public Task<int> Delete(int id);

        public Task<CharacterDTO> GetById(int id);
        public Task<IEnumerable<CharacterDTO>> GetAll(int? id=null);
    }
}
