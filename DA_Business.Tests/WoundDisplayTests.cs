using DA_Common;
using DA_Common.Localization;
using DA_Models.CharacterModels;

namespace DA_Business.Tests;

public class WoundDisplayTests
{
    [Fact]
    public void DisplayDescription_PreservesAttackerNameFromSeededEnglishText()
    {
        var wound = new WoundDTO
        {
            Description = "Wound inflicted by bandyta z toporem after Normal attack.",
        };

        var display = wound.DisplayDescription();
        Assert.Contains("bandyta z toporem", display);
    }

    [Theory]
    [InlineData("Main arm", "Main arm")]
    [InlineData("Ręka główna", "Main arm")]
    public void CanonicalKey_MapsWoundLocationAliases(string stored, string expected)
    {
        Assert.Equal(expected, LocCatalog.CanonicalKey(stored));
        Assert.Equal(expected, Wounds.Location.Canonical(stored));
    }
}
