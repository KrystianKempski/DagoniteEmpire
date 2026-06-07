namespace DA_Models.ComponentModels;

public sealed record FightSequenceDialogResult(
    string RichHtml,
    bool WrapForThread,
    string AttackerName,
    string DefenderName,
    FightPersistenceSnapshot? Persistence = null);
