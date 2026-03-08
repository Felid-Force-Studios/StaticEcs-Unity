#if ((DEBUG || FFS_ECS_ENABLE_DEBUG) && !FFS_ECS_DISABLE_DEBUG)
using System;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.EditorGUILayout;

namespace FFS.Libraries.StaticEcs.Unity.Editor {
    public class StaticEcsViewStatsTab<TWorld> : IStaticEcsViewTab 
        where TWorld : struct, IWorldType {
        
        private readonly Dictionary<Type, StatsDrawer<TWorld>> _drawersByWorldTypeType = new();
        private StatsDrawer<TWorld> _currentDrawer;

        public string Name() => "Stats";

        public void Init() { }

        public void Draw() {
            _currentDrawer.Draw();
        }

        public void Destroy() { }

        public void OnWorldChanged(AbstractWorldData newWorldData) {
            if (!_drawersByWorldTypeType.ContainsKey(newWorldData.Handle.WorldType)) {
                _drawersByWorldTypeType[newWorldData.Handle.WorldType] = new StatsDrawer<TWorld>(newWorldData);
            }

            _currentDrawer = _drawersByWorldTypeType[newWorldData.Handle.WorldType];
        }
    }

    public class StatsDrawer<TWorld> where TWorld : struct, IWorldType {
        private readonly ComponentsHandle?[] componentHandles;
        private readonly ComponentsHandle?[] tagHandles;
        private readonly EventsHandle?[] eventHandles;
        private readonly List<EditorEntityTypeMeta> registeredEntityTypes = new();

        private readonly WorldHandle _handle;
        private readonly AbstractWorldData _worldData;

        private bool _showNotRegistered = true;
        private Vector2 verticalScrollStatsPosition = Vector2.zero;

        public StatsDrawer(AbstractWorldData worldData) {
            _worldData = worldData;
            _handle = _worldData.Handle;

            componentHandles = new ComponentsHandle?[MetaData.Components.Count];
            for (var i = 0; i < MetaData.Components.Count; i++) {
                if (_handle.TryGetComponentsHandle(MetaData.Components[i].Type, out var handle)) {
                    componentHandles[i] = handle;
                }
            }

            tagHandles = new ComponentsHandle?[MetaData.Tags.Count];
            for (var i = 0; i < MetaData.Tags.Count; i++) {
                if (_handle.TryGetComponentsHandle(MetaData.Tags[i].Type, out var handle)) {
                    tagHandles[i] = handle;
                }
            }

            eventHandles = new EventsHandle?[MetaData.Events.Count];
            for (var i = 0; i < MetaData.Events.Count; i++) {
                if (_handle.TryGetEventsHandle(MetaData.Events[i].Type, out var handle)) {
                    eventHandles[i] = handle;
                }
            }

            for (var i = 0; i < MetaData.EntityTypes.Count; i++) {
                if (_handle.IsEntityTypeRegistered(MetaData.EntityTypes[i].Id)) {
                    registeredEntityTypes.Add(MetaData.EntityTypes[i]);
                }
            }
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

            DrawEntityTypeStats();
            DrawComponentsHandleStats("Components:", MetaData.Components, componentHandles, true);
            DrawTagsHandleStats("Tags:", MetaData.Tags, tagHandles, false);
            DrawEventsPoolsStats();

            EndScrollView();
        }

        private void DrawEntityTypeStats() {
            if (registeredEntityTypes.Count == 0) return;

            BeginHorizontal();
            SelectableLabel("Entity Types:", Ui.LabelStyleThemeCenter, Ui.WidthLine(200));
            Ui.DrawSeparator();
            SelectableLabel("Count", Ui.LabelStyleThemeCenter, Ui.WidthLine(90));
            Ui.DrawSeparator();
            SelectableLabel("Capacity", Ui.LabelStyleThemeCenter, Ui.WidthLine(90));
            Ui.DrawSeparator();
            EndHorizontal();
            Ui.DrawHorizontalSeparator(420f);

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

            Ui.DrawHorizontalSeparator(530f);

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

        private void DrawComponentsHandleStats(string type, List<EditorEntityDataMeta> indexes, ComponentsHandle?[] handles, bool withCapacity) {
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
                var handle = handles[i];
                if (handle.HasValue) {
                    BeginHorizontal();
                    if (idx.Type.EditorTypeColor(out var color)) {
                        SelectableLabel(idx.Name, Ui.LabelStyleThemeLeftColor(color), Ui.WidthLine(200));
                    } else {
                        SelectableLabel(idx.Name, Ui.WidthLine(200));
                    }

                    Ui.DrawSeparator();
                    LabelField(handle.Value.CalculateCount().ToString(), Ui.LabelStyleThemeCenter, Ui.WidthLine(90));
                    Ui.DrawSeparator();
                    if (withCapacity) {
                        LabelField(handle.Value.CalculateCapacity().ToString(), Ui.LabelStyleThemeCenter, Ui.WidthLine(90));
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

        private void DrawTagsHandleStats(string type, List<EditorEntityDataMeta> indexes, ComponentsHandle?[] handles, bool withCapacity) {
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
                var handle = handles[i];
                if (handle.HasValue) {
                    BeginHorizontal();
                    if (idx.Type.EditorTypeColor(out var color)) {
                        SelectableLabel(idx.Name, Ui.LabelStyleThemeLeftColor(color), Ui.WidthLine(200));
                    } else {
                        SelectableLabel(idx.Name, Ui.WidthLine(200));
                    }

                    Ui.DrawSeparator();
                    LabelField(handle.Value.CalculateCount().ToString(), Ui.LabelStyleThemeCenter, Ui.WidthLine(90));
                    Ui.DrawSeparator();
                    if (withCapacity) {
                        LabelField(handle.Value.CalculateCapacity().ToString(), Ui.LabelStyleThemeCenter, Ui.WidthLine(90));
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
    }
}
#endif