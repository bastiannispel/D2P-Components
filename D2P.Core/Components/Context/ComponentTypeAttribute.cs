using System;

namespace D2P.Core.Components {
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class ComponentTypeAttribute : Attribute {
        public new string TypeId { get; }
        public string Name { get; }
        public double LabelSize { get; set; } = 1.0;

        public ComponentTypeAttribute(string typeId, string name)
        {
            TypeId = typeId;
            Name = name ?? string.Empty;
        }
    }
}
