# Barony — katalog formuł PPB

Źródła runtime: `DA_Common/Barony/*PpbFormulas.cs` oraz `DagoniteEmpire/Pages/Barony/BaronyCalc.cs`.

## Social Groups — Tax

- Domyślne stawki: `SocialGroup.DefaultTax` — Nobility 5%, Burghers 15%, Peasants 30%
- Kolumna **Tax** w sekcji Social Groups; edycja w dialogu relacji
- Town Treasury używa `TownTaxRates.FromRelations` (Nob/Burg/Peas % × Pop × stawka)

## Terrain — Village

- Treasury: `(Nob%/100)×Pop×5 + (Burg%/100)×Pop×5 + (Peas%/100)×Pop×15`
- Klasa: `VillagePpbFormulas`

## Terrain — Town

- Treasury: `(Nob%/100)×Pop×20 + (Burg%/100)×Pop×25 + (Peas%/100)×Pop×10`
- Klasa: `TownPpbFormulas`

## Community Penalties

### Hunger
- `Hunger = max(0, -FoodBalance)`
- `% Economy/Production = max(-Hunger×10, -50)`
- `+ Loyalty/Stability = -Hunger×3`, `Law = -Hunger×2`, `Corruption = Hunger`

### Crime
- `Crime = max(0, -LawBalance)`
- `% Economy/Production = max(-Crime×5, -50)`
- `+ Loyalty = -Crime`, `Stability = -Crime×2`, `Corruption = Crime/2`

### Corruption
- `Corruption = max(0, CorruptionBalance)`
- `% Economy/Production = max(-Corruption×5, -50)`
- `+ Loyalty = -Corruption×2`, `Stability = -Corruption`

### Unrest
- `Unrest = Barony.Unrest`
- `% Economy/Production = -Unrest×15`
- `+ Loyalty/Stability/Law = -Unrest×3`

## Baron Card — From Skills

Klasa: `BaronSkillPpbFormulas`.

- Wyniki zaokrąglane w dół: `⌊...⌋`
- Atrybut w formule = `ModifierAbsolute` (bez ran i tymczasowych stanów)
- `Magic` to customowa specjalizacja Knowledge
- `Dowodzenie -> Inspire`, `Dworskie maniery -> Diplomacy`

## Baron and Advisors — wpływ Food barona

Logika: `BaronyCalc.BuildBaronAdvisorRow`.

- `X = BaronSkill[Food]`
- `%`: wszystkie PPB poza `Treasury` dostają `+X`, `Corruption` dostaje `-X`
- `+`: `Stability`, `Loyalty`, `Law`, `Science`, `Magic`, `Culture`, `Intelligence` dodawane z barona
- `Corruption` addytywnie odejmowane o `BaronSkill[Corruption]`
