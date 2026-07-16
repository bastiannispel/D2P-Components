using System;

namespace D2P.Core.Exceptions {
    public sealed class ModelContextNotSetException : Exception {
        public ModelContextNotSetException()
            : base("ModelContext is not set. Create a ModelContext and call Repository.Attach before using document operations.")
        {
        }
    }
}
