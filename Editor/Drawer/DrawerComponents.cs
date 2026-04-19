using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace FFS.Libraries.StaticEcs.Unity.Editor {
    public static partial class Drawer {

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

        private static readonly List<int> _sortOrderCache = new();

        private static List<int> BuildProviderSortedOrder(List<IComponentOrTagProvider> providers) {
            var order = _sortOrderCache;
            order.Clear();
            for (var i = 0; i < providers.Count; i++) order.Add(i);
            order.Sort((a, b) => {
                var pa = providers[a];
                var pb = providers[b];
                if (pa?.ComponentType == null) return pb?.ComponentType == null ? 0 : 1;
                if (pb?.ComponentType == null) return -1;
                var aSys = pa.ComponentType.IsSystemType();
                var bSys = pb.ComponentType.IsSystemType();
                if (aSys != bSys) return aSys ? 1 : -1;
                var aHasColor = pa.ComponentType.EditorTypeColor(out var aColor);
                var bHasColor = pb.ComponentType.EditorTypeColor(out var bColor);
                if (aHasColor != bHasColor) return aHasColor ? -1 : 1;
                if (aHasColor) {
                    var cr = aColor.r.CompareTo(bColor.r); if (cr != 0) return cr;
                    var cg = aColor.g.CompareTo(bColor.g); if (cg != 0) return cg;
                    var cb = aColor.b.CompareTo(bColor.b); if (cb != 0) return cb;
                }
                var aTag = pa.Kind.IsTag();
                var bTag = pb.Kind.IsTag();
                if (aTag != bTag) return aTag ? 1 : -1;
                return string.Compare(pa.ComponentType.EditorTypeName(), pb.ComponentType.EditorTypeName(), StringComparison.Ordinal);
            });
            return order;
        }

        private static void SortSerializedProviders<TWorld>(StaticEcsEntityProvider<TWorld> provider, Object obj) where TWorld : struct, IWorldType {
            var list = provider.SerializedProviders;
            list.Sort((a, b) => {
                if (a?.ComponentType == null) return b?.ComponentType == null ? 0 : 1;
                if (b?.ComponentType == null) return -1;
                var aSys = a.ComponentType.IsSystemType();
                var bSys = b.ComponentType.IsSystemType();
                if (aSys != bSys) return aSys ? 1 : -1;
                var aHasColor = a.ComponentType.EditorTypeColor(out var aColor);
                var bHasColor = b.ComponentType.EditorTypeColor(out var bColor);
                if (aHasColor != bHasColor) return aHasColor ? -1 : 1;
                if (aHasColor) {
                    var cr = aColor.r.CompareTo(bColor.r); if (cr != 0) return cr;
                    var cg = aColor.g.CompareTo(bColor.g); if (cg != 0) return cg;
                    var cb = aColor.b.CompareTo(bColor.b); if (cb != 0) return cb;
                }
                var aTag = a.Kind.IsTag();
                var bTag = b.Kind.IsTag();
                if (aTag != bTag) return aTag ? 1 : -1;
                return string.Compare(a.ComponentType.EditorTypeName(), b.ComponentType.EditorTypeName(), StringComparison.Ordinal);
            });
            EditorUtility.SetDirty(obj);
        }

        private static void DrawProviders<TWorld>(List<IComponentOrTagProvider> providers, Object obj, StaticEcsEntityProvider<TWorld> provider, DrawMode mode) where TWorld : struct, IWorldType {
            var worldMeta = MetaData.GetWorldMetaData(typeof(TWorld));

            EditorGUILayout.BeginHorizontal();
            {
                var hasAll = worldMeta.Components.Count + worldMeta.Tags.Count == providers.Count;
                using (Ui.EnabledScopeVal(!hasAll && GUI.enabled)) {
                    if (Ui.PlusDropDownButton && !hasAll) {
                        DrawProvidersMenu(providers, obj, provider);
                    }
                }
                EditorGUILayout.LabelField(" Components:", Ui.HeaderStyleTheme);
                if (!provider.EntityIsActual() && providers.Count >= 2 && GUILayout.Button("Sort", Ui.ButtonStyleThemeMini, Ui.WidthLine(40))) {
                    SortSerializedProviders(provider, obj);
                }
            }
            EditorGUILayout.EndHorizontal();

            var sorted = provider.EntityIsActual();
            var order = sorted ? BuildProviderSortedOrder(providers) : null;
            for (var o = 0; o < providers.Count; o++) {
                var i = sorted ? order[o] : o;
                var prov = providers[i];

                if (prov == null || prov.ComponentType == null) {
                    EditorGUILayout.LabelField($"Broken provider - is null, index {i}", EditorStyles.boldLabel);
                    if (GUILayout.Button("Delete all broken providers", Ui.ButtonStyleTheme, Ui.ExpandWidthFalse())) {
                        provider.DeleteAllBrokenProviders();
                        EditorUtility.SetDirty(obj);
                    }

                    EditorGUILayout.Space(2);
                    continue;
                }

                var type = prov.ComponentType;
                var colored = type.EditorTypeColor(out var color);

                if (prov.Kind.IsTag()) {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.BeginHorizontal(GUI.skin.box);
                    {
                        var labelStyle = colored
                            ? new GUIStyle(EditorStyles.boldLabel) { normal = { textColor = color } }
                            : EditorStyles.boldLabel;
                        GUILayout.Label(type.EditorTypeName(), labelStyle, GUILayout.MaxHeight(EditorGUIUtility.singleLineHeight));
                        var labelRect = GUILayoutUtility.GetLastRect();
                        var evt = Event.current;
                        if (evt.type == EventType.MouseDown && evt.button == 0 && labelRect.Contains(evt.mousePosition)) {
                            var script = TypeSourceNavigator.FindScript(type);
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
                            EditorUtility.SetDirty(obj);
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                    EditorGUI.indentLevel--;
                    continue;
                }

                var typeName = type.EditorTypeName();
                var disabled = provider.IsDisabled(type);
                if (disabled) {
                    typeName += " [Disabled]";
                }

                bool show;
                var foldoutKey = HashCode.Combine(provider, type.FullName);
                if (!initializedFoldouts.Contains(foldoutKey)) {
                    initializedFoldouts.Add(foldoutKey);
                    ApplyFoldoutConfig(foldoutKey, type, typeof(TWorld));
                }

                EditorGUILayout.BeginHorizontal(GUI.skin.box);
                {
                    if (colored) {
                        DrawFoldoutBox(foldoutKey, typeName, typeName, out show, color);
                    } else {
                        DrawFoldoutBox(foldoutKey, typeName, typeName, out show);
                    }
                    if (Ui.MenuButton) {
                        var menu = new GenericMenu();
                        if (provider.EntityIsActual()) {
                            if (disabled) {
                                menu.AddItem(new GUIContent("Enable"), false, () => {
                                    provider.Enable(type);
                                    EditorUtility.SetDirty(obj);
                                });
                            } else {
                                menu.AddItem(new GUIContent("Disable"), false, () => {
                                    provider.Disable(type);
                                    EditorUtility.SetDirty(obj);
                                });
                            }
                        }
                        menu.AddItem(new GUIContent("Delete"), false, () => {
                            provider.OnDeleteProvider(type);
                            EditorUtility.SetDirty(obj);
                        });
                        menu.ShowAsContext();
                    }
                }
                EditorGUILayout.EndHorizontal();

                if (show) {
                    EditorGUILayout.BeginVertical(GUI.skin.box);
                    TypeSourceNavigator.DrawScriptField(type);

                    IComponent componentValue = null;
                    if (prov is ComponentProvider cp) componentValue = cp.value;
                    else if (prov is LinkProvider lp) componentValue = (IComponent)lp.value;
                    else if (prov is LinksProvider lsp) componentValue = (IComponent)lsp.value;
                    else if (prov is MultiProvider mp) componentValue = mp.value;

                    if (componentValue != null && !TryDrawSpecialComponent(componentValue, type, prov, provider)) {
                        var wrapper = ComponentDrawerWrapper.GetFor(obj.GetInstanceID());
                        var so = new SerializedObject(wrapper);
                        var prop = so.FindProperty("value");
                        prop.managedReferenceValue = componentValue;
                        so.ApplyModifiedProperties();
                        so.Update();
                        prop = so.FindProperty("value");

                        if (prop != null && prop.propertyType == SerializedPropertyType.ManagedReference) {
                            DrawSerializedPropertyChildren(prop);

                            if (so.ApplyModifiedProperties()) {
                                var newProv = CreateProviderForComponent(type, wrapper.value);
                                provider.OnChangeProvider(newProv, type, deferred: mode != DrawMode.Inspector);
                                EditorUtility.SetDirty(obj);
                            }
                        }
                    }

                    EditorGUILayout.EndVertical();
                }
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
            var end = property.GetEndProperty();
            if (!iterator.NextVisible(true)) return;

            while (!SerializedProperty.EqualContents(iterator, end)) {
                EditorGUILayout.PropertyField(iterator, true);
                if (!iterator.NextVisible(false)) break;
            }
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
            } catch {
                return null;
            }
        }

        private static bool IsWorldWrapperType(Type type, out string baseName) {
            baseName = null;
            if (!type.IsGenericType) return false;
            var dt = type.DeclaringType;
            if (dt == null || !dt.IsGenericType) return false;
            if (dt.GetGenericTypeDefinition().FullName != "FFS.Libraries.StaticEcs.World`1") return false;
            var n = type.Name;
            if (n.StartsWith("Link`")) { baseName = "Link"; return true; }
            if (n.StartsWith("Links`")) { baseName = "Links"; return true; }
            if (n.StartsWith("Multi`")) { baseName = "Multi"; return true; }
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

        private static bool TryDrawSpecialComponent<TWorld>(IComponent component, Type type, IComponentOrTagProvider prov, StaticEcsEntityProvider<TWorld> entityProvider) where TWorld : struct, IWorldType {
            if (!IsWorldWrapperType(type, out var baseName)) return false;

            if (baseName == "Link") {
                DrawLinkComponent(component, type, prov, entityProvider);
            } else if (baseName == "Links") {
                DrawLinksComponent(component, type, prov, entityProvider);
            } else if (baseName == "Multi") {
                DrawMultiComponent(component, type);
            }

            return true;
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

        private static void DrawLinkComponent<TWorld>(IComponent component, Type type, IComponentOrTagProvider prov, StaticEcsEntityProvider<TWorld> entityProvider) where TWorld : struct, IWorldType {
            if (!entityProvider.EntityIsActual()) {
                var lp = prov as LinkProvider ?? FindProvider<TWorld, LinkProvider>(entityProvider, type);
                if (lp != null) {
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
                return;
            }

            var valueProp = type.GetProperty("Value", BindingFlags.Public | BindingFlags.Instance);
            if (valueProp == null) return;
            var gid = (EntityGID) valueProp.GetValue(component);
            DrawEntityGIDField("Value", gid);
        }

        private static void DrawLinksComponent<TWorld>(IComponent component, Type type, IComponentOrTagProvider prov, StaticEcsEntityProvider<TWorld> entityProvider) where TWorld : struct, IWorldType {
            if (!entityProvider.EntityIsActual()) {
                var lsp = prov as LinksProvider ?? FindProvider<TWorld, LinksProvider>(entityProvider, type);
                if (lsp != null) {
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
                return;
            }

            var lengthProp = type.GetProperty("Length", BindingFlags.Public | BindingFlags.Instance);
            if (lengthProp == null) return;
            var count = (ushort) lengthProp.GetValue(component);

            EditorGUILayout.LabelField("Count", count.ToString());

            var itemProp = type.GetProperty("Item", BindingFlags.Public | BindingFlags.Instance);
            if (itemProp == null) return;

            for (var i = 0; i < count; i++) {
                var link = itemProp.GetValue(component, new object[] { i });
                var linkType = link.GetType();
                var linkValueProp = linkType.GetProperty("Value", BindingFlags.Public | BindingFlags.Instance);
                if (linkValueProp == null) continue;
                var gid = (EntityGID) linkValueProp.GetValue(link);
                DrawEntityGIDField($"[{i}]", gid);
            }
        }

        private static void DrawMultiComponent(IComponent component, Type type) {
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

            var elementType = itemProp.PropertyType;
            for (var i = 0; i < count; i++) {
                var element = itemProp.GetValue(component, new object[] { i });
                var level = 5;
                TryDrawObject(ref level, $"[{i}]", elementType, element, out _);
            }
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
            if (GUILayout.Button("\u2299", EditorStyles.miniButton, GUILayout.Width(20))) {
                handler(gid);
                return true;
            }
            return false;
        }

        private static void DrawEntityGIDField(string label, EntityGID gid) {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(label);

            var (text, actual, worldType) = ResolveEntityGIDDisplay(gid);
            EditorGUILayout.SelectableLabel(text, GUILayout.MaxHeight(EditorGUIUtility.singleLineHeight));

            if (actual) {
                DrawInspectEntityButton(gid, worldType);
            }

            EditorGUILayout.EndHorizontal();
        }

        private static void DrawProvidersMenu<TWorld>(List<IComponentOrTagProvider> actualProviders, Object obj, StaticEcsEntityProvider<TWorld> provider) where TWorld : struct, IWorldType {
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
                EditorUtility.SetDirty(obj);
            });
        }
    }
}
