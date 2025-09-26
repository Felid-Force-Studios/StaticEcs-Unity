using System;
using System.Globalization;
using System.IO;
using System.Text;
using FFS.Libraries.StaticEcs.Unity.Editor.Inspectors;
using UnityEditor;
using UnityEngine;

namespace FFS.Libraries.StaticEcs.Unity.Editor {
    
    public class EventTemplateWindow : EditorWindow {
        ArrayDrawer<string> namesDrawer = new();
        string[] names = {"Event"};
        string path;
        Vector2 scroll;
        
        string nameSpace;
        string worldName;
        string worldTypeName;
        Type worldType;
        
        bool autoRegister = true;
        bool ignoreInEditor = false;
        
        bool withConfig = true;
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
            Drawer.openHideFlags.Add(typeof(string[]).FullName + "Events" + 0);
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
            var context = new DrawContext();
            namesDrawer.DrawValue(ref context, "Events", ref names);
            for (var i = 0; i < names.Length; i++) {
                ref var val = ref names[i];
                if (string.IsNullOrEmpty(val)) {
                    val = "Event";
                }
            }
            
            EditorGUILayout.Space(10);
            autoRegister = EditorGUILayout.Toggle("Auto registration", autoRegister);
            
            EditorGUILayout.Space(10);
            ignoreInEditor = EditorGUILayout.Toggle("Ignore in Editor view", ignoreInEditor);
            
            EditorGUILayout.Space(10);
            withConfig = EditorGUILayout.Toggle("Config", withConfig);
            if (withConfig) {
                EditorGUI.indentLevel++;
                serialization = EditorGUILayout.Toggle("Serialization", serialization);
                if (serialization) {
                    unmanaged = EditorGUILayout.Toggle("Unmanaged", unmanaged);
                }
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
            sb.AppendLine("using FFS.Libraries.StaticPack;", withConfig && serialization);
            sb.AppendLine("using FFS.Libraries.StaticEcs.Unity;", withColor || autoRegister || ignoreInEditor);
            sb.AppendLine("using UnityEngine.Scripting;", autoRegister);
            sb.AppendLine($"using static FFS.Libraries.StaticEcs.World<{worldTypeName}>;");
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
            sb.AppendLine($"{pad}    [Preserve]", autoRegister);
            sb.AppendLine($"{pad}    [StaticEcsAutoRegistration(typeof({worldTypeName}))]", autoRegister);
            sb.AppendLine($"{pad}    public static void RegisterFor{worldTypeName}() {{");
            sb.AppendLine($"{pad}        Events.RegisterEventType(new Config<{worldTypeName}>());", withConfig);
            sb.AppendLine($"{pad}        Events.RegisterEventType<{eventName}>();", !withConfig);
            sb.AppendLine($"{pad}    }}");
            sb.AppendLine();
            if (withConfig) {
                sb.AppendLine($"{pad}    public class Config<WorldType> : DefaultEventConfig<{eventName}, WorldType> where WorldType : struct, IWorldType {{");
                if (serialization) {
                    sb.AppendLine($"{pad}        public override Guid Id() => new(\"{GUID.Generate().ToString()}\");\n");
                    sb.AppendLine($"{pad}        public override BinaryWriter<{eventName}> Writer() => null; // TODO implement this\n");
                    sb.AppendLine($"{pad}        public override BinaryReader<{eventName}> Reader() => null; // TODO implement this\n");
                    sb.AppendLine($"{pad}        public override IPackArrayStrategy<{eventName}> ReadWriteStrategy() => new UnmanagedPackArrayStrategy<{eventName}>();\n", unmanaged);
                }
                sb.AppendLine($"{pad}    }}\n");
            }
            sb.AppendLine($"{pad}}}");
            sb.AppendLine("}", !string.IsNullOrEmpty(nameSpace));

            return sb.ToString();
        }
    }
}