using UnityEditor;
using UnityEngine;

namespace FFS.Libraries.StaticEcs.Unity.Editor {
    [CanEditMultipleObjects]
    public abstract class StaticEcsEvenTEntityProviderEditor<TWorld, TSelf> : UnityEditor.Editor
        where TWorld : struct, IWorldType
        where TSelf : StaticEcsEventProvider<TWorld> {

        private const string FoldoutKeyPrefix = "StaticEcsEventProviderFoldout_";

        public override bool RequiresConstantRepaint() => Application.isPlaying;

        public override void OnInspectorGUI() {
            if (targets.Length <= 1) {
                DrawProvider((TSelf) target);
                return;
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField($"Selected {targets.Length} event providers", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            for (var i = 0; i < targets.Length; i++) {
                var provider = (TSelf) targets[i];
                var key = FoldoutKeyPrefix + provider.GetInstanceID();
                var expanded = SessionState.GetBool(key, false);
                var newExpanded = EditorGUILayout.Foldout(expanded, provider.name, true);
                if (newExpanded != expanded) {
                    SessionState.SetBool(key, newExpanded);
                    expanded = newExpanded;
                }

                if (expanded) {
                    DrawProvider(provider);
                    EditorGUILayout.Space();
                }
            }
        }

        private void DrawProvider(TSelf provider) {
            if (Application.isPlaying && provider.EventTemplate == null) {
                EditorGUILayout.HelpBox("Event is not provided", MessageType.Warning, true);
                return;
            }

            if (!Application.isPlaying) {
                DrawDefaultInspector();
            }

            Drawer.DrawEvent<TWorld, TSelf>(provider, DrawMode.Inspector, p => p.SendEvent());
        }
    }
}
