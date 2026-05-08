using UnityEngine;
namespace Game.Core
{
    public static class AutoAttachOnboarding
    {

    // Compile-safe fallback logger (no asmdef deps)
    private static void LogUlf(string type, string data = null)
    {
        UnityEngine.Debug.Log("[ULF_FALLBACK] " + type + (data != null ? (" " + data) : ""));
    }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Attach()
        {
        // CLARITY_LOCK_AUTOATTACH: never auto-add onboarding in Play Mode
        if (UnityEngine.Application.isPlaying)
        {
            LogUlf("tutorial.autoattach.blocked", "reason=clarity_lock");
            return;
        }
        if (UnityEngine.Application.isPlaying)
        {
            LogUlf("tutorial.autoattach.blocked");
            return;
        }
            var go = GameObject.Find("FBL_ONBOARDING_BOOT");
            if (go == null)
            {
                go = new GameObject("FBL_ONBOARDING_BOOT");
                Object.DontDestroyOnLoad(go);
            }

            if (go.GetComponent<PlayPingLogger>() == null) go.AddComponent<PlayPingLogger>();
            if (go.GetComponent<LatestLogWriter>() == null) go.AddComponent<LatestLogWriter>();
            if (go.GetComponent<FirstRunDemo>() == null) go.AddComponent<FirstRunDemo>();

            Debug.Log("[FBL] AutoAttachOnboarding OK");
        }
    }
}





