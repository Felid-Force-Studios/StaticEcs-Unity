#if ((DEBUG || FFS_ECS_ENABLE_DEBUG) && !FFS_ECS_DISABLE_DEBUG)
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static UnityEditor.EditorGUILayout;
using Object = UnityEngine.Object;

namespace FFS.Libraries.StaticEcs.Unity.Editor {
    public class StaticEcsViewEntitiesTab : IStaticEcsViewTab {
        internal enum TabType: byte {
            Table,
            EntityBuilder
        }
        
        private static readonly TabType[] _tabs = { TabType.Table, TabType.EntityBuilder };
        private static readonly string[] _tabsNames = { "Table", "Entity builder" };
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

        private EditorEntityDataMetaByWorld _sortIdx;
        private readonly List<uint> _tempEntities = new();
        private readonly List<uint> _pinedEntities = new();
        
        private bool _gidFilterActive;
        private int _gidFilterValue;

        private readonly StaticEcsEntityProvider _entityBuilder;
        
        private readonly StaticEcsViewEntitiesTab _parent;
        private readonly AbstractWorldData _worldData;


        internal EntitiesDrawer(StaticEcsViewEntitiesTab parent, AbstractWorldData worldData) {
            _worldData = worldData;
            _parent = parent;
            
            foreach (var val in MetaData.Components) {
                if (worldData.World.TryGetComponentsRawPool(val.Type, out var pool)) {
                    _components.Add(new EditorEntityDataMetaByWorld(val, pool, e => pool.Has(e)));
                }
            }

            #if !FFS_ECS_DISABLE_TAGS
            foreach (var val in MetaData.Tags) {
                if (worldData.World.TryGetTagsRawPool(val.Type, out var pool)) {
                    _tags.Add(new EditorEntityDataMetaByWorld(val, pool, e => pool.Has(e)));
                }
            }
            #endif
            
            ShowAllColumns();

            var go = new GameObject("StaticEcsEntityBuider") {
                hideFlags = HideFlags.NotEditable,
            };
            Object.DontDestroyOnLoad(go);
            _entityBuilder = go.AddComponent<StaticEcsEntityProvider>();
            _entityBuilder.UsageType = UsageType.Manual;
            _entityBuilder.OnCreateType = OnCreateType.None;
            _entityBuilder.WorldTypeName = _worldData.WorldTypeTypeFullName;
            _entityBuilder.WorldEditorName = _worldData.worldEditorName;
        }

        internal void DrawEntitiesData() {
            switch (_parent.SelectedTab) {
                case StaticEcsViewEntitiesTab.TabType.Table:
                    DrawEntitiesFilter();
                    DrawEntitiesTable();
                    break;
                case StaticEcsViewEntitiesTab.TabType.EntityBuilder:
                    var prefab = ObjectField("Prefab", _entityBuilder.Prefab, typeof(StaticEcsEntityProvider), true);
                    if (prefab != _entityBuilder.Prefab) {
                        _entityBuilder.Prefab = (StaticEcsEntityProvider) prefab;
                        EditorUtility.SetDirty(_entityBuilder);
                    }
                    Drawer.DrawEntity(_entityBuilder, DrawMode.Builder, provider => {
                        provider.CreateEntity();
                        EntityInspectorWindow.ShowWindowForEntity(provider.World, provider.Entity);
                        provider.Entity = null;
                        EditorUtility.SetDirty(provider);
                    }, provider => {
                        provider.Entity = null;
                        EditorUtility.SetDirty(provider);
                    });
                    break;
            }
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
        }

        private void ShowNoneColumns() {
            _componentsColumns.Clear();
            #if !FFS_ECS_DISABLE_TAGS
            _tagsColumns.Clear();
            #endif
        }

        internal void Destroy() {
            if (Application.isPlaying) {
                Object.Destroy(_entityBuilder);
            } else {
                Object.DestroyImmediate(_entityBuilder);
            }
        }

