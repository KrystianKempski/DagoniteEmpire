using DA_Common;
using static DA_Common.SD;

namespace DA_Business.Tests;

public class WoundsPenaltyTests
{
    [Theory]
    [InlineData(Wounds.Severity.Light, false, 1)]
    [InlineData(Wounds.Severity.Light, true, 0)]
    [InlineData(Wounds.Severity.Moderate, false, 3)]
    [InlineData(Wounds.Severity.Moderate, true, 1)]
    [InlineData(Wounds.Severity.Heavy, false, 7)]
    [InlineData(Wounds.Severity.Heavy, true, 3)]
    [InlineData(Wounds.Severity.Critical, false, 12)]
    [InlineData(Wounds.Severity.Critical, true, 5)]
    [InlineData(Wounds.Severity.Deadly, false, 20)]
    [InlineData(Wounds.Severity.Deadly, true, 20)]
    public void GetPenaltyFromSeverity_MatchesPlayerWoundPenalty(string severity, bool isIgnored, int expected)
    {
        Assert.Equal(expected, Wounds.GetPenaltyFromSeverity(severity, isIgnored));
    }
}
