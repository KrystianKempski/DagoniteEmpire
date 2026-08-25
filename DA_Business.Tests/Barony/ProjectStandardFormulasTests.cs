using DA_Common.Barony;

namespace DA_Business.Tests.Barony;

public class ProjectStandardFormulasTests
{
    [Theory]
    [InlineData(0, 0)]
    [InlineData(1, 0)]
    [InlineData(2, 1)]
    [InlineData(5, 2)]
    [InlineData(11, 5)]
    public void MaxProductionFromLoyalty_FloorsHalf(decimal loyalty, int expected)
        => Assert.Equal(expected, ProjectStandardFormulas.MaxProductionFromLoyalty(loyalty));

    [Theory]
    [InlineData(0, 0)]
    [InlineData(2, 0)]
    [InlineData(3, 1)]
    [InlineData(8, 2)]
    [InlineData(9, 3)]
    public void ProductionFromGold_UsesThreeImperialsPerUnit(int gold, int expected)
        => Assert.Equal(expected, ProjectStandardFormulas.ProductionFromGold(gold));

    [Fact]
    public void ClampGoldSpend_RespectsLoyaltyCapAndStep()
    {
        Assert.Equal(0, ProjectStandardFormulas.ClampGoldSpend(0, loyalty: 10));
        Assert.Equal(15, ProjectStandardFormulas.ClampGoldSpend(16, loyalty: 10)); // max 5 prod → 15 gold
        Assert.Equal(15, ProjectStandardFormulas.ClampGoldSpend(99, loyalty: 10));
    }
}
