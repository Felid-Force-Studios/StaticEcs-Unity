using System;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace FFS.Libraries.StaticEcs.Unity.Editor {

    public class MultiComponentTemplateWindow : EditorWindow {
        string[] names = {"Element"};
        string path;
        Vector2 scroll;

        string nameSpace;
        string worldName;
        string worldTypeName;
        Type worldType;

        bool serialization = false;
        bool unmanaged = true;
        bool withHooks = false;

        bool withExtensions = true;
        bool refMethod = true;
        bool addMethod = true;
        bool setMethod = true;
        bool hasMethod = true;
        bool hasDisabledMethod = true;
        bool hasEnabledMethod = true;
        bool enableMethod = true;
        bool disableMethod = true;
        bool deleteMethod = true;
        bool copyMethod = true;
        bool moveMethod = true;


        bool withColor = true;
        Color color = Color.white;

        bool withGroup = false;
        string groupName = "";

        [MenuItem("Assets/Create/Static ECS/Multi-Components", false, -227)]
        static void ShowWindow() {
            var window = GetWindow<MultiComponentTemplateWindow>(true, "Multi-Component template");
            window.minSize = new Vector2(300, 200);
            window.path = AssetPath();
            window.nameSpace = EditorSettings.projectGenerationRootNamespace.Trim();
            Drawer.openHideFlags.Add((typeof(string[]).FullName + "MultiComponents" + 0).GetHashCode());
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
                if (worldType != null && names.Length > 0 && GUILayout.Button("Create Multi-Components", Ui.ButtonStyleYellow)) {
                    CreateFiles();
                    Close();
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10);
            nameSpace = EditorGUILayout.TextField("Namespace", nameSpace);

            EditorGUILayout.Space(10);
            Drawer.DrawStringArray("Elements", ref names);
            for (var i = 0; i < names.Length; i++) {
                ref var val = ref names[i];
                if (string.IsNullOrEmpty(val)) {
                    val = "Element";
                }
            }

            EditorGUILayout.Space(10);
            serialization = EditorGUILayout.Toggle("Serialization", serialization);
            if (serialization) {
                EditorGUI.indentLevel++;
                unmanaged = EditorGUILayout.Toggle("Unmanaged", unmanaged);
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space(10);
            withHooks = EditorGUILayout.Toggle("Hooks", withHooks);

            EditorGUILayout.Space(10);
            withColor = EditorGUILayout.Toggle("Editor color", withColor);
            if (withColor) {
                EditorGUI.indentLevel++;
                color = EditorGUILayout.ColorField("Color", color);
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space(10);
            withGroup = EditorGUILayout.Toggle("Editor group", withGroup);
            if (withGroup) {
                EditorGUI.indentLevel++;
                groupName = EditorGUILayout.TextField("Group name", groupName);
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space(10);
            withExtensions = EditorGUILayout.Toggle("Extensions", withExtensions);
            if (withExtensions) {
                EditorGUI.indentLevel++;
                refMethod = EditorGUILayout.Toggle("Ref", refMethod);
                addMethod = EditorGUILayout.Toggle("Add", addMethod);
                setMethod = EditorGUILayout.Toggle("Set", setMethod);
                hasMethod = EditorGUILayout.Toggle("Has", hasMethod);
                hasDisabledMethod = EditorGUILayout.Toggle("Has disabled", hasDisabledMethod);
                hasEnabledMethod = EditorGUILayout.Toggle("Has enabled", hasEnabledMethod);
                enableMethod = EditorGUILayout.Toggle("Enable", enableMethod);
                disableMethod = EditorGUILayout.Toggle("Disable", disableMethod);
                deleteMethod = EditorGUILayout.Toggle("Delete", deleteMethod);
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
            foreach (var elementName in names) {
                var fileName = $"{path}/{elementName}.cs";
                var text = CreateTemplate(elementName);
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

        public string CreateTemplate(string elementName) {
            var sb = new StringBuilder();
            var pad = string.IsNullOrEmpty(nameSpace) ? "" : "    ";
            sb.AppendLine("using System;");
            sb.AppendLine("using FFS.Libraries.StaticEcs;");
            sb.AppendLine("using FFS.Libraries.StaticPack;");
            sb.AppendLine("using FFS.Libraries.StaticEcs.Unity;", withColor);
            sb.AppendLine($"#if ENABLE_IL2CPP");
            sb.AppendLine($"using Unity.IL2CPP.CompilerServices;");
            sb.AppendLine($"#endif");
            sb.AppendLine("using System.Runtime.CompilerServices;", withExtensions);
            sb.AppendLine("using static System.Runtime.CompilerServices.MethodImplOptions;", withExtensions);
            sb.AppendLine($"using static FFS.Libraries.StaticEcs.World<{worldTypeName}>;", withExtensions);
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
            if (withGroup) {
                if (withColor) {
                    sb.AppendLine($"{pad}[StaticEcsEditorGroup(\"{groupName}\", " +
                        $"{color.r.ToString("0.###", CultureInfo.InvariantCulture)}f, " +
                        $"{color.g.ToString("0.###", CultureInfo.InvariantCulture)}f, " +
                        $"{color.b.ToString("0.###", CultureInfo.InvariantCulture)}f)]");
                } else {
                    sb.AppendLine($"{pad}[StaticEcsEditorGroup(\"{groupName}\")]");
                }
            }
            sb.AppendLine($"{pad}public struct {elementName} : IMultiComponent, IMultiComponentConfig<{elementName}> {{");
            sb.AppendLine($"{pad}    // TODO Write your element fields");
            sb.AppendLine("");
            if (serialization && unmanaged) {
                sb.AppendLine($"{pad}    public ComponentTypeConfig<World<TWorld>.Multi<{elementName}>> Config<TWorld>() where TWorld : struct, IWorldType => new(");
                sb.AppendLine($"{pad}        guid: new(\"{GUID.Generate().ToString()}\"),");
                sb.AppendLine($"{pad}        readWriteStrategy: new MultiUnmanagedPackArrayStrategy<TWorld, {elementName}>()");
                sb.AppendLine($"{pad}    );");
            } else {
                sb.AppendLine($"{pad}    public ComponentTypeConfig<World<TWorld>.Multi<{elementName}>> Config<TWorld>() where TWorld : struct, IWorldType");
                sb.AppendLine($"{pad}        => new(guid: new(\"{GUID.Generate().ToString()}\"));");
            }
            sb.AppendLine();
            if (unmanaged) {
                sb.AppendLine($"{pad}    public IPackArrayStrategy<{elementName}> ElementPackStrategy() => new UnmanagedPackArrayStrategy<{elementName}>();");
            } else {
                sb.AppendLine($"{pad}    public IPackArrayStrategy<{elementName}> ElementPackStrategy() => null;");
            }
            sb.AppendLine();
            if (withHooks) {
                sb.AppendLine($"{pad}    public void Write(ref BinaryPackWriter writer) {{");
                sb.AppendLine($"{pad}        throw new NotImplementedException(); // TODO implement this");
                sb.AppendLine($"{pad}    }}");
                sb.AppendLine();
                sb.AppendLine($"{pad}    public void Read(ref BinaryPackReader reader) {{");
                sb.AppendLine($"{pad}        throw new NotImplementedException(); // TODO implement this");
                sb.AppendLine($"{pad}    }}");
                sb.AppendLine();
            }
            sb.AppendLine($"{pad}}}");
            if (withExtensions) {
                sb.AppendLine();
                sb.AppendLine($"{pad}#if ENABLE_IL2CPP");
                sb.AppendLine($"{pad}[Il2CppSetOption(Option.NullChecks, false)]");
                sb.AppendLine($"{pad}[Il2CppSetOption(Option.ArrayBoundsChecks, false)]");
                sb.AppendLine($"{pad}#endif");
                sb.AppendLine($"{pad}public static class {elementName}ExtensionsFor{worldTypeName} {{");
                sb.AppendLine($"{pad}    [MethodImpl(AggressiveInlining)]\n{pad}    public static ref Multi<{elementName}> {elementName}Multi(this Entity entity) => ref Components<Multi<{elementName}>>.Instance.Ref(entity);\n", refMethod);
                sb.AppendLine($"{pad}    [MethodImpl(AggressiveInlining)]\n{pad}    public static ref Multi<{elementName}> Add{elementName}Multi(this Entity entity) => ref Components<Multi<{elementName}>>.Instance.Add(entity);\n", addMethod);
                sb.AppendLine($"{pad}    [MethodImpl(AggressiveInlining)]\n{pad}    public static void Set{elementName}Multi(this Entity entity, Multi<{elementName}> value) => Components<Multi<{elementName}>>.Instance.Set(entity, value);\n", setMethod);
                sb.AppendLine($"{pad}    [MethodImpl(AggressiveInlining)]\n{pad}    public static bool Has{elementName}Multi(this Entity entity) => Components<Multi<{elementName}>>.Instance.Has(entity);\n", hasMethod);
                sb.AppendLine($"{pad}    [MethodImpl(AggressiveInlining)]\n{pad}    public static bool HasDisabled{elementName}Multi(this Entity entity) => Components<Multi<{elementName}>>.Instance.HasDisabled(entity);\n",  hasDisabledMethod);
                sb.AppendLine($"{pad}    [MethodImpl(AggressiveInlining)]\n{pad}    public static bool HasEnabled{elementName}Multi(this Entity entity) => Components<Multi<{elementName}>>.Instance.HasEnabled(entity);\n",   hasEnabledMethod);
                sb.AppendLine($"{pad}    [MethodImpl(AggressiveInlining)]\n{pad}    public static ToggleResult Enable{elementName}Multi(this Entity entity) => Components<Multi<{elementName}>>.Instance.Enable(entity);\n", enableMethod);
                sb.AppendLine($"{pad}    [MethodImpl(AggressiveInlining)]\n{pad}    public static ToggleResult Disable{elementName}Multi(this Entity entity) => Components<Multi<{elementName}>>.Instance.Disable(entity);\n", disableMethod);
                sb.AppendLine($"{pad}    [MethodImpl(AggressiveInlining)]\n{pad}    public static bool Delete{elementName}Multi(this Entity entity) => Components<Multi<{elementName}>>.Instance.Delete(entity);\n", deleteMethod);
                sb.AppendLine($"{pad}    [MethodImpl(AggressiveInlining)]\n{pad}    public static void Copy{elementName}MultiTo(this Entity entity, Entity dst) => Components<Multi<{elementName}>>.Instance.Copy(entity, dst);\n", copyMethod);
                sb.AppendLine($"{pad}    [MethodImpl(AggressiveInlining)]\n{pad}    public static void Move{elementName}MultiTo(this Entity entity, Entity dst) => Components<Multi<{elementName}>>.Instance.Move(entity, dst);\n", moveMethod);
                sb.AppendLine($"{pad}}}");
            }
            sb.AppendLine("}", !string.IsNullOrEmpty(nameSpace));

            return sb.ToString();
        }
    }
}
