# Wiki w DagoniteEmpire

## Integracja

- Zakładka **Wiki** w panelu → `/wiki` (layout + iframe).
- Statyczny build Quartz w `DagoniteEmpire/wwwroot/wiki/`.
- Reguły dostępu: `dagonite-wiki/content/_meta/wiki-parties.json` (drużyny, kampanie).
- Manifest: `wwwroot/wiki/static/wiki-access.json` (generowany przy deploy).

### Widoczność (skrót)

- **Świat i zasady** — bez logowania.
- **Scena 3+ graczy** — cała drużyna (lub obie drużyny, jeśli gracze z dwóch party).
- **Scena 1–2 graczy** — tylko te postacie.
- **Wstęp …** — tylko bohater z tytułu.
- **NPC** — wszyscy zalogowani, chyba że w tekście jeden kontakt z bohaterem.
- **Wątki kroniki** — publiczne dla zalogowanych, chyba że dotyczą jednego bohatera (tag/treść).
- **Kampania W służbie Bonefyre** — postacie z drużyn Bonefyre + Pijany Smok (obie w kampanii).
- **MG/Admin** — wszystko.

## Publikacja na serwer (Docker)

1. Zaktualizuj treść i zbuduj Quartza:

   ```bash
   cd dagonite-wiki
   npx quartz build
   ```

2. Skopiuj do aplikacji (przed `docker build`):

   ```bash
   cd DagoniteEmpire/tools/Scripts
   ./deploy_wiki_to_wwwroot.sh
   ```

3. Zbuduj obraz Docker z katalogu `DagoniteEmpire` (zawiera `wwwroot/wiki`).

4. Wyłącz **GitHub Pages** w ustawieniach repo `dagonite-wiki` (workflow `deploy.yml` publikuje tylko ręcznie przez `workflow_dispatch`).

### Jedna komenda (build + kopiowanie)

```bash
cd DagoniteEmpire/tools/Scripts
./build_wiki_for_empire.sh
```

## Faza 3 — bezpieczeństwo (serwer)

Middleware filtruje lub blokuje:

| Plik | Zachowanie |
|------|------------|
| `static/contentIndex.json` | Filtrowany per postać (wyszukiwarka) |
| `static/encryptedContentIndex.json` | Gracze: puste `entries` (MG: pełny) |
| `sitemap.xml` | Tylko URL-e dozwolone dla użytkownika |
| `static/wiki-access.json`, `wiki-parties.json` | Tylko Admin/MG (404 dla reszty) |

## Test po wdrożeniu

- [ ] Gość: `/wiki` → lore (`świat-i-zasady`), sekrety → 404
- [ ] Gracz A: nie widzi wstępu postaci B (bezpośredni URL → 404)
- [ ] Wyszukiwarka: brak tytułów cudzych sekretów
- [ ] `/wiki/static/wiki-access.json` jako gracz → 404
- [ ] `/wiki/sitemap.xml` jako gracz → tylko własne ścieżki
- [ ] MG: pełna wiki + manifest

### baseUrl Quartza (opcjonalnie)

Dla czystszego buildu ustaw w `dagonite-wiki/quartz.config.yaml`:

```yaml
baseUrl: dagonite-empire.drik.it/wiki
```

Skrypt `deploy_wiki_to_wwwroot.sh` i tak przepisuje ścieżki z `/dagonite-wiki` na `/wiki`.

## Edycja drużyn

Zmień `dagonite-wiki/content/_meta/wiki-parties.json`, potem:

```bash
./deploy_wiki_to_wwwroot.sh
```

`NPCName` w bazie musi odpowiadać nazwom w `characters` (np. `Lawenda`, `Werner` dla Granita).

## Faza 4 — utrzymanie i UX

### Pełny pipeline treści

```bash
cd DagoniteEmpire/tools/Scripts
chmod +x sync_wiki_pipeline.sh build_wiki_for_empire.sh
./sync_wiki_pipeline.sh
```

Kolejność: `build_obsidian_vault.py` → `sync_vault_to_quartz.sh` → `build_wiki_for_empire.sh`.

### Podgląd MG „jako gracz”

Na `/wiki` pasek u góry (Admin/MG): wybór postaci → cookie `wiki_view_as` → middleware stosuje reguły jak dla tej postaci. **Pełny dostęp MG** = wyczyść podgląd.

API (opcjonalnie): `POST/DELETE/GET /api/wiki/view-as`.

### Linki wiki ↔ aplikacja

- `static/wiki-links.json` — mapowanie postaci / kampanii / rozdziałów (generowane z manifestem).
- Komponent `WikiNavLink` na liście postaci, kampanii i w wątku rozdziału (`ChapterThread`).

### DukePlayer

W `wiki-parties.json`: `dukeAccessibleCampaignIds` — Duke bez bohatera z drużyny wiki widzi tylko lore (`świat-i-zasady`) i slugi kampanii z tej listy. Duke z postacią w `characters` ma normalny dostęp.
