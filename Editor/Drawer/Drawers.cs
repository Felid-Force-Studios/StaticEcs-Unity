﻿using System.Globalization;
using UnityEditor;
using UnityEngine;

namespace FFS.Libraries.StaticEcs.Unity.Editor.Inspectors {
    sealed class BoolDrawer : IStaticEcsValueDrawer<bool> {
        protected override bool DrawValue(string label, ref bool value) {
            var newValue = EditorGUILayout.Toggle(label, value);
            if (newValue == value) {
                return false;
            }

            value = newValue;
            return true;
        }

        protected override void DrawTableValue(ref bool value, GUIStyle style, GUILayoutOption[] layoutOptions) {
            EditorGUILayout.Toggle(value, style, layoutOptions);
        }
    }

    sealed class DoubleDrawer : IStaticEcsValueDrawer<double> {
        protected override bool DrawValue(string label, ref double value) {
            var newValue = EditorGUILayout.DoubleField(label, value);
            if (System.Math.Abs(newValue - value) < double.Epsilon) {
                return false;
            }

            value = newValue;
            return true;
        }

        protected override void DrawTableValue(ref double value, GUIStyle style, GUILayoutOption[] layoutOptions) {
            EditorGUILayout.SelectableLabel(value.ToString("0.##", CultureInfo.InvariantCulture), style, layoutOptions);
        }
    }

    sealed class FloatDrawer : IStaticEcsValueDrawer<float> {
        protected override bool DrawValue(string label, ref float value) {
            var newValue = EditorGUILayout.FloatField(label, value);
            if (System.Math.Abs(newValue - value) < float.Epsilon) {
                return false;
            }

            value = newValue;
            return true;
        }

        protected override void DrawTableValue(ref float value, GUIStyle style, GUILayoutOption[] layoutOptions) {
            EditorGUILayout.SelectableLabel(value.ToString("0.##", CultureInfo.InvariantCulture), style, layoutOptions);
        }
    }
    
    sealed class ULongDrawer : IStaticEcsValueDrawer<ulong> {
        protected override bool DrawValue(string label, ref ulong value) {
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

        protected override void DrawTableValue(ref ulong value, GUIStyle style, GUILayoutOption[] layoutOptions) {
            EditorGUILayout.SelectableLabel(value.ToString(CultureInfo.InvariantCulture), style, layoutOptions);
        }
    }

    sealed class LongDrawer : IStaticEcsValueDrawer<long> {
        protected override bool DrawValue(string label, ref long value) {
            var newValue = EditorGUILayout.LongField(label, value);
            if (newValue == value) {
                return false;
            }

            value = newValue;
            return true;
        }

        protected override void DrawTableValue(ref long value, GUIStyle style, GUILayoutOption[] layoutOptions) {
            EditorGUILayout.SelectableLabel(value.ToString(CultureInfo.InvariantCulture), style, layoutOptions);
        }
    }
    
    sealed class UIntDrawer : IStaticEcsValueDrawer<uint> {
        protected override bool DrawValue(string label, ref uint value) {
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

        protected override void DrawTableValue(ref uint value, GUIStyle style, GUILayoutOption[] layoutOptions) {
            EditorGUILayout.SelectableLabel(value.ToString(CultureInfo.InvariantCulture), style, layoutOptions);
        }
    }

    sealed class IntDrawer : IStaticEcsValueDrawer<int> {
        protected override bool DrawValue(string label, ref int value) {
            var newValue = EditorGUILayout.IntField(label, value);
            if (newValue == value) {
                return false;
            }

            value = newValue;
            return true;
        }

        protected override void DrawTableValue(ref int value, GUIStyle style, GUILayoutOption[] layoutOptions) {
            EditorGUILayout.SelectableLabel(value.ToString(CultureInfo.InvariantCulture), style, layoutOptions);
        }
    }
    
    sealed class UShortDrawer : IStaticEcsValueDrawer<ushort> {
        protected override bool DrawValue(string label, ref ushort value) {
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

        protected override void DrawTableValue(ref ushort value, GUIStyle style, GUILayoutOption[] layoutOptions) {
            EditorGUILayout.SelectableLabel(value.ToString(CultureInfo.InvariantCulture), style, layoutOptions);
        }
    }
    
    sealed class ShortDrawer : IStaticEcsValueDrawer<short> {
        protected override bool DrawValue(string label, ref short value) {
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

        protected override void DrawTableValue(ref short value, GUIStyle style, GUILayoutOption[] layoutOptions) {
            EditorGUILayout.SelectableLabel(value.ToString(CultureInfo.InvariantCulture), style, layoutOptions);
        }
    }
    
