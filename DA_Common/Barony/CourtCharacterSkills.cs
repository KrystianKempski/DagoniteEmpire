namespace DA_Common.Barony;

/// <summary>Stable keys for court / retainer main skills.</summary>
public static class CourtMainSkill
{
    public const string Melee = "melee";
    public const string Shooting = "shooting";
    public const string Magic = "magic";
    public const string Knowledge = "knowledge";
    public const string Command = "command";
    public const string Diplomacy = "diplomacy";
    public const string Intimidation = "intimidation";
    public const string Administration = "administration";
    public const string Deceit = "deceit";
    public const string Craft = "craft";
}

/// <summary>Stable keys for optional court secondary skills.</summary>
public static class CourtSecondarySkill
{
    public const string LawInvestigation = "law-investigation";
    public const string AnimalHandlingRiding = "animal-handling-riding";
    public const string Medicine = "medicine";
    public const string FarmingHusbandry = "farming-husbandry";
    public const string Architecture = "architecture";
    public const string GeologyMining = "geology-mining";
    public const string SmithingMetallurgy = "smithing-metallurgy";
    public const string EngineeringGunsmithing = "engineering-gunsmithing";
    public const string StrategyTactics = "strategy-tactics";
    public const string HeraldryEtiquette = "heraldry-etiquette";
    public const string MathematicsLogicCiphers = "mathematics-logic-ciphers";
    public const string Languages = "languages";
    public const string AlchemyNaturalSciences = "alchemy-natural-sciences";
    public const string Athletics = "athletics";
    public const string Acrobatics = "acrobatics";
    public const string TrackingSurvival = "tracking-survival";
    public const string SeafaringNavigation = "seafaring-navigation";
    public const string Observation = "observation";
    public const string Trade = "trade";
    public const string GeographyNations = "geography-nations";
    public const string FineArts = "fine-arts";
    public const string FaithRites = "faith-rites";
    public const string PerformanceActing = "performance-acting";
    public const string LogisticsManagement = "logistics-management";
}

public sealed class CourtSkillInfo
{
    public string Key { get; init; } = string.Empty;
    public string NameEn { get; init; } = string.Empty;
    public string NamePl { get; init; } = string.Empty;
    public string ShortEn { get; init; } = string.Empty;
    public string ShortPl { get; init; } = string.Empty;

    /// <summary>Nazwa zależna od bieżącej kultury UI (PL/EN, EN = fallback).</summary>
    public string Name => BaronyCulture.IsPolish && !string.IsNullOrEmpty(NamePl) ? NamePl : NameEn;

    /// <summary>Skrót zależny od bieżącej kultury UI (PL/EN, EN = fallback).</summary>
    public string Short => BaronyCulture.IsPolish && !string.IsNullOrEmpty(ShortPl) ? ShortPl : ShortEn;
}

/// <summary>Catalog + value ranges for court character sheets.</summary>
public static class CourtSkillCatalog
{
    public const int MainMin = 3;
    public const int MainMax = 10;
    public const int MagicMin = 0;
    public const int MagicMax = 10;
    public const int SecondaryMin = 0;
    public const int SecondaryMax = 6;
    public const int DefaultMain = 3;
    public const int DefaultMagic = 0;

