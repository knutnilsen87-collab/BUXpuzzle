# First 30 Levels Plan

## Purpose

Create a consistent early-game progression where each group of levels teaches one thing and makes the board feel like it evolves.

The goal is not just difficulty. The goal is readable progression.

## Board row notation

Use this notation for `BoardRows` or equivalent level-layout data.

```text
. = normal playable cell
m = moss cell / moss underlay
# = blocked root-stone cell
x = inactive/hole cell
v = vine cell, later
d = dew/ice cell, later
```

Important: this notation describes cells, not tile types.

## Level structure

### Chapter 1 — Meadow Calm

Levels 1–5.

Player learns:

- swap,
- match,
- moves,
- score/objective,
- first completion loop.

Visual:

- meadow background,
- normal board panel,
- normal cell slots,
- no confusing moss/blockers.

#### Level 1 — First clear

```json
{
  "worldId": 1,
  "levelId": 1,
  "width": 6,
  "height": 6,
  "moveLimit": 6,
  "goals": [{ "type": "Score", "target": 500 }],
  "forceWinBias": true,
  "allowLose": false,
  "newMechanic": "None",
  "theme": "MeadowCalm",
  "designerNote": "First reward within 5 seconds. No blockers. Clear grid.",
  "boardRows": [
    "......",
    "......",
    "......",
    "......",
    "......",
    "......"
  ]
}
```

#### Level 2 — First special

Goal: teach 4-match safely.

Use 6x6. No blockers.

#### Level 3 — Board reading

Goal: introduce selected/hint state. No blockers unless tutorial-safe.

#### Level 4 — Moves matter

Goal: complete with limited moves but still low pressure.

#### Level 5 — First possible loss

Goal: mild loss possibility but still rewarded.

### Chapter 2 — Moss Grove

Levels 6–15.

Player learns:

- moss is on the cell,
- matching on/near moss clears it,
- moss is not a movable tile.

Visual:

- moss grove background,
- slightly deeper board panel,
- moss underlay.

#### Level 6 — Moss intro

```json
{
  "worldId": 2,
  "levelId": 6,
  "width": 6,
  "height": 6,
  "moveLimit": 12,
  "goals": [{ "type": "ClearBlockers", "target": 4 }],
  "forceWinBias": true,
  "allowLose": false,
  "newMechanic": "Moss",
  "theme": "MossGrove",
  "designerNote": "First moss. Moss must appear under tiles and match HUD icon.",
  "boardRows": [
    "......",
    "..mm..",
    "..mm..",
    "......",
    "......",
    "......"
  ]
}
```

#### Level 7 — Moss line

BoardRows:

```text
......
..mm..
..mm..
..mm..
......
......
```

#### Level 8 — Moss edges

Teach that moss can be near edges without confusing board bounds.

#### Level 9 — Moss cluster

Target: 8 moss.

#### Level 10 — First moss milestone

8x8 board, 10–12 moss cells.

```text
........
..mmmm..
..m..m..
........
........
..m..m..
..mmmm..
........
```

#### Level 11 — Moss with specials

Teach special interaction with moss.

#### Level 12 — Moss but fewer moves

Efficiency.

#### Level 13 — Mixed score + moss

Use two-goal system if supported; otherwise single goal.

#### Level 14 — Board shape hint

Introduce inactive cells visually, if implemented.

#### Level 15 — Chapter checkpoint

Reward and prepare for root-stone.

### Chapter 3 — Root Stones

Levels 16–30.

Player learns:

- root-stone blockers cannot move,
- blockers are cell objects,
- board shapes can change,
- moss and blockers can combine.

Visual:

- root-stone background,
- stronger board frame,
- anchored blockers.

#### Level 16 — Root-stone intro

```json
{
  "worldId": 3,
  "levelId": 16,
  "width": 7,
  "height": 7,
  "moveLimit": 18,
  "goals": [{ "type": "ClearBlockers", "target": 4 }],
  "forceWinBias": true,
  "allowLose": false,
  "newMechanic": "RootStone",
  "theme": "RootStones",
  "designerNote": "First permanent blocked cells. Player must see these cannot move.",
  "boardRows": [
    ".......",
    "...#...",
    "..###..",
    "...#...",
    ".......",
    ".......",
    "......."
  ]
}
```

#### Level 17 — Blocker corridor

```text
.......
..#.#..
.......
..#.#..
.......
..#.#..
.......
```

#### Level 18 — Blocker + moss separate

Use both, but do not overlap.

```text
........
..m..m..
..#..#..
........
..#..#..
..m..m..
........
........
```

#### Level 19 — Edge blockers

Teach fall/drop constraints.

#### Level 20 — First mixed milestone

8x8, moss and blockers. This is the “does the system work?” level.

```text
........
..mmmm..
..#..#..
..m..m..
..m..m..
..#..#..
..mmmm..
........
```

#### Level 21 — Special clear objective

Use specials with blockers nearby.

#### Level 22 — Color collection

No new visual mechanic. Let player breathe.

#### Level 23 — Special challenge

Encourage intentional 4/5 matches.

#### Level 24 — Cascade target

Board shaped for cascades.

#### Level 25 — First hard level

Still fair. Do not add new visual state.

#### Level 26 — Vine intro, optional later

Only introduce if vine art and logic are ready. Otherwise use root-stone variation.

#### Level 27 — Root-stone + drop objective, optional later

Only if drop objective exists.

#### Level 28 — Mixed objective

Moss + score or moss + specials.

#### Level 29 — Daily-style board

Replayable structure.

#### Level 30 — Chapter finale

8x8, strong board identity, reward unlock.

```json
{
  "worldId": 3,
  "levelId": 30,
  "width": 8,
  "height": 8,
  "moveLimit": 26,
  "goals": [{ "type": "ClearBlockers", "target": 14 }],
  "forceWinBias": false,
  "allowLose": true,
  "newMechanic": "RootStone",
  "theme": "RootStones",
  "designerNote": "Chapter finale. Clear visual distinction between moss, blockers and tiles.",
  "boardRows": [
    "........",
    ".m#mm#m.",
    "..#..#..",
    ".m....m.",
    ".m....m.",
    "..#..#..",
    ".m#mm#m.",
    "........"
  ]
}
```

## Level QA rules

For every level:

- there must be at least one valid move at start,
- no automatic start cascade unless intentionally designed,
- goal can be understood from HUD,
- objective visual exists on board,
- no impossible blocker/moss layout,
- first 5 levels should not punish experimentation,
- every new mechanic gets tutorial copy.

## Content tasks for owner/designer

- [ ] Create JSON for levels 1–30.
- [ ] Choose final objective for each level.
- [ ] Decide exact moment moss appears.
- [ ] Decide exact moment blockers appear.
- [ ] Decide whether vine/dew appears before level 30 or later.
- [ ] Playtest levels 1–10 with someone who has not seen the game.
- [ ] Adjust move limits after playtest.
