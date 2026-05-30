# SCRIBE - Deployment Guide

System SCRIBE (AI Memory for RPG) wymaga dwóch maszyn w sieci lokalnej:
- **Serwer aplikacji** - .NET + PostgreSQL
- **Maszyna GPU** - Ollama (embeddings + LLM)

## Architektura

```
┌─────────────────────────────────────────┐     ┌─────────────────────────────┐
│         SERWER APLIKACJI                │     │       MASZYNA GPU           │
│  ┌─────────────────────────────────┐    │     │  ┌─────────────────────┐    │
│  │  .NET App (Blazor Server)       │    │     │  │    Ollama Server    │    │
│  │  - Web UI                       │────┼─────┼─▶│  :11434             │    │
│  │  - SCRIBE API                   │    │ LAN │  │                     │    │
│  │  - Import service               │    │     │  │  Models:            │    │
│  └─────────────────────────────────┘    │     │  │  - nomic-embed-text │    │
│  ┌─────────────────────────────────┐    │     │  │  - gemma2:9b        │    │
│  │  PostgreSQL 16 + pgvector       │    │     │  └─────────────────────┘    │
│  │  - Vector storage (768 dim)     │    │     │                             │
│  │  - HNSW index                   │    │     │  Wymagania:                 │
│  └─────────────────────────────────┘    │     │  - AMD RX 9070 XT 16GB VRAM │
│                                         │     │  - ROCm drivers             │
│  Wymagania:                             │     │  - ~6GB disk (modele)       │
│  - .NET 9.0 Runtime                     │     │                             │
│  - PostgreSQL 16 + pgvector             │     │                             │
│  - ~1GB RAM dla aplikacji               │     │                             │
└─────────────────────────────────────────┘     └─────────────────────────────┘
        np. 192.168.1.10                              np. 192.168.1.20
```

---

## 1. Setup Maszyny GPU (Ollama)

### 1.1 Instalacja Ollama

```bash
# Linux
curl -fsSL https://ollama.com/install.sh | sh

# Lub Docker (alternatywnie)
docker run -d --gpus all -p 11434:11434 --name ollama ollama/ollama
```

### 1.2 Pobranie modeli

```bash
# Model do embeddingów (274MB)
ollama pull nomic-embed-text

# Model LLM do generowania odpowiedzi (5.4GB)
ollama pull gemma2:9b
```

### 1.3 Konfiguracja nasłuchu na sieci

Domyślnie Ollama słucha tylko na localhost. Aby umożliwić dostęp z sieci:

```bash
# Edytuj plik środowiskowy
sudo systemctl edit ollama

# Dodaj:
[Service]
Environment="OLLAMA_HOST=0.0.0.0"

# Restart
sudo systemctl restart ollama
```

Lub dla Docker:
```bash
docker run -d --gpus all -p 11434:11434 -e OLLAMA_HOST=0.0.0.0 --name ollama ollama/ollama
```

### 1.4 Weryfikacja

```bash
# Z maszyny GPU
curl http://localhost:11434/api/tags

# Z serwera aplikacji (zmień IP)
curl http://192.168.1.20:11434/api/tags
```

Oczekiwany wynik:
```json
{"models":[{"name":"gemma2:9b",...},{"name":"nomic-embed-text:latest",...}]}
```

---

## 2. Setup Serwera Aplikacji

### 2.1 PostgreSQL + pgvector

```bash
# Ubuntu/Debian
sudo apt install postgresql-16

# Instalacja pgvector
sudo apt install postgresql-16-pgvector

# Lub Docker (prostsze)
docker run -d \
  --name postgres-pgvector \
  -e POSTGRES_USER=dagonite \
  -e POSTGRES_PASSWORD=SECURE_PASSWORD \
  -e POSTGRES_DB=dagonite \
  -p 5432:5432 \
  pgvector/pgvector:pg16
```

### 2.2 Włączenie rozszerzenia pgvector

```bash
docker exec -it postgres-pgvector psql -U dagonite -d dagonite -c "CREATE EXTENSION IF NOT EXISTS vector;"
```

### 2.3 .NET 9.0 Runtime

```bash
# Ubuntu/Debian
wget https://dot.net/v1/dotnet-install.sh
chmod +x dotnet-install.sh
./dotnet-install.sh --channel 9.0 --runtime aspnetcore
```

### 2.4 Deploy aplikacji

```bash
# Na maszynie deweloperskiej - publikacja
dotnet publish DagoniteEmpire -c Release -o ./publish

# Kopiowanie na serwer
scp -r ./publish user@192.168.1.10:/opt/dagonite/

# Na serwerze - uruchomienie
cd /opt/dagonite
./DagoniteEmpire
```

