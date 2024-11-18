using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DA_DataAccess.CharacterClasses
{
    public class WealthRecord
    {
        public int Id { get;set; }
        public int CharacterId { get; set; } = 0;
        public string Description { get;set; } = string.Empty;
        public decimal Value { get;set; } = decimal.Zero;
        public int DateNumber { get; set; } = 1;

    }
}
