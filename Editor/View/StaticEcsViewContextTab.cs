#if ((DEBUG || FFS_ECS_ENABLE_DEBUG) && !FFS_ECS_DISABLE_DEBUG)
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FFS.Libraries.StaticEcs.Unity.Editor {
    public class StaticEcsViewContextTab : IStaticEcsViewTab {
        private readonly Dictionary<Type, ContextDrawer> _drawersByWorldTypeType = new();
        private ContextDrawer _currentDrawer;

        public string Name() => "Context";

        public void Init() { }

        public void Draw(StaticEcsView view) {
            _currentDrawer.Draw();
        }

        public void Destroy() { }

        public void OnWorldChanged(AbstractWorldData newWorldData) {
            if (!_drawersByWorldTypeType.ContainsKey(newWorldData.WorldTypeType)) {
                _drawersByWorldTypeType[newWorldData.WorldTypeType] = new ContextDrawer(newWorldData);
            }

            _currentDrawer = _drawersByWorldTypeType[newWorldData.WorldTypeType];
        }
    }

    public class ContextDrawer {
        private readonly IWorld _world;
        private readonly AbstractWorldData _worldData;
        
        private readonly List<string> _toRemoveNamedContext = new();
        private readonly List<Action> _toRemoveContext = new();
        private readonly Type _contextValueType = typeof(ContextValue);
        private DrawContext _drawContext;
        private ContextValue _contextValue;

        private Vector2 verticalScrollStatsPosition = Vector2.zero;

        public ContextDrawer(AbstractWorldData worldData) {
            _worldData = worldData;
            _world = _worldData.World;
            _drawContext.World = _world;
            _drawContext.Level = Drawer.MaxRecursionLvl;
        }

        [Serializable]
        struct ContextValue {
            public object Value;
        }

        internal void Draw() {
            verticalScrollStatsPosition = EditorGUILayout.BeginScrollView(verticalScrollStatsPosition);
            DrawContext();
            DrawNamedContext();
            EditorGUILayout.EndScrollView();
        }

        private void DrawContext() {
            DrawHeader("Context");

            object newValue = null;
            Action<object> setMethod = null;
            var changed = false;
            foreach (var val in _world.Context().GetAllGetSetRemoveValuesMethods()) {
                _contextValue.Value = val.Value.Item1();
                setMethod = val.Value.Item2;
                var name = val.Key.EditorTypeName();

                if (!val.Key.IsSerializable) {
                    continue;
                }

                bool show;
                EditorGUILayout.BeginHorizontal(GUI.skin.box);
                {
                    var key = "CONTEXT_" + val.Key.FullName;
                    Drawer.DrawFoldoutBox(key, name, name, out show);

                    EditorGUILayout.BeginVertical(GUILayout.MinWidth(32));
                    if (Ui.MenuButton) {
                        var menu = new GenericMenu();
                        menu.AddItem(new GUIContent("Remove"), false, () => _toRemoveContext.Add(val.Value.Item3));
                        menu.ShowAsContext();
                    }

                    EditorGUILayout.EndVertical();
                }
                EditorGUILayout.EndHorizontal();


                if (show) {
                    EditorGUILayout.BeginVertical(GUI.skin.box);
                    changed = Drawer.TryDrawObject(ref _drawContext, name, _contextValueType, _contextValue, out newValue);
                    EditorGUILayout.EndVertical();
                    if (changed) {
                        break;
                    }
                }
            }
            
            if (changed) {
                setMethod(((ContextValue) newValue).Value);
            }
            
            foreach (var action in _toRemoveContext) {
                action();
            }
            _toRemoveContext.Clear();
        }

        private void DrawNamedContext() {
            DrawHeader("Named context");
            
            object newValue = null;
            string name = null;
            var changed = false;
            foreach (var val in _world.Context().GetAllNamedValues()) {
                name = val.Key;
                _contextValue.Value = val.Value;

                if (!val.Value.GetType().IsSerializable) {
                    continue;
                }

                bool show;
                EditorGUILayout.BeginHorizontal(GUI.skin.box);
                {
                    var key = "CONTEXT_" + name;
                    Drawer.DrawFoldoutBox(key, name, name, out show);

                    EditorGUILayout.BeginVertical(GUILayout.MinWidth(32));
                    if (Ui.MenuButton) {
                        var menu = new GenericMenu();
                        menu.AddItem(new GUIContent("Remove"), false, () => _toRemoveNamedContext.Add(val.Key));
                        menu.ShowAsContext();
                    }

                    EditorGUILayout.EndVertical();
                }
                EditorGUILayout.EndHorizontal();

                if (show) {
                    EditorGUILayout.BeginVertical(GUI.skin.box);
                    changed = Drawer.TryDrawObject(ref _drawContext, name, _contextValueType, _contextValue, out newValue);
                    EditorGUILayout.EndVertical();
                    if (changed) {
                        break;
                    }
                }
            }
            
            if (changed) {
                _world.Context().ReplaceNamed(name, ((ContextValue) newValue).Value);
            }
            
            foreach (var val in _toRemoveNamedContext) {
                _world.Context().RemoveNamed(val);
            }
            _toRemoveNamedContext.Clear();
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