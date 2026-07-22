namespace DA_Common.Barony
{
    public enum SeatRoomLayoutIssue
    {
        None = 0,
        OutOfBounds,
        Overlap,
    }

    /// <summary>Grid placement rules for lord's seat chambers.</summary>
    public static class SeatRoomLayout
    {
        public static bool HasFootprint(int gridW, int gridH) => gridW > 0 && gridH > 0;

        public static bool IsOutOfBounds(int gridX, int gridY, int gridW, int gridH, int seatWidth, int seatHeight)
        {
            if (!HasFootprint(gridW, gridH))
                return true;

            if (gridX < 0 || gridY < 0)
                return true;

            return gridX + gridW > seatWidth || gridY + gridH > seatHeight;
        }

        public static bool RectanglesOverlap(
            int ax, int ay, int aw, int ah,
            int bx, int by, int bw, int bh)
        {
            if (!HasFootprint(aw, ah) || !HasFootprint(bw, bh))
                return false;

            return ax < bx + bw && ax + aw > bx && ay < by + bh && ay + ah > by;
        }

        public static SeatRoomLayoutIssue GetIssue(
            int roomId,
            int gridX,
            int gridY,
            int gridW,
            int gridH,
            IEnumerable<(int Id, int X, int Y, int W, int H)> others,
            int seatWidth,
            int seatHeight)
        {
            if (!HasFootprint(gridW, gridH))
                return SeatRoomLayoutIssue.OutOfBounds;

            if (IsOutOfBounds(gridX, gridY, gridW, gridH, seatWidth, seatHeight))
                return SeatRoomLayoutIssue.OutOfBounds;

            foreach (var other in others)
            {
                if (other.Id == roomId)
                    continue;

                if (RectanglesOverlap(gridX, gridY, gridW, gridH, other.X, other.Y, other.W, other.H))
                    return SeatRoomLayoutIssue.Overlap;
            }

            return SeatRoomLayoutIssue.None;
        }

        public static string IssueLabel(SeatRoomLayoutIssue issue) => issue switch
        {
            SeatRoomLayoutIssue.OutOfBounds => "Outside grid or zero size",
            SeatRoomLayoutIssue.Overlap => "Overlaps another chamber",
            _ => string.Empty,
        };
    }
}
