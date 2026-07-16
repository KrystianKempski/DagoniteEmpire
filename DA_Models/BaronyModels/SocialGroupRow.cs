using DA_Common.Barony;

namespace DA_Models.BaronyModels
{
    /// <summary>Row for the Social Group Relations section (fixed groups merged with DB data).</summary>
    public sealed class SocialGroupRow
    {
        public int Id { get; init; }
        public int BaronyId { get; init; }
        public string Group { get; init; } = string.Empty;
        public int InfluencePercent { get; init; }
        public bool IsActive { get; init; }
        public int RelationScore { get; init; }
        public PpbVector Additive { get; init; } = new();
        public PpbVector Percent { get; init; } = new();

        public string RelationLabel => SocialRelation.Label(RelationScore);

        public string RelationDisplay => $"{RelationScore} · {RelationLabel}";
    }
}
