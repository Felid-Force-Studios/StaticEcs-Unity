using System;
using System.Globalization;
using UnityEngine;
using static UnityEditor.EditorGUILayout;

namespace FFS.Libraries.StaticEcs.Unity.Editor.Inspectors {
    public sealed class BoolDrawer : IStaticEcsValueDrawer<bool> {
        public override bool DrawValue(ref DrawContext ctx, string label, ref bool value) {
            BeginHorizontal();
            PrefixLabel(label);
            var newValue = Toggle(value);
            EndHorizontal();
            if (newValue == value) {
                return false;
            }

            value = newValue;
            return true;
        }

        public override void DrawTableValue(ref bool value, GUIStyle style, GUILayoutOption[] layoutOptions) {
            Toggle(value, style, layoutOptions);
        }
    }

    public sealed class DoubleDrawer : IStaticEcsValueDrawer<double> {
        public override bool DrawValue(ref DrawContext ctx, string label, ref double value) {
            BeginHorizontal();
            PrefixLabel(label);
            double newValue = DoubleField(GUIContent.none, value);
            EndHorizontal();
            if (System.Math.Abs(newValue - value) < double.Epsilon || (double.IsNaN(newValue) && double.IsNaN(value))) {
                return false;
            }

            value = newValue;
            return true;
        }

        public override void DrawTableValue(ref double value, GUIStyle style, GUILayoutOption[] layoutOptions) {
            SelectableLabel(value.ToString("0.##", CultureInfo.InvariantCulture), style, layoutOptions);
        }
    }

    public sealed class FloatDrawer : IStaticEcsValueDrawer<float> {
        public override bool DrawValue(ref DrawContext ctx, string label, ref float value) {
            BeginHorizontal();
            PrefixLabel(label);
            float newValue = FloatField(GUIContent.none, value);
            EndHorizontal();
            if (System.Math.Abs(newValue - value) < float.Epsilon || (float.IsNaN(newValue) && float.IsNaN(value))) {
                return false;
            }

            value = newValue;
            return true;
        }

        public override void DrawTableValue(ref float value, GUIStyle style, GUILayoutOption[] layoutOptions) {
            SelectableLabel(value.ToString("0.##", CultureInfo.InvariantCulture), style, layoutOptions);
        }
    }

    public sealed class ULongDrawer : IStaticEcsValueDrawer<ulong> {
        public override bool DrawValue(ref DrawContext ctx, string label, ref ulong value) {
            var newValue = (long) value;
            BeginHorizontal();
            PrefixLabel(label);
            newValue = LongField(GUIContent.none, newValue);
            EndHorizontal();
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
            SelectableLabel(value.ToString(CultureInfo.InvariantCulture), style, layoutOptions);
        }
    }

    public sealed class LongDrawer : IStaticEcsValueDrawer<long> {
        public override bool DrawValue(ref DrawContext ctx, string label, ref long value) {
            var newValue = value;
            BeginHorizontal();
            PrefixLabel(label);
            newValue = LongField(GUIContent.none, newValue);
            EndHorizontal();
            if (newValue == value) {
                return false;
            }

            value = newValue;
            return true;
        }

        public override void DrawTableValue(ref long value, GUIStyle style, GUILayoutOption[] layoutOptions) {
            SelectableLabel(value.ToString(CultureInfo.InvariantCulture), style, layoutOptions);
        }
    }

    public sealed class UIntDrawer : IStaticEcsValueDrawer<uint> {
        public override bool DrawValue(ref DrawContext ctx, string label, ref uint value) {
            BeginHorizontal();
            PrefixLabel(label);
            var newValue = LongField(GUIContent.none, value);
            EndHorizontal();
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
            SelectableLabel(value.ToString(CultureInfo.InvariantCulture), style, layoutOptions);
        }
    }

