#if ((DEBUG || FFS_ECS_ENABLE_DEBUG) && !FFS_ECS_DISABLE_DEBUG)
#define FFS_ECS_DEBUG
#endif
#if ((DEBUG || FFS_ECS_ENABLE_DEBUG) && !FFS_ECS_DISABLE_DEBUG)
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FFS.Libraries.StaticEcs.Unity.Editor {

    public partial class EntitiesDrawer<TWorld, TEntityProvider> where TWorld : struct, IWorldType where TEntityProvider : StaticEcsEntityProvider<TWorld> {
        private int _maxEntityResult = 100;
        private int _currentEntityCount;
        private bool _filterActive;
        private bool _filterDirty = true;
        private CompositeHandleFilter _cachedFilter;

        internal enum FilterMode : byte { All, None, Any }
        internal enum FilterVariant : byte { Default, OnlyDisabled, WithDisabled }

        internal class FilterEntry {
            public EditorEntityDataMetaByWorld Meta;
            public FilterMode Mode;
            public FilterVariant Variant;
        }

        private readonly List<FilterEntry> _filters = new();
        private readonly List<EditorEntityDataMetaByWorld> _componentsAndTags = new();

        private static readonly Color _chipColorAll = new(0.30f, 0.70f, 0.30f, 0.25f);
        private static readonly Color _chipColorNone = new(0.80f, 0.30f, 0.30f, 0.25f);
        private static readonly Color _chipColorAny = new(0.30f, 0.55f, 0.85f, 0.25f);
        private static readonly Color _chipBorder = new(0f, 0f, 0f, 0.35f);

        private static GUIStyle _chipLabelStyle;
        private static GUIStyle _chipPrefixStyle;
        private static GUIStyle _chipCloseStyle;

        private void DrawEntitiesFilter() {
            SegmentFilter();
            ComponentsFilter();
            GidFilter();
            ColumnsFilter();
            DataShowFilter();
            EntityResultCountFilter();
            EditorGUILayout.Space(10);
        }

        private void SegmentFilter() {
            var system = EcsDebug<TWorld>.DebugViewSystem;
            if (system == null || !system.IsSegmentFilterActive) return;

            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField($"Segment: {system.SegmentFilterIdx}", Ui.WidthLine(200));
                if (GUILayout.Button("Clear", Ui.ButtonStyleTheme, Ui.WidthLine(60))) {
                    system.ClearSegmentFilter();
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        private void ComponentsFilter() {
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField("Filter:", Ui.WidthLine(120));
                var newFilterActive = EditorGUILayout.Toggle(_filterActive, Ui.WidthLine(20));
                if (newFilterActive != _filterActive) {
                    _filterActive = newFilterActive;
                    _filterDirty = true;
                }

                if (_filterActive) {
                    GUILayout.Space(6);
                    if (GUILayout.Button(new GUIContent(EditorGUIUtility.IconContent("d_Toolbar Plus").image),
                                         Ui.ButtonStyleTheme, Ui.WidthLine(110))) {
                        ShowAddFilterMenu();
                    }
                    if (_filters.Count > 0) {
                        if (GUILayout.Button("Clear", Ui.ButtonStyleTheme, Ui.WidthLine(60))) {
                            _filters.Clear();
                            _filterDirty = true;
                        }
                    }
                    GUILayout.FlexibleSpace();
                }
            }
            EditorGUILayout.EndHorizontal();

            if (!_filterActive) return;

            if (_filters.Count > 0) {
                EditorGUILayout.Space(2);
                DrawChips();
            }

            var hint = GetValidationHint();
            if (hint != null) {
                EditorGUILayout.HelpBox(hint, MessageType.Warning, true);
            }
        }

        private void DrawChips() {
            EnsureChipStyles();
            var maxWidth = Mathf.Max(200f, EditorGUIUtility.currentViewWidth - 30f);
            float used = 0;
            EditorGUILayout.BeginHorizontal();
            for (var i = 0; i < _filters.Count; i++) {
                var entry = _filters[i];
                var chipWidth = MeasureChip(entry);
                if (used > 0 && used + chipWidth > maxWidth) {
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                    used = 0;
                }
                if (DrawChip(entry, chipWidth)) {
                    _filters.RemoveAt(i);
                    _filterDirty = true;
                    i--;
                    continue;
                }
                used += chipWidth + 4f;
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }

        private float MeasureChip(FilterEntry entry) {
            var content = new GUIContent(entry.Meta.Name);
            var size = _chipLabelStyle.CalcSize(content);
            var extra = VariantBadge(entry.Variant) != null ? 18f : 0f;
            return Mathf.Ceil(size.x) + 22f + extra + 20f;
        }

        private bool DrawChip(FilterEntry entry, float width) {
            var rect = GUILayoutUtility.GetRect(width, 20f, GUILayout.Width(width), GUILayout.Height(20f));
            rect.y += 1f;
            rect.height -= 2f;

            if (Event.current.type == EventType.Repaint) {
                EditorGUI.DrawRect(rect, ModeColor(entry.Mode));
                DrawChipBorder(rect);
            }

            var prefixRect = new Rect(rect.x + 4f, rect.y, 14f, rect.height);
            GUI.Label(prefixRect, ModeGlyph(entry.Mode), _chipPrefixStyle);

            var closeRect = new Rect(rect.xMax - 18f, rect.y + 2f, 16f, 16f);
            var badge = VariantBadge(entry.Variant);
            var badgeWidth = badge != null ? 18f : 0f;
            var labelX = prefixRect.xMax + 2f;
            var labelWidth = rect.xMax - closeRect.width - 4f - badgeWidth - labelX;
            var labelRect = new Rect(labelX, rect.y, labelWidth, rect.height);

            if (entry.Meta.Type != null && entry.Meta.Type.EditorTypeColor(out var typeColor)) {
                var prev = GUI.contentColor;
                GUI.contentColor = typeColor;
                GUI.Label(labelRect, entry.Meta.Name, _chipLabelStyle);
                GUI.contentColor = prev;
            } else {
                GUI.Label(labelRect, entry.Meta.Name, _chipLabelStyle);
            }

            if (badge != null) {
                var badgeRect = new Rect(labelRect.xMax, rect.y, badgeWidth, rect.height);
                GUI.Label(badgeRect, badge, _chipPrefixStyle);
            }

            var remove = GUI.Button(closeRect, "\u2715", _chipCloseStyle);

            var evt = Event.current;
            var clickRect = new Rect(rect.x, rect.y, rect.width - closeRect.width - 2f, rect.height);
            if (evt.type == EventType.MouseDown && clickRect.Contains(evt.mousePosition)) {
                if (evt.button == 0) {
                    CycleMode(entry);
                    evt.Use();
                    GUI.changed = true;
                } else if (evt.button == 1) {
                    ShowChipContextMenu(entry);
                    evt.Use();
                }
            }

            return remove;
        }

        private void CycleMode(FilterEntry entry) {
            entry.Mode = (FilterMode) (((int) entry.Mode + 1) % 3);
            if (entry.Mode == FilterMode.None && entry.Variant == FilterVariant.OnlyDisabled) {
                entry.Variant = FilterVariant.Default;
            }
            _filterDirty = true;
        }

        private void ShowChipContextMenu(FilterEntry entry) {
            var menu = new GenericMenu();
            var isTag = !entry.Meta.ComponentHandle.HasValue;

            menu.AddItem(new GUIContent("Mode/All"), entry.Mode == FilterMode.All, () => SetMode(entry, FilterMode.All));
            menu.AddItem(new GUIContent("Mode/None"), entry.Mode == FilterMode.None, () => SetMode(entry, FilterMode.None));
            menu.AddItem(new GUIContent("Mode/Any"), entry.Mode == FilterMode.Any, () => SetMode(entry, FilterMode.Any));

            menu.AddItem(new GUIContent("Variant/Default"), entry.Variant == FilterVariant.Default, () => SetVariant(entry, FilterVariant.Default));
            if (isTag || entry.Mode == FilterMode.None) {
                menu.AddDisabledItem(new GUIContent("Variant/Only disabled"), entry.Variant == FilterVariant.OnlyDisabled);
            } else {
                menu.AddItem(new GUIContent("Variant/Only disabled"), entry.Variant == FilterVariant.OnlyDisabled, () => SetVariant(entry, FilterVariant.OnlyDisabled));
            }
            menu.AddItem(new GUIContent("Variant/With disabled"), entry.Variant == FilterVariant.WithDisabled, () => SetVariant(entry, FilterVariant.WithDisabled));

            menu.AddSeparator("");
            menu.AddItem(new GUIContent("Remove"), false, () => {
                _filters.Remove(entry);
                _filterDirty = true;
            });

            menu.ShowAsContext();
        }

        private void SetMode(FilterEntry entry, FilterMode mode) {
            entry.Mode = mode;
            if (mode == FilterMode.None && entry.Variant == FilterVariant.OnlyDisabled) {
                entry.Variant = FilterVariant.Default;
            }
            _filterDirty = true;
        }

        private void SetVariant(FilterEntry entry, FilterVariant variant) {
            entry.Variant = variant;
            _filterDirty = true;
        }

        private void ShowAddFilterMenu() {
            var items = new List<SearchableDropdown.Item>(_componentsAndTags.Count);
            foreach (var idx in _componentsAndTags) {
                if (ContainsMeta(idx)) continue;
                items.Add(new SearchableDropdown.Item(idx.FullName, idx));
            }
            SearchableDropdown.Show("Filter", items, payload => {
                _filters.Add(new FilterEntry {
                    Meta = (EditorEntityDataMetaByWorld) payload,
                    Mode = FilterMode.All,
                    Variant = FilterVariant.Default,
                });
                _filterDirty = true;
            });
        }

        private bool ContainsMeta(EditorEntityDataMetaByWorld meta) {
            foreach (var f in _filters) {
                if (f.Meta == meta) return true;
            }
            return false;
        }

        private static Color ModeColor(FilterMode mode) => mode switch {
            FilterMode.All => _chipColorAll,
            FilterMode.None => _chipColorNone,
            FilterMode.Any => _chipColorAny,
            _ => _chipColorAll,
        };

        private static string ModeGlyph(FilterMode mode) => mode switch {
            FilterMode.All => "\u2714",
            FilterMode.None => "\u2715",
            FilterMode.Any => "\u25C7",
            _ => "",
        };

        private static string VariantBadge(FilterVariant variant) => variant switch {
            FilterVariant.OnlyDisabled => "D",
            FilterVariant.WithDisabled => "\u00B1",
            _ => null,
        };

        private static void DrawChipBorder(Rect rect) {
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, rect.width, 1f), _chipBorder);
            EditorGUI.DrawRect(new Rect(rect.x, rect.yMax - 1f, rect.width, 1f), _chipBorder);
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, 1f, rect.height), _chipBorder);
            EditorGUI.DrawRect(new Rect(rect.xMax - 1f, rect.y, 1f, rect.height), _chipBorder);
        }

        private static void EnsureChipStyles() {
            if (_chipLabelStyle == null) {
                _chipLabelStyle = new GUIStyle(EditorStyles.label) {
                    alignment = TextAnchor.MiddleLeft,
                    padding = new RectOffset(0, 0, 0, 0),
                    margin = new RectOffset(0, 0, 0, 0),
                };
            }
            if (_chipPrefixStyle == null) {
                _chipPrefixStyle = new GUIStyle(EditorStyles.boldLabel) {
                    alignment = TextAnchor.MiddleCenter,
                    padding = new RectOffset(0, 0, 0, 0),
                    margin = new RectOffset(0, 0, 0, 0),
                };
            }
            if (_chipCloseStyle == null) {
                _chipCloseStyle = new GUIStyle(EditorStyles.iconButton) {
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = 10,
                };
            }
        }

        private bool IsFilterValid() {
            if (!_filterActive || _filters.Count == 0) return false;
            var anyCounts = new Dictionary<QueryMethodType, int>();
            foreach (var entry in _filters) {
                var method = ToQueryMethod(entry.Mode, entry.Variant);
                if (IsAnyMethod(method)) {
                    anyCounts.TryGetValue(method, out var c);
                    anyCounts[method] = c + 1;
                } else {
                    return true;
                }
            }
            foreach (var c in anyCounts.Values) {
                if (c >= 2) return true;
            }
            return false;
        }

        private string GetValidationHint() {
            if (_filters.Count == 0) return "Please, provide at least one filter";
            if (IsFilterValid()) {
                foreach (var entry in _filters) {
                    var method = ToQueryMethod(entry.Mode, entry.Variant);
                    if (IsAnyMethod(method)) {
                        var count = 0;
                        foreach (var e2 in _filters) {
                            if (ToQueryMethod(e2.Mode, e2.Variant) == method) count++;
                        }
                        if (count == 1) return "Please, provide at least two [Any] filter";
                    }
                }
                return null;
            }
            return "Please, provide at least two [Any] filter";
        }

        private void GidFilter() {
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField("Entity GID:", Ui.WidthLine(120));
                _gidFilterActive = EditorGUILayout.Toggle(_gidFilterActive);
            }
            EditorGUILayout.EndHorizontal();
            if (_gidFilterActive) {
                EditorGUI.indentLevel++;
                _gidFilterValue = EditorGUILayout.IntField("Value", _gidFilterValue, Ui.WidthLine(300));
                if (_gidFilterValue < 0) _gidFilterValue = 0;
                EditorGUI.indentLevel--;
            }
        }

        private void ColumnsFilter() {
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField("Select:", Ui.WidthLine(120));

                var incAll = _componentsColumns.Count == _components.Count;
                var excAll = _componentsColumns.Count == 0;

                incAll = incAll && _tagsColumns.Count == _tags.Count;
                excAll = excAll && _tagsColumns.Count == 0;

                if (GUILayout.Button("All", incAll ? Ui.ButtonStyleGrey : Ui.ButtonStyleTheme, Ui.WidthLine(60))) {
                    ShowAllColumns(null);
                }

                if (GUILayout.Button("None", excAll ? Ui.ButtonStyleGrey : Ui.ButtonStyleTheme, Ui.WidthLine(60))) {
                    ShowNoneColumns();
                }

                using (Ui.EnabledScopeVal(!incAll)) {
                    if (Ui.PlusDropDownButton) {
                        var items = new List<SearchableDropdown.Item>();

                        foreach (var idx in _components) {
                            if (!_componentsColumns.Contains(idx)) {
                                items.Add(new SearchableDropdown.Item(idx.FullName, new KeyValuePair<bool, EditorEntityDataMetaByWorld>(true, idx)));
                            }
                        }

                        foreach (var idx in _tags) {
                            if (!_tagsColumns.Contains(idx)) {
                                items.Add(new SearchableDropdown.Item(idx.FullName, new KeyValuePair<bool, EditorEntityDataMetaByWorld>(false, idx)));
                            }
                        }

                        SearchableDropdown.Show("Columns", items, payload => {
                            var kv = (KeyValuePair<bool, EditorEntityDataMetaByWorld>) payload;
                            (kv.Key ? _componentsColumns : _tagsColumns).Add(kv.Value);
                        });
                    }
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DataShowFilter() {
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField("Show data:", Ui.WidthLine(120));

                if (GUILayout.Button("All", Ui.ButtonStyleTheme, Ui.WidthLine(60))) {
                    for (var i = 0; i < _components.Count; i++) {
                        _components[i].ShowTableData = true;
                    }
                }

                if (GUILayout.Button("None", Ui.ButtonStyleTheme, Ui.WidthLine(60))) {
                    for (var i = 0; i < _components.Count; i++) {
                        _components[i].ShowTableData = false;
                    }
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        private void EntityResultCountFilter() {
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField("Max entities result:", Ui.WidthLine(120));
                _maxEntityResult = EditorGUILayout.IntSlider(_maxEntityResult, 0, 1000, Ui.WidthLine(185));
            }
            EditorGUILayout.EndHorizontal();
        }

        private CompositeHandleFilter GetOrBuildFilter() {
            if (!_filterDirty) return _cachedFilter;
            _filterDirty = false;

            _cachedFilter = new CompositeHandleFilter();

            var buckets = new Dictionary<QueryMethodType, List<ComponentsHandle>>();
            foreach (var entry in _filters) {
                var method = ToQueryMethod(entry.Mode, entry.Variant);
                if (!buckets.TryGetValue(method, out var list)) {
                    list = new List<ComponentsHandle>();
                    buckets[method] = list;
                }
                if (entry.Meta.ComponentHandle.HasValue) list.Add(entry.Meta.ComponentHandle.Value);
                else if (entry.Meta.TagHandle.HasValue) list.Add(entry.Meta.TagHandle.Value);
            }

            foreach (var kv in buckets) {
                if (IsAnyMethod(kv.Key) && kv.Value.Count < 2) continue;
                _cachedFilter.Add(new HandleComponentsFilter(kv.Value, kv.Key));
            }

            return _cachedFilter;
        }

        internal static QueryMethodType ToQueryMethod(FilterMode mode, FilterVariant variant) {
            return mode switch {
                FilterMode.All => variant switch {
                    FilterVariant.OnlyDisabled => QueryMethodType.ALL_ONLY_DISABLED,
                    FilterVariant.WithDisabled => QueryMethodType.ALL_WITH_DISABLED,
                    _ => QueryMethodType.ALL,
                },
                FilterMode.None => variant == FilterVariant.WithDisabled
                    ? QueryMethodType.NONE_WITH_DISABLED
                    : QueryMethodType.NONE,
                FilterMode.Any => variant switch {
                    FilterVariant.OnlyDisabled => QueryMethodType.ANY_ONLY_DISABLED,
                    FilterVariant.WithDisabled => QueryMethodType.ANY_WITH_DISABLED,
                    _ => QueryMethodType.ANY,
                },
                _ => QueryMethodType.ALL,
            };
        }

        private static bool IsAnyMethod(QueryMethodType m) =>
            m == QueryMethodType.ANY || m == QueryMethodType.ANY_ONLY_DISABLED || m == QueryMethodType.ANY_WITH_DISABLED;
    }

}
#endif
