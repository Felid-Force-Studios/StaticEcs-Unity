using System;
using UnityEditor;
using UnityEngine;

namespace FFS.Libraries.StaticEcs.Unity.Editor {
    public static partial class Drawer {
        public static void DrawEvent(StaticEcsEventProvider provider, DrawMode mode, Action<StaticEcsEventProvider> onClickBuild, Action<StaticEcsEventProvider> onCopyTemplate = null) {
            if (mode != DrawMode.Inspector) {
                provider.Scroll = EditorGUILayout.BeginScrollView(provider.Scroll);
            }
            EditorGUILayout.Space(10);

            if (string.IsNullOrEmpty(provider.WorldTypeName)) {
                EditorGUILayout.HelpBox("Please, provide world", MessageType.Warning, true);
            }

            EditorGUILayout.BeginHorizontal();
            {
                var allowChangeWorld = provider.RuntimeEvent.IsEmpty() && mode == DrawMode.Inspector;
                using (Ui.EnabledScopeVal(allowChangeWorld)) {
                    if (Ui.SettingButton && allowChangeWorld) {
                        DrawWorldMenu(provider);
                    }
                }

                EditorGUILayout.LabelField("World:", Ui.WidthLine(60));
                EditorGUILayout.LabelField(provider.WorldEditorName, Ui.LabelStyleThemeBold);
            }
            EditorGUILayout.EndHorizontal();

            if (string.IsNullOrEmpty(provider.WorldTypeName)) {
                if (mode != DrawMode.Inspector) {
                    EditorGUILayout.EndScrollView();
                }
                return;
            }

            if (provider.EventTemplate == null && provider.RuntimeEvent.IsEmpty()) {
                EditorGUILayout.HelpBox("Please, provide event type", MessageType.Warning, true);
            }

            EditorGUILayout.BeginHorizontal();
            {
                var allowChangeEventType = provider.RuntimeEvent.IsEmpty();
                using (Ui.EnabledScopeVal(allowChangeEventType)) {
                    if (Ui.PlusButton && allowChangeEventType) {
                        DrawEventsMenu(provider);
                    }
                }

                EditorGUILayout.LabelField("Type:", Ui.WidthLine(60));
                if (!provider.RuntimeEvent.IsEmpty()) {
                    EditorGUILayout.LabelField(provider.RuntimeEvent.Type.EditorTypeName(), Ui.LabelStyleThemeBold);
                } else if (provider.EventTemplate != null) {
                    EditorGUILayout.LabelField(provider.EventTemplate.GetType().EditorTypeName(), Ui.LabelStyleThemeBold, Ui.WidthLine(200));
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
                                if (provider.World.Events().TryGetPool(actualEvent.GetType(), out var pool)) {
                                    pool.AddRaw(actualEvent);
                                    provider.EventCache = actualEvent;
                                    provider.RuntimeEvent = new RuntimeEvent {
                                        InternalIdx = pool.Last(),
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

            if (provider.EventIsActual(Application.isPlaying)) {
                EditorGUILayout.Space(10);
                var ctx = new DrawContext {
                    World = provider.World,
                    Level = MaxRecursionLvl
                };
                DrawEvent(ref ctx, provider);
            }

            if (mode != DrawMode.Inspector) {
                EditorGUILayout.EndScrollView();
            }
        }

        private static void DrawEvent(ref DrawContext ctx, StaticEcsEventProvider provider) {
            var eventValue = provider.GetActualEvent(out var cached);
            var type = eventValue.GetType();
            var typeName = type.EditorTypeName();

            using (Ui.EnabledScopeVal(!cached)) {
                EditorGUILayout.BeginVertical(GUI.skin.box);
                {
                    if (TryDrawObject(ref ctx, typeName, type, eventValue, out var newValue)) {
                        provider.OnChangeEvent((IEvent) newValue);
                        EditorUtility.SetDirty(provider);
                    }
                }
                EditorGUILayout.EndVertical();
            }
        }

        private static void DrawEventsMenu(StaticEcsEventProvider provider) {
            var menu = new GenericMenu();
            foreach (var eventDataMeta in MetaData.Events) {
                if (provider.EventTemplate != null && provider.EventTemplate.GetType() == eventDataMeta.Type) {
                    continue;
                }

                if (provider.ShouldShowEvent(eventDataMeta.Type, Application.isPlaying)) {
                    menu.AddItem(new GUIContent(eventDataMeta.FullName), false, objType => {
                                     var objRaw = Activator.CreateInstance((Type) objType, true);
                                     provider.OnSelectEvent((IEvent) objRaw);
                                     EditorUtility.SetDirty(provider);
                                 },
                                 eventDataMeta.Type);
                } else {
                    menu.AddDisabledItem(new GUIContent(eventDataMeta.FullName));
                }
            }

            menu.ShowAsContext();
        }
    }
}