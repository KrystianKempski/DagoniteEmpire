# SCRIBE - Konfiguracja serwera GPU (Ollama)

Ten dokument opisuje jak skonfigurować serwer z GPU do obsługi modeli AI dla systemu SCRIBE.

## Docelowa konfiguracja

| Komponent | Specyfikacja |
|-----------|-------------|
| GPU | **AMD Radeon RX 9070 XT** |
| VRAM | **16 GB** |
| RAM | 16+ GB |
| Dysk | SSD 50GB+ |
| System | Ubuntu 22.04 LTS / Ubuntu 24.04 |
| Sterowniki | ROCm 6.x |

> **Uwaga**: Model gemma2:9b wymaga ~6GB VRAM. RX 9070 XT z 16GB VRAM obsłuży nawet większe modele.

## Krok 1: Instalacja sterowników AMD ROCm

### 1.1 Aktualizacja systemu
```bash
sudo apt update && sudo apt upgrade -y
```

### 1.2 Instalacja ROCm
```bash
# Dodaj repozytorium AMD ROCm
wget https://repo.radeon.com/amdgpu-install/latest/ubuntu/jammy/amdgpu-install_6.0.60002-1_all.deb
sudo apt install ./amdgpu-install_6.0.60002-1_all.deb

# Zainstaluj ROCm i sterowniki
sudo amdgpu-install --usecase=rocm,graphics --accept-eula

# Dodaj użytkownika do grup
sudo usermod -aG video,render $USER

# Restart
sudo reboot
```

### 1.3 Weryfikacja
```bash
rocm-smi
```

Powinieneś zobaczyć informacje o GPU:
```
======================= ROCm System Management Interface =======================
================================= Concise Info =================================
GPU  Temp   AvgPwr  SCLK    MCLK     Fan    Perf  PwrCap  VRAM%  GPU%  
0    35c    25W     500Mhz  1200Mhz  0%     auto  263W    0%     0%    
================================================================================
========================= End of ROCm SMI Log =================================
```

### 1.4 Sprawdź wykrywanie GPU
```bash
# Lista GPU
rocminfo | grep -E "(Name:|Marketing)"

# Powinno pokazać:
# Marketing Name: AMD Radeon RX 9070 XT
```

## Krok 2: Instalacja Ollama

```bash
# Instalacja Ollama
curl -fsSL https://ollama.com/install.sh | sh

# Weryfikacja instalacji
ollama --version
```

## Krok 3: Pobranie modeli

```bash
# Model embeddingów (wymagany, ~274MB)
ollama pull nomic-embed-text

# Model LLM (wymagany, ~5.4GB)
ollama pull gemma2:9b

# Weryfikacja
ollama list
```

Oczekiwany output:
```
NAME                 ID              SIZE      MODIFIED
gemma2:9b            a0f32e3f1c30    5.4 GB    Just now
nomic-embed-text     9c1ffec98e7b    274 MB    Just now
```

## Krok 4: Konfiguracja serwisu systemd

Utwórz plik serwisu:

```bash
sudo nano /etc/systemd/system/ollama.service
```

Zawartość:
```ini
[Unit]
Description=Ollama AI Server
After=network-online.target

[Service]
Type=simple
User=ollama
Group=ollama
Environment="OLLAMA_HOST=0.0.0.0"
Environment="OLLAMA_KEEP_ALIVE=24h"
Environment="OLLAMA_NUM_PARALLEL=4"
ExecStart=/usr/local/bin/ollama serve
Restart=always
RestartSec=3

[Install]
WantedBy=multi-user.target
```

Aktywacja:
```bash
# Utwórz użytkownika ollama (jeśli nie istnieje)
sudo useradd -r -s /bin/false -m -d /usr/share/ollama ollama

# Reload i start
sudo systemctl daemon-reload
sudo systemctl enable ollama
sudo systemctl start ollama

# Sprawdź status
sudo systemctl status ollama
```

## Krok 5: Konfiguracja firewalla

```bash
# Pozwól na dostęp tylko z serwera aplikacji
# Zamień APP_SERVER_IP na rzeczywisty adres IP serwera aplikacji
sudo ufw allow from APP_SERVER_IP to any port 11434

# Lub dla całej podsieci (mniej bezpieczne)
sudo ufw allow from 192.168.1.0/24 to any port 11434

# Włącz firewall
sudo ufw enable
sudo ufw status
```

## Krok 6: Test połączenia

Z serwera GPU:
```bash
# Test lokalny
curl http://localhost:11434/api/tags
```

Z serwera aplikacji:
```bash
# Test zdalny (zamień GPU_SERVER_IP)
curl http://GPU_SERVER_IP:11434/api/tags

# Test embeddingów
curl http://GPU_SERVER_IP:11434/api/embeddings -d '{
  "model": "nomic-embed-text",
  "prompt": "Test embedding"
}'

# Test generacji (powinno odpowiedzieć w <5s z GPU)
time curl http://GPU_SERVER_IP:11434/api/generate -d '{
  "model": "gemma2:9b",
  "prompt": "Witaj, jestem SCRIBE. Kim jesteś?",
  "stream": false
}'
```

## Krok 7: Konfiguracja aplikacji .NET

