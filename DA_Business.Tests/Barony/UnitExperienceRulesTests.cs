using DA_Common.Barony;

namespace DA_Business.Tests.Barony;

public class UnitExperienceRulesTests
{
    [Fact]
    public void DamageThresholdsUseFullChunksOnly()
    {
        var low = UnitExperienceRules.ComputeBattleXp(
            damageDealt: 2,
            engagedRounds: 0,
            kills: 0,
            damageTaken: 7,
            fled: false);
        var exact = UnitExperienceRules.ComputeBattleXp(
            damageDealt: 3,
            engagedRounds: 0,
            kills: 0,
            damageTaken: 8,
            fled: false);

        Assert.Equal(0, low.FromDamageDealt);
        Assert.Equal(0, low.FromDamageTakenLoss);
        Assert.Equal(1, exact.FromDamageDealt);
        Assert.Equal(1, exact.FromDamageTakenLoss);
    }

    [Fact]
    public void NetXpCombinesAllSources()
    {
        var xp = UnitExperienceRules.ComputeBattleXp(
            damageDealt: 14,   // +4
            engagedRounds: 3,  // +3
            kills: 2,          // +6
            damageTaken: 17,   // -2
            fled: true);       // -1

        Assert.Equal(4, xp.FromDamageDealt);
        Assert.Equal(3, xp.FromEngagedRounds);
        Assert.Equal(6, xp.FromKills);
        Assert.Equal(2, xp.FromDamageTakenLoss);
        Assert.Equal(1, xp.FromFleeLoss);
        Assert.Equal(10, xp.Net);
    }

    [Fact]
    public void NegativeInputsAreClampedToZero()
    {
        var xp = UnitExperienceRules.ComputeBattleXp(
            damageDealt: -10,
            engagedRounds: -2,
            kills: -1,
            damageTaken: -8,
            fled: false);

        Assert.Equal(0, xp.FromDamageDealt);
        Assert.Equal(0, xp.FromEngagedRounds);
        Assert.Equal(0, xp.FromKills);
        Assert.Equal(0, xp.FromDamageTakenLoss);
        Assert.Equal(0, xp.FromFleeLoss);
        Assert.Equal(0, xp.Net);
    }
}
