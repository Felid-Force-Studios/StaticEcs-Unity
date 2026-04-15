using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
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

        static MetaData() {
            Init();
        }

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
                foreach (var type in assembly.GetTypes()) {
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

            foreach (var worldType in worldTypes) {
                var data = new WorldMetaData(worldType);

                foreach (var type in componentTypes) {
                    HandleComponentMeta(data, type);
                }

                foreach (var type in tagTypes) {
                    HandleTagMeta(data, type);
                }

                foreach (var type in eventTypes) {
                    HandleEventMeta(data, type);
                }

                data.EntityTypes.AddRange(entityTypes);

                var worldOpenType = typeof(World<>);

                foreach (var linkType in linkTypes) {
                    var linkComponentType = worldOpenType.GetNestedType("Link`1").MakeGenericType(worldType, linkType);
                    HandleComponentMeta(data, linkComponentType);
                }

                foreach (var linksType in linksTypes) {
                    var linksComponentType = worldOpenType.GetNestedType("Links`1").MakeGenericType(worldType, linksType);
                    HandleComponentMeta(data, linksComponentType);
                }

                foreach (var multiType in multiTypes) {
                    var multiComponentType = worldOpenType.GetNestedType("Multi`1").MakeGenericType(worldType, multiType);
                    HandleComponentMeta(data, multiComponentType);
                }

                PerWorldMetaData[worldType] = data;
            }
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
            width = Math.Max(GUI.skin.label.CalcSize(new GUIContent(name)).x, width);

            data.Components.Add(new EditorEntityDataMeta(type, name, fullName, width, new[] { GUILayout.Width(width), GUILayout.MaxHeight(EditorGUIUtility.singleLineHeight) },
                                                          new[] { GUILayout.Width(width + 68f), GUILayout.MaxHeight(EditorGUIUtility.singleLineHeight) }, field, property));
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

            var width = GUI.skin.label.CalcSize(new GUIContent(name)).x;
            data.Tags.Add(new EditorEntityDataMeta(type, name, fullName, width, new[] { GUILayout.Width(width), GUILayout.MaxHeight(EditorGUIUtility.singleLineHeight) },
                                                    new[] { GUILayout.Width(width + 46f), GUILayout.MaxHeight(EditorGUIUtility.singleLineHeight) }, null, null));
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
            width = Math.Max(GUI.skin.label.CalcSize(new GUIContent(name)).x, width);

            data.Events.Add(new EditorEventDataMeta(type, name, fullName, width, new[] { GUILayout.Width(width), GUILayout.MaxHeight(EditorGUIUtility.singleLineHeight) },
                                                     new[] { GUILayout.Width(width + 70f), GUILayout.MaxHeight(EditorGUIUtility.singleLineHeight) }, field, property));
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
        public readonly float Width;
        public readonly GUILayoutOption[] Layout;
        public readonly GUILayoutOption[] LayoutWithOffset;
        public readonly FieldInfo FieldInfo;
        public readonly PropertyInfo PropertyInfo;

        public EditorEventDataMeta(Type type, string name, string fullName, float width, GUILayoutOption[] layout, GUILayoutOption[] layoutWithOffset, FieldInfo fieldInfo, PropertyInfo propertyInfo) {
            Type = type;
            Name = name;
            FullName = fullName;
            Width = width;
            Layout = layout;
            LayoutWithOffset = layoutWithOffset;
            FieldInfo = fieldInfo;
            PropertyInfo = propertyInfo;
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
        public readonly float Width;
        public readonly GUILayoutOption[] Layout;
        public readonly GUILayoutOption[] LayoutWithOffset;
        public readonly FieldInfo FieldInfo;
        public readonly PropertyInfo PropertyInfo;

        public EditorEntityDataMeta(Type type, string name, string fullName, float width, GUILayoutOption[] layout, GUILayoutOption[] layoutWithOffset, FieldInfo fieldInfo, PropertyInfo propertyInfo) {
            Type = type;
            Name = name;
            FullName = fullName;
            Width = width;
            Layout = layout;
            LayoutWithOffset = layoutWithOffset;
            FieldInfo = fieldInfo;
            PropertyInfo = propertyInfo;
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
