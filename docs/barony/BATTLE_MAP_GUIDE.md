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

- Ruch zużywa `Move` i uwzględnia teren.
- Pole **Difficult** kosztuje podwójnie.
- Pole **Impassable** blokuje przejście.
- Można planować trasę waypointami i obrót oddziału.
- Kolizje wrogich oddziałów podczas ruchu zatrzymują trasy i mogą wymusić kontakt bojowy.

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
- Przy resolve ruchu **szarże rozstrzygane są od najwyższej inicjatywy do najniższej**: szarżujący z wyższą init startuje wcześniej, więc częściej dobija kontakt i może uciąć ruch / szarżę przeciwnika.

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
