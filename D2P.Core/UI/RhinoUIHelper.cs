using System.Collections.Generic;
using System.Linq;

using D2P.Core.Interfaces;

using Rhino.Input.Custom;

namespace D2P.Core.UI {
    public static class RhinoUIHelper {
        public static T GetComponent<T>(IComponentRepository repository) where T : class, IComponentBase
        {
            if (!repository.Context.Registry.TryResolveType<T>(out string typeId))
                return null;

            var go = new GetObject();
            go.SetCommandPrompt($"{typeId} auswählen");
            go.GeometryFilter = Rhino.DocObjects.ObjectType.AnyObject;
            go.EnablePreSelect(false, true);
            go.DeselectAllBeforePostSelect = false;
            go.Get();

            if (go.CommandResult() != Rhino.Commands.Result.Success)
                return null;

            return repository.GetFromObject<T>(go.Object(0).Object());
        }

        public static IEnumerable<T> GetComponents<T>(IComponentRepository repository) where T : class, IComponentBase
        {
            if (!repository.Context.Registry.TryResolveType<T>(out string typeId))
                return null;

            var go = new GetObject();
            go.SetCommandPrompt($"{typeId}s auswählen");
            go.GeometryFilter = Rhino.DocObjects.ObjectType.AnyObject;
            go.EnablePreSelect(false, true);
            go.DeselectAllBeforePostSelect = false;
            go.GetMultiple(1, 0);

            if (go.CommandResult() != Rhino.Commands.Result.Success)
                return null;

            var objIds = go.Objects().Select(obj => obj.ObjectId);
            return repository.GetFromObjects<T>(objIds);
        }
    }
}
