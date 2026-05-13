# Board Readability QA Checklist

## Purpose

Validate that BUXPuzzle is readable on real mobile-like screens.

This QA checklist must pass before creating store screenshots or adding more advanced levels.

## Required captures

Capture screenshots for:

```text
Level 1  - normal board
Level 6  - first moss
Level 11 - moss progression
Level 16 - first blocker
Level 20 - moss + blocker mixed
Level 30 - chapter finale
```

Capture each in:

- Unity Game view,
- small Android portrait aspect,
- iPhone portrait aspect,
- screenshot scaled down to 50%.

## Visual state checklist

For each screenshot, answer yes/no.

### Board

- [ ] Can you see the grid/cell structure?
- [ ] Can you tell where the board begins and ends?
- [ ] Is the board stronger than the background?
- [ ] Are inactive/hole cells clearly absent or different?

### Tiles

- [ ] Can each tile type be recognized without relying only on color?
- [ ] Are tile symbols visible during normal play?
- [ ] Does selected tile state read clearly?
- [ ] Does hint state read clearly without being noisy?

### Moss

- [ ] Does moss look like a cell underlay?
- [ ] Does moss avoid the same shine/shadow as tiles?
- [ ] Can moss be distinguished from leaf/green tile?
- [ ] Does HUD moss icon match board moss?

### Blockers

- [ ] Does blocker look non-movable?
- [ ] Does blocker avoid tile-like glossy treatment?
- [ ] Does blocker have anchored/heavy silhouette?
- [ ] Does tapping blocker create clear feedback?

### HUD

- [ ] Is the current objective understandable?
- [ ] Does objective progress update after clear?
- [ ] Is there enough contrast for score/moves/objective?
- [ ] Does HUD avoid covering gameplay?

### Motion / VFX

- [ ] Match clear does not hide tile identity.
- [ ] Moss clear feedback explains progress.
- [ ] Invalid swap feedback is visible but not punishing.
- [ ] Cascades remain readable.

## 1-second test

Show the screenshot to someone unfamiliar with the game for 1 second.

Ask:

1. What can you move?
2. What looks blocked?
3. What is the goal object?
4. Where is the board grid?

Pass if they answer at least 3 of 4 correctly.

## Failure responses

If moss is confused with a green tile:

- reduce moss gloss,
- change moss silhouette,
- place moss clearly under tile,
- adjust leaf tile away from moss color/silhouette.

If blocker is confused with stone tile:

- make blocker larger/anchored to cell,
- reduce shine,
- add root/lock silhouette,
- make stone tile more gem-like or smaller.

If grid is hard to see:

- increase cell slot contrast slightly,
- adjust board panel contrast,
- reduce background detail behind board,
- check scale and sorting order.

If HUD objective is unclear:

- add matching icon,
- simplify copy,
- show remaining count,
- animate only when progress changes.

## Release gate

Board Clarity QA passes only when:

- [ ] all required screenshots exist,
- [ ] all P0 visual states pass,
- [ ] 1-second test passes with at least 3 people,
- [ ] no P0 confusion remains,
- [ ] all failures have assigned action cards.
