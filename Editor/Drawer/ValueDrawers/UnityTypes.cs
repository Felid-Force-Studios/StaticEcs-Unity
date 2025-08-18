using UnityEngine;
using static UnityEditor.EditorGUILayout;

namespace FFS.Libraries.StaticEcs.Unity.Editor.Inspectors {
    public sealed class AnimationCurveDrawer : IStaticEcsValueDrawer<AnimationCurve> {
        public override bool IsNullAllowed() => true;

        public override bool DrawValue(ref DrawContext ctx, string label, ref AnimationCurve value) {
            if (value == null) {
                var changed = false;
                BeginHorizontal();
                PrefixLabel(label);
                LabelField("NULL", Ui.MinWidth());
                GUILayout.FlexibleSpace();
                if (Ui.PlusButton) {
                    value = new AnimationCurve();
                    changed = true;
                }

                EndHorizontal();

                return changed;
            }
            
            BeginHorizontal();
            PrefixLabel(label);
            var newValue = CurveField(value);
            EndHorizontal();

            if (newValue.Equals(value)) {
                return false;
            }

            value = newValue;
            return true;
        }

        public override void DrawTableValue(ref AnimationCurve value, GUIStyle style, GUILayoutOption[] layoutOptions) {
            CurveField(value, layoutOptions);
        }
    }

    public sealed class BoundsDrawer : IStaticEcsValueDrawer<Bounds> {
        public override bool DrawValue(ref DrawContext ctx, string label, ref Bounds value) {
            BeginHorizontal();
            PrefixLabel(label);
            var newValue = BoundsField(value);
            EndHorizontal();
            if (newValue == value) {
                return false;
            }

            value = newValue;
            return true;
        }

        public override void DrawTableValue(ref Bounds value, GUIStyle style, GUILayoutOption[] layoutOptions) {
            BoundsField(value, layoutOptions);
        }
    }

    public sealed class BoundsIntDrawer : IStaticEcsValueDrawer<BoundsInt> {
        public override bool DrawValue(ref DrawContext ctx, string label, ref BoundsInt value) {
            BeginHorizontal();
            PrefixLabel(label);
            var newValue = BoundsIntField(value);
            EndHorizontal();
            if (newValue == value) {
                return false;
            }

            value = newValue;
            return true;
        }

        public override void DrawTableValue(ref BoundsInt value, GUIStyle style, GUILayoutOption[] layoutOptions) {
            BoundsIntField(value, layoutOptions);
        }
    }

    public sealed class ColorDrawer : IStaticEcsValueDrawer<Color> {
        public override bool DrawValue(ref DrawContext ctx, string label, ref Color value) {
            BeginHorizontal();
            PrefixLabel(label);
            var newValue = ColorField(value);
            EndHorizontal();
            if (newValue == value) {
                return false;
            }

            value = newValue;
            return true;
        }

        public override void DrawTableValue(ref Color value, GUIStyle style, GUILayoutOption[] layoutOptions) {
            ColorField(value, layoutOptions);
        }
    }

    public sealed class Color32Drawer : IStaticEcsValueDrawer<Color32> {
        public override bool DrawValue(ref DrawContext ctx, string label, ref Color32 value) {
            BeginHorizontal();
            PrefixLabel(label);
            var newValue = ColorField(value);
            EndHorizontal();
            if (newValue == value) {
                return false;
            }

            value = newValue;
            return true;
        }

        public override void DrawTableValue(ref Color32 value, GUIStyle style, GUILayoutOption[] layoutOptions) {
            ColorField(value, layoutOptions);
        }
    }

    public sealed class GradientDrawer : IStaticEcsValueDrawer<Gradient> {
        public override bool IsNullAllowed() => true;

        public override bool DrawValue(ref DrawContext ctx, string label, ref Gradient value) {
            if (value == null) {
                var changed = false;
                BeginHorizontal();
                PrefixLabel(label);
                LabelField("NULL", Ui.MinWidth());
                GUILayout.FlexibleSpace();
                if (Ui.PlusButton) {
                    value = new Gradient();
                    changed = true;
                }

                EndHorizontal();

                return changed;
            }
            
            BeginHorizontal();
            PrefixLabel(label);
            var newValue = GradientField(GUIContent.none, value);
            EndHorizontal();

            if (newValue.Equals(value)) {
                return false;
            }

            value = newValue;
            return true;
        }

        public override void DrawTableValue(ref Gradient value, GUIStyle style, GUILayoutOption[] layoutOptions) {
            GradientField(value, layoutOptions);
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

        public override bool DrawValue(ref DrawContext ctx, string label, ref LayerMask value) {
            BeginHorizontal();
            PrefixLabel(label);
            var newValue = MaskField(value, GetLayerNames());
            EndHorizontal();
            if (newValue == value) {
                return false;
            }

            value = newValue;
            return true;
        }

        public override void DrawTableValue(ref LayerMask value, GUIStyle style, GUILayoutOption[] layoutOptions) {
            MaskField(value, GetLayerNames(), layoutOptions);
        }
    }

