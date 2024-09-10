using DA_Models.CharacterModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DA_Business.Repository.CharacterReps.IRepository
{
    public interface IAttributeRepository
    {
        public Task<AttributeDTO> Create(AttributeDTO objDTO);

        public Task<AttributeDTO> Update(AttributeDTO objDTO);
        public Task<int> Delete(int id);

        public Task<AttributeDTO> GetById(int id);
        public Task<IDictionary<string,AttributeDTO>> GetAll(int? charId = null);
    }
}
