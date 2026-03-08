#if ((DEBUG || FFS_ECS_ENABLE_DEBUG) && !FFS_ECS_DISABLE_DEBUG)
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace FFS.Libraries.StaticEcs.Unity.Editor {
    public class StaticEcsViewContextTab<TWorld, TEntityProvider> : IStaticEcsViewTab
        where TWorld : struct, IWorldType
        where TEntityProvider : StaticEcsEntityProvider<TWorld> {
        private readonly Dictionary<Type, ContextDrawer> _drawersByWorldTypeType = new();
        private ContextDrawer _currentDrawer;

        public string Name() => "Context";

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

        private readonly List<string> _toRemoveNamedContext = new();
        private readonly List<Type> _toRemoveContext = new();
        private readonly Dictionary<Type, (ScriptableObject wrapper, FieldInfo field)> _valueTypeWrapperCache = new();

        private Vector2 verticalScrollStatsPosition = Vector2.zero;

        public ContextDrawer(AbstractWorldData worldData) {
            _worldData = worldData;
            _handle = _worldData.Handle;
        }

        internal void Draw() {
            verticalScrollStatsPosition = EditorGUILayout.BeginScrollView(verticalScrollStatsPosition);
            DrawContext();
            DrawNamedContext();
            EditorGUILayout.EndScrollView();
        }

        private void DrawContext() {
            DrawHeader("Context");

            Type changedType = null;
            object changedValue = null;
            foreach (var resourceType in _handle.GetAllResourcesTypes()) {
                var resourceValue = _handle.GetResource(resourceType);
                var name = resourceType.EditorTypeName();

                if (!resourceType.IsSerializable) {
                    continue;
                }

                bool show;
                EditorGUILayout.BeginHorizontal(GUI.skin.box);
                {
                    Drawer.DrawFoldoutBox(HashCode.Combine("CONTEXT_", resourceType.FullName), name, name, out show);

                    EditorGUILayout.BeginVertical(GUILayout.MinWidth(32));
                    if (Ui.MenuButton) {
                        var capturedType = resourceType;
                        var menu = new GenericMenu();
                        menu.AddItem(new GUIContent("Remove"), false, () => _toRemoveContext.Add(capturedType));
                        menu.ShowAsContext();
                    }

                    EditorGUILayout.EndVertical();
                }
                EditorGUILayout.EndHorizontal();

                if (show) {
                    EditorGUILayout.BeginVertical(GUI.skin.box);
                    if (TryDrawResourceValue(resourceType, resourceValue, out var newValue)) {
                        changedType = resourceType;
                        changedValue = newValue;
                    }
                    EditorGUILayout.EndVertical();
                    if (changedType != null) {
                        break;
                    }
                }
            }

            if (changedType != null) {
                _handle.SetResource(changedType, changedValue, true);
            }

            foreach (var type in _toRemoveContext) {
                _handle.RemoveResource(type);
            }
            _toRemoveContext.Clear();
        }

        private void DrawNamedContext() {
            DrawHeader("Named context");

            string changedKey = null;
            object changedValue = null;
            foreach (var resourceKey in _handle.GetAllResourcesKeys()) {
                var resourceValue = _handle.GetResource(resourceKey);
                var resourceType = resourceValue.GetType();

                if (!resourceType.IsSerializable) {
                    continue;
                }

                bool show;
                EditorGUILayout.BeginHorizontal(GUI.skin.box);
                {
                    Drawer.DrawFoldoutBox(HashCode.Combine("CONTEXT_", resourceKey), resourceKey, resourceKey, out show);

                    EditorGUILayout.BeginVertical(GUILayout.MinWidth(32));
                    if (Ui.MenuButton) {
                        var capturedKey = resourceKey;
                        var menu = new GenericMenu();
                        menu.AddItem(new GUIContent("Remove"), false, () => _toRemoveNamedContext.Add(capturedKey));
                        menu.ShowAsContext();
                    }

                    EditorGUILayout.EndVertical();
                }
                EditorGUILayout.EndHorizontal();

                if (show) {
                    EditorGUILayout.BeginVertical(GUI.skin.box);
                    if (TryDrawResourceValue(resourceType, resourceValue, out var newValue)) {
                        changedKey = resourceKey;
                        changedValue = newValue;
                    }
                    EditorGUILayout.EndVertical();
                    if (changedKey != null) {
                        break;
                    }
                }
            }

            if (changedKey != null) {
                _handle.SetResource(changedKey, changedValue, true);
            }

            foreach (var val in _toRemoveNamedContext) {
                _handle.RemoveResource(val);
            }
            _toRemoveNamedContext.Clear();
        }

        private bool TryDrawResourceValue(Type resourceType, object resourceValue, out object newValue) {
            newValue = null;
            if (resourceType.IsValueType) {
                return TryDrawValueType(resourceType, resourceValue, out newValue);
            }

            return TryDrawReferenceType(resourceValue, out newValue);
        }

        private bool TryDrawReferenceType(object resourceValue, out object newValue) {
            newValue = null;
            var wrapper = ContextDrawerWrapper.Instance;
            var so = new SerializedObject(wrapper);
            var prop = so.FindProperty("value");
            prop.managedReferenceValue = resourceValue;
            so.ApplyModifiedProperties();
            so.Update();
            prop = so.FindProperty("value");

            if (prop != null && prop.propertyType == SerializedPropertyType.ManagedReference) {
                Drawer.DrawSerializedPropertyChildren(prop);

                if (so.ApplyModifiedProperties()) {
                    newValue = wrapper.value;
                    return true;
                }
            }

            return false;
        }

        private bool TryDrawValueType(Type resourceType, object resourceValue, out object newValue) {
            newValue = null;
            var (wrapper, field) = GetValueTypeWrapper(resourceType);
            field.SetValue(wrapper, resourceValue);
            var so = new SerializedObject(wrapper);
            so.Update();
            var prop = so.FindProperty("value");

            if (prop != null) {
                Drawer.DrawSerializedPropertyChildren(prop);

                if (so.ApplyModifiedProperties()) {
                    newValue = field.GetValue(wrapper);
                    return true;
                }
            }

            return false;
        }

        private (ScriptableObject wrapper, FieldInfo field) GetValueTypeWrapper(Type valueType) {
            if (!_valueTypeWrapperCache.TryGetValue(valueType, out var cached)) {
                var wrapperType = typeof(ContextValueDrawerWrapper<>).MakeGenericType(valueType);
                var wrapper = ScriptableObject.CreateInstance(wrapperType);
                wrapper.hideFlags = HideFlags.DontSave;
                var field = wrapperType.GetField("value");
                cached = (wrapper, field);
                _valueTypeWrapperCache[valueType] = cached;
            }
            return cached;
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
