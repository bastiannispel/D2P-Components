using System;
using System.Collections.Generic;

using D2P.Core.Components;
using D2P.Core.Repository;

using Rhino.DocObjects;

namespace D2P.Core.Interfaces {
    public interface IComponentRepository {
        ModelContext Context { get; }

        void Attach(IComponentBase component);

        T? GetFromObject<T>(RhinoObject obj) where T : class, IComponentBase;
        IEnumerable<T> GetFromObjects<T>(IEnumerable<Guid> objectIds) where T : class, IComponentBase;
        public IEnumerable<IComponentBase> GetByType(string typeId, FilterOptions? filter = null);
        IEnumerable<T> GetByType<T>(FilterOptions? filter = null) where T : class, IComponentBase;
        IEnumerable<T> GetByName<T>(string nameFilter) where T : class, IComponentBase;
        T? GetByGroup<T>(int groupIndex) where T : class, IComponentBase;

        T? GetParent<T>(IComponentBase component, out int parentsFound) where T : class, IComponentBase;
        IEnumerable<T> GetChildren<T>(IComponentBase component, IEnumerable<string>? filterTypes = null) where T : class, IComponentBase;
        IEnumerable<T> GetJoints<T>(IComponentBase component, IEnumerable<string>? filterTypes = null) where T : class, IComponentBase;
        IEnumerable<T> GetConnected<T>(IComponentBase component, IEnumerable<string>? typeFilter = null) where T : class, IComponentBase;
        IEnumerable<IComponentType> GetComponentTypes();

        void Save(IComponentBase component, RepositoryOptions? options = null);
        void SaveMany(IEnumerable<IComponentBase> components, RepositoryOptions? options = null);
        void Delete(IComponentBase component);

        IComponentTransaction BeginTransaction();
    }
}