Na serwerze aplikacji ustaw zmienną środowiskową lub zaktualizuj `appsettings.json`:

### Opcja A: Zmienna środowiskowa (zalecane dla produkcji)
```bash
export Scribe__Ollama__BaseUrl="http://GPU_SERVER_IP:11434"
```

### Opcja B: appsettings.json
```json
{
  "Scribe": {
    "Ollama": {
      "BaseUrl": "http://GPU_SERVER_IP:11434",
      "EmbeddingModel": "nomic-embed-text",
      "ChatModel": "gemma2:9b",
      "Temperature": 0.7,
      "MaxTokens": 2048
    }
  }
}
```

## Monitoring i diagnostyka

### Sprawdź użycie GPU
```bash
# Ciągłe monitorowanie
watch -n 1 rocm-smi

# Lub szczegółowe
rocm-smi --showmeminfo vram
rocm-smi --showuse
```

### Logi Ollama
```bash
# Bieżące logi
sudo journalctl -u ollama -f

# Ostatnie 100 linii
sudo journalctl -u ollama -n 100
```

### Sprawdź wydajność
```bash
# Benchmark embeddingów (powinno być <100ms)
time curl -s http://localhost:11434/api/embeddings -d '{
  "model": "nomic-embed-text",
  "prompt": "Test wydajności embeddingów dla systemu SCRIBE"
}' | jq '.embedding | length'

# Benchmark LLM (powinno być <5s dla krótkiej odpowiedzi)
time curl -s http://localhost:11434/api/generate -d '{
  "model": "gemma2:9b",
  "prompt": "Odpowiedz jednym zdaniem: Kim jest Garrick?",
  "stream": false
}' | jq -r '.response'
```

## Oczekiwane czasy odpowiedzi

| Operacja | CPU (bez GPU) | GPU (RX 9070 XT) |
|----------|---------------|------------------|
| Embedding (1 chunk) | ~2s | ~80ms |
| LLM (krótka odp.) | timeout | ~2-4s |
| LLM (długa odp.) | timeout | ~8-25s |
| Import 50 chunków | ~100s | ~8s |

## Rozwiązywanie problemów

### Ollama nie startuje
```bash
# Sprawdź logi
sudo journalctl -u ollama -n 50

# Sprawdź czy port jest zajęty
sudo lsof -i :11434

# Restart
sudo systemctl restart ollama
```

### GPU nie wykrywane
```bash
# Sprawdź sterowniki ROCm
rocm-smi

# Sprawdź wykrywanie GPU
rocminfo

# Reinstaluj ROCm
sudo amdgpu-install --uninstall
sudo amdgpu-install --usecase=rocm,graphics --accept-eula

# Sprawdź czy Ollama widzi GPU
ollama run gemma2:9b "test"
# W logach powinno być: "using ROCm"
```

### Timeout na długich zapytaniach
```bash
# Zwiększ timeout w konfiguracji Ollama
Environment="OLLAMA_KEEP_ALIVE=24h"
Environment="OLLAMA_REQUEST_TIMEOUT=600s"
```

### Brak pamięci GPU (OOM)
```bash
# Użyj mniejszego modelu
ollama pull gemma2:2b

# Lub ogranicz kontekst w appsettings.json
"MaxTokens": 1024
```

## Bezpieczeństwo (opcjonalnie)

### SSH Tunnel (najprostsze)
Na serwerze aplikacji:
```bash
# Utwórz tunel
ssh -N -L 11434:localhost:11434 user@GPU_SERVER_IP &

# W appsettings.json użyj localhost
"BaseUrl": "http://localhost:11434"
```

### Nginx Reverse Proxy z Basic Auth
Na serwerze GPU:
```bash
sudo apt install nginx apache2-utils

# Utwórz hasło
sudo htpasswd -c /etc/nginx/.htpasswd scribe

# Konfiguracja nginx
sudo nano /etc/nginx/sites-available/ollama
```

```nginx
server {
    listen 11435 ssl;
    server_name _;
    
    ssl_certificate /etc/ssl/certs/ollama.crt;
    ssl_certificate_key /etc/ssl/private/ollama.key;
    
    location / {
        auth_basic "SCRIBE AI";
        auth_basic_user_file /etc/nginx/.htpasswd;
        
        proxy_pass http://localhost:11434;
        proxy_http_version 1.1;
        proxy_set_header Connection "";
        proxy_read_timeout 600s;
    }
}
```

## Checklist wdrożenia

- [ ] Zainstalowane sterowniki NVIDIA (`nvidia-smi` działa)
- [ ] Zainstalowane Ollama (`ollama --version`)
- [ ] Pobrane modele (`ollama list` pokazuje oba modele)
- [ ] Serwis systemd skonfigurowany i uruchomiony
- [ ] Firewall skonfigurowany
- [ ] Test połączenia z serwera aplikacji działa
- [ ] appsettings.json zaktualizowany z adresem GPU server
- [ ] Aplikacja .NET restartowana
- [ ] Test SCRIBE w przeglądarce - odpowiedzi generowane szybko

## Kontakt

W razie problemów sprawdź:
- Logi Ollama: `sudo journalctl -u ollama -f`
- Status GPU: `nvidia-smi`
- Dokumentacja Ollama: https://ollama.com/docs
