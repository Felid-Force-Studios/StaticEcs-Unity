using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace FFS.Libraries.StaticEcs.Unity.Editor {

    public static class MetaData {
        internal static readonly List<EditorEntityDataMeta> Components = new();
        internal static readonly List<EditorEntityDataMeta> Tags = new();
        internal static readonly List<EditorEventDataMeta> Events = new();
        internal static readonly List<EditorEntityTypeMeta> EntityTypes = new();

        internal static readonly List<(Type WorldTypeType, string EditorName)> WorldsMetaData = new();

        private static readonly Dictionary<byte, string> _entityTypeNames = new();
        
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
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies()) {
                foreach (var type in assembly.GetTypes()) {
                    if (type.IsValueType && type.GetInterfaces().Contains(typeof(IComponent)) && !type.IsGenericType) {
                        if (HandleComponentMeta(type)) {
                            continue;
                        }
                    }

                    if (type.IsValueType && type.GetInterfaces().Contains(typeof(ITag)) && !type.IsGenericType) {
                        if (HandleTagMeta(type)) {
                            continue;
                        }
                    }

                    if (type.IsValueType && type.GetInterfaces().Contains(typeof(IWorldType))) {
                        var name = type.FullName!;
                        
                        var (n, fn) = NameAttribute(type);
                        name = n ?? name;
                        
                        if (WorldsMetaData.Find(tuple => tuple.WorldTypeType == type).WorldTypeType != null) {
                            Debug.LogError($"World id `{name}` already registered, type `{type}` ignored");
                            continue;
                        }

                        WorldsMetaData.Add((type, name));
                    }
                    
                    if (type.IsValueType && type.GetInterfaces().Contains(typeof(IEvent)) && !type.IsGenericType) {
                        HandleEventMeta(type);
                    }

                    if (type.IsValueType && type.GetInterfaces().Contains(typeof(IEntityType)) && !type.IsGenericType) {
                        HandleEntityTypeMeta(type);
                    }
                }
            }

            EntityTypes.Sort((a, b) => a.Id.CompareTo(b.Id));
        }

        private static (string name, string fullName) NameAttribute(Type type) {
            foreach (var atr in type.GetCustomAttributesData()) {
                if (atr.AttributeType.Namespace + atr.AttributeType.FullName == nameAttr.Namespace + nameAttr.FullName) {
                    return (atr.ConstructorArguments[0].Value as string, atr.ConstructorArguments[1].Value as string);
                }
            }

            return (null, null);
        }

        private static void HandleEventMeta(Type type) {
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

            if (Events.Find(meta => meta.FullName == fullName) != null) {
                Debug.LogError($"Event `{fullName}` already registered, type `{type}` ignored");
                return;
            }

            var (field, property, width) = FindValueAttribute(type);
            width = Math.Max(GUI.skin.label.CalcSize(new GUIContent(name)).x, width);

            var item = new EditorEventDataMeta(type, name, fullName, width, new[] { GUILayout.Width(width), GUILayout.MaxHeight(EditorGUIUtility.singleLineHeight) },
                                               new[] { GUILayout.Width(width + 70f), GUILayout.MaxHeight(EditorGUIUtility.singleLineHeight) }, field, property);
            Events.Add(item);
        }

        private static bool HandleTagMeta(Type type) {
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

            if (Tags.Find(meta => meta.FullName == fullName) != null) {
                Debug.LogError($"Tag `{fullName}` already registered, type `{type}` ignored");
                return true;
            }

            var width = GUI.skin.label.CalcSize(new GUIContent(name)).x;
            Tags.Add(new EditorEntityDataMeta(type, name, fullName, width, new[] { GUILayout.Width(width), GUILayout.MaxHeight(EditorGUIUtility.singleLineHeight) },
                                              new[] { GUILayout.Width(width + 46f), GUILayout.MaxHeight(EditorGUIUtility.singleLineHeight) }, null, null));
            return false;
        }

        private static bool HandleComponentMeta(Type type) {
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

            if (Components.Find(meta => meta.FullName == fullName) != null) {
                Debug.LogError($"Component `{fullName}` already registered, type `{type}` ignored");
                return true;
            }

            var (field, property, width) = FindValueAttribute(type);
            width = Math.Max(GUI.skin.label.CalcSize(new GUIContent(name)).x, width);

            Drawer.openHideFlags.Add(type.FullName!.GetHashCode());
            Components.Add(new EditorEntityDataMeta(type, name, fullName, width, new[] { GUILayout.Width(width), GUILayout.MaxHeight(EditorGUIUtility.singleLineHeight) },
                                                    new[] { GUILayout.Width(width + 68f), GUILayout.MaxHeight(EditorGUIUtility.singleLineHeight) }, field, property));
            return false;
        }

        private static void HandleEntityTypeMeta(Type type) {
            var (n, _) = NameAttribute(type);
            var name = n ?? type.EditorTypeName();

            var idField = type.GetField("Id", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
            if (idField == null || idField.FieldType != typeof(byte)) return;

            var id = (byte) idField.GetValue(null);

            if (EntityTypes.Find(meta => meta.Id == id) != null) {
                Debug.LogError($"EntityType with id `{id}` already registered, type `{type}` ignored");
                return;
            }

            EntityTypes.Add(new EditorEntityTypeMeta(type, name, id));
            _entityTypeNames[id] = name;
        }

        internal static string GetEntityTypeName(byte id) {
            return _entityTypeNames.TryGetValue(id, out var name) ? name : $"Unknown({id})";
        }

        public static void EnrichByWorld(WorldHandle handle) {
            foreach (var compHandle in handle.GetAllComponentsHandles()) {
                if (compHandle.IsTag) continue;
                var type = compHandle.ComponentType;
                if (Components.Find(meta => meta.Type == type) == null) {
                    HandleComponentMeta(type);
                }
            }

            foreach (var tagHandle in handle.GetAllComponentsHandles()) {
                if (!tagHandle.IsTag) continue;
                var type = tagHandle.ComponentType;
                if (Tags.Find(meta => meta.Type == type) == null) {
                    HandleTagMeta(type);
                }
            }

            foreach (var eventsHandle in handle.GetAllEventsHandles()) {
                var type = eventsHandle.EventType;
                if (Events.Find(meta => meta.Type == type) == null) {
                    HandleEventMeta(type);
                }
            }
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