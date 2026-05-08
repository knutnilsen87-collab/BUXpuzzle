# Determinism Contract

## Formål
Definerer hva som må være deterministisk i runtime.

## Kjernekrav
- Samme seed + samme move-sequence => samme startboard, resolve-sekvens og sluttboard
- BoardEngine er single source of truth
- Presentation kan aldri påvirke runtime-resultat

## Inndata
- seed
- scenario_id
- ruleset_id
- engine_version
- move_sequence

## Output
- start board
- resolve steps
- final board
- resolve summary
