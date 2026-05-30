# SCRIBE - Quick Reference

> **MVP status (stan na 2026-05-30):** Skryba działa w trybie agentowym (Semantic
> Kernel + Ollama, model `qwen2.5:14b`), streaming domyślnie ON, cytowania
> wyświetlane jako numerowane fragmenty z tooltipem. Rate-limit per-user (z
> fallbackiem per-IP), `/health/scribe` jako readiness Ollamy, OpenTelemetry
> opt-in. Endpointy import/query wymagają auth + roli `GameMaster`/`Admin`.

## Endpointy

| Endpoint | Method | Auth | Limit | Opis |
|----------|--------|------|-------|------|
| `/api/scribe/status` | GET | `[Authorize]` | `scribe-query` | Status Ollama + załadowanego modelu |
| `/api/scribe/search` | POST | `GameMaster,Admin` | `scribe-query` | Vector search (bez LLM) |
| `/api/scribe/query` | POST | `GameMaster,Admin` | `scribe-query` | Klasyczny RAG (jednorazowa odpowiedź LLM) |
| `/api/scribe/agent/query` | POST | `Authorize` | `scribe-query` | Agent z tool-callingiem (cytowania + historia) |
| `/api/scribe/import/{cid}` | POST | `GameMaster,Admin` | `scribe-ingest` | Import multipart (do 10 000 chunków) |
| `/api/scribe/import-json/{cid}` | POST | `GameMaster,Admin` | `scribe-ingest` | Import JSON body (do 10 000 chunków) |
| `/api/scribe/ingest/chapter/{id}` | POST | `GameMaster,Admin` | `scribe-ingest` | Reindeks postów rozdziału |
| `/api/scribe/ingest/campaign/{id}` | POST | `GameMaster,Admin` | `scribe-ingest` | Reindeks postów całej kampanii (batched) |
| `/healthz` | GET | anon | – | Liveness (DB) |
| `/health/scribe` | GET | anon | – | Readiness Ollama (tag `scribe`) |

### Limity request body

`ScribeQueryRequest` (używany przez `/query` i `/agent/query`) jest walidowany
automatycznie przez `[ApiController]`:

| Pole | Wymagania |
|------|-----------|
| `Query` | `[Required]`, długość 1–4000 znaków |
| `TopK` | `[Range(1, 20)]` (opcjonalne) |

Endpointy `import*` odrzucają payload powyżej `MaxImportChunks = 10 000`
chunków (BadRequest 400).

## Rate limiting

| Polityka | Limit | Partycjonowanie |
|----------|-------|-----------------|
| `scribe-query` | 20 req / min | per-user (`u:<name>`), fallback per-IP (`ip:<addr>`) |
| `scribe-ingest` | 5 req / min | per-user / per-IP (jw.) |

Przekroczenie zwraca `429 Too Many Requests`.

## Przykłady curl

### Status (wymaga sesji / JWT)

```bash
curl -H "Authorization: Bearer $JWT" http://localhost:5000/api/scribe/status
```

### Vector search (szybkie, bez LLM)

```bash
curl -X POST http://localhost:5000/api/scribe/search \
  -H "Authorization: Bearer $JWT" \
  -H "Content-Type: application/json" \
  -d '{"query": "warsztat barona", "campaignId": 1, "topK": 5}'
```

### Pełny RAG

```bash
curl -X POST http://localhost:5000/api/scribe/query \
  -H "Authorization: Bearer $JWT" \
  -H "Content-Type: application/json" \
  -d '{"query": "Co wydarzyło się w zamku?", "campaignId": 1}'
```

### Agent z tool-callingiem (zwraca też `citations`)

```bash
curl -X POST http://localhost:5000/api/scribe/agent/query \
  -H "Authorization: Bearer $JWT" \
  -H "Content-Type: application/json" \
  -d '{"query": "Co Tomin wie o Krwawym Bractwie?", "campaignId": 1}'
```

Odpowiedź (skrócona):

