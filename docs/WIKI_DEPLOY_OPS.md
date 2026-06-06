# Wiki DagoniteEmpire — instrukcja dla administratora serwera

Dokument dla osoby wdrażającej aplikację na produkcję (Docker / Kubernetes).  
Reguły biznesowe i rozwój: [WIKI_DEPLOY.md](./WIKI_DEPLOY.md).

---

## Szybki start

1. Sklonuj **oba** repozytoria obok siebie (`Dag1/DagoniteEmpire` + `Dag1/dagonite-wiki`).
2. `git pull` w **obu** repozytoriach (`master` + `main`).
3. Na maszynie buildowej: `./DagoniteEmpire/tools/Scripts/build_wiki_for_empire.sh`
4. Zbuduj **nowy** obraz Docker (kontekst musi zawierać świeży `wwwroot/wiki/` z kroku 3).
5. Wdróż **konkretny nowy tag** obrazu i zrestartuj kontener/pody.

**Bez kroku 3** użytkownicy zobaczą: „Wiki nie jest jeszcze wdrożona na tym serwerze”.  
**Bez kroków 3–5** po `git pull` treść wiki **się nie zmieni** — HTML wiki nie jest w repozytorium Empire, tylko w obrazie Docker.

---

## Ważne: skąd bierze się treść na produkcji

| Co | Gdzie żyje | Jak trafia na serwer |
|----|------------|----------------------|
| Kod C# (Panel MG, ACL, …) | `DagoniteEmpire` git | `dotnet publish` w Dockerfile |
| Markdown wiki | `dagonite-wiki` git | `npx quartz build` → kopia do `wwwroot/wiki/` **przed** `docker build` |
| HTML/CSS/JS wiki | `DagoniteEmpire/wwwroot/wiki/` | **Tylko wewnątrz obrazu Docker** (nie commitowane do git) |

`git pull` samego Empire **nigdy** nie aktualizuje stron wiki. Restart kontenera **bez nowego obrazu** też nie.

### Kiedy CI buduje nowy obraz

| Workflow | Trigger | Tag obrazu (GHCR) |
|----------|---------|-------------------|
| `docker-image-dev.yml` | push do `DagoniteEmpire/master` **lub** ręcznie *Run workflow* | `dev.YYYY-MM-DDTHH:MM:SS-…` (brak stałego `latest`) |
| `docker-image.yml` | opublikowany **Release** na GitHub | tag wydania (np. `v1.2.3`) |

**Push tylko do `dagonite-wiki/main` nie uruchamia buildu Empire.** Po zmianie samej treści wiki:

- uruchom ręcznie workflow *Create and publish dev Docker image* w repo Empire, **albo**
- zrób dowolny push do `DagoniteEmpire/master`, **albo**
- na serwerze buildowym: `git pull` obu repo + `build_wiki_for_empire.sh` + `docker build`.

---

## Repozytoria

| Repo | Branch produkcyjny | Zawartość |
|------|-------------------|-----------|
| **DagoniteEmpire** | `master` | Aplikacja .NET, middleware ACL, skrypty w `tools/Scripts/` |
| **dagonite-wiki** | `main` | Markdown `content/`, Quartz, `content/_meta/wiki-parties.json` |

`git pull` samego Empire **nie** aktualizuje HTML wiki — trzeba przebudować z `dagonite-wiki`.

---

## Wymagania na maszynie buildowej

- Git (oba repozytoria, ta sama struktura `Dag1/`)
- **.NET 9 SDK** (opcjonalnie: testy; runtime w obrazie)
- **Node.js 22** + npm (`npx quartz build`)
- **Python 3** (`build_wiki_access_manifest.py`, `tag_wiki_content.py`)
- Docker (build + push obrazu)

```
Dag1/
├── DagoniteEmpire/          ← docker build (root według Waszego Dockerfile)
│   ├── DagoniteEmpire/
│   │   └── wwwroot/wiki/    ← powstaje przy buildzie, NIE w git
│   └── tools/Scripts/
└── dagonite-wiki/
    ├── content/
    ├── quartz.config.yaml
    └── public/              ← wynik `npx quartz build`
```

---

## Procedura standardowa (aplikacja + wiki)

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
chmod +x build_wiki_for_empire.sh deploy_wiki_to_wwwroot.sh

