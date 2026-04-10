#if ((DEBUG || FFS_ECS_ENABLE_DEBUG) && !FFS_ECS_DISABLE_DEBUG)
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static UnityEditor.EditorGUILayout;
using Object = UnityEngine.Object;

namespace FFS.Libraries.StaticEcs.Unity.Editor {
    public class StaticEcsViewEntitiesTab<TWorld, TEntityProvider> : IStaticEcsViewTab 
        where TWorld : struct, IWorldType
        where TEntityProvider : StaticEcsEntityProvider<TWorld> {
        internal enum TabType: byte {
            Table,
            EntityBuilder
        }
        
        private static readonly TabType[] _tabs = { TabType.Table, TabType.EntityBuilder };
        private static readonly string[] _tabsNames = { "Table", "Entity builder" };
        internal TabType SelectedTab;
        
        private readonly Dictionary<Type, EntitiesDrawer<TWorld, TEntityProvider>> _drawersByWorldTypeType = new();
        private EntitiesDrawer<TWorld, TEntityProvider> _currentDrawer;
        
        private EntitiesSettings _savedSettings;

        public string Name() => "Entities";

        public void Init() {}

        public void Draw() {
            Ui.DrawToolbar(_tabs, ref SelectedTab, type => _tabsNames[(int) type]);
            _currentDrawer?.DrawEntitiesData();
        }

        public void Destroy() {
            foreach (var drawer in _drawersByWorldTypeType) {
                drawer.Value.Destroy();
            }
        }

        public void OnWorldChanged(AbstractWorldData newWorldData) {
            if (!_drawersByWorldTypeType.ContainsKey(newWorldData.Handle.WorldType)) {
                _drawersByWorldTypeType[newWorldData.Handle.WorldType] = new EntitiesDrawer<TWorld, TEntityProvider>(this, newWorldData, _savedSettings);
            }

            _currentDrawer = _drawersByWorldTypeType[newWorldData.Handle.WorldType];
            _drawersByWorldTypeType[newWorldData.Handle.WorldType] = _currentDrawer;
        }

        public void SaveState(WorldViewSettings settings) {
            settings.entities.selectedTab = (int) SelectedTab;
            _currentDrawer?.SaveToConfig(settings.entities);
        }

        public void LoadState(WorldViewSettings settings) {
            _savedSettings = settings.entities;
            SelectedTab = (TabType) settings.entities.selectedTab;
            _currentDrawer?.LoadFromConfig(settings.entities);
        }
    }

    public partial class EntitiesDrawer<TWorld, TEntityProvider> where TWorld : struct, IWorldType where TEntityProvider : StaticEcsEntityProvider<TWorld> {
        private Vector2 verticalScrollEntitiesPosition = Vector2.zero;
        private Vector2 horizontalScrollEntitiesPosition = Vector2.zero;

        private float _maxWidth;
        
        private readonly List<EditorEntityDataMetaByWorld> _components = new();
        private readonly List<EditorEntityDataMetaByWorld> _componentsColumns = new();

        private readonly List<EditorEntityDataMetaByWorld> _tags = new();
        private readonly List<EditorEntityDataMetaByWorld> _tagsColumns = new();

        private EditorEntityDataMetaByWorld _sortIdx;
        private int _lastSnapshotCount;
        private readonly List<EntityGID> _pinedEntities = new();
        
        private bool _gidFilterActive;
        private int _gidFilterValue;

        private readonly TEntityProvider _entityBuilder;
        
        private readonly StaticEcsViewEntitiesTab<TWorld, TEntityProvider> _parent;


        internal EntitiesDrawer(StaticEcsViewEntitiesTab<TWorld, TEntityProvider> parent, AbstractWorldData worldData, EntitiesSettings savedSettings) {
            _parent = parent;

            var worldMeta = MetaData.GetWorldMetaData(typeof(TWorld));
            foreach (var val in worldMeta.Components) {
                if (worldData.Handle.TryGetComponentsHandle(val.Type, out var handle)) {
                    _components.Add(new EditorEntityDataMetaByWorld(val, handle, null, e => handle.Has(e)));
                }
            }

            foreach (var val in worldMeta.Tags) {
                if (worldData.Handle.TryGetComponentsHandle(val.Type, out var handle)) {
                    _tags.Add(new EditorEntityDataMetaByWorld(val, null, handle, e => handle.Has(e)));
                }
            }

            _componentsAndTags.AddRange(_components);
            _componentsAndTags.AddRange(_tags);

            if (savedSettings != null) {
                LoadFromConfig(savedSettings);
            } else {
                ShowAllColumns(true);
            }

            var go = new GameObject("StaticEcsEntityBuider") {
                hideFlags = HideFlags.NotEditable,
            };
            Object.DontDestroyOnLoad(go);
            _entityBuilder = go.AddComponent<TEntityProvider>();
            if (_entityBuilder) {
                _entityBuilder.UsageType = UsageType.Manual;
                _entityBuilder.OnCreateType = OnCreateType.None;
            }
        }

