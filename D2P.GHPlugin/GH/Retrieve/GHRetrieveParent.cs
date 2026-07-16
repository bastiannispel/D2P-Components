using System;

using D2P.Core.Interfaces;

using Grasshopper.Kernel;

namespace D2P.GHPlugin.GH.Retrieve {
    public class GHRetrieveParent : GHComponentPreview {
        public GHRetrieveParent()
          : base("RetrieveParentComponent", "ParentMember",
              "Retrieves the parent component of a given input component. E.g. If the component-instance is named “aa.01”, “aa.02”, “aa.03”, ... the parent-instance is named “aa”",
              "D2P", "02 Retrieve")
        { }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Component", "C", "The in-memory representation of a component instance", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("ParentComponent", "C", "The in-memory representation of the component-parent instance", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            IComponentBase component = null;
            DA.GetData(0, ref component);

            if (component == null) {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Component is null !");
                return;
            }

            var repository = (component.Context ?? _modelContext).Repository;
            repository.Attach(component);
            var parent = repository.GetParent<IComponentBase>(component, out int parentsFound);
            if (parent == null) {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, $"ParentMember of component {component.Name} not found !");
                return;
            }
            if (parentsFound > 1)
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, $"Found {parentsFound} parents for component {component.Name} !");

            _components.Add(parent);
            DA.SetData(0, parent);
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.GH_RetrieveParent;

        public override Guid ComponentGuid => new Guid("92F56E9E-C2DA-4ADE-9D04-061A6F88C739");
    }
}
