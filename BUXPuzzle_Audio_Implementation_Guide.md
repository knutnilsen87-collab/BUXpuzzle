# BUXPuzzle Audio Implementation Guide

## Purpose

This document explains what each audio file in the `Audio` folder is for, where it should be used, and when it should be triggered in the game.

The audio direction for BUXPuzzle is:

```text
calm nature puzzle
soft tactile feedback
dew, leaves, moss, glass beads, light, petals, warm wood
premium mobile match-3
```

BUXPuzzle should **not** sound like an aggressive candy/casino game. Audio should support clarity, feedback, comfort, and long-session playability.

---

## Audio folder

All audio files have been placed in:

```text
F:\prosjekter\candycrush\BUXPuzzle\Audio
```

Recommended Unity project path:

```text
Assets/Audio
```

If the current folder is outside `Assets`, move or copy the files into `Assets/Audio` before wiring them in Unity.

---

## Recommended rename for background music files

The current folder includes three background music files with human-readable names. Rename them before wiring so Codex/developers can map them cleanly.

```text
Main background music.mp3
→ mus_background_main_gameplay.mp3

Deeper focus _ later levels.mp3
→ mus_background_deeper_focus_later_levels.mp3

Relaxed ambient _ menus and easy levels.mp3
→ mus_background_relaxed_menus_easy_levels.mp3
```

---

## Audio principles

Use audio to explain gameplay:

```text
tap → select
drag → swap
match → clear
fall → land
cascade → reward
goal progress → progress
level complete → success
```

Rules:

- Short, soft, clear sounds.
- No casino coins.
- No aggressive reward explosions.
- No harsh buzzers.
- No cartoon boings.
- No sound spam during cascades.
- Do not play one tile landing sound per tile without grouping.
- Use music and ambience as emotional support, not as the main attention grabber.

---

# Recommended Unity audio structure

## AudioMixer groups

Create these AudioMixer groups:

```text
Master
Music
Ambience
BoardSFX
UISFX
RewardSFX
InvalidSFX
```

## Routing

```text
Music:
- mus_background_main_gameplay.mp3
- mus_background_deeper_focus_later_levels.mp3
- mus_background_relaxed_menus_easy_levels.mp3
- mus_puzzle_calm_loop_a.mp3
- mus_puzzle_focus_loop_b.mp3
- mus_layer_cascade_sparkle.mp3
- mus_stinger_level_complete.mp3

Ambience:
- amb_nature_light_morning_loop.mp3

BoardSFX:
- sfx_tile_select_leaf_tap.mp3
- sfx_tile_deselect_soft_release.mp3
- sfx_swap_valid_silk_slide.mp3
- sfx_swap_accept_warm_click.mp3
- sfx_swap_invalid_gentle_wood.mp3
- sfx_match3_dew_pop.mp3
- sfx_match4_petal_chime.mp3
- sfx_match5_light_bloom.mp3
- sfx_clear_small_pollen.mp3
- sfx_clear_medium_dew_sparkle.mp3
- sfx_clear_large_light_ring.mp3
- sfx_tile_fall_short_soft_air.mp3
- sfx_tile_fall_long_leaf_sweep.mp3
- sfx_tile_land_pebble_soft.mp3
- sfx_tile_land_cluster_moss.mp3
- sfx_cascade_1_seed_glow.mp3
- sfx_cascade_2_leaf_harmony.mp3
- sfx_cascade_3plus_sunbeam.mp3

UISFX:
- sfx_ui_button_tap_woodglass.mp3
- sfx_ui_overlay_open_air.mp3
- sfx_ui_overlay_close_air.mp3
- sfx_hint_soft_firefly.mp3
- sfx_hint_repeat_subtle.mp3

RewardSFX:
- sfx_goal_progress_warm_tick.mp3
- sfx_level_complete_morning_bloom.mp3
- sfx_special_charge_dewline.mp3
- sfx_special_trigger_lightwave.mp3
- sfx_special_created_seed_bloom.mp3
- sfx_special_combine_light_merge.mp3

InvalidSFX:
- sfx_swap_invalid_gentle_wood.mp3
- sfx_session_fail_sunset_soft.mp3
- sfx_session_fail_sunset_soft_alt.mp3
```

