# Wątpliwości i pytania do lokalizacji (PL)

> Tu zbieram pytania/niejasności napotkane w trakcie tłumaczenia.
> Dopisuj odpowiedzi pod pytaniami (np. w linii `ODP:`), a ja je uwzględnię.

## Status: 3 zrealizowane (waluty, DataLabel, Faza 2b), 2 odroczone do Fazy 8 (TraitLabel, trudność/rzut)

<!-- Format:
### [Faza X] Krótki tytuł — plik/miejsce
Pytanie...
ODP:
-->

### [Faza 7] Strategia nazw w katalogach (DA_Common/Barony) — NamePl/NameEn
Część katalogów jest już dwujęzyczna (Ppb.cs, CourtCharacterSkills.cs mają `NamePl`),
reszta ma tylko `NameEn`/`DisplayName`/`BonusSummary`. Jak spolszczyć spójnie?
ODP: **Hybryda** — (1) gdzie `NamePl` istnieje: nie ruszać `NameEn`, tylko przełączyć
konsumentów UI na `NamePl` i usunąć `UseEnglishLabels="true"`; (2) gdzie `NamePl` brak:
nadpisać angielskie literały polskimi w miejscu (czysta Opcja A, bez nowych pól);
(3) nigdy nie ruszać wartości używanych jako klucze dopasowań (TradeGoodLordNames,
KnownLordsCatalog, DisplayName porównywane w kodzie) — weryfikować każdą.
Podział: **7a i 7b razem** w jednym przebiegu i jednym commicie.

### [Faza 2] Komunikaty walidacji i błędów z frameworka (Identity / DataAnnotations)
Domyślne komunikaty ASP.NET Core są po angielsku i NIE są tekstem w naszych plikach:
- Błędy hasła/rejestracji z `IdentityErrorDescriber` (np. „Passwords must have at least one digit").
- Walidacja `DataAnnotations` bez własnego `ErrorMessage` (np. „The Email field is not a valid e-mail address").
Aby je spolszczyć, trzeba dodać (a) własny `IdentityErrorDescriber` po polsku
oraz (b) lokalizację DataAnnotations lub jawne `ErrorMessage` przy każdym polu.
Proponuję zrobić to w osobnej mini-turze (Faza 2b) albo w Fazie 10 (QA).
ODP: zrób kiedy Ci pasuje
✅ ZROBIONE: dodano `PolishIdentityErrorDescriber` (zarejestrowany w Program.cs) oraz
polskie `ErrorMessage` przy `[Required]`/`[EmailAddress]`/`[Phone]` we wszystkich stronach Account.

### [Faza 3] Nazwy walut (lore) — WealthRecordsDialog
Waluty świata gry: **Imperials / Talars / Hellers / Coppers** zostawiłem po angielsku
(nazwy własne/lore). Czy spolszczyć na Imperiały / Talary / Halerze / Miedziaki?
Uwaga: te nazwy mogą występować też w wielu innych miejscach (majątek postaci, baronia).
ODP: spolszcz na Imperiale / Talary/ Halerze /miedziaki
✅ ZROBIONE: WealthRecordsDialog — etykiety i tytuły kolumn → Imperiale/Talary/Halerze/Miedziaki
(nazwy właściwości/pól DTO bez zmian; poza tym waluty pojawiają się głównie jako ikony).

### [Faza 3] Etykiety mobilne DataLabel — ProfessionPage
Atrybuty `DataLabel="..."` (Spell slots, Prepared, Level, Known, Today, ready, actions)
pokazywane przez MudBlazor jako nagłówki kolumn na wąskim ekranie — zostały po angielsku
(poza pierwotnym allowlistem). Spolszczyć dla spójności na mobile? (drobne)
ODP:spolszczyć. 
✅ ZROBIONE: Poziom/Znane/Dzisiaj/Sloty zaklęć/Przygotowane/Gotowe/Akcje.

### [Faza 3] Podpisy przycisków z DTO (TraitLabel) — TraitsComponent
Napisy „Dodaj …/Istniejące …" biorą się z właściwości DTO `TraitLabel` (w DA_Models),
a nie z markup. Pełne spolszczenie wymaga tłumaczenia w DTO — do zrobienia w Fazie 8.
ODP: ok

### [Faza 3] Nazwy poziomów trudności / wynik rzutu — MakeSkillRollDialog
`SD.GetDifficultyName(...)` i wyjście `RollService` (DA_Common) nadal po angielsku —
do spolszczenia w Fazie 8 (backend).
ODP:ok

---

## Rozstrzygnięte
<!-- Przenoszę tu pytania z odpowiedziami, żeby był ślad decyzji. -->
