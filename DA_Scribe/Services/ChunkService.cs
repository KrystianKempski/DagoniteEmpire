using System.Text;
using System.Text.RegularExpressions;
using DA_Scribe.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace DA_Scribe.Services
{
    /// <summary>
    /// Service for splitting text into chunks suitable for embedding.
    /// Uses paragraph-aware chunking to maintain semantic coherence.
    /// </summary>
    public partial class ChunkService : IChunkService
    {
        private readonly ILogger<ChunkService> _logger;
        
        // Rough approximation: 1 token ≈ 4 characters for most languages
        // Polish tends to have slightly longer words, so we use 3.5
        private const double CharsPerToken = 3.5;
        
        public ChunkService(ILogger<ChunkService> logger)
        {
            _logger = logger;
        }
        
        public int EstimateTokenCount(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return 0;
            
            return (int)Math.Ceiling(text.Length / CharsPerToken);
        }
        
        public IList<string> ChunkText(string text, int maxTokens = 500, int overlapTokens = 50)
        {
            if (string.IsNullOrWhiteSpace(text))
                return new List<string>();
            
            var chunks = new List<string>();
            
            // Split into paragraphs first
            var paragraphs = SplitIntoParagraphs(text);
            
            var currentChunk = new StringBuilder();
            var currentTokens = 0;
            var overlapBuffer = new Queue<string>();
            var overlapTokenCount = 0;
            
            foreach (var paragraph in paragraphs)
            {
                var paraTokens = EstimateTokenCount(paragraph);
                
                // If single paragraph is too large, split it by sentences
                if (paraTokens > maxTokens)
                {
                    // Flush current chunk first
                    if (currentChunk.Length > 0)
                    {
                        chunks.Add(currentChunk.ToString().Trim());
                        currentChunk.Clear();
                        currentTokens = 0;
                    }
                    
                    // Split large paragraph by sentences
                    var sentenceChunks = ChunkBySentences(paragraph, maxTokens, overlapTokens);
                    chunks.AddRange(sentenceChunks);
                    
                    overlapBuffer.Clear();
                    overlapTokenCount = 0;
                    continue;
                }
                
                // Check if adding this paragraph would exceed limit
                if (currentTokens + paraTokens > maxTokens && currentChunk.Length > 0)
                {
                    // Save current chunk
                    chunks.Add(currentChunk.ToString().Trim());
                    
                    // Start new chunk with overlap
                    currentChunk.Clear();
                    currentTokens = 0;
                    
                    // Add overlap content
                    while (overlapBuffer.Count > 0 && overlapTokenCount > 0)
                    {
                        var overlapPara = overlapBuffer.Dequeue();
                        currentChunk.AppendLine(overlapPara);
                        currentTokens += EstimateTokenCount(overlapPara);
                    }
                    overlapBuffer.Clear();
                    overlapTokenCount = 0;
                }
                
                // Add paragraph to current chunk
                currentChunk.AppendLine(paragraph);
                currentTokens += paraTokens;
                
                // Maintain overlap buffer
                overlapBuffer.Enqueue(paragraph);
                overlapTokenCount += paraTokens;
                
                // Trim overlap buffer if too large
                while (overlapTokenCount > overlapTokens && overlapBuffer.Count > 1)
                {
                    var removed = overlapBuffer.Dequeue();
                    overlapTokenCount -= EstimateTokenCount(removed);
                }
            }
            
            // Don't forget the last chunk
            if (currentChunk.Length > 0)
            {
                chunks.Add(currentChunk.ToString().Trim());
            }
            
            _logger.LogDebug(
                "Chunked text into {ChunkCount} chunks (max {MaxTokens} tokens each)", 
                chunks.Count, 
                maxTokens);
            
            return chunks;
        }
        
        private IList<string> SplitIntoParagraphs(string text)
        {
            // Split by double line breaks or more
            var paragraphs = ParagraphSplitRegex()
                .Split(text)
                .Select(p => p.Trim())
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .ToList();
            
            return paragraphs;
        }
        
        private IList<string> ChunkBySentences(string paragraph, int maxTokens, int overlapTokens)
        {
            var chunks = new List<string>();
            
            // Split by sentence endings (handling Polish text with abbreviations)
            var sentences = SentenceSplitRegex()
                .Split(paragraph)
                .Select(s => s.Trim())
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToList();
            
            var currentChunk = new StringBuilder();
            var currentTokens = 0;
            
            foreach (var sentence in sentences)
            {
                var sentenceTokens = EstimateTokenCount(sentence);
                
                // If single sentence is too large, split by words
                if (sentenceTokens > maxTokens)
                {
                    if (currentChunk.Length > 0)
                    {
                        chunks.Add(currentChunk.ToString().Trim());
                        currentChunk.Clear();
                        currentTokens = 0;
                    }
                    
                    var wordChunks = ChunkByWords(sentence, maxTokens);
                    chunks.AddRange(wordChunks);
                    continue;
                }
                
                if (currentTokens + sentenceTokens > maxTokens && currentChunk.Length > 0)
                {
                    chunks.Add(currentChunk.ToString().Trim());
                    currentChunk.Clear();
                    currentTokens = 0;
                }
                
                currentChunk.Append(sentence).Append(' ');
                currentTokens += sentenceTokens;
            }
            
            if (currentChunk.Length > 0)
            {
                chunks.Add(currentChunk.ToString().Trim());
            }
            
            return chunks;
        }
        
        private IList<string> ChunkByWords(string text, int maxTokens)
        {
            var chunks = new List<string>();
            var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var maxChars = (int)(maxTokens * CharsPerToken);
            
            var currentChunk = new StringBuilder();
            
            foreach (var word in words)
            {
                if (currentChunk.Length + word.Length + 1 > maxChars && currentChunk.Length > 0)
                {
                    chunks.Add(currentChunk.ToString().Trim());
                    currentChunk.Clear();
                }
                
                currentChunk.Append(word).Append(' ');
            }
            
            if (currentChunk.Length > 0)
            {
                chunks.Add(currentChunk.ToString().Trim());
            }
            
            return chunks;
        }
        
        [GeneratedRegex(@"\n\s*\n+")]
        private static partial Regex ParagraphSplitRegex();
        
        // Sentence split that handles common abbreviations
        [GeneratedRegex(@"(?<=[.!?])\s+(?=[A-ZĄĆĘŁŃÓŚŹŻ])")]
        private static partial Regex SentenceSplitRegex();
    }
}
