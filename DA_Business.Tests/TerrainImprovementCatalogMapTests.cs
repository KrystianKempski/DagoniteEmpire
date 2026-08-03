using DA_Common.Barony;

namespace DA_Business.Tests;

public class TerrainImprovementCatalogMapTests
{
    [Theory]
    [InlineData("Sawmill - common", MapImprovement.Sawmill)]
    [InlineData("Sawmill - Ironwood", MapImprovement.Sawmill)]
    [InlineData("Sawmill - Elven alder", MapImprovement.Sawmill)]
    [InlineData("Sawmill - Shipbuilding wood", MapImprovement.Sawmill)]
    [InlineData("Mine - Salt", MapImprovement.Mine)]
    [InlineData("Quarry - Granite", MapImprovement.Mine)]
    [InlineData("Clay pit", MapImprovement.Mine)]
    [InlineData("Farm", MapImprovement.Farm)]
    [InlineData("Farm - fertile", MapImprovement.Farm)]
    [InlineData("Farm (Dye plant)", MapImprovement.Farm)]
    [InlineData("Fishing Pier", MapImprovement.FishingHarbor)]
    [InlineData("Hunter's Lodge", MapImprovement.HuntersLodge)]
    [InlineData("Unfortified Village", MapImprovement.Village)]
    [InlineData("Brewery", null)]
    public void MapKindFromCatalogTemplateName_ResolvesKnownTemplates(string catalog, string? expected)
    {
        Assert.Equal(expected, TerrainImprovementCatalogMap.MapKindFromCatalogTemplateName(catalog));
    }

    [Theory]
    [InlineData(TerrainResource.Woad)]
    [InlineData(TerrainResource.Madder)]
    [InlineData(TerrainResource.Weld)]
    public void ResolveTemplateName_FarmOnDyePlant_ReturnsDyeFarm(string dyeResource)
    {
        var name = TerrainImprovementCatalogMap.ResolveTemplateName(
            MapImprovement.Farm,
            fertility: 3,
            resource: dyeResource,
            featuresMask: 0,
            baseType: TerrainBaseType.Plains);

        Assert.Equal(TerrainImprovementCatalogMap.FarmDyePlant, name);
    }

    [Fact]
    public void ResolveTemplateName_FarmWithoutDyePlant_ReturnsGrainFarm()
    {
        var name = TerrainImprovementCatalogMap.ResolveTemplateName(
            MapImprovement.Farm,
            fertility: 3,
            resource: TerrainResource.Iron,
            featuresMask: 0,
            baseType: TerrainBaseType.Plains);

        Assert.Equal("Farm", name);
    }
}
