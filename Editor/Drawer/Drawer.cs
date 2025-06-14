﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace FFS.Libraries.StaticEcs.Unity.Editor {
    public static class Drawer {
        private const int MaxFieldToStringLength = 128;

        private static readonly List<IStandardComponent> _standardComponentsCache = new();
        private static readonly List<IComponent> _componentsCache = new();
        #if !FFS_ECS_DISABLE_TAGS
        private static readonly List<ITag> _tagsCache = new();
        #endif
        #if !FFS_ECS_DISABLE_MASKS
        private static readonly List<IMask> _masksCache = new();
        #endif

        #if !FFS_ECS_DISABLE_EVENTS
        public static void DrawEvent(StaticEcsEventProvider provider, bool viewer, Action<StaticEcsEventProvider> onClickBuild, Action<StaticEcsEventProvider> onCopyTemplate = null) {
            provider.Scroll = EditorGUILayout.BeginScrollView(provider.Scroll, Ui.MaxWidth600);
            EditorGUILayout.Space(10);

            if (string.IsNullOrEmpty(provider.WorldTypeName)) {
                EditorGUILayout.HelpBox("Please, provide world", MessageType.Warning, true);
            }

            EditorGUILayout.BeginHorizontal(Ui.MaxWidth600);
            {
                var allowChangeWorld = provider.RuntimeEvent.IsEmpty() && !viewer;
                if (GUILayout.Button("+", allowChangeWorld ? Ui.ButtonStyleWhite : Ui.ButtonStyleGrey, Ui.WidthLine(20)) && allowChangeWorld) {
                    DrawWorldMenu(provider);
                }

                EditorGUILayout.LabelField("World:", Ui.WidthLine(90));
                EditorGUILayout.LabelField(provider.WorldEditorName, Ui.LabelStyleWhiteBold);
            }
            EditorGUILayout.EndHorizontal();

            if (string.IsNullOrEmpty(provider.WorldTypeName)) {
                EditorGUILayout.EndScrollView();
                return;
            }
            
            if (provider.EventTemplate == null && provider.RuntimeEvent.IsEmpty()) {
                EditorGUILayout.HelpBox("Please, provide event type", MessageType.Warning, true);
            }

            EditorGUILayout.BeginHorizontal(Ui.MaxWidth600);
            {
                var allowChangeEventType = provider.RuntimeEvent.IsEmpty();
                if (GUILayout.Button("+", allowChangeEventType ? Ui.ButtonStyleWhite : Ui.ButtonStyleGrey, Ui.WidthLine(20)) && allowChangeEventType) {
                    DrawEventsMenu(provider);
                }

                EditorGUILayout.LabelField("Type:", Ui.WidthLine(90));
                if (!provider.RuntimeEvent.IsEmpty()) {
                    EditorGUILayout.LabelField(provider.RuntimeEvent.Type.EditorTypeName(), Ui.LabelStyleWhiteBold);
                } else if (provider.EventTemplate != null) {
                    EditorGUILayout.LabelField(provider.EventTemplate.GetType().EditorTypeName(), Ui.LabelStyleWhiteBold);
                    if (Application.isPlaying && GUILayout.Button("Send", Ui.ButtonStyleYellow, Ui.WidthLine(60))) {
                        onClickBuild(provider);
                    }
                } else {
                    EditorGUILayout.LabelField("---", Ui.LabelStyleWhiteBold);
                }
            }
            EditorGUILayout.EndHorizontal();

            if (!provider.RuntimeEvent.IsEmpty()) {
                EditorGUILayout.BeginHorizontal(Ui.MaxWidth600);
                {
                    if (GUILayout.Button(Ui.IconMenu, Ui.WidthLine(20))) {
                        var menu = new GenericMenu();
                        if (viewer) {
                            menu.AddItem(new GUIContent("Close"), false, () => {
                                provider.RuntimeEvent = RuntimeEvent.Empty;
                                provider.EventCache = null;
                            });
                            menu.AddItem(new GUIContent("Send as new event"), false, () => {
                                var actualEvent = provider.GetActualEvent(out var _);
                                if (provider.World.Events().TryGetPool(actualEvent.GetType(), out var pool)) {
                                    pool.AddRaw(actualEvent);
                                    provider.EventCache = actualEvent;
                                    provider.RuntimeEvent = new RuntimeEvent {
                                        InternalIdx = pool.Last(),
                                        Type = actualEvent.GetType(),
                                        Version = pool.Version(pool.Last())
                                    };
                                }
                            });
                            menu.AddItem(new GUIContent("Copy as builder template"), false, () => {
                                onCopyTemplate?.Invoke(provider);
                            });
                        }

                        if (!provider.IsCached()) {
                            menu.AddItem(new GUIContent("Delete event"), false, provider.DeleteEvent);
                        }
                        menu.ShowAsContext();
                    }
                    
                    EditorGUILayout.LabelField("Event:", Ui.WidthLine(90));
                    EditorGUILayout.LabelField(provider.IsCached() ? "Read or suppressed" : "Sent", Ui.LabelStyleWhiteBold);
                }
                EditorGUILayout.EndHorizontal();
            }
            
            EditorGUILayout.Space();
            Ui.DrawHorizontalSeparator(Ui.MaxWidth600);

            if (provider.EventIsActual(Application.isPlaying)) {
                EditorGUILayout.Space(10);
                DrawEvent(provider.World, provider, Ui.MaxWidth600);
            }

            EditorGUILayout.EndScrollView();
        }

        private static void DrawEvent(IWorld world, StaticEcsEventProvider provider, GUILayoutOption[] maxWidth) {
            var eventValue = provider.GetActualEvent(out var cached);
            var type = eventValue.GetType();
            var typeName = type.EditorTypeName();

            var guiState = GUI.enabled;
            GUI.enabled = !cached;
            GUILayout.BeginHorizontal(GUI.skin.box, maxWidth);
            {
                GUILayout.BeginVertical(GUI.skin.box);
                {
                    if (TryDrawValueByCustomDrawer(world, typeName, type, eventValue, out var changed, out var newValue)) {
                        if (changed) {
                            provider.OnChangeEvent((IEvent) newValue);
                            EditorUtility.SetDirty(provider);
                        }
                    } else {
                        EditorGUILayout.LabelField(typeName, EditorStyles.boldLabel);
                        EditorGUI.indentLevel++;
                        foreach (var field in MetaData.GetCachedType(type)) {
                            if (TryDrawField(world, eventValue, field, out newValue)) {
                                field.SetValue(eventValue, newValue);
                                provider.OnChangeEvent(eventValue);
                                EditorUtility.SetDirty(provider);
                            }
                        }

                        EditorGUI.indentLevel--;
                    }
                }
                GUILayout.EndVertical();
            }
            GUILayout.EndHorizontal();
            EditorGUILayout.Space(2);
            GUI.enabled = guiState;
        }
        
        private static void DrawEventsMenu(StaticEcsEventProvider provider) {
            var menu = new GenericMenu();
            foreach (var eventDataMeta in MetaData.Events) {
                if (provider.EventTemplate != null && provider.EventTemplate.GetType() == eventDataMeta.Type) {
                    continue;
                }

                if (provider.ShouldShowEvent(eventDataMeta.Type, Application.isPlaying)) {
                    menu.AddItem(new GUIContent(eventDataMeta.FullName), false, objType => {
                                     var objRaw = Activator.CreateInstance((Type) objType, true);
                                     provider.OnSelectEvent((IEvent) objRaw);
                                     EditorUtility.SetDirty(provider);
                                 },
                                 eventDataMeta.Type);
                } else {
                    menu.AddDisabledItem(new GUIContent(eventDataMeta.FullName));
                }
            }

            menu.ShowAsContext();
        }
        #endif

        public static void DrawEntity(StaticEcsEntityProvider provider, bool viewer, Action<StaticEcsEntityProvider> onClickBuild, bool allowStandardComponentsAddDelete) {
            provider.Scroll = EditorGUILayout.BeginScrollView(provider.Scroll, Ui.MaxWidth600);
            EditorGUILayout.Space(10);

            if (string.IsNullOrEmpty(provider.WorldTypeName)) {
                EditorGUILayout.HelpBox("Please, provide world", MessageType.Warning, true);
            }

            EditorGUILayout.BeginHorizontal(Ui.MaxWidth600);
            {
                var allowChangeWorld = !provider.EntityIsActual() && !viewer;
                if (GUILayout.Button("+", allowChangeWorld ? Ui.ButtonStyleWhite : Ui.ButtonStyleGrey, Ui.WidthLine(20)) && allowChangeWorld) {
                    DrawWorldMenu(provider);
                }

                EditorGUILayout.LabelField("World:", Ui.WidthLine(60));
                EditorGUILayout.LabelField(provider.WorldEditorName, Ui.LabelStyleWhiteBold);
            }
            EditorGUILayout.EndHorizontal();

            if (string.IsNullOrEmpty(provider.WorldTypeName)) {
                EditorGUILayout.EndScrollView();
                return;
            }

            EditorGUILayout.BeginHorizontal(Ui.MaxWidth600);
            {
                if (GUILayout.Button(Ui.IconMenu, Ui.WidthLine(20))) {
                    var menu = new GenericMenu();

                    if (provider.EntityIsActual()) {
                        if (viewer) {
                            menu.AddItem(new GUIContent("Close"), false, () => { provider.Entity = null; });
                            // menu.AddItem(new GUIContent("Copy as new entity"), false, () => { });
                            // menu.AddItem(new GUIContent("Copy as builder template"), false, () => { });
                        }

                        menu.AddItem(new GUIContent("Delete entity"), false, () => {
                            provider.Entity.Destroy();
                            provider.Entity = null;
                        });
                    } else {
                        menu.AddItem(new GUIContent("Clear template"), false, provider.Clear);
                    }

                    menu.ShowAsContext();
                }

                EditorGUILayout.LabelField("Entity ID:", Ui.WidthLine(60));
                if (provider.EntityIsActual()) {
                    EditorGUILayout.LabelField(Ui.IntToStringD6((int) provider.Entity.GetId()).d6, Ui.LabelStyleWhiteBold);
                } else {
                    EditorGUILayout.LabelField("---", Ui.LabelStyleWhiteBold);
                    if (Application.isPlaying && provider.HasComponents() && GUILayout.Button("Build", Ui.ButtonStyleYellow, Ui.WidthLine(60))) {
                        onClickBuild(provider);
                    }
                }
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
            Ui.DrawHorizontalSeparator(Ui.MaxWidth600);

            if (!provider.EntityIsActual() && !provider.HasComponents()) {
                EditorGUILayout.HelpBox("Please, provide at least one component", MessageType.Warning, true);
            }

            DrawEntity(provider.World, provider, allowStandardComponentsAddDelete);
            EditorGUILayout.EndScrollView();
        }

        public static void DrawEntity<TProvider>(IWorld world, TProvider provider, bool allowStandardComponentsAddDelete = true) where TProvider : Object, IStaticEcsEntityProvider {
            EditorGUILayout.Space(10);

            provider.StandardComponents(_standardComponentsCache);
            EditorGUILayout.Space(10);
            DrawStandardComponents(world, _standardComponentsCache, provider, Ui.MaxWidth600, allowStandardComponentsAddDelete);
            _standardComponentsCache.Clear();

            provider.Components(_componentsCache);
            EditorGUILayout.Space(10);
            DrawComponents(world, _componentsCache, provider, Ui.MaxWidth600);
            _componentsCache.Clear();

            #if !FFS_ECS_DISABLE_TAGS
            provider.Tags(_tagsCache);
            EditorGUILayout.Space(10);
            DrawTags(_tagsCache, provider, Ui.MaxWidth600);
            _tagsCache.Clear();
            #endif

            #if !FFS_ECS_DISABLE_MASKS
            provider.Masks(_masksCache);
            EditorGUILayout.Space(10);
            DrawMasks(_masksCache, provider, Ui.MaxWidth600);
            _masksCache.Clear();
            #endif
        }

        private static void DrawWorldMenu(AbstractStaticEcsProvider provider) {
            var menu = new GenericMenu();
            for (var i = 0; i < MetaData.WorldsMetaData.Count; i++) {
                if (provider.WorldTypeName != null && MetaData.WorldsMetaData[i].WorldTypeType.FullName == provider.WorldTypeName) {
                    continue;
                }

                var i1 = i;
                menu.AddItem(
                    new GUIContent(MetaData.WorldsMetaData[i].EditorName),
                    false,
                    objType => {
                        provider.WorldEditorName = MetaData.WorldsMetaData[i1].EditorName;
                        provider.WorldTypeName = ((Type) objType).FullName;
                        EditorUtility.SetDirty(provider);
                    },
                    MetaData.WorldsMetaData[i1].WorldTypeType);
            }

            menu.ShowAsContext();
        }

        private static void DrawStandardComponents<TProvider>(IWorld world, List<IStandardComponent> components, TProvider provider, GUILayoutOption[] maxWidth, bool allowAddDelete) where TProvider : Object, IStaticEcsEntityProvider {
            EditorGUILayout.BeginHorizontal();
            {
                var hasAll = MetaData.StandardComponents.Count == components.Count;
                GUI.enabled = allowAddDelete;
                if (GUILayout.Button("+", hasAll ? Ui.ButtonStyleGrey : Ui.ButtonStyleWhite, Ui.WidthLine(20)) && !hasAll) {
                    DrawStandardComponentsMenu(components, provider);
                }
                GUI.enabled = true;

                EditorGUILayout.LabelField("Standard components:", Ui.HeaderStyleWhite, Ui.WidthLine(200));
            }
            EditorGUILayout.EndHorizontal();

            for (int i = 0, iMax = components.Count; i < iMax; i++) {
                DrawStandardComponent(world, components[i], provider, maxWidth, false, allowAddDelete);
            }
        }

        private static void DrawStandardComponent<TProvider>(IWorld world, IStandardComponent component, TProvider provider, GUILayoutOption[] maxWidth, bool readOnly, bool allowAddDelete) where TProvider : Object, IStaticEcsEntityProvider {
            if (component == null) {
                EditorGUILayout.LabelField("Broken standard component - is null", EditorStyles.boldLabel);
                if (GUILayout.Button("Delete all broken standard components", Ui.ButtonStyleWhite, Ui.WidthLine(240))) {
                    provider.DeleteAllBrokenStandardComponents();
                    EditorUtility.SetDirty(provider);
                }
                EditorGUILayout.Space(2);
                return;
            }
            
            var type = component.GetType();
            var typeName = type.EditorTypeName();

            GUI.enabled = !readOnly;
            GUILayout.BeginHorizontal(GUI.skin.box, maxWidth);
            {
                GUILayout.BeginVertical(GUI.skin.box);
                {
                    if (TryDrawValueByCustomDrawer(world, typeName, type, component, out var changed, out var newValue)) {
                        if (changed) {
                            provider.OnChangeStandardComponent((IStandardComponent) newValue, type);
                            EditorUtility.SetDirty(provider);
                        }
                    } else {
                        EditorGUILayout.LabelField(typeName, EditorStyles.boldLabel);
                        EditorGUI.indentLevel++;
                        foreach (var field in MetaData.GetCachedType(type)) {
                            if (TryDrawField(world, component, field, out newValue)) {
                                field.SetValue(component, newValue);
                                provider.OnChangeStandardComponent(component, type);
                                EditorUtility.SetDirty(provider);
                            }
                        }

                        EditorGUI.indentLevel--;
                    }
                }
                GUILayout.EndVertical();
                
                GUI.enabled = allowAddDelete;
                if (GUILayout.Button(Ui.IconTrash, Ui.WidthLine(30))) {
                    provider.OnDeleteStandardComponent(type);
                    EditorUtility.SetDirty(provider);
                }
                GUI.enabled = !readOnly;
            }
            GUILayout.EndHorizontal();
            EditorGUILayout.Space(2);
            GUI.enabled = true;
        }
        
        private static void DrawStandardComponentsMenu<TProvider>(List<IStandardComponent> actualComponents, TProvider provider) where TProvider : Object, IStaticEcsEntityProvider {
            var menu = new GenericMenu();
            foreach (var component in MetaData.StandardComponents) {
                var has = false;
                foreach (var actual in actualComponents) {
                    if (actual.GetType() == component.Type) {
                        has = true;
                        break;
                    }
                }


                if (has) continue;

                if (provider.ShouldShowStandardComponent(component.Type, Application.isPlaying)) {
                    menu.AddItem(new GUIContent(component.FullName), false, objType => {
                                     var objRaw = Activator.CreateInstance((Type) objType, true);
                                     provider.OnSelectStandardComponent((IStandardComponent) objRaw);
                                     EditorUtility.SetDirty(provider);
                                 },
                                 component.Type);
                } else {
                    menu.AddDisabledItem(new GUIContent(component.FullName));
                }
            }

            menu.ShowAsContext();
        }

        private static void DrawComponents<TProvider>(IWorld world, List<IComponent> components, TProvider provider, GUILayoutOption[] maxWidth) where TProvider : Object, IStaticEcsEntityProvider {
            EditorGUILayout.BeginHorizontal();
            {
                var hasAll = MetaData.Components.Count == components.Count;
                if (GUILayout.Button("+", hasAll ? Ui.ButtonStyleGrey : Ui.ButtonStyleWhite, Ui.WidthLine(20)) && !hasAll) {
                    DrawComponentsMenu(components, provider);
                }

                EditorGUILayout.LabelField("Components:", Ui.HeaderStyleWhite, Ui.WidthLine(200));
            }
            EditorGUILayout.EndHorizontal();

            for (int i = 0, iMax = components.Count; i < iMax; i++) {
                var component = components[i];

                if (component == null) {
                    EditorGUILayout.LabelField($"Broken component - is null, index {i}", EditorStyles.boldLabel);
                    if (GUILayout.Button("Delete all broken components", Ui.ButtonStyleWhite, Ui.WidthLine(240))) {
                        provider.DeleteAllBrokenComponents();
                        EditorUtility.SetDirty(provider);
                    }
                    EditorGUILayout.Space(2);
                    continue;
                }
                
                var type = component.GetType();
                var typeName = type.EditorTypeName();
                var disabled = provider.IsDisabled(type);
                if (disabled) {
                    typeName += " [Disabled]";
                }

                GUILayout.BeginHorizontal(GUI.skin.box, maxWidth);
                {
                    GUILayout.BeginVertical(GUI.skin.box);
                    {
                        if (TryDrawValueByCustomDrawer(world, typeName, type, component, out var changed, out var newValue)) {
                            if (changed) {
                                provider.OnChangeComponent((IComponent) newValue, type);
                                EditorUtility.SetDirty(provider);
                            }
                        } else {
                            EditorGUILayout.LabelField(typeName, EditorStyles.boldLabel);
                            EditorGUI.indentLevel++;
                            foreach (var field in MetaData.GetCachedType(type)) {
                                if (TryDrawField(world, component, field, out newValue)) {
                                    field.SetValue(component, newValue);
                                    provider.OnChangeComponent(component, type);
                                    EditorUtility.SetDirty(provider);
                                }
                            }

                            EditorGUI.indentLevel--;
                        }
                    }
                    GUILayout.EndVertical();
                    
                    GUILayout.BeginVertical(Ui.Width(30));
                    if (GUILayout.Button(Ui.IconTrash, Ui.WidthLine(30))) {
                        provider.OnDeleteComponent(type);
                        EditorUtility.SetDirty(provider);
                    }
                    const string DataOn = "☑";
                    const string DataOff = "☐";
                    if (provider.EntityIsActual()) {
                        if (disabled) {
                            if (GUILayout.Button(DataOff, Ui.WidthLine(30))) {
                                provider.Enable(type);
                                EditorUtility.SetDirty(provider);
                            }
                        } else {
                            if (GUILayout.Button(DataOn, Ui.WidthLine(30))) {
                                provider.Disable(type);
                                EditorUtility.SetDirty(provider);
                            } 
                        }
                    }
         
                    GUILayout.EndVertical();
                }
                GUILayout.EndHorizontal();
                EditorGUILayout.Space(2);
            }
        }

        private static void DrawComponentsMenu<TProvider>(List<IComponent> actualComponents, TProvider provider) where TProvider : Object, IStaticEcsEntityProvider {
            var menu = new GenericMenu();
            foreach (var component in MetaData.Components) {
                var has = false;
                foreach (var actual in actualComponents) {
                    if (actual.GetType() == component.Type) {
                        has = true;
                        break;
                    }
                }


                if (has) continue;

                if (provider.ShouldShowComponent(component.Type, Application.isPlaying)) {
                    menu.AddItem(new GUIContent(component.FullName), false, objType => {
                                     var objRaw = Activator.CreateInstance((Type) objType, true);
                                     provider.OnSelectComponent((IComponent) objRaw);
                                     EditorUtility.SetDirty(provider);
                                 },
                                 component.Type);
                } else {
                    menu.AddDisabledItem(new GUIContent(component.FullName));
                }
            }

            menu.ShowAsContext();
        }
        
        #if !FFS_ECS_DISABLE_TAGS
        private static void DrawTags<TProvider>(List<ITag> tags, TProvider provider, GUILayoutOption[] maxWidth) where TProvider : Object, IStaticEcsEntityProvider {
            EditorGUILayout.BeginHorizontal();
            {
                var hasAll = MetaData.Tags.Count == tags.Count;
                if (GUILayout.Button("+", hasAll ? Ui.ButtonStyleGrey : Ui.ButtonStyleWhite, Ui.WidthLine(20)) && !hasAll) {
                    DrawTagsMenu(tags, provider);
                }

                EditorGUILayout.LabelField("Tags:", Ui.HeaderStyleWhite, Ui.WidthLine(200));
            }
            EditorGUILayout.EndHorizontal();

            for (int i = 0, iMax = tags.Count; i < iMax; i++) {
                var tag = tags[i];
                if (tag == null) {
                    EditorGUILayout.LabelField($"Broken tag - is null, index {i}", EditorStyles.boldLabel);
                    if (GUILayout.Button("Delete all broken tags", Ui.ButtonStyleWhite, Ui.WidthLine(240))) {
                        provider.DeleteAllBrokenTags();
                        EditorUtility.SetDirty(provider);
                    }
                    EditorGUILayout.Space(2);
                    continue;
                }
                
                var type = tag.GetType();
                EditorGUILayout.BeginHorizontal(GUI.skin.box, maxWidth);
                {
                    EditorGUILayout.SelectableLabel(type.EditorTypeName(), EditorStyles.boldLabel, Ui.MaxWidth600SingleLine);
                    if (GUILayout.Button(Ui.IconTrash, Ui.WidthLine(30))) {
                        provider.OnDeleteTag(type);
                        EditorUtility.SetDirty(provider);
                    }
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space(2);
            }
        }

        private static void DrawTagsMenu<TProvider>(List<ITag> actualTags, TProvider provider) where TProvider : Object, IStaticEcsEntityProvider {
            var menu = new GenericMenu();
            foreach (var tag in MetaData.Tags) {
                var has = false;
                foreach (var actual in actualTags) {
                    if (actual.GetType() == tag.Type) {
                        has = true;
                        break;
                    }
                }


                if (has) continue;

                if (provider.ShouldShowTag(tag.Type, Application.isPlaying)) {
                    menu.AddItem(new GUIContent(tag.FullName), false, objType => {
                                     provider.OnSelectTag((Type) objType);
                                     EditorUtility.SetDirty(provider);
                                 },
                                 tag.Type);
                } else {
                    menu.AddDisabledItem(new GUIContent(tag.FullName));
                }
            }

            menu.ShowAsContext();
        }
        #endif

        #if !FFS_ECS_DISABLE_MASKS
        private static void DrawMasks<TProvider>(List<IMask> masks, TProvider provider, GUILayoutOption[] maxWidth) where TProvider : Object, IStaticEcsEntityProvider {
            EditorGUILayout.BeginHorizontal();
            {
                var hasAll = MetaData.Masks.Count == masks.Count;
                if (GUILayout.Button("+", hasAll ? Ui.ButtonStyleGrey : Ui.ButtonStyleWhite, Ui.WidthLine(20)) && !hasAll) {
                    DrawMasksMenu(masks, provider);
                }

                EditorGUILayout.LabelField("Masks:", Ui.HeaderStyleWhite, Ui.WidthLine(200));
            }
            EditorGUILayout.EndHorizontal();

            for (int i = 0, iMax = masks.Count; i < iMax; i++) {
                var mask =  masks[i];
                if (mask == null) {
                    EditorGUILayout.LabelField($"Broken mask - is null, index {i}", EditorStyles.boldLabel);
                    if (GUILayout.Button("Delete all broken masks", Ui.ButtonStyleWhite, Ui.WidthLine(240))) {
                        provider.DeleteAllBrokenMasks();
                        EditorUtility.SetDirty(provider);
                    }
                    EditorGUILayout.Space(2);
                    continue;
                }
                var type = mask.GetType();
                EditorGUILayout.BeginHorizontal(GUI.skin.box, maxWidth);
                {
                    EditorGUILayout.SelectableLabel(type.EditorTypeName(), EditorStyles.boldLabel, Ui.MaxWidth600SingleLine);
                    if (GUILayout.Button(Ui.IconTrash, Ui.WidthLine(30))) {
                        provider.OnDeleteMask(type);
                        EditorUtility.SetDirty(provider);
                    }
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space(2);
            }
        }
        
        private static void DrawMasksMenu<TProvider>(List<IMask> actualMasks, TProvider provider) where TProvider : Object, IStaticEcsEntityProvider {
            var menu = new GenericMenu();
            foreach (var mask in MetaData.Masks) {
                var has = false;
                foreach (var actual in actualMasks) {
                    if (actual.GetType() == mask.Type) {
                        has = true;
                        break;
                    }
                }

                if (has) continue;

                if (provider.ShouldShowMask(mask.Type, Application.isPlaying)) {
                    menu.AddItem(new GUIContent(mask.FullName), false, objType => {
                                     provider.OnSelectMask((Type) objType);
                                     EditorUtility.SetDirty(provider);
                                 },
                                 mask.Type);
                } else {
                    menu.AddDisabledItem(new GUIContent(mask.FullName));
                }
            }

            menu.ShowAsContext();
        }
        #endif

        internal static void DrawField(object component, FieldInfo field, GUIStyle style, GUILayoutOption[] layout) {
            var fieldValue = field.GetValue(component);
            var fieldType = field.FieldType;
            DrawTableValue(style, layout, fieldType, fieldValue);
        }

        internal static void DrawProperty(object component, PropertyInfo field, GUIStyle style, GUILayoutOption[] layout) {
            var fieldValue = field.GetValue(component);
            var fieldType = field.PropertyType;
            DrawTableValue(style, layout, fieldType, fieldValue);
        }

        private static void DrawTableValue(GUIStyle style, GUILayoutOption[] layout, Type fieldType, object fieldValue) {
            if (TryDrawTableValueByCustomDrawer(fieldType, fieldValue, style, layout)) {
                return;
            }

            if (fieldType == MetaData.UnityObjectType || fieldType.IsSubclassOf(MetaData.UnityObjectType)) {
                EditorGUILayout.ObjectField((Object) fieldValue, fieldType, true, layout);
                return;
            }

            if (fieldType.IsEnum) {
                var isFlags = Attribute.IsDefined(fieldType, MetaData.EnumFlagsType);
                DrawEnum(fieldValue, isFlags, style, layout);
                return;
            }

            var strVal = fieldValue != null ? string.Format(CultureInfo.InvariantCulture, "{0}", fieldValue) : "null";
            if (strVal.Length > MaxFieldToStringLength) {
                strVal = strVal.Substring(0, MaxFieldToStringLength);
            }

            EditorGUILayout.LabelField(strVal, style, layout);
        }

        internal static bool TryDrawField(IWorld world, object component, FieldInfo field, out object newValue) {
            var fieldValue = field.GetValue(component);
            var fieldType = field.FieldType;
            if (TryDrawValueByCustomDrawer(world, field.Name, fieldType, fieldValue, out var changed, out newValue)) {
                return changed;
            }

            if (fieldType == MetaData.UnityObjectType || fieldType.IsSubclassOf(MetaData.UnityObjectType)) {
                GUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel(field.Name);
                newValue = EditorGUILayout.ObjectField((Object) fieldValue, fieldType, true);
                GUILayout.EndHorizontal();
                return (Object) newValue != (Object) fieldValue;
            }

            if (fieldType.IsEnum) {
                var isFlags = Attribute.IsDefined(fieldType, MetaData.EnumFlagsType);
                return DrawEnum(field.Name, fieldValue, isFlags, out newValue);
            }

            var strVal = fieldValue != null ? string.Format(CultureInfo.InvariantCulture, "{0}", fieldValue) : "null";
            if (strVal.Length > MaxFieldToStringLength) {
                strVal = strVal.Substring(0, MaxFieldToStringLength);
            }

            DrawSelectableText(field.Name, strVal);
            return false;
        }

        internal static bool TryDrawValueByCustomDrawer(IWorld world, string label, Type type, object value, out bool changed, out object newValue) {
            if (MetaData.Inspectors.TryGetValue(type, out var inspector)) {
                changed = inspector.DrawValue(world, label, value, out newValue);
                return true;
            }
            
            if (type.IsGenericType && MetaData.InspectorsGeneric.TryGetValue(type.GetGenericTypeDefinition(), out var inspectorType)) {
                var ins = (IStaticEcsValueDrawer) Activator.CreateInstance(inspectorType.MakeGenericType(type.GetGenericArguments()));
                MetaData.Inspectors[type] = ins;
                changed = ins.DrawValue(world, label, value, out newValue);
                return true;
            }
            
            if (type.IsArray && MetaData.InspectorsGeneric.TryGetValue(type.BaseType, out var inspectorTypeArray)) {
                var ins = (IStaticEcsValueDrawer) Activator.CreateInstance(inspectorTypeArray.MakeGenericType(type.GetElementType()));
                MetaData.Inspectors[type] = ins;
                changed = ins.DrawValue(world, label, value, out newValue);
                return true;
            }

            changed = false;
            newValue = default;
            return false;
        }

        internal static bool TryDrawTableValueByCustomDrawer(Type type, object value, GUIStyle style, GUILayoutOption[] layoutOptions) {
            if (MetaData.Inspectors.TryGetValue(type, out var inspector)) {
                inspector.DrawTableValue(value, style, layoutOptions);
                return true;
            }
            
            if (type.IsGenericType && MetaData.InspectorsGeneric.TryGetValue(type.GetGenericTypeDefinition(), out var inspectorType)) {
                var ins = (IStaticEcsValueDrawer) Activator.CreateInstance(inspectorType.MakeGenericType(type.GetGenericArguments()));
                MetaData.Inspectors[type] = ins;
                ins.DrawTableValue(value, style, layoutOptions);
                return true;
            }

            return false;
        }

        internal static void DrawSelectableText(string label, string text) {
            GUILayout.BeginHorizontal();
            {
                EditorGUILayout.PrefixLabel(label);
                EditorGUILayout.SelectableLabel(text, GUILayout.MaxHeight(EditorGUIUtility.singleLineHeight));
            }
            GUILayout.EndHorizontal();
        }

        internal static bool DrawEnum(string label, object value, bool isFlags, out object newValue) {
            newValue = isFlags
                ? EditorGUILayout.EnumFlagsField(label, (Enum) value)
                : EditorGUILayout.EnumPopup(label, (Enum) value);

            return !Equals(newValue, value);
        }

        static void DrawEnum(object value, bool isFlags, GUIStyle style, GUILayoutOption[] layout) {
            if (isFlags) {
                EditorGUILayout.EnumFlagsField((Enum) value, style, layout);
            } else {
                EditorGUILayout.EnumPopup((Enum) value, style, layout);
            }
        }
    }
}