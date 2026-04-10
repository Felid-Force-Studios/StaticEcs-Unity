using System;
using UnityEditor;
using UnityEngine;

namespace FFS.Libraries.StaticEcs.Unity.Editor {
    public static partial class Drawer {
        public static void DrawEvent<TWorld, TProvider>(
            TProvider provider, DrawMode mode, Action<TProvider> onClickBuild, Action<TProvider> onCopyTemplate = null
        ) where TProvider : StaticEcsEventProvider<TWorld>
          where TWorld : struct, IWorldType {
            if (mode != DrawMode.Inspector) {
                provider.Scroll = EditorGUILayout.BeginScrollView(provider.Scroll);
            }
            EditorGUILayout.Space(10);

            if (provider.EventTemplate == null && provider.RuntimeEvent.IsEmpty()) {
                EditorGUILayout.HelpBox("Please, provide event type", MessageType.Warning, true);
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
                    var wrapper = EventDrawerWrapper.Instance;
                    wrapper.value = eventValue;
                    var so = new SerializedObject(wrapper);
                    so.Update();

                    var prop = so.FindProperty("value");
                    DrawSerializedPropertyChildren(prop);

                    if (so.ApplyModifiedProperties()) {
                        provider.OnChangeEvent(wrapper.value);
                        EditorUtility.SetDirty(provider);
                    }
                }
                EditorGUILayout.EndVertical();
            }
        }

        private static void DrawEventsMenu<TWorld, TEntityProvider>(TEntityProvider provider) where TEntityProvider : StaticEcsEventProvider<TWorld> where TWorld : struct, IWorldType {
            var worldMeta = MetaData.GetWorldMetaData(typeof(TWorld));
            var menu = new GenericMenu();
            foreach (var eventDataMeta in worldMeta.Events) {
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
