using DA_Business.Repository.BaronyRepos;
using DA_Business.Tests.Fixtures;
using DA_Common;
using DA_Common.Barony;
using DA_DataAccess.CharacterClasses;
using DA_DataAccess.Data;

namespace DA_Business.Tests;

public class DemoBaronyResourceStartTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;

    public DemoBaronyResourceStartTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task CreateForCharacter_DarkholdDemoStartsWithRequestedStocks()
    {
        await using var ctx = _fixture.CreateContext();

        var profession = new Profession
        {
            Name = "Warrior",
            RelatedAttributeName = "Strength",
            IsApproved = true,
        };
        ctx.Professions.Add(profession);
        await ctx.SaveChangesAsync();

        var character = new Character
        {
            UserName = "demo-baron",
            NPCName = "Aldric Emberfall",
            NPCType = SD.NPCType.Duke,
            IsApproved = true,
            ProfessionId = profession.Id,
        };

        ctx.Characters.Add(character);
        await ctx.SaveChangesAsync();

        var repo = new BaronyRepository(_fixture.DbContextFactory);
        var barony = await repo.CreateForCharacter(character.Id, DarkholdSeeder.BaronyName, "Demo barony", "darkhold");

        Assert.Equal(5m, barony.FoodInGranaries);
        Assert.Equal(50m, barony.ResourceStocks[Ppb.Production]);
        Assert.Equal(50m, barony.ResourceStocks[Ppb.Defense]);
        Assert.Equal(100m, barony.TreasuryGold);
        Assert.Equal(100m, barony.ResourceStocks[Ppb.Treasury]);
    }
}