    public sealed class IntDrawer : IStaticEcsValueDrawer<int> {
        public override bool DrawValue(ref DrawContext ctx, string label, ref int value) {
            BeginHorizontal();
            PrefixLabel(label);
            var newValue = IntField(GUIContent.none, value);
            EndHorizontal();
            if (newValue == value) {
                return false;
            }

            value = newValue;
            return true;
        }

        public override void DrawTableValue(ref int value, GUIStyle style, GUILayoutOption[] layoutOptions) {
            SelectableLabel(value.ToString(CultureInfo.InvariantCulture), style, layoutOptions);
        }
    }

    public sealed class UShortDrawer : IStaticEcsValueDrawer<ushort> {
        public override bool DrawValue(ref DrawContext ctx, string label, ref ushort value) {
            BeginHorizontal();
            PrefixLabel(label);
            var newValue = IntField(GUIContent.none, value);
            EndHorizontal();
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
            SelectableLabel(value.ToString(CultureInfo.InvariantCulture), style, layoutOptions);
        }
    }

    public sealed class ShortDrawer : IStaticEcsValueDrawer<short> {
        public override bool DrawValue(ref DrawContext ctx, string label, ref short value) {
            BeginHorizontal();
            PrefixLabel(label);
            var newValue = IntField(GUIContent.none, value);
            EndHorizontal();
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
            SelectableLabel(value.ToString(CultureInfo.InvariantCulture), style, layoutOptions);
        }
    }

    public sealed class ByteDrawer : IStaticEcsValueDrawer<byte> {
        public override bool DrawValue(ref DrawContext ctx, string label, ref byte value) {
            BeginHorizontal();
            PrefixLabel(label);
            var newValue = IntField(GUIContent.none, value);
            EndHorizontal();
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
            SelectableLabel(value.ToString(CultureInfo.InvariantCulture), style, layoutOptions);
        }
    }

    public sealed class SByteDrawer : IStaticEcsValueDrawer<sbyte> {
        public override bool DrawValue(ref DrawContext ctx, string label, ref sbyte value) {
            BeginHorizontal();
            PrefixLabel(label);
            var newValue = IntField(GUIContent.none, value);
            EndHorizontal();
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
            SelectableLabel(value.ToString(CultureInfo.InvariantCulture), style, layoutOptions);
        }
    }

    public sealed class StringDrawer : IStaticEcsValueDrawer<string> {
        public override bool IsNullAllowed() => true;

        public override bool DrawValue(ref DrawContext ctx, string label, ref string value) {
            BeginHorizontal();
            PrefixLabel(label);
            var newValue = TextField(value);
            EndHorizontal();
            if (newValue == value) {
                return false;
            }

            value = newValue;
            return true;
        }

        public override void DrawTableValue(ref string value, GUIStyle style, GUILayoutOption[] layoutOptions) {
            SelectableLabel(value, style, layoutOptions);
        }
    }

    internal sealed class NullableDrawer<T> : IStaticEcsValueDrawer<Nullable<T>> where T : struct {
        public override bool IsNullAllowed() => true;

        public override bool DrawValue(ref DrawContext ctx, string label, ref Nullable<T> value) {
            var changed = false;
            var type = typeof(T);
            
            if (!value.HasValue) {
                BeginHorizontal();
                PrefixLabel(label);
                LabelField("NULL", Ui.MinWidth());
                GUILayout.FlexibleSpace();
                if (Ui.PlusButton) {
                    value = type.CreateDefault<T>();
                    changed = true;
                }
                EndHorizontal();

                return changed;
            }
            
            if (Drawer.TryDrawObject(ref ctx, label, type, value.Value, out var newValue)) {
                value = (T) newValue;
                return true;
            }
            
            return false;
        }

        public override void DrawTableValue(ref Nullable<T> value, GUIStyle style, GUILayoutOption[] layoutOptions) {
            SelectableLabel(value.HasValue ? value.Value.ToString() : "null", style, layoutOptions);
        }
    }
}