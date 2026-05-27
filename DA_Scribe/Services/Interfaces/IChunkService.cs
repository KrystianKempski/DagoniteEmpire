namespace DA_Scribe.Services.Interfaces
{
    /// <summary>
    /// Service for text chunking - splitting documents into smaller pieces for embedding
    /// </summary>
    public interface IChunkService
    {
        /// <summary>
        /// Split text into chunks suitable for embedding
        /// </summary>
        /// <param name="text">Text to chunk</param>
        /// <param name="maxTokens">Maximum tokens per chunk (default: 500)</param>
        /// <param name="overlapTokens">Tokens to overlap between chunks (default: 50)</param>
        /// <returns>List of text chunks</returns>
        IList<string> ChunkText(string text, int maxTokens = 500, int overlapTokens = 50);
        
        /// <summary>
        /// Estimate token count for a text string (rough approximation)
        /// </summary>
        /// <param name="text">Text to count</param>
        /// <returns>Estimated token count</returns>
        int EstimateTokenCount(string text);
    }
}
