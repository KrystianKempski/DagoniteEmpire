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
    [InlineData("Fishing Pier", MapImprovement.FishingHarbor)]
    [InlineData("Hunter's Lodge", MapImprovement.HuntersLodge)]
    [InlineData("Unfortified Village", MapImprovement.Village)]
    [InlineData("Brewery", null)]
    public void MapKindFromCatalogTemplateName_ResolvesKnownTemplates(string catalog, string? expected)
    {
        Assert.Equal(expected, TerrainImprovementCatalogMap.MapKindFromCatalogTemplateName(catalog));
    }
}
