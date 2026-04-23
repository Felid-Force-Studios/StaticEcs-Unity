#if ((DEBUG || FFS_ECS_ENABLE_DEBUG) && !FFS_ECS_DISABLE_DEBUG)
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FFS.Libraries.StaticEcs.Unity.Editor {
    public class StaticEcsViewSystemsTab<TWorld> : IStaticEcsViewTab
        where TWorld : struct, IWorldType {
        private static Dictionary<int, string> _formattedTime = new();

        private AbstractWorldData _currentWorldData;
        private Vector2 _verticalScroll = Vector2.zero;

        public string Name() => "Systems";

        public void Init() { }

        public void Draw() {
            if (_currentWorldData == null) {
                return;
            }

            _verticalScroll = EditorGUILayout.BeginScrollView(_verticalScroll);

            var handles = _currentWorldData.Handle.GetAllSystemsHandles();
            for (var h = 0; h < handles.Count; h++) {
                var handle = handles[h];
                var systems = handle.GetAllSystems();
                if (systems.Length == 0) {
                    continue;
                }

                DrawGroup(handle.SystemsType, systems);
            }

            EditorGUILayout.EndScrollView();
        }

        public void Destroy() { }

        public void OnWorldChanged(AbstractWorldData newWorldData) {
            _currentWorldData = newWorldData;
        }

        public void SaveState(WorldViewSettings settings) { }

        public void LoadState(WorldViewSettings settings) { }

        private static void DrawGroup(Type systemsType, Span<SystemData> systems) {
            DrawHeaderLabel($"Group: {systemsType.EditorTypeName()}");

            var idPrefix = systemsType.FullName ?? systemsType.EditorTypeName();

            for (var i = 0; i < systems.Length; i++) {
                ref var systemData = ref systems[i];
                DrawSystem(idPrefix, i, ref systemData);
            }

            EditorGUILayout.Space();
            Ui.DrawHorizontalSeparator(Ui.Width((int) (Math.Round((EditorGUIUtility.currentViewWidth - 30f) / (double) 5) * 5)));
            EditorGUILayout.Space();
        }

        private static void DrawSystem(string idPrefix, int index, ref SystemData systemData) {
            var systemType = systemData.System.GetType();
            var systemName = systemType.EditorTypeName();
            var foldoutKey = HashCode.Combine("SYS_", idPrefix, index);

            EditorGUILayout.BeginHorizontal(GUI.skin.box);
            {
                if (systemData.HasUpdate) {
                    var isActive = systemData.HasUpdateIsActive ? systemData.System.UpdateIsActive() : true;
                    var effectiveActive = isActive && !systemData.DebugDisabled;
                    var newActive = EditorGUILayout.Toggle(effectiveActive, Ui.WidthLine(16));
                    if (newActive != effectiveActive) {
                        systemData.DebugDisabled = !newActive;
                    }
                } else {
                    EditorGUILayout.LabelField(GUIContent.none, Ui.WidthLine(16));
                }

                Drawer.DrawFoldoutBox(foldoutKey, systemName, systemName, out var show);

                EditorGUILayout.BeginVertical(GUILayout.MinWidth(120));
                EditorGUILayout.BeginHorizontal();
                DrawMarker("I", systemData.HasInit);
                DrawMarker("U", systemData.HasUpdate);
                DrawMarker("D", systemData.HasDestroy);
                if (systemData.HasUpdate) {
                    EditorGUILayout.LabelField(FormatTime(systemData.AvgUpdateTime), Ui.LabelStyleThemeBold, Ui.WidthLine(70));
                } else {
                    EditorGUILayout.LabelField(GUIContent.none, Ui.WidthLine(70));
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();

                EditorGUILayout.EndHorizontal();

                if (show) {
                    EditorGUILayout.BeginVertical(GUI.skin.box);
                    MetaData.DrawSourceField(systemType);
                    if (systemType.IsSerializable) {
                        DrawSystemFields(foldoutKey, systemData.System);
                    }
                    EditorGUILayout.EndVertical();
                }
            }
        }

        private static void DrawMarker(string label, bool active) {
            var prevColor = GUI.color;
            GUI.color = active ? prevColor : new Color(prevColor.r, prevColor.g, prevColor.b, 0.25f);
            EditorGUILayout.LabelField(label, Ui.LabelStyleThemeBold, Ui.WidthLine(16));
            GUI.color = prevColor;
        }

        private static void DrawSystemFields(int wrapperKey, ISystem system) {
            var wrapper = SystemDrawerWrapper.GetFor(wrapperKey);
            wrapper.value = system;
            using var so = new SerializedObject(wrapper);
            var prop = so.FindProperty("value");

            if (prop != null && prop.propertyType == SerializedPropertyType.ManagedReference) {
                Drawer.DrawSerializedPropertyChildren(prop);
                so.ApplyModifiedProperties();
            }
        }

        private static void DrawHeaderLabel(string name) {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(name, Ui.LabelStyleThemeBold);
            EditorGUILayout.EndHorizontal();
            Ui.DrawHorizontalSeparator(Ui.Width((int) (Math.Round((EditorGUIUtility.currentViewWidth - 30f) / (double) 5) * 5)));
        }

        private static string FormatTime(float avgTime) {
            var key = (int) (avgTime * 100);
            if (!_formattedTime.TryGetValue(key, out var str)) {
                str = $"{avgTime:F2} ms";
                _formattedTime[key] = str;
            }
            return str;
        }
    }
}
#endif
