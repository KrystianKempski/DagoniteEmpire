using DA_Scribe.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace DA_Scribe.HealthChecks
{
    /// <summary>
    /// Readiness probe for the Scribe stack: confirms the remote Ollama host is
    /// reachable and the configured chat model is installed. Uses the
    /// 'scribe-ollama-health' named HttpClient which has a short (<= 10s) timeout
    /// so a slow GPU host cannot stall the entire /health response.
    /// </summary>
    public sealed class ScribeOllamaHealthCheck : IHealthCheck
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ScribeOptions _options;

        public ScribeOllamaHealthCheck(
            IHttpClientFactory httpClientFactory,
            IOptions<ScribeOptions> options)
        {
            _httpClientFactory = httpClientFactory;
            _options = options.Value;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            var chatModel = _options.Ollama.ChatModel;
            try
            {
                var client = _httpClientFactory.CreateClient("scribe-ollama-health");
                using var response = await client.GetAsync("/api/tags", cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    return HealthCheckResult.Unhealthy(
                        $"Ollama /api/tags returned {(int)response.StatusCode}");
                }

                var body = await response.Content.ReadAsStringAsync(cancellationToken);
                if (!body.Contains(chatModel, StringComparison.OrdinalIgnoreCase))
                {
                    return HealthCheckResult.Degraded(
                        $"Ollama reachable but chat model '{chatModel}' not installed");
                }

                return HealthCheckResult.Healthy(
                    $"Ollama reachable, chat model '{chatModel}' available");
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy(
                    $"Ollama unreachable: {ex.GetType().Name}", ex);
            }
        }
    }
}
