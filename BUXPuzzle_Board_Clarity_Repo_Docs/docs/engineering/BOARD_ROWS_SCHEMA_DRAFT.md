# BoardRows Schema Draft

## Purpose

Define a stable, human-authored board layout format for levels.

## Current direction

Use `BoardRows` on `LevelSpec` or JSON `boardRows` in `Assets/Game/Content/Levels/*.json`.

## Characters

| Char | Meaning | Layer | Blocks movement? | Has tile? |
|---|---|---|---:|---:|
| `.` | Normal active cell | Cell | No | Yes |
| `m` | Moss active cell | Cell underlay/objective | No | Yes |
| `#` | Root-stone blocked cell | Cell blocker | Yes | No |
| `x` | Inactive/hole cell | Cell absence | Yes | No |
| `v` | Vine cell | Cell underlay/objective | Maybe | Yes |
| `d` | Dew/ice cell | Cell overlay | Maybe | Yes |

## JSON example

```json
{
  "worldId": 2,
  "levelId": 6,
  "width": 6,
  "height": 6,
  "moveLimit": 12,
  "goals": [{ "type": "ClearBlockers", "target": 4 }],
  "newMechanic": "Moss",
  "theme": "MossGrove",
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

## Validation rules

- `boardRows.Length == height`
- every row length equals `width`
- unsupported characters fail with clear error
- if `boardRows` is missing, generate all normal active cells
- if target count does not match board rows, log warning or fail depending on build mode

## Authoring rule

Rows should be written top-to-bottom as seen by the designer. If engine uses bottom-left origin, parser must convert consistently and document it.
