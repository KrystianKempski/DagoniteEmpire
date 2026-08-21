using System.Text.Json;

namespace DA_Common.Barony;

/// <summary>Commander ability branch tags (model B).</summary>
public static class CourtCommanderBranch
{
    public const string Trunk = "trunk";
    public const string Shock = "shock";
    public const string Line = "line";
    public const string Skirmish = "skirmish";
    public const string Cunning = "cunning";
}

/// <summary>A single skill minimum shared by every ability in a branch+tier.</summary>
public sealed class CourtCommanderSkillRequirement
{
    public string SkillKey { get; init; } = string.Empty;
    public int Min { get; init; }
    public bool IsMain { get; init; }
    /// <summary>For character gates: <c>base</c> or <c>special</c>. Court sheet uses <see cref="IsMain"/>.</summary>
    public string Kind { get; init; } = string.Empty;
}

/// <summary>Skill requirements shared by every ability in a branch+tier (model B, v2).</summary>
public sealed class CourtCommanderTierRequirement
{
    public string Branch { get; init; } = CourtCommanderBranch.Trunk;
    public int Tier { get; init; } = 1;
    public int CxCost { get; init; } = 1;
    public string RequirementsText { get; init; } = string.Empty;
    public IReadOnlyList<CourtCommanderSkillRequirement> Requirements { get; init; } =
        Array.Empty<CourtCommanderSkillRequirement>();
    /// <summary>PC / linked character Absolute skill floors (preferred when a character sheet is present).</summary>
    public IReadOnlyList<CourtCommanderSkillRequirement> CharacterRequirements { get; init; } =
        Array.Empty<CourtCommanderSkillRequirement>();
}

/// <summary>Design status of a commander ability.</summary>
public static class CourtCommanderAbilityStatus
{
    public const string InCode = "in-code";
    public const string Proposal = "proposal";
    public const string Draft = "draft";
}

/// <summary>
/// A single commander ability. Data-only: <see cref="Effects"/> is a design shorthand and is
/// intentionally NOT applied to unit/combat stats yet.
/// </summary>
public sealed class CourtCommanderAbility
{
    public string Key { get; init; } = string.Empty;
    public string NameEn { get; init; } = string.Empty;
    public string DescriptionEn { get; init; } = string.Empty;
    public string Branch { get; init; } = CourtCommanderBranch.Trunk;
    public int Tier { get; init; } = 1;
    public int CxCost { get; init; } = 1;
    public string Effects { get; init; } = string.Empty;
    public string Status { get; init; } = CourtCommanderAbilityStatus.Proposal;
    public string? Notes { get; init; }
}

/// <summary>
/// Passive bonuses applied to a unit from its captain's unlocked abilities.
/// Battle-only flags are recomputed when the unit enters battle.
/// </summary>
public sealed class CommanderBonusResult
{
    public int CommanderAttack { get; set; }
    public int CommanderDefense { get; set; }
    public int OtherMove { get; set; }
    public int OtherArmor { get; set; }
    public int OtherHp { get; set; }
    public int OtherDamageMelee { get; set; }
    public int OtherDamageShooting { get; set; }
    public int OtherDiscipline { get; set; }
    public int OtherInitiative { get; set; }
    /// <summary>Riveted Plate: Pierce ignored when this unit computes EffectiveArmor as defender.</summary>
    public int PierceIgnore { get; set; }

