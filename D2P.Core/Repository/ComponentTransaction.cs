using System;
using System.Collections.Generic;

using D2P.Core.Interfaces;
using D2P.Core.Repository;

using Rhino;

namespace D2P.Core.Components {
    public sealed class ComponentTransaction : IComponentTransaction {
        readonly RhinoComponentRepository _repository;
        readonly RhinoDoc _document;
        readonly uint _undoRecord;
        bool _completed;

        public ComponentTransaction(RhinoComponentRepository repository)
        {
            _repository = repository;
            _document = repository.Context.Document;
            _undoRecord = _document.BeginUndoRecord("D2P Save Components");
        }

        public void Save(IEnumerable<IComponentBase> components, RepositoryOptions? options = null)
        {
            _repository.SaveMany(components, options);
            Complete();
        }

        public void Rollback()
        {
            _completed = true;
        }

        public void Dispose()
        {
            if (!_completed)
                _document.EndUndoRecord(_undoRecord);
        }

        void Complete()
        {
            _completed = true;
            _document.EndUndoRecord(_undoRecord);
        }
    }
}