### 2.5 Konfiguracja - appsettings.Production.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=dagonite;Username=dagonite;Password=SECURE_PASSWORD"
  },
  "Scribe": {
    "Ollama": {
      "BaseUrl": "http://192.168.1.20:11434",
      "ChatModel": "qwen2.5:14b",
      "PersonaFilePath": "Resources/scribe-persona.md"
    },
    "Ingest": {
      "BatchSize": 25,
      "BatchDelayMs": 0,
      "InterChapterDelayMs": 0
    }
  }
}
```

> **Model agenta:** dla `/api/scribe/agent/query` wymagany jest model z
> obsługą tool-callingu (`qwen2.5:14b` / `qwen2.5:7b` / `mistral-nemo` /
> `llama3.1`). Modele bez wsparcia (`gemma2`, `llama3.2`) **nie zadziałają**.

#### Sekret: JwtKey (wymagany, fail-fast)

Klucz JWT **nie jest** w `appsettings.json`. Aplikacja przerywa start jeśli
klucz jest pusty, równy placeholderowi lub krótszy niż 32 znaki.

```bash
# Dev: user-secrets (UserSecretsId jest już w csproj)
cd DagoniteEmpire
dotnet user-secrets set "Authentication:Schemes:Bearer:JwtKey" \
  "$(openssl rand -base64 48 | tr -d '\n')"

# Prod: zmienna środowiskowa (systemd / docker / kubernetes)
export Authentication__Schemes__Bearer__JwtKey="<>=32 znaki>"
```

#### Throttling indeksowania (opcjonalne)

Domyślnie indeks postów chodzi na pełnej prędkości. Dla wielkich kampanii
lub współdzielonego GPU można zwolnić:

```json
"Ingest": {
  "BatchSize": 25,           // co ile postów log + ewentualna pauza
  "BatchDelayMs": 500,       // pauza między batchami w obrębie rozdziału
  "InterChapterDelayMs": 1000 // pauza między rozdziałami w IngestCampaignPostsAsync
}
```

#### Telemetria (opcjonalna)

OpenTelemetry uruchamia się tylko gdy ustawione `OTEL_EXPORTER_OTLP_ENDPOINT`:

```bash
export OTEL_EXPORTER_OTLP_ENDPOINT="http://otel-collector:4317"
# (opcjonalnie) export OTEL_EXPORTER_OTLP_HEADERS="x-api-key=..."
```

Zbieramy: aktywności AspNetCore, HttpClient oraz Scribe
(`scribe.agent.invoke`, `scribe.search`, `scribe.embedding`).

### 2.6 Konfiguracja persony SCRIBE (opcjonalne)

SCRIBE może mieć własną "osobowość" zdefiniowaną w pliku tekstowym.
Plik ten jest **sekretem** i nie powinien być w repozytorium.

1. **Skopiuj template:**
   ```bash
   cp Resources/scribe-persona.template.md Resources/scribe-persona.md
   ```

2. **Edytuj plik `Resources/scribe-persona.md`:**
   - Zdefiniuj charakter archiwisty (ton, styl odpowiedzi)
   - Określ co może/nie może mówić
   - Dodaj ograniczenia (np. ochrona sekretów graczy)

3. **Ścieżka w konfiguracji:**
   ```json
   "PersonaFilePath": "Resources/scribe-persona.md"
   ```

Jeśli plik nie istnieje, SCRIBE używa domyślnego, neutralnego promptu.

### 2.7 Migracja bazy danych

```bash
# Przy pierwszym uruchomieniu lub po aktualizacji
dotnet ef database update --project DA_DataAccess --startup-project DagoniteEmpire
```

---

## 3. Import danych SCRIBE

### 3.1 Przygotowanie danych

Dane do importu znajdują się w `Resources/processed/all_chunks.json`.
Plik ten zawiera 2406 chunków wyekstrahowanych z kampanii.

### 3.2 Wykonanie importu

```bash
# Z serwera aplikacji (lub dowolnego miejsca z dostępem do API)
curl -X POST "http://localhost:5000/api/scribe/import/1" \
  -H "Content-Type: multipart/form-data" \
  -F "file=@all_chunks.json"

# Lub przez JSON body (dla mniejszych plików)
curl -X POST "http://localhost:5000/api/scribe/import-json/1" \
  -H "Content-Type: application/json" \
  -d @all_chunks.json
```

### 3.3 Czas importu

| Infrastruktura | Czas dla 2406 chunks |
|----------------|----------------------|
| GPU (RTX 3060+) | ~4 minuty |
| CPU only | ~80 minut |

### 3.4 Weryfikacja importu

```bash
# Sprawdź liczbę zaimportowanych chunków
curl "http://localhost:5000/api/scribe/status"

# Test wyszukiwania
curl -X POST "http://localhost:5000/api/scribe/search" \
  -H "Content-Type: application/json" \
  -d '{"query": "warsztat barona", "campaignId": 1, "topK": 3}'
