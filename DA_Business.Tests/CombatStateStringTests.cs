using DA_Common;
using DA_Models.ComponentModels;

namespace DA_Business.Tests;

public class CombatStateStringTests
{
    [Theory]
    [InlineData("Stunned:3, Bleeding:99, ")]
    [InlineData("Stunned:3,Bleeding:99,")]
    [InlineData("  Stunned : 3 ,  Bleeding : 99  ")]
    public void Parse_ToleratesBothSeparatorsAndWhitespace(string states)
    {
        var parsed = CombatStateString.Parse(states);

        Assert.Equal(2, parsed.Count);
        Assert.Equal(new CombatStateEntry("Stunned", 3), parsed[0]);
        Assert.Equal(new CombatStateEntry("Bleeding", 99), parsed[1]);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("garbage, Stunned")]
    public void Parse_SkipsEmptyOrMalformedTokens(string? states)
    {
        Assert.Empty(CombatStateString.Parse(states));
    }

    [Fact]
    public void Format_ProducesCanonicalTrailingSeparator()
    {
        var formatted = CombatStateString.Format(new[]
        {
            new CombatStateEntry("Stunned", 3),
            new CombatStateEntry("Bleeding", 99),
        });

        Assert.Equal("Stunned:3, Bleeding:99, ", formatted);
    }

    [Fact]
    public void Format_EmptyWhenNoEntries()
    {
        Assert.Equal(string.Empty, CombatStateString.Format(Array.Empty<CombatStateEntry>()));
        Assert.Equal(string.Empty, CombatStateString.Format(null));
    }

    [Fact]
    public void Parse_Format_RoundTripsCanonicalForm()
    {
        const string states = "Stunned:3, Bleeding:99, ";
        Assert.Equal(states, CombatStateString.Format(CombatStateString.Parse(states)));
    }

    [Fact]
    public void Merge_OverwritesDurationAndKeepsOrder()
    {
        var merged = CombatStateString.Merge("Stunned:3, Bleeding:99, ", "Stunned:5, Blinded:2, ");

        Assert.Equal("Stunned:5, Bleeding:99, Blinded:2, ", merged);
    }

    [Fact]
    public void Merge_NoTurnClearsHalfTurn()
    {
        var merged = CombatStateString.Merge("Half turn:1, Stunned:2, ", "No turn:1, ");

        Assert.False(CombatStateString.HasState(merged, States.Names.HalfTurn));
        Assert.True(CombatStateString.HasState(merged, States.Names.NoTurn));
        Assert.True(CombatStateString.HasState(merged, States.Names.Stunned));
    }

    [Fact]
    public void Merge_HandlesNullInputs()
    {
        Assert.Equal("Stunned:2, ", CombatStateString.Merge(null, "Stunned:2, "));
        Assert.Equal("Stunned:2, ", CombatStateString.Merge("Stunned:2, ", null));
        Assert.Equal(string.Empty, CombatStateString.Merge(null, null));
    }

    [Fact]
    public void Add_UpsertsWithoutDuplicating()
    {
        var once = CombatStateString.Add("Stunned:3, ", States.Names.Stunned, 5);

        Assert.Equal("Stunned:5, ", once);
    }

    [Fact]
    public void DecrementTurn_DropsExpiredStates()
    {
        var next = CombatStateString.DecrementTurn("Stunned:1, Bleeding:3, ");

        Assert.Equal("Bleeding:2, ", next);
    }

    [Fact]
    public void TryGetDuration_FindsMatchingState()
    {
        Assert.True(CombatStateString.TryGetDuration("Stunned:3, Bleeding:99, ", States.Names.Bleeding, out var duration));
        Assert.Equal(99, duration);
        Assert.False(CombatStateString.TryGetDuration("Stunned:3, ", States.Names.Bleeding, out _));
    }

    [Fact]
    public void Parse_DedupeIsCallersJob_KeepsBothRawEntries()
    {
        // Parse itself is a faithful reader; dedupe only happens on Merge/Add.
        var parsed = CombatStateString.Parse("Stunned:3, Stunned:7, ");

        Assert.Equal(2, parsed.Count);
    }

    [Fact]
    public void Merge_CollapsesDuplicatesWithinASingleInput()
    {
        var merged = CombatStateString.Merge("Stunned:3, Stunned:7, ", null);

        Assert.Equal("Stunned:7, ", merged);
    }

    [Fact]
    public void Merge_AppendsNewStatesAfterExistingOnes()
    {
        var merged = CombatStateString.Merge("Bleeding:99, ", "Stunned:2, ");

        Assert.Equal("Bleeding:99, Stunned:2, ", merged);
    }

    [Fact]
    public void Add_AppendsWhenStateNotPresent()
    {
        var result = CombatStateString.Add("Stunned:3, ", States.Names.Bleeding, 99);

        Assert.Equal("Stunned:3, Bleeding:99, ", result);
    }

    [Fact]
    public void Add_NoTurnClearsPendingHalfTurn()
    {
        var result = CombatStateString.Add("Half turn:1, ", States.Names.NoTurn, 1);

        Assert.False(CombatStateString.HasState(result, States.Names.HalfTurn));
        Assert.True(CombatStateString.HasState(result, States.Names.NoTurn));
    }

    [Fact]
    public void Add_ToEmptyOrNullStartsFresh()
    {
        Assert.Equal("Stunned:2, ", CombatStateString.Add(null, States.Names.Stunned, 2));
        Assert.Equal("Stunned:2, ", CombatStateString.Add(string.Empty, States.Names.Stunned, 2));
    }

    [Fact]
    public void DecrementTurn_DropsAllWhenEverythingExpires()
    {
        Assert.Equal(string.Empty, CombatStateString.DecrementTurn("Stunned:1, Stumbled:1, "));
    }

    [Fact]
    public void DecrementTurn_KeepsHighDurationStatesEssentiallyPermanent()
    {
        Assert.Equal("Dead:998, ", CombatStateString.DecrementTurn("Dead:999, "));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void DecrementTurn_EmptyStaysEmpty(string? states)
    {
        Assert.Equal(string.Empty, CombatStateString.DecrementTurn(states));
    }

    // --- Public API parity: FightSequenceModel.MergeMobStates now delegates here ---

    [Fact]
    public void MergeMobStates_OverwritesDurationsLikeBefore()
    {
        var merged = FightSequenceModel.MergeMobStates("Stunned:3, Bleeding:99, ", "Stunned:5, ");

        Assert.Equal("Stunned:5, Bleeding:99, ", merged);
    }

    [Fact]
    public void MergeMobStates_NoTurnClearsHalfTurn()
    {
        var merged = FightSequenceModel.MergeMobStates("Half turn:1, ", "No turn:1, ");

        Assert.Equal("No turn:1, ", merged);
    }

    [Fact]
    public void MergeMobStates_EmptyInputsReturnEmpty()
    {
        Assert.Equal(string.Empty, FightSequenceModel.MergeMobStates(null, null));
        Assert.Equal(string.Empty, FightSequenceModel.MergeMobStates(string.Empty, string.Empty));
    }
}
