using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace FFS.Libraries.StaticEcs.Unity.Editor {
    public static partial class Drawer {
        private static readonly List<IComponent> _componentsCache = new();

        private static void DrawComponents<TWorld>(List<IComponent> components, Object obj, StaticEcsEntityProvider<TWorld> provider) where TWorld : struct, IWorldType {
            EditorGUILayout.BeginHorizontal();
            {
                var hasAll = MetaData.Components.Count == components.Count;
                using (Ui.EnabledScopeVal(!hasAll && GUI.enabled)) {
                    if (Ui.PlusDropDownButton && !hasAll) {
                        DrawComponentsMenu(components, obj, provider);
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
                        EditorUtility.SetDirty(obj);
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
                        DrawFoldoutBox(HashCode.Combine(provider, type.FullName), typeName, typeName, out show, color);
                    } else {
                        DrawFoldoutBox(HashCode.Combine(provider, type.FullName), typeName, typeName, out show);
                    }
                    if (Ui.MenuButton) {
                        var menu = new GenericMenu();
                        if (provider.EntityIsActual()) {
                            if (disabled) {
                                menu.AddItem(new GUIContent("Enable"), false, () => {
                                    provider.Enable(type);
                                    EditorUtility.SetDirty(obj);
                                });
                            } else {
                                menu.AddItem(new GUIContent("Disable"), false, () => {
                                    provider.Disable(type);
                                    EditorUtility.SetDirty(obj);
                                });
                            }
                        }
                        menu.AddItem(new GUIContent("Delete"), false, () => {
                            provider.OnDeleteComponent(type);
                            EditorUtility.SetDirty(obj);
                        });
                        menu.ShowAsContext();
                    }
                }
                EditorGUILayout.EndHorizontal();

                if (show) {
                    EditorGUILayout.BeginVertical(GUI.skin.box);

                    if (!TryDrawSpecialComponent(component, type)) {
                        var wrapper = ComponentDrawerWrapper.Instance;
                        var so = new SerializedObject(wrapper);
                        var prop = so.FindProperty("value");
                        prop.managedReferenceValue = component;
                        so.ApplyModifiedProperties();
                        so.Update();
                        prop = so.FindProperty("value");

                        if (prop != null && prop.propertyType == SerializedPropertyType.ManagedReference) {
                            DrawSerializedPropertyChildren(prop);

                            if (so.ApplyModifiedProperties()) {
                                provider.OnChangeComponent(wrapper.value, type);
                                EditorUtility.SetDirty(obj);
                            }
                        }
                    }

                    EditorGUILayout.EndVertical();
                }
            }
        }

        internal static void DrawSerializedPropertyChildren(SerializedProperty property) {
            var iterator = property.Copy();
            var end = property.GetEndProperty();
            if (!iterator.NextVisible(true)) return;

            while (!SerializedProperty.EqualContents(iterator, end)) {
                EditorGUILayout.PropertyField(iterator, true);
                if (!iterator.NextVisible(false)) break;
            }
        }

        private static bool IsWorldWrapperType(Type type, out string baseName) {
            baseName = null;
            if (!type.IsGenericType) return false;
            var dt = type.DeclaringType;
            if (dt == null || !dt.IsGenericType) return false;
            if (dt.GetGenericTypeDefinition().FullName != "FFS.Libraries.StaticEcs.World`1") return false;
            var n = type.Name;
            if (n.StartsWith("Link`")) { baseName = "Link"; return true; }
            if (n.StartsWith("Links`")) { baseName = "Links"; return true; }
            if (n.StartsWith("Multi`")) { baseName = "Multi"; return true; }
            return false;
        }

        private static bool TryDrawSpecialComponent(IComponent component, Type type) {
            if (!IsWorldWrapperType(type, out var baseName)) return false;

            if (baseName == "Link") {
                DrawLinkComponent(component, type);
            } else if (baseName == "Links") {
                DrawLinksComponent(component, type);
            } else if (baseName == "Multi") {
                DrawMultiComponent(component, type);
            }

            return true;
        }

        private static void DrawLinkComponent(IComponent component, Type type) {
            var valueProp = type.GetProperty("Value", BindingFlags.Public | BindingFlags.Instance);
            if (valueProp == null) return;
            var gid = (EntityGID) valueProp.GetValue(component);
            DrawEntityGIDField("Value", gid);
        }

        private static void DrawLinksComponent(IComponent component, Type type) {
            if (!Application.isPlaying) {
                EditorGUILayout.LabelField("Runtime only", EditorStyles.centeredGreyMiniLabel);
                return;
            }

            var lengthProp = type.GetProperty("Length", BindingFlags.Public | BindingFlags.Instance);
            if (lengthProp == null) return;
            var count = (ushort) lengthProp.GetValue(component);

            EditorGUILayout.LabelField("Count", count.ToString());

            var itemProp = type.GetProperty("Item", BindingFlags.Public | BindingFlags.Instance);
            if (itemProp == null) return;

            for (var i = 0; i < count; i++) {
                var link = itemProp.GetValue(component, new object[] { i });
                var linkType = link.GetType();
                var linkValueProp = linkType.GetProperty("Value", BindingFlags.Public | BindingFlags.Instance);
                if (linkValueProp == null) continue;
                var gid = (EntityGID) linkValueProp.GetValue(link);
                DrawEntityGIDField($"[{i}]", gid);
            }
        }

        private static void DrawMultiComponent(IComponent component, Type type) {
            if (!Application.isPlaying) {
                EditorGUILayout.LabelField("Runtime only", EditorStyles.centeredGreyMiniLabel);
                return;
            }

            var lengthProp = type.GetProperty("Length", BindingFlags.Public | BindingFlags.Instance);
            if (lengthProp == null) return;
            var count = (ushort) lengthProp.GetValue(component);

            EditorGUILayout.LabelField("Count", count.ToString());

            var itemProp = type.GetProperty("Item", BindingFlags.Public | BindingFlags.Instance);
            if (itemProp == null) return;

            var elementType = itemProp.PropertyType;
            for (var i = 0; i < count; i++) {
                var element = itemProp.GetValue(component, new object[] { i });
                var level = 5;
                TryDrawObject(ref level, $"[{i}]", elementType, element, out _);
            }
        }

        private static void DrawEntityGIDField(string label, EntityGID gid) {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(label);

            var empty = gid.Raw == 0;
            string text;
            var actual = false;

            if (empty) {
                text = "Empty";
            } else {
                text = gid.ToString();

                foreach (var kvp in StaticEcsDebugData.Worlds) {
                    if (kvp.Value.Handle.GIDStatus(gid) == GIDStatus.Active) {
                        actual = true;
                        break;
                    }
                }

                if (!actual) {
                    text += " (Not actual)";
                }
            }

            EditorGUILayout.SelectableLabel(text, GUILayout.MaxHeight(EditorGUIUtility.singleLineHeight));

            if (Application.isPlaying && actual) {
                if (GUILayout.Button("\u2299", EditorStyles.miniButton, GUILayout.Width(20))) {
                    foreach (var kvp in StaticEcsDebugData.Worlds) {
                        if (kvp.Value.Handle.GIDStatus(gid) == GIDStatus.Active) {
                            if (EntityInspectorRegistry.ShowEntityHandlers.TryGetValue(kvp.Key, out var handler)) {
                                handler(gid);
                            }
                            break;
                        }
                    }
                }
            }

            EditorGUILayout.EndHorizontal();
        }

        private static void DrawComponentsMenu<TWorld>(List<IComponent> actualComponents, Object obj, StaticEcsEntityProvider<TWorld> provider) where TWorld : struct, IWorldType {
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
                                     EditorUtility.SetDirty(obj);
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
