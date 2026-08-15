namespace DA_Common.Barony
{
    public sealed record UnitRecruitEventEffect(
        string Name,
        int DurationTurns,
        int Loyalty,
        int Stability,
        string? Description = null);

    public sealed record UnitRecruitSelection(
        string Key,
        string Name,
        int AttributeScore,
        int DefenseCost,
        int MaxBaseSkill,
        int Wage,
        int GoldCost = 0,
        UnitRecruitEventEffect? EventEffect = null,
        string? Notes = null);

    public static class UnitRecruitSelectionCatalog
    {
        public static readonly UnitRecruitSelection Volunteers = new(
            "volunteers", "Ochotnicy", AttributeScore: 2, DefenseCost: 20, MaxBaseSkill: 3, Wage: 3);

        public static readonly UnitRecruitSelection SelectedVolunteers = new(
            "selected-volunteers", "Wyselekcjonowani ochotnicy", AttributeScore: 3, DefenseCost: 50, MaxBaseSkill: 4, Wage: 5);

        public static readonly UnitRecruitSelection BestAvailable = new(
            "best-available", "Najlepsi dostępni", AttributeScore: 4, DefenseCost: 100, MaxBaseSkill: 5, Wage: 8);

        public static readonly UnitRecruitSelection Mercenaries = new(
            "mercenaries", "Najemnicy", AttributeScore: 3, DefenseCost: 0, MaxBaseSkill: 4, Wage: 10,
            GoldCost: 80,
            Notes: "Najmij za złoto zamiast Obrony.");

        public static readonly UnitRecruitSelection ForcedHire = new(
            "forced-hire", "Przymusowy zaciąg", AttributeScore: 3, DefenseCost: 0, MaxBaseSkill: 4, Wage: 5,
            GoldCost: 0,
            EventEffect: new UnitRecruitEventEffect(
                Name: "Przymusowy zaciąg",
                DurationTurns: 3,
                Loyalty: -7,
                Stability: -7,
                Description: "Wcieleni pod przymusem rekruci. Lojalność −7 i Stabilność −7 przez 3 tury."),
            Notes: "Brak kosztu Obrony/złota. Rozpoczyna wydarzenie „Przymusowy zaciąg”: Lojalność −7, Stabilność −7 przez 3 tury.");

        public static readonly IReadOnlyList<UnitRecruitSelection> All = new[]
        {
            Volunteers, SelectedVolunteers, BestAvailable, Mercenaries, ForcedHire,
        };

        public static UnitRecruitSelection? Find(string? key) =>
            All.FirstOrDefault(x => string.Equals(x.Key, key, StringComparison.OrdinalIgnoreCase));
    }

    public sealed record UnitTrainingType(
        string Key,
        string Name,
        int Pd,
        int GoldCost,
        int Turns,
        int MaxBaseSkill,
        int StartingDiscipline,
        int Wage,
        int FreeAttributePoints,
        string? Notes);

    public static class UnitTrainingTypeCatalog
    {
        public static readonly UnitTrainingType Express = new(
            "express", "Express", Pd: 6, GoldCost: 10, Turns: 0, MaxBaseSkill: 1,
            StartingDiscipline: 3, Wage: 3, FreeAttributePoints: 0, Notes: null);

        public static readonly UnitTrainingType Accelerated = new(
            "accelerated", "Accelerated", Pd: 12, GoldCost: 20, Turns: 1, MaxBaseSkill: 2,
            StartingDiscipline: 6, Wage: 4, FreeAttributePoints: 0, Notes: null);

        public static readonly UnitTrainingType Standard = new(
            "standard", "Standard", Pd: 36, GoldCost: 40, Turns: 3, MaxBaseSkill: 3,
            StartingDiscipline: 9, Wage: 5, FreeAttributePoints: 1, Notes: "+1 do dowolnego atrybutu");

        public static readonly UnitTrainingType Elite = new(
            "elite", "Elite", Pd: 80, GoldCost: 75, Turns: 5, MaxBaseSkill: 5,
            StartingDiscipline: 13, Wage: 8, FreeAttributePoints: 3, Notes: "3 punkty atrybutów do rozdania");

        public static readonly IReadOnlyList<UnitTrainingType> All = new[]
        {
            Express, Accelerated, Standard, Elite,
        };

        public static UnitTrainingType? Find(string? key) =>
            All.FirstOrDefault(x => string.Equals(x.Key, key, StringComparison.OrdinalIgnoreCase));
    }

    public sealed record UnitSkillDef(
        string Key,
        string Name,
        string? ParentKey,
        string LinkedAttr,
        bool IsBase);

    public static class UnitSkillTree
    {
        public static readonly IReadOnlyList<UnitSkillDef> All = new[]
        {
            new UnitSkillDef(UnitSkillKey.Melee, "Melee", null, UnitAttr.Build, true),
            new UnitSkillDef(UnitSkillKey.Swords, "Swords", UnitSkillKey.Melee, UnitAttr.Build, false),
            new UnitSkillDef(UnitSkillKey.HeavyWeapons, "Heavy weapons", UnitSkillKey.Melee, UnitAttr.Build, false),
            new UnitSkillDef(UnitSkillKey.Spears, "Spears & lances", UnitSkillKey.Melee, UnitAttr.Build, false),
            new UnitSkillDef(UnitSkillKey.Shields, "Shields", UnitSkillKey.Melee, UnitAttr.Build, false),
            new UnitSkillDef(UnitSkillKey.LightWeapons, "Light weapons", UnitSkillKey.Melee, UnitAttr.Agility, false),
            new UnitSkillDef(UnitSkillKey.Exotic, "Exotic", UnitSkillKey.Melee, UnitAttr.Perception, false),

            new UnitSkillDef(UnitSkillKey.Ranged, "Ranged & thrown", null, UnitAttr.Agility, true),
            new UnitSkillDef(UnitSkillKey.Bows, "Bows", UnitSkillKey.Ranged, UnitAttr.Agility, false),
            new UnitSkillDef(UnitSkillKey.Crossbows, "Crossbows", UnitSkillKey.Ranged, UnitAttr.Perception, false),
            new UnitSkillDef(UnitSkillKey.Slings, "Slings", UnitSkillKey.Ranged, UnitAttr.Agility, false),
            new UnitSkillDef(UnitSkillKey.Javelins, "Javelins", UnitSkillKey.Ranged, UnitAttr.Build, false),
            new UnitSkillDef(UnitSkillKey.Firearms, "Firearms", UnitSkillKey.Ranged, UnitAttr.Perception, false),
            new UnitSkillDef(UnitSkillKey.Grenades, "Grenades", UnitSkillKey.Ranged, UnitAttr.Agility, false),

            new UnitSkillDef(UnitSkillKey.Athletics, "Athletics", null, UnitAttr.Build, true),
            new UnitSkillDef(UnitSkillKey.Endurance, "Endurance", UnitSkillKey.Athletics, UnitAttr.Build, false),
            new UnitSkillDef(UnitSkillKey.Lifting, "Lifting", UnitSkillKey.Athletics, UnitAttr.Build, false),
            new UnitSkillDef(UnitSkillKey.ArmorSkill, "Armor", UnitSkillKey.Athletics, UnitAttr.Build, false),
            new UnitSkillDef(UnitSkillKey.Wrestling, "Wrestling", UnitSkillKey.Athletics, UnitAttr.Build, false),

            new UnitSkillDef(UnitSkillKey.AgilitySkill, "Agility", null, UnitAttr.Agility, true),
            new UnitSkillDef(UnitSkillKey.Climbing, "Climbing", UnitSkillKey.AgilitySkill, UnitAttr.Agility, false),
            new UnitSkillDef(UnitSkillKey.Dodges, "Dodges", UnitSkillKey.AgilitySkill, UnitAttr.Agility, false),
            new UnitSkillDef(UnitSkillKey.Run, "Run", UnitSkillKey.AgilitySkill, UnitAttr.Agility, false),
            new UnitSkillDef(UnitSkillKey.Stealth, "Stealth", UnitSkillKey.AgilitySkill, UnitAttr.Agility, false),

            new UnitSkillDef(UnitSkillKey.Urban, "Urban", null, UnitAttr.Will, true),
            new UnitSkillDef(UnitSkillKey.CrowdFighting, "Crowd fighting", UnitSkillKey.Urban, UnitAttr.Build, false),
            new UnitSkillDef(UnitSkillKey.CityOrientation, "City orientation", UnitSkillKey.Urban, UnitAttr.Will, false),
            new UnitSkillDef(UnitSkillKey.Fortification, "Fortification building", UnitSkillKey.Urban, UnitAttr.Build, false),
            new UnitSkillDef(UnitSkillKey.CityPatrol, "City patrol", UnitSkillKey.Urban, UnitAttr.Will, false),
            new UnitSkillDef(UnitSkillKey.TreatWounded, "Treat wounded", UnitSkillKey.Urban, UnitAttr.Will, false),

            new UnitSkillDef(UnitSkillKey.Scout, "Scout", null, UnitAttr.Perception, true),
            new UnitSkillDef(UnitSkillKey.Vigilance, "Vigilance", UnitSkillKey.Scout, UnitAttr.Perception, false),
            new UnitSkillDef(UnitSkillKey.Tracking, "Tracking", UnitSkillKey.Scout, UnitAttr.Perception, false),
            new UnitSkillDef(UnitSkillKey.Wilderness, "Wilderness orientation", UnitSkillKey.Scout, UnitAttr.Perception, false),
            new UnitSkillDef(UnitSkillKey.Traps, "Traps", UnitSkillKey.Scout, UnitAttr.Will, false),
            new UnitSkillDef(UnitSkillKey.Camouflage, "Camouflage", UnitSkillKey.Scout, UnitAttr.Will, false),

            new UnitSkillDef(UnitSkillKey.Riding, "Riding", null, UnitAttr.Agility, true),
        };

        public static UnitSkillDef? Find(string? key) =>
            All.FirstOrDefault(x => string.Equals(x.Key, key, StringComparison.OrdinalIgnoreCase));

        public static string? SkillKeyForWeaponType(string? weaponType) => weaponType?.Trim() switch
        {
            "Miecze" or "Swords" => UnitSkillKey.Swords,
            "Ciężka broń" or "Heavy weapons" => UnitSkillKey.HeavyWeapons,
            "Włócznie i kopie" or "Spears & lances" or "Spears" => UnitSkillKey.Spears,
            "Lekka broń" or "Light weapons" => UnitSkillKey.LightWeapons,
            "Egzotyczna" or "Exotic" => UnitSkillKey.Exotic,
            "Łuki" or "Bows" => UnitSkillKey.Bows,
            "Kusze" or "Crossbows" => UnitSkillKey.Crossbows,
            "Proce" or "Slings" => UnitSkillKey.Slings,
            "Oszczepy" or "Javelins" => UnitSkillKey.Javelins,
            "Broń palna" or "Firearms" => UnitSkillKey.Firearms,
            "Granaty" or "Grenades" => UnitSkillKey.Grenades,
            "Tarcze" or "Shields" => UnitSkillKey.Shields,
            _ => null,
        };
    }
}
