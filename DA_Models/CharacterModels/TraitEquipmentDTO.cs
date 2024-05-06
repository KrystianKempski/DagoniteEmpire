using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DA_Models.CharacterModels
{
    public class TraitEquipmentDTO : TraitDTO
    {
        public TraitEquipmentDTO() { }
        public TraitEquipmentDTO(TraitDTO traitDTO, EquipmentDTO equipmentDTO)
        {
            foreach (var prop in traitDTO.GetType().GetProperties())
            {
                this.GetType().GetProperty(prop.Name).SetValue(this, prop.GetValue(traitDTO, null), null);
            }
            if (this.Equipment is not null)
            {
                var equ = this.Equipment.FirstOrDefault(r => r.Id == equipmentDTO.Id);
                if (equ is null)
                    this.Equipment.Add(equipmentDTO);
                else
                    equ = equipmentDTO;
            }
        }
        public ICollection<EquipmentDTO>? Equipment { get; set; }
    }
}
