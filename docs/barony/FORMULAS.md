# Barony — katalog formuł PPB

> **Źródło prawdy w runtime:** klasy C# w `DA_Common/Barony/*PpbFormulas.cs` oraz `DagoniteEmpire/Pages/Barony/BaronyCalc.cs`.  
> **Strukturalny zapis:** [`formulas.json`](./formulas.json).  
> **Tooltipy Domain Panel:** treść komórek = `ExplainAdditive` / `ExplainPercent` / `ExplainAdvisor*` w tych samych klasach (format `= …`).

Konwencje:
- `Pop` / `Population` = ludność osady (wioska/miasto).
- `Fertility` = `TerrainTile.Fertility` (−1 = nieznana, 0–5).
- `HasPalisade` = opcjonalna palisada **tylko** wioski.
- Umiejętności barona = `SumBonus` ze specjalizacji / bazowych; atrybut = `ModifierAbsolute`.
- Wyniki barona zaokrąglane w dół do liczb całkowitych (`Math.Floor` na końcu).

---

## Podsumowanie panelu (globalne)

```
final[ppb] = (base[ppb] + Σ Additive[ppb]) × (1 + Σ Percent[ppb] / 100)
```

`base` na razie zwykle 0. `Percent` w punktach procentowych (10 = +10%).

---

## Social Groups — Tax

- Domyślne stawki: `SocialGroup.DefaultTax` — Nobility 5%, Burghers 15%, Peasants 30%.
- Kolumna **Tax** w sekcji Social Groups; edycja w dialogu relacji.
- Town / Village **Treasury** używa `TownTaxRates.FromRelations`.

---

## Terrain — Village

Klasa: `VillagePpbFormulas`. Katalog nie ma stałych bonusów PPB.

### Food — baza jak farma (żyzność)

| Fertility | FarmFood |
|----------:|---------:|
| 2 | 0.8 |
| 3 | 1.5 |
| 4 | 2 |
| 5 | 3 |
| inne | 0 |

### PPB (additive)

| PPB | Formuła |
|-----|---------|
| Food | `FarmFood(Fertility) − Population` |
| Economy | `Population / 2` |
| Production | `Population` |
| Loyalty | `−Population` |
| Stability | `−2 × Population` (+3 jeśli palisada) |
| Law | `−Population / 2` (+1 jeśli palisada) |
| Corruption | `Population / 4` |
| Science / Culture | `Population / 4` |
| Magic | `Population / 8` |
| Defense | `Population` (+5 jeśli palisada) |
| Treasury | `(Nob%/100)×Pop×5 + (Burg%/100)×Pop×5 + (Peas%/100)×Pop×15` |

Palisada: Defense +5, Stability +3, Law +1.

---

## Terrain — Town

Klasa: `TownPpbFormulas`. Wiersz „Population of \<city\>” w City and Buildings.

| PPB | Formuła |
|-----|---------|
| Food | `−Population` |
| Economy | `Population` |
| Production | `2 × Population` |
| Loyalty | `−2 × Population` |
| Stability | `−3 × Population` |
| Law | `−Population` |
| Corruption | `Population` |
| Science / Culture | `Population / 2` |
| Magic | `Population / 4` |
| Defense | `2 × Population` |
| Treasury | `(Nob%/100)×Pop×20 + (Burg%/100)×Pop×25 + (Peas%/100)×Pop×10` |

---

## Community Penalties

Bilanse PPB **przed** wierszami Community (Hunger/Crime/Corruption/Unrest).

### Hunger — `HungerPpbFormulas`
- `Hunger = max(0, −FoodBalance)`
- `% Economy/Production = max(−Hunger×10, −50)`
- `+ Loyalty/Stability = −Hunger×3`, `Law = −Hunger×2`, `Corruption = Hunger`

### Crime — `CrimePpbFormulas`
- `Crime = max(0, −LawBalance)`
- `% Economy/Production = max(−Crime×5, −50)`
- `+ Loyalty = −Crime`, `Stability = −Crime×2`, `Corruption = Crime/2`

### Corruption — `CorruptionPpbFormulas`
- `Corruption = max(0, CorruptionBalance)`
- `% Economy/Production = max(−Corruption×5, −50)`
- `+ Loyalty = −Corruption×2`, `Stability = −Corruption`

