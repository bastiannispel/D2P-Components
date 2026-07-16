using D2P.Core.Interfaces;
using D2P.Core.Repository;
using System.Collections.Generic;

namespace D2P.Core.Extensions {
    public static class ComponentExtensions {
        public static void Save(this IEnumerable<IComponentBase> components, IComponentRepository repository, RepositoryOptions? options = null)
        {
            repository.SaveMany(components, options);
        }
    }
}
