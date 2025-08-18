#if !FFS_ECS_DISABLE_MASKS
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace FFS.Libraries.StaticEcs.Unity.Editor {
    public static partial class Drawer {
        private static readonly List<IMask> _masksCache = new();

        private static void DrawMasks<TProvider>(List<IMask> masks, TProvider provider) where TProvider : Object, IStaticEcsEntityProvider {
            EditorGUILayout.BeginHorizontal();
            {
                var hasAll = MetaData.Masks.Count == masks.Count;
                using (Ui.EnabledScopeVal(!hasAll && GUI.enabled)) {
                    if (Ui.PlusDropDownButton && !hasAll) {
                        DrawMasksMenu(masks, provider);
                    }
                }

                EditorGUILayout.LabelField(" Masks:", Ui.HeaderStyleTheme, Ui.ExpandWidthFalse());
            }
            EditorGUILayout.EndHorizontal();

            for (int i = 0, iMax = masks.Count; i < iMax; i++) {
                var mask =  masks[i];
                if (mask == null) {
                    EditorGUILayout.LabelField($"Broken mask - is null, index {i}", EditorStyles.boldLabel);
                    if (GUILayout.Button("Delete all broken masks", Ui.ButtonStyleTheme, Ui.ExpandWidthFalse())) {
                        provider.DeleteAllBrokenMasks();
                        EditorUtility.SetDirty(provider);
                    }
                    EditorGUILayout.Space(2);
                    continue;
                }
                var type = mask.GetType();
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
                        provider.OnDeleteMask(type);
                        EditorUtility.SetDirty(provider);
                    }
                }
                EditorGUILayout.EndHorizontal();
                EditorGUI.indentLevel--;
                EditorGUILayout.Space(2);
            }
        }
        
        private static void DrawMasksMenu<TProvider>(List<IMask> actualMasks, TProvider provider) where TProvider : Object, IStaticEcsEntityProvider {
            var menu = new GenericMenu();
            foreach (var mask in MetaData.Masks) {
                var has = false;
                foreach (var actual in actualMasks) {
                    if (actual.GetType() == mask.Type) {
                        has = true;
                        break;
                    }
                }

                if (has) continue;

                if (provider.ShouldShowMask(mask.Type, Application.isPlaying)) {
                    menu.AddItem(new GUIContent(mask.FullName), false, objType => {
                                     provider.OnSelectMask((Type) objType);
                                     EditorUtility.SetDirty(provider);
                                 },
                                 mask.Type);
                } else {
                    menu.AddDisabledItem(new GUIContent(mask.FullName));
                }
            }

            menu.ShowAsContext();
        }
    }
}
#endif
