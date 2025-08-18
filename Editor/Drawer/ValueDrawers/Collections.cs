using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static UnityEditor.EditorGUILayout;

namespace FFS.Libraries.StaticEcs.Unity.Editor.Inspectors {
    
    internal sealed class ListDrawer<T> : IStaticEcsValueDrawer<List<T>> where T : struct {
        public override bool IsNullAllowed() => true;

        public override bool DrawValue(ref DrawContext ctx, string label, ref List<T> value) {
            var changed = false;
            
            if (value == null) {
                BeginHorizontal();
                PrefixLabel(label);
                LabelField("NULL", Ui.MinWidth());
                GUILayout.FlexibleSpace();
                if (Ui.PlusButton) {
                    value = new();
                    changed = true;
                }
                EndHorizontal();

                return changed;
            }

            var type = typeof(T);
            
            BeginHorizontal();
            Drawer.DrawFoldoutBox(typeof(T[]).FullName + label + ctx.Level, label, label, out var show);
            GUILayout.FlexibleSpace();
            if (Ui.PlusButton) {
                value.Add(type.CreateDefault<T>());
                changed = true;
            }
            EndHorizontal();
            
            if (changed || !show) {
                return changed;
            }

            EditorGUI.indentLevel++;
            ctx.Level--;
            var simple = type.EditorTypeIsCompactView();
            
            for (var i = 0; i < value.Count; i++) {
                var val = value[i];
                BeginHorizontal();
                BeginVertical(GUI.skin.box);
                if (simple) {
                    if (Drawer.TryDrawObject(ref ctx, $"[{i}]", type, val, out var newValue)) {
                        value[i] = (T) newValue;
                        changed = true;
                    }
                } else {
                    LabelField($"[{i}] {type.EditorTypeName()}:");
                    if (Drawer.TryDrawObject(ref ctx, type.EditorTypeName(), type, val, out var newValue)) {
                        value[i] = (T) newValue;
                        changed = true;
                    }
                }
                EndVertical();
                if (Ui.MinusButton) {
                    if (value.Count == 1) {
                        value = null;
                    } else {
                        value.RemoveAt(i);
                    }
                    changed = true;
                }
                EndHorizontal();

                if (changed) {
                    break;
                }

                if (!simple) {
                    LabelField("", GUI.skin.horizontalSlider);
                }
            }
            
            ctx.Level++;
            EditorGUI.indentLevel--;
            return changed;
        }

        public override void DrawTableValue(ref List<T> value, GUIStyle style, GUILayoutOption[] layoutOptions) {
            SelectableLabel($"Count: {value.Count}, Cap: {value.Capacity}", style, layoutOptions);
        }
    }
    
    internal sealed class SetDrawer<T> : IStaticEcsValueDrawer<HashSet<T>> {
        public override bool IsNullAllowed() => true;

        public override bool DrawValue(ref DrawContext ctx, string label, ref HashSet<T> value) {
            var changed = false;
            
            if (value == null) {
                BeginHorizontal();
                PrefixLabel(label);
                LabelField("NULL", Ui.MinWidth());
                GUILayout.FlexibleSpace();
                if (Ui.PlusButton) {
                    value = new(4);
                    changed = true;
                }
                EndHorizontal();

                return changed;
            }

            var type = typeof(T);
            
            BeginHorizontal();
            Drawer.DrawFoldoutBox(typeof(T[]).FullName + label + ctx.Level, label, label, out var show);
            GUILayout.FlexibleSpace();
            if (Ui.PlusButton) {
                value.Add(type.CreateDefault<T>());
                changed = true;
            }
            EndHorizontal();
            
            if (changed || !show) {
                return changed;
            }

            EditorGUI.indentLevel++;
            ctx.Level--;
            var enumerator = value.GetEnumerator();
            var i = -1;
            var simple = type.EditorTypeIsCompactView();
            while (enumerator.MoveNext()) {
                i++;
                var val = enumerator.Current;
                BeginHorizontal();
                BeginVertical(GUI.skin.box);
                if (simple) {
                    if (Drawer.TryDrawObject(ref ctx, $"[{i}]", type, val, out var newValue)) {
                        enumerator.Dispose();
                        value.Remove(val);
                        value.Add((T) newValue);
                        changed = true;
                    }
                } else {
                    LabelField($"[{i}] {type.EditorTypeName()}:");
                    if (Drawer.TryDrawObject(ref ctx, type.EditorTypeName(), type, val, out var newValue)) {
                        enumerator.Dispose();
                        value.Remove(val);
                        value.Add((T) newValue);
                        changed = true;
                    }
                }
                EndVertical();
                if (Ui.MinusButton) {
                    enumerator.Dispose();
                    if (value.Count == 1) {
                        value = null;
                    } else {
                        value.Remove(val);
                    }
                    changed = true;
                }
                EndHorizontal();

                if (changed) {
                    break;
                }

                if (!simple) {
                    LabelField("", GUI.skin.horizontalSlider);
                }
            }
            if (!changed) {
                enumerator.Dispose();
            }
            ctx.Level++;
            EditorGUI.indentLevel--;
            return changed;
        }

