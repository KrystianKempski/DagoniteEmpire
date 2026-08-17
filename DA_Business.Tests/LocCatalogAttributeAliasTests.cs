using DA_Common;
using DA_Common.Localization;

namespace DA_Business.Tests;

public class LocCatalogAttributeAliasTests
{
    [Theory]
    [InlineData("Strength", SD.Attributes.Strength)]
    [InlineData("strength", SD.Attributes.Strength)]
    [InlineData("Siła", SD.Attributes.Strength)]
    [InlineData("siła", SD.Attributes.Strength)]
    [InlineData("Zręczność", SD.Attributes.Dexterity)]
    [InlineData("Wytrzymałość", SD.Attributes.Endurance)]
    [InlineData("Inteligencja", SD.Attributes.Intelligence)]
    [InlineData("Instynkt", SD.Attributes.Instinct)]
    [InlineData("Siła Woli", SD.Attributes.Willpower)]
    [InlineData("Siła woli", SD.Attributes.Willpower)]
    [InlineData("Sila", SD.Attributes.Strength)]
    [InlineData("Siła  woli", SD.Attributes.Willpower)]
    [InlineData("sila woli", SD.Attributes.Willpower)]
    [InlineData("Charyzma", SD.Attributes.Charisma)]
    public void CanonicalKey_MapsEnglishAndPolishAliases(string stored, string expected)
    {
        Assert.Equal(expected, LocCatalog.CanonicalKey(stored));
        Assert.Equal(expected, SD.Attributes.Canonical(stored));
    }

    [Fact]
    public void Name_UsesCanonicalEnglishKey()
    {
        // Loc is unconfigured in unit tests, so T() returns the English key.
        Assert.Equal(SD.Attributes.Intelligence, LocCatalog.Name("Inteligencja"));
        Assert.Equal(SD.Attributes.Strength, LocCatalog.Name("Siła"));
        Assert.Equal(SD.Attributes.Willpower, LocCatalog.Name("Siła Woli"));
    }

    [Fact]
    public void CharacterSeeder_UsesEnglishAttributeKeys()
    {
        var seeded = DA_Models.CharacterSeeder.GetAttributes();
        Assert.Equal(SD.Attributes.All.Length, seeded.Count);
        foreach (var key in SD.Attributes.All)
        {
            Assert.True(seeded.ContainsKey(key), $"Missing seeded attribute {key}");
            Assert.Equal(key, seeded[key].Name);
        }
    }
}
