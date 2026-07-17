using System.Linq;

using D2P.Core.Components;
using D2P.Core.Interfaces;

using Rhino.Geometry;

namespace D2P.GHPlugin {
    /// <summary>
    /// Runtime component used by Grasshopper for dynamically typed component instances.
    /// </summary>
    public class GHComponent : ComponentBase {

        public GHComponent() : base() { }
        protected GHComponent(IComponentBase other) : base(other) { }
        public GHComponent(IComponentType type,string name,Plane plane)
            : base(name,plane) {
            TypeId = type.TypeId;
            TypeName = type.TypeName;
            LayerColor = type.LayerColor;
            LabelSize = type.LabelSize;
            Label.Geometry.First().TextHeight = LabelSize;
        }

        public override IComponentBase Duplicate() {
            return new GHComponent(this);
        }
    }
}
