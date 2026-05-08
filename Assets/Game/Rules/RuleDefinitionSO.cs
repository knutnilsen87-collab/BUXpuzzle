using UnityEngine;

namespace Game.Rules
{
    /// <summary>
    /// 🔒 LÅST KRAV: Alle regler MÅ defineres som data.
    /// Minimum:
    /// - id
    /// - duration
    /// - visualSignature
    /// - audioSignature
    /// - params (int/float)
    /// </summary>
    [CreateAssetMenu(menuName = "Game/Rules/Rule Definition", fileName = "RuleDefinition")]
    public class RuleDefinitionSO : ScriptableObject
    {
        public RuleId id;
        public int durationTurns = 5;

        // "Signatures" are keys consumed by VFX/SFX systems (not text explanations).
        public string visualSignature;
        public string audioSignature;

        // Minimal param surface for tuning
        public int paramInt = 0;
        public float paramFloat = 0f;
    }
}