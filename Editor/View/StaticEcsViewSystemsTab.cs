using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEditor;
using UnityEngine;

namespace FFS.Libraries.StaticEcs.Unity.Editor {
    public class StaticEcsViewSystemsTab : IStaticEcsViewTab {
        internal enum TabType: byte {
            Init,
            Update,
            Destroy
        }
        
        private static readonly TabType[] _tabs = { TabType.Init, TabType.Update, TabType.Destroy };
        private static readonly string[] _tabsNames = { "Init", "Update", "Destroy" };
        internal TabType SelectedTab;

        private Dictionary<Type, SystemDrawer> _drawersBySystemIdType = new();
        private SystemDrawer _currentDrawer;
        private AbstractWorldData _currentWorldData;

        public string Name() => "Systems";

        public void Init() { }

        public void Draw(StaticEcsView view) {
            if (_currentDrawer == null && _currentWorldData != null) {
                foreach (var drawer in _drawersBySystemIdType) {
                    if (drawer.Value.Systems.worldType == _currentWorldData.WorldTypeType) {
                        _currentDrawer = drawer.Value;
                        break;
                    }
                }

                foreach (var typeToSystem in StaticEcsDebugData.Systems) {
                    if (typeToSystem.Value.worldType == _currentWorldData.WorldTypeType) {
                        _currentDrawer = new SystemDrawer {
                            Parent = this,
                            SysIdType = typeToSystem.Key,
                            Systems = typeToSystem.Value,
                            SystemsInfo = new List<SystemInfo>[typeToSystem.Value.systems.Length],
                        };

                        _drawersBySystemIdType[typeToSystem.Key] = _currentDrawer;
                        break;
                    }
                }
            }
            
            StaticEcsView.DrawWorldSelector();
            if (_currentDrawer != null) {
                DrawSystemsSelector();
                EditorGUILayout.Space(10);
                Ui.DrawToolbar(_tabs, ref SelectedTab, type => _tabsNames[(int) type]);
                _currentDrawer.Draw();
            }
        }

        public void Destroy() { }

        public void OnWorldChanged(AbstractWorldData newWorldData) {
            _currentWorldData = newWorldData;
            _currentDrawer = null;
        }
        
