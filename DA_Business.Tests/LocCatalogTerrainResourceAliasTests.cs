using DA_Common.Barony;
using DA_Common.Localization;

namespace DA_Business.Tests;

public class LocCatalogTerrainResourceAliasTests
{
    [Theory]
    [InlineData("Iron", TerrainResource.Iron)]
    [InlineData("Żelazo", TerrainResource.Iron)]
    [InlineData("Soft metals", TerrainResource.SoftMetals)]
    [InlineData("Metale miękkie", TerrainResource.SoftMetals)]
    [InlineData("Gold", TerrainResource.Gold)]
    [InlineData("Złoto", TerrainResource.Gold)]
    [InlineData("Fishery", TerrainResource.Fishery)]
    [InlineData("Łowisko", TerrainResource.Fishery)]
    [InlineData("Elven alder", TerrainResource.ElvenAlder)]
    [InlineData("Elfia olcha", TerrainResource.ElvenAlder)]
    [InlineData("Woad", TerrainResource.Woad)]
    [InlineData("Urzet", TerrainResource.Woad)]
    public void Canonical_MapsEnglishAndPolishTerrainResources(string stored, string expected)
    {
        Assert.Equal(expected, TerrainResource.Canonical(stored));
        Assert.Equal(expected, LocCatalog.CanonicalKey(stored));
    }

    [Theory]
    [InlineData("Forest", TerrainFeature.ForestName)]
    [InlineData("Las", TerrainFeature.ForestName)]
    [InlineData("Dense forest", TerrainFeature.DenseForestName)]
    [InlineData("Gęsty las", TerrainFeature.DenseForestName)]
    [InlineData("Swamp", TerrainFeature.SwampName)]
    [InlineData("Bagna", TerrainFeature.SwampName)]
    public void Canonical_MapsEnglishAndPolishTerrainFeatures(string stored, string expected)
    {
        Assert.Equal(expected, TerrainFeature.Canonical(stored));
        Assert.Equal(expected, LocCatalog.CanonicalKey(stored));
    }

    [Theory]
    [InlineData("Plains", TerrainBaseType.Plains)]
    [InlineData("Równiny", TerrainBaseType.Plains)]
    [InlineData("Hills", TerrainBaseType.Hills)]
    [InlineData("Wzgórza", TerrainBaseType.Hills)]
    public void Canonical_MapsEnglishAndPolishTerrainBaseTypes(string stored, string expected)
    {
        Assert.Equal(expected, TerrainBaseType.Canonical(stored));
        Assert.Equal(expected, LocCatalog.CanonicalKey(stored));
    }

    [Fact]
    public void CanonicalNameOrRaw_KeepsCustomResourceNames()
    {
        Assert.Equal("Weird ore", TerrainResource.CanonicalNameOrRaw("Weird ore"));
    }
}
