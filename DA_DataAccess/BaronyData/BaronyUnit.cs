using System.ComponentModel.DataAnnotations;

namespace DA_DataAccess.BaronyData
{
    /// <summary>Trained or in-training army unit (50 troops by default).</summary>
    public class BaronyUnit
    {
        [Key]
        public int Id { get; set; }
        public int BaronyId { get; set; }

        public string Name { get; set; } = string.Empty;
        public string Status { get; set; } = DA_Common.Barony.UnitStatus.Training;
        public int TroopCount { get; set; } = DA_Common.Barony.UnitRules.DefaultTroopCount;
        /// <summary>Nominal full strength (default 50). MG may raise or lower per unit.</summary>
        public int MaxTroopCount { get; set; } = DA_Common.Barony.UnitRules.DefaultTroopCount;

        public string RecruitSelectionKey { get; set; } = string.Empty;
        public string TrainingTypeKey { get; set; } = string.Empty;

        /// <summary>Unit race key (<see cref="DA_Common.Barony.UnitRaceKey"/>). Default Human.</summary>
        public string RaceKey { get; set; } = DA_Common.Barony.UnitRaceKey.Human;

        public int Wage { get; set; }
        public decimal UpkeepFood { get; set; } = DA_Common.Barony.UnitRules.DefaultUpkeepFood;
        public int UpkeepDefense { get; set; } = DA_Common.Barony.UnitRules.DefaultUpkeepDefense;

        public int Build { get; set; }
        public int Agility { get; set; }
        public int Will { get; set; }
        public int Perception { get; set; }
        public int AttrPenaltyBuild { get; set; }
        public int AttrPenaltyAgility { get; set; }
        public int AttrOtherBuild { get; set; }
        public int AttrOtherAgility { get; set; }
        public int AttrOtherWill { get; set; }
        public int AttrOtherPerception { get; set; }

        /// <summary>JSON map skillKey → base level (int).</summary>
        public string SkillsJson { get; set; } = "{}";

        /// <summary>JSON map skillKey → other bonus (int).</summary>
        public string SkillOtherJson { get; set; } = "{}";

        /// <summary>JSON map combatStatKey → list of {label,value} Other sources.</summary>
        public string CombatOtherJson { get; set; } = "{}";

        /// <summary>JSON map skillKey → list of {label,value} Other sources.</summary>
        public string SkillOtherSourcesJson { get; set; } = "{}";

        /// <summary>JSON map attrKey → list of {label,value} Other sources.</summary>
        public string AttrOtherSourcesJson { get; set; } = "{}";

        public string? Weapon1Key { get; set; }
        public string? Weapon2Key { get; set; }
        public string? ArmorKey { get; set; }
        public string? ShieldKey { get; set; }
        public string? MountKey { get; set; }
        public string Weapon1Quality { get; set; } = DA_Common.Barony.UnitWeaponQuality.Normal;
        public string Weapon2Quality { get; set; } = DA_Common.Barony.UnitWeaponQuality.Normal;

        public string DefenseSkillKey { get; set; } = DA_Common.Barony.UnitSkillKey.Dodges;

        public int CommanderAttack { get; set; }
        public int CommanderDefense { get; set; }

        /// <summary>Optional court person assigned as this unit's captain (1:1 per barony).</summary>
        public int? CaptainAvailableAdvisorId { get; set; }

        public int OtherAttack { get; set; }
        public int OtherDefense { get; set; }
        public int OtherDamage { get; set; }
        public int OtherMove { get; set; }
        public int OtherArmor { get; set; }
        public int OtherHp { get; set; }

        public int RemainingPd { get; set; }
        public int Discipline { get; set; } = 1;
        public int MaxBaseSkillAtGraduation { get; set; }
        public int FreeAttributePoints { get; set; }

        public int CurrentHp { get; set; }
        public string LogJson { get; set; } = "[]";

        public DateTime CreatedAtUtc { get; set; }
        public DateTime UpdatedAtUtc { get; set; }
    }
}
