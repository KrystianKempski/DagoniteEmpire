using DA_Business.Repository.BaronyRepos;
using DA_Business.Tests.Fixtures;
using DA_Common;
using DA_Common.Barony;
using DA_DataAccess.CharacterClasses;
using DA_DataAccess.Data;
using Microsoft.EntityFrameworkCore;

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

        var repo = new BaronyRepository(_fixture.DbContextFactory, characters: null!);
        var barony = await repo.CreateForCharacter(character.Id, DarkholdSeeder.BaronyName, "Demo barony", "darkhold");

        Assert.Equal(5m, barony.FoodInGranaries);
        Assert.Equal(50m, barony.ResourceStocks[Ppb.Production]);
        Assert.Equal(50m, barony.ResourceStocks[Ppb.Defense]);
        Assert.Equal(100m, barony.TreasuryGold);
        Assert.Equal(100m, barony.ResourceStocks[Ppb.Treasury]);
    }

    [Fact]
    public async Task CreateForCharacter_DarkholdSeedsOpeningAudiences()
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
            UserName = "demo-baron-audiences",
            NPCName = "Aldric Emberfall",
            NPCType = SD.NPCType.Duke,
            IsApproved = true,
            ProfessionId = profession.Id,
        };
        ctx.Characters.Add(character);
        await ctx.SaveChangesAsync();

        var repo = new BaronyRepository(_fixture.DbContextFactory, characters: null!);
        var barony = await repo.CreateForCharacter(character.Id, DarkholdSeeder.BaronyName, "Demo barony", "darkhold");

        await using var verify = _fixture.CreateContext();
        var audiences = verify.BaronAudiences
            .Include(a => a.Exchanges)
            .Where(a => a.BaronyId == barony.Id)
            .ToList();

        var petitions = audiences.Where(a => a.Kind == BaronAudienceKind.Audience).ToList();
        Assert.Equal(7, petitions.Count);
        Assert.All(petitions, a =>
        {
            Assert.Equal(BaronAudienceStatus.Scheduled, a.Status);
            Assert.Equal(barony.TurnNumber, a.TurnNumber);
            var opening = Assert.Single(a.Exchanges);
            Assert.True(opening.IsFromPetitioner);
            Assert.False(string.IsNullOrWhiteSpace(opening.Body));
        });
        Assert.Contains(petitions, a => a.PetitionerName == "Brother Squall");

        var council = audiences.Where(a => a.Kind == BaronAudienceKind.Council).ToList();
        Assert.Equal(DarkholdOpeningCouncilTopics.All.Length, council.Count);
        Assert.All(council, a =>
        {
            Assert.Equal(BaronAudienceStatus.Scheduled, a.Status);
            Assert.Equal(barony.TurnNumber, a.TurnNumber);
            var opening = Assert.Single(a.Exchanges);
            Assert.True(opening.IsFromPetitioner);
            Assert.Equal(a.PetitionerName, opening.SpeakerName);
            Assert.False(string.IsNullOrWhiteSpace(opening.Body));
        });
        Assert.Contains(council, a => a.Title == "Kobold raid on a farm" && a.PetitionerName == "Sir Loren Birely");
        Assert.Contains(council, a => a.Title == "The debt to our senior" && a.PetitionerName == "Albus Durdwale");
        Assert.Contains(council, a => a.Title == "Paper, ink, and the luxury toll");
        Assert.Contains(council, a => a.Title == "The beast of Ravenclaw Wood" && a.PetitionerName == "Merdred Igrus");
        Assert.Contains(council, a => a.Title == "Lands stolen by Brie — the circle of Haga" && a.PetitionerName == "Merdred Igrus");
        Assert.Contains(council, a => a.Title == "The pirate wreck on the eastern cliffs"
            && a.Exchanges.Any(x => x.Body.Contains("pirate drakkar", StringComparison.Ordinal)));
    }
}
