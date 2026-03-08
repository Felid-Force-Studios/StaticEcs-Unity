#if ((DEBUG || FFS_ECS_ENABLE_DEBUG) && !FFS_ECS_DISABLE_DEBUG)
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FFS.Libraries.StaticEcs.Unity.Editor {
    public class StaticEcsViewSystemsTab<TWorld> : IStaticEcsViewTab
        where TWorld : struct, IWorldType {
        internal enum TabType: byte {
            Init,
            Update,
            Destroy
        }

        private static readonly TabType[] _tabs = { TabType.Init, TabType.Update, TabType.Destroy };
        private static readonly string[] _tabsNames = { "Init", "Update", "Destroy" };
        internal TabType SelectedTab;

        private Dictionary<Type, SystemDrawer<TWorld>> _drawersBySystemIdType = new();
        private SystemDrawer<TWorld> _currentDrawer;
        private AbstractWorldData _currentWorldData;

        public string Name() => "Systems";

        public void Init() { }

        public void DrawHeader() {
            CheckDrawer();
            if (_currentDrawer != null) {
                DrawSystemsSelector();
            }
        }

        public void Draw() {
            CheckDrawer();

            if (_currentDrawer != null) {
                Ui.DrawToolbar(_tabs, ref SelectedTab, type => _tabsNames[(int) type]);
                _currentDrawer.Draw();
            }
        }

        private void CheckDrawer() {
            if (_currentDrawer == null && _currentWorldData != null) {
                foreach (var drawer in _drawersBySystemIdType) {
                    if (drawer.Value.Systems.worldType == _currentWorldData.Handle.WorldType) {
                        _currentDrawer = drawer.Value;
                        break;
                    }
                }

                foreach (var typeToSystem in StaticEcsDebugData.Systems) {
                    if (typeToSystem.Value.worldType == _currentWorldData.Handle.WorldType) {
                        _currentDrawer = new SystemDrawer<TWorld> {
                            Parent = this,
                            SysIdType = typeToSystem.Key,
                            Systems = typeToSystem.Value,
                        };

                        _drawersBySystemIdType[typeToSystem.Key] = _currentDrawer;
                        break;
                    }
                }
            }
        }

        public void Destroy() { }

        public void OnWorldChanged(AbstractWorldData newWorldData) {
            _currentWorldData = newWorldData;
            _currentDrawer = null;
        }

        internal void DrawSystemsSelector() {
            if (GUILayout.Button(new GUIContent("Systems:", EditorGUIUtility.IconContent("d_Preset.Context").image), Ui.IconButtonStretchedStyle, Ui.ExpandWidthFalse())) {
                var menu = new GenericMenu();
                foreach (var drawer in _drawersBySystemIdType) {
                    if (drawer.Value.Systems.worldType == _currentWorldData.Handle.WorldType && drawer.Value != _currentDrawer) {
                        menu.AddItem(new GUIContent(drawer.Value.SysIdType.EditorTypeName()), false, () => { _currentDrawer = drawer.Value; });
                    }
                }

                foreach (var typeToSystem in StaticEcsDebugData.Systems) {
                    if (typeToSystem.Value.worldType == _currentWorldData.Handle.WorldType && !_drawersBySystemIdType.ContainsKey(typeToSystem.Key)) {
                        menu.AddItem(new GUIContent(typeToSystem.Key.EditorTypeName()), false, () => {
                            _currentDrawer = new SystemDrawer<TWorld> {
                                Parent = this,
                                SysIdType = typeToSystem.Key,
                                Systems = typeToSystem.Value,
                            };

                            _drawersBySystemIdType[typeToSystem.Key] = _currentDrawer;
                        });
                    }
                }
                menu.ShowAsContext();
            }
            GUILayout.Label(_currentDrawer.SysIdType.Name, Ui.LabelStyleThemeBold2, Ui.ExpandWidthFalse());
        }
    }

    public class SystemDrawer<TWorld> where TWorld : struct, IWorldType {
        internal StaticEcsViewSystemsTab<TWorld> Parent;
        internal Type SysIdType;
        internal (SystemData[] systems, int count, Type worldType) Systems;

        private static Dictionary<int, string> _formattedTime = new();
        private Vector2 verticalScroll = Vector2.zero;
        private int _drawLevel = 10;

        private static string FormatTime(float avgTime) {
            var key = (int)(avgTime * 100);
            if (!_formattedTime.TryGetValue(key, out var str)) {
                str = $"{avgTime:F2} ms";
                _formattedTime[key] = str;
            }
            return str;
        }

        private bool DrawSystemNameFoldout(int index, SystemData systemData) {
            var systemType = systemData.System.GetType();
            var systemName = systemType.EditorTypeName();
            var foldoutKey = HashCode.Combine("SYS_", SysIdType.FullName, index);
            var isOpen = Drawer.openHideFlags.Contains(foldoutKey);

            var style = new GUIStyle(EditorStyles.boldLabel) {
                hover = EditorStyles.iconButton.hover,
                active = EditorStyles.iconButton.active,
                focused = EditorStyles.iconButton.focused,
            };

            var rect = EditorGUILayout.GetControlRect(Ui.WidthLine(400));
            rect = EditorGUI.IndentedRect(rect);
            using (Ui.EnabledScope) {
                if (GUI.Button(rect, (isOpen ? "▾ " : "▸ ") + systemName, style)) {
                    if (isOpen) Drawer.openHideFlags.Remove(foldoutKey);
                    else Drawer.openHideFlags.Add(foldoutKey);
                    isOpen = !isOpen;
                }
            }

            return isOpen;
        }

        private void DrawSystemFields(SystemData systemData) {
            var systemType = systemData.System.GetType();
            var systemName = systemType.EditorTypeName();
            _drawLevel = 10;
            EditorGUILayout.BeginVertical(GUI.skin.box);
            Drawer.TryDrawObject(ref _drawLevel, systemName, systemType, systemData.System, out _);
            EditorGUILayout.EndVertical();
        }

        internal void Draw() {
            switch (Parent.SelectedTab) {
                case StaticEcsViewSystemsTab<TWorld>.TabType.Init:
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.SelectableLabel("System", Ui.LabelStyleThemeCenter, Ui.WidthLine(400));
                    Ui.DrawSeparator();
                    EditorGUILayout.EndHorizontal();
                    Ui.DrawHorizontalSeparator(410f);
                    verticalScroll = EditorGUILayout.BeginScrollView(verticalScroll);

                    for (var i = 0; i < Systems.count; i++) {
                        var systemData = Systems.systems[i];
                        if (systemData.HasInit) {
                            EditorGUILayout.BeginHorizontal();
                            var isOpen = DrawSystemNameFoldout(i, systemData);
                            Ui.DrawSeparator();
                            EditorGUILayout.EndHorizontal();
                            if (isOpen) {
                                DrawSystemFields(systemData);
                            }
                            Ui.DrawHorizontalSeparator(410f);
                        }
                    }
                    EditorGUILayout.EndScrollView();

                    break;
                case StaticEcsViewSystemsTab<TWorld>.TabType.Update:
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.SelectableLabel("Active", Ui.LabelStyleThemeCenter, Ui.WidthLine(37));
                    Ui.DrawSeparator();
                    EditorGUILayout.SelectableLabel("System", Ui.LabelStyleThemeCenter, Ui.WidthLine(400));
                    Ui.DrawSeparator();
                    EditorGUILayout.SelectableLabel("Time avg", Ui.LabelStyleThemeCenter, Ui.WidthLine(60));
                    Ui.DrawSeparator();
                    EditorGUILayout.EndHorizontal();
                    Ui.DrawHorizontalSeparator(540f);
                    verticalScroll = EditorGUILayout.BeginScrollView(verticalScroll);

                    for (var i = 0; i < Systems.count; i++) {
                        ref var systemData = ref Systems.systems[i];
                        if (!systemData.HasUpdate) continue;

                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField(GUIContent.none, Ui.LabelStyleThemeBold, Ui.WidthLine(10));
                        var isActive = systemData.HasUpdateIsActive ? systemData.System.UpdateIsActive() : true;
                        var effectiveActive = isActive && !systemData.DebugDisabled;
                        var newActive = EditorGUILayout.Toggle(effectiveActive, Ui.WidthLine(10));
                        if (newActive != effectiveActive) {
                            systemData.DebugDisabled = !newActive;
                        }
                        EditorGUILayout.LabelField(GUIContent.none, Ui.LabelStyleThemeBold, Ui.WidthLine(10));
                        Ui.DrawSeparator();

                        var isOpen = DrawSystemNameFoldout(i, systemData);
                        Ui.DrawSeparator();

                        EditorGUILayout.LabelField(FormatTime(systemData.AvgUpdateTime), Ui.LabelStyleThemeBold, Ui.WidthLine(60));
                        Ui.DrawSeparator();
                        EditorGUILayout.EndHorizontal();
                        if (isOpen) {
                            DrawSystemFields(systemData);
                        }
                    }
                    Ui.DrawHorizontalSeparator(540f);
                    EditorGUILayout.EndScrollView();

                    break;
                case StaticEcsViewSystemsTab<TWorld>.TabType.Destroy:
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.SelectableLabel("System", Ui.LabelStyleThemeCenter, Ui.WidthLine(400));
                    Ui.DrawSeparator();
                    EditorGUILayout.EndHorizontal();
                    Ui.DrawHorizontalSeparator(410f);
                    verticalScroll = EditorGUILayout.BeginScrollView(verticalScroll);

                    for (var i = 0; i < Systems.count; i++) {
                        var systemData = Systems.systems[i];
                        if (systemData.HasDestroy) {
                            EditorGUILayout.BeginHorizontal();
                            var isOpen = DrawSystemNameFoldout(i, systemData);
                            Ui.DrawSeparator();
                            EditorGUILayout.EndHorizontal();
                            if (isOpen) {
                                DrawSystemFields(systemData);
                            }
                            Ui.DrawHorizontalSeparator(410f);
                        }
                    }

                    EditorGUILayout.EndScrollView();
                    break;
            }
        }
    }
}
#endif
