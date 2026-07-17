using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using D2P.Core.Components;
using D2P.Core.Interfaces;

using Rhino;
using Rhino.DocObjects;
using Rhino.Geometry;

namespace D2P.Core.Utility {
    public static class Layers {
        public static Layer CreateLayer(RhinoDoc doc,IComponentBase component) {
            if (!FindRootLayer(doc,out Layer rootLayer))
                rootLayer = CreateRootLayer(doc);
            var layerName = ComposeComponentTypeLayerName(component);
            var componentLayer = new Layer() {
                Id = Guid.NewGuid(),
                ParentLayerId = rootLayer.Id,
                Name = layerName,
                Color = component.LayerColor
            };
            var layerIdx = doc.Layers.Add(componentLayer);
            componentLayer.Index = layerIdx;
            return componentLayer;
        }

        public static Layer CreateLayer(RhinoDoc doc,IMember member) {
            var layerName = ComposeMemberLayerName(member);
            var layerSegments = new Queue<string>(layerName
                .Split(Settings.LayerNameDelimiter)
                .Where(s => !string.IsNullOrEmpty(s))
            );

            var componentLayer = FindComponentTypeRootLayer(doc,member.Component);
            if (componentLayer == null || componentLayer.Index == 0)
                componentLayer = CreateComponentTypeLayer(doc,member.Component);

            return TraverseLayers(doc,member,ref layerSegments,componentLayer.Id);
        }

        public static Layer CreateRootLayer(RhinoDoc doc) {
            if (!FindRootLayer(doc,out Layer rootLayer)) {
                var rootLayerIdx = doc.Layers.Add(Settings.RootLayerName,Settings.RootLayerColor);
                rootLayer = doc.Layers.FindIndex(rootLayerIdx);
            }
            return rootLayer;
        }

        public static Layer CreateComponentTypeLayer(RhinoDoc doc,IComponentBase component) {
            if (!FindRootLayer(doc,out Layer rootLayer))
                rootLayer = CreateRootLayer(doc);
            var layerName = ComposeComponentTypeLayerName(component);
            var componentLayer = new Layer() {
                Id = Guid.NewGuid(),
                ParentLayerId = rootLayer.Id,
                Name = layerName,
                Color = component.LayerColor
            };
            var layerIdx = doc.Layers.Add(componentLayer);
            componentLayer.Index = layerIdx;
            return componentLayer;
        }

        public static bool FindRootLayer(RhinoDoc doc,out Layer rootLayer) {
            rootLayer = FindLayerByName(doc,Settings.RootLayerName);
            return rootLayer != null;
        }

        public static Layer FindLayer(RhinoDoc doc,IMember member) {
            var layer = FindLayer(doc,member,out int layersFound);
            return layersFound != 1 ? null : layer;
        }

        public static Layer FindLayer(RhinoDoc doc,IMember member,out int layersFound) {
            if (member?.Component == null) {
                layersFound = 0;
                return null;
            }
            string layerName;
            if (string.IsNullOrEmpty(member.LayerInfo.RawLayerName))
                layerName = ComposeComponentTypeLayerName(member.Component);
            else
                layerName = ComposeFullLayerPath(doc,member);
            var componentLayers = GetComponentLayers(doc,member.Component);
            var matchedLayers = componentLayers
                .Where(l => !l.IsReference && l.FullPath == layerName)
                .ToList();
            layersFound = matchedLayers.Count;
            return layersFound == 1 ? matchedLayers[0] : null;
        }

        public static Layer FindLayer(RhinoDoc doc,int layerIndex) {
            return doc.Layers.FindIndex(layerIndex);
        }

        public static Layer FindComponentLayerByType(RhinoDoc doc,string type) {
            var layerNames = doc.Layers.Where(l => !l.IsReference).Select(l => l.Name);
            var componentLayerName = ComposeComponentTypeLayerName(type,"");
            componentLayerName = layerNames.FirstOrDefault(name => name.StartsWith(componentLayerName));
            if (componentLayerName == null)
                return null;
            return FindLayerByName(doc,componentLayerName);
        }

        public static Layer FindLayerByName(RhinoDoc doc,string layerName,bool includeReferenced = false) {
            bool condition(Layer layer) => includeReferenced || !layer.IsReference;
            var layerFound = doc.Layers
                .FirstOrDefault(l => condition(l) &&
                l.Name == layerName &&
                l.FullPath.StartsWith(Settings.RootLayerName));
            return layerFound;
        }

        public static IEnumerable<Layer> FindComponentTypeRootLayers(RhinoDoc doc) {
            if (!FindRootLayer(doc,out Layer rootLayer))
                return Enumerable.Empty<Layer>();
            var childLayers = GetChildLayers(doc,rootLayer);
            return childLayers.Where(layer => IsComponentTypeRootLayer(layer));
        }

