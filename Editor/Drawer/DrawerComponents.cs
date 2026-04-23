using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace FFS.Libraries.StaticEcs.Unity.Editor {
    public static partial class Drawer {
        private static readonly List<int> _sortOrderCache = new();
        private static readonly Dictionary<int, string> _componentFilters = new();
        private static readonly List<ManagedReferenceMissingType> _missingPool = new();
        private static readonly HashSet<long> _brokenShowData = new();
        private static readonly Dictionary<string, int> _groupMatchCounts = new();

        private static void DrawComponentsAndTags<TWorld>(List<IComponentOrTagProvider> componentsAndTags, StaticEcsEntityProvider<TWorld> provider, DrawMode mode) where TWorld : struct, IWorldType {
            if (componentsAndTags.Count == 0 && !provider.EntityIsActual()) {
                DrawFirstComponentButton(componentsAndTags, provider);
                return;
            }

            EditorGUILayout.BeginHorizontal();
            DrawSearchField(provider, out var filter);
            DrawSortButtons(provider);
            DrawAddComponentButton(componentsAndTags, provider);
            EditorGUILayout.EndHorizontal();
            
            DrawComponentsAndTagsInternal(componentsAndTags, provider, mode, filter);
        }

        private static void DrawFirstComponentButton<TWorld>(List<IComponentOrTagProvider> providers, StaticEcsEntityProvider<TWorld> provider) where TWorld : struct, IWorldType {
            EditorGUILayout.Space(4);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.Space(8);
            GUILayout.Label("No components or tags yet", Ui.LabelStyleGreyCenter);
            EditorGUILayout.Space(6);
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            using (Ui.EnabledScopeVal(GUI.enabled)) {
                if (GUILayout.Button(new GUIContent("＋  Add Component"), GUILayout.Width(180), GUILayout.Height(28))) {
                    DrawProvidersMenu(providers, provider);
                }
            }

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(8);
            EditorGUILayout.EndVertical();
        }

        private static void DrawSearchField<TWorld>(StaticEcsEntityProvider<TWorld> provider, out string filter)
            where TWorld : struct, IWorldType {
            var filterKey = provider.GetInstanceID();
            _componentFilters.TryGetValue(filterKey, out filter);
            filter ??= string.Empty;
            
            var searchStyle = GUI.skin.FindStyle("ToolbarSearchTextField") ?? EditorStyles.toolbarTextField;
            var hasFilter = filter.Length > 0;
            var cancelStyle = GUI.skin.FindStyle(hasFilter ? "ToolbarSearchCancelButton" : "ToolbarSearchCancelButtonEmpty") ?? EditorStyles.toolbarButton;
            var newFilter = GUILayout.TextField(filter, searchStyle, GUILayout.ExpandWidth(true), GUILayout.MinWidth(50));
            if (GUILayout.Button(GUIContent.none, cancelStyle) && hasFilter) {
                newFilter = string.Empty;
                GUI.FocusControl(null);
            }

            if (newFilter != filter) {
                if (string.IsNullOrEmpty(newFilter)) _componentFilters.Remove(filterKey);
                else _componentFilters[filterKey] = newFilter;
                filter = newFilter;
            }
        }

        private static void DrawSortButtons<TWorld>(StaticEcsEntityProvider<TWorld> provider)
            where TWorld : struct, IWorldType {
            var worldSettings = StaticEcsViewConfig.Active.GetOrCreate(typeof(TWorld).FullName);
            using (Ui.EnabledScopeVal(!provider.EntityIsActual() && GUI.enabled)) {
                if (GUILayout.Button(new GUIContent("⇅", "Sort components now"), EditorStyles.miniButton, GUILayout.Width(26))) {
                    SortProvidersInPlace(provider);
                }
            }

            var sortOn = worldSettings.entities.sortComponents;
            var newSortOn = GUILayout.Toggle(sortOn,
                                             new GUIContent("A⇅", sortOn ? "Auto-sort: ON (click to disable)" : "Auto-sort: OFF (click to enable)"),
                                             EditorStyles.miniButton, GUILayout.Width(26));
            if (newSortOn != sortOn) {
                worldSettings.entities.sortComponents = newSortOn;
                StaticEcsViewConfig.Active.MarkDirty();
            }
        }

        private static void DrawAddComponentButton<TWorld>(List<IComponentOrTagProvider> componentsAndTags, StaticEcsEntityProvider<TWorld> provider)
            where TWorld : struct, IWorldType {
            using (Ui.EnabledScopeVal(GUI.enabled)) {
                if (EditorGUILayout.DropdownButton(new GUIContent("＋ Add"), FocusType.Keyboard, EditorStyles.miniPullDown, GUILayout.Width(80))) {
                    DrawProvidersMenu(componentsAndTags, provider);
                }
            }
        }
        
        private static void DrawComponentsAndTagsInternal<TWorld>(List<IComponentOrTagProvider> componentsAndTags, StaticEcsEntityProvider<TWorld> provider, DrawMode mode, string filter)
            where TWorld : struct, IWorldType {

            var sorted = StaticEcsViewConfig.Active.GetOrCreate(typeof(TWorld).FullName).entities.sortComponents;
            var order = BuildProviderOrder(componentsAndTags, sorted);
            MissingReferenceMigration.FillMissing(provider, _missingPool);

            var hasFilter = filter.Length > 0;

            _groupMatchCounts.Clear();
            if (hasFilter) {
                for (var j = 0; j < componentsAndTags.Count; j++) {
                    var item = componentsAndTags[order[j]];
                    if (item?.ComponentType == null) continue;
                    if (item.ComponentType.EditorTypeName().IndexOf(filter, StringComparison.OrdinalIgnoreCase) < 0) continue;
                    if (item.ComponentType.EditorTypeGroup(out var gName, out _, out _)) {
                        _groupMatchCounts.TryGetValue(gName, out var c);
                        _groupMatchCounts[gName] = c + 1;
                    }
                }
            }

            string currentGroup = null;
            var currentGroupOpen = false;
            var currentGroupVerticalOpen = false;

            for (var j = 0; j < componentsAndTags.Count; j++) {
                var index = order[j];
                var componentOrTag = componentsAndTags[index];

                if (componentOrTag?.ComponentType == null) {
                    EndGroupSection(ref currentGroup, ref currentGroupVerticalOpen);
                    currentGroupOpen = false;
                    DrawBrokenProviderSlot(componentsAndTags, index, componentOrTag, provider, _missingPool);
                    continue;
                }

                var hasGroup = componentOrTag.ComponentType.EditorTypeGroup(out var groupName, out var groupColor, out var groupHasColor);

                if (hasFilter && componentOrTag.ComponentType.EditorTypeName().IndexOf(filter, StringComparison.OrdinalIgnoreCase) < 0) {
                    continue;
                }

                if (groupName != currentGroup) {
                    EndGroupSection(ref currentGroup, ref currentGroupVerticalOpen);
                    currentGroup = groupName;
                    currentGroupOpen = false;
                    if (hasGroup) {
                        if (hasFilter) {
                            _groupMatchCounts.TryGetValue(groupName, out var matches);
                            if (matches == 0) {
                                continue;
                            }
                        }
                        currentGroupOpen = BeginGroupSection(provider, typeof(TWorld), groupName, groupColor, groupHasColor, hasFilter);
                        currentGroupVerticalOpen = currentGroupOpen;
                    }
                }

                if (hasGroup && !currentGroupOpen) continue;

                if (componentOrTag.Kind.IsTag()) {
                    DrawTag(componentOrTag, provider);
                } else {
                    DrawComponent(componentOrTag, provider, mode);
                }
            }

            EndGroupSection(ref currentGroup, ref currentGroupVerticalOpen);
        }

        private static int GroupFoldoutKey<TWorld>(StaticEcsEntityProvider<TWorld> provider, string groupName) where TWorld : struct, IWorldType {
            return HashCode.Combine(provider.GetInstanceID(), "ECS_GROUP", groupName);
        }

        private static bool BeginGroupSection<TWorld>(StaticEcsEntityProvider<TWorld> provider, Type worldType, string groupName, Color groupColor, bool groupHasColor, bool forceOpen)
            where TWorld : struct, IWorldType {
            var key = GroupFoldoutKey(provider, groupName);
            if (initializedFoldouts.Add(key)) {
                ApplyGroupFoldoutConfig(key, groupName, worldType);
            }

            var barColor = groupHasColor ? groupColor : new Color(0.5f, 0.5f, 0.5f, 1f);
            bool show = DrawGroupFoldoutHeader(key, groupName, barColor, groupHasColor);

            if (!forceOpen) {
                SyncGroupFoldoutToConfig(key, groupName, worldType);
            }

            if (forceOpen) show = true;

            if (show) {
                EditorGUILayout.BeginVertical(GUI.skin.box);
                EditorGUI.indentLevel++;
            }

            return show;
        }

        private static bool DrawGroupFoldoutHeader(int keyHash, string groupName, Color barColor, bool useColorForText) {
            var open = openHideFlags.Contains(keyHash);

            EditorGUILayout.BeginVertical(GUI.skin.box);
            var rect = EditorGUILayout.GetControlRect();
            rect = EditorGUI.IndentedRect(rect);

            const float barWidth = 3f;
            const float gap = 6f;
            var barRect = new Rect(rect.x, rect.y, barWidth, rect.height);
            var btnRect = new Rect(rect.x + barWidth + gap, rect.y, rect.width - barWidth - gap, rect.height);

            EditorGUI.DrawRect(barRect, barColor);

            var style = new GUIStyle(EditorStyles.boldLabel) {
                hover = EditorStyles.iconButton.hover,
                active = EditorStyles.iconButton.active,
                focused = EditorStyles.iconButton.focused,
                alignment = TextAnchor.MiddleLeft,
            };
            if (useColorForText) {
                style.normal.textColor = barColor;
                style.normal.background = null;
            }

            using (Ui.EnabledScope) {
                if (GUI.Button(btnRect, $"{(open ? "▾" : "▸")} {groupName}", style)) {
                    if (open) openHideFlags.Remove(keyHash);
                    else openHideFlags.Add(keyHash);
                    GUIUtility.keyboardControl = 0;
                    GUIUtility.hotControl = 0;
                    EditorGUIUtility.editingTextField = false;
                    open = !open;
                }
            }

            EditorGUILayout.EndVertical();
            return open;
        }

        private static void EndGroupSection(ref string currentGroup, ref bool verticalOpen) {
            if (verticalOpen) {
                EditorGUI.indentLevel--;
                EditorGUILayout.EndVertical();
                verticalOpen = false;
            }
            currentGroup = null;
        }

        private static void ApplyGroupFoldoutConfig(int foldoutKey, string groupName, Type worldType) {
            var ws = StaticEcsViewConfig.Active.GetOrCreate(worldType.FullName);
            var inOverrides = ws.entities.groupOverrides.Contains(groupName);
            var open = ws.entities.defaultGroupExpanded ^ inOverrides;
            if (open) openHideFlags.Add(foldoutKey);
        }

        private static void SyncGroupFoldoutToConfig(int foldoutKey, string groupName, Type worldType) {
            var ws = StaticEcsViewConfig.Active.GetOrCreate(worldType.FullName);
            var nowOpen = openHideFlags.Contains(foldoutKey);
            var defaultOpen = ws.entities.defaultGroupExpanded;
            var shouldBeInOverrides = nowOpen != defaultOpen;
            var isInOverrides = ws.entities.groupOverrides.Contains(groupName);
            if (shouldBeInOverrides == isInOverrides) return;
            if (shouldBeInOverrides) ws.entities.groupOverrides.Add(groupName);
            else ws.entities.groupOverrides.Remove(groupName);
            StaticEcsViewConfig.Active.MarkDirty();
        }

        private static void DrawTag<TWorld>(IComponentOrTagProvider tag, StaticEcsEntityProvider<TWorld> provider) where TWorld : struct, IWorldType {
            var type = tag.ComponentType;

            EditorGUI.indentLevel++;
            EditorGUILayout.BeginHorizontal(GUI.skin.box);
            var colored = type.EditorTypeColor(out var color);
            var labelStyle = colored
                ? new GUIStyle(EditorStyles.boldLabel) { normal = { textColor = color } }
                : EditorStyles.boldLabel;
            GUILayout.Label(type.EditorTypeName(), labelStyle, GUILayout.MaxHeight(EditorGUIUtility.singleLineHeight));
            var labelRect = GUILayoutUtility.GetLastRect();
            var evt = Event.current;
            if (evt.type == EventType.MouseDown && evt.button == 0 && labelRect.Contains(evt.mousePosition)) {
                var script = MetaData.GetSourceScript(type);
                if (script != null) {
                    if (evt.clickCount >= 2) AssetDatabase.OpenAsset(script);
                    else EditorGUIUtility.PingObject(script);
                    evt.Use();
                }
            }

            if (evt.type == EventType.Repaint) {
                EditorGUIUtility.AddCursorRect(labelRect, MouseCursor.Link);
            }

            if (Ui.TrashButton) {
                provider.OnDeleteProvider(type);
                EditorUtility.SetDirty(provider);
            }

            EditorGUILayout.EndHorizontal();
            EditorGUI.indentLevel--;
        }

        private static void DrawComponent<TWorld>(IComponentOrTagProvider component, StaticEcsEntityProvider<TWorld> provider, DrawMode mode) where TWorld : struct, IWorldType {
            var type = component.ComponentType;

            var typeName = type.EditorTypeName();
            var disabled = provider.IsDisabled(type);
            if (disabled) {
                typeName += " [Disabled]";
            }

            bool show;
            var foldoutKey = HashCode.Combine(provider, type.FullName);
            if (initializedFoldouts.Add(foldoutKey)) {
                ApplyFoldoutConfig(foldoutKey, type, typeof(TWorld));
            }

            EditorGUILayout.BeginHorizontal(GUI.skin.box);
            {
                var colored = type.EditorTypeColor(out var color);
                if (colored) {
                    DrawFoldoutBox(foldoutKey, typeName, typeName, out show, color);
                } else {
                    DrawFoldoutBox(foldoutKey, typeName, typeName, out show);
                }

                if (Ui.MenuButton) {
                    var menu = new GenericMenu();
                    if (provider.EntityIsActual() && typeof(IDisableable).IsAssignableFrom(type)) {
                        if (disabled) {
                            menu.AddItem(new GUIContent("Enable"), false, () => {
                                provider.Enable(type);
                                EditorUtility.SetDirty(provider);
                            });
                        } else {
                            menu.AddItem(new GUIContent("Disable"), false, () => {
                                provider.Disable(type);
                                EditorUtility.SetDirty(provider);
                            });
                        }
                    }

                    menu.AddItem(new GUIContent("Delete"), false, () => {
                        provider.OnDeleteProvider(type);
                        EditorUtility.SetDirty(provider);
                    });
                    menu.ShowAsContext();
                }
            }
            EditorGUILayout.EndHorizontal();

            if (show) {
                EditorGUILayout.BeginVertical(GUI.skin.box);
                MetaData.DrawSourceField(type);

                IComponent componentValue = null;
                if (component is ComponentProvider cp) componentValue = cp.value;
                else if (component is LinkProvider lp) componentValue = lp.value;
                else if (component is LinksProvider lsp) componentValue = lsp.value;
                else if (component is MultiProvider mp) componentValue = mp.value;

                if (componentValue != null && !TryDrawSpecialComponent(componentValue, type, component, provider)) {
                    var wrapperKey = ((long) provider.GetInstanceID() << 32) ^ type.GetHashCode();
                    var wrapper = ComponentDrawerWrapper.GetFor(wrapperKey);
                    var so = new SerializedObject(wrapper);
                    var prop = so.FindProperty("value");
                    prop.managedReferenceValue = null;
                    so.ApplyModifiedPropertiesWithoutUndo();
                    prop.managedReferenceValue = componentValue;
                    so.ApplyModifiedPropertiesWithoutUndo();
                    so.Update();
                    prop = so.FindProperty("value");

                    if (prop != null && prop.propertyType == SerializedPropertyType.ManagedReference) {
                        DrawSerializedPropertyChildren(prop);

                        if (so.ApplyModifiedProperties()) {
                            var newProv = CreateProviderForComponent(type, wrapper.value);
                            provider.OnChangeProvider(newProv, type, deferred: mode != DrawMode.Inspector);
                            EditorUtility.SetDirty(provider);
                        }
                    }
                }

                EditorGUILayout.EndVertical();
            }
        }

        internal static void DrawSerializedPropertyChildren(SerializedProperty property) {
            if (property.propertyType == SerializedPropertyType.ManagedReference) {
                var refType = ResolveManagedReferenceType(property.managedReferenceFullTypename);
                if (refType != null && CustomPropertyDrawerRegistry.HasDrawerFor(refType)) {
                    EditorGUILayout.PropertyField(property, GUIContent.none, true);
                    return;
                }
            }

            var iterator = property.Copy();
            var startDepth = property.depth;
            if (!iterator.NextVisible(true)) return;

            while (iterator.depth > startDepth) {
                if (TryDrawEntityGIDInline(iterator)) {
                    if (!iterator.NextVisible(false)) break;
                    continue;
                }

                if (TryDrawWorldWrapperInline(iterator)) {
                    if (!iterator.NextVisible(false)) break;
                    continue;
                }

                try {
                    EditorGUILayout.PropertyField(iterator, true);
                } catch (System.ArgumentException) {
                    // Unity bug: stale managed reference index after value swap.
                    // Skip this property — next NextVisible(false) will move on.
                }
                if (!iterator.NextVisible(false)) break;
            }
        }

        // Unity's serialized property system cannot introspect value-type structs
        // nested in a generic outer class (World<TWorld>.Link<TLinkType> / .Links<...> / .Multi<...>).
        // PropertyField on such fields logs "seems to be referencing invalid or out of date data".
        // Resolve the actual boxed value via reflection and render directly.
        private static bool TryDrawWorldWrapperInline(SerializedProperty property) {
            var boxed = SerializedPropertyValueResolver.Resolve<object>(property);
            if (boxed == null) return false;

            var type = boxed.GetType();
            if (!IsWorldWrapperType(type, out var baseName)) return false;

            var label = ObjectNames.NicifyVariableName(property.name);
            var args = type.GetGenericArguments();
            EditorGUILayout.LabelField(label, $"{baseName}<{args[args.Length - 1].EditorTypeName()}>", EditorStyles.miniBoldLabel);
            EditorGUI.indentLevel++;
            switch (baseName) {
                case "Link":  DrawLinkRuntimeValue((IComponent) boxed, type); break;
                case "Links": DrawLinksRuntimeElements((IComponent) boxed, type); break;
                case "Multi": DrawMultiComponent((IComponent) boxed, type, property.propertyPath.GetHashCode()); break;
            }
            EditorGUI.indentLevel--;
            return true;
        }

        private static bool TryDrawEntityGIDInline(SerializedProperty property) {
            var typeName = property.type;
            if (typeName != "EntityGID" && typeName != "EntityGIDCompact") return false;

            var label = ObjectNames.NicifyVariableName(property.name);

            if (typeName == "EntityGID") {
                var gid = SerializedPropertyValueResolver.Resolve<EntityGID>(property);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel(label);
                var (text, actual, worldType) = ResolveEntityGIDDisplay(gid);
                EditorGUILayout.SelectableLabel(text, GUILayout.MaxHeight(EditorGUIUtility.singleLineHeight));
                if (actual) {
                    DrawInspectEntityButton(gid, worldType);
                }
                EditorGUILayout.EndHorizontal();
            } else {
                var gid = SerializedPropertyValueResolver.Resolve<EntityGIDCompact>(property);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel(label);
                var (text, actual, worldType) = ResolveEntityGIDDisplay(gid);
                EditorGUILayout.SelectableLabel(text, GUILayout.MaxHeight(EditorGUIUtility.singleLineHeight));
                if (actual) {
                    DrawInspectEntityButton(gid, worldType);
                }
                EditorGUILayout.EndHorizontal();
            }

            return true;
        }

        private static Type ResolveManagedReferenceType(string fullTypeName) {
            if (string.IsNullOrEmpty(fullTypeName)) return null;
            var sp = fullTypeName.IndexOf(' ');
            if (sp <= 0) return null;
            var asmName = fullTypeName.Substring(0, sp);
            var typeName = fullTypeName.Substring(sp + 1);
            try {
                var asm = Assembly.Load(asmName);
                return asm?.GetType(typeName);
            }
            catch {
                return null;
            }
        }

        // True when type is World<>.<baseName>`<arity>. If baseNameFilter is null, true for any wrapper.
        internal static bool IsWorldWrapperTypeMatch(Type type, string baseNameFilter) {
            if (!IsWorldWrapperType(type, out var baseName)) return false;
            return baseNameFilter == null || baseName == baseNameFilter;
        }

        private static bool IsWorldWrapperType(Type type, out string baseName) {
            baseName = null;
            if (!type.IsGenericType) return false;
            var dt = type.DeclaringType;
            if (dt == null || !dt.IsGenericType) return false;
            if (dt.GetGenericTypeDefinition().FullName != "FFS.Libraries.StaticEcs.World`1") return false;
            var n = type.Name;
            if (n.StartsWith("Link`")) {
                baseName = "Link";
                return true;
            }

            if (n.StartsWith("Links`")) {
                baseName = "Links";
                return true;
            }

            if (n.StartsWith("Multi`")) {
                baseName = "Multi";
                return true;
            }

            return false;
        }

        internal static IComponentOrTagProvider CreateProviderForComponent(Type type, IComponent component) {
            if (IsWorldWrapperType(type, out var baseName)) {
                switch (baseName) {
                    case "Link":  return new LinkProvider { value = (ILinkComponent) component };
                    case "Links": return new LinksProvider { value = (ILinksComponent) component };
                    case "Multi": return new MultiProvider { value = component };
                }
            }

            return new ComponentProvider { value = component };
        }

        private static bool TryDrawSpecialComponent<TWorld>(IComponent component, Type type, IComponentOrTagProvider prov, StaticEcsEntityProvider<TWorld> entityProvider)
            where TWorld : struct, IWorldType {
            if (!IsWorldWrapperType(type, out var baseName)) return false;

            var actual = entityProvider.EntityIsActual();

            if (baseName == "Link") {
                if (!actual) {
                    DrawAuthoringLinkTarget(type, prov, entityProvider);
                    return true;
                }
                DrawLinkRuntimeValue(component, type);
                return true;
            }

            if (baseName == "Links") {
                if (!actual) {
                    DrawAuthoringLinksTargets(type, prov, entityProvider);
                    return true;
                }
                DrawLinksRuntimeElements(component, type);
                return true;
            }

            if (baseName == "Multi") {
                DrawMultiComponent(component, type, entityProvider.GetInstanceID());
                return true;
            }

            return false;
        }

        private static bool ValidateLinkTarget(AbstractStaticEcsEntityProvider target) {
            if (target == null) return true;
            return target.gameObject.scene.IsValid();
        }

        private static TProvider FindProvider<TWorld, TProvider>(StaticEcsEntityProvider<TWorld> entityProvider, Type componentType)
            where TWorld : struct, IWorldType
            where TProvider : class, IComponentOrTagProvider {
            var providers = entityProvider.SerializedProviders;
            if (providers == null) return null;
            for (var i = 0; i < providers.Count; i++) {
                if (providers[i] is TProvider typed && typed.ComponentType == componentType) {
                    return typed;
                }
            }

            return null;
        }

        private static void DrawAuthoringLinkTarget<TWorld>(Type type, IComponentOrTagProvider prov, StaticEcsEntityProvider<TWorld> entityProvider) where TWorld : struct, IWorldType {
            var lp = prov as LinkProvider ?? FindProvider<TWorld, LinkProvider>(entityProvider, type);
            if (lp == null) return;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Target");
            EditorGUI.BeginChangeCheck();
            var newTarget = (AbstractStaticEcsEntityProvider) EditorGUILayout.ObjectField(
                lp.target, typeof(AbstractStaticEcsEntityProvider), true);
            if (EditorGUI.EndChangeCheck()) {
                if (ValidateLinkTarget(newTarget)) {
                    lp.target = newTarget;
                    EditorUtility.SetDirty(entityProvider);
                } else {
                    Debug.LogWarning("Link target must be a scene object, not a prefab.");
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        private static void DrawAuthoringLinksTargets<TWorld>(Type type, IComponentOrTagProvider prov, StaticEcsEntityProvider<TWorld> entityProvider) where TWorld : struct, IWorldType {
            var lsp = prov as LinksProvider ?? FindProvider<TWorld, LinksProvider>(entityProvider, type);
            if (lsp == null) return;
            lsp.targets ??= new();

            for (var i = 0; i < lsp.targets.Count; i++) {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel($"Target [{i}]");
                EditorGUI.BeginChangeCheck();
                var newTarget = (AbstractStaticEcsEntityProvider) EditorGUILayout.ObjectField(
                    lsp.targets[i], typeof(AbstractStaticEcsEntityProvider), true);
                if (EditorGUI.EndChangeCheck()) {
                    if (ValidateLinkTarget(newTarget)) {
                        lsp.targets[i] = newTarget;
                        EditorUtility.SetDirty(entityProvider);
                    } else {
                        Debug.LogWarning("Link target must be a scene object, not a prefab.");
                    }
                }

                if (GUILayout.Button("-", EditorStyles.miniButton, GUILayout.Width(20))) {
                    lsp.targets.RemoveAt(i);
                    EditorUtility.SetDirty(entityProvider);
                    i--;
                }

                EditorGUILayout.EndHorizontal();
            }

            if (GUILayout.Button("+ Target", EditorStyles.miniButton)) {
                lsp.targets.Add(null);
                EditorUtility.SetDirty(entityProvider);
            }
        }

        private static void DrawLinkRuntimeValue(IComponent component, Type type) {
            var valueProp = type.GetProperty("Value", BindingFlags.Public | BindingFlags.Instance);
            if (valueProp == null) return;
            var gid = (EntityGID) valueProp.GetValue(component);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Value");
            var (text, actual, worldType) = ResolveEntityGIDDisplay(gid);
            EditorGUILayout.SelectableLabel(text, GUILayout.MaxHeight(EditorGUIUtility.singleLineHeight));
            if (actual) {
                DrawInspectEntityButton(gid, worldType);
            }
            EditorGUILayout.EndHorizontal();
        }

        private static void DrawLinksRuntimeElements(IComponent component, Type type) {
            var lengthProp = type.GetProperty("Length", BindingFlags.Public | BindingFlags.Instance);
            if (lengthProp == null) return;
            var count = (ushort) lengthProp.GetValue(component);

            EditorGUILayout.LabelField("Count", count.ToString());

            var itemProp = type.GetProperty("Item", BindingFlags.Public | BindingFlags.Instance);
            if (itemProp == null) return;

            for (var i = 0; i < count; i++) {
                var link = itemProp.GetValue(component, new object[] { i });
                if (link == null) continue;

                var linkValueProp = link.GetType().GetProperty("Value", BindingFlags.Public | BindingFlags.Instance);
                if (linkValueProp == null) continue;
                var gid = (EntityGID) linkValueProp.GetValue(link);

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel($"[{i}]");
                var (text, actual, worldType) = ResolveEntityGIDDisplay(gid);
                EditorGUILayout.SelectableLabel(text, GUILayout.MaxHeight(EditorGUIUtility.singleLineHeight));
                if (actual) {
                    DrawInspectEntityButton(gid, worldType);
                }
                EditorGUILayout.EndHorizontal();
            }
        }

        private static void DrawMultiComponent(IComponent component, Type type, int ownerKey) {
            if (!Application.isPlaying) {
                EditorGUILayout.LabelField("Runtime only", EditorStyles.centeredGreyMiniLabel);
                return;
            }

            var lengthProp = type.GetProperty("Length", BindingFlags.Public | BindingFlags.Instance);
            if (lengthProp == null) return;
            var count = (ushort) lengthProp.GetValue(component);

            EditorGUILayout.LabelField("Count", count.ToString());

            var itemProp = type.GetProperty("Item", BindingFlags.Public | BindingFlags.Instance);
            if (itemProp == null) return;

            var typeKey = (long) ownerKey << 32 ^ type.GetHashCode();
            using (new EditorGUI.DisabledScope(true)) {
                for (var i = 0; i < count; i++) {
                    var element = itemProp.GetValue(component, new object[] { i });
                    if (element == null) continue;

                    EditorGUILayout.LabelField($"[{i}]", EditorStyles.miniBoldLabel);
                    var elementKey = typeKey ^ ((long) i * 1099511628211L);
                    DrawBoxedStructFields(element, elementKey);
                }
            }
        }

        private static void DrawBoxedStructFields(object boxedStruct, long key) {
            var wrapper = BoxedStructDrawerWrapper.GetFor(key);
            using var so = new SerializedObject(wrapper);
            var prop = so.FindProperty("value");
            if (prop == null) {
                EditorGUILayout.LabelField(boxedStruct?.ToString() ?? "null");
                return;
            }

            prop.managedReferenceValue = null;
            so.ApplyModifiedPropertiesWithoutUndo();
            prop.managedReferenceValue = boxedStruct;
            so.ApplyModifiedPropertiesWithoutUndo();
            so.Update();
            prop = so.FindProperty("value");

            if (prop == null || prop.propertyType != SerializedPropertyType.ManagedReference) {
                EditorGUILayout.LabelField(boxedStruct?.ToString() ?? "null");
                return;
            }

            EditorGUI.indentLevel++;
            DrawSerializedPropertyChildren(prop);
            EditorGUI.indentLevel--;
        }

        internal static (string text, bool actual, Type worldType) ResolveEntityGIDDisplay(EntityGIDCompact gid) {
            if (gid.Raw == 0) return ("Empty", false, null);
            return ResolveEntityGIDDisplay((EntityGID) gid);
        }

        internal static (string text, bool actual, Type worldType) ResolveEntityGIDDisplay(EntityGID gid) {
            if (gid.Raw == 0) return ("Empty", false, null);

            Type worldType = null;
            var actual = false;
            foreach (var kvp in StaticEcsDebugData.Worlds) {
                if (kvp.Value.Handle.GIDStatus(gid) == GIDStatus.Active) {
                    actual = true;
                    worldType = kvp.Key;
                    break;
                }
            }

            string text;
            if (actual && worldType != null) {
                var worldData = StaticEcsDebugData.Worlds[worldType];
                text = worldData.WindowNameFunction?.Invoke(gid) ?? gid.ToString();
            } else {
                text = gid.ToString();
                if (!actual) text += " (Not actual)";
            }

            return (text, actual, worldType);
        }

        internal static bool DrawInspectEntityButton(EntityGID gid, Type worldType) {
            if (!Application.isPlaying || worldType == null) return false;
            if (!EntityInspectorRegistry.ShowEntityHandlers.TryGetValue(worldType, out var handler)) return false;
            using (Ui.EnabledScope) {
                if (GUILayout.Button("\u2299", EditorStyles.miniButton, GUILayout.Width(20))) {
                    handler(gid);
                    return true;
                }
            }

            return false;
        }

        private static void DrawProvidersMenu<TWorld>(List<IComponentOrTagProvider> actualProviders, StaticEcsEntityProvider<TWorld> provider) where TWorld : struct, IWorldType {
            var worldMeta = MetaData.GetWorldMetaData(typeof(TWorld));
            var items = new List<SearchableDropdown.Item>(worldMeta.Components.Count + worldMeta.Tags.Count);

            foreach (var component in worldMeta.Components) {
                var has = false;
                foreach (var actual in actualProviders) {
                    if (actual != null && actual.ComponentType == component.Type) {
                        has = true;
                        break;
                    }
                }

                if (has) continue;

                var enabled = provider.ShouldShowProvider(component.Type, Application.isPlaying);
                items.Add(new SearchableDropdown.Item(component.FullName, component.Type, enabled));
            }

            foreach (var tag in worldMeta.Tags) {
                var has = false;
                foreach (var actual in actualProviders) {
                    if (actual != null && actual.ComponentType == tag.Type) {
                        has = true;
                        break;
                    }
                }

                if (has) continue;

                var enabled = provider.ShouldShowProvider(tag.Type, Application.isPlaying);
                items.Add(new SearchableDropdown.Item(tag.FullName, tag.Type, enabled));
            }

            SearchableDropdown.Show("Components & Tags", items, payload => {
                var t = (Type) payload;
                IComponentOrTagProvider prov;
                if (typeof(ITag).IsAssignableFrom(t)) {
                    prov = new TagProvider { value = (ITag) Activator.CreateInstance(t, true) };
                } else {
                    var objRaw = (IComponent) Activator.CreateInstance(t, true);
                    prov = CreateProviderForComponent(t, objRaw);
                }

                provider.OnSelectProvider(prov);
                EditorUtility.SetDirty(provider);
            });
        }

        private static ConfigKind? ResolveBrokenKind(IComponentOrTagProvider prov) {
            if (prov is ComponentProvider) return ConfigKind.Component;
            if (prov is TagProvider) return ConfigKind.Tag;
            if (prov is LinkProvider) return ConfigKind.Link;
            if (prov is LinksProvider) return ConfigKind.Links;
            if (prov is MultiProvider) return ConfigKind.Multi;
            return null;
        }

        private static void DrawPrefabAssetOpenPrompt(Object obj, string providerKind, int index) {
            EditorGUILayout.BeginVertical(GUI.skin.box);
            EditorGUILayout.HelpBox(
                $"Broken {providerKind} slot at index {index}.\n" +
                "Prefab has missing SerializeReference types. Unity restricts access to managed references from the Project view — " +
                "editing here would cause data loss. Open the prefab to fix.",
                MessageType.Warning);
            if (GUILayout.Button("Open Prefab", Ui.ButtonStyleTheme)) {
                var path = AssetDatabase.GetAssetPath(obj);
                var prefab = AssetDatabase.LoadMainAssetAtPath(path);
                if (prefab != null) AssetDatabase.OpenAsset(prefab);
                GUIUtility.ExitGUI();
            }

            EditorGUILayout.EndVertical();
        }

        private static string FormatMissingTypeName(ManagedReferenceMissingType m) {
            return $"{GuidTypeRegistry.PrettyMissingClassName(m.className)} (asm: {m.assemblyName})";
        }

        private static void DrawBrokenProviderSlot<TWorld>(
            List<IComponentOrTagProvider> componentsOrTags,
            int index,
            IComponentOrTagProvider componentOrTag,
            StaticEcsEntityProvider<TWorld> entityProvider,
            List<ManagedReferenceMissingType> missingPool
        ) where TWorld : struct, IWorldType {
            if (PrefabUtility.IsPartOfPrefabAsset(entityProvider)) {
                DrawPrefabAssetOpenPrompt(entityProvider, componentOrTag == null ? "provider" : componentOrTag.GetType().Name, index);
                return;
            }

            ManagedReferenceMissingType missing = default;
            var hasMissing = false;
            string label;

            long matchedId = 0;
            using (var so = new SerializedObject(entityProvider)) {
                var listProp = so.FindProperty("providers");
                if (listProp is { isArray: true } && index < listProp.arraySize) {
                    var elementProp = listProp.GetArrayElementAtIndex(index);
                    if (componentOrTag == null) {
                        if (elementProp.propertyType == SerializedPropertyType.ManagedReference
                            && missingPool.Find(type => type.referenceId == elementProp.managedReferenceId).referenceId != 0) {
                            hasMissing = true;
                            matchedId = elementProp.managedReferenceId;
                        }
                    } else {
                        var valueProp = elementProp.FindPropertyRelative("value");
                        if (valueProp is { propertyType: SerializedPropertyType.ManagedReference }
                            && missingPool.Find(type => type.referenceId == valueProp.managedReferenceId).referenceId != 0) {
                            hasMissing = true;
                            matchedId = valueProp.managedReferenceId;
                        }
                    }
                }
            }

            if (hasMissing) {
                for (var k = 0; k < missingPool.Count; k++) {
                    if (missingPool[k].referenceId == matchedId) {
                        missingPool.RemoveAt(k);
                        break;
                    }
                }
            } else if (missingPool.Count > 0) {
                missing = missingPool[0];
                missingPool.RemoveAt(0);
                hasMissing = true;
            }

            if (hasMissing) {
                label = $"Missing: {FormatMissingTypeName(missing)}";
            } else if (componentOrTag == null) {
                label = $"Broken slot at index {index}";
            } else {
                label = $"Broken {componentOrTag.GetType().Name} at index {index}";
            }

            var showKey = entityProvider.GetInstanceID() ^ (hasMissing ? missing.referenceId.GetHashCode() : index);
            var expanded = _brokenShowData.Contains(showKey);

            Type autoType = null;
            if (hasMissing) {
                var kind = ResolveBrokenKind(componentOrTag);
                if (kind.HasValue) {
                    GuidTypeRegistry.TryFindCurrentByMissingIdentity(missing.className, missing.namespaceName, missing.assemblyName, kind.Value, out autoType);
                }
            }

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                EditorGUILayout.BeginHorizontal();
                {
                    GUILayout.Label(EditorGUIUtility.IconContent("console.warnicon.sml"), GUILayout.Width(18), GUILayout.Height(20));
                    GUILayout.Label(new GUIContent(label, label), _brokenLabelStyle ??= new GUIStyle(EditorStyles.label) {
                        fontStyle = FontStyle.Bold,
                        clipping = TextClipping.Clip,
                        wordWrap = false,
                        alignment = TextAnchor.MiddleLeft,
                    }, GUILayout.Height(20), GUILayout.ExpandWidth(true), GUILayout.MinWidth(50));

                    using (Ui.EnabledScopeVal(GUI.enabled && !entityProvider.EntityIsActual())) {
                        if (EditorGUILayout.DropdownButton(new GUIContent("Replace…"), FocusType.Keyboard, EditorStyles.miniPullDown, GUILayout.Width(90), GUILayout.Height(20))) {
                            var brokenKind = ResolveBrokenKind(componentOrTag);
                            ShowBrokenReplaceMenu(componentsOrTags, index, componentOrTag, entityProvider, missing, brokenKind);
                        }
                    }

                    using (Ui.EnabledScopeVal(GUI.enabled && hasMissing)) {
                        if (GUILayout.Button(expanded ? "Hide" : "Show", EditorStyles.miniButton, GUILayout.Width(46), GUILayout.Height(20))) {
                            if (expanded) _brokenShowData.Remove(showKey);
                            else _brokenShowData.Add(showKey);
                        }
                    }

                    using (Ui.EnabledScopeVal(GUI.enabled && !entityProvider.EntityIsActual())) {
                        if (GUILayout.Button(new GUIContent(EditorGUIUtility.IconContent("TreeEditor.Trash").image, "Remove"), EditorStyles.iconButton, GUILayout.Width(22), GUILayout.Height(20))) {
                            using (var so = new SerializedObject(entityProvider)) {
                                var listProp = so.FindProperty("providers");
                                if (listProp != null && listProp.isArray && index < listProp.arraySize) {
                                    var before = listProp.arraySize;
                                    listProp.DeleteArrayElementAtIndex(index);
                                    if (listProp.arraySize == before) listProp.DeleteArrayElementAtIndex(index);
                                    so.ApplyModifiedProperties();
                                }
                            }

                            EditorUtility.SetDirty(entityProvider);
                            GUIUtility.ExitGUI();
                        }
                    }
                }
                EditorGUILayout.EndHorizontal();

                if (autoType != null) {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(22);
                    GUILayout.Label(new GUIContent($"Auto-match: {GuidTypeRegistry.PrettyTypeName(autoType)}", autoType.FullName), EditorStyles.miniLabel, GUILayout.Height(18), GUILayout.ExpandWidth(true));
                    using (Ui.EnabledScopeVal(GUI.enabled && !entityProvider.EntityIsActual())) {
                        if (GUILayout.Button("Apply", EditorStyles.miniButton, GUILayout.Width(60), GUILayout.Height(18))) {
                            MissingReferenceMigration.TryMigrateSlot(entityProvider, missing, autoType);
                            GUIUtility.ExitGUI();
                        }
                    }

                    EditorGUILayout.EndHorizontal();
                }

                if (expanded && hasMissing) {
                    var text = string.IsNullOrEmpty(missing.serializedData) ? "(empty)" : missing.serializedData;
                    EditorGUILayout.TextArea(text, EditorStyles.textArea);
                }
            }
            EditorGUILayout.EndVertical();
        }

        private static GUIStyle _brokenLabelStyle;

        private static void ShowBrokenReplaceMenu<TWorld>(
            List<IComponentOrTagProvider> providers, int index, IComponentOrTagProvider brokenProv,
            StaticEcsEntityProvider<TWorld> provider,
            ManagedReferenceMissingType missing,
            ConfigKind? brokenKind
        ) where TWorld : struct, IWorldType {
            var worldMeta = MetaData.GetWorldMetaData(typeof(TWorld));
            var items = new List<SearchableDropdown.Item>(worldMeta.Components.Count + worldMeta.Tags.Count);

            bool allowComponents, allowTags, allowMulti, allowLink, allowLinks;
            if (brokenKind.HasValue) {
                allowComponents = brokenKind.Value == ConfigKind.Component;
                allowTags = brokenKind.Value == ConfigKind.Tag;
                allowMulti = brokenKind.Value == ConfigKind.Multi;
                allowLink = brokenKind.Value == ConfigKind.Link;
                allowLinks = brokenKind.Value == ConfigKind.Links;
            } else {
                // Wrapper itself missing — allow anything.
                allowComponents = true;
                allowTags = true;
                allowMulti = true;
                allowLink = true;
                allowLinks = true;
            }

            foreach (var component in worldMeta.Components) {
                var has = false;
                foreach (var actual in providers) {
                    if (actual != null && actual.ComponentType == component.Type) {
                        has = true;
                        break;
                    }
                }
                if (has) continue;

                bool show;
                if (IsWorldWrapperType(component.Type, out var baseName)) {
                    show = baseName switch {
                        "Multi" => allowMulti,
                        "Link" => allowLink,
                        "Links" => allowLinks,
                        _ => false,
                    };
                } else {
                    show = allowComponents;
                }
                if (!show) continue;
                items.Add(new SearchableDropdown.Item(component.FullName, component.Type, true));
            }

            if (allowTags) {
                foreach (var tag in worldMeta.Tags) {
                    var has = false;
                    foreach (var actual in providers) {
                        if (actual != null && actual.ComponentType == tag.Type) {
                            has = true;
                            break;
                        }
                    }

                    if (has) continue;
                    items.Add(new SearchableDropdown.Item(tag.FullName, tag.Type, true));
                }
            }

            var capturedMissing = missing;
            SearchableDropdown.Show("Replace broken slot", items, payload => {
                var t = (Type) payload;
                if (MissingReferenceMigration.TryMigrateSlot(provider, capturedMissing, t)) {
                    return;
                }

                var newProv = typeof(ITag).IsAssignableFrom(t)
                    ? new TagProvider { value = (ITag) Activator.CreateInstance(t, true) }
                    : CreateProviderForComponent(t, (IComponent) Activator.CreateInstance(t, true));

                using (var so = new SerializedObject(provider)) {
                    var listProp = so.FindProperty("providers");
                    if (listProp != null && listProp.isArray && index < listProp.arraySize) {
                        var elem = listProp.GetArrayElementAtIndex(index);
                        elem.managedReferenceValue = newProv;
                        so.ApplyModifiedProperties();
                    }
                }

                EditorUtility.SetDirty(provider);
            });
        }

        private static void ApplyFoldoutConfig(int foldoutKey, Type componentType, Type worldType) {
            var config = StaticEcsViewConfig.Active;
            var worldSettings = config.GetOrCreate(worldType.FullName);
            switch (worldSettings.entities.foldoutMode) {
                case ComponentFoldoutMode.ExpandAll:
                    openHideFlags.Add(foldoutKey);
                    break;
                case ComponentFoldoutMode.CollapseAll:
                    break;
                case ComponentFoldoutMode.Custom:
                    if (worldSettings.entities.autoExpandComponentTypes.Contains(componentType.FullName)) {
                        openHideFlags.Add(foldoutKey);
                    }

                    break;
            }
        }

        private static void SortProvidersInPlace<TWorld>(StaticEcsEntityProvider<TWorld> provider) where TWorld : struct, IWorldType {
            var providers = provider.SerializedProviders;
            if (providers.Count < 2) return;
            var order = BuildProviderOrder(providers, true);
            var snapshot = new IComponentOrTagProvider[providers.Count];
            for (var i = 0; i < providers.Count; i++) snapshot[i] = providers[order[i]];
            Undo.RecordObject(provider, "Sort ECS Components");
            for (var i = 0; i < providers.Count; i++) providers[i] = snapshot[i];
            EditorUtility.SetDirty(provider);
        }

        private static List<int> BuildProviderOrder(List<IComponentOrTagProvider> providers, bool sorted) {
            var order = _sortOrderCache;
            order.Clear();
            for (var i = 0; i < providers.Count; i++) order.Add(i);

            order.Sort((a, b) => {
                var pa = providers[a];
                var pb = providers[b];
                if (pa?.ComponentType == null) return pb?.ComponentType == null ? a.CompareTo(b) : 1;
                if (pb?.ComponentType == null) return -1;

                var aSys = pa.ComponentType.IsSystemType();
                var bSys = pb.ComponentType.IsSystemType();
                if (aSys != bSys) return aSys ? 1 : -1;

                var aHasGroup = pa.ComponentType.EditorTypeGroup(out var aGroupName, out _, out _);
                var bHasGroup = pb.ComponentType.EditorTypeGroup(out var bGroupName, out _, out _);
                if (aHasGroup != bHasGroup) return aHasGroup ? -1 : 1;
                if (aHasGroup) {
                    var cn = string.Compare(aGroupName, bGroupName, StringComparison.Ordinal);
                    if (cn != 0) return cn;
                }

                if (!sorted) return a.CompareTo(b);

                var aHasColor = pa.ComponentType.EditorTypeColor(out var aColor);
                var bHasColor = pb.ComponentType.EditorTypeColor(out var bColor);
                if (aHasColor != bHasColor) return aHasColor ? -1 : 1;
                if (aHasColor) {
                    var cr = aColor.r.CompareTo(bColor.r);
                    if (cr != 0) return cr;
                    var cg = aColor.g.CompareTo(bColor.g);
                    if (cg != 0) return cg;
                    var cb = aColor.b.CompareTo(bColor.b);
                    if (cb != 0) return cb;
                }

                var aTag = pa.Kind.IsTag();
                var bTag = pb.Kind.IsTag();
                if (aTag != bTag) return aTag ? 1 : -1;
                return string.Compare(pa.ComponentType.EditorTypeName(), pb.ComponentType.EditorTypeName(), StringComparison.Ordinal);
            });
            return order;
        }
    }
}