using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace FFS.Libraries.StaticEcs.Unity.Editor {
    public class EntityInspectorWindow : EditorWindow {
        private static readonly Dictionary<Type, Dictionary<uint, EntityInspectorWindow>> data = new();
        private static EntityInspectorWindow lastFocused;

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

                lastFocused = null;
            };
        }

        private void Draw() {
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
            
            if (lastFocused) {
                DockNextTo(window, lastFocused);
            } else {
                window.Show();
            }
        }

        private static void DockNextTo(EditorWindow windowToDock, EditorWindow target) {
            var parentField = typeof(EditorWindow).GetField("m_Parent", BindingFlags.Instance | BindingFlags.NonPublic);
            var targetParent = parentField.GetValue(target);
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
            lastFocused = this;
        }
        
        private void OnLostFocus() {
            EditorApplication.update -= Draw;
        }

        private void OnDestroy() {
            if (data.TryGetValue(_world.GetWorldType(), out var entities)) {
                if (entities.TryGetValue(_entity.GetId(), out var window) && window == this) {
                    Destroy(window._provider);
                    entities.Remove(_entity.GetId());
                }
            }
        }

        private void OnGUI() {
            if (Application.isPlaying) {
                Drawer.DrawEntity(_provider, true, _ => { }, false, provider => Close());
            } else {
                EditorGUILayout.HelpBox("Data is only available in play mode", MessageType.Info, true);
            }
        }
    }
}