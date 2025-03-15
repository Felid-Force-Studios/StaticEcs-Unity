using UnityEditor;
using UnityEngine;

namespace FFS.Libraries.StaticEcs.Unity.Editor {
    [CustomEditor(typeof(StaticEcsEntityProvider), false)]
    internal sealed class StaticEcsEntityProviderEditor : UnityEditor.Editor {
        private StaticEcsEntityProvider _provider;

        void OnEnable() {
            if (target == null) return;
            _provider = (StaticEcsEntityProvider) target;
        }

        public override void OnInspectorGUI() {
            if (Application.isPlaying && !_provider.EntityIsActual()) {
                EditorGUILayout.HelpBox("Entity is not provided", MessageType.Warning, true);
                return;
            }
            
            if (!Application.isPlaying) {
                DrawDefaultInspector();
            }
            
            Drawer.DrawEntity(_provider, false, provider => provider.CreateEntity(), !_provider.EntityIsActual());
        }
    }
}