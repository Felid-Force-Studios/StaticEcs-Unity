#if ((DEBUG || FFS_ECS_ENABLE_DEBUG) && !FFS_ECS_DISABLE_DEBUG)
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using static UnityEditor.EditorGUILayout;

namespace FFS.Libraries.StaticEcs.Unity.Editor {
    public class StaticEcsViewStatsTab<TWorld> : IStaticEcsViewTab
        where TWorld : struct, IWorldType {

        private readonly Dictionary<Type, StatsDrawer<TWorld>> _drawersByWorldTypeType = new();
        private StatsDrawer<TWorld> _currentDrawer;
        private IStaticEcsViewNavigation _navigation;

        private StatsSettings _savedSettings;

        public string Name() => "Stats";

        public void Init() { }

        public void SetNavigation(IStaticEcsViewNavigation navigation) {
            _navigation = navigation;
        }

        public void Draw() {
            _currentDrawer?.Draw();
        }

        public void Destroy() { }

        public void OnWorldChanged(AbstractWorldData newWorldData) {
            if (!_drawersByWorldTypeType.ContainsKey(newWorldData.Handle.WorldType)) {
                var drawer = new StatsDrawer<TWorld>(newWorldData, _navigation);
                if (_savedSettings != null) drawer.LoadFromConfig(_savedSettings);
                _drawersByWorldTypeType[newWorldData.Handle.WorldType] = drawer;
            }

            _currentDrawer = _drawersByWorldTypeType[newWorldData.Handle.WorldType];
        }

        public void SaveState(WorldViewSettings settings) {
            _currentDrawer?.SaveToConfig(settings.stats);
        }

        public void LoadState(WorldViewSettings settings) {
            _savedSettings = settings.stats;
            _currentDrawer?.LoadFromConfig(settings.stats);
        }
    }

    public class StatsDrawer<TWorld> where TWorld : struct, IWorldType {
        private const char SegmentNull = '\u00B7';    // · — not allocated
        private const char SegmentEmpty = '\u25A1';   // □ — allocated, empty
        private const char SegmentBlock1 = '\u2591';  // ░
        private const char SegmentBlock2 = '\u2592';  // ▒
        private const char SegmentBlock3 = '\u2593';  // ▓
        private const char SegmentBlock4 = '\u2588';  // █

        private static readonly GUIContent FragmentationLabel = new(
            "Fragmentation threshold"
        );

        private readonly ComponentsHandle?[] componentHandles;
        private readonly ComponentsHandle?[] tagHandles;
        private readonly EventsHandle?[] eventHandles;
        private readonly int[] componentOrder;
        private readonly int[] tagOrder;
        private readonly int[] eventOrder;
        private readonly List<EditorEntityTypeMeta> registeredEntityTypes = new();

        private readonly WorldHandle _handle;
        private readonly AbstractWorldData _worldData;
        private readonly WorldMetaData _worldMeta;
        private readonly IStaticEcsViewNavigation _navigation;

        private bool _showNotRegistered = false;
        private int _fragmentationThreshold = 512;
        private bool _foldoutEntityTypes = true;
        private bool _foldoutEvents = true;
        private bool _foldoutComponents = true;
        private bool _foldoutTags = true;
        private Vector2 _verticalScroll = Vector2.zero;
        private float _segmentsScrollX = 0f;

        internal void SaveToConfig(StatsSettings settings) {
            settings.showNotRegistered = _showNotRegistered;
            settings.fragmentationThreshold = _fragmentationThreshold;
            settings.foldoutEntityTypes = _foldoutEntityTypes;
            settings.foldoutEvents = _foldoutEvents;
            settings.foldoutComponents = _foldoutComponents;
            settings.foldoutTags = _foldoutTags;
        }

        internal void LoadFromConfig(StatsSettings settings) {
            _showNotRegistered = settings.showNotRegistered;
            _fragmentationThreshold = settings.fragmentationThreshold;
            _foldoutEntityTypes = settings.foldoutEntityTypes;
            _foldoutEvents = settings.foldoutEvents;
            _foldoutComponents = settings.foldoutComponents;
            _foldoutTags = settings.foldoutTags;
        }

