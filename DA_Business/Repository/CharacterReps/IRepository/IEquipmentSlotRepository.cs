using DA_DataAccess.CharacterClasses;
using DA_Models.CharacterModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DA_Business.Repository.CharacterReps.IRepository
{
    public interface IEquipmentSlotRepository
    {
        public Task<EquipmentSlotDTO> Create(EquipmentSlotDTO objDTO);

        public Task<EquipmentSlotDTO> Update(EquipmentSlotDTO objDTO);
        public Task<int> Delete(int id);

        public Task<EquipmentSlotDTO> GetById(int id);
        public Task<IEnumerable<EquipmentSlotDTO>> GetAll(int? charId = null);
    }
}
