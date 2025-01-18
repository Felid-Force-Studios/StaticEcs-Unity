using System;
using System.IO;
using UnityEditor;
using UnityEditor.ProjectWindowCallback;
using UnityEngine;

namespace FFS.Libraries.StaticEcs.Unity.Editor {
    sealed class TemplateGenerator : ScriptableObject {

        [MenuItem("Assets/Create/Static ECS/Startup full", false, -202)]
        static void CreateStartupFull() {
            CreateAndRenameAsset($"{AssetPath()}/Startup.cs", (name) => {
                CreateTemplate(GetTemplateContent("StartupFull.cs.txt"), name);
            });
        }
        
        [MenuItem("Assets/Create/Static ECS/Startup simple", false, -201)]
        static void CreateStartupSimple() {
            CreateAndRenameAsset($"{AssetPath()}/Startup.cs", (name) => {
                CreateTemplate(GetTemplateContent("StartupSimple.cs.txt"), name);
            });
        }

        public static void CreateTemplate(string templateContent, string fileName) {
            if (string.IsNullOrEmpty(fileName)) {
                Debug.LogError("File name is null or empty!");
                return;
            }

            var nameSpace = EditorSettings.projectGenerationRootNamespace.Trim();
            if (string.IsNullOrEmpty(nameSpace)) {
                nameSpace = "Client";
            }

            templateContent = templateContent
                              .Replace("{{NAMESPACE}}", nameSpace)
                              .Replace("{{NAME}}", Path.GetFileNameWithoutExtension(fileName));

            try {
                File.WriteAllText(AssetDatabase.GenerateUniqueAssetPath(fileName), templateContent);
            }
            catch (Exception ex) {
                Debug.LogError(ex.Message);
                return;
            }

            AssetDatabase.Refresh();
        }

        static string GetTemplateContent(string templateName) {
            var pathHelper = CreateInstance<TemplateGenerator>();
            var path = Path.GetDirectoryName(AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(pathHelper)));
            DestroyImmediate(pathHelper);
            try {
                return File.ReadAllText(Path.Combine(path ?? "", templateName));
            }
            catch {
                return null;
            }
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

        static void CreateAndRenameAsset(string fileName, Action<string> onSuccess) {
            var action = CreateInstance<CustomEndNameAction>();
            action.Callback = onSuccess;
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0, action, fileName, null, null);
        }

        sealed class CustomEndNameAction : EndNameEditAction {
            [NonSerialized] public Action<string> Callback;

            public override void Action(int instanceId, string pathName, string resourceFile) {
                Callback?.Invoke(pathName);
            }
        }
    }
}