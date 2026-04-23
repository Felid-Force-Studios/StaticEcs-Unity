using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FFS.Libraries.StaticEcs.Unity.Editor {

    public enum ComponentFoldoutMode : byte {
        ExpandAll,
        CollapseAll,
        Custom
    }

    [Serializable]
    public class MainSettings {
        public string selectedTabName;
        public float drawRate = 0.5f;
        public float drawFrames = 2;
    }

    [Serializable]
    public class EntitiesSettings {
        public List<string> componentColumns = new();
        public List<string> tagColumns = new();
        public List<string> showTableDataTypes = new();
        public string sortByType;
        public int maxEntityResult = 100;
        public List<ulong> pinnedEntities = new();
        public bool filterActive;
        public bool gidFilterActive;
        public int gidFilterValue;
        public List<string> filterAll = new();
        public List<string> filterAllOnlyDisabled = new();
        public List<string> filterAllWithDisabled = new();
        public List<string> filterNone = new();
        public List<string> filterNoneWithDisabled = new();
        public List<string> filterAny = new();
        public List<string> filterAnyOnlyDisabled = new();
        public List<string> filterAnyWithDisabled = new();
        public ComponentFoldoutMode foldoutMode = ComponentFoldoutMode.CollapseAll;
        public List<string> autoExpandComponentTypes = new();
        public bool sortComponents;
        public bool defaultGroupExpanded = true;
        public List<string> groupOverrides = new();
    }

    [Serializable]
    public class EventsSettings {
        public bool latest = true;
        public List<string> filterTypeNames = new();
    }

    [Serializable]
    public class StatsSettings {
        public bool showNotRegistered;
        public int fragmentationThreshold = 512;
        public bool foldoutEntityTypes = true;
        public bool foldoutEvents = true;
        public bool foldoutComponents = true;
        public bool foldoutTags = true;
    }

    [Serializable]
    public class SystemsSettings {
        public int drawLevel = 10;
    }

    [Serializable]
    public class WorldViewSettings {
        public string worldTypeFullName;
        public MainSettings main = new();
        public EntitiesSettings entities = new();
        public EventsSettings events = new();
        public StatsSettings stats = new();
        public SystemsSettings systems = new();
    }

    [CreateAssetMenu(menuName = "Static ECS/View Config", fileName = "StaticEcsViewConfig")]
    public class StaticEcsViewConfig : ScriptableObject {
        private const string EditorPrefsKey = "StaticEcsView_ConfigGUID";
        private const string DefaultPath = "Assets/Editor/StaticEcsViewConfig.asset";

        [SerializeField] internal List<WorldViewSettings> worlds = new();

        private static StaticEcsViewConfig _cached;

        internal static StaticEcsViewConfig Active {
            get {
                if (_cached == null) _cached = LoadOrCreate();
                return _cached;
            }
        }

        internal WorldViewSettings GetOrCreate(string worldTypeFullName) {
            foreach (var w in worlds) {
                if (w.worldTypeFullName == worldTypeFullName) return w;
            }

            var settings = new WorldViewSettings { worldTypeFullName = worldTypeFullName };
            worlds.Add(settings);
            MarkDirty();
            return settings;
        }

        internal static StaticEcsViewConfig LoadOrCreate() {
            var guid = EditorPrefs.GetString(EditorPrefsKey, "");
            if (!string.IsNullOrEmpty(guid)) {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (!string.IsNullOrEmpty(path)) {
                    var config = AssetDatabase.LoadAssetAtPath<StaticEcsViewConfig>(path);
                    if (config != null) return config;
                }
            }

            var guids = AssetDatabase.FindAssets("t:StaticEcsViewConfig");
            if (guids.Length > 0) {
                var path = AssetDatabase.GUIDToAssetPath(guids[0]);
                var config = AssetDatabase.LoadAssetAtPath<StaticEcsViewConfig>(path);
                if (config != null) {
                    EditorPrefs.SetString(EditorPrefsKey, guids[0]);
                    return config;
                }
            }

            return CreateDefault();
        }

        private static StaticEcsViewConfig CreateDefault() {
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

            var config = CreateInstance<StaticEcsViewConfig>();
            AssetDatabase.CreateAsset(config, DefaultPath);
            AssetDatabase.SaveAssets();
            var newGuid = AssetDatabase.AssetPathToGUID(DefaultPath);
            EditorPrefs.SetString(EditorPrefsKey, newGuid);
            return config;
        }

        internal static void SetActive(StaticEcsViewConfig config) {
            if (config == null) return;
            _cached = config;
            var path = AssetDatabase.GetAssetPath(config);
            var guid = AssetDatabase.AssetPathToGUID(path);
            EditorPrefs.SetString(EditorPrefsKey, guid);
        }

        internal void MarkDirty() {
            EditorUtility.SetDirty(this);
        }

        internal void Save() {
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssetIfDirty(this);
        }
    }
}
