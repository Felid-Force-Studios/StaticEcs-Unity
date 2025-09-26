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
        public static void DrawEntity(
            StaticEcsEntityProvider provider, DrawMode mode, Action<StaticEcsEntityProvider> onClickSpawn, Action<StaticEcsEntityProvider> onClose
        ) {
            var prefabView = mode != DrawMode.Viewer && provider.Prefab;
            var origin = provider;
            
            if (prefabView) {
                provider = provider.Prefab;
            }
            using (Ui.EnabledScopeVal(!prefabView)) {
                if (mode != DrawMode.Inspector) {
                    provider.Scroll = EditorGUILayout.BeginScrollView(provider.Scroll);
                }

                EditorGUILayout.Space(10);

                if (string.IsNullOrEmpty(provider.WorldTypeName)) {
                    EditorGUILayout.HelpBox("Please, provide world", MessageType.Warning, true);
                }

                EditorGUILayout.BeginHorizontal();
                {
                    var allowChangeWorld = !provider.EntityIsActual() && mode == DrawMode.Inspector;
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

                EditorGUILayout.BeginHorizontal();
                {
                    if (Ui.MenuButton) {
                        var menu = new GenericMenu();

                        if (provider.EntityIsActual()) {
                            if (mode == DrawMode.Viewer) {
                                menu.AddItem(new GUIContent("Close"), false, () => onClose(provider));

                                // menu.AddItem(new GUIContent("Copy as new entity"), false, () => { });
                                // menu.AddItem(new GUIContent("Copy as builder template"), false, () => { });
                            }
                            
                            if (provider.Entity.IsEnabled()) {
                                menu.AddItem(new GUIContent("Disable"), false, () => provider.Entity.Disable());
                            } else {
                                menu.AddItem(new GUIContent("Enable"), false, () => provider.Entity.Enable());
                            }

                            menu.AddItem(new GUIContent("Destroy entity"), false, () => {
                                provider.Entity.Destroy();
                                provider.Entity = null;
                                EditorUtility.SetDirty(provider);
                            });
                        } else {
                            menu.AddItem(new GUIContent("Clear template"), false, () => {
                                provider.Clear();
                                EditorUtility.SetDirty(provider);
                            });
                        }

                        menu.ShowAsContext();
                    }

                    EditorGUILayout.LabelField("Entity ID:", Ui.WidthLine(60));
                    if (provider.EntityIsActual()) {
                        EditorGUILayout.SelectableLabel(Ui.IntToStringD6((int) provider.Entity.GetId()).d6, Ui.LabelStyleThemeBold, Ui.WidthLine(120));
                        if (provider.Entity.IsDisabled()) {
                            EditorGUILayout.LabelField("[Disabled]", Ui.LabelStyleThemeBold, Ui.WidthLine(70));
                        }
                    } else {
                        EditorGUILayout.LabelField("---", Ui.LabelStyleThemeBold, Ui.WidthLine(60));
                        using (Ui.EnabledScope) {
                            var text = mode == DrawMode.Inspector && !origin.IsPrefab() ? "Build" : "Spawn";
                            if (mode != DrawMode.Viewer && Application.isPlaying && provider.HasComponents() && GUILayout.Button(text, Ui.ButtonStyleYellow, Ui.WidthLine(60))) {
                                onClickSpawn(origin);
                            }
                        }
                    }
                }
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.BeginHorizontal();
                {
                    if (Ui.MenuButton) { }

                    EditorGUILayout.LabelField("Entity GID:", Ui.WidthLine(60));
                    if (provider.EntityIsActual()) {
                        EditorGUILayout.SelectableLabel(Ui.IntToStringD6((int) provider.Entity.Gid().Id()).d6, Ui.LabelStyleThemeBold, Ui.WidthLine(120));
                    } else {
                        EditorGUILayout.LabelField("---", Ui.LabelStyleThemeBold);
                    }
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space();

                Ui.DrawHorizontalSeparator();

                if (!provider.EntityIsActual() && !provider.HasComponents()) {
                    EditorGUILayout.HelpBox("Please, provide at least one component", MessageType.Warning, true);
                }

                var ctx = new DrawContext {
                    World = provider.World,
                    Level = MaxRecursionLvl
                };
                DrawEntity(ref ctx, provider);

                if (mode != DrawMode.Inspector) {
                    EditorGUILayout.EndScrollView();
                }
            }
        }

        private static void DrawEntity<TProvider>(ref DrawContext ctx, TProvider provider) where TProvider : Object, IStaticEcsEntityProvider {
            EditorGUILayout.Space(10);

            provider.Components(_componentsCache);
            EditorGUILayout.Space(10);
            DrawComponents(ref ctx, _componentsCache, provider);
            _componentsCache.Clear();

            #if !FFS_ECS_DISABLE_TAGS
            provider.Tags(_tagsCache);
            EditorGUILayout.Space(10);
            DrawTags(_tagsCache, provider);
            _tagsCache.Clear();
            #endif
        }

        private static void DrawWorldMenu(AbstractStaticEcsProvider provider) {
            var menu = new GenericMenu();
            for (var i = 0; i < MetaData.WorldsMetaData.Count; i++) {
                if (provider.WorldTypeName != null && MetaData.WorldsMetaData[i].WorldTypeType.FullName == provider.WorldTypeName) {
                    continue;
                }

                var i1 = i;
                menu.AddItem(
                    new GUIContent(MetaData.WorldsMetaData[i].EditorName),
                    false,
                    objType => {
                        provider.WorldEditorName = MetaData.WorldsMetaData[i1].EditorName;
                        provider.WorldTypeName = ((Type) objType).FullName;
                        EditorUtility.SetDirty(provider);
                    },
                    MetaData.WorldsMetaData[i1].WorldTypeType);
            }

            menu.ShowAsContext();
        }
    }
}