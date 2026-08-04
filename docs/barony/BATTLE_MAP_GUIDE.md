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
- Wejście na pole zajęte przez **własny oddział** liczy się jak trudny teren (×2) — patrz
  „Przejście przez sojusznika” niżej.
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
- **Sojusznik na drodze najpierw tylko opóźnia**: oddział czeka, aż pole się zwolni. Kolumna
  marszowa przepuszcza się więc sama, bez żadnej dodatkowej opłaty.
- Gdy dwa oddziały sięgają po to samo pole w tej samej chwili, pierwszeństwo ma **szarża**,
  a w jej braku **wyższa inicjatywa**.
- Krok na skos jest niemożliwy, gdy zajęte są **oba** sąsiednie pola narożne — nie da się
  przecisnąć między dwoma oddziałami ani przeniknąć przez siebie po krzyżujących się skosach.
- Dwa oddziały nigdy nie kończą ruchu na tym samym polu.

### Przejście przez sojusznika

- Jeśli zator nie ustąpi sam (mniej więcej czas trzech kroków), oddział **przepycha się przez
  sojusznika** stojącego mu na drodze.
- Warunek: sojusznik, przez którego się przechodzi, **nie może być związany walką**. Oddział
  wbity w melee trzyma swoje pole także przed swoimi — przed nim marsz się zatrzymuje
  (wpis w journalu).
- Każde pole zajęte przez sojusznika kosztuje jak **trudny teren** (×2). Kilka ciał jeden za
  drugim pokonuje się jednym ruchem, płacąc podwójnie za każde z nich.
- **Nie da się stanąć na sojuszniku**: przepchnięcie zaczyna się tylko wtedy, gdy po drugiej
  stronie jest wolne pole i starczy punktów ruchu, by na nie wyjść. W przeciwnym razie oddział
  zostaje tam, gdzie stał.
- Planując trasę, zasięg ruchu uwzględnia już podwójny koszt pól z sojusznikami, a waypointa
  nadal nie można postawić na cudzym polu ani na cudzym zaplanowanym końcu trasy.

## Szarża (Charge)

Szarża jest specjalnym wariantem ruchu wykonywanym w fazie **Movement**.

- Szarża idzie po **prostej linii** (8 kierunków: orto + skosy).
- **Minimalny dystans do zadeklarowania** szarży:
  - **3 kafelki** dla kierunków prostych (N/E/S/W) = **3** ruchu,
  - **2 kafelki** dla skosów = **3** ruchu.
- Koszt skosów jak w zwykłym ruchu (1 / 2 / 1 / 2… w punktach, łącznie `floor(3n/2)`):  
  **3** move → 2 skosy, **4** → 3, **5** → 3, **6** → 4, …
- Szarża **zawsze biegnie do końca możliwości**: aż wyczerpie punkty ruchu albo natrafi na kogoś
  na drodze. Nie da się jej zaplanować „na skróty” ani zatrzymać w połowie.
- Maksymalny dystans wprost/bokami = pozostały `Move` (1 kafelek = 1 ruch); na skos wg tabeli powyżej. Difficult ×2.
- Szarżę można rozpocząć także z aktualnego końca zaplanowanej ścieżki (jeśli zostaje ≥ 3 ruchu).
- **Po zadeklarowaniu szarży oddział nie przyjmuje już żadnych rozkazów** — ani dalszego ruchu,
  ani obrotu, ani drugiej szarży. Żeby coś zmienić, trzeba cofnąć plan (Undo albo kliknięcie
  w pole oddziału).
- Gdy przed minimalnym dystansem trasę blokuje oddział lub teren, szarża jest nieudana.
- Komunikat blokady przez jednostkę:  
  `Na drodze szarży stoi inny oddział uniemożliwiający szarżę.`

### Szarża z celem i "ślepa" szarża

