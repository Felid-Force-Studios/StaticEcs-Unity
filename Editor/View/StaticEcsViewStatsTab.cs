using System;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.EditorGUILayout;

namespace FFS.Libraries.StaticEcs.Unity.Editor {
    public class StaticEcsViewStatsTab : IStaticEcsViewTab {
        private readonly Dictionary<Type, StatsDrawer> _drawersByWorldTypeType = new();
        private StatsDrawer _currentDrawer;

        public string Name() => "Stats";

        public void Init() { }

        public void Draw(StaticEcsView view) {
            _currentDrawer.Draw();
        }

        public void Destroy() { }

        public void OnWorldChanged(AbstractWorldData newWorldData) {
            if (!_drawersByWorldTypeType.ContainsKey(newWorldData.WorldTypeType)) {
                _drawersByWorldTypeType[newWorldData.WorldTypeType] = new StatsDrawer(newWorldData);
            }

            _currentDrawer = _drawersByWorldTypeType[newWorldData.WorldTypeType];
        }
    }

    public class StatsDrawer {
        private readonly IRawPool[] componentPools;
        #if !FFS_ECS_DISABLE_TAGS
        private readonly IRawPool[] tagPools;
        #endif
        #if !FFS_ECS_DISABLE_EVENTS
        private readonly IEventPoolWrapper[] eventPools;
        #endif

        private readonly IWorld _world;
        private readonly AbstractWorldData _worldData;

        private bool _showNotRegistered = true;
        private Vector2 verticalScrollStatsPosition = Vector2.zero;

        public StatsDrawer(AbstractWorldData worldData) {
            _worldData = worldData;
            _world = _worldData.World;
            
            componentPools = new IRawPool[MetaData.Components.Count];
            for (var i = 0; i < MetaData.Components.Count; i++) {
                if (_world.TryGetComponentsRawPool(MetaData.Components[i].Type, out var pool)) {
                    componentPools[i] = pool;
                }
            }

            #if !FFS_ECS_DISABLE_TAGS
            tagPools = new IRawPool[MetaData.Tags.Count];
            for (var i = 0; i < MetaData.Tags.Count; i++) {
                if (_world.TryGetTagsRawPool(MetaData.Tags[i].Type, out var pool)) {
                    tagPools[i] = pool;
                }
            }
            #endif

            #if !FFS_ECS_DISABLE_EVENTS
            eventPools = new IEventPoolWrapper[MetaData.Events.Count];
            for (var i = 0; i < MetaData.Events.Count; i++) {
                if (_world.Events().TryGetPool(MetaData.Events[i].Type, out var pool)) {
                    eventPools[i] = pool;
                }
            }
            #endif

        }

        internal void Draw() {
            Space(10);
            DrawWorldStats();

            BeginHorizontal();
            LabelField("Show not registered", Ui.WidthLine(200));
            _showNotRegistered = Toggle(_showNotRegistered);
            EndHorizontal();
            Space();

            verticalScrollStatsPosition = BeginScrollView(verticalScrollStatsPosition);

            DrawComponentPoolsStats("Components:", MetaData.Components, componentPools, true);
            #if !FFS_ECS_DISABLE_TAGS
            DrawComponentPoolsStats("Tags:", MetaData.Tags, tagPools, true);
            #endif
            #if !FFS_ECS_DISABLE_EVENTS
            DrawEventsPoolsStats();
            #endif

            EndScrollView();
        }

        private void DrawWorldStats() {
            BeginHorizontal();
            SelectableLabel("World", Ui.LabelStyleThemeCenter, Ui.WidthLine(200));
            Ui.DrawSeparator();
            SelectableLabel("Count", Ui.LabelStyleThemeCenter, Ui.WidthLine(90));
            Ui.DrawSeparator();
            SelectableLabel("Capacity", Ui.LabelStyleThemeCenter, Ui.WidthLine(90));
            Ui.DrawSeparator();
            SelectableLabel("Destroyed", Ui.LabelStyleThemeCenter, Ui.WidthLine(90));
            Ui.DrawSeparator();
            SelectableLabel("Destroyed capacity", Ui.LabelStyleThemeCenter, Ui.WidthLine(120));
            Ui.DrawSeparator();
            EndHorizontal();

            Ui.DrawHorizontalSeparator(660f);

            BeginHorizontal();
            LabelField(string.Empty, Ui.LabelStyleThemeCenter, Ui.WidthLine(200));
            Ui.DrawSeparator();
            LabelField(_worldData.CountWithoutDestroyed.ToString(), Ui.LabelStyleThemeCenter, Ui.WidthLine(90));
            Ui.DrawSeparator();
            LabelField(_worldData.Capacity.ToString(), Ui.LabelStyleThemeCenter, Ui.WidthLine(90));
            Ui.DrawSeparator();
            LabelField(_worldData.Destroyed.ToString(), Ui.LabelStyleThemeCenter, Ui.WidthLine(90));
            Ui.DrawSeparator();
            LabelField(_worldData.DestroyedCapacity.ToString(), Ui.LabelStyleThemeCenter, Ui.WidthLine(120));
            Ui.DrawSeparator();
            EndHorizontal();

            Space(10);
        }

