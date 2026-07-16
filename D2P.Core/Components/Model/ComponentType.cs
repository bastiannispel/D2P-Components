using System.Drawing;

using D2P.Core.Interfaces;

using Rhino;
using Rhino.DocObjects;

namespace D2P.Core.Components {
    public class ComponentType : IComponentType {
        public string TypeId { get; set; }
        public string TypeName { get; set; }
        public double LabelSize { get; set; }
        public Color LayerColor { get; set; }

        public ComponentType(string typeID, string typeName, double? labelSize = null, Color? layerColor = null)
        {
            TypeId = typeID;
            TypeName = typeName;
            LabelSize = labelSize ?? RhinoDoc.ActiveDoc?.DimStyles.Current.TextHeight ?? 1.0;
            LayerColor = layerColor ?? Color.Black;
        }

        public ComponentType(RhinoDoc doc, Layer layer)
        {
            TypeId = Utility.Layers.GetComponentTypeID(layer);
            TypeName = Utility.Layers.GetComponentTypeName(layer);
            LabelSize = Utility.Layers.GetComponentTypeLabelSize(doc, layer);
            LayerColor = layer.Color;
        }
    }
}
