using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEditor;
using UnityEngine;

namespace FFS.Libraries.StaticEcs.Unity.Editor.Inspectors {
    
    internal sealed class EntityStatusDrawer : IStaticEcsValueDrawer<EntityStatus> {
        public override bool DrawValue(string label, ref EntityStatus value) {
            EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            var newValue = EditorGUILayout.Toggle("Enabled", value.Value == EntityStatusType.Enabled);
            EditorGUI.indentLevel--;
            if (newValue == (value.Value == EntityStatusType.Enabled)) {
                return false;
            }

            value.Value = newValue 
                ? EntityStatusType.Enabled 
                : EntityStatusType.Disabled;
            return true;
        }

        public override void DrawTableValue(ref EntityStatus value, GUIStyle style, GUILayoutOption[] layoutOptions) {
            EditorGUILayout.SelectableLabel(value.Value.ToString(), style, layoutOptions);
        }
    }

    internal sealed class MultiComponentDrawer<T> : IStaticEcsValueDrawer<Multi<T>> where T : struct {
        public override bool DrawValue(string label, ref Multi<T> value) {
            EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
            if (value.data == null) {
                EditorGUILayout.LabelField("Data is empty, it will be created when the entity is active", EditorStyles.boldLabel);
                return false;
            }
            EditorGUILayout.LabelField("Elements:", EditorStyles.boldLabel);

            EditorGUI.indentLevel++;
            var type = typeof(T);
            var typeName = type.EditorTypeName();
            for (ushort i = 0; i < value.Count; i++) {
                var val = value[i];
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("X", Ui.Width(20))) {
                    value.RemoveAt(i);
                    return true;
                }
                EditorGUILayout.EndHorizontal();
       
                if (Drawer.TryDrawValueByCustomDrawer(typeName, type, val, out var changed, out var newValue)) {
                    if (changed) {
                        value[i] = (T) newValue;
                    }
                } else {
                    EditorGUILayout.LabelField(typeName, EditorStyles.boldLabel);
                    EditorGUI.indentLevel++;
                    foreach (var field in MetaData.GetCachedType(type)) {
                        if (Drawer.TryDrawField(val, field, out newValue)) {
                            field.SetValue(val, newValue);
                            changed = true;
                            value[i] = val;
                        }
                    }

                    EditorGUI.indentLevel--;
                }

                if (changed) {
                    return true;
                }
            }

            if (GUILayout.Button("Add Element")) {
                value.Add(default(T));
                return true;
            }

            EditorGUI.indentLevel--;
            return false;
        }

