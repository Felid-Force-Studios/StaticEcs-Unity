using UnityEditor;
using UnityEngine;

namespace FFS.Libraries.StaticEcs.Unity.Editor {
    [CanEditMultipleObjects]
    public abstract class StaticEcsEntityProviderEditor<TWorld, TSelf> : UnityEditor.Editor
        where TWorld : struct, IWorldType
        where TSelf : StaticEcsEntityProvider<TWorld> {

        private const string FoldoutKeyPrefix = "StaticEcsEntityProviderFoldout_";

        public override bool RequiresConstantRepaint() => Application.isPlaying;

        public override void OnInspectorGUI() {
            if (targets.Length <= 1) {
                DrawProvider((TSelf) target);
                return;
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField($"Selected {targets.Length} entity providers", EditorStyles.boldLabel);
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
            if (!provider.EntityIsActual()) {
                DrawDefaultInspector();
            }

            Drawer.DrawEntity<TWorld, TSelf>(provider, DrawMode.Inspector, p => {
                p.CreateEntity();
                if (p.IsPrefab()) {
                    EntityInspectorHelper<TWorld, TSelf>.ShowWindowForEntity(p.EntityGid);
                    p.EntityGid = default;
                }
                p.ClearPrefab();
                EditorUtility.SetDirty(p);
            }, p => {
                p.EntityGid = default;
                EditorUtility.SetDirty(p);
            });
        }
    }
}