        private static GUIStyle _monoStyle;
        private static GUIStyle _monoStyleYellow;
        private static GUIStyle _monoStyleRed;

        private static GUIStyle MonoStyle {
            get {
                if (_monoStyle == null) {
                    _monoStyle = new GUIStyle(EditorStyles.label) {
                        font = GetFont(),
                        fontSize = 11,
                        alignment = TextAnchor.MiddleLeft,
                        padding = new RectOffset(0, 0, 0, 0),
                        normal = {
                            textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black
                        }
                    };
                }
                return _monoStyle;
            }
        }

        private static GUIStyle MonoStyleYellow {
            get {
                if (_monoStyleYellow == null) {
                    _monoStyleYellow = new GUIStyle(MonoStyle) {
                        normal = {
                            textColor = Color.yellow
                        }
                    };
                }
                return _monoStyleYellow;
            }
        }

        private static GUIStyle MonoStyleRed {
            get {
                if (_monoStyleRed == null) {
                    _monoStyleRed = new GUIStyle(MonoStyle) {
                        normal = {
                            textColor = Color.red
                        }
                    };
                }
                return _monoStyleRed;
            }
        }

        public StatsDrawer(AbstractWorldData worldData, IStaticEcsViewNavigation navigation) {
            _worldData = worldData;
            _handle = _worldData.Handle;
            _navigation = navigation;

            _worldMeta = MetaData.GetWorldMetaData(typeof(TWorld));

            componentHandles = new ComponentsHandle?[_worldMeta.Components.Count];
            for (var i = 0; i < _worldMeta.Components.Count; i++) {
                if (_handle.TryGetComponentsHandle(_worldMeta.Components[i].Type, out var handle)) {
                    componentHandles[i] = handle;
                }
            }

            tagHandles = new ComponentsHandle?[_worldMeta.Tags.Count];
            for (var i = 0; i < _worldMeta.Tags.Count; i++) {
                if (_handle.TryGetComponentsHandle(_worldMeta.Tags[i].Type, out var handle)) {
                    tagHandles[i] = handle;
                }
            }

            eventHandles = new EventsHandle?[_worldMeta.Events.Count];
            for (var i = 0; i < _worldMeta.Events.Count; i++) {
                if (_handle.TryGetEventsHandle(_worldMeta.Events[i].Type, out var handle)) {
                    eventHandles[i] = handle;
                }
            }

            for (var i = 0; i < _worldMeta.EntityTypes.Count; i++) {
                if (_handle.IsEntityTypeRegistered(_worldMeta.EntityTypes[i].Id)) {
                    registeredEntityTypes.Add(_worldMeta.EntityTypes[i]);
                }
            }

            componentOrder = BuildSortedOrder(_worldMeta.Components);
            tagOrder = BuildSortedOrder(_worldMeta.Tags);
            eventOrder = BuildSortedEventOrder(_worldMeta.Events);
        }

        private static int[] BuildSortedOrder(List<EditorEntityDataMeta> metas) {
            var order = new int[metas.Count];
            for (var i = 0; i < order.Length; i++) order[i] = i;
            Array.Sort(order, (a, b) => {
                var aSys = metas[a].Type.IsSystemType();
                var bSys = metas[b].Type.IsSystemType();
                if (aSys != bSys) return aSys ? 1 : -1;
                var aHasColor = metas[a].Type.EditorTypeColor(out var aColor);
                var bHasColor = metas[b].Type.EditorTypeColor(out var bColor);
                if (aHasColor != bHasColor) return aHasColor ? -1 : 1;
                if (aHasColor) {
                    var colorCmp = string.Compare(ColorToKey(aColor), ColorToKey(bColor), StringComparison.Ordinal);
                    if (colorCmp != 0) return colorCmp;
                }
                return string.Compare(metas[a].Name, metas[b].Name, StringComparison.Ordinal);
            });
            return order;
        }

