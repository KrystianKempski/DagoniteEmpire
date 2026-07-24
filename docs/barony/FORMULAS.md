# Barony — PPB formula catalog

> **Runtime source of truth:** C# classes in `DA_Common/Barony/*PpbFormulas.cs` and `DagoniteEmpire/Pages/Barony/BaronyCalc.cs`.  
> **Structured dump:** [`formulas.json`](./formulas.json).  
> **Domain Panel tooltips:** cell text = `ExplainAdditive` / `ExplainPercent` / `ExplainAdvisor*` in those same classes (format `= …`).

Conventions:
- `Population` = settlement population (village / town).
- `Fertility` = `TerrainTile.Fertility` (−1 = unknown, 0–5).
- `HasPalisade` = optional palisade on villages only.
- Baron skills = specialization / base `SumBonus`; attribute = `ModifierAbsolute`.
- Baron skill results are floored to integers (`Math.Floor` at the end).

---

## Domain Panel summary (Barony Summary)

Logic: `BaronyCalc.SummarizeSections` / `PpbMath.Summarize`.  
Sources: all Domain Panel sections (Baron and Advisors, City and Buildings, Social Groups,
Terrain Improvements, Decrees, Events, Community) — both **+** and **%** columns.

```
scalable_additive = Σ positive Additive per row  (Corruption: Σ negative)
percent_effect    = scalable_additive × (Σ Percent / 100)
final             = Σ Additive + percent_effect
```

`Percent` is in percentage points (10 = +10%). There is no separate “base” — only what appears in the sections counts.

---

## Social Groups — Tax

- Default rates: `SocialGroup.DefaultTax` — Nobility 5%, Burghers 15%, Peasants 30%.
- **Tax** column in Social Groups; edited in the relation dialog.
- Town / Village **Treasury** uses `TownTaxRates.FromRelations`.

---

## Terrain — Village

Class: `VillagePpbFormulas`. The catalog has no fixed PPB bonuses.

### Food — farm baseline (fertility)

| Fertility | FarmFood |
|----------:|---------:|
| 2 | 0.8 |
| 3 | 1.5 |
| 4 | 2 |
| 5 | 3 |
| other | 0 |

### PPB (additive)

| PPB | Formula |
|-----|---------|
| Food | `FarmFood(Fertility) − Population` |
| Economy | `Population / 2` |
| Production | `Population` |
| Loyalty | `−Population` |
| Stability | `−2 × Population` (+3 if palisade) |
| Law | `−Population / 2` (+1 if palisade) |
| Corruption | `Population / 4` |
| Science / Culture | `Population / 4` |
| Magic | `Population / 8` |
| Defense | `Population` (+5 if palisade) |
| Treasury | `(Nob%/100)×Population×5 + (Burg%/100)×Population×5 + (Peas%/100)×Population×15` |

Palisade: Defense +5, Stability +3, Law +1.

---

## Terrain — Town

Class: `TownPpbFormulas`. Row “Population of \<city\>” under City and Buildings.

| PPB | Formula |
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
| Treasury | `(Nob%/100)×Population×20 + (Burg%/100)×Population×25 + (Peas%/100)×Population×10` |

---

## Community Penalties

PPB balances **before** Community rows (Hunger / Crime / Corruption / Unrest / Economy).

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

### Economy (conjuncture) — `EconomyConjunctureFormulas`
- Inputs: `Economy` = Economy additive sum **before** Community rows; `Population` = settlement population; `Conjuncture` = 2d6 + MG modifier
- **Net Gold profit** = `(Economy + Conjuncture) × 2`
- `% Gold/Production/Loyalty/Stability/Magic/Culture/Science/Defense` = `clamp(50 × (Economy / (2 × Population) − 1) + (Conjuncture − 7), −40, +40)`

### Liege tribute (Budget Fief) — `FiefTributeFormulas`
- Gross gold income = Domain Panel positive Gold (+ % remainder) **before** expenses
- `Tribute = GrossIncome × (LiegeTributePercent / 100)` (default **15%**, MG-editable on Budget)
- Treasury turn balance = Domain Panel Final Gold − Tribute

### Vassal fief dues (village gold) — `FiefTributeFormulas`
- Villages on vassal fiefs (not baron demesne / domain-default) keep only a share of **Treasury** for the baron
- `Kept = FullVillageGold × (VassalTributePercent / 100)` (default **15%**, MG-editable on Budget)
- Applied in Domain Panel / Budget via `BaronyCalc.ImprovementRows` (full yield stays stored on the improvement)

