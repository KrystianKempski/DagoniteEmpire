using DA_Common.Barony;

namespace DA_Business.Tests.Barony;

public class UnitArmorPierceTests
{
    [Fact]
    public void PierceEatsIntoArmorOnePointAtATime()
    {
        Assert.Equal(8, UnitCombatFormulas.EffectiveArmor(armor: 10, pierce: 2));
        Assert.Equal(6, UnitCombatFormulas.EffectiveArmor(armor: 10, pierce: 4));
    }

    [Fact]
    public void PierceIsWorthNothingAgainstAnUnarmoredTarget()
    {
        Assert.Equal(0, UnitCombatFormulas.EffectiveArmor(armor: 0, pierce: 4));
    }

    [Fact]
    public void PierceNeverDrivesArmorBelowZero()
    {
        // Overkill is wasted: half plate (8) against Pierce 20 is still just stripped, not negative.
        Assert.Equal(0, UnitCombatFormulas.EffectiveArmor(armor: 8, pierce: 20));
    }

    [Fact]
    public void NoPierceLeavesArmorUntouched()
    {
        Assert.Equal(6, UnitCombatFormulas.EffectiveArmor(armor: 6, pierce: 0));
        Assert.Equal(6, UnitCombatFormulas.EffectiveArmor(armor: 6, pierce: -3));
    }

    [Fact]
    public void CatalogWeaponsCoverTheArmorRangeTheyAreMeantTo()
    {
        var siegeCrossbow = UnitWeaponCatalog.Find("siege-crossbow");
        var club = UnitWeaponCatalog.Find("studded-clubs");
        Assert.NotNull(siegeCrossbow);
        Assert.NotNull(club);

        // Full plate (10) still stops most of a siege bolt, but a club is stopped outright.
        Assert.Equal(6, UnitCombatFormulas.EffectiveArmor(10, siegeCrossbow!.Pierce));
        Assert.Equal(10, UnitCombatFormulas.EffectiveArmor(10, club!.Pierce));
    }
}
