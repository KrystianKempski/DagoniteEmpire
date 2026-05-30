using System.Runtime.CompilerServices;
using System.Text;
using DA_Scribe.Configuration;
using DA_Scribe.Kernel;
using DA_Scribe.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Ollama;

namespace DA_Scribe.Services
{
    /// <summary>
    /// LLM service backed by Semantic Kernel + Ollama chat completion.
    /// Preserves the original ILLMService contract used by ScribeService.
    /// </summary>
    public class LLMService : ILLMService
    {
        private readonly ILogger<LLMService> _logger;
        private readonly ScribeOptions _options;
        private readonly Microsoft.SemanticKernel.Kernel _kernel;
        private readonly IChatCompletionService _chat;
        private readonly HttpClient _httpClient;
        private readonly string _systemPrompt;

        public string ModelName => _options.Ollama.ChatModel;

        public LLMService(
            ILogger<LLMService> logger,
            IOptions<ScribeOptions> options,
            IScribeKernelFactory kernelFactory,
            IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _options = options.Value;
            _kernel = kernelFactory.Create();
            _chat = _kernel.GetRequiredService<IChatCompletionService>();

            _httpClient = httpClientFactory.CreateClient(nameof(LLMService));
            _httpClient.BaseAddress = new Uri(_options.Ollama.BaseUrl);
            _httpClient.Timeout = TimeSpan.FromSeconds(_options.Ollama.TimeoutSeconds);

            _systemPrompt = LoadPersonaFromFile();
        }

        private string LoadPersonaFromFile()
        {
            var personaPath = _options.Ollama.PersonaFilePath;
            if (string.IsNullOrEmpty(personaPath))
                return _options.Ollama.SystemPrompt;

            try
            {
                if (File.Exists(personaPath))
                {
                    var content = File.ReadAllText(personaPath);
                    _logger.LogInformation("Loaded SCRIBE persona from {Path} ({Length} chars)",
                        personaPath, content.Length);
                    return content;
                }
                _logger.LogWarning("Persona file not found: {Path}, using fallback prompt", personaPath);
                return _options.Ollama.SystemPrompt;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading persona file: {Path}", personaPath);
                return _options.Ollama.SystemPrompt;
            }
        }

        public async Task<string> GenerateResponseAsync(
            string prompt,
            IEnumerable<string> context,
            string? systemPrompt = null,
            CancellationToken cancellationToken = default)
        {
            var history = BuildHistory(prompt, context, systemPrompt);
            var settings = BuildSettings();

            try
            {
                _logger.LogDebug("SK: chat completion via {Model}", _options.Ollama.ChatModel);

                var result = await _chat.GetChatMessageContentAsync(
                    history,
                    settings,
                    _kernel,
                    cancellationToken);

                return result.Content ?? string.Empty;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Failed to connect to Ollama via Semantic Kernel");
                throw new InvalidOperationException(
                    $"Failed to connect to Ollama. Is it running at {_options.Ollama.BaseUrl}?",
                    ex);
            }
        }

        public async IAsyncEnumerable<string> GenerateResponseStreamAsync(
            string prompt,
            IEnumerable<string> context,
            string? systemPrompt = null,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var history = BuildHistory(prompt, context, systemPrompt);
            var settings = BuildSettings();

            await foreach (var chunk in _chat.GetStreamingChatMessageContentsAsync(
                history,
                settings,
                _kernel,
                cancellationToken))
            {
                if (!string.IsNullOrEmpty(chunk.Content))
                    yield return chunk.Content;
            }
        }

        public async Task<string> SummarizeAsync(string text, CancellationToken cancellationToken = default)
        {
            var summaryPrompt =
                "Stwórz zwięzłe podsumowanie poniższego tekstu w maksymalnie 3-4 zdaniach. " +
                "Skup się na najważniejszych wydarzeniach, postaciach i miejscach.\n\n" +
                $"Tekst:\n{text}";

            var systemPrompt =
                "Jesteś asystentem tworzącym podsumowania przygód RPG. " +
                "Pisz zwięźle, ale zachowaj kluczowe informacje.";

            return await GenerateResponseAsync(
                summaryPrompt,
                Enumerable.Empty<string>(),
                systemPrompt,
                cancellationToken);
        }

        public async Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _httpClient.GetAsync("/api/tags", cancellationToken);
                if (!response.IsSuccessStatusCode)
                    return false;

                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                return content.Contains(_options.Ollama.ChatModel);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Ollama LLM availability check failed");
                return false;
            }
        }

        private ChatHistory BuildHistory(string prompt, IEnumerable<string> context, string? systemPromptOverride)
        {
            var history = new ChatHistory();
            history.AddSystemMessage(systemPromptOverride ?? _systemPrompt);
            history.AddUserMessage(BuildRAGPrompt(prompt, context));
            return history;
        }

        private OllamaPromptExecutionSettings BuildSettings() => new()
        {
            Temperature = _options.Ollama.Temperature,
            NumPredict = _options.Ollama.MaxTokens,
        };

        private static string BuildRAGPrompt(string question, IEnumerable<string> context)
        {
            var contextList = context.ToList();
            if (contextList.Count == 0)
                return question;

            var sb = new StringBuilder();
            sb.AppendLine("Na podstawie poniższych fragmentów z archiwum przygód, odpowiedz na pytanie.");
            sb.AppendLine();
            sb.AppendLine("=== FRAGMENTY Z ARCHIWUM ===");
            sb.AppendLine();
            for (int i = 0; i < contextList.Count; i++)
            {
                sb.AppendLine($"[Fragment {i + 1}]");
                sb.AppendLine(contextList[i]);
                sb.AppendLine();
            }
            sb.AppendLine("=== PYTANIE ===");
            sb.AppendLine(question);
            sb.AppendLine();
            sb.AppendLine("Odpowiedź:");
            return sb.ToString();
        }
    }
}
