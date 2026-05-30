using DA_Scribe.Services.Interfaces;

namespace DA_Business.Tests.Scribe;

/// <summary>
/// Minimal stubs so we can construct a real ScribeService for tests that exercise
/// only conversation/lifecycle code paths (no embeddings or LLM calls).
/// Methods throw if accidentally invoked, which surfaces accidental dependencies.
/// </summary>
internal sealed class NoopEmbeddingService : IEmbeddingService
{
    private static float[] FakeVector() => Enumerable.Repeat(0.01f, 768).ToArray();

    public Task<float[]> GetEmbeddingAsync(string text, CancellationToken cancellationToken = default)
        => Task.FromResult(FakeVector());

    public Task<IList<float[]>> GetEmbeddingsAsync(IEnumerable<string> texts, CancellationToken cancellationToken = default)
        => Task.FromResult<IList<float[]>>(texts.Select(_ => FakeVector()).ToList());

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

internal sealed class NoopHttpClientFactory : IHttpClientFactory
{
    public HttpClient CreateClient(string name) => new HttpClient();
}
