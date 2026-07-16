using System;
using System.Reflection;

using D2P.Core.Interfaces;

using Rhino;

namespace D2P.Core.Components {
    public sealed class ModelContext {
        public RhinoDoc Document { get; }
        public ComponentRegistry Registry { get; }
        public IComponentRepository Repository { get; }

        public ModelContext(RhinoDoc document)
            : this(document, ctx => new RhinoComponentRepository(ctx))
        {
        }

        public ModelContext(RhinoDoc document, Func<ModelContext, IComponentRepository> createRepository)
        {
            Document = document ?? throw new ArgumentNullException(nameof(document));
            if (createRepository == null)
                throw new ArgumentNullException(nameof(createRepository));

            Registry = new ComponentRegistry();
            var componentAttribute = typeof(Component).GetCustomAttribute<ComponentTypeAttribute>(inherit: false);
            if (componentAttribute != null)
                Registry.Register(new TypeRegistration(typeof(Component), componentAttribute));
            else
                Registry.Register<Component>("COMP");
            Repository = createRepository(this);
        }
    }
}