    public static readonly IReadOnlyList<CourtSkillInfo> Main = new List<CourtSkillInfo>
    {
            new() { Key = CourtMainSkill.Melee, NameEn = "Melee", NamePl = "Walka wręcz", ShortEn = "Melee", ShortPl = "Wręcz" },
        new() { Key = CourtMainSkill.Shooting, NameEn = "Shooting", NamePl = "Strzelectwo", ShortEn = "Shoot", ShortPl = "Strzel" },
        new() { Key = CourtMainSkill.Magic, NameEn = "Magic", NamePl = "Magia", ShortEn = "Magic", ShortPl = "Magia" },
        new() { Key = CourtMainSkill.Knowledge, NameEn = "Knowledge", NamePl = "Wiedza", ShortEn = "Know", ShortPl = "Wiedza" },
        new() { Key = CourtMainSkill.Command, NameEn = "Command", NamePl = "Dowodzenie", ShortEn = "Cmd", ShortPl = "Dow" },
        new() { Key = CourtMainSkill.Diplomacy, NameEn = "Diplomacy", NamePl = "Dyplomacja", ShortEn = "Dipl", ShortPl = "Dypl" },
        new() { Key = CourtMainSkill.Intimidation, NameEn = "Intimidation", NamePl = "Zastraszanie", ShortEn = "Intim", ShortPl = "Zastr" },
        new() { Key = CourtMainSkill.Administration, NameEn = "Administration", NamePl = "Administracja", ShortEn = "Admin", ShortPl = "Admin" },
        new() { Key = CourtMainSkill.Deceit, NameEn = "Deceit", NamePl = "Podstęp", ShortEn = "Deceit", ShortPl = "Podst" },
        new() { Key = CourtMainSkill.Craft, NameEn = "Craft", NamePl = "Rzemiosło", ShortEn = "Craft", ShortPl = "Rzem" },
    };

    public static readonly IReadOnlyList<CourtSkillInfo> Secondary = new List<CourtSkillInfo>
    {
        new() { Key = CourtSecondarySkill.LawInvestigation, NameEn = "Law / investigation", NamePl = "Prawo / śledztwo" },
        new() { Key = CourtSecondarySkill.AnimalHandlingRiding, NameEn = "Animals / riding", NamePl = "Jeździectwo / zwierzęta" },
        new() { Key = CourtSecondarySkill.Medicine, NameEn = "Medicine", NamePl = "Medycyna" },
        new() { Key = CourtSecondarySkill.FarmingHusbandry, NameEn = "Farming / husbandry", NamePl = "Hodowla / uprawa" },
        new() { Key = CourtSecondarySkill.Architecture, NameEn = "Architecture", NamePl = "Architektura" },
        new() { Key = CourtSecondarySkill.GeologyMining, NameEn = "Geology / mining", NamePl = "Geologia / górnictwo" },
        new() { Key = CourtSecondarySkill.SmithingMetallurgy, NameEn = "Smithing / metallurgy", NamePl = "Kowalstwo / metalurgia" },
        new() { Key = CourtSecondarySkill.EngineeringGunsmithing, NameEn = "Engineering / gunsmithing", NamePl = "Inżynieria / rusznikarstwo" },
        new() { Key = CourtSecondarySkill.StrategyTactics, NameEn = "Strategy / tactics", NamePl = "Strategia / taktyka" },
        new() { Key = CourtSecondarySkill.HeraldryEtiquette, NameEn = "Heraldry / etiquette", NamePl = "Heraldyka / etykieta" },
        new() { Key = CourtSecondarySkill.MathematicsLogicCiphers, NameEn = "Mathematics / logic / ciphers", NamePl = "Matematyka / logika / szyfry" },
        new() { Key = CourtSecondarySkill.Languages, NameEn = "Languages", NamePl = "Języki" },
        new() { Key = CourtSecondarySkill.AlchemyNaturalSciences, NameEn = "Alchemy / natural sciences", NamePl = "Alchemia / nauki" },
        new() { Key = CourtSecondarySkill.Athletics, NameEn = "Athletics", NamePl = "Atletyka" },
        new() { Key = CourtSecondarySkill.Acrobatics, NameEn = "Acrobatics", NamePl = "Akrobatyka" },
        new() { Key = CourtSecondarySkill.TrackingSurvival, NameEn = "Tracking / survival", NamePl = "Tropienie / przetrwanie" },
        new() { Key = CourtSecondarySkill.SeafaringNavigation, NameEn = "Seafaring / navigation", NamePl = "Żeglarstwo / nawigacja" },
        new() { Key = CourtSecondarySkill.Observation, NameEn = "Observation", NamePl = "Spostrzegawczość" },
        new() { Key = CourtSecondarySkill.Trade, NameEn = "Trade", NamePl = "Handel" },
        new() { Key = CourtSecondarySkill.GeographyNations, NameEn = "Geography / nations", NamePl = "Geografia / kraje" },
        new() { Key = CourtSecondarySkill.FineArts, NameEn = "Fine arts", NamePl = "Sztuki piękne" },
        new() { Key = CourtSecondarySkill.FaithRites, NameEn = "Faith / rites", NamePl = "Wiara / obrzędy" },
        new() { Key = CourtSecondarySkill.PerformanceActing, NameEn = "Performance / acting", NamePl = "Występy / aktorstwo" },
        new() { Key = CourtSecondarySkill.LogisticsManagement, NameEn = "Logistics / management", NamePl = "Logistyka / zarządzanie" },
    };

