using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace FFS.Libraries.StaticEcs.Unity.Editor {
    public static partial class Drawer {
        private static readonly List<IComponent> _componentsCache = new();

        private static void DrawComponents<TProvider>(ref DrawContext ctx, List<IComponent> components, TProvider provider) where TProvider : Object, IStaticEcsEntityProvider {
            EditorGUILayout.BeginHorizontal();
            {
                var hasAll = MetaData.Components.Count == components.Count;
                using (Ui.EnabledScopeVal(!hasAll && GUI.enabled)) {
                    if (Ui.PlusDropDownButton && !hasAll) {
                        DrawComponentsMenu(components, provider);
                    }
                }
                EditorGUILayout.LabelField(" Components:", Ui.HeaderStyleTheme);
            }
            EditorGUILayout.EndHorizontal();

            for (int i = 0, iMax = components.Count; i < iMax; i++) {
                var component = components[i];

                if (component == null) {
                    EditorGUILayout.LabelField($"Broken component - is null, index {i}", EditorStyles.boldLabel);
                    if (GUILayout.Button("Delete all broken components", Ui.ButtonStyleTheme, Ui.ExpandWidthFalse())) {
                        provider.DeleteAllBrokenComponents();
                        EditorUtility.SetDirty(provider);
                    }

                    EditorGUILayout.Space(2);
                    continue;
                }

                var type = component.GetType();
                var colored = type.EditorTypeColor(out var color);
                var typeName = type.EditorTypeName();
                var disabled = provider.IsDisabled(type);
                if (disabled) {
                    typeName += " [Disabled]";
                }

                bool show;
                EditorGUILayout.BeginHorizontal(GUI.skin.box);
                {
                    if (colored) {
                        DrawFoldoutBox(type.FullName, typeName, typeName, out show, color);
                    } else {
                        DrawFoldoutBox(type.FullName, typeName, typeName, out show);
                    }
                    if (Ui.MenuButton) {
                        var menu = new GenericMenu();
                        if (provider.EntityIsActual()) {
                            if (disabled) {
                                menu.AddItem(new GUIContent("Enable"), false, () => {
                                    provider.Enable(type);
                                    EditorUtility.SetDirty(provider);
                                });
                            } else {
                                menu.AddItem(new GUIContent("Disable"), false, () => {
                                    provider.Disable(type);
                                    EditorUtility.SetDirty(provider);
                                });
                            }
                        }
                        menu.AddItem(new GUIContent("Delete"), false, () => {
                            provider.OnDeleteComponent(type);
                            EditorUtility.SetDirty(provider);
                        });
                        menu.ShowAsContext();
                    }
                }
                EditorGUILayout.EndHorizontal();

                if (show) {
                    EditorGUILayout.BeginVertical(GUI.skin.box);
                    if (TryDrawObject(ref ctx, typeName, type, component, out var newValue)) {
                        provider.OnChangeComponent((IComponent) newValue, type);
                        EditorUtility.SetDirty(provider);
                    }
                    EditorGUILayout.EndVertical();
                }
            }
        }

        private static void DrawComponentsMenu<TProvider>(List<IComponent> actualComponents, TProvider provider) where TProvider : Object, IStaticEcsEntityProvider {
            var menu = new GenericMenu();
            foreach (var component in MetaData.Components) {
                var has = false;
                foreach (var actual in actualComponents) {
                    if (actual.GetType() == component.Type) {
                        has = true;
                        break;
                    }
                }


                if (has) continue;

                if (provider.ShouldShowComponent(component.Type, Application.isPlaying)) {
                    menu.AddItem(new GUIContent(component.FullName), false, objType => {
                                     var objRaw = Activator.CreateInstance((Type) objType, true);
                                     provider.OnSelectComponent((IComponent) objRaw);
                                     EditorUtility.SetDirty(provider);
                                 },
                                 component.Type);
                } else {
                    menu.AddDisabledItem(new GUIContent(component.FullName));
                }
            }

            menu.ShowAsContext();
        }
    }
}