namespace DA_Common.Barony
{
    /// <summary>Shortest trade path on the Eastern March map (fewest nodes).</summary>
    public static class MarchMapTradePathfinder
    {
        public const decimal DefaultCustomsGoldPerTurn = 5m;

        public static decimal EffectiveCustoms(MarchMapNode? node) =>
            node?.DefaultCustomsGoldPerTurn is decimal v && v >= 0m ? v : DefaultCustomsGoldPerTurn;

        /// <summary>Economy contribution for one toll/destination lord: max(1, Wealth − 2).</summary>
        public static decimal EconomyFromWealth(int wealth) =>
            Math.Max(1m, wealth - 2m);

        public static MarchMapTradePath? FindShortestPath(
            MarchMapDocument document,
            string startNodeId,
            string endNodeId,
            IReadOnlyCollection<string> blockedLordKeys)
        {
            if (string.IsNullOrWhiteSpace(startNodeId) || string.IsNullOrWhiteSpace(endNodeId))
                return null;
            if (string.Equals(startNodeId, endNodeId, StringComparison.OrdinalIgnoreCase))
                return null;

            var nodes = document.Nodes ?? new List<MarchMapNode>();
            var byId = nodes.ToDictionary(n => n.Id, StringComparer.OrdinalIgnoreCase);
            if (!byId.TryGetValue(startNodeId, out var start) || !byId.TryGetValue(endNodeId, out var end))
                return null;

            if (string.IsNullOrWhiteSpace(start.LordKey) || string.IsNullOrWhiteSpace(end.LordKey))
                return null;

            var blocked = new HashSet<string>(
                blockedLordKeys ?? Array.Empty<string>(),
                StringComparer.OrdinalIgnoreCase);

            // Blocked destination is unreachable.
            if (blocked.Contains(end.LordKey!))
                return null;

            var adj = BuildAdjacency(document);
            var queue = new Queue<string>();
            var prev = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
            queue.Enqueue(start.Id);
            prev[start.Id] = null;

            while (queue.Count > 0)
            {
                var currentId = queue.Dequeue();
                if (string.Equals(currentId, end.Id, StringComparison.OrdinalIgnoreCase))
                    break;

                if (!adj.TryGetValue(currentId, out var neighbors))
                    continue;

                foreach (var nextId in neighbors)
                {
                    if (prev.ContainsKey(nextId))
                        continue;
                    if (!byId.TryGetValue(nextId, out var next))
                        continue;
                    if (string.IsNullOrWhiteSpace(next.LordKey))
                        continue;

                    // Start is never blocked for traversal; intermediates and end checked above/here.
                    if (!string.Equals(nextId, end.Id, StringComparison.OrdinalIgnoreCase) &&
                        blocked.Contains(next.LordKey!))
                        continue;

                    prev[nextId] = currentId;
                    queue.Enqueue(nextId);
                }
            }

            if (!prev.ContainsKey(end.Id))
                return null;

            var chain = new List<MarchMapNode>();
            for (string? id = end.Id; id is not null; id = prev[id])
                chain.Add(byId[id]);
            chain.Reverse();

            return new MarchMapTradePath(chain);
        }

