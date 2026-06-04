# Wiki DagoniteEmpire — instrukcja dla administratora serwera

Dokument dla osoby wdrażającej aplikację na produkcję (Docker / Kubernetes).  
Reguły biznesowe i rozwój: [WIKI_DEPLOY.md](./WIKI_DEPLOY.md).

---

## Szybki start

1. Sklonuj **oba** repozytoria obok siebie (`Dag1/DagoniteEmpire` + `Dag1/dagonite-wiki`).
2. Na maszynie buildowej: `./DagoniteEmpire/tools/Scripts/build_wiki_for_empire.sh`
3. Zbuduj obraz Docker z katalogu solution (zawiera `wwwroot/wiki/` z kroku 2).
4. Wdróż obraz. Wiki jest pod ścieżką `/wiki` tej samej domeny co aplikacja.

**Bez kroku 2** użytkownicy zobaczą: „Wiki nie jest jeszcze wdrożona na tym serwerze”.

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

### 4. Rollout + testy

| Test | URL / akcja | Oczekiwany wynik |
|------|-------------|------------------|
| Wiki działa | `/wiki` | Iframe z hubem kampanii lub lore |
| Lore publiczne | `/wiki/świat-i-zasady/` | Bez logowania |
| Sekret | URL sesji jako inna postać | „Brak dostępu” (403), nie treść |
| Home w wiki | Breadcrumb „Home” | Hub `index.html`, nie czarny ekran |
| Manifest | `/wiki/static/wiki-access.json` jako gracz | 403 |
| MG | Zalogowany Admin/MG | Pełna wiki, explorer, wszystkie strony |

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
| „Wiki nie jest wdrożona” | Brak `wwwroot/wiki/index.html` w obrazie | `build_wiki_for_empire.sh` przed `docker build` |
| Czarny ekran po **Home** | Stary build / `/wiki/` bez `index.html` | Najnowszy `master` Empire + przebudowa wiki |
| Gracz widzi cudze wątki | Stary manifest lub `team-bonefyre` na prywatnym wątku | Pull `dagonite-wiki`, przebuduj manifest |
| Explorer pusty | OK — pokazuje tylko dozwolone strony z `contentIndex` | — |
| Zmiana postaci bez efektu | Cache przeglądarki | Ctrl+Shift+R; manifest odświeża się po `mtime` |
| 502 aplikacji | Postgres / app, nie wiki | Logi poda, connection string |

---

## Pierwsze wdrożenie (checklist)

- [ ] Oba repozytoria obok siebie na build server
- [ ] `build_wiki_for_empire.sh` w pipeline CI lub ręcznie przed każdym release z treścią wiki
- [ ] Obraz Docker z `wwwroot/wiki/`
- [ ] Postgres dla aplikacji (postacie, `SelectedCharacterId`)
- [ ] GitHub Pages **wyłączone** w `dagonite-wiki`
- [ ] Test kont: gość, gracz drużyny, MG

---

## Eskalacja

| Temat | Kto |
|-------|-----|
| Kto widzi którą stronę (tagi, wątki) | Autor kampanii + `wiki-parties.json` |
| Docker / K8s / domena | Administrator + ten dokument |
| Błąd aplikacji po logowaniu | Zespół Empire (Blazor / DB) |
