# Board Visual Grammar

## Design goal

Create one consistent Nature Light language for the board, while making gameplay states unmistakable.

The player should never confuse:

- tile vs cell,
- moss vs green tile,
- blocker vs movable piece,
- background decoration vs gameplay object.

## Visual hierarchy

### 1. World background

Purpose: atmosphere only.

Rules:

- low detail behind board,
- lower contrast than board,
- no tile-like shapes under the board,
- should change by chapter, not every level.

### 2. Board panel

Purpose: contains the game space.

Rules:

- visible frame or plate,
- enough contrast from world background,
- should support chapter variations.

### 3. Cell slots

Purpose: explain the grid.

Rules:

- one subtle rounded slot per active cell,
- visible but not aggressive,
- should survive small-screen scaling,
- inactive cells must not render normal slots.

Suggested style:

- soft cream/green outline,
- very light inner shadow,
- subtle dew/glass feel,
- no hard black lines.

### 4. Cell states

Purpose: show what is on the coordinate independent of the tile.

Examples:

- moss underlay,
- vine layer,
- ice/dew layer,
- blocked root-stone,
- goal glow.

Rules:

- moss uses matte/organic texture,
- blockers use heavier/anchored silhouettes,
- cell states must not bounce like tiles,
- cell states must not use the same highlight/shadow as normal tiles.

### 5. Movable tiles

Purpose: interactable match objects.

Rules:

- distinct silhouette per tile type,
- identifiable without color alone,
- consistent glossy/tactile treatment,
- selection and hint state are obvious but not noisy.

## Concrete visual specifications

### Normal cell slot

File:

```text
Assets/Game/Presentation/NatureLight/Sprites/Board/cell_slot_normal.png
```

Intent:

- subtle rounded square,
- lower contrast than tiles,
- readable on small screen.

Do:

- use thin edge,
- use slight depth,
- keep center calm.

Do not:

- make it look like a collectible,
- use heavy grid lines,
- overpower tile symbols.

### Moss cell underlay

Files:

```text
Assets/Game/Presentation/NatureLight/Sprites/Board/cell_state_moss_underlay_01.png
Assets/Game/Presentation/NatureLight/Sprites/Board/cell_state_moss_underlay_02.png
Assets/Game/Presentation/NatureLight/Sprites/Board/cell_state_moss_underlay_clearing.png
```

Intent:

- cell has moss that should be cleared,
- moss is attached to the cell, not to the tile.

Do:

- use organic edge around cell,
- keep tile visible above it,
- use matte texture,
- use darker/earthier green than green tile.

Do not:

- make it glossy,
- give it tile drop-shadow,
- use the same size/silhouette as a tile.

### Root-stone blocker

Files:

```text
Assets/Game/Presentation/NatureLight/Sprites/Board/cell_state_blocked_root_stone.png
Assets/Game/Presentation/NatureLight/Sprites/Board/cell_state_blocked_root_stone_hit.png
```

Intent:

- this coordinate is not movable.

Do:

- make it visually heavy,
- anchor it into the cell with roots,
- use asymmetrical stone/wood shape,
- remove normal tile selection behavior.

Do not:

- make it round like a tile,
- make it shiny like a tile,
- let it animate like a draggable object.

### Selected tile

File:

```text
Assets/Game/Presentation/NatureLight/Sprites/Board/tile_selected_ring.png
```

Intent:

- show the currently selected tile.

Do:

- use soft golden/cream ring,
- keep it outside tile silhouette,
- ensure it does not cover tile symbol.

### Valid hint

File:

```text
Assets/Game/Presentation/NatureLight/Sprites/Board/tile_hint_pulse.png
```

Intent:

- teach possible move.

Do:

- use mild pulse,
- two-tile relationship should be visible.

Do not:

- pulse everything at once.

### Invalid blocker feedback

No permanent sprite required, but recommended hit overlay:

```text
Assets/Game/Presentation/NatureLight/Sprites/Board/cell_invalid_tap_flash.png
```

Intent:

- tell the player “not movable” without punishment.

Behavior:

- 120–180 ms pulse,
- low thud audio,
- no selection state,
- optional toast only first time.

## Chapter visual progression

### Chapter 1 — Meadow Calm

Levels 1–5.

Visual identity:

- light meadow,
- clean board panel,
- normal slots only,
- no blockers except tutorial-safe intro if needed.

### Chapter 2 — Moss Grove

Levels 6–15.

Visual identity:

- moss underlays,
- deeper green board panel,
- background with soft moss/forest cues.

### Chapter 3 — Root Stones

Levels 16–30.

Visual identity:

- root-stone blockers,
- board shapes with inactive/blocked cells,
- slightly more structured board frame.

### Chapter 4 — Dew Garden

Levels 31–50.

Visual identity:

- dew/ice cell states,
- cool highlights,
- water-drop special objectives.

### Chapter 5 — Moonlit Glade

Levels 51+.

Visual identity:

- darker background,
- stronger tile contrast,
- premium but still readable.

## Readability target

Every gameplay state must be identifiable in under 1 second on:

- small Android screen,
- iPhone portrait,
- tablet,
- screenshot scaled down to 50%.
