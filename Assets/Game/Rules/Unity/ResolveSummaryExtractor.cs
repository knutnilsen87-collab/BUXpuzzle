using System;
using System.Reflection;

namespace Game.Rules.Unity
{
    /// <summary>
    /// Extracts merges/combo/hadMatch from an arbitrary resolve result object using reflection.
    /// Supports common field/property names (case-insensitive).
    /// </summary>
    public static class ResolveSummaryExtractor
    {
        public static void Extract(object resolveResult, out int merges, out int combo, out bool hadMatch)
        {
            merges = 0; combo = 0; hadMatch = false;
            if (resolveResult == null) return;

            // Try properties/fields by common names
            merges = TryGetInt(resolveResult, "Merges", "merges", "MergeCount", "matchCount", "Matches");
            combo  = TryGetInt(resolveResult, "Combo", "combo", "ComboCount", "comboCount", "Chain", "chain");
            hadMatch = TryGetBool(resolveResult, "HadMatch", "hadMatch", "HasMatch", "hasMatch", "Matched", "matched");

            // If we can't find hadMatch but merges/matches suggests activity:
            if (!hadMatch && merges > 0) hadMatch = true;
        }

        private static int TryGetInt(object o, params string[] names)
        {
            foreach (var n in names)
            {
                if (TryGetMember(o, n, out var v))
                {
                    try { return Convert.ToInt32(v); } catch { }
                }
            }
            return 0;
        }

        private static bool TryGetBool(object o, params string[] names)
        {
            foreach (var n in names)
            {
                if (TryGetMember(o, n, out var v))
                {
                    try { return Convert.ToBoolean(v); } catch { }
                }
            }
            return false;
        }

        private static bool TryGetMember(object o, string name, out object value)
        {
            value = null;
            var t = o.GetType();
            const BindingFlags F = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreCase;

            var p = t.GetProperty(name, F);
            if (p != null)
            {
                value = p.GetValue(o);
                return true;
            }

            var f = t.GetField(name, F);
            if (f != null)
            {
                value = f.GetValue(o);
                return true;
            }

            return false;
        }
    }
}