        private static int[] BuildSortedEventOrder(List<EditorEventDataMeta> metas) {
            var order = new int[metas.Count];
            for (var i = 0; i < order.Length; i++) order[i] = i;
            Array.Sort(order, (a, b) => {
                var aSys = metas[a].Type.IsSystemType();
                var bSys = metas[b].Type.IsSystemType();
                if (aSys != bSys) return aSys ? 1 : -1;
                var aHasColor = metas[a].Type.EditorTypeColor(out var aColor);
                var bHasColor = metas[b].Type.EditorTypeColor(out var bColor);
                if (aHasColor != bHasColor) return aHasColor ? -1 : 1;
                if (aHasColor) {
                    var colorCmp = string.Compare(ColorToKey(aColor), ColorToKey(bColor), StringComparison.Ordinal);
                    if (colorCmp != 0) return colorCmp;
                }
                return string.Compare(metas[a].Name, metas[b].Name, StringComparison.Ordinal);
            });
            return order;
        }

        private static string ColorToKey(Color c) {
            return $"{c.r:F2}{c.g:F2}{c.b:F2}";
        }

        internal void Draw() {
            Space(10);

            BeginHorizontal();
            LabelField("Show not registered", Ui.WidthLine(200));
            _showNotRegistered = Toggle(_showNotRegistered);
            EndHorizontal();

            BeginHorizontal();
            LabelField(FragmentationLabel, Ui.WidthLine(200));
            _fragmentationThreshold = EditorGUILayout.IntSlider(_fragmentationThreshold, 256, 4096, GUILayout.MaxWidth(200)) / 256 * 256;
            EndHorizontal();
            Space();

            DrawWorldStats();

            var segmentsCount = GetWorldSegmentsCount();
            var compDrawCount = CalculateLastNonEmptySegment(componentHandles, segmentsCount) + 1;
            var tagDrawCount = CalculateLastNonEmptySegment(tagHandles, segmentsCount) + 1;
            var drawCount = Math.Max(compDrawCount, tagDrawCount);

            _verticalScroll = BeginScrollView(_verticalScroll);

            DrawEntityTypeStats();
            DrawEventsPoolsStats(eventOrder);
            DrawHandleStats("Components:", _worldMeta.Components, componentHandles, componentOrder, true, drawCount, segmentsCount, ref _foldoutComponents);
            DrawHandleStats("Tags:", _worldMeta.Tags, tagHandles, tagOrder, true, drawCount, segmentsCount, ref _foldoutTags);

            EndScrollView();

            if (drawCount > 0) {
                var contentWidth = drawCount * GetActualCharWidth();
                var viewWidth = Mathf.Max(1f, EditorGUIUtility.currentViewWidth - 450f);
                BeginHorizontal();
                GUILayout.Space(430f);
                _segmentsScrollX = GUILayout.HorizontalScrollbar(
                    _segmentsScrollX, Mathf.Min(viewWidth, contentWidth), 0f, contentWidth);
                EndHorizontal();
            }
        }

        private int GetWorldSegmentsCount() {
            return (int)(World<TWorld>.CalculateEntitiesCapacity() / Const.ENTITIES_IN_SEGMENT);
        }

        private static bool IsSegmentNonEmpty(ComponentsHandle handle, uint segmentIdx) {
            return handle.IsSegmentAllocated(segmentIdx)
                && (handle.AnyMask(segmentIdx, 0) != 0
                 || handle.AnyMask(segmentIdx, 1) != 0
                 || handle.AnyMask(segmentIdx, 2) != 0
                 || handle.AnyMask(segmentIdx, 3) != 0);
        }

        private static char GetSegmentChar(ComponentsHandle handle, uint segmentIdx) {
            if (!handle.IsSegmentAllocated(segmentIdx)) return SegmentNull;
            var filled = 0;
            if (handle.AnyMask(segmentIdx, 0) != 0) filled++;
            if (handle.AnyMask(segmentIdx, 1) != 0) filled++;
            if (handle.AnyMask(segmentIdx, 2) != 0) filled++;
            if (handle.AnyMask(segmentIdx, 3) != 0) filled++;
            return filled switch {
                1 => SegmentBlock1,
                2 => SegmentBlock2,
                3 => SegmentBlock3,
                4 => SegmentBlock4,
                _ => SegmentEmpty
            };
        }

