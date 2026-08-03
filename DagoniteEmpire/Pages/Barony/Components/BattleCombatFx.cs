namespace DagoniteEmpire.Pages.Barony.Components;

/// <summary>On-map FX for one combat exchange (clash icon ± floating damage).</summary>
public sealed class BattleCombatFx
{
    public string AttackerId { get; init; } = string.Empty;
    public string DefenderId { get; init; } = string.Empty;
    public int DealtToDefender { get; init; }
    public int DealtToAttacker { get; init; }
    /// <summary>Crossed-swords icon between the pair.</summary>
    public bool ShowClash { get; set; } = true;
    /// <summary>Floating −HP numbers next to each unit.</summary>
    public bool ShowDamage { get; set; }
}
