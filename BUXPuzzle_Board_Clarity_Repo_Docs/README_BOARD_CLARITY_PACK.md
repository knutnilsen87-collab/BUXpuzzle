# BUXPuzzle Board Clarity Pack

Dette dokumentpakken er laget for å legges direkte inn i `BUXPuzzle`-repoet og gi utvikler/Codex en tydelig, gjennomførbar plan for å fikse brettlesbarhet, blockers, moss, rutenett, level-progresjon og Nature Light-konsistens.

## Hva pakken løser

Skjermbildet viser at spilleren ikke raskt nok kan skille mellom:

- flyttbar tile
- moss/mål-underlag
- blokkert eller ikke-flyttbar celle
- vanlig celle
- brettflate
- verdensbakgrunn

Det gjør at spillet virker mer uklart enn spillmekanikken egentlig er.

## Viktig status fra repoet

Repoet har allerede flere relevante byggesteiner:

- `Assets/Game/Presentation/BoardView.cs` har allerede støtte for board surface og `CellSlots`.
- `Assets/Game/Levels/LevelSpec.cs` har allerede felter som `WorldId`, `ObjectiveType`, `NewMechanic` og `BoardRows`.
- `Assets/Game/Content/Levels/` inneholder allerede JSON-baserte onboarding-levels.
- `Assets/Game/Presentation/NatureLight/NatureLightArtManifest.json` beskriver en placeholder-ready Nature Light art pipeline.
- `Assets/Game/Presentation/NatureLight/Sprites/` og `Atlas/` finnes allerede som naturlige steder for authored art.

Målet er derfor ikke å bygge alt fra null. Målet er å gjøre state-modellen, visual grammar, assets, levels og QA tydelige nok til at spillet føles ferdig og lesbart.

## Foreslått plassering i repoet

Kopier filene i denne pakken til samme relative paths i repoet.

```text
AGENTS.md
docs/product/BOARD_CLARITY_MILESTONE.md
docs/design/BOARD_VISUAL_GRAMMAR.md
docs/art/ASSET_PRODUCTION_BACKLOG.md
docs/levels/FIRST_30_LEVELS_PLAN.md
docs/engineering/CODEX_BOARD_CLARITY_IMPLEMENTATION_PLAN.md
docs/tasks/BOARD_CLARITY_ACTION_CARDS.md
docs/tasks/NEXT_30_ACTIONS_BOARD_CLARITY.md
docs/qa/BOARD_READABILITY_QA_CHECKLIST.md
docs/prompts/CODEX_PROMPT_BOARD_CLARITY.md
```

## Anbefalt arbeidsrekkefølge

1. Les `AGENTS.md`.
2. Les `docs/product/BOARD_CLARITY_MILESTONE.md`.
3. Kjør Codex med `docs/prompts/CODEX_PROMPT_BOARD_CLARITY.md`.
4. Implementer først action cards BC-01 til BC-05.
5. Lag assets fra `docs/art/ASSET_PRODUCTION_BACKLOG.md`.
6. Lag/oppdater levels fra `docs/levels/FIRST_30_LEVELS_PLAN.md`.
7. Kjør QA-listen i `docs/qa/BOARD_READABILITY_QA_CHECKLIST.md`.

## Ikke gjør dette

- Ikke legg inn flere levels før brettets visuelle grammatikk er tydelig.
- Ikke bruk én grønn tile-lignende sprite både som moss, blocker og vanlig brikke.
- Ikke la presentation-laget ha egen gameplay-sannhet.
- Ikke introduser nye mechanics uten tutorial, måltekst, HUD-ikon og QA-screenshot.
