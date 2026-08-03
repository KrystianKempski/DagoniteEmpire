using DA_Common.Barony;

namespace DA_Business.Tests;

public class UnitMountCatalogTests
{
    [Theory]
    [InlineData("horses", 2, 2, 1, 3, 6, 150, 150, 200, "horses")]
    [InlineData("war-horses", 4, 4, 2, 3, 8, 250, 250, 300, "war-horses")]
    public void Catalog_MatchesSpec(
        string key, int atk, int def, int dmg, int move, int riding,
        int prod, int gold, int mkt, string trade)
    {
        var m = UnitMountCatalog.Find(key);
        Assert.NotNull(m);
        Assert.Equal(atk, m!.Attack);
        Assert.Equal(def, m.Defense);
        Assert.Equal(dmg, m.Damage);
        Assert.Equal(move, m.MoveBonus);
        Assert.Equal(riding, m.RequiredRiding);
        Assert.Equal(prod, m.ProductionCost);
        Assert.Equal(gold, m.GoldCost);
        Assert.Equal(mkt, m.MarketGold);
        Assert.Equal(trade, m.RequiredTradeGoodKey);
    }

    [Fact]
    public void SliceMount_DefenseIsTwiceMarket()
    {
        var horses = UnitMountCatalog.Find("horses")!;
        var war = UnitMountCatalog.Find("war-horses")!;
        Assert.Equal((0, 0, 400), UnitTrainingCostFormulas.SliceMount(horses, UnitEquipmentAcquireMode.Defense));
        Assert.Equal((0, 0, 600), UnitTrainingCostFormulas.SliceMount(war, UnitEquipmentAcquireMode.Defense));
        Assert.Equal((150, 150, 0), UnitTrainingCostFormulas.SliceMount(horses, UnitEquipmentAcquireMode.Craft));
    }

    [Fact]
    public void MeetsMount_RequiresRidingAndTrade()
    {
        var horses = UnitMountCatalog.Find("horses")!;
        Assert.False(UnitEquipmentTradeAccess.MeetsMount(horses, ridingSkill: 5, EmptySnap(), out _));
        Assert.False(UnitEquipmentTradeAccess.MeetsMount(horses, ridingSkill: 6, EmptySnap(), out _));
        Assert.True(UnitEquipmentTradeAccess.MeetsMount(horses, ridingSkill: 6, Snap("horses"), out _));
    }

    [Fact]
    public void Combat_AddsMountBonuses()
    {
        var skills = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            [UnitSkillKey.Melee] = 0,
            [UnitSkillKey.Dodges] = 1,
            [UnitSkillKey.Run] = 0,
            [UnitSkillKey.Endurance] = 0,
        };
        var spears = UnitWeaponCatalog.Find("short-spears")!;
        var skillKey = UnitSkillTree.SkillKeyForWeaponType(spears.WeaponType);
        if (skillKey is not null)
            skills[skillKey] = 2;

        var mount = UnitMountCatalog.Find("horses")!;
        var withMount = UnitCombatFormulas.Compute(
            build: 5, agility: 5, will: 5, perception: 5, discipline: 1,
            skills, spears, armor: null, shield: null, weaponQuality: UnitWeaponQuality.Normal,
            raceMoveBonus: 3, troopCount: 50, mount: mount);
        var without = UnitCombatFormulas.Compute(
            build: 5, agility: 5, will: 5, perception: 5, discipline: 1,
            skills, spears, armor: null, shield: null, weaponQuality: UnitWeaponQuality.Normal,
            raceMoveBonus: 3, troopCount: 50);

        Assert.Equal(without.Attack + 2, withMount.Attack);
        Assert.Equal(without.Defense + 2, withMount.Defense);
        Assert.Equal(without.Damage + 1, withMount.Damage);
        Assert.Equal(without.Move + 3, withMount.Move);
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
