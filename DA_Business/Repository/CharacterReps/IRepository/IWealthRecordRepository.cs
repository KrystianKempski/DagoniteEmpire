using DA_DataAccess.CharacterClasses;
using DA_Models.CharacterModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DA_Business.Repository.CharacterReps.IRepository
{
    public interface IWealthRecordRepository
    {
        public Task<WealthRecordDTO> Create(WealthRecordDTO objDTO);

        public Task<WealthRecordDTO> Update(WealthRecordDTO objDTO);
        public Task<int> Delete(int id);

        public Task<WealthRecordDTO> GetById(int id);
        public Task<IEnumerable<WealthRecordDTO>> GetAll(int? charId = null);
    }
}
