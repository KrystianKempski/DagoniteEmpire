namespace DA_Scribe.Services.Interfaces
{
    /// <summary>
    /// Service for generating text embeddings using Ollama
    /// </summary>
    public interface IEmbeddingService
    {
        /// <summary>
        /// Generate embedding vector for a text string
        /// </summary>
        /// <param name="text">Text to embed</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Embedding vector (768 dimensions for nomic-embed-text)</returns>
        Task<float[]> GetEmbeddingAsync(string text, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Generate embeddings for multiple texts in batch
        /// </summary>
        /// <param name="texts">Texts to embed</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of embedding vectors</returns>
        Task<IList<float[]>> GetEmbeddingsAsync(IEnumerable<string> texts, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Check if the embedding service is available
        /// </summary>
        Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default);
    }
}
