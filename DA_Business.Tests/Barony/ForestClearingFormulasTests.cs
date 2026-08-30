using DA_Common.Barony;
using Xunit;

namespace DA_Business.Tests.Barony;

public class ForestClearingFormulasTests
{
    [Theory]
    [InlineData(TerrainFeature.Forest, ForestClearingVariant.Forest)]
    [InlineData(TerrainFeature.DenseForest, ForestClearingVariant.DenseForest)]
    [InlineData(TerrainFeature.Forest | TerrainFeature.River, ForestClearingVariant.Forest)]
    public void DetectVariant_ReturnsExpected(int mask, ForestClearingVariant expected)
        => Assert.Equal(expected, ForestClearingFormulas.DetectVariant(mask));

    [Fact]
    public void DetectVariant_PrefersDenseForest_WhenBothWouldExist()
    {
        var mask = TerrainFeature.Forest | TerrainFeature.DenseForest;
        Assert.Equal(ForestClearingVariant.DenseForest, ForestClearingFormulas.DetectVariant(mask));
    }

    [Theory]
    [InlineData(TerrainFeature.Coast, false)]
    [InlineData(0, false)]
    public void CanClearTile_ReturnsFalseWithoutForest(int mask, bool expected)
        => Assert.Equal(expected, ForestClearingFormulas.CanClearTile(mask));

    [Fact]
    public void ForestCostAndReward_MatchDesign()
    {
        var cost = ForestClearingFormulas.Cost(ForestClearingVariant.Forest);
        var reward = ForestClearingFormulas.Reward(ForestClearingVariant.Forest);
        Assert.Equal(100m, cost[Ppb.Production]);
        Assert.Equal(30m, cost[Ppb.Treasury]);
        Assert.Equal(200m, reward[Ppb.Production]);
        Assert.Equal(50m, reward[Ppb.Treasury]);
    }

    [Fact]
    public void DenseForestCostAndReward_MatchDesign()
    {
        var cost = ForestClearingFormulas.Cost(ForestClearingVariant.DenseForest);
        var reward = ForestClearingFormulas.Reward(ForestClearingVariant.DenseForest);
        Assert.Equal(300m, cost[Ppb.Production]);
        Assert.Equal(100m, cost[Ppb.Treasury]);
        Assert.Equal(600m, reward[Ppb.Production]);
        Assert.Equal(200m, reward[Ppb.Treasury]);
    }

    [Fact]
    public void ClampTurnsRemaining_EnforcesMinimum()
    {
        Assert.Equal(1, ForestClearingFormulas.ClampTurnsRemaining(0));
        Assert.Equal(1, ForestClearingFormulas.ClampTurnsRemaining(1));
        Assert.Equal(3, ForestClearingFormulas.ClampTurnsRemaining(3));
    }
}