        internal void DrawEntitiesData() {
            switch (_parent.SelectedTab) {
                case StaticEcsViewEntitiesTab<TWorld, TEntityProvider>.TabType.Table:
                    DrawEntitiesFilter();
                    DrawEntitiesTable();
                    break;
                case StaticEcsViewEntitiesTab<TWorld, TEntityProvider>.TabType.EntityBuilder:
                    var prefab = ObjectField("Prefab", _entityBuilder.prefab, typeof(TEntityProvider), true);
                    if (prefab != _entityBuilder.prefab) {
                        _entityBuilder.prefab = (TEntityProvider) prefab;
                        EditorUtility.SetDirty(_entityBuilder);
                    }
                    Drawer.DrawEntity<TWorld, TEntityProvider>(_entityBuilder, DrawMode.Builder, provider => {
                        provider.CreateEntity();
                        EntityInspectorHelper<TWorld, TEntityProvider>.ShowWindowForEntity(provider.Entity);
                        provider.EntityGid = default;
                        EditorUtility.SetDirty(provider);
                    }, provider => {
                        provider.EntityGid = default;
                        EditorUtility.SetDirty(provider);
                    });
                    break;
            }
        }

        private void ShowAllColumns(bool? showTableData) {
            _componentsColumns.Clear();
            foreach (var val in _components) {
                if (showTableData.HasValue) {
                    val.ShowTableData = showTableData.Value;
                }
                _componentsColumns.Add(val);
            }

            _tagsColumns.Clear();
            foreach (var val in _tags) {
                _tagsColumns.Add(val);
            }
        }

        private void ShowNoneColumns() {
            _componentsColumns.Clear();
            _tagsColumns.Clear();
        }

        internal void SaveToConfig(EntitiesSettings settings) {
            settings.componentColumns.Clear();
            foreach (var col in _componentsColumns) settings.componentColumns.Add(col.FullName);

            settings.tagColumns.Clear();
            foreach (var col in _tagsColumns) settings.tagColumns.Add(col.FullName);

            settings.showTableDataTypes.Clear();
            foreach (var col in _componentsColumns) {
                if (col.ShowTableData) settings.showTableDataTypes.Add(col.FullName);
            }

            settings.sortByType = _sortIdx?.FullName ?? "";
            settings.maxEntityResult = _maxEntityResult;
            settings.filterActive = _filterActive;
            settings.gidFilterActive = _gidFilterActive;
            settings.gidFilterValue = _gidFilterValue;

            settings.pinnedEntities.Clear();
            foreach (var gid in _pinedEntities) settings.pinnedEntities.Add(gid.Raw);

            SaveFilterList(settings.filterAll, _all);
            SaveFilterList(settings.filterAllOnlyDisabled, _allOnlyDisabled);
            SaveFilterList(settings.filterAllWithDisabled, _allWithDisabled);
            SaveFilterList(settings.filterNone, _none);
            SaveFilterList(settings.filterNoneWithDisabled, _noneWithDisabled);
            SaveFilterList(settings.filterAny, _any);
            SaveFilterList(settings.filterAnyOnlyDisabled, _anyOnlyDisabled);
            SaveFilterList(settings.filterAnyWithDisabled, _anyWithDisabled);
        }

        private static void SaveFilterList(List<string> target, List<EditorEntityDataMetaByWorld> source) {
            target.Clear();
            foreach (var item in source) target.Add(item.FullName);
        }

