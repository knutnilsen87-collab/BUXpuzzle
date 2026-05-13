# Board Clarity Milestone

## Purpose

This milestone makes BUXPuzzle readable as a mobile match-3 board.

The current product issue is not only visual polish. It is a gameplay-readability issue. The player must immediately understand what can move, what cannot move, what must be cleared, and how the board evolves.

## Target player experience

Within the first 5 seconds of a level, a new player should be able to say:

- “These are the cells.”
- “These are the pieces I can move.”
- “This moss is something I must clear.”
- “This blocker cannot be moved.”
- “This level looks different from the previous one.”

## Current evidence

Observed from screenshot:

- moss/blocker visuals compete with normal green tiles,
- board cells are not visually strong enough,
- board/background relationship is flat,
- progression identity is weak.

Observed from repo:

- `BoardView` already has a `CellSlots` path, so the next task is likely visibility, hierarchy and state rendering, not necessarily adding a slot layer from scratch.
- `LevelSpec` already supports `BoardRows`, `WorldId`, objectives and mechanics, so the next task is to connect/use these consistently for authored board progression.
- `NatureLightArtManifest.json` already defines authored art intent and target identification time.

## Milestone scope

In scope:

1. board grid / cell slots,
2. moss cell underlay,
3. blocker / root-stone cell state,
4. HUD objective clarity,
5. invalid input feedback,
6. first 30-level progression plan,
7. authored asset backlog,
8. readability QA checklist.

Out of scope:

- monetization,
- store screenshots,
- new economy/currency,
- leaderboard,
- live ops,
- deep VFX pass,
- new special-tile system beyond current roadmap,
- App Store / Play Store submission polish.

## Visual layer model

The board must render in these layers:

```text
World background
Board panel / frame
Cell slots
Cell state underlays: moss, goal, ice, vine
Blocking cell overlays: root-stone, hard blocker
Movable tiles
Selection / hint / tutorial overlays
Match / clear VFX
HUD
```

## State vocabulary

Use separate concepts:

### Tile state

Describes the thing that moves:

- normal tile,
- selected tile,
- hinted tile,
- special tile,
- frozen/locked tile if it remains a tile.

### Cell state

Describes the board coordinate:

- normal cell,
- moss cell,
- blocked cell,
- hole/inactive cell,
- ice/dew cell,
- vine cell,
- goal underlay.

### Objective state

Describes what the level wants:

- clear moss,
- clear blockers,
- collect tile type,
- create specials,
- reach score,
- drop object.

Do not use the same sprite and same code meaning for all three concepts.

## Acceptance criteria

The milestone is complete when:

- [ ] Normal cells are visible as a readable grid on mobile.
- [ ] Moss is visually an underlay/objective cell, not a movable tile.
- [ ] Blockers are visually anchored/non-movable.
- [ ] Blockers cannot be swapped.
- [ ] Drop/spawn logic handles inactive/blocked cells correctly.
- [ ] HUD goal icon matches the board objective visual.
- [ ] Level 1, 6, 11, 16, 20 and 30 have authored layouts.
- [ ] At least 3 visual chapters exist.
- [ ] Small-screen QA screenshots pass readability review.
- [ ] A new player can identify movable vs blocked vs moss within 1 second.
