#if !FFS_ECS_DISABLE_EVENTS
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace FFS.Libraries.StaticEcs.Unity.Editor {
    
    public class StaticEcsViewEventsTab : IStaticEcsViewTab {
        internal enum TabType: byte {
            Table,
            Viewer,
            EventBuilder
        }
        
        private static readonly TabType[] _tabs = { TabType.Table, TabType.Viewer, TabType.EventBuilder };
        private static readonly string[] _tabsNames = { "Table", "Viewer", "Event builder" };
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
        private const float _maxWidth = 980;
        
        private Vector2 horizontalScroll = Vector2.zero;
        private Vector2 verticalScroll = Vector2.zero;
        
        private readonly StaticEcsViewEventsTab _parent;
        private readonly AbstractWorldData _worldData;
        private readonly EditorEventDataMetaByWorld[] _eventsMeta;
        private readonly Dictionary<Type, EditorEventDataMetaByWorld> _eventsByType = new();

        private readonly StaticEcsEventProvider _viewer;
        
        private readonly StaticEcsEventProvider _builder;
        private bool _showAfterBuild;

        private int _lastCount;
        
        // filter
        private readonly List<EditorEventDataMetaByWorld> _filterTypes = new();
        private bool _filterActive;

        internal EventsDrawer(StaticEcsViewEventsTab parent, AbstractWorldData worldData) {
            _parent = parent;
            _worldData = worldData;
            _eventsMeta = new EditorEventDataMetaByWorld[MetaData.Events.Count];
            for (var i = 0; i < MetaData.Events.Count; i++) {
                _eventsMeta[i] = new EditorEventDataMetaByWorld(MetaData.Events[i]);
                _eventsByType[_eventsMeta[i].Type] = _eventsMeta[i];
            }
            _viewer = CreateEventView();
            _builder = CreateEventView();
            _lastCount = _worldData.EventsCount;
        }

        internal void Draw() {
            switch (_parent.SelectedTab) {
                case StaticEcsViewEventsTab.TabType.Table:
                    StaticEcsView.DrawWorldSelector();
                    DrawFilter();
                    DrawTable();
                    break;
                case StaticEcsViewEventsTab.TabType.Viewer:
                    GUILayout.BeginHorizontal(Ui.MaxWidth600);
                    if (_viewer.RuntimeEvent.IsEmpty()) {
                        EditorGUILayout.HelpBox("Select an event from the [Table] or send from [Event builder]", MessageType.Info, true);
                    } else {
                        Drawer.DrawEvent(_viewer, true, _ => { }, provider => {
                            _builder.EventTemplate = provider.GetActualEvent(out var _);
                            _parent.SelectedTab = StaticEcsViewEventsTab.TabType.EventBuilder;
                        });
                    }
                    GUILayout.EndHorizontal();
                    break;
                case StaticEcsViewEventsTab.TabType.EventBuilder:
                    StaticEcsView.DrawWorldSelector();
                    GUILayout.BeginVertical(Ui.MaxWidth600);
                    EditorGUILayout.LabelField("Build settings:", Ui.WidthLine(90));
                    _showAfterBuild = EditorGUILayout.Toggle("Show after build", _showAfterBuild, Ui.WidthLine(90));
                    GUILayout.EndVertical();
                    EditorGUILayout.Space(10);

                    Drawer.DrawEvent(_builder, true, provider => {
                        provider.SendEvent();
                        if (_showAfterBuild) {
                            _parent.SelectedTab = StaticEcsViewEventsTab.TabType.Viewer;
                            _viewer.RuntimeEvent = _builder.RuntimeEvent;
                            _viewer.EventCache = _builder.EventCache;
                        }
                        _builder.RuntimeEvent = RuntimeEvent.Empty;
                        _builder.EventCache = null;
                    });
                    break;
            }
        }

        private void DrawFilter() {
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField("Filter:", Ui.WidthLine(90));
                _filterActive = EditorGUILayout.Toggle(_filterActive);
            }
            EditorGUILayout.EndHorizontal();
            if (_filterActive) {
                EditorGUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button("+", Ui.ButtonStyleWhite, Ui.WidthLine(20))) {
                        var menu = new GenericMenu();
                        foreach (var meta in _eventsMeta) {
                            if (_filterTypes.Contains(meta)) {
                                continue;
                            }

                            menu.AddItem(new GUIContent(meta.Name), false, () => { _filterTypes.Add(meta); });
                        }

                        menu.ShowAsContext();
                    }

                    EditorGUILayout.LabelField("Type:", Ui.WidthLine(90));

                    for (var i = 0; i < _filterTypes.Count;) {
                        var meta = _filterTypes[i];
                        EditorGUILayout.SelectableLabel(meta.Name, Ui.LabelStyleWhiteCenter, meta.Layout);
                        if (GUILayout.Button(Ui.IconTrash, Ui.WidthLine(30))) {
                            _filterTypes.RemoveAt(i);
                        } else {
                            i++;
                        }
                    }
                }
                EditorGUILayout.EndHorizontal();
            }


            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField("Show data:", Ui.WidthLine(90));

                if (GUILayout.Button("All", Ui.ButtonStyleWhite, Ui.WidthLine(60))) {
                    foreach (var meta in _eventsMeta) {
                        meta.ShowTableData = true;
                    }
                }

                if (GUILayout.Button("None", Ui.ButtonStyleWhite, Ui.WidthLine(60))) {
                    foreach (var meta in _eventsMeta) {
                        meta.ShowTableData = false;
                    }
                }

                if (GUILayout.Button("+", Ui.ButtonStyleWhite, Ui.WidthLine(20))) {
                    var menu = new GenericMenu();
                    foreach (var meta in _eventsMeta) {
                        if (meta.ShowTableData) {
                            continue;
                        }

                        menu.AddItem(new GUIContent(meta.Name), false, () => { meta.ShowTableData = true; });
                    }

                    menu.ShowAsContext();
                }
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(10);
        }

        private void DrawTable() {
            EditorGUI.BeginChangeCheck();
            horizontalScroll = GUILayout.BeginScrollView(horizontalScroll);
            DrawHeaders();
            EditorGUI.BeginChangeCheck();
            var userScrollChanged = false;
            verticalScroll = GUILayout.BeginScrollView(verticalScroll);
            if (EditorGUI.EndChangeCheck()) {
                userScrollChanged = true;
            }

            for (var i = _worldData.EventsCount - 1; i >= _worldData.EventsStart; i--) {
                ref var val = ref _worldData.Events[i];
                var meta = _eventsByType[val.TypeIdx.Type];

                if (!_filterActive || _filterTypes.Count == 0 || _filterTypes.Contains(meta)) {
                    var style = val.Status switch {
                        Status.Read       => Ui.LabelStyleGreyCenter,
                        Status.Suppressed => Ui.LabelStyleYellowCenter,
                        var _             => Ui.LabelStyleWhiteCenter
                    };

                    EditorGUILayout.BeginHorizontal();
                    {
                        DrawViewEventButton(ref val);
                        DrawDeleteEventButton(ref val);
                        Ui.DrawSeparator();
                        
                        _worldData.World.Events().TryGetPool(val.TypeIdx.Type, out var pool);
                        
                        EditorGUILayout.SelectableLabel(val.TypeIdx.Type.EditorTypeName(), style, Ui.WidthLine(200));
                        Ui.DrawSeparator();
                        if (meta.ShowTableData) {
                            IEvent e;
                            if (val.CachedData != null) {
                                e = val.CachedData;
                            } else {
                                e = pool.GetRaw(val.InternalIdx);
                            }
                        
                            if (meta.TryGetTableField(out var field)) {
                                Drawer.DrawField(e, field, style, Ui.WidthLine(600));
                            } else if (meta.TryGetTableProperty(out var property)) {
                                Drawer.DrawProperty(e, property, style, Ui.WidthLine(600));
                            } else {
                                EditorGUILayout.LabelField("✔", style, Ui.WidthLine(600));
                            }
                        } else {
                            EditorGUILayout.LabelField("Hidden", style, Ui.WidthLine(600));
                        }
                        Ui.DrawSeparator();
                        EditorGUILayout.SelectableLabel(Ui.IntToStringD6(val.Status is Status.Read or Status.Suppressed ? 0 : pool.UnreadCount(val.InternalIdx)).simple, style, Ui.WidthLine(60));
                        Ui.DrawSeparator();
                    }
                    EditorGUILayout.EndHorizontal();
                    Ui.DrawHorizontalSeparator(_maxWidth);
                }
            }

            GUILayout.EndScrollView();
            GUILayout.EndScrollView();

            if (verticalScroll.y > 0 && !userScrollChanged) {
                verticalScroll.y += (EditorGUIUtility.singleLineHeight + 9f) * (_worldData.EventsCount - _lastCount);
            }
            _lastCount = _worldData.EventsCount;
        }

        private void DrawDeleteEventButton(ref EventData data) {
            if (GUILayout.Button(Ui.IconTrash, Ui.WidthLine(30))) {
                if (_worldData.World.Events().TryGetPool(data.TypeIdx.Type, out var pool)) {
                    pool.Del(data.InternalIdx);
                }
            }
        }

        private void DrawViewEventButton(ref EventData data) {
            if (GUILayout.Button(Ui.IconView, Ui.WidthLine(30))) {
                _viewer.RuntimeEvent = new RuntimeEvent {
                    InternalIdx = data.InternalIdx,
                    Type = data.TypeIdx.Type,
                    Version = data.Version
                };
                _viewer.EventCache = data.CachedData;
                _parent.SelectedTab = StaticEcsViewEventsTab.TabType.Viewer;
            }
        }

        private void DrawHeaders() {
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField(GUIContent.none, Ui.WidthLine(63));
                Ui.DrawSeparator();
                EditorGUILayout.SelectableLabel("Event type", Ui.LabelStyleWhiteCenter, Ui.WidthLine(200));
                Ui.DrawSeparator();
                EditorGUILayout.SelectableLabel("Data", Ui.LabelStyleWhiteCenter, Ui.WidthLine(600));
                Ui.DrawSeparator();
                EditorGUILayout.SelectableLabel("Unread", Ui.LabelStyleWhiteCenter, Ui.WidthLine(60));
                Ui.DrawSeparator();
            }
            EditorGUILayout.EndHorizontal();
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
        public bool ShowTableData;

        public EditorEventDataMetaByWorld(EditorEventDataMeta meta)
            : base(meta.Type, meta.Name, meta.FullName, meta.Width, meta.Layout, meta.LayoutWithOffset, meta.FieldInfo, meta.PropertyInfo) {
            ShowTableData = false;
        }
    }
}
#endif
