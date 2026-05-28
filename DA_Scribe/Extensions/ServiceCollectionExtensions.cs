using DA_Scribe.Configuration;
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
        /// <param name="services">Service collection</param>
        /// <param name="configuration">Configuration</param>
        /// <returns>Service collection for chaining</returns>
        public static IServiceCollection AddScribe(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Bind configuration
            services.Configure<ScribeOptions>(
                configuration.GetSection(ScribeOptions.SectionName));
            
            // Register HTTP clients for Ollama services
            services.AddHttpClient<IEmbeddingService, EmbeddingService>();
            services.AddHttpClient<ILLMService, LLMService>();
            
            // Register other services
            services.AddSingleton<IChunkService, ChunkService>();
            services.AddSingleton<IDocumentParserService, DocumentParserService>();
            
            // Main SCRIBE service
            services.AddScoped<IScribeService, ScribeService>();
            
            return services;
        }
    }
}
