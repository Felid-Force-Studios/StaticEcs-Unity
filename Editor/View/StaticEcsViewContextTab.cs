#if ((DEBUG || FFS_ECS_ENABLE_DEBUG) && !FFS_ECS_DISABLE_DEBUG)
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FFS.Libraries.StaticEcs.Unity.Editor {
    public class StaticEcsViewContextTab<TWorld, TEntityProvider> : IStaticEcsViewTab
        where TWorld : struct, IWorldType
        where TEntityProvider : StaticEcsEntityProvider<TWorld> {
        private readonly Dictionary<Type, ContextDrawer> _drawersByWorldTypeType = new();
        private ContextDrawer _currentDrawer;

        public string Name() => "Resources";

        public void Init() { }

        public void Draw() {
            _currentDrawer.Draw();
        }

        public void Destroy() { }

        public void OnWorldChanged(AbstractWorldData newWorldData) {
            if (!_drawersByWorldTypeType.ContainsKey(newWorldData.Handle.WorldType)) {
                _drawersByWorldTypeType[newWorldData.Handle.WorldType] = new ContextDrawer(newWorldData);
            }

            _currentDrawer = _drawersByWorldTypeType[newWorldData.Handle.WorldType];
        }
    }

    public class ContextDrawer {
        private readonly WorldHandle _handle;
        private readonly AbstractWorldData _worldData;

        private readonly List<Action> _pendingRemovals = new();

        private Vector2 verticalScrollStatsPosition = Vector2.zero;

        public ContextDrawer(AbstractWorldData worldData) {
            _worldData = worldData;
            _handle = _worldData.Handle;
        }

        internal void Draw() {
            verticalScrollStatsPosition = EditorGUILayout.BeginScrollView(verticalScrollStatsPosition);

            DrawResourcesSection(
                "World", "World",
                _handle.GetAllResourcesTypes,
                _handle.GetAllResourcesKeys,
                _handle.GetResource,
                _handle.GetResource,
                _handle.SetResource,
                _handle.SetResource,
                _handle.RemoveResource,
                _handle.RemoveResource);

            foreach (var sh in _handle.GetAllSystemsHandles()) {
                var systemsTypeName = sh.SystemsType.EditorTypeName();
                var idPrefix = sh.SystemsType.FullName ?? systemsTypeName;
                var capturedHandle = sh;

                DrawResourcesSection(
                    $"Sys_{idPrefix}", $"Systems: {systemsTypeName}",
                    capturedHandle.GetAllResourcesTypes,
                    capturedHandle.GetAllResourcesKeys,
                    capturedHandle.GetResource,
                    capturedHandle.GetResource,
                    capturedHandle.SetResource,
                    capturedHandle.SetResource,
                    capturedHandle.RemoveResource,
                    capturedHandle.RemoveResource);
            }

            EditorGUILayout.EndScrollView();

            if (_pendingRemovals.Count > 0) {
                foreach (var action in _pendingRemovals) {
                    action();
                }
                _pendingRemovals.Clear();
            }
        }

        private void DrawResourcesSection(
            string sectionId,
            string header,
            Func<IReadOnlyCollection<Type>> getTypes,
            Func<IReadOnlyCollection<string>> getKeys,
            Func<Type, IResource> getByType,
            Func<string, IResource> getByKey,
            Action<Type, IResource, bool> setByType,
            Action<string, IResource, bool> setByKey,
            Action<Type> removeByType,
            Action<string> removeByKey) {
            var types = getTypes();
            var keys = getKeys();
            if (types.Count == 0 && keys.Count == 0) {
                return;
            }

            DrawHeader(header);

            Type changedType = null;
            IResource changedTypeValue = null;
            string changedKey = null;
            IResource changedKeyValue = null;

            foreach (var resourceType in types) {
                if (!resourceType.IsSerializable) {
                    continue;
                }

                var resourceValue = getByType(resourceType);
                var name = resourceType.EditorTypeName();

                bool show;
                EditorGUILayout.BeginHorizontal(GUI.skin.box);
                {
                    Drawer.DrawFoldoutBox(HashCode.Combine(sectionId, "T", resourceType.FullName), name, name, out show);

                    EditorGUILayout.BeginVertical(GUILayout.MinWidth(32));
                    if (Ui.MenuButton) {
                        var capturedType = resourceType;
                        var capturedRemove = removeByType;
                        var menu = new GenericMenu();
                        menu.AddItem(new GUIContent("Remove"), false, () => _pendingRemovals.Add(() => capturedRemove(capturedType)));
                        menu.ShowAsContext();
                    }

                    EditorGUILayout.EndVertical();
                }
                EditorGUILayout.EndHorizontal();

                if (show) {
                    EditorGUILayout.BeginVertical(GUI.skin.box);
                    MetaData.DrawSourceField(resourceType);
                    var wrapperKey = HashCode.Combine(sectionId, "T", resourceType.FullName);
                    if (TryDrawResourceValue(wrapperKey, resourceValue, out var newValue)) {
                        changedType = resourceType;
                        changedTypeValue = newValue;
                    }
                    EditorGUILayout.EndVertical();
                    if (changedType != null) {
                        break;
                    }
                }
            }

            if (changedType != null) {
                setByType(changedType, changedTypeValue, true);
            }

            foreach (var resourceKey in keys) {
                var resourceValue = getByKey(resourceKey);
                var resourceType = resourceValue.GetType();

                if (!resourceType.IsSerializable) {
                    continue;
                }

                bool show;
                EditorGUILayout.BeginHorizontal(GUI.skin.box);
                {
                    Drawer.DrawFoldoutBox(HashCode.Combine(sectionId, "K", resourceKey), resourceKey, resourceKey, out show);

                    EditorGUILayout.BeginVertical(GUILayout.MinWidth(32));
                    if (Ui.MenuButton) {
                        var capturedKey = resourceKey;
                        var capturedRemove = removeByKey;
                        var menu = new GenericMenu();
                        menu.AddItem(new GUIContent("Remove"), false, () => _pendingRemovals.Add(() => capturedRemove(capturedKey)));
                        menu.ShowAsContext();
                    }

                    EditorGUILayout.EndVertical();
                }
                EditorGUILayout.EndHorizontal();

                if (show) {
                    EditorGUILayout.BeginVertical(GUI.skin.box);
                    MetaData.DrawSourceField(resourceType);
                    var wrapperKey = HashCode.Combine(sectionId, "K", resourceKey);
                    if (TryDrawResourceValue(wrapperKey, resourceValue, out var newValue)) {
                        changedKey = resourceKey;
                        changedKeyValue = newValue;
                    }
                    EditorGUILayout.EndVertical();
                    if (changedKey != null) {
                        break;
                    }
                }
            }

            if (changedKey != null) {
                setByKey(changedKey, changedKeyValue, true);
            }

            EditorGUILayout.Space();
            Ui.DrawHorizontalSeparator(Ui.Width((int) (Math.Round((EditorGUIUtility.currentViewWidth - 30f) / (double) 5) * 5)));
            EditorGUILayout.Space();
        }

        private bool TryDrawResourceValue(int wrapperKey, IResource resourceValue, out IResource newValue) {
            newValue = null;
            var wrapper = ContextDrawerWrapper.GetFor(wrapperKey);
            wrapper.value = resourceValue;
            using var so = new SerializedObject(wrapper);
            var prop = so.FindProperty("value");

            if (prop != null && prop.propertyType == SerializedPropertyType.ManagedReference) {
                Drawer.DrawSerializedPropertyChildren(prop);

                if (so.ApplyModifiedProperties()) {
                    newValue = wrapper.value;
                    return true;
                }
            }

            return false;
        }

        private void DrawHeader(string name) {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(name, Ui.LabelStyleThemeBold);
            EditorGUILayout.EndHorizontal();

            Ui.DrawHorizontalSeparator(Ui.Width((int) (Math.Round((EditorGUIUtility.currentViewWidth - 30f) / (double) 5) * 5)));
        }
    }
}
#endif
