using System;

namespace FFS.Libraries.StaticEcs.Unity {
    [AttributeUsage(AttributeTargets.Struct)]
    public sealed class StaticEcsEditorNameAttribute : Attribute {
        public readonly string Name;
        public readonly string FullName;
        
        public StaticEcsEditorNameAttribute(string name, string fullName = null) {
            Name = name;
            FullName = fullName;
        }
    }
    
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public sealed class StaticEcsEditorTableValueAttribute : Attribute {
        public readonly float ColumnWidth;

        public StaticEcsEditorTableValueAttribute(float columnWidth = 0f) {
            ColumnWidth = columnWidth;
        }
    }
}