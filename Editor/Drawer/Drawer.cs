using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace FFS.Libraries.StaticEcs.Unity.Editor {

    public struct DrawContext {
        public IWorld World;
        public int Level;
    }
    
    public static partial class Drawer {
        private const int MaxFieldToStringLength = 128;
        internal const int MaxRecursionLvl = 10;

        internal static readonly HashSet<string> openHideFlags = new();
        
        public static void DrawFoldoutBox(string key, string open, string close, out bool show) {
            var style = new GUIStyle(EditorStyles.boldLabel) {
                hover  = EditorStyles.iconButton.hover,
                active = EditorStyles.iconButton.active,
                focused = EditorStyles.iconButton.focused,
            };
            DrawFoldoutBox(key, open, close, out show, style);
        }
        
        public static void DrawFoldoutBox(string key, string open, string close, out bool show, Color color) {
            var style = new GUIStyle(EditorStyles.boldLabel) {
                normal = {
                    textColor = color,
                    background = null
                },
                hover  = EditorStyles.iconButton.hover,
                active = EditorStyles.iconButton.active,
                focused = EditorStyles.iconButton.focused,
            };
            DrawFoldoutBox(key, open, close, out show, style);
        }
        
        public static void DrawFoldoutBox(string key, string open, string close, out bool show, GUIStyle style) {
            show = false;
            var rect = EditorGUILayout.GetControlRect();
            rect = EditorGUI.IndentedRect(rect);
            using (Ui.EnabledScope) {
                if (!openHideFlags.Contains(key)) {
                    EditorGUILayout.BeginVertical(GUI.skin.box);
                    if (GUI.Button(rect, $"▸ {open}", style)) {
                        openHideFlags.Add(key);
                    }

                    EditorGUILayout.EndVertical();
                } else {
                    EditorGUILayout.BeginVertical(GUI.skin.box);
                    if (GUI.Button(rect,$"▾ {close}", style)) {
                        openHideFlags.Remove(key);
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
            if (TryDrawTableValueByCustomDrawer(fieldType, fieldValue, style, layout)) {
                return;
            }

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

        public static bool TryDrawObject(ref DrawContext ctx, string name, Type type, object value, out object newValue) {
            newValue = null;
            
            if (type != null && FindCustomDrawer(type, out var drawer)) {
                return drawer.DrawValue(ref ctx, name, value, out newValue);
            }

            if (MetaData.UnityObjectType == type || type!.IsSubclassOf(MetaData.UnityObjectType)) {
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

            if (type != typeof(string) && ctx.Level >= 0) {
                var fields = MetaData.GetCachedSerializableType(type);

                if (fields.Length > 0) {
                    var changed = false;
                    
                    EditorGUI.indentLevel++;
                    ctx.Level--;
                    foreach (var field in fields) {
           				using (Ui.EnabledScopeVal(GUI.enabled && Application.isPlaying && !HasReadonlyAttribute(field))) {
                            var fValue = field.GetValue(value);
                            if (TryDrawObject(ref ctx, field.Name.ToPascalCaseNoUnderscore(), fValue?.GetType() ?? field.FieldType, fValue, out newValue)) {
                                field.SetValue(value, newValue);
                                changed = true;
                            }
                        }
                    }
                    ctx.Level++;
                    EditorGUI.indentLevel--;
                    
                    if (changed) {
                        newValue = value;
                    }

                    return changed;
                } 
            }

            if (ctx.Level < 0) {
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
        
        private static bool HasReadonlyAttribute(FieldInfo field) {
            var attrType = typeof(StaticEcsEditorRuntimeReadOnlyAttribute);
            foreach (var customAttribute in field.GetCustomAttributesData()) {
                if (customAttribute.AttributeType.Namespace == attrType.Namespace && customAttribute.AttributeType.FullName == attrType.FullName) {
                    return true;
                }
            }

            return false;
        }

        private static bool FindCustomDrawer(Type type, out IStaticEcsValueDrawer drawer) {
            if (MetaData.Inspectors.TryGetValue(type, out drawer)) {
                return true;
            }
            
            if (type.IsGenericType && MetaData.InspectorsGeneric.TryGetValue(type.GetGenericTypeDefinition(), out var inspectorType)) {
                var ins = (IStaticEcsValueDrawer) Activator.CreateInstance(inspectorType.MakeGenericType(type.GetGenericArguments()));
                MetaData.Inspectors[type] = ins;
                drawer = ins;
                return true;
            }
            
            if (type.IsArray && type.GetArrayRank() == 1 && MetaData.InspectorsGeneric.TryGetValue(type.BaseType, out var inspectorTypeArray)) {
                var ins = (IStaticEcsValueDrawer) Activator.CreateInstance(inspectorTypeArray.MakeGenericType(type.GetElementType()));
                MetaData.Inspectors[type] = ins;
                drawer = ins;
                return true;
            }
            
            if (type.IsArray && type.GetArrayRank() == 2 && MetaData.InspectorsmultiArray2.TryGetValue(type.BaseType, out inspectorTypeArray)) {
                var ins = (IStaticEcsValueDrawer)Activator.CreateInstance(inspectorTypeArray.MakeGenericType(type.GetElementType()));
                MetaData.Inspectors[type] = ins;
                drawer = ins;
                return true;
            }
            
            if (type.IsArray && type.GetArrayRank() == 3 && MetaData.InspectorsmultiArray3.TryGetValue(type.BaseType, out inspectorTypeArray)) {
                var ins = (IStaticEcsValueDrawer)Activator.CreateInstance(inspectorTypeArray.MakeGenericType(type.GetElementType()));
                MetaData.Inspectors[type] = ins;
                drawer = ins;
                return true;
            }

            return false;
        }

        internal static bool TryDrawTableValueByCustomDrawer(Type type, object value, GUIStyle style, GUILayoutOption[] layoutOptions) {
            if (MetaData.Inspectors.TryGetValue(type, out var inspector)) {
                inspector.DrawTableValue(value, style, layoutOptions);
                return true;
            }
            
            if (type.IsGenericType && MetaData.InspectorsGeneric.TryGetValue(type.GetGenericTypeDefinition(), out var inspectorType)) {
                var ins = (IStaticEcsValueDrawer) Activator.CreateInstance(inspectorType.MakeGenericType(type.GetGenericArguments()));
                MetaData.Inspectors[type] = ins;
                ins.DrawTableValue(value, style, layoutOptions);
                return true;
            }

            return false;
        }

        internal static void DrawSelectableText(string label, string text) {
            GUILayout.BeginHorizontal();
            {
                EditorGUILayout.PrefixLabel(label);
                EditorGUILayout.SelectableLabel(text, GUILayout.MaxHeight(EditorGUIUtility.singleLineHeight));
            }
            GUILayout.EndHorizontal();
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

        static void DrawEnum(object value, bool isFlags, GUIStyle style, GUILayoutOption[] layout) {
            if (isFlags) {
                EditorGUILayout.EnumFlagsField((Enum) value, style, layout);
            } else {
                EditorGUILayout.EnumPopup((Enum) value, style, layout);
            }
        }
    }
}