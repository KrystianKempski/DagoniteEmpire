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
        public Relation Relation { get; set; } = Relation.Enemy;
        public string Description { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = "/upload/portraits/def-char-img.webp";
        //equipment
        public string MainWeaponName { get; set; } = string.Empty;
        public string OffWeaponName { get; set; } = string.Empty;
        public string ShieldWeaponName { get; set; } = string.Empty;
        public string ArmorName { get; set; } = string.Empty;
        public EquipmentDTO? MainWeapon { get; set; } = null;
        public EquipmentDTO? OffWeapon { get; set; } = null;
        public EquipmentDTO? ShieldWeapon { get; set; } = null;
        public EquipmentDTO? Armor { get; set; } = null;
        public CampaignDTO? Campaign { get; set; } = new();
        public ChapterDTO? Chapter { get; set; } = new();
        public int CampaignId { get; set; }
        public int ChapterId { get; set; }
        public string RaceName { get; set; } = string.Empty;
        public bool IsApproved { get; set; }
        public string ProfessionName { get; set; } = string.Empty;
        //skills
        public int AttackSkillValue { get; set; } = 10;
        public int DodgeSkillValue { get; set; } = 10;
        public int ShieldSkillValue { get; set; } = 10;
        public int ArmorSkillValue { get; set; } = 10;
        public int ParrySkillValue { get; set; } = 10;
        public int PainResSkillValue { get; set; } = 10;
        public int LiftingSkillValue { get; set; } = 10;
        public int WrestlingSkillValue { get; set; } = 10;

        // wounds
        public int MaxWounds { get; set; } = 12;
        public int CurrentWounds { get; set; } = 0;
        public string States { get; set; } = string.Empty;

        public MobBattlePropertyModel BattleProperties { get; set; }
        public override string ToString() => Name;
    }

}
