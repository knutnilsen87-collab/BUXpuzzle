# Board Redraw Contract

## Formål
Beskriver kontrakten mellom BoardEngine, GameRoot og BoardView.

## Kontrakt
Swap -> ResolveSummary -> GameRoot -> BoardView.DrawOrRedrawFromEngine()

## Regler
- BoardView må aldri eie autoritativ state
- DrawOrRedrawFromEngine() må lese direkte fra engine
- Invalid swap skal ikke gi falsk redraw

## Failure modes
- stale view state
- redraw uten engine-endring
- missing TilePrefab
- view/engine desync