```jsonc
{
  "response": "...",
  "generationTimeMs": 4123,
  "modelUsed": "qwen2.5:14b",
  "toolCalls": ["search_memories", "get_character_by_name"],
  "citations": [
    {
      "memoryId": 42,
      "chunkId": 117,
      "title": "Akt II / Scena 3 – Spotkanie w karczmie",
      "memoryType": "Post",
      "similarity": 0.78,
      "snippet": "Tomin pochylił się nad stołem...",
      "sourcePostId": 5012,
      "sourceChapterId": 18,
      "sourceCampaignId": 1
    }
  ],
  "conversationId": 8
}
```

### Import danych

```bash
curl -X POST http://localhost:5000/api/scribe/import-json/1 \
  -H "Authorization: Bearer $JWT" \
  -H "Content-Type: application/json" \
  -d @Resources/processed/all_chunks.json
```

## Konfiguracja (`appsettings.json` / sekcja `Scribe`)

```json
{
  "Scribe": {
    "Ollama": {
      "BaseUrl": "http://GPU_SERVER_IP:11434",
      "EmbeddingModel": "nomic-embed-text",
      "ChatModel": "qwen2.5:14b",
      "Temperature": 0.7,
      "MaxTokens": 2048,
      "TimeoutSeconds": 120,
      "EmbeddingConcurrency": 3,
      "PersonaFilePath": "Resources/scribe-persona.md"
    },
    "Chunking": {
      "MaxTokensPerChunk": 500,
      "OverlapTokens": 50
    },
    "Search": {
      "TopK": 5,
      "SimilarityThreshold": 0.5
    },
    "Ingest": {
      "BatchSize": 25,
      "BatchDelayMs": 0,
      "InterChapterDelayMs": 0
    }
  }
}
```

Sekrety:

- `Authentication:Schemes:Bearer:JwtKey` **musi** być ustawione przez
  `dotnet user-secrets` (dev) lub zmienną środowiskową
  `Authentication__Schemes__Bearer__JwtKey` (prod). Minimum 32 znaki.
  Aplikacja nie wystartuje z pustym/placeholderowym kluczem.

Telemetria (opcjonalna):

- `OTEL_EXPORTER_OTLP_ENDPOINT` – ustawione = włącza OTLP export
  (`AspNetCore` + `HttpClient` + ActivitySource `DagoniteEmpire.Scribe`).
- Aktywności Scribe: `scribe.agent.invoke`, `scribe.search`,
  `scribe.embedding`.

## Modele Ollama

| Model | Rozmiar | GPU VRAM | Użycie |
|-------|---------|----------|--------|
| `nomic-embed-text` | 274 MB | ~1 GB | Embeddings (768 dim) |
| `qwen2.5:14b` | ~9 GB | ~12 GB | Chat + tool calling (zalecane dla agenta) |
| `qwen2.5:7b` | ~4.7 GB | ~8 GB | Tańszy fallback (też tool calling) |

> **Uwaga:** modele bez wsparcia tool-callingu (`gemma2`, `llama3.2`) **nie**
> nadają się dla `/agent/query`.

## Czasy odpowiedzi (przykładowo, GPU)

| Operacja | Czas |
|----------|------|
| Embedding | ~100 ms |
| Vector search | ~2 ms |
| LLM response (single turn) | ~5–15 s |
| Agent z 1–2 tool calls | ~8–20 s |

## Docker commands

```bash
# PostgreSQL + pgvector
docker run -d --name postgres-pgvector \
  -e POSTGRES_USER=dagonite \
  -e POSTGRES_PASSWORD=secret \
  -e POSTGRES_DB=dagonite \
  -p 5432:5432 \
  pgvector/pgvector:pg16

# Ollama z GPU
docker run -d --gpus all \
  -p 11434:11434 \
  -e OLLAMA_HOST=0.0.0.0 \
  --name ollama \
  ollama/ollama

# Pobranie modeli
docker exec ollama ollama pull nomic-embed-text
docker exec ollama ollama pull qwen2.5:14b
```