    public static CourtSkillInfo? FindMain(string key) =>
        Main.FirstOrDefault(s => string.Equals(s.Key, key, StringComparison.OrdinalIgnoreCase));

    public static CourtSkillInfo? FindSecondary(string key) =>
        Secondary.FirstOrDefault(s => string.Equals(s.Key, key, StringComparison.OrdinalIgnoreCase));

    public static int ClampMain(string key, int value)
    {
        if (string.Equals(key, CourtMainSkill.Magic, StringComparison.OrdinalIgnoreCase))
            return Math.Clamp(value, MagicMin, MagicMax);
        return Math.Clamp(value, MainMin, MainMax);
    }

    public static int ClampSecondary(int value) => Math.Clamp(value, SecondaryMin, SecondaryMax);
}

public sealed class CourtSecondaryEntry
{
    public string Key { get; set; } = string.Empty;
    public int Value { get; set; }
}

/// <summary>Named bonus toward one main skill (shown as the Main skills "Other" row).</summary>
public sealed class CourtMainOtherSource
{
    public string Name { get; set; } = string.Empty;
    public string SkillKey { get; set; } = string.Empty;
    public int Value { get; set; }
}

/// <summary>Named bonus toward one combat total (Attack / Shooting / Dodge).</summary>
public sealed class CourtCombatOtherSource
{
    public string Name { get; set; } = string.Empty;
    public string SkillKey { get; set; } = string.Empty;
    public int Value { get; set; }
}

/// <summary>Named domain/PPB bonus (summed into the Domain Skills "Other" row).</summary>
public sealed class CourtDomainOtherSource
{
    public string Name { get; set; } = string.Empty;
    public PpbVector Additive { get; set; } = new();
}

/// <summary>Simplified court / retainer character sheet (mains always present, secondaries optional).</summary>
public sealed class CourtCharacterSheet
{
    public const string FromSkillLabel = "From skill";
    public const string OtherLabel = "Other";

