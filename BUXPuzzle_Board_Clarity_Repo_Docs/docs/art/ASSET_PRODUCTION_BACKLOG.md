# Asset Production Backlog

This is the complete practical list of visual/audio/content assets that must be created for the Board Clarity Milestone.

Use existing repo structure when possible. The current repo already has:

```text
Assets/Game/Presentation/NatureLight/
Assets/Game/Presentation/NatureLight/Sprites/
Assets/Game/Presentation/NatureLight/Atlas/
Assets/Game/Content/Levels/
Assets/Game/Audio/
```

## Naming and import rules

- Use lowercase snake_case filenames.
- Prefer transparent PNG for sprites.
- Keep source working files outside Unity or in a clearly named source folder if committed.
- Imported Unity sprites should have consistent pivot: center.
- Keep gameplay-state sprites readable at small size.
- Add new sprite references to `Assets/Game/Presentation/NatureLight/NatureLightArtManifest.json` or to the relevant ScriptableObject/config.

## Folder structure to create

Create these folders if they do not exist:

```text
Assets/Game/Presentation/NatureLight/Sprites/Board/
Assets/Game/Presentation/NatureLight/Sprites/Backgrounds/
Assets/Game/Presentation/NatureLight/Sprites/HUD/
Assets/Game/Presentation/NatureLight/Sprites/Tutorial/
Assets/Game/Presentation/NatureLight/Sprites/VFX/
Assets/Game/Presentation/NatureLight/Sprites/Tiles/
Assets/Game/Presentation/NatureLight/Atlas/
Assets/Game/Audio/SFX/
Assets/Game/Content/Levels/
Assets/Game/Config/BoardThemes/
```

## A. Board and cell sprites

### Required for milestone

| Asset | File path | Purpose | Priority |
|---|---|---:|---:|
| Normal cell slot | `Assets/Game/Presentation/NatureLight/Sprites/Board/cell_slot_normal.png` | Makes grid readable | P0 |
| Inactive cell mask/hole | `Assets/Game/Presentation/NatureLight/Sprites/Board/cell_slot_inactive.png` | Shows absent/non-playable cells if needed | P1 |
| Moss underlay 01 | `Assets/Game/Presentation/NatureLight/Sprites/Board/cell_state_moss_underlay_01.png` | Moss objective cell | P0 |
| Moss underlay 02 | `Assets/Game/Presentation/NatureLight/Sprites/Board/cell_state_moss_underlay_02.png` | Visual variation | P1 |
| Moss clearing frame | `Assets/Game/Presentation/NatureLight/Sprites/Board/cell_state_moss_underlay_clearing.png` | Clear feedback | P1 |
| Root-stone blocker | `Assets/Game/Presentation/NatureLight/Sprites/Board/cell_state_blocked_root_stone.png` | Non-movable blocker | P0 |
| Root-stone hit | `Assets/Game/Presentation/NatureLight/Sprites/Board/cell_state_blocked_root_stone_hit.png` | Invalid tap feedback | P1 |
| Selected tile ring | `Assets/Game/Presentation/NatureLight/Sprites/Board/tile_selected_ring.png` | Selected tile clarity | P0 |
| Hint pulse | `Assets/Game/Presentation/NatureLight/Sprites/Board/tile_hint_pulse.png` | Valid move hint | P1 |
| Invalid tap flash | `Assets/Game/Presentation/NatureLight/Sprites/Board/cell_invalid_tap_flash.png` | Tap blocker feedback | P1 |

### Later

| Asset | File path | Purpose | Priority |
|---|---|---:|---:|
| Vine underlay | `Assets/Game/Presentation/NatureLight/Sprites/Board/cell_state_vine_underlay.png` | Vine mechanic | P2 |
| Dew/ice overlay | `Assets/Game/Presentation/NatureLight/Sprites/Board/cell_state_dew_overlay.png` | Dew mechanic | P2 |
| Goal cell glow | `Assets/Game/Presentation/NatureLight/Sprites/Board/cell_state_goal_glow.png` | Special objective cell | P2 |

## B. Board panels / frames

