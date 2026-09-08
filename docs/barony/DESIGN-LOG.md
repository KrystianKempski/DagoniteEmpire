# Barony — Dziennik Decyzji Projektowych

> Chronologiczny zapis decyzji architektonicznych i ich uzasadnień.
> Nowe decyzje dopisujemy na górze.

---

## 2026-09-08 — Okno rozstrzygania tury

### D-TR1. Osobna flaga `Barony.TurnResolving`
Rozstrzygnięcie tury nie kończy pracy MG — po nim MG dopisuje audiencje, wydarzenia
i listy. `Resolve Turn` ustawia `TurnResolving`, `Zakończ rozstrzyganie` je zdejmuje.
Flaga jest niezależna od `PlayerTurnReady` (tamta mówi „gracz skończył turę”, ta
„MG jeszcze pisze”).

### D-TR2. Blokada po stronie zakładek, nie repozytorium
Baron nie wchodzi na Salę audiencji, Audiencje, Projekty, Zasoby (i Budżet) oraz Listy —
zakładki są wyszarzone z kłódką, a strony renderują `BaronyTurnResolveLock` zamiast treści
(lista bram: `BaronyTurnGate`). MG zachowuje pełny dostęp. Mutacje w repozytorium nie są
blokowane: warstwa Blazor Server i tak renderuje się serwerowo, więc bramka w UI wystarcza.

### D-TR3. Natychmiastowa blokada przez SignalR
`ChatHub.NotifyBaronyTurnResolvingChanged` (nadaje Panel Domeny, odbiera `BaronyCardTabs`)
przełącza stan u barona bez czekania na przeładowanie strony; gdy baron stoi na bramkowanej
zakładce, strona jest przeładowywana. Gdy hub zawiedzie, blokada i tak zadziała przy
następnym wejściu na stronę.

---

## 2026-08-09 — Kampania przy tworzeniu baronii

### D-CAMP1. Seed kampanii w `CreateForCharacter`
Gdy GM tworzy baronię dla postaci Duke, w tym samym zapisie powstaje `Campaign`
o nazwie baronii, z baronem w `Campaign.Characters`. Bez FK `Barony↔Campaign`
(D2 nadal obowiązuje: baronia to atrybut postaci). Cel: baron od razu ma wątek
kampanii/threadów, bez ręcznego tworzenia kampanii.

---

## 2026-08-05 — Court commanders (model B)

### D-CMD1. Gałęzie + trzon
Zdolności dowódcy: wspólny **Trunk** oraz **Shock / Line / Skirmish / Cunning**.  
Court Attack/Defence **nie** wpływają na Cmd — tylko odblokowane ability.

### D-CMD2. Przypisanie 1:1
`BaronyUnit.CaptainAvailableAdvisorId` → AvailableAdvisor. Jeden Courtier na jeden oddział; oddział bez kapitana jest OK.

### D-CMD3. Dwa T3 szarży
**Thunder Charge** (+3 Atk/+2 Dmg) oraz **Flying Start** (min. ścieżka −1). Osobne klucze, niezależne.

Dokument: [`COMMANDER.md`](./COMMANDER.md).

---

## 2026-07-10 — Fundamenty warstwy Baronii

### D1. Rola Barona = istniejąca `DukePlayer` + `NPCType.Duke`
W kodzie już istnieje rola Identity `DukePlayer` oraz typ postaci `NPCType.Duke`
(gate tworzenia postaci Duke). Reużywamy ich zamiast tworzyć nową rolę.
Branch nazywa się `Duke`, co spójne z tą decyzją.

### D2. Baronia należy do postaci (Character)
Relacja 1:1: `Barony.CharacterId → Character`. Postać jest wiązana z użytkownikiem
przez `Character.UserName` (istniejący wzorzec własności w aplikacji).
Alternatywy odrzucone: powiązanie z `ApplicationUser` (mniej spójne z resztą gry),
z `Campaign` (baronia to atrybut postaci, nie kampanii).

### D3. Formuły — podejście hybrydowe
v1: obliczenia w C# + zapisany czytelny tekst formuły do podglądu (hover).
Pełny edytowalny silnik wyrażeń z DB → osobna, późniejsza faza.
Powód: szybkie dostarczenie wartości, uniknięcie przedwczesnej złożoności silnika.

**Katalog dokumentacyjny (2026-07-19):** [`FORMULAS.md`](./FORMULAS.md) + [`formulas.json`](./formulas.json)
— zbiorczy rejestr formuł; JSON jako seed pod przyszły silnik.

### D4. Centralny typ `PpbVector`
Wszystkie tabele Panelu Domeny mają kolumny = PPB. Wprowadzamy jeden typ wartości
`PpbVector` (13 pól decimal), używany jako `Additive` i `Percent` przez każde
źródło modyfikatora. Upraszcza UI (jednolity render tabel) oraz obliczenia
(sumowanie i podsumowanie). Wektory składowane jako owned type / JSON
(wzorem `BattleMap.CellsJson`).

### D5. Styl ograniczony do stron baronii
Aby nie ryzykować regresji na istniejących stronach (globalny `MudThemeProvider`
jest obecnie domyślny), motyw baronii wprowadzamy jako scope (klasy CSS + osobny
`barony.css` / komponenty), bez zmiany globalnej palety aplikacji.

### D6. Fazowanie
Faza 0: branch + dokumenty. Faza 1: system stylów + komponenty (sekcja rozwijana,
dolny pasek kart). Faza 2: rdzeń modelu danych + migracja + repo + DI.
Faza 3: Panel Domeny. Faza 4+: pozostałe strony osobnymi etapami.
