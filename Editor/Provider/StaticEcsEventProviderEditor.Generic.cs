using UnityEditor;
using UnityEngine;

namespace FFS.Libraries.StaticEcs.Unity.Editor {
    [CanEditMultipleObjects]
    public abstract class StaticEcsEvenTEntityProviderEditor<TWorld, TSelf> : UnityEditor.Editor
        where TWorld : struct, IWorldType
        where TSelf : StaticEcsEventProvider<TWorld> {

        public override bool RequiresConstantRepaint() => Application.isPlaying;

        public override void OnInspectorGUI() {
            foreach (var t in targets) {
                var provider = (TSelf) t;

                if (targets.Length > 1) {
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField(provider.name, EditorStyles.boldLabel);
                }

                if (Application.isPlaying && provider.EventTemplate == null) {
                    EditorGUILayout.HelpBox("Event is not provided", MessageType.Warning, true);
                    continue;
                }

                if (!Application.isPlaying) {
                    DrawDefaultInspector();
                }

                Drawer.DrawEvent<TWorld, TSelf>(provider, DrawMode.Inspector, p => p.SendEvent());
            }
        }
    }
}
