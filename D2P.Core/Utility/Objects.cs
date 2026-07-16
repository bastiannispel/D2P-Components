using System;
using System.Collections.Generic;
using System.Linq;

using D2P.Core.Components;
using D2P.Core.Interfaces;

using Rhino;
using Rhino.DocObjects;
using Rhino.Geometry;

namespace D2P.Core.Utility {
    public static class Objects {
        public static ComponentType GetComponentTypeFromObject(RhinoDoc doc, RhinoObject rhObj)
        {
            var typeLayer = Layers.FindComponentTypeRootLayer(doc, rhObj);
            var typeID = rhObj.Name.Split(Settings.TypeDelimiter).FirstOrDefault();
            var typeName = Layers.GetComponentTypeName(typeLayer);
            var labelSize = Layers.GetComponentTypeLabelSize(doc, typeLayer);
            var layerColor = Layers.FindComponentTypeRootLayer(doc, rhObj)?.Color;
            return new ComponentType(typeID, typeName, labelSize, layerColor);
        }

        public static IEnumerable<RhinoObject> ObjectsByName(RhinoDoc doc, string name, ObjectType objectTypeFilter)
        {
            var objEnumSettings = Constants.ObjectEnumeratorSettings(name, objectTypeFilter);
            return doc.Objects.GetObjectList(objEnumSettings);
        }

        public static IEnumerable<RhinoObject> ObjectsByLayer(RhinoDoc doc, IComponentBase component, int layerIdx)
        {
            return ObjectsByGroup(doc, component.GroupIndex)
                .Where(rh => rh.Attributes.LayerIndex == layerIdx);
        }

        public static IEnumerable<RhinoObject> ObjectsByLayer(RhinoDoc doc, Layer layer)
        {
            return doc.Objects.FindByLayer(layer);
        }

        public static IEnumerable<T> GeometryByLayer<T>(RhinoDoc doc, IComponentBase component, int layerIdx) where T : GeometryBase
        {
            return ObjectsByLayer(doc, component, layerIdx)
                .Select(rhObj => rhObj.Geometry)
                .OfType<T>();
        }

        public static IEnumerable<GeometryBase> GeometryByLayer(RhinoDoc doc, IComponentBase component, int layerIdx)
        {
            return GeometryByLayer<GeometryBase>(doc, component, layerIdx);
        }

        public static IEnumerable<RhinoObject> ObjectsByGroup(RhinoDoc doc, int grpIdx)
        {
            return doc.Groups.GroupMembers(grpIdx);
        }

        public static int DeleteObjects(RhinoDoc doc, IComponentBase component, Layer layer, bool recursive = false)
        {
            if (component == null || layer == null)
                return 0;
            if (!Group.GetGroupIndex(doc, component, out int grpIdx))
                return 0;
            var rhObjects = ObjectsByLayer(doc, component, layer.Index);
            if (rhObjects == null)
                return 0;
            var objectIds = rhObjects.Select(rh => rh.Id);
            var nDeleted = doc.Objects.Delete(objectIds, true);

            if (recursive) {
                var sublayers = Layers.GetChildLayers(doc, layer);
                foreach (var sublayer in sublayers)
                    nDeleted += DeleteObjects(doc, component, sublayer, recursive);
            }

            return nDeleted;
        }

        public static int DeleteObjects(RhinoDoc doc, IMember member)
        {
            var layer = Layers.FindLayer(doc, member);
            return DeleteObjects(doc, member.Component, layer);
        }

        public static int DeleteComponent(RhinoDoc doc, IComponentBase component)
        {
            if (!Group.GetGroupIndex(doc, component, out int grpIdx)) return -1;
            var objectIds = ObjectsByGroup(doc, grpIdx).Select(rh => rh.Id);
            return doc.Objects.Delete(objectIds, true);
        }

        public static int DeleteComponents(RhinoDoc doc, IEnumerable<IComponentBase> components)
        {
            return components.Sum(comp => DeleteComponent(doc, comp));
        }

        public static void AddObjects(RhinoDoc doc, IMember member)
        {
            Layers.FindLayer(doc, member);
        }

        public static int GetObjectGroupID(RhinoDoc doc, Guid objectID)
        {
            var rhinoObject = doc.Objects.Find(objectID);
            if (rhinoObject.GroupCount < 1) { return -1; }
            if (rhinoObject.GroupCount > 1) { return -2; }
            return rhinoObject.GetGroupList()[0];
        }

        public static IEnumerable<int> GetObjectGroupIDs(RhinoDoc doc, Guid objectID)
        {
            var rhinoObject = doc.Objects.Find(objectID);
            if (rhinoObject == null || rhinoObject.GroupCount < 1) return new List<int>();
            return rhinoObject.GetGroupList();
        }

        public static IEnumerable<int> GetObjectGroupIDs(RhinoDoc doc, RhinoObject rhinoObject)
        {
            if (rhinoObject == null || rhinoObject.GroupCount < 1) return new List<int>();
            return rhinoObject.GetGroupList();
        }
    }
}
