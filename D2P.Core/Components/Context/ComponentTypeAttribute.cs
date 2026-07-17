using System;

namespace D2P.Core.Components {
    [AttributeUsage(AttributeTargets.Class,Inherited = false,AllowMultiple = false)]
    public sealed class ComponentTypeAttribute : Attribute {
        public new string TypeId { get; }
        public string Name { get; }

        public ComponentTypeAttribute(string typeId,string name) {
            TypeId = typeId;
            Name = name ?? string.Empty;
        }
    }
}
