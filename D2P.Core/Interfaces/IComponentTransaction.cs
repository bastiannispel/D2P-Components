using System;
using System.Collections.Generic;

using D2P.Core.Repository;

namespace D2P.Core.Interfaces {
    public interface IComponentTransaction : IDisposable {
        void Save(IEnumerable<IComponentBase> components,RepositoryOptions? options = null);
        void Rollback();
    }
}
