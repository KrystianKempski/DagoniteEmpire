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

        public string? NPCName { get; set; }
        public string? Description { get; set; }
        public int Age { get; set; }
        public string? ImageUrl { get; set; }

        public string? NPCType { get; set; }
        public int AttributePoints { get; set; }
        public int CurrentExpPoints { get; set; }
        public int UsedExpPoints { get; set; }

        public int TraitBalance { get; set; }

        public ICollection<Attribute>? Attributes { get; set; }
        public ICollection<BaseSkill>? BaseSkills { get; set; }

        public ICollection<SpecialSkill>? SpecialSkills { get; set; }
        public ICollection<TraitAdv>? TraitsAdv { get; set; }

        public ICollection<EquipmentSlot>? EquipmentSlots { get; set; }

        public ICollection<Campaign>? Campaigns { get; set; }

        public ICollection<Post>? Posts { get; set; }
        public ICollection<Chapter>? Chapters { get; set; }

        public int? RaceId { get; set; }
        [ForeignKey(nameof(RaceId))]
        public Race? Race { get; set; } = null;

        public bool IsApproved { get; set; }

        public int ProfessionId { get; set; } = 0;
        [ForeignKey(nameof(ProfessionId))]
        public Profession? Profession { get; set; } = null;

        ////used equipment
        //public int WeaponId { get; set; } = 0;
        //[ForeignKey(nameof(WeaponId))]
        //public Equipment? Weapon { get; set; } = null;
        //public int FaceId { get; set; } = 0;
        //[ForeignKey(nameof(FaceId))]
        //public Equipment? Face { get; set; } = null;
        //public int ThroatId { get; set; } = 0;
        //[ForeignKey(nameof(ThroatId))]
        //public Equipment? Throat { get; set; } = null;
        //public int BodyId { get; set; } = 0;
        //[ForeignKey(nameof(BodyId))]
        //public Equipment? Body { get; set; } = null;
        //public int HandsId { get; set; } = 0;
        //[ForeignKey(nameof(HandsId))]
        //public Equipment? Hands { get; set; } = null;
        //public int WaistId { get; set; } = 0;
        //[ForeignKey(nameof(WaistId))]
        //public Equipment? Waist { get; set; } = null;
        //public int FeetId { get; set; } = 0;
        //[ForeignKey(nameof(FeetId))]
        //public Equipment? Feet { get; set; } = null;
        //public int HeadId { get; set; } = 0;
        //[ForeignKey(nameof(HeadId))]
        //public Equipment? Head { get; set; } = null;
        //public int ShouldersId { get; set; } = 0;
        //[ForeignKey(nameof(ShouldersId))]
        //public Equipment? Shoulders { get; set; } = null;
        //public int TorsoId { get; set; } = 0;
        //[ForeignKey(nameof(TorsoId))]
        //public Equipment? Torso { get; set; } = null;
        //public int ArmsId { get; set; } = 0;
        //[ForeignKey(nameof(ArmsId))]
        //public Equipment? Arms { get; set; } = null;
        //public int Ring1Id { get; set; } = 0;
        //[ForeignKey(nameof(Ring1Id))]
        //public Equipment? Ring1 { get; set; } = null;
        //public int Ring2Id { get; set; } = 0;
        //[ForeignKey(nameof(Ring2Id))]
        //public Equipment? Ring2 { get; set; } = null;

    }
}