    public bool ThunderCharge { get; set; }
    public bool FlyingStart { get; set; }
    public bool ShockLance { get; set; }
    public bool Wedge { get; set; }
    public bool UnbrokenMomentum { get; set; }
    public bool BlindFury { get; set; }
    public bool Overrun { get; set; }
    public bool DrillShot { get; set; }
    public bool KillTheCaptain { get; set; }
    public bool PikeHedge { get; set; }
    public bool ReturnStroke { get; set; }
    public bool FightingWithdrawal { get; set; }
    public bool RotatingRanks { get; set; }
    public bool NcoScreen { get; set; }
    public bool MountedSuperiority { get; set; }
    public bool CounterCharge { get; set; }
    public bool LongShot { get; set; }
    public bool SnapShot { get; set; }
    public bool ExtendedRange { get; set; }
    public bool SkirmishScreen { get; set; }
    public bool Enfilade { get; set; }
    public bool HarassingFire { get; set; }
    public bool KnifeInTheDark { get; set; }
    public bool KeepFacing { get; set; }
    public bool LookAway { get; set; }
    public bool ColumnMarch { get; set; }
    public bool LooseFiles { get; set; }
    public bool Pathfinder { get; set; }
    public bool FeignedRetreat { get; set; }
    public bool AmbushMark { get; set; }
    public bool CoordinatedVolley { get; set; }
    public bool BackstabDoctrine { get; set; }
    public bool ReadTheEnemy { get; set; }
    public bool CaptainsPresence { get; set; }
    public bool Ironclad { get; set; }
    public bool NoStepBack { get; set; }

    /// <summary>Default charge Atk bonus (2), or 3 with Thunder Charge.</summary>
    public int ChargeAttackBonus => ThunderCharge ? 3 : 2;
    /// <summary>Default charge Dmg bonus (1), or 2 with Thunder Charge (+ Shock Lance when mounted).</summary>
    public int ChargeDamageBonus(bool mounted)
    {
        var dmg = ThunderCharge ? 2 : 1;
        if (ShockLance && mounted)
            dmg += 1;
        return dmg;
    }
}

/// <summary>Catalog + unlock rules for court commander abilities (model B).</summary>
public static class CourtCommanderCatalog
{
    public const int MaxTier3 = 2;
    public const int SoftCapCmdAttack = 2;
    public const int SoftCapCmdDefense = 2;
    public const int SoftCapMove = 2;

