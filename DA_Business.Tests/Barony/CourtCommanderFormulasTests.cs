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
    public void ComputeBonuses_WiresFlatPassives()
    {
        var sheet = CourtCharacterSheet.CreateDefault();
        sheet.Main[CourtMainSkill.Command] = 8;
        sheet.CommanderXp = 20;
        Assert.True(CourtCommanderFormulas.TryUnlock(sheet, "hold-the-line", out _));   // +8 HP
        Assert.True(CourtCommanderFormulas.TryUnlock(sheet, "discipline-boost", out _)); // 2nd T1 (gate)
        Assert.True(CourtCommanderFormulas.TryUnlock(sheet, "march-cadence", out _));   // +1 Move

        var bonuses = CourtCommanderFormulas.ComputeBonuses(sheet, unitHasMount: true, unitHasShield: true);
        Assert.Equal(8, bonuses.OtherHp);
        Assert.Equal(1, bonuses.OtherMove);
        Assert.Equal(1, bonuses.OtherDiscipline);
        Assert.Equal(0, bonuses.CommanderAttack);
        Assert.False(bonuses.ThunderCharge);
    }

    [Fact]
    public void ComputeBonuses_Phase2_WiresContextFlags()
    {
        var sheet = CourtCharacterSheet.CreateDefault();
        sheet.Main[CourtMainSkill.Command] = 8;
        sheet.Main[CourtMainSkill.Shooting] = 6;
        sheet.Main[CourtMainSkill.Melee] = 6;
        sheet.Secondary =
        [
            new CourtSecondaryEntry { Key = CourtSecondarySkill.Acrobatics, Value = 3 },
            new CourtSecondaryEntry { Key = CourtSecondarySkill.Athletics, Value = 3 },
            new CourtSecondaryEntry { Key = CourtSecondarySkill.AnimalHandlingRiding, Value = 3 },
            new CourtSecondaryEntry { Key = CourtSecondarySkill.Observation, Value = 3 },
        ];
        sheet.Main[CourtMainSkill.Deceit] = 6;
        sheet.CommanderXp = 100;
        Assert.True(CourtCommanderFormulas.TryUnlock(sheet, "drill-shot", out var a), a);
        Assert.True(CourtCommanderFormulas.TryUnlock(sheet, "counter-charge", out var b), b);
        Assert.True(CourtCommanderFormulas.TryUnlock(sheet, "riveted-plate", out var c), c);
        Assert.True(CourtCommanderFormulas.TryUnlock(sheet, "initiative-boost", out var d), d);

        var bonuses = CourtCommanderFormulas.ComputeBonuses(sheet, unitHasMount: false, unitHasShield: false);
        Assert.True(bonuses.DrillShot);
        Assert.True(bonuses.CounterCharge);
        Assert.Equal(1, bonuses.PierceIgnore);
        Assert.Equal(2, bonuses.OtherInitiative);
    }

    [Fact]
    public void ComputeBonuses_Phase3_WiresChargeFlags()
    {
        var sheet = CourtCharacterSheet.CreateDefault();
        sheet.Main[CourtMainSkill.Command] = 10;
        sheet.Secondary =
        [
            new CourtSecondaryEntry { Key = CourtSecondarySkill.Athletics, Value = 6 },
            new CourtSecondaryEntry { Key = CourtSecondarySkill.AnimalHandlingRiding, Value = 6 },
        ];
        sheet.CommanderXp = 100;
        Assert.True(CourtCommanderFormulas.TryUnlock(sheet, "shock-lance", out var a), a);        // T1
        Assert.True(CourtCommanderFormulas.TryUnlock(sheet, "wedge", out var b), b);              // T1
        Assert.True(CourtCommanderFormulas.TryUnlock(sheet, "unbroken-momentum", out var c), c);  // T2
        Assert.True(CourtCommanderFormulas.TryUnlock(sheet, "mounted-superiority", out var m), m); // T2 (T3 gate)
        Assert.True(CourtCommanderFormulas.TryUnlock(sheet, "thunder-charge", out var d), d);     // T3
        Assert.True(CourtCommanderFormulas.TryUnlock(sheet, "flying-start", out var e), e);       // T3

        var bonuses = CourtCommanderFormulas.ComputeBonuses(sheet, unitHasMount: true, unitHasShield: false);
        Assert.True(bonuses.ShockLance);
        Assert.True(bonuses.Wedge);
        Assert.True(bonuses.UnbrokenMomentum);
        Assert.True(bonuses.ThunderCharge);
        Assert.True(bonuses.FlyingStart);
        Assert.Equal(3, bonuses.ChargeAttackBonus);   // Thunder Charge override
        Assert.Equal(3, bonuses.ChargeDamageBonus(mounted: true)); // 2 (thunder) + 1 (shock lance)
    }

    [Fact]
    public void ComputeBonuses_Phase4_WiresRangedFlags()
    {
        var sheet = CourtCharacterSheet.CreateDefault();
        sheet.Main[CourtMainSkill.Shooting] = 10;
        sheet.Secondary =
        [
            new CourtSecondaryEntry { Key = CourtSecondarySkill.Acrobatics, Value = 6 },
        ];
        sheet.CommanderXp = 100;
        Assert.True(CourtCommanderFormulas.TryUnlock(sheet, "snap-shot", out var a), a);        // T1
        Assert.True(CourtCommanderFormulas.TryUnlock(sheet, "drill-shot", out var b), b);       // T1
        Assert.True(CourtCommanderFormulas.TryUnlock(sheet, "long-shot", out var c), c);        // T2
        Assert.True(CourtCommanderFormulas.TryUnlock(sheet, "harassing-fire", out var d), d);   // T2 (T3 gate)
        Assert.True(CourtCommanderFormulas.TryUnlock(sheet, "extended-range", out var e), e);   // T3
        Assert.True(CourtCommanderFormulas.TryUnlock(sheet, "skirmish-screen", out var f), f);  // T3

        var bonuses = CourtCommanderFormulas.ComputeBonuses(sheet, unitHasMount: false, unitHasShield: false);
        Assert.True(bonuses.SnapShot);
        Assert.True(bonuses.LongShot);
        Assert.True(bonuses.HarassingFire);
        Assert.True(bonuses.ExtendedRange);
        Assert.True(bonuses.SkirmishScreen);
        Assert.False(bonuses.Enfilade);
    }

    [Fact]
    public void ComputeBonuses_Tier1Finishers_Wired()
    {
        var sheet = CourtCharacterSheet.CreateDefault();
        sheet.Main[CourtMainSkill.Command] = 4;   // trunk T1
        sheet.Main[CourtMainSkill.Deceit] = 5;     // cunning T1
        sheet.Main[CourtMainSkill.Shooting] = 5;   // skirmish T1
        sheet.Secondary =
        [
            new CourtSecondaryEntry { Key = CourtSecondarySkill.Observation, Value = 2 },
            new CourtSecondaryEntry { Key = CourtSecondarySkill.Acrobatics, Value = 2 },
        ];
        sheet.CommanderXp = 20;
        Assert.True(CourtCommanderFormulas.TryUnlock(sheet, "knife-in-the-dark", out var a), a);
        Assert.True(CourtCommanderFormulas.TryUnlock(sheet, "keep-facing", out var b), b);
        Assert.True(CourtCommanderFormulas.TryUnlock(sheet, "look-away", out var c), c);
        Assert.True(CourtCommanderFormulas.TryUnlock(sheet, "column-march", out var d), d);

        var bonuses = CourtCommanderFormulas.ComputeBonuses(sheet, unitHasMount: false, unitHasShield: false);
        Assert.True(bonuses.KnifeInTheDark);
        Assert.True(bonuses.KeepFacing);
        Assert.True(bonuses.LookAway);
        Assert.True(bonuses.ColumnMarch);
    }

    [Fact]
    public void LooseFiles_IsDisabled_CannotUnlock_AndHasNoEffect()
    {
        var sheet = CourtCharacterSheet.CreateDefault();
        sheet.Main[CourtMainSkill.Command] = 4;
        sheet.Main[CourtMainSkill.Shooting] = 5;
        sheet.Secondary = [new CourtSecondaryEntry { Key = CourtSecondarySkill.Acrobatics, Value = 2 }];
        sheet.CommanderXp = 20;

        Assert.True(CourtCommanderCatalog.IsDisabled("loose-files"));
        Assert.False(CourtCommanderFormulas.TryUnlock(sheet, "loose-files", out var e), e);

        // Even a captain who somehow already has the key gets no battle effect.
        sheet.UnlockedCommanderAbilities.Add("loose-files");
        var bonuses = CourtCommanderFormulas.ComputeBonuses(sheet, unitHasMount: false, unitHasShield: false);
        Assert.False(bonuses.LooseFiles);
    }

    [Fact]
    public void ComputeBonuses_Ironclad_WiresActiveFlag()
    {
        var sheet = CourtCharacterSheet.CreateDefault();
        sheet.UnlockedCommanderAbilities.Add("ironclad");

        var bonuses = CourtCommanderFormulas.ComputeBonuses(sheet, unitHasMount: false, unitHasShield: false);
        Assert.True(bonuses.Ironclad);
    }

    [Fact]
    public void ComputeBonuses_ShieldWall_OnlyWithShield()
    {
        var sheet = CourtCharacterSheet.CreateDefault();
        sheet.Main[CourtMainSkill.Melee] = 6;
        sheet.Secondary = [new CourtSecondaryEntry { Key = CourtSecondarySkill.Athletics, Value = 3 }];
        sheet.CommanderXp = 10;
        Assert.True(CourtCommanderFormulas.TryUnlock(sheet, "shield-wall-basics", out var e), e);

        Assert.Equal(1, CourtCommanderFormulas.ComputeBonuses(sheet, false, unitHasShield: true).CommanderDefense);
        Assert.Equal(0, CourtCommanderFormulas.ComputeBonuses(sheet, false, unitHasShield: false).CommanderDefense);
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

    /// <summary>
    /// The enemy commander path builds a sheet straight from selected keys (bypassing unlock gates)
    /// and re-derives token stats from these passive deltas — lock that mapping in.
    /// </summary>
    [Fact]
    public void ComputeBonuses_FromRawKeys_MapsEnemyPassiveDeltas()
    {
        var sheet = new CourtCharacterSheet
        {
            UnlockedCommanderAbilities =
            {
                "hold-the-line",
                "march-cadence",
                "killing-blow",
                "shield-wall-basics",
                "discipline-boost",
                "initiative-boost",
            },
        };

        var bonuses = CourtCommanderFormulas.ComputeBonuses(sheet, unitHasMount: false, unitHasShield: true);

        Assert.Equal(8, bonuses.OtherHp);          // hold-the-line
        Assert.Equal(1, bonuses.OtherMove);        // march-cadence
        Assert.Equal(1, bonuses.OtherDamageMelee); // killing-blow
        Assert.Equal(1, bonuses.CommanderDefense); // shield-wall-basics (enemies imply a shield)
        Assert.Equal(1, bonuses.OtherDiscipline);  // discipline-boost
        Assert.Equal(2, bonuses.OtherInitiative);  // initiative-boost
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