    sealed class ByteDrawer : IStaticEcsValueDrawer<byte> {
        protected override bool DrawValue(string label, ref byte value) {
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

        protected override void DrawTableValue(ref byte value, GUIStyle style, GUILayoutOption[] layoutOptions) {
            EditorGUILayout.SelectableLabel(value.ToString(CultureInfo.InvariantCulture), style, layoutOptions);
        }
    }
    
    sealed class SByteDrawer : IStaticEcsValueDrawer<sbyte> {
        protected override bool DrawValue(string label, ref sbyte value) {
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

        protected override void DrawTableValue(ref sbyte value, GUIStyle style, GUILayoutOption[] layoutOptions) {
            EditorGUILayout.SelectableLabel(value.ToString(CultureInfo.InvariantCulture), style, layoutOptions);
        }
    }

    sealed class StringDrawer : IStaticEcsValueDrawer<string> {
        public override bool IsNullAllowed() {
            return true;
        }

        protected override bool DrawValue(string label, ref string value) {
            var newValue = EditorGUILayout.TextField(label, value);
            if (newValue == value) {
                return false;
            }

            value = newValue;
            return true;
        }

        protected override void DrawTableValue(ref string value, GUIStyle style, GUILayoutOption[] layoutOptions) {
            EditorGUILayout.SelectableLabel(value, style, layoutOptions);
        }
    }


    sealed class AnimationCurveDrawer : IStaticEcsValueDrawer<AnimationCurve> {
        protected override bool DrawValue(string label, ref AnimationCurve value) {
            var newValue = EditorGUILayout.CurveField(label, value);
            if (newValue.Equals(value)) {
                return false;
            }

            value = newValue;
            return true;
        }

        protected override void DrawTableValue(ref AnimationCurve value, GUIStyle style, GUILayoutOption[] layoutOptions) {
            EditorGUILayout.CurveField(value, layoutOptions);
        }
    }

    sealed class BoundsDrawer : IStaticEcsValueDrawer<Bounds> {
        protected override bool DrawValue(string label, ref Bounds value) {
            var newValue = EditorGUILayout.BoundsField(label, value);
            if (newValue == value) {
                return false;
            }

            value = newValue;
            return true;
        }

        protected override void DrawTableValue(ref Bounds value, GUIStyle style, GUILayoutOption[] layoutOptions) {
            EditorGUILayout.BoundsField(value, layoutOptions);
        }
    }

    sealed class BoundsIntDrawer : IStaticEcsValueDrawer<BoundsInt> {
        protected override bool DrawValue(string label, ref BoundsInt value) {
            var newValue = EditorGUILayout.BoundsIntField(label, value);
            if (newValue == value) {
                return false;
            }

            value = newValue;
            return true;
        }

        protected override void DrawTableValue(ref BoundsInt value, GUIStyle style, GUILayoutOption[] layoutOptions) {
            EditorGUILayout.BoundsIntField(value, layoutOptions);
        }
    }

    sealed class ColorDrawer : IStaticEcsValueDrawer<Color> {
        protected override bool DrawValue(string label, ref Color value) {
            var newValue = EditorGUILayout.ColorField(label, value);
            if (newValue == value) {
                return false;
            }

            value = newValue;
            return true;
        }

        protected override void DrawTableValue(ref Color value, GUIStyle style, GUILayoutOption[] layoutOptions) {
            EditorGUILayout.ColorField(value, layoutOptions);
        }
    }

    sealed class Color32Drawer : IStaticEcsValueDrawer<Color32> {
        protected override bool DrawValue(string label, ref Color32 value) {
            var newValue = EditorGUILayout.ColorField(label, value);
            if (newValue == value) {
                return false;
            }

            value = newValue;
            return true;
        }

        protected override void DrawTableValue(ref Color32 value, GUIStyle style, GUILayoutOption[] layoutOptions) {
            EditorGUILayout.ColorField(value, layoutOptions);
        }
    }

    sealed class GradientDrawer : IStaticEcsValueDrawer<Gradient> {
        protected override bool DrawValue(string label, ref Gradient value) {
            var newValue = EditorGUILayout.GradientField(label, value);
            if (newValue.Equals(value)) {
                return false;
            }

            value = newValue;
            return true;
        }

        protected override void DrawTableValue(ref Gradient value, GUIStyle style, GUILayoutOption[] layoutOptions) {
            EditorGUILayout.GradientField(value, layoutOptions);
        }
    }

    sealed class LayerMaskDrawer : IStaticEcsValueDrawer<LayerMask> {
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

        protected override bool DrawValue(string label, ref LayerMask value) {
            var newValue = EditorGUILayout.MaskField(label, value, GetLayerNames());
            if (newValue == value) {
                return false;
            }

            value = newValue;
            return true;
        }