```

### 3.5 Indeksowanie postów z wątków kampanii

Po zaimportowaniu archiwum Word, posty z bieżących wątków (rozdziałów) są indeksowane **automatycznie** przy tworzeniu.

#### Indeksowanie istniejących postów (jednorazowo)

```bash
# Wszystkie posty z całej kampanii
curl -X POST "http://localhost:5000/api/scribe/ingest/campaign/1"

# Tylko konkretny rozdział
curl -X POST "http://localhost:5000/api/scribe/ingest/chapter/5"

# Re-indeksowanie (usunięcie i ponowne przetworzenie)
curl -X POST "http://localhost:5000/api/scribe/ingest/campaign/1?reindex=true"
```

#### Jak działa auto-sync

- Każdy nowy post w wątku rozdziału jest automatycznie indeksowany w tle
- Posty krótsze niż 50 znaków są pomijane
- Dostęp do posta mają postacie obecne w rozdziale w momencie publikacji
- GM widzi wszystkie posty

---

## 4. Testowanie

### 4.1 Health check

```bash
# Liveness aplikacji (DB)
curl http://localhost:5000/healthz

# Readiness Scribe (Ollama + obecność ChatModel)
# Healthy   = Ollama OK i model zainstalowany
# Degraded  = Ollama OK ale brak modelu (200 z payloadem 'Degraded')
# Unhealthy = Ollama nieosiągalne
curl http://localhost:5000/health/scribe
```

### 4.2 Test wyszukiwania (vector search)

```bash
curl -X POST "http://localhost:5000/api/scribe/search" \
  -H "Content-Type: application/json" \
  -d '{"query": "co wie Granit o zamku", "campaignId": 1}'
```

### 4.3 Test pełnego RAG (wymaga GPU)

```bash
curl -X POST "http://localhost:5000/api/scribe/query" \
  -H "Content-Type: application/json" \
  -d '{"query": "Co wydarzyło się w zamku?", "campaignId": 1}'
```

---

## 5. Troubleshooting

### Ollama nie odpowiada z sieci

```bash
# Sprawdź czy Ollama słucha na wszystkich interfejsach
ss -tlnp | grep 11434

# Powinno pokazać: 0.0.0.0:11434, nie 127.0.0.1:11434
```

### Timeout przy zapytaniach LLM

LLM na CPU jest bardzo wolne. Upewnij się, że:
- Ollama używa GPU (`rocm-smi` pokazuje proces ollama)
- Model jest załadowany do VRAM

```bash
# Sprawdź GPU
rocm-smi

# Sprawdź załadowane modele
curl http://localhost:11434/api/ps
```

### Błąd połączenia z PostgreSQL

```bash
# Sprawdź czy PostgreSQL działa
docker ps | grep postgres

# Sprawdź logi
docker logs postgres-pgvector
```

### pgvector extension nie istnieje

```bash
docker exec -it postgres-pgvector psql -U dagonite -d dagonite -c "CREATE EXTENSION IF NOT EXISTS vector;"
```

---

## 6. Backup i restore

### Backup bazy danych

```bash
docker exec postgres-pgvector pg_dump -U dagonite dagonite > backup_$(date +%Y%m%d).sql
```

### Restore

```bash
cat backup_20260528.sql | docker exec -i postgres-pgvector psql -U dagonite dagonite
```

---

## 7. Porty i firewall

| Port | Usługa | Dostęp |
|------|--------|--------|
| 5000 | .NET App | Zewnętrzny (lub za reverse proxy) |
| 5432 | PostgreSQL | Tylko localhost |
| 11434 | Ollama | Tylko sieć lokalna |

```bash
# Przykład UFW (serwer aplikacji)
ufw allow 5000/tcp

# Przykład UFW (maszyna GPU) - tylko z sieci lokalnej
ufw allow from 192.168.1.0/24 to any port 11434
```

---

## Checklist przed uruchomieniem

- [ ] PostgreSQL działa i ma rozszerzenie pgvector
- [ ] Ollama działa i ma modele: `nomic-embed-text`, `qwen2.5:14b` (lub inny z tool-callingiem)
- [ ] Ollama słucha na 0.0.0.0:11434 (nie tylko localhost)
- [ ] `appsettings.Production.json` ma poprawny IP maszyny GPU
- [ ] `Authentication:Schemes:Bearer:JwtKey` ustawione (user-secrets lub env, min. 32 znaki)
- [ ] Migracja bazy danych wykonana (`dotnet ef database update`)
- [ ] Import danych wykonany
- [ ] `/healthz` zwraca 200
- [ ] `/health/scribe` zwraca 200 Healthy
- [ ] Test `/api/scribe/search` działa
- [ ] Test `/api/scribe/agent/query` działa
- [ ] (opcjonalnie) `OTEL_EXPORTER_OTLP_ENDPOINT` ustawione i traces docierają do backendu
