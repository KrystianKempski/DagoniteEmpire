using DA_Scribe.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;

namespace DA_Scribe.Kernel
{
    public interface IScribeKernelFactory
    {
        Microsoft.SemanticKernel.Kernel Create();
    }

    public class ScribeKernelFactory : IScribeKernelFactory
    {
        private readonly ScribeOptions _options;
        private readonly ILoggerFactory _loggerFactory;
        private readonly HttpClient _httpClient;

        public ScribeKernelFactory(
            IOptions<ScribeOptions> options,
            ILoggerFactory loggerFactory,
            IHttpClientFactory httpClientFactory)
        {
            _options = options.Value;
            _loggerFactory = loggerFactory;
            _httpClient = httpClientFactory.CreateClient(nameof(ScribeKernelFactory));
            _httpClient.BaseAddress = new Uri(_options.Ollama.BaseUrl);
            _httpClient.Timeout = TimeSpan.FromSeconds(_options.Ollama.TimeoutSeconds * 2);
        }

        public Microsoft.SemanticKernel.Kernel Create()
        {
            var builder = Microsoft.SemanticKernel.Kernel.CreateBuilder();

            builder.Services.AddSingleton(_loggerFactory);

            builder.AddOllamaChatCompletion(
                modelId: _options.Ollama.ChatModel,
                httpClient: _httpClient);

            builder.AddOllamaEmbeddingGenerator(
                modelId: _options.Ollama.EmbeddingModel,
                httpClient: _httpClient);

            return builder.Build();
        }
    }
}
