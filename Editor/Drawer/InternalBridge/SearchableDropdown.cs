using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace FFS.Libraries.StaticEcs.Unity.Editor {

    public static class SearchableDropdown {
        private const int GroupByNamespaceThreshold = 20;

        public readonly struct Item {
            public readonly string Label;
            public readonly object Payload;
            public readonly bool Enabled;

            public Item(string label, object payload, bool enabled = true) {
                Label = label;
                Payload = payload;
                Enabled = enabled;
            }
        }

        public static void Show(string title, IEnumerable<Item> items, Action<object> onSelect) {
            var mouse = Event.current != null ? Event.current.mousePosition : Vector2.zero;
            var rect = new Rect(mouse.x, mouse.y, 1f, 1f);
            new TypedDropdown(title, items, onSelect).Show(rect);
        }

        internal static int Threshold => GroupByNamespaceThreshold;
    }

    internal sealed class TypedDropdown : AdvancedDropdown {
        private readonly string _title;
        private readonly IEnumerable<SearchableDropdown.Item> _items;
        private readonly Action<object> _onSelect;

        public TypedDropdown(string title, IEnumerable<SearchableDropdown.Item> items, Action<object> onSelect) : base(new AdvancedDropdownState()) {
            _title = title;
            _items = items;
            _onSelect = onSelect;
            minimumSize = new Vector2(320f, 400f);
            m_DataSource = new CallbackDataSource(BuildRoot);
            m_Gui = new TypedDropdownGUI(m_DataSource);
        }

        public new void Show(Rect rect) {
            base.Show(rect);
            if (m_WindowInstance != null) {
                m_WindowInstance.wantsMouseMove = true;
            }
        }

        protected override AdvancedDropdownItem BuildRoot() {
            var list = _items as IList<SearchableDropdown.Item> ?? _items.ToList();
            var groupByNs = list.Count > SearchableDropdown.Threshold;

            var root = new GroupNode(_title);
            foreach (var item in list) {
                var (displayName, nsPath) = SplitLabel(item.Label);
                var parts = groupByNs && nsPath.Length > 0 ? nsPath.Split('.', '/') : Array.Empty<string>();
                IEnumerator<string> enumerator = ((IEnumerable<string>) parts).GetEnumerator();
                root.Add(item, displayName, nsPath, enumerator);
            }

            root.Collapse();
            var rootItem = new AdvancedDropdownItem(_title);
            root.Materialize(rootItem);
            return rootItem;
        }

        protected override void ItemSelected(AdvancedDropdownItem item) {
            if (item is TypedDropdownItem t && t.Enabled) {
                _onSelect?.Invoke(t.Payload);
            }
        }

        private static (string name, string ns) SplitLabel(string label) {
            var depth = 0;
            var lastSep = -1;
            for (var i = 0; i < label.Length; i++) {
                var c = label[i];
                if (c == '<') depth++;
                else if (c == '>') depth--;
                else if (depth == 0 && (c == '.' || c == '/')) lastSep = i;
            }
            return lastSep < 0
                ? (label, string.Empty)
                : (label.Substring(lastSep + 1), label.Substring(0, lastSep));
        }

        private sealed class GroupNode {
            public string Name;
            public readonly List<LeafInfo> Leaves = new();
            public readonly Dictionary<string, GroupNode> Children = new();

            public GroupNode(string name) { Name = name; }

            public void Add(SearchableDropdown.Item source, string displayName, string nsPath, IEnumerator<string> remaining) {
                if (!remaining.MoveNext()) {
                    Leaves.Add(new LeafInfo { Source = source, DisplayName = displayName, NamespacePath = nsPath });
                    return;
                }
                var segment = remaining.Current ?? string.Empty;
                if (!Children.TryGetValue(segment, out var child)) {
                    Children[segment] = child = new GroupNode(segment);
                }
                child.Add(source, displayName, nsPath, remaining);
            }

            public void Collapse() {
                foreach (var child in Children.Values) child.Collapse();
                while (Leaves.Count == 0 && Children.Count == 1) {
                    var only = Children.Values.First();
                    Name = Name + "." + only.Name;
                    Children.Clear();
                    foreach (var kv in only.Children) Children[kv.Key] = kv.Value;
                    Leaves.AddRange(only.Leaves);
                }
            }

            public void Materialize(AdvancedDropdownItem parent) {
                foreach (var child in Children.Values.OrderBy(c => c.Name, StringComparer.Ordinal)) {
                    var childItem = new AdvancedDropdownItem(child.Name);
                    parent.AddChild(childItem);
                    child.Materialize(childItem);
                }
                foreach (var leaf in Leaves) {
                    parent.AddChild(new TypedDropdownItem(leaf.DisplayName, leaf.NamespacePath, leaf.Source));
                }
            }
        }

        private struct LeafInfo {
            public SearchableDropdown.Item Source;
            public string DisplayName;
            public string NamespacePath;
        }
    }

    internal sealed class TypedDropdownItem : AdvancedDropdownItem {
        private static readonly Action<AdvancedDropdownItem, string> s_SetName;

        static TypedDropdownItem() {
            var field = typeof(AdvancedDropdownItem).GetField("m_Name", BindingFlags.NonPublic | BindingFlags.Instance);
            if (field != null) {
                s_SetName = (it, n) => field.SetValue(it, n);
            }
        }

        public readonly string DisplayName;
        public readonly string NamespacePath;
        public readonly GUIContent NameContent;
        public readonly GUIContent NamespaceContent;
        public readonly object Payload;
        public readonly bool Enabled;

        public TypedDropdownItem(string displayName, string namespacePath, SearchableDropdown.Item source) : base(displayName) {
            DisplayName = displayName;
            NamespacePath = namespacePath ?? string.Empty;
            Payload = source.Payload;
            Enabled = source.Enabled;
            enabled = source.Enabled;

            var icon = ResolveIcon(source.Payload);
            var fullPath = NamespacePath.Length > 0 ? NamespacePath + "." + displayName : displayName;
            NameContent = new GUIContent(displayName, icon, fullPath);
            NamespaceContent = NamespacePath.Length == 0 ? GUIContent.none : new GUIContent(" " + NamespacePath, null, fullPath);
            content.tooltip = fullPath;

            if (s_SetName != null && NamespacePath.Length > 0) {
                s_SetName(this, NamespacePath + "." + displayName);
            }
        }

        public void Draw(Rect rect, GUIStyle mainStyle, GUIStyle faintStyle, bool isHover, bool isActive, bool on, bool hasKeyboardFocus) {
            var typeStyle = new GUIStyle(mainStyle) { clipping = TextClipping.Overflow };
            typeStyle.Draw(rect, NameContent, isHover, isActive, on, hasKeyboardFocus);

            if (NamespaceContent == GUIContent.none) return;
            var typeSize = typeStyle.CalcSize(NameContent);
            var nsRect = new Rect(rect.x + typeSize.x, rect.y, rect.width - typeSize.x, rect.height);
            faintStyle.Draw(nsRect, NamespaceContent, on || hasKeyboardFocus, false, false, false);
        }

        private static Texture2D ResolveIcon(object payload) {
            if (payload is Type type) {
                var icon = AssetPreview.GetMiniTypeThumbnail(type);
                if (icon != null) return icon;
            }
            return EditorGUIUtility.FindTexture("cs Script Icon");
        }
    }

    internal sealed class TypedDropdownGUI : AdvancedDropdownGUI {
        private static GUIStyle s_LineStyle;
        private static GUIStyle s_FaintStyle;

        internal override GUIStyle lineStyle => s_LineStyle ??= new GUIStyle("DD LargeItemStyle");

        private static GUIStyle FaintStyle {
            get {
                if (s_FaintStyle != null) return s_FaintStyle;
                s_FaintStyle = new GUIStyle("DD LargeItemStyle") { clipping = TextClipping.Clip };
                var gray = EditorGUIUtility.isProSkin
                    ? new Color(0.5f, 0.5f, 0.5f, 1f)
                    : new Color(0.35f, 0.35f, 0.35f, 1f);
                s_FaintStyle.normal.textColor = gray;
                s_FaintStyle.onNormal.textColor = gray;
                s_FaintStyle.hover.textColor = gray;
                s_FaintStyle.onHover.textColor = gray;
                s_FaintStyle.focused.textColor = gray;
                s_FaintStyle.onFocused.textColor = gray;
                s_FaintStyle.active.textColor = gray;
                s_FaintStyle.onActive.textColor = gray;
                return s_FaintStyle;
            }
        }

        public TypedDropdownGUI(AdvancedDropdownDataSource dataSource) : base(dataSource) {}

        internal override void DrawItemContent(AdvancedDropdownItem item, Rect rect, GUIContent content, bool isHover, bool isActive, bool on, bool hasKeyboardFocus) {
            if (item is TypedDropdownItem t) {
                t.Draw(rect, lineStyle, FaintStyle, isHover, isActive, on, hasKeyboardFocus);
            } else {
                lineStyle.Draw(rect, content, isHover, isActive, on, hasKeyboardFocus);
            }
        }
    }
}
