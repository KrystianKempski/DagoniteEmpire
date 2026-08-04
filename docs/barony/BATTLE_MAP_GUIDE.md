# Battle Map Guide (User)

Ten dokument opisuje zasady rozgrywki na Battle Map z perspektywy gracza: fazy tury, ruch, szarżę, planowanie ataku i rozstrzygnięcie walki.

## Powiązane dokumenty

- Szczegółowe wzory i źródła statystyk: [`FORMULAS.md`](./FORMULAS.md)
- Kontekst systemu Barony: [`CONCEPT.md`](./CONCEPT.md)

## Przebieg tury na Battle Map

Każda runda przebiega w fazach:

1. **Movement** - planowanie i wykonanie ruchu oddziałów.
2. **Attack Planning** - wskazanie celu ataku dla oddziałów, które mają kontakt z wrogiem.
3. **Combat** - rozstrzygnięcie wszystkich wymian obrażeń i ewentualnych ucieczek.

## Movement

Oddziały planują trasy po kolei, od najniższej inicjatywy, ale **wykonują ruch równocześnie**:
po zatwierdzeniu ostatniego planu wszystkie ruszają w tej samej chwili i przez cały przebieg
reagują na to, co faktycznie dzieje się na mapie. Cały przebieg liczy jedna symulacja, a to, co
widać na mapie, jest jej wiernym odtworzeniem — animacja i wynik nigdy się nie rozjeżdżają.

### Koszt ruchu

- Krok prosto (N/E/S/W) kosztuje **1** punkt ruchu, krok na skos **1,5**.
- Pole **Difficult** podwaja koszt kroku, pole **Impassable** blokuje przejście.
- Budżet oddziału to `Move` punktów z zapasem pół punktu, stąd tabela skosów:
  Move 3 → 2 skosy, Move 4 → 3, Move 5 → 3, Move 6 → 4.
- Trasę planuje się waypointami; obrót oddziału również kosztuje.

### Prędkość

- Czas kroku jest proporcjonalny do jego kosztu i odwrotnie proporcjonalny do `Move`.
  Oddział o `Move 8` przebywa w tym samym czasie dwa razy dłuższą drogę niż oddział o `Move 4`.
- Skos trwa półtora raza dłużej niż krok prosty, więc **fizyczna prędkość jest jednakowa we
  wszystkich ośmiu kierunkach** — marsz zygzakiem niczego nie przyspiesza.
- Nikt nie dostaje fory na starcie; wszyscy ruszają jednocześnie.

### Spotkania na mapie

- Oddział rezerwuje pole, na które wchodzi, na cały czas trwania kroku, więc nikt się przez nie
  nie prześlizgnie. Wrogi oddział osłania dodatkowo pole, które właśnie opuszcza.
- **Wróg na drodze zatrzymuje ruch** na miejscu, w kontakcie bojowym — oddział obraca się
  przodem do przeciwnika.
- **Sojusznik na drodze tylko opóźnia**: oddział czeka, aż pole się zwolni. Jeśli zator nie
  ustąpi (mniej więcej czas trzech kroków), oddział zatrzymuje się przed nim i trafia to do
  journalu.
- Gdy dwa oddziały sięgają po to samo pole w tej samej chwili, pierwszeństwo ma **szarża**,
  a w jej braku **wyższa inicjatywa**.
- Krok na skos jest niemożliwy, gdy zajęte są **oba** sąsiednie pola narożne — nie da się
  przecisnąć między dwoma oddziałami ani przeniknąć przez siebie po krzyżujących się skosach.
- Dwa oddziały nigdy nie kończą ruchu na tym samym polu.

## Szarża (Charge)

Szarża jest specjalnym wariantem ruchu wykonywanym w fazie **Movement**.

- Szarża idzie po **prostej linii** (8 kierunków: orto + skosy).
- Minimalny dystans:
  - **3 kafelki** dla kierunków prostych (N/E/S/W) = **3** ruchu,
  - **2 kafelki** dla skosów = **3** ruchu.