    /// <summary>
    /// Abilities temporarily disabled: not unlockable and their battle effect is neutralised
    /// (e.g. Loose Files, whose diagonal squeeze the movement planner cannot yet mirror).
    /// </summary>
    public static readonly IReadOnlySet<string> DisabledKeys =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "loose-files" };

    public static bool IsDisabled(string? key) =>
        !string.IsNullOrWhiteSpace(key) && DisabledKeys.Contains(key.Trim());

    public static int CxCostForTier(int tier) => tier switch
    {
        1 => 1,
        2 => 2,
        3 => 3,
        _ => 1,
    };

    private const string ResourceName = "DA_Common.Barony.commander-skill-tree.json";

    private static readonly TreeData Data = Load();

    public static IReadOnlyList<CourtCommanderAbility> All => Data.Abilities;

    public static IReadOnlyList<CourtCommanderTierRequirement> TierRequirements => Data.TierRequirements;

    public static CourtCommanderAbility? Find(string key) =>
        Data.Abilities.FirstOrDefault(a => string.Equals(a.Key, key, StringComparison.OrdinalIgnoreCase));

    public static IEnumerable<CourtCommanderAbility> ForBranch(string branch) =>
        Data.Abilities.Where(a => string.Equals(a.Branch, branch, StringComparison.OrdinalIgnoreCase));

    public static CourtCommanderTierRequirement? FindTierRequirement(string branch, int tier) =>
        Data.TierRequirements.FirstOrDefault(t =>
            string.Equals(t.Branch, branch, StringComparison.OrdinalIgnoreCase) && t.Tier == tier);

    private sealed class TreeData
    {
        public IReadOnlyList<CourtCommanderAbility> Abilities { get; init; } =
            Array.Empty<CourtCommanderAbility>();
        public IReadOnlyList<CourtCommanderTierRequirement> TierRequirements { get; init; } =
            Array.Empty<CourtCommanderTierRequirement>();
    }

    private static TreeData Load()
    {
        var assembly = typeof(CourtCommanderCatalog).Assembly;
        using var stream = assembly.GetManifestResourceStream(ResourceName)
            ?? throw new InvalidOperationException(
                $"Embedded commander skill-tree resource '{ResourceName}' was not found.");
        using var reader = new StreamReader(stream);
        var json = reader.ReadToEnd();

        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var file = JsonSerializer.Deserialize<TreeFileDto>(json, options)
            ?? throw new InvalidOperationException("Commander skill-tree JSON could not be parsed.");

        var tierRequirements = (file.TierRequirements ?? new List<TierRequirementDto>())
            .Select(t => new CourtCommanderTierRequirement
            {
                Branch = (t.Branch ?? CourtCommanderBranch.Trunk).Trim().ToLowerInvariant(),
                Tier = t.Tier,
                CxCost = t.CxCost > 0 ? t.CxCost : CxCostForTier(t.Tier),
                RequirementsText = t.RequirementsText ?? string.Empty,
                Requirements = (t.Requirements ?? new List<RequirementDto>())
                    .Select(MapCourtRequirement)
                    .ToList(),
                CharacterRequirements = (t.CharacterRequirements ?? new List<RequirementDto>())
                    .Select(MapCharacterRequirement)
                    .ToList(),
            })
            .ToList();

        var abilities = (file.Abilities ?? new List<AbilityDto>())
            .Where(a => !string.IsNullOrWhiteSpace(a.Key))
            .Select(a => new CourtCommanderAbility
            {
                Key = a.Key!.Trim(),
                NameEn = a.Name ?? a.Key!,
                DescriptionEn = a.Description ?? string.Empty,
                Branch = (a.Branch ?? CourtCommanderBranch.Trunk).Trim().ToLowerInvariant(),
                Tier = a.Tier,
                CxCost = a.CxCost > 0 ? a.CxCost : CxCostForTier(a.Tier),
                Effects = a.Effects ?? string.Empty,
                Status = a.Status ?? CourtCommanderAbilityStatus.Proposal,
                Notes = string.IsNullOrWhiteSpace(a.Notes) ? null : a.Notes,
            })
            .ToList();

        return new TreeData { Abilities = abilities, TierRequirements = tierRequirements };
    }

    private static CourtCommanderSkillRequirement MapCourtRequirement(RequirementDto r)
    {
        var kind = (r.Kind ?? string.Empty).Trim().ToLowerInvariant();
        return new CourtCommanderSkillRequirement
        {
            SkillKey = ResolveSkillKey(r.Skill ?? string.Empty),
            Min = r.Min,
            IsMain = kind is "main",
            Kind = kind,
        };
    }

    private static CourtCommanderSkillRequirement MapCharacterRequirement(RequirementDto r)
    {
        var kind = (r.Kind ?? "base").Trim().ToLowerInvariant();
        return new CourtCommanderSkillRequirement
        {
            SkillKey = (r.Skill ?? string.Empty).Trim(),
            Min = r.Min,
            IsMain = false,
            Kind = kind is "special" ? "special" : "base",
        };
    }

    // JSON uses display / PascalCase skill names; map them onto our stable skill keys.
    private static string ResolveSkillKey(string skill)
    {
        var name = skill.Trim();
        if (name.Length == 0)
            return name;
        if (name.Equals("Riding", StringComparison.OrdinalIgnoreCase)
            || name.Equals("AnimalHandlingRiding", StringComparison.OrdinalIgnoreCase))
            return CourtSecondarySkill.AnimalHandlingRiding;
        if (name.Equals("Armor", StringComparison.OrdinalIgnoreCase))
            return CourtSecondarySkill.Athletics;
        if (name.Equals("Perception", StringComparison.OrdinalIgnoreCase))
            return CourtSecondarySkill.Observation;

        var lower = name.ToLowerInvariant();
        if (CourtSkillCatalog.FindMain(lower) is { } main)
            return main.Key;
        if (CourtSkillCatalog.FindSecondary(lower) is { } secondary)
            return secondary.Key;

        var byName = CourtSkillCatalog.Main.Concat(CourtSkillCatalog.Secondary)
            .FirstOrDefault(i => string.Equals(i.NameEn, name, StringComparison.OrdinalIgnoreCase));
        return byName?.Key ?? lower;
    }

    private sealed class TreeFileDto
    {
        public List<AbilityDto>? Abilities { get; set; }
        public List<TierRequirementDto>? TierRequirements { get; set; }
    }

    private sealed class AbilityDto
    {
        public string? Key { get; set; }
        public string? Name { get; set; }
        public string? Branch { get; set; }
        public int Tier { get; set; }
        public int CxCost { get; set; }
        public string? Description { get; set; }
        public string? Effects { get; set; }
        public string? Status { get; set; }
        public string? Notes { get; set; }
    }

    private sealed class TierRequirementDto
    {
        public string? Branch { get; set; }
        public int Tier { get; set; }
        public int CxCost { get; set; }
        public string? RequirementsText { get; set; }
        public List<RequirementDto>? Requirements { get; set; }
        public List<RequirementDto>? CharacterRequirements { get; set; }
    }

    private sealed class RequirementDto
    {
        public string? Skill { get; set; }
        public int Min { get; set; }
        public string? Kind { get; set; }
    }
}

