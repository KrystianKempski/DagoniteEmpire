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
    public class AttributesModel : ParameterModel<AttributeDTO>
    {

        public AttributesModel(AllParamsModel allParams) : base(allParams)
        {

        }

        public string RuntimeMessage { get; set; }
        public bool IsLeaveAllowed { get; set; } = true;

        public string IncrAttr(AttributeDTO obj)
        {
            var ok = false;
            if (_allParams.Character.IsApproved == false)
            {
                if (obj.BaseBonus < 18)
                {
                    if (obj.BaseBonus < 11)
                    {
                        if (_allParams.Character.AttributePoints > 0 && _allParams.Character.IsApproved == false)
                        {
                            ok = true;
                            _allParams.Character.AttributePoints--;
                        }
                    }
                    else
                    {
                        if (_allParams.Character.AttributePoints >= (obj.BaseBonus - 9))
                        {
                            _allParams.Character.AttributePoints -= obj.BaseBonus - 9;
                            ok = true;
                        }
                    }

                    if (ok)
                    {
                        obj.BaseBonus++;
                        IsLeaveAllowed = false;
                    }
                    else
                        return "Not enough attribute points";
                }
                else
                {
                    return "Maximium base bonus limit is 18";
                }
            }
            else if (_allParams.Character.IsApproved)
            {
                if (_allParams.Character.CurrentExpPoints >= obj.BaseBonus * 2)
                {
                    _allParams.Character.CurrentExpPoints -= obj.BaseBonus * 2;
                    obj.BaseBonus++;
                    IsLeaveAllowed = false;
                }
                else
                    return "Not enough experience points";
            }
            return "";
        }


        public string DecrAttr(AttributeDTO obj)
        {

            if (_allParams.Character.IsApproved == false)
            {
                if (obj.BaseBonus > 6)
                {
                    if (obj.BaseBonus < 11)
                    {
                        _allParams.Character.AttributePoints++;
                    }
                    else
                    {
                        _allParams.Character.AttributePoints += (obj.BaseBonus - 10);
                    }
                    obj.BaseBonus--;
                    IsLeaveAllowed = false;
                }
                else
                    return  "Minimum base bonus limit is 6";
            }
            else
            {
                return "To revert changes reload page";
            }
            return "";
        }


    }
}
