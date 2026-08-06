# Court commanders (model B)

Logic: `CourtCommanderCatalog` / `CourtCommanderFormulas` / `UnitCommanderSync`  
UI: Offices → Court (Commander section + Skill tree dialog), Army → unit Captain picker  
Data: `docs/barony/commander-skill-tree.json` (embedded as `DA_Common.Barony.commander-skill-tree.json`)

## Data source

The catalog (abilities + per-tier requirements) is authored in
`docs/barony/commander-skill-tree.json` and embedded into `DA_Common`.
`CourtCommanderCatalog.Load()` deserializes it at startup — edit the JSON,
rebuild `DA_Common`, and the tree/rules update. There is no hard-coded ability list.

Each ability carries a **status**: `in-code` (effect wired in battle),
`proposal`, or `draft`. The tree dialog shows this as a badge.

> **Effects are not wired yet.** `CourtCommanderFormulas.ComputeBonuses` returns an
> empty `CommanderBonusResult`, so assigning a captain currently applies **no**
> stat/flag changes in battle. The unlock economy, requirements, gates, tree UI and
> persistence are all live; hooking each ability's effect is a later phase. The
> `CommanderBonusResult` shape (charge flags etc.) is retained so battle code keeps
> compiling.

## Locked rules

- Shared **Trunk** + branches **Shock / Line / Skirmish / Cunning**.
- Court **Attack / Defence do not** feed `CommanderAttack` / `CommanderDefense` — only unlocked abilities (once effects are wired).
- **1 Courtier ↔ 1 unit** captain at a time; units may have no captain.
- Progress (`CommanderXp`, `UnlockedCommanderAbilities`) lives on the court `SheetJson`.
- `CourtCharacterSheet.Normalize()` drops any unlocked key not present in the catalog (legacy keys are pruned automatically).
- Unit stores `CaptainAvailableAdvisorId`; sync runs on assign / captain save.

## Requirements (branch + tier only)

Requirements are per **branch + tier**, not per ability — every ability in a
tier shares the same skill gate (`CourtCommanderCatalog.FindTierRequirement`).

| Branch | T1 | T2 | T3 |
|---|---|---|---|
| Trunk | Command ≥ 4 | Command ≥ 6 | Command ≥ 8 |
| Shock | Athletics ≥ 2, Riding ≥ 2 | Athletics ≥ 3, Riding ≥ 3 | Athletics ≥ 5, Riding ≥ 5 |
| Line | Athletics ≥ 2, Melee ≥ 5 | Athletics ≥ 3, Melee ≥ 6 | Athletics ≥ 5, Melee ≥ 8 |
| Skirmish | Acrobatics ≥ 2, Shooting ≥ 5 | Acrobatics ≥ 3, Shooting ≥ 6 | Acrobatics ≥ 5, Shooting ≥ 8 |
| Cunning | Deceit ≥ 5, Observation ≥ 2 | Deceit ≥ 6, Observation ≥ 3 | Deceit ≥ 8, Observation ≥ 5 |

(“Riding” maps to the `animal-handling-riding` secondary skill.)

## CX

- Costs: T1 = 1, T2 = 2, T3 = 3.
- After battle (captain assigned):  
  `⌊dealt/5⌋ + engagedRounds + 2×kills − ⌊taken/10⌋`, floored at 0, then × `(0.8 + Command/20)`.
- MG can edit CX pool on the Court card.

## Gates

- T2 (branch): ≥2 T1 from Trunk ∪ that branch.
- T3 (branch): ≥2 T2 from Trunk ∪ branch, **or** ≥1 in-branch T2 + ≥1 Trunk T2.
- Max **2** T3 per captain.
- Soft caps (reserved for when effects are wired): Cmd Atk ≤2, Cmd Def ≤2, Move ≤2 from commander sources.

## Buying abilities

The **Skill tree** dialog (Offices → Court → Commander → Skill tree) is the single
buy surface. Select a node to see its description, effect shorthand, status badge
and the tier requirement; MG clicks **Unlock** when it is ready. The old dropdown
unlock dialog has been retired.