/// <summary>Unlock / CX helpers for court commander progress.</summary>
public static class CourtCommanderFormulas
{
    public static int SpentCx(CourtCharacterSheet sheet)
    {
        sheet.Normalize();
        var spent = 0;
        foreach (var key in sheet.UnlockedCommanderAbilities)
        {
            var ability = CourtCommanderCatalog.Find(key);
            if (ability is not null)
                spent += ability.CxCost;
        }
        return spent;
    }

    public static int AvailableCx(CourtCharacterSheet sheet) =>
        Math.Max(0, sheet.CommanderXp - SpentCx(sheet));

    public static bool MeetsSkillRequirements(CourtCharacterSheet sheet, CourtCommanderAbility ability)
    {
        var tier = CourtCommanderCatalog.FindTierRequirement(ability.Branch, ability.Tier);
        if (tier is null)
            return true;
        foreach (var req in tier.Requirements)
        {
            var value = req.IsMain ? sheet.GetMain(req.SkillKey) : sheet.GetSecondary(req.SkillKey);
            if (value < req.Min)
                return false;
        }
        return true;
    }

    /// <summary>
    /// When <paramref name="meetsCharacterSkills"/> is provided and returns a non-null result,
    /// that result is used instead of the court-sheet skill gate (PC / linked characters).
    /// </summary>
    public static bool MeetsSkillRequirements(
        CourtCharacterSheet sheet,
        CourtCommanderAbility ability,
        Func<CourtCommanderAbility, bool?>? meetsCharacterSkills)
    {
        if (meetsCharacterSkills is not null)
        {
            var characterResult = meetsCharacterSkills(ability);
            if (characterResult is not null)
                return characterResult.Value;
        }
        return MeetsSkillRequirements(sheet, ability);
    }

