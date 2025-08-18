#if !FFS_ECS_DISABLE_EVENTS
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static UnityEditor.EditorGUILayout;
using Object = UnityEngine.Object;

namespace FFS.Libraries.StaticEcs.Unity.Editor {
    
    public class StaticEcsViewEventsTab : IStaticEcsViewTab {
        internal enum TabType: byte {
            Table,
            EventBuilder
        }
        
        private static readonly TabType[] _tabs = { TabType.Table, TabType.EventBuilder };
        private static readonly string[] _tabsNames = { "Table", "Event builder" };
        internal TabType SelectedTab;
        
        private readonly Dictionary<Type, EventsDrawer> _drawersByWorldTypeType = new();
        private EventsDrawer _currentDrawer;
        
        public string Name() => "Events";
        public void Init() {}

        public void Draw(StaticEcsView view) {
            Ui.DrawToolbar(_tabs, ref SelectedTab, type => _tabsNames[(int) type]);
            _currentDrawer.Draw();
        }
        public void Destroy() {}

        public void OnWorldChanged(AbstractWorldData newWorldData) {
            if (!_drawersByWorldTypeType.ContainsKey(newWorldData.WorldTypeType)) {
                _drawersByWorldTypeType[newWorldData.WorldTypeType] =  new EventsDrawer(this, newWorldData);
            }
            
            _currentDrawer = _drawersByWorldTypeType[newWorldData.WorldTypeType];
            _drawersByWorldTypeType[newWorldData.WorldTypeType] = _currentDrawer;
        }
    }

    public class EventsDrawer {
        private const float _maxWidth = 1056f;
        
        private Vector2 horizontalScroll = Vector2.zero;
        private Vector2 verticalScroll = Vector2.zero;
        
        private readonly StaticEcsViewEventsTab _parent;
        private readonly AbstractWorldData _worldData;
        private readonly EditorEventDataMetaByWorld[] _eventsMeta;
        private readonly Dictionary<Type, EditorEventDataMetaByWorld> _eventsByType = new();

        private readonly StaticEcsEventProvider _builder;
        private bool _showAfterBuild;

        private PageRingBuffer<EventData> events;
        private PageRingBuffer<EventData> filteredEvents;
        private PageView<EventData> currentPage;
        private PageView<EventData> currentFilteredPage;
        private bool Latest;
        
        // filter
        private readonly List<EditorEventDataMetaByWorld> _filterTypes = new();

        internal EventsDrawer(StaticEcsViewEventsTab parent, AbstractWorldData worldData) {
            _parent = parent;
            _worldData = worldData;
            _eventsMeta = new EditorEventDataMetaByWorld[MetaData.Events.Count];
            for (var i = 0; i < MetaData.Events.Count; i++) {
                _eventsMeta[i] = new EditorEventDataMetaByWorld(MetaData.Events[i]);
                _eventsByType[_eventsMeta[i].Type] = _eventsMeta[i];
            }
            _builder = CreateEventView();
            events = _worldData.Events;
            filteredEvents = new PageRingBuffer<EventData>(events.PageCount, events.PageSize);
            currentPage = events.GetPageView(0);
            currentFilteredPage = filteredEvents.GetPageView(0);
            Latest = true;
            events.SetOnPush((ref EventData item) => {
                if (_filterTypes.Count > 0 && _filterTypes.Contains(_eventsByType[item.TypeIdx.Type])) {
                    filteredEvents.Push(item);
                }
            });
            events.SetOnChange((ref EventData item) => {
                if (_filterTypes.Count > 0 && _filterTypes.Contains(_eventsByType[item.TypeIdx.Type])) {
                    filteredEvents.Change(item, (EventData template, ref EventData data) => {
                        data = template;
                    });
                }
            });
        }

        internal void Draw() {
            switch (_parent.SelectedTab) {
                case StaticEcsViewEventsTab.TabType.Table:
                    if (_filterTypes.Count > 0) {
                        DrawFilter(ref currentFilteredPage);
                    } else {
                        DrawFilter(ref currentPage);
                    }
                    DrawTable();
                    break;
                case StaticEcsViewEventsTab.TabType.EventBuilder:
                    GUILayout.BeginVertical(Ui.MaxWidth600);
                    LabelField("Build settings:", Ui.WidthLine(90));
                    _showAfterBuild = Toggle("Show after build", _showAfterBuild, Ui.WidthLine(90));
                    GUILayout.EndVertical();
                    Space(10);

                    Drawer.DrawEvent(_builder, DrawMode.Builder, provider => {
                        provider.SendEvent();
                        if (_showAfterBuild) {
                            EventInspectorWindow.ShowWindowForEvent(_worldData.World, in _builder.RuntimeEvent, _builder.EventCache);
                        }
                        _builder.RuntimeEvent = RuntimeEvent.Empty;
                        _builder.EventCache = null;
                    });
                    break;
            }
        }

