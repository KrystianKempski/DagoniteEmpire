using DA_Common.Barony;
using Xunit;

namespace DA_Business.Tests.Barony;

public class ProjectTurnTickTests
{
    [Fact]
    public void Advance_ZeroTurnProject_CompletesImmediatelyAfterFunding()
    {
        var turns = 0;
        Assert.Equal(ProjectTurnTick.Step.CompleteNow, ProjectTurnTick.Advance(ref turns, justFunded: true));
        Assert.Equal(0, turns);
    }

    [Fact]
    public void Advance_ZeroTurnProject_InProgress_CompletesWithoutTick()
    {
        var turns = 0;
        Assert.Equal(ProjectTurnTick.Step.CompleteNow, ProjectTurnTick.Advance(ref turns, justFunded: false));
        Assert.Equal(0, turns);
    }

    [Fact]
    public void Advance_OneTurnProject_WaitsAfterFunding()
    {
        var turns = 1;
        Assert.Equal(ProjectTurnTick.Step.WaitAfterFunding, ProjectTurnTick.Advance(ref turns, justFunded: true));
        Assert.Equal(1, turns);
    }

    [Fact]
    public void Advance_OneTurnProject_CompletesOnNextResolve()
    {
        var turns = 1;
        Assert.Equal(ProjectTurnTick.Step.TickAndComplete, ProjectTurnTick.Advance(ref turns, justFunded: false));
        Assert.Equal(0, turns);
    }

    [Fact]
    public void Advance_MultiTurnProject_TicksDown()
    {
        var turns = 3;
        Assert.Equal(ProjectTurnTick.Step.TickAndContinue, ProjectTurnTick.Advance(ref turns, justFunded: false));
        Assert.Equal(2, turns);
    }
}
