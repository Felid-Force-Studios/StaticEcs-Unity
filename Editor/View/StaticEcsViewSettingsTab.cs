using UnityEditor;
using UnityEngine;

namespace FFS.Libraries.StaticEcs.Unity.Editor {
    public class StaticEcsViewSettingsTab : IStaticEcsViewTab {
        public string Name() => "Settings";
        
        public void Init() { }

        public void Draw(StaticEcsView view) {
            const string Update = "Update per second:";

            GUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField(Update, Ui.WidthLine(120));
                view.drawFrames = EditorGUILayout.Slider(view.drawFrames, 0.25f, 30f, Ui.WidthLine(200));
                view.drawRate = 1f / view.drawFrames;
            }
            GUILayout.EndHorizontal();
        }
        
        public void Destroy() { }
        
        
        public void OnWorldChanged(AbstractWorldData newWorldData) { }
    }
}