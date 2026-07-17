using System;

namespace D2P.Core.Exceptions {
    public sealed class ComponentTypeAttributeMissingException : Exception {
        public Type ComponentType { get; }

        public ComponentTypeAttributeMissingException(Type componentType)
            : base($"Component type '{componentType.FullName}' must be decorated with [ComponentType].") {
            ComponentType = componentType;
        }
    }
}
