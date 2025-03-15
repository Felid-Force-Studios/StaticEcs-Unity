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
        private readonly List<EditorEntityDataMetaByWorld> _none = new();
        private readonly List<EditorEntityDataMetaByWorld> _any = new();
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
                    if (GUILayout.Button("+", Ui.ButtonStyleWhite, Ui.WidthLine(20))) {
                        DrawShowFilterMenu(_components, _all);
                    }
                    EditorGUILayout.LabelField("All:", Ui.WidthLine(90));
                    DrawFilterLabels(_all);
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button("+", Ui.ButtonStyleWhite, Ui.WidthLine(20))) {
                        DrawShowFilterMenu(_components, _none);
                    }
                    EditorGUILayout.LabelField("None:", Ui.WidthLine(90));
                    DrawFilterLabels(_none);
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button("+", Ui.ButtonStyleWhite, Ui.WidthLine(20))) {
                        DrawShowFilterMenu(_components, _any);
                    }
                    EditorGUILayout.LabelField("Any:", Ui.WidthLine(90));
                    DrawFilterLabels(_any);
                }
                EditorGUILayout.EndHorizontal();
                
                #if !FFS_ECS_DISABLE_TAGS
                EditorGUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button("+", Ui.ButtonStyleWhite, Ui.WidthLine(20))) {
                        DrawShowFilterMenu(_tags, _tagAll);
                    }
                    EditorGUILayout.LabelField("Tag all:", Ui.WidthLine(90));
                    DrawFilterLabels(_tagAll);
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button("+", Ui.ButtonStyleWhite, Ui.WidthLine(20))) {
                        DrawShowFilterMenu(_tags, _tagNone);
                    }
                    EditorGUILayout.LabelField("Tag none:", Ui.WidthLine(90));
                    DrawFilterLabels(_tagNone);
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button("+", Ui.ButtonStyleWhite, Ui.WidthLine(20))) {
                        DrawShowFilterMenu(_tags, _tagAny);
                    }
                    EditorGUILayout.LabelField("Tag any:", Ui.WidthLine(90));
                    DrawFilterLabels(_tagAny);
                }
                EditorGUILayout.EndHorizontal();
                #endif

                #if !FFS_ECS_DISABLE_MASKS
                EditorGUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button("+", Ui.ButtonStyleWhite, Ui.WidthLine(20))) {
                        DrawShowFilterMenu(_masks, _maskAll);
                    }
                    EditorGUILayout.LabelField("Mask all:", Ui.WidthLine(90));
                    DrawFilterLabels(_maskAll);
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button("+", Ui.ButtonStyleWhite, Ui.WidthLine(20))) {
                        DrawShowFilterMenu(_masks, _maskNone);
                    }
                    EditorGUILayout.LabelField("Mask none:", Ui.WidthLine(90));
                    DrawFilterLabels(_maskNone);
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button("+", Ui.ButtonStyleWhite, Ui.WidthLine(20))) {
                        DrawShowFilterMenu(_masks, _maskAny);
                    }
                    EditorGUILayout.LabelField("Mask any:", Ui.WidthLine(90));
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
            return _filterActive && (_all.Count > 0 || _tagAll.Count > 0);
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
                if (GUILayout.Button("All", incAll ? Ui.ButtonStyleGrey : Ui.ButtonStyleWhite, Ui.WidthLine(60))) {
                    ShowAllColumns();
                }

                if (GUILayout.Button("None", excAll ? Ui.ButtonStyleGrey : Ui.ButtonStyleWhite, Ui.WidthLine(60))) {
                    ShowNoneColumns();
                }

                if (GUILayout.Button("+", incAll ? Ui.ButtonStyleGrey : Ui.ButtonStyleWhite, Ui.WidthLine(60))) {
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
            EditorGUILayout.EndHorizontal();
        }
        
        private void DataShowFilter() {
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField("Show data:", Ui.WidthLine(120));

                if (GUILayout.Button("All", Ui.ButtonStyleWhite, Ui.WidthLine(60))) {
                    for (var i = 0; i < _components.Count; i++) {
                        _components[i].ShowTableData = true;
                    }
                    for (var i = 0; i < _standardComponents.Count; i++) {
                        _standardComponents[i].ShowTableData = true;
                    }
                }

                if (GUILayout.Button("None", Ui.ButtonStyleWhite, Ui.WidthLine(60))) {
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
            
            if (_all.Count > 0) methods.Add(new TypesArray(_all).All());
            if (_none.Count > 0) methods.Add(new TypesArray(_none).None());
            if (_any.Count > 1) methods.Add(new TypesArray(_any).Any());
            #if !FFS_ECS_DISABLE_TAGS
            if (_tagAll.Count > 0) methods.Add(new TagArray(_tagAll).All());
            if (_tagNone.Count > 0) methods.Add(new TagArray(_tagNone).None());
            if (_tagAny.Count > 1) methods.Add(new TagArray(_tagAny).Any());
            #endif
            #if !FFS_ECS_DISABLE_MASKS
            if (_maskAll.Count > 0) methods.Add(new MaskArray(_maskAll).All());
            if (_maskNone.Count > 0) methods.Add(new MaskArray(_maskNone).None());
            if (_maskAny.Count > 1) methods.Add(new MaskArray(_maskAny).Any());
            #endif

            return _ecsWithFilter;
        }

        private void DrawFilterLabels(List<EditorEntityDataMetaByWorld> types) {
            for (var i = 0; i < types.Count;) {
                var idx = types[i];
                EditorGUILayout.SelectableLabel(idx.Name, Ui.LabelStyleWhiteCenter, idx.Layout);
                if (GUILayout.Button(Ui.IconTrash, Ui.WidthLine(30))) {
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
        public void SetData<WorldType>(ref int minCount, ref int[] entities) where WorldType : struct, IWorldType {
            foreach (var type in Types) {
                Ecs<WorldType>.ModuleComponents.Value.GetPool(type.Type).SetDataIfCountLess(ref minCount, ref entities);
            }
        }

        [MethodImpl(AggressiveInlining)]
        public void SetBitMask<WorldType>(byte bufId) where WorldType : struct, IWorldType {
            foreach (var type in Types) {
                Ecs<WorldType>.ModuleComponents.Value.BitMask.SetInBuffer(bufId, Ecs<WorldType>.ModuleComponents.Value.GetPool(type.Type).DynamicId().Value);
                #if DEBUG || FFS_ECS_ENABLE_DEBUG
                Ecs<WorldType>.ModuleComponents.Value.GetPool(type.Type).AddBlocker(1);
                #endif
            }
        }

        #if DEBUG || FFS_ECS_ENABLE_DEBUG
        [MethodImpl(AggressiveInlining)]
        public void Dispose<WorldType>() where WorldType : struct, IWorldType {
            foreach (var type in Types) {
                Ecs<WorldType>.ModuleComponents.Value.GetPool(type.Type).AddBlocker(-1);
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
        public void SetMask<WorldType>(byte bufId) where WorldType : struct, IWorldType {
            for (var i = 0; i < Mask.Count; i++) {
                Ecs<WorldType>.ModuleMasks.Value.BitMask.SetInBuffer(bufId, Ecs<WorldType>.ModuleMasks.Value.GetPool(Mask[i].Type).DynamicId().Value);
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
        public void SetData<WorldType>(ref int minCount, ref int[] entities) where WorldType : struct, IWorldType {
            foreach (var type in Tags) {
                Ecs<WorldType>.ModuleTags.Value.GetPool(type.Type).SetDataIfCountLess(ref minCount, ref entities);
            }
        }

        [MethodImpl(AggressiveInlining)]
        public void SetMask<WorldType>(byte bufId) where WorldType : struct, IWorldType {
            foreach (var type in Tags) {
                Ecs<WorldType>.ModuleTags.Value.BitMask.SetInBuffer(bufId, Ecs<WorldType>.ModuleTags.Value.GetPool(type.Type).DynamicId().Value);
                #if DEBUG || FFS_ECS_ENABLE_DEBUG
                Ecs<WorldType>.ModuleTags.Value.GetPool(type.Type).AddBlocker(1);
                #endif
            }
        }

        #if DEBUG || FFS_ECS_ENABLE_DEBUG
        [MethodImpl(AggressiveInlining)]
        public void Dispose<WorldType>() where WorldType : struct, IWorldType {
            foreach (var type in Tags) {
                Ecs<WorldType>.ModuleTags.Value.GetPool(type.Type).AddBlocker(-1);
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
        public void SetData<WorldType>(ref int minComponentsCount, ref int[] minEntities) where WorldType : struct, IWorldType {
            foreach (var method in _methods) {
                method.SetData<WorldType>(ref minComponentsCount, ref minEntities);
            }
        }

        [MethodImpl(AggressiveInlining)]
        public bool CheckEntity(int entityId) {
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