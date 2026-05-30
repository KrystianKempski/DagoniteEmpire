namespace DA_Scribe.Models
{
    /// <summary>
    /// Structured citation collected from agent tool calls (search_memories results).
    /// Surfaced in the UI so the user can see exactly which archive fragments
    /// backed the assistant's answer.
    /// </summary>
    public sealed class ScribeCitation
    {
        public int MemoryId { get; set; }
        public int ChunkId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string MemoryType { get; set; } = string.Empty;
        public float Similarity { get; set; }
        public string Snippet { get; set; } = string.Empty;
        public int? SourcePostId { get; set; }
        public int? SourceChapterId { get; set; }
        public int? SourceCampaignId { get; set; }
    }
}
