#if ((DEBUG || FFS_ECS_ENABLE_DEBUG) && !FFS_ECS_DISABLE_DEBUG)
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
                
                if (!IsFilterValid()) {
                    EditorGUILayout.HelpBox("Please, provide at least one filter", MessageType.Warning, true);
                }
                else if (_any.Count == 1) {
                    EditorGUILayout.HelpBox("Please, provide at least two [Any] filter", MessageType.Warning, true);
                }
                else if (_tagAny.Count == 1) {
                    EditorGUILayout.HelpBox("Please, provide at least two [Tag any] filter", MessageType.Warning, true);
                }
            }
        }

        private bool IsFilterValid() {
            return _filterActive && (_all.Count > 0 || _allOnlyDisabled.Count > 0 || _allWithDisabled.Count > 0 || _none.Count > 0 || _noneWithDisabled.Count > 0 || _any.Count > 1 
                || _anyOnlyDisabled.Count > 1 || _anyWithDisabled.Count > 1 || _tagAll.Count > 0 || _tagNone.Count > 0 || _tagAny.Count > 1);
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

        private WithArray MakeEcsWithFilter() {
            var methods = _ecsWithFilter._methods;
            methods.Clear();
            
            if (_all.Count > 0) methods.Add(new TypesArray(_all, QueryMethodType.ALL));
            if (_allOnlyDisabled.Count > 0) methods.Add(new TypesArray(_allOnlyDisabled, QueryMethodType.ALL_ONLY_DISABLED));
            if (_allWithDisabled.Count > 0) methods.Add(new TypesArray(_allWithDisabled, QueryMethodType.ALL_WITH_DISABLED));
            if (_none.Count > 0) methods.Add(new TypesArray(_none, QueryMethodType.NONE));
            if (_noneWithDisabled.Count > 0) methods.Add(new TypesArray(_noneWithDisabled, QueryMethodType.NONE_WITH_DISABLED));
            if (_any.Count > 1) methods.Add(new TypesArray(_any, QueryMethodType.ANY));
            if (_anyOnlyDisabled.Count > 1) methods.Add(new TypesArray(_anyOnlyDisabled, QueryMethodType.ANY_ONLY_DISABLED));
            if (_anyWithDisabled.Count > 1) methods.Add(new TypesArray(_anyWithDisabled, QueryMethodType.ANY_WITH_DISABLED));
            if (_tagAll.Count > 0) methods.Add(new TagArray(_tagAll, QueryMethodType.ALL));
            if (_tagNone.Count > 0) methods.Add(new TagArray(_tagNone, QueryMethodType.NONE));
            if (_tagAny.Count > 1) methods.Add(new TagArray(_tagAny, QueryMethodType.ANY));

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

    public enum QueryMethodType {
        ALL,
        ALL_ONLY_DISABLED,
        ALL_WITH_DISABLED,
        NONE,
        NONE_WITH_DISABLED,
        ANY,
        ANY_ONLY_DISABLED,
        ANY_WITH_DISABLED,
        
    }

    public struct TypesArray : IQueryMethod {
        public List<EditorEntityDataMetaByWorld> Types;
        private QueryMethodType Type;

        [MethodImpl(AggressiveInlining)]
        public TypesArray(List<EditorEntityDataMetaByWorld> types, QueryMethodType type) {
            Type = type;
            Types = types;
        }

        public void CheckChunk<WorldType>(ref ulong chunkMask, uint chunkIdx) where WorldType : struct, IWorldType {
            switch (Type) {
                case QueryMethodType.ALL:  
                case QueryMethodType.ALL_ONLY_DISABLED:  
                case QueryMethodType.ALL_WITH_DISABLED:  
                    foreach (var type in Types) {
                        chunkMask &= type.CopmponentsPool.Chunk(chunkIdx).notEmptyBlocks;
                    }
                    break;
                case QueryMethodType.NONE: 
                case QueryMethodType.NONE_WITH_DISABLED: 
                    foreach (var type in Types) {
                        chunkMask &= ~type.CopmponentsPool.Chunk(chunkIdx).fullBlocks;
                    }
                    break;
                case QueryMethodType.ANY:  
                case QueryMethodType.ANY_ONLY_DISABLED:  
                case QueryMethodType.ANY_WITH_DISABLED:  
                    ulong mask = 0;
                    foreach (var type in Types) {
                        mask |= type.CopmponentsPool.Chunk(chunkIdx).notEmptyBlocks;
                    }
                    chunkMask &= mask;
                    break;
            }
        }

        public void CheckEntities<WorldType>(ref ulong entitiesMask, uint chunkIdx, int blockIdx) where WorldType : struct, IWorldType {
            switch (Type) {
                case QueryMethodType.ALL:  
                    foreach (var type in Types) {
                        entitiesMask &= type.CopmponentsPool.EMask(chunkIdx, blockIdx);
                    }
                    break;  
                case QueryMethodType.ALL_ONLY_DISABLED:  
                    foreach (var type in Types) {
                        entitiesMask &= type.CopmponentsPool.DMask(chunkIdx, blockIdx);
                    }
                    break;  
                case QueryMethodType.ALL_WITH_DISABLED:  
                    foreach (var type in Types) {
                        entitiesMask &= type.CopmponentsPool.AMask(chunkIdx, blockIdx);
                    }
                    break;
                case QueryMethodType.NONE:  
                    foreach (var type in Types) {
                        entitiesMask &= ~type.CopmponentsPool.EMask(chunkIdx, blockIdx);
                    }
                    break; 
                case QueryMethodType.NONE_WITH_DISABLED:  
                    foreach (var type in Types) {
                        entitiesMask &= ~type.CopmponentsPool.AMask(chunkIdx, blockIdx);
                    }
                    break; 
                case QueryMethodType.ANY:  
                    ulong mask1 = 0;
                    foreach (var type in Types) {
                        mask1 |= type.CopmponentsPool.EMask(chunkIdx, blockIdx);
                    }
                    entitiesMask &= mask1;
                    break;  
                case QueryMethodType.ANY_ONLY_DISABLED:  
                    ulong mask2 = 0;
                    foreach (var type in Types) {
                        mask2 |= type.CopmponentsPool.DMask(chunkIdx, blockIdx);
                    }
                    entitiesMask &= mask2;
                    break;  
                case QueryMethodType.ANY_WITH_DISABLED:  
                    ulong mask3 = 0;
                    foreach (var type in Types) {
                        mask3 |= type.CopmponentsPool.AMask(chunkIdx, blockIdx);
                    }
                    entitiesMask &= mask3;
                    break;
            }
        }
        public void IncQ<WorldType>(QueryData data) where WorldType : struct, IWorldType { }
        public void DecQ<WorldType>() where WorldType : struct, IWorldType { }
        public void BlockQ<WorldType>(int val) where WorldType : struct, IWorldType { }
        public void Assert<WorldType>() where WorldType : struct, IWorldType { }
    }

    #if ENABLE_IL2CPP
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    #endif
    public struct TagArray : IQueryMethod {
        public List<EditorEntityDataMetaByWorld> Tags;
        private QueryMethodType Type;

        [MethodImpl(AggressiveInlining)]
        public TagArray(List<EditorEntityDataMetaByWorld> tags, QueryMethodType type) {
            Type = type;
            Tags = tags;
        }


        public void CheckChunk<WorldType>(ref ulong chunkMask, uint chunkIdx) where WorldType : struct, IWorldType {
            switch (Type) {
                case QueryMethodType.ALL:  
                    foreach (var type in Tags) {
                        chunkMask &= type.TagsPool.Chunk(chunkIdx).notEmptyBlocks;
                    }
                    break;
                case QueryMethodType.NONE: 
                    foreach (var type in Tags) {
                        chunkMask &= ~type.TagsPool.Chunk(chunkIdx).fullBlocks;
                    }
                    break;
                case QueryMethodType.ANY:  
                    ulong mask = 0;
                    foreach (var type in Tags) {
                        mask |= type.TagsPool.Chunk(chunkIdx).notEmptyBlocks;
                    }
                    chunkMask &= mask;
                    break;
            }
        }

        public void CheckEntities<WorldType>(ref ulong entitiesMask, uint chunkIdx, int blockIdx) where WorldType : struct, IWorldType {
            switch (Type) {
                case QueryMethodType.ALL:
                    foreach (var type in Tags) {
                        entitiesMask &= type.TagsPool.EMask(chunkIdx, blockIdx);
                    }
                    break;
                case QueryMethodType.NONE:
                    foreach (var type in Tags) {
                        entitiesMask &= ~type.TagsPool.EMask(chunkIdx, blockIdx);
                    }
                    break; 
                case QueryMethodType.ANY:
                    ulong mask3 = 0;
                    foreach (var type in Tags) {
                        mask3 |= type.TagsPool.EMask(chunkIdx, blockIdx);
                    }
                    entitiesMask &= mask3;
                    break;
            }
        }
        public void IncQ<WorldType>(QueryData data) where WorldType : struct, IWorldType { }
        public void DecQ<WorldType>() where WorldType : struct, IWorldType { }
        public void Assert<WorldType>() where WorldType : struct, IWorldType { }
        public void BlockQ<WorldType>(int val) where WorldType : struct, IWorldType { }
    }

    public struct WithArray : IQueryMethod {
        internal List<IQueryMethod> _methods;

        public WithArray(List<IQueryMethod> methods) {
            _methods = methods;
        }

        public void CheckChunk<WorldType>(ref ulong chunkMask, uint chunkIdx) where WorldType : struct, IWorldType {
            foreach (var method in _methods) {
                method.CheckChunk<WorldType>(ref chunkMask, chunkIdx);
            }
        }
        public void CheckEntities<WorldType>(ref ulong entitiesMask, uint chunkIdx, int blockIdx) where WorldType : struct, IWorldType {
            foreach (var method in _methods) {
                method.CheckEntities<WorldType>(ref entitiesMask, chunkIdx, blockIdx);
            }
        }
        public void IncQ<WorldType>(QueryData data) where WorldType : struct, IWorldType {
            foreach (var method in _methods) {
                method.IncQ<WorldType>(data);
            }
        }
        public void DecQ<WorldType>() where WorldType : struct, IWorldType {
            foreach (var method in _methods) {
                method.DecQ<WorldType>();
            }
        }
        public void BlockQ<WorldType>(int val) where WorldType : struct, IWorldType {
            foreach (var method in _methods) {
                method.BlockQ<WorldType>(val);
            }
        }

        public void Assert<WorldType>() where WorldType : struct, IWorldType {
            foreach (var method in _methods) {
                method.Assert<WorldType>();
            }
        }
    }
}
#endif
