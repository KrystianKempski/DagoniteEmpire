using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using DA_Scribe.Configuration;
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
            var results = new List<float[]>();
            
            // Ollama doesn't support batch embeddings yet, so we process one at a time
            // Consider parallel processing with semaphore for better performance
            foreach (var text in texts)
            {
                cancellationToken.ThrowIfCancellationRequested();
                
                var embedding = await GetEmbeddingAsync(text, cancellationToken);
                results.Add(embedding);
            }
            
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
