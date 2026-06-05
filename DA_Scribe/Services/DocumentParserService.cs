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
    /// Parser for Word documents (.docx) with character color detection
    /// </summary>
    public partial class DocumentParserService : IDocumentParserService
    {
        private readonly ILogger<DocumentParserService> _logger;
        
        private static readonly HashSet<string> _supportedExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".docx"
        };
        
        /// <summary>
        /// Maps text color hex codes to character names.
        /// Based on campaign convention where each player uses a specific color.
        /// </summary>
        private static readonly Dictionary<string, string> CharacterColors = new(StringComparer.OrdinalIgnoreCase)
        {
            { "b45f06", "Udar" },
            { "ff9900", "Udar" },
            { "38761d", "Tomin" },
            { "274e13", "Tomin" },
            { "0000ff", "Granit" },
            { "4a86e8", "Granit" },
            { "ff0000", "Glorio" },
            { "c27ba0", "Bjorn" },
            { "a64d79", "Bjorn" },
            { "980000", "Sharu" },
            { "990000", "Sharu" },
            { "cc0000", "Sharu" },
            { "5b0f00", "Sharu" },
            { "6fa8dc", "Sharu" },
            { "660000", "Sir Cedrick" },
        };
        
        /// <summary>
        /// Colors to ignore (MG black, headers, NPC dialogue colours)
        /// </summary>
        private static readonly HashSet<string> IgnoredColors = new(StringComparer.OrdinalIgnoreCase)
        {
            "0000ee", "000000", "1155cc", "auto",
            "674ea7", "351c75", "20124d",
            "e69138", "783f04", "434343", "85200c", "a61c00",
            "666666", "999999", "d5a6bd", "741b47", "e06666",
            "b7b7b7", "6aa84f", "1c4587",
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
                
                // Extract body text with color detection
                var body = document.MainDocumentPart?.Document?.Body;
                if (body != null)
                {
                    var plainSb = new StringBuilder();
                    var annotatedSb = new StringBuilder();
                    var characterTextLengths = new Dictionary<string, int>();
                    
                    ExtractTextWithColors(body, plainSb, annotatedSb, result.CharactersPresent, characterTextLengths);
                    
                    result.Content = CleanText(plainSb.ToString());
                    result.ContentAnnotated = CleanText(annotatedSb.ToString());
                    
                    // Determine POV character (most text)
                    if (characterTextLengths.Count > 0)
                    {
                        result.PovCharacter = characterTextLengths.MaxBy(kvp => kvp.Value).Key;
                    }
                    
                    // Detect dialogue and game mechanics
                    result.HasDialogue = DialogueRegex().IsMatch(result.Content);
                    result.HasGameMechanics = GameMechanicsRegex().IsMatch(result.Content);
                }
                
                // Count words (rough estimate)
                result.WordCount = CountWords(result.Content);
                
                _logger.LogInformation(
                    "Parsed document {FileName}: {WordCount} words, {CharCount} characters detected", 
                    fileName, 
                    result.WordCount,
                    result.CharactersPresent.Count);
                
                return Task.FromResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing Word document: {FileName}", fileName);
                throw;
            }
        }
        
        private void ExtractTextWithColors(
            OpenXmlElement element, 
            StringBuilder plainSb, 
            StringBuilder annotatedSb,
            HashSet<string> charactersFound,
            Dictionary<string, int> characterTextLengths)
        {
            foreach (var child in element.ChildElements)
            {
                switch (child)
                {
                    case Paragraph paragraph:
                        ExtractParagraphWithColors(paragraph, plainSb, annotatedSb, charactersFound, characterTextLengths);
                        break;
                        
                    case Table table:
                        ExtractTableText(table, plainSb);
                        annotatedSb.Append(plainSb.ToString().Split('\n').Last());
                        break;
                        
                    default:
                        if (child.HasChildren)
                        {
                            ExtractTextWithColors(child, plainSb, annotatedSb, charactersFound, characterTextLengths);
                        }
                        break;
                }
            }
        }
        
        private void ExtractParagraphWithColors(
            Paragraph paragraph,
            StringBuilder plainSb,
            StringBuilder annotatedSb,
            HashSet<string> charactersFound,
            Dictionary<string, int> characterTextLengths)
        {
            var paraPlain = new StringBuilder();
            var paraAnnotated = new StringBuilder();
            
            foreach (var run in paragraph.Elements<Run>())
            {
                var text = run.InnerText;
                if (string.IsNullOrEmpty(text))
                    continue;
                
                paraPlain.Append(text);
                
                // Get color from run properties
                var runProps = run.RunProperties;
                var color = runProps?.Color?.Val?.Value;
                
                string? character = null;
                if (!string.IsNullOrEmpty(color) && !IgnoredColors.Contains(color))
                {
                    CharacterColors.TryGetValue(color, out character);
                    // 660000 = Sir Cedrick; Triaxianka is Sharu in disguise (Akt 8)
                    if (character == "Sir Cedrick"
                        && TriaxiankaKeywordRegex().IsMatch(paraPlain.ToString() + text))
                    {
                        character = "Sharu";
                    }
                }
                
                if (character != null)
                {
                    charactersFound.Add(character);
                    paraAnnotated.Append($"[{character}]{text}");
                    
                    // Track text length per character
                    if (!characterTextLengths.ContainsKey(character))
                        characterTextLengths[character] = 0;
                    characterTextLengths[character] += text.Length;
                }
                else
                {
                    paraAnnotated.Append(text);
                }
            }
            
            var plainText = paraPlain.ToString();
            if (!string.IsNullOrWhiteSpace(plainText))
            {
                plainSb.AppendLine(plainText);
                annotatedSb.AppendLine(paraAnnotated.ToString());
            }
            plainSb.AppendLine();
            annotatedSb.AppendLine();
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
        
        /// <summary>Detects dialogue lines (starting with dash/em-dash)</summary>
        [GeneratedRegex(@"^[\-–—]", RegexOptions.Multiline)]
        private static partial Regex DialogueRegex();
        
        /// <summary>Detects game mechanics (dice rolls, tests, skill checks in Polish)</summary>
        [GeneratedRegex(@"\((?:test|rzut|trafienie|obrażenia|inicjatywa|spostrzegawczość|siła|zręczność|wytrzymałość|inteligencja|mądrość|charyzma|atletyka|akrobatyka|percepcja|skradanie|perswazja|zastraszanie|oszustwo|wnikliwość|natura|religia|medycyna|przetrwanie|historia|arkana|vs|sprawność|biegłość|mod|bonus|kość|k\d+|d\d+)", RegexOptions.IgnoreCase)]
        private static partial Regex GameMechanicsRegex();

        /// <summary>Triaxianka reused Sir Cedrick colour (#660000) in Akt 8</summary>
        [GeneratedRegex(@"triaxianka|dertu\s+terh|dawhar|musheee\s+mojha", RegexOptions.IgnoreCase)]
        private static partial Regex TriaxiankaKeywordRegex();
    }
}
