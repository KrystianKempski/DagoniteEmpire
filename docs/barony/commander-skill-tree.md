# Commander skill tree (design)

Source: rearranged Excel tree. Requirements are **per branch+tier only**. CX = tier (1/2/3).

Status: `in-code` | `proposal` (★) | `draft` (◆ new from tree notes)

## Tier requirements

| Branch | Tier | CX | Requirements |
|---|---:|---:|---|
| trunk | 1 | 1 | Command ≥ 4 |
| trunk | 2 | 2 | Command ≥ 6 |
| trunk | 3 | 3 | Command ≥ 8 |
| shock | 1 | 1 | Melee ≥ 2, Riding ≥ 5 |
| shock | 2 | 2 | Melee ≥ 3, Riding ≥ 8 |
| shock | 3 | 3 | Melee ≥ 5, Riding ≥ 12 |
| line | 1 | 1 | Armor ≥ 5, Melee ≥ 2 |
| line | 2 | 2 | Armor ≥ 8, Melee ≥ 3 |
| line | 3 | 3 | Armor ≥ 12, Melee ≥ 5 |
| skirmish | 1 | 1 | Shooting ≥ 2, Acrobatics ≥ 2 |
| skirmish | 2 | 2 | Shooting ≥ 3, Acrobatics ≥ 3 |
| skirmish | 3 | 3 | Shooting ≥ 5, Acrobatics ≥ 5 |
| cunning | 1 | 1 | Deceit ≥ 2, Perception ≥ 2 |
| cunning | 2 | 2 | Deceit ≥ 3, Perception ≥ 3 |
| cunning | 3 | 3 | Deceit ≥ 5, Perception ≥ 5 |

## Progression gates (unchanged)

- T2: ≥2 T1 from Trunk ∪ branch
- T3: ≥2 T2 from Trunk ∪ branch, OR ≥1 in-branch T2 + ≥1 Trunk T2
- Max 2× Tier-3 abilities per captain
- Soft-cap: Cmd Atk ≤2, Cmd Def ≤2, Move ≤2 from commander sources

## TRUNK

### Tier 1 — 1 CX — req: Command ≥ 4

- **Hold the Line** (`hold-the-line`) — *in-code*
  - +8 Max HP for the commanded unit.
  - effects: `Hp+8`
  - notes: Was Hp+2 in code; tree comment says +8.
- **+1 Discipline** (`discipline-boost`) — *draft*
  - +1 Discipline for the commanded unit.
  - effects: `Discipline+1`
  - notes: New from tree cell.
- **Column March** (`column-march`) — *proposal*
  - Passing through a friendly body costs open-ground rates instead of Difficult ×2 (still cannot end on ally).
  - effects: `FriendlyPassCostHalved`
- **Keep Facing** (`keep-facing`) — *proposal*
  - Once per Movement phase: one in-place facing change does not spend Move.
  - effects: `FreeFacingOnce`

### Tier 2 — 2 CX — req: Command ≥ 6

- **Fighting Withdrawal** (`fighting-withdrawal`) — *in-code*
  - Disengage free hit deals one-third damage instead of half.
  - effects: `FightingWithdrawal`
- **Second Wind** (`second-wind`) — *proposal*
  - Between Combat rounds: restore 4 HP (not above Max HP).
  - effects: `HpRegenBetweenRounds+4`
  - notes: Was +2; tree comment says +4.
- **March Cadence** (`march-cadence`) — *in-code*
  - +1 Move for the commanded unit.
  - effects: `Move+1`
  - notes: Moved to Trunk T2 in tree.

### Tier 3 — 3 CX — req: Command ≥ 8

- **Front Reface (Planning)** (`front-reface-planning`) — *draft*
  - May change facing to Front during the Attack Planning phase.
  - effects: `FrontFacingInAttackPlanning`
  - notes: New from tree: 'Can do Front direction change during attack planning phase'.
- **Close Ranks** (`close-ranks`) — *proposal*
  - When attacked from the side: Exposure bonus against this unit is −0.5 (min 0).
  - effects: `ExposureVsSide-0.5`
  - notes: Moved to Trunk T3 in tree.
- **Pathfinder** (`pathfinder`) — *in-code*
  - Treat the first and every odd Difficult terrain tile as open.
  - effects: `PathfinderOddTiles`
  - notes: Moved to Trunk T3; comment clarifies odd tiles.

## SHOCK

### Tier 1 — 1 CX — req: Athletics ≥ 2, Riding ≥ 2