- Koszt skosów jak w zwykłym ruchu (1 / 2 / 1 / 2… w punktach, łącznie `floor(3n/2)`):  
  **3** move → 2 skosy, **4** → 3, **5** → 3, **6** → 4, …
- Maksymalny dystans wprost/bokami = pozostały `Move` (1 kafelek = 1 ruch); na skos wg tabeli powyżej. Difficult ×2.
- Szarżę można rozpocząć także z aktualnego końca zaplanowanej ścieżki (jeśli zostaje ≥ 3 ruchu).
- Gdy przed minimalnym dystansem trasę blokuje oddział lub teren, szarża jest nieudana.
- Komunikat blokady przez jednostkę:  
  `Na drodze szarży stoi inny oddział uniemożliwiający szarżę.`

### Szarża z celem i "ślepa" szarża

- Jeśli na trasie (po osiągnięciu minimum startowego) stoi wróg, oddział zatrzymuje się przed nim i łapie cel szarży.
- Jeśli po przebiegnięciu minimum wróg stoi **przed frontem** albo **na skos do przodu** (łuk: prosto + oba skosy), też łapie cel szarży.
- Możliwa jest także **ślepa szarża**: bez celu w momencie planowania, by utrzymać prosty sprint.
- Start ślepej szarży nadal wymaga pełnego minimum kierunku (**3** prosto / **2** skos).
- Jeśli ślepa szarża skoliduje z wrogiem po przebiegnięciu **co najmniej 2** kafelków, cel szarży jest przypinany automatycznie.
- Po dojściu na koniec ścieżki ślepa szarża może też złapać wroga stojącego **na skos** w łuku do przodu.
- Jeśli inny oddział przetnie drogę **zanim** szarża się rozpędzi (poniżej progu sukcesu), szarża jest **przerwana** — w journalu pojawia się komunikat o przerwaniu rozpędu.
- Szarża nie daje fory na starcie, tylko **pierwszeństwo w sporze o pole**: gdy szarżujący i inny oddział sięgają po to samo pole w tej samej chwili, pole bierze szarża. Spór dwóch szarż rozstrzyga inicjatywa.

### Bonus szarży

Przy poprawnym ataku w Combat na przypięty cel szarży:

- **Attack +2**
- **Damage +1**

Bonus działa tylko dla właściwego celu szarży stojącego w **łuku do przodu** (prosto lub skos frontowy) i jest czyszczony, jeśli warunki kontaktu nie zostaną utrzymane (np. wróg odsunie się na bok / tylny róg).

## Attack Planning

- Atak można zaplanować tylko przy kontakcie (adjacency) z przeciwnikiem.
- Dla oddziału po udanej szarży cel ataku jest **zablokowany** po resolve ruchu — nie da się go odwołać ani zmienić w Attack Planning.
- Po kolizjach i przeliczeniu ruchu cele mogą zostać skorygowane przez system.

## Combat

- Wymiany obrażeń liczone są na podstawie statystyk tokena i rzutów (szczegóły wzorów w [`FORMULAS.md`](./FORMULAS.md)).
- Ataki szarży są rozpatrywane **przed** pozostałymi atakami (z pominięciem globalnej inicjatywy).
- Wciąż obowiązuje kontratak defensywny w ramach wymiany.
- Jednostki z HP <= 0 uciekają/schodzą zgodnie z logiką bitwy.

## Szybki skrót dla gracza

- Chcesz bonus szarży? Utrzymaj linię, dystans minimalny i kontakt z celem.
- By **zacząć** szarżę prosto: potrzeba 3 pól; skos: 2.
- Ślepa szarża może „załapać” cel na kolizji już po **2** przebiegniętych polach.
- Jeśli ktoś przetnie drogę za wcześnie — szarża jest przerwana (wpis w logu).
- Bonus dotyczy tylko ataku na przypięty cel szarży.
