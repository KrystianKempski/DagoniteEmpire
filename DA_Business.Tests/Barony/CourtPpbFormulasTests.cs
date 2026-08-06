using DA_Common.Barony;

namespace DA_Business.Tests.Barony;

public class CourtPpbFormulasTests
{
    [Fact]
    public void DefaultSheet_UsesBaseMainsWithoutSecondaries()
    {
        var sheet = CourtCharacterSheet.CreateDefault();
        var ppb = CourtPpbFormulas.Compute(sheet);

        // Knowledge 3 + 0 + 0
        Assert.Equal(3m, ppb[Ppb.Food]);
        // Administration 3
        Assert.Equal(3m, ppb[Ppb.Economy]);
        Assert.Equal(3m, ppb[Ppb.Stability]);
        // Magic 0
        Assert.Equal(0m, ppb[Ppb.Magic]);
        // Deceit 3 → Corruption -3
        Assert.Equal(-3m, ppb[Ppb.Corruption]);
        Assert.Equal(0m, ppb[Ppb.Treasury]);
    }

    [Fact]
    public void SumsMainPlusTwoSecondaries_PerAgreedMapping()
    {
        var sheet = CourtCharacterSheet.CreateDefault();
        sheet.Main[CourtMainSkill.Knowledge] = 5;
        sheet.Main[CourtMainSkill.Command] = 8;
        sheet.Secondary =
        [
            new CourtSecondaryEntry { Key = CourtSecondarySkill.FarmingHusbandry, Value = 4 },
            new CourtSecondaryEntry { Key = CourtSecondarySkill.AnimalHandlingRiding, Value = 2 },
            new CourtSecondaryEntry { Key = CourtSecondarySkill.StrategyTactics, Value = 6 },
            new CourtSecondaryEntry { Key = CourtSecondarySkill.GeographyNations, Value = 3 },
        ];

        var ppb = CourtPpbFormulas.Compute(sheet);

        Assert.Equal(11m, ppb[Ppb.Food]); // 5+4+2
        Assert.Equal(17m, ppb[Ppb.Defense]); // 8+6+3
    }

    [Fact]
    public void ClampRejectsOutOfRangeValues()
    {
        var sheet = CourtCharacterSheet.CreateDefault();
        sheet.Main[CourtMainSkill.Melee] = 1; // below min → 3
        sheet.Main[CourtMainSkill.Magic] = 99; // → 10
        sheet.Secondary =
        [
            new CourtSecondaryEntry { Key = CourtSecondarySkill.Trade, Value = 20 },
        ];
        sheet.Normalize();

        Assert.Equal(3, sheet.GetMain(CourtMainSkill.Melee));
        Assert.Equal(10, sheet.GetMain(CourtMainSkill.Magic));
        Assert.Equal(6, sheet.GetSecondary(CourtSecondarySkill.Trade));
    }

    [Fact]
    public void DomainOther_AddsToComputeTotal_NotToCompute()
    {
        var sheet = CourtCharacterSheet.CreateDefault();
        var patronage = new PpbVector();
        patronage[Ppb.Economy] = 4m;
        patronage[Ppb.Loyalty] = 2m;
        sheet.DomainOther =
        [
            new CourtDomainOtherSource
            {
                Name = "Patronage",
                Additive = patronage,
            },
        ];

        var fromSkills = CourtPpbFormulas.Compute(sheet);
        var total = CourtPpbFormulas.ComputeTotal(sheet);

        Assert.Equal(3m, fromSkills[Ppb.Economy]);
        Assert.Equal(7m, total[Ppb.Economy]);
        Assert.Equal(5m, total[Ppb.Loyalty]);
        Assert.Equal(4m, sheet.SumDomainOther()[Ppb.Economy]);
    }

    [Fact]
    public void MainOther_SumsPerSkill_DoesNotChangeDomainFromSkill()
    {
        var sheet = CourtCharacterSheet.CreateDefault();
        sheet.Main[CourtMainSkill.Administration] = 5;
        sheet.MainOther =
        [
            new CourtMainOtherSource { Name = "Seal", SkillKey = CourtMainSkill.Administration, Value = 2 },
            new CourtMainOtherSource { Name = "Aide", SkillKey = CourtMainSkill.Administration, Value = 1 },
        ];

        Assert.Equal(3, sheet.GetMainOtherSum(CourtMainSkill.Administration));
        Assert.Equal(5, sheet.GetMain(CourtMainSkill.Administration));
        Assert.Equal(5m, CourtPpbFormulas.Compute(sheet)[Ppb.Economy]);
    }

    [Fact]
    public void CombatSkills_UseAgreedSumsAndMaxes()
    {
        var sheet = CourtCharacterSheet.CreateDefault();
        sheet.Main[CourtMainSkill.Melee] = 6;
        sheet.Main[CourtMainSkill.Shooting] = 4;
        sheet.MainOther =
        [
            new CourtMainOtherSource { Name = "Blade", SkillKey = CourtMainSkill.Melee, Value = 1 },
        ];
        sheet.Secondary =
        [
            new CourtSecondaryEntry { Key = CourtSecondarySkill.Athletics, Value = 3 },
            new CourtSecondaryEntry { Key = CourtSecondarySkill.Acrobatics, Value = 5 },
            new CourtSecondaryEntry { Key = CourtSecondarySkill.Observation, Value = 2 },
        ];

        var combat = CourtCombatFormulas.Compute(sheet);

        Assert.Equal(10, combat.Attack); // Melee 7 + Athletics 3
        Assert.Equal(6, combat.Shooting); // Shooting 4 + Observation 2
        Assert.Equal(12, combat.Dodge); // max(7,4) + Acrobatics 5
        Assert.Equal(10, combat.Defence); // Melee 7 + Athletics 3
    }

    [Fact]
    public void CombatOther_SumsSeparatelyFromFromSkill()
    {
        var sheet = CourtCharacterSheet.CreateDefault();
        sheet.CombatOther =
        [
            new CourtCombatOtherSource { Name = "Blessing", SkillKey = CourtCombatSkill.Attack, Value = 2 },
            new CourtCombatOtherSource { Name = "Shield", SkillKey = CourtCombatSkill.Dodge, Value = 1 },
        ];

        var combat = CourtCombatFormulas.Compute(sheet);
        Assert.Equal(3, combat.Attack); // default melee 3 + athletics 0
        Assert.Equal(2, sheet.GetCombatOtherSum(CourtCombatSkill.Attack));
        Assert.Equal(1, sheet.GetCombatOtherSum(CourtCombatSkill.Dodge));
        Assert.Equal(0, sheet.GetCombatOtherSum(CourtCombatSkill.Shooting));
    }
}