- **Counter-Charge** (`counter-charge`) — *in-code*
  - +2 Defense when this unit is the target of a charge.
  - effects: `CounterCharge+2`
  - notes: Was +1; tree comment says +2. Moved to Shock T1.
- **Shock Lance** (`shock-lance`) — *in-code*
  - +1 charge Damage.
  - effects: `ChargeDamage+1`
  - notes: Tree comment dropped 'when mounted' requirement.
- **Wedge** (`wedge`) — *proposal*
  - During a charge: treat 1 Difficult tile as open.
  - effects: `ChargeIgnoreDifficult1`
  - notes: Moved to Shock T1.

### Tier 2 — 2 CX — req: Athletics ≥ 3, Riding ≥ 3

- **Mounted Superiority** (`mounted-superiority`) — *in-code*
  - +1 Attack and +1 Defense when mounted vs unmounted foes.
  - effects: `MountedSuperiorityAtkDef+1`
  - notes: Tree: Atk and Def.
- **Unbroken Momentum** (`unbroken-momentum`) — *proposal*
  - Charge interrupt distance is lowered to 1 tile (default 2).
  - effects: `ChargeInterruptMinSteps1`
  - notes: Reworded from once-immune.
- **Blind Fury** (`blind-fury`) — *proposal*
  - Blind charge end-lock also catches enemies on side-front corners (wider forward arc).
  - effects: `BlindChargeForwardArcWiden`
  - notes: Moved to Shock T2.

### Tier 3 — 3 CX — req: Athletics ≥ 5, Riding ≥ 5

- **Flying Start** (`flying-start`) — *in-code*
  - Charge minimum path is 1 tile shorter (cardinal 3→2, diagonal 2→1).
  - effects: `FlyingStart`
- **Thunder Charge** (`thunder-charge`) — *in-code*
  - Charge bonus becomes +3 Attack / +2 Damage (instead of +2/+1).
  - effects: `ChargeBonusOverride`
- **Overrun** (`overrun`) — *proposal*
  - If the charge target flees this Combat: this unit heals 12 HP.
  - effects: `ChargeKillHeal+12`
  - notes: Changed from leftover Move step.

## LINE

### Tier 1 — 1 CX — req: Athletics ≥ 2, Melee ≥ 5

- **Riveted Plate** (`riveted-plate`) — *proposal*
  - Ignore 1 Pierce when computing EffectiveArmor.
  - effects: `PierceIgnore1`
  - notes: Moved to Line T1.
- **Shield Wall Basics** (`shield-wall-basics`) — *in-code*
  - +1 Defense while the unit carries a shield.
  - effects: `CmdDefense+1WhileShield`
- **Return Stroke** (`return-stroke`) — *proposal*
  - +2 Defense on melee defensive return hits.
  - effects: `DefensiveDefense+2`
  - notes: Tree: +2 Defence on defensive return.

### Tier 2 — 2 CX — req: Athletics ≥ 3, Melee ≥ 6

- **Killing Blow** (`killing-blow`) — *in-code*
  - +1 melee Damage (Other Damage).
  - effects: `DamageMelee+1`
- **Pike Hedge** (`pike-hedge`) — *proposal*
  - +3 Defense when the attacker has a charge bonus against this unit.
  - effects: `VsChargeDefense+3`
  - notes: Was +1; tree says +3.
- **Form Testudo** (`form-testudo`) — *draft*
  - May form Testudo: Exposure from side and front-corner lowered by 0.5; Move −1 while formed.
  - effects: `FormTestudo`
  - notes: New from tree.

### Tier 3 — 3 CX — req: Athletics ≥ 5, Melee ≥ 8

- **Ironclad** (`ironclad`) — *in-code*
  - Once per battle: +2 Armor for one combat round (active).
  - effects: `Ironclad+2`
- **No Step Back** (`no-step-back`) — *in-code*
  - Once per battle: when unit HP falls below 40%, automatically regain 20 HP.
  - effects: `NoStepBackHeal20Below40pct`
  - notes: Reworked vs old panic/flee ignore.
- **Form Square** (`form-square`) — *draft*
  - May form Square: all sides treated as front; Move lowered to 0 while formed.
  - effects: `FormSquare`
  - notes: New from tree.

## SKIRMISH

### Tier 1 — 1 CX — req: Acrobatics ≥ 2, Shooting ≥ 5

- **Drill — Shot** (`drill-shot`) — *in-code*
  - +1 Attack on ranged attacks.
  - effects: `CmdAttack+1Ranged`
