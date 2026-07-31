namespace DA_Models.BaronyModels;

/// <summary>Eight compass facings for battle tokens (0 = North, clockwise).</summary>
public static class BaronyBattleFacing
{
    public const int North = 0;
    public const int NorthEast = 1;
    public const int East = 2;
    public const int SouthEast = 3;
    public const int South = 4;
    public const int SouthWest = 5;
    public const int West = 6;
    public const int NorthWest = 7;

    public static int Clamp(int facing) => ((facing % 8) + 8) % 8;

    /// <summary>CSS degrees: 0 = North (up), clockwise.</summary>
    public static double ToDegrees(int facing) => Clamp(facing) * 45.0;

    /// <summary>
    /// Continuous facing angle from cell center toward a point in screen space
    /// (Y increases downward). 0° = North.
    /// </summary>
    public static double DegreesToward(double fromX, double fromY, double toX, double toY)
    {
        var dx = toX - fromX;
        var dy = toY - fromY;
        if (Math.Abs(dx) < 0.001 && Math.Abs(dy) < 0.001)
            return 0;
        // atan2(dx, -dy): 0 when pointing up (negative screen Y).
        var deg = Math.Atan2(dx, -dy) * (180.0 / Math.PI);
        if (deg < 0)
            deg += 360.0;
        return deg;
    }

    public static int SnapFromDegrees(double degrees)
    {
        var normalized = degrees % 360.0;
        if (normalized < 0)
            normalized += 360.0;
        return Clamp((int)Math.Round(normalized / 45.0) % 8);
    }

    public static int FromScreenVector(double dx, double dy) =>
        SnapFromDegrees(DegreesToward(0, 0, dx, dy));

    /// <summary>Facing from one grid cell toward another (Y increases south / down).</summary>
    public static int FromGridStep(int fromX, int fromY, int toX, int toY)
    {
        if (fromX == toX && fromY == toY)
            return North;
        return FromScreenVector(toX - fromX, toY - fromY);
    }

    /// <summary>
    /// Adjust continuous degrees so CSS rotation takes the shortest turn toward <paramref name="targetDeg"/>.
    /// </summary>
    public static double ShortestTurnTarget(double currentDeg, double targetDeg)
    {
        var delta = (targetDeg - currentDeg) % 360.0;
        if (delta > 180.0) delta -= 360.0;
        if (delta < -180.0) delta += 360.0;
        return currentDeg + delta;
    }
}
