using DA_Common;

namespace DA_Business.Tests;

public class RollServiceTests : IDisposable
{
    public RollServiceTests()
    {
        RollService.TestDiceOverride = null;
    }

    public void Dispose()
    {
        RollService.TestDiceOverride = null;
    }

    [Fact]
    public void MakeRollTest_SucceedsWhenTotalMeetsDc()
    {
        RollService.TestDiceOverride = () => (2, 2, 2);

        var result = RollService.MakeRollTest(10, 4);

        Assert.True(result.Success);
        Assert.Contains("10", result.Text);
        Assert.Contains("Sukces!", result.Text);
    }

    [Fact]
    public void MakeRollTest_FailsWhenTotalBelowDc()
    {
        RollService.TestDiceOverride = () => (1, 1, 1);

        var result = RollService.MakeRollTest(10, 4);

        Assert.False(result.Success);
        Assert.Contains("Porażka!", result.Text);
    }

    [Fact]
    public void MakeOppositeRollTest_WithBonuses_IncludesBothSidesInText()
    {
        RollService.TestDiceOverride = null;
        var diceQueue = new Queue<(int, int, int)>(new[]
        {
            (2, 2, 2),
            (1, 1, 1),
        });
        RollService.TestDiceOverride = () => diceQueue.Dequeue();

        var result = RollService.MakeOppositeRollTest(
            "Attacker",
            new List<Pair<string, int>> { new("skill", 10), new("flank", 2) },
            "Defender",
            new List<Pair<string, int>> { new("skill", 8) });

        Assert.Contains("Attacker:", result.Text);
        Assert.Contains("+10 (skill)", result.Text);
        Assert.Contains("+10 (skill) +2 (flank)", result.Text);
        Assert.Contains("+2 (flank)", result.Text);
        Assert.Contains("Defender:", result.Text);
        Assert.Contains("+8 (skill)", result.Text);
        Assert.True(result.FirstSideWins);
    }

    [Fact]
    public void MakeOppositeRollTest_SecondSideWinsWhenHigher()
    {
        RollService.TestDiceOverride = () => (1, 1, 1);

        var result = RollService.MakeOppositeRollTest("Low", 2, "High", 20);

        Assert.False(result.FirstSideWins);
        Assert.Contains("High", result.Text);
        Assert.Contains("wygrywa!", result.Text);
    }

    [Theory]
    [InlineData(17, true)]
    [InlineData(18, true)]
    [InlineData(16, false)]
    [InlineData(3, false)]
    public void IsCriticalSuccess_IsTrueOnlyForSeventeenOrEighteen(int diceSum, bool expected)
    {
        Assert.Equal(expected, RollService.IsCriticalSuccess(diceSum));
    }

    [Fact]
    public void RollD6_ReturnsValuesFromOneThroughSix()
    {
        var seen = new HashSet<int>();
        for (var i = 0; i < 2000; i++)
        {
            seen.Add(RollService.RollD6());
            if (seen.Count == 6)
                break;
        }

        Assert.Equal(new[] { 1, 2, 3, 4, 5, 6 }, seen.OrderBy(v => v));
    }

    [Fact]
    public void RollDice_CanReachCriticalHitSum()
    {
        RollService.TestDiceOverride = null;
        var maxSum = 0;
        for (var i = 0; i < 5000; i++)
        {
            maxSum = Math.Max(maxSum, RollService.RollDice().Sum);
            if (maxSum >= 17)
                break;
        }

        Assert.True(maxSum >= 17, $"Expected sum >= 17 with true 3d6, got max {maxSum}");
    }

    [Fact]
    public void MakeOppositeRollTest_TieGoesToFirstSide()
    {
        var call = 0;
        RollService.TestDiceOverride = () =>
        {
            call++;
            return (2, 2, 2);
        };

        var result = RollService.MakeOppositeRollTest("Alpha", 5, "Beta", 5);

        Assert.True(result.IsTie);
        Assert.True(result.FirstSideWins);
        Assert.Contains("Remis!", result.Text);
        Assert.Contains("Alpha", result.Text);
    }
}