        protected override void DrawTableValue(ref LayerMask value, GUIStyle style, GUILayoutOption[] layoutOptions) {
            EditorGUILayout.MaskField(value, GetLayerNames(), layoutOptions);
        }
    }

    sealed class QuaternionDrawer : IStaticEcsValueDrawer<Quaternion> {
        protected override bool DrawValue(string label, ref Quaternion value) {
            var eulerAngles = value.eulerAngles;
            var newValue = EditorGUILayout.Vector3Field(label, eulerAngles);
            if (newValue == eulerAngles) {
                return false;
            }

            value = Quaternion.Euler(newValue);
            return true;
        }

        protected override void DrawTableValue(ref Quaternion value, GUIStyle style, GUILayoutOption[] layoutOptions) {
            EditorGUILayout.Vector3Field("", value.eulerAngles, layoutOptions);
        }
    }

    sealed class RectDrawer : IStaticEcsValueDrawer<Rect> {
        protected override bool DrawValue(string label, ref Rect value) {
            var newValue = EditorGUILayout.RectField(label, value);
            if (newValue == value) {
                return false;
            }

            value = newValue;
            return true;
        }

        protected override void DrawTableValue(ref Rect value, GUIStyle style, GUILayoutOption[] layoutOptions) {
            EditorGUILayout.RectField(value, layoutOptions);
        }
    }

    sealed class Vector2Drawer : IStaticEcsValueDrawer<Vector2> {
        protected override bool DrawValue(string label, ref Vector2 value) {
            var newValue = EditorGUILayout.Vector2Field(label, value);
            if (newValue == value) {
                return false;
            }

            value = newValue;
            return true;
        }

        protected override void DrawTableValue(ref Vector2 value, GUIStyle style, GUILayoutOption[] layoutOptions) {
            EditorGUILayout.SelectableLabel(value.ToString(), layoutOptions);
        }
    }

    sealed class Vector2IntDrawer : IStaticEcsValueDrawer<Vector2Int> {
        protected override bool DrawValue(string label, ref Vector2Int value) {
            var newValue = EditorGUILayout.Vector2IntField(label, value);
            if (newValue == value) {
                return false;
            }

            value = newValue;
            return true;
        }

        protected override void DrawTableValue(ref Vector2Int value, GUIStyle style, GUILayoutOption[] layoutOptions) {
            EditorGUILayout.SelectableLabel(value.ToString(), layoutOptions);
        }
    }

    sealed class Vector3Drawer : IStaticEcsValueDrawer<Vector3> {
        protected override bool DrawValue(string label, ref Vector3 value) {
            EditorGUILayout.BeginHorizontal();
            var newValue = EditorGUILayout.Vector3Field(label, value);
            if (GUILayout.Button(Ui.IconView, GUILayout.ExpandWidth(false))) {
                MoveSceneCameraTo(value, Vector3.zero, 10f);
            }
            EditorGUILayout.EndHorizontal();

            if (newValue == value) {
                return false;
            }

            value = newValue;
            return true;
        }

        protected override void DrawTableValue(ref Vector3 value, GUIStyle style, GUILayoutOption[] layoutOptions) {
            EditorGUILayout.SelectableLabel(value.ToString(), layoutOptions);
        }

        public static void MoveSceneCameraTo(Vector3 position, Vector3 targetRotation, float cameraDistance) {
            var sceneView = SceneView.lastActiveSceneView;
            if (sceneView != null) {
                var direction = Quaternion.Euler(targetRotation) * Vector3.back;
                sceneView.pivot = position;
                // sceneView.rotation = Quaternion.Euler(targetRotation);
                sceneView.size = cameraDistance;
                sceneView.Repaint();
            } else {
                Debug.LogWarning("No active Scene View found!");
            }
        }
    }

    sealed class Vector3IntDrawer : IStaticEcsValueDrawer<Vector3Int> {
        protected override bool DrawValue(string label, ref Vector3Int value) {
            var newValue = EditorGUILayout.Vector3IntField(label, value);
            if (newValue == value) {
                return false;
            }

            value = newValue;
            return true;
        }

        protected override void DrawTableValue(ref Vector3Int value, GUIStyle style, GUILayoutOption[] layoutOptions) {
            EditorGUILayout.SelectableLabel(value.ToString(), layoutOptions);
        }
    }

    sealed class Vector4Drawer : IStaticEcsValueDrawer<Vector4> {
        protected override bool DrawValue(string label, ref Vector4 value) {
            var newValue = EditorGUILayout.Vector4Field(label, value);
            if (newValue == value) {
                return false;
            }

            value = newValue;
            return true;
        }

        protected override void DrawTableValue(ref Vector4 value, GUIStyle style, GUILayoutOption[] layoutOptions) {
            EditorGUILayout.SelectableLabel(value.ToString(), layoutOptions);
        }
    }
}