        private static string BuildSegmentsString(ComponentsHandle handle, int drawCount) {
            var chars = new char[drawCount];
            for (var s = 0; s < drawCount; s++) {
                chars[s] = GetSegmentChar(handle, (uint)s);
            }
            return new string(chars);
        }

        private static int CountAllocatedSegments(ComponentsHandle handle, int segmentsCount) {
            var count = 0;
            for (var s = 0; s < segmentsCount; s++) {
                if (handle.IsSegmentAllocated((uint)s)) count++;
            }
            return count;
        }

        private GUIStyle GetSegmentStyle(uint entityCount, ComponentsHandle handle, int segmentsCount) {
            var allocated = CountAllocatedSegments(handle, segmentsCount);
            if (allocated == 0) return MonoStyle;
            if (entityCount == 0) return MonoStyleRed;
            var needed = (int)((entityCount + Const.ENTITIES_IN_SEGMENT - 1) / Const.ENTITIES_IN_SEGMENT);
            var waste = (allocated - needed) * Const.ENTITIES_IN_SEGMENT;
            if (waste >= _fragmentationThreshold * 2) return MonoStyleRed;
            if (waste >= _fragmentationThreshold) return MonoStyleYellow;
            return MonoStyle;
        }

        private int CalculateLastNonEmptySegment(ComponentsHandle?[] handles, int segmentsCount) {
            for (var s = segmentsCount - 1; s >= 0; s--) {
                for (var i = 0; i < handles.Length; i++) {
                    if (handles[i].HasValue && IsSegmentNonEmpty(handles[i].Value, (uint)s)) {
                        return s;
                    }
                }
            }
            return -1;
        }

        private void DrawSegmentCell(ComponentsHandle handle, int drawCount, GUIStyle style) {
            var charW = GetActualCharWidth();
            var contentWidth = drawCount * charW;
            var cellRect = GUILayoutUtility.GetRect(0, EditorGUIUtility.singleLineHeight, GUILayout.ExpandWidth(true));

            var hoveredSeg = -1;
            if (cellRect.Contains(Event.current.mousePosition)) {
                var localX = Event.current.mousePosition.x - cellRect.x + _segmentsScrollX;
                var seg = (int)(localX / charW);
                if (seg >= 0 && seg < drawCount && handle.IsSegmentAllocated((uint)seg)) {
                    hoveredSeg = seg;
                    EditorGUIUtility.AddCursorRect(cellRect, MouseCursor.Link);

                    if (Event.current.type == EventType.MouseDown) {
                        var system = EcsDebug<TWorld>.DebugViewSystem;
                        system.SetSegmentFilter(handle, (uint)seg);
                        _navigation?.SelectTab("Entities");
                        Event.current.Use();
                    }
                }
            }

            GUI.BeginClip(cellRect);
            GUI.Label(new Rect(-_segmentsScrollX, 0, contentWidth, cellRect.height),
                BuildSegmentsString(handle, drawCount), style);

            if (hoveredSeg >= 0) {
                var highlightRect = new Rect(hoveredSeg * charW - _segmentsScrollX, 0, charW, cellRect.height);
                EditorGUI.DrawRect(highlightRect, new Color(1f, 1f, 1f, 0.15f));
            }

            GUI.EndClip();
        }

        private float _cachedCharWidth;

        private float GetActualCharWidth() {
            if (_cachedCharWidth <= 0) {
                _cachedCharWidth = MonoStyle.CalcSize(new GUIContent(SegmentBlock4.ToString())).x;
            }
            return _cachedCharWidth;
        }

