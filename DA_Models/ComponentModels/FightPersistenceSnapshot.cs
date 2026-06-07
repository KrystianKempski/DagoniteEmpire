using DA_Models.CharacterModels;

namespace DA_Models.ComponentModels;

public sealed record FightPersistenceSnapshot(
    string SelectedAttacker,
    string SelectedDefender,
    string AttackerNewStates,
    string DefenderNewStates,
    List<WoundDTO> NewWounds,
    string WoundSeverity);
