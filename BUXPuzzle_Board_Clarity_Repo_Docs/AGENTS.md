# AGENTS.md — BUXPuzzle Board Clarity Instructions

## Mission

Fix BUXPuzzle board readability and early-level progression without breaking deterministic gameplay truth.

The player must understand, within 1 second:

1. which cells are playable,
2. which tiles can move,
3. which cells contain moss,
4. which cells are blocked or non-movable,
5. what the level goal is,
6. why the current board differs from the previous chapter/level.

## Hard rules

- `BoardEngine` remains the source of gameplay truth.
- Presentation must project engine state, not create alternative state.
- Do not duplicate existing systems if a current class already provides the right extension point.
- Prefer extending `BoardRows`, `LevelSpec`, `LevelConfig`, `BoardView`, `TileView`, `NatureLightRuntimeArt`, and the Nature Light art manifest before adding parallel systems.
- All new visual states must have:
  - engine or level data source,
  - presentation rendering,
  - input behavior,
  - HUD/tutorial language where relevant,
  - tests or QA checklist coverage.

## Critical product rule

A blocker must never look like a normal tile.

A moss cell must read as underlay/objective state, not as a movable tile.

A normal tile must read as movable.

## Required outcome

The next milestone is **Board Clarity Milestone**, not “more content”.

Done means:

- board grid/slots are clearly visible on small screens,
- moss is rendered as cell underlay or cell state,
- blocked cells have a separate visual language,
- invalid interactions on blocked cells produce clear but gentle feedback,
- levels 1–30 define a progression arc,
- at least levels 1, 6, 11, 16, 20 and 30 have hand-authored layouts,
- QA screenshots prove states are readable.

## Implementation priority

1. Verify existing `CellSlots` are visible and scale correctly.
2. Separate movable tile visuals from cell-state visuals.
3. Implement/verify `CellState` or equivalent board cell data.
4. Render moss/blocker via cell layer, not tile identity.
5. Ensure swap/drop/spawn ignore or respect blocked cells correctly.
6. Connect level JSON / `BoardRows` to board layout.
7. Add authored assets and update `NatureLightArtManifest.json`.
8. Build first 30-level progression.
9. Add readability QA gate.

## Definition of Done for code changes

For each meaningful change, include:

- affected files,
- reason for change,
- tests or manual QA steps,
- acceptance criteria,
- screenshots for visual changes where possible.

## What not to do

- Do not make all green objects brighter; that worsens readability.
- Do not solve blocker confusion using only text.
- Do not hide grid entirely for aesthetic reasons.
- Do not add monetization, store assets, or new currencies during this milestone.
- Do not add “10x” mechanics before moss/blocker readability is solved.
