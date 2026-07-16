using System;
using System.Collections.Generic;

using D2P.Core.Components;

using Rhino.Geometry;

namespace D2P.Core.Interfaces {
    public interface IComponentBase :
        IMemberCollection,
        IComponentType,
        IDocObject<IComponentBase>,
        IComparable {

        ModelContext Context { get; set; }
        bool IsDirty { get; }

        IMember<TextEntity> Label { get; }

        Guid ID { get; set; }
        int GroupIndex { get; set; }
        string Name { get; }
        string ShortName { get; set; }
        Plane Plane { get; set; }

        IEnumerable<GeometryBase> Geometry { get; }

        bool Transform(Transform xform);
        void Cache();
        void Commit(bool deleteExisting, bool onlyDirty);
        void MarkClean();
    }
}
