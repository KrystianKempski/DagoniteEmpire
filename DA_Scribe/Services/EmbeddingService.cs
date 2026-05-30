using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using DA_Scribe.Configuration;
using DA_Scribe.Diagnostics;
using DA_Scribe.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DA_Scribe.Services
{
    /// <summary>
    /// Embedding service using Ollama API
    /// </summary>
    public class EmbeddingService : IEmbeddingService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<EmbeddingService> _logger;
        private readonly ScribeOptions _options;
        
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
        
        public EmbeddingService(
            HttpClient httpClient,
            ILogger<EmbeddingService> logger,
            IOptions<ScribeOptions> options)
        {
            _httpClient = httpClient;
            _logger = logger;
            _options = options.Value;
            
            _httpClient.BaseAddress = new Uri(_options.Ollama.BaseUrl);
            _httpClient.Timeout = TimeSpan.FromSeconds(_options.Ollama.TimeoutSeconds);
        }
        
        public async Task<float[]> GetEmbeddingAsync(string text, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                throw new ArgumentException("Text cannot be empty", nameof(text));
            }

            using var activity = ScribeTelemetry.ActivitySource.StartActivity("scribe.embedding");
            activity?.SetTag("scribe.embedding.model", _options.Ollama.EmbeddingModel);
            activity?.SetTag("scribe.embedding.text_length", text.Length);

            var request = new EmbeddingRequest
            {
                Model = _options.Ollama.EmbeddingModel,
                Prompt = text
            };
            
            try
            {
                var response = await _httpClient.PostAsJsonAsync(
                    "/api/embeddings", 
                    request, 
                    JsonOptions,
                    cancellationToken);
                
                response.EnsureSuccessStatusCode();
                
                var result = await response.Content.ReadFromJsonAsync<EmbeddingResponse>(
                    JsonOptions, 
                    cancellationToken);
                
                if (result?.Embedding == null || result.Embedding.Length == 0)
                {
                    throw new InvalidOperationException("Empty embedding returned from Ollama");
                }
                
                _logger.LogDebug(
                    "Generated embedding with {Dimensions} dimensions for text of {Length} chars",
                    result.Embedding.Length,
                    text.Length);
                
                return result.Embedding;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Failed to connect to Ollama at {BaseUrl}", _options.Ollama.BaseUrl);
                throw new InvalidOperationException(
                    $"Failed to connect to Ollama. Is it running at {_options.Ollama.BaseUrl}?", 
                    ex);
            }
            catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
            {
                _logger.LogError(ex, "Ollama request timed out");
                throw new TimeoutException("Ollama embedding request timed out", ex);
            }
        }
        
        public async Task<IList<float[]>> GetEmbeddingsAsync(
            IEnumerable<string> texts,
            CancellationToken cancellationToken = default)
        {
            // Ollama's /api/embeddings is single-prompt. We fan out with a small
            // degree of parallelism to keep latency reasonable on large imports
            // while not overwhelming a single remote GPU host. Order is preserved
            // via index-based assignment.
            var items = texts.ToList();
            var results = new float[items.Count][];

            var maxParallel = Math.Clamp(_options.Ollama.EmbeddingConcurrency, 1, 8);

            await Parallel.ForEachAsync(
                Enumerable.Range(0, items.Count),
                new ParallelOptions
                {
                    MaxDegreeOfParallelism = maxParallel,
                    CancellationToken = cancellationToken,
                },
                async (i, ct) =>
                {
                    results[i] = await GetEmbeddingAsync(items[i], ct);
                });

            return results;
        }
        
        public async Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _httpClient.GetAsync("/api/tags", cancellationToken);
                
                if (!response.IsSuccessStatusCode)
                    return false;
                
                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                
                // Check if embedding model is available
                return content.Contains(_options.Ollama.EmbeddingModel);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Ollama availability check failed");
                return false;
            }
        }
        
        // Request/Response DTOs for Ollama API
        
        private class EmbeddingRequest
        {
            public string Model { get; set; } = string.Empty;
            public string Prompt { get; set; } = string.Empty;
        }
        
        private class EmbeddingResponse
        {
            public float[] Embedding { get; set; } = Array.Empty<float>();
        }
    }
}
