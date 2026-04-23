using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace FFS.Libraries.StaticEcs.Unity.Editor {
    public static class MissingReferenceMigration {

        public static void FillMissing(Object target, List<ManagedReferenceMissingType> pool) {
            pool.Clear();
            var arr = SerializationUtility.GetManagedReferencesWithMissingTypes(target);
            if (arr == null) return;
            foreach (var m in arr) {
                pool.Add(m);
            }
        }

        public static bool TryMigrateSlot(Object target, ManagedReferenceMissingType missing, Type newType) {
            if (TryMigrateSlotBatch(target, missing, newType, out var assetPath)) {
                var targetGid = target != null ? GlobalObjectId.GetGlobalObjectIdSlow(target) : default;
                var selectionGids = CaptureSelectionGlobalIds();
                AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
                AssetDatabase.Refresh();

                EditorApplication.delayCall += () => RestoreSelection(targetGid, selectionGids);
                return true;
            }

            return false;
        }

        private static GlobalObjectId[] CaptureSelectionGlobalIds() {
            var objects = Selection.objects;
            if (objects == null || objects.Length == 0) return Array.Empty<GlobalObjectId>();
            var ids = new GlobalObjectId[objects.Length];
            GlobalObjectId.GetGlobalObjectIdsSlow(objects, ids);
            return ids;
        }

        private static void RestoreSelection(GlobalObjectId targetGid, GlobalObjectId[] selectionGids) {
            if (selectionGids != null && selectionGids.Length > 0) {
                var resolved = new Object[selectionGids.Length];
                GlobalObjectId.GlobalObjectIdentifiersToObjectsSlow(selectionGids, resolved);
                var alive = new List<Object>(resolved.Length);
                for (var i = 0; i < resolved.Length; i++) {
                    if (resolved[i] != null) alive.Add(resolved[i]);
                }
                if (alive.Count > 0) {
                    Selection.objects = alive.ToArray();
                    return;
                }
            }

            if (targetGid.assetGUID != default || targetGid.targetObjectId != 0) {
                var obj = GlobalObjectId.GlobalObjectIdentifierToObjectSlow(targetGid);
                if (obj != null) Selection.activeObject = obj;
            }
        }

        public static bool TryMigrateSlotBatch(Object target, ManagedReferenceMissingType missing, Type newType, out string assetPath) {
            assetPath = null;
            if (target == null || newType == null) return false;

            assetPath = ResolveAssetPath(target);
            if (string.IsNullOrEmpty(assetPath)) {
                Debug.LogWarning("[StaticEcs] Migration: cannot resolve asset path for target.");
                return false;
            }

            string content;
            try {
                content = File.ReadAllText(assetPath);
            }
            catch (Exception e) {
                Debug.LogWarning($"[StaticEcs] Migration: read of {assetPath} failed: {e.Message}");
                return false;
            }

            var localFileId = GlobalObjectId.GetGlobalObjectIdSlow(target).targetObjectId;
            if (!TryRewriteYamlTypeBlock(content, localFileId, missing.referenceId, newType, out var newContent)) {
                Debug.LogWarning($"[StaticEcs] Migration: rid {missing.referenceId} not found for target (fileID {localFileId}) in {assetPath}.");
                return false;
            }

            try {
                var bytes = new UTF8Encoding(false).GetBytes(newContent);
                using (var fs = new FileStream(assetPath, FileMode.Create, FileAccess.Write, FileShare.Read)) {
                    fs.Write(bytes, 0, bytes.Length);
                    fs.Flush(true);
                }
            }
            catch (Exception e) {
                Debug.LogWarning($"[StaticEcs] Migration: write of {assetPath} failed: {e.Message}");
                return false;
            }

            return true;
        }

        public static void FinishBatch(IEnumerable<string> affectedPaths) {
            if (affectedPaths?.Any(static p => !string.IsNullOrEmpty(p)) ?? false) {
                AssetDatabase.Refresh();
            }
        }

        private static string ResolveAssetPath(Object target) {
            var direct = AssetDatabase.GetAssetPath(target);
            if (!string.IsNullOrEmpty(direct)) return direct;

            if (target is Component comp) {
                var src = PrefabUtility.GetCorrespondingObjectFromSource(comp);
                if (src != null) {
                    var srcPath = AssetDatabase.GetAssetPath(src);
                    if (!string.IsNullOrEmpty(srcPath)) return srcPath;
                }

                var stage = UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage();
                if (stage != null && stage.IsPartOfPrefabContents(comp.gameObject)) {
                    return stage.assetPath;
                }

                var scene = comp.gameObject.scene;
                if (scene.IsValid() && !string.IsNullOrEmpty(scene.path)) return scene.path;
            }

            return null;
        }

        private static bool TryLocateObjectSection(string content, ulong localFileId, out int sectionStart, out int sectionEnd) {
            sectionStart = 0;
            sectionEnd = content.Length;
            if (localFileId == 0) return true;

            var anchor = "&" + localFileId;
            var idx = 0;
            while (idx < content.Length) {
                var found = content.IndexOf(anchor, idx, StringComparison.Ordinal);
                if (found < 0) return false;
                var lineStart = content.LastIndexOf('\n', found) + 1;
                var lineHead = content.Substring(lineStart, Math.Min(8, content.Length - lineStart));
                if (lineHead.StartsWith("--- !u!")) {
                    sectionStart = lineStart;
                    var next = content.IndexOf("\n--- !u!", found, StringComparison.Ordinal);
                    sectionEnd = next < 0 ? content.Length : next;
                    return true;
                }

                idx = found + anchor.Length;
            }

            return false;
        }

        private static bool TryRewriteYamlTypeBlock(string content, ulong localFileId, long rid, Type newType, out string result) {
            result = null;
            if (TryLocateObjectSection(content, localFileId, out var secStart, out var secEnd)) {
                var pattern = @"(\n[ \t]*-[ \t]+rid:[ \t]+" + rid + @"[ \t]*\r?\n[ \t]+type:[ \t]*\{)([^}\r\n]*)(\})";
                var section = content.Substring(secStart, secEnd - secStart);
                var match = Regex.Match(section, pattern);
                if (match.Success) {
                    BuildUnityYamlTypeIdentity(newType, out var cls, out var ns, out var asm);
                    var inner = $"class: {cls}, ns: {ns}, asm: {asm}";
                    var innerGroup = match.Groups[2];
                    var absIndex = secStart + innerGroup.Index;
                    result = content.Substring(0, absIndex) + inner + content.Substring(absIndex + innerGroup.Length);
                    return true;
                }
            }

            return false;
        }

        // Encodes type as Unity SerializeReference YAML stores it.
        // For nested constructed generics like World<W>.Multi<T> Unity writes:
        //   class: 'World`1/Multi`1[[W.FullName, W.Asm],[T.FullName, T.Asm]]'
        //   ns: <namespace of declaring open generic>
        //   asm: <assembly of declaring open generic>
        private static void BuildUnityYamlTypeIdentity(Type t, out string cls, out string ns, out string asm) {
            if (t.IsGenericType && t.DeclaringType != null && t.DeclaringType.IsGenericType) {
                var declaringOpen = t.DeclaringType.IsGenericTypeDefinition
                    ? t.DeclaringType
                    : t.DeclaringType.GetGenericTypeDefinition();
                ns = declaringOpen.Namespace ?? "";
                asm = declaringOpen.Assembly.GetName().Name;

                var args = t.GetGenericArguments();
                var sb = new StringBuilder();
                sb.Append('\'');
                sb.Append(declaringOpen.Name);
                sb.Append('/');
                sb.Append(t.Name);
                sb.Append("[[");
                for (var i = 0; i < args.Length; i++) {
                    if (i > 0) sb.Append("],[");
                    sb.Append(args[i].FullName ?? args[i].Name);
                    sb.Append(", ");
                    sb.Append(args[i].Assembly.GetName().Name);
                }
                sb.Append("]]");
                sb.Append('\'');
                cls = sb.ToString();
                return;
            }

            cls = t.Name;
            ns = t.Namespace ?? "";
            asm = t.Assembly.GetName().Name;
        }

        public static bool CleanAllMissing(Object target) {
            if (target == null) return false;
            var arr = SerializationUtility.GetManagedReferencesWithMissingTypes(target);
            if (arr == null || arr.Length == 0) return false;
            SerializationUtility.ClearAllManagedReferencesWithMissingTypes(target);
            EditorUtility.SetDirty(target);
            return true;
        }
    }
}