using DA_DataAccess.CharacterClasses;
using DA_Models.CharacterModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DA_Business.Repository.CharacterReps.IRepository
{
    public interface IEquipmentRepository
    {
        public Task<EquipmentDTO> Create(EquipmentDTO objDTO);

        public Task<EquipmentDTO> Update(EquipmentDTO objDTO);
        public Task<int> Delete(int id);

        public Task<EquipmentDTO> GetById(int id);
        public Task<IEnumerable<EquipmentDTO>> GetAll(int? charId = null);

        public Task<IEnumerable<EquipmentDTO>> GetAllApproved();
    }
}
