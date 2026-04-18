using UnityEditor;
using UnityEngine;

namespace FFS.Libraries.StaticEcs.Unity.Editor {
    [CustomPropertyDrawer(typeof(EntityGID))]
    public class EntityGIDPropertyDrawer : PropertyDrawer {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            EditorGUI.BeginProperty(position, label, property);

            var rawProp = property.FindPropertyRelative("Raw");
            var raw = rawProp != null ? (ulong) rawProp.longValue : 0UL;
            var gid = raw == 0 ? default : new EntityGID(raw);

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
                if (GUI.Button(buttonRect, "\u2299", EditorStyles.miniButton)) {
                    EntityInspectorRegistry.ShowEntityHandlers[worldType](gid);
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
