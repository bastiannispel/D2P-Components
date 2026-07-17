using System.Drawing;

using Rhino;
using Rhino.DocObjects;

namespace D2P.Core.Components {
    public static class Settings {
        public static string RootLayerName { get; set; } = "D2P";
        public static Color RootLayerColor { get; set; } = Color.FromArgb(220,75,58);

        public static string DimensionStyleName => "D2P";
        public static string AngularDimensionStyleName => "D2P_ANGULAR";

        public static DimensionStyle GetDimensionStyle(RhinoDoc doc) {
            return doc.DimStyles.FindName(DimensionStyleName) ?? doc.DimStyles.Current;
        }

        public static DimensionStyle GetAngularDimensionStyle(RhinoDoc doc) {
            return doc.DimStyles.FindName(AngularDimensionStyleName) ?? doc.DimStyles.Current;
        }

        public static double GetTolerance(RhinoDoc doc) => doc.ModelAbsoluteTolerance;
        public static double GetAngleTolerance(RhinoDoc doc) => doc.ModelAngleToleranceDegrees;

        public static char TypeDelimiter { get; set; } = ':';
        public static char LayerDelimiter { get; set; } = '_';
        public static char NameDelimiter { get; set; } = '.';
        public static char LayerDescriptionDelimiter { get; set; } = '-';
        public static char LayerNameDelimiter { get; set; } = ':';
        public static char CountDelimiter { get; set; } = '#';
        public static char JointDelimiter { get; set; } = 'x';
    }
}
