using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DA_DataAccess.CharacterClasses
{
    public class Wound
    {
        public int Id { get; set; }
        public string Description { get; set; } = "";
        public string Location { get; set; } = "";
        public int Value { get; set; }
        public bool IsIgnored { get; set; }
        public bool IsTended { get; set; }
        public bool IsMagicHealed { get; set; }
        public int DateMonth { get; set; }
        public int DateDay { get; set; }
        public int DateYear { get; set; }
        //public int DayOfInjury { get; set; }
        public int HealTime { get; set; }
        public bool IsCondition { get; set; } = false;

        public int CharacterId { get; set; }
        [ForeignKey(nameof(CharacterId))]
        public Character Character { get; set; }
    }
}
