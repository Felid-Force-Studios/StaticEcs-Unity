using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace FFS.Libraries.StaticEcs.Unity.Editor {

    public enum ConfigKind {
        Component, Tag, Event,
        Link, Links, Multi
    }

    public static class GuidTypeRegistry {

        public static IEnumerable<Type> GetLiveTypesForKind(ConfigKind kind) {
            var reg = StaticEcsTypeGuidRegistry.Active;
            var result = new List<Type>();
            for (var w = 0; w < reg.worlds.Count; w++) {
                var ws = reg.worlds[w];
                var entries = EntriesFor(ws, kind);
                for (var i = 0; i < entries.Count; i++) {
                    var t = ResolveEntry(kind, ws.worldTypeFullName, entries[i].current);
                    if (t != null && !result.Contains(t)) result.Add(t);
                }
            }

            return result;
        }

        // Searches all registry categories for a matching missing identity. Returns the first kind that
        // contains an entry whose current/history matches the identity. For wrapper kinds the className
        // is expected to be Unity's encoded wrapper string.
        public static bool TryFindAnyKindByMissingIdentity(string cls, string ns, string asm, out ConfigKind foundKind, out Type liveType) {
            foreach (ConfigKind k in Enum.GetValues(typeof(ConfigKind))) {
                if (TryFindCurrentByMissingIdentity(cls, ns, asm, k, out liveType)) {
                    foundKind = k;
                    return true;
                }
            }
            foundKind = default;
            liveType = null;
            return false;
        }

        public static bool TryFindCurrentByMissingIdentity(string cls, string ns, string asm, ConfigKind kind, out Type liveType) {
            liveType = null;
            var lookupCls = cls;
            var lookupNs = ns;
            var lookupAsm = asm;
            if (IsWrapperKind(kind) && TryParseWrapperInnerIdentity(cls, kind, out var innerCls, out var innerNs, out var innerAsm)) {
                lookupCls = innerCls;
                lookupNs = innerNs;
                lookupAsm = innerAsm;
            }

            var reg = StaticEcsTypeGuidRegistry.Active;
            for (var w = 0; w < reg.worlds.Count; w++) {
                var ws = reg.worlds[w];
                var entries = EntriesFor(ws, kind);
                for (var i = 0; i < entries.Count; i++) {
                    var e = entries[i];
                    if (IdMatches(e.current, lookupCls, lookupNs, lookupAsm) || HistoryHas(e.history, lookupCls, lookupNs, lookupAsm)) {
                        liveType = ResolveEntry(kind, ws.worldTypeFullName, e.current);
                        if (liveType != null) return true;
                    }
                }
            }

            return false;
        }

        // Parses Unity's encoded wrapper class string of form:
        //   "World`1/<Wrapper>`1[[<WTypeFullName>, <WTypeAsm>],[<InnerFullName>, <InnerAsm>]]"
        // and extracts (cls, ns, asm) of the inner generic argument.
        internal static bool TryParseWrapperInnerIdentity(string className, ConfigKind kind, out string innerCls, out string innerNs, out string innerAsm) {
            innerCls = null;
            innerNs = null;
            innerAsm = null;
            if (string.IsNullOrEmpty(className)) return false;

            var wrapperName = WrapperNestedName(kind);
            if (wrapperName == null) return false;

            const string outerPrefix = "World`1/";
            if (!className.StartsWith(outerPrefix, StringComparison.Ordinal)) return false;

            var afterOuter = outerPrefix.Length;
            if (string.CompareOrdinal(className, afterOuter, wrapperName, 0, wrapperName.Length) != 0) return false;
            var argsStart = afterOuter + wrapperName.Length;
            if (argsStart >= className.Length || className[argsStart] != '[') return false;
            if (argsStart + 1 >= className.Length || className[argsStart + 1] != '[') return false;

            // Skip first generic argument [WType, asm].
            var firstArgEnd = className.IndexOf(']', argsStart + 2);
            if (firstArgEnd < 0) return false;

            // Expect ",[" between args.
            var secondArgStart = firstArgEnd + 1;
            if (secondArgStart + 1 >= className.Length || className[secondArgStart] != ',' || className[secondArgStart + 1] != '[') return false;

            var secondArgInner = secondArgStart + 2;
            var secondArgEnd = className.IndexOf(']', secondArgInner);
            if (secondArgEnd < 0) return false;

            var arg = className.Substring(secondArgInner, secondArgEnd - secondArgInner);
            var commaIdx = arg.IndexOf(',');
            if (commaIdx < 0) return false;

            var fullName = arg.Substring(0, commaIdx).Trim();
            var asmName = arg.Substring(commaIdx + 1).Trim();
            if (fullName.Length == 0 || asmName.Length == 0) return false;

            var dotIdx = fullName.LastIndexOf('.');
            if (dotIdx < 0) {
                innerCls = fullName;
                innerNs = "";
            } else {
                innerCls = fullName.Substring(dotIdx + 1);
                innerNs = fullName.Substring(0, dotIdx);
            }
            innerAsm = asmName;
            return true;
        }

        private static Type ResolveEntry(ConfigKind kind, string worldTypeFullName, StaticEcsTypeGuidRegistry.TypeIdentity id) {
            var innerOrSimple = ResolveType(id);
            if (innerOrSimple == null) return null;
            if (!IsWrapperKind(kind)) return innerOrSimple;

            var worldType = FindTypeByFullName(worldTypeFullName);
            if (worldType == null) return null;
            var nested = typeof(World<>).GetNestedType(WrapperNestedName(kind));
            if (nested == null) return null;
            return nested.MakeGenericType(worldType, innerOrSimple);
        }

        private static Type ResolveType(StaticEcsTypeGuidRegistry.TypeIdentity id) {
            var full = string.IsNullOrEmpty(id.namespaceName) ? id.className : id.namespaceName + "." + id.className;
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies()) {
                if (asm.GetName().Name != id.assembly) continue;
                var t = asm.GetType(full);
                if (t != null) return t;
            }

            return null;
        }

        private static Type FindTypeByFullName(string fullName) {
            if (string.IsNullOrEmpty(fullName)) return null;
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies()) {
                var t = asm.GetType(fullName);
                if (t != null) return t;
            }

            return null;
        }

        private static bool IsWrapperKind(ConfigKind kind) => kind is ConfigKind.Link or ConfigKind.Links or ConfigKind.Multi;

        public static string PrettyTypeName(Type t) {
            if (t == null) return "<null>";
            if (t.IsGenericType) {
                var args = t.GetGenericArguments();
                if (args.Length > 0) return args[args.Length - 1].Name;
            }
            return t.Name;
        }

        // Formats a Unity-encoded missing-class name for display. For wrapper-encoded names
        // ("World`1/<Wrapper>`1[[<W>, <Wasm>],[<Inner>, <Innerasm>]]") returns "<Wrapper><<InnerSimpleName>>".
        // For ordinary class names returns the name with the back-tick arity suffix stripped.
        public static string PrettyMissingClassName(string className) {
            if (string.IsNullOrEmpty(className)) return className;
            const string outerPrefix = "World`1/";
            if (className.StartsWith(outerPrefix, StringComparison.Ordinal)) {
                var afterOuter = outerPrefix.Length;
                var argsStart = className.IndexOf('[', afterOuter);
                if (argsStart > afterOuter) {
                    var wrapperName = className.Substring(afterOuter, argsStart - afterOuter);
                    var tickIdx = wrapperName.IndexOf('`');
                    if (tickIdx > 0) wrapperName = wrapperName.Substring(0, tickIdx);

                    if (argsStart + 1 < className.Length && className[argsStart + 1] == '[') {
                        var firstArgEnd = className.IndexOf(']', argsStart + 2);
                        if (firstArgEnd > 0 && firstArgEnd + 2 < className.Length
                            && className[firstArgEnd + 1] == ',' && className[firstArgEnd + 2] == '[') {
                            var secondArgInner = firstArgEnd + 3;
                            var secondArgEnd = className.IndexOf(']', secondArgInner);
                            if (secondArgEnd > 0) {
                                var arg = className.Substring(secondArgInner, secondArgEnd - secondArgInner);
                                var commaIdx = arg.IndexOf(',');
                                var fullName = (commaIdx < 0 ? arg : arg.Substring(0, commaIdx)).Trim();
                                var dotIdx = fullName.LastIndexOf('.');
                                var inner = dotIdx < 0 ? fullName : fullName.Substring(dotIdx + 1);
                                return $"{wrapperName}<{inner}>";
                            }
                        }
                    }
                }
            }
            var tick = className.IndexOf('`');
            return tick > 0 ? className.Substring(0, tick) : className;
        }

        private static bool IdMatches(StaticEcsTypeGuidRegistry.TypeIdentity id, string cls, string ns, string asm) {
            return id.className == cls && id.namespaceName == (ns ?? "") && id.assembly == asm;
        }

        private static bool HistoryHas(List<StaticEcsTypeGuidRegistry.TypeIdentity> history, string cls, string ns, string asm) {
            if (history == null) return false;
            for (var i = 0; i < history.Count; i++) {
                if (IdMatches(history[i], cls, ns, asm)) {
                    return true;
                }
            }

            return false;
        }

        private static string WrapperNestedName(ConfigKind kind) => kind switch {
            ConfigKind.Link => "Link`1",
            ConfigKind.Links => "Links`1",
            ConfigKind.Multi => "Multi`1",
            _ => null,
        };

        private static List<StaticEcsTypeGuidRegistry.Entry> EntriesFor(StaticEcsTypeGuidRegistry.WorldSettings ws, ConfigKind k) => k switch {
            ConfigKind.Component => ws.components,
            ConfigKind.Tag => ws.tags,
            ConfigKind.Event => ws.events,
            ConfigKind.Link => ws.links,
            ConfigKind.Links => ws.linksList,
            ConfigKind.Multi => ws.multi,
            _ => ws.components,
        };
    }
}