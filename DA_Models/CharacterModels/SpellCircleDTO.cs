using DA_Common;
using DA_Models.CharacterModels;
using DagoniteEmpire.Exceptions;
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
            if(Profession.RelatedAttribute.Modifier>0)
                PerDay = SD.SpellsPerDay[(int)Profession.CasterType, Profession.ClassLevel, Level] +
                             SD.AbilityModifBonusSpell[Profession.ClassLevel, Profession.RelatedAttribute.Modifier];
            
        }
    }
}
