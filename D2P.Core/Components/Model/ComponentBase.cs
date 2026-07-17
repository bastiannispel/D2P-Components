using System;

using System.Collections.Generic;

using System.Drawing;

using System.Linq;

using System.Reflection;

using D2P.Core.Components.Member;

using D2P.Core.Extensions;

using D2P.Core.Interfaces;

using D2P.Core.Utility;

using Rhino;

using Rhino.DocObjects;

using Rhino.Geometry;

namespace D2P.Core.Components {

    public abstract class ComponentBase : MemberCollection, IComponentBase {

        public ModelContext Context { get; set; }

        public bool IsDirty { get; private set; }

        public Guid ID { get; set; } = Guid.Empty;

        public int GroupIndex { get; set; }

        public string Name => TypeId + Settings.TypeDelimiter + ShortName;

        public string ShortName {

            get => Label.Geometry.FirstOrDefault().PlainText;

            set => Label.Geometry.FirstOrDefault().PlainText = value;

        }

        public Plane Plane {

            get => Label.Geometry.FirstOrDefault().Plane;

            set => Label.Geometry.FirstOrDefault().Plane = value;

        }

        public virtual string TypeId { get; set; } = "COMP";

        public virtual string TypeName { get; set; } = "Base Component";

        public virtual Color LayerColor { get; set; } = Color.Black;

        public virtual double LabelSize { get; set; } = 5.0;

        public IEnumerable<GeometryBase> Geometry => AllMembers.SelectMany(m => m.Geometry);

        public IMember<TextEntity> Label { get; private set; }

        protected virtual void Init() {
            Label = new Member<TextEntity>(this,"",LayerColor);
        }

        public abstract IComponentBase Duplicate();

        public ComponentBase() {
            ApplyTypeDefaults();
            Init();
            var dimStyle = RhinoDoc.ActiveDoc != null
                ? Settings.GetDimensionStyle(RhinoDoc.ActiveDoc)
                : null;
            var label = TextEntity.Create("",Plane.WorldXY,dimStyle,false,0,0);
            if (LabelSize > 0)
                label.TextHeight = LabelSize;
            Label.SetObject(label);
        }

        public ComponentBase(string name,Plane plane) : this() {

            Label.Geometry.First().PlainText = name;

            Label.Geometry.First().Plane = plane;

        }

        protected ComponentBase(IComponentBase other) : this() {

            Context = other.Context;

            TypeId = other.TypeId;

            TypeName = other.TypeName;

            LayerColor = other.LayerColor;

            LabelSize = other.LabelSize;

            Label = other.Label.Duplicate();

            DynamicMembers = other.DynamicMembers.Duplicate();

        }

        void ApplyTypeDefaults() {

            var attribute = GetType().GetCustomAttribute<ComponentTypeAttribute>(inherit: false);

            if (attribute == null)

                return;

            TypeId = attribute.TypeId;
            TypeName = attribute.Name ?? string.Empty;
        }

        public bool Transform(Transform xform) {

            var result = true;

            foreach (var geometry in Geometry) {

                if (!geometry.Transform(xform))

                    result = false;

            }

            if (result)

                MarkDirty();

            return result;

        }

        public virtual bool Exists() {

            var doc = Context?.Document;

            return doc != null && doc.Objects.FindId(ID) != null;

        }

        public virtual void Delete() => Objects.DeleteComponent(DocHelper.Require(this),this);

        public virtual void Commit(bool deleteExisting = true) {

            Commit(deleteExisting,false);

        }

        public virtual void Commit(bool deleteExisting,bool onlyDirty) {

            var doc = DocHelper.Require(this);

            if (deleteExisting) {

                var existingObjects = Objects.ObjectsByName(doc,Name,ObjectType.AnyObject)

                    .Where(obj => obj.GroupCount != 0 && !obj.GetGroupList().Contains(GroupIndex))

                    .Select(obj => obj.Id);

                doc.Objects.Delete(existingObjects,true);

            }

            if (!Exists())

                create(doc);

            AllMembers.SetComponent(this);

            foreach (var member in AllMembers.Where(m => !Members.IsComponentLabel(this,m))) {

                if (!onlyDirty || member.IsDirty)

                    member.Commit(deleteExisting,onlyDirty);

            }

            MarkClean();

        }

        void create(RhinoDoc doc) {

            if (!Utility.Group.GetGroupIndex(doc,this,out int grpIdx))

                grpIdx = Utility.Group.AddGroup(doc);

            GroupIndex = grpIdx;

            var componentLayer = Layers.FindComponentTypeRootLayer(doc,this);

            if (componentLayer == null || componentLayer.Index == 0)

                componentLayer = Layers.CreateComponentTypeLayer(doc,this);

            var attributes = new ObjectAttributes() { Name = Name,LayerIndex = componentLayer.Index };

            attributes.AddToGroup(GroupIndex);

            var label = Label.Geometry.FirstOrDefault();

            label.TextHeight = LabelSize > 0 ? LabelSize : Settings.GetDimensionStyle(doc).TextHeight;

            ID = doc.Objects.AddText(label,attributes);

        }

        public virtual void Cache() {

            foreach (var member in AllMembers)

                member.Cache();

        }

        public void MarkClean() {

            IsDirty = false;

            Label.MarkClean();

        }

        internal void MarkDirty() {

            IsDirty = true;

        }

        public int CompareTo(object obj) {

            if (obj == null) return 1;

            var other = obj as IComponentBase;

            if (other != null)

                return ShortName.CompareTo(other.ShortName);

            throw new ArgumentException("Object is not an IComponentBase");

        }

    }

}

