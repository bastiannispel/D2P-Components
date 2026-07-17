using System;

namespace D2P.Core.Components {
    public sealed class TypeRegistration {
        public Type ClrType { get; }
        public string TypeId { get; }
        public string TypeName { get; }
        public double LabelSize { get; }

        public TypeRegistration(Type clrType,ComponentTypeAttribute attribute) {
            ClrType = clrType;
            TypeId = attribute.TypeId;
            TypeName = attribute.Name;
            LabelSize = attribute.LabelSize;
        }

        public TypeRegistration(Type clrType,string typeId) {
            ClrType = clrType;
            TypeId = typeId;
            TypeName = string.Empty;
            LabelSize = 0;
        }
    }
}