        public override void DrawTableValue(ref Multi<T> value, GUIStyle style, GUILayoutOption[] layoutOptions) {
            EditorGUILayout.SelectableLabel($"Count: {value.Count}, Cap: {value.Capacity}", style, layoutOptions);
        }
    }
    
    internal sealed class ListDrawer<T> : IStaticEcsValueDrawer<List<T>> where T : struct {
        
        public override bool IsNullAllowed() => true;
        
        public override bool DrawValue(string label, ref List<T> value) {
            EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
            if (value == null) {
                EditorGUILayout.LabelField("Data is empty", EditorStyles.boldLabel);
                if (GUILayout.Button("Create new")) {
                    value = new List<T>();
                    return true;
                }
                return false;
            }
            EditorGUILayout.LabelField("Elements:", EditorStyles.boldLabel);

            EditorGUI.indentLevel++;
            var type = typeof(T);
            var typeName = type.EditorTypeName();
            for (ushort i = 0; i < value.Count; i++) {
                var val = value[i];
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("X", Ui.Width(20))) {
                    value.RemoveAt(i);
                    return true;
                }
                EditorGUILayout.EndHorizontal();
       
                if (Drawer.TryDrawValueByCustomDrawer(typeName, type, val, out var changed, out var newValue)) {
                    if (changed) {
                        value[i] = (T) newValue;
                    }
                } else {
                    EditorGUILayout.LabelField(typeName, EditorStyles.boldLabel);
                    EditorGUI.indentLevel++;
                    foreach (var field in MetaData.GetCachedType(type)) {
                        if (Drawer.TryDrawField(val, field, out newValue)) {
                            field.SetValue(val, newValue);
                            changed = true;
                            value[i] = val;
                        }
                    }

                    EditorGUI.indentLevel--;
                }

                if (changed) {
                    return true;
                }
            }

            if (GUILayout.Button("Add Element")) {
                value.Add(default(T));
                return true;
            }

            EditorGUI.indentLevel--;
            return false;
        }

        public override void DrawTableValue(ref List<T> value, GUIStyle style, GUILayoutOption[] layoutOptions) {
            EditorGUILayout.SelectableLabel($"Count: {value.Count}, Cap: {value.Capacity}", style, layoutOptions);
        }
    }
    
    internal sealed class ArrayDrawer<T> : IStaticEcsValueDrawer<T[]> {
        public override bool IsNullAllowed() => true;

        public override bool DrawValue(string label, ref T[] value) {
            EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
            if (value == null) {
                EditorGUILayout.LabelField("Data is null", EditorStyles.boldLabel);
                if (GUILayout.Button("Create new")) {
                    value = new T[4];
                    return true;
                }
                return false;
            }
            EditorGUILayout.LabelField("Elements:", EditorStyles.boldLabel);

            EditorGUI.indentLevel++;
            var type = typeof(T);
            var typeName = type.EditorTypeName();
            for (ushort i = 0; i < value.Length; i++) {
                var val = value[i];
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("X", Ui.Width(20))) {
                    if (i == value.Length - 1) {
                        value[i] = default;
                    } else {
                        Utils.LoopFallbackCopy(value, (uint) (i + 1), value, i, (uint) (value.Length - 1 - i));
                        value[value.Length - 1] = default;
                    }
                    return true;
                }
                EditorGUILayout.EndHorizontal();
       
                if (Drawer.TryDrawValueByCustomDrawer(typeName, type, val, out var changed, out var newValue)) {
                    if (changed) {
                        value[i] = (T) newValue;
                    }
                } else {
                    EditorGUILayout.LabelField(typeName, EditorStyles.boldLabel);
                    EditorGUI.indentLevel++;
                    foreach (var field in MetaData.GetCachedType(type)) {
                        if (Drawer.TryDrawField(val, field, out newValue)) {
                            field.SetValue(val, newValue);
                            changed = true;
                            value[i] = val;
                        }
                    }

                    EditorGUI.indentLevel--;
                }

                if (changed) {
                    return true;
                }
            }
            
            if (GUILayout.Button("Resize")) {
                var arr = new T[value.Length << 1];
                Array.Copy(value, arr, value.Length);
                value = arr;
                return true;
            }

            EditorGUI.indentLevel--;
            return false;
        }

        public override void DrawTableValue(ref T[] value, GUIStyle style, GUILayoutOption[] layoutOptions) {
            EditorGUILayout.SelectableLabel($"Length: {value.Length}", style, layoutOptions);
        }
    }
    
    public sealed class BoolDrawer : IStaticEcsValueDrawer<bool> {
        public override bool DrawValue(string label, ref bool value) {
            var newValue = EditorGUILayout.Toggle(label, value);
            if (newValue == value) {
                return false;
            }

            value = newValue;
            return true;
        }

        public override void DrawTableValue(ref bool value, GUIStyle style, GUILayoutOption[] layoutOptions) {
            EditorGUILayout.Toggle(value, style, layoutOptions);
        }
    }

    public sealed class DoubleDrawer : IStaticEcsValueDrawer<double> {
        public override bool DrawValue(string label, ref double value) {
            var newValue = EditorGUILayout.DoubleField(label, value);
            if (System.Math.Abs(newValue - value) < double.Epsilon) {
                return false;
            }

            value = newValue;
            return true;
        }

        public override void DrawTableValue(ref double value, GUIStyle style, GUILayoutOption[] layoutOptions) {
            EditorGUILayout.SelectableLabel(value.ToString("0.##", CultureInfo.InvariantCulture), style, layoutOptions);
        }
    }

    public sealed class FloatDrawer : IStaticEcsValueDrawer<float> {
        public override bool DrawValue(string label, ref float value) {
            var newValue = EditorGUILayout.FloatField(label, value);
            if (System.Math.Abs(newValue - value) < float.Epsilon) {
                return false;
            }

            value = newValue;
            return true;
        }

        public override void DrawTableValue(ref float value, GUIStyle style, GUILayoutOption[] layoutOptions) {
            EditorGUILayout.SelectableLabel(value.ToString("0.##", CultureInfo.InvariantCulture), style, layoutOptions);
        }
    }
    
    public sealed class ULongDrawer : IStaticEcsValueDrawer<ulong> {
        public override bool DrawValue(string label, ref ulong value) {
            var newValue = (long) value;
            newValue = EditorGUILayout.LongField(label, newValue);
            if (newValue < 0) {
                newValue = 0;
            }
            
            if (newValue == (long) value) {
                return false;
            }

            value = (ulong) newValue;
            return true;
        }

        public override void DrawTableValue(ref ulong value, GUIStyle style, GUILayoutOption[] layoutOptions) {
            EditorGUILayout.SelectableLabel(value.ToString(CultureInfo.InvariantCulture), style, layoutOptions);
        }
    }

    public sealed class LongDrawer : IStaticEcsValueDrawer<long> {
        public override bool DrawValue(string label, ref long value) {
            var newValue = EditorGUILayout.LongField(label, value);
            if (newValue == value) {
                return false;
            }

            value = newValue;
            return true;
        }

        public override void DrawTableValue(ref long value, GUIStyle style, GUILayoutOption[] layoutOptions) {
            EditorGUILayout.SelectableLabel(value.ToString(CultureInfo.InvariantCulture), style, layoutOptions);
        }
    }
    
    public sealed class UIntDrawer : IStaticEcsValueDrawer<uint> {
        public override bool DrawValue(string label, ref uint value) {
            var newValue = EditorGUILayout.LongField(label, value);
            if (newValue < 0) {
                newValue = 0;
            } else if (newValue > uint.MaxValue) {
                newValue = uint.MaxValue;
            }
            
            if (newValue == value) {
                return false;
            }

            value = (uint) newValue;
            return true;
        }

        public override void DrawTableValue(ref uint value, GUIStyle style, GUILayoutOption[] layoutOptions) {
            EditorGUILayout.SelectableLabel(value.ToString(CultureInfo.InvariantCulture), style, layoutOptions);
        }
    }

    public sealed class IntDrawer : IStaticEcsValueDrawer<int> {
        public override bool DrawValue(string label, ref int value) {
            var newValue = EditorGUILayout.IntField(label, value);
            if (newValue == value) {
                return false;
            }

            value = newValue;
            return true;
        }

        public override void DrawTableValue(ref int value, GUIStyle style, GUILayoutOption[] layoutOptions) {
            EditorGUILayout.SelectableLabel(value.ToString(CultureInfo.InvariantCulture), style, layoutOptions);
        }
    }
    
    public sealed class UShortDrawer : IStaticEcsValueDrawer<ushort> {
        public override bool DrawValue(string label, ref ushort value) {
            var newValue = EditorGUILayout.IntField(label, value);
            if (newValue < 0) {
                newValue = 0;
            } else if (newValue > ushort.MaxValue) {
                newValue = ushort.MaxValue;
            }
            
            if (newValue == value) {
                return false;
            }

            value = (ushort) newValue;
            return true;
        }

        public override void DrawTableValue(ref ushort value, GUIStyle style, GUILayoutOption[] layoutOptions) {
            EditorGUILayout.SelectableLabel(value.ToString(CultureInfo.InvariantCulture), style, layoutOptions);
        }
    }
    
    public sealed class ShortDrawer : IStaticEcsValueDrawer<short> {
        public override bool DrawValue(string label, ref short value) {
            var newValue = EditorGUILayout.IntField(label, value);
            if (newValue < short.MinValue) {
                newValue = short.MinValue;
            } else if (newValue > short.MaxValue) {
                newValue = short.MaxValue;
            }
            
            if (newValue == value) {
                return false;
            }

            value = (short) newValue;
            return true;
        }

        public override void DrawTableValue(ref short value, GUIStyle style, GUILayoutOption[] layoutOptions) {
            EditorGUILayout.SelectableLabel(value.ToString(CultureInfo.InvariantCulture), style, layoutOptions);
        }
    }
    
    public sealed class ByteDrawer : IStaticEcsValueDrawer<byte> {
        public override bool DrawValue(string label, ref byte value) {
            var newValue = EditorGUILayout.IntField(label, value);
            if (newValue < byte.MinValue) {
                newValue = byte.MinValue;
            } else if (newValue > byte.MaxValue) {
                newValue = byte.MaxValue;
            }
            
            if (newValue == value) {
                return false;
            }

            value = (byte) newValue;
            return true;
        }

        public override void DrawTableValue(ref byte value, GUIStyle style, GUILayoutOption[] layoutOptions) {
            EditorGUILayout.SelectableLabel(value.ToString(CultureInfo.InvariantCulture), style, layoutOptions);
        }
    }
    
    public sealed class SByteDrawer : IStaticEcsValueDrawer<sbyte> {
        public override bool DrawValue(string label, ref sbyte value) {
            var newValue = EditorGUILayout.IntField(label, value);
            if (newValue < sbyte.MinValue) {
                newValue = sbyte.MinValue;
            } else if (newValue > sbyte.MaxValue) {
                newValue = sbyte.MaxValue;
            }
            
            if (newValue == value) {
                return false;
            }

            value = (sbyte) newValue;
            return true;
        }

        public override void DrawTableValue(ref sbyte value, GUIStyle style, GUILayoutOption[] layoutOptions) {
            EditorGUILayout.SelectableLabel(value.ToString(CultureInfo.InvariantCulture), style, layoutOptions);
        }
    }

    public sealed class StringDrawer : IStaticEcsValueDrawer<string> {
        public override bool IsNullAllowed() {
            return true;
        }

        public override bool DrawValue(string label, ref string value) {
            var newValue = EditorGUILayout.TextField(label, value);
            if (newValue == value) {
                return false;
            }

            value = newValue;
            return true;
        }

        public override void DrawTableValue(ref string value, GUIStyle style, GUILayoutOption[] layoutOptions) {
            EditorGUILayout.SelectableLabel(value, style, layoutOptions);
        }
    }


    public sealed class AnimationCurveDrawer : IStaticEcsValueDrawer<AnimationCurve> {
        public override bool DrawValue(string label, ref AnimationCurve value) {
            var newValue = EditorGUILayout.CurveField(label, value);
            if (newValue.Equals(value)) {
                return false;
            }

            value = newValue;
            return true;
        }

        public override void DrawTableValue(ref AnimationCurve value, GUIStyle style, GUILayoutOption[] layoutOptions) {
            EditorGUILayout.CurveField(value, layoutOptions);
        }
    }

    public sealed class BoundsDrawer : IStaticEcsValueDrawer<Bounds> {
        public override bool DrawValue(string label, ref Bounds value) {
            var newValue = EditorGUILayout.BoundsField(label, value);
            if (newValue == value) {
                return false;
            }

            value = newValue;
            return true;
        }

        public override void DrawTableValue(ref Bounds value, GUIStyle style, GUILayoutOption[] layoutOptions) {
            EditorGUILayout.BoundsField(value, layoutOptions);
        }
    }

    public sealed class BoundsIntDrawer : IStaticEcsValueDrawer<BoundsInt> {
        public override bool DrawValue(string label, ref BoundsInt value) {
            var newValue = EditorGUILayout.BoundsIntField(label, value);
            if (newValue == value) {
                return false;
            }

            value = newValue;
            return true;
        }

        public override void DrawTableValue(ref BoundsInt value, GUIStyle style, GUILayoutOption[] layoutOptions) {
            EditorGUILayout.BoundsIntField(value, layoutOptions);
        }
    }

    public sealed class ColorDrawer : IStaticEcsValueDrawer<Color> {
        public override bool DrawValue(string label, ref Color value) {
            var newValue = EditorGUILayout.ColorField(label, value);
            if (newValue == value) {
                return false;
            }

            value = newValue;
            return true;
        }

        public override void DrawTableValue(ref Color value, GUIStyle style, GUILayoutOption[] layoutOptions) {
            EditorGUILayout.ColorField(value, layoutOptions);
        }
    }

    public sealed class Color32Drawer : IStaticEcsValueDrawer<Color32> {
        public override bool DrawValue(string label, ref Color32 value) {
            var newValue = EditorGUILayout.ColorField(label, value);
            if (newValue == value) {
                return false;
            }

            value = newValue;
            return true;
        }

        public override void DrawTableValue(ref Color32 value, GUIStyle style, GUILayoutOption[] layoutOptions) {
            EditorGUILayout.ColorField(value, layoutOptions);
        }
    }

    public sealed class GradientDrawer : IStaticEcsValueDrawer<Gradient> {
        public override bool DrawValue(string label, ref Gradient value) {
            var newValue = EditorGUILayout.GradientField(label, value);
            if (newValue.Equals(value)) {
                return false;
            }

            value = newValue;
            return true;
        }

        public override void DrawTableValue(ref Gradient value, GUIStyle style, GUILayoutOption[] layoutOptions) {
            EditorGUILayout.GradientField(value, layoutOptions);
        }
    }

    public sealed class LayerMaskDrawer : IStaticEcsValueDrawer<LayerMask> {
        static string[] _layerNames;

        static string[] GetLayerNames() {
            if (_layerNames == null) {
                var size = 0;
                var names = new string[32];
                for (var i = 0; i < names.Length; i++) {
                    names[i] = LayerMask.LayerToName(i);
                    if (names[i].Length > 0) {
                        size = i + 1;
                    }
                }

                if (size != names.Length) {
                    System.Array.Resize(ref names, size);
                }

                _layerNames = names;
            }

            return _layerNames;
        }

        public override bool DrawValue(string label, ref LayerMask value) {
            var newValue = EditorGUILayout.MaskField(label, value, GetLayerNames());
            if (newValue == value) {
                return false;
            }

            value = newValue;
            return true;
        }

        public override void DrawTableValue(ref LayerMask value, GUIStyle style, GUILayoutOption[] layoutOptions) {
            EditorGUILayout.MaskField(value, GetLayerNames(), layoutOptions);
        }
    }

    public sealed class QuaternionDrawer : IStaticEcsValueDrawer<Quaternion> {
        public override bool DrawValue(string label, ref Quaternion value) {
            var eulerAngles = value.eulerAngles;
            var newValue = EditorGUILayout.Vector3Field(label, eulerAngles);
            if (newValue == eulerAngles) {
                return false;
            }

            value = Quaternion.Euler(newValue);
            return true;
        }

        public override void DrawTableValue(ref Quaternion value, GUIStyle style, GUILayoutOption[] layoutOptions) {
            EditorGUILayout.SelectableLabel(value.eulerAngles.ToString(), style, layoutOptions);
        }
    }

    public sealed class RectDrawer : IStaticEcsValueDrawer<Rect> {
        public override bool DrawValue(string label, ref Rect value) {
            var newValue = EditorGUILayout.RectField(label, value);
            if (newValue == value) {
                return false;
            }

            value = newValue;
            return true;
        }

        public override void DrawTableValue(ref Rect value, GUIStyle style, GUILayoutOption[] layoutOptions) {
            EditorGUILayout.RectField(value, layoutOptions);
        }
    }

    public sealed class Vector2Drawer : IStaticEcsValueDrawer<Vector2> {
        public override bool DrawValue(string label, ref Vector2 value) {
            var newValue = EditorGUILayout.Vector2Field(label, value);
            if (newValue == value) {
                return false;
            }

            value = newValue;
            return true;
        }

        public override void DrawTableValue(ref Vector2 value, GUIStyle style, GUILayoutOption[] layoutOptions) {
            EditorGUILayout.SelectableLabel(value.ToString(), style, layoutOptions);
        }
    }

    public sealed class Vector2IntDrawer : IStaticEcsValueDrawer<Vector2Int> {
        public override bool DrawValue(string label, ref Vector2Int value) {
            var newValue = EditorGUILayout.Vector2IntField(label, value);
            if (newValue == value) {
                return false;
            }

            value = newValue;
            return true;
        }

        public override void DrawTableValue(ref Vector2Int value, GUIStyle style, GUILayoutOption[] layoutOptions) {
            EditorGUILayout.SelectableLabel(value.ToString(), style, layoutOptions);
        }
    }

    public sealed class Vector3Drawer : IStaticEcsValueDrawer<Vector3> {
        public override bool DrawValue(string label, ref Vector3 value) {
            var newValue = EditorGUILayout.Vector3Field(label, value);
            if (newValue == value) {
                return false;
            }

            value = newValue;
            return true;
        }

        public override void DrawTableValue(ref Vector3 value, GUIStyle style, GUILayoutOption[] layoutOptions) {
            EditorGUILayout.SelectableLabel(value.ToString(), style, layoutOptions);
        }
    }

    public sealed class Vector3IntDrawer : IStaticEcsValueDrawer<Vector3Int> {
        public override bool DrawValue(string label, ref Vector3Int value) {
            var newValue = EditorGUILayout.Vector3IntField(label, value);
            if (newValue == value) {
                return false;
            }

            value = newValue;
            return true;
        }

        public override void DrawTableValue(ref Vector3Int value, GUIStyle style, GUILayoutOption[] layoutOptions) {
            EditorGUILayout.SelectableLabel(value.ToString(), style, layoutOptions);
        }
    }

    public sealed class Vector4Drawer : IStaticEcsValueDrawer<Vector4> {
        public override bool DrawValue(string label, ref Vector4 value) {
            var newValue = EditorGUILayout.Vector4Field(label, value);
            if (newValue == value) {
                return false;
            }

            value = newValue;
            return true;
        }

        public override void DrawTableValue(ref Vector4 value, GUIStyle style, GUILayoutOption[] layoutOptions) {
            EditorGUILayout.SelectableLabel(value.ToString(), style, layoutOptions);
        }
    }
}