./build_wiki_for_empire.sh
```

Skrypt wykonuje:

1. `npx quartz build` w `../dagonite-wiki`
2. Kopię `public/` → `DagoniteEmpire/wwwroot/wiki/`
3. Przepisanie ścieżek `/dagonite-wiki` → `/wiki`
4. Generowanie `static/wiki-access.json`, `wiki-parties.json`, `wiki-links.json`

**Weryfikacja:**

```bash
WIKI=/ścieżka/Dag1/DagoniteEmpire/DagoniteEmpire/wwwroot/wiki
test -f "$WIKI/index.html" && echo "OK: wiki hub"
test -f "$WIKI/static/wiki-access.json" && echo "OK: manifest"
wc -c "$WIKI/static/wiki-access.json"
```

### 3. Zbuduj i wdróż obraz Docker

```bash
cd /ścieżka/Dag1/DagoniteEmpire
docker build -f DagoniteEmpire/Dockerfile -t <registry>/dagonite-empire:<tag> .
docker push <registry>/dagonite-empire:<tag>
```

Kontekst buildu musi zawierać **świeży** katalog `wwwroot/wiki/` z kroku 2.

### 4. Wdróż nowy obraz (nie sam restart)

Dev CI publikuje tag z datą, np. `dev.2026-06-06T01:30:00-…`. Sprawdź w GitHub → Packages → `DagoniteEmpire`, skopiuj **najnowszy** tag.

```bash
# przykład — podstaw swój registry i tag z CI
docker pull ghcr.io/krystiankempski/dagoniteempire:dev.2026-06-06T01:30:00-
# zaktualizuj deployment (K8s / compose / skrypt) na TEN tag
# dopiero potem rollout / restart
```

Sam `docker compose restart` lub restart poda **bez zmiany tagu obrazu** = stara treść.

### 5. Weryfikacja wdrożonej wersji

Po buildzie wiki powstaje plik `wwwroot/wiki/.wiki-build-info.json` (commit Empire, commit wiki, data UTC).

**W działającym kontenerze:**

```bash
docker exec <kontener> cat /app/wwwroot/wiki/.wiki-build-info.json
```

**Z obrazu bez uruchamiania aplikacji:**

```bash
docker run --rm --entrypoint cat <obraz> /app/wwwroot/wiki/.wiki-build-info.json
```

Porównaj `wikiCommit` z `git rev-parse --short HEAD` w lokalnym `dagonite-wiki`. Jeśli się nie zgadza — wdrożono stary obraz lub pominięto `build_wiki_for_empire.sh`.

### 6. Rollout + testy funkcjonalne

| Test | URL / akcja | Oczekiwany wynik |
|------|-------------|------------------|
| Wersja buildu | `/wiki/.wiki-build-info.json` (MG) lub `docker exec … cat` | Aktualne SHA obu repo |
| Wiki działa | `/wiki` | Zakładka Blazor z iframe (hub kampanii lub lore wewnątrz) |
| Lore publiczne | `/wiki/świat-i-zasady/` | Bez logowania |
| Sekret | URL sesji jako inna postać | „Brak dostępu” (403), nie treść |
| Home w wiki | Breadcrumb „Home” | Hub `index.html`, nie czarny ekran |
| Manifest | `/wiki/static/wiki-access.json` jako gracz | 403 |
| MG | Zalogowany Admin/MG | Pełna wiki, explorer, wszystkie strony |
| Cache przeglądarki | Po deploy | Twarde odświeżenie (Ctrl+Shift+R) na `/wiki` |

---

## Inne procedury

### Tylko nowa treść Markdown (bez zmian C#)

1. `git pull` w `dagonite-wiki` (+ ewentualnie Empire jeśli skrypty się zmieniły)
2. `./build_wiki_for_empire.sh`
3. Nowy `docker build` + deploy

### Pełna regeneracja z docx

```bash
cd DagoniteEmpire/tools/Scripts
./sync_wiki_pipeline.sh
```

Potem docker build jak wyżej.

### Tylko uprawnienia / drużyny

Edycja w repo wiki:

- `content/_meta/wiki-parties.json`
- tagi w `content/**/*.md` (np. `lawenda`, `team-bonefyre`)

Na buildowej:

```bash
cd DagoniteEmpire/tools/Scripts
./build_wiki_for_empire.sh
# lub jeśli HTML się nie zmienił, a jest już public/:
python3 build_wiki_access_manifest.py
./deploy_wiki_to_wwwroot.sh
```

Ponownie **docker build** + deploy.

---

## Co NIE robić na produkcji

- Instalować Node na **podzie** aplikacji (wiki jest statyczne w obrazie).
- Włączać publiczne GitHub Pages dla `dagonite-wiki` (wyciek treści + inny host).
- Kopiować pliki wiki do działającego poda bez przebudowy obrazu (chyba że macie custom volume — domyślnie: wszystko w obrazie).
- Commitować `wwwroot/wiki/` do git (generowane przy CI/build).

---

## Rozwiązywanie problemów

| Objaw | Przyczyna | Działanie |
|-------|-----------|-----------|
| **Treść wiki / aplikacji nie zmienia się po redeploy** | Brak `build_wiki_for_empire.sh` przed `docker build`; wdrożono stary tag obrazu; restart bez `docker pull`; zmiany tylko w `dagonite-wiki` bez nowego buildu Empire | Patrz sekcja poniżej |
| „Wiki nie jest wdrożona” | Brak `wwwroot/wiki/index.html` w obrazie | `build_wiki_for_empire.sh` przed `docker build` |
| Czarny ekran po **Home** | Stary build / `/wiki/` bez `index.html` | Najnowszy `master` Empire + przebudowa wiki |
| Gracz widzi cudze wątki | Stary manifest lub `team-bonefyre` na prywatnym wątku | Pull `dagonite-wiki`, przebuduj manifest |
| Explorer pusty | OK — pokazuje tylko dozwolone strony z `contentIndex` | — |
| Zmiana postaci bez efektu | Cache przeglądarki | Ctrl+Shift+R; manifest odświeża się po `mtime` |
| 502 aplikacji | Postgres / app, nie wiki | Logi poda, connection string |

### Treść nie zmienia się po redeploy — checklist

Wykonaj **po kolei** i sprawdź po każdym kroku:

1. **`git pull` w obu repo** na maszynie buildowej (lub poczekaj na CI z najnowszym `dagonite-wiki/main`).
2. **`./build_wiki_for_empire.sh`** — w logu muszą być commity Empire i wiki; powstaje `.wiki-build-info.json`.
3. **`docker build`** z katalogu `DagoniteEmpire/` (tam gdzie jest `DagoniteEmpire/Dockerfile`). Przy podejrzeniu cache: `docker build --no-cache …`.
4. **`docker push`** nowego tagu (dev: tag z datą z Actions, nie zakładaj że jest `latest`).
5. **Na serwerze:** `docker pull <nowy-tag>` → zaktualizuj manifest deploymentu na ten tag → rollout/restart.
6. **Weryfikacja:** `docker exec … cat /app/wwwroot/wiki/.wiki-build-info.json` — `wikiCommit` = oczekiwany SHA.
7. **Przeglądarka:** twarde odświeżenie `/wiki` (service worker + cache HTML).

Typowe błędy:

- Zmiana tylko w `dagonite-wiki` → push do `main`, ale **bez** nowego workflow Empire = stary obraz na GHCR.
- `git pull` Empire na serwerze + restart kontenera **bez** kroków 2–5 = stary HTML w starym obrazie.
- CI dev publikuje `dev.{timestamp}-…` — deployment wskazuje na **stary** tag z poprzedniego tygodnia.

---

## Pierwsze wdrożenie (checklist)

- [ ] Oba repozytoria obok siebie na build server
- [ ] `build_wiki_for_empire.sh` w pipeline CI lub ręcznie przed każdym release z treścią wiki
- [ ] Obraz Docker z `wwwroot/wiki/`
- [ ] Postgres dla aplikacji (postacie, `SelectedCharacterId`)
- [ ] GitHub Pages **wyłączone** + repo **private** (`dagonite-wiki/docs/ACCESS_AND_HOSTING.md`)
- [ ] Test kont: gość, gracz drużyny, MG

---

## Eskalacja

| Temat | Kto |
|-------|-----|
| Kto widzi którą stronę (tagi, wątki) | Autor kampanii + `wiki-parties.json` |
| Docker / K8s / domena | Administrator + ten dokument |
| Błąd aplikacji po logowaniu | Zespół Empire (Blazor / DB) |
