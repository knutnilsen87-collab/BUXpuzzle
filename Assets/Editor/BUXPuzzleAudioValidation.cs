using System;
using System.Collections.Generic;
using System.IO;
using Game.Audio;
using UnityEditor;
using UnityEngine;

namespace Game.EditorTools
{
    public static class BUXPuzzleAudioValidation
    {
        private const string ReportPath = "Logs/codex_audio_validation.json";
        private const string AudioPath = "Assets/Audio/";

        private static readonly List<string> Passed = new List<string>();
        private static readonly List<string> Warnings = new List<string>();
        private static readonly List<string> Failures = new List<string>();

        public static void RunAndExit()
        {
            Run();
            EditorApplication.Exit(Failures.Count == 0 ? 0 : 1);
        }

        public static void Run()
        {
            Passed.Clear();
            Warnings.Clear();
            Failures.Clear();

            BUXPuzzleAudioConfigBuilder.CreateOrUpdate();
            ValidateFiles();
            ValidateConfig();
            WriteReport();

            foreach (var pass in Passed) Debug.Log("[BUX_AUDIO_PASS] " + pass);
            foreach (var warning in Warnings) Debug.LogWarning("[BUX_AUDIO_WARN] " + warning);
            foreach (var failure in Failures) Debug.LogError("[BUX_AUDIO_FAIL] " + failure);
        }

        private static void ValidateFiles()
        {
            foreach (AudioEvent audioEvent in Enum.GetValues(typeof(AudioEvent)))
            {
                string file = BUXPuzzleAudioConfigBuilder.FileNameFor(audioEvent);
                Require(File.Exists(AudioPath + file), "Audio file exists: " + file, "Missing AudioEvent file: " + file);
            }

            Require(File.Exists(AudioPath + "mus_background_relaxed_menus_easy_levels.mp3"), "Relaxed menu music file exists.", "Missing relaxed menu music.");
            Require(File.Exists(AudioPath + "mus_background_main_gameplay.mp3"), "Main gameplay music file exists.", "Missing main gameplay music.");
            Require(File.Exists(AudioPath + "mus_background_deeper_focus_later_levels.mp3"), "Deeper focus music file exists.", "Missing deeper focus music.");
            Require(File.Exists(AudioPath + "amb_nature_light_morning_loop.mp3"), "Nature Light ambience file exists.", "Missing Nature Light ambience.");
        }

        private static void ValidateConfig()
        {
            var config = AssetDatabase.LoadAssetAtPath<BUXPuzzleAudioConfig>(BUXPuzzleAudioConfigBuilder.ConfigPath);
            Require(config != null, "BUXPuzzleAudioConfig asset exists.", "BUXPuzzleAudioConfig asset is missing.");
            if (config == null) return;

            foreach (AudioEvent audioEvent in Enum.GetValues(typeof(AudioEvent)))
            {
                bool mapped = config.TryGet(audioEvent, out var binding) && binding != null && binding.Clip != null;
                Require(mapped, "AudioEvent mapped: " + audioEvent, "AudioEvent is not mapped to a clip: " + audioEvent);
            }

            foreach (MusicTrack track in Enum.GetValues(typeof(MusicTrack)))
            {
                bool mapped = config.TryGet(track, out var binding) && binding != null && binding.Clip != null;
                Require(mapped, "MusicTrack mapped: " + track, "MusicTrack is not mapped to a clip: " + track);
            }

            foreach (AmbienceTrack track in Enum.GetValues(typeof(AmbienceTrack)))
            {
                bool mapped = config.TryGet(track, out var binding) && binding != null && binding.Clip != null;
                Require(mapped, "AmbienceTrack mapped: " + track, "AmbienceTrack is not mapped to a clip: " + track);
            }
        }

        private static void Require(bool condition, string pass, string fail)
        {
            if (condition) Passed.Add(pass);
            else Failures.Add(fail);
        }

        private static void WriteReport()
        {
            Directory.CreateDirectory("Logs");
            var json = JsonUtility.ToJson(new AudioValidationReport
            {
                generatedUtc = DateTime.UtcNow.ToString("O"),
                passed = Passed.ToArray(),
                warnings = Warnings.ToArray(),
                failures = Failures.ToArray()
            }, true);
            File.WriteAllText(ReportPath, json);
        }

        [Serializable]
        private sealed class AudioValidationReport
        {
            public string generatedUtc;
            public string[] passed;
            public string[] warnings;
            public string[] failures;
        }
    }
}
