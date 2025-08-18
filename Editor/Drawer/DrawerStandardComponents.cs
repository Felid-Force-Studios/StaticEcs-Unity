using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace FFS.Libraries.StaticEcs.Unity.Editor {
    public static partial class Drawer {
        private static readonly List<IStandardComponent> _standardComponentsCache = new();

        private static void DrawStandardComponents<TProvider>(ref DrawContext ctx, List<IStandardComponent> components, TProvider provider, bool allowAddDelete)
            where TProvider : Object, IStaticEcsEntityProvider {
            EditorGUILayout.BeginHorizontal();
            {
                var hasAll = MetaData.StandardComponents.Count == components.Count;
                using (Ui.EnabledScopeVal(!hasAll && GUI.enabled)) {
                    if (Ui.PlusDropDownButton && !hasAll) {
                        DrawStandardComponentsMenu(components, provider);
                    }
                }

                EditorGUILayout.LabelField(" Standard components:", Ui.HeaderStyleTheme, Ui.ExpandWidthFalse());
            }
            EditorGUILayout.EndHorizontal();

            for (int i = 0, iMax = components.Count; i < iMax; i++) {
                DrawStandardComponent(ref ctx, components[i], provider);
            }
        }

        private static void DrawStandardComponent<TProvider>(ref DrawContext ctx, IStandardComponent component, TProvider provider) where TProvider : Object, IStaticEcsEntityProvider {
            if (component == null) {
                EditorGUILayout.LabelField("Broken standard component - is null", EditorStyles.boldLabel);
                if (GUILayout.Button("Delete all broken standard components", Ui.ButtonStyleTheme, Ui.ExpandWidthFalse())) {
                    provider.DeleteAllBrokenStandardComponents();
                    EditorUtility.SetDirty(provider);
                }

                EditorGUILayout.Space(2);
                return;
            }

            var type = component.GetType();
            var typeName = type.EditorTypeName();
            var colored = type.EditorTypeColor(out var color);

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
                    // menu.AddItem(new GUIContent("Clear"), false, () => {
                    //     // TODO
                    //     EditorUtility.SetDirty(provider);
                    // });
                    menu.ShowAsContext();
                }
            }
            EditorGUILayout.EndHorizontal();


            EditorGUILayout.BeginVertical(GUI.skin.box);
            if (show && TryDrawObject(ref ctx, typeName, type, component, out var newValue)) {
                provider.OnChangeStandardComponent((IStandardComponent) newValue, type);
                EditorUtility.SetDirty(provider);
            }

            EditorGUILayout.EndVertical();
        }

        private static void DrawStandardComponentsMenu<TProvider>(List<IStandardComponent> actualComponents, TProvider provider) where TProvider : Object, IStaticEcsEntityProvider {
            var menu = new GenericMenu();
            foreach (var component in MetaData.StandardComponents) {
                var has = false;
                foreach (var actual in actualComponents) {
                    if (actual.GetType() == component.Type) {
                        has = true;
                        break;
                    }
                }


                if (has) continue;

                if (provider.ShouldShowStandardComponent(component.Type, Application.isPlaying)) {
                    menu.AddItem(new GUIContent(component.FullName), false, objType => {
                                     var objRaw = Activator.CreateInstance((Type) objType, true);
                                     provider.OnSelectStandardComponent((IStandardComponent) objRaw);
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