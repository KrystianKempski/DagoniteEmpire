namespace DagoniteEmpire.Pages.Barony.Components;

/// <summary>One token being animated during movement resolution.</summary>
public sealed class BattleMoveAnimFrame
{
    public string TokenId { get; set; } = string.Empty;
    public double LeftPx { get; set; }
    public double TopPx { get; set; }
    public int Size { get; set; } = 1;
    /// <summary>Continuous facing degrees (may unwrap beyond 0–360 for shortest turns).</summary>
    public double FacingDeg { get; set; }
}
