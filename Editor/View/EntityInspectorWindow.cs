using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace FFS.Libraries.StaticEcs.Unity.Editor {

    public static class EntityInspectorRegistry {
        public static readonly Dictionary<Type, Func<EntityGID, bool>> ShowEntityHandlers = new();
        public static readonly List<Action> PlayModeCleanup = new();
    }

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
        private static readonly Dictionary<(Type, uint), EntityInspectorWindow> data = new();

        internal AbstractStaticEcsProvider provider;
        internal EntityGID entityGid;
        internal Type worldType;
        internal Action<EntityInspectorWindow> drawAction;
        internal Func<EntityInspectorWindow, bool> isActualFunc;

        internal float drawRate = 0.5f;
        internal float drawFrames = 2;
        private float _acc;
        private bool _subscribed;

        static EntityInspectorWindow() {
            EditorApplication.playModeStateChanged += state => {
                if (state == PlayModeStateChange.ExitingPlayMode) {

                    var allWindows = new List<EntityInspectorWindow>(data.Values);

                    foreach (var window in allWindows) {
                        window.Close();
                    }

                    data.Clear();

                    foreach (var cleanup in EntityInspectorRegistry.PlayModeCleanup) {
                        try { cleanup?.Invoke(); } catch (Exception e) { Debug.LogException(e); }
                    }
                }

                LastFocusedInspectorWindow.lastFocused = null;
            };
        }

        private void Draw() {
            _acc += Time.deltaTime;
            if (_acc >= drawRate) {
                Repaint();
                _acc = 0f;
            }
        }

        internal static bool ShowWindow(Type worldType, EntityGID gid, string title,
            Func<EntityGID, AbstractStaticEcsProvider> createProvider,
            Action<EntityInspectorWindow> drawAction,
            Func<EntityInspectorWindow, bool> isActualFunc) {

            var key = (worldType, gid.Id);
            if (data.TryGetValue(key, out var existingWindow)) {
                existingWindow.Focus();
                return true;
            }

            var window = CreateInstance<EntityInspectorWindow>();
            data[key] = window;
            window.titleContent = new GUIContent(title);
            window.entityGid = gid;
            window.worldType = worldType;
            window.provider = createProvider(gid);
            window.drawAction = drawAction;
            window.isActualFunc = isActualFunc;

            LastFocusedInspectorWindow.DockNextTo(window);
            return true;
        }

        private void OnFocus() {
            LastFocusedInspectorWindow.lastFocused = this;
            if (!_subscribed) {
                EditorApplication.update += Draw;
                _subscribed = true;
            }
        }

        private void OnDisable() {
            if (_subscribed) {
                EditorApplication.update -= Draw;
                _subscribed = false;
            }
        }

        private void OnDestroy() {
            var key = (worldType, entityGid.Id);
            if (data.TryGetValue(key, out var window) && window == this) {
                if (window.provider && window.provider.gameObject) {
                    Destroy(window.provider.gameObject);
                }

                data.Remove(key);
            }
        }

        private void OnGUI() {
            if (Application.isPlaying) {
                if (isActualFunc != null && !isActualFunc(this)) {
                    EditorGUILayout.HelpBox("Entity is destroyed or not actual", MessageType.Info, true);
                } else {
                    drawAction?.Invoke(this);
                }
            } else {
                EditorGUILayout.HelpBox("Data is only available in play mode", MessageType.Info, true);
            }
        }
    }

    public static class EntityInspectorHelper<TWorld, TProvider>
        where TWorld : struct, IWorldType
        where TProvider : StaticEcsEntityProvider<TWorld> {

        private static TProvider _inspectorAnchor;
        private static bool _cleanupRegistered;

        public static void ShowInInspector(EntityGID gid) {
            if (gid.Status<TWorld>() != GIDStatus.Active) {
                return;
            }

            if (_inspectorAnchor == null || !_inspectorAnchor) {
                _inspectorAnchor = CreateAnchor();
                if (!_cleanupRegistered) {
                    _cleanupRegistered = true;
                    EntityInspectorRegistry.PlayModeCleanup.Add(DestroyAnchor);
                }
            }

            _inspectorAnchor.EntityGid = gid;
            _inspectorAnchor.gameObject.name = BuildAnchorName(gid, hasEntity: true);
            Selection.activeGameObject = _inspectorAnchor.gameObject;
            EditorGUIUtility.PingObject(_inspectorAnchor.gameObject);
        }

        private static TProvider CreateAnchor() {
            var go = new GameObject(BuildAnchorName(default, hasEntity: false)) {
                hideFlags = HideFlags.DontSave,
            };
            UnityEngine.Object.DontDestroyOnLoad(go);
            var p = go.AddComponent<TProvider>();
            p.UsageType = UsageType.Manual;
            p.OnCreateType = OnCreateType.None;
            p.onEnableAndDisable = false;
            return p;
        }

        private static void DestroyAnchor() {
            if (_inspectorAnchor != null && _inspectorAnchor) {
                UnityEngine.Object.DestroyImmediate(_inspectorAnchor.gameObject);
            }
            _inspectorAnchor = null;
        }

        private static string BuildAnchorName(EntityGID gid, bool hasEntity) {
            if (!hasEntity) {
                return $"[StaticEcs:{typeof(TWorld).Name}]";
            }
            var fn = StaticEcsDebugData.Worlds.TryGetValue(typeof(TWorld), out var worldData) ? worldData.WindowNameFunction : null;
            return $"[StaticEcs:{typeof(TWorld).Name}] — " + (fn?.Invoke(gid) ?? $"Entity {gid.Id}");
        }

        public static bool ShowWindowForEntity(EntityGID gid) {
            if (gid.Status<TWorld>() != GIDStatus.Active) {
                return false;
            }

            var nameFunction = StaticEcsDebugData.Worlds.TryGetValue(typeof(TWorld), out var worldData) ? worldData.WindowNameFunction : null;
            var title = nameFunction?.Invoke(gid) ?? $"Entity {gid.Id}";

            return EntityInspectorWindow.ShowWindow(typeof(TWorld), gid, title,
                CreateProvider, DrawEntity, IsActual);
        }

        private static AbstractStaticEcsProvider CreateProvider(EntityGID gid) {
            var go = new GameObject("StaticEcsEntityDebugView") {
                hideFlags = HideFlags.NotEditable,
            };
            UnityEngine.Object.DontDestroyOnLoad(go);
            var provider = go.AddComponent<TProvider>();
            provider.EntityGid = gid;
            provider.UsageType = UsageType.Manual;
            provider.OnCreateType = OnCreateType.None;
            provider.onEnableAndDisable = false;
            return provider;
        }

        private static void DrawEntity(EntityInspectorWindow window) {
            Drawer.DrawEntity<TWorld, TProvider>((TProvider) window.provider, DrawMode.Viewer, _ => {}, _ => window.Close());
        }

        private static bool IsActual(EntityInspectorWindow window) {
            return ((TProvider) window.provider).EntityIsActual();
        }
    }

    public readonly struct EventId : IEquatable<EventId> {
        public readonly int InternalIdx;

        public EventId(int internalIdx) {
            InternalIdx = internalIdx;
        }

        public bool Equals(EventId other) {
            return InternalIdx == other.InternalIdx;
        }

        public override bool Equals(object obj) {
            return obj is EventId other && Equals(other);
        }

        public override int GetHashCode() {
            return HashCode.Combine(InternalIdx);
        }
    }


    public class EventInspectorWindow : EditorWindow {
        private static readonly Dictionary<(Type, EventId), EventInspectorWindow> data = new();

        internal AbstractStaticEcsProvider provider;
        internal Type worldType;
        internal EventId id;
        internal Action<EventInspectorWindow> drawAction;
        internal Func<EventInspectorWindow, bool> isEmptyFunc;

        internal float drawRate = 0.5f;
        internal float drawFrames = 2;
        private float _acc;
        private bool _subscribed;

        static EventInspectorWindow() {
            EditorApplication.playModeStateChanged += state => {
                if (state == PlayModeStateChange.ExitingPlayMode) {

                    var allWindows = new List<EventInspectorWindow>(data.Values);

                    foreach (var window in allWindows) {
                        window.Close();
                    }

                    data.Clear();
                }

                LastFocusedInspectorWindow.lastFocused = null;
            };
        }

        private void Draw() {
            _acc += Time.deltaTime;
            if (_acc >= drawRate) {
                Repaint();
                _acc = 0f;
            }
        }

        internal static void ShowWindow(Type worldType, in RuntimeEvent runtimeEvent, IEvent cachedEvent,
            Func<RuntimeEvent, IEvent, AbstractStaticEcsProvider> createProvider,
            Action<EventInspectorWindow> drawAction,
            Func<EventInspectorWindow, bool> isEmptyFunc) {

            var eventId = new EventId(runtimeEvent.InternalIdx);
            var key = (worldType, eventId);

            if (data.TryGetValue(key, out var existingWindow)) {
                existingWindow.Focus();
                return;
            }

            var window = CreateInstance<EventInspectorWindow>();
            data[key] = window;
            window.titleContent = new GUIContent(runtimeEvent.Type.EditorTypeName());
            window.id = eventId;
            window.worldType = worldType;
            window.provider = createProvider(runtimeEvent, cachedEvent);
            window.drawAction = drawAction;
            window.isEmptyFunc = isEmptyFunc;

            LastFocusedInspectorWindow.DockNextTo(window);
        }

        private void OnFocus() {
            LastFocusedInspectorWindow.lastFocused = this;
            if (!_subscribed) {
                EditorApplication.update += Draw;
                _subscribed = true;
            }
        }

        private void OnDisable() {
            if (_subscribed) {
                EditorApplication.update -= Draw;
                _subscribed = false;
            }
        }

        private void OnDestroy() {
            var key = (worldType, id);
            if (data.TryGetValue(key, out var window) && window == this) {
                if (window.provider && window.provider.gameObject) {
                    Destroy(window.provider.gameObject);
                }
                data.Remove(key);
            }
        }

        private void OnGUI() {
            if (Application.isPlaying) {
                if (isEmptyFunc != null && isEmptyFunc(this)) {
                    EditorGUILayout.HelpBox("Entity is destroyed or not actual", MessageType.Info, true);
                } else {
                    drawAction?.Invoke(this);
                }
            } else {
                EditorGUILayout.HelpBox("Data is only available in play mode", MessageType.Info, true);
            }
        }
    }

    public static class EventInspectorHelper<TWorld, TProvider>
        where TWorld : struct, IWorldType
        where TProvider : StaticEcsEventProvider<TWorld> {

        public static void ShowWindowForEvent(in EventData eventData) {
            ShowWindowForEvent(new RuntimeEvent {
                InternalIdx = eventData.InternalIdx,
                Status = eventData.EventStatus,
                Type = eventData.TypeIdx.Type
            }, eventData.CachedData);
        }

        public static void ShowWindowForEvent(in RuntimeEvent runtimeEvent, IEvent cachedEvent) {
            EventInspectorWindow.ShowWindow(typeof(TWorld), in runtimeEvent, cachedEvent,
                CreateProvider, DrawEvent, IsEmpty);
        }

        private static AbstractStaticEcsProvider CreateProvider(RuntimeEvent runtimeEvent, IEvent cachedEvent) {
            var go = new GameObject("StaticEcsEventDebugView") {
                hideFlags = HideFlags.NotEditable,
            };
            UnityEngine.Object.DontDestroyOnLoad(go);
            var provider = go.AddComponent<TProvider>();
            provider.RuntimeEvent = runtimeEvent;
            provider.EventCache = cachedEvent;
            provider.UsageType = UsageType.Manual;
            provider.OnCreateType = OnCreateType.None;
            return provider;
        }

        private static void DrawEvent(EventInspectorWindow window) {
            Drawer.DrawEvent<TWorld, TProvider>((TProvider) window.provider, DrawMode.Viewer, _ => { }, _ => {});
        }

        private static bool IsEmpty(EventInspectorWindow window) {
            return ((TProvider) window.provider).RuntimeEvent.IsEmpty();
        }
    }
}