    public Dictionary<string, int> Main { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public List<CourtSecondaryEntry> Secondary { get; set; } = new();
    public List<CourtMainOtherSource> MainOther { get; set; } = new();
    public List<CourtDomainOtherSource> DomainOther { get; set; } = new();
    public List<CourtCombatOtherSource> CombatOther { get; set; } = new();

    /// <summary>Lifetime commander experience (CX) for the skill tree.</summary>
    public int CommanderXp { get; set; }

    /// <summary>Unlocked commander ability keys (see <see cref="CourtCommanderCatalog"/>).</summary>
    public List<string> UnlockedCommanderAbilities { get; set; } = new();

    public static CourtCharacterSheet CreateDefault()
    {
        var sheet = new CourtCharacterSheet();
        foreach (var skill in CourtSkillCatalog.Main)
        {
            sheet.Main[skill.Key] = string.Equals(skill.Key, CourtMainSkill.Magic, StringComparison.OrdinalIgnoreCase)
                ? CourtSkillCatalog.DefaultMagic
                : CourtSkillCatalog.DefaultMain;
        }
        return sheet;
    }

    public int GetMain(string key)
    {
        if (Main.TryGetValue(key, out var value))
            return CourtSkillCatalog.ClampMain(key, value);
        return string.Equals(key, CourtMainSkill.Magic, StringComparison.OrdinalIgnoreCase)
            ? CourtSkillCatalog.DefaultMagic
            : CourtSkillCatalog.DefaultMain;
    }

    public int GetSecondary(string key)
    {
        var entry = Secondary.FirstOrDefault(s =>
            string.Equals(s.Key, key, StringComparison.OrdinalIgnoreCase));
        return entry is null ? 0 : CourtSkillCatalog.ClampSecondary(entry.Value);
    }

    public int GetMainOtherSum(string skillKey)
    {
        var sum = 0;
        foreach (var src in MainOther)
        {
            if (string.Equals(src.SkillKey, skillKey, StringComparison.OrdinalIgnoreCase))
                sum += src.Value;
        }
        return sum;
    }

    public int GetCombatOtherSum(string skillKey)
    {
        var sum = 0;
        foreach (var src in CombatOther)
        {
            if (string.Equals(src.SkillKey, skillKey, StringComparison.OrdinalIgnoreCase))
                sum += src.Value;
        }
        return sum;
    }

    public string? MainOtherTooltip(string skillKey)
    {
        var parts = MainOther
            .Where(s => string.Equals(s.SkillKey, skillKey, StringComparison.OrdinalIgnoreCase) && s.Value != 0)
            .Select(s => $"{s.Name}: {(s.Value > 0 ? "+" : "")}{s.Value}")
            .ToList();
        return parts.Count == 0 ? null : string.Join("\n", parts);
    }

    public string? CombatOtherTooltip(string skillKey)
    {
        var parts = CombatOther
            .Where(s => string.Equals(s.SkillKey, skillKey, StringComparison.OrdinalIgnoreCase) && s.Value != 0)
            .Select(s => $"{s.Name}: {(s.Value > 0 ? "+" : "")}{s.Value}")
            .ToList();
        return parts.Count == 0 ? null : string.Join("\n", parts);
    }

    public PpbVector SumDomainOther()
    {
        var sum = new PpbVector();
        sum.EnsureSize();
        foreach (var src in DomainOther)
            sum.AddInPlace(src.Additive ?? new PpbVector());
        return sum;
    }

    public string? DomainOtherTooltip(Ppb key)
    {
        var parts = new List<string>();
        foreach (var src in DomainOther)
        {
            var v = (src.Additive ?? new PpbVector())[key];
            if (v == 0m)
                continue;
            parts.Add($"{src.Name}: {PpbFormat.Additive(v)}");
        }
        return parts.Count == 0 ? null : string.Join("\n", parts);
    }

    public string? DomainOtherSourcesTooltip()
    {
        if (DomainOther.Count == 0)
            return null;
        return string.Join("\n", DomainOther.Select(s => s.Name));
    }

    public void Normalize()
    {
        var normalized = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (var skill in CourtSkillCatalog.Main)
            normalized[skill.Key] = GetMain(skill.Key);
        Main = normalized;

        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var cleaned = new List<CourtSecondaryEntry>();
        foreach (var entry in Secondary)
        {
            if (string.IsNullOrWhiteSpace(entry.Key))
                continue;
            if (CourtSkillCatalog.FindSecondary(entry.Key) is null)
                continue;
            if (!seen.Add(entry.Key))
                continue;
            cleaned.Add(new CourtSecondaryEntry
            {
                Key = entry.Key,
                Value = CourtSkillCatalog.ClampSecondary(entry.Value),
            });
        }
        Secondary = cleaned;

        MainOther = MainOther
            .Where(s => !string.IsNullOrWhiteSpace(s.Name)
                        && CourtSkillCatalog.FindMain(s.SkillKey) is not null
                        && s.Value != 0)
            .Select(s => new CourtMainOtherSource
            {
                Name = s.Name.Trim(),
                SkillKey = CourtSkillCatalog.FindMain(s.SkillKey)!.Key,
                Value = s.Value,
            })
            .ToList();

        DomainOther = DomainOther
            .Where(s => !string.IsNullOrWhiteSpace(s.Name))
            .Select(s =>
            {
                var vec = (s.Additive ?? new PpbVector()).Clone();
                vec.EnsureSize();
                vec[Ppb.Treasury] = 0m;
                return new CourtDomainOtherSource
                {
                    Name = s.Name.Trim(),
                    Additive = vec,
                };
            })
            .Where(s => HasNonZeroPpb(s.Additive))
            .ToList();

        CombatOther = CombatOther
            .Where(s => !string.IsNullOrWhiteSpace(s.Name)
                        && CourtCombatCatalog.Find(s.SkillKey) is not null
                        && s.Value != 0)
            .Select(s => new CourtCombatOtherSource
            {
                Name = s.Name.Trim(),
                SkillKey = CourtCombatCatalog.Find(s.SkillKey)!.Key,
                Value = s.Value,
            })
            .ToList();

        CommanderXp = Math.Max(0, CommanderXp);
        var seenAbilities = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var cleanedAbilities = new List<string>();
        foreach (var key in UnlockedCommanderAbilities)
        {
            if (string.IsNullOrWhiteSpace(key))
                continue;
            var ability = CourtCommanderCatalog.Find(key);
            if (ability is null || !seenAbilities.Add(ability.Key))
                continue;
            cleanedAbilities.Add(ability.Key);
        }
        UnlockedCommanderAbilities = cleanedAbilities;
    }

    private static bool HasNonZeroPpb(PpbVector v)
    {
        foreach (var info in PpbCatalog.All)
        {
            if (info.Key == Ppb.Treasury)
                continue;
            if (v[info.Key] != 0m)
                return true;
        }
        return false;
    }
}

/// <summary>
/// Court sheet → administrative PPB. Each PPB = one main + two secondaries (sum).
/// Corruption is stored as a negative contribution.
/// Domain "Other" sources are added in <see cref="ComputeTotal"/>.
/// </summary>
public static class CourtPpbFormulas
{
    /// <summary>PPB from main + secondary skills only (Domain Skills "From skill" row).</summary>
    public static PpbVector Compute(CourtCharacterSheet? sheet)
    {
        sheet ??= CourtCharacterSheet.CreateDefault();
        sheet.Normalize();

        int M(string key) => sheet.GetMain(key);
        int S(string key) => sheet.GetSecondary(key);

        var v = new PpbVector();
        v.EnsureSize();

        v[Ppb.Food] = M(CourtMainSkill.Knowledge)
                      + S(CourtSecondarySkill.FarmingHusbandry)
                      + S(CourtSecondarySkill.AnimalHandlingRiding)
                      + S(CourtSecondarySkill.AlchemyNaturalSciences);

        v[Ppb.Economy] = M(CourtMainSkill.Administration)
                         + S(CourtSecondarySkill.Trade)
                         + S(CourtSecondarySkill.MathematicsLogicCiphers)
                         + S(CourtSecondarySkill.LogisticsManagement);

        v[Ppb.Production] = M(CourtMainSkill.Craft)
                            + S(CourtSecondarySkill.EngineeringGunsmithing)
                            + S(CourtSecondarySkill.SmithingMetallurgy)
                            + S(CourtSecondarySkill.GeologyMining);

        v[Ppb.Loyalty] = M(CourtMainSkill.Diplomacy)
                         + S(CourtSecondarySkill.PerformanceActing)
                         + S(CourtSecondarySkill.FaithRites)
                         + S(CourtSecondarySkill.Observation);

        v[Ppb.Stability] = M(CourtMainSkill.Administration)
                           + S(CourtSecondarySkill.LawInvestigation)
                           + S(CourtSecondarySkill.HeraldryEtiquette)
                           + S(CourtSecondarySkill.Medicine);

        v[Ppb.Law] = M(CourtMainSkill.Intimidation)
                     + S(CourtSecondarySkill.LawInvestigation)
                     + S(CourtSecondarySkill.Observation)
                     + S(CourtSecondarySkill.TrackingSurvival);

        v[Ppb.Corruption] = -((M(CourtMainSkill.Deceit)
                               + S(CourtSecondarySkill.HeraldryEtiquette)
                               + S(CourtSecondarySkill.PerformanceActing)
                               + S(CourtSecondarySkill.Athletics)) / 3);

        v[Ppb.Science] = M(CourtMainSkill.Knowledge)
                          + S(CourtSecondarySkill.MathematicsLogicCiphers)
                          + S(CourtSecondarySkill.AlchemyNaturalSciences)
                          + S(CourtSecondarySkill.GeographyNations);

        v[Ppb.Magic] = M(CourtMainSkill.Magic)
                       + S(CourtSecondarySkill.AlchemyNaturalSciences)
                       + S(CourtSecondarySkill.FaithRites)
                       + S(CourtSecondarySkill.Languages);

        v[Ppb.Culture] = M(CourtMainSkill.Diplomacy)
                          + S(CourtSecondarySkill.FineArts)
                          + S(CourtSecondarySkill.Languages)
                          + S(CourtSecondarySkill.HeraldryEtiquette);

        v[Ppb.Intelligence] = M(CourtMainSkill.Deceit)
                              + S(CourtSecondarySkill.Observation)
                              + S(CourtSecondarySkill.TrackingSurvival)
                              + S(CourtSecondarySkill.Acrobatics);

        v[Ppb.Defense] = M(CourtMainSkill.Command)
                          + S(CourtSecondarySkill.StrategyTactics)
                          + S(CourtSecondarySkill.LogisticsManagement)
                          + S(CourtSecondarySkill.SmithingMetallurgy);

        v[Ppb.Treasury] = 0m;
        return v;
    }

