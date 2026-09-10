using DA_Common.Barony;

namespace DA_Business.Tests.Barony;

public class ResourceStockChangeLogTests
{
    [Fact]
    public void Describe_FormatsIncreaseAndDecrease()
    {
        var before = new PpbVector();
        before[Ppb.Production] = 20m;
        before[Ppb.Food] = 10m;
        var after = before.Clone();
        after[Ppb.Production] = 33m;
        after[Ppb.Food] = 7m;

        var lines = ResourceStockChangeLog.Describe(before, after);

        Assert.Equal(
            new[]
            {
                "Food decreased by 3 from 10 to 7",
                "Production increased by 13 from 20 to 33",
            },
            lines);
    }

    [Fact]
    public void DescribeCompact_UsesArrowAndDelta()
    {
        var before = new PpbVector();
        before[Ppb.Treasury] = 100m;
        var after = before.Clone();
        after[Ppb.Treasury] = 85m;

        var line = Assert.Single(ResourceStockChangeLog.DescribeCompact(before, after));
        Assert.Equal("Treasury/Gold: 100 → 85 (Δ -15)", line);
    }

    [Fact]
    public void Summarize_ReturnsNullWhenUnchanged()
    {
        var stocks = new PpbVector();
        stocks[Ppb.Production] = 5m;
        var (summary, details) = ResourceStockChangeLog.Summarize(stocks, stocks.Clone());
        Assert.Null(summary);
        Assert.Null(details);
    }

    [Fact]
    public void DescribeAdditive_ListsNonZeroShortNames()
    {
        var additive = new PpbVector();
        additive[Ppb.Production] = 13m;
        additive[Ppb.Treasury] = -5m;
        Assert.Equal("Prod +13, Gold -5", ResourceStockChangeLog.DescribeAdditive(additive));
    }
}
