# SCRIBE - AI Memory System for Dagonite Empire

## Overview

**SCRIBE** (Semantic Campaign Retrieval and Intelligence Based Explorer) is an AI-powered memory system that helps players and Game Masters quickly access information about characters, locations, adventures, and events from their RPG campaigns.

> **MVP status (2026-05-30).** The system is shipped as an MVP on branch
> `feature/scribe-semantic-kernel`. Beyond the original RAG flow described
> below, the live implementation now includes:
>
> - **Agentic layer** based on Microsoft Semantic Kernel
>   (`Microsoft.SemanticKernel` 1.77 + `Connectors.Ollama` alpha) with auto
>   tool-calling (`FunctionChoiceBehavior.Auto()`). Plugins:
>   `ScribeSearchPlugin` (semantic memory), `CharacterPlugin`, `ChapterPlugin`.
>   Default chat model: `qwen2.5:14b`.
> - **Structured citations** (`ScribeCitation`) surfaced from every
>   `search_memories` tool call – displayed in `ScribePage` / `ScribeDrawer`
>   as numbered `[1]..[N]` items with similarity score and content snippet
>   tooltip.
> - **Conversation history** persisted per-user with 14-day retention
>   (`ScribeRetentionService` hosted service).
> - **Hardening:** input validation (`[StringLength]`, `[Range]`,
>   `MaxImportChunks = 10_000`), `[Authorize]` on `/status`, unique partial
>   index on `ScribeMemory.SourcePostId`, per-user **and** per-IP rate
>   limiters (`scribe-query` 20/min, `scribe-ingest` 5/min), `JwtKey` moved
>   to user-secrets/env with fail-fast validation on startup.
> - **Observability:** `/health/scribe` readiness probe (Ollama + ChatModel
>   presence), OpenTelemetry tracing opt-in via `OTEL_EXPORTER_OTLP_ENDPOINT`
>   (ActivitySource `DagoniteEmpire.Scribe`, activities `scribe.agent.invoke`,
>   `scribe.search`, `scribe.embedding`), per-batch ingest progress logs.
> - **Throttling:** `ScribeOptions.Ingest.BatchSize` /
>   `BatchDelayMs` / `InterChapterDelayMs` for slow / shared GPU hosts.
>
> The `ILLMService` interface referenced in earlier drafts of this doc was
> removed; chat is now driven through Semantic Kernel’s
> `IChatCompletionService` (Ollama). Embeddings still flow through
> `IEmbeddingService` (`EmbeddingService` over a Polly-resilient named
> HttpClient `scribe-embedding`).

## Table of Contents

