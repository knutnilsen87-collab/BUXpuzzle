using System;
using System.IO;
using Game.Audio;
using UnityEditor;
using UnityEngine;

namespace Game.EditorTools
{
    public static class BUXPuzzleAudioConfigBuilder
    {
        public const string ConfigPath = "Assets/Resources/Audio/BUXPuzzleAudioConfig.asset";
        private const string AudioPath = "Assets/Audio/";

        [MenuItem("BUXPuzzle/Audio/Rebuild Audio Config")]
        public static void CreateOrUpdate()
        {
            Directory.CreateDirectory("Assets/Resources/Audio");

            var config = AssetDatabase.LoadAssetAtPath<BUXPuzzleAudioConfig>(ConfigPath);
            if (config == null)
            {
                config = ScriptableObject.CreateInstance<BUXPuzzleAudioConfig>();
                AssetDatabase.CreateAsset(config, ConfigPath);
            }

            config.Events = new[]
            {
                Event(AudioEvent.TileSelect, "sfx_tile_select_leaf_tap.mp3", AudioRoute.BoardSFX, 0.78f, 0.97f, 1.03f, 0.04f, 4),
                Event(AudioEvent.TileDeselect, "sfx_tile_deselect_soft_release.mp3", AudioRoute.BoardSFX, 0.52f, 0.97f, 1.02f, 0.05f, 2),
                Event(AudioEvent.SwapValidStart, "sfx_swap_valid_silk_slide.mp3", AudioRoute.BoardSFX, 0.62f, 0.98f, 1.02f, 0.05f, 2),
                Event(AudioEvent.SwapAcceptedSettle, "sfx_swap_accept_warm_click.mp3", AudioRoute.BoardSFX, 0.70f, 0.98f, 1.02f, 0.05f, 2),
                Event(AudioEvent.SwapInvalid, "sfx_swap_invalid_gentle_wood.mp3", AudioRoute.InvalidSFX, 0.62f, 0.98f, 1.01f, 0.20f, 1),
                Event(AudioEvent.Match3, "sfx_match3_dew_pop.mp3", AudioRoute.BoardSFX, 0.75f, 0.97f, 1.03f, 0.05f, 3),
                Event(AudioEvent.Match4, "sfx_match4_petal_chime.mp3", AudioRoute.BoardSFX, 0.78f, 0.97f, 1.03f, 0.05f, 3),
                Event(AudioEvent.Match5, "sfx_match5_light_bloom.mp3", AudioRoute.RewardSFX, 0.78f, 0.97f, 1.03f, 0.08f, 2),
                Event(AudioEvent.ClearSmall, "sfx_clear_small_pollen.mp3", AudioRoute.BoardSFX, 0.65f, 0.98f, 1.03f, 0.06f, 2),
                Event(AudioEvent.ClearMedium, "sfx_clear_medium_dew_sparkle.mp3", AudioRoute.BoardSFX, 0.70f, 0.98f, 1.03f, 0.07f, 2),
                Event(AudioEvent.ClearLarge, "sfx_clear_large_light_ring.mp3", AudioRoute.RewardSFX, 0.76f, 0.98f, 1.02f, 0.10f, 1),
                Event(AudioEvent.TileFallShort, "sfx_tile_fall_short_soft_air.mp3", AudioRoute.BoardSFX, 0.50f, 0.98f, 1.02f, 0.08f, 1),
                Event(AudioEvent.TileFallLong, "sfx_tile_fall_long_leaf_sweep.mp3", AudioRoute.BoardSFX, 0.56f, 0.98f, 1.02f, 0.10f, 1),
                Event(AudioEvent.TileLandSingle, "sfx_tile_land_pebble_soft.mp3", AudioRoute.BoardSFX, 0.48f, 0.97f, 1.02f, 0.08f, 1),
                Event(AudioEvent.TileLandCluster, "sfx_tile_land_cluster_moss.mp3", AudioRoute.BoardSFX, 0.55f, 0.97f, 1.02f, 0.10f, 1),
                Event(AudioEvent.Cascade1, "sfx_cascade_1_seed_glow.mp3", AudioRoute.RewardSFX, 0.62f, 0.99f, 1.02f, 0.18f, 1),
                Event(AudioEvent.Cascade2, "sfx_cascade_2_leaf_harmony.mp3", AudioRoute.RewardSFX, 0.66f, 0.99f, 1.02f, 0.20f, 1),
                Event(AudioEvent.Cascade3Plus, "sfx_cascade_3plus_sunbeam.mp3", AudioRoute.RewardSFX, 0.72f, 0.99f, 1.02f, 0.22f, 1),
                Event(AudioEvent.GoalProgress, "sfx_goal_progress_warm_tick.mp3", AudioRoute.RewardSFX, 0.68f, 0.98f, 1.02f, 0.12f, 1),
                Event(AudioEvent.LevelComplete, "sfx_level_complete_morning_bloom.mp3", AudioRoute.RewardSFX, 0.78f, 0.99f, 1.01f, 0.50f, 1),
                Event(AudioEvent.SessionFail, "sfx_session_fail_sunset_soft.mp3", AudioRoute.InvalidSFX, 0.62f, 1.00f, 1.00f, 0.50f, 1),
                Event(AudioEvent.UIButtonTap, "sfx_ui_button_tap_woodglass.mp3", AudioRoute.UISFX, 0.56f, 0.98f, 1.02f, 0.05f, 2),
                Event(AudioEvent.UIOverlayOpen, "sfx_ui_overlay_open_air.mp3", AudioRoute.UISFX, 0.52f, 1.00f, 1.00f, 0.15f, 1),
                Event(AudioEvent.UIOverlayClose, "sfx_ui_overlay_close_air.mp3", AudioRoute.UISFX, 0.46f, 1.00f, 1.00f, 0.15f, 1),
                Event(AudioEvent.TutorialHint, "sfx_hint_soft_firefly.mp3", AudioRoute.UISFX, 0.56f, 0.99f, 1.01f, 0.50f, 1),
                Event(AudioEvent.HintRepeat, "sfx_hint_repeat_subtle.mp3", AudioRoute.UISFX, 0.42f, 0.99f, 1.01f, 1.00f, 1),
                Event(AudioEvent.SpecialCharge, "sfx_special_charge_dewline.mp3", AudioRoute.RewardSFX, 0.58f, 0.99f, 1.02f, 0.15f, 1),
                Event(AudioEvent.SpecialTrigger, "sfx_special_trigger_lightwave.mp3", AudioRoute.RewardSFX, 0.75f, 0.99f, 1.02f, 0.15f, 1),
                Event(AudioEvent.SpecialCreated, "sfx_special_created_seed_bloom.mp3", AudioRoute.RewardSFX, 0.68f, 0.99f, 1.02f, 0.20f, 1),
                Event(AudioEvent.SpecialCombine, "sfx_special_combine_light_merge.mp3", AudioRoute.RewardSFX, 0.78f, 0.99f, 1.01f, 0.25f, 1),
            };

            config.Music = new[]
            {
                Music(MusicTrack.RelaxedMenusEasyLevels, "mus_background_relaxed_menus_easy_levels.mp3", 0.42f, true),
                Music(MusicTrack.MainGameplay, "mus_background_main_gameplay.mp3", 0.46f, true),
                Music(MusicTrack.DeeperFocusLaterLevels, "mus_background_deeper_focus_later_levels.mp3", 0.44f, true),
                Music(MusicTrack.PuzzleCalmLoopA, "mus_puzzle_calm_loop_a.mp3", 0.42f, true),
                Music(MusicTrack.PuzzleFocusLoopB, "mus_puzzle_focus_loop_b.mp3", 0.42f, true),
                Music(MusicTrack.CascadeSparkleLayer, "mus_layer_cascade_sparkle.mp3", 0.30f, false),
                Music(MusicTrack.LevelCompleteStinger, "mus_stinger_level_complete.mp3", 0.50f, false),
            };

            config.Ambience = new[]
            {
                Ambience(AmbienceTrack.NatureLightMorning, "amb_nature_light_morning_loop.mp3", 0.22f, true),
            };

            EditorUtility.SetDirty(config);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[BUXPuzzleAudioConfigBuilder] Audio config rebuilt: " + ConfigPath);
        }

