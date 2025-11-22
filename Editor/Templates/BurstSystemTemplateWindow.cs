using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using FFS.Libraries.StaticEcs.Unity.Editor.Inspectors;
using UnityEditor;
using UnityEngine;

namespace FFS.Libraries.StaticEcs.Unity.Editor {
    
    public class BurstTemplateWindow : EditorWindow {
        List<EditorEntityDataMeta> components = new(8);
        string path;
        Vector2 scroll;
        
        string nameSpace;
        string systemName = "BurstSystem";
        string worldName;
        string worldTypeName;
        Type worldType;
        
        bool parallel;

        [MenuItem("Assets/Create/Static ECS/Burst System", false, -190)]
        static void ShowWindow() {
            var window = GetWindow<BurstTemplateWindow>(true, "Burst system template");
            window.minSize = new Vector2(500, 200);
            window.path = AssetPath();
            window.nameSpace = EditorSettings.projectGenerationRootNamespace.Trim();
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
                if (worldType != null && components.Count > 0 && systemName.Length > 0 && GUILayout.Button("Create System", Ui.ButtonStyleYellow)) {
                    CreateFile();
                    Close();
                }
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(10);
            nameSpace = EditorGUILayout.TextField("Namespace", nameSpace);
            systemName = EditorGUILayout.TextField("System name", systemName);

            EditorGUILayout.Space(10);
            parallel = EditorGUILayout.Toggle("Parallel", parallel);

            EditorGUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();
            {
                var hasAll = components.Count == 8;
                using (Ui.EnabledScopeVal(!hasAll && GUI.enabled)) {
                    if (Ui.PlusDropDownButton && !hasAll) {
                        var menu = new GenericMenu();
                        foreach (var component in MetaData.Components) {
                            var has = false;
                            foreach (var actual in components) {
                                if (actual.Type == component.Type) {
                                    has = true;
                                    break;
                                }
                            }


                            if (has) continue;

                            menu.AddItem(new GUIContent(component.FullName), false, v => components.Add((EditorEntityDataMeta) v), component);
                        }

                        menu.ShowAsContext();
                    }
                }

                EditorGUILayout.LabelField(" Components:", Ui.HeaderStyleTheme);
            }
            EditorGUILayout.EndHorizontal();

            for (var i = 0; i < components.Count; i++) {
                var meta = components[i];
                EditorGUILayout.BeginHorizontal(GUI.skin.box);
                EditorGUILayout.SelectableLabel(meta.Name, Ui.LabelStyleThemeBold, Ui.WidthLine(460));
                if (Ui.MinusButton) {
                    components.RemoveAt(i);
                    break;
                }

                EditorGUILayout.EndHorizontal();
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
        
        public void CreateFile() {
            var fileName = $"{path}/{systemName}.cs";
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

        public string GenericComponentsString() {
            var result = "<";
            result += string.Join(", ", components.Select(meta => meta.Name));
            result += ">";
            return result;
        }

        public string InvokeOneComponentsString() {
            return string.Join(", ", components.Select(meta => $"ref {meta.Name} {FirstCharToLower(meta.Name)}"));
        }

        public string InvokeOneCallComponentsString() {
            return string.Join(", ", components.Select(meta => $"ref {FirstCharToLower(meta.Name)}s[dIdx]"));
        }

        public string InvokeBlockComponentsString() {
            return string.Join(", ", components.Select(meta => $"{meta.Name}* {FirstCharToLower(meta.Name)}s"));
        }

        public static string FirstCharToLower(string s) {
            return char.ToLowerInvariant(s[0]) + s.Substring(1);
        }

        public void RunnerAppendDataMapping(string pad, StringBuilder sb) {
            for (var i = 0; i < components.Count; i++) {
                var meta = components[i];
                sb.AppendLine($"{pad}                var {FirstCharToLower(meta.Name)}s = blocks->d{i+1};");
            }
        }
        
        public string RunnerInvokeOneComponentsString() {
            return string.Join(", ", components.Select(meta => $"{FirstCharToLower(meta.Name)}s"));
        }

        public string CreateTemplate() {
            var sb = new StringBuilder();
            var pad = string.IsNullOrEmpty(nameSpace) ? "" : "    ";
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Runtime.CompilerServices;");
            sb.AppendLine("using FFS.Libraries.StaticEcs;");
            sb.AppendLine("using Unity.Burst;");
            sb.AppendLine("using Unity.Mathematics;");
            sb.AppendLine("using static System.Runtime.CompilerServices.MethodImplOptions;");
            sb.AppendLine("#if ENABLE_IL2CPP");
            sb.AppendLine("using Unity.IL2CPP.CompilerServices;");
            sb.AppendLine("#endif");
            sb.AppendLine();
            sb.AppendLine($"using QueryBlocks = FFS.Libraries.StaticEcs.QueryBlocks{GenericComponentsString()};");
            sb.AppendLine();
            sb.AppendLine($"namespace {nameSpace} {{", !string.IsNullOrEmpty(nameSpace));
            sb.AppendLine($"{pad}#if ENABLE_IL2CPP");
            sb.AppendLine($"{pad}[Il2CppSetOption(Option.NullChecks, false)]");
            sb.AppendLine($"{pad}[Il2CppSetOption(Option.ArrayBoundsChecks, false)]");
            sb.AppendLine($"{pad}#endif");
            sb.AppendLine($"{pad}public unsafe struct {systemName} : IUpdateSystem {{");
            sb.AppendLine($"{pad}    private const EntityStatusType entityStatusType = EntityStatusType.Enabled;");
            sb.AppendLine($"{pad}    private const ComponentStatus componentStatus = ComponentStatus.Enabled;");
            sb.AppendLine($"{pad}    private static readonly ushort[] clusters = Array.Empty<ushort>(); // Empty == all clusters");
            sb.AppendLine($"{pad}    private static readonly WithNothing with = default;");
            sb.AppendLine();
            sb.AppendLine($"{pad}    private static bool parallel = false;", !parallel);
            sb.AppendLine($"{pad}    private static bool parallel = true;", parallel);
            sb.AppendLine($"{pad}    private static uint minEntitiesPerThread; // default - 64");
            sb.AppendLine($"{pad}    private static uint workersLimit;         // default - max threads");
            sb.AppendLine();
            sb.AppendLine($"{pad}    [MethodImpl(AggressiveInlining)]");
            sb.AppendLine($"{pad}    private void BeforeUpdate() {{ }}");
            sb.AppendLine();
            sb.AppendLine($"{pad}    [MethodImpl(AggressiveInlining)]");
            sb.AppendLine($"{pad}    private void AfterUpdate() {{ }}");
            sb.AppendLine();
            sb.AppendLine($"{pad}    [MethodImpl(AggressiveInlining)]");
            sb.AppendLine($"{pad}    private void InvokeOne({InvokeOneComponentsString()}, int worker) {{");
            sb.AppendLine($"{pad}        // TODO Write a function for processing a single entity here");
            sb.AppendLine($"{pad}    }}");
            sb.AppendLine();
            sb.AppendLine($"{pad}    [MethodImpl(AggressiveInlining)]");
            sb.AppendLine($"{pad}    private void InvokeBlock({InvokeBlockComponentsString()}, int worker, uint dataOffest) {{");
            sb.AppendLine($"{pad}        // Here, custom optimization of the entire entity block is possible. (SIMD, unroll, etc.)");
            sb.AppendLine($"{pad}        for (var i = 0; i < Const.ENTITIES_IN_BLOCK; i++) {{");
            sb.AppendLine($"{pad}            var dIdx = i + dataOffest;");
            sb.AppendLine($"{pad}            InvokeOne({InvokeOneCallComponentsString()}, worker);");
            sb.AppendLine($"{pad}        }}");
            sb.AppendLine($"{pad}    }}");
            sb.AppendLine();
            sb.AppendLine();
            sb.AppendLine($"{pad}    #region GENERATED");
            sb.AppendLine($"{pad}    [MethodImpl(AggressiveInlining)]");
            sb.AppendLine($"{pad}    public void Update() {{");
            sb.AppendLine($"{pad}        BeforeUpdate();");
            sb.AppendLine($"{pad}        QueryBurstFunctionRunner<WT>.Prepare(W.HandleClustersRange(clusters), with, entityStatusType, componentStatus, out QueryBlocks* blocks, out var blocksCount);");
            sb.AppendLine($"{pad}        if (parallel) {{");
            sb.AppendLine($"{pad}            var runner = W.Context.Value.GetOrCreate<ParallelRunner>();");
            sb.AppendLine($"{pad}            runner.Blocks = blocks;");
            sb.AppendLine($"{pad}            runner.System = this;");
            sb.AppendLine($"{pad}            ParallelRunner<WT>.Run(runner, (uint) blocksCount, math.max(minEntitiesPerThread / Const.BLOCK_IN_CHUNK, 2), 0);");
            sb.AppendLine($"{pad}            this = runner.System;");
            sb.AppendLine($"{pad}            runner.Blocks = default;");
            sb.AppendLine($"{pad}        }} else {{");
            sb.AppendLine($"{pad}            Runner.Run(ref this, blocks, 0, (uint) blocksCount, 0);");
            sb.AppendLine($"{pad}        }}");
            sb.AppendLine($"{pad}        AfterUpdate();");
            sb.AppendLine($"{pad}    }}");
            sb.AppendLine();
            sb.AppendLine($"{pad}    #if ENABLE_IL2CPP");
            sb.AppendLine($"{pad}    [Il2CppSetOption(Option.NullChecks, false)]");
            sb.AppendLine($"{pad}    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]");
            sb.AppendLine($"{pad}    #endif");
            sb.AppendLine($"{pad}    private class ParallelRunner : AbstractParallelTask {{");
            sb.AppendLine($"{pad}        internal QueryBlocks* Blocks;");
            sb.AppendLine($"{pad}        internal {systemName} System;");
            sb.AppendLine($"{pad}        public override void Run(uint from, uint to, int worker) => Runner.Run(ref System, Blocks, from, to, worker);");
            sb.AppendLine($"{pad}    }}");
            sb.AppendLine();
            sb.AppendLine($"{pad}    #if ENABLE_IL2CPP");
            sb.AppendLine($"{pad}    [Il2CppSetOption(Option.NullChecks, false)]");
            sb.AppendLine($"{pad}    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]");
            sb.AppendLine($"{pad}    #endif");
            sb.AppendLine($"{pad}    [BurstCompile]");
            sb.AppendLine($"{pad}    private static class Runner {{");
            sb.AppendLine($"{pad}        [BurstCompile]");
            sb.AppendLine($"{pad}        [MethodImpl(AggressiveInlining)]");
            sb.AppendLine($"{pad}        internal static void Run(ref {systemName} system, QueryBlocks* blocks, uint from, uint to, int worker) {{");
            sb.AppendLine($"{pad}            blocks += from;");
            sb.AppendLine($"{pad}            for (var i = from; i < to; i++, blocks++) {{");
            RunnerAppendDataMapping(pad, sb);
            sb.AppendLine($"{pad}                ref var entities = ref blocks->EntitiesMask;");
            sb.AppendLine($"{pad}                var dataOffset = (blocks->BlockIdx << Const.BLOCK_IN_CHUNK_SHIFT) & Const.DATA_ENTITY_MASK;");
            sb.AppendLine($"{pad}                if (entities == ulong.MaxValue) {{");
            sb.AppendLine($"{pad}                    system.InvokeBlock({RunnerInvokeOneComponentsString()}, worker, dataOffset);");
            sb.AppendLine($"{pad}                    continue;");
            sb.AppendLine($"{pad}                }}");
            sb.AppendLine($"{pad}                var idx = math.tzcnt(entities);");
            sb.AppendLine($"{pad}                var end = Const.BITS_PER_LONG - math.lzcnt(entities);");
            sb.AppendLine($"{pad}                var total = math.countbits(entities);");
            sb.AppendLine($"{pad}                if (total >= (end - idx) >> 1) {{");
            sb.AppendLine($"{pad}                    for (; idx < end; idx++) {{");
            sb.AppendLine($"{pad}                        if ((entities & (1UL << idx)) != 0UL) {{");
            sb.AppendLine($"{pad}                            var dIdx = idx + dataOffset;");
            sb.AppendLine($"{pad}                            system.InvokeOne({InvokeOneCallComponentsString()}, worker);");
            sb.AppendLine($"{pad}                        }}");
            sb.AppendLine($"{pad}                    }}");
            sb.AppendLine($"{pad}                }} else {{");
            sb.AppendLine($"{pad}                    do {{");
            sb.AppendLine($"{pad}                        var dIdx = idx + dataOffset;");
            sb.AppendLine($"{pad}                        system.InvokeOne({InvokeOneCallComponentsString()}, worker);");
            sb.AppendLine($"{pad}                        entities &= entities - 1UL;");
            sb.AppendLine($"{pad}                        idx = math.tzcnt(entities);");
            sb.AppendLine($"{pad}                    }} while (entities != 0UL);");
            sb.AppendLine($"{pad}                }}");
            sb.AppendLine($"{pad}            }}");
            sb.AppendLine($"{pad}        }}");
            sb.AppendLine($"{pad}    }}");
            sb.AppendLine($"{pad}    #endregion");
            sb.AppendLine($"{pad}}}");
            sb.AppendLine("}", !string.IsNullOrEmpty(nameSpace));
            sb.AppendLine();

            return sb.ToString();
        }
    }
}