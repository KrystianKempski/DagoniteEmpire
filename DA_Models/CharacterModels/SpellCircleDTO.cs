using AutoMapper;
using DA_Common;
using DA_Models.CharacterModels;
using DagoniteEmpire.Exceptions;
using Microsoft.AspNetCore.Rewrite;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DA_DataAccess.CharacterClasses
{
    public class SpellCircleDTO
    {
        [Key] public int Id { get; set; }
        public int Level { get; set; }
        public int KnownSpells { get; set; } = 0;
        public int PerDay { get; set; } = 0;
        public ICollection<SpellSlot>? SpellSlots { get; set; } = new HashSet<SpellSlot>();
        public int ProfessionId { get; set; }
        public ProfessionDTO? Profession { get; set; }


        public void CalculateSpells()
        {
            if (Profession == null || Profession.CasterType == SpellcasterType.None || Profession.RelatedAttribute is null)
                throw new WarningException("Error calculating spells. Maybe choose spellcaster type"); ;

            KnownSpells = SD.SpellsKnown[(int)Profession.CasterType, Profession.ClassLevel, Level];
            if(Profession.RelatedAttribute.ModifierAbsolute>0)
                PerDay = SD.SpellsPerDay[(int)Profession.CasterType, Profession.ClassLevel, Level] +
                             SD.AbilityModifBonusSpell[Profession.ClassLevel, Profession.RelatedAttribute.ModifierAbsolute];

            if(SpellSlots is  null)
            {
                SpellSlots = new List<SpellSlot>();
            }
            
            if(SpellSlots.Count < KnownSpells)
            {
                for (int i = SpellSlots.Count; i < KnownSpells; i++)
                {
                    SpellCircle circle = new();
                    circle.Level = Level;
                    SpellSlots.Add(new SpellSlot(circle));
                }
            }
            else if(SpellSlots.Count > KnownSpells)
            {
                int i = SpellSlots.Count;
                while (i > KnownSpells)
                {
                    SpellSlots.Remove(SpellSlots.ElementAt(i-1));
                }  
            }
        }
    }
}
