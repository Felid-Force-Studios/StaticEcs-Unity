using UnityEditor;
using UnityEngine;

namespace FFS.Libraries.StaticEcs.Unity.Editor {
    [CanEditMultipleObjects]
    public abstract class StaticEcsEntityProviderEditor<TWorld, TSelf> : UnityEditor.Editor
        where TWorld : struct, IWorldType
        where TSelf : StaticEcsEntityProvider<TWorld> {

        public override bool RequiresConstantRepaint() => Application.isPlaying;

        public override void OnInspectorGUI() {
            foreach (var t in targets) {
                var provider = (TSelf) t;

                if (targets.Length > 1) {
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField(provider.name, EditorStyles.boldLabel);
                }

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
}