---

# Required AudioEvent enum

Update or create the gameplay audio enum:

```csharp
public enum AudioEvent
{
    TileSelect,
    TileDeselect,

    SwapValidStart,
    SwapAcceptedSettle,
    SwapInvalid,

    Match3,
    Match4,
    Match5,

    ClearSmall,
    ClearMedium,
    ClearLarge,

    TileFallShort,
    TileFallLong,
    TileLandSingle,
    TileLandCluster,

    Cascade1,
    Cascade2,
    Cascade3Plus,

    GoalProgress,
    LevelComplete,
    SessionFail,

    UIButtonTap,
    UIOverlayOpen,
    UIOverlayClose,

    TutorialHint,
    HintRepeat,

    SpecialCharge,
    SpecialTrigger,
    SpecialCreated,
    SpecialCombine
}
```

---

# MusicTrack enum

```csharp
public enum MusicTrack
{
    RelaxedMenusEasyLevels,
    MainGameplay,
    DeeperFocusLaterLevels,
    PuzzleCalmLoopA,
    PuzzleFocusLoopB,
    CascadeSparkleLayer,
    LevelCompleteStinger
}
```

---

# AmbienceTrack enum

```csharp
public enum AmbienceTrack
{
    NatureLightMorning
}
```

---

# Exact file-to-event mapping

## Gameplay input

### TileSelect

```text
File:
sfx_tile_select_leaf_tap.mp3

Use when:
Player taps/selects a tile.

Where:
TileInput / BoardView selection logic.

Trigger:
Immediately on first tile selection.

Mixer group:
BoardSFX
```

### TileDeselect

```text
File:
sfx_tile_deselect_soft_release.mp3

Use when:
Player cancels selection or taps another non-swappable tile.

Where:
TileInput / BoardView selection logic.

Mixer group:
BoardSFX
```

### SwapValidStart

```text
File:
sfx_swap_valid_silk_slide.mp3

Use when:
Player starts a valid adjacent swap.

Where:
BoardView before or while swap animation begins.

Do not use:
Invalid swaps.

Mixer group:
BoardSFX
```

### SwapAcceptedSettle

```text
File:
sfx_swap_accept_warm_click.mp3

Use when:
Engine confirms that a swap produced a valid match.

Where:
After BoardEngine accepts the swap / after swap animation settles.

Mixer group:
BoardSFX
```

### SwapInvalid

```text
File:
sfx_swap_invalid_gentle_wood.mp3

Use when:
Player attempts an invalid swap or a swap produces no match.

Where:
BoardView rejected swap flow.

Important:
This sound must feel gentle, not punishing.

Mixer group:
InvalidSFX
```

---

## Match and clear

### Match3

```text
File:
sfx_match3_dew_pop.mp3

Use when:
A match of exactly 3 tiles is detected.

Where:
BoardResolveAnimator / BoardView resolve flow.

Mixer group:
BoardSFX
```

### Match4

```text
File:
sfx_match4_petal_chime.mp3

Use when:
A match of 4 tiles is detected.

Where:
BoardResolveAnimator / BoardView resolve flow.

Mixer group:
BoardSFX
```

### Match5

```text
File:
sfx_match5_light_bloom.mp3

Use when:
A match of 5+ tiles is detected.

Where:
BoardResolveAnimator / BoardView resolve flow.

Mixer group:
BoardSFX / RewardSFX
```

### ClearSmall

```text
File:
sfx_clear_small_pollen.mp3

Use when:
Small clear, usually 3–4 tiles.

Where:
During clear animation.

Mixer group:
BoardSFX
```

