# Board Clarity Action Cards

## BC-01 — Make cell slots visibly readable

**Severity:** S1  
**Opportunity:** Trust/readability unlock  
**Certainty:** Strongly indicated from screenshot; code path exists and must be verified  
**Files:**  
`Assets/Game/Presentation/BoardView.cs`  
`Assets/Game/Presentation/NatureLightRuntimeArt.cs`  
`Assets/Game/Presentation/NatureLight/Sprites/Board/cell_slot_normal.png`

### Problem

The board does not read strongly enough as a grid. Players see floating objects instead of a clear set of cells.

### Fix

Verify existing `CellSlots` path and make normal cell slots visible, scaled and sorted correctly.

### Implementation steps

1. Inspect `EnsureCellSlots`.
2. Confirm every active cell gets one slot.
3. Confirm inactive cells do not render normal slots.
4. Increase slot readability if too subtle.
5. Use authored `cell_slot_normal.png` when available.
6. Add screenshot before/after.

### Tests

- 6x6, 7x7 and 8x8 boards show correct cells.
- Small-screen screenshot still shows grid.
- Slots do not cover tile symbols.

### Acceptance criteria

- A new player can identify grid boundaries in under 1 second.
- Grid is visible but not visually louder than tiles.

---

## BC-02 — Render moss as cell underlay

**Severity:** S1  
**Opportunity:** Objective clarity unlock  
**Certainty:** Verified need from screenshot  

### Problem

Moss can be confused with a green movable tile.

### Fix

Represent moss as a cell state underlay, not a tile.

### Implementation steps

1. Add/verify `CellState.Moss`.
2. Parse `m` in `BoardRows`.
3. Render moss under tile via cell-state layer.
4. Add clear animation when moss is cleared.
5. Connect HUD moss icon to same visual.
6. Ensure leaf tile and moss have different silhouettes/textures.

### Tests

- Moss under tile remains visible.
- Tile on moss remains movable if rules allow.
- Clearing moss updates objective.
- Moss is not selected as a tile.

### Acceptance criteria

- Moss cannot be mistaken for a normal tile on screenshot.

---

## BC-03 — Implement root-stone blocked cell

**Severity:** S1  
**Opportunity:** Rules clarity unlock  
**Certainty:** Strongly indicated  

### Problem

The player cannot easily identify what is blocked/non-movable.

### Fix

Add a heavy, anchored root-stone cell state.

### Implementation steps

1. Add/verify `CellState.Blocked`.
2. Parse `#` in `BoardRows`.
3. Prevent swap into/out of blocked cells.
4. Prevent spawn/drop into blocked cells.
5. Render root-stone blocker via cell layer.
6. Add invalid tap feedback.

### Tests

- Blocker cannot be swapped.
- Blocker cannot be selected.
- Tiles do not fall into blocker.
- Invalid tap feedback plays.

### Acceptance criteria

- Blocker reads as fixed object, not a draggable tile.

---

## BC-04 — BoardRows parser and validation

**Severity:** S1  
**Opportunity:** Level production unlock  
**Certainty:** Verified need  

### Problem

Level progression needs authored layouts, not only generated difficulty.

### Fix

Complete `BoardRows` parsing and validation.

### Implementation steps

1. Support `.`, `m`, `#`, `x`, `v`, `d`.
2. Validate width and height.
3. Default to all normal cells if no rows.
4. Fail loudly on unknown characters.
5. Add tests.

### Acceptance criteria

- `Level_6.json` can define moss.
- `Level_16.json` can define blockers.
- Invalid level files do not silently produce broken boards.

---

## BC-05 — Chapter theme mapping

**Severity:** S2  
**Opportunity:** Progression/value unlock  
**Certainty:** Strongly indicated  

### Problem

Levels do not feel like an evolving board/world.

### Fix

Map world/chapter to visual theme.

### Implementation steps

1. Create theme ids: MeadowCalm, MossGrove, RootStones, DewGarden, MoonlitGlade.
2. Map `WorldId` or `theme` to background, board panel, slot and accent.
3. Apply theme in board presentation.
4. Ensure tile identities remain stable.

### Acceptance criteria

- Level 1, 6 and 16 look like different chapters.
- Tiles remain familiar and readable.

---

## BC-06 — HUD objective icon match

**Severity:** S2  
**Opportunity:** UX clarity unlock  

### Problem

HUD text can say “Rydd Moss”, but board visual does not clearly match.

### Fix

Use objective icons that match board cell visuals.

### Acceptance criteria

- Moss HUD icon looks like moss underlay.
- Blocker HUD icon looks like root-stone blocker.
- Progress updates correctly.

---

## BC-07 — First mechanic tutorial cards

**Severity:** S2  
**Opportunity:** Activation unlock  

### Problem

New mechanics need one clear first-time explanation.

### Fix

Add tutorial cards for moss and blockers.

### Copy suggestion

Moss:

```text
Mose ligger på rutene. Lag matcher på mose-ruter for å rydde dem.
```

Blocker:

```text
Rotstein kan ikke flyttes. Lag matcher rundt den for å åpne brettet.
```

### Acceptance criteria

- Tutorial appears only first time or on first relevant level.
- Tutorial does not interrupt every move.

---

## BC-08 — Readability screenshot QA

**Severity:** S2  
**Opportunity:** Release quality unlock  

### Problem

Visual clarity must be tested on small screens, not only in editor.

### Fix

Create screenshot QA pass for key states.

### Acceptance criteria

- Captures at levels 1, 6, 11, 16, 20, 30.
- Reviewer can identify all cell/tile states at 50% screenshot size.

---

## BC-09 — First 30 authored level files

**Severity:** S2  
**Opportunity:** Retention/progression unlock  

### Problem

Progression needs designed boards.

### Fix

Create/update `Level_1.json` to `Level_30.json`.

### Acceptance criteria

- First 30 levels exist.
- All pass level validation.
- Levels 1–5 onboarding, 6–15 moss, 16–30 blockers/mixed.

---

## BC-10 — Update art manifest

**Severity:** S3  
**Opportunity:** Production pipeline unlock  

### Problem

New assets need a stable contract.

### Fix

Update `NatureLightArtManifest.json` with board, background, HUD and VFX references.

### Acceptance criteria

- Manifest lists every required P0/P1 asset.
- No ambiguous asset names.
