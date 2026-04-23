using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using Object = UnityEngine.Object;

namespace FFS.Libraries.StaticEcs.Unity.Editor {

    public static class MetaData {
        internal static readonly List<(Type WorldTypeType, string EditorName)> WorldsMetaData = new();
        internal static readonly Dictionary<Type, WorldMetaData> PerWorldMetaData = new();

        internal static readonly Type UnityObjectType = typeof(Object);
        internal static readonly Type EnumFlagsType = typeof(FlagsAttribute);

        private static readonly Type nameAttr = typeof(StaticEcsEditorNameAttribute);
        private static readonly Type valueAttr = typeof(StaticEcsEditorTableValueAttribute);
        private static readonly Dictionary<Type, FieldInfo[]> _typesCacheWithNonPublic = new();
        private static readonly Dictionary<Type, FieldInfo[]> _typesCache = new();

        internal static readonly Dictionary<Type, MonoScript> SourceCache = new();
        private static readonly Dictionary<string, Type> _sourceLookup = new();
        private static bool _sourceCacheReady;
        private static Task<List<(string path, Type type)>> _sourceCacheTask;
        private static readonly Regex _sourceNsRegex = new(@"(?m)^\s*namespace\s+([\w.]+)\s*[;{]", RegexOptions.Compiled);
        private static readonly Regex _sourceDeclRegex = new(@"\b(?:struct|class|interface|record)\s+(\w+)", RegexOptions.Compiled);

        [InitializeOnLoadMethod]
        private static void Init() {
            var componentTypes = new List<Type>();
            var tagTypes = new List<Type>();
            var eventTypes = new List<Type>();
            var entityTypes = new List<EditorEntityTypeMeta>();
            var linkTypes = new List<Type>();
            var linksTypes = new List<Type>();
            var multiTypes = new List<Type>();
            var worldTypes = new List<Type>();

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies()) {
                Type[] assemblyTypes;
                try { assemblyTypes = assembly.GetTypes(); }
                catch (ReflectionTypeLoadException e) { assemblyTypes = e.Types; }
                catch { continue; }

                foreach (var type in assemblyTypes) {
                    if (type == null) continue;
                    RegisterSourceLookup(type);
                    if (!type.IsValueType) continue;
                    var interfaces = type.GetInterfaces();

                    if (interfaces.Contains(typeof(IWorldType))) {
                        var name = type.FullName!;
                        var (n, _) = NameAttribute(type);
                        name = n ?? name;

                        if (WorldsMetaData.Find(tuple => tuple.WorldTypeType == type).WorldTypeType != null) {
                            Debug.LogError($"World id `{name}` already registered, type `{type}` ignored");
                            continue;
                        }

                        WorldsMetaData.Add((type, name));
                        worldTypes.Add(type);
                    }

                    if (!type.IsGenericType) {
                        if (interfaces.Contains(typeof(IComponent))) {
                            componentTypes.Add(type);
                        }

                        if (interfaces.Contains(typeof(ITag))) {
                            tagTypes.Add(type);
                        }

                        if (interfaces.Contains(typeof(IEvent))) {
                            eventTypes.Add(type);
                        }

                        if (interfaces.Contains(typeof(IEntityType))) {
                            var meta = CreateEntityTypeMeta(type);
                            if (meta != null) {
                                entityTypes.Add(meta);
                            }
                        }

                        if (interfaces.Contains(typeof(ILinksType))) {
                            linksTypes.Add(type);
                        } else if (interfaces.Contains(typeof(ILinkType))) {
                            linkTypes.Add(type);
                        }

                        if (interfaces.Contains(typeof(IMultiComponent))) {
                            multiTypes.Add(type);
                        }
                    }
                }
            }

            entityTypes.Sort((a, b) => a.Id.CompareTo(b.Id));

            var reg = StaticEcsTypeGuidRegistry.Active;
            var changed = false;