        private void DrawComponentPoolsStats(string type, List<EditorEntityDataMeta> indexes, IRawPool[] pools, bool withCapacity) {
            BeginHorizontal();
            SelectableLabel(type, Ui.LabelStyleThemeCenter, Ui.WidthLine(200));
            Ui.DrawSeparator();
            SelectableLabel("Count", Ui.LabelStyleThemeCenter, Ui.WidthLine(90));
            Ui.DrawSeparator();
            SelectableLabel("Capacity", Ui.LabelStyleThemeCenter, Ui.WidthLine(90));
            Ui.DrawSeparator();
            EndHorizontal();
            Ui.DrawHorizontalSeparator(420f);

            for (var i = 0; i < indexes.Count; i++) {
                var idx = indexes[i];
                var pool = pools[i];
                if (pool != null) {
                    BeginHorizontal();
                    if (idx.Type.EditorTypeColor(out var color)) {
                        SelectableLabel(idx.Name, Ui.LabelStyleThemeLeftColor(color), Ui.WidthLine(200));
                    } else {
                        SelectableLabel(idx.Name, Ui.WidthLine(200));
                    }

                    Ui.DrawSeparator();
                    LabelField(pool.CalculateCount().ToString(), Ui.LabelStyleThemeCenter, Ui.WidthLine(90));
                    Ui.DrawSeparator();
                    if (withCapacity) {
                        LabelField(pool.CalculateCapacity().ToString(), Ui.LabelStyleThemeCenter, Ui.WidthLine(90));
                    } else {
                        LabelField("N/A", Ui.LabelStyleGreyCenter, Ui.WidthLine(90));
                    }

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
                    EndHorizontal();
                }
            }

            Space(20);
        }
        
        #if !FFS_ECS_DISABLE_EVENTS
        private void DrawEventsPoolsStats() {
            BeginHorizontal();
            SelectableLabel("Events:", Ui.LabelStyleThemeCenter, Ui.WidthLine(200));
            Ui.DrawSeparator();
            SelectableLabel("Count", Ui.LabelStyleThemeCenter, Ui.WidthLine(90));
            Ui.DrawSeparator();
            SelectableLabel("Capacity", Ui.LabelStyleThemeCenter, Ui.WidthLine(90));
            Ui.DrawSeparator();
            SelectableLabel("Receivers", Ui.LabelStyleThemeCenter, Ui.WidthLine(90));
            Ui.DrawSeparator();
            EndHorizontal();
            Ui.DrawHorizontalSeparator(525f);

            for (var i = 0; i < MetaData.Events.Count; i++) {
                var idx = MetaData.Events[i];
                var pool = eventPools[i];
                if (pool != null) {
                    BeginHorizontal();
                    if (idx.Type.EditorTypeColor(out var color)) {
                        SelectableLabel(idx.Name, Ui.LabelStyleThemeLeftColor(color), Ui.WidthLine(200));
                    } else {
                        SelectableLabel(idx.Name, Ui.WidthLine(200));
                    }
                    Ui.DrawSeparator();
                    LabelField(pool.NotDeletedCount().ToString(), Ui.LabelStyleThemeCenter, Ui.WidthLine(90));
                    Ui.DrawSeparator();
                    LabelField(pool.Capacity().ToString(), Ui.LabelStyleThemeCenter, Ui.WidthLine(90));
                    Ui.DrawSeparator();
                    LabelField(pool.ReceiversCount().ToString(), Ui.LabelStyleThemeCenter, Ui.WidthLine(90));
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
        #endif
    }
}