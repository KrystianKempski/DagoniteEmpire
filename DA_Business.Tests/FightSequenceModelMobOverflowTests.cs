using DA_Common;
using DA_Models.ComponentModels;

namespace DA_Business.Tests;

public class FightSequenceModelMobOverflowTests
{
    [Theory]
    [InlineData(12, 12, false, false)]
    [InlineData(13, 12, false, true)]
    [InlineData(19, 12, false, true)]
    [InlineData(20, 12, true, false)]
    [InlineData(30, 12, true, false)]
    public void EvaluateMobWoundOverflow_UsesThresholds(int projectedWounds, int maxWounds, bool expectDead, bool expectUnconscious)
    {
        var result = FightSequenceModel.EvaluateMobWoundOverflow(projectedWounds, maxWounds);

        Assert.Equal(expectDead, result.IsDead);
        Assert.Equal(expectUnconscious, result.IsUnconscious);

        if (expectDead)
        {
            Assert.Equal(
                CombatStateString.Add(null, States.Names.Dead, States.Duration.Permanent),
                result.NewStates);
        }
        else if (expectUnconscious)
        {
            Assert.Equal(
                CombatStateString.Add(null, States.Names.Unconscious, States.Duration.UntilResolved),
                result.NewStates);
        }
        else
        {
            Assert.Equal(FightSequenceModel.MobWoundOverflowResult.None, result);
        }
    }

    [Fact]
    public void EvaluateMobWoundOverflow_DeadTakesPriorityOverUnconscious()
    {
        var result = FightSequenceModel.EvaluateMobWoundOverflow(25, 12);

        Assert.True(result.IsDead);
        Assert.False(result.IsUnconscious);
    }
}
