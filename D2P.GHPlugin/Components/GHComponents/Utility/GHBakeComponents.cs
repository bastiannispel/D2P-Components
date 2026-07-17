using System;
using System.Collections.Generic;
using System.Windows.Forms;

using D2P.Core.Interfaces;
using D2P.Core.Repository;
using D2P.Core.Utility;

using Grasshopper.Kernel;

using Rhino;
using Rhino.DocObjects;

namespace D2P.GHPlugin.Components.GHComponents.Utility {
    public class GHBakeComponents : GHComponentPreview {
        bool _replaceExisting = true;
        bool _purgeEmptyLayers = true;

        /// <summary>
        /// Initializes a new instance of the IO_BakeComponents class.
        /// </summary>
        public GHBakeComponents()
          : base("BakeComponents","BakeComp",
              "Bake component-instances to the Rhino document",
              "D2P","04 Utility") { }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager) {
            pManager.AddGenericParameter("Components","C","The in-memory representation of the component instances",GH_ParamAccess.list);
            pManager.AddBooleanParameter("Bake","B","Adds all input component-instances to the current Rhino document",GH_ParamAccess.item,false);
            pManager[0].DataMapping = GH_DataMapping.Flatten;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager) { }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA) {
            bool bake = false;
            DA.GetDataList(0,_components);
            DA.GetData(1,ref bake);

            if (bake) BakeComponents();
        }

        protected override void AppendAdditionalComponentMenuItems(ToolStripDropDown menu) {
            base.AppendAdditionalComponentMenuItems(menu);
            Menu_AppendItem(menu,"Replace Existing",ClickOnReplaceExisting,true,_replaceExisting);
            Menu_AppendItem(menu,"Purge Empty Layers",ClickOnPurgeLayers,true,_purgeEmptyLayers);
        }

        private void ClickOnReplaceExisting(object sender,EventArgs e) {
            _replaceExisting = !_replaceExisting;
        }

        private void ClickOnPurgeLayers(object sender,EventArgs e) {
            _purgeEmptyLayers = !_purgeEmptyLayers;
        }

        public override void BakeGeometry(RhinoDoc doc,ObjectAttributes att,List<Guid> obj_ids) {
            BakeComponents();
        }

        void BakeComponents() {
            var doc = RhinoDoc.ActiveDoc;
            var context = _modelContext;
            var repository = context.Repository;
            RHDoc.UpdateComponentLayerColors(doc,_components);

            foreach (var component in _components) {
                if (component == null) continue;

                repository.Attach(component);
                var existingComponents = repository.GetByName<IComponentBase>(component.Name);
                foreach (var existing in existingComponents)
                    repository.Delete(existing);

                repository.Save(component,new RepositoryOptions {
                    DeleteExisting = _replaceExisting,
                    OnlyDirty = false
                });
            }

            if (_purgeEmptyLayers)
                RHDoc.Purge(doc);

            MarkStreamComponentsOutdated();
        }

        void MarkStreamComponentsOutdated() {
            var ghDoc = OnPingDocument();
            if (ghDoc == null) return;

            foreach (var obj in ghDoc.Objects) {
                if (obj is GHComponentStreamBase stream)
                    stream.MarkStreamOutdated();
            }
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon {
            get {
                //You can add image files to your project resources and access them like this:                
                return Properties.Resources.GH_BakeComponents;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid {
            get { return new Guid("B991E1EE-5839-474B-8F0C-338077BB7B5C"); }
        }
    }
}