### Unrest — `UnrestPpbFormulas`
- `Unrest = Barony.Unrest`
- `% Economy/Production = −Unrest×15`
- `+ Loyalty/Stability/Law = −Unrest×3`

---

## Baron Card — From Skills

Klasa: `BaronSkillPpbFormulas`. Wpływ ze **wszystkich** umiejętności postaci (Baron Card).

| PPB | Formuła (`Compute`, floored) |
|-----|------------------------------|
| Food | `(Plants and mushrooms + Animals care + Beasts) / 3` |
| Economy | `(Mathematics and logic + Races and nations + Trade) / 3` |
| Production | `Craft + Intelligence mod` |
| Loyalty | `(Bluff + Public speech + Sense motives) / 3` |
| Stability | `(Intimidate + History and religion + Persuasion) / 3` |
| Law | `(Investigation + Observation + Tracking) / 3` |
| Corruption | `−(Vigilance + Gambling + Acting) / 3` |
| Science | `Knowledge + Intelligence mod` |
| Magic | `Magic + Willpower mod` |
| Culture | `(Fine arts + Linguistics + Diplomacy) / 3` |
| Intelligence | `Perception/2 + Survival/2 + Deceit` |
| Defense | `(Strategy and tactics + Inspire + Geography) / 3` |

Mapowanie PL → katalog: Dowodzenie → **Inspire**, Dworskie maniery → **Diplomacy**, Magia → custom **Magic** (Knowledge).

---

## Baron and Advisors — wiersz barona (Domain Panel)

Logika: `BaronyCalc.BuildBaronAdvisorRow`. Każdy PPB baronii bierze wartość ze **swojej** kolumny w wierszu **From Skills** na Baron Card (ta sama mapowanie umiejętności → PPB co wyżej).

### Additive (+)

| PPB | Źródło | Tooltip |
|-----|--------|---------|
| Loyalty, Stability, Law, Science, Magic, Culture, Intelligence | wartość From Skills dla tego PPB | `= {ppb} skill` |
| Corruption | From Skills Corruption (ujemna) | `= corruption skill` |
| Food, Economy, Production, Defense | brak (tylko %) | *(brak)* |
| Treasury (Gold) | brak | *(brak)* |

### Percent (%)

Dla każdego PPB: `X` = wartość **tego samego** PPB z From Skills.

| PPB | Formuła | Tooltip |
|-----|---------|---------|
| Food, Economy, Production, Defense | `(1 + X/60)` | `= {ppb} skill/60` (zaokr. do 0,1 pp) |
| pozostałe oprócz Gold | `(1 + X/100)` | `= {ppb} skill/100` |
| Corruption | `(1 + X/100)` (X ujemne) | `= corruption skill/100` |
| Treasury (Gold) | brak | *(brak)* |

W kolumnie % w UI widać **efektywny** procent (np. skill 30 → +50% przy dzielniku 60; wynik `/60` zaokrąglany do 0,1).

Tooltip na **imieniu barona**: `BaronSkillPpbFormulas.BaronAdvisorNameTooltip`.  
Szczegółowe formuły umiejętności — tylko na **Baron Card → From Skills** (`ExplainAdditive`).

---

## Baron and Advisors — wiersze doradców (Domain Panel)

Logika: `BaronyCalc.ApplyAdvisorSkillInfluence`. Umiejętności urzędnika zapisane w `Advisor.Skills` (Offices → Skills).

**Active skill** = wpis w `SignificantSkills` urzędu (domyślnie per `AdvisorSignificantSkills.DefaultForOffice`).

Mapowanie na PPB jak u barona, ale **tylko** dla active skills. **Korupcja** w `Advisor.Skills` jest ze znakiem (ujemna = redukcja), bez dodatkowej inwersji przy mapowaniu.

| | Baron's rule | Advisor |
|---|--------------|---------|
| Źródło wartości | Baron Card From Skills | `Advisor.Skills` (masked) |
| Zakres PPB | wszystkie oprócz Gold | tylko SignificantSkills |
| Additive | jak baron (bez Food/Econ/Prod/Def) | jak baron |
| Percent | jak baron (/60 lub /100) | jak baron |
| Custom bonus | `BaronInfluenceModifierDTO` | `AdvisorInfluenceModifierDTO` (+ only) |

Tooltipy komórek: `ExplainOfficeAdvisorAdditive` / `ExplainOfficeAdvisorPercent` (format `= {ppb} skill`, `/60` dla Food/Economy/Production/Defense).
