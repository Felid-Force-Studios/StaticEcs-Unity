using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace FFS.Libraries.StaticEcs.Unity.Editor {

    public class StartupTemplateWindow : EditorWindow {
        string path;
        
        string nameSpace;
        
        string startupName = "Startup";
        string worldTypeName;
        string worldAliasName;
        string worldEditorName;
        
        bool updateSystems = true;
        string updateSystemsTypeName;
        string updateSystemsAliasName;
        
        bool fixedUpdateSystems = false;
        string fixedUpdateSystemsTypeName;
        string fixedUpdateSystemsAliasName;
        
        bool lateUpdateSystems = false;
        string lateUpdateSystemsTypeName;
        string lateUpdateSystemsAliasName;
        
        bool autoRegister = true;
        bool debugWorld = true;

        [MenuItem("Assets/Create/Static ECS/Startup", false, -210)]
        static void ShowWindow() {
            var window = GetWindow<StartupTemplateWindow>(true, "Startup template");
            window.minSize = new Vector2(300, 200);
            window.path = AssetPath();
            window.nameSpace = EditorSettings.projectGenerationRootNamespace.Trim();
        }

        void OnGUI() {
            if (!string.IsNullOrEmpty(startupName) && GUILayout.Button("Create Startup", Ui.ButtonStyleYellow)) {
                CreateFile();
                Close();
            }
            
            EditorGUILayout.Space(10);
            nameSpace = EditorGUILayout.TextField("Namespace", nameSpace);
            
            EditorGUILayout.Space(10);
            startupName = EditorGUILayout.TextField("Class Name", startupName);
            if (string.IsNullOrEmpty(startupName)) {
                startupName = "Startup";
            }
            worldTypeName = EditorGUILayout.TextField("World Type Name", worldTypeName);
            if (string.IsNullOrEmpty(worldTypeName)) {
                worldTypeName = "WT";
            }
            worldAliasName = EditorGUILayout.TextField("World Alias Name", worldAliasName);
            if (string.IsNullOrEmpty(worldAliasName)) {
                worldAliasName = "W";
            }
            worldEditorName = EditorGUILayout.TextField("World Editor Name", worldEditorName);
 
            
            EditorGUILayout.Space(10);
            autoRegister = EditorGUILayout.Toggle("Auto registration", autoRegister);
            debugWorld = EditorGUILayout.Toggle("World debug", debugWorld);
            
            EditorGUILayout.Space(5);
            updateSystems = EditorGUILayout.Toggle("Update systems", updateSystems);
            if (updateSystems) {
                EditorGUI.indentLevel++;
                updateSystemsTypeName = EditorGUILayout.TextField("Systems Type Name", updateSystemsTypeName);
                if (string.IsNullOrEmpty(updateSystemsTypeName)) {
                    updateSystemsTypeName = "UpdateSystemsType";
                }
                updateSystemsAliasName = EditorGUILayout.TextField("Systems Alias Name", updateSystemsAliasName);
                if (string.IsNullOrEmpty(updateSystemsAliasName)) {
                    updateSystemsAliasName = "UpdateSystems";
                }
                EditorGUI.indentLevel--;
            }
            
            EditorGUILayout.Space(5);
            fixedUpdateSystems = EditorGUILayout.Toggle("Fixed Update systems", fixedUpdateSystems);
            if (fixedUpdateSystems) {
                EditorGUI.indentLevel++;
                fixedUpdateSystemsTypeName = EditorGUILayout.TextField("Systems Type Name", fixedUpdateSystemsTypeName);
                if (string.IsNullOrEmpty(fixedUpdateSystemsTypeName)) {
                    fixedUpdateSystemsTypeName = "FixedUpdateSystemsType";
                }
                fixedUpdateSystemsAliasName = EditorGUILayout.TextField("Systems Alias Name", fixedUpdateSystemsAliasName);
                if (string.IsNullOrEmpty(fixedUpdateSystemsAliasName)) {
                    fixedUpdateSystemsAliasName = "FixedUpdateSystems";
                }
                EditorGUI.indentLevel--;
            }
            
            EditorGUILayout.Space(5);
            lateUpdateSystems = EditorGUILayout.Toggle("Late Update systems", lateUpdateSystems);
            if (lateUpdateSystems) {
                EditorGUI.indentLevel++;
                lateUpdateSystemsTypeName = EditorGUILayout.TextField("Systems Type Name", lateUpdateSystemsTypeName);
                if (string.IsNullOrEmpty(lateUpdateSystemsTypeName)) {
                    lateUpdateSystemsTypeName = "LateUpdateSystemsType";
                }
                lateUpdateSystemsAliasName = EditorGUILayout.TextField("Systems Alias Name", lateUpdateSystemsAliasName);
                if (string.IsNullOrEmpty(lateUpdateSystemsAliasName)) {
                    lateUpdateSystemsAliasName = "LateUpdateSystems";
                }
                EditorGUI.indentLevel--;
            }
        }
        
        public void CreateFile() {
            var fileName = $"{path}/{startupName}.cs";
            var text = CreateTemplate();
            try {
                File.WriteAllText(AssetDatabase.GenerateUniqueAssetPath(fileName), text);
            }
            catch (Exception ex) {
                Debug.LogError(ex.Message);
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

        public string CreateTemplate() {
            var sb = new StringBuilder();
            var pad = string.IsNullOrEmpty(nameSpace) ? "" : "    ";
            sb.AppendLine("using System;");
            sb.AppendLine("using FFS.Libraries.StaticEcs;");
            sb.AppendLine("using FFS.Libraries.StaticEcs.Unity;", debugWorld || autoRegister);
            sb.AppendLine("using UnityEngine;");
            sb.AppendLine();
            sb.AppendLine($"[StaticEcsEditorName(\"{worldEditorName}\")]", !string.IsNullOrEmpty(worldEditorName));
            sb.AppendLine($"public struct {worldTypeName} : IWorldType {{ }}");
            sb.AppendLine($"public abstract class {worldAliasName} : World<{worldTypeName}> {{ }}");
            sb.AppendLine($"public abstract class {worldAliasName}Events : World<{worldTypeName}>.Events {{ }}");
            sb.AppendLine($"public struct {updateSystemsTypeName} : ISystemsType {{ }}", updateSystems);
            sb.AppendLine($"public struct {fixedUpdateSystemsTypeName} : ISystemsType {{ }}", fixedUpdateSystems);
            sb.AppendLine($"public struct {lateUpdateSystemsTypeName} : ISystemsType {{ }}", lateUpdateSystems);
            sb.AppendLine($"public abstract class {updateSystemsAliasName} : {worldAliasName}.Systems<{updateSystemsTypeName}> {{ }}", updateSystems);
            sb.AppendLine($"public abstract class {fixedUpdateSystemsAliasName} : {worldAliasName}.Systems<{fixedUpdateSystemsTypeName}> {{ }} ", fixedUpdateSystems);
            sb.AppendLine($"public abstract class {lateUpdateSystemsAliasName} : {worldAliasName}.Systems<{lateUpdateSystemsTypeName}> {{ }}", lateUpdateSystems);
            sb.AppendLine();
            sb.AppendLine();
            sb.AppendLine($"namespace {nameSpace} {{", !string.IsNullOrEmpty(nameSpace));
            sb.AppendLine($"{pad}#if ENABLE_IL2CPP");
            sb.AppendLine($"{pad}[Il2CppSetOption(Option.NullChecks, false)]");
            sb.AppendLine($"{pad}[Il2CppSetOption(Option.ArrayBoundsChecks, false)]");
            sb.AppendLine($"{pad}#endif");
            sb.AppendLine($"{pad}[Serializable]");
            sb.AppendLine($"{pad}public class {worldAliasName}SceneData {{");
            sb.AppendLine($"{pad}    // TODO Write your context fields");
            sb.AppendLine($"{pad}}}");
            sb.AppendLine();
            sb.AppendLine($"{pad}public class {startupName} : MonoBehaviour {{");
            sb.AppendLine($"{pad}    public {worldAliasName}SceneData sceneData;");
            sb.AppendLine();
            sb.AppendLine($"{pad}    private void Start() {{");
            sb.AppendLine($"{pad}        // ============================================ MAIN INITIALIZATION ======================================================");
            sb.AppendLine($"{pad}        {worldAliasName}.Create(WorldConfig.Default());");
            sb.AppendLine();
            sb.AppendLine($"{pad}        // {worldAliasName}.RegisterComponentType<YourComponentType>();");
            sb.AppendLine($"{pad}        // {worldAliasName}.RegisterTagType<YourTagType>();");
            sb.AppendLine($"{pad}        // {worldAliasName}Events.RegisterEventType<YourEventType>();");
            sb.AppendLine();
            sb.AppendLine($"{pad}        EcsDebug<{worldTypeName}>.AddWorld();", debugWorld);
            sb.AppendLine($"{pad}        AutoRegister<{worldTypeName}>.Apply();", autoRegister);
            sb.AppendLine();
            sb.AppendLine($"{pad}        {worldAliasName}.Initialize();");
            sb.AppendLine();
            sb.AppendLine($"{pad}        // ============================================ CONTEXT INITIALIZATION ====================================================");
            sb.AppendLine($"{pad}        {worldAliasName}.Context<{worldAliasName}SceneData>.Set(sceneData);");
            sb.AppendLine();
            if (updateSystems) {
                sb.AppendLine($"{pad}        // ============================================ MAIN SYSTEMS INITIALIZATION ===============================================");
                sb.AppendLine($"{pad}        {updateSystemsAliasName}.Create();");
                sb.AppendLine($"{pad}        // {updateSystemsAliasName}.AddCallOnce(new YourInitOrAndDestroySystem());");
                sb.AppendLine($"{pad}        // {updateSystemsAliasName}.AddUpdate(new YourUpdateSystem1(), new YourUpdateSystem2(), new YourUpdateSystem3());");
                sb.AppendLine();
                sb.AppendLine($"{pad}        {updateSystemsAliasName}.Initialize();");
                sb.AppendLine($"{pad}        EcsDebug<{worldTypeName}>.AddSystem<{updateSystemsTypeName}>();");
                sb.AppendLine();  
            }

            if (fixedUpdateSystems) {
                sb.AppendLine($"{pad}        // ============================================ FIXED SYSTEMS INITIALIZATION ==============================================");
                sb.AppendLine($"{pad}        {fixedUpdateSystemsAliasName}.Create();");
                sb.AppendLine($"{pad}        // {fixedUpdateSystemsAliasName}.AddCallOnce(new YourInitOrAndDestroySystem());");
                sb.AppendLine($"{pad}        // {fixedUpdateSystemsAliasName}.AddUpdate(new YourFixedUpdateSystem1(), new YourFixedUpdateSystem2(), new YourFixedUpdateSystem3());");
                sb.AppendLine();
                sb.AppendLine($"{pad}        {fixedUpdateSystemsAliasName}.Initialize();");
                sb.AppendLine($"{pad}        EcsDebug<{worldTypeName}>.AddSystem<{fixedUpdateSystemsTypeName}>();");
                sb.AppendLine(); 
            }

            if (lateUpdateSystems) {
                sb.AppendLine($"{pad}        // ============================================ LATE SYSTEMS INITIALIZATION ==============================================");
                sb.AppendLine($"{pad}        {lateUpdateSystemsAliasName}.Create();");
                sb.AppendLine($"{pad}        // {lateUpdateSystemsAliasName}.AddCallOnce(new YourInitOrAndDestroySystem());");
                sb.AppendLine($"{pad}        // {lateUpdateSystemsAliasName}.AddUpdate(new YourLateUpdateSystem1(), new YourLateUpdateSystem2(), new YourLateUpdateSystem3());");
                sb.AppendLine();
                sb.AppendLine($"{pad}        {lateUpdateSystemsAliasName}.Initialize();");
                sb.AppendLine($"{pad}        EcsDebug<{worldTypeName}>.AddSystem<{lateUpdateSystemsTypeName}>();");
            }
            sb.AppendLine($"{pad}    }}");
            sb.AppendLine();
            
            if (updateSystems) {
                sb.AppendLine($"{pad}    private void Update() {{");
                sb.AppendLine($"{pad}        {updateSystemsAliasName}.Update();");
                sb.AppendLine($"{pad}    }}");
                sb.AppendLine();
            }

            if (fixedUpdateSystems) {
                sb.AppendLine($"{pad}    private void FixedUpdate() {{");
                sb.AppendLine($"{pad}        {fixedUpdateSystemsAliasName}.Update();");
                sb.AppendLine($"{pad}    }}");
                sb.AppendLine();
            }

            if (lateUpdateSystems) {
                sb.AppendLine($"{pad}    private void LateUpdate() {{");
                sb.AppendLine($"{pad}        {lateUpdateSystemsAliasName}.Update();");
                sb.AppendLine($"{pad}    }}");
                sb.AppendLine();
            }

            sb.AppendLine($"{pad}    private void OnDestroy() {{");
            sb.AppendLine($"{pad}        {updateSystemsAliasName}.Destroy();", updateSystems);
            sb.AppendLine($"{pad}        {fixedUpdateSystemsAliasName}.Destroy();",  fixedUpdateSystems);
            sb.AppendLine($"{pad}        {lateUpdateSystemsAliasName}.Destroy();", lateUpdateSystems);
            sb.AppendLine($"{pad}        {worldAliasName}.Destroy();");
            sb.AppendLine($"{pad}    }}");
            sb.AppendLine();
            sb.AppendLine($"{pad}}}");
            sb.AppendLine("}", !string.IsNullOrEmpty(nameSpace));

            return sb.ToString();
        }
    }
}