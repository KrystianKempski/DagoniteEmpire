using DA_Common.Barony;

namespace DA_Business.Tests;

public class HerdStockRequirementsTests
{
    [Theory]
    [InlineData("Sheep pastures", "sheep")]
    [InlineData("Pastures (cattle)", "cattle")]
    [InlineData("Horse Stud (regular)", "horses")]
    [InlineData("Horse Stud (military)", "war-horses")]
    [InlineData("Horse Stud (noble)", "noble-horses")]
    public void TryGetRequiredGoodKey_KnownTemplates(string template, string expectedKey)
    {
        Assert.True(HerdStockRequirements.TryGetRequiredGoodKey(template, out var key));
        Assert.Equal(expectedKey, key);
    }

    [Fact]
    public void HasAccess_FalseWithoutAvailability()
    {
        Assert.False(HerdStockRequirements.HasAccess("Sheep pastures", availability: null));
    }

    [Fact]
    public void HasAccess_TrueWhenGoodAvailable()
    {
        var snap = TradeGoodAvailability.Resolve(
            facilityNames: Array.Empty<string>(),
            treaties: Array.Empty<BaronyTradeTreaty>(),
            mgOverrideKeys: new[] { "sheep" });

        Assert.True(HerdStockRequirements.HasAccess("Sheep pastures", snap));
        Assert.False(HerdStockRequirements.HasAccess("Pastures (cattle)", snap));
    }

    [Fact]
    public void ProducedKeys_SheepPastures_UnlockSheepAndWool()
    {
        var produced = TradeGoodAvailability.ProducedKeysFromFacilityNames(new[] { "Sheep pastures" });
        Assert.Contains("sheep", produced);
        Assert.Contains("wool", produced);
    }
}
