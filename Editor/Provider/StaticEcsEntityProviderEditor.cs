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
            if (!_provider.EntityIsActual()) {
                DrawDefaultInspector();
            }
            
            Drawer.DrawEntity(_provider, DrawMode.Inspector, provider => {
                provider.CreateEntity();
                if (provider.IsPrefab()) {
                    EntityInspectorWindow.ShowWindowForEntity(provider.World, provider.Entity);
                    provider.Entity = null;
                    provider.World = null;
                }
                provider.Prefab = null;
                EditorUtility.SetDirty(provider);
            }, !_provider.EntityIsActual(), provider => {
                provider.Entity = null;
                EditorUtility.SetDirty(provider);
            });
        }
    }
}