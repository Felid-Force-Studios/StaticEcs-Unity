using System;
using System.Globalization;
using System.IO;
using System.Text;
using FFS.Libraries.StaticEcs.Unity.Editor.Inspectors;
using UnityEditor;
using UnityEngine;

namespace FFS.Libraries.StaticEcs.Unity.Editor {
    public class TagTemplateWindow : EditorWindow {
        ArrayDrawer<string> namesDrawer = new();
        string[] names = {"Tag"};
        string path;
        Vector2 scroll;
        
        string nameSpace;
        string worldName;
        string worldTypeName;
        Type worldType;
        
        bool autoRegister = true;
        bool serialization = true;
        
        bool withExtensions = true;
        bool hasMethod = true;
        bool hasNotMethod = false;
        bool isMethod = false;
        bool isNotMethod = false;
        bool setMethod = true;
        bool deleteMethod = true;
        bool toggleMethod = true;
        bool applyMethod = true;
        bool copyMethod = true;
        bool moveMethod = true;
        
        bool withColor = true;
        Color color = Color.white;

        [MenuItem("Assets/Create/Static ECS/Tags", false, -229)]
        static void ShowWindow() {
            var window = GetWindow<TagTemplateWindow>(true, "Tag template");
            window.minSize = new Vector2(300, 200);
            window.path = AssetPath();
            window.nameSpace = EditorSettings.projectGenerationRootNamespace.Trim();
            Drawer.openHideFlags.Add(typeof(string[]).FullName + "Tags" + 0);
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
                if (worldType != null && names.Length > 0 && GUILayout.Button("Create Tags", Ui.ButtonStyleYellow)) {
                    CreateFiles();
                    Close();
                }
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(10);
            nameSpace = EditorGUILayout.TextField("Namespace", nameSpace);
            
            EditorGUILayout.Space(10);
            var context = new DrawContext();
            namesDrawer.DrawValue(ref context, "Tags", ref names);
            for (var i = 0; i < names.Length; i++) {
                ref var val = ref names[i];
                if (string.IsNullOrEmpty(val)) {
                    val = "Tag";
                }
            }
            
            EditorGUILayout.Space(10);
            autoRegister = EditorGUILayout.Toggle("Auto registration", autoRegister);
            
            EditorGUILayout.Space(10);
            serialization = EditorGUILayout.Toggle("Serialization", serialization);
            
            EditorGUILayout.Space(10);
            withColor = EditorGUILayout.Toggle("Editor color", withColor);
            if (withColor) {
                EditorGUI.indentLevel++;
                color = EditorGUILayout.ColorField("Color", color);
                EditorGUI.indentLevel--;
            }
            
            EditorGUILayout.Space(10);
            withExtensions = EditorGUILayout.Toggle("Extensions", withExtensions);
            if (withExtensions) {
                EditorGUI.indentLevel++;
                hasMethod = EditorGUILayout.Toggle("Has", hasMethod);
                hasNotMethod = EditorGUILayout.Toggle("Has not", hasNotMethod);
                isMethod = EditorGUILayout.Toggle("Is", isMethod);
                isNotMethod = EditorGUILayout.Toggle("Is not", isNotMethod);
                setMethod = EditorGUILayout.Toggle("Set", setMethod);
                deleteMethod = EditorGUILayout.Toggle("Delete", deleteMethod);
                toggleMethod = EditorGUILayout.Toggle("Toggle", toggleMethod);
                applyMethod = EditorGUILayout.Toggle("Apply", applyMethod);
                copyMethod = EditorGUILayout.Toggle("Copy", copyMethod);
                moveMethod = EditorGUILayout.Toggle("Move", moveMethod);
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

        public string CreateTemplate(string tagName) {
            var sb = new StringBuilder();
            var pad = string.IsNullOrEmpty(nameSpace) ? "" : "    ";
            sb.AppendLine("using System;");
            sb.AppendLine("using FFS.Libraries.StaticEcs;");
            sb.AppendLine("using FFS.Libraries.StaticEcs.Unity;", withColor || autoRegister);
            sb.AppendLine("using UnityEngine.Scripting;", autoRegister);
            sb.AppendLine($"#if ENABLE_IL2CPP");
            sb.AppendLine($"using Unity.IL2CPP.CompilerServices;");
            sb.AppendLine($"#endif");
            sb.AppendLine("using System.Runtime.CompilerServices;", withExtensions);
            sb.AppendLine("using static System.Runtime.CompilerServices.MethodImplOptions;", withExtensions);
            sb.AppendLine($"using static FFS.Libraries.StaticEcs.World<{worldTypeName}>;");
            sb.AppendLine();
            sb.AppendLine($"namespace {nameSpace} {{", !string.IsNullOrEmpty(nameSpace));
            sb.AppendLine($"{pad}#if ENABLE_IL2CPP");
            sb.AppendLine($"{pad}[Il2CppSetOption(Option.NullChecks, false)]");
            sb.AppendLine($"{pad}[Il2CppSetOption(Option.ArrayBoundsChecks, false)]");
            sb.AppendLine($"{pad}#endif");
            sb.AppendLine($"{pad}[Serializable]");
            sb.AppendLine($"{pad}[StaticEcsEditorColor(" +
                $"{color.r.ToString("0.###", CultureInfo.InvariantCulture)}f, " +
                $"{color.g.ToString("0.###", CultureInfo.InvariantCulture)}f, " +
                $"{color.b.ToString("0.###", CultureInfo.InvariantCulture)}f)]", withColor);
            sb.AppendLine($"{pad}public struct {tagName} : ITag {{");
            sb.AppendLine("");
            sb.AppendLine($"{pad}    [Preserve]", autoRegister);
            sb.AppendLine($"{pad}    [StaticEcsAutoRegistration(typeof({worldTypeName}))]", autoRegister);
            sb.AppendLine($"{pad}    public static void RegisterFor{worldTypeName}() {{");
            sb.AppendLine($"{pad}        RegisterTagType<{tagName}>(new(\"{GUID.Generate().ToString()}\"));", serialization);
            sb.AppendLine($"{pad}        RegisterTagType<{tagName}>();", !serialization);
            sb.AppendLine($"{pad}    }}");
            sb.AppendLine();
            sb.AppendLine($"{pad}}}");
            if (withExtensions) {
                sb.AppendLine();
                sb.AppendLine($"{pad}#if ENABLE_IL2CPP");
                sb.AppendLine($"{pad}[Il2CppSetOption(Option.NullChecks, false)]");
                sb.AppendLine($"{pad}[Il2CppSetOption(Option.ArrayBoundsChecks, false)]");
                sb.AppendLine($"{pad}#endif");
                sb.AppendLine($"{pad}public static class {tagName}ExtensionsFor{worldTypeName} {{");
                sb.AppendLine($"{pad}    [MethodImpl(AggressiveInlining)]\n{pad}    public static bool Has{tagName}(this Entity entity) => Tags<{tagName}>.Value.Has(entity);\n", hasMethod);
                sb.AppendLine($"{pad}    [MethodImpl(AggressiveInlining)]\n{pad}    public static bool HasNot{tagName}(this Entity entity) => !Tags<{tagName}>.Value.Has(entity);\n", hasMethod);
                sb.AppendLine($"{pad}    [MethodImpl(AggressiveInlining)]\n{pad}    public static bool Is{tagName}(this Entity entity) => Tags<{tagName}>.Value.Has(entity);\n", isMethod);
                sb.AppendLine($"{pad}    [MethodImpl(AggressiveInlining)]\n{pad}    public static bool IsNot{tagName}(this Entity entity) => !Tags<{tagName}>.Value.Has(entity);\n", isNotMethod);
                sb.AppendLine($"{pad}    [MethodImpl(AggressiveInlining)]\n{pad}    public static bool Set{tagName}(this Entity entity) => Tags<{tagName}>.Value.Set(entity);\n", setMethod);
                sb.AppendLine($"{pad}    [MethodImpl(AggressiveInlining)]\n{pad}    public static bool Delete{tagName}(this Entity entity) => Tags<{tagName}>.Value.Delete(entity);\n", deleteMethod);
                sb.AppendLine($"{pad}    [MethodImpl(AggressiveInlining)]\n{pad}    public static bool Toggle{tagName}(this Entity entity) => Tags<{tagName}>.Value.Toggle(entity);\n", toggleMethod);
                sb.AppendLine($"{pad}    [MethodImpl(AggressiveInlining)]\n{pad}    public static void Apply{tagName}(this Entity entity, bool state) => Tags<{tagName}>.Value.Apply(entity, state);\n", applyMethod);
                sb.AppendLine($"{pad}    [MethodImpl(AggressiveInlining)]\n{pad}    public static void Copy{tagName}To(this Entity entity, Entity dst) => Tags<{tagName}>.Value.Copy(entity, dst);\n", copyMethod);
                sb.AppendLine($"{pad}    [MethodImpl(AggressiveInlining)]\n{pad}    public static void Move{tagName}To(this Entity entity, Entity dst) => Tags<{tagName}>.Value.Move(entity, dst);\n", moveMethod);
                sb.AppendLine($"{pad}}}");
            }
            sb.AppendLine("}", !string.IsNullOrEmpty(nameSpace));

            return sb.ToString();
        }
    }
}