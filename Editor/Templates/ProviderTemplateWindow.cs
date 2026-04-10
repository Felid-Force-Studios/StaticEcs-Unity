using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace FFS.Libraries.StaticEcs.Unity.Editor {

    public class ProviderTemplateWindow : EditorWindow {
        string runtimePath;
        string editorPath;
        Vector2 scroll;

        string nameSpace;
        string worldName;
        string worldTypeName;
        Type worldType;

        string prefix;
        string menuItemPath;

        bool generateGUI = true;
        bool generateTMP = true;
        bool generateMouse = true;
        bool generatePhysics3D = true;
        bool generatePhysics2D = true;
        bool generateAnimation = true;

        [MenuItem("Assets/Create/Static ECS/Providers", false, -215)]
        static void ShowWindow() {
            var window = GetWindow<ProviderTemplateWindow>(true, "Provider template");
            window.minSize = new Vector2(300, 260);
            window.runtimePath = AssetPath();
            window.editorPath = window.runtimePath + "/Editor";
            window.nameSpace = EditorSettings.projectGenerationRootNamespace.Trim();
        }

        void OnGUI() {
            if (worldType == null) {
                for (var i = 0; i < MetaData.WorldsMetaData.Count; i++) {
                    worldType = MetaData.WorldsMetaData[i].WorldTypeType;
                    worldName = MetaData.WorldsMetaData[i].EditorName;
                    worldTypeName = worldType.Name;
                    prefix = ShortName(worldName);
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
                if (worldType != null && !string.IsNullOrEmpty(prefix) && GUILayout.Button("Create Providers", Ui.ButtonStyleYellow)) {
                    CreateFiles();
                    Close();
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10);
            nameSpace = EditorGUILayout.TextField("Namespace", nameSpace);

            EditorGUILayout.Space(10);
            prefix = EditorGUILayout.TextField("Prefix", prefix);
            if (string.IsNullOrEmpty(prefix)) {
                prefix = ShortName(worldName) ?? "Ecs";
            }

            EditorGUILayout.Space(10);
            menuItemPath = EditorGUILayout.TextField("Menu Item Path", menuItemPath);
            if (string.IsNullOrEmpty(menuItemPath)) {
                menuItemPath = "Window/" + prefix + " ECS";
            }

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Event providers:", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            generateGUI = EditorGUILayout.Toggle("GUI", generateGUI);
            generateTMP = EditorGUILayout.Toggle("TextMeshPro", generateTMP);
            generateMouse = EditorGUILayout.Toggle("Mouse", generateMouse);
            generatePhysics3D = EditorGUILayout.Toggle("Physics 3D", generatePhysics3D);
            generatePhysics2D = EditorGUILayout.Toggle("Physics 2D", generatePhysics2D);
            generateAnimation = EditorGUILayout.Toggle("Animation", generateAnimation);
            EditorGUI.indentLevel--;

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Generated classes:", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            EditorGUILayout.LabelField("Entity Provider:", prefix + "EntityProvider");
            EditorGUILayout.LabelField("Event Provider:", prefix + "EventProvider");
            EditorGUILayout.LabelField("Entity Provider Editor:", prefix + "EntityProviderEditor");
            EditorGUILayout.LabelField("Event Provider Editor:", prefix + "EventProviderEditor");
            EditorGUILayout.LabelField("ECS View:", prefix + "EcsView");

            var eventCount = 0;
            if (generateGUI) eventCount += 27;
            if (generateTMP) eventCount += 9;
            if (generateMouse) eventCount += 9;
            if (generatePhysics3D) eventCount += 11;
            if (generatePhysics2D) eventCount += 6;
            if (generateAnimation) eventCount += 6;
            if (eventCount > 0) {
                EditorGUILayout.LabelField("Events:", $"Events/ ({eventCount} providers)");
            }
            EditorGUI.indentLevel--;

            EditorGUILayout.Space(10);
            runtimePath = EditorGUILayout.TextField("Runtime Path", runtimePath);
            editorPath = EditorGUILayout.TextField("Editor Path", editorPath);

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
                        prefix = ShortName(worldName);
                        menuItemPath = "Window/" + prefix + " ECS";
                    },
                    MetaData.WorldsMetaData[i1].WorldTypeType);
            }

            menu.ShowAsContext();
        }

        public void CreateFiles() {
            var entityProviderName = prefix + "EntityProvider";
            var eventProviderName = prefix + "EventProvider";
            var entityProviderEditorName = prefix + "EntityProviderEditor";
            var eventProviderEditorName = prefix + "EventProviderEditor";
            var ecsViewName = prefix + "EcsView";

            WriteFile(runtimePath, entityProviderName, CreateEntityProviderTemplate(entityProviderName));
            WriteFile(runtimePath, eventProviderName, CreateEventProviderTemplate(eventProviderName));

            if (!AssetDatabase.IsValidFolder(editorPath)) {
                var parent = Path.GetDirectoryName(editorPath);
                var folder = Path.GetFileName(editorPath);
                AssetDatabase.CreateFolder(parent, folder);
            }

            WriteFile(editorPath, entityProviderEditorName, CreateEntityProviderEditorTemplate(entityProviderName, entityProviderEditorName));
            WriteFile(editorPath, eventProviderEditorName, CreateEventProviderEditorTemplate(eventProviderName, eventProviderEditorName));
            WriteFile(editorPath, ecsViewName, CreateEcsViewTemplate(entityProviderName, eventProviderName, ecsViewName));

            CreateEventProviderFiles(entityProviderName);

            AssetDatabase.Refresh();
        }

        void CreateEventProviderFiles(string entityProviderName) {
            if (!generateGUI && !generateTMP && !generateMouse && !generatePhysics3D && !generatePhysics2D && !generateAnimation) return;

            var eventsPath = runtimePath + "/Events";
            EnsureFolder(eventsPath);

            if (generateMouse) {
                var path = eventsPath + "/Mouse";
                EnsureFolder(path);
                GenerateEventProviderTriple(path, entityProviderName, "MouseDownUp");
                GenerateEventProviderTriple(path, entityProviderName, "MouseEnterExit");
                GenerateEventProviderTriple(path, entityProviderName, "MouseUpAsButton");
            }

            if (generatePhysics3D) {
                var path = eventsPath + "/Physics3D";
                EnsureFolder(path);
                GenerateEventProviderTriple(path, entityProviderName, "Collision3D");
                GenerateEventProviderTriple(path, entityProviderName, "Trigger3D");
                GenerateEventProviderTriple(path, entityProviderName, "ControllerColliderHit3D");
                GenerateContactColliderProviderPair(path, entityProviderName);
            }

            if (generatePhysics2D) {
                var path = eventsPath + "/Physics2D";
                EnsureFolder(path);
                GenerateEventProviderTriple(path, entityProviderName, "Collision2D");
                GenerateEventProviderTriple(path, entityProviderName, "Trigger2D");
            }

            if (generateAnimation) {
                var path = eventsPath + "/Animation";
                EnsureFolder(path);
                GenerateEventProviderTriple(path, entityProviderName, "AnimationEvent");
                GenerateStateMachineBehaviourGroup(path, entityProviderName);
            }

            if (generateGUI || generateTMP) {
                var path = eventsPath + "/GUI";
                EnsureFolder(path);

                if (generateGUI) {
                    GenerateEventProviderTriple(path, entityProviderName, "Click");
                    GenerateEventProviderTriple(path, entityProviderName, "PointerEnterExit");
                    GenerateEventProviderTriple(path, entityProviderName, "PointerUpDown");
                    GenerateEventProviderTriple(path, entityProviderName, "Drag");
                    GenerateEventProviderTriple(path, entityProviderName, "Drop");
                    GenerateEventProviderTriple(path, entityProviderName, "ScrollView");
                    GenerateEventProviderTriple(path, entityProviderName, "SliderChange");
                    GenerateEventProviderTriple(path, entityProviderName, "SubmitCancel");
                    GenerateEventProviderTriple(path, entityProviderName, "ButtonClick");
                }

                if (generateTMP) {
                    GenerateEventProviderTriple(path, entityProviderName, "DropdownChange");
                    GenerateEventProviderTriple(path, entityProviderName, "InputChange");
                    GenerateEventProviderTriple(path, entityProviderName, "InputEnd");
                }
            }
        }

        void GenerateEventProviderTriple(string dir, string entityProviderName, string name) {
            var className = prefix + name;

            WriteFile(dir, className,
                CreateSealedProviderFile(className, $"{name}Provider<{worldTypeName}>"));

            WriteFile(dir, className + "EntityGID",
                CreateSealedProviderFile(className + "EntityGID", $"{name}EntityGIDProvider<{worldTypeName}>"));

            WriteFile(dir, className + "EntityRef",
                CreateSealedProviderFile(className + "EntityRef", $"{name}EntityRefProvider<{worldTypeName}, {entityProviderName}>"));
        }

        void GenerateStateMachineBehaviourGroup(string dir, string entityProviderName) {
            var className = prefix + "StateMachine";

            WriteFile(dir, className,
                CreateSealedProviderFile(className, $"StaticEcsStateMachineBehaviour<{worldTypeName}>"));

            WriteFile(dir, className + "Entity",
                CreateSealedProviderFile(className + "Entity", $"StaticEcsEntityStateMachineBehaviour<{worldTypeName}>"));

            WriteFile(dir, className + "Linker",
                CreateSealedProviderFile(className + "Linker", $"StaticEcsStateMachineBehaviourLinker<{worldTypeName}, {entityProviderName}>"));
        }

        void GenerateContactColliderProviderPair(string dir, string entityProviderName) {
            var className = prefix + "ContactCollider";

            WriteFile(dir, className + "EntityGID",
                CreateSealedProviderFile(className + "EntityGID", $"ContactColliderGIDProvider<{worldTypeName}>"));

            WriteFile(dir, className + "EntityRef",
                CreateSealedProviderFile(className + "EntityRef", $"ContactColliderRefProvider<{worldTypeName}, {entityProviderName}>"));
        }

        string CreateSealedProviderFile(string className, string baseType) {
            var sb = new StringBuilder();
            var pad = string.IsNullOrEmpty(nameSpace) ? "" : "    ";
            sb.AppendLine("using FFS.Libraries.StaticEcs.Unity;");
            sb.AppendLine();
            sb.AppendLine($"namespace {nameSpace} {{", !string.IsNullOrEmpty(nameSpace));
            sb.AppendLine($"{pad}public sealed class {className} : {baseType} {{ }}");
            sb.AppendLine("}", !string.IsNullOrEmpty(nameSpace));
            return sb.ToString();
        }

        void EnsureFolder(string path) {
            if (!AssetDatabase.IsValidFolder(path)) {
                var parent = Path.GetDirectoryName(path);
                var folder = Path.GetFileName(path);
                AssetDatabase.CreateFolder(parent, folder);
            }
        }

        void WriteFile(string dir, string className, string text) {
            var fileName = $"{dir}/{className}.cs";
            try {
                File.WriteAllText(AssetDatabase.GenerateUniqueAssetPath(fileName), text);
            }
            catch (Exception ex) {
                Debug.LogError(ex.Message);
            }
        }

        static string ShortName(string name) {
            if (string.IsNullOrEmpty(name)) return name;
            var idx = name.LastIndexOf('.');
            return idx >= 0 ? name.Substring(idx + 1) : name;
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

        public string CreateEntityProviderTemplate(string className) {
            var sb = new StringBuilder();
            var pad = string.IsNullOrEmpty(nameSpace) ? "" : "    ";
            sb.AppendLine("using FFS.Libraries.StaticEcs.Unity;");
            sb.AppendLine();
            sb.AppendLine($"namespace {nameSpace} {{", !string.IsNullOrEmpty(nameSpace));
            sb.AppendLine($"{pad}public class {className} : StaticEcsEntityProvider<{worldTypeName}> {{ }}");
            sb.AppendLine("}", !string.IsNullOrEmpty(nameSpace));
            return sb.ToString();
        }

        public string CreateEventProviderTemplate(string className) {
            var sb = new StringBuilder();
            var pad = string.IsNullOrEmpty(nameSpace) ? "" : "    ";
            sb.AppendLine("using FFS.Libraries.StaticEcs.Unity;");
            sb.AppendLine();
            sb.AppendLine($"namespace {nameSpace} {{", !string.IsNullOrEmpty(nameSpace));
            sb.AppendLine($"{pad}public class {className} : StaticEcsEventProvider<{worldTypeName}> {{ }}");
            sb.AppendLine("}", !string.IsNullOrEmpty(nameSpace));
            return sb.ToString();
        }

        public string CreateEntityProviderEditorTemplate(string providerName, string editorName) {
            var sb = new StringBuilder();
            var pad = string.IsNullOrEmpty(nameSpace) ? "" : "    ";
            sb.AppendLine("using FFS.Libraries.StaticEcs.Unity.Editor;");
            sb.AppendLine("using UnityEditor;");
            if (!string.IsNullOrEmpty(nameSpace)) {
                sb.AppendLine($"using {nameSpace};");
            }
            sb.AppendLine();
            sb.AppendLine($"namespace {nameSpace}.Editor {{", !string.IsNullOrEmpty(nameSpace));
            sb.AppendLine($"{pad}[CustomEditor(typeof({providerName})), CanEditMultipleObjects]");
            sb.AppendLine($"{pad}public class {editorName} : StaticEcsEntityProviderEditor<{worldTypeName}, {providerName}> {{ }}");
            sb.AppendLine("}", !string.IsNullOrEmpty(nameSpace));
            return sb.ToString();
        }

        public string CreateEventProviderEditorTemplate(string providerName, string editorName) {
            var sb = new StringBuilder();
            var pad = string.IsNullOrEmpty(nameSpace) ? "" : "    ";
            sb.AppendLine("using FFS.Libraries.StaticEcs.Unity.Editor;");
            sb.AppendLine("using UnityEditor;");
            if (!string.IsNullOrEmpty(nameSpace)) {
                sb.AppendLine($"using {nameSpace};");
            }
            sb.AppendLine();
            sb.AppendLine($"namespace {nameSpace}.Editor {{", !string.IsNullOrEmpty(nameSpace));
            sb.AppendLine($"{pad}[CustomEditor(typeof({providerName})), CanEditMultipleObjects]");
            sb.AppendLine($"{pad}public class {editorName} : StaticEcsEvenTEntityProviderEditor<{worldTypeName}, {providerName}> {{ }}");
            sb.AppendLine("}", !string.IsNullOrEmpty(nameSpace));
            return sb.ToString();
        }

        public string CreateEcsViewTemplate(string entityProviderName, string eventProviderName, string viewName) {
            var sb = new StringBuilder();
            var pad = string.IsNullOrEmpty(nameSpace) ? "" : "    ";
            sb.AppendLine("using FFS.Libraries.StaticEcs.Unity.Editor;");
            sb.AppendLine("using UnityEditor;");
            if (!string.IsNullOrEmpty(nameSpace)) {
                sb.AppendLine($"using {nameSpace};");
            }
            sb.AppendLine();
            sb.AppendLine($"namespace {nameSpace}.Editor {{", !string.IsNullOrEmpty(nameSpace));
            sb.AppendLine($"{pad}public class {viewName} : StaticEcsView<{worldTypeName}, {entityProviderName}, {eventProviderName}> {{");
            sb.AppendLine($"{pad}    [MenuItem(\"{menuItemPath}\")]");
            sb.AppendLine($"{pad}    public static void OpenWindow() {{");
            sb.AppendLine($"{pad}        var window = GetWindow<{viewName}>();");
            sb.AppendLine($"{pad}        window.Show();");
            sb.AppendLine($"{pad}        window.Focus();");
            sb.AppendLine($"{pad}    }}");
            sb.AppendLine($"{pad}}}");
            sb.AppendLine("}", !string.IsNullOrEmpty(nameSpace));
            return sb.ToString();
        }
    }
}
