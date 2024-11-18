using DA_Common;
using DA_DataAccess.Chat;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DA_DataAccess.CharacterClasses
{
    public class Character 
    {
        [Key]
        public int Id { get; set; }
        public required string UserName { get; set; }
        public Relation Relation { get; set; } = Relation.Teammate;

        public string? NPCName { get; set; }
        public string? Description { get; set; }
        public int Age { get; set; }
        public string? ImageUrl { get; set; }

        public string? NPCType { get; set; } = SD.NPCType.Hero;
        public int AttributePoints { get; set; }
        public int CurrentExpPoints { get; set; }
        public int UsedExpPoints { get; set; }

        public int TraitBalance { get; set; }

        public ICollection<Attribute>? Attributes { get; set; }
        public ICollection<BaseSkill>? BaseSkills { get; set; }

        public ICollection<SpecialSkill>? SpecialSkills { get; set; }
        //public ICollection<TraitCharacter>? TraitsCharacter { get; set; }

        public ICollection<EquipmentSlot>? EquipmentSlots { get; set; }

        public ICollection<Campaign>? Campaigns { get; set; }

        public ICollection<Post>? Posts { get; set; }
        public ICollection<Chapter>? Chapters { get; set; }

        public ICollection<Wound>? Wounds { get; set; }

        public int? RaceId { get; set; }
        [ForeignKey(nameof(RaceId))]
        public Race? Race { get; set; } = null;

        public bool IsApproved { get; set; }

        public int ProfessionId { get; set; } = 0;
        [ForeignKey(nameof(ProfessionId))]
        public Profession? Profession { get; set; } = null;

        public int WeaponSet { get; set; } = 0;

        //public int CurrentDay { get; set; } = 1;
        //public int CurrentMonth { get; set; } = 1;
        //public int CurrentYear { get; set; } = SD.Calendar.StartYear;

        public int DateNumber { get; set; } = 1;
        public decimal Gold { get; set; } = 0.0m;
        public ICollection<WealthRecord>? WealthLog { get; set; }

    }
}
