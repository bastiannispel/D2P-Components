using System;

namespace D2P.Core.Exceptions {
    public sealed class TypeIdAlreadyRegisteredException : Exception {
        public string TypeId { get; }
        public Type ExistingType { get; }
        public Type RequestedType { get; }

        public TypeIdAlreadyRegisteredException(string typeId,Type existingType,Type requestedType)
            : base($"TypeId '{typeId}' is already registered to {existingType.Name}. Cannot register {requestedType.Name}.") {
            TypeId = typeId;
            ExistingType = existingType;
            RequestedType = requestedType;
        }
    }
}
