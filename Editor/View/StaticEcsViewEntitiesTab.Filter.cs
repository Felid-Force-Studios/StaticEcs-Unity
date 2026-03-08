#if ((DEBUG || FFS_ECS_ENABLE_DEBUG) && !FFS_ECS_DISABLE_DEBUG)
#define FFS_ECS_DEBUG
#endif
#if ((DEBUG || FFS_ECS_ENABLE_DEBUG) && !FFS_ECS_DISABLE_DEBUG)
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FFS.Libraries.StaticEcs.Unity.Editor {

    public partial class EntitiesDrawer<TWorld, TEntityProvider> where TWorld : struct, IWorldType where TEntityProvider : StaticEcsEntityProvider<TWorld> {
        private int _maxEntityResult = 100;
        private int _currentEntityCount;
        private bool _filterActive;
        private bool _filterDirty = true;
        private CompositeHandleFilter _cachedFilter;

        private readonly List<EditorEntityDataMetaByWorld> _all = new();
        private readonly List<EditorEntityDataMetaByWorld> _allOnlyDisabled = new();
        private readonly List<EditorEntityDataMetaByWorld> _allWithDisabled = new();
        private readonly List<EditorEntityDataMetaByWorld> _none = new();
        private readonly List<EditorEntityDataMetaByWorld> _noneWithDisabled = new();
        private readonly List<EditorEntityDataMetaByWorld> _any = new();
        private readonly List<EditorEntityDataMetaByWorld> _anyOnlyDisabled = new();
        private readonly List<EditorEntityDataMetaByWorld> _anyWithDisabled = new();
        private readonly List<EditorEntityDataMetaByWorld> _componentsAndTags = new();

        private void DrawEntitiesFilter() {
            ComponentsFilter();
            GidFilter();
            ColumnsFilter();
            DataShowFilter();
            EntityResultCountFilter();
            EditorGUILayout.Space(10);
        }

        private void ComponentsFilter() {
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField("Filter:", Ui.WidthLine(120));
                var newFilterActive = EditorGUILayout.Toggle(_filterActive);
                if (newFilterActive != _filterActive) {
                    _filterActive = newFilterActive;
                    _filterDirty = true;
                }
            }
            EditorGUILayout.EndHorizontal();

            if (_filterActive) {
                EditorGUILayout.BeginHorizontal();
                {
                    if (Ui.PlusButton) {
                        DrawShowFilterMenu(_componentsAndTags, _all);
                    }
                    EditorGUILayout.LabelField("All:", Ui.WidthLine(120));
                    DrawFilterLabels(_all);
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                {
                    if (Ui.PlusButton) {
                        DrawShowFilterMenu(_components, _allOnlyDisabled);
                    }
                    EditorGUILayout.LabelField("All only disabled:", Ui.WidthLine(120));
                    DrawFilterLabels(_allOnlyDisabled);
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                {
                    if (Ui.PlusButton) {
                        DrawShowFilterMenu(_componentsAndTags, _allWithDisabled);
                    }
                    EditorGUILayout.LabelField("All with disabled:", Ui.WidthLine(120));
                    DrawFilterLabels(_allWithDisabled);
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                {
                    if (Ui.PlusButton) {
                        DrawShowFilterMenu(_componentsAndTags, _none);
                    }
                    EditorGUILayout.LabelField("None:", Ui.WidthLine(120));
                    DrawFilterLabels(_none);
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                {
                    if (Ui.PlusButton) {
                        DrawShowFilterMenu(_componentsAndTags, _noneWithDisabled);
                    }
                    EditorGUILayout.LabelField("None with disabled:", Ui.WidthLine(120));
                    DrawFilterLabels(_noneWithDisabled);
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                {
                    if (Ui.PlusButton) {
                        DrawShowFilterMenu(_componentsAndTags, _any);
                    }
                    EditorGUILayout.LabelField("Any:", Ui.WidthLine(120));
                    DrawFilterLabels(_any);
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                {
                    if (Ui.PlusButton) {
                        DrawShowFilterMenu(_components, _anyOnlyDisabled);
                    }
                    EditorGUILayout.LabelField("Any only disabled:", Ui.WidthLine(120));
                    DrawFilterLabels(_anyOnlyDisabled);
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                {
                    if (Ui.PlusButton) {
                        DrawShowFilterMenu(_componentsAndTags, _anyWithDisabled);
                    }
                    EditorGUILayout.LabelField("Any with disabled:", Ui.WidthLine(120));
                    DrawFilterLabels(_anyWithDisabled);
                }
                EditorGUILayout.EndHorizontal();

                if (!IsFilterValid()) {
                    EditorGUILayout.HelpBox("Please, provide at least one filter", MessageType.Warning, true);
                }
                else if (_any.Count == 1) {
                    EditorGUILayout.HelpBox("Please, provide at least two [Any] filter", MessageType.Warning, true);
                }
            }
        }

        private bool IsFilterValid() {
            return _filterActive && (_all.Count > 0 || _allOnlyDisabled.Count > 0 || _allWithDisabled.Count > 0 || _none.Count > 0 || _noneWithDisabled.Count > 0 || _any.Count > 1
                || _anyOnlyDisabled.Count > 1 || _anyWithDisabled.Count > 1);
        }

        private void GidFilter() {
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField("Entity GID:", Ui.WidthLine(120));
                _gidFilterActive = EditorGUILayout.Toggle(_gidFilterActive);
            }
            EditorGUILayout.EndHorizontal();
            if (_gidFilterActive) {
                EditorGUI.indentLevel++;
                _gidFilterValue = EditorGUILayout.IntField("Value", _gidFilterValue, Ui.WidthLine(300));
                if (_gidFilterValue < 0) _gidFilterValue = 0;
                EditorGUI.indentLevel--;
            }
        }

        private void ColumnsFilter() {
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField("Select:", Ui.WidthLine(120));

                var incAll = _componentsColumns.Count == _components.Count;
                var excAll = _componentsColumns.Count == 0;

                incAll = incAll && _tagsColumns.Count == _tags.Count;
                excAll = excAll && _tagsColumns.Count == 0;

                if (GUILayout.Button("All", incAll ? Ui.ButtonStyleGrey : Ui.ButtonStyleTheme, Ui.WidthLine(60))) {
                    ShowAllColumns(null);
                }

                if (GUILayout.Button("None", excAll ? Ui.ButtonStyleGrey : Ui.ButtonStyleTheme, Ui.WidthLine(60))) {
                    ShowNoneColumns();
                }

                using (Ui.EnabledScopeVal(!incAll)) {
                    if (Ui.PlusDropDownButton) {
                        var menu = new GenericMenu();

                        foreach (var idx in _components) {
                            if (!_componentsColumns.Contains(idx)) {
                                menu.AddItem(new GUIContent(idx.FullName), false, objType => _componentsColumns.Add((EditorEntityDataMetaByWorld) objType), idx);
                            }
                        }

                        foreach (var idx in _tags) {
                            if (!_tagsColumns.Contains(idx)) {
                                menu.AddItem(new GUIContent(idx.FullName), false, objType => _tagsColumns.Add((EditorEntityDataMetaByWorld) objType), idx);
                            }
                        }

                        menu.ShowAsContext();
                    }
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DataShowFilter() {
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField("Show data:", Ui.WidthLine(120));

                if (GUILayout.Button("All", Ui.ButtonStyleTheme, Ui.WidthLine(60))) {
                    for (var i = 0; i < _components.Count; i++) {
                        _components[i].ShowTableData = true;
                    }
                }

                if (GUILayout.Button("None", Ui.ButtonStyleTheme, Ui.WidthLine(60))) {
                    for (var i = 0; i < _components.Count; i++) {
                        _components[i].ShowTableData = false;
                    }
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        private void EntityResultCountFilter() {
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField("Max entities result:", Ui.WidthLine(120));
                _maxEntityResult = EditorGUILayout.IntSlider(_maxEntityResult, 0, 1000, Ui.WidthLine(185));
            }
            EditorGUILayout.EndHorizontal();
        }

        private CompositeHandleFilter GetOrBuildFilter() {
            if (!_filterDirty) return _cachedFilter;
            _filterDirty = false;

            _cachedFilter = new CompositeHandleFilter();

            if (_all.Count > 0) _cachedFilter.Add(new HandleComponentsFilter(ExtractComponentHandles(_all), QueryMethodType.ALL));
            if (_allOnlyDisabled.Count > 0) _cachedFilter.Add(new HandleComponentsFilter(ExtractComponentHandles(_allOnlyDisabled), QueryMethodType.ALL_ONLY_DISABLED));
            if (_allWithDisabled.Count > 0) _cachedFilter.Add(new HandleComponentsFilter(ExtractComponentHandles(_allWithDisabled), QueryMethodType.ALL_WITH_DISABLED));
            if (_none.Count > 0) _cachedFilter.Add(new HandleComponentsFilter(ExtractComponentHandles(_none), QueryMethodType.NONE));
            if (_noneWithDisabled.Count > 0) _cachedFilter.Add(new HandleComponentsFilter(ExtractComponentHandles(_noneWithDisabled), QueryMethodType.NONE_WITH_DISABLED));
            if (_any.Count > 1) _cachedFilter.Add(new HandleComponentsFilter(ExtractComponentHandles(_any), QueryMethodType.ANY));
            if (_anyOnlyDisabled.Count > 1) _cachedFilter.Add(new HandleComponentsFilter(ExtractComponentHandles(_anyOnlyDisabled), QueryMethodType.ANY_ONLY_DISABLED));
            if (_anyWithDisabled.Count > 1) _cachedFilter.Add(new HandleComponentsFilter(ExtractComponentHandles(_anyWithDisabled), QueryMethodType.ANY_WITH_DISABLED));
            return _cachedFilter;
        }

        private static List<ComponentsHandle> ExtractComponentHandles(List<EditorEntityDataMetaByWorld> list) {
            var result = new List<ComponentsHandle>(list.Count);
            foreach (var item in list) {
                if (item.ComponentHandle.HasValue)
                    result.Add(item.ComponentHandle.Value);
                else if (item.TagHandle.HasValue)
                    result.Add(item.TagHandle.Value);
            }
            return result;
        }

        private void DrawFilterLabels(List<EditorEntityDataMetaByWorld> types) {
            for (var i = 0; i < types.Count;) {
                var idx = types[i];
                EditorGUILayout.SelectableLabel(idx.Name, Ui.LabelStyleThemeCenter, idx.Layout);
                if (Ui.TrashButton) {
                    types.RemoveAt(i);
                    _filterDirty = true;
                } else {
                    i++;
                }
            }
        }

        private void DrawShowFilterMenu(List<EditorEntityDataMetaByWorld> types, List<EditorEntityDataMetaByWorld> result) {
            var menu = new GenericMenu();
            foreach (var idx in types) {
                if (result.Contains(idx)) {
                    continue;
                }
                menu.AddItem(
                    new GUIContent(idx.FullName),
                    false,
                    objType => {
                        result.Add((EditorEntityDataMetaByWorld) objType);
                        _filterDirty = true;
                    },
                    idx);
            }

            menu.ShowAsContext();
        }
    }

}
#endif
