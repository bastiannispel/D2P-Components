using System.Drawing;

using D2P.Core.Interfaces;

using Rhino.Geometry;

namespace D2P.Core.Components {
    [ComponentType("COMP","Base Component")]
    public sealed class Component : ComponentBase {
        public override Color LayerColor { get; set; } = Color.Brown;
        public override double LabelSize { get; set; } = 5;

        public Component() : base() { }
        public Component(string name,Plane plane) : base(name,plane) { }
        private Component(IComponentBase other) : base(other) { }

        public override IComponentBase Duplicate() {
            return new Component(this);
        }
    }
}
