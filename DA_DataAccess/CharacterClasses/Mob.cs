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
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Notes { get; set; }
        public string? ImageUrl { get; set; }
        public string? MainWeaponName { get; set; } = null;
        public string? OffWeaponName { get; set; } = null;
        public string? ShieldWeaponName { get; set; } = null;
        public string? ArmorName { get; set; } = null;
        public int CampaignId { get; set; }
        public int ChapterId { get; set; }
        public string? RaceName { get; set; }
        public bool IsApproved { get; set; }
        public int ProfessionName { get; set; } = 0;
        public int CurrentDay { get; set; } = 1;
        public int CurrentMonth { get; set; } = 1;
        public int CurrentYear { get; set; } = SD.Calendar.StartYear;

        //battle properties

        public int AttackSkillValue { get; set; } = 0;
        public int DodgeSkillValue { get; set; } = 0;
        public int ShieldSkillValue { get; set; } = 0;
        public int ArmorSkillValue { get; set; } = 0;
        public int ParrySkillValue { get; set; } = 0;

        // wounds
        public int MaxWounds { get; set; } = 0;
        public int CurrentWounds { get; set; } = 0;

    }
}
