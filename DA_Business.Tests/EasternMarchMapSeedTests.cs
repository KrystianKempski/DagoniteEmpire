using DA_Common.Barony;

namespace DA_Business.Tests;

public class EasternMarchMapSeedTests
{    [Fact]
    public void CreateSeedDocument_LoadsAuthoredNodesAndRoutes()
    {
        var doc = EasternMarchMapDefaults.CreateSeedDocument();

        Assert.Equal(55, doc.Nodes.Count);
        Assert.Equal(74, doc.Routes.Count);
        Assert.All(doc.Nodes, n => Assert.False(string.IsNullOrWhiteSpace(n.LordKey)));

        // Every route connects two real nodes (authored links must be intact).
        var ids = doc.Nodes.Select(n => n.Id).ToHashSet(StringComparer.OrdinalIgnoreCase);
        Assert.All(doc.Routes, r =>
        {
            Assert.Contains(r.FromNodeId, ids);
            Assert.Contains(r.ToNodeId, ids);
        });
    }

    [Fact]
    public void CreateSeedDocument_DarkholdIsResolvableAsBaronSeat()
    {
        var doc = EasternMarchMapDefaults.CreateSeedDocument();

        var darkhold = doc.Nodes.SingleOrDefault(n =>
            string.Equals(n.Label, "Darkhold", StringComparison.OrdinalIgnoreCase));
        Assert.NotNull(darkhold);
        Assert.Equal("thaddeus", darkhold!.LordKey);

        var seatId = PlayerBaronyMarchSeats.ResolveSeatNodeId(doc.Nodes, "Darkhold");
        Assert.Equal(darkhold.Id, seatId);
    }
}

public class MarchMapSeederSelfHealTests : IClassFixture<DA_Business.Tests.Fixtures.DatabaseFixture>
{
    private readonly DA_Business.Tests.Fixtures.DatabaseFixture _fixture;

    public MarchMapSeederSelfHealTests(DA_Business.Tests.Fixtures.DatabaseFixture fixture) => _fixture = fixture;

    private static readonly System.Text.Json.JsonSerializerOptions CamelCase = new()
    {
        PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
    };

    [Fact]
    public async Task EnsureInitialized_OverwritesLegacyMapWithoutSeedVersion()
    {
        await using var ctx = _fixture.CreateContext();
        ctx.MarchMapStates.RemoveRange(ctx.MarchMapStates);
        await ctx.SaveChangesAsync();

        // Legacy payload from before versioning: a tiny procedural-style map, seedVersion absent (= 0).
        ctx.MarchMapStates.Add(new DA_DataAccess.BaronyData.MarchMapState
        {
            Id = DA_DataAccess.BaronyData.MarchMapState.GlobalId,
            PayloadJson = "{\"imageUrl\":\"/maps/old.jpg\",\"nodes\":[{\"id\":\"a\",\"label\":\"A\"}],\"routes\":[]}",
        });
        await ctx.SaveChangesAsync();

        await DA_Business.Repository.MarchMapRepos.MarchMapSeeder.EnsureInitializedAsync(ctx);

        var row = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions
            .FirstAsync(ctx.MarchMapStates, x => x.Id == DA_DataAccess.BaronyData.MarchMapState.GlobalId);
        var doc = System.Text.Json.JsonSerializer.Deserialize<MarchMapDocument>(row.PayloadJson, CamelCase)!;

        Assert.Equal(EasternMarchMapDefaults.CurrentSeedVersion, doc.SeedVersion);
        Assert.Equal(55, doc.Nodes.Count);
        Assert.Equal(74, doc.Routes.Count);
    }

    [Fact]
    public async Task EnsureInitialized_PreservesMapAlreadyAtCurrentSeedVersion()
    {
        await using var ctx = _fixture.CreateContext();
        ctx.MarchMapStates.RemoveRange(ctx.MarchMapStates);
        await ctx.SaveChangesAsync();

        // An MG-customized single-node map already stamped with the current seed version must not be reset.
        var customized = new MarchMapDocument
        {
            SeedVersion = EasternMarchMapDefaults.CurrentSeedVersion,
            ImageUrl = "/maps/custom.jpg",
            Nodes = { new MarchMapNode { Id = "only", Label = "Only", LordKey = "thaddeus" } },
        };
        ctx.MarchMapStates.Add(new DA_DataAccess.BaronyData.MarchMapState
        {
            Id = DA_DataAccess.BaronyData.MarchMapState.GlobalId,
            PayloadJson = System.Text.Json.JsonSerializer.Serialize(customized, CamelCase),
        });
        await ctx.SaveChangesAsync();

        await DA_Business.Repository.MarchMapRepos.MarchMapSeeder.EnsureInitializedAsync(ctx);

        var row = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions
            .FirstAsync(ctx.MarchMapStates, x => x.Id == DA_DataAccess.BaronyData.MarchMapState.GlobalId);
        var doc = System.Text.Json.JsonSerializer.Deserialize<MarchMapDocument>(row.PayloadJson, CamelCase)!;

        Assert.Single(doc.Nodes);
        Assert.Equal("only", doc.Nodes[0].Id);
    }
}
