# SCRIBE - Quick Reference

## Endpoints

| Endpoint | Method | Opis |
|----------|--------|------|
| `/api/scribe/status` | GET | Sprawdź status Ollama |
| `/api/scribe/search` | POST | Vector search (bez LLM) |
| `/api/scribe/query` | POST | Pełny RAG (z LLM) |
| `/api/scribe/import/{campaignId}` | POST | Import z pliku JSON |
| `/api/scribe/import-json/{campaignId}` | POST | Import z body JSON |

## Przykłady curl

### Status
```bash
curl http://localhost:5000/api/scribe/status
```

### Wyszukiwanie (szybkie, bez LLM)
```bash
curl -X POST http://localhost:5000/api/scribe/search \
  -H "Content-Type: application/json" \
  -d '{"query": "warsztat barona", "campaignId": 1, "topK": 5}'
```

### Pytanie z odpowiedzią AI (wymaga GPU)
```bash
curl -X POST http://localhost:5000/api/scribe/query \
  -H "Content-Type: application/json" \
  -d '{"query": "Co wydarzyło się w zamku?", "campaignId": 1}'
```

### Import danych
```bash
curl -X POST http://localhost:5000/api/scribe/import-json/1 \
  -H "Content-Type: application/json" \
  -d @tools/Resources/processed/all_chunks.json
```

## Request body

```json
{
  "query": "Twoje pytanie",
  "campaignId": 1,
  "characterId": null,  // Opcjonalne - filtruje wyniki do wiedzy postaci
  "topK": 5,            // Opcjonalne - ile chunków pobrać
  "userId": "anonymous" // Opcjonalne - do logowania
}
```

## Konfiguracja

### appsettings.json
```json
{
  "Scribe": {
    "Ollama": {
      "BaseUrl": "http://GPU_SERVER_IP:11434",
      "EmbeddingModel": "nomic-embed-text",
      "ChatModel": "gemma2:9b",
      "TimeoutSeconds": 300
    },
    "Embedding": {
      "Dimensions": 768
    },
    "Search": {
      "TopK": 5,
      "MinSimilarity": 0.5
    }
  }
}
```

## Modele Ollama

| Model | Rozmiar | GPU VRAM | Użycie |
|-------|---------|----------|--------|
| nomic-embed-text | 274MB | ~1GB | Embeddings (768 dim) |
| gemma2:9b | 5.4GB | ~6GB | Generowanie odpowiedzi |

## Czasy odpowiedzi (GPU)

| Operacja | Czas |
|----------|------|
| Embedding | ~100ms |
| Vector search | ~2ms |
| LLM response | ~5-15s |
| Full RAG query | ~6-16s |

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
docker exec ollama ollama pull gemma2:9b
```
