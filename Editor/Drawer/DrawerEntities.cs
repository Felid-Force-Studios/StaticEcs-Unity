using System;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace FFS.Libraries.StaticEcs.Unity.Editor {

    public enum DrawMode {
        Inspector,
        Builder,
        Viewer
    }

    public static partial class Drawer {
        public static void DrawEntity<TWorld, TEntityProvider>(
            TEntityProvider provider, DrawMode mode, Action<TEntityProvider> onClickSpawn, Action<TEntityProvider> onClose
        ) where TEntityProvider : StaticEcsEntityProvider<TWorld> where TWorld : struct, IWorldType {
            var prefab = provider.GetPrefab();
            var prefabView = mode != DrawMode.Viewer && prefab != null && prefab is StaticEcsEntityProvider<TWorld>;
            var origin = provider;

            AbstractStaticEcsProvider activeBase = provider;
            StaticEcsEntityProvider<TWorld> active = provider;
            if (prefabView) {
                activeBase = prefab;
                active = (StaticEcsEntityProvider<TWorld>) prefab;
            }

            using (Ui.EnabledScopeVal(!prefabView)) {
                if (mode != DrawMode.Inspector) {
                    activeBase.Scroll = EditorGUILayout.BeginScrollView(activeBase.Scroll);
                }

                EditorGUILayout.Space(10);

                EditorGUILayout.BeginHorizontal();
                {
                    if (Ui.MenuButton) {
                        var menu = new GenericMenu();

                        if (active.EntityIsActual()) {
                            if (mode == DrawMode.Viewer) {
                                menu.AddItem(new GUIContent("Close"), false, () => onClose(origin));
                            }

                            var entity = active.EntityGid.Unpack<TWorld>();
                            if (entity.IsEnabled) {
                                menu.AddItem(new GUIContent("Disable"), false, () => {
                                    EcsDebug<TWorld>.DebugViewSystem.EnqueueCommand(new DebugCommand {
                                        Type = DebugCommandType.DisableEntity,
                                        EntityGid = entity.GID,
                                    });
                                });
                            } else {
                                menu.AddItem(new GUIContent("Enable"), false, () => {
                                    EcsDebug<TWorld>.DebugViewSystem.EnqueueCommand(new DebugCommand {
                                        Type = DebugCommandType.EnableEntity,
                                        EntityGid = entity.GID,
                                    });
                                });
                            }

                            menu.AddItem(new GUIContent("Destroy entity"), false, () => {
                                EcsDebug<TWorld>.DebugViewSystem.EnqueueCommand(new DebugCommand {
                                    Type = DebugCommandType.DestroyEntity,
                                    EntityGid = entity.GID,
                                });
                                active.EntityGid = default;
                                EditorUtility.SetDirty(activeBase);
                            });
                        } else {
                            menu.AddItem(new GUIContent("Clear template"), false, () => {
                                active.Clear();
                                EditorUtility.SetDirty(activeBase);
                            });
                        }

                        menu.ShowAsContext();
                    }

                    EditorGUILayout.LabelField("Entity GID:", Ui.WidthLine(90));
                    if (active.EntityIsActual()) {
                        EditorGUILayout.SelectableLabel(Ui.IntToStringD6((int) active.EntityGid.Id).d6, Ui.LabelStyleThemeBold, Ui.WidthLine(120));
                        if (active.EntityGid.Unpack<TWorld>().IsDisabled) {
                            EditorGUILayout.LabelField("[Disabled]", Ui.LabelStyleThemeBold, Ui.WidthLine(70));
                        }
                    } else {
                        EditorGUILayout.LabelField("---", Ui.LabelStyleThemeBold, Ui.WidthLine(60));
                        using (Ui.EnabledScope) {
                            var text = mode == DrawMode.Inspector && !origin.IsPrefab() ? "Build" : "Spawn";
                            if (mode != DrawMode.Viewer && Application.isPlaying && active.HasComponents() && GUILayout.Button(text, Ui.ButtonStyleYellow, Ui.WidthLine(60))) {
                                onClickSpawn(origin);
                            }
                        }
                    }
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                {
                    if (Ui.MenuButton) { }

                    EditorGUILayout.LabelField("Entity Type:", Ui.WidthLine(90));
                    if (active.EntityIsActual()) {
                        var entity = active.EntityGid.Unpack<TWorld>();
                        var entityTypeByte = entity.EntityType;
                        var typeName = MetaData.GetEntityTypeName(entityTypeByte);
                        EditorGUILayout.SelectableLabel($"{typeName} ({entityTypeByte})", Ui.LabelStyleThemeBold, Ui.WidthLine(120));
                    } else {
                        if (MetaData.EntityTypes.Count > 0) {
                            var currentIdx = MetaData.EntityTypes.FindIndex(et => et.Id == active.entityType);
                            if (currentIdx < 0) currentIdx = 0;
                            var names = new string[MetaData.EntityTypes.Count];
                            for (var i = 0; i < MetaData.EntityTypes.Count; i++) {
                                names[i] = $"{MetaData.EntityTypes[i].Name} ({MetaData.EntityTypes[i].Id})";
                            }
                            var newIdx = EditorGUILayout.Popup(currentIdx, names, Ui.WidthLine(120));
                            if (newIdx != currentIdx) {
                                active.entityType = MetaData.EntityTypes[newIdx].Id;
                                EditorUtility.SetDirty(activeBase);
                            }
                        } else {
                            EditorGUILayout.LabelField("---", Ui.LabelStyleThemeBold, Ui.WidthLine(60));
                        }
                    }
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                {
                    if (Ui.MenuButton) { }

                    EditorGUILayout.LabelField("Cluster ID:", Ui.WidthLine(90));
                    if (active.EntityIsActual()) {
                        EditorGUILayout.SelectableLabel(Ui.IntToStringD6(active.EntityGid.ClusterId).d6, Ui.LabelStyleThemeBold, Ui.WidthLine(120));
                    } else {
                        EditorGUILayout.LabelField("---", Ui.LabelStyleThemeBold);
                    }
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space();

                Ui.DrawHorizontalSeparator();

                if (!active.EntityIsActual() && !active.HasComponents()) {
                    EditorGUILayout.HelpBox("Please, provide at least one component", MessageType.Warning, true);
                }

                DrawEntity(activeBase, active);

                if (mode != DrawMode.Inspector) {
                    EditorGUILayout.EndScrollView();
                }
            }
        }

        private static void DrawEntity<TWorld>(Object obj, StaticEcsEntityProvider<TWorld> provider) where TWorld : struct, IWorldType {
            EditorGUILayout.Space(10);

            provider.Components(_componentsCache);
            EditorGUILayout.Space(10);
            DrawComponents(_componentsCache, obj, provider);
            _componentsCache.Clear();

            provider.Tags(_tagsCache);
            EditorGUILayout.Space(10);
            DrawTags(_tagsCache, obj, provider);
            _tagsCache.Clear();
        }
    }
}