### ClearMedium

```text
File:
sfx_clear_medium_dew_sparkle.mp3

Use when:
Medium clear, usually 5–8 tiles.

Where:
During clear animation.

Mixer group:
BoardSFX
```

### ClearLarge

```text
File:
sfx_clear_large_light_ring.mp3

Use when:
Large clear, combo clear, or special clear.

Where:
During big clear animation.

Mixer group:
RewardSFX
```

Recommended clear logic:

```csharp
if (clearedTileCount <= 4)
    audio.Play(AudioEvent.ClearSmall);
else if (clearedTileCount <= 8)
    audio.Play(AudioEvent.ClearMedium);
else
    audio.Play(AudioEvent.ClearLarge);
```

---

## Falling and landing

### TileFallShort

```text
File:
sfx_tile_fall_short_soft_air.mp3

Use when:
Tiles fall 1–2 rows.

Where:
Drop animation.

Mixer group:
BoardSFX
```

### TileFallLong

```text
File:
sfx_tile_fall_long_leaf_sweep.mp3

Use when:
Tiles fall 3+ rows.

Where:
Drop animation.

Mixer group:
BoardSFX
```

### TileLandSingle

```text
File:
sfx_tile_land_pebble_soft.mp3

Use when:
One or very few tiles land.

Where:
End of drop animation.

Mixer group:
BoardSFX
```

### TileLandCluster

```text
File:
sfx_tile_land_cluster_moss.mp3

Use when:
Multiple tiles land after a clear/cascade.

Where:
End of drop animation.

Mixer group:
BoardSFX
```

Important:

```text
Do not play one landing sound per tile without limiting.
Use TileLandCluster for grouped landings.
```

Recommended landing logic:

```csharp
if (landingTileCount <= 2)
    audio.Play(AudioEvent.TileLandSingle);
else
    audio.Play(AudioEvent.TileLandCluster);
```

---

## Cascades

### Cascade1

```text
File:
sfx_cascade_1_seed_glow.mp3

Use when:
First cascade occurs after initial match.

Where:
Resolve loop, cascade index 1.

Mixer group:
RewardSFX
```

### Cascade2

```text
File:
sfx_cascade_2_leaf_harmony.mp3

Use when:
Second cascade occurs.

Where:
Resolve loop, cascade index 2.

Mixer group:
RewardSFX
```

### Cascade3Plus

```text
File:
sfx_cascade_3plus_sunbeam.mp3

Use when:
Third or higher cascade occurs.

Where:
Resolve loop, cascade index >= 3.

Mixer group:
RewardSFX
```

Recommended cascade logic:

```csharp
if (cascadeIndex == 1)
    audio.Play(AudioEvent.Cascade1);
else if (cascadeIndex == 2)
    audio.Play(AudioEvent.Cascade2);
else if (cascadeIndex >= 3)
    audio.Play(AudioEvent.Cascade3Plus);
```

Do not stack cascade sounds too tightly. Use cooldown around `0.15–0.25s`.

---

## Goal and level state

### GoalProgress

```text
File:
sfx_goal_progress_warm_tick.mp3

Use when:
Player progresses toward a level objective.

Where:
HUD / goal tracker update.

Mixer group:
RewardSFX
```

### LevelComplete

```text
File:
sfx_level_complete_morning_bloom.mp3

Use when:
Level is completed successfully.

Where:
Level complete flow before or during win overlay.

Mixer group:
RewardSFX
```

### SessionFail

```text
File:
sfx_session_fail_sunset_soft.mp3

Use when:
Player fails a level or runs out of moves.

Where:
Fail state overlay.

Mixer group:
InvalidSFX
```

### SessionFail alternate

```text
File:
sfx_session_fail_sunset_soft_alt.mp3

Use when:
Optional alternate fail variant.

Important:
Do not play both session fail files at the same time.
```