        private void DrawHandleStats(string type, List<EditorEntityDataMeta> indexes, ComponentsHandle?[] handles, int[] order, bool withCapacity, int drawCount, int segmentsCount, ref bool foldout) {
            foldout = EditorGUILayout.Foldout(foldout, type, true, Ui.FoldoutStyleTheme);
            if (!foldout) {
                Space(20);
                return;
            }

            BeginHorizontal();
            SelectableLabel("Name", Ui.LabelStyleThemeCenter, Ui.WidthLine(200));
            Ui.DrawSeparator();
            SelectableLabel("Count", Ui.LabelStyleThemeCenter, Ui.WidthLine(90));
            Ui.DrawSeparator();
            SelectableLabel("Capacity", Ui.LabelStyleThemeCenter, Ui.WidthLine(90));
            Ui.DrawSeparator();
            EndHorizontal();

            for (var o = 0; o < order.Length; o++) {
                var i = order[o];
                var idx = indexes[i];
                var handle = handles[i];
                if (handle.HasValue) {
                    var h = handle.Value;
                    var count = h.CalculateCount();
                    var segStyle = GetSegmentStyle(count, h, segmentsCount);

                    BeginHorizontal();
                    if (idx.Type.EditorTypeColor(out var color)) {
                        SelectableLabel(idx.Name, Ui.LabelStyleThemeLeftColor(color), Ui.WidthLine(200));
                    } else {
                        SelectableLabel(idx.Name, Ui.WidthLine(200));
                    }

                    Ui.DrawSeparator();
                    LabelField(count.ToString(), Ui.LabelStyleThemeCenter, Ui.WidthLine(90));
                    Ui.DrawSeparator();
                    if (withCapacity) {
                        LabelField(h.CalculateCapacity().ToString(), Ui.LabelStyleThemeCenter, Ui.WidthLine(90));
                    } else {
                        LabelField("N/A", Ui.LabelStyleGreyCenter, Ui.WidthLine(90));
                    }

                    Ui.DrawSeparator();
                    if (drawCount > 0) {
                        DrawSegmentCell(h, drawCount, segStyle);
                    }
                    EndHorizontal();
                } else if (_showNotRegistered) {
                    BeginHorizontal();
                    SelectableLabel(idx.Name, Ui.WidthLine(200));
                    Ui.DrawSeparator();
                    LabelField("N/A", Ui.LabelStyleGreyCenter, Ui.WidthLine(90));
                    Ui.DrawSeparator();
                    LabelField("N/A", Ui.LabelStyleGreyCenter, Ui.WidthLine(90));
                    Ui.DrawSeparator();
                    EndHorizontal();
                }
            }

            Space(20);
        }

        private void DrawEntityTypeStats() {
            if (registeredEntityTypes.Count == 0) return;

            _foldoutEntityTypes = EditorGUILayout.Foldout(_foldoutEntityTypes, "Entity Types:", true, Ui.FoldoutStyleTheme);
            if (!_foldoutEntityTypes) {
                Space(20);
                return;
            }

            BeginHorizontal();
            SelectableLabel("Name", Ui.LabelStyleThemeCenter, Ui.WidthLine(200));
            Ui.DrawSeparator();
            SelectableLabel("Count", Ui.LabelStyleThemeCenter, Ui.WidthLine(90));
            Ui.DrawSeparator();
            SelectableLabel("Capacity", Ui.LabelStyleThemeCenter, Ui.WidthLine(90));
            Ui.DrawSeparator();
            EndHorizontal();

            for (var i = 0; i < registeredEntityTypes.Count; i++) {
                var meta = registeredEntityTypes[i];
                BeginHorizontal();
                if (meta.Type.EditorTypeColor(out var color)) {
                    SelectableLabel(meta.Name, Ui.LabelStyleThemeLeftColor(color), Ui.WidthLine(200));
                } else {
                    SelectableLabel(meta.Name, Ui.WidthLine(200));
                }
                Ui.DrawSeparator();
                LabelField(_handle.CalculateEntitiesCountByType(meta.Id).ToString(), Ui.LabelStyleThemeCenter, Ui.WidthLine(90));
                Ui.DrawSeparator();
                LabelField(_handle.CalculateEntitiesCapacityByType(meta.Id).ToString(), Ui.LabelStyleThemeCenter, Ui.WidthLine(90));
                Ui.DrawSeparator();
                EndHorizontal();
            }

            Space(20);
        }

