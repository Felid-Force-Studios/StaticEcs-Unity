using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FFS.Libraries.StaticEcs.Unity.Editor {
    public class StaticEcsView : EditorWindow {
        private static StaticEcsView _instance;

        [MenuItem("Window/Static ECS")]
        public static void OpenWindow() {
            if (!_instance) {
                _instance = CreateInstance<StaticEcsView>();
            }
            _instance.Show();
            _instance.Focus();
        }

        private readonly List<IStaticEcsViewTab> _tabs = new();
        private IStaticEcsViewTab _selectedTab;

        private readonly Dictionary<Type, AbstractWorldData> _WorldTypeTypeToData = new();
        private AbstractWorldData _currentWorldData;

        internal float drawRate = 0.5f;
        internal float drawFrames = 2;
        private float _acc;
        
        private bool _initialized;

        public void Init() {
            if (!_instance) {
                _instance = this;
            }

            if (!_initialized || _tabs.Count == 0) {
                titleContent = new GUIContent("Static ECS");

                _tabs.Add(new StaticEcsViewEntitiesTab());
                _tabs.Add(new StaticEcsViewStatsTab());
                #if !FFS_ECS_DISABLE_EVENTS
                _tabs.Add(new StaticEcsViewEventsTab());
                #endif
                _tabs.Add(new StaticEcsViewContextTab());
                _tabs.Add(new StaticEcsViewSystemsTab());
                _tabs.Add(new StaticEcsViewSettingsTab());
                
                foreach (var tab in _tabs) {
                    tab.Init();
                }
                _selectedTab = _tabs[0];
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
                foreach (var typeToWorld in StaticEcsDebugData.Worlds) {
                    SetWorldData(typeToWorld.Value, typeToWorld.Key);
                    break;
                }
            }
            
            if (_currentWorldData == null) return;

            if (_currentWorldData.World.Status() != WorldStatus.Initialized) {
                EditorGUILayout.HelpBox("World not initialized", MessageType.Info);
            GUILayout.BeginHorizontal();
                
                DrawWorldSelector();
            GUILayout.EndHorizontal();
                
                return;
            }
            
            EditorGUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();
            DrawWorldSelector();
            _selectedTab.DrawHeader(this);
            EditorGUILayout.EndHorizontal();
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
            
            _selectedTab.Draw(this);
        }

        internal static void DrawWorldSelector() {
            if (_instance == null) return;

            // GUILayout.BeginHorizontal();
            // {
                using (Ui.EnabledScopeVal(MetaData.WorldsMetaData.Count > 1)) {
                    if (GUILayout.Button(new GUIContent("World: ", EditorGUIUtility.IconContent("d_Preset.Context").image), Ui.IconButtonStretchedStyle, Ui.ExpandWidthFalse())) {
                        var menu = new GenericMenu();
                        foreach (var typeToWorld in StaticEcsDebugData.Worlds) {
                            if (_instance._currentWorldData.WorldTypeTypeFullName != null && typeToWorld.Key.FullName == _instance._currentWorldData.WorldTypeTypeFullName) {
                                continue;
                            }

                            var editorName = MetaData.WorldsMetaData.Find(t => t.WorldTypeType == typeToWorld.Key).EditorName;
                            menu.AddItem(
                                new GUIContent(editorName),
                                false,
                                () => { _instance.SetWorldData(typeToWorld.Value, typeToWorld.Key); });
                        }

                        menu.ShowAsContext();
                    }
                }

                GUILayout.Label(_instance._currentWorldData.worldEditorName, Ui.LabelStyleThemeBold2, Ui.ExpandWidthFalse());
            // }
            // GUILayout.EndHorizontal();
        }

        private void SetWorldData(AbstractWorldData data, Type type) {
            if (!_WorldTypeTypeToData.ContainsKey(type)) {
                data.worldEditorName = MetaData.WorldsMetaData.Find(t => t.WorldTypeType == type).EditorName;
                data.WorldTypeTypeFullName = type.FullName;
                _WorldTypeTypeToData[type] = data;
                MetaData.EnrichByWorld(data.World);
            }
            
            _currentWorldData =  _WorldTypeTypeToData[type];
            
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
            _instance = null;
        }
    }

    public interface IStaticEcsViewTab {
        public string Name();
        public void OnWorldChanged(AbstractWorldData newWorldData);
        public void Draw(StaticEcsView view);
        public virtual void DrawHeader(StaticEcsView view) {}
        public void Init();
        public void Destroy();
    }
}