    /// <summary>From-skill PPB plus Domain Other sources (stored on AvailableAdvisor.Skills).</summary>
    public static PpbVector ComputeTotal(CourtCharacterSheet? sheet)
    {
        sheet ??= CourtCharacterSheet.CreateDefault();
        sheet.Normalize();
        var total = Compute(sheet);
        total.AddInPlace(sheet.SumDomainOther());
        return total;
    }

    /// <summary>Human-readable formula for what drives each PPB from court skills.</summary>
    public static string? FormulaLabel(Ppb key) => key switch
    {
        Ppb.Food        => "Knowledge + Farming/husbandry + Animals/riding + Alchemy",
        Ppb.Economy     => "Administration + Trade + Mathematics + Logistics/management",
        Ppb.Production  => "Craft + Engineering + Smithing/metallurgy + Geology/mining",
        Ppb.Loyalty     => "Diplomacy + Performance/acting + Faith/rites + Observation",
        Ppb.Stability   => "Administration + Law/investigation + Heraldry/etiquette + Medicine",
        Ppb.Law         => "Intimidation + Law/investigation + Observation + Tracking/survival",
        Ppb.Corruption  => "−(Deceit + Heraldry/etiquette + Performance/acting + Athletics) / 3",
        Ppb.Science     => "Knowledge + Mathematics + Alchemy + Geography/nations",
        Ppb.Magic       => "Magic + Alchemy + Faith/rites + Languages",
        Ppb.Culture     => "Diplomacy + Fine arts + Languages + Heraldry/etiquette",
        Ppb.Intelligence => "Deceit + Observation + Tracking/survival + Acrobatics",
        Ppb.Defense     => "Command + Strategy/tactics + Logistics/management + Smithing/metallurgy",
        _               => null,
    };
}

/// <summary>Stable keys for derived court combat totals.</summary>
public static class CourtCombatSkill
{
    public const string Attack = "attack";
    public const string Shooting = "shooting";
    public const string Dodge = "dodge";
    public const string Defence = "defence";
}

/// <summary>Catalog of combat totals shown on the court sheet.</summary>
public static class CourtCombatCatalog
{
    public static readonly IReadOnlyList<CourtSkillInfo> All = new List<CourtSkillInfo>
    {
        new() { Key = CourtCombatSkill.Attack, NameEn = "Attack", NamePl = "Atak", ShortEn = "Attack" },
        new() { Key = CourtCombatSkill.Shooting, NameEn = "Shooting", NamePl = "Strzelanie", ShortEn = "Shoot" },
        new() { Key = CourtCombatSkill.Dodge, NameEn = "Dodge", NamePl = "Unik", ShortEn = "Dodge" },
        new() { Key = CourtCombatSkill.Defence, NameEn = "Defence", NamePl = "Obrona", ShortEn = "Defence" },
    };

