using UnityEditor;
using UnityEngine;

public static class FBL_RuntimeCaptureOrchestrator
{
    [MenuItem("FBL/Runtime Capture/Validate Phase B Setup")]
    public static void ValidatePhaseBSetup()
    {
        Debug.Log("[FBL_RuntimeCaptureOrchestrator] Validate Phase B Setup invoked.");
    }

    [MenuItem("FBL/Runtime Capture/Open Playable Scene")]
    public static void OpenPlayableScene()
    {
        Debug.Log("[FBL_RuntimeCaptureOrchestrator] Open Playable Scene invoked.");
    }
}
