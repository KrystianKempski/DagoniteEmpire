using DA_Common;
using DA_DataAccess.Chat;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DA_DataAccess.CharacterClasses
{
    public class Mob
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public Relation Relation { get; set; } = Relation.Enemy;
        public string Description { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public string MainWeaponName { get; set; } = string.Empty;
        public string OffWeaponName { get; set; } = string.Empty;
        public string ShieldWeaponName { get; set; } = string.Empty;
        public string ArmorName { get; set; } = string.Empty;
        public int CampaignId { get; set; }
        public int ChapterId { get; set; }
        public string? RaceName { get; set; }
        public bool IsApproved { get; set; }
        public string ProfessionName { get; set; } = string.Empty;
        //battle properties

        public int AttackSkillValue { get; set; } = 0;
        public int DodgeSkillValue { get; set; } = 0;
        public int ShieldSkillValue { get; set; } = 0;
        public int ArmorSkillValue { get; set; } = 0;
        public int ParrySkillValue { get; set; } = 0;
        public int PainResSkillValue { get; set; } = 0;
        public int LiftingSkillValue { get; set; } = 0;
        public int WrestlingSkillValue { get; set; } = 0;

        // wounds
        public int MaxWounds { get; set; } = 0;
        public int CurrentWounds { get; set; } = 0;

        public string States { get; set; } = string.Empty;




    }
}