    public static CourtSkillInfo? Find(string key) =>
        All.FirstOrDefault(s => string.Equals(s.Key, key, StringComparison.OrdinalIgnoreCase));
}

/// <summary>Derived combat totals from a court character sheet (From skill row).</summary>
public sealed class CourtCombatSkills
{
    public int Attack { get; init; }
    public int Shooting { get; init; }
    /// <summary>Dodge: max(Melee, Shooting) + Acrobatics.</summary>
    public int Dodge { get; init; }
    /// <summary>Defence: Melee + Athletics.</summary>
    public int Defence { get; init; }

    public string AttackFormula { get; init; } = string.Empty;
    public string ShootingFormula { get; init; } = string.Empty;
    public string DodgeFormula { get; init; } = string.Empty;
    public string DefenceFormula { get; init; } = string.Empty;

    public int Get(string key) => key switch
    {
        _ when string.Equals(key, CourtCombatSkill.Attack, StringComparison.OrdinalIgnoreCase) => Attack,
        _ when string.Equals(key, CourtCombatSkill.Shooting, StringComparison.OrdinalIgnoreCase) => Shooting,
        _ when string.Equals(key, CourtCombatSkill.Dodge, StringComparison.OrdinalIgnoreCase) => Dodge,
        _ when string.Equals(key, CourtCombatSkill.Defence, StringComparison.OrdinalIgnoreCase) => Defence,
        _ => 0,
    };

