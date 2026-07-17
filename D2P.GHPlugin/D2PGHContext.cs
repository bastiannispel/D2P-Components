using System.Collections.Generic;

using D2P.Core.Components;

using Rhino;

namespace D2P.GHPlugin {
    public static class D2PGHContext {
        static readonly Dictionary<uint,ModelContext> _contexts = new Dictionary<uint,ModelContext>();

        public static ModelContext Create(RhinoDoc doc) {
            var serialNumber = doc.RuntimeSerialNumber;
            if (!_contexts.TryGetValue(serialNumber,out var context)) {
                context = new ModelContext(doc);
                _contexts[serialNumber] = context;
            }
            return context;
        }

        public static void RegisterType(RhinoDoc doc,string typeId) {
            Create(doc).Registry.Register<GHComponent>(typeId);
        }
    }
}
