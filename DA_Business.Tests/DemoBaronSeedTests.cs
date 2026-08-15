using DA_Models;
using Xunit;

namespace DA_Business.Tests;

public class DemoBaronSeedTests
{
    [Fact]
    public void Load_ReturnsFullyPopulatedAldric()
    {
        var baron = DemoBaronSeed.Load();

        Assert.Equal("Aldric Emberfall", baron.NpcName);
        Assert.Equal("/images/aldric.jpg", baron.ImageUrl);
        Assert.Equal("/images/aldric-icon.webp", baron.IconUrl);

        Assert.NotNull(baron.Profession);
        Assert.Equal("Warrior", baron.Profession!.Name);
        Assert.NotNull(baron.Race);
        Assert.Equal("Human", baron.Race!.Name);

        Assert.Equal(7, baron.Attributes.Length);
        Assert.Equal(13, baron.BaseSkills.Length);
        Assert.Equal(80, baron.SpecialSkills.Length);
        Assert.NotEmpty(baron.Languages);

        // The sheet must carry real values, not a blank template.
        Assert.Contains(baron.Attributes, a => a.Name == "Strength" && a.BaseBonus > 0);
        Assert.Contains(baron.SpecialSkills, s => s.BaseBonus > 0);
    }
}
