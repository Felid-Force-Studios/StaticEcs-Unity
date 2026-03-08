using UnityEditor;
using UnityEngine;

namespace FFS.Libraries.StaticEcs.Unity.Editor {
    public abstract class StaticEcsEntityProviderEditor<TWorld, TSelf> : UnityEditor.Editor
        where TWorld : struct, IWorldType
        where TSelf : StaticEcsEntityProvider<TWorld> {
        private TSelf _provider;

        protected virtual void OnEnable() {
            if (target == null) return;
            _provider = (TSelf) target;
        }

        public override void OnInspectorGUI() {
            if (!_provider.EntityIsActual()) {
                DrawDefaultInspector();
            }

            Drawer.DrawEntity<TWorld, TSelf>(_provider, DrawMode.Inspector, provider => {
                provider.CreateEntity();
                if (provider.IsPrefab()) {
                    EntityInspectorHelper<TWorld, TSelf>.ShowWindowForEntity(provider.EntityGid);
                    provider.EntityGid = default;
                }
                provider.ClearPrefab();
                EditorUtility.SetDirty(provider);
            }, provider => {
                provider.EntityGid = default;
                EditorUtility.SetDirty(provider);
            });
        }
    }
}
