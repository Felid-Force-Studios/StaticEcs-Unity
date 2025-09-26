using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using FFS.Libraries.StaticEcs.Unity.Editor.Inspectors;
using UnityEditor;
using UnityEngine;

namespace FFS.Libraries.StaticEcs.Unity.Editor {
    public class EntityLinksComponentTemplateWindow : EditorWindow {
        ArrayDrawer<string> namesDrawer = new();
        string[] names = {"Links"};
        string path;
        Vector2 scroll;
        
        string nameSpace;
        string worldName;
        string worldTypeName;
        Type worldType;
        
        bool autoRegister = true;
        
        bool withConfig = true;
        bool serialization = false;
        bool onComponentChanged = false;
        bool copyable = true;
        bool clearable = true;
        
        bool withExtensions = true;
        bool refMethod = true;
        bool addMethod = true;
        bool tryAddMethod = true;
        bool putMethod = true;
        bool hasMethod = true;
        bool hasDisabledMethod = true;
        bool hasEnabledMethod = true;
        bool enableMethod = true;
        bool disableMethod = true;
        bool deleteMethod = true;
        bool tryDeleteMethod = true;
        bool copyMethod = true;
        bool tryCopyMethod = true;
        bool moveMethod = true;
        bool tryMoveMethod = true;
        bool setLinksMethod = true;
        bool tryDeleteLinksMethod = true;
        
        
        bool withColor = true;
        Color color = Color.white;

