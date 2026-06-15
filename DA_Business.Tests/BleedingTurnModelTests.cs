using DA_Common;
using DA_Models.ComponentModels;

namespace DA_Business.Tests;

public class BleedingTurnModelTests
{
    [Theory]
    [InlineData(99, 5)]
    [InlineData(50, 54)]
    [InlineData(1, 103)]
    [InlineData(104, 1)]
    public void GetBleedingPainTestDc_Returns104MinusTurnsRemaining(int turnsRemaining, int expectedDc)
    {
        Assert.Equal(expectedDc, BleedingTurnModel.GetBleedingPainTestDc(turnsRemaining));
    }

    [Fact]
    public void TryParseMobStateDuration_FindsBleedingTurns()
    {
        var states = "Stunned:3, Bleeding:99, ";

        Assert.True(BleedingTurnModel.TryParseMobStateDuration(states, States.Names.Bleeding, out var duration));
        Assert.Equal(99, duration);
    }

    [Fact]
    public void RunPainTest_UnconsciousDurationScalesWithDc()
    {
        var result = BleedingTurnModel.RunPainTest(50, 10);

        Assert.Equal(54, result.Dc);
        Assert.Equal(5, result.UnconsciousDuration);
    }
}