        private void DrawEntitiesTable() {
            horizontalScrollEntitiesPosition = GUILayout.BeginScrollView(horizontalScrollEntitiesPosition);
            DrawHeaders();
            DrawPined();

            verticalScrollEntitiesPosition = GUILayout.BeginScrollView(verticalScrollEntitiesPosition, Ui.Width(_maxWidth + 25f));
            _currentEntityCount = 0;
            if (_gidFilterActive) {
                if (_worldData.FindEntityByGid((uint) _gidFilterValue, out var entity)) {
                    DrawEntityRow(entity.GetId(), false, false);
                }
            } else if (IsFilterValid()) {
                _worldData.ForAll(MakeEcsWithFilter(), this);
            } else {
                for (var entIdx = _worldData.Capacity; entIdx > 0; entIdx--) { // TODO capacity?
                    if (!DrawEntityRow(entIdx - 1, true, false)) {
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

        public bool ForAll(uint entityId) {
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
   
            BeginHorizontal();
            {
                LabelField(GUIContent.none, Ui.WidthLine(96));
                Ui.DrawSeparator();
                SelectableLabel(EntityID, Ui.LabelStyleThemeCenter, Ui.WidthLine(60));
                Ui.DrawSeparator();

                #if !FFS_ECS_DISABLE_TAGS
                for (var i = 0; i < _tagsColumns.Count;) {
                    var idx = _tagsColumns[i];
                    SelectableLabel(idx.Name, idx.Type.EditorTypeColor(out var color) ? Ui.LabelStyleThemeCenterColor(color) : Ui.LabelStyleThemeCenter, idx.Layout);
                    DrawSortButton(idx);
                    DrawDeleteColumnButton(ref i, _tagsColumns);
                    Ui.DrawSeparator();
                }
                #endif
                DrawComponents(_componentsColumns);
            }
            EndHorizontal();
            Ui.DrawHorizontalSeparator(_maxWidth);
        }

        private void DrawComponents(List<EditorEntityDataMetaByWorld> columns) {
            const string DataOn = "☑";
            const string DataOff = "☐";
            
            for (var i = 0; i < columns.Count;) {
                var idx = columns[i];
                SelectableLabel(idx.Name, idx.Type.EditorTypeColor(out var color) ? Ui.LabelStyleThemeCenterColor(color) : Ui.LabelStyleThemeCenter, idx.Layout);

                if (idx.ShowTableData) {
                    if (GUILayout.Button(DataOn, Ui.ButtonIconStyleGreen, Ui.WidthLine(21))) {
                        idx.ShowTableData = false;
                    }
                } else {
                    if (GUILayout.Button(DataOff, Ui.ButtonIconStyleTheme, Ui.WidthLine(21))) {
                        idx.ShowTableData = true;
                    }
                }

                DrawSortButton(idx);
                DrawDeleteColumnButton(ref i, columns);
                Ui.DrawSeparator();
            }
        }

        private void DrawSortButton(EditorEntityDataMetaByWorld idx) {
            const string Label = "⇧";

            if (idx == _sortIdx) {
                if (GUILayout.Button(Label, Ui.ButtonIconStyleGreen, Ui.WidthLine(20))) {
                    _sortIdx = null;
                }
            } else {
                if (GUILayout.Button(Label, Ui.ButtonIconStyleTheme, Ui.WidthLine(20))) {
                    _sortIdx = idx;
                }
            }
        }

        private void DrawDeleteColumnButton<T>(ref int i, List<T> values) {
            const string Label = "✖";

            if (GUILayout.Button(Label, Ui.ButtonIconStyleTheme, Ui.WidthLine(20))) {
                values.RemoveAt(i);
            } else {
                i++;
            }
        }

        private bool DrawEntityRow(uint entIdx, bool sorted, bool pined) {
            if (_currentEntityCount >= _maxEntityResult && !pined) {
                return false;
            }

            if (!_worldData.IsActual(entIdx) || (!pined && _pinedEntities.Contains(entIdx))) {
                return true;
            }

            if (sorted && _sortIdx != null) {
                if (!_sortIdx.HasComponent(entIdx)) {
                    if (_tempEntities.Count < _maxEntityResult - _currentEntityCount) {
                        _tempEntities.Add(entIdx);
                    }

                    return true;
                }

                if (_currentEntityCount >= _sortIdx.Pool.CalculateCount() && _tempEntities.Count >= _maxEntityResult - _currentEntityCount) { // Todo CalculateCount slow
                    return false;
                }
            }

            _currentEntityCount++;
            BeginHorizontal();
            {
                BeginHorizontal(Ui.WidthLine(97));
                DrawViewEntityButton(entIdx);
                DrawPinEntityButton(entIdx, pined);
                if (DrawDeleteEntityButton(entIdx)) {
                    EndHorizontal();
                    EndHorizontal();
                    return true;
                }
                EndHorizontal();

                Ui.DrawSeparator();
                DrawEntityId(entIdx);
                DrawComponents(entIdx);
            }
            EndHorizontal();
            Ui.DrawHorizontalSeparator(_maxWidth);
            return true;
        }

        private void DrawComponents(uint entIdx) {
            _maxWidth = 180f;
            const float baseWidth = 16f;
            #if !FFS_ECS_DISABLE_TAGS
            DrawComponents(entIdx, _tagsColumns, 46f + baseWidth);
            #endif
            DrawComponents(entIdx, _componentsColumns, 68f + baseWidth);
        }

        private static void DrawEntityId(uint entIdx) {
            SelectableLabel(Ui.IntToStringD6((int) entIdx).d6, Ui.LabelStyleThemeCenter, Ui.WidthLine(60));
            Ui.DrawSeparator();
        }

        private bool DrawDeleteEntityButton(uint entIdx) {
            if (Ui.TrashButtonExpand) {
                _worldData.DestroyEntity(entIdx);
                return true;
            }

            return false;
        }

        private void DrawPinEntityButton(uint entIdx, bool pinned) {
            if (Ui.LockButtonExpand) {
                if (pinned) {
                    _pinedEntities.Remove(entIdx);
                } else {
                    _pinedEntities.Add(entIdx);
                }
            }
        }

        private void DrawViewEntityButton(uint entIdx) {
            LabelField(GUIContent.none, Ui.Width(10));
            if (Ui.ViewButtonExpand) {
                EntityInspectorWindow.ShowWindowForEntity(_worldData.World, _worldData.GetEntity(entIdx));
            }
        }

        private void DrawComponents(uint entIdx, List<EditorEntityDataMetaByWorld> types, float widthAdd) {
            const string HasComponent = "✔";

            foreach (var idx in types) {
                if (idx.HasComponent(entIdx)) {
                    var style = idx.CopmponentsPool != null && idx.CopmponentsPool.HasDisabled(entIdx) 
                        ? Ui.LabelStyleGreyCenter 
                        : Ui.LabelStyleThemeCenter;
                    if (idx.ShowTableData) {
                        if (MetaData.Inspectors.TryGetValue(idx.Type, out var inspector)) {
                            inspector.DrawTableValue(idx.Pool.GetRaw(entIdx), style, idx.LayoutWithOffset);
                        } else {
                            if (idx.TryGetTableField(out var field)) {
                                Drawer.DrawField(idx.Pool.GetRaw(entIdx), field, style, idx.LayoutWithOffset);
                            } else if (idx.TryGetTableProperty(out var property)) {
                                Drawer.DrawProperty(idx.Pool.GetRaw(entIdx), property, style, idx.LayoutWithOffset);
                            } else {
                                LabelField(HasComponent, style, idx.LayoutWithOffset);
                            }
                        }
                    } else {
                        LabelField(HasComponent, style, idx.LayoutWithOffset);
                    }
                } else {
                    LabelField(GUIContent.none, Ui.LabelStyleGreyCenter, idx.LayoutWithOffset);
                }

                Ui.DrawSeparator();
                _maxWidth += idx.Width + widthAdd;
            }
        }
    }

    public class EditorEntityDataMetaByWorld : EditorEntityDataMeta {
        public readonly IRawPool Pool;
        public readonly IRawComponentPool CopmponentsPool;
        public readonly IRawTagPool TagsPool;
        private readonly Func<uint, bool> _hasComponentFunc;
        public bool ShowTableData;

        public EditorEntityDataMetaByWorld(EditorEntityDataMeta meta, IRawPool pool, Func<uint, bool> hasComponentFunc)
            : base(meta.Type, meta.Name, meta.FullName, meta.Width, meta.Layout, meta.LayoutWithOffset, meta.FieldInfo, meta.PropertyInfo) {
            _hasComponentFunc = hasComponentFunc;
            ShowTableData = false;
            Pool = pool;
            if (Pool is IRawComponentPool componentPool) {
                CopmponentsPool = componentPool;
            }
            if (Pool is IRawTagPool tagPool) {
                TagsPool = tagPool;
            }
        }

        public bool HasComponent(uint entIdx) => _hasComponentFunc(entIdx);
    }
}
#endif