        [MenuItem("Assets/Create/Static ECS/Entity Links-Components", false, -225)]
        static void ShowWindow() {
            var window = GetWindow<EntityLinksComponentTemplateWindow>(true, "Entity Links-Component template");
            window.minSize = new Vector2(300, 200);
            window.path = AssetPath();
            window.nameSpace = EditorSettings.projectGenerationRootNamespace.Trim();
            Drawer.openHideFlags.Add(typeof(string[]).FullName + "Links" + 10);
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
            var context = new DrawContext() {
                Level = 10
            };
            namesDrawer.DrawValue(ref context, "Links", ref names);
            for (var i = 0; i < names.Length; i++) {
                ref var val = ref names[i];
                if (string.IsNullOrEmpty(val)) {
                    val = "Component";
                }
            }
            
            EditorGUILayout.Space(10);
            autoRegister = EditorGUILayout.Toggle("Auto registration", autoRegister);

            EditorGUILayout.Space(10);
            withConfig = EditorGUILayout.Toggle("Config", withConfig);
            if (withConfig) {
                EditorGUI.indentLevel++;
                serialization = EditorGUILayout.Toggle("Serialization", serialization);
                onComponentChanged = EditorGUILayout.Toggle("Events", onComponentChanged);
                copyable = EditorGUILayout.Toggle("Copyable", copyable);
                clearable = EditorGUILayout.Toggle("Clearable", clearable);
                EditorGUI.indentLevel--;
            }
            
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
                refMethod = EditorGUILayout.Toggle("Ref", refMethod);
                addMethod = EditorGUILayout.Toggle("Add", addMethod);
                tryAddMethod = EditorGUILayout.Toggle("Try add", tryAddMethod);
                putMethod = EditorGUILayout.Toggle("Put", putMethod);
                hasMethod = EditorGUILayout.Toggle("Has", hasMethod);
                hasDisabledMethod = EditorGUILayout.Toggle("Has disabled", hasDisabledMethod);
                hasEnabledMethod = EditorGUILayout.Toggle("Has enabled", hasEnabledMethod);
                enableMethod = EditorGUILayout.Toggle("Enable", enableMethod);
                disableMethod = EditorGUILayout.Toggle("Disable", disableMethod);
                deleteMethod = EditorGUILayout.Toggle("Delete", deleteMethod);
                tryDeleteMethod = EditorGUILayout.Toggle("Try delete", tryDeleteMethod);
                copyMethod = EditorGUILayout.Toggle("Copy", copyMethod);
                tryCopyMethod = EditorGUILayout.Toggle("Try copy", tryCopyMethod);
                moveMethod = EditorGUILayout.Toggle("Move", moveMethod);
                tryMoveMethod = EditorGUILayout.Toggle("Try move", tryMoveMethod);
                setLinksMethod = EditorGUILayout.Toggle("Set links", setLinksMethod);
                tryDeleteLinksMethod = EditorGUILayout.Toggle("Try delete links", tryDeleteLinksMethod);
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
            sb.AppendLine("using FFS.Libraries.StaticPack;", withConfig && serialization);
            sb.AppendLine("using FFS.Libraries.StaticEcs.Unity;", withColor || autoRegister);
            sb.AppendLine("using UnityEngine.Scripting;", autoRegister);
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
            sb.AppendLine($"{pad}public struct {componentName} : IEntityLinksComponent<{componentName}> {{");
            sb.AppendLine($"{pad}    public ROMulti<EntityGID> Links;");
            sb.AppendLine();
            sb.AppendLine($"{pad}    [MethodImpl(AggressiveInlining)]");
            sb.AppendLine($"{pad}    ref ROMulti<EntityGID> IRefProvider<{componentName}, ROMulti<EntityGID>>.RefValue(ref {componentName} component) => ref component.Links;");
            sb.AppendLine();
            sb.AppendLine($"{pad}    [Preserve]", autoRegister);
            sb.AppendLine($"{pad}    [StaticEcsAutoRegistration(typeof({worldTypeName}))]", autoRegister);
            sb.AppendLine($"{pad}    public static void RegisterFor{worldTypeName}() {{");
            sb.AppendLine($"{pad}        throw new NotImplementedException(\"Implement ONE of the following relations for {componentName}:\");");
            sb.AppendLine();
            sb.AppendLine($"{pad}        /*");
            sb.AppendLine($"{pad}        RegisterToManyRelationType<{componentName}>(8,");
            sb.AppendLine($"{pad}                                           deleteStrategy : OneDirectionalDeleteStrategy.Default,");
            sb.AppendLine($"{pad}                                           copyStrategy : CopyStrategy.Default,");
            sb.AppendLine($"{pad}                                           config : new Config<{worldTypeName}>()", withConfig);
            sb.AppendLine($"{pad}                                           config : null", !withConfig);
            sb.AppendLine($"{pad}        );");
            sb.AppendLine($"{pad}        RegisterOneToManyRelationType<ANOTHER_LINK_TYPE, {componentName}>(8,");
            sb.AppendLine($"{pad}                                                                 leftDeleteStrategy: BiDirectionalDeleteStrategy.DeleteAnotherLink,");
            sb.AppendLine($"{pad}                                                                 rightDeleteStrategy: BiDirectionalDeleteStrategy.DeleteAnotherLink,");
            sb.AppendLine($"{pad}                                                                 leftCopyStrategy: CopyStrategy.Default,");
            sb.AppendLine($"{pad}                                                                 rightCopyStrategy: CopyStrategy.Default,");
            sb.AppendLine($"{pad}                                                                 leftConfig: ANOTHER_LINK_TYPE_CONFIG_OR_NULL,");
            sb.AppendLine($"{pad}                                                                 rightConfig: new Config<{worldTypeName}>()", withConfig);
            sb.AppendLine($"{pad}                                                                 rightConfig: null", !withConfig);
            sb.AppendLine($"{pad}        );");
            sb.AppendLine($"{pad}        RegisterManyToManyRelationType<ANOTHER_LINK_TYPE, {componentName}>(8,");
            sb.AppendLine($"{pad}                                                                  leftDeleteStrategy: BiDirectionalDeleteStrategy.DeleteAnotherLink,");
            sb.AppendLine($"{pad}                                                                  rightDeleteStrategy: BiDirectionalDeleteStrategy.DeleteAnotherLink,");
            sb.AppendLine($"{pad}                                                                  leftCopyStrategy: CopyStrategy.Default,");
            sb.AppendLine($"{pad}                                                                  rightCopyStrategy: CopyStrategy.Default,");
            sb.AppendLine($"{pad}                                                                  leftConfig: ANOTHER_LINK_TYPE_CONFIG_OR_NULL,");
            sb.AppendLine($"{pad}                                                                  rightConfig: new Config<{worldTypeName}>()", withConfig);
            sb.AppendLine($"{pad}                                                                  rightConfig: null", !withConfig);
            sb.AppendLine($"{pad}        );");
            sb.AppendLine($"{pad}        */");
            sb.AppendLine($"{pad}    }}");
            sb.AppendLine();
            if (withConfig) {
                sb.AppendLine($"{pad}    public class Config<WorldType> : DefaultComponentConfig<{componentName}, WorldType> where WorldType : struct, IWorldType {{");
                if (serialization) {
                    sb.AppendLine($"{pad}        public override Guid Id() => new(\"{GUID.Generate().ToString()}\");\n");
                    sb.AppendLine($"{pad}        public override BinaryWriter<{componentName}> Writer() => (ref BinaryPackWriter writer, in {componentName} value) => {{");
                    sb.AppendLine($"{pad}            writer.WriteROMulti(value.Links);");
                    sb.AppendLine($"{pad}        }};\n");
                    sb.AppendLine($"{pad}        public override BinaryReader<{componentName}> Reader() => (ref BinaryPackReader reader) => new {componentName} {{");
                    sb.AppendLine($"{pad}            Links = reader.ReadROMulti<{worldTypeName}, EntityGID>()");
                    sb.AppendLine($"{pad}        }};\n");
                }
                if (onComponentChanged) {
                    sb.AppendLine($"{pad}        public override World<WorldType>.OnComponentHandler<{componentName}> OnAdd() => null;\n");
                    sb.AppendLine($"{pad}        public override World<WorldType>.OnComponentHandler<{componentName}> OnPut() => null;\n");
                    sb.AppendLine($"{pad}        public override World<WorldType>.OnComponentHandler<{componentName}> OnDelete() => null;\n");
                    sb.AppendLine($"{pad}        public override World<WorldType>.OnCopyHandler<{componentName}> OnCopy() => null;\n"); 
                }
                sb.AppendLine($"{pad}        public override bool IsCopyable() => {copyable.ToString().ToLower()};\n");
                sb.AppendLine($"{pad}        public override bool IsClearable() => {clearable.ToString().ToLower()};\n");
                sb.AppendLine($"{pad}    }}\n");
            }
            sb.AppendLine($"{pad}}}");
            if (withExtensions) {
                sb.AppendLine();
                sb.AppendLine($"{pad}#if ENABLE_IL2CPP");
                sb.AppendLine($"{pad}[Il2CppSetOption(Option.NullChecks, false)]");
                sb.AppendLine($"{pad}[Il2CppSetOption(Option.ArrayBoundsChecks, false)]");
                sb.AppendLine($"{pad}#endif");
                sb.AppendLine($"{pad}public static class {componentName}ExtensionsFor{worldTypeName} {{");
                sb.AppendLine($"{pad}    [MethodImpl(AggressiveInlining)]\n{pad}    public static ref {componentName} {componentName}(this Entity entity) => ref Components<{componentName}>.Value.Ref(entity);\n", refMethod);
                sb.AppendLine($"{pad}    [MethodImpl(AggressiveInlining)]\n{pad}    public static ref {componentName} Add{componentName}(this Entity entity) => ref Components<{componentName}>.Value.Add(entity);\n", addMethod);
                sb.AppendLine($"{pad}    [MethodImpl(AggressiveInlining)]\n{pad}    public static void Add{componentName}(this Entity entity, {componentName} value) => Components<{componentName}>.Value.Add(entity) = value;\n", addMethod);
                sb.AppendLine($"{pad}    [MethodImpl(AggressiveInlining)]\n{pad}    public static ref {componentName} TryAdd{componentName}(this Entity entity) => ref Components<{componentName}>.Value.TryAdd(entity);\n", tryAddMethod);
                sb.AppendLine($"{pad}    [MethodImpl(AggressiveInlining)]\n{pad}    public static void TryAdd{componentName}(this Entity entity, {componentName} value) => Components<{componentName}>.Value.TryAdd(entity) = value;\n", tryAddMethod);
                sb.AppendLine($"{pad}    [MethodImpl(AggressiveInlining)]\n{pad}    public static void Put{componentName}(this Entity entity, {componentName} value) => Components<{componentName}>.Value.Put(entity, value);\n", putMethod);
                sb.AppendLine($"{pad}    [MethodImpl(AggressiveInlining)]\n{pad}    public static bool Has{componentName}(this Entity entity) => Components<{componentName}>.Value.Has(entity);\n", hasMethod);
                sb.AppendLine($"{pad}    [MethodImpl(AggressiveInlining)]\n{pad}    public static bool HasDisabled{componentName}(this Entity entity) => Components<{componentName}>.Value.HasDisabled(entity);\n",  hasDisabledMethod);
                sb.AppendLine($"{pad}    [MethodImpl(AggressiveInlining)]\n{pad}    public static bool HasEnabled{componentName}(this Entity entity) => Components<{componentName}>.Value.HasEnabled(entity);\n",   hasEnabledMethod);
                sb.AppendLine($"{pad}    [MethodImpl(AggressiveInlining)]\n{pad}    public static void Enable{componentName}(this Entity entity) => Components<{componentName}>.Value.Enable(entity);\n", enableMethod);
                sb.AppendLine($"{pad}    [MethodImpl(AggressiveInlining)]\n{pad}    public static void Disable{componentName}(this Entity entity) => Components<{componentName}>.Value.Disable(entity);\n", disableMethod);
                sb.AppendLine($"{pad}    [MethodImpl(AggressiveInlining)]\n{pad}    public static void Delete{componentName}(this Entity entity) => Components<{componentName}>.Value.Delete(entity);\n", deleteMethod);
                sb.AppendLine($"{pad}    [MethodImpl(AggressiveInlining)]\n{pad}    public static bool TryDelete{componentName}(this Entity entity) => Components<{componentName}>.Value.TryDelete(entity);\n",  tryDeleteMethod);
                sb.AppendLine($"{pad}    [MethodImpl(AggressiveInlining)]\n{pad}    public static void Copy{componentName}To(this Entity entity, Entity dst) => Components<{componentName}>.Value.Copy(entity, dst);\n", copyMethod);
                sb.AppendLine($"{pad}    [MethodImpl(AggressiveInlining)]\n{pad}    public static bool TryCopy{componentName}To(this Entity entity, Entity dst) => Components<{componentName}>.Value.TryCopy(entity, dst);\n", tryCopyMethod);
                sb.AppendLine($"{pad}    [MethodImpl(AggressiveInlining)]\n{pad}    public static void Move{componentName}To(this Entity entity, Entity dst) => Components<{componentName}>.Value.Move(entity, dst);\n", moveMethod);
                sb.AppendLine($"{pad}    [MethodImpl(AggressiveInlining)]\n{pad}    public static bool TryMove{componentName}To(this Entity entity, Entity dst) => Components<{componentName}>.Value.TryMove(entity, dst);\n",  tryMoveMethod);
                sb.AppendLine($"{pad}    [MethodImpl(AggressiveInlining)]\n{pad}    public static void Set{componentName}Links(this Entity entity, EntityGID link1) => entity.SetLinks<{componentName}>(link1);\n",  setLinksMethod);
                sb.AppendLine($"{pad}    [MethodImpl(AggressiveInlining)]\n{pad}    public static void Set{componentName}Links(this Entity entity, EntityGID link1, EntityGID link2) => entity.SetLinks<{componentName}>(link1, link2);\n",  setLinksMethod);
                sb.AppendLine($"{pad}    [MethodImpl(AggressiveInlining)]\n{pad}    public static void Set{componentName}Links(this Entity entity, EntityGID link1, EntityGID link2, EntityGID link3) => entity.SetLinks<{componentName}>(link1, link2, link3);\n",  setLinksMethod);
                sb.AppendLine($"{pad}    [MethodImpl(AggressiveInlining)]\n{pad}    public static void TryDelete{componentName}Links(this Entity entity, EntityGID link1) => entity.TryDeleteLinks<{componentName}>(link1);\n",  tryDeleteLinksMethod);
                sb.AppendLine($"{pad}    [MethodImpl(AggressiveInlining)]\n{pad}    public static void TryDelete{componentName}Links(this Entity entity, EntityGID link1, EntityGID link2) => entity.TryDeleteLinks<{componentName}>(link1, link2);\n",  tryDeleteLinksMethod);
                sb.AppendLine($"{pad}    [MethodImpl(AggressiveInlining)]\n{pad}    public static void TryDelete{componentName}Links(this Entity entity, EntityGID link1, EntityGID link2, EntityGID link3) => entity.TryDeleteLinks<{componentName}>(link1, link2, link3);\n",  tryDeleteLinksMethod);
                sb.AppendLine($"{pad}}}");
            }
            sb.AppendLine("}", !string.IsNullOrEmpty(nameSpace));

            return sb.ToString();
        }
    }
}