            foreach (var worldType in worldTypes) {
                var data = new WorldMetaData(worldType);
                var ws = reg.GetOrCreate(worldType);

                foreach (var type in componentTypes) {
                    HandleComponentMeta(data, type);
                    changed |= RegisterGuid(ConfigKind.Component, worldType, type, ws);
                }

                foreach (var type in tagTypes) {
                    HandleTagMeta(data, type);
                    changed |= RegisterGuid(ConfigKind.Tag, worldType, type, ws);
                }

                foreach (var type in eventTypes) {
                    HandleEventMeta(data, type);
                    changed |= RegisterGuid(ConfigKind.Event, worldType, type, ws);
                }

                data.EntityTypes.AddRange(entityTypes);

                var worldOpenType = typeof(World<>);

                foreach (var linkType in linkTypes) {
                    var linkComponentType = worldOpenType.GetNestedType("Link`1").MakeGenericType(worldType, linkType);
                    HandleComponentMeta(data, linkComponentType);
                    changed |= RegisterGuid(ConfigKind.Link, worldType, linkType, ws);
                }

                foreach (var linksType in linksTypes) {
                    var linksComponentType = worldOpenType.GetNestedType("Links`1").MakeGenericType(worldType, linksType);
                    HandleComponentMeta(data, linksComponentType);
                    changed |= RegisterGuid(ConfigKind.Links, worldType, linksType, ws);
                }

                foreach (var multiType in multiTypes) {
                    var multiComponentType = worldOpenType.GetNestedType("Multi`1").MakeGenericType(worldType, multiType);
                    HandleComponentMeta(data, multiComponentType);
                    changed |= RegisterGuid(ConfigKind.Multi, worldType, multiType, ws);
                }

                PerWorldMetaData[worldType] = data;
            }

            if (changed) {
                reg.Save();
            }
        }
        
        private static void RegisterSourceLookup(Type type) {
            if (type.IsGenericTypeDefinition) return;
            if (!(typeof(IComponentOrTag).IsAssignableFrom(type)
                  || typeof(IEvent).IsAssignableFrom(type)
                  || typeof(IResource).IsAssignableFrom(type)
                  || typeof(ISystem).IsAssignableFrom(type)
                  || typeof(ILinkType).IsAssignableFrom(type)
                  || typeof(ILinksType).IsAssignableFrom(type)
                  || typeof(IMultiComponent).IsAssignableFrom(type))) {
                return;
            }
            var name = type.Name;
            var tickIdx = name.IndexOf('`');
            if (tickIdx > 0) name = name.Substring(0, tickIdx);
            _sourceLookup[(type.Namespace ?? string.Empty) + "|" + name] = type;
        }

        internal static void DrawSourceField(Type type) {
            if (!_sourceCacheReady) {
                EnsureSourceCacheStarted();
                using (new EditorGUI.DisabledScope(true)) {
                    EditorGUILayout.TextField("Source", "Loading…");
                }
                return;
            }
            var script = GetSourceScript(type);
            using (new EditorGUI.DisabledScope(true)) {
                EditorGUILayout.ObjectField("Source", script, typeof(MonoScript), false);
            }
        }

        internal static MonoScript GetSourceScript(Type type) {
            type = UnwrapSourceTarget(type);
            if (type == null) return null;
            SourceCache.TryGetValue(type, out var script);
            return script;
        }

        private static Type UnwrapSourceTarget(Type type) {
            if (type == null || !type.IsGenericType) return type;
            foreach (var arg in type.GetGenericArguments()) {
                if (typeof(ILinkType).IsAssignableFrom(arg)
                    || typeof(ILinksType).IsAssignableFrom(arg)
                    || typeof(IMultiComponent).IsAssignableFrom(arg)) {
                    return arg;
                }
            }
            return type;
        }

        private static void EnsureSourceCacheStarted() {
            if (_sourceCacheReady || _sourceCacheTask != null) return;
            if (_sourceLookup.Count == 0) {
                _sourceCacheReady = true;
                return;
            }

            var guids = AssetDatabase.FindAssets("t:MonoScript");
            var paths = new List<string>(guids.Length);
            foreach (var g in guids) {
                var p = AssetDatabase.GUIDToAssetPath(g);
                if (p.EndsWith(".cs", StringComparison.Ordinal)) paths.Add(p);
            }

            _sourceCacheTask = Task.Run(() => ParseSources(paths));
            EditorApplication.update += PollSourceCacheTask;
        }

