namespace DA_Models.ComponentModels;

public enum BattleTurnEventKind
{
    Wound,
    Unconscious,
    Dead,
    State
}

public sealed record BattleTurnEvent(
    BattleTurnEventKind Kind,
    string TargetName,
    string? CausedBy,
    string Description);

public sealed record StatusAddedSnapshot(
    string TargetName,
    string StateName,
    int Duration,
    bool IsMob);
