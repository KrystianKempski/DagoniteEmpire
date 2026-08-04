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
Terrain Improvements, Decrees, Events, Army, Community) — both **+** and **%** columns.

```
scalable_additive = Σ positive Additive per row
percent_effect    = scalable_additive × (Σ Percent / 100)
final             = Σ Additive + percent_effect
```

`Percent` is in percentage points (10 = +10%). There is no separate “base” — only what appears in the sections counts.

---

## Trade goods — herd / stud founding

Logic: `HerdStockRequirements` + `TradeGoodAvailability`.

To **found** these improvements you must already have the matching good available
(treaty, import, or MG override). After the improvement is built it becomes a
**local production** source of that good:

| Improvement | Requires trade access | Then produces |
|---|---|---|
| Sheep pastures | Sheep | Sheep & Wool |
| Pastures (cattle) | Cattle | Cattle |
| Horse Stud (regular) | Horses | Horses |
| Horse Stud (military) | War horses | War horses |
| Horse Stud (noble) | Noble horses | Noble horses |

Shown in the Buildings catalog description as **Requires trade access…** / **Produces…**.
Player construction on Terrain filters out locked herd templates until stock is available.

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

PPB balances **before** Community rows use Domain **Final** Food / Law / Corruption from sections above Community (Hunger / Crime / Corruption inputs). Economy conjuncture uses Final Economy after Hunger/Crime/Corruption/Unrest.

### Hunger — `HungerPpbFormulas`
- `Hunger = max(0, −FoodBalance)` — `FoodBalance` = Final Food before Community
- `% Economy/Production = max(−Hunger×5, −50)`
- `+ Loyalty/Stability = −Hunger×3`, `Law = −Hunger×2`, `Corruption = Hunger`

### Crime — `CrimePpbFormulas`
- `Crime = max(0, −Final Law)` — only when Law is negative; then Crime = |Law|
- Final Law includes Hunger and Unrest Law penalties (Crime itself does not change Law)
- `% Economy/Production = max(−Crime×3, −50)`
- `+ Loyalty = −Crime`, `Stability = −Crime×2`, `Corruption = Crime/2`

### Corruption — `CorruptionPpbFormulas`
- `Corruption = max(0, CorruptionBalance)` — `CorruptionBalance` = Final Corruption before Community
- `% Economy/Production = max(−Corruption×3, −50)`
- `+ Loyalty = −Corruption×2`, `Stability = −Corruption`

### Unrest — `UnrestPpbFormulas`
- `Unrest = Barony.Unrest`
- `% Economy/Production = −Unrest×10`
- `+ Loyalty/Stability/Law = −Unrest×3`

### Economy (conjuncture) — `EconomyConjunctureFormulas`
- Inputs: `Economy` = Domain **Final Economy** after Hunger/Crime/Corruption/Unrest (this row does not modify Economy, so no loop); `Population` = settlement population; `Conjuncture` = 2d6 + MG modifier
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
1. Snapshot stocks as `PreviousTurnStock` (opening stock before income/grants). Delete **all** `BaronyResourceSources` (no multi-turn ledger history). Apply `ExpectedResourceIncome` to stocks; store as `PreviousTurnIncome`.
2. Funded projects (`HasRemainingCost` false): `TurnsRemaining = max(0, TurnsRemaining − 1)`; if `TurnsRemaining == 0` → Complete and apply `OutputKind` results into the turn log:
   - **Unit Training** → linked unit `Training` → `Active`
   - **Unit Reinforce** → add `ReinforceTroops` (from Notes / description; else fill toward 50)
   - **Unit Change Equipment** → apply new loadout from Notes (`W1Key` / `W2Key` / `ArmorKey` / `ShieldKey` / `Qual`)
   - **One-time resources** → add `ResultAdditive` to stocks **and** a new Resource Balance source row for the new turn (wiped into opening stock on the next Resolve)
   - **Decree / Technology** → new active row in Domain Panel → Decrees and Technologies (`ResultAdditive` / `ResultPercent`)
   - **Event** → Domain Panel event from `ResultAdditive`/`ResultPercent`, active from the **new** turn (ongoing until MG sets an end)
   - **Building / Improvement** (incl. map tile) as before
   - Auto-generated unit/map projects start as **Resource allocation** (turns do not tick until fully funded); then → **In progress**
3. Sync `Size` = primary-domain tile count
4. If Final Stability ≤ 0 → loyalty test (below)
5. Advance calendar one season (`BaronyCalendarFormulas`); re-roll Conjuncture 2d6
6. Reset Baron's Time: remove non-system actions; restore management to `RequiredManagementJc` (100 BT). Percent time modifiers are kept.
7. Letter communication quotas refresh with the new turn number (inbound caps per correspondent/region; awaiting-reply lock is only for the current turn).
8. Depleted units regenerate troops (`UnitRules.TroopRegenPerTurn`)
9. Clear `PlayerTurnReady`

