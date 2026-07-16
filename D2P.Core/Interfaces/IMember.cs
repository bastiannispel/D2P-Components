using System.Collections.Generic;

using Rhino.DocObjects;
using Rhino.Geometry;

namespace D2P.Core.Interfaces {

    public interface IMember : IMemberCollection, IDocObject<IMember> {
        IComponentBase Component { get; set; }
        bool IsDirty { get; }

        ILayerInfo LayerInfo { get; set; }
        IEnumerable<IBaseObject> BaseObjects { get; set; }

        IEnumerable<ObjectAttributes> Attributes { get; }
        IEnumerable<GeometryBase> Geometry { get; }

        void SetObject(IBaseObject baseObject);
        void SetObject(GeometryBase geometry);
        void SetObjects(IEnumerable<IBaseObject> baseObjects);
        void SetObjects(IEnumerable<GeometryBase> geometries);

        void Cache();
        void Commit(bool deleteExisting, bool onlyDirty);
        void MarkClean();
    }

    public interface IMember<T> : IMember where T : GeometryBase {
        new IEnumerable<IBaseObject<T>> BaseObjects { get; set; }

        new IEnumerable<T> Geometry { get; }

        void SetObject(IBaseObject<T> baseObject);
        void SetObject(T geometry);
        void SetObjects(IEnumerable<IBaseObject<T>> baseObjects);
        void SetObjects(IEnumerable<T> geometries);

        new IMember<T> Duplicate();
    }
}