        internal void LoadFromConfig(EntitiesSettings settings) {
            _componentsColumns.Clear();
            _tagsColumns.Clear();

            var hasColumns = settings.componentColumns.Count > 0 || settings.tagColumns.Count > 0;
            if (!hasColumns) {
                ShowAllColumns(true);
                return;
            }

            foreach (var name in settings.componentColumns) {
                var meta = FindByFullName(_components, name);
                if (meta != null) _componentsColumns.Add(meta);
            }

            foreach (var name in settings.tagColumns) {
                var meta = FindByFullName(_tags, name);
                if (meta != null) _tagsColumns.Add(meta);
            }

            foreach (var name in settings.showTableDataTypes) {
                var meta = FindByFullName(_components, name);
                if (meta != null) meta.ShowTableData = true;
            }

            if (!string.IsNullOrEmpty(settings.sortByType)) {
                _sortIdx = FindByFullName(_componentsAndTags, settings.sortByType);
            }

            _maxEntityResult = settings.maxEntityResult;
            _filterActive = settings.filterActive;
            _gidFilterActive = settings.gidFilterActive;
            _gidFilterValue = settings.gidFilterValue;

            foreach (var raw in settings.pinnedEntities) {
                _pinedEntities.Add(new EntityGID(raw));
            }

            LoadFilterList(settings.filterAll, _all);
            LoadFilterList(settings.filterAllOnlyDisabled, _allOnlyDisabled);
            LoadFilterList(settings.filterAllWithDisabled, _allWithDisabled);
            LoadFilterList(settings.filterNone, _none);
            LoadFilterList(settings.filterNoneWithDisabled, _noneWithDisabled);
            LoadFilterList(settings.filterAny, _any);
            LoadFilterList(settings.filterAnyOnlyDisabled, _anyOnlyDisabled);
            LoadFilterList(settings.filterAnyWithDisabled, _anyWithDisabled);
            _filterDirty = true;
        }

        private void LoadFilterList(List<string> source, List<EditorEntityDataMetaByWorld> target) {
            target.Clear();
            foreach (var name in source) {
                var meta = FindByFullName(_componentsAndTags, name);
                if (meta != null) target.Add(meta);
            }
        }

        private static EditorEntityDataMetaByWorld FindByFullName(List<EditorEntityDataMetaByWorld> list, string fullName) {
            foreach (var item in list) {
                if (item.FullName == fullName) return item;
            }
            return null;
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
                var gidEntity = new World<TWorld>.Entity((uint)_gidFilterValue);
                if (!gidEntity.IsDestroyed) {
                    DrawEntityRow(gidEntity, false, false);
                }
            } else {
                var system = EcsDebug<TWorld>.DebugViewSystem;
                system.MaxEntityResult = _maxEntityResult;
                system.SetFilter(_filterActive && IsFilterValid() ? GetOrBuildFilter() : default);
                system.SetSortHandle(_sortIdx?.ComponentHandle ?? _sortIdx?.TagHandle);

                var snapshot = system.ReadSnapshot();
                if (snapshot != null) {
                    if (Event.current.type == EventType.Layout) {
                        _lastSnapshotCount = snapshot.Count;
                    }
                    for (var i = 0; i < _lastSnapshotCount; i++) {
                        if (!DrawEntityRow(snapshot.Entities[i], true, false)) {
                            break;
                        }
                    }
                }
            }

            GUILayout.EndScrollView();
            GUILayout.EndScrollView();
        }

