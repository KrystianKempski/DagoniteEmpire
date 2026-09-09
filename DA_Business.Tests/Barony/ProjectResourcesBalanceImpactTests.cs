using DA_Common.Barony;
using DA_Models.BaronyModels;

namespace DA_Business.Tests.Barony;

public class ProjectResourcesBalanceImpactTests
{
    [Fact]
    public void ResourcesBalanceImpact_UsesOnlyThisTurnDelta()
    {
        var project = new BaronyProjectDTO
        {
            Status = ProjectStatus.ResourceAllocation,
            AllocatedAtTurnStart = Vec(treasury: 10m, production: 10m),
            Allocated = Vec(treasury: 10m, production: 10m),
        };

        var impact = project.ResourcesBalanceImpact();
        Assert.Equal(0m, impact[Ppb.Treasury]);
        Assert.Equal(0m, impact[Ppb.Production]);
    }

    [Fact]
    public void ResourcesBalanceImpact_ShowsNewFundingAfterPriorTurn()
    {
        var project = new BaronyProjectDTO
        {
            Status = ProjectStatus.ResourceAllocation,
            AllocatedAtTurnStart = Vec(treasury: 10m, production: 10m),
            Allocated = Vec(treasury: 20m, production: 15m),
        };

        var impact = project.ResourcesBalanceImpact();
        Assert.Equal(-10m, impact[Ppb.Treasury]);
        Assert.Equal(-5m, impact[Ppb.Production]);
    }

    [Fact]
    public void ResourcesBalanceImpact_ClearAfterPriorTurn_ShowsRefund()
    {
        // Clear refunds full Allocated to stocks; balance must credit the prior-turn
        // portion so Σ still matches (that portion already sat in PreviousTurnStock).
        var project = new BaronyProjectDTO
        {
            Status = ProjectStatus.ResourceAllocation,
            AllocatedAtTurnStart = Vec(treasury: 10m, production: 10m),
            Allocated = new PpbVector(),
        };

        var impact = project.ResourcesBalanceImpact();
        Assert.Equal(10m, impact[Ppb.Treasury]);
        Assert.Equal(10m, impact[Ppb.Production]);
    }

    [Fact]
    public void ResourcesBalanceImpact_SameTurnAllocation_ShowsFullCost()
    {
        var project = new BaronyProjectDTO
        {
            Status = ProjectStatus.ResourceAllocation,
            AllocatedAtTurnStart = new PpbVector(),
            Allocated = Vec(treasury: 10m, production: 10m),
        };

        var impact = project.ResourcesBalanceImpact();
        Assert.Equal(-10m, impact[Ppb.Treasury]);
        Assert.Equal(-10m, impact[Ppb.Production]);
    }

    private static PpbVector Vec(decimal treasury = 0m, decimal production = 0m)
    {
        var v = new PpbVector();
        v[Ppb.Treasury] = treasury;
        v[Ppb.Production] = production;
        return v;
    }
}
