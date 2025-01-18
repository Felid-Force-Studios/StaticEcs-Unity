using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace FFS.Libraries.StaticEcs.Unity.Editor {
    public class StaticEcsViewEntitiesTab : IStaticEcsViewTab {
        internal enum TabType: byte {
            Table,
            Viewer,
            EntityBuilder
        }
        
        private static readonly TabType[] _tabs = { TabType.Table, TabType.Viewer, TabType.EntityBuilder };
        private static readonly string[] _tabsNames = { "Table", "Viewer", "Entity builder" };
        internal TabType SelectedTab;
        
        private readonly Dictionary<Type, EntitiesDrawer> _drawersByWorldTypeType = new();
        private EntitiesDrawer _currentDrawer;
        
        public string Name() => "Entities";
        
        public void Init() {}

        public void Draw(StaticEcsView view) {
            Ui.DrawToolbar(_tabs, ref SelectedTab, type => _tabsNames[(int) type]);
            _currentDrawer.DrawEntitiesData();
        }

        public void Destroy() {
            foreach (var drawer in _drawersByWorldTypeType) {
                drawer.Value.Destroy();
            }
        }

        public void OnWorldChanged(AbstractWorldData newWorldData) {
            if (!_drawersByWorldTypeType.ContainsKey(newWorldData.WorldTypeType)) {
                _drawersByWorldTypeType[newWorldData.WorldTypeType] =  new EntitiesDrawer(this, newWorldData);
            }
            
            _currentDrawer = _drawersByWorldTypeType[newWorldData.WorldTypeType];
            _drawersByWorldTypeType[newWorldData.WorldTypeType] = _currentDrawer;
        }
    }

    public partial class EntitiesDrawer : IForAll {
        private Vector2 verticalScrollEntitiesPosition = Vector2.zero;
        private Vector2 horizontalScrollEntitiesPosition = Vector2.zero;

        private float _maxWidth;
        
        private readonly List<EditorEntityDataMetaByWorld> _components = new();
        private readonly List<EditorEntityDataMetaByWorld> _componentsColumns = new();

        #if !FFS_ECS_DISABLE_TAGS
        private readonly List<EditorEntityDataMetaByWorld> _tags = new();
        private readonly List<EditorEntityDataMetaByWorld> _tagsColumns = new();
        #endif

        #if !FFS_ECS_DISABLE_MASKS
        private readonly List<EditorEntityDataMetaByWorld> _masks = new();
        private readonly List<EditorEntityDataMetaByWorld> _maskColumns = new();
        #endif

        private IRawPool _sortPool;
        private readonly List<int> _tempEntities = new();
        private readonly List<int> _pinedEntities = new();

        private readonly StaticEcsEntityProvider _entityBuilder;
        private bool _entityBuilderShowLeftTab = true;
        private bool _entityBuilderShowRightTab;
        private readonly StaticEcsEntityProvider _entityViewWindowLeft;
        private readonly StaticEcsEntityProvider _entityViewWindowRight;
        
        private readonly StaticEcsViewEntitiesTab _parent;
        private readonly AbstractWorldData _worldData;


        internal EntitiesDrawer(StaticEcsViewEntitiesTab parent, AbstractWorldData worldData) {
            _worldData = worldData;
            _parent = parent;
            foreach (var val in MetaData.Components) {
                if (worldData.World.TryGetComponentsRawPool(val.Type, out var pool)) {
                    _components.Add(new EditorEntityDataMetaByWorld(val, pool));
                }
            }

            #if !FFS_ECS_DISABLE_TAGS
            foreach (var val in MetaData.Tags) {
                if (worldData.World.TryGetTagsRawPool(val.Type, out var pool)) {
                    _tags.Add(new EditorEntityDataMetaByWorld(val, pool));
                }
            }
            #endif
            
            #if !FFS_ECS_DISABLE_MASKS
            foreach (var val in MetaData.Masks) {
                if (worldData.World.TryGetMasksRawPool(val.Type, out var pool)) {
                    _masks.Add(new EditorEntityDataMetaByWorld(val, pool));
                }
            }
            #endif
            
            ShowAllColumns();

            _entityViewWindowLeft = CreateStaticEcsEntityDebugView();
            _entityViewWindowRight = CreateStaticEcsEntityDebugView();
            _entityBuilder = CreateStaticEcsEntityDebugView();
        }

        internal void DrawEntitiesData() {
            switch (_parent.SelectedTab) {
                case StaticEcsViewEntitiesTab.TabType.Table:
                    StaticEcsView.DrawWorldSelector();
                    DrawEntitiesFilter();
                    DrawEntitiesTable();
                    break;
                case StaticEcsViewEntitiesTab.TabType.Viewer:
                    GUILayout.BeginHorizontal(Ui.MaxWidth1200);
                    if (!_entityViewWindowLeft.EntityIsActual()) {
                        GUILayout.BeginVertical(Ui.MaxWidth600);
                        EditorGUILayout.HelpBox("Select an entity from the [Table] or build from [Entity builder]", MessageType.Info, true);
                        GUILayout.EndVertical();
                    } else {
                        Drawer.DrawEntity(_entityViewWindowLeft, true, _ => { });
                    }

                    if (_entityViewWindowRight.EntityIsActual()) {
                        Ui.DrawVerticalSeparator();
                        Drawer.DrawEntity(_entityViewWindowRight, true, _ => { });
                    }

                    GUILayout.EndHorizontal();
                    break;
                case StaticEcsViewEntitiesTab.TabType.EntityBuilder:
                    StaticEcsView.DrawWorldSelector();
                    GUILayout.BeginVertical(Ui.MaxWidth600);
                    EditorGUILayout.LabelField("Build settings:", Ui.WidthLine(90));
                    _entityBuilderShowLeftTab = EditorGUILayout.Toggle("Show at left tab", _entityBuilderShowLeftTab, Ui.WidthLine(90));
                    if (_entityBuilderShowLeftTab) _entityBuilderShowRightTab = false;
                    _entityBuilderShowRightTab = EditorGUILayout.Toggle("Show at right tab", _entityBuilderShowRightTab, Ui.WidthLine(90));
                    if (_entityBuilderShowRightTab) _entityBuilderShowLeftTab = false;
                    GUILayout.EndVertical();
                    EditorGUILayout.Space(10);

                    Drawer.DrawEntity(_entityBuilder, true, provider => {
                        provider.CreateEntity();
                        if (_entityBuilderShowLeftTab) {
                            _parent.SelectedTab = StaticEcsViewEntitiesTab.TabType.Viewer;
                            _entityViewWindowLeft.Entity = provider.Entity;
                        }

                        if (_entityBuilderShowRightTab) {
                            _parent.SelectedTab = StaticEcsViewEntitiesTab.TabType.Viewer;
                            _entityViewWindowRight.Entity = provider.Entity;
                        }

                        provider.Entity = null;
                    });
                    break;
            }
        }

        private StaticEcsEntityProvider CreateStaticEcsEntityDebugView() {
            var viewLeft = new GameObject("StaticEcsEntityDebugView") {
                hideFlags = HideFlags.NotEditable,
            };
            Object.DontDestroyOnLoad(viewLeft);
            var entityDebugViewLeft = viewLeft.AddComponent<StaticEcsEntityProvider>();
            entityDebugViewLeft.UsageType = UsageType.Manual;
            entityDebugViewLeft.OnCreateType = OnCreateType.None;
            entityDebugViewLeft.WorldTypeName = _worldData.WorldTypeTypeFullName;
            entityDebugViewLeft.WorldEditorName = _worldData.worldEditorName;
            return entityDebugViewLeft;
        }

        private void ShowAllColumns() {
            _componentsColumns.Clear();
            foreach (var val in _components) {
                _componentsColumns.Add(val);
            }

            #if !FFS_ECS_DISABLE_TAGS
            _tagsColumns.Clear();
            foreach (var val in _tags) {
                _tagsColumns.Add(val);
            }
            #endif

            #if !FFS_ECS_DISABLE_MASKS
            _maskColumns.Clear();
            foreach (var val in _masks) {
                _maskColumns.Add(val);
            }
            #endif
        }

        private void ShowNoneColumns() {
            _componentsColumns.Clear();
            #if !FFS_ECS_DISABLE_TAGS
            _tagsColumns.Clear();
            #endif
            #if !FFS_ECS_DISABLE_MASKS
            _maskColumns.Clear();
            #endif
        }

        internal void Destroy() {
            if (Application.isPlaying) {
                Object.Destroy(_entityViewWindowLeft);
                Object.Destroy(_entityViewWindowRight);
            } else {
                Object.DestroyImmediate(_entityViewWindowLeft);
                Object.DestroyImmediate(_entityViewWindowRight);
            }
        }

        private void DrawEntitiesTable() {
            horizontalScrollEntitiesPosition = GUILayout.BeginScrollView(horizontalScrollEntitiesPosition);
            DrawHeaders();
            DrawPined();

            verticalScrollEntitiesPosition = GUILayout.BeginScrollView(verticalScrollEntitiesPosition, Ui.Width(_maxWidth + 10f));
            _currentEntityCount = 0;
            if (IsFilterValid()) {
                _worldData.ForAll(MakeEcsWithFilter(), this);
            } else {
                for (var entIdx = _worldData.Count - 1; entIdx >= 0; entIdx--) {
                    if (!DrawEntityRow(entIdx, true, false)) {
                        break;
                    }
                }
            }

            foreach (var entIdx in _tempEntities) {
                if (!DrawEntityRow(entIdx, false, false)) {
                    break;
                }
            }

            _tempEntities.Clear();

            GUILayout.EndScrollView();
            GUILayout.EndScrollView();
        }

        public bool ForAll(int entityId) {
            return DrawEntityRow(entityId, true, false);
        }

        private void DrawPined() {
            var count = _pinedEntities.Count;
            for (var i = count - 1; i >= 0; i--) {
                var entIdx = _pinedEntities[i];
                if (_worldData.IsActual(entIdx)) {
                    DrawEntityRow(entIdx, false, true);
                } else {
                    _pinedEntities.RemoveAt(i);
                }
            }

            if (_pinedEntities.Count > 0) {
                Ui.DrawHorizontalSeparator(_maxWidth);
            }
        }

        private void DrawHeaders() {
            const string EntityID = "Entity ID";
            const string DataOn = "☑";
            const string DataOff = "☐";

            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField(GUIContent.none, Ui.WidthLine(96));
                Ui.DrawSeparator();
                EditorGUILayout.SelectableLabel(EntityID, Ui.LabelStyleWhiteCenter, Ui.WidthLine(60));
                Ui.DrawSeparator();

                #if !FFS_ECS_DISABLE_TAGS
                for (var i = 0; i < _tagsColumns.Count;) {
                    var idx = _tagsColumns[i];
                    EditorGUILayout.SelectableLabel(idx.Name, Ui.LabelStyleWhiteCenter, idx.Layout);
                    DrawSortButton(idx.Pool);
                    DrawDeleteColumnButton(ref i, _tagsColumns);
                    Ui.DrawSeparator();
                }
                #endif
                
                #if !FFS_ECS_DISABLE_MASKS
                for (var i = 0; i < _maskColumns.Count;) {
                    var idx = _maskColumns[i];
                    EditorGUILayout.SelectableLabel(idx.Name, Ui.LabelStyleWhiteCenter, idx.Layout);

                    DrawSortButton(idx.Pool);
                    DrawDeleteColumnButton(ref i, _maskColumns);
                    Ui.DrawSeparator();
                }
                #endif

                for (var i = 0; i < _componentsColumns.Count;) {
                    var idx = _componentsColumns[i];
                    EditorGUILayout.SelectableLabel(idx.Name, Ui.LabelStyleWhiteCenter, idx.Layout);

                    if (idx.ShowTableData) {
                        if (GUILayout.Button(DataOn, Ui.ButtonStyleGreen, Ui.WidthLine(21))) {
                            idx.ShowTableData = false;
                        }
                    } else {
                        if (GUILayout.Button(DataOff, Ui.ButtonStyleWhite, Ui.WidthLine(21))) {
                            idx.ShowTableData = true;
                        }
                    }

                    DrawSortButton(idx.Pool);
                    DrawDeleteColumnButton(ref i, _componentsColumns);
                    Ui.DrawSeparator();
                }
            }
            EditorGUILayout.EndHorizontal();
            Ui.DrawHorizontalSeparator(_maxWidth);
        }

        private void DrawSortButton(IRawPool pool) {
            const string Label = "⇧";

            if (pool == _sortPool) {
                if (GUILayout.Button(Label, Ui.ButtonStyleGreen, Ui.WidthLine(20))) {
                    _sortPool = null;
                }
            } else {
                if (GUILayout.Button(Label, Ui.ButtonStyleWhite, Ui.WidthLine(20))) {
                    _sortPool = pool;
                }
            }
        }

        private void DrawDeleteColumnButton<T>(ref int i, List<T> values) {
            const string Label = "✖";

            if (GUILayout.Button(Label, Ui.ButtonStyleWhite, Ui.WidthLine(20))) {
                values.RemoveAt(i);
            } else {
                i++;
            }
        }

        private bool DrawEntityRow(int entIdx, bool sorted, bool pined) {
            if (_currentEntityCount >= _maxEntityResult && !pined) {
                return false;
            }

            if (!_worldData.IsActual(entIdx) || (!pined && _pinedEntities.Contains(entIdx))) {
                return true;
            }

            if (sorted && _sortPool != null) {
                if (!_sortPool.Has(entIdx)) {
                    if (_tempEntities.Count < _maxEntityResult - _currentEntityCount) {
                        _tempEntities.Add(entIdx);
                    }

                    return true;
                }

                if (_currentEntityCount >= _sortPool.Count() && _tempEntities.Count >= _maxEntityResult - _currentEntityCount) {
                    return false;
                }
            }

            _currentEntityCount++;
            EditorGUILayout.BeginHorizontal();
            {
                DrawViewEntityButton(entIdx);
                DrawPinEntityButton(entIdx, pined);
                if (DrawDeleteEntityButton(entIdx)) {
                    EditorGUILayout.EndHorizontal();
                    return true;
                }

                Ui.DrawSeparator();
                DrawEntityId(entIdx);
                DrawComponents(entIdx);
            }
            EditorGUILayout.EndHorizontal();
            Ui.DrawHorizontalSeparator(_maxWidth);
            return true;
        }

        private void DrawComponents(int entIdx) {
            _maxWidth = 180f;
            #if !FFS_ECS_DISABLE_TAGS
            DrawComponents(entIdx, _tagsColumns, 46f + 16f);
            #endif
            #if !FFS_ECS_DISABLE_MASKS
            DrawComponents(entIdx, _maskColumns, 46f + 16f);
            #endif
            DrawComponents(entIdx, _componentsColumns, 70f + 16f);
        }

        private static void DrawEntityId(int entIdx) {
            EditorGUILayout.SelectableLabel(Ui.IntToStringD6(entIdx).d6, Ui.LabelStyleWhiteCenter, Ui.WidthLine(60));
            Ui.DrawSeparator();
        }

        private bool DrawDeleteEntityButton(int entIdx) {
            if (GUILayout.Button(Ui.IconTrash, Ui.WidthLine(30))) {
                _worldData.DestroyEntity(entIdx);
                return true;
            }

            return false;
        }

        private void DrawPinEntityButton(int entIdx, bool pinned) {
            var pinedIcon = pinned
                ? Ui.IconLockOn
                : Ui.IconLockOff;

            if (GUILayout.Button(pinedIcon, Ui.Width30Height20)) {
                if (pinned) {
                    _pinedEntities.Remove(entIdx);
                } else {
                    _pinedEntities.Add(entIdx);
                }
            }
        }

        private void DrawViewEntityButton(int entIdx) {
            if (GUILayout.Button(Ui.IconView, Ui.WidthLine(30))) {
                var menu = new GenericMenu();
                menu.AddItem(new GUIContent("Left tab"), false, () => {
                    _entityViewWindowLeft.Entity = _worldData.GetEntity(entIdx);
                    _entityViewWindowLeft.WorldTypeName = _worldData.WorldTypeTypeFullName;
                    _entityViewWindowLeft.WorldEditorName = _worldData.worldEditorName;
                    _parent.SelectedTab = StaticEcsViewEntitiesTab.TabType.Viewer;
                });
                menu.AddItem(new GUIContent("Right tab"), false, () => {
                    _entityViewWindowRight.Entity = _worldData.GetEntity(entIdx);
                    _entityViewWindowRight.WorldTypeName = _worldData.WorldTypeTypeFullName;
                    _entityViewWindowRight.WorldEditorName = _worldData.worldEditorName;
                    _parent.SelectedTab = StaticEcsViewEntitiesTab.TabType.Viewer;
                });
                menu.ShowAsContext();
            }
        }

        private void DrawComponents(int entIdx, List<EditorEntityDataMetaByWorld> types, float widthAdd) {
            const string HasComponent = "✔";

            foreach (var idx in types) {
                if (idx.Pool.Has(entIdx)) {
                    if (idx.ShowTableData && idx.TryGetTableField(out var field)) {
                        Drawer.DrawField(idx.Pool.GetRaw(entIdx), field, Ui.LabelStyleWhiteCenter, idx.LayoutWithOffset);
                    } else if (idx.ShowTableData && idx.TryGetTableProperty(out var property)) {
                        Drawer.DrawProperty(idx.Pool.GetRaw(entIdx), property, Ui.LabelStyleWhiteCenter, idx.LayoutWithOffset);
                    } else {
                        EditorGUILayout.LabelField(HasComponent, Ui.LabelStyleWhiteCenter, idx.LayoutWithOffset);
                    }
                } else {
                    EditorGUILayout.LabelField(GUIContent.none, Ui.LabelStyleGreyCenter, idx.LayoutWithOffset);
                }

                Ui.DrawSeparator();
                _maxWidth += idx.Width + widthAdd;
            }
        }
    }

    public class EditorEntityDataMetaByWorld : EditorEntityDataMeta {
        public readonly IRawPool Pool;
        public bool ShowTableData;

        public EditorEntityDataMetaByWorld(EditorEntityDataMeta meta, IRawPool pool)
            : base(meta.Type, meta.Name, meta.FullName, meta.Width, meta.Layout, meta.LayoutWithOffset, meta.FieldInfo, meta.PropertyInfo) {
            ShowTableData = false;
            Pool = pool;
        }
    }
}