Resources tab → Resource Balance: **Σ of all rows = current stocks = HUD**.
Rows = Stock from previous turn (`PreviousTurnStock`) + Income from previous turn (`PreviousTurnIncome`) + current ledger sources (project grants, Budget transfers, MG Add Source) + **Audiences** (cumulative grants this turn, already in stocks) + Project costs (if any). Everything except Domain Panel income from the prior turn is folded into the next opening stock on Resolve.

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

### Starter units (new barony only)
Seeded once in `CreateForCharacter` via `StarterUnitsSeeder` (not Ensure):
- **City Watch** — Active, 50 troops, wage/food/defense upkeep **0**. Short spears, light leather, wooden medium shield. Attrs 4/3/3/3, Discipline 10.
- **Baron's Guard** — Active, **10**/50 troops (casualty Loss), wage/food/defense upkeep **0**. Longswords + simple bows, mail and gambeson, studded medium shield. Attrs 4/4/4/4, Discipline 10.

### Creation costs
- Recruit cost from selection catalog: usually Defense; **Mercenaries** = **80** gold (0 Defense); **Forced hire** = 0/0 and creates event **Forced hire** (Loyalty −7, Stability −7 for 3 turns) when training starts
- **Gear trade access** (`UnitEquipmentTradeAccess`): military weapons need **Military weapons**; powder weapons need **Firearms**; Simple / Medium / Heavy armor (and shields in those Excel tiers) need **Light / Medium / Heavy armor** respectively. Simple weapons need no trade good. Mounts: **Horses** / **War horses**. Generator and re-equip dialog grey locked rows; training / change-equipment APIs reject gear without access.
- **Mounts** (`UnitMountCatalog`): optional. Horses = +3 Move, +2 Atk/Def, +1 Dmg; Craft 150 Prod + 150 Gold or Buy Mkt **200** or Defense **400**; Riding **6**. War horses = +3 Move, +4 Atk/Def, +2 Dmg; Craft 250+250 / Mkt **300** / Defense **600**; Riding **8**. Bonuses stack on combat totals; Mkt counts toward upkeep.
- Per-item acquire mode:
  - **Craft** = Production + catalog Gold
  - **Buy** = Market gold (`Mkt`) only (no Production)
  - **Defense** = `2 × Mkt` as Defense (clears that item’s Prod/Gold)