---

## UI sounds

### UIButtonTap

```text
File:
sfx_ui_button_tap_woodglass.mp3

Use when:
Player taps normal UI buttons.

Where:
Buttons, menus, settings, next level, retry.

Mixer group:
UISFX
```

### UIOverlayOpen

```text
File:
sfx_ui_overlay_open_air.mp3

Use when:
Modal, pause, settings, win/fail overlay opens.

Where:
UI overlay controller.

Mixer group:
UISFX
```

### UIOverlayClose

```text
File:
sfx_ui_overlay_close_air.mp3

Use when:
Modal, pause, settings, win/fail overlay closes.

Where:
UI overlay controller.

Mixer group:
UISFX
```

### TutorialHint

```text
File:
sfx_hint_soft_firefly.mp3

Use when:
First tutorial hint appears.

Where:
Onboarding / hint system.

Mixer group:
UISFX
```

### HintRepeat

```text
File:
sfx_hint_repeat_subtle.mp3

Use when:
Hint repeats after inactivity.

Where:
Hint system.

Important:
Should be quieter and less frequent than TutorialHint.

Mixer group:
UISFX
```

---

## Special tile sounds

### SpecialCharge

```text
File:
sfx_special_charge_dewline.mp3

Use when:
Special tile is charging, preparing, or about to activate.

Where:
Special tile animation.

Mixer group:
RewardSFX
```

### SpecialTrigger

```text
File:
sfx_special_trigger_lightwave.mp3

Use when:
Special tile activates.

Where:
Special tile effect execution.

Mixer group:
RewardSFX
```

### SpecialCreated

```text
File:
sfx_special_created_seed_bloom.mp3

Use when:
Player creates a special tile from match 4/5 or other rule.

Where:
Match resolution / special tile creation.

Mixer group:
RewardSFX
```

### SpecialCombine

```text
File:
sfx_special_combine_light_merge.mp3

Use when:
Two special tiles are combined.

Where:
Special combo resolution.

Mixer group:
RewardSFX
```

---

# Background music usage

Use music as states. Do not play all background music tracks at once.

## Main gameplay music

```text
File:
mus_background_main_gameplay.mp3

Original file:
Main background music.mp3

Use when:
Normal gameplay.

Where:
Main board scene.

Loop:
Yes.

Mixer group:
Music
```

This should be the default background track for most levels.

---

## Deeper focus / later levels

```text
File:
mus_background_deeper_focus_later_levels.mp3

Original file:
Deeper focus _ later levels.mp3

Use when:
Later levels, harder levels, or more focused puzzle states.

Where:
Level index threshold, difficulty tier, or world progression.

Loop:
Yes.

Mixer group:
Music
```

Recommended trigger:

```csharp
if (levelIndex >= 10)
    music.Play(MusicTrack.DeeperFocusLaterLevels);
else
    music.Play(MusicTrack.MainGameplay);
```

---

## Relaxed ambient / menus and easy levels

```text
File:
mus_background_relaxed_menus_easy_levels.mp3

Original file:
Relaxed ambient _ menus and easy levels.mp3

Use when:
Main menu, level select, early levels, relaxed state.

Where:
Menu scene, onboarding levels, first session.

Loop:
Yes.

Mixer group:
Music
```

Recommended use:

```text
Main menu: RelaxedMenusEasyLevels
Levels 1–3: RelaxedMenusEasyLevels or MainGameplay
Normal levels: MainGameplay
Harder/later levels: DeeperFocusLaterLevels
```

---

## Existing music loops

### PuzzleCalmLoopA

```text
File:
mus_puzzle_calm_loop_a.mp3

Use when:
Alternative calm gameplay loop.

Loop:
Yes.

Mixer group:
Music
```

### PuzzleFocusLoopB

```text
File:
mus_puzzle_focus_loop_b.mp3

Use when:
Alternative focus gameplay loop.

Loop:
Yes.

Mixer group:
Music
```

