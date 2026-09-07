using DA_Business.Services;
using DA_Business.Tests.Fixtures;
using DA_Common;
using DA_DataAccess;
using DA_DataAccess.BaronyData;
using DA_DataAccess.CharacterClasses;
using Microsoft.AspNetCore.Identity;

namespace DA_Business.Tests.Notifications;

public class NotificationRecipientLookupTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;
    private readonly NotificationRecipientLookup _lookup;

    public NotificationRecipientLookupTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
        _fixture.ResetDatabase();
        _lookup = new NotificationRecipientLookup(_fixture.DbContextFactory);
    }

    [Fact]
    public async Task UserIdsForUserNames_MatchesIgnoringCase_AndSkipsDemoAccounts()
    {
        SeedUser("id-player", "player");
        SeedUser("id-demo", SD.DemoBaronUserName);

        var ids = await _lookup.UserIdsForUserNames(new[] { "PLAYER", SD.DemoBaronUserName, null, "  " });

        Assert.Equal(new[] { "id-player" }, ids);
    }

    [Fact]
    public async Task UserIdsForUserNames_ReturnsEmpty_WhenNothingUsable()
    {
        Assert.Empty(await _lookup.UserIdsForUserNames(new string?[] { null, "" }));
        Assert.Empty(await _lookup.UserIdsForUserNames(Array.Empty<string>()));
    }

    [Fact]
    public async Task UserIdForBarony_ResolvesThroughBaronCharacter()
    {
        SeedUser("id-baron", "duke");
        var characterId = SeedCharacter("duke");
        var baronyId = SeedBarony(characterId);

        Assert.Equal("id-baron", await _lookup.UserIdForBarony(baronyId));
    }

    [Fact]
    public async Task UserIdForBarony_ReturnsNull_ForUnknownBarony()
    {
        Assert.Null(await _lookup.UserIdForBarony(0));
        Assert.Null(await _lookup.UserIdForBarony(4242));
    }

    [Fact]
    public async Task GameMasterUserIds_ReturnsRealGmsOnly()
    {
        SeedUser("id-gm", "gm");
        SeedUser("id-demo-gm", SD.DemoGmUserName);
        SeedUser("id-player", "player");
        SeedRole("role-gm", SD.Role_GameMaster);
        SeedRole("role-player", SD.Role_HeroPlayer);
        SeedUserRole("id-gm", "role-gm");
        SeedUserRole("id-demo-gm", "role-gm");
        SeedUserRole("id-player", "role-player");

        var ids = await _lookup.GameMasterUserIds();

        Assert.Equal(new[] { "id-gm" }, ids);
    }

    private void SeedUser(string id, string userName)
    {
        using var ctx = _fixture.CreateContext();
        ctx.ApplicationUsers.Add(new ApplicationUser
        {
            Id = id,
            UserName = userName,
            NormalizedUserName = userName.ToUpperInvariant(),
        });
        ctx.SaveChanges();
    }

    private void SeedRole(string id, string name)
    {
        using var ctx = _fixture.CreateContext();
        ctx.Roles.Add(new IdentityRole { Id = id, Name = name, NormalizedName = name.ToUpperInvariant() });
        ctx.SaveChanges();
    }

    private void SeedUserRole(string userId, string roleId)
    {
        using var ctx = _fixture.CreateContext();
        ctx.UserRoles.Add(new IdentityUserRole<string> { UserId = userId, RoleId = roleId });
        ctx.SaveChanges();
    }

    private int SeedCharacter(string userName)
    {
        using var ctx = _fixture.CreateContext();
        var profession = new Profession { Name = "Noble", Description = "", RelatedAttributeName = "" };
        ctx.Professions.Add(profession);
        ctx.SaveChanges();

        var character = new Character
        {
            UserName = userName,
            NPCName = "Baron " + userName,
            ProfessionId = profession.Id,
        };
        ctx.Characters.Add(character);
        ctx.SaveChanges();
        return character.Id;
    }

    private int SeedBarony(int characterId)
    {
        using var ctx = _fixture.CreateContext();
        var barony = new DA_DataAccess.BaronyData.Barony { CharacterId = characterId, Name = "Darkhold" };
        ctx.Baronies.Add(barony);
        ctx.SaveChanges();
        return barony.Id;
    }
}