        public override void DrawTableValue(ref HashSet<T> value, GUIStyle style, GUILayoutOption[] layoutOptions) {
            SelectableLabel($"Count: {value.Count}", style, layoutOptions);
        }
    }

    internal sealed class ArrayDrawer<T> : IStaticEcsValueDrawer<T[]> {
        public override bool IsNullAllowed() => true;

        public override bool DrawValue(ref DrawContext ctx, string label, ref T[] value) {
            var changed = false;
            
            if (value == null) {
                BeginHorizontal();
                PrefixLabel(label);
                LabelField("NULL", Ui.MinWidth());
                GUILayout.FlexibleSpace();
                if (Ui.PlusButton) {
                    value = new T[4];
                    changed = true;
                }
                EndHorizontal();

                return changed;
            }

            var type = typeof(T);
            
            BeginHorizontal();
            Drawer.DrawFoldoutBox(typeof(T[]).FullName + label + ctx.Level, label, label, out var show);
            GUILayout.FlexibleSpace();
            if (Ui.PlusButton) {
                var arr = new T[value.Length + 1];
                Array.Copy(value, arr, value.Length);
                arr[value.Length] = type.CreateDefault<T>();
                value = arr;
                changed = true;
            }
            EndHorizontal();
            
            if (changed || !show) {
                return changed;
            }
            
            EditorGUI.indentLevel++;
            ctx.Level--;
            var simple = type.EditorTypeIsCompactView();
            
            for (var i = 0; i < value.Length; i++) {
                var val = value[i];
                BeginHorizontal();
                BeginVertical(GUI.skin.box);
                if (simple) {
                    if (Drawer.TryDrawObject(ref ctx, $"[{i}]", type, val, out var newValue)) {
                        value[i] = (T) newValue;
                        changed = true;
                    }
                } else {
                    LabelField($"[{i}] {type.EditorTypeName()}:");
                    if (Drawer.TryDrawObject(ref ctx, type.EditorTypeName(), type, val, out var newValue)) {
                        value[i] = (T) newValue;
                        changed = true;
                    }
                }
                EndVertical();
                if (Ui.MinusButton) {
                    if (value.Length == 1) {
                        value = null;
                    } else {
                        var copy = new T[value.Length - 1];
                        var count = 0;
                        for (var i1 = 0; i1 < value.Length; i1++) {
                            if (i1 == i) continue;
                            copy[count++] = value[i1];
                        }

                        value = copy;
                    }
                    changed = true;
                }
                EndHorizontal();

                if (changed) {
                    break;
                }

                if (!simple) {
                    LabelField("", GUI.skin.horizontalSlider);
                }
            }
            
            ctx.Level++;
            EditorGUI.indentLevel--;
            return changed;
        }

        public override void DrawTableValue(ref T[] value, GUIStyle style, GUILayoutOption[] layoutOptions) {
            SelectableLabel($"Length: {value.Length}", style, layoutOptions);
        }
    }

    internal sealed class Array2DDrawer<T> : IStaticEcsValueDrawer<T[,]> {
        public override bool IsNullAllowed() => true;

