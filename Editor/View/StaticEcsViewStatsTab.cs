using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

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
        private readonly IStandardRawPool[] standardComponentPools;
        private readonly IRawPool[] componentPools;
        #if !FFS_ECS_DISABLE_TAGS
        private readonly IRawPool[] tagPools;
        #endif
        #if !FFS_ECS_DISABLE_MASKS
        private readonly IRawPool[] maskPools;
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
            standardComponentPools = new IStandardRawPool[MetaData.StandardComponents.Count];
            for (var i = 0; i < MetaData.StandardComponents.Count; i++) {
                if (_world.TryGetStandardComponentsRawPool(MetaData.StandardComponents[i].Type, out var pool)) {
                    standardComponentPools[i] = pool;
                }
            }
            
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

            #if !FFS_ECS_DISABLE_MASKS
            maskPools = new IRawPool[MetaData.Masks.Count];
            for (var i = 0; i < MetaData.Masks.Count; i++) {
                if (_world.TryGetMasksRawPool(MetaData.Masks[i].Type, out var pool)) {
                    maskPools[i] = pool;
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
            StaticEcsView.DrawWorldSelector();

            EditorGUILayout.Space(10);
            DrawWorldStats();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Show not registered", Ui.WidthLine(200));
            _showNotRegistered = EditorGUILayout.Toggle(_showNotRegistered);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();

            verticalScrollStatsPosition = EditorGUILayout.BeginScrollView(verticalScrollStatsPosition);

            DrawComponentPoolsStats("Standard components:", MetaData.StandardComponents, standardComponentPools);
            DrawComponentPoolsStats("Components:", MetaData.Components, componentPools);
            #if !FFS_ECS_DISABLE_TAGS
            DrawComponentPoolsStats("Tags:", MetaData.Tags, tagPools);
            #endif
            #if !FFS_ECS_DISABLE_MASKS
            DrawComponentPoolsStats("Masks:", MetaData.Masks, maskPools);
            #endif
            #if !FFS_ECS_DISABLE_EVENTS
            DrawEventsPoolsStats();
            #endif

            EditorGUILayout.EndScrollView();
        }

        private void DrawWorldStats() {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.SelectableLabel("World", Ui.LabelStyleWhiteCenter, Ui.WidthLine(200));
            Ui.DrawSeparator();
            EditorGUILayout.SelectableLabel("Count", Ui.LabelStyleWhiteCenter, Ui.WidthLine(90));
            Ui.DrawSeparator();
            EditorGUILayout.SelectableLabel("Capacity", Ui.LabelStyleWhiteCenter, Ui.WidthLine(90));
            Ui.DrawSeparator();
            EditorGUILayout.SelectableLabel("Destroyed", Ui.LabelStyleWhiteCenter, Ui.WidthLine(90));
            Ui.DrawSeparator();
            EditorGUILayout.SelectableLabel("Destroyed capacity", Ui.LabelStyleWhiteCenter, Ui.WidthLine(120));
            Ui.DrawSeparator();
            EditorGUILayout.EndHorizontal();

            Ui.DrawHorizontalSeparator(660f);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(string.Empty, Ui.LabelStyleWhiteCenter, Ui.WidthLine(200));
            Ui.DrawSeparator();
            EditorGUILayout.LabelField(_worldData.CountWithoutDestroyed.ToString(), Ui.LabelStyleWhiteCenter, Ui.WidthLine(90));
            Ui.DrawSeparator();
            EditorGUILayout.LabelField(_worldData.Capacity.ToString(), Ui.LabelStyleWhiteCenter, Ui.WidthLine(90));
            Ui.DrawSeparator();
            EditorGUILayout.LabelField(_worldData.Destroyed.ToString(), Ui.LabelStyleWhiteCenter, Ui.WidthLine(90));
            Ui.DrawSeparator();
            EditorGUILayout.LabelField(_worldData.DestroyedCapacity.ToString(), Ui.LabelStyleWhiteCenter, Ui.WidthLine(120));
            Ui.DrawSeparator();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10);
        }

        private void DrawComponentPoolsStats(string type, List<EditorEntityDataMeta> indexes, IStandardRawPool[] pools) {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.SelectableLabel(type, Ui.LabelStyleWhiteCenter, Ui.WidthLine(200));
            Ui.DrawSeparator();
            EditorGUILayout.SelectableLabel("Count", Ui.LabelStyleWhiteCenter, Ui.WidthLine(90));
            Ui.DrawSeparator();
            EditorGUILayout.SelectableLabel("Capacity", Ui.LabelStyleWhiteCenter, Ui.WidthLine(90));
            Ui.DrawSeparator();
            EditorGUILayout.EndHorizontal();
            Ui.DrawHorizontalSeparator(420f);

            for (var i = 0; i < indexes.Count; i++) {
                var idx = indexes[i];
                var pool = pools[i];
                if (pool != null) {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.SelectableLabel(idx.Name, Ui.WidthLine(200));
                    Ui.DrawSeparator();
                    EditorGUILayout.LabelField(pool.Count().ToString(), Ui.LabelStyleWhiteCenter, Ui.WidthLine(90));
                    Ui.DrawSeparator();
                    if (pool.Capacity() < 0) {
                        EditorGUILayout.LabelField("N/A", Ui.LabelStyleGreyCenter, Ui.WidthLine(90));
                    } else {
                        EditorGUILayout.LabelField(pool.Capacity().ToString(), Ui.LabelStyleWhiteCenter, Ui.WidthLine(90));
                    }

                    Ui.DrawSeparator();
                    EditorGUILayout.EndHorizontal();
                } else if (_showNotRegistered) {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.SelectableLabel(idx.Name, Ui.WidthLine(200));
                    Ui.DrawSeparator();
                    EditorGUILayout.LabelField("N/A", Ui.LabelStyleGreyCenter, Ui.WidthLine(90));
                    Ui.DrawSeparator();
                    EditorGUILayout.LabelField("N/A", Ui.LabelStyleGreyCenter, Ui.WidthLine(90));
                    Ui.DrawSeparator();
                    EditorGUILayout.EndHorizontal();
                }
            }

            EditorGUILayout.Space(20);
        }
        
        #if !FFS_ECS_DISABLE_EVENTS
        private void DrawEventsPoolsStats() {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.SelectableLabel("Events:", Ui.LabelStyleWhiteCenter, Ui.WidthLine(200));
            Ui.DrawSeparator();
            EditorGUILayout.SelectableLabel("Count", Ui.LabelStyleWhiteCenter, Ui.WidthLine(90));
            Ui.DrawSeparator();
            EditorGUILayout.SelectableLabel("Capacity", Ui.LabelStyleWhiteCenter, Ui.WidthLine(90));
            Ui.DrawSeparator();
            EditorGUILayout.SelectableLabel("Receivers", Ui.LabelStyleWhiteCenter, Ui.WidthLine(90));
            Ui.DrawSeparator();
            EditorGUILayout.EndHorizontal();
            Ui.DrawHorizontalSeparator(525f);

            for (var i = 0; i < MetaData.Events.Count; i++) {
                var idx = MetaData.Events[i];
                var pool = eventPools[i];
                if (pool != null) {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.SelectableLabel(idx.Name, Ui.WidthLine(200));
                    Ui.DrawSeparator();
                    EditorGUILayout.LabelField(pool.NotDeletedCount().ToString(), Ui.LabelStyleWhiteCenter, Ui.WidthLine(90));
                    Ui.DrawSeparator();
                    EditorGUILayout.LabelField(pool.Capacity().ToString(), Ui.LabelStyleWhiteCenter, Ui.WidthLine(90));
                    Ui.DrawSeparator();
                    EditorGUILayout.LabelField(pool.ReceiversCount().ToString(), Ui.LabelStyleWhiteCenter, Ui.WidthLine(90));
                    Ui.DrawSeparator();
                    EditorGUILayout.EndHorizontal();
                } else if (_showNotRegistered) {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.SelectableLabel(idx.Name, Ui.WidthLine(200));
                    Ui.DrawSeparator();
                    EditorGUILayout.LabelField("N/A", Ui.LabelStyleGreyCenter, Ui.WidthLine(90));
                    Ui.DrawSeparator();
                    EditorGUILayout.LabelField("N/A", Ui.LabelStyleGreyCenter, Ui.WidthLine(90));
                    Ui.DrawSeparator();
                    EditorGUILayout.LabelField("N/A", Ui.LabelStyleGreyCenter, Ui.WidthLine(90));
                    Ui.DrawSeparator();
                    EditorGUILayout.EndHorizontal();
                }
            }

            EditorGUILayout.Space(20);
        }
        #endif
    }
}