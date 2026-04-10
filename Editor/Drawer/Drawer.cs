using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace FFS.Libraries.StaticEcs.Unity.Editor {

    public static partial class Drawer {
        private const int MaxFieldToStringLength = 128;

        internal static readonly HashSet<int> openHideFlags = new();
        internal static readonly HashSet<int> initializedFoldouts = new();

        public static void DrawFoldoutBox(int keyHash, string open, string close, out bool show) {
            var style = new GUIStyle(EditorStyles.boldLabel) {
                hover = EditorStyles.iconButton.hover,
                active = EditorStyles.iconButton.active,
                focused = EditorStyles.iconButton.focused,
            };
            DrawFoldoutBox(keyHash, open, close, out show, style);
        }

        public static void DrawFoldoutBox(int keyHash, string open, string close, out bool show, Color color) {
            var style = new GUIStyle(EditorStyles.boldLabel) {
                normal = {
                    textColor = color,
                    background = null
                },
                hover = EditorStyles.iconButton.hover,
                active = EditorStyles.iconButton.active,
                focused = EditorStyles.iconButton.focused,
            };
            DrawFoldoutBox(keyHash, open, close, out show, style);
        }

        public static void DrawFoldoutBox(int keyHash, string open, string close, out bool show, GUIStyle style) {
            show = false;
            var rect = EditorGUILayout.GetControlRect();
            rect = EditorGUI.IndentedRect(rect);
            using (Ui.EnabledScope) {
                if (!openHideFlags.Contains(keyHash)) {
                    EditorGUILayout.BeginVertical(GUI.skin.box);
                    if (GUI.Button(rect, $"▸ {open}", style)) {
                        openHideFlags.Add(keyHash);
                    }

                    EditorGUILayout.EndVertical();
                } else {
                    EditorGUILayout.BeginVertical(GUI.skin.box);
                    if (GUI.Button(rect, $"▾ {close}", style)) {
                        openHideFlags.Remove(keyHash);
                    }

                    EditorGUILayout.EndVertical();
                    show = true;
                }
            }
        }

        internal static void DrawField(object component, FieldInfo field, GUIStyle style, GUILayoutOption[] layout) {
            var fieldValue = field.GetValue(component);
            var fieldType = field.FieldType;
            DrawTableValue(style, layout, fieldType, fieldValue);
        }

        internal static void DrawProperty(object component, PropertyInfo field, GUIStyle style, GUILayoutOption[] layout) {
            var fieldValue = field.GetValue(component);
            var fieldType = field.PropertyType;
            DrawTableValue(style, layout, fieldType, fieldValue);
        }

        private static void DrawTableValue(GUIStyle style, GUILayoutOption[] layout, Type fieldType, object fieldValue) {
            if (fieldType == MetaData.UnityObjectType || fieldType.IsSubclassOf(MetaData.UnityObjectType)) {
                EditorGUILayout.ObjectField((Object) fieldValue, fieldType, true, layout);
                return;
            }

            if (fieldType.IsEnum) {
                var isFlags = Attribute.IsDefined(fieldType, MetaData.EnumFlagsType);
                DrawEnum(fieldValue, isFlags, style, layout);
                return;
            }

            var strVal = fieldValue != null ? string.Format(CultureInfo.InvariantCulture, "{0}", fieldValue) : "null";
            if (strVal.Length > MaxFieldToStringLength) {
                strVal = strVal.Substring(0, MaxFieldToStringLength);
            }

            EditorGUILayout.LabelField(strVal, style, layout);
        }

        public static bool TryDrawObject(ref int level, string name, Type type, object value, out object newValue) {
            newValue = null;

            if (type == null) {
                DrawSelectableText(name, "null");
                return false;
            }

            if (MetaData.UnityObjectType == type || type.IsSubclassOf(MetaData.UnityObjectType)) {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel(name);
                newValue = EditorGUILayout.ObjectField("", (Object) value, type, true);
                EditorGUILayout.EndHorizontal();
                return (Object) newValue != (Object) value;
            }

            if (type.IsEnum) {
                var isFlags = Attribute.IsDefined(type, MetaData.EnumFlagsType);
                return DrawEnum(name, value, isFlags, out newValue);
            }

            if (type != typeof(string) && level >= 0) {
                var fields = MetaData.GetCachedSerializableType(type);

                if (fields.Length > 0) {
                    var changed = false;

                    EditorGUI.indentLevel++;
                    level--;
                    foreach (var field in fields) {
                        var fValue = field.GetValue(value);
                        if (TryDrawObject(ref level, field.Name, fValue?.GetType() ?? field.FieldType, fValue, out newValue)) {
                            field.SetValue(value, newValue);
                            changed = true;
                        }
                    }
                    level++;
                    EditorGUI.indentLevel--;

                    if (changed) {
                        newValue = value;
                    }

                    return changed;
                }
            }

            if (level < 0) {
                DrawSelectableText(name, "Display level of nested objects exceeded!");
            }

            if (value == null) {
                DrawSelectableText(name, "null");
                return false;
            }

            var strVal = string.Format(CultureInfo.InvariantCulture, "{0}", value);
            if (strVal.Length > MaxFieldToStringLength) {
                strVal = strVal.Substring(0, MaxFieldToStringLength);
            }

            DrawSelectableText(name, strVal);
            return false;
        }

        internal static bool DrawEnum(string label, object value, bool isFlags, out object newValue) {
            GUILayout.BeginHorizontal();
            {
                EditorGUILayout.PrefixLabel(label);
                newValue = isFlags
                    ? EditorGUILayout.EnumFlagsField((Enum) value)
                    : EditorGUILayout.EnumPopup((Enum) value);
            }
            GUILayout.EndHorizontal();
            return !Equals(newValue, value);
        }

        public static bool DrawStringArray(string label, ref string[] array) {
            var changed = false;
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
            if (GUILayout.Button("+", GUILayout.Width(24))) {
                Array.Resize(ref array, array.Length + 1);
                array[^1] = "New";
                changed = true;
            }
            EditorGUILayout.EndHorizontal();

            for (var i = 0; i < array.Length; i++) {
                EditorGUILayout.BeginHorizontal();
                var newVal = EditorGUILayout.TextField($"[{i}]", array[i]);
                if (newVal != array[i]) {
                    array[i] = newVal;
                    changed = true;
                }
                if (GUILayout.Button("-", GUILayout.Width(24))) {
                    var list = new System.Collections.Generic.List<string>(array);
                    list.RemoveAt(i);
                    array = list.ToArray();
                    changed = true;
                }
                EditorGUILayout.EndHorizontal();
                if (changed) break;
            }

            return changed;
        }

        public static bool DrawArray<T>(string label, ref T[] array, int level = 10) where T : struct {
            var changed = false;
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
            if (GUILayout.Button("+", GUILayout.Width(24))) {
                Array.Resize(ref array, array.Length + 1);
                changed = true;
            }
            EditorGUILayout.EndHorizontal();

            for (var i = 0; i < array.Length; i++) {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.BeginVertical(GUI.skin.box);
                var val = (object) array[i];
                if (TryDrawObject(ref level, $"[{i}]", typeof(T), val, out var newVal)) {
                    array[i] = (T) newVal;
                    changed = true;
                }
                EditorGUILayout.EndVertical();
                if (GUILayout.Button("-", GUILayout.Width(24))) {
                    var list = new System.Collections.Generic.List<T>(array);
                    list.RemoveAt(i);
                    array = list.ToArray();
                    changed = true;
                }
                EditorGUILayout.EndHorizontal();
                if (changed) break;
            }

            return changed;
        }

        internal static void DrawSelectableText(string label, string text) {
            GUILayout.BeginHorizontal();
            {
                EditorGUILayout.PrefixLabel(label);
                EditorGUILayout.SelectableLabel(text, GUILayout.MaxHeight(EditorGUIUtility.singleLineHeight));
            }
            GUILayout.EndHorizontal();
        }

        static void DrawEnum(object value, bool isFlags, GUIStyle style, GUILayoutOption[] layout) {
            if (isFlags) {
                EditorGUILayout.EnumFlagsField((Enum) value, style, layout);
            } else {
                EditorGUILayout.EnumPopup((Enum) value, style, layout);
            }
        }
    }
}
