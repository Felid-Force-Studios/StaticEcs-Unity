using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;
using static System.Runtime.CompilerServices.MethodImplOptions;

namespace FFS.Libraries.StaticEcs.Unity.Editor {

    public partial class EntitiesDrawer {
        private int _maxEntityResult = 100;
        private int _currentEntityCount;
        private bool _filterActive;

        private readonly List<EditorEntityDataMetaByWorld> _all = new();
        private readonly List<EditorEntityDataMetaByWorld> _allOnlyDisabled = new();
        private readonly List<EditorEntityDataMetaByWorld> _allWithDisabled = new();
        private readonly List<EditorEntityDataMetaByWorld> _none = new();
        private readonly List<EditorEntityDataMetaByWorld> _noneWithDisabled = new();
        private readonly List<EditorEntityDataMetaByWorld> _any = new();
        private readonly List<EditorEntityDataMetaByWorld> _anyOnlyDisabled = new();
        private readonly List<EditorEntityDataMetaByWorld> _anyWithDisabled = new();
        private readonly List<EditorEntityDataMetaByWorld> _tagAll = new();
        private readonly List<EditorEntityDataMetaByWorld> _tagNone = new();
        private readonly List<EditorEntityDataMetaByWorld> _tagAny = new();
        #if !FFS_ECS_DISABLE_MASKS
        private readonly List<EditorEntityDataMetaByWorld> _maskAll = new();
        private readonly List<EditorEntityDataMetaByWorld> _maskNone = new();
        private readonly List<EditorEntityDataMetaByWorld> _maskAny = new();
        #endif
        
        private readonly WithArray _ecsWithFilter = new(new List<IQueryMethod>());

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
                _filterActive = EditorGUILayout.Toggle(_filterActive);
            }
            EditorGUILayout.EndHorizontal();

