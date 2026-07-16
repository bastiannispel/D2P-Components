using System.Collections.Generic;
using System.IO;
using System.Linq;

using D2P.Core.Components;
using D2P.Core.Interfaces;
using D2P.Core.Repository;

namespace D2P.Core.Utility {
    public static class IO {
        public static void ExportWithHeadless(ModelContext context, IEnumerable<IComponentBase> components, string directory, string fileName)
        {
            if (!components.Any())
                return;

            var currentDoc = context.Document;
            var exportContext = new ModelContext(RHDoc.CreateHeadless(currentDoc));
            CopyRegistry(context, exportContext);

            foreach (var component in components) {
                exportContext.Repository.Attach(component);
                exportContext.Repository.Save(component, new RepositoryOptions { DeleteExisting = false, OnlyDirty = false });
            }

            var filePath = Path.Combine(directory, fileName);
            if (!Path.HasExtension(filePath))
                filePath = Path.ChangeExtension(filePath, "3dm");

            RHDoc.Purge(exportContext.Document);
            exportContext.Document.Export(filePath);
        }

        public static void ExportWithHeadless(ModelContext context, IComponentBase component, string directory)
        {
            ExportWithHeadless(context, new[] { component }, directory, component.Name);
        }

        static void CopyRegistry(ModelContext source, ModelContext target)
        {
            foreach (var registration in source.Registry.Registrations)
                target.Registry.Register(registration);
        }
    }
}
