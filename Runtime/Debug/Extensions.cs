#if UNITY_EDITOR
using System;
using System.Collections.Generic;

namespace FFS.Libraries.StaticEcs.Unity {
    public static class Extensions {
        static readonly Dictionary<Type, string> _namesCache = new();

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

                    var genericIndex = type.Name.LastIndexOf("`", StringComparison.Ordinal);
                    var typeName = genericIndex == -1
                        ? type.Name
                        : type.Name.Substring(0, genericIndex);
                    name = $"{typeName}<{constraints}>";
                }

                _namesCache[type] = name;
            }

            return name;
        }
    }
}
#endif