| Asset | File path | Use | Priority |
|---|---|---:|---:|
| Meadow board panel | `Assets/Game/Presentation/NatureLight/Sprites/Board/board_panel_meadow.png` | Levels 1–5 | P0 |
| Moss Grove board panel | `Assets/Game/Presentation/NatureLight/Sprites/Board/board_panel_moss_grove.png` | Levels 6–15 | P0 |
| Root Stones board panel | `Assets/Game/Presentation/NatureLight/Sprites/Board/board_panel_root_stones.png` | Levels 16–30 | P1 |
| Dew Garden board panel | `Assets/Game/Presentation/NatureLight/Sprites/Board/board_panel_dew_garden.png` | Levels 31–50 | P2 |
| Moonlit board panel | `Assets/Game/Presentation/NatureLight/Sprites/Board/board_panel_moonlit_glade.png` | Levels 51+ | P3 |

## C. Backgrounds

Recommended source size: 2048x2048 or larger. Keep focal detail away from the board center.

| Asset | File path | Use | Priority |
|---|---|---:|---:|
| Meadow background | `Assets/Game/Presentation/NatureLight/Sprites/Backgrounds/bg_meadow_calm.png` | Chapter 1 | P0 |
| Moss Grove background | `Assets/Game/Presentation/NatureLight/Sprites/Backgrounds/bg_moss_grove.png` | Chapter 2 | P0 |
| Root Stones background | `Assets/Game/Presentation/NatureLight/Sprites/Backgrounds/bg_root_stones.png` | Chapter 3 | P1 |
| Dew Garden background | `Assets/Game/Presentation/NatureLight/Sprites/Backgrounds/bg_dew_garden.png` | Chapter 4 | P2 |
| Moonlit Glade background | `Assets/Game/Presentation/NatureLight/Sprites/Backgrounds/bg_moonlit_glade.png` | Chapter 5 | P3 |

## D. Tile assets

The existing manifest already references these tile concepts: Leaf, Flower, Berry, Sun, Water, Stone.

Create or replace these under:

```text
Assets/Game/Presentation/NatureLight/Sprites/Tiles/
```

Required per tile kind:

```text
tile_leaf_base.png
tile_leaf_highlight.png
tile_leaf_symbol.png

tile_flower_base.png
tile_flower_highlight.png
tile_flower_symbol.png

tile_berry_base.png
tile_berry_highlight.png
tile_berry_symbol.png

tile_sun_base.png
tile_sun_highlight.png
tile_sun_symbol.png

tile_water_base.png
tile_water_highlight.png
tile_water_symbol.png

tile_stone_base.png
tile_stone_highlight.png
tile_stone_symbol.png
```

Rules:

- Each tile must be identifiable by silhouette and symbol, not color alone.
- Leaf tile must not be visually confused with moss.
- Stone tile must not be visually confused with root-stone blocker.
- Highlight must not cover symbol.

## E. HUD assets

Place under:

```text
Assets/Game/Presentation/NatureLight/Sprites/HUD/
```

Required:

| Asset | File path | Purpose | Priority |
|---|---|---:|---:|
| Moss goal icon | `hud_goal_moss.png` | Matches moss underlay | P0 |
| Blocker goal icon | `hud_goal_root_stone.png` | Matches blocker | P0 |
| Move counter leaf | `hud_icon_moves.png` | Clear moves left display | P1 |
| Score icon | `hud_icon_score.png` | Existing score clarity | P1 |
| Chapter badge meadow | `hud_chapter_badge_meadow.png` | Progression | P2 |
| Chapter badge moss | `hud_chapter_badge_moss_grove.png` | Progression | P2 |
| Chapter badge root | `hud_chapter_badge_root_stones.png` | Progression | P2 |

## F. Tutorial assets

Place under:

```text
Assets/Game/Presentation/NatureLight/Sprites/Tutorial/
```

Required:

| Asset | File path | Purpose | Priority |
|---|---|---:|---:|
| Hand swipe cue | `tutorial_swipe_cue.png` | First move teaching | P1 |
| Moss explanation card | `tutorial_card_moss.png` | Level 6/11 moss explanation | P0 |
| Blocker explanation card | `tutorial_card_blocker.png` | First root-stone explanation | P0 |
| Valid swap marker | `tutorial_valid_swap_marker.png` | Onboarding hint | P1 |

