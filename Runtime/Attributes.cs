using System;

namespace FFS.Libraries.StaticEcs.Unity {
    [AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class)]
    public sealed class StaticEcsEditorNameAttribute : Attribute {
        public readonly string Name;
        public readonly string FullName;
        
        public StaticEcsEditorNameAttribute(string name, string fullName = null) {
            Name = name;
            FullName = fullName;
        }
    }

    [AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class)]
    public sealed class StaticEcsEditorCompactViewAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class)]
    public class StaticEcsEditorColorAttribute : Attribute {
        public readonly float R;
        public readonly float G;
        public readonly float B;

        public StaticEcsEditorColorAttribute(float r, float g, float b) {
            R = r;
            G = g;
            B = b;
        }

        public StaticEcsEditorColorAttribute(int r, int g, int b) {
            R = r / 255f;
            G = g / 255f;
            B = b / 255f;
        }

        public StaticEcsEditorColorAttribute(string hex) {
            if (string.IsNullOrEmpty(hex)) {
                throw new ArgumentException("Hex string cannot be null or empty");
            }

            if (hex.StartsWith("#")) {
                hex = hex[1..];
            }

            if (hex.Length != 6 && hex.Length != 8) {
                throw new ArgumentException("Hex string must be 6 or 8 characters long");
            }

            R = Convert.ToByte(hex.Substring(0, 2), 16) / 255f;
            G = Convert.ToByte(hex.Substring(2, 2), 16) / 255f;
            B = Convert.ToByte(hex.Substring(4, 2), 16) / 255f;
        }
    }
    
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public sealed class StaticEcsEditorTableValueAttribute : Attribute {
        public readonly float ColumnWidth;

        public StaticEcsEditorTableValueAttribute(float columnWidth = 0f) {
            ColumnWidth = columnWidth;
        }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public sealed class StaticEcsEditorShowAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Field)]
    public sealed class StaticEcsEditorHideAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class)]
    public sealed class StaticEcsIgnoreEventAttribute : Attribute { }
}