        private void DrawFilter<T>(ref PageView<T> pageView) where T : IEquatable<T> {
            Space(10);
            
            BeginHorizontal();
            {
                if (!pageView.IsActual) {
                    pageView.MoveToNewer();
                }
                
                Latest = GUILayout.Toggle(Latest, "| <<<", Ui.ButtonStyleTheme, Ui.Width(60));
                while (Latest && pageView.HasNewer) {
                    pageView.MoveToNewer();
                }

                using (Ui.EnabledScopeVal(pageView.HasNewer)) {
                    if (GUILayout.Button("<-", Ui.ButtonStyleTheme, Ui.Width(60))) {
                        pageView.MoveToNewer();
                    }
                }

                using (Ui.EnabledScopeVal(pageView.HasOlder)) {
                    if (GUILayout.Button("->", Ui.ButtonStyleTheme, Ui.Width(60))) {
                        Latest = false;
                        pageView.MoveToOlder();
                    }
                }
                
                using (Ui.EnabledScopeVal(pageView.HasOlder)) {
                    if (GUILayout.Button(">>> |", Ui.ButtonStyleTheme, Ui.Width(60))) {
                        Latest = false;
                        while (pageView.HasOlder) {
                            pageView.MoveToOlder();
                        }
                    }
                }
                
                LabelField(GUIContent.none, Ui.WidthLine(20));
                if (Ui.PlusButton) {
                    var menu = new GenericMenu();
                    foreach (var meta in _eventsMeta) {
                        if (_filterTypes.Contains(meta)) {
                            continue;
                        }

                        menu.AddItem(new GUIContent(meta.Name), false, () => {
                            _filterTypes.Add(meta);
                            UpdateFilteredPage();
                        });
                    }

                    menu.ShowAsContext();
                }

                LabelField("Filter:", Ui.WidthLine(60));

                var deleted = false;
                for (var i = 0; i < _filterTypes.Count;) {
                    var meta = _filterTypes[i];
                    SelectableLabel(meta.Name, Ui.LabelStyleThemeCenter, meta.Layout);
                    if (Ui.TrashButton) {
                        _filterTypes.RemoveAt(i);
                        deleted = true;
                    } else {
                        i++;
                    }
                }

                if (deleted) {
                    UpdateFilteredPage();
                }
            }
            EndHorizontal();
            Space(20);
        }

        private void UpdateFilteredPage() {
            filteredEvents.Reset();
            var page = events.GetPageView(0);
            Fill();
            while (page.HasNewer) {
                page.MoveToNewer();
                Fill();
            }
            currentFilteredPage = filteredEvents.GetPageView(0);

            return;

            void Fill() {
                for (var i = 0; i < page.Count; i++) {
                    ref var item = ref page[i];
                    if (_filterTypes.Count > 0 && _filterTypes.Contains(_eventsByType[item.TypeIdx.Type])) {
                        filteredEvents.Push(item);
                    }
                }
            }
        }

        private void DrawTable() {
            horizontalScroll = GUILayout.BeginScrollView(horizontalScroll);
            DrawHeaders();
            verticalScroll = GUILayout.BeginScrollView(verticalScroll);

            var count = DrawPage();
            while (currentPage.PageSize - count > 0) {
                DrawFakeRow();
                count++;
            }

            GUILayout.EndScrollView();
            GUILayout.EndScrollView();
        }

        private int DrawPage() {
            var count = 0;
            if (_filterTypes.Count == 0) {
                foreach (ref var val in currentPage) {
                    count++;
                    DrawRow(ref val);
                }
            } else {
                foreach (ref var val in currentFilteredPage) {
                    count++;
                    DrawRow(ref val);
                }
            }

            return count;
        }