### CascadeSparkleLayer

```text
File:
mus_layer_cascade_sparkle.mp3

Use when:
Optional subtle layer during cascade moments.

Loop:
Optional / short layer.

Mixer group:
Music or RewardSFX.

Important:
Do not keep this permanently active.
Fade in briefly during cascade chain, then fade out.
```

### LevelCompleteStinger

```text
File:
mus_stinger_level_complete.mp3

Use when:
Optional musical stinger on level complete.

Loop:
No.

Mixer group:
Music or RewardSFX.

Important:
Can be used together with sfx_level_complete_morning_bloom.mp3, but reduce volume to avoid clutter.
```

---

# Ambience usage

### NatureLightMorning

```text
File:
amb_nature_light_morning_loop.mp3

Use when:
Gameplay or menus need subtle nature atmosphere.

Loop:
Yes.

Mixer group:
Ambience.

Suggested volume:
0.15–0.30 relative to SFX.
```

Ambience should be low enough that the player only notices it when muted. Do not make ambience compete with music.

---

# Suggested GameAudioController API

```csharp
public sealed class GameAudioController : MonoBehaviour
{
    public void Play(AudioEvent audioEvent);
    public void Play(AudioEvent audioEvent, float volumeMultiplier);

    public void PlayMusic(MusicTrack track, float fadeSeconds = 0.75f);
    public void StopMusic(float fadeSeconds = 0.75f);

    public void PlayAmbience(AmbienceTrack track, float fadeSeconds = 1.0f);
    public void StopAmbience(float fadeSeconds = 1.0f);

    public void SetSfxVolume(float value);
    public void SetMusicVolume(float value);
    public void SetAmbienceVolume(float value);
}
```

Responsibilities:

```text
- Own all AudioSources.
- Play one-shot SFX.
- Start/stop/fade music.
- Start/stop/fade ambience.
- Apply volume settings.
- Prevent sound spam with cooldowns and voice limits.
- Add slight pitch variation for repeated SFX.
- Handle missing clips safely.
```

---

# Suggested audio event config

Each sound mapping should support:

```text
AudioEvent or MusicTrack
AudioClip
AudioMixerGroup
Volume
PitchMin
PitchMax
CooldownSeconds
MaxSimultaneousVoices
Loop
Preload
```

Suggested defaults:

```text
TileSelect:
Cooldown 0.03–0.05s
Pitch variation ±3%

SwapInvalid:
Cooldown 0.15–0.25s
Max voices 1

TileLand:
Cooldown 0.05–0.10s
Use cluster sound for 3+ landings

Clear:
One clear sound per resolve step, not per tile

Cascade:
One cascade sound per cascade iteration

GoalProgress:
If many goals update at once, play once
```

---

# Import settings

## SFX

```text
Load Type: Decompress On Load
Compression: Vorbis or PCM depending final build size
Preload Audio Data: On
Force To Mono: On for most SFX
Normalize: Off
```

## Music and ambience

```text
Load Type: Streaming
Compression: Vorbis
Preload Audio Data: Off
Loop: controlled by AudioSource
Force To Mono: Off unless file is mono
Normalize: Off
```

---

# Recommended default volume balance

```text
Board SFX: 0.75–0.90
UI SFX: 0.45–0.65
Reward SFX: 0.65–0.80
Invalid SFX: 0.55–0.70
Music: 0.35–0.55
Ambience: 0.15–0.30
```

---

# Haptics pairing

Haptics should mirror the audio, but stay subtle.

```text
TileSelect: very light tap
SwapAcceptedSettle: light tap
SwapInvalid: soft warning tap
Match4/Match5: light success tap
Cascade3Plus: medium success tap
LevelComplete: success haptic
```

Do not use heavy vibration for normal matches.

---

# Anti-spam rules