        internal void DrawSystemsSelector() {
            GUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField("Systems", Ui.WidthLine(120));
                EditorGUILayout.SelectableLabel(_currentDrawer.SysIdType.Name, Ui.LabelStyleWhiteBold, Ui.WidthLine(120));
                if (GUILayout.Button("Change", Ui.ButtonStyleWhite, Ui.WidthLine(60))) {
                    var menu = new GenericMenu();
                    foreach (var drawer in _drawersBySystemIdType) {
                        if (drawer.Value.Systems.worldType == _currentWorldData.WorldTypeType && drawer.Value != _currentDrawer) {
                            menu.AddItem(new GUIContent(drawer.Value.SysIdType.EditorTypeName()), false, () => { _currentDrawer = drawer.Value; });
                        }
                    }

                    foreach (var typeToSystem in StaticEcsDebugData.Systems) {
                        if (typeToSystem.Value.worldType == _currentWorldData.WorldTypeType && !_drawersBySystemIdType.ContainsKey(typeToSystem.Key)) {
                            menu.AddItem(new GUIContent(typeToSystem.Key.EditorTypeName()), false, () => {
                                _currentDrawer = new SystemDrawer {
                                    Parent = this,
                                    SysIdType = typeToSystem.Key,
                                    Systems = typeToSystem.Value,
                                    SystemsInfo = new List<SystemInfo>[typeToSystem.Value.systems.Length],
                                };

                                _drawersBySystemIdType[typeToSystem.Key] = _currentDrawer;
                            });
                        }
                    }
                    menu.ShowAsContext();
                }
            }
            GUILayout.EndHorizontal();
        }
    }

    public class SystemDrawer {
        internal StaticEcsViewSystemsTab Parent;
        internal Type SysIdType;
        internal ((ISystem system, short order)[] systems, int count, Type worldType) Systems;
        internal List<SystemInfo>[] SystemsInfo;

        private static Dictionary<int, string> _formattedTime = new();

        private static readonly Type IInitSystemType = typeof(IInitSystem);
        private static readonly Type ISystemsBatchType = typeof(ISystemsBatch);
        private static readonly Type IDestroySystemType = typeof(IDestroySystem);
        private Vector2 verticalScroll = Vector2.zero;

        internal void Draw() {
            switch (Parent.SelectedTab) {
                case StaticEcsViewSystemsTab.TabType.Init:
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.SelectableLabel("System", Ui.LabelStyleWhiteCenter, Ui.WidthLine(400));
                    Ui.DrawSeparator();
                    EditorGUILayout.EndHorizontal();
                    Ui.DrawHorizontalSeparator(410f);
                    verticalScroll = EditorGUILayout.BeginScrollView(verticalScroll);
                    
                    for (var i = 0; i < Systems.count; i++) {
                        var item = Systems.systems[i].system;
                        var itemType = item.GetType();
                        if (IInitSystemType.IsAssignableFrom(itemType)) {
                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.SelectableLabel(itemType.EditorTypeName(), Ui.LabelStyleWhiteBold, Ui.WidthLine(400));
                            Ui.DrawSeparator();
                            EditorGUILayout.EndHorizontal();
                            Ui.DrawHorizontalSeparator(410f);
                        }
                        if (ISystemsBatchType.IsAssignableFrom(itemType)) {
                            var batch = (ISystemsBatch) item;
                            SystemsInfo[i] ??= new List<SystemInfo>();
                            batch.Info(SystemsInfo[i]);

                            bool hasInitSystems = false;
                            for (var sysId = 0; sysId < SystemsInfo[i].Count; sysId++) {
                                var systemInfo = SystemsInfo[i][sysId];
                                if (systemInfo.InitSystem) {
                                    hasInitSystems = true;
                                    EditorGUILayout.BeginHorizontal();
                                    EditorGUILayout.SelectableLabel(systemInfo.SystemType.EditorTypeName(), Ui.LabelStyleWhiteBold, Ui.WidthLine(400));
                                    Ui.DrawSeparator();
                                    EditorGUILayout.EndHorizontal();
                                }
                            }

                            if (hasInitSystems) {
                                Ui.DrawHorizontalSeparator(410f);
                            }
                        }
                    }
                    EditorGUILayout.EndScrollView();

                    break;
                case StaticEcsViewSystemsTab.TabType.Update:
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.SelectableLabel("Active", Ui.LabelStyleWhiteCenter, Ui.WidthLine(37));
                    Ui.DrawSeparator();
                    EditorGUILayout.SelectableLabel("System", Ui.LabelStyleWhiteCenter, Ui.WidthLine(400));
                    Ui.DrawSeparator();
                    EditorGUILayout.SelectableLabel("Time avg", Ui.LabelStyleWhiteCenter, Ui.WidthLine(60));
                    Ui.DrawSeparator();
                    EditorGUILayout.EndHorizontal();
                    Ui.DrawHorizontalSeparator(540f);
                    verticalScroll = EditorGUILayout.BeginScrollView(verticalScroll);

                    for (var i = 0; i < Systems.count; i++) {
                        var item = Systems.systems[i].system;
                        var itemType = item.GetType();
                        if (ISystemsBatchType.IsAssignableFrom(itemType)) {
                            var batch = (ISystemsBatch) item;
                            SystemsInfo[i] ??= new List<SystemInfo>();
                            batch.Info(SystemsInfo[i]);
                            
                            for (var sysId = 0; sysId < SystemsInfo[i].Count; sysId++) {
                                var systemInfo = SystemsInfo[i][sysId];
                                EditorGUILayout.BeginHorizontal();
                                EditorGUILayout.LabelField(GUIContent.none, Ui.LabelStyleWhiteBold, Ui.WidthLine(10));
                                var newEnabled = EditorGUILayout.Toggle(systemInfo.Enabled, Ui.WidthLine(10));
                                EditorGUILayout.LabelField(GUIContent.none, Ui.LabelStyleWhiteBold, Ui.WidthLine(10));
                                Ui.DrawSeparator();
                                if (newEnabled != systemInfo.Enabled) {
                                    batch.SetActive(sysId, newEnabled);
                                }
                                EditorGUILayout.SelectableLabel(systemInfo.SystemType.EditorTypeName(), Ui.LabelStyleWhiteBold, Ui.WidthLine(400));
                                Ui.DrawSeparator();

                                if (!_formattedTime.TryGetValue((int) (systemInfo.AvgUpdateTime * 1000), out var time)) {
                                    time = $"{systemInfo.AvgUpdateTime.ToString("F3", CultureInfo.InvariantCulture)} ms";
                                    _formattedTime[(int) (systemInfo.AvgUpdateTime * 1000)] = time;
                                }
                                
                                EditorGUILayout.LabelField(time, Ui.LabelStyleWhiteBold, Ui.WidthLine(60));
                                Ui.DrawSeparator();
                                EditorGUILayout.EndHorizontal();
                            }
                            Ui.DrawHorizontalSeparator(540f);
                        }
                    }
                    EditorGUILayout.EndScrollView();

                    break;
                case StaticEcsViewSystemsTab.TabType.Destroy:
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.SelectableLabel("System", Ui.LabelStyleWhiteCenter, Ui.WidthLine(400));
                    Ui.DrawSeparator();
                    EditorGUILayout.EndHorizontal();
                    Ui.DrawHorizontalSeparator(410f);
                    verticalScroll = EditorGUILayout.BeginScrollView(verticalScroll);

                    for (var i = 0; i < Systems.count; i++) {
                        var item = Systems.systems[i].system;
                        var itemType = item.GetType();
                        if (IDestroySystemType.IsAssignableFrom(itemType)) {
                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.SelectableLabel(itemType.EditorTypeName(), Ui.LabelStyleWhiteBold, Ui.WidthLine(400));
                            Ui.DrawSeparator();
                            EditorGUILayout.EndHorizontal();
                            Ui.DrawHorizontalSeparator(410f);
                        }
                        if (ISystemsBatchType.IsAssignableFrom(itemType)) {
                            var batch = (ISystemsBatch) item;
                            SystemsInfo[i] ??= new List<SystemInfo>();
                            batch.Info(SystemsInfo[i]);

                            var hasDestroySystems = false;
                            for (var sysId = 0; sysId < SystemsInfo[i].Count; sysId++) {
                                var systemInfo = SystemsInfo[i][sysId];
                                if (systemInfo.DestroySystem) {
                                    hasDestroySystems = true;
                                    EditorGUILayout.BeginHorizontal();
                                    EditorGUILayout.SelectableLabel(systemInfo.SystemType.EditorTypeName(), Ui.LabelStyleWhiteBold, Ui.WidthLine(400));
                                    Ui.DrawSeparator();
                                    EditorGUILayout.EndHorizontal();
                                }
                            }

                            if (hasDestroySystems) {
                                Ui.DrawHorizontalSeparator(410f);
                            }
                        }
                    }

                    EditorGUILayout.EndScrollView();
                    break;
            }
        }
    }
}