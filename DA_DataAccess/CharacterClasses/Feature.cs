using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DA_DataAccess.CharacterClasses
{
    public abstract class Feature
    {
        [Key] public int Id { get; set; }

        public string? Name { get; set; }
        public string? FeatureType { get; set; }

        public int Index { get; set; }

        public int BaseBonus { get; set; } = 0;
        public int RaceBonus { get; set; } = 0;
        public int GearBonus { get; set; } = 0;
        public int OtherBonuses { get; set; } = 0;
        public int TempBonuses { get; set; } = 0;
        public int HealthBonus { get; set; } = 0;
        public int CharacterId { get; set; }
        [ForeignKey(nameof(CharacterId))]
        public Character Character { get; set; }
    }
}
