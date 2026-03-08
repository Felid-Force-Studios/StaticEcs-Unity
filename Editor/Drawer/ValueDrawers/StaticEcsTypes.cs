using UnityEditor;
using UnityEngine;

namespace FFS.Libraries.StaticEcs.Unity.Editor {
    [CustomPropertyDrawer(typeof(EntityGID))]
    public class EntityGIDPropertyDrawer : PropertyDrawer {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            EditorGUI.BeginProperty(position, label, property);

            var rawProp = property.FindPropertyRelative("Raw");
            var raw = rawProp != null ? (ulong) rawProp.longValue : 0UL;

            var labelRect = new Rect(position.x, position.y, EditorGUIUtility.labelWidth, position.height);
            EditorGUI.PrefixLabel(labelRect, label);

            var valueRect = new Rect(position.x + EditorGUIUtility.labelWidth, position.y,
                position.width - EditorGUIUtility.labelWidth - (Application.isPlaying ? 24 : 0), position.height);

            var empty = raw == 0;
            string text;
            bool actual = false;

            if (empty) {
                text = "Empty";
            } else {
                var gid = new EntityGID(raw);
                text = gid.ToString();

                foreach (var kvp in StaticEcsDebugData.Worlds) {
                    if (kvp.Value.Handle.GIDStatus(gid) == GIDStatus.Active) {
                        actual = true;
                        break;
                    }
                }

                if (!actual) {
                    text += " (Not actual)";
                }
            }

            EditorGUI.LabelField(valueRect, text);

            if (Application.isPlaying && actual) {
                var buttonRect = new Rect(position.xMax - 20, position.y, 20, position.height);
                if (GUI.Button(buttonRect, "⊙", EditorStyles.miniButton)) {
                    var gid = new EntityGID(raw);
                    foreach (var kvp in StaticEcsDebugData.Worlds) {
                        if (kvp.Value.Handle.GIDStatus(gid) == GIDStatus.Active) {
                            if (EntityInspectorRegistry.ShowEntityHandlers.TryGetValue(kvp.Key, out var handler)) {
                                handler(gid);
                            }
                            break;
                        }
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

            var rawProp = property.FindPropertyRelative("Raw");
            var raw = rawProp != null ? (uint) rawProp.intValue : 0u;

            var labelRect = new Rect(position.x, position.y, EditorGUIUtility.labelWidth, position.height);
            EditorGUI.PrefixLabel(labelRect, label);

            var valueRect = new Rect(position.x + EditorGUIUtility.labelWidth, position.y,
                position.width - EditorGUIUtility.labelWidth, position.height);

            var empty = raw == 0;
            string text;

            if (empty) {
                text = "Empty";
            } else {
                var gid = new EntityGIDCompact(raw);
                text = gid.ToString();

                bool actual = false;
                foreach (var kvp in StaticEcsDebugData.Worlds) {
                    if (kvp.Value.Handle.GIDStatus(gid) == GIDStatus.Active) {
                        actual = true;
                        break;
                    }
                }

                if (!actual) {
                    text += " (Not actual)";
                }
            }

            EditorGUI.LabelField(valueRect, text);
            EditorGUI.EndProperty();
        }
    }
}
