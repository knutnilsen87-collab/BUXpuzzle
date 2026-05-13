# Next 30 Actions — Board Clarity

## Week 1 — Make the board readable

1. Verify current `CellSlots` implementation in `BoardView`.
2. Create/import `cell_slot_normal.png`.
3. Tune sorting and scale for cell slots.
4. Add/verify explicit cell state model.
5. Parse `BoardRows` into cell states.
6. Add moss underlay rendering.
7. Add root-stone blocker rendering.
8. Prevent selection/swap of blockers.
9. Make drop/spawn respect blockers/inactive cells.
10. Add invalid blocker tap feedback.

## Week 2 — Make objectives and levels understandable

11. Create/import `hud_goal_moss.png`.
12. Create/import `hud_goal_root_stone.png`.
13. Update HUD objective display for moss/blockers.
14. Create `tutorial_card_moss.png`.
15. Create `tutorial_card_blocker.png`.
16. Add first-time tutorial copy for moss.
17. Add first-time tutorial copy for blockers.
18. Create/update `Level_6.json` for moss intro.
19. Create/update `Level_11.json` for moss progression.
20. Create/update `Level_16.json` for blocker intro.

## Week 3 — Build progression

21. Create Meadow chapter theme.
22. Create Moss Grove chapter theme.
23. Create Root Stones chapter theme.
24. Add theme mapping by `WorldId` or `theme`.
25. Create/update levels 1–10.
26. Create/update levels 11–20.
27. Create/update levels 21–30.
28. Validate all authored levels.
29. Playtest levels 1–10 with fresh player.
30. Run screenshot readability QA for levels 1, 6, 11, 16, 20 and 30.

## Stop conditions

Stop and fix before moving on if:

- moss is still confused with leaf/green tile,
- blocker can be mistaken for a movable tile,
- a player cannot see the grid on a phone screenshot,
- authored level rows silently fail,
- invalid tap feedback feels like a bug instead of a rule.
