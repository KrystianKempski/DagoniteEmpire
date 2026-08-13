using DA_Common.Barony;

namespace DA_Business.Tests.Barony;

public class BaronyCalendarFormulasTests
{
    [Fact]
    public void SeasonOrder_StartsAtSpring()
    {
        Assert.Equal(new[] { "Spring", "Summer", "Fall", "Winter" }, BaronyCalendarFormulas.SeasonOrder);
    }

    [Theory]
    [InlineData("Spring", "Summer", 625, 625)]
    [InlineData("Summer", "Fall", 625, 625)]
    [InlineData("Fall", "Winter", 625, 625)]
    [InlineData("Winter", "Spring", 625, 626)]
    [InlineData("Autumn", "Winter", 625, 625)]
    public void AdvanceOneTurn_CyclesAndBumpsYearOnSpring(
        string current, string expectedSeason, int year, int expectedYear)
    {
        var next = BaronyCalendarFormulas.AdvanceOneTurn(year, 1, 3, current);
        Assert.Equal(expectedSeason, next.Season);
        Assert.Equal(expectedYear, next.Year);
        Assert.Equal(4, next.TurnNumber);
    }

    [Theory]
    [InlineData(null, true)]
    [InlineData("Spring", true)]
    [InlineData("Summer", true)]
    [InlineData("Fall", true)]
    [InlineData("Autumn", true)]
    [InlineData("Winter", false)]
    public void FarmsProduceFood_FalseOnlyInWinter(string? season, bool expected) =>
        Assert.Equal(expected, BaronyCalendarFormulas.FarmsProduceFood(season));
}

public class VillagePpbFormulasSeasonTests
{
    [Fact]
    public void FarmFood_ZeroInWinter_OtherwiseByFertility()
    {
        Assert.Equal(1.5m, VillagePpbFormulas.FarmFoodForFertility(3, "Spring"));
        Assert.Equal(0m, VillagePpbFormulas.FarmFoodForFertility(3, "Winter"));
        Assert.Equal(1.5m, VillagePpbFormulas.FarmFoodForFertility(3));
    }

    [Fact]
    public void Compute_Winter_FoodIsOnlyPopulationDrain()
    {
        var spring = VillagePpbFormulas.Compute(2, 3, false, TownTaxRates.Defaults, "Spring");
        var winter = VillagePpbFormulas.Compute(2, 3, false, TownTaxRates.Defaults, "Winter");

        Assert.Equal(1.5m - 2m, spring[Ppb.Food]);
        Assert.Equal(-2m, winter[Ppb.Food]);
        Assert.Equal(spring[Ppb.Economy], winter[Ppb.Economy]);
    }
}
