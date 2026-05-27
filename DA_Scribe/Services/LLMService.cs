using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using DA_Scribe.Configuration;
using DA_Scribe.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DA_Scribe.Services
{
    /// <summary>
    /// LLM service using Ollama API for text generation
    /// </summary>
    public class LLMService : ILLMService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<LLMService> _logger;
        private readonly ScribeOptions _options;
        
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
        
        public string ModelName => _options.Ollama.ChatModel;
        
        public LLMService(
            HttpClient httpClient,
            ILogger<LLMService> logger,
            IOptions<ScribeOptions> options)
        {
            _httpClient = httpClient;
            _logger = logger;
            _options = options.Value;
            
            _httpClient.BaseAddress = new Uri(_options.Ollama.BaseUrl);
            _httpClient.Timeout = TimeSpan.FromSeconds(_options.Ollama.TimeoutSeconds * 2); // Longer for generation
        }
        
        public async Task<string> GenerateResponseAsync(
            string prompt,
            IEnumerable<string> context,
            string? systemPrompt = null,
            CancellationToken cancellationToken = default)
        {
            var fullPrompt = BuildRAGPrompt(prompt, context);
            var system = systemPrompt ?? _options.Ollama.SystemPrompt;
            
            var request = new GenerateRequest
            {
                Model = _options.Ollama.ChatModel,
                Prompt = fullPrompt,
                System = system,
                Stream = false,
                Options = new GenerateOptions
                {
                    Temperature = _options.Ollama.Temperature,
                    NumPredict = _options.Ollama.MaxTokens
                }
            };
            
            try
            {
                _logger.LogDebug("Sending prompt to {Model}", _options.Ollama.ChatModel);
                
                var response = await _httpClient.PostAsJsonAsync(
                    "/api/generate",
                    request,
                    JsonOptions,
                    cancellationToken);
                
                response.EnsureSuccessStatusCode();
                
                var result = await response.Content.ReadFromJsonAsync<GenerateResponse>(
                    JsonOptions,
                    cancellationToken);
                
                _logger.LogDebug(
                    "Generated response in {Duration}ms, {TokenCount} tokens",
                    result?.TotalDuration / 1_000_000, // nanoseconds to ms
                    result?.EvalCount);
                
                return result?.Response ?? string.Empty;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Failed to connect to Ollama for generation");
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
            var fullPrompt = BuildRAGPrompt(prompt, context);
            var system = systemPrompt ?? _options.Ollama.SystemPrompt;
            
            var request = new GenerateRequest
            {
                Model = _options.Ollama.ChatModel,
                Prompt = fullPrompt,
                System = system,
                Stream = true,
                Options = new GenerateOptions
                {
                    Temperature = _options.Ollama.Temperature,
                    NumPredict = _options.Ollama.MaxTokens
                }
            };
            
            var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/api/generate")
            {
                Content = new StringContent(
                    JsonSerializer.Serialize(request, JsonOptions),
                    Encoding.UTF8,
                    "application/json")
            };
            
            var response = await _httpClient.SendAsync(
                httpRequest,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);
            
            response.EnsureSuccessStatusCode();
            
            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var reader = new StreamReader(stream);
            
            while (!reader.EndOfStream)
            {
                cancellationToken.ThrowIfCancellationRequested();
                
                var line = await reader.ReadLineAsync(cancellationToken);
                if (string.IsNullOrEmpty(line)) continue;
                
                var chunk = JsonSerializer.Deserialize<GenerateResponse>(line, JsonOptions);
                if (chunk?.Response != null)
                {
                    yield return chunk.Response;
                }
                
                if (chunk?.Done == true)
                {
                    break;
                }
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
                
                // Check if chat model is available
                return content.Contains(_options.Ollama.ChatModel);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Ollama LLM availability check failed");
                return false;
            }
        }
        
        private static string BuildRAGPrompt(string question, IEnumerable<string> context)
        {
            var contextList = context.ToList();
            
            if (!contextList.Any())
            {
                return question;
            }
            
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
        
        // Request/Response DTOs
        
        private class GenerateRequest
        {
            public string Model { get; set; } = string.Empty;
            public string Prompt { get; set; } = string.Empty;
            public string? System { get; set; }
            public bool Stream { get; set; }
            public GenerateOptions? Options { get; set; }
        }
        
        private class GenerateOptions
        {
            public float Temperature { get; set; }
            [JsonPropertyName("num_predict")]
            public int NumPredict { get; set; }
        }
        
        private class GenerateResponse
        {
            public string? Response { get; set; }
            public bool Done { get; set; }
            [JsonPropertyName("total_duration")]
            public long? TotalDuration { get; set; }
            [JsonPropertyName("eval_count")]
            public int? EvalCount { get; set; }
        }
    }
}