- Jeśli na trasie (po osiągnięciu minimum startowego) stoi wróg, oddział zatrzymuje się przed nim i łapie cel szarży.
- Jeśli po przebiegnięciu minimum wróg stoi **przed frontem** albo **na skos do przodu** (łuk: prosto + oba skosy), też łapie cel szarży.
- Możliwa jest także **ślepa szarża**: bez celu w momencie planowania, by utrzymać prosty sprint.
- Start ślepej szarży nadal wymaga pełnego minimum kierunku (**3** prosto / **2** skos).
- **Szarża liczy się jako udana, gdy oddział przebiegł co najmniej 2 kafelki** — niezależnie od
  kierunku i od tego, czy była wycelowana, czy ślepa. Po takim rozbiegu oddział łapie na kolizji
  cel szarży i może dostać bonus.
- Po dojściu na koniec ścieżki ślepa szarża może też złapać wroga stojącego **na skos** w łuku do przodu.
- Jeśli inny oddział przetnie drogę, **zanim szarża przebiegnie 2 kafelki**, szarża jest **przerwana** — w journalu pojawia się komunikat o przerwaniu rozpędu.
- Jeśli natomiast rozpęd był już wystarczający, a w szarżę wejdzie **inny wrogi oddział** niż wybrany cel, szarża **przenosi się na przechwytującego** (o ile stoi w łuku do przodu — prosto lub na skos) i to on obrywa razem z bonusem. Cel szarży i cel ataku są przepinane automatycznie, z wpisem w journalu.
- Szarża nie daje fory na starcie, tylko **pierwszeństwo w sporze o pole**: gdy szarżujący i inny oddział sięgają po to samo pole w tej samej chwili, pole bierze szarża. Spór dwóch szarż rozstrzyga inicjatywa.

### Bonus szarży

Przy poprawnym ataku w Combat na przypięty cel szarży:

- **Attack +2**
- **Damage +1**

Bonus działa tylko dla właściwego celu szarży stojącego w **łuku do przodu** (prosto lub skos frontowy) i jest czyszczony, jeśli warunki kontaktu nie zostaną utrzymane (np. wróg odsunie się na bok / tylny róg).

## Związanie walką (Engagement)

Ikona skrzyżowanych mieczy na żetonie oznacza, że oddział jest **związany walką** z jednym lub więcej wrogami. Licznik na ikonie pojawia się przy więcej niż jednym przeciwniku; tooltip wymienia ich nazwy.

### Kiedy powstaje

- **Kolizja w ruchu** — dwa wrogie oddziały zderzą się podczas resolve fazy Movement.
- **Wymiana ciosów wręcz w Combat** — atakujący i atakowany wiążą się wzajemnie (o ile obaj przeżyją). **Strzał nie tworzy związania.**

### Kiedy trwa / gaśnie

- Związanie trwa, dopóki footprinty się **stykają** (8 kierunków, w tym narożniki).
- Gaśnie, gdy po ruchu oddziały przestaną się stykać albo gdy partner **zginie / ucieknie**.
- Na starcie i na końcu fazy ruchu pary, które się już nie stykają, są cicho zrywane (bez dodatkowej kary).

### Kara za ruch w związaniu

- **Jakikolwiek ruch** związanego oddziału kosztuje darmowy cios — nie tylko wyjście ze strefy. Krok w bok obok tego samego wroga jest karany tak samo jak ucieczka.
- Ruch jest dozwolony, ale przy planowaniu pierwszego pola pojawia się ostrzeżenie z liczbą ciosów, które oddział przyjmie.
- Podczas resolve ruchu, w chwili gdy **pierwszy krok** się kończy, każdy związany wróg zadaje **darmowy atak za połowę** normalnych obrażeń (min. 1). Każda para płaci raz na fazę, niezależnie od długości marszu.
- Front i pozycje do tego ataku biorą się ze stanu **sprzed rozpoczęcia ruchu**.
- Ruszający się **nie** oddaje ataku obronnego.
- Jeśli atak zniszczy ruszającego się, zatrzymuje się on na polu, na którym padł, i zostaje przeszkodą do końca fazy ruchu.
- Stanie w miejscu nie kosztuje nic — obrót w miejscu też nie jest ruchem w tym sensie.

