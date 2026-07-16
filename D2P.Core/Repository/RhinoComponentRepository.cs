using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

using D2P.Core.Interfaces;
using D2P.Core.Repository;

using Rhino.DocObjects;

namespace D2P.Core.Components {
    public sealed class RhinoComponentRepository : IComponentRepository {
        readonly ModelContext _context;

        public ModelContext Context => _context;

        public RhinoComponentRepository(ModelContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public void Attach(IComponentBase component)
        {
            component.Context = _context;
            foreach (var member in component.AllMembers)
                AttachMember(component, member);
        }

        public T? GetFromObject<T>(RhinoObject obj) where T : class, IComponentBase
        {
            var groupIds = Utility.Objects.GetObjectGroupIDs(_context.Document, obj.Id);
            foreach (var groupIndex in groupIds) {
                var component = GetByGroup<T>(groupIndex);
                if (component != null)
                    return component;
            }
            return null;
        }

        public IEnumerable<T> GetFromObjects<T>(IEnumerable<Guid> objectIds) where T : class, IComponentBase
        {
            var rhObjects = objectIds
                .Select(id => _context.Document.Objects.Find(id))
                .Where(obj => obj != null);
            return GetFromObjects<T>(rhObjects);
        }

        public IEnumerable<IComponentBase> GetByType(string typeId, FilterOptions? filter = null)
        {
            filter ??= new FilterOptions();
            var nameFilter = $"{typeId}{Settings.TypeDelimiter}*";
            var objEnumSettings = Utility.Constants.ObjectEnumeratorSettings(nameFilter);
            var reg = new Regex(filter.RegexPattern);
            var rhObjects = _context.Document.Objects
                .GetObjectList(objEnumSettings)
                .Where(rhObj => reg.IsMatch(rhObj.Name) == !filter.ReversePattern)
                .ToList();
            return GetFromObjects<IComponentBase>(rhObjects);
        }

        public IEnumerable<T> GetByType<T>(FilterOptions? filter = null) where T : class, IComponentBase
        {
            filter ??= new FilterOptions();
            if (!_context.Registry.TryResolveType<T>(out var typeId))
                return Enumerable.Empty<T>();

            var nameFilter = $"{typeId}{Settings.TypeDelimiter}*";
            var objEnumSettings = Utility.Constants.ObjectEnumeratorSettings(nameFilter);
            var reg = new Regex(filter.RegexPattern);
            var rhObjects = _context.Document.Objects
                .GetObjectList(objEnumSettings)
                .Where(rhObj => reg.IsMatch(rhObj.Name) == !filter.ReversePattern);
            return GetFromObjects<T>(rhObjects);
        }

        public IEnumerable<T> GetByName<T>(string nameFilter) where T : class, IComponentBase
        {
            var objEnumSettings = Utility.Constants.ObjectEnumeratorSettings(nameFilter);
            var rhObjects = _context.Document.Objects
                .GetObjectList(objEnumSettings)
                .Where(rhObj => rhObj.Attributes.GroupCount > 0);
            return GetFromObjects<T>(rhObjects);
        }

        public T? GetByGroup<T>(int groupIndex) where T : class, IComponentBase
        {
            return HydrateFromGroup<T>(groupIndex);
        }

        public T? GetParent<T>(IComponentBase component, out int parentsFound) where T : class, IComponentBase
        {
            return GetParent(component, out parentsFound) as T;
        }

        public IEnumerable<T> GetChildren<T>(IComponentBase component, IEnumerable<string>? filterTypes = null) where T : class, IComponentBase
        {
            return GetChildren(component, filterTypes).OfType<T>();
        }

        public IEnumerable<T> GetJoints<T>(IComponentBase component, IEnumerable<string>? filterTypes = null) where T : class, IComponentBase
        {
            return GetJoints(component, filterTypes).OfType<T>();
        }

        public IEnumerable<T> GetConnected<T>(IComponentBase component, IEnumerable<string>? typeFilter = null) where T : class, IComponentBase
        {
            return GetConnected(component, typeFilter).OfType<T>();
        }

        public IEnumerable<IComponentType> GetComponentTypes()
        {
            var componentTypeLayers = Utility.Layers.FindComponentTypeRootLayers(_context.Document);
            return componentTypeLayers.Select(layer => new ComponentType(_context.Document, layer));
        }

        public void Save(IComponentBase component, RepositoryOptions? options = null)
        {
            options ??= new RepositoryOptions();
            Attach(component);
            component.Commit(options.DeleteExisting, options.OnlyDirty);
            if (options.OnlyDirty)
                MarkClean(component);
        }

        public void SaveMany(IEnumerable<IComponentBase> components, RepositoryOptions? options = null)
        {
            options ??= new RepositoryOptions();
            foreach (var component in components)
                Save(component, options);
        }

        public void Delete(IComponentBase component)
        {
            Attach(component);
            component.Delete();
        }

        public IComponentTransaction BeginTransaction()
        {
            return new ComponentTransaction(this);
        }

        IEnumerable<T> GetFromObjects<T>(IEnumerable<RhinoObject> rhObjects) where T : class, IComponentBase
        {
            var components = new List<T>();
            var groupIndices = rhObjects
                .SelectMany(obj => obj.GetGroupList())
                .ToHashSet();
            foreach (var groupIndex in groupIndices) {
                var component = HydrateFromGroup<T>(groupIndex);
                if (component != null)
                    components.Add(component);
            }
            return components;
        }

        T? HydrateFromGroup<T>(int groupIndex) where T : class, IComponentBase
        {
            var grpObjects = Utility.Objects.ObjectsByGroup(_context.Document, groupIndex);
            foreach (var txtLabel in grpObjects.OfType<TextObject>()) {
                if (!txtLabel.Name.Contains(txtLabel.TextGeometry.PlainText))
                    continue;

                var componentType = Utility.Objects.GetComponentTypeFromObject(_context.Document, txtLabel);
                if (!_context.Registry.TryResolve(componentType.TypeId, out var registration)) {
                    try {
                        _context.Registry.Register<Component>(componentType.TypeId);
                        _context.Registry.TryResolve(componentType.TypeId, out registration);
                    }
                    catch {
                        registration = new TypeRegistration(typeof(Component), componentType.TypeId);
                    }
                }

                T component;
                try {
                    component = (T)Activator.CreateInstance(registration.ClrType);
                }
                catch (Exception ex) {
                    throw new InvalidOperationException(
                        $"Failed to create component instance of type '{registration.ClrType.FullName}' for TypeId '{componentType.TypeId}'.",
                        ex);
                }

                Attach(component);
                component.ID = txtLabel.Id;
                component.GroupIndex = groupIndex;

                // Same assignment path as the original Instantiation.InstanceFromGroup:
                // type metadata comes only from the Rhino document via GetComponentTypeFromObject.
                component.TypeId = componentType.TypeId;
                component.TypeName = componentType.TypeName;
                component.LayerColor = componentType.LayerColor;
                component.LabelSize = componentType.LabelSize;

                var label = txtLabel.TextGeometry;
                component.Label.SetObject(label);

                var isGenericType = registration.ClrType == typeof(Component)
                    || registration.ClrType.GetCustomAttribute<ComponentTypeAttribute>(inherit: false) == null;
                if (isGenericType) {
                    var members = Utility.Members.FindMembers(_context.Document, component);
                    component.SetMembers(members);
                }

                return component;
            }
            return null;
        }

        IComponentBase? GetParent(IComponentBase component, out int parentsFound)
        {
            parentsFound = 0;
            var parentNameSegments = component.ShortName.Split(Settings.NameDelimiter).ToList();
            if (parentNameSegments.Count <= 1)
                return null;

            parentNameSegments.RemoveAt(parentNameSegments.Count - 1);
            var parentName = string.Join(Settings.NameDelimiter.ToString(), parentNameSegments);
            var namingCondition = $"*{Settings.TypeDelimiter}{parentName}";
            var objEnumSettings = Utility.Constants.ObjectEnumeratorSettings(namingCondition);
            var rhObjects = _context.Document.Objects.GetObjectList(objEnumSettings);
            var parents = GetFromObjects<IComponentBase>(rhObjects).ToList();
            parentsFound = parents.Count;
            return parents.FirstOrDefault();
        }

        IEnumerable<IComponentBase> GetChildren(IComponentBase component, IEnumerable<string>? filterTypes = null)
        {
            var namingCondition = $"*{Settings.TypeDelimiter}{component.ShortName}{Settings.NameDelimiter}*";
            var objEnumSettings = Utility.Constants.ObjectEnumeratorSettings(namingCondition);
            var rhObjects = _context.Document.Objects.GetObjectList(objEnumSettings)
                .Where(rhObj => !rhObj.Name.Contains(Settings.JointDelimiter));
            if (filterTypes != null && filterTypes.Any())
                rhObjects = rhObjects.Where(rhObj => filterTypes.Contains(rhObj.Name.Split(Settings.TypeDelimiter)[0]));
            return GetFromObjects<IComponentBase>(rhObjects);
        }

        IEnumerable<IComponentBase> GetJoints(IComponentBase component, IEnumerable<string>? filterTypes = null)
        {
            var namingCondition = $"*{component.ShortName}*";
            var objEnumSettings = Utility.Constants.ObjectEnumeratorSettings(namingCondition);
            var escapedString = $"(.*{Settings.TypeDelimiter}{component.ShortName}{Settings.JointDelimiter}.*)" +
                $"|(.*{Settings.JointDelimiter}{component.ShortName}{Settings.JointDelimiter}.*)" +
                $"|(.*{Settings.JointDelimiter}{component.ShortName}$)";
            escapedString = escapedString.Replace("+", "\\+");
            var reg = new Regex(escapedString);
            var rhObjects = _context.Document.Objects.GetObjectList(objEnumSettings)
                .Where(rhObj => reg.IsMatch(rhObj.Name));
            if (filterTypes != null && filterTypes.Any())
                rhObjects = rhObjects.Where(rhObj => filterTypes.Contains(rhObj.Name.Split(Settings.TypeDelimiter)[0]));
            return GetFromObjects<IComponentBase>(rhObjects);
        }

        IEnumerable<IComponentBase> GetConnected(IComponentBase component, IEnumerable<string>? typeFilter = null)
        {
            IEnumerable<IComponentBase> joints;
            if (component.ShortName.Contains(Settings.JointDelimiter))
                joints = component.ShortName.Split(Settings.JointDelimiter)
                    .SelectMany(x => GetByName<IComponentBase>($"*{Settings.TypeDelimiter}{x}"));
            else
                joints = GetJoints(component);

            var connectedComponentNames = joints
                .SelectMany(x => x.ShortName.Split(Settings.JointDelimiter)
                    .Where(y => y != component.ShortName))
                .ToHashSet();
            var connectedComponents = connectedComponentNames
                .SelectMany(x => GetByName<IComponentBase>($"*{Settings.TypeDelimiter}{x}"));
            if (typeFilter != null && typeFilter.Any())
                connectedComponents = connectedComponents.Where(x => typeFilter.Contains(x.TypeId));
            return connectedComponents;
        }

        static void AttachMember(IComponentBase component, IMember member)
        {
            member.Component = component;
            foreach (var child in member.AllMembers)
                AttachMember(component, child);
        }

        static void MarkClean(IComponentBase component)
        {
            component.MarkClean();
            foreach (var member in component.AllMembers)
                MarkClean(member);
        }

        static void MarkClean(IMember member)
        {
            member.MarkClean();
            foreach (var child in member.AllMembers)
                MarkClean(child);
        }
    }
}
