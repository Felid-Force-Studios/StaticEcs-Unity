using UnityEditor;
using UnityEngine;
using static UnityEditor.EditorGUILayout;

namespace FFS.Libraries.StaticEcs.Unity.Editor.Inspectors {
    internal sealed class EntityGIDDrawer : IStaticEcsValueDrawer<EntityGID> {
        public override bool DrawValue(ref DrawContext ctx, string label, ref EntityGID value) {
            using (new HorizontalScope()) {
                PrefixLabel(label);
                var empty = value.IsEmpty();
                if (!empty && ctx.World != null && ctx.World.TryGetEntity(value, out var entity)) {
                    LabelField(empty ? "Empty" : value.ToString(), Ui.MinWidth());
                } else {
                    LabelField(empty ? "Empty" : value.ToString() + " (Not actual)", Ui.MinWidth());
                    empty = true;
                }
                
                using (Ui.EnabledScopeVal(!empty)) {
                    if (Application.isPlaying && !value.IsEmpty() && Ui.ViewButton) {
                        if (!EntityInspectorWindow.ShowWindowForEntity(ctx.World, value)) {
                            Debug.LogWarning($"Entity with EntityGID {value} is not available");
                        }
                    }
                }
            }

            return false;
        }

        public override void DrawTableValue(ref EntityGID value, GUIStyle style, GUILayoutOption[] layoutOptions) {
            SelectableLabel(value.ToString(), style, layoutOptions);
        }
    }

    internal sealed class EntityStatusDrawer : IStaticEcsValueDrawer<EntityStatus> {
        public override bool DrawValue(ref DrawContext ctx, string label, ref EntityStatus value) {
            if (ctx.Level != Drawer.MaxRecursionLvl) {
                LabelField(label);
            }
            EditorGUI.indentLevel++;
            Drawer.DrawEnum("Value", value.Value, false, out var newValue);
            EditorGUI.indentLevel--;
                
            if ((EntityStatusType)newValue == value.Value) {
                return false;
            }

            value.Value = (EntityStatusType)newValue;
            return true;
        }

        public override void DrawTableValue(ref EntityStatus value, GUIStyle style, GUILayoutOption[] layoutOptions) {
            SelectableLabel(value.Value.ToString(), style, layoutOptions);
        }
    }

    internal sealed class MultiComponentDrawer<T> : IStaticEcsValueDrawer<Multi<T>> where T : struct {
        public override bool DrawValue(ref DrawContext ctx, string label, ref Multi<T> value) {
            
            if (value.data == null) {
                BeginHorizontal();
                PrefixLabel(label);
                LabelField("NULL", Ui.MinWidth());
                GUILayout.FlexibleSpace();
                using (Ui.DisabledScope) {
                    if (Ui.MenuButton) {}
                }
                EndHorizontal();
                return false;
            }

            var changed = false;
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
                if (Ui.TrashButton) {
                    value.RemoveAt(i);
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

        public override void DrawTableValue(ref Multi<T> value, GUIStyle style, GUILayoutOption[] layoutOptions) {
            SelectableLabel($"Count: {value.Count}, Cap: {value.Capacity}", style, layoutOptions);
        }
    }

    internal sealed class RoMultiComponentDrawer<T> : IStaticEcsValueDrawer<ROMulti<T>> where T : struct {
        public override bool DrawValue(ref DrawContext ctx, string label, ref ROMulti<T> value) {
            if (value.multi.data == null) {
                BeginHorizontal();
                PrefixLabel(label);
                LabelField("NULL", Ui.MinWidth());
                GUILayout.FlexibleSpace();
                using (Ui.DisabledScope) {
                    if (Ui.MenuButton) { }
                }
                EndHorizontal();
                return false;
            }

            var type = typeof(T);
            
            BeginHorizontal();
            Drawer.DrawFoldoutBox(typeof(ROMulti<T>).FullName + label + ctx.Level, label, label, out var show);
            GUILayout.FlexibleSpace();
            using (Ui.DisabledScope) {
                if (Ui.MenuButton) { }
            }
            EndHorizontal();
            
            if (!show) {
                return false;
            }

            EditorGUI.indentLevel++;
            ctx.Level--;
            var typeName = type.EditorTypeName();
            var simple = type.EditorTypeIsCompactView();
            
            using (Ui.DisabledScope) {
                for (ushort i = 0; i < value.Count; i++) {
                    BeginVertical(GUI.skin.box);
                    if (simple) {
                        Drawer.TryDrawObject(ref ctx, $"[{i}]", type, value[i], out var _);
                    } else {
                        LabelField($"[{i}] {type.EditorTypeName()}:");
                        Drawer.TryDrawObject(ref ctx, typeName, type, value[i], out var _);
                    }
                    EndVertical();
                }
            }

            ctx.Level++;
            EditorGUI.indentLevel--;
            return false;
        }

        public override void DrawTableValue(ref ROMulti<T> value, GUIStyle style, GUILayoutOption[] layoutOptions) {
            SelectableLabel($"Count: {value.Count}, Cap: {value.Capacity}", style, layoutOptions);
        }
    }
}