        private void DrawWorldStats() {
            BeginHorizontal();
            SelectableLabel("World", Ui.LabelStyleThemeCenter, Ui.WidthLine(200));
            Ui.DrawSeparator();
            SelectableLabel("Active Entities", Ui.LabelStyleThemeCenter, Ui.WidthLine(90));
            Ui.DrawSeparator();
            SelectableLabel("Capacity Entities", Ui.LabelStyleThemeCenter, Ui.WidthLine(90));
            Ui.DrawSeparator();
            EndHorizontal();

            Ui.DrawHorizontalSeparator(420f);

            BeginHorizontal();
            LabelField(string.Empty, Ui.LabelStyleThemeCenter, Ui.WidthLine(200));
            Ui.DrawSeparator();
            LabelField(World<TWorld>.CalculateEntitiesCount().ToString(), Ui.LabelStyleThemeCenter, Ui.WidthLine(90));
            Ui.DrawSeparator();
            LabelField(World<TWorld>.CalculateEntitiesCapacity().ToString(), Ui.LabelStyleThemeCenter, Ui.WidthLine(90));
            Ui.DrawSeparator();
            EndHorizontal();

            Space(10);
        }

        private void DrawEventsPoolsStats(int[] order) {
            _foldoutEvents = EditorGUILayout.Foldout(_foldoutEvents, "Events:", true, Ui.FoldoutStyleTheme);
            if (!_foldoutEvents) {
                Space(20);
                return;
            }

            BeginHorizontal();
            SelectableLabel("Name", Ui.LabelStyleThemeCenter, Ui.WidthLine(200));
            Ui.DrawSeparator();
            SelectableLabel("Count", Ui.LabelStyleThemeCenter, Ui.WidthLine(90));
            Ui.DrawSeparator();
            SelectableLabel("Capacity", Ui.LabelStyleThemeCenter, Ui.WidthLine(90));
            Ui.DrawSeparator();
            SelectableLabel("Receivers", Ui.LabelStyleThemeCenter, Ui.WidthLine(90));
            Ui.DrawSeparator();
            EndHorizontal();

            for (var o = 0; o < order.Length; o++) {
                var i = order[o];
                var idx = _worldMeta.Events[i];
                var handle = eventHandles[i];
                if (handle.HasValue) {
                    BeginHorizontal();
                    if (idx.Type.EditorTypeColor(out var color)) {
                        SelectableLabel(idx.Name, Ui.LabelStyleThemeLeftColor(color), Ui.WidthLine(200));
                    } else {
                        SelectableLabel(idx.Name, Ui.WidthLine(200));
                    }
                    Ui.DrawSeparator();
                    LabelField(handle.Value.NotDeletedCount().ToString(), Ui.LabelStyleThemeCenter, Ui.WidthLine(90));
                    Ui.DrawSeparator();
                    LabelField(handle.Value.Capacity().ToString(), Ui.LabelStyleThemeCenter, Ui.WidthLine(90));
                    Ui.DrawSeparator();
                    LabelField(handle.Value.ReceiversCount().ToString(), Ui.LabelStyleThemeCenter, Ui.WidthLine(90));
                    Ui.DrawSeparator();
                    EndHorizontal();
                } else if (_showNotRegistered) {
                    BeginHorizontal();
                    SelectableLabel(idx.Name, Ui.WidthLine(200));
                    Ui.DrawSeparator();
                    LabelField("N/A", Ui.LabelStyleGreyCenter, Ui.WidthLine(90));
                    Ui.DrawSeparator();
                    LabelField("N/A", Ui.LabelStyleGreyCenter, Ui.WidthLine(90));
                    Ui.DrawSeparator();
                    LabelField("N/A", Ui.LabelStyleGreyCenter, Ui.WidthLine(90));
                    Ui.DrawSeparator();
                    EndHorizontal();
                }
            }

            Space(20);
        }

        public static UnityEngine.Font GetFont()
        {
            switch (Application.platform)
            {
                case RuntimePlatform.WindowsEditor:
                    return Font.CreateDynamicFontFromOSFont("Consolas", 11);
                case RuntimePlatform.OSXEditor:
                    return Font.CreateDynamicFontFromOSFont("Menlo", 11);
                default:
                    return null; // default unity font
            }
        }
    }
}
#endif