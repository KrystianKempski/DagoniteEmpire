using DA_Common.Barony;

namespace DA_Business.Tests;

public class UnitUpkeepFormulasTests
{
    [Fact]
    public void Compute_DefenseBlocksUseMarketGoldDivisor50_GoldUses100()
    {
        // Short spears Mkt 20 + light leather 60 + wooden medium shield 30 = 110.
        // Gold blocks: floor(110/100) = 1 → gear gold 2.
        // Defense blocks: floor(110/50) = 2 → defense 2.
        var u = UnitUpkeepFormulas.Compute(
            baseWage: 10,
            upkeepFood: 0.5m,
            storedUpkeepDefense: 5,
            weapon1Key: "short-spears",
            weapon2Key: null,
            armorKey: "light-leather",
            shieldKey: "wooden-medium-shield");

        Assert.Equal(110, u.GearMarketGold);
        Assert.Equal(1, u.GearBlocks);
        Assert.Equal(2, u.GearDefenseBlocks);
        Assert.Equal(2, u.GearGold);
        Assert.Equal(2, u.Defense);
        Assert.Equal(12, u.Gold);
    }

    [Fact]
    public void Compute_DefenseBlockSizeIs50()
    {
        Assert.Equal(100, UnitRules.GearUpkeepMarketGoldPerBlock);
        Assert.Equal(50, UnitRules.GearUpkeepDefenseMarketGoldPerBlock);
    }
}
