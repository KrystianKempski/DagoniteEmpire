using DA_Common.Barony;

namespace DA_Business.Tests.Barony;

public class CourtCommanderFormulasTests
{
    [Fact]
    public void Catalog_LoadsFromEmbeddedJson()
    {
        Assert.NotEmpty(CourtCommanderCatalog.All);
        Assert.NotEmpty(CourtCommanderCatalog.TierRequirements);

        var trunkT1 = CourtCommanderCatalog.FindTierRequirement(CourtCommanderBranch.Trunk, 1);
        Assert.NotNull(trunkT1);
        Assert.Contains(trunkT1!.Requirements,
            r => r.IsMain && r.SkillKey == CourtMainSkill.Command && r.Min == 4);

        var shockT1 = CourtCommanderCatalog.FindTierRequirement(CourtCommanderBranch.Shock, 1);
        Assert.NotNull(shockT1);
        Assert.Contains(shockT1!.Requirements,
            r => !r.IsMain && r.SkillKey == CourtSecondarySkill.AnimalHandlingRiding && r.Min == 2);
    }

    [Fact]
    public void TierRequirements_GateUnlock()
    {
        var sheet = CourtCharacterSheet.CreateDefault();
        sheet.Main[CourtMainSkill.Command] = 3; // below the Trunk T1 minimum of 4
        sheet.CommanderXp = 5;
        Assert.False(CourtCommanderFormulas.CanUnlock(sheet, "hold-the-line", out _));

        sheet.Main[CourtMainSkill.Command] = 4;
        Assert.True(CourtCommanderFormulas.CanUnlock(sheet, "hold-the-line", out var reason), reason);
    }

    [Fact]
    public void Tier2_RequiresTwoTier1InTrunkOrBranch()
    {
        var sheet = CourtCharacterSheet.CreateDefault();
        sheet.Main[CourtMainSkill.Command] = 8;
        sheet.CommanderXp = 20;
        Assert.False(CourtCommanderFormulas.CanUnlock(sheet, "fighting-withdrawal", out _));

        Assert.True(CourtCommanderFormulas.TryUnlock(sheet, "hold-the-line", out var e1), e1);
        Assert.True(CourtCommanderFormulas.TryUnlock(sheet, "discipline-boost", out var e2), e2);
        Assert.True(CourtCommanderFormulas.CanUnlock(sheet, "fighting-withdrawal", out var e3), e3);
    }

    [Fact]
    public void BattleCx_ScalesWithCommand()
    {
        var sheet = CourtCharacterSheet.CreateDefault();
        sheet.Main[CourtMainSkill.Command] = 10; // ×1.3
        var cx = CourtCommanderFormulas.ComputeBattleCx(sheet, damageDealt: 50, engagedRounds: 2, kills: 1, damageTaken: 0);
        // raw = 10 + 2 + 2 = 14; ×1.3 = 18.2 → 18
        Assert.Equal(18, cx);
    }

    [Fact]
    public void ComputeBonuses_IsEmpty_EffectsNotWiredYet()
    {
        var sheet = CourtCharacterSheet.CreateDefault();
        sheet.Main[CourtMainSkill.Command] = 8;
        sheet.CommanderXp = 10;
        Assert.True(CourtCommanderFormulas.TryUnlock(sheet, "hold-the-line", out _));

        var bonuses = CourtCommanderFormulas.ComputeBonuses(sheet, unitHasMount: true, unitHasShield: true);
        Assert.Equal(0, bonuses.CommanderAttack);
        Assert.Equal(0, bonuses.CommanderDefense);
        Assert.Equal(0, bonuses.OtherHp);
        Assert.False(bonuses.ThunderCharge);
    }

    [Fact]
    public void Normalize_StripsUnknownAbilityKeys()
    {
        var sheet = CourtCharacterSheet.CreateDefault();
        sheet.UnlockedCommanderAbilities.Add("voice-of-command"); // removed legacy key
        sheet.UnlockedCommanderAbilities.Add("hold-the-line");
        sheet.Normalize();

        Assert.DoesNotContain("voice-of-command", sheet.UnlockedCommanderAbilities);
        Assert.Contains("hold-the-line", sheet.UnlockedCommanderAbilities);
    }

    [Fact]
    public void MaxTwoTier3PerCaptain()
    {
        var sheet = CourtCharacterSheet.CreateDefault();
        sheet.Main[CourtMainSkill.Command] = 10;
        sheet.Secondary =
        [
            new CourtSecondaryEntry { Key = CourtSecondarySkill.Athletics, Value = 6 },
            new CourtSecondaryEntry { Key = CourtSecondarySkill.AnimalHandlingRiding, Value = 6 },
        ];
        sheet.CommanderXp = 100;

        Assert.True(CourtCommanderFormulas.TryUnlock(sheet, "counter-charge", out var a), a);
        Assert.True(CourtCommanderFormulas.TryUnlock(sheet, "shock-lance", out var b), b);
        Assert.True(CourtCommanderFormulas.TryUnlock(sheet, "mounted-superiority", out var c), c);
        Assert.True(CourtCommanderFormulas.TryUnlock(sheet, "unbroken-momentum", out var d), d);
        Assert.True(CourtCommanderFormulas.TryUnlock(sheet, "flying-start", out var e), e);
        Assert.True(CourtCommanderFormulas.TryUnlock(sheet, "thunder-charge", out var f), f);

        // Third Tier-3 is blocked by the max of two per captain.
        Assert.False(CourtCommanderFormulas.CanUnlock(sheet, "overrun", out _));
    }
}
