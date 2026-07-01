using DA_Common;
using DA_Models.ComponentModels;

namespace DA_Business.Tests;

public class BattleTurnServiceTests
{
    [Fact]
    public void AdvanceMobStates_DecrementsAndDropsExpired()
    {
        Assert.Equal("Bleeding:2, ", BattleTurnService.AdvanceMobStates("Stunned:1, Bleeding:3, "));
    }

    [Theory]
    [InlineData(3, 2)]
    [InlineData(1, 0)]
    [InlineData(0, -1)]
    public void DecrementDuration_SubtractsOne(int duration, int expected)
    {
        Assert.Equal(expected, BattleTurnService.DecrementDuration(duration));
    }

    [Theory]
    [InlineData(0, true)]
    [InlineData(-1, true)]
    [InlineData(1, false)]
    public void IsExpired_TrueAtOrBelowZero(int duration, bool expected)
    {
        Assert.Equal(expected, BattleTurnService.IsExpired(duration));
    }

    [Fact]
    public void ResolveEndOfBattleMobStates_KeepsDeadAsPermanent()
    {
        var result = BattleTurnService.ResolveEndOfBattleMobStates("Stunned:2, Dead:5, ");

        Assert.Equal(CombatStateString.Add(null, States.Names.Dead, States.Duration.Permanent), result);
    }

    [Fact]
    public void ResolveEndOfBattleMobStates_KeepsUnconsciousWhenNotDead()
    {
        var result = BattleTurnService.ResolveEndOfBattleMobStates("Unconscious:2, Bleeding:9, ");

        Assert.Equal(CombatStateString.Add(null, States.Names.Unconscious, States.Duration.UntilResolved), result);
    }

    [Fact]
    public void ResolveEndOfBattleMobStates_ClearsEverythingElse()
    {
        Assert.Equal(string.Empty, BattleTurnService.ResolveEndOfBattleMobStates("Stunned:2, Bleeding:9, "));
        Assert.Equal(string.Empty, BattleTurnService.ResolveEndOfBattleMobStates(null));
    }

    [Theory]
    [InlineData(States.Names.Dead, true)]
    [InlineData(States.Names.Unconscious, true)]
    [InlineData(States.Names.Stunned, false)]
    [InlineData(States.Names.Bleeding, false)]
    [InlineData(null, false)]
    public void PersistsAfterBattle_OnlyDeadAndUnconscious(string? stateName, bool expected)
    {
        Assert.Equal(expected, BattleTurnService.PersistsAfterBattle(stateName));
    }
}