        public override bool DrawValue(ref DrawContext ctx, string label, ref T[,] value) {
            var changed = false;
            
            if (value == null) {
                BeginHorizontal();
                PrefixLabel(label);
                LabelField("NULL", Ui.MinWidth());
                GUILayout.FlexibleSpace();
                if (Ui.PlusButton) {
                    value = new T[4, 4];
                    changed = true;
                }
                EndHorizontal();

                return changed;
            }

            BeginHorizontal();
            Drawer.DrawFoldoutBox(typeof(T[,]).FullName + label + ctx.Level, label, label, out var show);
            GUILayout.FlexibleSpace();
            using (Ui.DisabledScope) {
                if (Ui.FakeButton) { }
            }
            EndHorizontal();
            
            if (!show) {
                return false;
            }

            var type = typeof(T);
            var simple = type.EditorTypeIsCompactView();
            
            var dim0 = value.GetLength(0);
            var dim1 = value.GetLength(1);

            EditorGUI.indentLevel++;
            ctx.Level--;
            for (var i = 0; i < dim0; i++) {
                LabelField($"[{i}, _ ]");
                EditorGUI.indentLevel++;
                for (var j = 0; j < dim1; j++) {
                    var val = value[i, j];
                    BeginVertical(GUI.skin.box);
                    if (simple) {
                        if (Drawer.TryDrawObject(ref ctx, $"[{j}]", type, val, out var newValue)) {
                            value[i, j] = (T) newValue;
                            changed = true;
                        }
                    } else {
                        LabelField($"[{j}] {type.EditorTypeName()}:");
                        if (Drawer.TryDrawObject(ref ctx, type.EditorTypeName(), type, val, out var newValue)) {
                            value[i, j] = (T) newValue;
                            changed = true;
                        }
                    }
                    EndVertical();
                    
                    if (changed) {
                        break;
                    }
                }
                EditorGUI.indentLevel--;
                
                if (changed) {
                    break;
                }
            }
            ctx.Level++;
            EditorGUI.indentLevel--;
            return changed;
        }

        public override void DrawTableValue(ref T[,] value, GUIStyle style, GUILayoutOption[] layoutOptions) {
            SelectableLabel($"Size: {value.GetLength(0)} x {value.GetLength(1)}", style, layoutOptions);
        }
    }

    internal sealed class Array3DDrawer<T> : IStaticEcsValueDrawer<T[,,]> {
        public override bool IsNullAllowed() => true;

        public override bool DrawValue(ref DrawContext ctx, string label, ref T[,,] value) {
            var changed = false;
            
            if (value == null) {
                BeginHorizontal();
                PrefixLabel(label);
                LabelField("NULL", Ui.MinWidth());
                GUILayout.FlexibleSpace();
                if (Ui.PlusButton) {
                    value = new T[4, 4, 4];
                    changed = true;
                }
                EndHorizontal();

                return changed;
            }

            BeginHorizontal();
            Drawer.DrawFoldoutBox(typeof(T[,,]).FullName + label + ctx.Level, label, label, out var show);
            GUILayout.FlexibleSpace();
            using (Ui.DisabledScope) {
                if (Ui.FakeButton) { }
            }
            EndHorizontal();
            
            if (!show) {
                return false;
            }

            var type = typeof(T);
            var simple = type.EditorTypeIsCompactView();
            
            var dim0 = value.GetLength(0);
            var dim1 = value.GetLength(1);
            var dim2 = value.GetLength(2);

            EditorGUI.indentLevel++;
            ctx.Level--;
            for (var i = 0; i < dim0; i++) {
                LabelField($"[{i}, _, _ ]");
                EditorGUI.indentLevel++;
                for (var j = 0; j < dim1; j++) {
                    LabelField($"[{i}, {j}, _ ]");
                    EditorGUI.indentLevel++;
                    for (var k = 0; k < dim2; k++) {
                        var val = value[i, j, k];
                        BeginVertical(GUI.skin.box);
                        if (simple) {
                            if (Drawer.TryDrawObject(ref ctx, $"[{k}]", type, val, out var newValue)) {
                                value[i, j, k] = (T) newValue;
                                changed = true;
                            }
                        } else {
                            LabelField($"[{k}] {type.EditorTypeName()}:");
                            if (Drawer.TryDrawObject(ref ctx, type.EditorTypeName(), type, val, out var newValue)) {
                                value[i, j, k] = (T) newValue;
                                changed = true;
                            }
                        }
                        EndVertical();
                    
                        if (changed) {
                            break;
                        }
                    }
                    EditorGUI.indentLevel--;
                    
                    if (changed) {
                        break;
                    }
                }
                EditorGUI.indentLevel--;
                
                if (changed) {
                    break;
                }
            }
            ctx.Level++;
            EditorGUI.indentLevel--;
            return changed;
        }

        public override void DrawTableValue(ref T[,,] value, GUIStyle style, GUILayoutOption[] layoutOptions) {
            SelectableLabel($"Size: {value.GetLength(0)} x {value.GetLength(1)} x {value.GetLength(2)}", style, layoutOptions);
        }
    }
}