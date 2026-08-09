using DA_Common.Barony;

namespace DA_Business.Tests.Barony;

public class BaronAudienceChapterTests
{
    [Theory]
    [InlineData(625, "Spring", "Grain petition", "Audience 625, Spring, Grain petition")]
    [InlineData(625, "Fall", "Taxes", "Audience 625, Autumn, Taxes")]
    [InlineData(626, "Winter", "  Border dispute  ", "Audience 626, Winter, Border dispute")]
    [InlineData(1, null, null, "Audience 1, Spring, Untitled")]
    public void FormatName_BuildsSearchableTitle(int year, string? season, string? title, string expected) =>
        Assert.Equal(expected, BaronAudienceChapter.FormatName(year, season, title));
}