        private static List<(string path, Type type)> ParseSources(List<string> paths) {
            var result = new List<(string, Type)>();
            foreach (var path in paths) {
                string text;
                try { text = File.ReadAllText(path); }
                catch { continue; }

                var declMatches = _sourceDeclRegex.Matches(text);
                if (declMatches.Count == 0) continue;

                var nsMatch = _sourceNsRegex.Match(text);
                var nsKey = (nsMatch.Success ? nsMatch.Groups[1].Value : string.Empty) + "|";

                foreach (Match m in declMatches) {
                    if (_sourceLookup.TryGetValue(nsKey + m.Groups[1].Value, out var t)) {
                        result.Add((path, t));
                    }
                }
            }
            return result;
        }

        private static void PollSourceCacheTask() {
            var task = _sourceCacheTask;
            if (task == null || !task.IsCompleted) return;
            EditorApplication.update -= PollSourceCacheTask;
            if (task.Status == TaskStatus.RanToCompletion) {
                foreach (var (path, type) in task.Result) {
                    if (SourceCache.ContainsKey(type)) continue;
                    var script = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
                    if (script != null) SourceCache[type] = script;
                }
            }
            _sourceCacheReady = true;
            _sourceCacheTask = null;
            InternalEditorUtility.RepaintAllViews();
        }

        internal static bool RegisterGuid(ConfigKind kind, Type worldType, Type type, StaticEcsTypeGuidRegistry.WorldSettings ws) {
            var configOpenType = kind switch {
                ConfigKind.Component => typeof(IComponentConfig<>),
                ConfigKind.Tag => typeof(ITagConfig<>),
                ConfigKind.Event => typeof(IEventConfig<>),
                ConfigKind.Link => typeof(ILinkConfig<>),
                ConfigKind.Links => typeof(ILinksConfig<>),
                ConfigKind.Multi => typeof(IMultiComponentConfig<>),
                _ => typeof(IComponentConfig<>),
            };

            var configType = configOpenType.MakeGenericType(type);

            Guid? guid = null;
            if (kind is ConfigKind.Link or ConfigKind.Links or ConfigKind.Multi) {
                try {
                    var config = configType.GetMethod("Config")?.MakeGenericMethod(worldType).Invoke(Activator.CreateInstance(type), null);
                    if (config != null) {
                        var guidField = config.GetType().GetField("Guid", BindingFlags.Public | BindingFlags.Instance);
                        guid = (Guid?)guidField!.GetValue(config);
                    }
                }
                catch (Exception) {
                    // ignored
                }
            }
            else {
                try {
                    var config = configType.GetMethod("Config")?.Invoke(Activator.CreateInstance(type), null);
                    if (config != null) {
                        var guidField = config.GetType().GetField("Guid", BindingFlags.Public | BindingFlags.Instance);
                        guid = (Guid?)guidField!.GetValue(config);
                    }
                }
                catch (Exception) {
                    // ignored
                }
            }

            return guid.HasValue && ws.SyncEntry(kind, guid.Value, new StaticEcsTypeGuidRegistry.TypeIdentity {
                className = type.Name,
                namespaceName = type.Namespace ?? "",
                assembly = type.Assembly.GetName().Name
            });
        }

        internal static WorldMetaData GetWorldMetaData(Type worldType) {
            PerWorldMetaData.TryGetValue(worldType, out var data);
            return data;
        }

