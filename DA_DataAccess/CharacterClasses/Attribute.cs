using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DA_DataAccess.CharacterClasses
{
    public class Attribute
    {
        [Key] public int Id { get; set; }

        public string? Name { get; set; }

        public int BaseBonus { get; set; } = 0;
        public int RaceBonus { get; set; } = 0;
        public int GearBonus { get; set; } = 0;
        public Dictionary<string, int> OtherBonuses = new Dictionary<string, int>();
        public int HealthBonus { get; set; } = 0;

        //public int SumOfAttribute()
        //{
        //   return BaseBonus+ RaceBonus+ GearBonus+ HealthBonus + OtherBonuses.Values.AsEnumerable().Sum();
        //}
        //public int GetModyfier()
        //{
        //    return (int)Math.Floor( (double)(SumOfAttribute() - 10) / 2.0);
        //}
    }
}

