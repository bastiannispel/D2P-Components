using System;
using System.Collections.Generic;
using System.Linq;

using D2P.Core.Interfaces;

using Rhino;

namespace D2P.Core.Utility {
    internal static class Group {
        internal static int AddGroup(RhinoDoc doc) {
            return doc.Groups.Add();
        }

        internal static int AddObjectsToGroup(RhinoDoc doc,IEnumerable<Guid> objIDs,int grpIdx) {
            if (!objIDs.Any() || grpIdx < 0) return 0;
            if (!doc.Groups.AddToGroup(grpIdx,objIDs)) return 0;
            return objIDs.Count();
        }

        internal static bool RemoveObjectFromAllGroups(RhinoDoc doc,Guid objectID) {
            var rhinoObj = doc.Objects.Find(objectID);
            if (rhinoObj.GroupCount < 1) return false;
            var attr = rhinoObj.Attributes;
            attr.RemoveFromAllGroups();
            return doc.Objects.ModifyAttributes(rhinoObj,attr,true);
        }

        internal static bool GetGroupIndex(RhinoDoc doc,IComponentBase component,out int grpIdx) {
            grpIdx = -1;
            if (component.ID.Equals(Guid.Empty))
                return false;
            var rhObj = doc.Objects.FindId(component.ID);
            if (rhObj?.GroupCount != 1)
                return false;
            grpIdx = rhObj.GetGroupList()[0];
            return true;
        }
    }
}
