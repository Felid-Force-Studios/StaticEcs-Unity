using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FFS.Libraries.StaticEcs.Unity.Editor {

    public enum DrawMode {
        Inspector,
        Viewer
    }

    public static partial class Drawer {
        private static readonly List<IComponentOrTagProvider> _ComponentsAndTagsCache = new();

        public static void DrawEntity<TWorld, TEntityProvider>(TEntityProvider provider, DrawMode mode) where TEntityProvider : StaticEcsEntityProvider<TWorld> where TWorld : struct, IWorldType {
            using (Ui.EnabledScope) {
                if (mode != DrawMode.Inspector) {
                    provider.Scroll = EditorGUILayout.BeginScrollView(provider.Scroll);
                }

                DrawHeader(provider, mode);
                DrawComponentsAndTags(provider, mode);

                if (mode != DrawMode.Inspector) {
                    EditorGUILayout.EndScrollView();
                }
            }
        }

        private static void DrawHeader<TWorld>(StaticEcsEntityProvider<TWorld> provider, DrawMode mode) where TWorld : struct, IWorldType {
            EditorGUILayout.BeginHorizontal();

            DrawEntityLabel();
            DrawEntityType(provider);
            DrawSectionSeparator(EditorGUIUtility.singleLineHeight);
            DrawEntityInfo(provider);
            GUILayout.FlexibleSpace();
            DrawEntityDisabled(provider);
            DrawSpawnButton(provider, mode);
            DrawActionMenu(provider);

            EditorGUILayout.EndHorizontal();

            DrawHeaderSeparator();
        }

        private static void DrawComponentsAndTags<TWorld>(StaticEcsEntityProvider<TWorld> provider, DrawMode mode) where TWorld : struct, IWorldType {
            provider.GetComponentsAndTags(_ComponentsAndTagsCache);
            DrawComponentsAndTags(_ComponentsAndTagsCache, provider, mode);
            _ComponentsAndTagsCache.Clear();
        }

        private static void DrawEntityLabel() {
            GUILayout.Label("Entity", Ui.LabelStyleThemeLeftColor(new Color(1f, 1f, 1f, 0.55f)), GUILayout.Width(45), GUILayout.Height(EditorGUIUtility.singleLineHeight));
            DrawSectionSeparator(EditorGUIUtility.singleLineHeight);
        }

        private static void DrawEntityType<TWorld>(StaticEcsEntityProvider<TWorld> provider) where TWorld : struct, IWorldType {
            var worldMeta = MetaData.GetWorldMetaData(typeof(TWorld));
            if (provider.EntityIsActual()) {
                var entity = provider.EntityGid.Unpack<TWorld>();
                var typeName = worldMeta.GetEntityTypeName(entity.EntityType);
                GUILayout.Label(typeName, Ui.LabelStyleThemeBold, GUILayout.Height(EditorGUIUtility.singleLineHeight), GUILayout.ExpandWidth(false));
            } else if (worldMeta != null && worldMeta.EntityTypes.Count > 0) {
                var currentIdx = worldMeta.EntityTypes.FindIndex(et => et.Id == provider.entityType);
                if (currentIdx < 0) currentIdx = 0;
                var names = new string[worldMeta.EntityTypes.Count];
                for (var i = 0; i < worldMeta.EntityTypes.Count; i++) {
                    names[i] = worldMeta.EntityTypes[i].Name;
                }
                var newIdx = EditorGUILayout.Popup(currentIdx, names, GUILayout.Width(100), GUILayout.Height(EditorGUIUtility.singleLineHeight));
                if (newIdx != currentIdx) {
                    provider.entityType = worldMeta.EntityTypes[newIdx].Id;
                    EditorUtility.SetDirty(provider);
                }
            } else {
                GUILayout.Label("---", Ui.LabelStyleThemeBold, GUILayout.Height(EditorGUIUtility.singleLineHeight), GUILayout.ExpandWidth(false));
            }
        }

        private static void DrawEntityInfo<TWorld>(StaticEcsEntityProvider<TWorld> provider) where TWorld : struct, IWorldType {
            if (provider.EntityIsActual()) {
                var gid = Ui.IntToStringD6((int) provider.EntityGid.Id).simple;
                var content = new GUIContent($"GID {gid}");
                EditorGUILayout.SelectableLabel(content.text, Ui.LabelStyleThemeBold, GUILayout.Height(EditorGUIUtility.singleLineHeight), GUILayout.Width(Ui.LabelStyleThemeBold.CalcSize(content).x));

                DrawSectionSeparator(EditorGUIUtility.singleLineHeight);

                var clusterId = Ui.IntToStringD6(provider.EntityGid.ClusterId).simple;
                var clContent = new GUIContent($"Cluster {clusterId}");
                EditorGUILayout.SelectableLabel(clContent.text, Ui.LabelStyleThemeBold, GUILayout.Height(EditorGUIUtility.singleLineHeight), GUILayout.Width(Ui.LabelStyleThemeBold.CalcSize(clContent).x));
            }
        }

        private static void DrawEntityDisabled<TWorld>(StaticEcsEntityProvider<TWorld> provider) where TWorld : struct, IWorldType {
            if (provider.EntityIsActual() && provider.EntityGid.Unpack<TWorld>().IsDisabled) {
                GUILayout.Label("[Disabled]", Ui.LabelStyleYellowCenter, GUILayout.Height(EditorGUIUtility.singleLineHeight), GUILayout.ExpandWidth(false));
            }
        }

        private static void DrawSpawnButton<TWorld>(StaticEcsEntityProvider<TWorld> provider, DrawMode mode) where TWorld : struct, IWorldType {
            if (!provider.EntityIsActual() && mode != DrawMode.Viewer && Application.isPlaying) {
                using (Ui.EnabledScope) {
                    if (GUILayout.Button("Spawn", Ui.ButtonStyleYellow, GUILayout.Width(60), GUILayout.Height(EditorGUIUtility.singleLineHeight))) {
                        provider.CreateEntity();
                        EditorUtility.SetDirty(provider);
                    }
                }
            }
        }

        private static void DrawActionMenu<TWorld>(StaticEcsEntityProvider<TWorld> provider) where TWorld : struct, IWorldType {
            if (Ui.MenuButton) {
                var menu = new GenericMenu();
                if (provider.EntityIsActual()) {
                    var entity = provider.EntityGid.Unpack<TWorld>();
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
                    menu.AddItem(new GUIContent("Destroy"), false, () => {
                        EcsDebug<TWorld>.DebugViewSystem.EnqueueCommand(new DebugCommand {
                            Type = DebugCommandType.DestroyEntity,
                            EntityGid = entity.GID,
                        });
                        provider.EntityGid = default;
                        EditorUtility.SetDirty(provider);
                    });
                } else {
                    menu.AddItem(new GUIContent("Clear"), false, () => {
                        provider.Clear();
                        EditorUtility.SetDirty(provider);
                    });
                }
                menu.ShowAsContext();
            }
        }

        private static void DrawHeaderSeparator() {
            EditorGUILayout.Space(6);
            var sepRect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.ExpandWidth(true), GUILayout.Height(1));
            EditorGUI.DrawRect(sepRect, new Color(1f, 1f, 1f, 0.12f));
            EditorGUILayout.Space(6);
        }

        private static void DrawSectionSeparator(float height) {
            GUILayout.Space(6);
            var rect = GUILayoutUtility.GetRect(1, height, GUILayout.Width(1), GUILayout.Height(height));
            rect.x += 0;
            rect.y += 3;
            rect.width = 1;
            rect.height = height - 6;
            EditorGUI.DrawRect(rect, new Color(1f, 1f, 1f, 0.18f));
            GUILayout.Space(6);
        }

    }
}
