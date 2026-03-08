using UnityEditor;
using UnityEngine;

namespace FFS.Libraries.StaticEcs.Unity.Editor {
    public abstract class StaticEcsEvenTEntityProviderEditor<TWorld, TSelf> : UnityEditor.Editor
        where TWorld : struct, IWorldType
        where TSelf : StaticEcsEventProvider<TWorld> {
        private TSelf _provider;

        protected virtual void OnEnable() {
            if (target == null) return;
            _provider = (TSelf) target;
        }

        public override void OnInspectorGUI() {
            if (Application.isPlaying && _provider.EventTemplate == null) {
                EditorGUILayout.HelpBox("Event is not provided", MessageType.Warning, true);
                return;
            }

            if (!Application.isPlaying) {
                DrawDefaultInspector();
            }

            Drawer.DrawEvent<TWorld, TSelf>(_provider, DrawMode.Inspector, provider => provider.SendEvent());
        }
    }
}
