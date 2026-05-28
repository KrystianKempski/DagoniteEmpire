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
│  └─────────────────────────────────┘    │     │  - NVIDIA GPU 8GB+ VRAM     │
│                                         │     │  - CUDA drivers             │
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
      "BaseUrl": "http://192.168.1.20:11434"
    }
  }
}
```

### 2.6 Migracja bazy danych

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

---

## 4. Testowanie

### 4.1 Health check

```bash
# Aplikacja
curl http://localhost:5000/healthz

# SCRIBE status (sprawdza połączenie z Ollama)
curl http://localhost:5000/api/scribe/status
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
- Ollama używa GPU (`nvidia-smi` pokazuje proces ollama)
- Model jest załadowany do VRAM

```bash
# Sprawdź GPU
nvidia-smi

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
- [ ] Ollama działa i ma modele: nomic-embed-text, gemma2:9b
- [ ] Ollama słucha na 0.0.0.0:11434 (nie tylko localhost)
- [ ] appsettings.Production.json ma poprawny IP maszyny GPU
- [ ] Migracja bazy danych wykonana
- [ ] Import danych wykonany
- [ ] Test wyszukiwania działa
- [ ] Test RAG query działa