- **Loose Files** (`loose-files`) — *proposal*
  - Once per Movement: may take one diagonal step even if both corner tiles are occupied.
  - effects: `DiagonalSqueezeBypassOnce`
- **Snap Shot** (`snap-shot`) — *proposal*
  - +1 Damage on shots against adjacent targets (still a shot: no return, no bind).
  - effects: `PointBlankDamage+1`
  - notes: Tree: damage not Attack.

### Tier 2 — 2 CX — req: Acrobatics ≥ 3, Shooting ≥ 6

- **Feigned Retreat** (`feigned-retreat`) — *in-code*
  - After Disengage: +1 Move that turn.
  - effects: `FeignedRetreat`
- **Long Shot** (`long-shot`) — *in-code*
  - Shot range Attack penalty is −1 per tile instead of −2.
  - effects: `LongShot`
- **Harassing Fire** (`harassing-fire`) — *proposal*
  - On a damaging shot: target’s next turn Move is lowered by 1.
  - effects: `ShotNextTurnMove-1`
  - notes: Reworded from same-phase remaining Move.

### Tier 3 — 3 CX — req: Acrobatics ≥ 5, Shooting ≥ 8

- **Extended Range** (`extended-range`) — *draft*
  - +1 Range on ranged attacks.
  - effects: `Range+1`
  - notes: New from tree: '+1 range to range attacks'.
- **Skirmish Screen** (`skirmish-screen`) — *proposal*
  - May shoot even while engaged.
  - effects: `ShootWhileEngaged`
  - notes: Tree dropped 'once per battle'.
- **Enfilade** (`enfilade`) — *proposal*
  - Shots treat target side Exposure as rear-corner (+1.5→+2) and rear-corner as rear (+2→+2.5).
  - effects: `ShotExposureEscalate`

## CUNNING

### Tier 1 — 1 CX — req: Deceit ≥ 5, Observation ≥ 2

- **Kill the Captain** (`kill-the-captain`) — *proposal*
  - +2 Attack against units that have an assigned captain.
  - effects: `VsCaptainAttack+2`
  - notes: Was +1; tree says +2. Moved to T1.
- **Look Away** (`look-away`) — *proposal*
  - Once per Movement phase: after this unit finishes its move, set facing for free (no Move spend).
  - effects: `FreeFacingAfterMoveOnce`
- **Knife in the Dark** (`knife-in-the-dark`) — *proposal*
  - Attacks from rear or rear-corner: +1 Damage (stacks with Backstab Doctrine).
  - effects: `RearDamage+1`
  - notes: Tree dropped 'first each battle'.
- **+2 Initiative** (`initiative-boost`) — *draft*
  - +2 Initiative for contested-tile ties and Combat resolve order.
  - effects: `Initiative+2`
  - notes: New from tree cell.

### Tier 2 — 2 CX — req: Deceit ≥ 6, Observation ≥ 3

- **Decoy Banner** (`decoy-banner`) — *proposal*
  - Once per Attack Planning: if an enemy targets an adjacent ally and this unit is a valid target, pull the order onto yourself.
  - effects: `PullAttackOntoSelfOnce`
  - notes: Moved to T2.
- **False Retreat Trap** (`false-retreat-trap`) — *proposal*
  - When an enemy moves while engaged with this unit: free hit deals full Damage instead of half (once per battle).
  - effects: `DisengageHitFullOnce`
  - notes: Moved to T2.
- **Turn the Flank** (`turn-the-flank`) — *proposal*
  - When attacking from the side: Aim counts as corner (1.5) instead of side/rear (1).
  - effects: `AimVsSideAsCorner`

### Tier 3 — 3 CX — req: Deceit ≥ 8, Observation ≥ 5

- **Backstab Doctrine** (`backstab-doctrine`) — *in-code*
  - +1 front multiplier on rear and side-rear attacks.
  - effects: `BackstabDoctrine`
  - notes: Moved to T3; includes side-rear.
- **Read the Enemy** (`read-the-enemy`) — *in-code*
  - Nearest enemy within 3 movement range reveals planned movement.
  - effects: `ReadTheEnemyRange3`
- **Clean Disengage** (`clean-disengage`) — *draft*
  - Disengage does not provoke free attacks.
  - effects: `DisengageNoProvoke`
  - notes: New from tree.
- **Mask of Dust** (`mask-of-dust`) — *proposal*
  - Opponent sees no movement ghost for this unit until Finish move.
  - effects: `HideMoveGhost`
  - notes: Moved to T3.
