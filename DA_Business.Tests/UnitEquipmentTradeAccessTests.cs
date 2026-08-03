using DA_Common.Barony;

namespace DA_Business.Tests;

public class UnitEquipmentTradeAccessTests
{
    [Theory]
    [InlineData("short-spears", null)]
    [InlineData("simple-bows", null)]
    [InlineData("longswords", "access-arms-military")]
    [InlineData("war-bows", "access-arms-military")]
    [InlineData("arquebuses", "access-arms-firearms")]
    public void RequiredGoodKey_Weapons(string weaponKey, string? expectedGood)
    {
        var w = UnitWeaponCatalog.Find(weaponKey);
        Assert.NotNull(w);
        Assert.Equal(expectedGood, UnitEquipmentTradeAccess.RequiredGoodKey(w!));
    }

    [Theory]
    [InlineData("wooden-medium-shield", "access-armor-light")]
    [InlineData("light-leather", "access-armor-light")]
    [InlineData("mail-and-gambeson", "access-armor-medium")]
    [InlineData("studded-medium-shield", "access-armor-medium")]
    [InlineData("full-plate", "access-armor-heavy")]
    [InlineData("metal-large-shield", "access-armor-heavy")]
    public void RequiredGoodKey_ArmorTiers(string armorKey, string expectedGood)
    {
        var a = UnitArmorCatalog.Find(armorKey);
        Assert.NotNull(a);
        Assert.Equal(expectedGood, UnitEquipmentTradeAccess.RequiredGoodKey(a!));
    }

    [Fact]
    public void MeetsWeapon_SimpleAlwaysWithoutTradeGood()
    {
        var spears = UnitWeaponCatalog.Find("short-spears")!;
        Assert.True(UnitEquipmentTradeAccess.MeetsWeapon(spears, build: 1, agility: 1, availability: EmptySnap(), out _));
    }

    [Fact]
    public void MeetsWeapon_MilitaryRequiresAccess()
    {
        var sword = UnitWeaponCatalog.Find("longswords")!;
        Assert.False(UnitEquipmentTradeAccess.MeetsWeapon(sword, build: 10, agility: 10, EmptySnap(), out var why));
        Assert.Contains("Military", why, StringComparison.OrdinalIgnoreCase);

        var withAccess = Snap("access-arms-military");
        Assert.True(UnitEquipmentTradeAccess.MeetsWeapon(sword, build: 10, agility: 10, withAccess, out _));
    }

    [Fact]
    public void MeetsArmor_MediumRequiresAccess()
    {
        var mail = UnitArmorCatalog.Find("mail-and-gambeson")!;
        Assert.False(UnitEquipmentTradeAccess.MeetsArmor(mail, build: 10, armorSkill: 10, EmptySnap(), out _));
        Assert.True(UnitEquipmentTradeAccess.MeetsArmor(mail, build: 10, armorSkill: 10, Snap("access-armor-medium"), out _));
    }

    [Fact]
    public void HasAccess_NullAvailability_DoesNotLock()
    {
        var sword = UnitWeaponCatalog.Find("longswords")!;
        Assert.True(UnitEquipmentTradeAccess.HasAccess(sword, availability: null));
    }

    [Fact]
    public void FirstEligibleWeaponKey_FallsBackToSimpleWithoutMilitary()
    {
        var key = UnitEquipmentTradeAccess.FirstEligibleWeaponKey(10, 10, EmptySnap());
        Assert.NotNull(key);
        var w = UnitWeaponCatalog.Find(key!);
        Assert.NotNull(w);
        Assert.Equal("simple", w!.Category, ignoreCase: true);
    }

    private static TradeGoodAvailabilitySnapshot EmptySnap() =>
        TradeGoodAvailability.Resolve(
            facilityNames: Array.Empty<string>(),
            treaties: Array.Empty<BaronyTradeTreaty>(),
            mgOverrideKeys: Array.Empty<string>());

    private static TradeGoodAvailabilitySnapshot Snap(params string[] keys) =>
        TradeGoodAvailability.Resolve(
            facilityNames: Array.Empty<string>(),
            treaties: Array.Empty<BaronyTradeTreaty>(),
            mgOverrideKeys: keys);
}