        public static void EnrichByWorld(WorldHandle handle) {
            var worldType = handle.WorldType;
            if (!PerWorldMetaData.TryGetValue(worldType, out var data)) {
                data = new WorldMetaData(worldType);
                PerWorldMetaData[worldType] = data;
            }

            if (data.Enriched) return;

            data.Components.RemoveAll(meta => !handle.TryGetComponentsHandle(meta.Type, out _));
            data.Tags.RemoveAll(meta => !handle.TryGetComponentsHandle(meta.Type, out _));
            data.Events.RemoveAll(meta => !handle.TryGetEventsHandle(meta.Type, out _));
            data.EntityTypes.RemoveAll(meta => !handle.IsEntityTypeRegistered(meta.Id));

            foreach (var compHandle in handle.GetAllComponentsHandles()) {
                var type = compHandle.ComponentType;
                if (compHandle.IsTag) {
                    if (data.Tags.Find(meta => meta.Type == type) == null) {
                        HandleTagMeta(data, type);
                    }
                } else {
                    if (data.Components.Find(meta => meta.Type == type) == null) {
                        HandleComponentMeta(data, type);
                    }
                }
            }

            foreach (var eventsHandle in handle.GetAllEventsHandles()) {
                var type = eventsHandle.EventType;
                if (data.Events.Find(meta => meta.Type == type) == null) {
                    HandleEventMeta(data, type);
                }
            }

            data.Enriched = true;
        }

        private static (string name, string fullName) NameAttribute(Type type) {
            foreach (var atr in type.GetCustomAttributesData()) {
                if (atr.AttributeType.Namespace + atr.AttributeType.FullName == nameAttr.Namespace + nameAttr.FullName) {
                    return (atr.ConstructorArguments[0].Value as string, atr.ConstructorArguments[1].Value as string);
                }
            }

            return (null, null);
        }

        private static void HandleComponentMeta(WorldMetaData data, Type type) {
            var fullName = "";
            var name = "";
            var (n, fn) = NameAttribute(type);
            fullName = fn ?? fullName;
            name = n ?? name;

            if (string.IsNullOrEmpty(fullName)) {
                fullName = type.EditorFullTypeName();
            }

            if (string.IsNullOrEmpty(name)) {
                name = type.EditorTypeName();
            }

            if (data.Components.Find(meta => meta.FullName == fullName) != null) {
                return;
            }

            var (field, property, width) = FindValueAttribute(type);

            data.Components.Add(new EditorEntityDataMeta(type, name, fullName, width, 68f, field, property));
        }

        private static void HandleTagMeta(WorldMetaData data, Type type) {
            var fullName = "";
            var name = "";
            var (n, fn) = NameAttribute(type);
            fullName = fn ?? fullName;
            name = n ?? name;

            if (string.IsNullOrEmpty(fullName)) {
                fullName = type.EditorFullTypeName();
            }

            if (string.IsNullOrEmpty(name)) {
                name = type.EditorTypeName();
            }

            if (data.Tags.Find(meta => meta.FullName == fullName) != null) {
                return;
            }

            data.Tags.Add(new EditorEntityDataMeta(type, name, fullName, -1f, 46f, null, null));
        }

        private static void HandleEventMeta(WorldMetaData data, Type type) {
            var fullName = "";
            var name = "";

            var (n, fn) = NameAttribute(type);
            fullName = fn ?? fullName;
            name = n ?? name;

            if (string.IsNullOrEmpty(fullName)) {
                fullName = type.EditorFullTypeName();
            }

            if (string.IsNullOrEmpty(name)) {
                name = type.EditorTypeName();
            }

            if (data.Events.Find(meta => meta.FullName == fullName) != null) {
                return;
            }

            var (field, property, width) = FindValueAttribute(type);

            data.Events.Add(new EditorEventDataMeta(type, name, fullName, width, 70f, field, property));
        }

        private static EditorEntityTypeMeta CreateEntityTypeMeta(Type type) {
            var (n, _) = NameAttribute(type);
            var name = n ?? type.EditorTypeName();
            var id = (byte) type.GetMethod("Id")!.Invoke(Activator.CreateInstance(type), null);
            return new EditorEntityTypeMeta(type, name, id);
        }