        private void DrawRow(ref EventData val) {
            var meta = _eventsByType[val.TypeIdx.Type];

            var style = val.Status switch {
                Status.Read       => Ui.LabelStyleGreyCenter,
                Status.Suppressed => Ui.LabelStyleYellowCenter,
                var _             => Ui.LabelStyleThemeCenter
            };

            BeginHorizontal();
            {
                BeginHorizontal(Ui.WidthLine(50));
                DrawViewEventButton(ref val);
                DrawDeleteEventButton(ref val);
                EndHorizontal();

                Ui.DrawSeparator();
                IntField(val.ReceivedIdx, style, Ui.WidthLine(60));
                Ui.DrawSeparator();

                _worldData.World.Events().TryGetPool(val.TypeIdx.Type, out var pool);

                SelectableLabel(val.TypeIdx.Type.EditorTypeName(), val.TypeIdx.Type.EditorTypeColor(out var color) ? Ui.LabelStyleThemeCenterColor(color) : style, Ui.WidthLine(200));
                Ui.DrawSeparator();
                var e = val.CachedData ?? pool.GetRaw(val.InternalIdx);

                if (MetaData.Inspectors.TryGetValue(meta.Type, out var inspector)) {
                    inspector.DrawTableValue(e, Ui.LabelStyleThemeCenter, Ui.WidthLine(600));
                } else {
                    if (meta.TryGetTableField(out var field)) {
                        Drawer.DrawField(e, field, style, Ui.WidthLine(600));
                    } else if (meta.TryGetTableProperty(out var property)) {
                        Drawer.DrawProperty(e, property, style, Ui.WidthLine(600));
                    } else {
                        LabelField("✔", style, Ui.WidthLine(600));
                    }
                }

                Ui.DrawSeparator();
                SelectableLabel(Ui.IntToStringD6(val.Status is Status.Read or Status.Suppressed ? 0 : pool.UnreadCount(val.InternalIdx)).simple, style, Ui.WidthLine(60));
                Ui.DrawSeparator();
            }
            EndHorizontal();
            Ui.DrawHorizontalSeparator(_maxWidth);
        }

        private void DrawFakeRow() {
                var style = Ui.LabelStyleGreyCenter;

                BeginHorizontal();
                {
                    using (Ui.DisabledScope) {
                        BeginHorizontal(Ui.WidthLine(50));
                        LabelField(GUIContent.none, Ui.Width(10));
                        _ = Ui.ViewButtonExpand;
                        LabelField(GUIContent.none, Ui.Width(10));
                        _ = Ui.TrashButtonExpand;
                        EndHorizontal();
                    }
                    Ui.DrawSeparator();
                    LabelField("---", style, Ui.WidthLine(60));
                    Ui.DrawSeparator();
                    LabelField("---", style, Ui.WidthLine(200));
                    Ui.DrawSeparator();
                    LabelField("---", style, Ui.WidthLine(600));
                    Ui.DrawSeparator();
                    LabelField("---", style, Ui.WidthLine(60));
                    Ui.DrawSeparator();
                }
                EndHorizontal();
                Ui.DrawHorizontalSeparator(_maxWidth);
        }

        private void DrawDeleteEventButton(ref EventData data) {
            LabelField(GUIContent.none, Ui.Width(10));
            if (Ui.TrashButtonExpand) {
                if (_worldData.World.Events().TryGetPool(data.TypeIdx.Type, out var pool)) {
                    pool.Del(data.InternalIdx);
                }
            }
        }

        private void DrawViewEventButton(ref EventData data) {
            LabelField(GUIContent.none, Ui.Width(10));
            if (Ui.ViewButtonExpand) {
                EventInspectorWindow.ShowWindowForEvent(_worldData.World, in data);
            }
        }

        private void DrawHeaders() {
            BeginHorizontal();
            {
                LabelField(GUIContent.none, Ui.WidthLine(63));
                Ui.DrawSeparator();
                SelectableLabel("Counter", Ui.LabelStyleThemeCenter, Ui.WidthLine(60));
                Ui.DrawSeparator();
                SelectableLabel("Event type", Ui.LabelStyleThemeCenter, Ui.WidthLine(200));
                Ui.DrawSeparator();
                SelectableLabel("Data", Ui.LabelStyleThemeCenter, Ui.WidthLine(600));
                Ui.DrawSeparator();
                SelectableLabel("Unread", Ui.LabelStyleThemeCenter, Ui.WidthLine(60));
                Ui.DrawSeparator();
            }
            EndHorizontal();
            Ui.DrawHorizontalSeparator(_maxWidth);
        }

        private StaticEcsEventProvider CreateEventView() {
            var go = new GameObject($"StaticEcsEventProvider") {
                hideFlags = HideFlags.NotEditable,
            };
            Object.DontDestroyOnLoad(go);
            var view = go.AddComponent<StaticEcsEventProvider>();
            view.UsageType = UsageType.Manual;
            view.OnCreateType = OnCreateType.None;
            view.WorldTypeName = _worldData.WorldTypeTypeFullName;
            view.WorldEditorName = _worldData.worldEditorName;
            return view;
        }
    }

    public class EditorEventDataMetaByWorld : EditorEventDataMeta {

        public EditorEventDataMetaByWorld(EditorEventDataMeta meta)
            : base(meta.Type, meta.Name, meta.FullName, meta.Width, meta.Layout, meta.LayoutWithOffset, meta.FieldInfo, meta.PropertyInfo) {
        }
    }
}
#endif
