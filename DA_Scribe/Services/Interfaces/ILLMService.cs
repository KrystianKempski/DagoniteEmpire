namespace DA_Scribe.Services.Interfaces
{
    /// <summary>
    /// Service for interacting with the LLM via Ollama
    /// </summary>
    public interface ILLMService
    {
        /// <summary>
        /// Generate a response from the LLM
        /// </summary>
        /// <param name="prompt">User prompt</param>
        /// <param name="context">Retrieved context chunks to include</param>
        /// <param name="systemPrompt">Optional system prompt override</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Generated response</returns>
        Task<string> GenerateResponseAsync(
            string prompt, 
            IEnumerable<string> context, 
            string? systemPrompt = null,
            CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Generate a response with streaming output
        /// </summary>
        /// <param name="prompt">User prompt</param>
        /// <param name="context">Retrieved context chunks</param>
        /// <param name="onToken">Callback for each token</param>
        /// <param name="systemPrompt">Optional system prompt override</param>
        /// <param name="cancellationToken">Cancellation token</param>
        IAsyncEnumerable<string> GenerateResponseStreamAsync(
            string prompt,
            IEnumerable<string> context,
            string? systemPrompt = null,
            CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Generate a summary of the given text
        /// </summary>
        /// <param name="text">Text to summarize</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Summary text</returns>
        Task<string> SummarizeAsync(string text, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Check if the LLM service is available
        /// </summary>
        Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Get the name of the model being used
        /// </summary>
        string ModelName { get; }
    }
}
