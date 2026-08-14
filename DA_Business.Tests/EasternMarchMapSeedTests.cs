using DA_Common.Barony;

namespace DA_Business.Tests;

public class EasternMarchMapSeedTests
{
    [Fact]
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
