using DA_Common;
using DA_DataAccess;

namespace DA_Business.Tests;

public class DemoGmCharacterAccessTests
{
    [Fact]
    public void DemoGm_DoesNotHaveGlobalCharacterAccess()
    {
        Assert.False(SD.HasGlobalCharacterAccess(SD.DemoGmUserName, isAdminOrMg: true));
        Assert.False(SD.HasGlobalCharacterAccess(SD.DemoBaronUserName, isAdminOrMg: false));
        Assert.True(SD.HasGlobalCharacterAccess("real-gm", isAdminOrMg: true));
        Assert.False(SD.HasGlobalCharacterAccess("player", isAdminOrMg: false));
    }

    [Fact]
    public void DemoGm_CanAccessOnlySelectedDemoBaron()
    {
        var demoGm = new UserInfo
        {
            UserName = SD.DemoGmUserName,
            IsAdminOrMG = true,
            SelectedCharacterId = 42,
        };

        Assert.True(demoGm.IsDemoSession);
        Assert.False(demoGm.HasGlobalCharacterAccess);
        Assert.True(demoGm.CanAccessCharacter(42));
        Assert.False(demoGm.CanAccessCharacter(1));
        Assert.False(demoGm.CanAccessCharacter(0));
    }

    [Fact]
    public void RealGm_CanAccessAnyCharacter()
    {
        var gm = new UserInfo
        {
            UserName = "Krystian",
            IsAdminOrMG = true,
            SelectedCharacterId = 42,
        };

        Assert.True(gm.HasGlobalCharacterAccess);
        Assert.True(gm.CanAccessCharacter(1));
        Assert.True(gm.CanAccessCharacter(999));
    }
}
