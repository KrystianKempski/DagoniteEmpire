namespace DA_Common.Barony
{
    public sealed class MarchMapNode
    {
        public string Id { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string Kind { get; set; } = MarchMapNodeKind.City;
        /// <summary>Optional link to <see cref="KnownLordsCatalog"/> lord key.</summary>
        public string? LordKey { get; set; }
        /// <summary>Normalized X in 0–1000 (matches SVG viewBox).</summary>
        public double X { get; set; }
        /// <summary>Normalized Y in 0–1000.</summary>
        public double Y { get; set; }

        /// <summary>
        /// MG-set default customs (gold/turn) when this seat is a transit node on a designed route.
        /// Null = use <see cref="MarchMapTradePathfinder.DefaultCustomsGoldPerTurn"/>.
        /// </summary>
        public decimal? DefaultCustomsGoldPerTurn { get; set; }
    }

    public sealed class MarchMapRoute
    {
        public string Id { get; set; } = string.Empty;
        public string FromNodeId { get; set; } = string.Empty;
        public string ToNodeId { get; set; } = string.Empty;
        public string Kind { get; set; } = MarchRouteKind.Road;
        public string? Label { get; set; }
    }

    public sealed class MarchMapDocument
    {
        public const string DefaultImageUrl = "/maps/eastern-march.jpg";

        /// <summary>Authored-seed revision baked into this payload; 0 = legacy/procedural. See <see cref="EasternMarchMapDefaults.CurrentSeedVersion"/>.</summary>
        public int SeedVersion { get; set; }

        public string ImageUrl { get; set; } = DefaultImageUrl;
        public List<MarchMapNode> Nodes { get; set; } = new();
        public List<MarchMapRoute> Routes { get; set; } = new();
    }

    public sealed class BlockTradeToggleRequest
    {
        public string LordKey { get; set; } = string.Empty;
        public bool Block { get; set; }
    }
}