        private static AudioEventBinding Event(AudioEvent audioEvent, string file, AudioRoute route, float volume, float pitchMin, float pitchMax, float cooldown, int maxVoices)
        {
            return new AudioEventBinding
            {
                Event = audioEvent,
                Route = route,
                Clip = Clip(file),
                Volume = volume,
                PitchMin = pitchMin,
                PitchMax = pitchMax,
                CooldownSeconds = cooldown,
                MaxSimultaneousVoices = maxVoices
            };
        }

        private static MusicTrackBinding Music(MusicTrack track, string file, float volume, bool loop)
        {
            return new MusicTrackBinding
            {
                Track = track,
                Clip = Clip(file),
                Volume = volume,
                Loop = loop
            };
        }

        private static AmbienceTrackBinding Ambience(AmbienceTrack track, string file, float volume, bool loop)
        {
            return new AmbienceTrackBinding
            {
                Track = track,
                Clip = Clip(file),
                Volume = volume,
                Loop = loop
            };
        }

        private static AudioClip Clip(string file)
        {
            return AssetDatabase.LoadAssetAtPath<AudioClip>(AudioPath + file);
        }

        public static string FileNameFor(AudioEvent audioEvent)
        {
            switch (audioEvent)
            {
                case AudioEvent.TileSelect: return "sfx_tile_select_leaf_tap.mp3";
                case AudioEvent.TileDeselect: return "sfx_tile_deselect_soft_release.mp3";
                case AudioEvent.SwapValidStart: return "sfx_swap_valid_silk_slide.mp3";
                case AudioEvent.SwapAcceptedSettle: return "sfx_swap_accept_warm_click.mp3";
                case AudioEvent.SwapInvalid: return "sfx_swap_invalid_gentle_wood.mp3";
                case AudioEvent.Match3: return "sfx_match3_dew_pop.mp3";
                case AudioEvent.Match4: return "sfx_match4_petal_chime.mp3";
                case AudioEvent.Match5: return "sfx_match5_light_bloom.mp3";
                case AudioEvent.ClearSmall: return "sfx_clear_small_pollen.mp3";
                case AudioEvent.ClearMedium: return "sfx_clear_medium_dew_sparkle.mp3";
                case AudioEvent.ClearLarge: return "sfx_clear_large_light_ring.mp3";
                case AudioEvent.TileFallShort: return "sfx_tile_fall_short_soft_air.mp3";
                case AudioEvent.TileFallLong: return "sfx_tile_fall_long_leaf_sweep.mp3";
                case AudioEvent.TileLandSingle: return "sfx_tile_land_pebble_soft.mp3";
                case AudioEvent.TileLandCluster: return "sfx_tile_land_cluster_moss.mp3";
                case AudioEvent.Cascade1: return "sfx_cascade_1_seed_glow.mp3";
                case AudioEvent.Cascade2: return "sfx_cascade_2_leaf_harmony.mp3";
                case AudioEvent.Cascade3Plus: return "sfx_cascade_3plus_sunbeam.mp3";
                case AudioEvent.GoalProgress: return "sfx_goal_progress_warm_tick.mp3";
                case AudioEvent.LevelComplete: return "sfx_level_complete_morning_bloom.mp3";
                case AudioEvent.SessionFail: return "sfx_session_fail_sunset_soft.mp3";
                case AudioEvent.UIButtonTap: return "sfx_ui_button_tap_woodglass.mp3";
                case AudioEvent.UIOverlayOpen: return "sfx_ui_overlay_open_air.mp3";
                case AudioEvent.UIOverlayClose: return "sfx_ui_overlay_close_air.mp3";
                case AudioEvent.TutorialHint: return "sfx_hint_soft_firefly.mp3";
                case AudioEvent.HintRepeat: return "sfx_hint_repeat_subtle.mp3";
                case AudioEvent.SpecialCharge: return "sfx_special_charge_dewline.mp3";
                case AudioEvent.SpecialTrigger: return "sfx_special_trigger_lightwave.mp3";
                case AudioEvent.SpecialCreated: return "sfx_special_created_seed_bloom.mp3";
                case AudioEvent.SpecialCombine: return "sfx_special_combine_light_merge.mp3";
                default: throw new ArgumentOutOfRangeException(nameof(audioEvent), audioEvent, null);
            }
        }
    }
}
