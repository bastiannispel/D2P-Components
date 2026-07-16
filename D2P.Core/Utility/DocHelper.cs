using D2P.Core.Exceptions;
using D2P.Core.Interfaces;

using Rhino;

namespace D2P.Core.Utility {
    internal static class DocHelper {
        internal static RhinoDoc Require(IComponentBase component)
        {
            if (component?.Context?.Document == null)
                throw new ModelContextNotSetException();
            return component.Context.Document;
        }

        internal static RhinoDoc Require(IMember member)
        {
            if (member?.Component == null)
                throw new ModelContextNotSetException();
            return Require(member.Component);
        }
    }
}
