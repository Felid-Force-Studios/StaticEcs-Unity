using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace FFS.Libraries.StaticEcs.Unity.Editor {
    internal static class TypeSourceNavigator {
        private static readonly Dictionary<Type, MonoScript> _cache = new();
        private static readonly Regex _nsRegex = new(@"(?m)^\s*namespace\s+([\w.]+)\s*[;{]", RegexOptions.Compiled);

        public static MonoScript FindScript(Type type) {
            type = UnwrapNavigationTarget(type);
            if (type == null) return null;
            if (_cache.TryGetValue(type, out var cached)) return cached;
            var script = FindByGetClass(type) ?? FindByTextScan(type);
            _cache[type] = script;
            return script;
        }

        public static void DrawScriptField(Type type) {
            var script = FindScript(type);
            using (new EditorGUI.DisabledScope(true)) {
                EditorGUILayout.ObjectField("Source", script, typeof(MonoScript), false);
            }
        }

        public static Type UnwrapNavigationTarget(Type type) {
            if (type == null || !type.IsGenericType) return type;
            foreach (var arg in type.GetGenericArguments()) {
                if (typeof(ILinkType).IsAssignableFrom(arg)
                    || typeof(ILinksType).IsAssignableFrom(arg)
                    || typeof(IMultiComponent).IsAssignableFrom(arg)) {
                    return arg;
                }
            }
            return type;
        }

        private static MonoScript FindByGetClass(Type type) {
            foreach (var guid in AssetDatabase.FindAssets($"t:MonoScript {type.Name}")) {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var s = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
                if (s != null && s.GetClass() == type) return s;
            }
            return null;
        }

        private static MonoScript FindByTextScan(Type type) {
            var ns = type.Namespace;
            var decl = new Regex($@"\b(struct|class|interface|record)\s+{Regex.Escape(type.Name)}\b");
            foreach (var guid in AssetDatabase.FindAssets("t:MonoScript")) {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (!path.EndsWith(".cs", StringComparison.Ordinal)) continue;
                string text;
                try { text = File.ReadAllText(path); } catch { continue; }
                if (!decl.IsMatch(text)) continue;
                if (!FileNamespaceMatches(text, ns)) continue;
                var s = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
                if (s != null) return s;
            }
            return null;
        }

        private static bool FileNamespaceMatches(string text, string expectedNs) {
            var matches = _nsRegex.Matches(text);
            if (matches.Count == 0) return expectedNs == null;
            if (expectedNs == null) return false;
            foreach (Match m in matches) {
                if (m.Groups[1].Value == expectedNs) return true;
            }
            return false;
        }
    }
}
