# BUXPuzzle Audio Implementation

Date: 2026-05-11

## Final asset paths

Source handoff files were copied from the repository root `Audio/` folder into:

```text
Assets/Audio/
```

The runtime config asset is:

```text
Assets/Resources/Audio/BUXPuzzleAudioConfig.asset
```

The config lives under `Resources` so builds can load one data asset and include all referenced clips without hardcoding sprite/audio references into gameplay logic.

## Runtime architecture

`BoardEngine` remains logic-only and has no audio references.

Runtime audio is owned by:

```text
Assets/Game/Audio/GameAudioController.cs
Assets/Game/Audio/BUXPuzzleAudioConfig.cs
Assets/Game/Audio/AudioEvent.cs
```

`GameAudioController` supports:

- one-shot SFX by `AudioEvent`
- looping music by `MusicTrack`
- looping ambience by `AmbienceTrack`
- separate Board/UI/Reward/Invalid SFX sources
- music crossfade
- ambience fade
- SFX cooldowns and max voice limits
- pitch variation for repeated SFX
- separate SFX, Music and Ambience volume settings
- editor-only warnings for missing clips

## Mapping source

`Assets/Editor/BUXPuzzleAudioConfigBuilder.cs` rebuilds the mapping from enum values to files in `Assets/Audio`.

Run:

```text
BUXPuzzle/Audio/Rebuild Audio Config
```

or batchmode:

```text
-executeMethod Game.EditorTools.BUXPuzzleAudioValidation.RunAndExit
```

## Gameplay wiring

Current wiring:

- tile select/deselect: `TileSelect`, `TileDeselect`
- valid swap animation start: `SwapValidStart`
- accepted swap settle: `SwapAcceptedSettle`
- rejected swap: `SwapInvalid`
- match size: `Match3`, `Match4`, `Match5`
- clear size: `ClearSmall`, `ClearMedium`, `ClearLarge`
- drop distance: `TileFallShort`, `TileFallLong`
- grouped landing: `TileLandSingle`, `TileLandCluster`
- cascade chain: `Cascade1`, `Cascade2`, `Cascade3Plus`
- objective progress: `GoalProgress`
- win/fail: `LevelComplete`, `SessionFail`
- result overlay/buttons: `UIOverlayOpen`, `UIOverlayClose`, `UIButtonTap`
- tutorial: `TutorialHint`, `HintRepeat`
- specials: `SpecialTrigger`, `SpecialCombine`, `SpecialCreated`

Music starts from `GameRoot`:

- levels 1-3: `RelaxedMenusEasyLevels`
- levels 4-9: `MainGameplay`
- level 10+: `DeeperFocusLaterLevels`
- ambience: `NatureLightMorning`

## Import settings

`Assets/Editor/BUXPuzzleAudioAssetImporter.cs` configures files under `Assets/Audio`:

- SFX: Decompress On Load, Vorbis, preload on, force mono
- Music/Ambience: Streaming, Vorbis, preload off, stereo allowed

## Validation

Audio-specific validation:

```text
Game.EditorTools.BUXPuzzleAudioValidation.RunAndExit
```

Report:

```text
Logs/codex_audio_validation.json
```

Release validation also checks that the final audio files and config asset exist.

## Known limitations

- AudioMixer group assets are not authored yet. Runtime currently routes through named AudioSources and optional serialized `AudioMixerGroup` fields.
- There is not yet an in-game settings menu for changing SFX/Music/Ambience volume, but the controller and `GameSettings` support the separate values.
- Physical Android/iOS audio playback still needs device QA.