        public static Layer FindComponentTypeRootLayer(RhinoDoc doc,RhinoObject obj) {
            var objLayer = FindLayer(doc,obj.Attributes.LayerIndex);
            if (IsComponentTypeRootLayer(objLayer))
                return objLayer;

            var componentTypeAncestorLayers = new List<Layer>();
            TraverseAncestorLayers(doc,objLayer.Id,ref componentTypeAncestorLayers);
            return componentTypeAncestorLayers.Find(l => IsComponentTypeRootLayer(l));
        }

        public static Layer FindComponentTypeRootLayer(RhinoDoc doc,IComponentBase component) {
            var componentTypeRootLayerName = ComposeComponentTypeLayerName(component);
            return FindLayerByName(doc,componentTypeRootLayerName);
        }

        public static bool IsComponentTypeRootLayer(IComponentBase component,string layerName) {
            return layerName.Split(Settings.LayerDescriptionDelimiter).FirstOrDefault() == component.TypeId;
        }

        public static bool IsComponentTypeRootLayer(Layer layer) {
            if (layer == null)
                return false;
            var regex = new Regex($".*(?<!/s){Settings.LayerDescriptionDelimiter}(?<!/s).*");
            return regex.IsMatch(layer.Name);
        }

        public static string ComposeComponentLayerName(IComponentBase component,string rawLayerName) {
            return $"{component.TypeId}{Settings.LayerDelimiter}{rawLayerName.Split(Settings.LayerNameDelimiter).LastOrDefault()}";
        }

        public static string ComposeComponentTypeLayerName(IComponentType componentType) {
            return ComposeComponentTypeLayerName(componentType.TypeId,componentType.TypeName);
        }

        public static string ComposeComponentTypeLayerName(string type,string description) {
            return $"{type} {Settings.LayerDescriptionDelimiter} {description}";
        }

        public static string ComposeFullLayerPath(RhinoDoc doc,IMember member) {
            var layerPath = string.Empty;
            composeLayerPath(member,ref layerPath);
            var typeLayer = FindComponentTypeRootLayer(doc,member.Component);
            var typeLayerName = typeLayer?.FullPath;
            if (typeLayer == null) {
                var composedTypeLayerName = ComposeComponentTypeLayerName(member.Component);
                typeLayerName = $"{Settings.RootLayerName}::{composedTypeLayerName}";
            }
            return $"{typeLayerName}::{layerPath}";
        }

        static void composeLayerPath(IMember member,ref string layerPath) {
            var layerNames = member.LayerInfo.RawLayerName.Split(':')
                .Where(name => !string.IsNullOrEmpty(name))
                .Select(name => name.Replace($"{member.Component.TypeId}{Settings.LayerDelimiter}",""))
                .Select(name => $"{member.Component.TypeId}{Settings.LayerDelimiter}{name}");
            var result = string.Join("::",layerNames);

            layerPath = layerPath.Insert(0,result);
            if (member.ParentMember == null)
                return;
            layerPath = layerPath.Insert(0,"::");
            composeLayerPath(member.ParentMember,ref layerPath);
        }

        public static string DecomposeLayerName(IComponentBase component,string layerName) {
            if (IsComponentTypeRootLayer(component,layerName)) {
                var idx = layerName.IndexOf(Settings.LayerDescriptionDelimiter);
                return layerName.Substring(idx);
            }
            return layerName.Substring(layerName.IndexOf(Settings.LayerDelimiter));
        }

        public static string ComposeMemberLayerName(IMember member) {
            if (member == null)
                return string.Empty;

            var layerName = member.LayerInfo?.RawLayerName ?? string.Empty;
            if (member.ParentMember == null)
                return layerName;

            var parentLayerName = ComposeMemberLayerName(member.ParentMember);
            if (string.IsNullOrEmpty(parentLayerName))
                return layerName;

            var layerDelimiter = Settings.LayerNameDelimiter;
            return $"{parentLayerName}{layerDelimiter}{layerDelimiter}{layerName}";
        }

        public static LayerInfo GetLayerInfo(Layer layer) {
            return new LayerInfo(GetRawLayerName(layer),layer.Color);
        }

        public static string GetRawLayerName(Layer layer) {
            return layer.Name.Split(Settings.LayerDelimiter).LastOrDefault();
        }

        public static string GetComponentTypeID(Layer layer) {
            if (!IsComponentTypeRootLayer(layer))
                return string.Empty;
            var substringStartIdx = layer.Name.IndexOf(Settings.LayerDescriptionDelimiter);
            return layer.Name.Substring(0,substringStartIdx - 1);
        }