1. [What is RAG?](#what-is-rag)
2. [Architecture Overview](#architecture-overview)
3. [Technology Stack](#technology-stack)
4. [Data Model](#data-model)
5. [Implementation Phases](#implementation-phases)
6. [Access Control](#access-control)
7. [Local LLM Options](#local-llm-options)
8. [Getting Started](#getting-started)

---

## What is RAG?

**RAG (Retrieval-Augmented Generation)** is an AI architecture that combines:

1. **Vector Database**: Stores your text as numerical vectors (embeddings) that capture semantic meaning
2. **Embedding Model**: Converts text into vectors - similar concepts get similar vectors
3. **Retrieval System**: When you ask a question, finds the most relevant chunks of text
4. **LLM (Language Model)**: Generates human-readable answers based on the retrieved context

### Why RAG instead of just feeding everything to an LLM?

| Problem | RAG Solution |
|---------|--------------|
| LLMs have limited context windows (can't read 1000 pages at once) | RAG retrieves only relevant chunks (5-10 pieces) |
| LLMs can hallucinate (make things up) | RAG grounds answers in your actual content |
| Fine-tuning LLMs is expensive | RAG requires no model training |
| Data changes frequently | Just update the vector database |

### How RAG Works - Visual Flow

```
┌──────────────────────────────────────────────────────────────────────────────┐
│                           INGESTION PHASE (one-time)                         │
└──────────────────────────────────────────────────────────────────────────────┘

  ┌─────────────┐    ┌──────────────┐    ┌────────────────┐    ┌─────────────┐
  │   Posts     │───▶│   Chunker    │───▶│   Embedding    │───▶│   Vector    │
  │   Chapters  │    │ (split text) │    │     Model      │    │  Database   │
  │   NPCs      │    │              │    │ (text→vector)  │    │  (pgvector) │
  └─────────────┘    └──────────────┘    └────────────────┘    └─────────────┘
                           │                    │                     │
                     "Garrick entered     [0.12, -0.45,         Store with
                      the dark tavern..."   0.78, ...]           metadata


┌──────────────────────────────────────────────────────────────────────────────┐
│                           QUERY PHASE (every question)                       │
└──────────────────────────────────────────────────────────────────────────────┘

  ┌─────────────┐    ┌────────────────┐    ┌─────────────┐    ┌─────────────┐
  │   User      │───▶│   Embedding    │───▶│   Vector    │───▶│   Top 5     │
  │  Question   │    │     Model      │    │   Search    │    │   Chunks    │
  └─────────────┘    └────────────────┘    └─────────────┘    └─────────────┘
        │                    │                    │                  │
  "Who is Garrick?"   [0.15, -0.42,       Find similar          Relevant
                        0.80, ...]         vectors               passages

  ┌─────────────────────────────────────────────────────────────────────────────┐
  │                                                                             │
  │   ┌─────────────┐         ┌─────────────────────────────────────────────┐   │
  │   │   Top 5     │────────▶│                    LLM                      │   │
  │   │   Chunks    │         │                                             │   │
  │   └─────────────┘         │  Prompt:                                    │   │
  │                           │  "Based on this context: {chunks}           │   │
  │   ┌─────────────┐         │   Answer the question: Who is Garrick?"     │   │
  │   │   User      │────────▶│                                             │   │
  │   │  Question   │         │  Output: "Garrick is a human warrior        │   │
  │   └─────────────┘         │  who first appeared in Chapter 3..."        │   │
  │                           └─────────────────────────────────────────────┘   │
  │                                                                             │
  └─────────────────────────────────────────────────────────────────────────────┘
```

---

## Architecture Overview

### System Components

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                         DAGONITE EMPIRE APPLICATION                         │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  ┌────────────────────────────────────────────────────────────────────────┐ │
│  │                        PRESENTATION LAYER                              │ │
│  │  ┌─────────────────┐  ┌─────────────────┐  ┌───────────────────────┐   │ │
│  │  │  Scribe Chat    │  │  Memory Browser │  │  Summary Generator    │   │ │
│  │  │  Component      │  │  (search/view)  │  │  (chapter summaries)  │   │ │
│  │  └─────────────────┘  └─────────────────┘  └───────────────────────┘   │ │
│  └────────────────────────────────────────────────────────────────────────┘ │
│                                    │                                        │
│  ┌────────────────────────────────────────────────────────────────────────┐ │
│  │                         SCRIBE SERVICE LAYER                           │ │
│  │  ┌─────────────────┐  ┌─────────────────┐  ┌───────────────────────┐   │ │
│  │  │  IScribeService │  │  IMemoryService │  │  IEmbeddingService    │   │ │
│  │  │  (orchestrator) │  │  (CRUD memories)│  │  (text → vectors)     │   │ │
│  │  └─────────────────┘  └─────────────────┘  └───────────────────────┘   │ │
│  │  ┌─────────────────┐  ┌─────────────────┐  ┌───────────────────────┐   │ │
  │  │  │  IChunkService  │  │  Semantic Kernel│  │  IAccessControlSvc    │   │ │
  │  │  │  (text → chunks)│  │  (agent + LLM)  │  │  (permission filter)  │   │ │
│  │  └─────────────────┘  └─────────────────┘  └───────────────────────┘   │ │
│  └────────────────────────────────────────────────────────────────────────┘ │
│                                    │                                        │
│  ┌────────────────────────────────────────────────────────────────────────┐ │
│  │                          DATA ACCESS LAYER                             │ │
│  │  ┌─────────────────────────────────────────────────────────────────┐   │ │
│  │  │              PostgreSQL Database (existing)                     │   │ │
│  │  │  ┌───────────────┐  ┌───────────────┐  ┌─────────────────────┐  │   │ │
│  │  │  │    Posts      │  │   Chapters    │  │    Characters       │  │   │ │
│  │  │  └───────────────┘  └───────────────┘  └─────────────────────┘  │   │ │
│  │  │  ┌───────────────┐  ┌───────────────┐  ┌─────────────────────┐  │   │ │
│  │  │  │ ScribeMemory  │  │ ScribeChunk   │  │  ScribeConversation │  │   │ │
│  │  │  │ (NEW - RAG)   │  │ (NEW - embed) │  │  (NEW - chat hist)  │  │   │ │
│  │  │  └───────────────┘  └───────────────┘  └─────────────────────┘  │   │ │
│  │  └─────────────────────────────────────────────────────────────────┘   │ │
│  └────────────────────────────────────────────────────────────────────────┘ │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
                                     │
                                     ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                           EXTERNAL SERVICES                                 │
│  ┌─────────────────────────────────────────────────────────────────────┐    │
│  │                         OLLAMA (local)                               │    │
│  │  ┌───────────────────────┐  ┌─────────────────────────────────────┐  │    │
│  │  │  Embedding Model      │  │  LLM (Chat/Generation)              │  │    │
│  │  │  nomic-embed-text     │  │  llama3.2 / gemma2 / mistral        │  │    │
│  │  │  (768 dimensions)     │  │                                     │  │    │
│  │  └───────────────────────┘  └─────────────────────────────────────┘  │    │
│  └─────────────────────────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## Technology Stack

### Recommended Stack (fits your existing setup)

| Component | Technology | Reason |
|-----------|------------|--------|
| **Vector Database** | PostgreSQL + pgvector | You already use PostgreSQL! No new database needed |
| **Embedding Model** | Ollama + nomic-embed-text | Free, local, high quality, 768 dimensions |
| **LLM** | Ollama + llama3.2:8b or gemma2:9b | Free, local, runs on consumer GPU |
| **Orchestration** | Microsoft Semantic Kernel | .NET native, great Ollama integration |
| **API** | Your existing ASP.NET Core | Just add new services |

### Alternative Options

| Component | Alternatives | Trade-offs |
|-----------|--------------|------------|
| **Vector DB** | Qdrant, ChromaDB, Milvus | Separate service, more features, more complexity |
| **Embedding** | all-MiniLM-L6-v2, BGE | Smaller dimensions (384), slightly lower quality |
| **LLM** | Mistral, Phi-3, DeepSeek | Different performance/quality trade-offs |
| **Orchestration** | LangChain.NET, direct API calls | Less .NET integration, more flexibility |

### Hardware Requirements

| Model | VRAM Required | Quality |
|-------|---------------|---------|
| llama3.2:3b | 4 GB | Good for summaries |
| llama3.2:8b | 8 GB | Great balance |
| gemma2:9b | 8 GB | Excellent |
| gemma2:27b | 16+ GB | Best quality |
| mistral:7b | 8 GB | Fast, good quality |

---

## Data Model

### New Entities for SCRIBE

```csharp
// Core memory unit - extracted and processed content
public class ScribeMemory
{
    public int Id { get; set; }
    public string Title { get; set; }           // "Garrick meets the Black Dragon"
    public string Content { get; set; }          // Full processed text
    public MemoryType Type { get; set; }         // Character, Location, Event, Item, Quest
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Source tracking
    public int? SourcePostId { get; set; }
    public int? SourceChapterId { get; set; }
    public int? SourceCampaignId { get; set; }
    
    // Access control
    public ICollection<int> CharacterIds { get; set; }  // Characters who "know" this
    public bool IsPublic { get; set; }                   // GM-only if false
    
    // For LLM-generated summaries
    public bool IsGenerated { get; set; }
    public string? GeneratedBy { get; set; }             // Model name
}

// Chunked and embedded content for vector search
public class ScribeChunk
{
    public int Id { get; set; }
    public int ScribeMemoryId { get; set; }
    public string Content { get; set; }                  // Chunk text (500-1000 tokens)
    public Vector Embedding { get; set; }                // pgvector - float[768]
    public int ChunkIndex { get; set; }                  // Order within memory
    
    // Metadata for filtering
    public int? CampaignId { get; set; }
    public int? ChapterId { get; set; }
    public ICollection<int> CharacterIds { get; set; }   // For access control
}

// Chat history for contextual conversations  
public class ScribeConversation
{
    public int Id { get; set; }
    public string UserId { get; set; }
    public int? CharacterId { get; set; }
    public DateTime StartedAt { get; set; }
    public ICollection<ScribeMessage> Messages { get; set; }
}

public class ScribeMessage
{
    public int Id { get; set; }
    public int ConversationId { get; set; }
    public string Role { get; set; }          // "user" or "assistant"
    public string Content { get; set; }
    public DateTime Timestamp { get; set; }
    public string? SourceChunkIds { get; set; } // Which chunks were used
}

public enum MemoryType
{
    Character,      // NPC or PC descriptions
    Location,       // Places, cities, dungeons
    Event,          // Important happenings
    Item,           // Artifacts, weapons, etc.
    Quest,          // Objectives, missions
    Lore,           // World-building, history
    ChapterSummary, // Auto-generated summaries
    SessionNotes    // GM notes
}
```

### Database Migration (pgvector)

```sql
-- Enable pgvector extension
CREATE EXTENSION IF NOT EXISTS vector;

-- ScribeChunks table with vector column
CREATE TABLE "ScribeChunks" (
    "Id" SERIAL PRIMARY KEY,
    "ScribeMemoryId" INTEGER NOT NULL REFERENCES "ScribeMemories"("Id"),
    "Content" TEXT NOT NULL,
    "Embedding" vector(768) NOT NULL,  -- 768 dimensions for nomic-embed-text
    "ChunkIndex" INTEGER NOT NULL,
    "CampaignId" INTEGER,
    "ChapterId" INTEGER,
    "CharacterIds" INTEGER[]
);

-- Index for fast similarity search
CREATE INDEX idx_scribe_chunks_embedding 
ON "ScribeChunks" 
USING ivfflat ("Embedding" vector_cosine_ops)
WITH (lists = 100);

-- Filtered search example
SELECT c."Content", c."ScribeMemoryId", 
       1 - (c."Embedding" <=> @query_vector) as similarity
FROM "ScribeChunks" c
WHERE c."CampaignId" = @campaign_id
  AND (c."CharacterIds" && @user_character_ids OR @is_gm = true)
ORDER BY c."Embedding" <=> @query_vector
LIMIT 5;
```

---

## Implementation Phases

### Phase 1: Foundation (Week 1-2)

**Goal**: Set up infrastructure and basic ingestion

1. **Install Ollama on server**
   ```bash
   curl -fsSL https://ollama.com/install.sh | sh
   ollama pull nomic-embed-text
   ollama pull llama3.2:8b
   ```

2. **Add pgvector extension to PostgreSQL**

3. **Create new project structure**
   ```
   DA_Scribe/
   ├── DA_Scribe.csproj
   ├── Entities/
   │   ├── ScribeMemory.cs
   │   ├── ScribeChunk.cs
   │   └── ScribeConversation.cs
   ├── Services/
   │   ├── Interfaces/
   │   │   ├── IScribeService.cs
   │   │   ├── IEmbeddingService.cs
   │   │   ├── IChunkService.cs
   │   │   └── ILLMService.cs
   │   ├── EmbeddingService.cs
   │   ├── ChunkService.cs
   │   ├── LLMService.cs
   │   └── ScribeService.cs
   └── Repository/
       ├── ScribeMemoryRepository.cs
       └── ScribeChunkRepository.cs
   ```

4. **Implement basic embedding service**
   ```csharp
   public class EmbeddingService : IEmbeddingService
   {
       private readonly HttpClient _httpClient;
       private const string OllamaUrl = "http://localhost:11434";
       
       public async Task<float[]> GetEmbeddingAsync(string text)
       {
           var response = await _httpClient.PostAsJsonAsync(
               $"{OllamaUrl}/api/embeddings",
               new { model = "nomic-embed-text", prompt = text }
           );
           var result = await response.Content.ReadFromJsonAsync<EmbeddingResponse>();
           return result.Embedding;
       }
   }
   ```

### Phase 2: Ingestion Pipeline (Week 2-3)

**Goal**: Process existing content into chunks and embeddings

1. **Create chunking service**
   - Extract text from HTML (posts have HTML content)
   - Split into semantic chunks (paragraphs, ~500 tokens each)
   - Preserve context (chapter, characters involved)

2. **Build ingestion job**
   ```csharp
   public class IngestionService
   {
       public async Task IngestChapterAsync(int chapterId)
       {
           var chapter = await _chapterRepo.GetById(chapterId);
           var posts = await _postRepo.GetAllForChapter(chapterId);
           
           foreach (var post in posts)
           {
               var plainText = HtmlToText(post.Content);
               var chunks = _chunker.ChunkText(plainText, maxTokens: 500);
               
               foreach (var chunk in chunks)
               {
                   var embedding = await _embeddingService.GetEmbeddingAsync(chunk);
                   await _chunkRepo.SaveAsync(new ScribeChunk
                   {
                       Content = chunk,
                       Embedding = embedding,
                       CampaignId = chapter.CampaignId,
                       ChapterId = chapterId,
                       CharacterIds = new[] { post.CharacterId }
                   });
               }
           }
       }
   }
   ```

3. **Process your ~1000 pages**
   - Run batch job for existing chapters
   - Set up triggers for new posts

### Phase 3: Query System (Week 3-4)

**Goal**: Enable semantic search with access control

1. **Vector similarity search**
   ```csharp
   public async Task<IEnumerable<ScribeChunk>> SearchAsync(
       string query, 
       int userId, 
       int? characterId,
       int topK = 5)
   {
       var queryEmbedding = await _embeddingService.GetEmbeddingAsync(query);
       var userCharacterIds = await GetUserCharacterIds(userId);
       var isGM = await IsUserGM(userId);
       
       return await _db.ScribeChunks
           .Where(c => isGM || c.CharacterIds.Overlaps(userCharacterIds))
           .OrderBy(c => c.Embedding.CosineDistance(queryEmbedding))
           .Take(topK)
           .ToListAsync();
   }
   ```

2. **Build RAG pipeline**
   - Retrieve top chunks
   - Format prompt with context
   - Send to LLM
   - Return response

### Phase 4: Chat Interface (Week 4-5)

**Goal**: User-facing chat component

1. **Blazor chat component**
   - Message history display
   - Input field
   - Loading states
   - Source citations

2. **Conversation memory**
   - Track conversation context
   - Allow follow-up questions

### Phase 5: Advanced Features (Week 5+)

**Goal**: Summary generation, memory curation

1. **Auto-generate chapter summaries**
2. **NPC/Location extraction**
3. **GM tools for managing memories**
4. **Timeline view**

---

## Access Control

### Permission Model

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                           ACCESS CONTROL FLOW                               │
└─────────────────────────────────────────────────────────────────────────────┘

  User asks: "What happened in the Black Tower?"
                          │
                          ▼
              ┌───────────────────────┐
              │ Get User's Characters │
              │ (from session/claims) │
              └───────────────────────┘
                          │
                          ▼
              ┌───────────────────────┐
              │   Is User a GM?       │
              └───────────────────────┘
                    │         │
                   Yes        No
                    │         │
                    ▼         ▼
         ┌──────────────┐  ┌──────────────────────┐
         │  Return ALL  │  │  Filter chunks where │
         │   chunks     │  │  CharacterIds ∩      │
         │              │  │  UserCharacterIds    │
         └──────────────┘  │  is NOT empty        │
                           └──────────────────────┘
                                      │
                                      ▼
                           ┌──────────────────────┐
                           │  "Your character     │
                           │  wasn't present for  │
                           │  that event"         │
                           │  (if no results)     │
                           └──────────────────────┘
```

### Implementation

```csharp
public class AccessControlService : IAccessControlService
{
    public async Task<bool> CanAccessChunk(ScribeChunk chunk, UserInfo user)
    {
        // GM can access everything
        if (user.IsAdminOrMG) return true;
        
        // Public memories are accessible to all
        if (chunk.Memory.IsPublic) return true;
        
        // Check if user's character was involved
        var userCharacterIds = await GetUserCharacterIds(user.Id);
        return chunk.CharacterIds.Intersect(userCharacterIds).Any();
    }
    
    public async Task<string> GetAccessDeniedMessage(string query)
    {
        return $"Your character doesn't have knowledge about '{query}'. " +
               $"This information may have been revealed to other party members.";
    }
}
```

---

## Local LLM Options

### Comparison for Your Use Case

| Model | Size | Quality | Speed | Best For |
|-------|------|---------|-------|----------|
| **llama3.2:3b** | 2GB | Good | Fast | Quick answers, summaries |
| **llama3.2:8b** | 4.7GB | Great | Medium | Best balance ✓ |
| **gemma2:9b** | 5.4GB | Excellent | Medium | Rich storytelling |
| **gemma2:27b** | 16GB | Best | Slow | Complex reasoning |
| **mistral:7b** | 4.1GB | Great | Fast | Good alternative |
| **phi3:14b** | 8GB | Very Good | Medium | Instruction following |

### Recommended Configuration

```json
{
  "Scribe": {
    "Ollama": {
      "BaseUrl": "http://localhost:11434",
      "EmbeddingModel": "nomic-embed-text",
      "ChatModel": "llama3.2:8b",
      "Temperature": 0.7,
      "MaxTokens": 2048
    },
    "Chunking": {
      "MaxTokensPerChunk": 500,
      "OverlapTokens": 50
    },
    "Search": {
      "TopK": 5,
      "SimilarityThreshold": 0.7
    }
  }
}
```

---

## Getting Started

### Prerequisites

1. **Ollama** installed on your server
2. **PostgreSQL 15+** (for pgvector)
3. **.NET 8 SDK**

### Quick Start Commands

```bash
# 1. Install Ollama
curl -fsSL https://ollama.com/install.sh | sh

# 2. Pull required models
ollama pull nomic-embed-text
ollama pull llama3.2:8b

# 3. Enable pgvector in PostgreSQL
psql -d your_database -c "CREATE EXTENSION IF NOT EXISTS vector;"

# 4. Create the DA_Scribe project
dotnet new classlib -n DA_Scribe -o DA_Scribe
dotnet sln add DA_Scribe/DA_Scribe.csproj

# 5. Add NuGet packages
cd DA_Scribe
dotnet add package Microsoft.SemanticKernel
dotnet add package Microsoft.SemanticKernel.Connectors.Ollama
dotnet add package Pgvector
dotnet add package Pgvector.EntityFrameworkCore
```

### Next Steps

1. Review this architecture document
2. Confirm technology choices
3. Start with Phase 1 implementation
4. Process one campaign as proof of concept

---

## Configuration Decisions

| Aspect | Decision |
|--------|----------|
| **GPU** | ✅ Available on server |
| **Ollama location** | Same server, containerized (URL: `http://ollama:11434`) |
| **First campaign** | "Kraina Możliwości" (Polish) |
| **Source format** | Word documents (.docx) + Campaign posts |
| **Import method** | Manual upload via GM interface |
| **Future** | Automatic indexing of campaign threads |

## User Interface

### 1. SCRIBE Page (`/scribe`)
Full-featured interface for extended conversations with SCRIBE:
- Chat interface with message history
- Source citations with expandable details
- Campaign selector
- GM-only tools: document import, post indexing, summary generation

### 2. Quick Access Drawer
Compact chat accessible from any page via navbar button:
- Lightweight chat interface
- Link to open full SCRIBE page
- Context-aware (inherits current campaign if viewing chapter)

## Polish Language Support

For Polish language content, **gemma2:9b** is recommended as Google's models have excellent multilingual support. Alternative: `llama3.2:8b`.

```json
{
  "Scribe": {
    "Ollama": {
      "BaseUrl": "http://ollama:11434",
      "EmbeddingModel": "nomic-embed-text",
      "ChatModel": "gemma2:9b",
      "SystemPrompt": "Jesteś SCRIBE - pomocnikiem archiwistą w grze RPG. Odpowiadaj po polsku na podstawie dostarczonych fragmentów tekstu. Bądź zwięzły i precyzyjny."
    }
  }
}
```

## Word Document Ingestion

### Parsing .docx Files

Using `DocumentFormat.OpenXml` library to extract text:

```csharp
public class WordDocumentParser : IDocumentParser
{
    public async Task<string> ExtractTextAsync(Stream fileStream)
    {
        using var document = WordprocessingDocument.Open(fileStream, false);
        var body = document.MainDocumentPart?.Document?.Body;
        
        if (body == null) return string.Empty;
        
        var sb = new StringBuilder();
        foreach (var paragraph in body.Elements<Paragraph>())
        {
            sb.AppendLine(paragraph.InnerText);
        }
        return sb.ToString();
    }
}
```

### Import Workflow

```
┌──────────────────────────────────────────────────────────────────────────┐
│                         DOCUMENT IMPORT FLOW                             │
└──────────────────────────────────────────────────────────────────────────┘

  GM uploads .docx files            SCRIBE Page (/scribe)
  via browser                       ┌─────────────────────────────────────┐
  ┌─────────────────┐               │  GM Tools Panel                     │
  │  .docx          │  ──────────▶  │  ┌───────────────────────────────┐  │
  │  files          │               │  │ [Import Documents]            │  │
  │                 │               │  │                               │  │
  └─────────────────┘               │  │ Selected: chapter-01.docx     │  │
                                    │  │           chapter-02.docx     │  │
                                    │  │                               │  │
                                    │  │ Type: [Document ▼]            │  │
                                    │  │ [x] Public to all players     │  │
                                    │  │                               │  │
                                    │  │ [Import]                      │  │
                                    │  └───────────────────────────────┘  │
                                    └─────────────────────────────────────┘
                                                   │
                                                   ▼
                                    ┌─────────────────────────────────────┐
                                    │  Ingestion Pipeline:                │
                                    │  1. Parse .docx → plain text        │
                                    │  2. Chunk into ~500 token segments  │
                                    │  3. Generate embeddings (Ollama)    │
                                    │  4. Store in PostgreSQL (pgvector)  │
                                    └─────────────────────────────────────┘

  Future: Campaign Post Indexing
  ┌─────────────────────────────────────────────────────────────────────────┐
  │  [Index Campaign Posts] button will:                                   │
  │  1. Load all posts from campaign chapters                              │
  │  2. Extract text content (strip HTML)                                  │
  │  3. Associate with characters who were present                         │
  │  4. Chunk, embed, and store with access control                        │
  └─────────────────────────────────────────────────────────────────────────┘
```

## Implemented Components

### UI Components
- `/scribe` - Full SCRIBE page with chat and GM tools
- `ScribeDrawer.razor` - Quick-access chat drawer (navbar button)

### Services (DA_Scribe project)
- `ScribeService` - Main RAG orchestrator
- `EmbeddingService` - Ollama embedding client
- `LLMService` - Ollama chat client
- `ChunkService` - Text splitting
- `DocumentParserService` - Word document parser

### Entities
- `ScribeMemory` - Main knowledge unit
- `ScribeChunk` - Vector-embedded text fragment
- `ScribeConversation` - Chat session
- `ScribeMessage` - Individual message

## Questions Still Open

1. **UI preferences**: Any specific styling or behavior for the SCRIBE interface?

---

## Resources

- [Ollama Documentation](https://ollama.com/)
- [pgvector GitHub](https://github.com/pgvector/pgvector)
- [Microsoft Semantic Kernel](https://github.com/microsoft/semantic-kernel)
- [nomic-embed-text](https://ollama.com/library/nomic-embed-text)
- [Pgvector.EntityFrameworkCore](https://github.com/pgvector/pgvector-dotnet)