        public static bool AreAdjacent(MarchMapDocument document, string nodeIdA, string nodeIdB)
        {
            if (string.IsNullOrWhiteSpace(nodeIdA) || string.IsNullOrWhiteSpace(nodeIdB))
                return false;
            var adj = BuildAdjacency(document);
            return adj.TryGetValue(nodeIdA, out var neighbors) &&
                   neighbors.Contains(nodeIdB, StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>Whether a walk from the seat exists that visits paragraph lords in order (any path, not necessarily shortest).</summary>
        public static bool RouteLordSequenceExists(
            MarchMapDocument document,
            string startNodeId,
            IReadOnlyList<string> lordKeysInOrder,
            IReadOnlyCollection<string> blockedLordKeys)
        {
            if (string.IsNullOrWhiteSpace(startNodeId) || lordKeysInOrder.Count == 0)
                return false;

            var nodes = (document.Nodes ?? new List<MarchMapNode>())
                .ToDictionary(n => n.Id, StringComparer.OrdinalIgnoreCase);
            if (!nodes.ContainsKey(startNodeId))
                return false;

            var blocked = new HashSet<string>(blockedLordKeys ?? Array.Empty<string>(), StringComparer.OrdinalIgnoreCase);
            var adj = BuildAdjacency(document);
            var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { startNodeId };

            return VisitNext(0, startNodeId);

            bool VisitNext(int lordIndex, string currentId)
            {
                if (!adj.TryGetValue(currentId, out var neighbors))
                    return false;

                var expectedLord = lordKeysInOrder[lordIndex];
                foreach (var nextId in neighbors)
                {
                    if (visited.Contains(nextId))
                        continue;
                    if (!nodes.TryGetValue(nextId, out var next))
                        continue;
                    if (string.IsNullOrWhiteSpace(next.LordKey))
                        continue;
                    if (!string.Equals(next.LordKey, expectedLord, StringComparison.OrdinalIgnoreCase))
                        continue;

                    var isDest = lordIndex == lordKeysInOrder.Count - 1;
                    if (blocked.Contains(next.LordKey!))
                        continue;

                    visited.Add(nextId);
                    if (isDest)
                        return true;
                    if (VisitNext(lordIndex + 1, nextId))
                        return true;
                    visited.Remove(nextId);
                }

                return false;
            }
        }

        public static MarchMapTradePath? PathFromNodeIds(MarchMapDocument document, IReadOnlyList<string> nodeIds)
        {
            if (nodeIds.Count < 2)
                return null;

            var chain = new List<MarchMapNode>();
            foreach (var id in nodeIds)
            {
                var node = document.Nodes?.FirstOrDefault(n =>
                    string.Equals(n.Id, id, StringComparison.OrdinalIgnoreCase));
                if (node is null)
                    return null;
                chain.Add(node);
            }

            return new MarchMapTradePath(chain);
        }

        public static IReadOnlyList<string> ReachableNeighborIds(
            MarchMapDocument document,
            string fromNodeId,
            IReadOnlyCollection<string> blockedLordKeys,
            string? playerSeatNodeId,
            IReadOnlyCollection<string> alreadyOnPathNodeIds)
        {
            var adj = BuildAdjacency(document);
            if (!adj.TryGetValue(fromNodeId, out var neighbors))
                return Array.Empty<string>();

            var onPath = new HashSet<string>(alreadyOnPathNodeIds ?? Array.Empty<string>(), StringComparer.OrdinalIgnoreCase);
            var blocked = new HashSet<string>(blockedLordKeys ?? Array.Empty<string>(), StringComparer.OrdinalIgnoreCase);
            var result = new List<string>();

            foreach (var nextId in neighbors)
            {
                if (onPath.Contains(nextId))
                    continue;
                var node = document.Nodes?.FirstOrDefault(n =>
                    string.Equals(n.Id, nextId, StringComparison.OrdinalIgnoreCase));
                if (node is null || string.IsNullOrWhiteSpace(node.LordKey))
                    continue;
                if (!string.IsNullOrWhiteSpace(playerSeatNodeId) &&
                    string.Equals(node.Id, playerSeatNodeId, StringComparison.OrdinalIgnoreCase))
                    continue;
                if (blocked.Contains(node.LordKey!))
                    continue;
                result.Add(nextId);
            }

            return result;
        }

        public static Dictionary<string, List<string>> BuildAdjacency(MarchMapDocument document)
        {
            var adj = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
            void Link(string a, string b)
            {
                if (!adj.TryGetValue(a, out var listA))
                    adj[a] = listA = new List<string>();
                if (!adj.TryGetValue(b, out var listB))
                    adj[b] = listB = new List<string>();
                if (!listA.Contains(b, StringComparer.OrdinalIgnoreCase))
                    listA.Add(b);
                if (!listB.Contains(a, StringComparer.OrdinalIgnoreCase))
                    listB.Add(a);
            }

            foreach (var route in document.Routes ?? Enumerable.Empty<MarchMapRoute>())
                Link(route.FromNodeId, route.ToNodeId);

            return adj;
        }

        /// <summary>Build one paragraph per route seat after the player's start (transit… + destination).</summary>
        public static List<TradeTreatyParagraph> ToRouteParagraphs(
            MarchMapTradePath path,
            IReadOnlyList<TradeTreatyParagraph>? preserveGoodsFrom = null)
        {
            var byLord = (preserveGoodsFrom ?? Array.Empty<TradeTreatyParagraph>())
                .Where(p => !string.IsNullOrWhiteSpace(p.LordKey))
                .GroupBy(p => p.LordKey, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

            var paragraphs = new List<TradeTreatyParagraph>();
            for (var i = 1; i < path.Nodes.Count; i++)
            {
                var node = path.Nodes[i];
                var isDest = i == path.Nodes.Count - 1;
                byLord.TryGetValue(node.LordKey!, out var prior);
                paragraphs.Add(new TradeTreatyParagraph
                {
                    LordKey = node.LordKey!,
                    IsDestination = isDest,
                    CustomsGoldPerTurn = isDest ? 0m : EffectiveCustoms(node),
                    SweetenerGoldPerTurn = prior?.SweetenerGoldPerTurn ?? 0m,
                    BaronyGrantsGoodKeys = prior?.BaronyGrantsGoodKeys.ToList() ?? new List<string>(),
                    CounterpartyGrantsGoodKeys = prior?.CounterpartyGrantsGoodKeys.ToList() ?? new List<string>(),
                });
            }

            return paragraphs;
        }

        /// <summary>Build transit legs (intermediate nodes only) with effective customs.</summary>
        [Obsolete("Use ToRouteParagraphs — one paragraph per seat.")]
        public static List<TradeTreatyTransitLeg> ToTransitLegs(MarchMapTradePath path)
        {
            var legs = new List<TradeTreatyTransitLeg>();
            for (var i = 1; i < path.Nodes.Count - 1; i++)
            {
                var node = path.Nodes[i];
                legs.Add(new TradeTreatyTransitLeg
                {
                    LordKey = node.LordKey!,
                    CustomsGoldPerTurn = EffectiveCustoms(node),
                });
            }

            return legs;
        }

        public static decimal EstimateRouteEconomy(MarchMapTradePath path)
        {
            // Toll nodes (intermediates) + destination.
            decimal total = 0;
            for (var i = 1; i < path.Nodes.Count; i++)
            {
                var lord = KnownLordsCatalog.FindByKey(path.Nodes[i].LordKey);
                if (lord is not null)
                    total += EconomyFromWealth(lord.Wealth);
            }

            return total;
        }

        public static decimal EstimateCustoms(MarchMapTradePath path) =>
            path.TransitNodes.Sum(EffectiveCustoms);
    }

    public sealed class MarchMapTradePath
    {
        public MarchMapTradePath(IReadOnlyList<MarchMapNode> nodes)
        {
            Nodes = nodes;
        }

        public IReadOnlyList<MarchMapNode> Nodes { get; }

        public MarchMapNode Start => Nodes[0];
        public MarchMapNode End => Nodes[^1];

        public IEnumerable<MarchMapNode> TransitNodes
        {
            get
            {
                for (var i = 1; i < Nodes.Count - 1; i++)
                    yield return Nodes[i];
            }
        }

        public IReadOnlyList<string> NodeIds => Nodes.Select(n => n.Id).ToList();
    }
}
