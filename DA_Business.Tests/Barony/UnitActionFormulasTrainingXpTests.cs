using DA_Common.Barony;

namespace DA_Business.Tests.Barony;

public class UnitActionFormulasTrainingXpTests
{
    [Fact]
    public void CommandStrategyFromSheet_ReadsCommandAndStrategy()
    {
        var sheet = CourtCharacterSheet.CreateDefault();
        sheet.Main[CourtMainSkill.Command] = 8;
        sheet.Secondary.Add(new CourtSecondaryEntry
        {
            Key = CourtSecondarySkill.StrategyTactics,
            Value = 4,
        });

        var (cmd, strat) = UnitActionFormulas.CommandStrategyFromSheet(sheet);

        Assert.Equal(8, cmd);
        Assert.Equal(4, strat);
    }

    [Theory]
    [InlineData(UnitCaptainKind.CourtSheet, 8, 4, 100, 12)]
    [InlineData(UnitCaptainKind.LinkedCharacter, 8, 4, 100, 6)]
    [InlineData(UnitCaptainKind.Baron, 8, 4, 100, 6)]
    [InlineData(UnitCaptainKind.Baron, 8, 4, 50, 3)]
    [InlineData(UnitCaptainKind.None, 8, 4, 100, 0)]
    public void TrainingXpFromSheet_MatchesKindAndBt(
        UnitCaptainKind kind, int command, int strategy, int bt, int expected)
    {
        var sheet = CourtCharacterSheet.CreateDefault();
        sheet.Main[CourtMainSkill.Command] = command;
        sheet.Secondary.Add(new CourtSecondaryEntry
        {
            Key = CourtSecondarySkill.StrategyTactics,
            Value = strategy,
        });

        Assert.Equal(expected, UnitActionFormulas.TrainingXpFromSheet(kind, sheet, bt));
    }

    [Fact]
    public void KindForCourtier_DistinguishesLinkedCharacter()
    {
        Assert.Equal(UnitCaptainKind.LinkedCharacter, UnitActionFormulas.KindForCourtier(true));
        Assert.Equal(UnitCaptainKind.CourtSheet, UnitActionFormulas.KindForCourtier(false));
    }
}
