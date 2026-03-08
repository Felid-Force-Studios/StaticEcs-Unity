using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace FFS.Libraries.StaticEcs.Unity.Editor {
    public static partial class Drawer {
        private static readonly List<ITag> _tagsCache = new();

        private static void DrawTags<TWorld>(List<ITag> tags, Object obj, StaticEcsEntityProvider<TWorld> provider) where TWorld : struct, IWorldType {
            EditorGUILayout.BeginHorizontal();
            {
                var hasAll = MetaData.Tags.Count == tags.Count;
                using (Ui.EnabledScopeVal(!hasAll && GUI.enabled)) {
                    if (Ui.PlusDropDownButton && !hasAll) {
                        DrawTagsMenu(tags, obj, provider);
                    }
                }

                EditorGUILayout.LabelField(" Tags:", Ui.HeaderStyleTheme, Ui.ExpandWidthFalse());
            }
            EditorGUILayout.EndHorizontal();

            for (int i = 0, iMax = tags.Count; i < iMax; i++) {
                var tag = tags[i];
                if (tag == null) {
                    EditorGUILayout.LabelField($"Broken tag - is null, index {i}", EditorStyles.boldLabel);
                    if (GUILayout.Button("Delete all broken tags", Ui.ButtonStyleTheme, Ui.ExpandWidthFalse())) {
                        provider.DeleteAllBrokenTags();
                        EditorUtility.SetDirty(obj);
                    }

                    EditorGUILayout.Space(2);
                    continue;
                }

                var type = tag.GetType();
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
                        provider.OnDeleteTag(type);
                        EditorUtility.SetDirty(obj);
                    }
                }
                EditorGUILayout.EndHorizontal();
                EditorGUI.indentLevel--;
                EditorGUILayout.Space(2);
            }
        }

        private static void DrawTagsMenu<TWorld>(List<ITag> actualTags, Object obj, StaticEcsEntityProvider<TWorld> provider) where TWorld : struct, IWorldType {
            var menu = new GenericMenu();
            foreach (var tag in MetaData.Tags) {
                var has = false;
                foreach (var actual in actualTags) {
                    if (actual.GetType() == tag.Type) {
                        has = true;
                        break;
                    }
                }

                if (has) continue;

                if (provider.ShouldShowTag(tag.Type, Application.isPlaying)) {
                    menu.AddItem(new GUIContent(tag.FullName), false, objType => {
                                     provider.OnSelectTag((Type) objType);
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
