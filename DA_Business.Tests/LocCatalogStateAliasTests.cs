using DA_Common;
using DA_Common.Localization;

namespace DA_Business.Tests;

public class LocCatalogStateAliasTests
{
    [Theory]
    [InlineData("Stunned", States.Names.Stunned)]
    [InlineData("Ogłuszony", States.Names.Stunned)]
    [InlineData("Potknięty", States.Names.Stumbled)]
    [InlineData("Full defence", States.Names.FullDefence)]
    [InlineData("Pełna obrona", States.Names.FullDefence)]
    [InlineData("Half turn", States.Names.HalfTurn)]
    [InlineData("Pół tury", States.Names.HalfTurn)]
    [InlineData("No turn", States.Names.NoTurn)]
    [InlineData("Brak tury", States.Names.NoTurn)]
    [InlineData("Krwawiący", States.Names.Bleeding)]
    public void CanonicalKey_MapsEnglishAndPolishTemporaryStateAliases(string stored, string expected)
    {
        Assert.Equal(expected, LocCatalog.CanonicalKey(stored));
        Assert.Equal(expected, States.Names.Canonical(stored));
    }
}
