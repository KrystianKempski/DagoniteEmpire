using DA_Common;
using DA_Common.Localization;

namespace DA_Business.Tests;

public class LocCatalogWeaponAliasTests
{
    [Theory]
    [InlineData("Weapon melee", SD.EquipmentType.WeaponMelee)]
    [InlineData("Broń wręcz", SD.EquipmentType.WeaponMelee)]
    [InlineData("Weapon ranged", SD.EquipmentType.WeaponRanged)]
    [InlineData("Broń dystansowa", SD.EquipmentType.WeaponRanged)]
    [InlineData("other", SD.EquipmentType.Other)]
    [InlineData("Tarcza", SD.EquipmentType.Shield)]
    [InlineData("Głowa", SD.EquipmentType.Head)]
    public void CanonicalKey_MapsEnglishAndPolishEquipmentTypeAliases(string stored, string expected)
    {
        Assert.Equal(expected, LocCatalog.CanonicalKey(stored));
        Assert.Equal(expected, SD.EquipmentType.Canonical(stored));
    }

    [Theory]
    [InlineData("Fast", SD.WeaponQuality.Fast)]
    [InlineData("Szybki", SD.WeaponQuality.Fast)]
    [InlineData("Szybka", SD.WeaponQuality.Fast)]
    [InlineData("Armor piercing", SD.WeaponQuality.ArmorPiercing)]
    [InlineData("Przebijający", SD.WeaponQuality.ArmorPiercing)]
    [InlineData("Przebijająca pancerz", SD.WeaponQuality.ArmorPiercing)]
    [InlineData("Heavy", SD.WeaponQuality.Heavy)]
    [InlineData("Ciężki", SD.WeaponQuality.Heavy)]
    [InlineData("Ciężka", SD.WeaponQuality.Heavy)]
    [InlineData("Devastating", SD.WeaponQuality.Devastating)]
    [InlineData("Druzgocący", SD.WeaponQuality.Devastating)]
    [InlineData("Stumbling", SD.WeaponQuality.Stumbling)]
    [InlineData("Potykający", SD.WeaponQuality.Stumbling)]
    [InlineData("Snatching", SD.WeaponQuality.Snatching)]
    [InlineData("Pochwycająca", SD.WeaponQuality.Snatching)]
    [InlineData("Bulky", SD.WeaponQuality.Bulky)]
    [InlineData("Niewygodny", SD.WeaponQuality.Bulky)]
    [InlineData("Precise", SD.WeaponQuality.Precise)]
    [InlineData("Celny", SD.WeaponQuality.Precise)]
    [InlineData("Light", SD.WeaponQuality.Light)]
    [InlineData("Lekka", SD.WeaponQuality.Light)]
    [InlineData("Trwałość", SD.WeaponQuality.Durability)]
    [InlineData("Armor Penalty", SD.WeaponQuality.ArmorPenalty)]
    [InlineData("Kara (akrobatyka)", SD.WeaponQuality.ArmorPenalty)]
    [InlineData("Przeładowanie", SD.WeaponQuality.Reload)]
    public void CanonicalKey_MapsEnglishAndPolishWeaponQualityAliases(string stored, string expected)
    {
        Assert.Equal(expected, LocCatalog.CanonicalKey(stored));
        Assert.Equal(expected, SD.WeaponQuality.Canonical(stored));
    }

    [Theory]
    [InlineData("Dagger", SD.BasicWeaponsMelee.Dagger)]
    [InlineData("Sztylet", SD.BasicWeaponsMelee.Dagger)]
    [InlineData("Long sword", SD.BasicWeaponsMelee.LongSword)]
    [InlineData("Długi miecz", SD.BasicWeaponsMelee.LongSword)]
    [InlineData("Miecz długi", SD.BasicWeaponsMelee.LongSword)]
    [InlineData("Halberd", SD.BasicWeaponsMelee.Halberd)]
    [InlineData("Halabarda", SD.BasicWeaponsMelee.Halberd)]
    [InlineData("Slingshot", SD.BasicWeaponsShooting.Slingshot)]
    [InlineData("Proca", SD.BasicWeaponsShooting.Slingshot)]
    [InlineData("Javelin", SD.BasicWeaponsShooting.Javelin)]
    [InlineData("Oszczep", SD.BasicWeaponsShooting.Javelin)]
    [InlineData("Kusza lekka", SD.BasicWeaponsShooting.CrossbowLight)]
    [InlineData("Wooden shield", SD.BasicShields.WoodenShield)]
    [InlineData("Tarcza drewniana", SD.BasicShields.WoodenShield)]
    public void CanonicalKey_MapsEnglishAndPolishItemNameAliases(string stored, string expected)
    {
        Assert.Equal(expected, LocCatalog.CanonicalKey(stored));
        Assert.Equal(expected, SD.BasicEquipment.Canonical(stored));
    }

    [Fact]
    public void CanonicalKey_MapsWeaponParametersTraitName()
    {
        Assert.Equal(SD.WeaponParametersDescr, LocCatalog.CanonicalKey("Weapon parameters"));
        Assert.Equal(SD.WeaponParametersDescr, LocCatalog.CanonicalKey("Parametry broni"));
    }

    [Fact]
    public void Heavy_IsWeaponQuality_NotHeavyWeaponsSkill()
    {
        Assert.Equal(SD.WeaponQuality.Heavy, LocCatalog.CanonicalKey("Heavy"));
        Assert.Equal(SD.SpecialSkills.Melee.Heavy, LocCatalog.CanonicalKey("Heavy weapons"));
        Assert.Equal(SD.SpecialSkills.Melee.Heavy, LocCatalog.CanonicalKey("Broń ciężka"));
    }

    [Fact]
    public void CanonicalNameOrRaw_LeavesCustomItemNamesUnchanged()
    {
        Assert.Equal("Mój miecz", SD.BasicEquipment.CanonicalNameOrRaw("Mój miecz"));
        Assert.Equal("custom long sword", SD.BasicEquipment.CanonicalNameOrRaw("custom long sword"));
        Assert.Equal(SD.BasicWeaponsMelee.Dagger, SD.BasicEquipment.CanonicalNameOrRaw("Sztylet"));
    }

    [Fact]
    public void NameOrRaw_LocalizesCatalogItemsOnly()
    {
        Assert.Equal(SD.BasicWeaponsMelee.Dagger, LocCatalog.NameOrRaw("Sztylet", SD.BasicEquipment.Names));
        Assert.Equal("Mój miecz", LocCatalog.NameOrRaw("Mój miecz", SD.BasicEquipment.Names));
    }
}
