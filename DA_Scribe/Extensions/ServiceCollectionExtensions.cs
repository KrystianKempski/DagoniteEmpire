using DA_Scribe.Configuration;
using DA_Scribe.Kernel;
using DA_Scribe.Services;
using DA_Scribe.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Polly;

namespace DA_Scribe.Extensions
{
    /// <summary>
    /// Extension methods for registering SCRIBE services
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Add SCRIBE services to the dependency injection container
        /// </summary>
        public static IServiceCollection AddScribe(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Bind configuration
            services.Configure<ScribeOptions>(
                configuration.GetSection(ScribeOptions.SectionName));

            var scribeOptions = configuration
                .GetSection(ScribeOptions.SectionName)
                .Get<ScribeOptions>() ?? new ScribeOptions();

            // Embedding service still uses typed HttpClient (raw Ollama embedding API).
            // Embeddings are short calls -> aggressive retry on transient failures.
            services
                .AddHttpClient<IEmbeddingService, EmbeddingService>()
                .AddResilienceHandler("scribe-embedding", b => ConfigureOllamaResilience(
                    b,
                    perAttemptSeconds: Math.Min(scribeOptions.Ollama.TimeoutSeconds, 60),
                    totalSeconds: scribeOptions.Ollama.TimeoutSeconds * 2,
                    maxRetryAttempts: 3));

            // Named clients used by LLMService and ScribeKernelFactory for chat completion.
            // Chat calls can be slow (large model + tool calls) -> long timeouts, fewer retries.
            services
                .AddHttpClient(nameof(LLMService))
                .AddResilienceHandler("scribe-llm", b => ConfigureOllamaResilience(
                    b,
                    perAttemptSeconds: scribeOptions.Ollama.TimeoutSeconds,
                    totalSeconds: scribeOptions.Ollama.TimeoutSeconds * 3,
                    maxRetryAttempts: 2));

            services
                .AddHttpClient(nameof(ScribeKernelFactory))
                .AddResilienceHandler("scribe-kernel", b => ConfigureOllamaResilience(
                    b,
                    perAttemptSeconds: scribeOptions.Ollama.TimeoutSeconds * 2,
                    totalSeconds: scribeOptions.Ollama.TimeoutSeconds * 4,
                    maxRetryAttempts: 2));

            // Fallback default client (used elsewhere if anyone calls CreateClient() w/o a name).
            services.AddHttpClient();

            // Semantic Kernel factory (one kernel per app, but lightweight to build)
            services.AddSingleton<IScribeKernelFactory, ScribeKernelFactory>();

            // LLM service now backed by Semantic Kernel
            services.AddSingleton<ILLMService, LLMService>();

            services.AddSingleton<IChunkService, ChunkService>();
            services.AddSingleton<IDocumentParserService, DocumentParserService>();

            // Main SCRIBE service
            services.AddScoped<IScribeService, ScribeService>();

            return services;
        }

        private static void ConfigureOllamaResilience(
            ResiliencePipelineBuilder<HttpResponseMessage> builder,
            int perAttemptSeconds,
            int totalSeconds,
            int maxRetryAttempts)
        {
            // Total request timeout (outer)
            builder.AddTimeout(TimeSpan.FromSeconds(totalSeconds));

            // Retry transient failures (5xx, 408, network errors) with exponential backoff + jitter
            builder.AddRetry(new HttpRetryStrategyOptions
            {
                MaxRetryAttempts = maxRetryAttempts,
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true,
                Delay = TimeSpan.FromSeconds(2),
            });

            // Per-attempt timeout (inner) - guards against hung connections to a remote GPU host
            builder.AddTimeout(TimeSpan.FromSeconds(perAttemptSeconds));
        }
    }
}


