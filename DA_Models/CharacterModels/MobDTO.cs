using DA_Common;
using DA_DataAccess.CharacterClasses;
using DA_DataAccess.Chat;
using DA_Models.ComponentModels;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography.X509Certificates;
using Attribute = DA_DataAccess.CharacterClasses.Attribute;

namespace DA_Models.CharacterModels
{
    public class MobDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public string ImageUrl { get; set; } = "/upload/portraits/def-char-img.webp";
        //equipment
        public string? MainWeaponName { get; set; } = null;
        public string? OffWeaponName { get; set; } = null;
        public string? ShieldWeaponName { get; set; } = null;
        public string? ArmorName { get; set; } = null;
        public EquipmentSlotDTO? MainWeapon { get; set; } = null;
        public EquipmentSlotDTO? OffWeapon { get; set; } = null;
        public EquipmentSlotDTO? ShieldWeapon { get; set; } = null;
        public EquipmentSlotDTO? Armor { get; set; } = null;
        public CampaignDTO? Campaign { get; set; }
        public ChapterDTO? Chapter { get; set; }
        public int CampaignId { get; set; }
        public int ChapterId { get; set; }
        public string? RaceName { get; set; }
        public bool IsApproved { get; set; }
        public int ProfessionName { get; set; } = 0;
        //skills
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
        public string Statuses { get; set; } = string.Empty;        
        public override string ToString() => Name;

    }

}
