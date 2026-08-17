using DA_Common;
using DA_Common.Localization;

namespace DA_Business.Tests;

public class LocCatalogSkillAliasTests
{
    [Theory]
    [InlineData("Melee", SD.BaseSkills.Melee)]
    [InlineData("Walka wręcz", SD.BaseSkills.Melee)]
    [InlineData("walka wrecz", SD.BaseSkills.Melee)]
    [InlineData("Sleight of hands", SD.BaseSkills.SleightOfHands)]
    [InlineData("Zręczność rąk", SD.BaseSkills.SleightOfHands)]
    [InlineData("Zwinne dłonie", SD.BaseSkills.SleightOfHands)]
    [InlineData("Animal handle", SD.BaseSkills.AnimalHandle)]
    [InlineData("Obchodzenie się ze zwierzętami", SD.BaseSkills.AnimalHandle)]
    [InlineData("Spostrzegawczość", SD.BaseSkills.Perception)]
    [InlineData("Strzelectwo", SD.BaseSkills.Shooting)]
    public void CanonicalKey_MapsEnglishAndPolishBaseSkillAliases(string stored, string expected)
    {
        Assert.Equal(expected, LocCatalog.CanonicalKey(stored));
        Assert.Equal(expected, SD.BaseSkills.Canonical(stored));
    }

    [Theory]
    [InlineData("Heavy weapons", SD.SpecialSkills.Melee.Heavy)]
    [InlineData("Broń ciężka", SD.SpecialSkills.Melee.Heavy)]
    [InlineData("Ciężka broń", SD.SpecialSkills.Melee.Heavy)]
    [InlineData("Swords and sabres", SD.SpecialSkills.Melee.Swords)]
    [InlineData("Miecze i szable", SD.SpecialSkills.Melee.Swords)]
    [InlineData("Pain Resistance", SD.SpecialSkills.Athletics.PainResistance)]
    [InlineData("Odporność na ból", SD.SpecialSkills.Athletics.PainResistance)]
    [InlineData("odpornosc na bol", SD.SpecialSkills.Athletics.PainResistance)]
    [InlineData("Firearms", SD.SpecialSkills.Shooting.Firearms)]
    [InlineData("Broń palna", SD.SpecialSkills.Shooting.Firearms)]
    [InlineData("Linguistics", SD.SpecialSkills.Knowledge.Linguistics)]
    [InlineData("Lingwistyka", SD.SpecialSkills.Knowledge.Linguistics)]
    [InlineData("Fine arts", SD.SpecialSkills.Craft.FineArts)]
    [InlineData("Sztuki piękne", SD.SpecialSkills.Craft.FineArts)]
    public void CanonicalKey_MapsEnglishAndPolishSpecialSkillAliases(string stored, string expected)
    {
        Assert.Equal(expected, LocCatalog.CanonicalKey(stored));
        Assert.Equal(expected, SD.SpecialSkills.Canonical(stored));
    }

    [Fact]
    public void Name_UsesCanonicalEnglishKey()
    {
        Assert.Equal(SD.BaseSkills.Melee, LocCatalog.Name("Walka wręcz"));
        Assert.Equal(SD.SpecialSkills.Melee.Swords, LocCatalog.Name("Miecze i szable"));
        Assert.Equal(SD.SpecialSkills.Athletics.PainResistance, LocCatalog.Name("Odporność na ból"));
    }

    [Fact]
    public void CharacterSeeder_UsesEnglishBaseSkillKeys()
    {
        var seeded = DA_Models.CharacterSeeder.GetBaseSkills().ToList();
        Assert.Equal(SD.BaseSkills.All.Length, seeded.Count);
        foreach (var key in SD.BaseSkills.All)
        {
            var skill = seeded.Single(s => s.Name == key);
            Assert.Equal(key, skill.Name);
            Assert.Contains(skill.RelatedAttribute1, SD.Attributes.All);
            Assert.Contains(skill.RelatedAttribute2, SD.Attributes.All);
        }
    }

    [Fact]
    public void CharacterSeeder_UsesEnglishSpecialSkillKeys()
    {
        var seeded = DA_Models.CharacterSeeder.GetSpecialSkills().ToList();
        var seededNames = seeded.Select(s => s.Name).ToHashSet(StringComparer.Ordinal);
        Assert.Equal(SD.SpecialSkills.All.Length, seeded.Count);
        foreach (var key in SD.SpecialSkills.All)
            Assert.Contains(key, seededNames);

        foreach (var skill in seeded)
        {
            Assert.Equal(skill.Name, SD.SpecialSkills.Canonical(skill.Name));
            Assert.Contains(skill.Name, SD.SpecialSkills.All);
            Assert.Contains(skill.RelatedBaseSkillName, SD.BaseSkills.All);
            if (!string.IsNullOrEmpty(skill.RelatedAttribute1))
                Assert.Contains(skill.RelatedAttribute1, SD.Attributes.All);
            if (!string.IsNullOrEmpty(skill.RelatedAttribute2))
                Assert.Contains(skill.RelatedAttribute2, SD.Attributes.All);
            if (!string.IsNullOrEmpty(skill.ChosenAttribute))
                Assert.Contains(skill.ChosenAttribute, SD.Attributes.All);
        }
    }

    [Fact]
    public void Canonical_LeavesCustomEditableSkillNamesUnchanged()
    {
        Assert.Equal("Painting", SD.SpecialSkills.Canonical("Painting"));
        Assert.Equal("Mój miecz", SD.SpecialSkills.Canonical("Mój miecz"));
    }
}
