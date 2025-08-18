using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace FFS.Libraries.StaticEcs.Unity.Editor {

    internal static class LastFocusedInspectorWindow {
        internal static EditorWindow lastFocused;

        internal static void DockNextTo(EditorWindow windowToDock) {
            if (!lastFocused) {
                windowToDock.Show();
                return;
            }
            
            var parentField = typeof(EditorWindow).GetField("m_Parent", BindingFlags.Instance | BindingFlags.NonPublic);
            if (parentField != null) {
                var targetParent = parentField.GetValue(lastFocused);
                var dockAreaType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.DockArea");

                if (targetParent != null && targetParent.GetType() == dockAreaType) {
                    var addTabMethod = dockAreaType.GetMethod("AddTab", new Type[] { typeof(EditorWindow), typeof(bool) });
                    if (addTabMethod != null) {
                        addTabMethod.Invoke(targetParent, new object[] { windowToDock, true });
                        windowToDock.Show();
                    }
                } else {
                    Debug.LogWarning("Target window is not dockable.");
                }
            }
        }
    }
    
    
    public class EntityInspectorWindow : EditorWindow {
        private static readonly Dictionary<Type, Dictionary<uint, EntityInspectorWindow>> data = new();

        private IWorld _world;
        private StaticEcsEntityProvider _provider;
        private IEntity _entity;
        
        internal float drawRate = 0.5f;
        internal float drawFrames = 2;
        private float _acc;

        static EntityInspectorWindow() {
            EditorApplication.playModeStateChanged += state => {
                if (state == PlayModeStateChange.ExitingPlayMode) {
                    
                    var allWindows = new List<EntityInspectorWindow>();
                    foreach (var (key, value) in data) {
                        foreach (var (id, window) in value) {
                            allWindows.Add(window);
                        }
                    }
                    
                    foreach (var window in allWindows) {
                        window.Close();
                    }

                    data.Clear();
                }

                LastFocusedInspectorWindow.lastFocused = null;
            };
        }

        private void Draw() {
            if (LastFocusedInspectorWindow.lastFocused != this) {
                EditorApplication.update -= Draw;
                return;
            }
            
            _acc += Time.deltaTime;
            if (_acc >= drawRate) {
                Repaint();
                _acc = 0f;
            }
        }

        public static bool ShowWindowForEntity(IWorld world, EntityGID gid) {
            if (world.TryGetEntity(gid, out var entity)) {
                ShowWindowForEntity(world, entity);
                return true;
            }

            return false;
        }

        public static void ShowWindowForEntity(IWorld world, IEntity entity) {
            if (data.TryGetValue(world.GetWorldType(), out var entities)) { } else {
                entities = new Dictionary<uint, EntityInspectorWindow>();
                data[world.GetWorldType()] = entities;
            }

            if (entities.TryGetValue(entity.GetId(), out var existingWindow)) {
                existingWindow.Focus();
                return;
            }

            var window = CreateInstance<EntityInspectorWindow>();
            entities[entity.GetId()] = window;
            var nameFunction = StaticEcsDebugData.Worlds[world.GetWorldType()].WindowNameFunction;
            window.titleContent = new GUIContent(nameFunction?.Invoke(entity) ?? $"Entity {entity.GetId()}");
            window._entity = entity;
            window._world = world;
            window._provider = CreateStaticEcsEntityDebugView(world, entity);
            
            LastFocusedInspectorWindow.DockNextTo(window);
        }

        private static StaticEcsEntityProvider CreateStaticEcsEntityDebugView(IWorld world, IEntity entity) {
            var wType = world.GetWorldType();

            var go = new GameObject("StaticEcsEntityDebugView") {
                hideFlags = HideFlags.NotEditable,
            };
            DontDestroyOnLoad(go);
            var provider = go.AddComponent<StaticEcsEntityProvider>();
            provider.Entity = entity;
            provider.UsageType = UsageType.Manual;
            provider.OnCreateType = OnCreateType.None;
            provider.WorldTypeName = wType.FullName;
            provider.WorldEditorName = MetaData.WorldsMetaData.Find(t => t.WorldTypeType == wType).EditorName;
            return provider;
        }

        private void OnFocus() {
            EditorApplication.update += Draw;
            LastFocusedInspectorWindow.lastFocused = this;
        }
        
        private void OnDisable() {
            EditorApplication.update -= Draw;
        }

        private void OnDestroy() {
            if (data.TryGetValue(_world.GetWorldType(), out var entities)) {
                if (entities.TryGetValue(_entity.GetId(), out var window) && window == this) {
                    Destroy(window._provider.gameObject);
                    entities.Remove(_entity.GetId());
                }
            }
        }

        private void OnGUI() {
            if (Application.isPlaying) {
                if (!_provider.EntityIsActual()) {
                    EditorGUILayout.HelpBox("Entity is destroyed or not actual", MessageType.Info, true);
                } else {
                    Drawer.DrawEntity(_provider, DrawMode.Viewer, _ => {}, false, _ => Close());
                }
            } else {
                EditorGUILayout.HelpBox("Data is only available in play mode", MessageType.Info, true);
            }
        }
    }
    
    public readonly struct EventId : IEquatable<EventId> {
        public readonly int InternalIdx;
        public readonly short Version;
        
        public EventId(int internalIdx, short version) {
            InternalIdx = internalIdx;
            Version = version;
        }

        public bool Equals(EventId other) {
            return InternalIdx == other.InternalIdx && Version == other.Version;
        }

        public override bool Equals(object obj) {
            return obj is EventId other && Equals(other);
        }

        public override int GetHashCode() {
            return HashCode.Combine(InternalIdx, Version);
        }
    }
    
    
    public class EventInspectorWindow : EditorWindow {
        private static readonly Dictionary<Type, Dictionary<EventId, EventInspectorWindow>> data = new();

        private IWorld _world;
        private StaticEcsEventProvider _provider;
        private EventId _id;
        
        internal float drawRate = 0.5f;
        internal float drawFrames = 2;
        private float _acc;

        static EventInspectorWindow() {
            EditorApplication.playModeStateChanged += state => {
                if (state == PlayModeStateChange.ExitingPlayMode) {
                    
                    var allWindows = new List<EventInspectorWindow>();
                    foreach (var (key, value) in data) {
                        foreach (var (id, window) in value) {
                            allWindows.Add(window);
                        }
                    }
                    
                    foreach (var window in allWindows) {
                        window.Close();
                    }

                    data.Clear();
                }

                LastFocusedInspectorWindow.lastFocused = null;
            };
        }

        private void Draw() {
            if (LastFocusedInspectorWindow.lastFocused != this) {
                EditorApplication.update -= Draw;
                return;
            }
            
            _acc += Time.deltaTime;
            if (_acc >= drawRate) {
                Repaint();
                _acc = 0f;
            }
        }

        public static void ShowWindowForEvent(IWorld world, in EventData eventData) {
            ShowWindowForEvent(world, new RuntimeEvent {
                InternalIdx = eventData.InternalIdx,
                Version = eventData.Version,
                Type = eventData.TypeIdx.Type
            }, eventData.CachedData);
        }

        public static void ShowWindowForEvent(IWorld world, in RuntimeEvent runtimeEvent, IEvent cachedEvent) {
            if (data.TryGetValue(world.GetWorldType(), out var events)) { } else {
                events = new Dictionary<EventId, EventInspectorWindow>();
                data[world.GetWorldType()] = events;
            }

            var id = new EventId(runtimeEvent.InternalIdx, runtimeEvent.Version);

            if (events.TryGetValue(id, out var existingWindow)) {
                existingWindow.Focus();
                return;
            }

            var window = CreateInstance<EventInspectorWindow>();
            events[id] = window;
            window.titleContent = new GUIContent(runtimeEvent.Type.EditorTypeName());
            window._id = id;
            window._world = world;
            window._provider = CreateStaticEcsEventDebugView(world, in runtimeEvent, cachedEvent);
            
            LastFocusedInspectorWindow.DockNextTo(window);
        }

        private static StaticEcsEventProvider CreateStaticEcsEventDebugView(IWorld world, in RuntimeEvent runtimeEvent, IEvent cachedEvent) {
            var wType = world.GetWorldType();
            var go = new GameObject("StaticEcsEventDebugView") {
                hideFlags = HideFlags.NotEditable,
            };
            DontDestroyOnLoad(go);
            var provider = go.AddComponent<StaticEcsEventProvider>();
            provider.RuntimeEvent = runtimeEvent;
            provider.EventCache = cachedEvent;
            provider.UsageType = UsageType.Manual;
            provider.OnCreateType = OnCreateType.None;
            provider.WorldTypeName = wType.FullName;
            provider.WorldEditorName = MetaData.WorldsMetaData.Find(t => t.WorldTypeType == wType).EditorName;
            return provider;
        }

        private void OnFocus() {
            EditorApplication.update += Draw;
            LastFocusedInspectorWindow.lastFocused = this;
        }
        
        private void OnDisable() {
            EditorApplication.update -= Draw;
        }

        private void OnDestroy() {
            if (data.TryGetValue(_world.GetWorldType(), out var events)) {
                if (events.TryGetValue(_id, out var window) && window == this) {
                    Destroy(window._provider.gameObject);
                    events.Remove(_id);
                }
            }
        }

        private void OnGUI() {
            if (Application.isPlaying) {
                if (_provider.RuntimeEvent.IsEmpty()) {
                    EditorGUILayout.HelpBox("Entity is destroyed or not actual", MessageType.Info, true);
                } else {
                    Drawer.DrawEvent(_provider, DrawMode.Viewer, _ => { }, provider => {});
                }
            } else {
                EditorGUILayout.HelpBox("Data is only available in play mode", MessageType.Info, true);
            }
        }
    }
}