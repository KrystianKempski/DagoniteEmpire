using DA_Scribe.Configuration;
using DA_Scribe.Kernel;
using DA_Scribe.Services;
using DA_Scribe.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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

            // Embedding service still uses typed HttpClient (raw Ollama embedding API)
            services.AddHttpClient<IEmbeddingService, EmbeddingService>();

            // Ensure IHttpClientFactory is available for LLMService + kernel factory
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
    }
}

