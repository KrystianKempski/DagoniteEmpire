using DA_Common;
using DA_Models.ComponentModels;

namespace DA_Business.Tests;

public class SurroundedDefenceTests
{
    [Theory]
    [InlineData(SD.DefenceType.Armor, 2, 1)]
    [InlineData(SD.DefenceType.Armor, 4, 3)]
    [InlineData(SD.DefenceType.Dodge, 2, 2)]
    [InlineData(SD.DefenceType.Dodge, 3, 4)]
    [InlineData(SD.DefenceType.Dodge, 4, 6)]
    [InlineData(SD.DefenceType.Shield, 3, 4)]
    public void GetSurroundedDefencePenalty_ScalesWithDefenceTypeAndAttackerCount(
        string defenceType, int attackerCount, int expectedPenalty)
    {
        Assert.Equal(expectedPenalty, FightSequenceModel.GetSurroundedDefencePenalty(defenceType, attackerCount));
    }

    [Fact]
    public void GetSurroundedDefencePenalty_ReturnsZeroForSingleAttacker()
    {
        Assert.Equal(0, FightSequenceModel.GetSurroundedDefencePenalty(SD.DefenceType.Armor, 1));
    }

    [Fact]
    public void EnsureDefenceType_SetsDodgeWhenUnsetAndPropsMissing()
    {
        var fight = new FightSequenceModel(new DateModel(1, 1)) { DefenceType = string.Empty, Defender = new FighterModel() };
        fight.EnsureDefenceType();
        Assert.Equal(SD.DefenceType.Dodge, fight.DefenceType);
    }

    [Fact]
    public void EnsureDefenceType_DoesNotOverrideExistingChoice()
    {
        var fight = new FightSequenceModel(new DateModel(1, 1)) { DefenceType = SD.DefenceType.Armor, Defender = new FighterModel() };
        fight.EnsureDefenceType();
        Assert.Equal(SD.DefenceType.Armor, fight.DefenceType);
    }
}
