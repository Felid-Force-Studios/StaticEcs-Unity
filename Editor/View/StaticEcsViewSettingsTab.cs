#if ((DEBUG || FFS_ECS_ENABLE_DEBUG) && !FFS_ECS_DISABLE_DEBUG)
using UnityEditor;
using UnityEngine;

namespace FFS.Libraries.StaticEcs.Unity.Editor {
    public class StaticEcsViewSettingsTab<TWorld> : IStaticEcsViewTab
        where TWorld : struct, IWorldType {

        private readonly EditorWindow _parentWindow;
        private Vector2 _scroll;
        private WorldMetaData _worldMeta;

        internal StaticEcsViewSettingsTab(EditorWindow parentWindow) {
            _parentWindow = parentWindow;
        }

        public string Name() => "Settings";
        public void Init() {}
        public void Destroy() {}

        public void OnWorldChanged(AbstractWorldData newWorldData) {
            _worldMeta = MetaData.GetWorldMetaData(typeof(TWorld));
        }

        public void Draw() {
            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            DrawConfigSection();
            EditorGUILayout.Space(10);
            DrawFoldoutSection();
            EditorGUILayout.Space(10);
            DrawResetSection();

            EditorGUILayout.EndScrollView();
        }

        private void DrawConfigSection() {
            EditorGUILayout.LabelField("Config", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField("Config asset:", Ui.WidthLine(120));
                var current = StaticEcsViewConfig.Active;
                var newConfig = (StaticEcsViewConfig) EditorGUILayout.ObjectField(current, typeof(StaticEcsViewConfig), false, Ui.WidthLine(250));
                if (newConfig != null && newConfig != current) {
                    if (_parentWindow is IStaticEcsViewConfigHost host) {
                        host.SaveAllTabs();
                    }
                    StaticEcsViewConfig.SetActive(newConfig);
                    if (_parentWindow is IStaticEcsViewConfigHost host2) {
                        host2.ReloadFromConfig(newConfig);
                    }
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawFoldoutSection() {
            EditorGUILayout.LabelField("Component Foldouts", EditorStyles.boldLabel);

            var config = StaticEcsViewConfig.Active;
            var worldSettings = config.GetOrCreate(typeof(TWorld).FullName);
            var settings = worldSettings.entities;

            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField("Mode:", Ui.WidthLine(120));
                var newMode = (ComponentFoldoutMode) EditorGUILayout.EnumPopup(settings.foldoutMode, Ui.WidthLine(150));
                if (newMode != settings.foldoutMode) {
                    settings.foldoutMode = newMode;
                    Drawer.initializedFoldouts.Clear();
                    Drawer.openHideFlags.Clear();
                    config.MarkDirty();
                }
            }
            EditorGUILayout.EndHorizontal();

            if (settings.foldoutMode == ComponentFoldoutMode.Custom) {
                EditorGUILayout.Space(5);
                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField("Auto-expand types:", Ui.WidthLine(120));
                    if (GUILayout.Button("+", Ui.ButtonStyleTheme, Ui.WidthLine(30))) {
                        var menu = new GenericMenu();
                        if (_worldMeta != null) {
                            foreach (var comp in _worldMeta.Components) {
                                if (settings.autoExpandComponentTypes.Contains(comp.FullName)) continue;
                                var fullName = comp.FullName;
                                menu.AddItem(new GUIContent(fullName), false, () => {
                                    settings.autoExpandComponentTypes.Add(fullName);
                                    Drawer.initializedFoldouts.Clear();
                                    Drawer.openHideFlags.Clear();
                                    config.MarkDirty();
                                });
                            }
                        }
                        menu.ShowAsContext();
                    }
                }
                EditorGUILayout.EndHorizontal();

                EditorGUI.indentLevel++;
                for (var i = 0; i < settings.autoExpandComponentTypes.Count;) {
                    var name = settings.autoExpandComponentTypes[i];
                    var displayName = name;
                    if (_worldMeta != null) {
                        foreach (var comp in _worldMeta.Components) {
                            if (comp.FullName == name) {
                                displayName = comp.Name;
                                break;
                            }
                        }
                    }

                    EditorGUILayout.BeginHorizontal();
                    {
                        EditorGUILayout.LabelField(displayName, Ui.WidthLine(250));
                        if (Ui.TrashButton) {
                            settings.autoExpandComponentTypes.RemoveAt(i);
                            Drawer.initializedFoldouts.Clear();
                            Drawer.openHideFlags.Clear();
                            config.MarkDirty();
                        } else {
                            i++;
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUI.indentLevel--;
            }
        }

        private void DrawResetSection() {
            EditorGUILayout.LabelField("Reset", EditorStyles.boldLabel);

            if (GUILayout.Button("Reset config to defaults", Ui.ButtonStyleTheme, Ui.WidthLine(200))) {
                var config = StaticEcsViewConfig.Active;
                var worldKey = typeof(TWorld).FullName;
                config.worlds.RemoveAll(w => w.worldTypeFullName == worldKey);
                config.GetOrCreate(worldKey);
                Drawer.initializedFoldouts.Clear();
                Drawer.openHideFlags.Clear();
                config.Save();

                if (_parentWindow is IStaticEcsViewConfigHost host) {
                    host.ReloadFromConfig(config);
                }
            }
        }
    }

    internal interface IStaticEcsViewConfigHost {
        void SaveAllTabs();
        void ReloadFromConfig(StaticEcsViewConfig config);
    }
}
#endif
