using DA_Business.Repository.CharacterReps.IRepository;
using DA_Scribe.Configuration;
using DA_Scribe.Kernel;
using DA_Scribe.Plugins;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Ollama;

namespace DagoniteEmpire.Service.Scribe
{
    public class ScribeAgentRequest
    {
        public required string Question { get; set; }
        public required string UserId { get; set; }
        public int? CharacterId { get; set; }
        public int? CampaignId { get; set; }
        public bool IsGameMaster { get; set; }
    }

    public class ScribeAgentResult
    {
        public string Response { get; set; } = string.Empty;
        public int GenerationTimeMs { get; set; }
        public string? ModelUsed { get; set; }
        public List<string> ToolCalls { get; set; } = new();
    }

    public interface IScribeAgentService
    {
        Task<ScribeAgentResult> InvokeAsync(ScribeAgentRequest request, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Agentic Skryba: a Semantic-Kernel chat completion loop with auto tool-calling.
    /// The LLM decides when to call search_memories / get_character / list_chapters etc.
    /// </summary>
    public class ScribeAgentService : IScribeAgentService
    {
        private const string DefaultInstructions = """
            Jesteś Skrybą - archiwistą kampanii Dagonite Empire.
            Odpowiadasz po polsku, zwięźle i konkretnie.

            Zasady:
            1. ZAWSZE używaj dostępnych narzędzi do wyszukania faktów przed odpowiedzią.
            2. Jeśli pytanie dotyczy wydarzeń, miejsc lub przeszłych zdarzeń - użyj 'search_memories'.
            3. Jeśli pytanie dotyczy konkretnej postaci - użyj 'get_character_by_name'.
            4. Jeśli potrzebujesz orientacji w kampanii - użyj 'list_campaign_chapters' lub 'list_campaign_characters'.
            5. Możesz wywołać narzędzia wielokrotnie aby zebrać kontekst zanim odpowiesz.
            6. NIGDY nie zmyślaj. Jeśli narzędzia nie zwróciły informacji, powiedz "Nie mam o tym wzmianek w archiwum".
            7. W odpowiedzi cytuj źródła (rozdział / postać / fragment), gdy to możliwe.
            """;

        private readonly IScribeKernelFactory _kernelFactory;
        private readonly ScribeOptions _options;
        private readonly ILogger<ScribeAgentService> _logger;
        private readonly IServiceProvider _sp;

        public ScribeAgentService(
            IScribeKernelFactory kernelFactory,
            IOptions<ScribeOptions> options,
            ILogger<ScribeAgentService> logger,
            IServiceProvider sp)
        {
            _kernelFactory = kernelFactory;
            _options = options.Value;
            _logger = logger;
            _sp = sp;
        }

        public async Task<ScribeAgentResult> InvokeAsync(
            ScribeAgentRequest request,
            CancellationToken cancellationToken = default)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();

            var kernel = _kernelFactory.Create();

            // Build plugins with per-request context
            var search = ActivatorUtilities.CreateInstance<ScribeSearchPlugin>(_sp);
            search.UserId = request.UserId;
            search.CharacterId = request.CharacterId;
            search.CampaignId = request.CampaignId;
            search.IsGameMaster = request.IsGameMaster;
            kernel.Plugins.AddFromObject(search, "scribe");

            var characterPlugin = ActivatorUtilities.CreateInstance<CharacterPlugin>(_sp);
            characterPlugin.CampaignId = request.CampaignId;
            kernel.Plugins.AddFromObject(characterPlugin, "characters");

            var chapterPlugin = ActivatorUtilities.CreateInstance<ChapterPlugin>(_sp);
            chapterPlugin.CampaignId = request.CampaignId;
            kernel.Plugins.AddFromObject(chapterPlugin, "chapters");

            var chat = kernel.GetRequiredService<IChatCompletionService>();

            var history = new ChatHistory();
            history.AddSystemMessage(DefaultInstructions);
            history.AddUserMessage(request.Question);

            var settings = new OllamaPromptExecutionSettings
            {
                Temperature = _options.Ollama.Temperature,
                NumPredict = _options.Ollama.MaxTokens,
                FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),
            };

            _logger.LogInformation(
                "Agent invoke: question='{Question}' user={User} campaign={Campaign}",
                request.Question, request.UserId, request.CampaignId);

            var reply = await chat.GetChatMessageContentAsync(history, settings, kernel, cancellationToken);

            sw.Stop();

            // Collect tool-call trace from history (auto-invoked functions are appended)
            var toolCalls = history
                .Where(m => m.Role == AuthorRole.Tool)
                .Select(m => m.AuthorName ?? "tool")
                .Distinct()
                .ToList();

            return new ScribeAgentResult
            {
                Response = reply.Content ?? string.Empty,
                GenerationTimeMs = (int)sw.ElapsedMilliseconds,
                ModelUsed = _options.Ollama.ChatModel,
                ToolCalls = toolCalls,
            };
        }
    }
}
