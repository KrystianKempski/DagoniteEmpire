# Plan lokalizacji Dagonite Empire na język polski

> Branch roboczy: `Localisation`
> Data utworzenia planu: 2026-08-15
> Status: **Strategia zatwierdzona — Opcja A. Glosariusz uzgodniony.**
> Nazwa aplikacji w PL: **Dagonitowe Imperium**

---

## 1. Cel i zakres

Przetłumaczyć **całą aplikację** na język polski: interfejs (chrome, nawigacja),
wszystkie zakładki, dialogi, komunikaty (snackbar), tytuły stron, treści opisowe
(tooltips, opisy urzędów, katalogi domenowe), komunikaty backendu oraz dane
seedowe/enumy prezentowane użytkownikowi.

**Poza zakresem** (na tym etapie):
- Tłumaczenie treści generowanych przez użytkownika (posty kampanii, notatki graczy).
- Tłumaczenie treści generowanych przez AI (moduł Scribe) — to zależy od promptów/modelu.
- Zmiana formatów liczb/dat w obliczeniach (`CultureInfo.InvariantCulture` w renderowaniu
  map/SVG **musi zostać** — to serializacja techniczna, nie tekst UI).

---

## 2. Stan obecny (audyt)

Skan repozytorium (branch `master`) wykazał:

- **269** plików `.razor`, kod inline (brak plików `.razor.cs` — logika w blokach `@code`).
- **Brak jakiejkolwiek infrastruktury i18n**: brak `IStringLocalizer`, brak `.resx`,
  brak `AddLocalization`, brak `RequestLocalizationOptions`. Jedyne wystąpienia
  `CultureInfo` to `InvariantCulture` używane technicznie w renderowaniu map/SVG.
- Aplikacja jest **w przeważającej części po angielsku** — także treści domenowe
  (np. `BaronyUiTooltips.cs`, `OfficeDescriptions.cs`, `CourtCommanderCatalog.cs` są po ang.).
- Część "chrome" jest już **częściowo po polsku** (np. nawigacja: „Baronia", „Panel MG",
  niektóre komunikaty snackbar). Oznacza to **niespójność językową**, którą trzeba ujednolicić.

### Szacunkowa objętość stringów do przetłumaczenia

| Źródło | Ilość (szac.) |
|---|---:|
| Teksty inline w `.razor` (`>Tekst<`) | ~1 098 |
| Atrybuty `Label` / `Text` / `Title` / `Placeholder` | ~874 |
| Bloki `MudText` | ~538 (część pokrywa się z inline) |
| Komunikaty `Snackbar.Add(...)` | ~452 |
| `<PageTitle>` | ~61 |
| Literały w katalogach `DA_Common/Barony` (opisy, nazwy, tooltipy) | ~1 458 (część to kody/klucze) |
| **Szacunek łączny stringów użytkownika** | **~3 500 – 5 000** |

Rozkład plików `.razor` wg folderów:

| Folder | Pliki |
|---|---:|
| `Pages/Barony/Components` | 136 |
| `Pages/Dialogs` | 28 |
| `Pages/Barony` | 22 |
| `Account/Pages` (+ `Manage`) | 19 + 13 |
| `Pages/Components` | 18 |
| `Shared` (+ `Account/Shared`) | 8 + 7 |
| `Pages/CharacterPages` | 5 |
| `Pages/GameMaster` | 4 |
| `Pages/Campaign` | 3 |
| `Pages/Wiki`, `Pages/Scribe`, root | 1 + 1 + 3 |

---

## 3. Strategia lokalizacji — do decyzji

Cel to wersja **wyłącznie polska** (nie wielojęzyczna z przełącznikiem). Stąd dwie drogi:

