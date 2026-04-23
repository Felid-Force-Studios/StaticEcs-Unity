using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FFS.Libraries.StaticEcs.Unity.Editor {

    public class BrokenProvidersFixerWindow : EditorWindow {

        [MenuItem("Tools/Static ECS/Fix Broken Providers")]
        private static void Open() {
            var w = GetWindow<BrokenProvidersFixerWindow>();
            w.titleContent = new GUIContent("Fix Broken Providers");
            w.Show();
        }

        private enum ScanMode { ActiveScene, PrefabsFolder }

        private const int EventSlot = -1;
        private const int UnknownSlot = -2;

        private class Occurrence {
            public AbstractStaticEcsProvider Provider;
            public string ProviderPath;
            public string SourcePath;
            public int SlotIndex;
            public ManagedReferenceMissingType Missing;
            public GameObject PrefabAssetRoot; // null in scene mode; set in folder scan
        }

        private class Group {
            public string ClassName;
            public string Ns;
            public string Asm;
            public ConfigKind Kind;
            public bool KindKnown = true;
            public Type AutoType;
            public List<Occurrence> Occurrences = new();
            public bool Expanded;
        }

        private readonly List<Group> _groups = new();
        private readonly HashSet<string> _seenOccurrences = new();
        private Vector2 _scroll;

        private ScanMode _scanMode = ScanMode.ActiveScene;
        private DefaultAsset _folder;
        private string _folderWarning;

        private void OnEnable() {
            EditorApplication.delayCall += DeferredInitialRescan;
            EditorSceneManager.sceneOpened += OnSceneOpened;
            EditorSceneManager.sceneClosed += OnSceneClosed;
        }

        private void OnDisable() {
            EditorApplication.delayCall -= DeferredInitialRescan;
            EditorSceneManager.sceneOpened -= OnSceneOpened;
            EditorSceneManager.sceneClosed -= OnSceneClosed;
        }

        private void OnFocus() {
            // External edits (provider inspector fixes, manual prefab edits, scene saves) can leave
            // _groups stale. Rescan when the window regains focus, but defer to avoid Unity's YAML
            // assertion during compile/update phases.
            EditorApplication.delayCall += DeferredInitialRescan;
        }

        private void OnSceneOpened(UnityEngine.SceneManagement.Scene scene, OpenSceneMode mode) {
            EditorApplication.delayCall += DeferredInitialRescan;
        }

        private void OnSceneClosed(UnityEngine.SceneManagement.Scene scene) {
            EditorApplication.delayCall += DeferredInitialRescan;
        }

        private void DeferredInitialRescan() {
            if (this == null) return;
            if (EditorApplication.isCompiling || EditorApplication.isUpdating) {
                EditorApplication.delayCall += DeferredInitialRescan;
                return;
            }
            Rescan();
        }

        private void OnGUI() {
            DrawToolbar();
            DrawModeBar();
            DrawStats();
            EditorGUILayout.Space(4);

            if (_groups.Count == 0) {
                var msg = _scanMode == ScanMode.ActiveScene
                    ? "No broken providers in the active scene."
                    : "No broken providers in the selected folder.";
                EditorGUILayout.HelpBox(msg, MessageType.Info);
                return;
            }

            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            for (var i = 0; i < _groups.Count; i++) DrawGroup(_groups[i]);
            EditorGUILayout.EndScrollView();
        }

        private void DrawToolbar() {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            if (GUILayout.Button("Rescan", EditorStyles.toolbarButton, GUILayout.Width(80))) Rescan();
            GUILayout.FlexibleSpace();
            var autoGroups = CountAutoGroups(out var autoOccurrences);
            using (new EditorGUI.DisabledScope(autoGroups == 0)) {
                if (GUILayout.Button($"Auto-fix all by GUID ({autoGroups} groups, {autoOccurrences} slots)",
                        EditorStyles.toolbarButton, GUILayout.Width(280))) {
                    AutoFixAll();
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawModeBar() {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            // EditorGUILayout.BeginHorizontal();
            // EditorGUILayout.LabelField("Scan mode", GUILayout.Width(80));
            // var newMode = (ScanMode) EditorGUILayout.EnumPopup(_scanMode, GUILayout.Width(160));
            // if (newMode != _scanMode) {
            //     _scanMode = newMode;
            //     _folderWarning = null;
            //     Rescan();
            // }
            // EditorGUILayout.EndHorizontal();

            if (_scanMode == ScanMode.PrefabsFolder) {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Folder", GUILayout.Width(80));
                var newFolder = (DefaultAsset) EditorGUILayout.ObjectField(_folder, typeof(DefaultAsset), false);
                if (newFolder != _folder) {
                    if (newFolder == null) {
                        _folder = null;
                        _folderWarning = null;
                        Rescan();
                    } else {
                        var path = AssetDatabase.GetAssetPath(newFolder);
                        if (!string.IsNullOrEmpty(path) && AssetDatabase.IsValidFolder(path)) {
                            _folder = newFolder;
                            _folderWarning = null;
                            Rescan();
                        } else {
                            _folder = null;
                            _folderWarning = "Selected asset is not a folder. Reset to default (Assets/).";
                        }
                    }
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.LabelField("Default: Assets/  (includes prefab variants)", EditorStyles.miniLabel);
                if (!string.IsNullOrEmpty(_folderWarning)) {
                    EditorGUILayout.HelpBox(_folderWarning, MessageType.Warning);
                }
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawStats() {
            var totalSlots = 0;
            var autoTypes = 0;
            for (var i = 0; i < _groups.Count; i++) {
                totalSlots += _groups[i].Occurrences.Count;
                if (_groups[i].AutoType != null) autoTypes++;
            }
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Statistics", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Missing type groups: {_groups.Count}");
            EditorGUILayout.LabelField($"Total broken slots: {totalSlots}");
            EditorGUILayout.LabelField($"Auto-matchable groups: {autoTypes}");
            EditorGUILayout.EndVertical();

            if (_scanMode == ScanMode.ActiveScene && SceneManager.sceneCount > 1) {
                EditorGUILayout.HelpBox(
                    $"Multi-scene editing detected: scanning only active scene '{SceneManager.GetActiveScene().name}'. Other loaded scenes are ignored.",
                    MessageType.Info);
            }
        }

        private int CountAutoGroups(out int occurrences) {
            var groups = 0;
            occurrences = 0;
            for (var i = 0; i < _groups.Count; i++) {
                if (_groups[i].AutoType == null) continue;
                groups++;
                occurrences += _groups[i].Occurrences.Count;
            }
            return groups;
        }

        private void DrawGroup(Group g) {
            EditorGUILayout.BeginVertical(GUI.skin.box);

            EditorGUILayout.BeginHorizontal();
            g.Expanded = EditorGUILayout.Foldout(g.Expanded, GroupHeaderLabel(g), true);
            EditorGUILayout.EndHorizontal();

            if (g.AutoType != null) {
                EditorGUILayout.HelpBox($"Auto-match by GUID → {GuidTypeRegistry.PrettyTypeName(g.AutoType)}", MessageType.Info);
            }
            if (!g.KindKnown) {
                EditorGUILayout.HelpBox("Wrapper kind unknown — Replace menu shows all registered types.", MessageType.Warning);
            }

            EditorGUILayout.BeginHorizontal();
            using (new EditorGUI.DisabledScope(g.AutoType == null)) {
                if (GUILayout.Button("Auto-fix group", GUILayout.Height(22))) {
                    AutoFixGroup(g);
                    GUIUtility.ExitGUI();
                }
            }
            if (GUILayout.Button("Replace group with...", GUILayout.Height(22))) {
                ShowReplaceMenu(g);
            }
            if (GUILayout.Button("Remove group", GUILayout.Height(22))) {
                RemoveGroup(g);
                GUIUtility.ExitGUI();
            }
            EditorGUILayout.EndHorizontal();

            if (g.Expanded) {
                EditorGUI.indentLevel++;
                for (var i = 0; i < g.Occurrences.Count; i++) DrawOccurrence(g, g.Occurrences[i]);
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawOccurrence(Group g, Occurrence o) {
            EditorGUILayout.BeginHorizontal();
            string slotLabel;
            if (o.SlotIndex == EventSlot) slotLabel = "event";
            else if (o.SlotIndex == UnknownSlot) slotLabel = "?";
            else slotLabel = $"#{o.SlotIndex}";
            var line = $"{o.SourcePath}  —  {o.ProviderPath}  [{slotLabel}]";
            EditorGUILayout.LabelField(line, EditorStyles.miniLabel);
            if (GUILayout.Button(new GUIContent("⊙", "Ping"), EditorStyles.miniButton, GUILayout.Width(28))) {
                if (o.Provider != null) EditorGUIUtility.PingObject(o.Provider);
            }
            EditorGUILayout.EndHorizontal();
        }

        private static string GroupHeaderLabel(Group g) {
            var kindLabel = g.KindKnown ? g.Kind.ToString() : $"{g.Kind}?";
            return $"[{kindLabel}] {GuidTypeRegistry.PrettyMissingClassName(g.ClassName)}  (asm: {g.Asm})  ×{g.Occurrences.Count}";
        }

        private void AutoFixAll() {
            var paths = new HashSet<string>();
            var fixedOccurrences = new HashSet<Occurrence>();
            var applied = 0;
            var failed = 0;
            for (var i = 0; i < _groups.Count; i++) {
                var g = _groups[i];
                if (g.AutoType == null) continue;
                foreach (var o in g.Occurrences) {
                    if (o.Provider == null) continue;
                    if (MissingReferenceMigration.TryMigrateSlotBatch(o.Provider, o.Missing, g.AutoType, out var path)) {
                        if (!string.IsNullOrEmpty(path)) paths.Add(path);
                        fixedOccurrences.Add(o);
                        applied++;
                    } else {
                        failed++;
                        Debug.LogWarning($"[StaticEcs] Auto-fix failed: {o.SourcePath} — {o.ProviderPath} — rid {o.Missing.referenceId} ({g.ClassName} → {g.AutoType.FullName})");
                    }
                }
            }
            MissingReferenceMigration.FinishBatch(paths);
            Debug.Log($"[StaticEcs] Auto-fix all: applied {applied}, failed {failed}, across {paths.Count} file(s).");
            FinalizeAfterBatch(paths.Count > 0, fixedOccurrences);
        }

        private void AutoFixGroup(Group g) {
            if (g.AutoType == null) return;
            var paths = new HashSet<string>();
            var fixedOccurrences = new HashSet<Occurrence>();
            var applied = 0;
            var failed = 0;
            foreach (var o in g.Occurrences) {
                if (o.Provider == null) continue;
                if (MissingReferenceMigration.TryMigrateSlotBatch(o.Provider, o.Missing, g.AutoType, out var path)) {
                    if (!string.IsNullOrEmpty(path)) paths.Add(path);
                    fixedOccurrences.Add(o);
                    applied++;
                } else {
                    failed++;
                    Debug.LogWarning($"[StaticEcs] Auto-fix failed: {o.SourcePath} — {o.ProviderPath} — rid {o.Missing.referenceId} ({g.ClassName} → {g.AutoType.FullName})");
                }
            }
            MissingReferenceMigration.FinishBatch(paths);
            Debug.Log($"[StaticEcs] Auto-fix group: applied {applied}, failed {failed} → {g.AutoType.FullName}");
            FinalizeAfterBatch(paths.Count > 0, fixedOccurrences);
        }

        private void ShowReplaceMenu(Group g) {
            var items = new List<SearchableDropdown.Item>();
            var seen = new HashSet<Type>();
            if (g.KindKnown) {
                foreach (var t in CollectTypesForKind(g.Kind)) {
                    if (seen.Add(t)) items.Add(new SearchableDropdown.Item(t.EditorFullTypeName(), t, true));
                }
            } else {
                foreach (var k in (ConfigKind[]) Enum.GetValues(typeof(ConfigKind))) {
                    foreach (var t in CollectTypesForKind(k)) {
                        if (seen.Add(t)) items.Add(new SearchableDropdown.Item($"[{k}] {t.EditorFullTypeName()}", t, true));
                    }
                }
            }
            if (items.Count == 0) {
                Debug.LogWarning($"[StaticEcs] No registered types for kind {g.Kind}.");
                return;
            }
            SearchableDropdown.Show($"Replace {g.Kind}: {g.ClassName}", items, payload => {
                var newType = (Type) payload;
                var paths = new HashSet<string>();
                var fixedOccurrences = new HashSet<Occurrence>();
                var applied = 0;
                var failed = 0;
                foreach (var o in g.Occurrences) {
                    if (o.Provider == null) continue;
                    if (MissingReferenceMigration.TryMigrateSlotBatch(o.Provider, o.Missing, newType, out var path)) {
                        if (!string.IsNullOrEmpty(path)) paths.Add(path);
                        fixedOccurrences.Add(o);
                        applied++;
                    } else {
                        failed++;
                        Debug.LogWarning($"[StaticEcs] Replace failed: {o.SourcePath} — {o.ProviderPath} — rid {o.Missing.referenceId} ({g.ClassName} → {newType.FullName})");
                    }
                }
                MissingReferenceMigration.FinishBatch(paths);
                Debug.Log($"[StaticEcs] Manual replace group: applied {applied}, failed {failed} → {newType.FullName}");
                FinalizeAfterBatch(paths.Count > 0, fixedOccurrences);
            });
        }

        private void RemoveGroup(Group g) {
            var byProvider = new Dictionary<AbstractStaticEcsProvider, List<Occurrence>>();
            foreach (var o in g.Occurrences) {
                if (o.Provider == null) continue;
                if (!byProvider.TryGetValue(o.Provider, out var list)) {
                    list = new List<Occurrence>();
                    byProvider[o.Provider] = list;
                }
                list.Add(o);
            }

            var prefabRootsToSave = new HashSet<GameObject>();
            var fixedOccurrences = new HashSet<Occurrence>();
            var totalRemoved = 0;
            var totalFailed = 0;

            foreach (var kv in byProvider) {
                var prov = kv.Key;
                var occurrences = kv.Value;

                using var so = new SerializedObject(prov);
                var listProp = so.FindProperty("providers");
                var eventProp = so.FindProperty("eventTemplate");

                // Bucket: events first, then unknowns (targeted clear by refId), then real indices desc.
                var realSlots = new List<Occurrence>();
                var unknownSlots = new List<Occurrence>();
                var hasEvent = false;
                foreach (var o in occurrences) {
                    if (o.SlotIndex == EventSlot) hasEvent = true;
                    else if (o.SlotIndex == UnknownSlot) unknownSlots.Add(o);
                    else if (o.SlotIndex >= 0) realSlots.Add(o);
                }
                realSlots.Sort((a, b) => b.SlotIndex.CompareTo(a.SlotIndex));

                if (hasEvent && eventProp != null && eventProp.propertyType == SerializedPropertyType.ManagedReference) {
                    eventProp.managedReferenceValue = null;
                    foreach (var o in occurrences) if (o.SlotIndex == EventSlot) fixedOccurrences.Add(o);
                    totalRemoved++;
                }

                foreach (var o in unknownSlots) {
                    if (TryClearMissingReferenceById(so, o.Missing.referenceId)) {
                        fixedOccurrences.Add(o);
                        totalRemoved++;
                    } else {
                        totalFailed++;
                        Debug.LogWarning($"[StaticEcs] Remove: cannot locate slot for rid {o.Missing.referenceId} on {o.ProviderPath} ({o.SourcePath}). Skipped.");
                    }
                }

                foreach (var o in realSlots) {
                    if (listProp == null || !listProp.isArray || o.SlotIndex >= listProp.arraySize) {
                        totalFailed++;
                        continue;
                    }
                    var before = listProp.arraySize;
                    listProp.DeleteArrayElementAtIndex(o.SlotIndex);
                    if (listProp.arraySize == before) {
                        // Managed-ref array: first delete may only null the slot. Do NOT delete again,
                        // that would shift indices and remove a neighbor.
                        Debug.LogWarning($"[StaticEcs] Remove: slot #{o.SlotIndex} on {o.ProviderPath} was nulled, not removed (Unity behavior). May need a second Remove pass.");
                    }
                    fixedOccurrences.Add(o);
                    totalRemoved++;
                }

                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(prov);

                var prefabRoot = ResolvePrefabRootForSave(prov, occurrences);
                if (prefabRoot != null) prefabRootsToSave.Add(prefabRoot);
            }

            foreach (var root in prefabRootsToSave) {
                PrefabUtility.SavePrefabAsset(root);
            }

            Debug.Log($"[StaticEcs] Removed group {g.ClassName}: removed {totalRemoved}, failed {totalFailed}, saved {prefabRootsToSave.Count} prefab(s).");
            FinalizeAfterBatch(prefabRootsToSave.Count > 0, fixedOccurrences);
        }

        // Asset reserialize/refresh invalidates cached Object references on remaining occurrences,
        // making o.Provider == null in subsequent batch operations. After any persistent change rescan.
        private void FinalizeAfterBatch(bool assetsChanged, HashSet<Occurrence> fixedOccurrences) {
            if (assetsChanged) Rescan();
            else DropOccurrencesAndRepaint(fixedOccurrences);
        }

        private void DropOccurrencesAndRepaint(HashSet<Occurrence> fixedOccurrences) {
            if (fixedOccurrences.Count == 0) {
                Repaint();
                return;
            }
            for (var i = _groups.Count - 1; i >= 0; i--) {
                var g = _groups[i];
                g.Occurrences.RemoveAll(o => fixedOccurrences.Contains(o));
                if (g.Occurrences.Count == 0) _groups.RemoveAt(i);
            }
            Repaint();
        }

        private static GameObject ResolvePrefabRootForSave(AbstractStaticEcsProvider prov, List<Occurrence> occurrences) {
            // Prefer the asset root captured at scan time (correct for nested prefabs/variants).
            for (var i = 0; i < occurrences.Count; i++) {
                if (occurrences[i].PrefabAssetRoot != null) return occurrences[i].PrefabAssetRoot;
            }
            // Fallback: if the provider itself is a prefab asset, walk via AssetDatabase.
            if (PrefabUtility.IsPartOfPrefabAsset(prov)) {
                var path = AssetDatabase.GetAssetPath(prov);
                if (!string.IsNullOrEmpty(path)) {
                    var root = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                    if (root != null) return root;
                }
            }
            return null;
        }

        private static bool TryClearMissingReferenceById(SerializedObject so, long refId) {
            var iter = so.GetIterator();
            var enter = true;
            while (iter.Next(enter)) {
                enter = true;
                if (iter.propertyType != SerializedPropertyType.ManagedReference) continue;
                if (iter.managedReferenceId != refId) continue;
                iter.managedReferenceValue = null;
                return true;
            }
            return false;
        }

        private static IEnumerable<Type> CollectTypesForKind(ConfigKind kind) {
            var seen = new HashSet<Type>();
            string wrapperBase = kind switch {
                ConfigKind.Link => "Link",
                ConfigKind.Links => "Links",
                ConfigKind.Multi => "Multi",
                _ => null,
            };
            foreach (var meta in MetaData.PerWorldMetaData.Values) {
                if (wrapperBase != null) {
                    foreach (var m in meta.Components) {
                        if (m.Type == null) continue;
                        if (!Drawer.IsWorldWrapperTypeMatch(m.Type, wrapperBase)) continue;
                        if (seen.Add(m.Type)) yield return m.Type;
                    }
                    continue;
                }
                switch (kind) {
                    case ConfigKind.Component:
                        foreach (var m in meta.Components) {
                            if (m.Type == null) continue;
                            if (Drawer.IsWorldWrapperTypeMatch(m.Type, null)) continue;
                            if (seen.Add(m.Type)) yield return m.Type;
                        }
                        break;
                    case ConfigKind.Tag:
                        foreach (var m in meta.Tags) if (m.Type != null && seen.Add(m.Type)) yield return m.Type;
                        break;
                    case ConfigKind.Event:
                        foreach (var m in meta.Events) if (m.Type != null && seen.Add(m.Type)) yield return m.Type;
                        break;
                }
            }
        }

        private void Rescan() {
            _groups.Clear();
            _seenOccurrences.Clear();
            var groupIndex = new Dictionary<(string, string, string, ConfigKind), Group>();

            switch (_scanMode) {
                case ScanMode.ActiveScene:
                    RescanActiveScene(groupIndex);
                    break;
                case ScanMode.PrefabsFolder:
                    RescanPrefabsFolder(groupIndex);
                    break;
            }

            _groups.AddRange(groupIndex.Values);

            _groups.Sort((a, b) => {
                var k = a.Kind.CompareTo(b.Kind);
                if (k != 0) return k;
                return string.Compare(a.ClassName, b.ClassName, StringComparison.Ordinal);
            });
            Repaint();
        }

        private void RescanActiveScene(Dictionary<(string, string, string, ConfigKind), Group> groupIndex) {
            var scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || !scene.isLoaded) return;
            foreach (var root in scene.GetRootGameObjects()) {
                foreach (var prov in root.GetComponentsInChildren<AbstractStaticEcsProvider>(true)) {
                    ScanProvider(prov, scene.path, prefabAssetRoot: null, groupIndex);
                }
            }
        }

        private void RescanPrefabsFolder(Dictionary<(string, string, string, ConfigKind), Group> groupIndex) {
            var root = _folder != null ? AssetDatabase.GetAssetPath(_folder) : "Assets";
            if (string.IsNullOrEmpty(root) || !AssetDatabase.IsValidFolder(root)) return;

            var guids = AssetDatabase.FindAssets("t:Prefab", new[] { root });
            for (var i = 0; i < guids.Length; i++) {
                var path = AssetDatabase.GUIDToAssetPath(guids[i]);
                if (string.IsNullOrEmpty(path)) continue;
                var go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (go == null) continue;
                foreach (var prov in go.GetComponentsInChildren<AbstractStaticEcsProvider>(true)) {
                    ScanProvider(prov, path, prefabAssetRoot: go, groupIndex);
                }
            }
        }

        // Authoritative source of broken slots: SerializationUtility.GetManagedReferencesWithMissingTypes.
        // Every returned entry becomes an Occurrence; SlotIndex/Kind are best-effort via SerializedObject
        // (only used for display and Remove — migration uses referenceId).
        // Dedup by (assetGuid, fileId, refId) so providers reachable through multiple prefab scans (nested
        // prefabs, variants) are not counted twice.
        private void ScanProvider(AbstractStaticEcsProvider prov, string sourcePath, GameObject prefabAssetRoot,
                                   Dictionary<(string, string, string, ConfigKind), Group> groupIndex) {
            var missing = SerializationUtility.GetManagedReferencesWithMissingTypes(prov);
            if (missing == null || missing.Length == 0) return;

            var providerPath = $"{GetHierarchyPath(prov.gameObject)} / {prov.GetType().Name}";

            // refId → (slotIndex, kind, isWrapperMissing). Wrapper-missing entries get their kind
            // refined later from the missing entry's className (see TryDeriveKindFromWrapperClassName).
            var slotMap = new Dictionary<long, (int slot, ConfigKind kind, bool wrapperMissing)>();
            using (var so = new SerializedObject(prov)) {
                var listProp = so.FindProperty("providers");
                if (listProp != null && listProp.isArray) {
                    for (var i = 0; i < listProp.arraySize; i++) {
                        var elem = listProp.GetArrayElementAtIndex(i);
                        if (elem.propertyType != SerializedPropertyType.ManagedReference) continue;
                        var refVal = elem.managedReferenceValue;
                        if (refVal == null) {
                            var rid = elem.managedReferenceId;
                            if (rid > 0) slotMap[rid] = (i, ConfigKind.Component, true);
                        } else {
                            var kind = ResolveKindFromWrapper(refVal) ?? ConfigKind.Component;
                            var valueProp = elem.FindPropertyRelative("value");
                            if (valueProp != null
                                && valueProp.propertyType == SerializedPropertyType.ManagedReference
                                && valueProp.managedReferenceValue == null) {
                                var rid = valueProp.managedReferenceId;
                                if (rid > 0) slotMap[rid] = (i, kind, false);
                            }
                        }
                    }
                }
                var eventProp = so.FindProperty("eventTemplate");
                if (eventProp != null && eventProp.propertyType == SerializedPropertyType.ManagedReference
                    && eventProp.managedReferenceValue == null) {
                    var rid = eventProp.managedReferenceId;
                    if (rid > 0) slotMap[rid] = (EventSlot, ConfigKind.Event, false);
                }
            }

            var gid = GlobalObjectId.GetGlobalObjectIdSlow(prov);
            var gidPrefix = $"{gid.assetGUID}:{gid.targetObjectId}";

            foreach (var m in missing) {
                var dedupKey = $"{gidPrefix}:{m.referenceId}";
                if (!_seenOccurrences.Add(dedupKey)) continue;

                int slot;
                ConfigKind kind;
                var kindKnown = true;
                if (slotMap.TryGetValue(m.referenceId, out var info)) {
                    slot = info.slot;
                    kind = info.kind;
                    if (info.wrapperMissing) {
                        var derived = TryDeriveKindFromWrapperClassName(m.className);
                        if (derived.HasValue) kind = derived.Value;
                        else kindKnown = false;
                    }
                } else {
                    slot = UnknownSlot;
                    var derived = TryDeriveKindFromWrapperClassName(m.className);
                    if (derived.HasValue) kind = derived.Value;
                    else { kind = ConfigKind.Component; kindKnown = false; }
                }

                AddOccurrence(groupIndex, kind, kindKnown, m, new Occurrence {
                    Provider = prov,
                    ProviderPath = providerPath,
                    SourcePath = sourcePath,
                    SlotIndex = slot,
                    Missing = m,
                    PrefabAssetRoot = prefabAssetRoot,
                });
            }
        }

        private static void AddOccurrence(Dictionary<(string, string, string, ConfigKind), Group> groupIndex,
                                          ConfigKind kind, bool kindKnown, ManagedReferenceMissingType m, Occurrence occ) {
            // If the kind is unknown, try to recover it by exhaustive registry search — useful for tags
            // (no wrapper-encoded className) and any case where Unity's rid lookup misses the slotMap.
            Type recoveredType = null;
            if (!kindKnown && GuidTypeRegistry.TryFindAnyKindByMissingIdentity(m.className, m.namespaceName, m.assemblyName, out var recoveredKind, out recoveredType)) {
                kind = recoveredKind;
                kindKnown = true;
            }

            var key = (m.className ?? "", m.namespaceName ?? "", m.assemblyName ?? "", kind);
            if (!groupIndex.TryGetValue(key, out var group)) {
                group = new Group {
                    ClassName = m.className,
                    Ns = m.namespaceName,
                    Asm = m.assemblyName,
                    Kind = kind,
                    KindKnown = kindKnown,
                };
                if (recoveredType != null) {
                    group.AutoType = recoveredType;
                } else {
                    GuidTypeRegistry.TryFindCurrentByMissingIdentity(m.className, m.namespaceName, m.assemblyName, kind, out group.AutoType);
                }
                groupIndex[key] = group;
            } else if (kindKnown) {
                // If any occurrence knows the kind for sure, prefer that.
                group.KindKnown = true;
            }
            group.Occurrences.Add(occ);
        }

        private static ConfigKind? ResolveKindFromWrapper(object refVal) {
            if (refVal is ComponentProvider) return ConfigKind.Component;
            if (refVal is TagProvider) return ConfigKind.Tag;
            if (refVal is LinkProvider) return ConfigKind.Link;
            if (refVal is LinksProvider) return ConfigKind.Links;
            if (refVal is MultiProvider) return ConfigKind.Multi;
            return null;
        }

        private static ConfigKind? TryDeriveKindFromWrapperClassName(string className) {
            if (string.IsNullOrEmpty(className)) return null;
            switch (className) {
                case nameof(ComponentProvider): return ConfigKind.Component;
                case nameof(TagProvider): return ConfigKind.Tag;
                case nameof(LinkProvider): return ConfigKind.Link;
                case nameof(LinksProvider): return ConfigKind.Links;
                case nameof(MultiProvider): return ConfigKind.Multi;
            }
            // Unity-encoded inner wrapper class: "World`1/<Wrapper>`1[[...]]"
            if (className.StartsWith("World`1/Link`1", StringComparison.Ordinal)) return ConfigKind.Link;
            if (className.StartsWith("World`1/Links`1", StringComparison.Ordinal)) return ConfigKind.Links;
            if (className.StartsWith("World`1/Multi`1", StringComparison.Ordinal)) return ConfigKind.Multi;
            return null;
        }

        private static string GetHierarchyPath(GameObject go) {
            var sb = new System.Text.StringBuilder(go.name);
            var t = go.transform.parent;
            while (t != null) {
                sb.Insert(0, t.name + "/");
                t = t.parent;
            }
            return sb.ToString();
        }

    }
}