- Recruit Defense cost from selection catalog
- Accelerate: −1 turn / **50** Defense (max = training turns)
- Training gold = one-time fee by type: Express **10**, Accelerated **20**, Standard **40**, Elite **75** (not per turn)
- Project gold track = gear gold + training gold + recruit gold; Defense track = recruit Defense + gear-as-defense + accelerate
- **Unit Training cost mode** = `Combined` when both gold/production and Defense are needed: both tracks must be funded together (exception to the usual Gold&Prod vs Materials either/or rule)
- Gear can be paid entirely as Defense; training gold remains a gold fee unless covered separately
- XP / starting Discipline / max base skill from training ∩ recruit — **max base skill** = `min(recruit.Skl, training.Skl)` (lower wins). Cap blocks raising **Base** above that while Training / in the generator; **lifted when the unit becomes Active** (`MaxBaseSkillAtGraduation` cleared).
- Wage (stored) = recruit wage + training wage
- **Per-turn upkeep (Active only)** — Domain Panel → **Army** (`UnitUpkeepFormulas` / `BaronyCalc.ArmyRows`):
  - **Gold** = base `Wage` (recruit + training) + `floor(Σ equipment Mkt / 100) × 2`
  - **Defense** = `floor(Σ equipment Mkt / 100) × 5` (replaces the old flat 5)
  - **Food** = `UpkeepFood` (default **0.5**)
  - Equipment Mkt = weapon1 + weapon2 + armor + shield catalog market gold; round blocks **down**.
  - Seeded free companies (wage/food/stored defense all **0**, e.g. City Watch / Baron's Guard) pay **nothing**.
  - Included in Expected Income / Budget / Resolve Turn. Food feeds Community Hunger. Training units do not count until graduation.
- **Human race**: Move +3; base skills start at **0**; player picks **two** base skills for **+1 Other** each (`SkillOtherSources` label `Race`) — that is the only racial skill bonus.

### Combat totals
- **Attack** = Skl (weapon-type skill) + Gear (weapon Atk ± quality) + Cmd + Oth + **Loss**  
  Quality: Good +1 / Poor −1 to Atk and Dmg
- **Defense** = Skl (highest eligible among Dodges always; Shields if shield equipped; Armor if armor equipped) + Gear (weapon/armor/shield Def) + Cmd + Oth + **Loss**
- **Damage** = **Attr** (linked to primary weapon skill, e.g. Heavy weapons → Build, Bows → Agility) + Gear (weapon Dmg ± quality) + Oth
- **Move** = Race (`UnitRaceCatalog`, Humans **+3**) + `floor(Run/3)` + Gear (Mov penalties) + Oth
  Only Human race is playable for now; `RaceKey` on the unit defaults to `human`.
- **Armor** = Gear (body armor Arm only — shields do **not** add Armor) + Oth
- **Max HP** = Build×2 + Will×2 + Endurance + Discipline×3 + Oth + **Loss**
- Combat **Oth** / skill **Other** = sum of named sources (MG dialog). Persist in `CombatOtherJson` / `SkillOtherSourcesJson`; totals synced to `OtherAttack`… / `SkillOther`.
- **Casualty Loss** (`UnitCasualtyFormulas`): nominal full strength = **50**. For each full **10%** of strength missing: **−1** Attack, **−1** Defense, **−4** Max HP. Example: 10/50 → 8 steps → −8 Atk/Def, −32 HP. While depleted (steps > 0): floors Atk/Def ≥ **1**, Max HP ≥ **10**. MG edits troop count by clicking Troops on the unit card.

### Battle Map — movement phase (`BattleMovementSimulator` / `BattleMovementRules`)
Movement is resolved by one deterministic simulation in `DA_Common/Barony/Battle`, shared by planning, resolution and the replay animation — the animation is a playback of the run that produced the final positions, not a second calculation. Every unit sets off at the same instant and reacts to the field as it actually is at that moment. Covered by `DA_Business.Tests/Barony/BattleMovementSimulatorTests.cs`.

- **Step cost** (half-move points): ortho **2**, diagonal **3**, ×**2** when the step pulls new difficult tiles under the footprint. Budget = `Move × 2 + 1`; displayed spend = `floor(half / 2)`.
- **Step duration** = `halfCost × 2800 / (2 × Move)` ms — ortho at Move 4 = **700 ms**, diagonal = **1050 ms**. Speed is proportional to `Move`, and since a diagonal costs half again as much time as a straight step, physical speed is identical in all eight directions.
- **Occupancy**: a unit holds the tile it is stepping into for the whole step; a hostile unit additionally screens the tile it is vacating. Nothing passes through anything.
- **Hostile contact** ends movement on the spot (`EnemyContact`) and the unit turns to face whoever stopped it.
- **Friendly block** only delays: the unit waits and retries each tick, halting only after `3 × orthoStep` ms without progress (`BlockedByAlly`).
- **Contested tile** priority: charge, then `InitiativeTotal`, then `InitiativeDie`, then token id — fully deterministic, no random tie-breaks.
- **Diagonal** steps are refused when both flanking tiles are held, which also stops two units swapping across the same diagonal.
- Output: timed legs for the animation, plus per-unit outcomes (final tile, facing, remaining move, charge tiles travelled, who stopped it) and log events.

### Battle Map — combat damage (`BattleMapPage.ResolveCombatRoundDamage`)
Phases: Movement → Attack planning → Combat. Stats are frozen on the token at deploy (`UnitCombatFormulas` / MG enemy draft). Attacks resolve in reverse initiative order (highest first). Skip if attacker/defender dead, no target, or not adjacent. No separate melee/ranged damage path — adjacency only.

User-facing flow/rules guide: [`BATTLE_MAP_GUIDE.md`](./BATTLE_MAP_GUIDE.md)

- **dealt** = `max(1, round((Damage + k4 − Armor) × (Attack+k6) / max(1, Defense+k6) × front))`
  - Rounding: away from zero (`MidpointRounding.AwayFromZero`)
  - **front** = AimBase + ExposureBonus (`GetFrontBreakdown`), range **1–4.5**
    - **Aim** — attacker facing toward defender: front **2.5**, corner **1.5**, side/rear-corner/rear **1**
    - **Exposure** — attacker position vs defender facing: front **+0**, corner **+0.5**, side **+1.5**, rear-corner **+1.5**, rear **+2**
    - Example log: `(Dmg+k4−Arm=5+3−3=5) × (7+k6=12)/(12+k6=16) ×3 (Front Vs Corner) => 11 dmg.`
  - If `Damage + k4 < Armor`, raw can be ≤ 0, but dealt is still floored at **1**
- **defensive dealt** (same exchange) = `max(1, round((Dmg_def + k4 − Arm_atk) × (Def_def + k6) / max(1, Def_atk + k6) × front_def))`
  - Ratio is **Defense vs Defense** (not Attack), both sides roll k6
  - **front_def** = same Aim/Exposure rules with roles swapped (`GetFrontBreakdown(defender, attacker)`)
  - Example log: `(Dmg+k4−Arm=4+2−1=5) × (12+k6=15)/(10+k6=14) ×3 (Front Vs Front) => 16 dmg.` tagged `(defensive)`
- Both sides may flee at HP ≤ 0 after the exchange. Defender still returns defensive damage even if reduced to 0 by the attack.
- **Charge** (Movement phase): any unit may charge in a straight line (8 directions). To **start** a charge: minimum path length **3** tiles on cardinal directions (N/E/S/W), **2** tiles on diagonals — both require **3** move points on open ground. Charge steps use the same costs as normal movement (half-move budget `Move*2+1`: ortho 2, diagonal 3 → displayed MP `floor(spent/2)`; e.g. Move 4 = 4 cardinal tiles or 3 diagonal; Move 5 = 3 diagonal; Move 6 = 4 diagonal; difficult ×2). Blocked before required start minimum → charge fails. After that, enemy on path means stop before contact and set charge target. A charge may also be planned "blind" (without immediate target). Blind charges may lock a target on collision after **2** tiles of travel (even on cardinal lines), but still need the normal start minimum to declare. If another unit cuts the path before the charge builds up speed, the charge is interrupted (journal note). Charges get **priority on contested tiles** rather than a head start: when a charger and another unit reach for the same tile in the same instant the charger takes it, and between two chargers the higher initiative wins. At the end of a full charge path, an enemy in the **forward arc** (straight ahead or either forward diagonal) can also be locked as the charge target. In Combat vs charge target: use **Attack+2** and **Damage+1**, but only while the target remains in the charger's **forward arc** (front or forward diagonal). Side / rear / rear-corner contact after movement clears the charge. Charge attacks resolve before non-charge attacks; changing target or losing valid contact clears the charge bonus.
- **End battle** (MG): ends immediately regardless of field state; writes a summary log; syncs each deployed ally’s token HP → `BaronyUnit.CurrentHp` (fled allies → **0**). Army roster is locked while `Phase = battle`.
- **Troop recovery**: understrength units (not Disbanded) regain **+5** troops per Resolve Turn until full (`UnitRules.TroopRegenPerTurn`). Shown in the turn report.
- **Reinforce project** (`ProjectOutputKind.UnitReinforce`): button on understrength Active units with no open project. People cost = Selected volunteers + Standard, scaled `× N/50` (floor). Gear = current loadout at **50%** salvage × same scale (`× N/100` of full gear). Acquire modes Craft/Buy/Defense like the generator. On complete: add N troops (cap 50) and sync Max HP.
- **Change equipment project** (`ProjectOutputKind.UnitChangeEquipment`): button on Active units with no open project. Dialog picks new loadout + Craft/Buy/Defense pay modes (unit generator Gear UX). Cost = full gear `SumGear` scaled `× troopCount/50`; turns = max(1, Standard×N/50). Starts in Resource allocation. On complete: write keys/quality, refresh agility penalty / Defense skill / Max HP.

### Skill totals (Excel Generator / Oddziały)
- **Base skills** (Melee, Ranged, Athletics, Agility, Urban, Scout): `Razem = Bazowo + Inne` — **no** attribute.
- **Starting Bazowo** (Humans): all base skills at **0**. Racial skill bonus = only the two **+1 Other** picks. Riding starts at 0. Raised later with XP / MG edit.
- **Specializations**: `Razem = parent Razem + linked attr + Bazowo + Inne` (specialization Bazowo starts at 0).
- **Riding**: total like a Melee specialization (`parent = Melee Razem + Agility + base + other`), shown on its own row. **XP raise** costs like a base skill (`level × 3`) and uses the training max-base cap — not limited by Melee base.
- Attr letters: B Build / A Agility / W Will / P Perception (Excel S = Sprawność).
- Linked attribute only appears on specializations (and Riding); changing recruit attributes updates those totals live.

Skill total (legacy one-liner, specializations only) = parent + linked attribute + base level + other.

### XP spend (Active units)
- Attribute → level×10
- Base skill (incl. **Riding**) → level×3
- Special skill → level×1 and ≤ parent base
- Discipline (1–18) → cost = current level
- MG may set Base / Other freely; Baron raises Base with XP when Active.
