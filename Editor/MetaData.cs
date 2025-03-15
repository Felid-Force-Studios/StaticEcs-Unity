using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace FFS.Libraries.StaticEcs.Unity.Editor {

    public static class MetaData {
        internal static readonly List<EditorEntityDataMeta> StandardComponents = new();
        internal static readonly List<EditorEntityDataMeta> Components = new();
        internal static readonly List<EditorEntityDataMeta> Tags = new();
        internal static readonly List<EditorEntityDataMeta> Masks = new();
        internal static readonly List<EditorEventDataMeta> Events = new();

        internal static readonly List<(Type WorldTypeType, string EditorName)> WorldsMetaData = new();
        
        internal static readonly Dictionary<Type, IStaticEcsValueDrawer> Inspectors = new();
        
        internal static readonly Type UnityObjectType = typeof(Object);
        internal static readonly Type EnumFlagsType = typeof(FlagsAttribute);
        
        private static readonly Type nameAttr = typeof(StaticEcsEditorNameAttribute);
        private static readonly Type valueAttr = typeof(StaticEcsEditorTableValueAttribute);
        private static readonly Dictionary<Type, FieldInfo[]> _typesCache = new();

        static MetaData() {
            Init();
        }

        private static void Init() {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies()) {
                foreach (var type in assembly.GetTypes()) {
                    if (!type.IsInterface && !type.IsAbstract && typeof(IStaticEcsValueDrawer).IsAssignableFrom(type)) {
                        var ins = (IStaticEcsValueDrawer) Activator.CreateInstance(type);
                        var cType = ins.ItemType();
                        if (!Inspectors.TryGetValue(cType, out var prevIns) || ins.Priority() > prevIns.Priority()) {
                            Inspectors[cType] = ins;
                        }
                        continue;
                    }

                    if (type.IsValueType && type.GetInterfaces().Contains(typeof(IStandardComponent)) && type != typeof(EntityVersion)) {
                        var fullName = "";
                        var name = "";
                        if (Attribute.IsDefined(type, nameAttr)) {
                            fullName = ((StaticEcsEditorNameAttribute) Attribute.GetCustomAttribute(type, nameAttr)).FullName;
                            name = ((StaticEcsEditorNameAttribute) Attribute.GetCustomAttribute(type, nameAttr)).Name;
                        }

                        if (string.IsNullOrEmpty(fullName)) {
                            fullName = type.FullName!.Replace('.', '/').Replace('+', '/');
                        }

                        if (string.IsNullOrEmpty(name)) {
                            name = type.EditorTypeName();
                        }

                        if (StandardComponents.Find(meta => meta.FullName == fullName) != null) {
                            Debug.LogError($"StandardComponent `{fullName}` already registered, type `{type}` ignored");
                            continue;
                        }
                        
                        var (field, property, width) = FindValueAttribute(type);
                        width = Math.Max(GUI.skin.label.CalcSize(new GUIContent(name)).x, width);

                        StandardComponents.Add(new EditorEntityDataMeta(type, name, fullName, width, new[] { GUILayout.Width(width), GUILayout.MaxHeight(EditorGUIUtility.singleLineHeight) }, new[] { GUILayout.Width(width + 70f), GUILayout.MaxHeight(EditorGUIUtility.singleLineHeight) }, field, property));
                    }

                    if (type.IsValueType && type.GetInterfaces().Contains(typeof(IComponent))) {
                        var fullName = "";
                        var name = "";
                        if (Attribute.IsDefined(type, nameAttr)) {
                            fullName = ((StaticEcsEditorNameAttribute) Attribute.GetCustomAttribute(type, nameAttr)).FullName;
                            name = ((StaticEcsEditorNameAttribute) Attribute.GetCustomAttribute(type, nameAttr)).Name;
                        }

                        if (string.IsNullOrEmpty(fullName)) {
                            fullName = type.FullName!.Replace('.', '/').Replace('+', '/');
                        }

                        if (string.IsNullOrEmpty(name)) {
                            name = type.EditorTypeName();
                        }

                        if (Components.Find(meta => meta.FullName == fullName) != null) {
                            Debug.LogError($"Component `{fullName}` already registered, type `{type}` ignored");
                            continue;
                        }
                        
                        var (field, property, width) = FindValueAttribute(type);
                        width = Math.Max(GUI.skin.label.CalcSize(new GUIContent(name)).x, width);

                        Components.Add(new EditorEntityDataMeta(type, name, fullName, width, new[] { GUILayout.Width(width), GUILayout.MaxHeight(EditorGUIUtility.singleLineHeight) }, new[] { GUILayout.Width(width + 70f), GUILayout.MaxHeight(EditorGUIUtility.singleLineHeight) }, field, property));
                    }

                    if (type.IsValueType && type.GetInterfaces().Contains(typeof(ITag))) {
                        var fullName = "";
                        var name = "";
                        if (Attribute.IsDefined(type, nameAttr)) {
                            fullName = ((StaticEcsEditorNameAttribute) Attribute.GetCustomAttribute(type, nameAttr)).FullName;
                            name = ((StaticEcsEditorNameAttribute) Attribute.GetCustomAttribute(type, nameAttr)).Name;
                        }

                        if (string.IsNullOrEmpty(fullName)) {
                            fullName = type.FullName!.Replace('.', '/').Replace('+', '/');
                        }
                        
                        if (string.IsNullOrEmpty(name)) {
                            name = type.EditorTypeName();
                        }

                        if (Tags.Find(meta => meta.FullName == fullName) != null) {
                            Debug.LogError($"Tag `{fullName}` already registered, type `{type}` ignored");
                            continue;
                        }

                        var width = GUI.skin.label.CalcSize(new GUIContent(name)).x;
                        Tags.Add(new EditorEntityDataMeta(type, name, fullName, width, new[] { GUILayout.Width(width), GUILayout.MaxHeight(EditorGUIUtility.singleLineHeight) }, new[] { GUILayout.Width(width + 46f), GUILayout.MaxHeight(EditorGUIUtility.singleLineHeight) }, null, null));
                    }

                    if (type.IsValueType && type.GetInterfaces().Contains(typeof(IMask))) {
                        var fullName = "";
                        var name = "";
                        
                        if (Attribute.IsDefined(type, nameAttr)) {
                            fullName = ((StaticEcsEditorNameAttribute) Attribute.GetCustomAttribute(type, nameAttr)).FullName;
                            name = ((StaticEcsEditorNameAttribute) Attribute.GetCustomAttribute(type, nameAttr)).Name;
                        }

                        if (string.IsNullOrEmpty(fullName)) {
                            fullName = type.FullName!.Replace('.', '/').Replace('+', '/');
                        }
                        
                        if (string.IsNullOrEmpty(name)) {
                            name = type.EditorTypeName();
                        }

                        if (Masks.Find(meta => meta.FullName == fullName) != null) {
                            Debug.LogError($"Mask `{fullName}` already registered, type `{type}` ignored");
                            continue;
                        }

                        var width = GUI.skin.label.CalcSize(new GUIContent(name)).x;
                        Masks.Add(new EditorEntityDataMeta(type, name, fullName, width, new[] { GUILayout.Width(width), GUILayout.MaxHeight(EditorGUIUtility.singleLineHeight) }, new[] { GUILayout.Width(width + 46f), GUILayout.MaxHeight(EditorGUIUtility.singleLineHeight) }, null, null));
                    }

                    if (type.IsValueType && type.GetInterfaces().Contains(typeof(IWorldType))) {
                        var name = type.FullName!;
                        
                        if (Attribute.IsDefined(type, nameAttr)) {
                            name = ((StaticEcsEditorNameAttribute) Attribute.GetCustomAttribute(type, nameAttr)).Name;
                        }
                        
                        if (WorldsMetaData.Find(tuple => tuple.WorldTypeType == type).WorldTypeType != null) {
                            Debug.LogError($"World id `{name}` already registered, type `{type}` ignored");
                            continue;
                        }

                        WorldsMetaData.Add((type, name));
                    }
                    
                    if (type.IsValueType && type.GetInterfaces().Contains(typeof(IEvent))) {
                        var fullName = "";
                        var name = "";
                        if (Attribute.IsDefined(type, nameAttr)) {
                            fullName = ((StaticEcsEditorNameAttribute) Attribute.GetCustomAttribute(type, nameAttr)).FullName;
                            name = ((StaticEcsEditorNameAttribute) Attribute.GetCustomAttribute(type, nameAttr)).Name;
                        }

                        if (string.IsNullOrEmpty(fullName)) {
                            fullName = type.FullName!.Replace('.', '/').Replace('+', '/');
                        }

                        if (string.IsNullOrEmpty(name)) {
                            name = type.EditorTypeName();
                        }

                        if (Events.Find(meta => meta.FullName == fullName) != null) {
                            Debug.LogError($"Event `{fullName}` already registered, type `{type}` ignored");
                            continue;
                        }

                        var (field, property, width) = FindValueAttribute(type);
                        width = Math.Max(GUI.skin.label.CalcSize(new GUIContent(name)).x, width);

                        var item = new EditorEventDataMeta(type, name, fullName, width, new[] { GUILayout.Width(width), GUILayout.MaxHeight(EditorGUIUtility.singleLineHeight) }, new[] { GUILayout.Width(width + 70f), GUILayout.MaxHeight(EditorGUIUtility.singleLineHeight) }, field, property);
                        Events.Add(item);
                    }
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

        internal static FieldInfo[] GetCachedType(Type type) {
            if (!_typesCache.TryGetValue(type, out var fields)) {
                fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                _typesCache[type] = fields;
            }

            return fields;
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

    public interface IStaticEcsValueDrawer {
        Type ItemType();
        int Priority();
        bool DrawValue(string label, object value, out object newValue);
        void DrawTableValue(object value, GUIStyle style, GUILayoutOption[] layoutOptions);
    }

    public abstract class IStaticEcsValueDrawer<T> : IStaticEcsValueDrawer {
        public Type ItemType() => typeof(T);
        public virtual bool IsNullAllowed() => false;
        public virtual int Priority() => 0;

        public bool DrawValue(string label, object value, out object newValue) {
            if (value == null && !IsNullAllowed()) {
                Drawer.DrawSelectableText(label, "null");
                newValue = default;
                return false;
            }

            var typedValue = (T) value;
            if (DrawValue(label, ref typedValue)) {
                newValue = typedValue;
                return true;
            }

            newValue = default;
            return false;
        }
        
        public void DrawTableValue(object value, GUIStyle style, GUILayoutOption[] layoutOptions) {
            if (value == null && !IsNullAllowed()) {
                EditorGUILayout.SelectableLabel("null", style, layoutOptions);
                return;
            }

            var typedValue = (T) value;
            DrawTableValue(ref typedValue, style, layoutOptions);
        }

        public abstract void DrawTableValue(ref T value, GUIStyle style, GUILayoutOption[] layoutOptions);

        public abstract bool DrawValue(string label, ref T value);
    }
}