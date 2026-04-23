using System;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace FFS.Libraries.StaticEcs.Unity.Editor {

    internal static class Extensions {
        public static void AppendLine(this StringBuilder builder, string line, bool val) {
            if (val) {
                builder.AppendLine(line);
            }
        }
    }

    public class ComponentTemplateWindow : EditorWindow {
        string[] names = {"Component"};
        string path;
        Vector2 scroll;

        string nameSpace;
        string worldName;
        string worldTypeName;
        Type worldType;

        bool serialization = false;
        bool unmanaged = false;
        bool withHooks = false;

        bool disableable = false;
        bool trackableAdded = false;
        bool trackableDeleted = false;
        bool trackableChanged = false;

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

        [MenuItem("Assets/Create/Static ECS/Components", false, -230)]
        static void ShowWindow() {
            var window = GetWindow<ComponentTemplateWindow>(true, "Component template");
            window.minSize = new Vector2(300, 200);
            window.path = AssetPath();
            window.nameSpace = EditorSettings.projectGenerationRootNamespace.Trim();
            Drawer.openHideFlags.Add((typeof(string[]).FullName + "Components" + 0).GetHashCode());
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
                if (worldType != null && names.Length > 0 && GUILayout.Button("Create Components", Ui.ButtonStyleYellow)) {
                    CreateFiles();
                    Close();
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10);
            nameSpace = EditorGUILayout.TextField("Namespace", nameSpace);

            EditorGUILayout.Space(10);
            Drawer.DrawStringArray("Components", ref names);
            for (var i = 0; i < names.Length; i++) {
                ref var val = ref names[i];
                if (string.IsNullOrEmpty(val)) {
                    val = "Component";
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
            EditorGUILayout.LabelField("Markers:", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            disableable = EditorGUILayout.Toggle("IDisableable", disableable);
            trackableAdded = EditorGUILayout.Toggle("ITrackableAdded", trackableAdded);
            trackableDeleted = EditorGUILayout.Toggle("ITrackableDeleted", trackableDeleted);
            trackableChanged = EditorGUILayout.Toggle("ITrackableChanged", trackableChanged);
            EditorGUI.indentLevel--;

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
                if (disableable) {
                    hasDisabledMethod = EditorGUILayout.Toggle("Has disabled", hasDisabledMethod);
                    hasEnabledMethod = EditorGUILayout.Toggle("Has enabled", hasEnabledMethod);
                    enableMethod = EditorGUILayout.Toggle("Enable", enableMethod);
                    disableMethod = EditorGUILayout.Toggle("Disable", disableMethod);
                } else {
                    hasDisabledMethod = false;
                    hasEnabledMethod = false;
                    enableMethod = false;
                    disableMethod = false;
                }
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

        public string CreateTemplate(string componentName) {
            var sb = new StringBuilder();
            var pad = string.IsNullOrEmpty(nameSpace) ? "" : "    ";
            sb.AppendLine("using System;");
            sb.AppendLine("using FFS.Libraries.StaticEcs;");
            sb.AppendLine("using FFS.Libraries.StaticPack;", serialization);
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
            var interfaces = $"IComponent, IComponentConfig<{componentName}>";
            if (disableable) interfaces += ", IDisableable";
            if (trackableAdded) interfaces += ", ITrackableAdded";
            if (trackableDeleted) interfaces += ", ITrackableDeleted";
            if (trackableChanged) interfaces += ", ITrackableChanged";
            sb.AppendLine($"{pad}public struct {componentName} : {interfaces} {{");
            sb.AppendLine($"{pad}    // TODO Write your component fields");
            sb.AppendLine("");
            if (serialization && unmanaged) {
                sb.AppendLine($"{pad}    public ComponentTypeConfig<{componentName}> Config() => new(");
                sb.AppendLine($"{pad}        guid: new(\"{GUID.Generate().ToString()}\"),");
                sb.AppendLine($"{pad}        readWriteStrategy: new UnmanagedPackArrayStrategy<{componentName}>()");
                sb.AppendLine($"{pad}    );");
            } else {
                sb.AppendLine($"{pad}    public ComponentTypeConfig<{componentName}> Config() => new(guid: new(\"{GUID.Generate().ToString()}\"));");
            }
            sb.AppendLine();
            if (serialization) {
                sb.AppendLine($"{pad}    public void Write<TWorld>(ref BinaryPackWriter writer, World<TWorld>.Entity self) where TWorld : struct, IWorldType {{");
                sb.AppendLine($"{pad}        throw new NotImplementedException(); // TODO implement this");
                sb.AppendLine($"{pad}    }}");
                sb.AppendLine();
                sb.AppendLine($"{pad}    public void Read<TWorld>(ref BinaryPackReader reader, World<TWorld>.Entity self, byte version, bool disabled) where TWorld : struct, IWorldType {{");
                sb.AppendLine($"{pad}        throw new NotImplementedException(); // TODO implement this");
                sb.AppendLine($"{pad}    }}");
                sb.AppendLine();
            }
            if (withHooks) {
                sb.AppendLine($"{pad}    public void OnAdd<TWorld>(World<TWorld>.Entity self) where TWorld : struct, IWorldType {{");
                sb.AppendLine($"{pad}        // TODO implement this");
                sb.AppendLine($"{pad}    }}");
                sb.AppendLine();
                sb.AppendLine($"{pad}    public void OnDelete<TWorld>(World<TWorld>.Entity self, HookReason reason) where TWorld : struct, IWorldType {{");
                sb.AppendLine($"{pad}        // TODO implement this");
                sb.AppendLine($"{pad}    }}");
                sb.AppendLine();
                sb.AppendLine($"{pad}    public void CopyTo<TWorld>(World<TWorld>.Entity self, World<TWorld>.Entity other, bool disabled) where TWorld : struct, IWorldType {{");
                sb.AppendLine($"{pad}        // TODO implement this");
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
                sb.AppendLine($"{pad}public static class {componentName}ExtensionsFor{worldTypeName} {{");
                sb.AppendLine($"{pad}    [MethodImpl(AggressiveInlining)]\n{pad}    public static ref {componentName} {componentName}(this Entity entity) => ref Components<{componentName}>.Instance.Ref(entity);\n", refMethod);
                sb.AppendLine($"{pad}    [MethodImpl(AggressiveInlining)]\n{pad}    public static ref {componentName} Add{componentName}(this Entity entity) => ref Components<{componentName}>.Instance.Add(entity);\n", addMethod);
                sb.AppendLine($"{pad}    [MethodImpl(AggressiveInlining)]\n{pad}    public static void Add{componentName}(this Entity entity, {componentName} value) => Components<{componentName}>.Instance.Add(entity) = value;\n", addMethod);
                sb.AppendLine($"{pad}    [MethodImpl(AggressiveInlining)]\n{pad}    public static void Set{componentName}(this Entity entity, {componentName} value) => Components<{componentName}>.Instance.Set(entity, value);\n", setMethod);
                sb.AppendLine($"{pad}    [MethodImpl(AggressiveInlining)]\n{pad}    public static bool Has{componentName}(this Entity entity) => Components<{componentName}>.Instance.Has(entity);\n", hasMethod);
                sb.AppendLine($"{pad}    [MethodImpl(AggressiveInlining)]\n{pad}    public static bool HasDisabled{componentName}(this Entity entity) => Components<{componentName}>.Instance.HasDisabled(entity);\n",  hasDisabledMethod);
                sb.AppendLine($"{pad}    [MethodImpl(AggressiveInlining)]\n{pad}    public static bool HasEnabled{componentName}(this Entity entity) => Components<{componentName}>.Instance.HasEnabled(entity);\n",   hasEnabledMethod);
                sb.AppendLine($"{pad}    [MethodImpl(AggressiveInlining)]\n{pad}    public static ToggleResult Enable{componentName}(this Entity entity) => Components<{componentName}>.Instance.Enable(entity);\n", enableMethod);
                sb.AppendLine($"{pad}    [MethodImpl(AggressiveInlining)]\n{pad}    public static ToggleResult Disable{componentName}(this Entity entity) => Components<{componentName}>.Instance.Disable(entity);\n", disableMethod);
                sb.AppendLine($"{pad}    [MethodImpl(AggressiveInlining)]\n{pad}    public static bool Delete{componentName}(this Entity entity) => Components<{componentName}>.Instance.Delete(entity);\n", deleteMethod);
                sb.AppendLine($"{pad}    [MethodImpl(AggressiveInlining)]\n{pad}    public static void Copy{componentName}To(this Entity entity, Entity dst) => Components<{componentName}>.Instance.Copy(entity, dst);\n", copyMethod);
                sb.AppendLine($"{pad}    [MethodImpl(AggressiveInlining)]\n{pad}    public static void Move{componentName}To(this Entity entity, Entity dst) => Components<{componentName}>.Instance.Move(entity, dst);\n", moveMethod);
                sb.AppendLine($"{pad}}}");
            }
            sb.AppendLine("}", !string.IsNullOrEmpty(nameSpace));

            return sb.ToString();
        }
    }
}
