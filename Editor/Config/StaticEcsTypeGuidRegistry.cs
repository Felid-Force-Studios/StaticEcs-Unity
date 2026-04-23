using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FFS.Libraries.StaticEcs.Unity.Editor {

    [CreateAssetMenu(menuName = "Static ECS/Type Guid Registry", fileName = "StaticEcsTypeGuidRegistry")]
    public sealed class StaticEcsTypeGuidRegistry : ScriptableObject {

        [Serializable]
        public struct TypeIdentity : IEquatable<TypeIdentity> {
            public string className;
            public string namespaceName;
            public string assembly;

            public bool Equals(TypeIdentity o) => className == o.className && namespaceName == o.namespaceName && assembly == o.assembly;

            public override bool Equals(object o) => o is TypeIdentity t && Equals(t);

            public override int GetHashCode() => HashCode.Combine(className, namespaceName, assembly);
        }

        [Serializable]
        public class Entry {
            public string guid;
            public TypeIdentity current;
            public List<TypeIdentity> history = new();
        }

        [Serializable]
        public class WorldSettings {
            public string worldTypeFullName;
            public List<Entry> components = new();
            public List<Entry> tags = new();
            public List<Entry> events = new();
            public List<Entry> links = new();
            public List<Entry> linksList = new();
            public List<Entry> multi = new();

            internal bool SyncEntry(ConfigKind kind, Guid guid, TypeIdentity current) {
                var entries = kind switch {
                    ConfigKind.Component => components,
                    ConfigKind.Tag => tags,
                    ConfigKind.Event => events,
                    ConfigKind.Link => links,
                    ConfigKind.Links => linksList,
                    ConfigKind.Multi => multi,
                    _ => throw new Exception(kind.ToString()),
                };

                var key = guid.ToString("D");
                Entry entry = null;
                for (var i = 0; i < entries.Count; i++) {
                    if (entries[i].guid == key) {
                        entry = entries[i];
                        break;
                    }
                }

                if (entry == null) {
                    entry = new Entry { guid = key, current = current, history = new List<TypeIdentity> { current } };
                    entries.Add(entry);
                    return true;
                }

                if (entry.current.Equals(current)) return false;
                if (!entry.history.Contains(current)) entry.history.Add(current);
                if (!entry.history.Contains(entry.current)) entry.history.Add(entry.current);
                entry.current = current;
                return true;
            }
        }

        private const string EditorPrefsKey = "StaticEcsTypeGuid_ConfigGUID";
        private const string DefaultPath = "Assets/Editor/StaticEcsTypeGuidRegistry.asset";

        [SerializeField]
        internal List<WorldSettings> worlds = new();

        private static StaticEcsTypeGuidRegistry _cached;

        internal static StaticEcsTypeGuidRegistry Active {
            get {
                if (_cached == null) _cached = LoadOrCreate();
                return _cached;
            }
        }

        internal WorldSettings GetOrCreate(Type worldType) {
            foreach (var worldData in worlds) {
                if (worldData.worldTypeFullName == worldType.FullName) return worldData;
            }

            var settings = new WorldSettings { worldTypeFullName = worldType.FullName };
            worlds.Add(settings);
            return settings;
        }

        internal static StaticEcsTypeGuidRegistry LoadOrCreate() {
            var guid = EditorPrefs.GetString(EditorPrefsKey, "");
            if (!string.IsNullOrEmpty(guid)) {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (!string.IsNullOrEmpty(path)) {
                    var config = AssetDatabase.LoadAssetAtPath<StaticEcsTypeGuidRegistry>(path);
                    if (config != null) return config;
                }
            }

            return CreateInstance<StaticEcsTypeGuidRegistry>();
        }

        internal void Save() {
            if (!AssetDatabase.Contains(this)) {
                EditorApplication.delayCall += PersistNewAsset;
            } else {
                EditorUtility.SetDirty(this);
                AssetDatabase.SaveAssetIfDirty(this);
            }
        }

        private void PersistNewAsset() {
            if (AssetDatabase.Contains(this)) return;
            EnsureDirectory();
            AssetDatabase.CreateAsset(this, DefaultPath);
            AssetDatabase.SaveAssets();
            EditorPrefs.SetString(EditorPrefsKey, AssetDatabase.AssetPathToGUID(DefaultPath));
        }

        private static void EnsureDirectory() {
            var dir = System.IO.Path.GetDirectoryName(DefaultPath);
            if (!string.IsNullOrEmpty(dir) && !AssetDatabase.IsValidFolder(dir)) {
                var parts = dir.Replace('\\', '/').Split('/');
                var current = parts[0];
                for (var i = 1; i < parts.Length; i++) {
                    var next = current + "/" + parts[i];
                    if (!AssetDatabase.IsValidFolder(next)) {
                        AssetDatabase.CreateFolder(current, parts[i]);
                    }

                    current = next;
                }
            }
        }
    }
}