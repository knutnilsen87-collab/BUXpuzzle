# Tile Asset Import and Mapping

## Final Asset Paths

Base tile sprites:

- `Assets/Game/Art/Tiles/Base/tile_dew.png`
- `Assets/Game/Art/Tiles/Base/tile_leaf.png`
- `Assets/Game/Art/Tiles/Base/tile_sun.png`
- `Assets/Game/Art/Tiles/Base/tile_berry.png`
- `Assets/Game/Art/Tiles/Base/tile_flower.png`
- `Assets/Game/Art/Tiles/Base/tile_pebble.png`

Special tile sprites:

- `Assets/Game/Art/Tiles/Specials/special_sunbeam_vertical.png`
- `Assets/Game/Art/Tiles/Specials/special_sunbeam_horizontal.png`
- `Assets/Game/Art/Tiles/Specials/special_bloom_bomb.png`
- `Assets/Game/Art/Tiles/Specials/special_sun_orb.png`

Blocker sprites:

- `Assets/Game/Art/Blockers/blocker_moss.png`
- `Assets/Game/Art/Blockers/blocker_vine.png`

The source drop remains in `tiles2/` as untracked raw source material and is not required by runtime.

## Runtime Mapping

The active mapping lives in `Assets/Game/Content/Tiles/TileSet_NatureLight.asset`, using `TileSetConfig`.

Base tile mapping:

- `TileType.A` -> Dew -> `tile_dew.png`
- `TileType.B` -> Leaf -> `tile_leaf.png`
- `TileType.C` -> Sun -> `tile_sun.png`
- `TileType.D` -> Berry -> `tile_berry.png`
- `TileType.E` -> Flower -> `tile_flower.png`
- `TileType.F` -> Pebble -> `tile_pebble.png`

Special visual mapping:

- `TileState.Line` -> `special_sunbeam_horizontal.png`
- Line Vertical -> `special_sunbeam_vertical.png`, prepared in config
- `TileState.Burst` -> `special_bloom_bomb.png`
- `TileState.ColorBomb` -> `special_sun_orb.png`

Blocker visual mapping:

- `CellBlockerType.Moss` -> `blocker_moss.png`
- `CellBlockerType.Vine` -> `blocker_vine.png`

## Architecture

`BoardEngine` remains deterministic and logic-only. It owns board state, tile states, blocker cells, swaps, matches, drops and spawning.

Presentation reads engine state and asks `TileSetConfig` which sprites to show:

- `BoardView` reads tile type/state and cell blocker type from `BoardEngine`.
- `TileView` receives those values and renders base, special and blocker sprites.
- `TileSetConfig` is the only place that maps gameplay enums to Unity art assets.

## Unity Import Settings

`Assets/Editor/BUXPuzzleTileArtImporter.cs` applies consistent settings to sprites under `Assets/Game/Art/Tiles/` and `Assets/Game/Art/Blockers/`:

- Texture Type: Sprite (2D and UI)
- Sprite Mode: Single
- Alpha Is Transparency: On
- Mesh Type: Full Rect
- Filter Mode: Bilinear
- Max Size: 1024
- Pixels Per Unit: 100
- Mip Maps: Off
- Compression: High Quality

If Unity does not reimport automatically, right-click `Assets/Game/Art` and choose `Reimport`.

## Visual Scale

Authored tile sprites are fitted in `TileView` to 92% of the board cell's largest sprite dimension. This keeps oversized source art from covering neighboring cells while making the tiles read as large, tactile match-3 pieces.

## Known Limitations

- The engine currently stores `TileState.Line` without a horizontal/vertical orientation value, so runtime line specials resolve to the horizontal sunbeam overlay. The vertical sunbeam asset is imported and mapped for the future orientation-aware path.
- Moss and Vine are now rendered as blocker overlays from the existing `BoardCell.Blocker` layer. Pebble and Ice blocker sprites are not part of this final asset drop.
- The imported JPG source art was converted to PNG with transparent keyed backgrounds. Manual visual QA should confirm there are no remaining unwanted background pixels around each sprite.
- Screenshots are not committed in this pass; visual QA should be done in the Unity Game view.

## Manual QA Checklist

- Open the `game` scene.
- Run Level 5 or another Moss/Vine blocker level.
- Check 6x6, 7x7 and 8x8 boards if those presets are available.
- Verify all six base tiles are distinguishable at phone scale.
- Verify tiles fill the cell without cropping or overlapping neighbors.
- Verify HUD remains readable.
- Verify Moss and Vine look like blockers rather than normal matchable tiles.
- Verify specials look more powerful than base tiles.
- Verify swap, match, drop and spawn still behave normally.
