#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace FFS.Libraries.StaticEcs.Unity {
    public static class Extensions {
        static readonly Dictionary<Type, string> _namesCache = new();
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
                    name = $"{typeName}<{constraints}>";
                }

                _namesCache[type] = name;
            }

            return name;
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