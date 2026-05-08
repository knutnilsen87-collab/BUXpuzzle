using System;
using UnityEngine;

namespace Game.UX
{
    /// <summary>
    /// 🔒 LÅST KRAV: Ingen parallelle UI-states.
    /// Guard: logger ERROR hvis en state forsøkes åpnet mens en annen er aktiv.
    ///
    /// (CI build fail kan kobles på senere ved å gjøre ERROR->Exception i batchmode.)
    /// </summary>
    public enum UIState
    {
        None = 0,
        Gameplay = 1,
        RuleEvent = 2,
        SocialEvent = 3,
        Results = 4
    }

    public class UIStateMachine : MonoBehaviour
    {
        public UIState Current { get; private set; } = UIState.None;

        public event Action<UIState, UIState> OnStateChanged;

        public void Set(UIState next)
        {
            if (next == Current) return;

            // no-overlap: you can always go to None, otherwise you must come from None
            if (Current != UIState.None && next != UIState.None)
            {
                Debug.LogError("[UIStateMachine] UI state overlap blocked: " + Current + " -> " + next);
                return;
            }

            var prev = Current;
            Current = next;
            Debug.Log("[UIStateMachine] State " + prev + " -> " + next);
            OnStateChanged?.Invoke(prev, next);
        }
    }
}