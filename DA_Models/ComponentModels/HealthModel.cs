using Abp.Collections.Extensions;
using DA_Common;
using DA_DataAccess.CharacterClasses;
using DA_Models.CharacterModels;
using DA_Models.ComponentInterfaces;
using Microsoft.AspNetCore.Components.Web.Virtualization;
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
        private readonly AllParamsModel? _allParams;
        private ICollection<WoundDTO> Wounds { get; set; } = new List<WoundDTO>();

        public HealthModel(AllParamsModel? allParams)
        {
            _allParams = allParams;
        }
        public virtual int MaxWounds 
        { 
            get
            {
                int? res = _allParams?.Attributes?.Get(SD.Attributes.Endurance)?.SumAbsolute + _allParams?.Attributes?.Get(SD.Attributes.Willpower)?.SumAbsolute;

                if(res is not null)
                {
                    int res2 = ((int)res + 1) / 2 + 2;
                    foreach (var w in Wounds)
                    {
                        if(w.Penalty < 0 && w.Location != SD.Condition.Cleanliness && w.Location != SD.Condition.Wellbeing)
                            res2 -= w.Penalty;
                    }
                    return res2;
                }

                return 0;
            }
        }
        public virtual int CurrentWounds
        {
            get
            {
                int res = 0;
                foreach (var w in Wounds)
                {
                    if(w.Location != SD.Condition.Cleanliness && w.Location != SD.Condition.Wellbeing && w.Penalty>0)
                        res += w.Penalty;
                }
                return res;
            }
        }
        public virtual int HealingModyfier
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

        public virtual void FillPropertiesContainer(IEnumerable<WoundDTO>? properties)
        {
            if (properties is null) return;
            Wounds = (ICollection<WoundDTO>)properties;
        }
        public virtual void AddWound(WoundDTO wound)
        {
            Wounds.Add(wound);
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
                        case WoundSeverity.Critical: wound.Value = SD.GetValueFromSeverity(WoundSeverity.Heavy); break;
                        case WoundSeverity.Heavy: wound.Value = SD.GetValueFromSeverity(WoundSeverity.Moderate); break;
                        case WoundSeverity.Moderate: wound.Value = SD.GetValueFromSeverity(WoundSeverity.Light); break;
                        case WoundSeverity.Light: wound.Value = SD.GetValueFromSeverity(WoundSeverity.Scars); break;
                        default: return wound.DateStart;
                    }
                    
                    wound.DateStart = new( dateReduce.Day, dateReduce.Month, dateReduce.Year);
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
                foreach (var attrName in obj.GetAttributeNamesFromLocation())
                {
                    if (string.IsNullOrEmpty(attrName)==false)
                        _allParams.Attributes.Get(attrName).HealthBonus -= obj.Penalty;
                }
            }
        }
    }
}
