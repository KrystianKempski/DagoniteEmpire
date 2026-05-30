namespace DA_Scribe.Configuration
{
    /// <summary>
    /// Configuration options for SCRIBE
    /// </summary>
    public class ScribeOptions
    {
        public const string SectionName = "Scribe";
        
        /// <summary>
        /// Ollama configuration
        /// </summary>
        public OllamaOptions Ollama { get; set; } = new();
        
        /// <summary>
        /// Chunking configuration
        /// </summary>
        public ChunkingOptions Chunking { get; set; } = new();
        
        /// <summary>
        /// Search configuration
        /// </summary>
        public SearchOptions Search { get; set; } = new();
    }
    
    /// <summary>
    /// Ollama service configuration
    /// </summary>
    public class OllamaOptions
    {
        /// <summary>
        /// Base URL for Ollama API (e.g., "http://ollama:11434")
        /// </summary>
        public string BaseUrl { get; set; } = "http://localhost:11434";
        
        /// <summary>
        /// Model to use for embeddings (e.g., "nomic-embed-text")
        /// </summary>
        public string EmbeddingModel { get; set; } = "nomic-embed-text";
        
        /// <summary>
        /// Model to use for chat/generation (e.g., "qwen2.5:14b").
        /// MUST support tool/function calling for the agentic Skryba.
        /// Verified tool-calling models: qwen2.5, llama3.1, mistral-small, mistral-nemo.
        /// NOT supported: gemma2 (no tools), llama3.2 (limited).
        /// </summary>
        public string ChatModel { get; set; } = "qwen2.5:14b";
        
        /// <summary>
        /// Temperature for generation (0.0 = deterministic, 1.0 = creative)
        /// </summary>
        public float Temperature { get; set; } = 0.7f;
        
        /// <summary>
        /// Maximum tokens to generate in response
        /// </summary>
        public int MaxTokens { get; set; } = 2048;
        
        /// <summary>
        /// Path to the persona file for SCRIBE (relative to app root)
        /// If not found, falls back to SystemPrompt
        /// </summary>
        public string PersonaFilePath { get; set; } = "Resources/scribe-persona.md";
        
        /// <summary>
        /// Fallback system prompt if persona file is not found
        /// </summary>
        public string SystemPrompt { get; set; } = 
            "Jesteś archiwistą w grze RPG. Odpowiadaj po polsku na podstawie " +
            "dostarczonych fragmentów. Bądź zwięzły. Nie wymyślaj informacji.";
        
        /// <summary>
        /// Timeout in seconds for Ollama requests
        /// </summary>
        public int TimeoutSeconds { get; set; } = 120;

        /// <summary>
        /// Max number of concurrent embedding requests issued against Ollama.
        /// Bumps batch throughput on imports without overwhelming a single GPU host.
        /// Clamped to [1, 8].
        /// </summary>
        public int EmbeddingConcurrency { get; set; } = 3;
    }
    
    /// <summary>
    /// Text chunking configuration
    /// </summary>
    public class ChunkingOptions
    {
        /// <summary>
        /// Maximum tokens per chunk
        /// </summary>
        public int MaxTokensPerChunk { get; set; } = 500;
        
        /// <summary>
        /// Tokens to overlap between chunks (for context continuity)
        /// </summary>
        public int OverlapTokens { get; set; } = 50;
    }
    
    /// <summary>
    /// Vector search configuration
    /// </summary>
    public class SearchOptions
    {
        /// <summary>
        /// Number of top results to retrieve
        /// </summary>
        public int TopK { get; set; } = 5;
        
        /// <summary>
        /// Minimum similarity threshold (0.0 - 1.0)
        /// </summary>
        public float SimilarityThreshold { get; set; } = 0.5f;
    }
}
