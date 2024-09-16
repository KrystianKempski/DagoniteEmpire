using Abp.Collections.Extensions;
using DA_Common;
using DA_DataAccess.CharacterClasses;
using DA_Models.CharacterModels;
using DA_Models.ComponentInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DA_Common.SD;

namespace DA_Models.ComponentModels
{
    public class HealthModel 
    {
        private readonly AllParamsModel _allParams;
        private ICollection<WoundDTO> Wounds { get; set; } = new List<WoundDTO>();
        public int MaxWounds 
        { 
            get
            {
                int? res = _allParams?.Attributes?.Get(SD.Attributes.Endurance)?.SumAbsolute + _allParams?.Attributes?.Get(SD.Attributes.Willpower)?.SumAbsolute;

                if(res is not null)
                {
                    return ((int)res + 1) / 2 + 2;
                }
                return 0;
            }
        }
        public int CurrentWounds
        {
            get
            {
                int res = 0;
                foreach (var w in Wounds)
                {
                    res += w.Penalty;
                }
                return res;
            }
        }
        public int HealingModyfier
        {
            get
            {
                int? res = _allParams?.Attributes?.Get(SD.Attributes.Endurance)?.ModifierAbsolute;
                if(res is not null)
                {
                    return (int)((100.0 + (double)res * 12.5)) ;
                }
                return 100;
            }
        }

        public HealthModel(AllParamsModel allParams)
        {
            _allParams = allParams;
        }
        public void FillPropertiesContainer(IEnumerable<WoundDTO> properties)
        {
            Wounds = (ICollection<WoundDTO>)properties;
        }

        public ICollection<WoundDTO> GetAll()
        {
            return Wounds.ToList();
        }
        public WoundDTO? Get(int id)
        {
            return Wounds.FirstOrDefault(w=>w.Id == id);
        }
        public ICollection<WoundDTO>? GetLocation(string location)
        {
            return (ICollection<WoundDTO>)Wounds.Where(w => w.Location == location);
        }

    }
}