### End of turn — `BaronyRepository.ResolveTurn`
Pipeline (player End Turn flag → MG Resolve Turn):
1. Apply `ExpectedResourceIncome` to stocks; store as `PreviousTurnIncome`
2. Funded projects: `TurnsRemaining = max(0, TurnsRemaining − 1)`; if `TurnsRemaining == 0` → Complete + apply `OutputKind` results (Building / Improvement / Decree / Event / resources / **Unit Training** → set linked unit Active)
3. Sync `Size` = primary-domain tile count
4. If Final Stability ≤ 0 → loyalty test (below)
5. Advance calendar one season (`BaronyCalendarFormulas`); re-roll Conjuncture 2d6
6. Reset Baron's Time: remove non-system actions; restore management to `RequiredManagementJc` (100 BT). Percent time modifiers are kept.
7. Letter communication quotas refresh with the new turn number (inbound caps per correspondent/region; awaiting-reply lock is only for the current turn).
8. Clear `PlayerTurnReady`

### Letters — inbound caps / turn — `BaronLetterRules`
- Eastern March: max **3** inbound letters from the same correspondent per turn
- Empire / Other: max **1** inbound letter from the same correspondent per turn
- Outbound from the baron is unlimited (except one unanswered outbound per thread **this turn**)
- Caps and “awaiting reply” unlock automatically when turn advances on Resolve Turn

### Control DC & loyalty test — `ControlDcFormulas`
- `ControlDc = Size + 2 × Population + 5` (Population = settlement population)
- When Stability ≤ 0: `result = Loyalty + d20 − ControlDc`
  - `result ≥ 0` → Unrest unchanged
  - `result < 0` → Unrest +1
  - `result ≤ −(2 × first digit of ControlDc)` → Unrest +2

---

## Baron Card — From Skills

Class: `BaronSkillPpbFormulas`. Influence from **all** character skills (Baron Card).

| PPB | Formula (`Compute`, floored) |
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

PL → catalog mapping: Dowodzenie → **Inspire**, Dworskie maniery → **Diplomacy**, Magia → custom **Magic** (Knowledge).

---

## Baron and Advisors — baron row (Domain Panel)

Logic: `BaronyCalc.BuildBaronAdvisorRow`.

**X** = sum of skill-PPB units from the Baron Card: **From Skills** (± management BT) + Prestige/Honor/Fear + custom `BaronInfluenceModifierDTO`.  
Same total as Σ on the Baron Card (`BuildInfluenceRows` / `SumInfluenceRows`).

Then for each PPB: Additive/Percent from the mapping formulas below — custom sources are **not** added as raw Additive outside that map.

### Additive (+)

| PPB | Source | Tooltip |
|-----|--------|---------|
| Loyalty, Stability, Law, Science, Magic, Culture, Intelligence | X for that PPB | `= {ppb} skill` |
| Corruption | X Corruption (negative) | `= corruption skill` |
| Food, Economy, Production, Defense | none (percent only) | *(none)* |
| Treasury (Gold) | none | *(none)* |

### Percent (%)

For each PPB: `X` = that same PPB value from the Baron Card sum (skills + sources).

| PPB | Formula | Tooltip |
|-----|---------|---------|
| Food, Economy, Production, Defense | `(1 + X/60)` | `= {ppb} skill/60` (rounded to 0.1 pp) |
| all others except Gold | `(1 + X/100)` | `= {ppb} skill/100` |
| Corruption | `(1 + X/100)` (X negative) | `= corruption skill/100` |
| Treasury (Gold) | none | *(none)* |

The % column in the UI shows the **effective** percent (e.g. skill 30 → +50% with divisor 60; `/60` rounded to 0.1).

Tooltip on the **baron name**: `BaronSkillPpbFormulas.BaronAdvisorNameTooltip`.  
Detailed skill formulas — only on **Baron Card → From Skills** (`ExplainAdditive`).

---

## Baron and Advisors — advisor rows (Domain Panel)

Logic: `BaronyCalc.ApplyAdvisorSkillInfluence`.

**Active skill** = entry in the office `SignificantSkills` (defaults from `AdvisorSignificantSkills.DefaultForOffice`).