            if (_filterActive) {
                EditorGUILayout.BeginHorizontal();
                {
                    if (Ui.PlusButton) {
                        DrawShowFilterMenu(_components, _all);
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
                        DrawShowFilterMenu(_components, _allWithDisabled);
                    }
                    EditorGUILayout.LabelField("All with disabled:", Ui.WidthLine(120));
                    DrawFilterLabels(_allWithDisabled);
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                {
                    if (Ui.PlusButton) {
                        DrawShowFilterMenu(_components, _none);
                    }
                    EditorGUILayout.LabelField("None:", Ui.WidthLine(120));
                    DrawFilterLabels(_none);
                }
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.BeginHorizontal();
                {
                    if (Ui.PlusButton) {
                        DrawShowFilterMenu(_components, _noneWithDisabled);
                    }
                    EditorGUILayout.LabelField("None with disabled:", Ui.WidthLine(120));
                    DrawFilterLabels(_noneWithDisabled);
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                {
                    if (Ui.PlusButton) {
                        DrawShowFilterMenu(_components, _any);
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
                        DrawShowFilterMenu(_components, _anyWithDisabled);
                    }
                    EditorGUILayout.LabelField("Any with disabled:", Ui.WidthLine(120));
                    DrawFilterLabels(_anyWithDisabled);
                }
                EditorGUILayout.EndHorizontal();
                
                #if !FFS_ECS_DISABLE_TAGS
                EditorGUILayout.BeginHorizontal();
                {
                    if (Ui.PlusButton) {
                        DrawShowFilterMenu(_tags, _tagAll);
                    }
                    EditorGUILayout.LabelField("Tag all:", Ui.WidthLine(120));
                    DrawFilterLabels(_tagAll);
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                {
                    if (Ui.PlusButton) {
                        DrawShowFilterMenu(_tags, _tagNone);
                    }
                    EditorGUILayout.LabelField("Tag none:", Ui.WidthLine(120));
                    DrawFilterLabels(_tagNone);
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                {
                    if (Ui.PlusButton) {
                        DrawShowFilterMenu(_tags, _tagAny);
                    }
                    EditorGUILayout.LabelField("Tag any:", Ui.WidthLine(120));
                    DrawFilterLabels(_tagAny);
                }
                EditorGUILayout.EndHorizontal();
                #endif

                #if !FFS_ECS_DISABLE_MASKS
                EditorGUILayout.BeginHorizontal();
                {
                    if (Ui.PlusButton) {
                        DrawShowFilterMenu(_masks, _maskAll);
                    }
                    EditorGUILayout.LabelField("Mask all:", Ui.WidthLine(120));
                    DrawFilterLabels(_maskAll);
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                {
                    if (Ui.PlusButton) {
                        DrawShowFilterMenu(_masks, _maskNone);
                    }
                    EditorGUILayout.LabelField("Mask none:", Ui.WidthLine(120));
                    DrawFilterLabels(_maskNone);
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                {
                    if (Ui.PlusButton) {
                        DrawShowFilterMenu(_masks, _maskAny);
                    }
                    EditorGUILayout.LabelField("Mask any:", Ui.WidthLine(120));
                    DrawFilterLabels(_maskAny);
                }
                EditorGUILayout.EndHorizontal();
                #endif
                
                if (!IsFilterValid()) {
                    EditorGUILayout.HelpBox("Please, provide at least one [All] or [Tag all] filter", MessageType.Warning, true);
                }
                else if (_any.Count == 1) {
                    EditorGUILayout.HelpBox("Please, provide at least two [Any] filter", MessageType.Warning, true);
                }
                #if !FFS_ECS_DISABLE_TAGS
                else if (_tagAny.Count == 1) {
                    EditorGUILayout.HelpBox("Please, provide at least two [Tag any] filter", MessageType.Warning, true);
                }
                #endif
                #if !FFS_ECS_DISABLE_MASKS
                else if (_maskAny.Count == 1) {
                    EditorGUILayout.HelpBox("Please, provide at least two [Mask any] filter", MessageType.Warning, true);
                }
                #endif
            }
        }

        private bool IsFilterValid() {
            return _filterActive && (_all.Count > 0 || _tagAll.Count > 0 || _allWithDisabled.Count > 0 || _allOnlyDisabled.Count > 0);
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
                
                #if !FFS_ECS_DISABLE_TAGS
                incAll = incAll && _tagsColumns.Count == _tags.Count;
                excAll = excAll && _tagsColumns.Count == 0;
                #endif
                
                #if !FFS_ECS_DISABLE_MASKS
                incAll = incAll && _maskColumns.Count == _masks.Count;
                excAll = excAll && _maskColumns.Count == 0;
                #endif
                if (GUILayout.Button("All", incAll ? Ui.ButtonStyleGrey : Ui.ButtonStyleTheme, Ui.WidthLine(60))) {
                    ShowAllColumns();
                }

                if (GUILayout.Button("None", excAll ? Ui.ButtonStyleGrey : Ui.ButtonStyleTheme, Ui.WidthLine(60))) {
                    ShowNoneColumns();
                }

                using (Ui.EnabledScopeVal(!incAll)) {
                    if (Ui.PlusDropDownButton) {
                        var menu = new GenericMenu();
                        foreach (var idx in _standardComponents) {
                            if (!_standardComponentsColumns.Contains(idx)) {
                                menu.AddItem(new GUIContent(idx.FullName), false, objType => _standardComponentsColumns.Add((EditorEntityDataMetaByWorld) objType), idx);
                            }
                        }
                    
                        foreach (var idx in _components) {
                            if (!_componentsColumns.Contains(idx)) {
                                menu.AddItem(new GUIContent(idx.FullName), false, objType => _componentsColumns.Add((EditorEntityDataMetaByWorld) objType), idx);
                            }
                        }
                    
                        #if !FFS_ECS_DISABLE_TAGS
                        foreach (var idx in _tags) {
                            if (!_tagsColumns.Contains(idx)) {
                                menu.AddItem(new GUIContent(idx.FullName), false, objType => _tagsColumns.Add((EditorEntityDataMetaByWorld) objType), idx);
                            }
                        }
                        #endif
                    
                        #if !FFS_ECS_DISABLE_MASKS
                        foreach (var idx in _masks) {
                            if (!_maskColumns.Contains(idx)) {
                                menu.AddItem(new GUIContent(idx.FullName), false, objType => _maskColumns.Add((EditorEntityDataMetaByWorld) objType), idx);
                            }
                        }
                        #endif

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
                    for (var i = 0; i < _standardComponents.Count; i++) {
                        _standardComponents[i].ShowTableData = true;
                    }
                }

                if (GUILayout.Button("None", Ui.ButtonStyleTheme, Ui.WidthLine(60))) {
                    for (var i = 0; i < _components.Count; i++) {
                        _components[i].ShowTableData = false;
                    }
                    for (var i = 0; i < _standardComponents.Count; i++) {
                        _standardComponents[i].ShowTableData = false;
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

        private WithArray MakeEcsWithFilter() {
            var methods = _ecsWithFilter._methods;
            methods.Clear();
            
            if (_all.Count > 0) methods.Add(new AllTypes<TypesArray>(new TypesArray(_all)));
            if (_allOnlyDisabled.Count > 0) methods.Add(new AllOnlyDisabledTypes<TypesArray>(new TypesArray(_allOnlyDisabled)));
            if (_allWithDisabled.Count > 0) methods.Add(new AllWithDisabledTypes<TypesArray>(new TypesArray(_allWithDisabled)));
            if (_none.Count > 0) methods.Add(new NoneTypes<TypesArray>(new TypesArray(_none)));
            if (_noneWithDisabled.Count > 0) methods.Add(new NoneWithDisabledTypes<TypesArray>(new TypesArray(_noneWithDisabled)));
            if (_any.Count > 1) methods.Add(new AnyTypes<TypesArray>(new TypesArray(_any)));
            if (_anyOnlyDisabled.Count > 1) methods.Add(new AnyOnlyDisabledTypes<TypesArray>(new TypesArray(_anyOnlyDisabled)));
            if (_anyWithDisabled.Count > 1) methods.Add(new AnyWithDisabledTypes<TypesArray>(new TypesArray(_anyWithDisabled)));
            #if !FFS_ECS_DISABLE_TAGS
            if (_tagAll.Count > 0) methods.Add(new TagAllTypes<TagArray>(new TagArray(_tagAll)));
            if (_tagNone.Count > 0) methods.Add(new TagNoneTypes<TagArray>(new TagArray(_tagNone)));
            if (_tagAny.Count > 1) methods.Add(new TagAnyTypes<TagArray>(new TagArray(_tagAny)));
            #endif
            #if !FFS_ECS_DISABLE_MASKS
            if (_maskAll.Count > 0) methods.Add(new MaskAllTypes<MaskArray>(new MaskArray(_maskAll)));
            if (_maskNone.Count > 0) methods.Add(new MaskNoneTypes<MaskArray>(new MaskArray(_maskNone)));
            if (_maskAny.Count > 1) methods.Add(new MaskAnyTypes<MaskArray>(new MaskArray(_maskAny)));
            #endif

            return _ecsWithFilter;
        }

        private void DrawFilterLabels(List<EditorEntityDataMetaByWorld> types) {
            for (var i = 0; i < types.Count;) {
                var idx = types[i];
                EditorGUILayout.SelectableLabel(idx.Name, Ui.LabelStyleThemeCenter, idx.Layout);
                if (Ui.TrashButton) {
                    types.RemoveAt(i);
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
                    },
                    idx);
            }

            menu.ShowAsContext();
        }
    }

    public struct TypesArray : IComponentTypes {
        public List<EditorEntityDataMetaByWorld> Types;

        [MethodImpl(AggressiveInlining)]
        public TypesArray(List<EditorEntityDataMetaByWorld> types) {
            Types = types;
        }

        [MethodImpl(AggressiveInlining)]
        public void SetMinData<WorldType>(ref uint minCount, ref uint[] entities) where WorldType : struct, IWorldType {
            foreach (var type in Types) {
                World<WorldType>.ModuleComponents.Value.GetPool(type.Type).SetDataIfCountLess(ref minCount, ref entities);
            }
        }

        [MethodImpl(AggressiveInlining)]
        public void SetBitMask<WorldType>(byte bufId) where WorldType : struct, IWorldType {
            foreach (var type in Types) {
                World<WorldType>.ModuleComponents.Value.BitMask.SetInBuffer(bufId, World<WorldType>.ModuleComponents.Value.GetPool(type.Type).DynamicId());
            }
        }

        #if DEBUG || FFS_ECS_ENABLE_DEBUG
        [MethodImpl(AggressiveInlining)]
        public void Block<WorldType>(int val) where WorldType : struct, IWorldType {
            foreach (var type in Types) {
                World<WorldType>.ModuleComponents.Value.GetPool(type.Type).AddBlocker(val);
            }
        }
        #endif
    }

    #if !FFS_ECS_DISABLE_MASKS
    public struct MaskArray : IComponentMasks {
        public List<EditorEntityDataMetaByWorld> Mask;

        [MethodImpl(AggressiveInlining)]
        public MaskArray(List<EditorEntityDataMetaByWorld> mask) {
            Mask = mask;
        }

        [MethodImpl(AggressiveInlining)]
        public void SetBitMask<WorldType>(byte bufId) where WorldType : struct, IWorldType {
            for (var i = 0; i < Mask.Count; i++) {
                World<WorldType>.ModuleMasks.Value.BitMask.SetInBuffer(bufId, World<WorldType>.ModuleMasks.Value.GetPool(Mask[i].Type).DynamicId());
            }
        }
    }
    #endif

    #if !FFS_ECS_DISABLE_TAGS
    #if ENABLE_IL2CPP
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    #endif
    public struct TagArray : IComponentTags {
        public List<EditorEntityDataMetaByWorld> Tags;

        [MethodImpl(AggressiveInlining)]
        public TagArray(List<EditorEntityDataMetaByWorld> tags) {
            Tags = tags;
        }

        [MethodImpl(AggressiveInlining)]
        public void SetMinData<WorldType>(ref uint minCount, ref uint[] entities) where WorldType : struct, IWorldType {
            foreach (var type in Tags) {
                World<WorldType>.ModuleTags.Value.GetPool(type.Type).SetDataIfCountLess(ref minCount, ref entities);
            }
        }

        [MethodImpl(AggressiveInlining)]
        public void SetBitMask<WorldType>(byte bufId) where WorldType : struct, IWorldType {
            foreach (var type in Tags) {
                World<WorldType>.ModuleTags.Value.BitMask.SetInBuffer(bufId, World<WorldType>.ModuleTags.Value.GetPool(type.Type).DynamicId());
            }
        }

        #if DEBUG || FFS_ECS_ENABLE_DEBUG
        [MethodImpl(AggressiveInlining)]
        public void Block<WorldType>(int val) where WorldType : struct, IWorldType {
            foreach (var type in Tags) {
                World<WorldType>.ModuleTags.Value.GetPool(type.Type).AddBlocker(val);
            }
        }
        #endif
    }
    #endif

    public struct WithArray : IPrimaryQueryMethod {
        internal List<IQueryMethod> _methods;

        public WithArray(List<IQueryMethod> methods) {
            _methods = methods;
        }

        [MethodImpl(AggressiveInlining)]
        public void SetData<WorldType>(ref uint minComponentsCount, ref uint[] minEntities) where WorldType : struct, IWorldType {
            foreach (var method in _methods) {
                method.SetData<WorldType>(ref minComponentsCount, ref minEntities);
            }
        }

        [MethodImpl(AggressiveInlining)]
        public bool CheckEntity(uint entityId) {
            foreach (var method in _methods) {
                if (!method.CheckEntity(entityId)) {
                    return false;
                }
            }

            return true;
        }

        [MethodImpl(AggressiveInlining)]
        public void Dispose<WorldType>() where WorldType : struct, IWorldType {
            foreach (var method in _methods) {
                method.Dispose<WorldType>();
            }
        }
    }
}