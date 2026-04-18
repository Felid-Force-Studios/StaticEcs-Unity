using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;

namespace FFS.Libraries.StaticEcs.Unity.Editor {
    internal static class CustomPropertyDrawerRegistry {
        private static Dictionary<Type, Type> _exact;
        private static List<(Type baseType, Type drawer)> _useForChildren;

        internal static bool HasDrawerFor(Type type) {
            if (type == null) return false;
            EnsureBuilt();
            if (_exact.ContainsKey(type)) return true;
            for (var i = 0; i < _useForChildren.Count; i++) {
                if (_useForChildren[i].baseType.IsAssignableFrom(type)) return true;
            }
            return false;
        }

        private static void EnsureBuilt() {
            if (_exact != null) return;
            _exact = new Dictionary<Type, Type>();
            _useForChildren = new List<(Type, Type)>();

            var attrType = typeof(CustomPropertyDrawer);
            var typeField = attrType.GetField("m_Type", BindingFlags.NonPublic | BindingFlags.Instance);
            var childField = attrType.GetField("m_UseForChildren", BindingFlags.NonPublic | BindingFlags.Instance);
            if (typeField == null) return;

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            for (var a = 0; a < assemblies.Length; a++) {
                Type[] types;
                try {
                    types = assemblies[a].GetTypes();
                } catch (ReflectionTypeLoadException ex) {
                    types = ex.Types;
                } catch {
                    continue;
                }

                for (var t = 0; t < types.Length; t++) {
                    var type = types[t];
                    if (type == null) continue;
                    object[] attrs;
                    try {
                        attrs = type.GetCustomAttributes(attrType, false);
                    } catch {
                        continue;
                    }
                    if (attrs == null || attrs.Length == 0) continue;

                    for (var i = 0; i < attrs.Length; i++) {
                        var attr = attrs[i];
                        var target = typeField.GetValue(attr) as Type;
                        if (target == null) continue;
                        var useForChildren = childField != null && childField.GetValue(attr) is bool b && b;
                        if (!_exact.ContainsKey(target)) {
                            _exact.Add(target, type);
                        }
                        if (useForChildren) {
                            _useForChildren.Add((target, type));
                        }
                    }
                }
            }
        }
    }
}
