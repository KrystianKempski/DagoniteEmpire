using DA_Models.ComponentModels;

namespace DA_Business.Tests;

public class MobHealthModelTests
{
    [Theory]
    [InlineData(16, true, 5, 11)]
    [InlineData(16, false, 0, 16)]
    [InlineData(5, true, 2, 3)]
    [InlineData(2, true, 1, 1)]
    [InlineData(1, true, 0, 1)]
    [InlineData(0, true, 0, 0)]
    public void ApplyIncomingDamage_IgnoresOneThirdWhenPainResistanceSucceeds(
        int damage,
        bool painRes,
        int expectedIgnored,
        int expectedApplied)
    {
        var applied = MobHealthModel.ApplyIncomingDamage(damage, painRes);
        var ignored = damage - applied;

        Assert.Equal(expectedApplied, applied);
        Assert.Equal(expectedIgnored, ignored);
    }

    [Theory]
    [InlineData(0, 12, 0)]   // 100%
    [InlineData(3, 12, 0)]   // 75%
    [InlineData(4, 12, 2)]   // 67%
    [InlineData(6, 12, 2)]   // 50%
    [InlineData(7, 12, 4)]   // 42%
    [InlineData(9, 12, 4)]   // 25%
    [InlineData(10, 12, 6)]  // 17%
    [InlineData(12, 12, 6)]  // 0%
    [InlineData(20, 12, 6)]  // overflow
    public void CombatPenalty_UsesRemainingHpBands(int currentWounds, int maxWounds, int expectedPenalty)
    {
        Assert.Equal(expectedPenalty, MobHealthModel.CombatPenalty(currentWounds, maxWounds));
    }

    [Fact]
    public void FormatHpLog_IncludesAppliedPenalty()
    {
        var log = MobHealthModel.FormatHpLog(currentWounds: 10, maxWounds: 12);

        Assert.Contains("HP 2/12", log);
        Assert.Contains("-6 attack/defence", log);
    }
}
