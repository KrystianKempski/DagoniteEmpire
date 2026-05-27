namespace DA_Scribe.Services.Interfaces
{
    /// <summary>
    /// Result of parsing a document
    /// </summary>
    public class ParsedDocument
    {
        /// <summary>
        /// Original filename
        /// </summary>
        public string FileName { get; set; } = string.Empty;
        
        /// <summary>
        /// Extracted plain text content
        /// </summary>
        public string Content { get; set; } = string.Empty;
        
        /// <summary>
        /// Document title (if available from metadata)
        /// </summary>
        public string? Title { get; set; }
        
        /// <summary>
        /// Document author (if available from metadata)
        /// </summary>
        public string? Author { get; set; }
        
        /// <summary>
        /// When the document was created
        /// </summary>
        public DateTime? CreatedDate { get; set; }
        
        /// <summary>
        /// Estimated word count
        /// </summary>
        public int WordCount { get; set; }
    }
    
    /// <summary>
    /// Service for parsing documents (Word, PDF, etc.) into plain text
    /// </summary>
    public interface IDocumentParserService
    {
        /// <summary>
        /// Parse a Word document (.docx) from a stream
        /// </summary>
        /// <param name="stream">Document stream</param>
        /// <param name="fileName">Original filename</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Parsed document with extracted text</returns>
        Task<ParsedDocument> ParseWordDocumentAsync(
            Stream stream, 
            string fileName,
            CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Parse a Word document from a file path
        /// </summary>
        /// <param name="filePath">Path to .docx file</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Parsed document with extracted text</returns>
        Task<ParsedDocument> ParseWordDocumentAsync(
            string filePath,
            CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Check if a file is a supported document format
        /// </summary>
        /// <param name="fileName">Filename to check</param>
        /// <returns>True if supported</returns>
        bool IsSupported(string fileName);
        
        /// <summary>
        /// Get supported file extensions
        /// </summary>
        IEnumerable<string> SupportedExtensions { get; }
    }
}
