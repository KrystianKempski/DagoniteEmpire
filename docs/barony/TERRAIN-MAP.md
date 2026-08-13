# Terrain Map — Design & Implementation Plan

## Overview

The **Terrain** tab (`/barony/terrain`) is a Game Master tool for painting the barony (and later the shared domain) on a fixed **15×15** tile grid. The same tile data feeds the **Terrain Improvements** section on the Domain Panel.

Inspired by the tactical **Battle Map** (`BattleMapComponent.razor`): CSS Grid, coordinate loops, sparse tile storage — but domain-focused layers instead of combat tokens.

## Grid

| Property | Value |
|----------|--------|
| Size | 15 × 15 (225 tiles) |
| Coordinates | `X`, `Y` from 0 to 14 |
| Storage | Sparse `TerrainTile` rows in DB; empty cells = default plains, fertility 0 |
| `Barony.Size` | Should reflect assigned tile count (future sync) |

## Layer stack (bottom → top)

Toggle visibility per layer in the UI. Lower layers remain visible through semi-transparent overlays.

| Order | Layer | Purpose | Initial representation |
|-------|--------|---------|------------------------|
| 1 (bottom) | **Fertility** | Soil quality 0–5 | Cell background tint: 0 = yellow → 5 = deep green; numeric label |
| 2 | **Terrain type** | Base type + features | Semi-transparent icons scattered on tile (plains / hills / mountains + forest, river, coast, swamp, …) |
| 3 | **Domain** | Which barony owns the tile | Semi-transparent fill color per barony (neighbour baronies later) |
| 4 | **Fief** | Liege / vassal demesne | Distinct semi-transparent color per `Fief` |
| 5 | **Resources** | Natural deposits | Small icon at bottom-right corner (one resource type for now) |
| 6 (top) | **Improvements** | Farm, city, mill, bridge, … | Token overlay (placeholder until art) |

## Layer details (target behaviour)

### Domain layer
- Shows territorial ownership across **neighbouring baronies** on one shared map.
- Baron Drik = red tint, Baron Kil = blue tint (example); borders TBD.
- **Phase 1:** current barony only — single accent tint on all tiles.

### Fief layer
- Baron demesne vs vassal fiefs (e.g. Baron 10 tiles, vassals Michu / Zdzichu 5 each).
- Colours keyed by `FiefId`; legend in sidebar (future).

### Resources layer
- Icon per `TerrainTile.Resource` (catalog in `TerrainResource`).
- Colored opaque SVG mask in bottom-right corner (metals, stone, wood, clay, salt, fishery, gemstones).

### Terrain type layer
- **Base:** Plains, Hills, Mountains (`TerrainBaseType`).
- **Features** (combinable): Forest, Coast, River, Wasteland, Swamp (`TerrainFeature` CSV on tile).
- Icons: scattered trees (forest), water drop (lake), river line, wilted tree (swamp), etc.

### Fertility layer
- Integer 0–5; only meaningful on Plains / Hills (`TerrainBaseType.SupportsFertility`).
- Always drawn as bottom fill when layer enabled.

### Improvements layer
- Links to `TerrainImprovement` by `TileId` (catalog in `MapImprovement`).
- Large bright SVG icon centered on the tile (town, village, farm, mine, sawmill, hunter's lodge, fishing harbor).

## Data model (existing)

```
Barony ──< TerrainTile (X, Y, BaseType, FeaturesMask, Fertility, Resource, FiefId?, Comment)
       ──< Fief (Name, LiegeName, IsBaronDemesne, BonusMultiplier)
       ──< TerrainImprovement (TileId?, Name, AdditiveJson, PercentJson, …)
```

Repository: `GetTiles`, `SaveTile`, `DeleteTile`, `GetFiefs`, `SaveFief`, `GetImprovements`, `SaveImprovement`.

## UI structure

```
TerrainPage.razor
├── BaronyPageHeader ("Terrain")
├── Layer toggles (MudChip / checkbox row)
├── TerrainMapGrid.razor (15×15)
└── Selection panel (Phase 2: tile editor dialog)
```

## Permissions

| Action | Duke | MG / Admin |
|--------|------|------------|
| View map | ✓ | ✓ |
| Edit tiles / fiefs | — | ✓ (`CanManageAsMg`) |

## Implementation phases

### Phase 1 — **Done / in progress**
- [x] Design doc (this file)
- [x] `TerrainPage.razor` + route `/barony/terrain`
- [x] `TerrainMapGrid` 15×15 with layer toggles (visual placeholders)
- [x] Load tiles / fiefs / improvements from DB

### Phase 2
- Tile edit dialog (base type, features, fertility, resource, fief, comment)
- MG save via `SaveTile`
- Fief CRUD + assign tiles

### Phase 3
- Multi-barony domain layer (world map, barony colours, borders)
- Resource type catalog + icons
- Improvement tokens (Farm, City, Mill, Bridge, …)

### Phase 4
- Terrain Improvements section wired to map tokens
- `Barony.Size` sync, unique index `(BaronyId, X, Y)`

## Reference

- Tactical map: `Pages/Components/BattleMapComponent.razor` (+ `.razor.css`)
- Styles: `wwwroot/css/barony.css` (`.barony-terrain-*`)
- CONCEPT: `docs/barony/CONCEPT.md` § terrain / fiefs