    public sealed class RectDrawer : IStaticEcsValueDrawer<Rect> {
        public override bool DrawValue(ref DrawContext ctx, string label, ref Rect value) {
            BeginHorizontal();
            PrefixLabel(label);
            var newValue = RectField(value);
            EndHorizontal();
            if (newValue == value) {
                return false;
            }

            value = newValue;
            return true;
        }

        public override void DrawTableValue(ref Rect value, GUIStyle style, GUILayoutOption[] layoutOptions) {
            RectField(value, layoutOptions);
        }
    }

    public sealed class QuaternionDrawer : IStaticEcsValueDrawer<Quaternion> {
        public override bool DrawValue(ref DrawContext ctx, string label, ref Quaternion value) {
            var eulerAngles = value.eulerAngles;
            BeginHorizontal();
            PrefixLabel(label);
            var newValue = Vector3Field(GUIContent.none, eulerAngles);
            EndHorizontal();
            if (newValue == eulerAngles) {
                return false;
            }

            value = Quaternion.Euler(newValue);
            return true;
        }

        public override void DrawTableValue(ref Quaternion value, GUIStyle style, GUILayoutOption[] layoutOptions) {
            SelectableLabel(value.eulerAngles.ToString(), style, layoutOptions);
        }
    }

    public sealed class Vector2Drawer : IStaticEcsValueDrawer<Vector2> {
        public override bool DrawValue(ref DrawContext ctx, string label, ref Vector2 value) {
            BeginHorizontal();
            PrefixLabel(label);
            var newValue = Vector2Field(GUIContent.none, value);
            EndHorizontal();
            if (newValue == value) {
                return false;
            }

            value = newValue;
            return true;
        }

        public override void DrawTableValue(ref Vector2 value, GUIStyle style, GUILayoutOption[] layoutOptions) {
            SelectableLabel(value.ToString(), style, layoutOptions);
        }
    }

    public sealed class Vector2IntDrawer : IStaticEcsValueDrawer<Vector2Int> {
        public override bool DrawValue(ref DrawContext ctx, string label, ref Vector2Int value) {
            BeginHorizontal();
            PrefixLabel(label);
            var newValue = Vector2IntField(GUIContent.none, value);
            EndHorizontal();
            if (newValue == value) {
                return false;
            }

            value = newValue;
            return true;
        }

        public override void DrawTableValue(ref Vector2Int value, GUIStyle style, GUILayoutOption[] layoutOptions) {
            SelectableLabel(value.ToString(), style, layoutOptions);
        }
    }

    public sealed class Vector3Drawer : IStaticEcsValueDrawer<Vector3> {
        public override bool DrawValue(ref DrawContext ctx, string label, ref Vector3 value) {
            BeginHorizontal();
            PrefixLabel(label);
            var newValue = Vector3Field(GUIContent.none, value);
            EndHorizontal();
            if (newValue == value) {
                return false;
            }

            value = newValue;
            return true;
        }

        public override void DrawTableValue(ref Vector3 value, GUIStyle style, GUILayoutOption[] layoutOptions) {
            SelectableLabel(value.ToString(), style, layoutOptions);
        }
    }

    public sealed class Vector3IntDrawer : IStaticEcsValueDrawer<Vector3Int> {
        public override bool DrawValue(ref DrawContext ctx, string label, ref Vector3Int value) {
            BeginHorizontal();
            PrefixLabel(label);
            var newValue = Vector3IntField(GUIContent.none, value);
            EndHorizontal();
            if (newValue == value) {
                return false;
            }

            value = newValue;
            return true;
        }

        public override void DrawTableValue(ref Vector3Int value, GUIStyle style, GUILayoutOption[] layoutOptions) {
            SelectableLabel(value.ToString(), style, layoutOptions);
        }
    }

    public sealed class Vector4Drawer : IStaticEcsValueDrawer<Vector4> {
        public override bool DrawValue(ref DrawContext ctx, string label, ref Vector4 value) {
            BeginHorizontal();
            PrefixLabel(label);
            var newValue = Vector4Field(GUIContent.none, value);
            EndHorizontal();
            if (newValue == value) {
                return false;
            }

            value = newValue;
            return true;
        }

        public override void DrawTableValue(ref Vector4 value, GUIStyle style, GUILayoutOption[] layoutOptions) {
            SelectableLabel(value.ToString(), style, layoutOptions);
        }
    }
}