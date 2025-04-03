#if UNITY_EDITOR
using System;
using System.Collections.Generic;

namespace FFS.Libraries.StaticEcs.Unity {
    public partial interface IStaticEcsEntityProvider {
        bool EntityIsActual();

        bool HasStandardComponents();
        void StandardComponents(List<IStandardComponent> result);
        bool ShouldShowStandardComponent(Type componentType, bool runtime);
        void OnChangeStandardComponent(IStandardComponent component, Type componentType);
        void OnSelectStandardComponent(IStandardComponent component);
        void OnDeleteStandardComponent(Type componentType);
        void DeleteAllBrokenStandardComponents();

        bool HasComponents();
        bool IsDisabled(Type componentType);
        void Disable(Type componentType);
        void Enable(Type componentType);
        void Components(List<IComponent> result);
        void OnSelectComponent(IComponent component);
        void OnChangeComponent(IComponent component, Type componentType);
        void OnDeleteComponent(Type componentType);
        bool ShouldShowComponent(Type componentType, bool runtime);
        void DeleteAllBrokenComponents();


        #if !FFS_ECS_DISABLE_TAGS
        void Tags(List<ITag> result);
        void OnSelectTag(Type tagType);
        void OnDeleteTag(Type tagType);
        bool ShouldShowTag(Type tagType, bool runtime);
        void DeleteAllBrokenTags();
        #endif

        #if !FFS_ECS_DISABLE_MASKS
        void Masks(List<IMask> result);
        void OnSelectMask(Type maskType);
        void OnDeleteMask(Type maskType);
        bool ShouldShowMask(Type maskType, bool runtime);
        void DeleteAllBrokenMasks();
        #endif
    }
}
#endif