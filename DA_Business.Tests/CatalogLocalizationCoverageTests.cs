using System.Xml.Linq;
using DA_Common;

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
    public static IEnumerable<object[]> BaseSkillKeys() => SD.BaseSkills.All.Select(k => new object[] { k });
    public static IEnumerable<object[]> SpecialSkillKeys() => SD.SpecialSkills.All.Select(k => new object[] { k });
    public static IEnumerable<object[]> TerrainBaseTypeKeys() => DA_Common.Barony.TerrainBaseType.All.Select(k => new object[] { k });
    public static IEnumerable<object[]> ProjectStatusKeys() => DA_Common.Barony.ProjectStatus.All.Select(k => new object[] { k });

    [Fact]
    public void CatalogKeys_AreNotShadowedByDifferentCasing()
    {
        // .resx compilation is case-insensitive: "strength" (a sentence fragment) silently
        // drops "Strength" (the attribute catalog key) from the satellite DLL.
        var keys = LoadPolishKeys().ToList();
        var byIgnoreCase = keys.ToLookup(k => k, StringComparer.OrdinalIgnoreCase);
        var catalog = SD.Attributes.All
            .Concat(SD.BaseSkills.All)
            .Concat(SD.SpecialSkills.All);
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
}
