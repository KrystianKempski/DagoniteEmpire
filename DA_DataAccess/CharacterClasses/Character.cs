﻿using System;
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
        [Key] public int Id { get; set; }

        public string? NPCName { get; set; }
        public string? Description { get; set; }
        public string? Class {  get; set; }
        public string? Race { get; set; }
        public int Age { get; set; }
        public string? ImageUrl { get; set; }
        //public int UserId { get; set; }
        //[ForeignKey(nameof(UserId))]
        public ICollection<Attribute>? Attributes { get; set; }
        public ICollection<BaseSkill>? BaseSkills { get; set; }
       


    }
}
