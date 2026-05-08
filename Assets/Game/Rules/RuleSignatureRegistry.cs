using System.Collections.Generic;
using UnityEngine;

namespace Game.Rules
{
    public interface IRuleSignatureRegistry
    {
        bool TryValidate(RuleId id, out string reason);
        bool TryGetDefinition(RuleId id, out RuleDefinitionSO def);
    }

    /// <summary>
    /// 🔒 LÅST: Ingen regel uten signatur.
    /// Runtime assert on activation:
    /// - visualSignature != null/empty
    /// - audioSignature  != null/empty
    /// Fail -> ERROR + disable rule + fallback ladder.
    /// </summary>
    public class RuleSignatureRegistry : IRuleSignatureRegistry
    {
        private readonly Dictionary<RuleId, RuleDefinitionSO> _defs = new();

        public RuleSignatureRegistry(IEnumerable<RuleDefinitionSO> defs)
        {
            if (defs == null) return;
            foreach (var d in defs)
            {
                if (d == null) continue;
                _defs[d.id] = d;
            }
        }

        public bool TryGetDefinition(RuleId id, out RuleDefinitionSO def) => _defs.TryGetValue(id, out def);

        public bool TryValidate(RuleId id, out string reason)
        {
            reason = null;

            if (!_defs.TryGetValue(id, out var def) || def == null)
            {
                reason = "Missing RuleDefinitionSO for rule: " + id;
                return false;
            }

            if (string.IsNullOrEmpty(def.visualSignature))
            {
                reason = "Missing visualSignature for rule: " + id;
                return false;
            }

            if (string.IsNullOrEmpty(def.audioSignature))
            {
                reason = "Missing audioSignature for rule: " + id;
                return false;
            }

            return true;
        }
    }
}