## G. VFX sprites

Place under:

```text
Assets/Game/Presentation/NatureLight/Sprites/VFX/
```

Required:

| Asset | File path | Purpose | Priority |
|---|---|---:|---:|
| Moss clear particles | `vfx_moss_clear_particles.png` | Clear moss feedback | P1 |
| Match sparkle | `vfx_match_sparkle.png` | Match feedback | P1 |
| Blocker thud puff | `vfx_blocker_thud_puff.png` | Invalid tap / hit | P1 |
| Chapter transition mist | `vfx_chapter_transition_mist.png` | Level progression polish | P3 |

## H. Audio assets

Place under:

```text
Assets/Game/Audio/SFX/
```

Required:

| Asset | File path | Purpose | Priority |
|---|---|---:|---:|
| Valid swap | `sfx_swap_valid.wav` | Positive interaction | P1 |
| Invalid swap | `sfx_swap_invalid_soft.wav` | Non-punitive feedback | P1 |
| Blocker thud | `sfx_blocker_thud.wav` | “Cannot move” signal | P0 |
| Moss clear | `sfx_moss_clear.wav` | Objective progress | P0 |
| Level complete | `sfx_level_complete_soft.wav` | Completion reward | P1 |

## I. Level content to create

Create or update:

```text
Assets/Game/Content/Levels/Level_1.json
Assets/Game/Content/Levels/Level_2.json
...
Assets/Game/Content/Levels/Level_30.json
```

Priority authored levels:

```text
Level_1.json
Level_2.json
Level_3.json
Level_4.json
Level_5.json
Level_6.json
Level_10.json
Level_11.json
Level_12.json
Level_15.json
Level_16.json
Level_20.json
Level_25.json
Level_30.json
```

Each authored level should include:

- level id,
- world id,
- width,
- height,
- move limit,
- goals,
- board rows,
- new mechanic,
- designer note,
- force win bias where appropriate.

## J. Config assets / ScriptableObjects

Create these in Unity if the code path supports or after Codex adds support:

```text
Assets/Game/Config/BoardThemes/BoardTheme_Meadow.asset
Assets/Game/Config/BoardThemes/BoardTheme_MossGrove.asset
Assets/Game/Config/BoardThemes/BoardTheme_RootStones.asset
Assets/Game/Config/BoardThemes/BoardTheme_DewGarden.asset
Assets/Game/Config/BoardThemes/BoardTheme_MoonlitGlade.asset
```

Each theme should map:

- background sprite,
- board panel sprite,
- normal cell slot sprite,
- moss underlay sprite,
- blocker sprite,
- HUD accent,
- optional ambient color.

## K. Manifest update

Update:

```text
Assets/Game/Presentation/NatureLight/NatureLightArtManifest.json
```

Add sections for:

```json
{
  "board": {
    "cell_slot_normal": "Sprites/Board/cell_slot_normal.png",
    "moss_underlay": "Sprites/Board/cell_state_moss_underlay_01.png",
    "root_stone_blocker": "Sprites/Board/cell_state_blocked_root_stone.png"
  },
  "backgrounds": {
    "meadow": "Sprites/Backgrounds/bg_meadow_calm.png",
    "moss_grove": "Sprites/Backgrounds/bg_moss_grove.png",
    "root_stones": "Sprites/Backgrounds/bg_root_stones.png"
  },
  "hud": {
    "goal_moss": "Sprites/HUD/hud_goal_moss.png",
    "goal_blocker": "Sprites/HUD/hud_goal_root_stone.png"
  }
}
```

## Completion checklist

- [ ] Board sprites created.
- [ ] Backgrounds created for at least Meadow, Moss Grove and Root Stones.
- [ ] Moss underlay cannot be confused with leaf tile.
- [ ] Root-stone blocker cannot be confused with stone tile.
- [ ] HUD goal icon matches board visual.
- [ ] Assets imported as Unity sprites.
- [ ] Assets referenced in config/manifest.
- [ ] Screenshots approved in QA checklist.