        private static (FieldInfo fieldInfo, PropertyInfo propertyInfo, float width) FindValueAttribute(Type type) {
            foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)) {
                foreach (var customAttribute in field.GetCustomAttributesData()) {
                    if (customAttribute.AttributeType.Namespace + customAttribute.AttributeType.FullName == valueAttr.Namespace + valueAttr.FullName) {
                        foreach (var constructorArgument in customAttribute.ConstructorArguments) {
                            if (constructorArgument.ArgumentType == typeof(float)) {
                                return (field, null, (float)constructorArgument.Value);
                            }
                        }

                        return (field, null, -1);
                    }
                }
            }

            foreach (var field in type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)) {
                foreach (var customAttribute in field.GetCustomAttributesData()) {
                    if (customAttribute.AttributeType.Namespace + customAttribute.AttributeType.FullName == valueAttr.Namespace + valueAttr.FullName) {
                        foreach (var constructorArgument in customAttribute.ConstructorArguments) {
                            if (constructorArgument.ArgumentType == typeof(float)) {
                                return (null, field, (float)constructorArgument.Value);
                            }
                        }

                        return (null, field, -1);
                    }
                }
            }

            return (null, null, -1);
        }

        internal static FieldInfo[] GetCachedTypeWithNonPublic(Type type) {
            if (!_typesCacheWithNonPublic.TryGetValue(type, out var fields)) {
                fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                _typesCacheWithNonPublic[type] = fields;
            }

            return fields;
        }

        internal static FieldInfo[] GetCachedSerializableType(Type type) {
            if (!_typesCache.TryGetValue(type, out var fields)) {
                if (!Attribute.IsDefined(type, typeof(SerializableAttribute)) && !typeof(IComponent).IsAssignableFrom(type) && !typeof(IEvent).IsAssignableFrom(type) && !typeof(ISystem).IsAssignableFrom(type) && !HasShowAttribute(type)) {
                    fields = Array.Empty<FieldInfo>();
                    _typesCache[type] = fields;
                    return fields;
                }

                var publicFields = type.GetFields(BindingFlags.Instance | BindingFlags.Public)
                                       .Where(f => !HasHideAttribute(f));

                var nonPublicSerializable = type
                                            .GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
                                            .Where(HasShowAttribute);

                fields = publicFields.Concat(nonPublicSerializable).ToArray();
                _typesCache[type] = fields;
            }

            return fields;
        }

        private static bool HasShowAttribute(FieldInfo field) {
            var showType = typeof(StaticEcsEditorShowAttribute);
            foreach (var customAttribute in field.GetCustomAttributesData()) {
                if (customAttribute.AttributeType.Namespace + customAttribute.AttributeType.FullName == showType.Namespace + showType.FullName) {
                    return true;
                }
            }

            return false;
        }

        private static bool HasShowAttribute(Type type) {
            var showType = typeof(StaticEcsEditorShowAttribute);
            foreach (var customAttribute in type.GetCustomAttributesData()) {
                if (customAttribute.AttributeType.Namespace + customAttribute.AttributeType.FullName == showType.Namespace + showType.FullName) {
                    return true;
                }
            }

            return false;
        }

        private static bool HasHideAttribute(FieldInfo field) {
            var hideType = typeof(StaticEcsEditorHideAttribute);
            foreach (var customAttribute in field.GetCustomAttributesData()) {
                if (customAttribute.AttributeType.Namespace + customAttribute.AttributeType.FullName == hideType.Namespace + hideType.FullName) {
                    return true;
                }
            }

            return false;
        }
    }

    public class WorldMetaData {
        public readonly Type WorldType;
        public readonly List<EditorEntityDataMeta> Components = new();
        public readonly List<EditorEntityDataMeta> Tags = new();
        public readonly List<EditorEventDataMeta> Events = new();
        public readonly List<EditorEntityTypeMeta> EntityTypes = new();
        public bool Enriched;

        public WorldMetaData(Type worldType) {
            WorldType = worldType;
        }

        public string GetEntityTypeName(byte id) {
            var meta = EntityTypes.Find(m => m.Id == id);
            return meta != null ? meta.Name : $"Unknown({id})";
        }
    }

    public class EditorEventDataMeta {
        public readonly Type Type;
        public readonly string Name;
        public readonly string FullName;
        public readonly FieldInfo FieldInfo;
        public readonly PropertyInfo PropertyInfo;
        private readonly float _extraWidth;
        private readonly float _offsetDelta;
        private float _width = -1f;
        private GUILayoutOption[] _layout;
        private GUILayoutOption[] _layoutWithOffset;

        public float Width {
            get {
                if (_width < 0f) _width = Math.Max(GUI.skin.label.CalcSize(new GUIContent(Name)).x, _extraWidth);
                return _width;
            }
        }

        public GUILayoutOption[] Layout => _layout ??= new[] { GUILayout.Width(Width), GUILayout.MaxHeight(EditorGUIUtility.singleLineHeight) };
        public GUILayoutOption[] LayoutWithOffset => _layoutWithOffset ??= new[] { GUILayout.Width(Width + _offsetDelta), GUILayout.MaxHeight(EditorGUIUtility.singleLineHeight) };

        public EditorEventDataMeta(Type type, string name, string fullName, float extraWidth, float offsetDelta, FieldInfo fieldInfo, PropertyInfo propertyInfo) {
            Type = type;
            Name = name;
            FullName = fullName;
            _extraWidth = extraWidth;
            _offsetDelta = offsetDelta;
            FieldInfo = fieldInfo;
            PropertyInfo = propertyInfo;
        }

        protected EditorEventDataMeta(EditorEventDataMeta other) {
            Type = other.Type;
            Name = other.Name;
            FullName = other.FullName;
            _extraWidth = other._extraWidth;
            _offsetDelta = other._offsetDelta;
            FieldInfo = other.FieldInfo;
            PropertyInfo = other.PropertyInfo;
        }

        public bool TryGetTableField(out FieldInfo field) {
            field = FieldInfo;
            return field != null;
        }

        public bool TryGetTableProperty(out PropertyInfo field) {
            field = PropertyInfo;
            return field != null;
        }
    }

    public class EditorEntityDataMeta {
        public readonly Type Type;
        public readonly string Name;
        public readonly string FullName;
        public readonly FieldInfo FieldInfo;
        public readonly PropertyInfo PropertyInfo;
        private readonly float _extraWidth;
        private readonly float _offsetDelta;
        private float _width = -1f;
        private GUILayoutOption[] _layout;
        private GUILayoutOption[] _layoutWithOffset;

        public float Width {
            get {
                if (_width < 0f) _width = Math.Max(GUI.skin.label.CalcSize(new GUIContent(Name)).x, _extraWidth);
                return _width;
            }
        }

        public GUILayoutOption[] Layout => _layout ??= new[] { GUILayout.Width(Width), GUILayout.MaxHeight(EditorGUIUtility.singleLineHeight) };
        public GUILayoutOption[] LayoutWithOffset => _layoutWithOffset ??= new[] { GUILayout.Width(Width + _offsetDelta), GUILayout.MaxHeight(EditorGUIUtility.singleLineHeight) };

        public EditorEntityDataMeta(Type type, string name, string fullName, float extraWidth, float offsetDelta, FieldInfo fieldInfo, PropertyInfo propertyInfo) {
            Type = type;
            Name = name;
            FullName = fullName;
            _extraWidth = extraWidth;
            _offsetDelta = offsetDelta;
            FieldInfo = fieldInfo;
            PropertyInfo = propertyInfo;
        }

        protected EditorEntityDataMeta(EditorEntityDataMeta other) {
            Type = other.Type;
            Name = other.Name;
            FullName = other.FullName;
            _extraWidth = other._extraWidth;
            _offsetDelta = other._offsetDelta;
            FieldInfo = other.FieldInfo;
            PropertyInfo = other.PropertyInfo;
        }

        public bool TryGetTableField(out FieldInfo field) {
            field = FieldInfo;
            return field != null;
        }

        public bool TryGetTableProperty(out PropertyInfo field) {
            field = PropertyInfo;
            return field != null;
        }
    }

    public class EditorEntityTypeMeta {
        public readonly Type Type;
        public readonly string Name;
        public readonly byte Id;

        public EditorEntityTypeMeta(Type type, string name, byte id) {
            Type = type;
            Name = name;
            Id = id;
        }
    }
}
