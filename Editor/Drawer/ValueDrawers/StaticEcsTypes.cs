using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace FFS.Libraries.StaticEcs.Unity.Editor {
    [CustomPropertyDrawer(typeof(EntityGID))]
    public class EntityGIDPropertyDrawer : PropertyDrawer {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            EditorGUI.BeginProperty(position, label, property);

            var gid = SerializedPropertyValueResolver.Resolve<EntityGID>(property);
            var (text, actual, worldType) = Drawer.ResolveEntityGIDDisplay(gid);

            var labelRect = new Rect(position.x, position.y, EditorGUIUtility.labelWidth, position.height);
            EditorGUI.PrefixLabel(labelRect, label);

            var hasButton = Application.isPlaying && actual && worldType != null
                            && EntityInspectorRegistry.ShowEntityHandlers.ContainsKey(worldType);
            var valueRect = new Rect(position.x + EditorGUIUtility.labelWidth, position.y,
                position.width - EditorGUIUtility.labelWidth - (hasButton ? 24 : 0), position.height);

            EditorGUI.LabelField(valueRect, text);

            if (hasButton) {
                var buttonRect = new Rect(position.xMax - 20, position.y, 20, position.height);
                
                using (Ui.EnabledScope) {
                    if (GUI.Button(buttonRect, "⊙", EditorStyles.miniButton)) {
                        EntityInspectorRegistry.ShowEntityHandlers[worldType](gid);
                    }
                }
            }

            EditorGUI.EndProperty();
        }
    }

    [CustomPropertyDrawer(typeof(EntityGIDCompact))]
    public class EntityGIDCompactPropertyDrawer : PropertyDrawer {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            EditorGUI.BeginProperty(position, label, property);

            var gid = SerializedPropertyValueResolver.Resolve<EntityGIDCompact>(property);
            var (text, actual, worldType) = Drawer.ResolveEntityGIDDisplay(gid);

            var labelRect = new Rect(position.x, position.y, EditorGUIUtility.labelWidth, position.height);
            EditorGUI.PrefixLabel(labelRect, label);

            var hasButton = Application.isPlaying && actual && worldType != null
                            && EntityInspectorRegistry.ShowEntityHandlers.ContainsKey(worldType);
            var valueRect = new Rect(position.x + EditorGUIUtility.labelWidth, position.y,
                position.width - EditorGUIUtility.labelWidth - (hasButton ? 24 : 0), position.height);

            EditorGUI.LabelField(valueRect, text);

            if (hasButton) {
                var buttonRect = new Rect(position.xMax - 20, position.y, 20, position.height);

                using (Ui.EnabledScope) {
                    if (GUI.Button(buttonRect, "⊙", EditorStyles.miniButton)) {
                        EntityInspectorRegistry.ShowEntityHandlers[worldType](gid);
                    }
                }
            }

            EditorGUI.EndProperty();
        }
    }

    internal static class SerializedPropertyValueResolver {
        private const BindingFlags FieldFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        public static T Resolve<T>(SerializedProperty property) {
            var target = ResolveObject(property);
            return target is T typed ? typed : default;
        }

        private static object ResolveObject(SerializedProperty property) {
            if (property == null) return null;
            object current = property.serializedObject.targetObject;
            if (current == null) return null;

            var path = property.propertyPath.Replace(".Array.data[", "[");
            var tokens = path.Split('.');
            foreach (var token in tokens) {
                if (current == null) return null;
                var bracketIdx = token.IndexOf('[');
                if (bracketIdx >= 0) {
                    var fieldName = token.Substring(0, bracketIdx);
                    var indexStr = token.Substring(bracketIdx + 1, token.Length - bracketIdx - 2);
                    if (!int.TryParse(indexStr, out var index)) return null;
                    current = GetFieldValue(current, fieldName);
                    current = GetIndexed(current, index);
                } else {
                    current = GetFieldValue(current, token);
                }
            }

            return current;
        }

        private static object GetFieldValue(object source, string name) {
            if (source == null) return null;
            var type = source.GetType();
            while (type != null) {
                var field = type.GetField(name, FieldFlags);
                if (field != null) return field.GetValue(source);
                type = type.BaseType;
            }
            return null;
        }

        private static object GetIndexed(object source, int index) {
            if (source is System.Collections.IList list) {
                if (index < 0 || index >= list.Count) return null;
                return list[index];
            }
            if (source is System.Collections.IEnumerable enumerable) {
                var i = 0;
                foreach (var item in enumerable) {
                    if (i++ == index) return item;
                }
            }
            return null;
        }
    }
}
