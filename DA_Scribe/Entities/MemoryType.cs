namespace DA_Scribe.Entities
{
    /// <summary>
    /// Type of memory stored in SCRIBE
    /// </summary>
    public enum MemoryType
    {
        /// <summary>NPC or PC descriptions</summary>
        Character = 0,
        
        /// <summary>Places, cities, dungeons</summary>
        Location = 1,
        
        /// <summary>Important happenings</summary>
        Event = 2,
        
        /// <summary>Artifacts, weapons, etc.</summary>
        Item = 3,
        
        /// <summary>Objectives, missions</summary>
        Quest = 4,
        
        /// <summary>World-building, history</summary>
        Lore = 5,
        
        /// <summary>Auto-generated chapter summaries</summary>
        ChapterSummary = 6,
        
        /// <summary>GM notes</summary>
        SessionNotes = 7,
        
        /// <summary>Raw imported document content</summary>
        Document = 8
    }
}
