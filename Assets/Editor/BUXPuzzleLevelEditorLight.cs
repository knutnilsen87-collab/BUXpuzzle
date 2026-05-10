using System.Collections.Generic;
using Game.Levels;
using UnityEditor;
using UnityEngine;

public sealed class BUXPuzzleLevelEditorLight : EditorWindow
{
    private readonly List<string> _messages = new List<string>();
    private int _levelId = 1;

    [MenuItem("BUXPuzzle/Levels/Level Editor Light")]
    public static void Open()
    {
        GetWindow<BUXPuzzleLevelEditorLight>("BUX Levels");
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Level Pipeline", EditorStyles.boldLabel);
        _levelId = EditorGUILayout.IntField("Level Id", Mathf.Max(1, _levelId));

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Validate Level"))
        {
            ValidateSingle();
        }

        if (GUILayout.Button("Validate 1-30"))
        {
            ValidateRange();
        }

        if (GUILayout.Button("Open Content Folder"))
        {
            EditorUtility.RevealInFinder("Assets/Game/Content/Levels");
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(8f);
        foreach (var message in _messages)
        {
            EditorGUILayout.HelpBox(message, message.StartsWith("PASS") ? MessageType.Info : MessageType.Warning);
        }
    }

    private void ValidateSingle()
    {
        _messages.Clear();
        ValidateLevel(_levelId);
    }

    private void ValidateRange()
    {
        _messages.Clear();
        for (int i = 1; i <= 30; i++)
        {
            ValidateLevel(i);
        }
    }

    private void ValidateLevel(int id)
    {
        var repo = new LevelRepository();
        var spec = repo.GetLevel(id);
        var result = LevelValidator.Validate(spec);
        if (result.IsValid)
        {
            _messages.Add("PASS Level " + id + " " + spec.Width + "x" + spec.Height + " objectives=" + (spec.Objectives != null ? spec.Objectives.Length : 0));
        }
        else
        {
            _messages.Add("FAIL Level " + id + ": " + result.Message);
        }
    }
}
