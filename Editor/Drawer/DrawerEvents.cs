using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FFS.Libraries.StaticEcs.Unity.Editor {
    public static partial class Drawer {
        private static readonly List<ManagedReferenceMissingType> _eventMissingPool = new();
        private static readonly HashSet<long> _eventShowData = new();

        public static void DrawEvent<TWorld, TProvider>(
            TProvider provider, DrawMode mode, Action<TProvider> onClickBuild, Action<TProvider> onCopyTemplate = null
        ) where TProvider : StaticEcsEventProvider<TWorld>
          where TWorld : struct, IWorldType {
            if (mode != DrawMode.Inspector) {
                provider.Scroll = EditorGUILayout.BeginScrollView(provider.Scroll);
            }
            EditorGUILayout.Space(10);

            if (provider.EventTemplate == null && provider.RuntimeEvent.IsEmpty()) {
                if (!TryDrawBrokenEventSlot<TWorld, TProvider>(provider)) {
                    EditorGUILayout.HelpBox("Please, provide event type", MessageType.Warning, true);
                }
            }

            Type knownEventType = null;
            if (!provider.RuntimeEvent.IsEmpty()) {
                knownEventType = provider.RuntimeEvent.Type;
            } else if (provider.EventTemplate != null) {
                knownEventType = provider.EventTemplate.GetType();
            }

            EditorGUILayout.BeginHorizontal();
            {
                var allowChangeEventType = provider.RuntimeEvent.IsEmpty();
                using (Ui.EnabledScopeVal(allowChangeEventType)) {
                    if (Ui.PlusButton && allowChangeEventType) {
                        DrawEventsMenu<TWorld, TProvider>(provider);
                    }
                }

                EditorGUILayout.LabelField("Type:", Ui.WidthLine(60));
                if (!provider.RuntimeEvent.IsEmpty()) {
                    EditorGUILayout.LabelField(knownEventType.EditorTypeName(), Ui.LabelStyleThemeBold);
                } else if (provider.EventTemplate != null) {
                    EditorGUILayout.LabelField(knownEventType.EditorTypeName(), Ui.LabelStyleThemeBold, Ui.WidthLine(200));
                    if (Application.isPlaying && GUILayout.Button("Send", Ui.ButtonStyleYellow, Ui.WidthLine(60))) {
                        onClickBuild(provider);
                    }
                } else {
                    EditorGUILayout.LabelField("---", Ui.LabelStyleThemeBold);
                }
            }
            EditorGUILayout.EndHorizontal();

            if (!provider.RuntimeEvent.IsEmpty()) {
                EditorGUILayout.BeginHorizontal();
                {
                    if (Ui.MenuButton) {
                        var menu = new GenericMenu();
                        if (mode == DrawMode.Viewer) {
                            menu.AddItem(new GUIContent("Close"), false, () => {
                                provider.RuntimeEvent = RuntimeEvent.Empty;
                                provider.EventCache = null;
                            });
                            menu.AddItem(new GUIContent("Send as new event"), false, () => {
                                var actualEvent = provider.GetActualEvent(out var _);
                                if (World<TWorld>._TryGetEventsHandle(actualEvent.GetType(), out var eventsHandle)) {
                                    eventsHandle.AddRaw(actualEvent);
                                    provider.EventCache = actualEvent;
                                    provider.RuntimeEvent = new RuntimeEvent {
                                        InternalIdx = eventsHandle.Last(),
                                        Type = actualEvent.GetType(),
                                        Status = EventStatus.Sent
                                    };
                                }
                            });
                        }

                        if (!provider.IsCached()) {
                            menu.AddItem(new GUIContent("Delete event"), false, provider.DeleteEvent);
                        }

                        menu.ShowAsContext();
                    }

                    EditorGUILayout.LabelField("Event:", Ui.WidthLine(60));
                    EditorGUILayout.LabelField(provider.IsCached() ? "Read or suppressed" : "Sent", Ui.LabelStyleThemeBold);
                }
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.Space();
            Ui.DrawHorizontalSeparator();

            if (knownEventType != null) {
                EditorGUILayout.BeginVertical(GUI.skin.box);
                MetaData.DrawSourceField(knownEventType);
                EditorGUILayout.EndVertical();
            }

            if (provider.EventIsActual(Application.isPlaying)) {
                EditorGUILayout.Space(10);
                DrawEventValue<TWorld, TProvider>(provider);
            }

            if (mode != DrawMode.Inspector) {
                EditorGUILayout.EndScrollView();
            }
        }

        private static void DrawEventValue<TWorld, TEntityProvider>(TEntityProvider provider) where TEntityProvider : StaticEcsEventProvider<TWorld> where TWorld : struct, IWorldType {
            var eventValue = provider.GetActualEvent(out var cached);
            var type = eventValue.GetType();
            var typeName = type.EditorTypeName();

            using (Ui.EnabledScopeVal(!cached)) {
                EditorGUILayout.BeginVertical(GUI.skin.box);
                {
                    var wrapper = EventDrawerWrapper.GetFor(provider.GetEntityId());
                    using var so = new SerializedObject(wrapper);
                    var prop = so.FindProperty("value");
                    prop.managedReferenceValue = null;
                    so.ApplyModifiedPropertiesWithoutUndo();
                    prop.managedReferenceValue = eventValue;
                    so.ApplyModifiedPropertiesWithoutUndo();
                    so.Update();
                    prop = so.FindProperty("value");

                    DrawSerializedPropertyChildren(prop);

                    if (so.ApplyModifiedProperties()) {
                        provider.OnChangeEvent(wrapper.value);
                        EditorUtility.SetDirty(provider);
                    }
                }
                EditorGUILayout.EndVertical();
            }
        }

        private static bool TryDrawBrokenEventSlot<TWorld, TProvider>(TProvider provider)
            where TProvider : StaticEcsEventProvider<TWorld>
            where TWorld : struct, IWorldType {

            if (PrefabUtility.IsPartOfPrefabAsset(provider)) {
                EditorGUILayout.BeginVertical(GUI.skin.box);
                EditorGUILayout.HelpBox(
                    "Broken event template.\n" +
                    "Prefab has missing SerializeReference types. Unity restricts access to managed references from the Project view — " +
                    "editing here would cause data loss. Open the prefab to fix.",
                    MessageType.Warning);
                if (GUILayout.Button("Open Prefab", Ui.ButtonStyleTheme)) {
                    var path = AssetDatabase.GetAssetPath(provider);
                    var prefab = AssetDatabase.LoadMainAssetAtPath(path);
                    if (prefab != null) AssetDatabase.OpenAsset(prefab);
                    GUIUtility.ExitGUI();
                }
                EditorGUILayout.EndVertical();
                return true;
            }

            MissingReferenceMigration.FillMissing(provider, _eventMissingPool);
            if (_eventMissingPool.Count == 0) return false;

            ManagedReferenceMissingType missing = default;
            var found = false;
            using (var so = new SerializedObject(provider)) {
                var prop = so.FindProperty("eventTemplate");
                if (prop == null || prop.propertyType != SerializedPropertyType.ManagedReference) return false;
                if (_eventMissingPool.Find(type => type.referenceId == prop.managedReferenceId).referenceId != 0) found = true;
            }
            if (!found) {
                missing = _eventMissingPool[0];
            }

            var ns = string.IsNullOrEmpty(missing.namespaceName) ? "" : missing.namespaceName + ".";
            EditorGUILayout.HelpBox($"Missing event type:\n{ns}{missing.className} (asm: {missing.assemblyName})", MessageType.Warning);

            Type autoType = null;
            GuidTypeRegistry.TryFindCurrentByMissingIdentity(missing.className, missing.namespaceName, missing.assemblyName, ConfigKind.Event, out autoType);
            if (autoType != null) {
                EditorGUILayout.HelpBox($"Auto-match by GUID → {autoType.FullName}", MessageType.Info);
                if (GUILayout.Button("Apply auto-migration", Ui.ButtonStyleTheme)) {
                    MissingReferenceMigration.TryMigrateSlot(provider, missing, autoType);
                    GUIUtility.ExitGUI();
                }
            }

            var showKey = provider.GetEntityId().GetHashCode() ^ missing.referenceId.GetHashCode();
            var expanded = _eventShowData.Contains(showKey);

            EditorGUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Replace with...", Ui.ButtonStyleTheme)) {
                    ShowBrokenEventReplaceMenu<TWorld, TProvider>(provider, missing);
                }
                if (GUILayout.Button("Remove", Ui.ButtonStyleTheme)) {
                    using (var so = new SerializedObject(provider)) {
                        var prop = so.FindProperty("eventTemplate");
                        if (prop != null) {
                            prop.managedReferenceValue = null;
                            so.ApplyModifiedProperties();
                        }
                    }
                    MissingReferenceMigration.CleanAllMissing(provider);
                    EditorUtility.SetDirty(provider);
                    GUIUtility.ExitGUI();
                }
                if (GUILayout.Button(expanded ? "Hide data" : "Show data", Ui.ButtonStyleTheme)) {
                    if (expanded) _eventShowData.Remove(showKey);
                    else _eventShowData.Add(showKey);
                }
            }
            EditorGUILayout.EndHorizontal();

            if (expanded) {
                var text = string.IsNullOrEmpty(missing.serializedData) ? "(empty)" : missing.serializedData;
                EditorGUILayout.TextArea(text, EditorStyles.textArea);
            }
            return true;
        }

        private static void ShowBrokenEventReplaceMenu<TWorld, TProvider>(TProvider provider, ManagedReferenceMissingType missing)
            where TProvider : StaticEcsEventProvider<TWorld>
            where TWorld : struct, IWorldType {

            var worldMeta = MetaData.GetWorldMetaData(typeof(TWorld));
            var items = new List<SearchableDropdown.Item>(worldMeta.Events.Count);
            foreach (var eventDataMeta in worldMeta.Events) {
                items.Add(new SearchableDropdown.Item(eventDataMeta.FullName, eventDataMeta.Type, true));
            }

            var capturedMissing = missing;
            SearchableDropdown.Show("Replace broken event", items, payload => {
                var t = (Type) payload;
                var migrated = MissingReferenceMigration.TryMigrateSlot(provider, capturedMissing, t);
                if (!migrated) {
                    var raw = (IEvent) Activator.CreateInstance(t, true);
                    using (var so = new SerializedObject(provider)) {
                        var prop = so.FindProperty("eventTemplate");
                        if (prop != null) {
                            prop.managedReferenceValue = raw;
                            so.ApplyModifiedProperties();
                        }
                    }
                    MissingReferenceMigration.CleanAllMissing(provider);
                    EditorUtility.SetDirty(provider);
                }
            });
        }

        private static void DrawEventsMenu<TWorld, TEntityProvider>(TEntityProvider provider) where TEntityProvider : StaticEcsEventProvider<TWorld> where TWorld : struct, IWorldType {
            var worldMeta = MetaData.GetWorldMetaData(typeof(TWorld));
            var items = new List<SearchableDropdown.Item>(worldMeta.Events.Count);
            foreach (var eventDataMeta in worldMeta.Events) {
                if (provider.EventTemplate != null && provider.EventTemplate.GetType() == eventDataMeta.Type) {
                    continue;
                }

                var enabled = provider.ShouldShowEvent(eventDataMeta.Type, Application.isPlaying);
                items.Add(new SearchableDropdown.Item(eventDataMeta.FullName, eventDataMeta.Type, enabled));
            }

            SearchableDropdown.Show("Events", items, payload => {
                var objRaw = Activator.CreateInstance((Type) payload, true);
                provider.OnSelectEvent((IEvent) objRaw);
                EditorUtility.SetDirty(provider);
            });
        }
    }
}