### Opcja A — Tłumaczenie „w miejscu" (rekomendowana) ✅
Zamiana angielskich stringów bezpośrednio na polskie w `.razor` i katalogach `.cs`.
- ✅ Szybciej, bez refaktoryzacji architektury.
- ✅ Zgodne z tym, co już zrobiono (część PL wpisano wprost).
- ✅ Zero narzutu na utrzymanie kluczy/zasobów.
- ❌ Brak łatwego przełączania języka w przyszłości.
- ❌ Zmiany rozproszone po 269 plikach (trudniejszy przegląd „słownika").

### Opcja B — Pełne i18n (`IStringLocalizer` + `.resx` + kultura `pl-PL`)
Owinięcie każdego stringa w lokalizator + plik zasobów z kluczami.
- ✅ Umożliwia przyszłe języki i przełącznik.
- ✅ Centralny „słownik" tłumaczeń (jeden przegląd spójności).
- ❌ Ogromny narzut mechaniczny: refaktor każdego stringa + zarządzanie kluczami.
- ❌ MudBlazor + Blazor Server wymagają dodatkowej konfiguracji kultury per-obwód.

**Wybrano: Opcja A** ✅ (decyzja z 2026-08-15) — pragmatyczna dla prywatnego
narzędzia RPG z jednym językiem docelowym. Sekcja słownika (pkt 4) pełni rolę
spójnościowego „glosariusza". Jeśli w przyszłości pojawi się potrzeba
wielojęzyczności — można migrować do B stopniowo.

---

## 4. Słownik terminów (glosariusz) — UZGODNIONY

Decyzje z 2026-08-15. Spójność terminologii jest obowiązkowa.

| Angielski | Polski | Uwagi |
|---|---|---|
| Barony | Baronia | |
| Baron / ruler | Baron / władca | |
| Character | Postać | |
| Class / Profession | **Profesja** | jeden termin; też „Umiejętność profesji", „Poziom profesji" |
| Equipment | Ekwipunek | |
| Health | Zdrowie | |
| Campaign | Kampania | |
| Chapter / Post | Rozdział / Wpis | |
| Advisor | Doradca | |
| Office | Urząd | |
| Court | Dwór | |
| Decree | Dekret | |
| Commander / Captain | Dowódca | |
| Unit | **Oddział** | |
| Troop count | Liczebność | |
| Army | Armia | |
| Terrain | Teren | |
| Fief | Lenno | |
| Domain | Domena | |
| Trade goods | Towary handlowe | |
| Treaty | Traktat | |
| Resource / Stock | Surowiec / Zapas | |
| Budget | Budżet | |
| Reputation | Reputacja | |
| Relations | Relacje | |
| Audience | Audiencja | |
| Letter | List | |
| Turn | Tura | |
| Battle / Fight | Bitwa / Walka | |
| Skill | Umiejętność | |
| Attribute | **Atrybut** | |
| Trait | **Cecha** | odróżnić od Atrybutu |
| Wound | Rana | |
| Spell | Zaklęcie | |
| Mob / Enemy | Przeciwnik / Potwór | |
| Game Master / GM | **MG / Mistrz Gry** | także w wersji ang. używać „MG", nie „GM" |
| Reputation tier | Poziom reputacji | |

### Nazwy własne i akronimy — reguły specjalne

| Element | Reguła |
|---|---|
| **Dagonite Empire** | Tłumaczyć na **Dagonitowe Imperium** wszędzie, gdzie to możliwe (nagłówek nawigacji, tytuły). |
| **GM → MG** | Zamieniać „GM" na „MG" także w tekstach angielskich (to polski skrót). |
| **PPB** | Polski akronim = **Podstawowe Parametry Baronii** (zasoby i parametry baronii). Zostaje jako „PPB"; ew. angielskie rozwinięcia poprawić na polskie. Nigdy nie używać ang. rozwinięcia. |
| **PHP** | Polski akronim = **Prestiż, Honor i Postrach**. Zostaje jako „PHP"; ew. angielskie rozwinięcia poprawić na polskie. |
| **Wiki** | Zostaje bez zmian. |
| **Scribe** | **Nie tłumaczyć i nie ruszać** — moduł nieużywany (wyłączony z zakresu). |

> Uzupełniać podczas prac. Wątpliwe/wieloznaczne terminy oznaczać `// TODO(term)`.

---

## 5. Konwencje techniczne i stylistyczne

- **Ton wypowiedzi**: **bezosobowy / instrukcyjny** — np. „Wybierz oddział",
  „Zapisano zmiany", „Brak wybranej postaci". Nie stosować formy na „Ty" ani „Pan/Pani".
- **Akronimy PL**: `PPB` i `PHP` traktować jak polskie skróty (patrz pkt 4) — jeśli
  w kodzie istnieje angielskie rozwinięcie (np. „Prestige, Honor…"), poprawić na polskie.
- **`GM` → `MG`**: w każdym widocznym tekście (także angielskim).
- **Scribe**: pomijać całkowicie — nie tłumaczyć plików modułu Scribe.
- **Nie tłumaczyć**: kodów enumów, kluczy słownikowych, nazw zasobów JSON,
  identyfikatorów ról (`SD.Role_*`), `InvariantCulture` w SVG/mapach, nazw ikon MudBlazor.
- **Encje HTML**: polskie znaki diakrytyczne wpisywać wprost w UTF-8 (pliki są UTF-8).
- **Interpolacja**: zachować zmienne `@(...)` / `{0}` w komunikatach — tłumaczyć wokół nich.
- **aria-label / alt / tooltip**: tłumaczyć tak samo jak tekst widoczny.
- **Testy**: po każdej fazie uruchomić build + testy (`dotnet build`, `dotnet test`).
  Uwaga: testy w `DA_Business.Tests` mogą asertować konkretne stringi (np.
  `CombatStateStringTests`, `RichTextTests`) — te trzeba zaktualizować razem z kodem.

---

## 6. Podział na fazy (zakładka po zakładce)

Legenda złożoności: **S** = małe, **M** = średnie, **L** = duże, **XL** = bardzo duże.
Legenda ryzyka: 🟢 niskie, 🟡 średnie, 🔴 wysokie (dużo logiki/asercji w testach).

### Faza 0 — Przygotowanie 🟢 (S)
- [ ] Zatwierdzić strategię (Opcja A vs B) — pkt 3.
- [ ] Zatwierdzić glosariusz — pkt 4.
- [ ] Ustalić kolejność priorytetów (poniższa domyślna: od najczęściej używanych ekranów).
- [ ] Dodać do repo-memory notatkę o konwencjach tłumaczeń.

### Faza 1 — Nawigacja i chrome aplikacji 🟢 (S) — ~10 plików ✅ UKOŃCZONA
Najczęściej widziane elementy; szybki, widoczny efekt.
- [x] `Shared/NavMenu.razor` (nagłówek → „Dagonitowe Imperium")
- [x] `Shared/CharacterNavButtons.razor` (etykiety + wszystkie komunikaty snackbar)
- [x] `Shared/MainLayout.razor`, `Shared/LoginDisplay.razor` (`LoadingPage.razor` — brak tekstu)
- [x] `_DeleteConfirmation.razor`, `_LeavePage.razor` (`ScribeDrawer.razor` — pominięty, moduł Scribe)
- [x] `Pages/Index.razor`, `App.razor`, `Routes.razor` (tytuły, meta, `lang="pl"`)

### Faza 2 — Konto / Tożsamość (Identity) 🟢 (M) — 19 + 13 + 7 plików ✅ UKOŃCZONA
Strony scaffoldowane ASP.NET (angielskie): logowanie, rejestracja, 2FA, zarządzanie kontem.
- [x] `Account/Pages/*.razor` (Login, Register, ForgotPassword, ResetPassword, 2FA, Lockout, …)
- [x] `Account/Pages/Manage/*.razor` (profil, hasło, e-mail, klucze, dane osobowe)
- [x] `Account/Shared/*.razor` (`StatusMessage` — przełącznik koloru na prefiks „Błąd")
- [x] **Faza 2b**: komunikaty walidacji frameworka — `PolishIdentityErrorDescriber` (reguły haseł/konta/role) + polskie `ErrorMessage` przy `[Required]`/`[EmailAddress]`/`[Phone]`

### Faza 3 — System postaci 🟡 (L) — ~18 + 5 + część Dialogs ✅ UKOŃCZONA
Rdzeń rozgrywki gracza-bohatera.
- [x] `Pages/Components/*` (PanelCharacter, Attribute/BaseSkills/SpecialSkills/Traits, Race, Profession, Equipment, Portrait, Languages, Date, BattleStats, BattleMap, BattleDrawer, MobsList)
- [x] `Pages/CharacterPages/*` (CharacterList, CharacterUpsert, ProfessionPage, EquipmentPage, HealthPage)
- [x] Dialogi postaci w `Pages/Dialogs/*` (CreateTrait, CreateWound, CreateRace, CreateProfession(+Skill), AddStatus, SelectLanguage, MakeSkillRoll, Spell, HumanRaceTraits, ExistingTrait/Equipment, EquipmentTemplateSelect, CreateEquipmentSlot, CharDescription, WealthRecords)
- [x] Waluty (lore) → Imperiale/Talary/Halerze/Miedziaki (WealthRecordsDialog); DataLabel mobilne w ProfessionPage spolszczone
- [ ] Odłożone do Fazy 8: TraitLabel z DTO, nazwy trudności / wynik rzutu — patrz QUESTIONS

### Faza 4 — Kampania i czat � (M) — 3 + 2 Dialogs ✅ UKOŃCZONA
- [x] `Pages/Campaign/*` (CampaignList, ChapterList, ChapterThread)
- [x] `Pages/Dialogs/CreateCampaignDialog.razor`, `CreateChapterDialog.razor`
- [x] Chrome wątku/czatu (przyciski, snackbary, dialogi, breadcrumbs, etykiety edytora)
- [ ] Odłożone do Fazy 8 (sprzężone z danymi/pipeline walki): klucze `AlternativeName`
  („Battle turn/started/ended"), `GetBattleSummaryHeader`/`GetBattleSummaryContent`,
  generowane treści postów (podsumowania tur, „Barony resources update")

### Faza 5 — Baronia: strony główne � (L) — 22 pliki ✅ UKOŃCZONA
- [x] `Pages/Barony/*`: ArmyPage, AudiencesPage, BaronCardPage, BudgetPage, BuildingsPage, DomainPanel, KnownLordsPage, LettersPage, LordsSeatPage, MarchMapPage, NotesPage, OfficesPage, ProjectsPage, RelationsPage, ResourcesPage, TerrainPage, TradeGoodsPage, BattleMapPage, BaronyLayout, DemoEnter, DemoModeBanner, StyleLab
- [x] `BattleMapPage.razor` (5797 wierszy) i `MarchMapPage.razor` — chrome UI + snackbary + dialogi spolszczone
- [ ] Odłożone do Fazy 7 (katalogi DA_Common): `*.DisplayName` katalogów (terrain/trade/resource),
  `BudgetSource.*`, `Season` (wartości z backendu wyświetlane w UI) — spolszczyć razem z katalogami
- [ ] Odłożone do Fazy 8 (pipeline walki/logów): narracja dziennika bitwy w BattleMapPage
  („{token.Label} uses Full Defense…", wzory ataku k6), `AddUnitLogEntry`/`SaveAsync(systemMessage:…)`,
  komunikat sprzężony kluczem `"…juz dodana"` (Contains), skróty statów Atk/Def/Dmg/Arm/Prc/Disc/Mv

### Faza 6 — Baronia: komponenty � (XL) — 136 plików ✅ UKOŃCZONA
Wszystkie 136 komponentów w `Pages/Barony/Components/` spolszczone (12 partii × podagent, build 0 błędów).
Pokryte pod-partie tematyczne:
- [ ] **Dwór / Doradcy / Urzędy**: Advisor*, Office*, Court*, Decree*, Audience*
- [ ] **Reputacja / Relacje / Wpływy**: BaronInfluence*, BaronReputation*, Relation*, SocialGroupRelation*, CommunityPenalties
- [ ] **Wojsko / Bitwa**: Unit*, Army*, Battle*, EnemyCommanderAbility, CourtCommander*
- [ ] **Ekonomia / Handel / Budżet**: TradeGoods*, TradeTreaty*, Resource*, Ppb*, Php*, Conjuncture, Project*, Budget-related
- [ ] **Teren / Mapa / Budynki**: Terrain*, MarchMap*, City/Village/Town*, Building*, SeatRoom/SeatGrid/SeatPurpose
- [ ] **Listy / Czas / Artefakty**: BaronLetter*, BaronTime*, BaronArtifact*, Baron reminder/PHP
- [ ] **Pomoc / Tooltipy / UI helpers**: BaronyHelpDialog, BaronyTooltip, BaronyHudTip, BaronyPageHeader, BaronyCardTabs, CharacterMark*

**Wynik Fazy 6 — wszystkie ✅ przetłumaczone.** Odłożone (spójnie z wcześniejszymi fazami):
- Do Fazy 7 (katalogi DA_Common): wszystkie `*.NameEn`/`*.DescriptionEn`/`*.DisplayName`/`*.ShortEn`,
  skróty PPB `PpbShort()` (Food/Econ/Prod/Loy/Stab/Law/Corr/Sci/Mag/Cult/Intel/Def/Gold),
  etykiety katalogów (Seat*/CourtSkill*/UnitEquipment*/TradeGoods* itd.)
- Do Fazy 8 (walka/logi): narracja Full Defense/engagement w BaronyBattleMapGrid + UnitCombatBreakdown,
  skróty statów Atk/Def/Dmg/Arm/Prc/Disc/Mv, kody kolumn siatki umiejętności (Skill/Attr/From/Base/Other/Total —
  sprzężone z prozą wzorów + `_editField`), klucze przechowywane+porównywane ("Unassigned", "__new__",
  "__discipline__", "other", TerrainPresets, ProjectOutputKind Reinforce/ChangeEquipment)

### Faza 7 — Katalogi domenowe (`DA_Common/Barony`) 🔴 (XL) — ~60 plików `.cs`
Duże bloki opisowego tekstu (angielskiego) prezentowanego w UI.
**Podział dwuetapowy** (decyzja): najpierw krótkie etykiety/nazwy, długie opisy później.

**7a — Krótkie etykiety i nazwy (TERAZ):**
- [ ] Nazwy urzędów, dowódców, towarów, poziomów reputacji, etykiety PPB/PHP
- [ ] `BaronReputationTiers.cs`, `BaronPhpSourceLabel.cs`, `TradeGoodLordNames.cs`
- [ ] Krótkie etykiety w `*Catalog.cs`, `*Label*.cs`

**7b — Długie opisy (OSOBNA, PÓŹNIEJSZA TURA):**
- [ ] `BaronyUiTooltips.cs`, `OfficeDescriptions.cs`, `BaronyHelpCatalog.cs`
- [ ] Rozbudowane opisy w `CourtCommanderCatalog.cs`, `KnownLordsCatalog.cs`, `TradeGoodsCatalog.cs`
- [ ] `LuxuryGoodsAccessCatalog.cs`, `TerrainImprovementCatalogMap.cs`, `UnitEquipmentCatalog.cs`
- [ ] Reszta długich `*Descriptions.cs` / opisowych bloków tekstu

### Faza 8 — Backend: komunikaty, enumy, dane seedowe 🟡 (M)
- [ ] Komunikaty w `DA_Business/Services/*` (~89 literałów) i repozytoriach
- [ ] Teksty walki/tur generowane programowo (`DA_Common/Barony/Battle`, `CombatStateString.cs` w `DA_Common`)
- [ ] Enumy prezentowane w UI (8 plików z `enum` w `DA_Models`/`DA_Common`) — dodać mapowanie etykiet PL, nie zmieniać kodów
- [ ] Dane seedowe widoczne w UI (`DA_Models/SeedData`, seedery postaci/baronów) — decyzja: co tłumaczyć
- [ ] **Audyt akronimów**: wyszukać i poprawić ewentualne angielskie rozwinięcia `PPB`/`PHP`
      na polskie (PPB = Podstawowe Parametry Baronii, PHP = Prestiż, Honor i Postrach)
      oraz „GM" → „MG" w całym kodzie

### Faza 9 — Panel MG, Wiki, Scribe 🟡 (M)
- [ ] `Pages/GameMaster/*` (GameMasterPanel, CreateBaronyDialog, GmBaronySection, GmApprovedEquipmentSection)
- [ ] `Pages/Wiki/WikiPage.razor` + `Components/WikiNavLink.razor`
- [ ] ~~`Pages/Scribe/ScribePage.razor`~~ — **poza zakresem** (moduł Scribe nieużywany, nie tłumaczyć)

### Faza 10 — QA, testy, spójność 🟡 (M)
- [ ] Zaktualizować testy asertujące stringi (`CombatStateStringTests`, `RichTextTests`, `WoundsTurnSummaryTests`, `CombatStateStringTests` itp.)
- [ ] `dotnet build` + `dotnet test` — zielone.
- [ ] Przegląd spójności terminologii wg glosariusza (grep pozostałych ang. słów kluczowych).
- [ ] Przegląd wizualny kluczowych ekranów (przycięcia tekstu, layout MudBlazor).
- [ ] Sprawdzić brak „przecieków" ang. w snackbarach i tooltipach.

---

## 7. Szacunek zbiorczy nakładu

Bez podawania czasu kalendarzowego — miara w plikach, stringach i względnej złożoności.

| Faza | Pliki (szac.) | Stringi (szac.) | Złożoność |
|---|---:|---:|---|
| 0 Przygotowanie | – | – | S |
| 1 Chrome/nawigacja | ~10 | ~80 | S |
| 2 Konto/Identity | ~39 | ~350 | M |
| 3 System postaci | ~30 | ~500 | L |
| 4 Kampania/czat | ~8 | ~120 | M |
| 5 Baronia strony | 22 | ~700 | L |
| 6 Baronia komponenty | 136 | ~1 500 | XL |
| 7 Katalogi domenowe | ~60 | ~1 000 | XL (7a teraz / 7b później) |
| 8 Backend/enumy/seed | ~40 | ~300 | M |
| 9 MG/Wiki | ~6 | ~110 | M |
| 10 QA/testy | – | – | M |
| **Razem** | **~350 plików** | **~4 500 stringów** | **XL (projekt wieloetapowy)** |

**Ścieżka krytyczna nakładu**: Fazy 6 i 7 (Baronia) to ~55–60% całości.

---

## 8. Rekomendowana kolejność realizacji

1. Faza 0 (decyzje) → 1 (szybki widoczny efekt) → 2 (wejście do apki).
2. Faza 3 → 4 (rdzeń gracza-bohatera).
3. Faza 5 → 6 → 7 (Baronia — największy blok, partiami tematycznymi).
4. Faza 8 (backend/enumy) równolegle wspiera 5–7.
5. Faza 9 → 10 (domknięcie i QA).

Po **każdej** fazie: build + testy + commit na branchu `Localisation`.

---

## 9. Ryzyka i uwagi

- 🔴 **Testy asertujące stringi** — zmiana tekstu psuje testy; aktualizować równolegle.
- 🟡 **Layout MudBlazor** — polskie napisy bywają dłuższe; możliwe przycięcia/łamania.
- 🟡 **Niespójność początkowa** — część UI już po PL; łatwo o dublowanie/kolizje terminów.
- 🟡 **Treści AI (Scribe)** — jeśli prompty są ang., PL UI może zderzać się z ang. wyjściem.
- 🟢 **Ryzyko techniczne niskie** — brak zmian architektury (przy Opcji A).
- ⚠️ **Nie ruszać** `InvariantCulture` w renderowaniu map/SVG (to nie tekst UI).

---

## 10. Śledzenie postępu

Odhaczać checkboxy w pkt 6 w miarę postępu. Sugerowana konwencja commitów:

```
i18n(pl): <faza/obszar> — <krótki opis>
```

Przykład: `i18n(pl): Faza 1 — nawigacja i przyciski postaci`
