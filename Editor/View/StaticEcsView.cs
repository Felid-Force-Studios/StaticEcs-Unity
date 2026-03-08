using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FFS.Libraries.StaticEcs.Unity.Editor {

    public abstract class StaticEcsView<TWorld, TEntityProvider, TEventProvider> : EditorWindow 
        where TWorld : struct, IWorldType
        where TEntityProvider : StaticEcsEntityProvider<TWorld>
        where TEventProvider : StaticEcsEventProvider<TWorld> {

        private readonly List<IStaticEcsViewTab> _tabs = new();
        private IStaticEcsViewTab _selectedTab;

        private AbstractWorldData _currentWorldData;

        internal float drawRate = 0.5f;
        internal float drawFrames = 2;
        private float _acc;

        private bool _initialized;

        public void Init() {
            if (!_initialized || _tabs.Count == 0) {
                titleContent = new GUIContent($"Static ECS - {typeof(TWorld).Name}");

                #if ((DEBUG || FFS_ECS_ENABLE_DEBUG) && !FFS_ECS_DISABLE_DEBUG)
                _tabs.Add(new StaticEcsViewEntitiesTab<TWorld, TEntityProvider>());
                _tabs.Add(new StaticEcsViewStatsTab<TWorld>());
                _tabs.Add(new StaticEcsViewEventsTab<TWorld, TEventProvider>());
                _tabs.Add(new StaticEcsViewContextTab<TWorld, TEntityProvider>());
                _tabs.Add(new StaticEcsViewSystemsTab<TWorld>());
                #endif

                foreach (var tab in _tabs) {
                    tab.Init();
                }

                if (_tabs.Count > 0) {
                    _selectedTab = _tabs[0];
                }

                EntityInspectorRegistry.ShowEntityHandlers[typeof(TWorld)] = EntityInspectorHelper<TWorld, TEntityProvider>.ShowWindowForEntity;

                _initialized = true;
            }
        }

        private void OnEnable() {
            EditorApplication.update += Draw;
        }

        private void Draw() {
            _acc += Time.deltaTime;
            if (_acc >= drawRate) {
                Repaint();
                _acc = 0f;
            }
        }

        private void OnGUI() {
            if (!Application.isPlaying) {
                EditorGUILayout.HelpBox("Data is only available in play mode", MessageType.Info);
                return;
            }

            Init();

            if (_currentWorldData == null) {
                if (StaticEcsDebugData.Worlds.TryGetValue(typeof(TWorld), out var worldData)) {
                    SetWorldData(worldData);
                }
            }

            if (_currentWorldData == null) {
                EditorGUILayout.HelpBox($"World {typeof(TWorld).Name} is not registered. Call EcsDebug<{typeof(TWorld).Name}>.AddWorld()", MessageType.Warning);
                return;
            }

            if (_currentWorldData.Handle.Status() != WorldStatus.Initialized) {
                EditorGUILayout.HelpBox("World not initialized", MessageType.Info);
                return;
            }

            EditorGUILayout.Space(10);
            _selectedTab?.DrawHeader();
            EditorGUILayout.Space(10);

            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            {
                foreach (var tab in _tabs) {
                    if (GUILayout.Toggle(_selectedTab == tab, tab.Name(), Ui.ButtonStyleTheme, Ui.WidthLine(90))) {
                        if (_selectedTab != tab) {
                            GUI.FocusControl("");
                            _selectedTab = tab;
                        }
                    }
                }
            }
            GUILayout.EndHorizontal();
            EditorGUILayout.Space();

            _selectedTab?.Draw();
        }

        private void SetWorldData(AbstractWorldData data) {
            MetaData.EnrichByWorld(data.Handle);

            _currentWorldData = data;

            foreach (var tab in _tabs) {
                tab.OnWorldChanged(_currentWorldData);
            }
        }

        private void OnDisable() {
            EditorApplication.update -= Draw;
            foreach (var tab in _tabs) {
                tab.Destroy();
            }

            if (Application.isPlaying) {
                Destroy(this);
            }
        }
    }

    public interface IStaticEcsViewTab {
        public string Name();
        public void OnWorldChanged(AbstractWorldData newWorldData);
        public void Draw();
        public void DrawHeader() {}
        public void Init();
        public void Destroy();
    }
}