        private void DrawPined() {
            var count = _pinedEntities.Count;
            for (var i = count - 1; i >= 0; i--) {
                var gid = _pinedEntities[i];
                if (gid.TryUnpack<TWorld>(out var entity)) {
                    DrawEntityRow(entity, false, true);
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

                for (var i = 0; i < _tagsColumns.Count;) {
                    var idx = _tagsColumns[i];
                    SelectableLabel(idx.Name, idx.Type.EditorTypeColor(out var color) ? Ui.LabelStyleThemeCenterColor(color) : Ui.LabelStyleThemeCenter, idx.Layout);
                    DrawSortButton(idx);
                    DrawDeleteColumnButton(ref i, _tagsColumns);
                    Ui.DrawSeparator();
                }
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

        private bool DrawEntityRow(World<TWorld>.Entity entity, bool sorted, bool pined) {
            if (_currentEntityCount >= _maxEntityResult && !pined) {
                return false;
            }

            if (entity.IsDestroyed || (!pined && _pinedEntities.Contains(entity))) {
                return true;
            }

            _currentEntityCount++;
            BeginHorizontal();
            {
                BeginHorizontal(Ui.WidthLine(97));
                DrawViewEntityButton(entity);
                DrawPinEntityButton(entity, pined);
                if (DrawDeleteEntityButton(entity)) {
                    EndHorizontal();
                    EndHorizontal();
                    return true;
                }
                EndHorizontal();

                Ui.DrawSeparator();
                DrawEntityId(entity);
                DrawComponents(entity);
            }
            EndHorizontal();
            Ui.DrawHorizontalSeparator(_maxWidth);
            return true;
        }

        private void DrawComponents(World<TWorld>.Entity entity) {
            _maxWidth = 180f;
            const float baseWidth = 16f;
            DrawComponents(entity, _tagsColumns, 46f + baseWidth);
            DrawComponents(entity, _componentsColumns, 68f + baseWidth);
        }

        private static void DrawEntityId(World<TWorld>.Entity entity) {
            SelectableLabel(Ui.IntToStringD6((int) entity.ID).d6, Ui.LabelStyleThemeCenter, Ui.WidthLine(60));
            Ui.DrawSeparator();
        }

        private bool DrawDeleteEntityButton(World<TWorld>.Entity entity) {
            if (Ui.TrashButtonExpand) {
                EcsDebug<TWorld>.DebugViewSystem.EnqueueCommand(new DebugCommand {
                    Type = DebugCommandType.DestroyEntity,
                    EntityGid = entity.GID,
                });
                return true;
            }

            return false;
        }

        private void DrawPinEntityButton(World<TWorld>.Entity entity, bool pinned) {
            if (Ui.LockButtonExpand) {
                if (pinned) {
                    _pinedEntities.Remove(entity);
                } else {
                    _pinedEntities.Add(entity);
                }
            }
        }

        private void DrawViewEntityButton(World<TWorld>.Entity entity) {
            LabelField(GUIContent.none, Ui.Width(10));
            if (Ui.ViewButtonExpand) {
                EntityInspectorHelper<TWorld, TEntityProvider>.ShowWindowForEntity(entity.GID);
            }
        }

        private void DrawComponents(World<TWorld>.Entity entity, List<EditorEntityDataMetaByWorld> types, float widthAdd) {
            const string HasComponent = "✔";

            foreach (var idx in types) {
                if (idx.HasComponent(entity)) {
                    var style = idx.ComponentHandle.HasValue && idx.ComponentHandle.Value.HasDisabled(entity.ID)
                        ? Ui.LabelStyleGreyCenter
                        : Ui.LabelStyleThemeCenter;
                    if (idx.ShowTableData) {
                        if (idx.ComponentHandle.HasValue && idx.ComponentHandle.Value.TryGetRaw(entity.ID, out var rawValue)) {
                                if (idx.TryGetTableField(out var field)) {
                                    Drawer.DrawField(rawValue, field, style, idx.LayoutWithOffset);
                                } else if (idx.TryGetTableProperty(out var property)) {
                                    Drawer.DrawProperty(rawValue, property, style, idx.LayoutWithOffset);
                                } else {
                                    LabelField(HasComponent, style, idx.LayoutWithOffset);
                                }
                        } else {
                            LabelField(HasComponent, style, idx.LayoutWithOffset);
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
        public readonly ComponentsHandle? ComponentHandle;
        public readonly ComponentsHandle? TagHandle;
        private readonly Func<uint, bool> _hasComponentFunc;
        public bool ShowTableData;

        public EditorEntityDataMetaByWorld(EditorEntityDataMeta meta, ComponentsHandle? componentHandle, ComponentsHandle? tagHandle, Func<uint, bool> hasComponentFunc)
            : base(meta.Type, meta.Name, meta.FullName, meta.Width, meta.Layout, meta.LayoutWithOffset, meta.FieldInfo, meta.PropertyInfo) {
            _hasComponentFunc = hasComponentFunc;
            ShowTableData = false;
            ComponentHandle = componentHandle;
            TagHandle = tagHandle;
        }

        public bool HasComponent<TWorld>(World<TWorld>.Entity entity) where TWorld : struct, IWorldType => _hasComponentFunc(entity.ID);

        public uint CalculateCount() {
            if (ComponentHandle.HasValue) return ComponentHandle.Value.CalculateCount();
            if (TagHandle.HasValue) return TagHandle.Value.CalculateCount();
            return 0;
        }
    }
}
#endif