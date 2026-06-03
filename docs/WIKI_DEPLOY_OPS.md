# Wiki DagoniteEmpire — instrukcja dla administratora serwera

Dokument dla osoby wdrażającej aplikację na produkcję (Docker / Kubernetes).  
Szczegóły reguł dostępu i developera: [WIKI_DEPLOY.md](./WIKI_DEPLOY.md).

---

## Co musisz wiedzieć na start

| Element | Gdzie jest | Uwagi |
|---------|------------|--------|
| Kod aplikacji | Repo **DagoniteEmpire** (`master`) | Sam `git pull` **nie** wgrywa treści wiki |
| Generator strony wiki | Repo **dagonite-wiki** (obok Empire) | Quartz, Node.js — potrzebny **przy każdym buildzie** obrazu z wiki |
| Pliki wiki w kontenerze | `DagoniteEmpire/wwwroot/wiki/` | Tworzone skryptem **przed** `docker build` |
| Publiczne GitHub Pages | Wyłączone | Wiki tylko pod `https://dagonite-empire.drik.it/wiki` |

**Bez katalogu `wwwroot/wiki/` w obrazie Docker** zakładka Wiki w aplikacji pokaże ostrzeżenie „Wiki nie jest wdrożona”.

---

## Wymagania na maszynie buildowej

- Git (oba repozytoria w jednym katalogu nadrzędnym, np. `Dag1/`)
- **.NET 9 SDK** (build aplikacji)
- **Node.js 22** + npm (build Quartza)
- **Python 3** (skrypty manifestu ACL)
- Dostęp do Docker registry (push obrazu)

Struktura katalogów:

```
Dag1/
├── DagoniteEmpire/          ← docker build stąd (root solution)
└── dagonite-wiki/           ← npm / quartz build
```

---

## Procedura: aktualizacja aplikacji + wiki (standard)

Wykonaj na hoście CI lub lokalnie przed push obrazu.

### 1. Pobierz kod

```bash
cd /ścieżka/Dag1/DagoniteEmpire
git pull origin master

cd /ścieżka/Dag1/dagonite-wiki
git pull origin main
```

### 2. Zbuduj wiki i skopiuj do wwwroot

```bash
cd /ścieżka/Dag1/DagoniteEmpire/tools/Scripts
chmod +x build_wiki_for_empire.sh sync_wiki_pipeline.sh deploy_wiki_to_wwwroot.sh

./build_wiki_for_empire.sh
```

Skrypt:

1. Uruchamia `npx quartz build` w `dagonite-wiki`
2. Kopiuje `dagonite-wiki/public/` → `DagoniteEmpire/DagoniteEmpire/wwwroot/wiki/`
3. Przepisuje ścieżki `/dagonite-wiki` → `/wiki`
4. Generuje `wwwroot/wiki/static/wiki-access.json`, `wiki-parties.json`, `wiki-links.json`

**Sprawdzenie:**

```bash
test -f /ścieżka/Dag1/DagoniteEmpire/DagoniteEmpire/wwwroot/wiki/index.html && echo OK
ls /ścieżka/Dag1/DagoniteEmpire/DagoniteEmpire/wwwroot/wiki/static/wiki-access.json
```

### 3. Zbuduj obraz Docker

Z katalogu, w którym jest Dockerfile (zazwyczaj `DagoniteEmpire/` — zgodnie z Waszym pipeline):

```bash
cd /ścieżka/Dag1/DagoniteEmpire
docker build -f DagoniteEmpire/Dockerfile -t <registry>/dagonite-empire:<tag> .
docker push <registry>/dagonite-empire:<tag>
```

Upewnij się, że kontekst buildu **zawiera** wypełniony `DagoniteEmpire/wwwroot/wiki/` (nie jest w git — musi powstać w kroku 2).

### 4. Wdróż nowy obraz

Według Waszego procesu (FluxCD / Helm / ręczny restart):

- Zaktualizuj tag obrazu w manifeście
- Poczekaj na rollout poda
- Sprawdź logi startu aplikacji (migracje DB, brak błędów ładowania wiki)

### 5. Testy po wdrożeniu

| Test | Oczekiwany wynik |
|------|------------------|
| `https://dagonite-empire.drik.it/wiki` | Strona wiki (logowanie jeśli wymagane) |
| Gość → `…/wiki/świat-i-zasady/` | Lore bez logowania |
| Gracz bez drużyny → link do sekretu kampanii | Komunikat **„Brak dostępu do tej strony”** (nie czarny ekran) |
| MG / Admin | Pełna wiki |

---

## Procedura: tylko nowa treść wiki (bez zmian w C#)

Gdy zmieniła się tylko treść w `dagonite-wiki` / docx, a kod aplikacji ten sam:

1. Kroki 1–2 jak wyżej (`build_wiki_for_empire.sh`)
2. Nowy `docker build` + deploy (wiki jest **wbudowane** w obraz)

Nie ma osobnego wolumenu wiki na serwerze — treść musi być w obrazie przy buildzie.

---

## Procedura: pełna regeneracja z docx

Gdy autor kampanii zaktualizował pliki źródłowe w `DagoniteEmpire/tools/Resources/`:

```bash
cd DagoniteEmpire/tools/Scripts
./sync_wiki_pipeline.sh
```

Następnie `docker build` + deploy jak w sekcji standardowej.

---

## Procedura: tylko zmiana drużyn / uprawnień

Edycja pliku w repo wiki (autor/MG):

`dagonite-wiki/content/_meta/wiki-parties.json`

Potem na buildowej:

```bash
cd DagoniteEmpire/tools/Scripts
./deploy_wiki_to_wwwroot.sh
# opcjonalnie sam manifest bez pełnego quartz build, jeśli HTML się nie zmienił:
# python3 build_wiki_access_manifest.py
```

I ponownie **docker build** + deploy.

---

## Co NIE trzeba robić na serwerze produkcyjnym

- Instalacja Node.js na **podzie** aplikacji (wiki jest statyczne)
- Włączanie GitHub Pages w repo `dagonite-wiki`
- Ręczne kopiowanie plików do działającego poda (chyba że macie niestandardowy proces — domyślnie wszystko w obrazie)

---

## Rozwiązywanie problemów

| Objaw | Przyczyna | Działanie |
|-------|-----------|-----------|
| „Wiki nie jest wdrożona” | Brak `wwwroot/wiki/index.html` w obrazie | Powtórz `build_wiki_for_empire.sh` przed `docker build` |
| Czarny ekran w wiki | Stary obraz bez `wiki-access-denied.html` | Pull najnowszego `master`, przebuduj obraz |
| Gracz widzi cudze sekrety | Stary manifest lub publiczne Pages | Wyłącz Pages; przebuduj wiki + manifest |
| 502 / aplikacja nie startuje | Błąd DB, nie wiki | Logi poda, connection string Postgres |

---

## Repozytorium `dagonite-wiki` — czy można je „zamknąć”?

**Nie usuwać** — nadal jest potrzebne do:

- przechowywania treści Markdown (`content/`)
- konfiguracji Quartz (`quartz.config.yaml`)
- `wiki-parties.json`
- budowania HTML (`npm` / `quartz build`)

**Można wyłączyć** tylko publiczne **GitHub Pages** (już wyłączone auto-deploy; workflow tylko ręczny).

Hosting produkcyjny = wyłącznie aplikacja DagoniteEmpire pod `/wiki`.

---

## Kontakt / eskalacja

Problemy z regułami widoczności (kto co widzi): autor kampanii + plik `wiki-parties.json`.  
Problemy z buildem Docker / K8s: administrator infrastruktury + ten dokument.