    public string? Formula(string key) => key switch
    {
        _ when string.Equals(key, CourtCombatSkill.Attack, StringComparison.OrdinalIgnoreCase) => AttackFormula,
        _ when string.Equals(key, CourtCombatSkill.Shooting, StringComparison.OrdinalIgnoreCase) => ShootingFormula,
        _ when string.Equals(key, CourtCombatSkill.Dodge, StringComparison.OrdinalIgnoreCase) => DodgeFormula,
        _ when string.Equals(key, CourtCombatSkill.Defence, StringComparison.OrdinalIgnoreCase) => DefenceFormula,
        _ => null,
    };
}

/// <summary>
/// Court sheet → combat totals (From skill).
/// Attack = Melee + Athletics;
/// Shooting = Shooting + Observation;
/// Dodge = max(Melee, Shooting) + Acrobatics;
/// Defence = Melee + Athletics.
/// Main Other bonuses are included in Melee / Shooting.
/// Combat Other is a separate row on the sheet.
/// </summary>
public static class CourtCombatFormulas
{
    public static CourtCombatSkills Compute(CourtCharacterSheet? sheet)
    {
        sheet ??= CourtCharacterSheet.CreateDefault();
        sheet.Normalize();

        var melee = sheet.GetMain(CourtMainSkill.Melee) + sheet.GetMainOtherSum(CourtMainSkill.Melee);
        var shooting = sheet.GetMain(CourtMainSkill.Shooting) + sheet.GetMainOtherSum(CourtMainSkill.Shooting);
        var athletics = sheet.GetSecondary(CourtSecondarySkill.Athletics);
        var acrobatics = sheet.GetSecondary(CourtSecondarySkill.Acrobatics);
        var observation = sheet.GetSecondary(CourtSecondarySkill.Observation);

        var weapon = Math.Max(melee, shooting);
        var weaponLabel = melee >= shooting ? "Melee" : "Shooting";

        return new CourtCombatSkills
        {
            Attack = melee + athletics,
            Shooting = shooting + observation,
            Dodge = weapon + acrobatics,
            Defence = melee + athletics,
            AttackFormula = $"Melee {melee} + Athletics {athletics}",
            ShootingFormula = $"Shooting {shooting} + Observation {observation}",
            DodgeFormula = $"{weaponLabel} {weapon} + Acrobatics {acrobatics}",
            DefenceFormula = $"Melee {melee} + Athletics {athletics}",
        };
    }
}
