#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace FFS.Libraries.StaticEcs.Unity {
    public static class Extensions {
        static readonly Dictionary<Type, bool> _compactViewTypeCache = new();
        static readonly Dictionary<Type, Color> _colorsCache = new();
        static readonly Dictionary<Type, string> _namesCache = new();
        static readonly Dictionary<Type, bool> _ignoredEventsCache = new();
        static readonly Dictionary<Type, string> _fullNamesCache = new();
        static readonly Dictionary<Type, GroupCacheEntry> _groupCache = new();

        private readonly struct GroupCacheEntry {
            public readonly bool Has;
            public readonly string Name;
            public readonly Color Color;
            public readonly bool HasColor;

            public GroupCacheEntry(bool has, string name, Color color, bool hasColor) {
                Has = has;
                Name = name;
                Color = color;
                HasColor = hasColor;
            }

            public static readonly GroupCacheEntry Empty = new(false, null, default, false);
        }

        private static bool IsWorldWrapperType(Type type, out string baseName) {
            baseName = null;
            if (!type.IsGenericType) return false;
            var dt = type.DeclaringType;
            if (dt == null || !dt.IsGenericType) return false;
            if (dt.GetGenericTypeDefinition().FullName != "FFS.Libraries.StaticEcs.World`1") return false;
            var n = type.Name;
            if (n.StartsWith("Link`")) {
                baseName = "Link";
                return true;
            }

            if (n.StartsWith("Links`")) {
                baseName = "Links";
                return true;
            }

            if (n.StartsWith("Multi`")) {
                baseName = "Multi";
                return true;
            }

            return false;
        }

        public static string EditorTypeName(this Type type) {
            if (!_namesCache.TryGetValue(type, out var name)) {
                var authAttrType = typeof(StaticEcsEditorNameAttribute);
                if (Attribute.IsDefined(type, authAttrType)) {
                    var attribute = (StaticEcsEditorNameAttribute) Attribute.GetCustomAttribute(type, authAttrType);
                    name = attribute.Name;
                    if (string.IsNullOrEmpty(name)) {
                        name = type.Name;
                    }
                } else if (!type.IsGenericType) {
                    name = type.Name;
                } else if (IsWorldWrapperType(type, out var baseName)) {
                    var args = type.GetGenericArguments();
                    var skip = type.DeclaringType!.GetGenericArguments().Length;
                    var constraints = "";
                    for (var i = skip; i < args.Length; i++) {
                        if (constraints.Length > 0) constraints += ", ";
                        constraints += args[i].EditorTypeName();
                    }

                    name = $"{baseName}<{constraints}>";
                } else {
                    var constraints = "";
                    foreach (var constraint in type.GetGenericArguments()) {
                        if (constraints.Length > 0) {
                            constraints += ", ";
                        }

                        constraints += constraint.EditorTypeName();
                    }

                    var genericIndex = type.Name.IndexOf("`", StringComparison.Ordinal);
                    var typeName = genericIndex == -1
                        ? type.Name
                        : type.Name.Substring(0, genericIndex);
                    if (typeName == "Nullable") {
                        name = $"{constraints}?";
                    } else {
                        name = $"{typeName}<{constraints}>";
                    }
                }

                _namesCache[type] = name;
            }

            return name;
        }

        public static bool IsSystemType(this Type type) {
            return type.Namespace == "FFS.Libraries.StaticEcs.Unity";
        }

        public static bool EditorTypeColor(this Type type, out Color color) {
            if (!_colorsCache.TryGetValue(type, out color)) {
                var lookupType = type;
                if (IsWorldWrapperType(type, out _)) {
                    var args = type.GetGenericArguments();
                    lookupType = args[args.Length - 1];
                }

                var authAttrType = typeof(StaticEcsEditorColorAttribute);
                foreach (var customAttribute in lookupType.GetCustomAttributes()) {
                    if (customAttribute.GetType().Namespace + customAttribute.GetType().FullName == authAttrType.Namespace + authAttrType.FullName) {
                        var fields = customAttribute.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                        if (fields.Length == 3) {
                            var floatCount = fields.Count(f => f.FieldType == typeof(float));
                            if (floatCount == 3) {
                                color = new Color((float) fields[0].GetValue(customAttribute), (float) fields[1].GetValue(customAttribute), (float) fields[2].GetValue(customAttribute), 1f);
                                _colorsCache[type] = color;
                                return true;
                            }
                        }
                    }
                }

                return false;
            }

            return true;
        }

        public static bool EditorTypeGroup(this Type type, out string name, out Color color, out bool hasColor) {
            if (_groupCache.TryGetValue(type, out var cached)) {
                name = cached.Name;
                color = cached.Color;
                hasColor = cached.HasColor;
                return cached.Has;
            }

            var lookupType = type;
            if (IsWorldWrapperType(type, out _)) {
                var args = type.GetGenericArguments();
                lookupType = args[args.Length - 1];
            }

            var attrType = typeof(StaticEcsEditorGroupAttribute);
            foreach (var customAttribute in lookupType.GetCustomAttributes()) {
                var ct = customAttribute.GetType();
                if (ct.Namespace + ct.FullName != attrType.Namespace + attrType.FullName) continue;

                var fields = ct.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                string n = null;
                bool hc = false;
                float r = 0f, g = 0f, b = 0f;
                foreach (var f in fields) {
                    switch (f.Name) {
                        case "Name":     n  = f.GetValue(customAttribute) as string; break;
                        case "HasColor": hc = (bool)  f.GetValue(customAttribute); break;
                        case "R":        r  = (float) f.GetValue(customAttribute); break;
                        case "G":        g  = (float) f.GetValue(customAttribute); break;
                        case "B":        b  = (float) f.GetValue(customAttribute); break;
                    }
                }

                if (string.IsNullOrEmpty(n)) {
                    break;
                }

                var entry = new GroupCacheEntry(true, n, new Color(r, g, b, 1f), hc);
                _groupCache[type] = entry;
                name = entry.Name;
                color = entry.Color;
                hasColor = entry.HasColor;
                return true;
            }

            _groupCache[type] = GroupCacheEntry.Empty;
            name = null;
            color = default;
            hasColor = false;
            return false;
        }

        public static bool IsIgnored(this Type type) {
            if (!_ignoredEventsCache.TryGetValue(type, out var ignored)) {
                var authAttrType = typeof(StaticEcsIgnoreEventAttribute);
                foreach (var customAttribute in type.GetCustomAttributes()) {
                    if (customAttribute.GetType().Namespace + customAttribute.GetType().FullName == authAttrType.Namespace + authAttrType.FullName) {
                        ignored = true;
                        break;
                    }
                }

                _compactViewTypeCache[type] = ignored;
            }

            return ignored;
        }

        public static string EditorFullTypeName(this Type type) {
            if (!_fullNamesCache.TryGetValue(type, out var name)) {
                if (IsWorldWrapperType(type, out _)) {
                    var args = type.GetGenericArguments();
                    var userType = args[args.Length - 1];
                    var userPath = userType.EditorFullTypeName();
                    var lastSlash = userPath.LastIndexOf('/');
                    var dir = lastSlash >= 0 ? userPath.Substring(0, lastSlash + 1) : "";
                    name = dir + type.EditorTypeName();
                    _fullNamesCache[type] = name;
                    return name;
                }

                var authAttrType = typeof(StaticEcsEditorNameAttribute);
                if (Attribute.IsDefined(type, authAttrType)) {
                    var attribute = (StaticEcsEditorNameAttribute) Attribute.GetCustomAttribute(type, authAttrType);
                    name = attribute.FullName;
                    if (string.IsNullOrEmpty(name)) {
                        name = type.FullName;
                    }
                } else {
                    var s = type.FullName!.Replace('+', '.');
                    if (!s.Contains("[")) {
                        name = type.FullName!.Replace('.', '/').Replace('+', '/');
                    } else {
                        s = Regex.Replace(s, @",[^[\]]*(?=[\[\]])", "");
                        s = Regex.Replace(s, @"`[^.\[]*[\[.]", "");
                        s = s.Replace("[[", "[").Replace("]]", "]");

                        var min = s.IndexOf('[');
                        var started = true;
                        var res = new StringBuilder();
                        for (var i = s.Length - 1; i >= 0; i--) {
                            var c = s[i];
                            if (!started && (c == '[' || c == ']')) {
                                started = true;
                                res.Append(c);
                                continue;
                            }

                            if (started && (c == '[' || c == '.')) {
                                started = false;
                            }

                            if (i <= min || started) {
                                res.Append(c);
                            }
                        }

                        name = new string(res.ToString().Reverse().ToArray()).Replace('.', '/');
                    }
                }

                _fullNamesCache[type] = name;
            }

            return name;
        }
    }
}
#endif