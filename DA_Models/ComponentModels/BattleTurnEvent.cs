namespace DA_Models.ComponentModels;

public enum BattleTurnEventKind
{
    Wound,
    Unconscious,
    Dead,
    State,
    BleedingPainTest,
}

/// <summary>
/// How significant a battle event is. <see cref="Minor"/> events show up only in the
/// per-turn report; <see cref="Major"/> events are the ones important enough to also
/// appear in the end-of-battle summary (and are bolded in the battle log).
/// </summary>
public enum BattleEventImportance
{
    Minor = 0,
    Major = 1,
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

public sealed record BattleParticipantSnapshot(string Name, DA_Common.Relation Relation);
