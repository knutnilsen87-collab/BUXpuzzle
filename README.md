# BUXPuzzle

BUXPuzzle is the active Unity project for the CandyCrushLab match-3 game.

## Active Project

- Unity version: `6000.3.3f1`
- Project root: `BUXPuzzle/`
- Main scene: `Assets/Scenes/game.unity`
- Theme direction: Nature Light

The parent `candycrush` folder contains recovery material, old mirrors, logs, and handoff bundles. This repository intentionally tracks only the active Unity project line.

## Current Product Direction

The target Definition of Done is a mobile release candidate that can be submitted to Apple App Store and Google Play Store. See `RELEASE_DOD.md`.

## Key Documentation

- `docs/style-guide/Nature_Light_Production_Style_Guide_v2.md`
- `docs/implementation-spec/Nature_Light_Unity_Implementation_Spec.md`
- `docs/runtime/Determinism_Contract.md`
- `docs/runtime/Board_Redraw_Contract.md`

## Runtime Notes

The project currently contains a playable match-3 runtime path:

- `GameRoot` initializes the board runtime.
- `BoardEngine` owns deterministic gameplay state.
- `BoardView` renders the authoritative board.
- `TileInput` routes input into validated swaps.
- Nature Light presentation and placeholder audio assets are present.

Before release, placeholder assets, mobile signing, device verification, store compliance, and release candidate testing still need to be completed.
