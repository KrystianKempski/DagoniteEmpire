using DA_Scribe.Services.Interfaces;

namespace DA_Business.Tests.Scribe;

/// <summary>
/// Minimal stubs so we can construct a real ScribeService for tests that exercise
/// only conversation/lifecycle code paths (no embeddings or LLM calls).
/// Methods throw if accidentally invoked, which surfaces accidental dependencies.
/// </summary>
internal sealed class NoopEmbeddingService : IEmbeddingService
{
    public Task<float[]> GetEmbeddingAsync(string text, CancellationToken cancellationToken = default)
        => throw new NotSupportedException("Embeddings not available in unit tests");

    public Task<IList<float[]>> GetEmbeddingsAsync(IEnumerable<string> texts, CancellationToken cancellationToken = default)
        => throw new NotSupportedException("Embeddings not available in unit tests");

    public Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(false);
}

internal sealed class NoopLLMService : ILLMService
{
    public string ModelName => "noop";

    public Task<string> GenerateResponseAsync(string prompt, IEnumerable<string> context, string? systemPrompt = null, CancellationToken cancellationToken = default)
        => throw new NotSupportedException("LLM not available in unit tests");

    public IAsyncEnumerable<string> GenerateResponseStreamAsync(string prompt, IEnumerable<string> context, string? systemPrompt = null, CancellationToken cancellationToken = default)
        => throw new NotSupportedException("LLM not available in unit tests");

    public Task<string> SummarizeAsync(string text, CancellationToken cancellationToken = default)
        => throw new NotSupportedException("LLM not available in unit tests");

    public Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(false);
}

internal sealed class NoopChunkService : IChunkService
{
    public IList<string> ChunkText(string text, int maxTokens = 500, int overlapTokens = 50)
        => new List<string> { text };

    public int EstimateTokenCount(string text) => text.Length / 4;
}

internal sealed class NoopDocumentParserService : IDocumentParserService
{
    public IEnumerable<string> SupportedExtensions => new[] { ".docx" };

    public bool IsSupported(string fileName) => false;

    public Task<ParsedDocument> ParseWordDocumentAsync(Stream stream, string fileName, CancellationToken cancellationToken = default)
        => throw new NotSupportedException();

    public Task<ParsedDocument> ParseWordDocumentAsync(string filePath, CancellationToken cancellationToken = default)
        => throw new NotSupportedException();
}
