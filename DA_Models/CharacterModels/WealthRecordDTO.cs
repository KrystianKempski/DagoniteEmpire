using DA_Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DA_Models.CharacterModels
{
    public  class WealthRecordDTO
    {
        public int Id { get; set; }
        public int CharacterId { get; set; } = 0;
        public string Description { get; set; } = string.Empty;
        public decimal Value { get; set; } = decimal.Zero;
        public int Imperials 
        { 
            get
            {
                return (int)Value;  
            }
        } 
        public int Talars
        {
            get
            {
                return (int)((Value - Imperials) * 10) ;
            }
        }
        public int Hellers
        {
            get
            {
                return (int)(((Value - Imperials)*10 - Talars) * 10);
            }
        }
        public int Coppers
        {
            get
            {
                return (int)((((Value - Imperials) * 10 - Talars) * 10 - Hellers) *100);
            }
        }
        public int DateNumber { get; set; } = 1;
        public DateModel CurrentDate
        {
            set
            {
                DateNumber = value.AllDays;
            }
            get => new DateModel(DateNumber);
        }
    }
}
