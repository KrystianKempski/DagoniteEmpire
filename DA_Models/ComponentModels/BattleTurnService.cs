using DA_Common;

namespace DA_Models.ComponentModels;

/// <summary>
/// Pure, UI-independent decisions for the battle turn lifecycle. Keeping them here
/// (instead of inline in ChapterThread) makes the rules testable and gives the empty
/// BattlePhaseDTO stubs a real home.
/// </summary>
public static class BattleTurnService
{
    /// <summary>Advances a mob's serialized states by one turn (drops expired ones).</summary>
    public static string AdvanceMobStates(string? states) =>
        CombatStateString.DecrementTurn(states);

    /// <summary>New remaining duration for a temporary state after one turn passes.</summary>
    public static int DecrementDuration(int duration) => duration - 1;

    /// <summary>A temporary state is cleared once its remaining duration reaches zero.</summary>
    public static bool IsExpired(int duration) => duration <= 0;

    /// <summary>
    /// When a battle ends, Dead and Unconscious linger (as long-lived states); everything
    /// else is cleared. Returns the mob's post-battle state string.
    /// </summary>
    public static string ResolveEndOfBattleMobStates(string? states)
    {
        if (CombatStateString.HasState(states, States.Names.Dead))
            return CombatStateString.Add(null, States.Names.Dead, States.Duration.Permanent);

        if (CombatStateString.HasState(states, States.Names.Unconscious))
            return CombatStateString.Add(null, States.Names.Unconscious, States.Duration.UntilResolved);

        return string.Empty;
    }

    /// <summary>Whether a character's temporary state survives the end of a battle (Dead / Unconscious).</summary>
    public static bool PersistsAfterBattle(string? stateName) =>
        stateName == States.Names.Dead || stateName == States.Names.Unconscious;
}
