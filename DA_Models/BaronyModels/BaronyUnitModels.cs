namespace DA_Models.BaronyModels
{
    public class BaronyUnitSkillLevels
    {
        public Dictionary<string, int> Base { get; set; } = new(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, int> Other { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    }

    public class BaronyUnitDTO
    {
        public int Id { get; set; }
        public int BaronyId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Status { get; set; } = DA_Common.Barony.UnitStatus.Training;
        public int TroopCount { get; set; } = DA_Common.Barony.UnitRules.DefaultTroopCount;

        public string RecruitSelectionKey { get; set; } = string.Empty;
        public string TrainingTypeKey { get; set; } = string.Empty;

        /// <summary>Unit race (<see cref="DA_Common.Barony.UnitRaceKey"/>). Only Human for now.</summary>
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

        /// <summary>Named Other sources per attribute key (<see cref="DA_Common.Barony.UnitAttr"/>); sum synced into AttrOther*.</summary>
        public Dictionary<string, List<DA_Common.Barony.UnitCombatModifierEntry>> AttrOtherSources { get; set; } =
            new(StringComparer.OrdinalIgnoreCase);

        public Dictionary<string, int> SkillBase { get; set; } = new(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, int> SkillOther { get; set; } = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>Named Other sources per combat stat key (<see cref="DA_Common.Barony.UnitCombatStatKey"/>).</summary>
        public Dictionary<string, List<DA_Common.Barony.UnitCombatModifierEntry>> CombatOther { get; set; } =
            new(StringComparer.OrdinalIgnoreCase);

        /// <summary>Named Other sources per skill key (sum synced into <see cref="SkillOther"/>).</summary>
        public Dictionary<string, List<DA_Common.Barony.UnitCombatModifierEntry>> SkillOtherSources { get; set; } =
            new(StringComparer.OrdinalIgnoreCase);

        public string? Weapon1Key { get; set; }
        public string? Weapon2Key { get; set; }
        public string? ArmorKey { get; set; }
        public string? ShieldKey { get; set; }
        public string Weapon1Quality { get; set; } = DA_Common.Barony.UnitWeaponQuality.Normal;
        public string Weapon2Quality { get; set; } = DA_Common.Barony.UnitWeaponQuality.Normal;

        public string DefenseSkillKey { get; set; } = DA_Common.Barony.UnitSkillKey.Dodges;

        public int CommanderAttack { get; set; }
        public int CommanderDefense { get; set; }
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

        public int? TrainingProjectId { get; set; }
        public int? TrainingTurnsRemaining { get; set; }
        /// <summary>OutputKind of the open unit-linked project (training / reinforce), if any.</summary>
        public string? OpenProjectOutputKind { get; set; }
        /// <summary>Status of the open unit-linked project, if any.</summary>
        public string? OpenProjectStatus { get; set; }

        public int EffectiveBuild => Build + AttrPenaltyBuild + AttrOtherBuild;
        public int EffectiveAgility => Agility + AttrPenaltyAgility + AttrOtherAgility;
        public int EffectiveWill => Will + AttrOtherWill;
        public int EffectivePerception => Perception + AttrOtherPerception;

        public bool IsTraining =>
            string.Equals(Status, DA_Common.Barony.UnitStatus.Training, StringComparison.OrdinalIgnoreCase);
        public bool IsActive =>
            string.Equals(Status, DA_Common.Barony.UnitStatus.Active, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>Payload to create a training unit + Unit Training project.</summary>
    public class StartUnitTrainingRequest
    {
        public int BaronyId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string RecruitSelectionKey { get; set; } = string.Empty;
        public string TrainingTypeKey { get; set; } = string.Empty;
        public string RaceKey { get; set; } = DA_Common.Barony.UnitRaceKey.Human;
        public string? Weapon1Key { get; set; }
        public string? Weapon2Key { get; set; }
        public string? ArmorKey { get; set; }
        public string? ShieldKey { get; set; }
        public string Weapon1Quality { get; set; } = DA_Common.Barony.UnitWeaponQuality.Normal;
        public string Weapon1AcquireMode { get; set; } = DA_Common.Barony.UnitEquipmentAcquireMode.Craft;
        public string Weapon2AcquireMode { get; set; } = DA_Common.Barony.UnitEquipmentAcquireMode.Craft;
        public string ArmorAcquireMode { get; set; } = DA_Common.Barony.UnitEquipmentAcquireMode.Craft;
        public string ShieldAcquireMode { get; set; } = DA_Common.Barony.UnitEquipmentAcquireMode.Craft;
        public int AccelerateTurns { get; set; }
        public string DefenseSkillKey { get; set; } = DA_Common.Barony.UnitSkillKey.Dodges;
        public Dictionary<string, int> SkillBase { get; set; } = new(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, int> SkillOther { get; set; } = new(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, List<DA_Common.Barony.UnitCombatModifierEntry>> CombatOther { get; set; } =
            new(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, List<DA_Common.Barony.UnitCombatModifierEntry>> SkillOtherSources { get; set; } =
            new(StringComparer.OrdinalIgnoreCase);

        /// <summary>Optional pre-spend from the generator card (attrs / discipline / XP).</summary>
        public int? Build { get; set; }
        public int? Agility { get; set; }
        public int? Will { get; set; }
        public int? Perception { get; set; }
        public int? Discipline { get; set; }
        public int? RemainingPd { get; set; }
        public int? FreeAttributePoints { get; set; }
        public Dictionary<string, List<DA_Common.Barony.UnitCombatModifierEntry>> AttrOtherSources { get; set; } =
            new(StringComparer.OrdinalIgnoreCase);
    }

    public class StartUnitTrainingResult
    {
        public BaronyUnitDTO Unit { get; set; } = new();
        public BaronyProjectDTO Project { get; set; } = new();
    }

    /// <summary>Payload to create a Unit Reinforce project for an understrength Active unit.</summary>
    public class StartUnitReinforceRequest
    {
        public int BaronyId { get; set; }
        public int UnitId { get; set; }
        /// <summary>Troops to add (1 … missing). Defaults to all missing when ≤ 0.</summary>
        public int TroopCount { get; set; }
        public string Weapon1AcquireMode { get; set; } = DA_Common.Barony.UnitEquipmentAcquireMode.Craft;
        public string Weapon2AcquireMode { get; set; } = DA_Common.Barony.UnitEquipmentAcquireMode.Craft;
        public string ArmorAcquireMode { get; set; } = DA_Common.Barony.UnitEquipmentAcquireMode.Craft;
        public string ShieldAcquireMode { get; set; } = DA_Common.Barony.UnitEquipmentAcquireMode.Craft;
    }

    public class StartUnitReinforceResult
    {
        public BaronyUnitDTO Unit { get; set; } = new();
        public BaronyProjectDTO Project { get; set; } = new();
    }
}