        public static string GetComponentTypeName(Layer layer) {
            if (!IsComponentTypeRootLayer(layer))
                return string.Empty;
            var substringStartIdx = layer.Name.IndexOf(Settings.LayerDescriptionDelimiter);
            return layer.Name.Substring(substringStartIdx + 2);
        }

        public static string GetComponentTypeName(RhinoDoc doc,RhinoObject rhObj) {
            var layer = FindComponentTypeRootLayer(doc,rhObj);
            return GetComponentTypeName(layer);
        }

        public static double GetComponentTypeLabelSize(RhinoDoc doc,Layer componentLayer) {
            var compObj = Objects.ObjectsByLayer(doc,componentLayer).FirstOrDefault();
            if (IsComponentTypeRootLayer(componentLayer) && compObj?.Geometry is TextEntity textEntity)
                return textEntity.TextHeight;
            return Settings.GetDimensionStyle(doc).TextHeight;
        }

        public static IEnumerable<Layer> GetComponentLayers(RhinoDoc doc,IComponentBase component) {
            var componentLayers = new List<Layer>();
            var componentTypeRootLayer = FindComponentTypeRootLayer(doc,component);
            if (componentTypeRootLayer == null)
                return componentLayers;
            TraverseChildLayers(doc,componentTypeRootLayer.Id,ref componentLayers);
            return componentLayers;
        }

        public static IEnumerable<Layer> GetAncestorLayers(RhinoDoc doc,Layer layer,bool includeRoot = false) {
            var parentId = layer.ParentLayerId;
            var ancestorLayers = new List<Layer>();
            if (includeRoot)
                ancestorLayers.Add(layer);
            TraverseAncestorLayers(doc,parentId,ref ancestorLayers);
            return ancestorLayers;
        }

        public static IEnumerable<Layer> GetChildLayers(RhinoDoc doc,Layer layer) {
            var rootId = layer?.Id ?? Guid.Empty;
            if (rootId == Guid.Empty)
                return Enumerable.Empty<Layer>();
            var childLayers = new List<Layer>();
            TraverseChildLayers(doc,rootId,ref childLayers);
            return childLayers.Where(l => l.Id != rootId);
        }

        public static IEnumerable<Layer> GetChildLayers(RhinoDoc doc,int layerIdx) {
            return GetChildLayers(doc,doc.Layers.FindIndex(layerIdx));
        }

        public static IEnumerable<int> GetChildLayerIndices(RhinoDoc doc,int layerIdx) {
            return GetChildLayers(doc,layerIdx).Select(layer => layer.Index);
        }

        static Layer TraverseLayers(RhinoDoc doc,IMember member,ref Queue<string> layerQueue,Guid parentLayerId) {
            if (string.IsNullOrEmpty(member.LayerInfo.RawLayerName))
                return FindComponentTypeRootLayer(doc,member.Component);

            var parentLayer = doc.Layers.FindId(parentLayerId);
            if (parentLayer == null) return null;

            var layerName = ComposeComponentLayerName(member.Component,layerQueue.Dequeue());
            var layerPath = $"{parentLayer.FullPath}::{layerName}";
            var docLayerIdx = doc.Layers.FindByFullPath(layerPath,-1);
            var docLayer = doc.Layers.FindIndex(docLayerIdx);
            if (docLayer == null && member.LayerInfo != null) {
                docLayer = new Layer() {
                    Name = layerName,
                    Id = Guid.NewGuid(),
                    ParentLayerId = parentLayerId,
                    Color = member.LayerInfo.LayerColor
                };
                docLayer.Index = doc.Layers.Add(docLayer);
            }
            if (docLayer == null || !layerQueue.Any())
                return docLayer;
            return TraverseLayers(doc,member,ref layerQueue,docLayer.Id);
        }

        static void TraverseAncestorLayers(RhinoDoc doc,Guid parentLayerId,ref List<Layer> ancestorLayers) {
            if (parentLayerId == Guid.Empty) return;
            var parentLayer = doc.Layers.FindId(parentLayerId);
            ancestorLayers.Add(parentLayer);
            TraverseAncestorLayers(doc,parentLayer.ParentLayerId,ref ancestorLayers);
        }

        static void TraverseChildLayers(RhinoDoc doc,Guid layerId,ref List<Layer> childLayers) {
            if (layerId == Guid.Empty) return;
            var layer = doc.Layers.FindId(layerId);
            childLayers.Add(layer);
            var children = layer.GetChildren();
            if (children == null) return;
            foreach (var childLayer in children)
                TraverseChildLayers(doc,childLayer.Id,ref childLayers);
        }
    }
}
