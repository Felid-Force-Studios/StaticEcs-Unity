#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using Object = UnityEngine.Object;

namespace FFS.Libraries.StaticEcs.Unity {
    public static class Extensions {
        static readonly Dictionary<Type, bool> _compactViewTypeCache = new();
        static readonly Dictionary<Type, Color> _colorsCache = new();
        static readonly Dictionary<Type, string> _namesCache = new();
        static readonly Dictionary<Type, bool> _ignoredEventsCache = new();
        static readonly Dictionary<Type, string> _fullNamesCache = new();

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

        public static bool EditorTypeColor(this Type type, out Color color) {
            if (!_colorsCache.TryGetValue(type, out color)) {
                var authAttrType = typeof(StaticEcsEditorColorAttribute);
                foreach (var customAttribute in type.GetCustomAttributes()) {
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
        
        public static bool EditorTypeIsCompactView(this Type type) {
            if (!_compactViewTypeCache.TryGetValue(type, out var compact)) {
                var underlying = Nullable.GetUnderlyingType(type);
                if (underlying != null) {
                    type = underlying;
                }
                
                compact = type == typeof(byte) 
                         || type == typeof(sbyte)
                         || type == typeof(short)
                         || type == typeof(ushort)
                         || type == typeof(int)
                         || type == typeof(uint)
                         || type == typeof(long)
                         || type == typeof(ulong)
                         || type == typeof(float)
                         || type == typeof(double)
                         || type == typeof(bool)
                         || type == typeof(char)
                         || type == typeof(string)
                         || type == typeof(EntityGID)
                         || type == typeof(AnimationCurve)
                         || type == typeof(Color)
                         || type == typeof(Color32)
                         || type == typeof(LayerMask)
                         || type == typeof(Quaternion)
                         || type == typeof(Vector2)
                         || type == typeof(Vector2Int)
                         || type == typeof(Vector3)
                         || type == typeof(Vector3Int)
                         || type == typeof(Vector4)
                         || type.IsEnum
                         || typeof(Object) == type || type!.IsSubclassOf(typeof(Object))
                         ;

                if (!compact) {
                    var authAttrType = typeof(StaticEcsEditorCompactViewAttribute);
                    foreach (var customAttribute in type.GetCustomAttributes()) {
                        if (customAttribute.GetType().Namespace + customAttribute.GetType().FullName == authAttrType.Namespace + authAttrType.FullName) {
                            compact = true;
                            break;
                        }
                    }
                }

                _compactViewTypeCache[type] = compact;
            }

            return compact;
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

        public static string ToPascalCaseNoUnderscore(this string name) {
            if (string.IsNullOrEmpty(name))
                return name;

            int start = name[0] == '_' ? 1 : 0;
            if (start >= name.Length)
                return name;

            var chars = name.ToCharArray();
            chars[start] = char.ToUpperInvariant(chars[start]);

            return start == 0
                ? new string(chars)
                : new string(chars, start, chars.Length - start);
        }

        public static object CreateDefault(this Type type) {
            if (type.IsValueType) {
                return Activator.CreateInstance(type);
            }

            var ctor = type.GetConstructor(Type.EmptyTypes);
            return ctor != null ? Activator.CreateInstance(type) : null;
        }

        public static T CreateDefault<T>(this Type type) {
            if (type.IsValueType)
                return default;

            var ctor = type.GetConstructor(Type.EmptyTypes);
            if (ctor != null)
                return (T) Activator.CreateInstance(type);

            return default;
        }

        public static string EditorFullTypeName(this Type type) {
            if (!_fullNamesCache.TryGetValue(type, out var name)) {
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