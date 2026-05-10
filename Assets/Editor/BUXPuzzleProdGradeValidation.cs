using System;
using System.Collections.Generic;
using System.IO;
using Game.Levels;
using UnityEditor;
using UnityEngine;

public static class BUXPuzzleProdGradeValidation
{
    private const string ReportPath = "Logs/codex_prod_grade_validation.json";

    public static void RunAndExit()
    {
        var passed = new List<string>();
        var warnings = new List<string>();
        var failures = new List<string>();

        ValidateLevelContent(passed, failures);
        ValidateReleaseDocs(passed, warnings);
        ValidatePlayerSettings(passed, warnings);

        Directory.CreateDirectory(Path.GetDirectoryName(ReportPath) ?? "Logs");
        File.WriteAllText(ReportPath, JsonUtility.ToJson(new Report
        {
            generatedUtc = DateTime.UtcNow.ToString("O"),
            passed = passed.ToArray(),
            warnings = warnings.ToArray(),
            failures = failures.ToArray()
        }, true));

        foreach (var pass in passed) Debug.Log("[BUX_PROD_PASS] " + pass);
        foreach (var warning in warnings) Debug.LogWarning("[BUX_PROD_WARN] " + warning);
        foreach (var failure in failures) Debug.LogError("[BUX_PROD_FAIL] " + failure);
        EditorApplication.Exit(failures.Count == 0 ? 0 : 1);
    }

    private static void ValidateLevelContent(List<string> passed, List<string> failures)
    {
        var repo = new LevelRepository();
        for (int i = 1; i <= 30; i++)
        {
            var spec = repo.GetLevel(i);
            var result = LevelValidator.Validate(spec);
            if (result.IsValid)
            {
                passed.Add("Level " + i + " validates as " + spec.Width + "x" + spec.Height);
            }
            else
            {
                failures.Add("Level " + i + " failed validation: " + result.Message);
            }
        }
    }

    private static void ValidateReleaseDocs(List<string> passed, List<string> warnings)
    {
        RequireFile("RELEASE_DOD.md", passed, warnings);
        RequireFile("STORE_READINESS.md", passed, warnings);
        RequireFile("STORE_METADATA_DRAFT.md", passed, warnings);
        RequireFile("PRIVACY_POLICY_DRAFT.md", passed, warnings);
        RequireFile("QA_DEVICE_MATRIX.md", passed, warnings);
    }

    private static void ValidatePlayerSettings(List<string> passed, List<string> warnings)
    {
        if (!string.IsNullOrEmpty(PlayerSettings.productName)) passed.Add("Product name set: " + PlayerSettings.productName);
        else warnings.Add("Product name is empty.");

        string android = PlayerSettings.GetApplicationIdentifier(UnityEditor.Build.NamedBuildTarget.Android);
        string ios = PlayerSettings.GetApplicationIdentifier(UnityEditor.Build.NamedBuildTarget.iOS);
        if (!string.IsNullOrEmpty(android) && android.Contains(".")) passed.Add("Android application id set.");
        else warnings.Add("Android application id needs final store value.");
        if (!string.IsNullOrEmpty(ios) && ios.Contains(".")) passed.Add("iOS bundle id set.");
        else warnings.Add("iOS bundle id needs final store value.");
    }

    private static void RequireFile(string path, List<string> passed, List<string> warnings)
    {
        if (File.Exists(path) && new FileInfo(path).Length > 0) passed.Add(path + " exists.");
        else warnings.Add(path + " is missing or empty.");
    }

    [Serializable]
    private sealed class Report
    {
        public string generatedUtc;
        public string[] passed;
        public string[] warnings;
        public string[] failures;
    }
}
