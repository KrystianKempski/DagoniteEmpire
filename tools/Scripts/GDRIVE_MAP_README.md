# Adventure Map Builder — Instrukcja

Skrypt `gdrive_map_links.py` przechodzi przez folder Google Drive, wyciąga linki między Google Docs i buduje graf powiązań.

---

## Instalacja (jednorazowo)

```bash
pip install -r requirements_gdrive.txt
```

---

## Konfiguracja Google Cloud (jednorazowo)

1. Wejdź na [Google Cloud Console](https://console.cloud.google.com/)
2. Stwórz nowy projekt (lub użyj istniejącego)
3. Włącz dwa API:
   - **Google Drive API**
   - **Google Docs API**
4. Przejdź do **APIs & Services → Credentials**
5. Kliknij **Create Credentials → OAuth 2.0 Client ID**
   - Application type: **Desktop app**
6. Pobierz plik JSON i zapisz jako `credentials.json` obok skryptu
7. W **OAuth consent screen** dodaj swój email jako Test User

---

## Użycie

```bash
# Podstawowe — podaj ID folderu lub pełny URL z Drive
python gdrive_map_links.py --folder "https://drive.google.com/drive/folders/1ABC..."

# Lub sam ID
python gdrive_map_links.py --folder 1ABCdef123...

# Pełne opcje
python gdrive_map_links.py \
    --folder 1ABCdef123... \
    --output ./output \
    --credentials path/to/credentials.json \
    --depth 5 \
    --prefix moja_przygoda
```

### Parametry

| Parametr | Opis | Domyślnie |
|----------|------|-----------|
| `--folder` | ID lub URL folderu root na Drive | *wymagany* |
| `--output` | Katalog wyjściowy | `.` (bieżący) |
| `--credentials` | Ścieżka do credentials.json | `credentials.json` |
| `--depth` | Głębokość rekurencji folderów | `10` |
| `--prefix` | Prefiks nazw plików wyjściowych | `adventure_map` |

---

## Pliki wyjściowe

### `adventure_map.json`
Graf w formacie JSON gotowy do użycia przez agenta AI:
```json
{
  "nodeCount": 42,
  "edgeCount": 87,
  "nodes": [
    { "id": "...", "name": "Rozdział 1", "mimeType": "...", "url": "...", "folder": "Akt I" }
  ],
  "edges": [
    { "from": "...", "to": "...", "fromName": "Rozdział 1", "toName": "Rozdział 2" }
  ]
}
```

### `adventure_map.md`
Diagram Mermaid + tabela węzłów. Można go otworzyć w VS Code (rozszerzenie Markdown Preview) lub wkleić na GitHub.

---

## Jak działa

```
1. Faza 1 — skanowanie folderów
   Root folder → BFS przez wszystkie podfoldery → lista wszystkich plików

2. Faza 2 — ekstrakcja linków
   Dla każdego Google Doca:
     - Pobiera treść przez Docs API
     - Szuka wszystkich hiperlinków w akapitach i tabelach
     - Filtruje tylko linki do Google Drive
     - Jeśli znaleziony plik nie był jeszcze w grafie → pobiera jego metadane
       i też skanuje (odkrywa pliki poza folderem root)

3. Wynik → graf skierowany (kto linkuje do kogo)
```

---

## Pierwsze uruchomienie — autoryzacja

Przy pierwszym uruchomieniu skrypt otworzy przeglądarkę z ekranem zgody Google.
Po zatwierdzeniu token zostanie zapisany jako `token.json` obok `credentials.json`
i przy kolejnych uruchomieniach nie będzie już potrzebna interakcja.

---

## Wskazówki

- **ID folderu** znajdziesz w URL: `drive.google.com/drive/folders/`**`TEN_CIAG_ZNAKOW`**
- Skrypt podąża za linkami **poza** folder root (jeśli dokument linkuje do pliku w innym folderze, ten plik też zostanie dodany do grafu z adnotacją `<external>`)
- Linki do zewnętrznych stron (spoza Google Drive) są ignorowane
- Pętle w grafie są obsługiwane — każdy plik jest skanowany tylko raz
