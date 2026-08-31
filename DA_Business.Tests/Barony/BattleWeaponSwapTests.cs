using DA_Common.Barony.Battle;
using Xunit;

namespace DA_Business.Tests.Barony;

public class BattleWeaponSwapTests
{
    [Fact]
    public void ActiveShield_OnlyWhenOneHandedWeaponActive()
    {
        var bow = "simple-bows";
        var sword = "short-spears";
        var shield = "wooden-buckler";

        Assert.Null(BattleWeaponSwap.ActiveShieldKey(bow, sword, shield, activeSlot: 1));
        Assert.Equal(shield, BattleWeaponSwap.ActiveShieldKey(bow, sword, shield, activeSlot: 2));
    }

    [Fact]
    public void NormalizeToActiveSlot_SwapsWeaponKeysWhenSlotTwo()
    {
        var (w1, w2, q1, q2) = BattleWeaponSwap.NormalizeToActiveSlot(
            "simple-bows", "short-spears", "Good", "Normal", activeSlot: 2);

        Assert.Equal("short-spears", w1);
        Assert.Equal("simple-bows", w2);
        Assert.Equal("Normal", q1);
        Assert.Equal("Good", q2);
    }

    [Fact]
    public void CanSwap_RequiresWeaponTwo()
    {
        Assert.False(BattleWeaponSwap.CanSwap(null));
        Assert.True(BattleWeaponSwap.CanSwap("short-spears"));
    }
}
