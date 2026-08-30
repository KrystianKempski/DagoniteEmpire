using DA_Common.Barony;
using Xunit;

namespace DA_Business.Tests.Barony;

public class ProjectConstructionRulesTests
{
    [Theory]
    [InlineData(ProjectOutputKind.Building, true)]
    [InlineData(ProjectOutputKind.Improvement, true)]
    [InlineData(ProjectOutputKind.ForestClearing, true)]
    [InlineData(ProjectOutputKind.DecreeOrTechnology, false)]
    [InlineData(ProjectOutputKind.OneTimeResources, false)]
    public void RequiresMinTurn_MatchesConstructionKinds(string kind, bool expected)
        => Assert.Equal(expected, ProjectConstructionRules.RequiresMinTurn(kind));

    [Fact]
    public void ClampTurnsRemaining_EnforcesMinimum()
    {
        Assert.Equal(1, ProjectConstructionRules.ClampTurnsRemaining(0));
        Assert.Equal(2, ProjectConstructionRules.ClampTurnsRemaining(2));
    }

    [Theory]
    [InlineData(ProjectOutputKind.Building, 1)]
    [InlineData(ProjectOutputKind.Improvement, 1)]
    [InlineData(ProjectOutputKind.ForestClearing, 1)]
    [InlineData(ProjectOutputKind.Event, 0)]
    public void DefaultTurnsFor_ReturnsExpected(string kind, int expected)
        => Assert.Equal(expected, ProjectConstructionRules.DefaultTurnsFor(kind));
}
