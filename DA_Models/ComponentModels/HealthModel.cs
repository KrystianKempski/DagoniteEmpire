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

        public HealthModel(AllParamsModel allParams)
        {
            _allParams = allParams;
        }
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

        public void FillPropertiesContainer(IEnumerable<WoundDTO> properties)
        {
            Wounds = (ICollection<WoundDTO>)properties;
        }

        public ICollection<WoundDTO> GetAll()
        {
            return Wounds;
        }
        public WoundDTO? Get(int id)
        {
            return Wounds.FirstOrDefault(w=>w.Id == id);
        }
        public ICollection<WoundDTO>? GetAllForLocation(string location)
        {
            return (ICollection<WoundDTO>)Wounds.Where(w => w.Location == location);
        }

        public static ICollection<string> GetAttributeNamesFromLocation(string location)
        {
            if (string.IsNullOrEmpty(location))
                return null;
            ICollection<string> res = new List<string>();
            int i = 0;
            switch (location)
            {
                case SD.WoundLocation.Head: i = (int)SD.WoundLocationEnum.Head; break;
                case SD.WoundLocation.Neck: i = (int)SD.WoundLocationEnum.Neck; break;
                case SD.WoundLocation.MainHand: i = (int)SD.WoundLocationEnum.MainHand; break;
                case SD.WoundLocation.OffHand: i = (int)SD.WoundLocationEnum.OffHand; break;
                case SD.WoundLocation.MainArm: i = (int)SD.WoundLocationEnum.MainArm; break;
                case SD.WoundLocation.OffArm: i = (int)SD.WoundLocationEnum.OffArm; break;
                case SD.WoundLocation.Body: i = (int)SD.WoundLocationEnum.Body; break;
                case SD.WoundLocation.Back: i = (int)SD.WoundLocationEnum.Back; break;
                case SD.WoundLocation.LeftLeg: i = (int)SD.WoundLocationEnum.LeftLeg; break;
                case SD.WoundLocation.RightLeg: i = (int)SD.WoundLocationEnum.RightLeg; break;
                case SD.WoundLocation.Face: i = (int)SD.WoundLocationEnum.Face; break;
            }

            res.Add(SD.WoundAttributes[i, 0]);
            res.Add(SD.WoundAttributes[i, 1]);
            return res;
        }
        public DateModel CalculateHealTime(WoundDTO wound)
        {
            int daysToReduce = (int)(wound.HealTime * (2.0 - HealingModyfier / 100.0));
            DateModel dateReduce = wound.DateStart + daysToReduce;

            if(_allParams.Character.CurrentDate > wound.DateStart)
            {
               while (_allParams.Character.CurrentDate >= dateReduce)
               {
                    switch (wound.Severity)
                    {
                        case WoundSeverity.Critical: wound.Value = wound.GetValueFromSeverity(WoundSeverity.Heavy); break;
                        case WoundSeverity.Heavy: wound.Value = wound.GetValueFromSeverity(WoundSeverity.Moderate); break;
                        case WoundSeverity.Moderate: wound.Value = wound.GetValueFromSeverity(WoundSeverity.Light); break;
                        case WoundSeverity.Light: wound.Value = wound.GetValueFromSeverity(WoundSeverity.Scars); break;
                        default: break;
                    }
                    
                    wound.DateDay = dateReduce.Day;
                    wound.DateMonth = dateReduce.Month;
                    wound.DateYear = dateReduce.Year;
                    daysToReduce = (int)(wound.HealTime * (2.0 - HealingModyfier / 100.0));
                    dateReduce = wound.DateStart + daysToReduce;
                    if (wound.Value == 0)
                        break;
               }
            }
            wound.DateReduce = dateReduce;
            return dateReduce;
        }

        public void UpdateHealthBonusesInAttributes()
        {
            foreach (var obj in _allParams.Attributes.GetAllArray())
            {
                obj.HealthBonus = 0;
            }

            foreach (var obj in _allParams.Health.GetAll())
            {
                // add health bonus from wounds
                foreach (var attrName in HealthModel.GetAttributeNamesFromLocation(obj.Location))
                {
                    if (string.IsNullOrEmpty(attrName)==false)
                        _allParams.Attributes.Get(attrName).HealthBonus -= obj.Penalty;
                }
            }
        }
    }
}
