using System.Text;
using System.Text.RegularExpressions;
using DA_Scribe.Services.Interfaces;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.Extensions.Logging;

namespace DA_Scribe.Services
{
    /// <summary>
    /// Parser for Word documents (.docx)
    /// </summary>
    public partial class DocumentParserService : IDocumentParserService
    {
        private readonly ILogger<DocumentParserService> _logger;
        
        private static readonly HashSet<string> _supportedExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".docx"
        };
        
        public IEnumerable<string> SupportedExtensions => _supportedExtensions;
        
        public DocumentParserService(ILogger<DocumentParserService> logger)
        {
            _logger = logger;
        }
        
        public bool IsSupported(string fileName)
        {
            var ext = Path.GetExtension(fileName);
            return _supportedExtensions.Contains(ext);
        }
        
        public async Task<ParsedDocument> ParseWordDocumentAsync(
            string filePath, 
            CancellationToken cancellationToken = default)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Document not found: {filePath}");
            }
            
            await using var stream = File.OpenRead(filePath);
            return await ParseWordDocumentAsync(stream, Path.GetFileName(filePath), cancellationToken);
        }
        
        public Task<ParsedDocument> ParseWordDocumentAsync(
            Stream stream, 
            string fileName,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Parsing Word document: {FileName}", fileName);
            
            try
            {
                using var document = WordprocessingDocument.Open(stream, false);
                var result = new ParsedDocument
                {
                    FileName = fileName
                };
                
                // Extract metadata
                var coreProps = document.PackageProperties;
                result.Title = coreProps.Title;
                result.Author = coreProps.Creator;
                result.CreatedDate = coreProps.Created;
                
                // Extract body text
                var body = document.MainDocumentPart?.Document?.Body;
                if (body != null)
                {
                    var sb = new StringBuilder();
                    ExtractText(body, sb);
                    result.Content = CleanText(sb.ToString());
                }
                
                // Count words (rough estimate)
                result.WordCount = CountWords(result.Content);
                
                _logger.LogInformation(
                    "Parsed document {FileName}: {WordCount} words", 
                    fileName, 
                    result.WordCount);
                
                return Task.FromResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing Word document: {FileName}", fileName);
                throw;
            }
        }
        
        private void ExtractText(OpenXmlElement element, StringBuilder sb)
        {
            foreach (var child in element.ChildElements)
            {
                switch (child)
                {
                    case Paragraph paragraph:
                        var paraText = paragraph.InnerText;
                        if (!string.IsNullOrWhiteSpace(paraText))
                        {
                            sb.AppendLine(paraText);
                        }
                        sb.AppendLine(); // Extra line break between paragraphs
                        break;
                        
                    case Table table:
                        ExtractTableText(table, sb);
                        break;
                        
                    default:
                        // Recurse into other elements
                        if (child.HasChildren)
                        {
                            ExtractText(child, sb);
                        }
                        break;
                }
            }
        }
        
        private void ExtractTableText(Table table, StringBuilder sb)
        {
            foreach (var row in table.Elements<TableRow>())
            {
                var cells = new List<string>();
                foreach (var cell in row.Elements<TableCell>())
                {
                    cells.Add(cell.InnerText.Trim());
                }
                sb.AppendLine(string.Join(" | ", cells));
            }
            sb.AppendLine();
        }
        
        private static string CleanText(string text)
        {
            // Remove multiple consecutive line breaks
            text = MultipleLineBreaksRegex().Replace(text, "\n\n");
            
            // Remove multiple consecutive spaces
            text = MultipleSpacesRegex().Replace(text, " ");
            
            // Trim each line
            var lines = text.Split('\n')
                .Select(l => l.Trim())
                .ToArray();
            
            return string.Join("\n", lines).Trim();
        }
        
        private static int CountWords(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return 0;
            
            return WordBoundaryRegex().Matches(text).Count;
        }
        
        [GeneratedRegex(@"\n{3,}")]
        private static partial Regex MultipleLineBreaksRegex();
        
        [GeneratedRegex(@" {2,}")]
        private static partial Regex MultipleSpacesRegex();
        
        [GeneratedRegex(@"\b\w+\b")]
        private static partial Regex WordBoundaryRegex();
    }
}
