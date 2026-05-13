# BUXPuzzle First-Session UX QA

Date: 2026-05-08

## Scope

This checklist covers the first playable minute in `Assets/Scenes/game.unity`:

- first move tutorial
- tap-tap and drag swap input
- invalid swap feedback
- match, clear, drop, spawn, cascade readability
- HUD goal/progress
- small-screen readability

## Manual Verification

Run the scene from a clean local tutorial state or call `TutorialStateStore.ResetForQa()`.

1. First launch shows one highlighted valid move.
2. Tutorial copy says: `Bytt disse to for å lage din første match.`
3. Waiting 2-3 seconds makes the hint more visible.
4. Player can complete the highlighted move manually.
5. Tutorial does not auto-play the move.
6. After the first successful match, copy says: `Bra! Brikker faller ned og kan lage nye matcher.`
7. Tutorial does not show again after completion unless reset for QA.
8. Tap one tile, then an adjacent tile: swap is requested once.
9. Drag left/right/up/down from a tile: swap is requested once.
10. Diagonal drag resolves to the dominant axis.
11. Input is ignored while resolve presentation is running.
12. First invalid swap says: `Bytt to nabobrikker som lager 3 like på rad.`
13. Later invalid swaps say: `Prøv en annen match.`
14. During tutorial, invalid swaps say: `Nesten. Prøv de markerte brikkene.`
15. HUD shows level, goal, moves and progress before first action.
16. HUD progress updates after successful match.
17. Toast feedback does not cover the HUD.
18. Tile types remain distinguishable without relying only on color.
19. Match highlights and clearing do not hide tile identity for longer than the brief clear beat.
20. No critical Unity console errors appear.

## Small-Screen Gate

Minimum simulator fallback if physical devices are unavailable:

- portrait 360 x 640
- portrait 390 x 844
- landscape sanity check at 844 x 390

Pass criteria:

- all HUD text is readable and unclipped
- board remains the dominant visual area
- each tile remains tappable
- drag threshold feels reachable without accidental swaps
- tutorial message fits within the screen
- game can be understood with audio muted
- no excessive flashing

## Device Status

Physical iPhone and Android device testing remains unresolved until devices are available.

Required before store RC:

- iPhone small-screen touch pass
- Android small/mid-screen touch pass
- battery/performance sanity pass
- no blocking runtime crashes during one full first session

## Visual Satisfaction Addendum Gate

Run after the first-session checks above:

1. HUD is in the top safe-area row and does not overlap any tile.
2. Top board row is fully visible with 12-24 px breathing room below the HUD.
3. Board remains centered in the remaining viewport in portrait and editor free aspect.
4. Tile select gives a small scale/glow response and soft select audio.
5. Valid swap slides before match highlight/clear/drop/spawn/cascade.
6. Invalid swap uses mild motion, soft audio, and helpful copy.
7. Match, clear, drop, landing, and cascade moments are readable without becoming noisy.
8. Progress bar animates with a small glow sweep and soft progress audio.
9. Mission completion waits until the final resolve finishes, then shows `Mission accomplished`.
10. `Neste level` advances to a fresh board with updated goal/moves.
11. `Spill igjen` restarts the current level without leaking old progress.
12. SFX can be disabled via `GameSettings.SfxEnabled`; haptics can be disabled via `GameSettings.HapticsEnabled`.