**X** = sum of skill-PPB units from Offices: `Advisor.Skills` + custom `AdvisorInfluenceModifierDTO`, then masked to active skills (`SumAdvisorInfluenceRows`).  
Then Additive/Percent like the baron from X — customs are **not** injected as raw Additive outside the map.

| | Baron | Advisor |
|---|--------------|---------|
| X source | Σ Baron Card (skills ± management + PHP + custom) | Σ Offices (skills + custom), masked |
| PPB scope | all except Gold | SignificantSkills only |
| Additive | MapToAdvisorAdditive(X) | same |
| Percent | MapToAdvisorPercent(X) | same |

Cell tooltips: `ExplainOfficeAdvisorAdditive` / `ExplainOfficeAdvisorPercent` (format `= {ppb} skill`, `/60` for Food/Economy/Production/Defense).

---

## Army units — `UnitCombatFormulas` / `UnitTrainingCostFormulas`

One unit = **50** troops (default). Training is a project (`ProjectOutputKind.UnitTraining`).

### Creation costs
- Recruit cost from selection catalog: usually Defense; **Mercenaries** = **80** gold (0 Defense); **Forced hire** = 0/0 and creates event **Forced hire** (Loyalty −7, Stability −7 for 3 turns) when training starts
- Per-item acquire mode:
  - **Craft** = Production + catalog Gold
  - **Buy** = Market gold (`Mkt`) only (no Production)
  - **Defense** = `2 × Mkt` as Defense (clears that item’s Prod/Gold)
- Recruit Defense cost from selection catalog
- Accelerate: −1 turn / **50** Defense (max = training turns)
- Training gold = one-time fee by type: Express **10**, Accelerated **20**, Standard **40**, Elite **75** (not per turn)
- Project gold track = gear gold + training gold; Defense track = recruit + gear-as-defense + accelerate
- Gear can be paid entirely as Defense; training gold remains a gold fee unless covered separately
- XP / starting Discipline / max base skill from training ∩ recruit — **max base skill** = `min(recruit.Skl, training.Skl)` (lower wins). Cap blocks raising **Base** above that while Training / in the generator; lifted after Active graduation.
- Wage (stored, Budget later) = recruit wage + training wage
- **Human race**: Move +3; player picks **two** base skills for **+1 Other** each (`SkillOtherSources` label `Race`)

### Combat totals
- **Attack** = Skl (weapon-type skill) + Gear (weapon Atk ± quality) + Cmd + Oth  
  Quality: Good +1 / Poor −1 to Atk and Dmg
- **Defense** = Skl (highest eligible among Dodges always; Shields if shield equipped; Armor if armor equipped) + Gear (weapon/armor/shield Def) + Cmd + Oth
- **Damage** = Gear (weapon Dmg ± quality) + Oth
- **Move** = Race (`UnitRaceCatalog`, Humans **+3**) + `floor((Agility + Run)/2)` + Gear (Mov penalties) + Oth
  Only Human race is playable for now; `RaceKey` on the unit defaults to `human`.
- **Armor** = Gear (armor/shield Arm) + Oth
- Combat **Oth** / skill **Other** = sum of named sources (MG dialog). Persist in `CombatOtherJson` / `SkillOtherSourcesJson`; totals synced to `OtherAttack`… / `SkillOther`.

### Skill totals (Excel Generator / Oddziały)
- **Base skills** (Melee, Ranged, Athletics, Agility, Urban, Scout): `Razem = Bazowo + Inne` — **no** attribute.
- **Starting Bazowo** (same for every unit, Excel Generator/Oddziały): Melee 3, Ranged 3, Athletics 2, Agility 2, Urban 1, Scout 2. Riding starts at 0. Raised later with XP / MG edit.
- **Specializations**: `Razem = parent Razem + linked attr + Bazowo + Inne` (specialization Bazowo starts at 0).
- **Riding**: treated as a Melee specialization (`parent = Melee Razem + Agility + base + other`), shown on its own row.
- Attr letters: B Build / A Agility / W Will / P Perception (Excel S = Sprawność).
- Linked attribute only appears on specializations (and Riding); changing recruit attributes updates those totals live.

Skill total (legacy one-liner, specializations only) = parent + linked attribute + base level + other.

### XP spend (Active units)
- Attribute → level×10
- Base skill → level×3
- Special skill → level×1 and ≤ parent base
- Discipline (1–18) → cost = current level
- MG may set Base / Other freely; Baron raises Base with XP when Active.