```text
TileSelect:
- Can play often.
- Small cooldown: 0.03–0.05s.

SwapInvalid:
- Max one at a time.
- Cooldown: 0.15–0.25s.

TileLand:
- Never play per tile without grouping.
- Use TileLandCluster for 3+ tiles.

Clear:
- One clear sound per resolve step, not per tile.

Cascade:
- One cascade sound per cascade iteration.

GoalProgress:
- If many goals update at once, play once, not repeatedly.
```

---

# Codex implementation prompt

```text
Implement the BUXPuzzle audio system using the files in /Audio.

Create or update GameAudioController so it supports:
- one-shot SFX by AudioEvent
- looping music by MusicTrack
- looping ambience by AmbienceTrack
- AudioMixer routing for Music, Ambience, BoardSFX, UISFX, RewardSFX, InvalidSFX
- cooldowns and voice limits to prevent sound spam
- pitch variation for repeated SFX
- volume controls for music, SFX, and ambience
- safe handling for missing clips with warnings in editor only

Map each AudioEvent to the correct file:
TileSelect -> sfx_tile_select_leaf_tap.mp3
TileDeselect -> sfx_tile_deselect_soft_release.mp3
SwapValidStart -> sfx_swap_valid_silk_slide.mp3
SwapAcceptedSettle -> sfx_swap_accept_warm_click.mp3
SwapInvalid -> sfx_swap_invalid_gentle_wood.mp3
Match3 -> sfx_match3_dew_pop.mp3
Match4 -> sfx_match4_petal_chime.mp3
Match5 -> sfx_match5_light_bloom.mp3
ClearSmall -> sfx_clear_small_pollen.mp3
ClearMedium -> sfx_clear_medium_dew_sparkle.mp3
ClearLarge -> sfx_clear_large_light_ring.mp3
TileFallShort -> sfx_tile_fall_short_soft_air.mp3
TileFallLong -> sfx_tile_fall_long_leaf_sweep.mp3
TileLandSingle -> sfx_tile_land_pebble_soft.mp3
TileLandCluster -> sfx_tile_land_cluster_moss.mp3
Cascade1 -> sfx_cascade_1_seed_glow.mp3
Cascade2 -> sfx_cascade_2_leaf_harmony.mp3
Cascade3Plus -> sfx_cascade_3plus_sunbeam.mp3
GoalProgress -> sfx_goal_progress_warm_tick.mp3
LevelComplete -> sfx_level_complete_morning_bloom.mp3
SessionFail -> sfx_session_fail_sunset_soft.mp3
UIButtonTap -> sfx_ui_button_tap_woodglass.mp3
UIOverlayOpen -> sfx_ui_overlay_open_air.mp3
UIOverlayClose -> sfx_ui_overlay_close_air.mp3
TutorialHint -> sfx_hint_soft_firefly.mp3
HintRepeat -> sfx_hint_repeat_subtle.mp3
SpecialCharge -> sfx_special_charge_dewline.mp3
SpecialTrigger -> sfx_special_trigger_lightwave.mp3
SpecialCreated -> sfx_special_created_seed_bloom.mp3
SpecialCombine -> sfx_special_combine_light_merge.mp3

Map music:
RelaxedMenusEasyLevels -> mus_background_relaxed_menus_easy_levels.mp3
MainGameplay -> mus_background_main_gameplay.mp3
DeeperFocusLaterLevels -> mus_background_deeper_focus_later_levels.mp3
PuzzleCalmLoopA -> mus_puzzle_calm_loop_a.mp3
PuzzleFocusLoopB -> mus_puzzle_focus_loop_b.mp3
CascadeSparkleLayer -> mus_layer_cascade_sparkle.mp3
LevelCompleteStinger -> mus_stinger_level_complete.mp3

Map ambience:
NatureLightMorning -> amb_nature_light_morning_loop.mp3

Wire gameplay:
- Play TileSelect when selecting a tile.
- Play TileDeselect when deselecting.
- Play SwapValidStart when valid adjacent swap animation begins.
- Play SwapAcceptedSettle when a swap is accepted.
- Play SwapInvalid when a swap is rejected.
- Play Match3/Match4/Match5 based on match size.
- Play ClearSmall/ClearMedium/ClearLarge based on cleared tile count.
- Play TileFallShort or TileFallLong based on drop distance.
- Play TileLandSingle or TileLandCluster based on landing count.
- Play Cascade1/Cascade2/Cascade3Plus based on cascade index.
- Play GoalProgress when level objectives update.
- Play LevelComplete on win.
- Play SessionFail on fail.
- Play UI sounds on buttons and overlays.
- Play TutorialHint and HintRepeat from onboarding/hint system.

Wire music:
- Main menu: RelaxedMenusEasyLevels.
- Levels 1–3: RelaxedMenusEasyLevels or MainGameplay.
- Normal gameplay: MainGameplay.
- Later/harder levels: DeeperFocusLaterLevels.
- Level complete: optionally play LevelCompleteStinger while ducking gameplay music.

Acceptance criteria:
- Every AudioEvent has a mapped clip.
- Missing clips show editor warnings but do not crash builds.
- Music loops seamlessly and crossfades between states.
- SFX does not spam during cascades or tile drops.
- Player can mute music, SFX, and ambience separately.
- Audio works on Android and iOS builds.
```

