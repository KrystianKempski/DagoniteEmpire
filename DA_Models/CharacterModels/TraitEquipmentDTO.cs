﻿using DA_Common;
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
        public override string TraitType { get; set; } = SD.TraitType_Gear;
        public TraitEquipmentDTO(TraitDTO traitDTO, EquipmentDTO? equipmentDTO)
        {
            foreach (var prop in traitDTO.GetType().GetProperties())
            {
                if(this.GetType().GetProperty(prop.Name).CanWrite)
                    this.GetType().GetProperty(prop.Name).SetValue(this, prop.GetValue(traitDTO, null), null);
            }
            if (this.Equipment is not null && equipmentDTO is not null)
            {
                var equ = this.Equipment.FirstOrDefault(r => r.Id == equipmentDTO.Id);
                if (equ is null)
                    this.Equipment.Add(equipmentDTO);
                else
                    equ = equipmentDTO;
            }
        }
        public ICollection<EquipmentDTO>? Equipment { get; set; }
        public override string TraitLabel { get => "item property"; }
    }
}
