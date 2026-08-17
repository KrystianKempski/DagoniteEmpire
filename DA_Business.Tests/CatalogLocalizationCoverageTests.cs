using System.Xml.Linq;
using DA_Common;
using DA_Common.Localization;

namespace DA_Business.Tests;

/// <summary>
/// Guards that canonical catalog values which are localized at display time (via
/// <c>LocCatalog</c>/<c>Loc.T</c>) actually have Polish translations in the shared resx.
/// English is always the key (built-in fallback), so only the Polish file is checked.
/// A new attribute/skill without a translation fails here instead of leaking English into the UI.
/// </summary>
public class CatalogLocalizationCoverageTests
{
    private static HashSet<string> LoadPolishKeys()
    {
        var resx = LocateResx();
        var doc = XDocument.Load(resx);
        return doc.Root!
            .Elements("data")
            .Select(e => (string?)e.Attribute("name"))
            .Where(n => !string.IsNullOrEmpty(n))
            .Select(n => n!)
            .ToHashSet(StringComparer.Ordinal);
    }

    private static string LocateResx()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "DagoniteEmpire.sln")))
            dir = dir.Parent;

        Assert.NotNull(dir);
        var path = Path.Combine(dir!.FullName, "DagoniteEmpire", "Resources", "Localization", "SharedResources.pl.resx");
        Assert.True(File.Exists(path), $"Polish resx not found at {path}");
        return path;
    }

    public static IEnumerable<object[]> AttributeKeys() => SD.Attributes.All.Select(k => new object[] { k });
    public static IEnumerable<object[]> EquipmentTypeKeys() => SD.EquipmentType.All.Select(k => new object[] { k });
    public static IEnumerable<object[]> WeaponQualityKeys() => SD.WeaponQuality.All.Select(k => new object[] { k });
    public static IEnumerable<object[]> BasicEquipmentNameKeys() => SD.BasicEquipment.Names.Select(k => new object[] { k });
    public static IEnumerable<object[]> EquipmentDescriptionKeys() => SD.BasicEquipment.CatalogDescriptions.Select(k => new object[] { k });
    public static IEnumerable<object[]> TemporaryStateNameKeys() => States.Names.All.Select(k => new object[] { k });
    public static IEnumerable<object[]> TemporaryStateDescriptionKeys() => States.CatalogDescriptions.Select(k => new object[] { k });
    public static IEnumerable<object[]> WoundLocationKeys() => Wounds.Location.All.Select(k => new object[] { k });
    public static IEnumerable<object[]> WoundSeverityDisplayKeys() => new[]
    {
        "Scars", "Light wound", "Moderate wound", "Heavy wound", "Critical wound", "Deadly wound",
    }.Select(k => new object[] { k });
    public static IEnumerable<object[]> AttackActionKeys() => SD.AttackAction.All.Select(k => new object[] { k });
    public static IEnumerable<object[]> CalendarMonthKeys() => SD.Calendar.Months.Select(m => new object[] { m.Name });
    public static IEnumerable<object[]> CalendarWeekdayKeys() => SD.Calendar.AllWeek.Select(k => new object[] { k });
    public static IEnumerable<object[]> UnitWeaponNameKeys() =>
        DA_Common.Barony.UnitWeaponCatalog.All.Select(w => w.Name).Distinct().Select(k => new object[] { k });
    public static IEnumerable<object[]> UnitWeaponTypeKeys() =>
        DA_Common.Barony.UnitWeaponCatalog.All.Select(w => w.WeaponType).Distinct().Select(k => new object[] { k });
    public static IEnumerable<object[]> UnitArmorNameKeys() =>
        DA_Common.Barony.UnitArmorCatalog.All.Select(a => a.Name).Distinct().Select(k => new object[] { k });
    public static IEnumerable<object[]> UnitMountNameKeys() =>
        DA_Common.Barony.UnitMountCatalog.All.Select(m => m.Name).Distinct().Select(k => new object[] { k });
    public static IEnumerable<object[]> UnitArmorTierKeys() =>
        DA_Common.Barony.UnitArmorCatalog.ExcelTiers.Select(t => t.Title).Select(k => new object[] { k });
    public static IEnumerable<object[]> BaseSkillKeys() => SD.BaseSkills.All.Select(k => new object[] { k });
    public static IEnumerable<object[]> SpecialSkillKeys() => SD.SpecialSkills.All.Select(k => new object[] { k });
    public static IEnumerable<object[]> TerrainBaseTypeKeys() => DA_Common.Barony.TerrainBaseType.All.Select(k => new object[] { k });
    public static IEnumerable<object[]> ProjectStatusKeys() => DA_Common.Barony.ProjectStatus.All.Select(k => new object[] { k });
    public static IEnumerable<object[]> DifficultyLevelKeys() => DifficultyDisplay.ResxKeys.Select(k => new object[] { k });

    [Fact]
    public void CatalogKeys_AreNotShadowedByDifferentCasing()
    {
        // .resx compilation is case-insensitive: "strength" (a sentence fragment) silently
        // drops "Strength" (the attribute catalog key) from the satellite DLL.
        var keys = LoadPolishKeys().ToList();
        var byIgnoreCase = keys.ToLookup(k => k, StringComparer.OrdinalIgnoreCase);
        var catalog = SD.Attributes.All
            .Concat(SD.BaseSkills.All)
            .Concat(SD.SpecialSkills.All)
            .Concat(SD.EquipmentType.All)
            .Concat(SD.WeaponQuality.All)
            .Concat(SD.BasicEquipment.Names)
            .Concat(SD.BasicEquipment.CatalogDescriptions)
            .Concat(States.Names.All)
            .Concat(States.CatalogDescriptions)
            .Concat(Wounds.Location.All)
            .Concat(new[] { "Scars", "Light wound", "Moderate wound", "Heavy wound", "Critical wound", "Deadly wound" })
            .Concat(SD.AttackAction.All)
            .Concat(SD.Calendar.Months.Select(m => m.Name))
            .Concat(SD.Calendar.AllWeek)
            .Append("{0}, {1}. {2}, year {3}")
            .Append(SD.WeaponParametersDescr)
            .Concat(DifficultyDisplay.ResxKeys)
            .Append("Difficulty level: {0}");
        var collisions = catalog
            .Select(key => (key, group: byIgnoreCase[key].Distinct(StringComparer.Ordinal).ToList()))
            .Where(x => x.group.Count > 1 || (x.group.Count == 1 && !string.Equals(x.group[0], x.key, StringComparison.Ordinal)))
            .Select(x => $"{x.key} shadowed by: {string.Join(", ", x.group.Select(k => $"'{k}'"))}")
            .ToList();
        Assert.True(collisions.Count == 0, string.Join(Environment.NewLine, collisions));
    }

    [Theory]
    [MemberData(nameof(AttributeKeys))]
    public void EveryAttribute_HasPolishTranslation(string key)
    {
        Assert.Contains(key, LoadPolishKeys());
    }

    [Theory]
    [MemberData(nameof(BaseSkillKeys))]
    public void EveryBaseSkill_HasPolishTranslation(string key)
    {
        Assert.Contains(key, LoadPolishKeys());
    }

    [Theory]
    [MemberData(nameof(SpecialSkillKeys))]
    public void EverySpecialSkill_HasPolishTranslation(string key)
    {
        Assert.Contains(key, LoadPolishKeys());
    }

    [Theory]
    [MemberData(nameof(TerrainBaseTypeKeys))]
    public void EveryTerrainBaseType_HasPolishTranslation(string key)
    {
        Assert.Contains(key, LoadPolishKeys());
    }

    [Theory]
    [MemberData(nameof(ProjectStatusKeys))]
    public void EveryProjectStatus_HasPolishTranslation(string key)
    {
        Assert.Contains(key, LoadPolishKeys());
    }

    [Theory]
    [MemberData(nameof(EquipmentTypeKeys))]
    public void EveryEquipmentType_HasPolishTranslation(string key)
    {
        Assert.Contains(key, LoadPolishKeys());
    }

    [Theory]
    [MemberData(nameof(WeaponQualityKeys))]
    public void EveryWeaponQuality_HasPolishTranslation(string key)
    {
        Assert.Contains(key, LoadPolishKeys());
    }

    [Theory]
    [MemberData(nameof(BasicEquipmentNameKeys))]
    public void EveryBasicEquipmentName_HasPolishTranslation(string key)
    {
        Assert.Contains(key, LoadPolishKeys());
    }

    [Theory]
    [MemberData(nameof(EquipmentDescriptionKeys))]
    public void EveryEquipmentDescription_HasPolishTranslation(string key)
    {
        Assert.Contains(key, LoadPolishKeys());
    }

    [Theory]
    [MemberData(nameof(TemporaryStateNameKeys))]
    public void EveryTemporaryStateName_HasPolishTranslation(string key)
    {
        Assert.Contains(key, LoadPolishKeys());
    }

    [Theory]
    [MemberData(nameof(TemporaryStateDescriptionKeys))]
    public void EveryTemporaryStateDescription_HasPolishTranslation(string key)
    {
        Assert.Contains(key, LoadPolishKeys());
    }

    [Theory]
    [MemberData(nameof(WoundLocationKeys))]
    public void EveryWoundLocation_HasPolishTranslation(string key)
    {
        Assert.Contains(key, LoadPolishKeys());
    }

    [Theory]
    [MemberData(nameof(WoundSeverityDisplayKeys))]
    public void EveryWoundSeverityDisplayKey_HasPolishTranslation(string key)
    {
        Assert.Contains(key, LoadPolishKeys());
    }

    [Theory]
    [MemberData(nameof(AttackActionKeys))]
    public void EveryAttackAction_HasPolishTranslation(string key)
    {
        Assert.Contains(key, LoadPolishKeys());
    }

    [Theory]
    [MemberData(nameof(CalendarMonthKeys))]
    public void EveryCalendarMonth_HasPolishTranslation(string key)
    {
        Assert.Contains(key, LoadPolishKeys());
    }

    [Theory]
    [MemberData(nameof(CalendarWeekdayKeys))]
    public void EveryCalendarWeekday_HasPolishTranslation(string key)
    {
        Assert.Contains(key, LoadPolishKeys());
    }

    [Fact]
    public void WeaponParameters_HasPolishTranslation()
    {
        Assert.Contains(SD.WeaponParametersDescr, LoadPolishKeys());
    }

    [Theory]
    [MemberData(nameof(UnitWeaponNameKeys))]
    public void EveryUnitWeaponName_HasPolishTranslation(string key)
    {
        Assert.Contains(key, LoadPolishKeys());
    }

    [Theory]
    [MemberData(nameof(UnitWeaponTypeKeys))]
    public void EveryUnitWeaponType_HasPolishTranslation(string key)
    {
        Assert.Contains(key, LoadPolishKeys());
    }

    [Theory]
    [MemberData(nameof(UnitArmorNameKeys))]
    public void EveryUnitArmorName_HasPolishTranslation(string key)
    {
        Assert.Contains(key, LoadPolishKeys());
    }

    [Theory]
    [MemberData(nameof(UnitMountNameKeys))]
    public void EveryUnitMountName_HasPolishTranslation(string key)
    {
        Assert.Contains(key, LoadPolishKeys());
    }

    [Theory]
    [MemberData(nameof(UnitArmorTierKeys))]
    public void EveryUnitArmorTier_HasPolishTranslation(string key)
    {
        Assert.Contains(key, LoadPolishKeys());
    }

    [Theory]
    [MemberData(nameof(DifficultyLevelKeys))]
    public void EveryDifficultyLevel_HasPolishTranslation(string key)
    {
        Assert.Contains(key, LoadPolishKeys());
    }
}
