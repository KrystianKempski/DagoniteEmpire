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

> **Effects — phased wiring in progress.** `CourtCommanderFormulas.ComputeBonuses`
> now applies **Phase 1** flat passives to the commanded unit: Hold the Line (+8 HP),
> March Cadence (+1 Move), Killing Blow (+1 melee Damage), Shield Wall Basics
> (+1 Defense while carrying a shield). These sync onto the unit via `UnitCommanderSync`
> on captain assign / save.
>
> **Phase 2** adds flat + context-sensitive combat bonuses, resolved in `BattleMapPage`
> combat (`ApplyCommanderBattleFlags` copies flags onto the token; `PlanCombatExchanges`
> applies them): +1 Discipline and +2 Initiative (fold into InitiativeTotal), Riveted Plate
> (ignore 1 Pierce as defender), Drill—Shot (+1 Attack on shots), Mounted Superiority
> (+1 Atk/Def mounted vs unmounted), Counter-Charge (+2 Def vs charge), Pike Hedge
> (+3 Def vs charge), Return Stroke (+2 Def on defensive return), Kill the Captain
> (+2 Attack vs captained targets).
>
> **Phase 3** wires the Shock-branch charge tricks (resolved during charge pathing and
> combat in `BattleMapPage`): Shock Lance (+1 charge Damage), Thunder Charge (charge
> Attack +3 and +2 Damage, overriding lesser charge bonuses), Flying Start (charge-path
> minimum reduced by 1), Wedge (ignore the movement cost of 1 difficult tile per charge),
> Unbroken Momentum (charge counts as landed after only 1 step for interrupt checks),
> Blind Fury (wider charge-contact arc), Overrun (heal 12 HP, capped at MaxHp, when a
> charge makes the target flee that exchange).
>
> **Phase 4** wires the Skirmish-branch ranged tricks: Snap Shot (+1 Damage on shots at
> adjacent targets), Long Shot (range Attack penalty −1/tile instead of −2), Harassing Fire
> (a damaging shot lowers the surviving target's next-turn Move by 1, applied when the
> Movement phase resets), Extended Range (+1 Range on the battle token), Skirmish Screen
> (may shoot while engaged/pinned), Enfilade (flank/rear shots escalate the target's
> Exposure bonus by +0.5).
>
> **Tier 1 finishers** close out every T1 ability across all branches:
> Knife in the Dark (+1 Damage on a shot/strike hitting the target's rear, Exposure band
> ≥ 4), Keep Facing (one *in-place* facing change per Movement phase spends no Move),
> Look Away (one facing change *after* moving spends no Move — both gated by
> `FreeFacingUsed`, reset in `EndCombatPhase`), Column March (squeezing past a free
> comrade costs open-ground rates instead of difficult ×2 — applied in both planning
> `PlannedStepHalfCost` and the simulator `TrySlipPast`), Loose Files (one diagonal step
> per Movement phase may pass between two occupied corners — `MoverState.LooseFilesLeft`
> consumed in `TryStartStep`). Tier 1 is fully wired.
>
> Remaining abilities (movement, active once-per-battle,
> formations, information) are wired in later phases; until then they are buyable but
> inert. The `CommanderBonusResult` shape (charge flags etc.) is retained so battle code
> keeps compiling.

- Shared **Trunk** + branches **Shock / Line / Skirmish / Cunning**.
- Court **Attack / Defence do not** feed `CommanderAttack` / `CommanderDefense` — only unlocked abilities (once effects are wired).
- **1 Courtier ↔ 1 unit** captain at a time; units may have no captain.
- Army UI: helm icon on the unit card opens **Assign captain** dialog (free courtiers only). Assigned captain shows as `(Name)` beside the unit title; details (abilities) are in a Barony tooltip.
- Progress (`CommanderXp`, `UnlockedCommanderAbilities`) lives on the court `SheetJson`.
- `CourtCharacterSheet.Normalize()` drops any unlocked key not present in the catalog (legacy keys are pruned automatically).
- Unit stores `CaptainAvailableAdvisorId`; sync runs on assign / captain save.

## Requirements (branch + tier only)

Requirements are per **branch + tier**, not per ability — every ability in a
tier shares the same skill gate (`CourtCommanderCatalog.FindTierRequirement`).

**PC / linked characters** use Absolute skill totals (`SumAbsolute`) from
`characterRequirements`. Court-only sheets use the mapped `requirements`
(Armor → Athletics, Perception → Observation; Riding/Athletics capped at secondary max 6).

| Branch | T1 | T2 | T3 |
|---|---|---|---|
| Trunk | Command ≥ 4 | Command ≥ 6 | Command ≥ 8 |
| Shock | Melee ≥ 2, Riding ≥ 5 | Melee ≥ 3, Riding ≥ 8 | Melee ≥ 5, Riding ≥ 12 |
| Line | Armor ≥ 5, Melee ≥ 2 | Armor ≥ 8, Melee ≥ 3 | Armor ≥ 12, Melee ≥ 5 |
| Skirmish | Shooting ≥ 2, Acrobatics ≥ 2 | Shooting ≥ 3, Acrobatics ≥ 3 | Shooting ≥ 5, Acrobatics ≥ 5 |
| Cunning | Deceit ≥ 2, Perception ≥ 2 | Deceit ≥ 3, Perception ≥ 3 | Deceit ≥ 5, Perception ≥ 5 |

(“Riding” / “Armor” on a PC are special skills; court sheets map Riding → `animal-handling-riding`, Armor → Athletics.)

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

