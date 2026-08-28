using Markdig;

namespace DA_Common.Barony
{
    /// <summary>Safe markdown → HTML for barony narrative text (hall events, etc.).</summary>
    public static class BaronyMarkdown
    {
        private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder()
            .UseAdvancedExtensions()
            .DisableHtml()
            .Build();

        public static string ToHtml(string? markdown)
        {
            if (string.IsNullOrWhiteSpace(markdown))
                return string.Empty;

            return Markdown.ToHtml(markdown.Trim(), Pipeline);
        }
    }
}
