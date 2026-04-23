using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FFS.Libraries.StaticEcs.Unity.Editor {

    public abstract class StaticEcsView<TWorld, TEntityProvider, TEventProvider> : EditorWindow, IStaticEcsViewNavigation
        #if ((DEBUG || FFS_ECS_ENABLE_DEBUG) && !FFS_ECS_DISABLE_DEBUG)
        , IStaticEcsViewConfigHost
        #endif
        where TWorld : struct, IWorldType
        where TEntityProvider : StaticEcsEntityProvider<TWorld>
        where TEventProvider : StaticEcsEventProvider<TWorld> {

        private readonly List<IStaticEcsViewTab> _tabs = new();
        private IStaticEcsViewTab _selectedTab;
        private string _pendingTabSwitch;

        private AbstractWorldData _currentWorldData;

        internal float drawRate = 0.5f;
        internal float drawFrames = 2;
        private float _acc;

        private bool _initialized;
        private string _worldKey;

        private const float SaveInterval = 30f;
        private float _saveAcc;

        public void Init() {
            if (!_initialized || _tabs.Count == 0) {
                titleContent = new GUIContent($"Static ECS - {typeof(TWorld).Name}");
                _worldKey = typeof(TWorld).FullName;

                #if ((DEBUG || FFS_ECS_ENABLE_DEBUG) && !FFS_ECS_DISABLE_DEBUG)
                _tabs.Add(new StaticEcsViewEntitiesTab<TWorld, TEntityProvider>());
                _tabs.Add(new StaticEcsViewStatsTab<TWorld>());
                _tabs.Add(new StaticEcsViewEventsTab<TWorld, TEventProvider>());
                _tabs.Add(new StaticEcsViewContextTab<TWorld, TEntityProvider>());
                _tabs.Add(new StaticEcsViewSystemsTab<TWorld>());
                _tabs.Add(new StaticEcsViewSettingsTab<TWorld>(this));
                #endif

                var config = StaticEcsViewConfig.Active;
                var worldSettings = config.GetOrCreate(_worldKey);
                drawRate = worldSettings.main.drawRate;
                drawFrames = worldSettings.main.drawFrames;

                foreach (var tab in _tabs) {
                    tab.SetNavigation(this);
                    tab.Init();
                    tab.LoadState(worldSettings);
                }

                if (!string.IsNullOrEmpty(worldSettings.main.selectedTabName)) {
                    _pendingTabSwitch = worldSettings.main.selectedTabName;
                }

                if (_pendingTabSwitch == null && _tabs.Count > 0) {
                    _selectedTab = _tabs[0];
                }

                EntityInspectorRegistry.ShowEntityHandlers[typeof(TWorld)] = EntityInspectorHelper<TWorld, TEntityProvider>.ShowWindowForEntity;

                _initialized = true;
            }
        }

        private void OnEnable() {
            EditorApplication.update += Draw;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private void Draw() {
            if (!Application.isPlaying) return;
            _acc += Time.deltaTime;
            if (_acc >= drawRate) {
                Repaint();
                _acc = 0f;
            }

            _saveAcc += Time.deltaTime;
            if (_saveAcc >= SaveInterval) {
                _saveAcc = 0f;
                SaveAllTabs();
            }
        }

        private void OnGUI() {
            if (!Application.isPlaying) {
                EditorGUILayout.HelpBox("Data is only available in play mode", MessageType.Info);
                return;
            }

            Init();

            if (StaticEcsDebugData.Worlds.TryGetValue(typeof(TWorld), out var worldData)) {
                if (_currentWorldData != worldData) {
                    SetWorldData(worldData);
                }
            } else {
                _currentWorldData = null;
            }

            if (_currentWorldData == null) {
                EditorGUILayout.HelpBox($"World {typeof(TWorld).Name} is not registered. Call EcsDebug<{typeof(TWorld).Name}>.AddWorld()", MessageType.Warning);
                return;
            }

            if (_currentWorldData.Handle.Status() != WorldStatus.Initialized) {
                EditorGUILayout.HelpBox("World not initialized", MessageType.Info);
                return;
            }

            if (_pendingTabSwitch != null) {
                foreach (var tab in _tabs) {
                    if (tab.Name() == _pendingTabSwitch) {
                        _selectedTab = tab;
                        break;
                    }
                }
                _pendingTabSwitch = null;
            }

            DrawTabStrip();
            EditorGUILayout.Space();

            _selectedTab?.Draw();
        }

        private void DrawTabStrip() {
            const float Height = 28f;
            const float AccentThickness = 2f;
            var stripRect = GUILayoutUtility.GetRect(0, Height, GUILayout.ExpandWidth(true));
            var evt = Event.current;
            var style = Ui.TabStyle;

            if (evt.type == EventType.Repaint) {
                EditorGUI.DrawRect(stripRect, Ui.TabStripBg);
                EditorGUI.DrawRect(new Rect(stripRect.x, stripRect.yMax - 1, stripRect.width, 1), Ui.TabSeparator);
            }

            var x = stripRect.x;
            foreach (var tab in _tabs) {
                var content = new GUIContent(tab.Name());
                var size = style.CalcSize(content);
                var tabRect = new Rect(x, stripRect.y, size.x, stripRect.height);
                var isActive = _selectedTab == tab;

                switch (evt.type) {
                    case EventType.Repaint:
                        if (isActive) {
                            EditorGUI.DrawRect(tabRect, Ui.TabActiveBg);
                            EditorGUI.DrawRect(new Rect(tabRect.x, tabRect.yMax - AccentThickness, tabRect.width, AccentThickness), Ui.TabAccentColor);
                        } else if (tabRect.Contains(evt.mousePosition)) {
                            EditorGUI.DrawRect(tabRect, Ui.TabHoverBg);
                        }
                        style.Draw(tabRect, content, false, false, isActive, false);
                        break;
                    case EventType.MouseDown:
                        if (evt.button == 0 && tabRect.Contains(evt.mousePosition)) {
                            if (!isActive) {
                                GUI.FocusControl("");
                                _selectedTab = tab;
                            }
                            evt.Use();
                        }
                        break;
                }

                x += size.x;
            }
        }

        public void ReloadFromConfig(StaticEcsViewConfig config) {
            if (!_initialized || _worldKey == null) return;
            var worldSettings = config.GetOrCreate(_worldKey);
            foreach (var tab in _tabs) {
                tab.LoadState(worldSettings);
            }
            drawRate = worldSettings.main.drawRate;
            drawFrames = worldSettings.main.drawFrames;
            if (!string.IsNullOrEmpty(worldSettings.main.selectedTabName)) {
                _pendingTabSwitch = worldSettings.main.selectedTabName;
            }
        }

        private void SetWorldData(AbstractWorldData data) {
            MetaData.EnrichByWorld(data.Handle);

            _currentWorldData = data;

            foreach (var tab in _tabs) {
                tab.OnWorldChanged(_currentWorldData);
            }
        }

        public void SaveAllTabs() {
            if (!_initialized || _worldKey == null) return;
            var config = StaticEcsViewConfig.Active;
            var worldSettings = config.GetOrCreate(_worldKey);
            worldSettings.main.selectedTabName = _selectedTab?.Name();
            worldSettings.main.drawRate = drawRate;
            worldSettings.main.drawFrames = drawFrames;
            foreach (var tab in _tabs) {
                tab.SaveState(worldSettings);
            }
            config.Save();
        }

        private void OnPlayModeStateChanged(PlayModeStateChange state) {
            if (state == PlayModeStateChange.ExitingPlayMode) {
                SaveAllTabs();
                Drawer.openHideFlags.Clear();
                Drawer.initializedFoldouts.Clear();
                _currentWorldData = null;
            }
        }

        private void OnDisable() {
            EditorApplication.update -= Draw;
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            SaveAllTabs();
            foreach (var tab in _tabs) {
                tab.Destroy();
            }

            if (Application.isPlaying) {
                Destroy(this);
            }
        }

        public void SelectTab(string tabName) {
            _pendingTabSwitch = tabName;
        }
    }

    public interface IStaticEcsViewNavigation {
        void SelectTab(string tabName);
    }

    public interface IStaticEcsViewTab {
        public string Name();
        public void OnWorldChanged(AbstractWorldData newWorldData);
        public void Draw();
        public void Init();
        public void Destroy();
        public void SetNavigation(IStaticEcsViewNavigation navigation) {}
        public void SaveState(WorldViewSettings settings) {}
        public void LoadState(WorldViewSettings settings) {}
    }
}
