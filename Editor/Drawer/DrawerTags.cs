using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace FFS.Libraries.StaticEcs.Unity.Editor {
    public static partial class Drawer {

        private static void DrawTags<TWorld>(List<IComponentOrTagProvider> providers, Object obj, StaticEcsEntityProvider<TWorld> provider) where TWorld : struct, IWorldType {
            var worldMeta = MetaData.GetWorldMetaData(typeof(TWorld));

            EditorGUILayout.BeginHorizontal();
            {
                var hasAll = worldMeta.Tags.Count == providers.Count;
                using (Ui.EnabledScopeVal(!hasAll && GUI.enabled)) {
                    if (Ui.PlusDropDownButton && !hasAll) {
                        DrawTagsMenu(providers, obj, provider);
                    }
                }

                EditorGUILayout.LabelField(" Tags:", Ui.HeaderStyleTheme);
                if (!provider.EntityIsActual() && providers.Count >= 2 && GUILayout.Button("Sort", Ui.ButtonStyleThemeMini, Ui.WidthLine(40))) {
                    SortSerializedProviders(provider, obj);
                }
            }
            EditorGUILayout.EndHorizontal();

            var sorted = provider.EntityIsActual();
            var order = sorted ? BuildProviderSortedOrder(providers) : null;
            for (var o = 0; o < providers.Count; o++) {
                var i = sorted ? order[o] : o;
                var prov = providers[i];
                if (prov == null || prov.ComponentType == null) {
                    EditorGUILayout.LabelField($"Broken tag - is null, index {i}", EditorStyles.boldLabel);
                    if (GUILayout.Button("Delete all broken tags", Ui.ButtonStyleTheme, Ui.ExpandWidthFalse())) {
                        provider.DeleteAllBrokenProviders();
                        EditorUtility.SetDirty(obj);
                    }

                    EditorGUILayout.Space(2);
                    continue;
                }

                var type = prov.ComponentType;
                var colored = type.EditorTypeColor(out var color);

                EditorGUI.indentLevel++;
                EditorGUILayout.BeginHorizontal(GUI.skin.box);
                {
                    if (colored) {
                        EditorGUILayout.SelectableLabel(type.EditorTypeName(), new GUIStyle(EditorStyles.boldLabel) {
                            normal = {
                                textColor = color
                            }
                        }, GUILayout.MaxHeight(EditorGUIUtility.singleLineHeight));
                    } else {
                        EditorGUILayout.SelectableLabel(type.EditorTypeName(), EditorStyles.boldLabel, GUILayout.MaxHeight(EditorGUIUtility.singleLineHeight));
                    }
                    if (Ui.TrashButton) {
                        provider.OnDeleteProvider(type);
                        EditorUtility.SetDirty(obj);
                    }
                }
                EditorGUILayout.EndHorizontal();
                EditorGUI.indentLevel--;
                EditorGUILayout.Space(2);
            }
        }

        private static void DrawTagsMenu<TWorld>(List<IComponentOrTagProvider> actualProviders, Object obj, StaticEcsEntityProvider<TWorld> provider) where TWorld : struct, IWorldType {
            var worldMeta = MetaData.GetWorldMetaData(typeof(TWorld));
            var menu = new GenericMenu();
            foreach (var tag in worldMeta.Tags) {
                var has = false;
                foreach (var actual in actualProviders) {
                    if (actual != null && actual.ComponentType == tag.Type) {
                        has = true;
                        break;
                    }
                }

                if (has) continue;

                if (provider.ShouldShowProvider(tag.Type, Application.isPlaying)) {
                    menu.AddItem(new GUIContent(tag.FullName), false, objType => {
                                     var tagInstance = (ITag) Activator.CreateInstance((Type) objType, true);
                                     provider.OnSelectProvider(new TagProvider { value = tagInstance });
                                     EditorUtility.SetDirty(obj);
                                 },
                                 tag.Type);
                } else {
                    menu.AddDisabledItem(new GUIContent(tag.FullName));
                }
            }

            menu.ShowAsContext();
        }
    }
}
