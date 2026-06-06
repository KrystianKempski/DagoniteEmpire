# Wiki w DagoniteEmpire

**Instrukcja dla administratora serwera (Docker/K8s):** [WIKI_DEPLOY_OPS.md](./WIKI_DEPLOY_OPS.md)

## Architektura (dwa repozytoria)

| Repo | Rola |
|------|------|
| **[dagonite-wiki](https://github.com/…/dagonite-wiki)** (obok Empire w `Dag1/`) | Treść Markdown, Quartz, `wiki-parties.json`, tagi w frontmatter |
| **DagoniteEmpire** | Aplikacja ASP.NET — serwuje build pod `/wiki/*`, ACL per postać |

Wiki **nie** jest osobnym serwisem. Host produkcyjny: `https://dagonite-empire.drik.it/wiki` (statyczne pliki + middleware ACL w tej samej aplikacji).

```
Dag1/
├── dagonite-wiki/     ← autorzy treści, npx quartz build
└── DagoniteEmpire/    ← dotnet run, wwwroot/wiki/ (generowany przy buildzie obrazu)
```

## Integracja w aplikacji

- Zakładka **Wiki** → `/wiki` (`WikiPage.razor`): pasek MudBlazor + **`<iframe>`** ładujący Quartz pod `/wiki/…`.
- Build Quartz kopiowany do `DagoniteEmpire/wwwroot/wiki/` (nie commitowany do git — patrz `.gitignore`).

### Dlaczego iframe?

Quartz buduje **samodzielną statyczną aplikację web** (HTML, bundlowany JS, SPA-routing, explorer, search). DagoniteEmpire to **osobna aplikacja Blazor** — nie da się wkleić wygenerowanych stron Quartz jako komponentów Razor bez przepisania całej wiki.

**Iframe to tu świadomy wybór**, nie obejście:

| Powód | Wyjaśnienie |
|--------|-------------|
| Osobny runtime | Quartz ma własny JS (nawigacja, explorer, Mermaid). Iframe izoluje go od Blazor bez konfliktów DOM/routera. |
| Ten sam origin | `src="/wiki/…"` na tej samej domenie — żądania z iframe niosą te same cookies i sesję ASP.NET co reszta aplikacji. |
| ACL po stronie serwera | Każde żądanie `/wiki/*` (także z iframe) przechodzi `WikiStaticFileMiddleware` — filtrowanie nie musi być w JS. |
| Mały koszt utrzymania | Build Quartz → kopia do `wwwroot/wiki/`; bez forkowania szablonów ani przepisywania UI wiki w MudBlazor. |

**Alternatywy odrzucone:** przepisanie wiki w Blazor (zbyt duży nakład); osobna subdomena (gorsze UX, ten sam problem osadzenia w zakładce); „goły” redirect z `/wiki` na statyczne HTML (tracimy chrome aplikacji na głównej zakładce Wiki).

Głębokie linki (`WikiNavLink` z `forceLoad`) omijają shell Blazor i ładują stronę Quartz bezpośrednio — to ten sam build, ten sam middleware ACL.
- **WikiStaticFileMiddleware** — każde żądanie `/wiki/*` przechodzi ACL przed serwowaniem pliku.
- **WikiAccessService** + manifest `wwwroot/wiki/static/wiki-access.json` (generowany ze tagów).
- Tożsamość wiki: **suma wszystkich zatwierdzonych postaci** gracza (`Character.IsApproved`). Wybór w menu (**Select character**) nie jest wymagany do ACL.

## Widoczność (skrót)

| Typ treści | Kto widzi |
|------------|-----------|
| `wiki-public` / **Świat i zasady** | Wszyscy (bez logowania) |
| `wiki-logged-in` / index, mapy | Zalogowany + zatwierdzona postać z drużyny kampanii |
| Tag bohatera (`lawenda`, `werner`, …) | Tylko ta postać (+ ewentualnie inne tagi na stronie) |
| `team-bonefyre` / `team-pijany-smok` | Członkowie danej drużyny |
| **Wątek z imieniem PC w tytule** (np. „Klątwa Lawendy”) | Tylko ten bohater i wpisani współwiedzący — **bez** `team-bonefyre` |
| Kampania (folder) | Uczestnik drużyny z `wiki-parties.json` |
| **MG / Admin** | Wszystko (pełny explorer, search, manifest) |

Szczegóły tagów: `dagonite-wiki/content/_meta/wiki-parties.json`.

## Model dostępu (runtime)

1. **Tagi** w frontmatter → `build_wiki_access_manifest.py` → `wiki-access.json`.
2. **Middleware** sprawdza slug (normalizacja końcowego `/`, mapowanie folderów na `index`).
3. Brak dostępu → **403** + `wiki-access-denied.html` (strony) lub **404** (zasoby graficzne — bez zdradzania istnienia pliku).
4. **Explorer Quartz** — widoczny u graczy; drzewo budowane z **filtrowanego** `contentIndex.json` (tylko dozwolone strony).
5. **MG/Admin** — pełna wiki bez filtrów indeksu.
6. Pliki chronione: `wiki-access.json`, `wiki-parties.json`, `wiki-links.json` — tylko MG/Admin po HTTP (`wiki-links` czytany też po stronie serwera w `WikiLinkService`).

### Co middleware filtruje

| Zasób | Zachowanie |
|-------|------------|
| `static/contentIndex.json` | Per postać (fail-closed przy braku manifestu) |
| `static/encryptedContentIndex.json` | Gracze: puste `entries` |
| `sitemap.xml` | Tylko dozwolone URL-e |
| Obrazy/og-image przy stronach | ACL strony-właściciela |
| `/wiki/` lub `/wiki` (bez pliku) | Serwuje `index.html` hubu (nie pusty shell Blazor) |

## Publikacja na serwer

**Pełna procedura, CI, tagi obrazów i troubleshooting:** [WIKI_DEPLOY_OPS.md](./WIKI_DEPLOY_OPS.md)

Kluczowe: treść wiki jest **wbudowana w obraz Docker** przy `build_wiki_for_empire.sh`. Sam `git pull` lub restart kontenera **nie** aktualizuje stron. Po zmianie tylko w `dagonite-wiki` trzeba zbudować nowy obraz Empire (push do `master` lub ręczny *Run workflow* w Actions).

### Standard (tylko treść wiki)

```bash
cd /ścieżka/Dag1/dagonite-wiki
git pull origin main

cd /ścieżka/Dag1/DagoniteEmpire
git pull origin master

cd tools/Scripts
chmod +x build_wiki_for_empire.sh
./build_wiki_for_empire.sh   # tworzy wwwroot/wiki/ + .wiki-build-info.json
```

Następnie `docker build` + `docker push` + wdrożenie **nowego tagu** obrazu (wiki musi być już w `wwwroot/wiki/`).

### Pełny pipeline (docx → vault → wiki)

```bash
cd DagoniteEmpire/tools/Scripts
./sync_wiki_pipeline.sh
```

Kolejność: `build_obsidian_vault.py` → `sync_vault_to_quartz.sh` → `build_wiki_for_empire.sh`.

### Tylko drużyny / manifest ACL

Edycja `dagonite-wiki/content/_meta/wiki-parties.json` lub tagów w `.md`, potem:

```bash
cd DagoniteEmpire/tools/Scripts
python3 build_wiki_access_manifest.py   # wymaga wcześniejszego public/ z quartza
# lub pełne:
./deploy_wiki_to_wwwroot.sh
```

`NPCName` w bazie musi odpowiadać nazwom w `characters` (aliasy w `characterAliases`).

## Edycja treści i tagów (autor)

W `dagonite-wiki`:

1. Edytuj `content/**/*.md` — frontmatter `tags: [lawenda, …]`.
2. Dla wątków prywatnych: **imię bohatera w tytule** + tylko tagi konkretnych postaci (nie `team-bonefyre`).
3. `npx quartz build` (lokalnie podgląd) — **produkcja** i tak buduje Empire.

Po zmianach tagów na serwerze buildowej: `./build_wiki_for_empire.sh` + nowy obraz Docker.

Skrypt `tag_wiki_content.py` (opcjonalnie, masowo):

```bash
cd DagoniteEmpire/tools/Scripts
python3 tag_wiki_content.py
python3 build_wiki_access_manifest.py
```

## Test po wdrożeniu

- [ ] Gość: `/wiki/świat-i-zasady/` → lore; sekret kampanii → 403/404
- [ ] Gracz (np. Werner): wątek „Klątwa Lawendy” → brak dostępu; własne wątki → OK
- [ ] Explorer: tylko strony dozwolone dla postaci
- [ ] Link **Home** w breadcrumb → hub wiki (`index.html`), nie czarny ekran
- [ ] `/wiki/static/wiki-access.json` jako gracz → 403
- [ ] MG: pełna wiki + manifest + diagnostyka dev: `/wiki/debug/access.json`

## Konfiguracja Quartz (opcjonalnie)

W `dagonite-wiki/quartz.config.yaml`:

```yaml
baseUrl: dagonite-empire.drik.it/wiki
```

`deploy_wiki_to_wwwroot.sh` i tak przepisuje `/dagonite-wiki` → `/wiki`.

## GitHub Pages i dostęp do repo (dagonite-wiki)

Wiki produkcyjne **tylko** w Empire. Publiczne `github.io` wyłącz w ustawieniach GitHub; repo ustaw na **private** i dodaj collaboratorów — krok po kroku: `dagonite-wiki/docs/ACCESS_AND_HOSTING.md`. Workflow CI buduje artefakt bez publikacji na Pages.

## DukePlayer

`dukeAccessibleCampaignIds` w `wiki-parties.json` — lore + wybrane kampanie bez bohatera z drużyny. Z postacią w `characters` — normalny ACL.

## Rozwój lokalny

```bash
# Terminal 1 — baza
docker start postgres-pgvector

# Terminal 2 — aplikacja
cd DagoniteEmpire/DagoniteEmpire
dotnet run --launch-profile http

# Po zmianie treści wiki:
cd ../tools/Scripts && ./build_wiki_for_empire.sh
```

Wiki: http://127.0.0.1:5093/wiki

Testy ACL: `dotnet test DA_Business.Tests --filter WikiAccessEvaluatorTests`
