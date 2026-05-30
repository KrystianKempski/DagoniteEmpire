using System.ComponentModel;
using System.Text;
using DA_Scribe.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;

namespace DA_Scribe.Plugins
{
    /// <summary>
    /// Kernel plugin exposing semantic memory search to the LLM agent.
    /// Backed by ScribeService (pgvector similarity search).
    /// </summary>
    public sealed class ScribeSearchPlugin
    {
        private readonly IScribeService _scribe;
        private readonly ILogger<ScribeSearchPlugin> _logger;

        public ScribeSearchPlugin(IScribeService scribe, ILogger<ScribeSearchPlugin> logger)
        {
            _scribe = scribe;
            _logger = logger;
        }

        // Per-invocation context (set by the agent host before running the kernel).
        public string UserId { get; set; } = string.Empty;
        public int? CharacterId { get; set; }
        public int? CampaignId { get; set; }
        public bool IsGameMaster { get; set; }

        [KernelFunction("search_memories")]
        [Description("Searches the campaign memory archive (posts, lore, notes) by semantic similarity. " +
                     "Returns up to 'limit' matching fragments with source titles. " +
                     "Always call this before answering questions about past events, characters, locations or items.")]
        public async Task<string> SearchMemoriesAsync(
            [Description("Search query in Polish. Use the user's wording, expanded with key entities (e.g. character or place names).")]
            string query,
            [Description("Maximum number of fragments to return. Default 5, max 10.")]
            int limit = 5,
            CancellationToken cancellationToken = default)
        {
            limit = Math.Clamp(limit, 1, 10);
            _logger.LogInformation("Agent search: '{Query}' (limit={Limit}, user={User})", query, limit, UserId);

            var results = await _scribe.SearchAsync(
                query: query,
                userId: UserId,
                characterId: CharacterId,
                campaignId: CampaignId,
                topK: limit,
                isGameMaster: IsGameMaster,
                cancellationToken: cancellationToken);

            if (results.Count == 0)
                return "Brak fragmentów pasujących do zapytania.";

            var sb = new StringBuilder();
            sb.AppendLine($"Znaleziono {results.Count} fragmentów:");
            sb.AppendLine();
            int i = 1;
            foreach (var r in results)
            {
                var title = r.Memory?.Title ?? "(bez tytułu)";
                sb.AppendLine($"[Fragment {i++}] {title} (podobieństwo: {r.Similarity:F2})");
                sb.AppendLine(r.Chunk.Content);
                sb.AppendLine();
            }
            return sb.ToString();
        }
    }
}