    public static bool MeetsBranchGates(CourtCharacterSheet sheet, CourtCommanderAbility ability)
    {
        var unlocked = sheet.UnlockedCommanderAbilities
            .Select(CourtCommanderCatalog.Find)
            .Where(a => a is not null)
            .Cast<CourtCommanderAbility>()
            .ToList();

        if (ability.Tier >= 3)
        {
            var t3Count = unlocked.Count(a => a.Tier >= 3);
            if (t3Count >= CourtCommanderCatalog.MaxTier3
                && unlocked.All(a => !string.Equals(a.Key, ability.Key, StringComparison.OrdinalIgnoreCase)))
                return false;
        }

        if (ability.Tier <= 1)
            return true;

        var pool = unlocked.Where(a =>
            string.Equals(a.Branch, CourtCommanderBranch.Trunk, StringComparison.OrdinalIgnoreCase)
            || string.Equals(a.Branch, ability.Branch, StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (ability.Tier == 2)
            return pool.Count(a => a.Tier == 1) >= 2;

        // Tier 3: ≥2 T2 from Trunk∪branch, or ≥1 in-branch T2 + ≥1 Trunk T2
        var t2Pool = pool.Where(a => a.Tier == 2).ToList();
        if (t2Pool.Count >= 2)
            return true;
        var inBranchT2 = t2Pool.Count(a =>
            string.Equals(a.Branch, ability.Branch, StringComparison.OrdinalIgnoreCase));
        var trunkT2 = t2Pool.Count(a =>
            string.Equals(a.Branch, CourtCommanderBranch.Trunk, StringComparison.OrdinalIgnoreCase));
        return inBranchT2 >= 1 && trunkT2 >= 1;
    }

    public static bool CanUnlock(CourtCharacterSheet sheet, string abilityKey, out string? reason) =>
        CanUnlock(sheet, abilityKey, out reason, meetsCharacterSkills: null);

    public static bool CanUnlock(
        CourtCharacterSheet sheet,
        string abilityKey,
        out string? reason,
        Func<CourtCommanderAbility, bool?>? meetsCharacterSkills)
    {
        reason = null;
        var ability = CourtCommanderCatalog.Find(abilityKey);
        if (ability is null)
        {
            reason = "Unknown ability.";
            return false;
        }

        if (CourtCommanderCatalog.IsDisabled(ability.Key))
        {
            reason = "Temporarily disabled.";
            return false;
        }

        if (sheet.UnlockedCommanderAbilities.Any(k =>
                string.Equals(k, ability.Key, StringComparison.OrdinalIgnoreCase)))
        {
            reason = "Already unlocked.";
            return false;
        }

        if (!MeetsSkillRequirements(sheet, ability, meetsCharacterSkills))
        {
            reason = "Skill requirements not met.";
            return false;
        }

        if (!MeetsBranchGates(sheet, ability))
        {
            reason = "Branch / tier gates not met.";
            return false;
        }

        if (AvailableCx(sheet) < ability.CxCost)
        {
            reason = $"Need {ability.CxCost} CX (have {AvailableCx(sheet)}).";
            return false;
        }

        return true;
    }

    public static bool TryUnlock(CourtCharacterSheet sheet, string abilityKey, out string? error) =>
        TryUnlock(sheet, abilityKey, out error, meetsCharacterSkills: null);

    public static bool TryUnlock(
        CourtCharacterSheet sheet,
        string abilityKey,
        out string? error,
        Func<CourtCommanderAbility, bool?>? meetsCharacterSkills)
    {
        if (!CanUnlock(sheet, abilityKey, out error, meetsCharacterSkills))
            return false;
        sheet.UnlockedCommanderAbilities.Add(CourtCommanderCatalog.Find(abilityKey)!.Key);
        sheet.Normalize();
        error = null;
        return true;
    }

    /// <summary>CX earned from a battle as captain. Command multiplies: ×(0.8 + Command/20).</summary>
    public static int ComputeBattleCx(
        CourtCharacterSheet sheet,
        int damageDealt,
        int engagedRounds,
        int kills,
        int damageTaken,
        int mgBonus = 0)
    {
        sheet.Normalize();
        var raw = Math.Max(0, damageDealt) / 5
                  + Math.Max(0, engagedRounds)
                  + 2 * Math.Max(0, kills)
                  - Math.Max(0, damageTaken) / 10;
        raw = Math.Max(0, raw + mgBonus);
        var command = sheet.GetMain(CourtMainSkill.Command);
        var mult = 0.8m + command / 20m;
        return Math.Max(0, (int)Math.Floor(raw * mult));
    }

    /// <summary>
    /// Passive stat bonuses applied to the commanded unit from unlocked abilities.
    /// Phase 1 wires the flat, always-on passives (HP / Move / melee Damage / shield Defense);
    /// context-sensitive and battle-flow abilities are computed elsewhere as they are wired.
    /// </summary>
    public static CommanderBonusResult ComputeBonuses(
        CourtCharacterSheet? sheet,
        bool unitHasMount,
        bool unitHasShield)
    {
        var result = new CommanderBonusResult();
        if (sheet is null)
            return result;
        sheet.Normalize();

        foreach (var key in sheet.UnlockedCommanderAbilities)
        {
            if (CourtCommanderCatalog.IsDisabled(key))
                continue;
            switch (key.Trim().ToLowerInvariant())
            {
                case "hold-the-line":
                    result.OtherHp += 8;
                    break;
                case "march-cadence":
                    result.OtherMove += 1;
                    break;
                case "killing-blow":
                    result.OtherDamageMelee += 1;
                    break;
                case "shield-wall-basics":
                    if (unitHasShield)
                        result.CommanderDefense += 1;
                    break;

                // Phase 2 — flat passives.
                case "discipline-boost":
                    result.OtherDiscipline += 1;
                    break;
                case "initiative-boost":
                    result.OtherInitiative += 2;
                    break;
                case "riveted-plate":
                    result.PierceIgnore += 1;
                    break;

                // Phase 2 — context-sensitive combat flags (applied during battle resolution).
                case "drill-shot":
                    result.DrillShot = true;
                    break;
                case "mounted-superiority":
                    result.MountedSuperiority = true;
                    break;
                case "counter-charge":
                    result.CounterCharge = true;
                    break;
                case "pike-hedge":
                    result.PikeHedge = true;
                    break;
                case "return-stroke":
                    result.ReturnStroke = true;
                    break;
                case "kill-the-captain":
                    result.KillTheCaptain = true;
                    break;

                // Phase 3 — charge mechanics.
                case "shock-lance":
                    result.ShockLance = true;
                    break;
                case "thunder-charge":
                    result.ThunderCharge = true;
                    break;
                case "flying-start":
                    result.FlyingStart = true;
                    break;
                case "wedge":
                    result.Wedge = true;
                    break;
                case "unbroken-momentum":
                    result.UnbrokenMomentum = true;
                    break;
                case "blind-fury":
                    result.BlindFury = true;
                    break;
                case "overrun":
                    result.Overrun = true;
                    break;

                // Phase 4 — ranged / skirmish.
                case "long-shot":
                    result.LongShot = true;
                    break;
                case "snap-shot":
                    result.SnapShot = true;
                    break;
                case "extended-range":
                    result.ExtendedRange = true;
                    break;
                case "skirmish-screen":
                    result.SkirmishScreen = true;
                    break;
                case "enfilade":
                    result.Enfilade = true;
                    break;
                case "harassing-fire":
                    result.HarassingFire = true;
                    break;

                // Tier 1 finishers — rear damage + free facing.
                case "knife-in-the-dark":
                    result.KnifeInTheDark = true;
                    break;
                case "keep-facing":
                    result.KeepFacing = true;
                    break;
                case "look-away":
                    result.LookAway = true;
                    break;
                case "column-march":
                    result.ColumnMarch = true;
                    break;
                case "loose-files":
                    result.LooseFiles = true;
                    break;
                case "ironclad":
                    result.Ironclad = true;
                    break;
            }
        }

        // Soft caps on commander-sourced stats.
        result.OtherMove = Math.Min(result.OtherMove, CourtCommanderCatalog.SoftCapMove);
        result.CommanderAttack = Math.Min(result.CommanderAttack, CourtCommanderCatalog.SoftCapCmdAttack);
        result.CommanderDefense = Math.Min(result.CommanderDefense, CourtCommanderCatalog.SoftCapCmdDefense);
        return result;
    }
}
