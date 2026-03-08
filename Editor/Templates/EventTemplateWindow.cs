using System;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace FFS.Libraries.StaticEcs.Unity.Editor {

    public class EventTemplateWindow : EditorWindow {
        string[] names = {"Event"};
        string path;
        Vector2 scroll;

        string nameSpace;
        string worldName;
        string worldTypeName;
        Type worldType;

        bool ignoreInEditor = false;

        bool serialization = false;
        bool unmanaged = false;

        bool withColor = true;
        Color color = Color.white;

        [MenuItem("Assets/Create/Static ECS/Events", false, -228)]
        static void ShowWindow() {
            var window = GetWindow<EventTemplateWindow>(true, "Event template");
            window.minSize = new Vector2(300, 200);
            window.path = AssetPath();
            window.nameSpace = EditorSettings.projectGenerationRootNamespace.Trim();
            Drawer.openHideFlags.Add((typeof(string[]).FullName + "Events" + 0).GetHashCode());
        }

        void OnGUI() {
            if (worldType == null) {
                for (var i = 0; i < MetaData.WorldsMetaData.Count; i++) {
                    worldType = MetaData.WorldsMetaData[i].WorldTypeType;
                    worldName = MetaData.WorldsMetaData[i].EditorName;
                    worldTypeName = worldType.Name;
                    break;
                }
            }
            scroll = EditorGUILayout.BeginScrollView(scroll);


            EditorGUILayout.BeginHorizontal();
            {
                if (Ui.SettingButton) {
                    DrawWorldMenu();
                }
                EditorGUILayout.LabelField("World:", Ui.WidthLine(60));
                EditorGUILayout.LabelField(worldName, Ui.LabelStyleThemeBold);
                if (worldType != null && names.Length > 0 && GUILayout.Button("Create Events", Ui.ButtonStyleYellow)) {
                    CreateFiles();
                    Close();
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10);
            nameSpace = EditorGUILayout.TextField("Namespace", nameSpace);

            EditorGUILayout.Space(10);
            Drawer.DrawStringArray("Events", ref names);
            for (var i = 0; i < names.Length; i++) {
                ref var val = ref names[i];
                if (string.IsNullOrEmpty(val)) {
                    val = "Event";
                }
            }

            EditorGUILayout.Space(10);
            ignoreInEditor = EditorGUILayout.Toggle("Ignore in Editor view", ignoreInEditor);

            EditorGUILayout.Space(10);
            serialization = EditorGUILayout.Toggle("Serialization", serialization);
            if (serialization) {
                EditorGUI.indentLevel++;
                unmanaged = EditorGUILayout.Toggle("Unmanaged", unmanaged);
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space(10);
            withColor = EditorGUILayout.Toggle("Editor color", withColor);
            if (withColor) {
                EditorGUI.indentLevel++;
                color = EditorGUILayout.ColorField("Color", color);
                EditorGUI.indentLevel--;
            }

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndScrollView();
        }

        private void DrawWorldMenu() {
            var menu = new GenericMenu();
            for (var i = 0; i < MetaData.WorldsMetaData.Count; i++) {
                var i1 = i;
                menu.AddItem(
                    new GUIContent(MetaData.WorldsMetaData[i].EditorName),
                    false,
                    objType => {
                        worldType = MetaData.WorldsMetaData[i1].WorldTypeType;
                        worldName = MetaData.WorldsMetaData[i1].EditorName;
                        worldTypeName = worldType.Name;
                    },
                    MetaData.WorldsMetaData[i1].WorldTypeType);
            }

            menu.ShowAsContext();
        }

        public void CreateFiles() {
            foreach (var componentName in names) {
                var fileName = $"{path}/{componentName}.cs";
                var text = CreateTemplate(componentName);
                try {
                    File.WriteAllText(AssetDatabase.GenerateUniqueAssetPath(fileName), text);
                }
                catch (Exception ex) {
                    Debug.LogError(ex.Message);
                }
            }

            AssetDatabase.Refresh();
        }

        static string AssetPath() {
            var path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (!string.IsNullOrEmpty(path) && AssetDatabase.Contains(Selection.activeObject)) {
                if (!AssetDatabase.IsValidFolder(path)) {
                    path = Path.GetDirectoryName(path);
                }
            } else {
                path = "Assets";
            }

            return path;
        }

        public string CreateTemplate(string eventName) {
            var sb = new StringBuilder();
            var pad = string.IsNullOrEmpty(nameSpace) ? "" : "    ";
            sb.AppendLine("using System;");
            sb.AppendLine("using FFS.Libraries.StaticEcs;");
            sb.AppendLine("using FFS.Libraries.StaticPack;", serialization);
            sb.AppendLine("using FFS.Libraries.StaticEcs.Unity;", withColor || ignoreInEditor);
            sb.AppendLine($"#if ENABLE_IL2CPP");
            sb.AppendLine($"using Unity.IL2CPP.CompilerServices;");
            sb.AppendLine($"#endif");
            sb.AppendLine();
            sb.AppendLine($"namespace {nameSpace} {{", !string.IsNullOrEmpty(nameSpace));
            sb.AppendLine($"{pad}#if ENABLE_IL2CPP");
            sb.AppendLine($"{pad}[Il2CppSetOption(Option.NullChecks, false)]");
            sb.AppendLine($"{pad}[Il2CppSetOption(Option.ArrayBoundsChecks, false)]");
            sb.AppendLine($"{pad}#endif");
            sb.AppendLine($"{pad}[Serializable]");
            sb.AppendLine($"{pad}[StaticEcsIgnoreEvent]", ignoreInEditor);
            sb.AppendLine($"{pad}[StaticEcsEditorColor(" +
                $"{color.r.ToString("0.###", CultureInfo.InvariantCulture)}f, " +
                $"{color.g.ToString("0.###", CultureInfo.InvariantCulture)}f, " +
                $"{color.b.ToString("0.###", CultureInfo.InvariantCulture)}f)]", withColor);
            sb.AppendLine($"{pad}public struct {eventName} : IEvent {{");
            sb.AppendLine($"{pad}    // TODO Write your event fields");
            sb.AppendLine("");
            sb.AppendLine($"{pad}    [StaticEcsEditorTableValue] public string Debug => \"Not implemented\"; // TODO implement this", ignoreInEditor);
            sb.AppendLine("", ignoreInEditor);
            if (serialization) {
                sb.AppendLine($"{pad}    public static readonly EventTypeConfig<{eventName}> Config = new(");
                sb.AppendLine($"{pad}        guid: new(\"{GUID.Generate().ToString()}\")");
                sb.AppendLine($"{pad}    );");
                sb.AppendLine();
                sb.AppendLine($"{pad}    public void Write(ref BinaryPackWriter writer) {{");
                sb.AppendLine($"{pad}        throw new NotImplementedException(); // TODO implement this");
                sb.AppendLine($"{pad}    }}");
                sb.AppendLine();
                sb.AppendLine($"{pad}    public void Read(ref BinaryPackReader reader, byte version) {{");
                sb.AppendLine($"{pad}        throw new NotImplementedException(); // TODO implement this");
                sb.AppendLine($"{pad}    }}");
                sb.AppendLine();
            }
            sb.AppendLine($"{pad}}}");
            sb.AppendLine("}", !string.IsNullOrEmpty(nameSpace));

            return sb.ToString();
        }
    }
}
