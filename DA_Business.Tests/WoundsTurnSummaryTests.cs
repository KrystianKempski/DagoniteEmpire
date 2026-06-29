using DA_Common;

namespace DA_Business.Tests;

public class WoundsTurnSummaryTests
{
    [Theory]
    [InlineData(Wounds.Severity.Light, true)]
    [InlineData(Wounds.Severity.Moderate, true)]
    [InlineData(Wounds.Severity.Heavy, true)]
    [InlineData(Wounds.Severity.Critical, true)]
    [InlineData(Wounds.Severity.Deadly, true)]
    [InlineData(Wounds.Severity.Scars, false)]
    [InlineData("no", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void IsReportableInTurnSummary_FiltersBySeverity(string? severity, bool expected) =>
        Assert.Equal(expected, Wounds.IsReportableInTurnSummary(severity));
}
