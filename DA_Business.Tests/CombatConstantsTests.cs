using DA_Common;

namespace DA_Business.Tests;

public class CombatConstantsTests
{
    [Fact]
    public void WoundLocationAll_HasNoDuplicates()
    {
        Assert.Equal(Wounds.Location.All.Length, Wounds.Location.All.Distinct().Count());
    }

    [Fact]
    public void WoundLocationAll_MatchesLocationEnumCount()
    {
        var enumCount = Enum.GetValues<Wounds.LocationEnum>().Length;
        Assert.Equal(enumCount, Wounds.Location.All.Length);
    }

    [Fact]
    public void StateNamesAll_ContainsDead()
    {
        Assert.Contains(States.Names.Dead, States.Names.All);
    }

    [Fact]
    public void StateNamesAll_HasNoDuplicates()
    {
        Assert.Equal(States.Names.All.Length, States.Names.All.Distinct().Count());
    }

    [Theory]
    [InlineData(States.Duration.SingleTurn, 1)]
    [InlineData(States.Duration.BleedingDefault, 10)]
    [InlineData(States.Duration.UntilResolved, 99)]
    [InlineData(States.Duration.Permanent, 999)]
    public void Durations_AreOrderedAndStable(int value, int expected)
    {
        Assert.Equal(expected, value);
    }
}