---

# Recommended final file list

```text
amb_nature_light_morning_loop.mp3

mus_background_main_gameplay.mp3
mus_background_deeper_focus_later_levels.mp3
mus_background_relaxed_menus_easy_levels.mp3
mus_layer_cascade_sparkle.mp3
mus_puzzle_calm_loop_a.mp3
mus_puzzle_focus_loop_b.mp3
mus_stinger_level_complete.mp3

sfx_cascade_1_seed_glow.mp3
sfx_cascade_2_leaf_harmony.mp3
sfx_cascade_3plus_sunbeam.mp3
sfx_clear_large_light_ring.mp3
sfx_clear_medium_dew_sparkle.mp3
sfx_clear_small_pollen.mp3
sfx_goal_progress_warm_tick.mp3
sfx_hint_repeat_subtle.mp3
sfx_hint_soft_firefly.mp3
sfx_level_complete_morning_bloom.mp3
sfx_match3_dew_pop.mp3
sfx_match4_petal_chime.mp3
sfx_match5_light_bloom.mp3
sfx_session_fail_sunset_soft.mp3
sfx_session_fail_sunset_soft_alt.mp3
sfx_special_charge_dewline.mp3
sfx_special_combine_light_merge.mp3
sfx_special_created_seed_bloom.mp3
sfx_special_trigger_lightwave.mp3
sfx_swap_accept_warm_click.mp3
sfx_swap_invalid_gentle_wood.mp3
sfx_swap_valid_silk_slide.mp3
sfx_tile_deselect_soft_release.mp3
sfx_tile_fall_long_leaf_sweep.mp3
sfx_tile_fall_short_soft_air.mp3
sfx_tile_land_cluster_moss.mp3
sfx_tile_land_pebble_soft.mp3
sfx_tile_select_leaf_tap.mp3
sfx_ui_button_tap_woodglass.mp3
sfx_ui_overlay_close_air.mp3
sfx_ui_overlay_open_air.mp3
```

---

# Definition of done

The audio implementation is complete when:

```text
- All required SFX files are imported into Unity.
- All three main background music tracks are renamed and mapped.
- Music can crossfade between menu, normal gameplay, and harder/later gameplay.
- Ambience loops quietly and can be muted separately.
- Every AudioEvent plays the correct file.
- Cascades and tile drops do not create audio spam.
- Missing clips do not crash runtime.
- Settings allow the player to control Music, SFX, and Ambience.
- Android and iOS builds play audio correctly.
```
