using UnityEditor;
using UnityEngine;

namespace FFS.Libraries.StaticEcs.Unity.Editor {
    [CustomEditor(typeof(StaticEcsEventProvider), false)]
    internal sealed class StaticEcsEventProviderEditor : UnityEditor.Editor {
        private StaticEcsEventProvider _provider;

        void OnEnable() {
            if (target == null) return;
            _provider = (StaticEcsEventProvider) target;
        }

        public override void OnInspectorGUI() {
            if (Application.isPlaying && _provider.EventTemplate == null) {
                EditorGUILayout.HelpBox("Event is not provided", MessageType.Warning, true);
                return;
            }
            
            if (!Application.isPlaying) {
                DrawDefaultInspector();
            }
            
            Drawer.DrawEvent(_provider, DrawMode.Inspector, provider => provider.SendEvent());
        }
    }
}