## Jednostki zasięgowe (łucznicy)

Zielona odznaka z łukiem i liczbą na żetonie oznacza jednostkę zasięgową (`Range > 0`).

### Skąd bierze się zasięg

- **Sojusznicy** — z wyposażonej broni głównej (`UnitWeaponDef.Range`: proce 2, proste łuki 3, wojenne 4, długie 5; broń biała 0).
- **Wrogowie** — ręczne pole **Range** w formularzu MG (domyślnie 0 = wręcz).

### Zasady strzału

- Zasięg liczony **tą samą metryką co ruch** (prosto 2, skos 3 pół-punktu, budżet `Range × 2 + 1`), ale strzał leci ponad wszystkim — teren i oddziały **nie blokują**.
- Strzał **nie wywołuje obrażeń obronnych** i **nie wiąże walką**.
- Strzelec zawsze celuje dobrze (Aim = front); ekspozycja celu nadal działa (strzał w plecy boli bardziej).
- **Związany walką łucznik nie strzela** — może tylko bić się wręcz z sąsiadem (z pełną wymianą ciosów). Sam kontakt bez związania nie przeszkadza.
- Strzał do sąsiada (gdy nie ma związania) to nadal strzał — bez obrażeń obronnych.

### Kara za dystans

- Cel **tuż obok** strzelca (kontakt, w tym po skosie) dostaje **pełne** obrażenia — bez kary.
- Każdy dalszy kafelek lotu to **−2 do ataku**: 1 kafelek odstępu −2, 2 kafelki −4, 3 kafelki −6
  i tak dalej.
- Dystans liczony jest tą samą metryką co zasięg, więc skosy kosztują jak w ruchu (dwa skosy =
  3 punkty lotu = −4).
- Kara obniża tylko **atak**, więc nie zeruje trafienia — daleki strzał robi się jednak
  wyraźnie słabszy. Journal pokazuje ją jako `[range −N att]`.

## Attack Planning

- Atak wręcz wymaga kontaktu (adjacency); jednostki zasięgowe mogą celować w każdego wroga w zasięgu (podświetlone pola).
- Dla oddziału po udanej szarży cel ataku jest **zablokowany** po resolve ruchu — nie da się go odwołać ani zmienić w Attack Planning.
- Po kolizjach i przeliczeniu ruchu cele mogą zostać skorygowane przez system.

## Combat

- Wymiany obrażeń liczone są na podstawie statystyk tokena i rzutów (szczegóły wzorów w [`FORMULAS.md`](./FORMULAS.md)).
- Ataki szarży są rozpatrywane **przed** pozostałymi atakami (z pominięciem globalnej inicjatywy).
- Wymiana wręcz nadal ma kontratak defensywny; **strzał go nie ma**.
- Jednostki z HP <= 0 uciekają/schodzą zgodnie z logiką bitwy.

## Szybki skrót dla gracza

- Chcesz bonus szarży? Utrzymaj linię, rozbieg i kontakt z celem.
- By **zacząć** szarżę prosto: potrzeba 3 pól; skos: 2.
- Żeby szarża **się liczyła**, wystarczą **2** faktycznie przebiegnięte pola.
- Szarża to decyzja ostateczna — po jej zadeklarowaniu oddział nie przyjmuje już rozkazów.
- Jeśli ktoś przetnie drogę za wcześnie — szarża jest przerwana (wpis w logu). Po rozpędzie szarża po prostu przeskakuje na tego, kto ją zatrzymał.
- Bonus dotyczy tylko ataku na przypięty cel szarży.
- Skrzyżowane miecze = związanie walką. Każde ruszenie się w związaniu = darmowy atak za pół obrażeń od każdego związanego wroga (bez kontrataku).
- Zielony łuk = jednostka zasięgowa. Strzał na dystans bez kontrataku; związanie walką blokuje strzał (zostaje tylko wręcz).
